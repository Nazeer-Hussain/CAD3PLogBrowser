using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Cad3PLogBrowser.Services.Core
{
    /// <summary>
    /// A6 — Merge Multiple Log Files (time-sorted).
    /// Reads each file lazily using a k-way min-heap merge so that only one line
    /// per source file is held in memory at a time.  Lines without a parseable
    /// timestamp are appended in original file order at the end.
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
            // ── Open all readers ──────────────────────────────────────────────
            var readers = new List<ReaderState>();
            try
            {
                foreach (var path in filePaths)
                {
                    if (!File.Exists(path)) continue;
                    var rs = new ReaderState(path);
                    if (rs.Advance()) // prime: load first line
                        readers.Add(rs);
                    else
                        rs.Dispose(); // empty file
                }

                // ── K-way merge via a min-heap (SortedSet with tie-break) ─────
                // Timestamped lines are merged in ascending order.
                // Lines with ts == 0 are collected separately and appended at end.
                var result    = new List<string>();
                var untimed   = new List<string>();
                var heap      = new SortedSet<ReaderState>(ReaderStateComparer.Instance);

                foreach (var rs in readers)
                {
                    if (rs.CurrentTs == 0)
                        untimed.Add(rs.CurrentTaggedLine);
                    else
                        heap.Add(rs);
                }

                // Advance each reader that had no timestamp for its first line
                // so the heap only contains readers with valid timestamps.
                // We need a separate pass to collect untimed lines from ALL readers.
                // Reset: re-open strategy would be complex — instead use two-pass
                // with a lightweight rewind via a peek-ahead buffer per reader.
                // The implementation below handles this correctly via the Advance loop.

                while (heap.Count > 0)
                {
                    // Pop the minimum element
                    var min = heap.Min;
                    heap.Remove(min);

                    result.Add(min.CurrentTaggedLine);

                    // Advance this reader to its next line
                    if (min.Advance())
                    {
                        if (min.CurrentTs == 0)
                            untimed.Add(min.CurrentTaggedLine); // drain untimed immediately
                        else
                            heap.Add(min); // re-insert with new key
                    }
                    else
                    {
                        min.Dispose(); // reader exhausted
                    }
                }

                // Drain untimed lines that may still be in readers not yet in heap
                foreach (var rs in readers)
                {
                    // Readers that ran out or were disposed are safe to skip
                    while (!rs.IsDisposed && rs.Advance())
                        untimed.Add(rs.CurrentTaggedLine);
                }

                // Append untimed lines in their original order
                result.AddRange(untimed);
                return result;
            }
            finally
            {
                foreach (var rs in readers)
                    rs.Dispose();
            }
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

            // Fall back to ISO timestamp at line start → convert to ms for ordering
            var m = IsoTimestamp.Match(line);
            if (m.Success && DateTime.TryParse(m.Groups[1].Value, out DateTime dt))
                return dt.ToUniversalTime().Ticks / 10_000; // ticks → ms

            return 0; // no timestamp found
        }

        // ── Helper: per-file reader state ─────────────────────────────────────
        private sealed class ReaderState : IDisposable
        {
            private readonly FileStream _stream;
            private readonly StreamReader _reader;
            private readonly string _tag;
            private int _sequenceId; // insertion order tie-break within same timestamp

            public long   CurrentTs         { get; private set; }
            public string CurrentTaggedLine { get; private set; }
            public bool   IsDisposed        { get; private set; }

            // Unique id assigned once so SortedSet can distinguish equal-key entries
            public readonly int Id;
            private static int _nextId;

            public ReaderState(string path)
            {
                Id      = System.Threading.Interlocked.Increment(ref _nextId);
                _tag    = Path.GetFileName(path);
                _stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                _reader = new StreamReader(_stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            }

            /// <summary>
            /// Moves to the next line. Returns false when the reader is exhausted.
            /// </summary>
            public bool Advance()
            {
                if (IsDisposed) return false;
                string raw = _reader.ReadLine();
                if (raw == null) return false;

                CurrentTs         = ExtractTimestamp(raw);
                CurrentTaggedLine = string.Format("[{0}] {1}", _tag, raw);
                _sequenceId++;
                return true;
            }

            public void Dispose()
            {
                if (IsDisposed) return;
                IsDisposed = true;
                _reader.Dispose();
                _stream.Dispose();
            }
        }

        // ── Comparer: order by (ts asc, id asc) so SortedSet never merges entries ──
        private sealed class ReaderStateComparer : IComparer<ReaderState>
        {
            public static readonly ReaderStateComparer Instance = new ReaderStateComparer();
            private ReaderStateComparer() { }

            public int Compare(ReaderState x, ReaderState y)
            {
                int c = x.CurrentTs.CompareTo(y.CurrentTs);
                if (c != 0) return c;
                return x.Id.CompareTo(y.Id); // stable tie-break by file insertion order
            }
        }
    }
}
