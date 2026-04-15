using System;
using System.Collections.Generic;

namespace Cad3PLogBrowser.Services
{
    /// <summary>
    /// Parses log lines produced by the UWGM CAD adapter debug logger.
    ///
    /// Line format (colon-separated prefix, then tab-separated payload):
    ///   {ISO-datetime}: {Level}: {PID}: {TID}: {App}: {Area}: {State}\t{Module}\t{SourceFile}\t{ApiName}\t{ENTER|EXIT}\t{EpochMs}
    ///
    /// Only lines whose tab-field[4] is exactly "ENTER" or "EXIT" are API call lines.
    /// All other lines are plain log lines (debug messages, config, errors, etc.)
    ///
    /// Log levels:  D=Debug  I=Info  W=Warning  E=Error  C=Config  X=Unknown
    /// </summary>
    public class LogParserService
    {
        // ── Log level codes ───────────────────────────────────────────────────
        public const string LevelDebug   = "D";
        public const string LevelInfo    = "I";
        public const string LevelWarning = "W";
        public const string LevelError   = "E";
        public const string LevelConfig  = "C";

        // ── Tab field indices (within the message payload after the 7th colon) ─
        private const int TabFieldState      = 0;   // e.g. "idle"
        private const int TabFieldModule     = 1;   // e.g. "Icubed"
        private const int TabFieldSourceFile = 2;   // e.g. "Cswadapter_app"
        private const int TabFieldApiName    = 3;   // e.g. "ConnectToSW"
        private const int TabFieldEntryType  = 4;   // "ENTER" or "EXIT"
        private const int TabFieldEpochMs    = 5;   // e.g. "1774943280304"

        // ── Colon field indices ───────────────────────────────────────────────
        private const int ColonFieldDateTime = 0;   // "2026-03-31T07:48:00.304Z"
        private const int ColonFieldLevel    = 1;   // "D"
        private const int ColonFieldPid      = 2;   // "P6c24"
        private const int ColonFieldTid      = 3;   // "T5aa4"
        private const int ColonFieldApp      = 4;   // "uwgmapp"
        private const int ColonFieldArea     = 5;   // "UWGM_ADAPTER"
        // ColonField[6] onwards = the tab-delimited payload

        // ── Public parse entry point ──────────────────────────────────────────
        /// <summary>Converts raw string lines into structured <see cref="LogEntry"/> list.</summary>
        public List<LogEntry> Parse(IList<string> rawLines)
        {
            var entries = new List<LogEntry>(rawLines.Count);
            for (int i = 0; i < rawLines.Count; i++)
                entries.Add(ParseLine(rawLines[i], i + 1));
            return entries;
        }

        private static LogEntry ParseLine(string raw, int lineNumber)
        {
            var entry = new LogEntry
            {
                LineNumber = lineNumber,
                RawText    = raw
            };

            if (string.IsNullOrEmpty(raw))
                return entry;

            // Walk colon-separated fields without allocating a string array.
            // Format: {DateTime}: {Level}: {PID}: {TID}: {App}: {Area}: {payload}
            // We need fields at indices 1 (Level), 3 (TID), 4 (App), 5 (Area), 6+ (payload).
            const string sep = ": ";
            int pos = 0;
            int fieldIndex = 0;
            int payloadStart = -1;

            while (pos < raw.Length)
            {
                int next = raw.IndexOf(sep, pos, StringComparison.Ordinal);
                if (next < 0 || fieldIndex >= 6)
                {
                    // fieldIndex 6 = rest of string is the tab-delimited payload
                    if (fieldIndex == 6) payloadStart = pos;
                    break;
                }

                string field = raw.Substring(pos, next - pos);

                switch (fieldIndex)
                {
                    case ColonFieldLevel:  entry.Level    = field; break;
                    case ColonFieldTid:    entry.ThreadId = field; break;
                    case ColonFieldApp:    entry.App      = field; break;
                    case ColonFieldArea:   entry.Area     = field; break;
                }

                fieldIndex++;
                pos = next + sep.Length;
            }

            // pos now points at field 6 (the payload) if we exited normally
            if (fieldIndex == 6) payloadStart = pos;

            if (payloadStart < 0 || payloadStart >= raw.Length)
                return entry;

            // Parse tab-delimited payload fields without Split
            // Fields: [State]\t[Module]\t[SourceFile]\t[ApiName]\t[ENTER|EXIT]\t[EpochMs]
            string payload = raw.Substring(payloadStart);
            int[] tabPos = new int[7];
            int tabCount = 0;
            tabPos[0] = 0;
            for (int i = 0; i < payload.Length && tabCount < 6; i++)
            {
                if (payload[i] == '\t')
                    tabPos[++tabCount] = i + 1;
            }

            if (tabCount <= TabFieldEntryType)
                return entry;

            // Extract entryType field (index 4): ends at the next tab or end of string
            int etStart = tabPos[TabFieldEntryType];
            int etEnd   = tabCount > TabFieldEntryType
                ? tabPos[TabFieldEntryType + 1] - 1
                : payload.Length;

            string entryType = payload.Substring(etStart, etEnd - etStart).Trim();

            if (entryType != "ENTER" && entryType != "EXIT")
                return entry;

            entry.IsApiCall   = true;
            entry.IsCallEnter = entryType == "ENTER";
            entry.IsCallExit  = !entry.IsCallEnter;

            // ApiName (field 3)
            int anEnd = tabPos[TabFieldApiName + 1] - 1;
            entry.ApiName = payload.Substring(tabPos[TabFieldApiName], anEnd - tabPos[TabFieldApiName]).Trim();

            // SourceFile (field 2)
            int sfEnd = tabPos[TabFieldSourceFile + 1] - 1;
            entry.SourceFile = payload.Substring(tabPos[TabFieldSourceFile], sfEnd - tabPos[TabFieldSourceFile]).Trim();

            // Module (field 1)
            int modEnd = tabPos[TabFieldModule + 1] - 1;
            entry.Module = payload.Substring(tabPos[TabFieldModule], modEnd - tabPos[TabFieldModule]).Trim();

            // EpochMs (field 5) — only if present
            if (tabCount >= TabFieldEpochMs + 1)
            {
                int emStart = tabPos[TabFieldEpochMs];
                int emEnd   = tabCount > TabFieldEpochMs ? tabPos[TabFieldEpochMs + 1] - 1 : payload.Length;
                if (tabCount < TabFieldEpochMs + 1) emEnd = payload.Length;
                string epochStr = payload.Substring(emStart, Math.Max(0, emEnd - emStart)).Trim();
                long.TryParse(epochStr, out long epochMs);
                entry.EpochMs = epochMs;
            }

            return entry;
        }

        // ── API list (flat, unique names) ─────────────────────────────────────
        /// <summary>
        /// Returns one <see cref="ApiCallNode"/> per unique API name found in the log,
        /// with all line numbers at which that API appears.
        /// </summary>
        public List<ApiCallNode> BuildApiList(List<LogEntry> entries)
        {
            var map = new Dictionary<string, ApiCallNode>(StringComparer.Ordinal);

            foreach (var entry in entries)
            {
                if (!entry.IsApiCall) continue;

                if (!map.TryGetValue(entry.ApiName, out var node))
                {
                    node = new ApiCallNode { ApiName = entry.ApiName };
                    map[entry.ApiName] = node;
                }
                node.LineNumbers.Add(entry.LineNumber);
            }

            var list = new List<ApiCallNode>(map.Values);
            list.Sort((a, b) => string.Compare(a.ApiName, b.ApiName, StringComparison.Ordinal));
            return list;
        }

        // ── Call stack tree ───────────────────────────────────────────────────
        /// <summary>
        /// Builds a hierarchical call stack tree using ENTER/EXIT pairs.
        /// Returns root-level <see cref="CallStackNode"/> objects.
        /// </summary>
        public List<CallStackNode> BuildCallTree(List<LogEntry> entries)
        {
            var roots = new List<CallStackNode>();
            var stack = new Stack<CallStackNode>();

            foreach (var entry in entries)
            {
                if (!entry.IsApiCall) continue;

                if (entry.IsCallEnter)
                {
                    var node = new CallStackNode
                    {
                        Label      = entry.ApiName,
                        LineNumber = entry.LineNumber,
                        Depth      = stack.Count,
                        SourceFile = entry.SourceFile,
                        Module     = entry.Module,
                        EpochMs    = entry.EpochMs
                    };

                    if (stack.Count == 0)
                        roots.Add(node);
                    else
                        stack.Peek().Children.Add(node);

                    stack.Push(node);
                }
                else if (entry.IsCallExit)
                {
                    if (stack.Count == 0) continue;

                    // Walk the stack looking for the matching ENTER.
                    var tempPopped = new Stack<CallStackNode>();
                    bool matched = false;

                    while (stack.Count > 0)
                    {
                        var top = stack.Pop();
                        if (top.Label == entry.ApiName)
                        {
                            // Found the match — record timing
                            top.ExitLineNumber = entry.LineNumber;
                            top.ExitEpochMs    = entry.EpochMs;
                            if (top.EpochMs > 0 && entry.EpochMs > 0)
                                top.DurationMs = entry.EpochMs - top.EpochMs;
                            matched = true;
                            break;
                        }
                        // Not a match — keep it so we can restore if needed
                        tempPopped.Push(top);
                    }

                    if (!matched)
                    {
                        // No matching ENTER found — restore everything we popped
                        while (tempPopped.Count > 0)
                            stack.Push(tempPopped.Pop());
                    }
                    // If matched, the mismatched intermediate nodes stay as children
                    // of whatever is now on top of the stack — correct behaviour.
                }
            }

            return roots;
        }

        // ── Performance statistics ────────────────────────────────────────────
        /// <summary>
        /// Walks the entire call tree and aggregates per-API timing statistics.
        /// Returns one <see cref="ApiPerfStats"/> per unique API name, sorted by
        /// total time descending (biggest time consumers first).
        /// </summary>
        public List<ApiPerfStats> BuildPerformanceStats(List<CallStackNode> roots)
        {
            var map = new Dictionary<string, ApiPerfStats>(StringComparer.Ordinal);
            foreach (var root in roots)
                CollectStats(root, map);

            var list = new List<ApiPerfStats>(map.Values);
            list.Sort((a, b) => b.TotalDurationMs.CompareTo(a.TotalDurationMs));
            return list;
        }

        private static void CollectStats(CallStackNode node, Dictionary<string, ApiPerfStats> map)
        {
            if (!map.TryGetValue(node.Label, out var stats))
            {
                stats = new ApiPerfStats { ApiName = node.Label, SourceFile = node.SourceFile };
                map[node.Label] = stats;
            }

            stats.CallCount++;

            if (node.DurationMs > 0)
            {
                // Self time = this node's duration minus direct children's durations
                long childrenTime = 0;
                foreach (var child in node.Children)
                    childrenTime += child.DurationMs;
                long selfTime = Math.Max(0, node.DurationMs - childrenTime);

                stats.TimedCallCount++;
                stats.TotalDurationMs += node.DurationMs;
                stats.SelfDurationMs  += selfTime;
                if (node.DurationMs < stats.MinDurationMs || stats.MinDurationMs < 0)
                    stats.MinDurationMs = node.DurationMs;
                if (node.DurationMs > stats.MaxDurationMs)
                    stats.MaxDurationMs = node.DurationMs;
            }

            foreach (var child in node.Children)
                CollectStats(child, map);
        }
    }

    // ── Data models ───────────────────────────────────────────────────────────

    /// <summary>One parsed log line.</summary>
    public class LogEntry
    {
        public int    LineNumber   { get; set; }
        public string RawText      { get; set; }
        public string Level        { get; set; }   // D I W E C
        public string ThreadId     { get; set; }
        public string App          { get; set; }
        public string Area         { get; set; }

        // API call fields (only set when IsApiCall == true)
        public bool   IsApiCall    { get; set; }
        public bool   IsCallEnter  { get; set; }
        public bool   IsCallExit   { get; set; }
        public string ApiName      { get; set; }
        public string SourceFile   { get; set; }
        public string Module       { get; set; }
        public long   EpochMs      { get; set; }
    }

    /// <summary>One unique API name with all line numbers it appears on.</summary>
    public class ApiCallNode
    {
        public string    ApiName     { get; set; }
        public List<int> LineNumbers { get; } = new List<int>();
        public int       FirstLine   => LineNumbers.Count > 0 ? LineNumbers[0] : -1;
        public override string ToString() =>
            string.Format("{0}  ({1}×)", ApiName, LineNumbers.Count);
    }

    /// <summary>One node in the hierarchical call stack tree.</summary>
    public class CallStackNode
    {
        public string              Label          { get; set; }
        public int                 LineNumber      { get; set; }
        public int                 ExitLineNumber  { get; set; }
        public int                 Depth           { get; set; }
        public string              SourceFile      { get; set; }
        public string              Module          { get; set; }
        public long                EpochMs         { get; set; }
        public long                ExitEpochMs     { get; set; }
        public long                DurationMs      { get; set; }
        public List<CallStackNode> Children        { get; } = new List<CallStackNode>();

        public override string ToString() =>
            DurationMs > 0
                ? string.Format("{0}  [{1} ms]", Label, DurationMs)
                : Label;
    }

    /// <summary>Aggregated timing statistics for one API across all its calls.</summary>
    public class ApiPerfStats
    {
        public string ApiName         { get; set; }
        public string SourceFile      { get; set; }
        public int    CallCount       { get; set; }       // total ENTER lines
        public int    TimedCallCount  { get; set; }       // calls with a matching EXIT
        public long   TotalDurationMs { get; set; }
        public long   SelfDurationMs  { get; set; }       // excludes time in child calls
        public long   MinDurationMs   { get; set; } = -1; // -1 = not yet set
        public long   MaxDurationMs   { get; set; }
        public long   AvgDurationMs   =>
            TimedCallCount > 0 ? TotalDurationMs / TimedCallCount : 0;
        public long   AvgSelfMs       =>
            TimedCallCount > 0 ? SelfDurationMs  / TimedCallCount : 0;
    }
}