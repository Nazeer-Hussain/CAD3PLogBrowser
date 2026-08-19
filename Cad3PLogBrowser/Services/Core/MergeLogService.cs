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

        private static readonly CompressedLogService _compressedLogService = new CompressedLogService();

        /// <summary>A6: routes through CompressedLogService for .gz/.zip sources so merging
        /// a compressed file no longer reads its compressed bytes as garbage text — merge
        /// previously always used a raw StreamReader regardless of the drag-drop/Open
        /// dialog's A7 decompression support.</summary>
        private static IEnumerable<string> ReadRawLines(string path)
        {
            if (CompressedLogService.IsCompressed(path))
                return _compressedLogService.ReadLines(path);

            return ReadPlainTextLines(path);
        }

        private static IEnumerable<string> ReadPlainTextLines(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open,
                FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream, Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true))
            {
                string raw;
                while ((raw = reader.ReadLine()) != null)
                    yield return raw;
            }
        }

        public Task<List<string>> MergeAsync(IEnumerable<string> filePaths)
        {
            return Task.Run(() => Merge(filePaths));
        }

        private List<string> Merge(IEnumerable<string> filePaths)
        {
            // Estimate total lines across all files
            long totalBytes = 0;
            var paths = new List<string>();
            foreach (var p in filePaths)
            {
                if (!File.Exists(p)) continue;
                paths.Add(p);
                totalBytes += new FileInfo(p).Length;
            }
            // D3: cap capacity to avoid passing int.MaxValue (2B) to List<T> on huge files.
            const int maxEstimate = 5_000_000;
            int estimatedLines = totalBytes > 0 ? (int)Math.Min(totalBytes / 120L, maxEstimate) : 4096;

            // BUG FIX: List<T>.Sort is an unstable introsort. Lines sharing the same
            // epoch timestamp (very common — ENTER/EXIT pairs and lines from different
            // files logged in the same millisecond) could get reordered relative to each
            // other, breaking the ENTER/EXIT nesting that BuildCallTree relies on and
            // producing a corrupted merged call tree (missing/mismatched nodes). Adding
            // the original insertion index as a tiebreaker makes the sort stable.
            var buckets = new List<(long ts, int seq, string line)>(estimatedLines);

            int seq = 0;
            foreach (var path in paths)
            {
                string tag = Path.GetFileName(path);

                foreach (var raw in ReadRawLines(path))
                {
                    long ts = ExtractTimestamp(raw);
                    string tagged = string.Format("[{0}] {1}", tag, raw);
                    buckets.Add((ts, seq++, tagged));
                }
            }

            // Stable sort: lines with timestamp first (ascending), unknowns last,
            // ties broken by original order.
            // P3: use a static Comparison<T> to avoid allocating a new delegate on every call.
            buckets.Sort(TimestampComparison);

            var result = new List<string>(buckets.Count);
            foreach (var (_, _, line) in buckets)
                result.Add(line);

            return result;
        }

        // P3: static comparison method — captured once, no per-sort delegate allocation.
        private static int TimestampComparison((long ts, int seq, string line) a, (long ts, int seq, string line) b)
        {
            if (a.ts == 0 && b.ts == 0) return a.seq.CompareTo(b.seq);
            if (a.ts == 0) return 1;
            if (b.ts == 0) return -1;
            int cmp = a.ts.CompareTo(b.ts);
            return cmp != 0 ? cmp : a.seq.CompareTo(b.seq);
        }

        /// <summary>Extracts a raw log line's epoch-ms timestamp (from the trailing
        /// ENTER/EXIT tab-field, or a leading ISO timestamp), 0 if neither is found.
        /// Shared with G7's branch-to-XLSX export for its Timestamp column.</summary>
        public static long ExtractTimestamp(string line)
        {
            // Fast path: epoch ms is the last tab-field on ENTER/EXIT lines
            int lastTab = line.LastIndexOf('\t');
            if (lastTab >= 0 && lastTab < line.Length - 1)
            {
                string candidate = line.Substring(lastTab + 1).Trim();
                if (long.TryParse(candidate, out long epochMs) && epochMs > 1_000_000_000_000L)
                    return epochMs;
            }

            // BUG FIX: merged/tagged lines carry a "[filename] " prefix (see Merge()
            // below), so the ISO timestamp does not start at index 0 for them. Skip
            // past a leading "[...] " tag — mirrors LogParserService.ParseLine's own
            // [filename]-stripping logic — before checking for a digit start. Without
            // this, every merged line without an ENTER/EXIT tab-epoch (e.g. UWGM client
            // logs) always resolved to timestamp 0, breaking any time-based lookups.
            string effective = line;
            if (effective.Length > 2 && effective[0] == '[')
            {
                int closingBracket = effective.IndexOf("] ", StringComparison.Ordinal);
                if (closingBracket > 1 && effective.Substring(1, closingBracket - 1).IndexOf('.') >= 0)
                    effective = effective.Substring(closingBracket + 2);
            }

            // Only run the regex if the line looks like it starts with an ISO timestamp
            if (effective.Length < 20 || !char.IsDigit(effective[0]))
                return 0;

            // Fall back to ISO timestamp at line start
            var m = IsoTimestamp.Match(effective);
            if (m.Success && DateTime.TryParse(m.Groups[1].Value,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal,
                    out DateTime dt))
            {
                // BUG FIX: this must be a genuine Unix-epoch millisecond value so it is
                // directly comparable to the epoch-ms returned by the ENTER/EXIT fast
                // path above (e.g. cadapp lines). The previous code returned
                // dt.Ticks / 10_000 — .NET ticks (since 0001-01-01) divided down, which
                // is a completely different numeric scale from Unix epoch ms and can
                // never be compared against it. That silently broke any cross-file
                // time-based lookup (e.g. the UWGM Client tab's ±N-second scroll)
                // whenever one side used the tab-epoch fast path and the other used
                // this ISO fallback.
                return new DateTimeOffset(dt, TimeSpan.Zero).ToUnixTimeMilliseconds();
            }

            return 0;
        }

        /// <summary>
        /// Merges already-tagged in-memory lines (lines that already carry a
        /// <c>[filename]</c> prefix added by a previous merge) with one or more
        /// additional files, without re-tagging the existing lines.
        /// Use this when the original source files are no longer available on disk.
        /// </summary>
        public Task<List<string>> MergeTaggedWithNewFilesAsync(
            IList<string> existingTaggedLines,
            IEnumerable<string> newFilePaths)
        {
            return Task.Run(() =>
            {
                var buckets = new List<(long ts, int seq, string line)>(existingTaggedLines.Count + 512);
                int seq = 0;

                // Existing lines already carry correct [filename] tags — keep as-is
                foreach (var line in existingTaggedLines)
                    buckets.Add((ExtractTimestamp(line), seq++, line));

                // New files: tag and add
                foreach (var path in newFilePaths)
                {
                    if (!File.Exists(path)) continue;
                    string tag = Path.GetFileName(path);
                    foreach (var raw in ReadRawLines(path))
                    {
                        long ts = ExtractTimestamp(raw);
                        buckets.Add((ts, seq++, string.Format("[{0}] {1}", tag, raw)));
                    }
                }

                buckets.Sort(TimestampComparison);

                var result = new List<string>(buckets.Count);
                foreach (var (_, _, line) in buckets)
                    result.Add(line);
                return result;
            });
        }
    }
}
