using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cad3PLogBrowser.Models;

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
                        if (m.Success) current.ExceptionType = m.Value;
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
                if (!m.Success) continue;
                string id = m.Groups[2].Value;
                if (!map.ContainsKey(id)) map[id] = new List<int>();
                map[id].Add(e.LineNumber);
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
