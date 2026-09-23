using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cad3PLogBrowser.Services;

namespace Cad3PLogBrowser.Services.Analysis
{
    /// <summary>
    /// K2 — Exception Trace Grouping.
    /// Scans log lines for exception patterns and groups related lines together.
    /// K3 — Correlation ID / Request Tracking.
    /// Extracts and groups log lines by correlation/request ID.
    /// K4 — Jump to Source Code.
    /// Extracts file path + line number references from log lines.
    /// </summary>
    public class ExceptionGroupingService
    {
        // Common exception patterns in UWGM logs
        private static readonly Regex ExceptionPattern =
            new Regex(@"(exception|error|xcpt|XComm|uwgmcommexc)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex SourcePathPattern =
            new Regex(@"([A-Za-z]:\\[^\s:]+\.(cpp|cxx|h|cs|java))[:\s]+(\d+)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex CorrelationPattern =
            new Regex(@"(requestId|correlationId|sessionId|traceId)[=:\s]+([A-Za-z0-9\-]+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // UWGM-style logs mark an individual operation/request with a named token followed by a
        // timestamp-like identifier, e.g. "File-Open 2026_09_15-05_03_58". This is the most precise
        // correlation key available (one value per user action), so it's tried before the P/T fallback.
        private static readonly Regex OperationTokenPattern =
            new Regex(@"\b([A-Za-z][A-Za-z0-9]*-[A-Za-z0-9]+)\s+(\d{4}_\d{2}_\d{2}-\d{2}_\d{2}_\d{2})\b", RegexOptions.Compiled);

        // Fallback: when a line has no explicit correlation keyword or operation token, group by
        // LogEntry.ThreadId (already parsed by LogParserService for every supported log format —
        // including Format B/Inventor logs, which have no process id at all, only "[Thread:NNN]").
        // This spans the whole log for that thread rather than a single request, but at least keeps
        // related lines together instead of being silently dropped from correlation.

        // K2: Group exception blocks
        public List<ExceptionGroup> GroupExceptions(List<LogEntry> entries)
        {
            var groups  = new List<ExceptionGroup>();
            ExceptionGroup current = null;

            foreach (var e in entries)
            {
                if (ExceptionPattern.IsMatch(e.RawText))
                {
                    if (current == null)
                    {
                        current = new ExceptionGroup { StartLine = e.LineNumber };
                        groups.Add(current);
                    }
                    current.Lines.Add(e.LineNumber);
                    current.EndLine = e.LineNumber;

                    // Try extract exception type
                    if (current.ExceptionType == null)
                    {
                        var m = Regex.Match(e.RawText, @"([A-Za-z]+Exception|XComm|xcpt[A-Za-z]+)");
                        if (m.Success)
                        {
                            current.ExceptionType = m.Value;
                        }
                        else
                        {
                            // Fallback: many real-world logs report errors as
                            // "Error: <FunctionName> - <message>" or "<Class>::<Method>: failed ..."
                            // rather than a dedicated exception class name. Require an explicit "Error:"
                            // keyword or a "::" separator so we don't accidentally match timestamp
                            // fragments (e.g. the "T07" in "...T07:02:14.652Z...").
                            var fn = Regex.Match(e.RawText,
                                @"Error:\s*([A-Za-z_][A-Za-z0-9_]*)|([A-Za-z_][A-Za-z0-9_]*::[A-Za-z_][A-Za-z0-9_]*)");
                            if (fn.Success)
                                current.ExceptionType = fn.Groups[1].Success ? fn.Groups[1].Value : fn.Groups[2].Value;
                        }
                    }
                }
                else if (current != null && e.LineNumber > current.EndLine + 5)
                {
                    // Gap of 5+ non-exception lines ends the group
                    current = null;
                }
            }
            return groups;
        }

        // K3: Group by Correlation / Request ID
        public Dictionary<string, List<int>> GroupByCorrelationId(List<LogEntry> entries)
        {
            var map = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
            foreach (var e in entries)
            {
                var m = CorrelationPattern.Match(e.RawText);
                if (m.Success)
                {
                    string id = m.Groups[2].Value;
                    if (!map.ContainsKey(id)) map[id] = new List<int>();
                    map[id].Add(e.LineNumber);
                    continue;
                }

                // Next, prefer a named operation token (e.g. "File-Open 2026_09_15-05_03_58"),
                // which identifies a single logical request/operation.
                var op = OperationTokenPattern.Match(e.RawText);
                if (op.Success)
                {
                    string opId = op.Groups[1].Value + " " + op.Groups[2].Value;
                    if (!map.ContainsKey(opId)) map[opId] = new List<int>();
                    map[opId].Add(e.LineNumber);
                    continue;
                }

                // Fallback: group by the already-parsed thread id (works across all log formats,
                // including Inventor's [Thread:NNN] format which carries no process id).
                if (string.IsNullOrEmpty(e.ThreadId)) continue;
                string ptId = "Thread " + e.ThreadId;
                if (!map.ContainsKey(ptId)) map[ptId] = new List<int>();
                map[ptId].Add(e.LineNumber);
            }
            return map;
        }

        // K4: Extract source file references
        public List<SourceReference> ExtractSourceReferences(List<LogEntry> entries)
        {
            var refs = new List<SourceReference>();
            foreach (var e in entries)
            {
                var m = SourcePathPattern.Match(e.RawText);
                if (!m.Success) continue;
                refs.Add(new SourceReference
                {
                    LogLine    = e.LineNumber,
                    FilePath   = m.Groups[1].Value,
                    SourceLine = int.TryParse(m.Groups[3].Value, out int ln) ? ln : 0
                });
            }
            return refs;
        }
    }

    public class ExceptionGroup
    {
        public string      ExceptionType { get; set; } = "Exception";
        public int         StartLine     { get; set; }
        public int         EndLine       { get; set; }
        public List<int>   Lines         { get; } = new List<int>();
        public int         Count         => Lines.Count;
        public override string ToString() =>
            string.Format("{0} (Ln {1}–{2}, {3} lines)", ExceptionType, StartLine, EndLine, Count);
    }

    public class SourceReference
    {
        public int    LogLine    { get; set; }
        public string FilePath   { get; set; }
        public int    SourceLine { get; set; }
        public override string ToString() =>
            string.Format("{0}:{1} (log line {2})", FilePath, SourceLine, LogLine);
    }
}
