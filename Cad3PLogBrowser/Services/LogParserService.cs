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

            // Split on ": " to get the colon-separated header fields.
            // Maximum 7 splits — the 7th token is the full tab-delimited payload.
            string[] colonParts = raw.Split(new[] { ": " }, 7, StringSplitOptions.None);

            if (colonParts.Length >= 2)
                entry.Level = colonParts[ColonFieldLevel].Trim();

            if (colonParts.Length >= 4)
                entry.ThreadId = colonParts[ColonFieldTid].Trim();

            if (colonParts.Length >= 5)
                entry.App = colonParts[ColonFieldApp].Trim();

            if (colonParts.Length >= 6)
                entry.Area = colonParts[ColonFieldArea].Trim();

            // The 7th colon-part is the tab-delimited payload (may not exist on config/error lines)
            if (colonParts.Length >= 7)
            {
                string payload = colonParts[6];
                string[] tabParts = payload.Split('\t');

                if (tabParts.Length > TabFieldEntryType)
                {
                    string entryType = tabParts[TabFieldEntryType].Trim();
                    if (entryType == "ENTER" || entryType == "EXIT")
                    {
                        entry.IsApiCall    = true;
                        entry.IsCallEnter  = entryType == "ENTER";
                        entry.IsCallExit   = entryType == "EXIT";
                        entry.ApiName      = tabParts[TabFieldApiName].Trim();
                        entry.SourceFile   = tabParts[TabFieldSourceFile].Trim();
                        entry.Module       = tabParts[TabFieldModule].Trim();

                        if (tabParts.Length > TabFieldEpochMs &&
                            long.TryParse(tabParts[TabFieldEpochMs].Trim(), out long epochMs))
                            entry.EpochMs = epochMs;
                    }
                }
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
                    // Walk the stack looking for the matching ENTER.
                    // This handles cases where an inner ENTER has no matching EXIT
                    // (the API exited abnormally or the log was truncated).
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
}
