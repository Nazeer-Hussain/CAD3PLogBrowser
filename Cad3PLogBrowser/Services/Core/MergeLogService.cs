using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Cad3PLogBrowser.Services.Core
{
    /// <summary>
    /// A6 — Merge Multiple Log Files (time-sorted).
    /// Reads each file asynchronously, tags each line with its source filename,
    /// then merges them in ascending epoch-timestamp order.
    /// Lines without a parseable timestamp are appended in file order at the end.
    /// </summary>
    public class MergeLogService
    {
        // Epoch ms is the last tab-field on ENTER/EXIT lines; for other lines
        // we extract the ISO timestamp from the line prefix.
        private static readonly System.Text.RegularExpressions.Regex IsoTimestamp =
            new System.Text.RegularExpressions.Regex(
                @"^(\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d+Z)",
                System.Text.RegularExpressions.RegexOptions.Compiled);

        public Task<List<string>> MergeAsync(IEnumerable<string> filePaths)
        {
            return Task.Run(() => Merge(filePaths));
        }

        private List<string> Merge(IEnumerable<string> filePaths)
        {
            var buckets = new List<(long ts, string line)>();

            foreach (var path in filePaths)
            {
                if (!File.Exists(path)) continue;
                string tag = Path.GetFileName(path);

                using (var stream = new FileStream(path, FileMode.Open,
                    FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(stream, Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: true))
                {
                    string raw;
                    while ((raw = reader.ReadLine()) != null)
                    {
                        long ts = ExtractTimestamp(raw);
                        // Prefix line with source filename for traceability
                        string tagged = string.Format("[{0}] {1}", tag, raw);
                        buckets.Add((ts, tagged));
                    }
                }
            }

            // Stable sort: lines with timestamp first (ascending), unknowns last
            buckets.Sort((a, b) =>
            {
                if (a.ts == 0 && b.ts == 0) return 0;
                if (a.ts == 0) return 1;
                if (b.ts == 0) return -1;
                return a.ts.CompareTo(b.ts);
            });

            var result = new List<string>(buckets.Count);
            foreach (var (_, line) in buckets)
                result.Add(line);

            return result;
        }

        private static long ExtractTimestamp(string line)
        {
            // Try tab-delimited epoch ms (ENTER/EXIT lines, last field)
            int lastTab = line.LastIndexOf('\t');
            if (lastTab >= 0 && lastTab < line.Length - 1)
            {
                string candidate = line.Substring(lastTab + 1).Trim();
                if (long.TryParse(candidate, out long epochMs) && epochMs > 1_000_000_000_000L)
                    return epochMs;
            }

            // Fall back to ISO timestamp at line start → convert to ticks for ordering
            var m = IsoTimestamp.Match(line);
            if (m.Success && DateTime.TryParse(m.Groups[1].Value, out DateTime dt))
                return dt.ToUniversalTime().Ticks / 10_000; // ticks → ms

            return 0; // no timestamp found
        }
    }
}
