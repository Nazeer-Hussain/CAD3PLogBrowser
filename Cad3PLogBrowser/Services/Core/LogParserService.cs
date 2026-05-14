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

        // ── Format detection ──────────────────────────────────────────────────
        // Format A (CADAPP):  2026-03-31T07:48:00.304Z: D: P6c24: T5aa4: uwgmapp: UWGM_ADAPTER: {tab-payload}
        // Format B (FILESYNC):  04/16/2026 17:29:11 [Thread:40056]: {tab-payload or plain text}
        //   Detected by: raw starts with MM/DD/ pattern — digit, digit, '/'
        private static bool IsFileSyncFormat(string raw) =>
            raw.Length > 10 && char.IsDigit(raw[0]) && char.IsDigit(raw[1]) && raw[2] == '/';

        private static LogEntry ParseLine(string raw, int lineNumber)
        {
            var entry = new LogEntry
            {
                LineNumber = lineNumber,
                RawText    = raw
            };

            if (string.IsNullOrEmpty(raw))
                return entry;

            // ── Issue 3 Fix: strip [filename] prefix added by MergeLogService ──────
            // Merged lines look like: "[log1.txt] 2026-03-31T07:48:00.304Z: D: ..."
            // Only strip if the bracket content looks like a filename (contains a '.')
            // to avoid false-positives on FileSync lines that start with "[Thread:NNN]".
            string line = raw;
            if (line.Length > 2 && line[0] == '[')
            {
                int closingBracket = line.IndexOf("] ", StringComparison.Ordinal);
                if (closingBracket > 1)
                {
                    string bracketContent = line.Substring(1, closingBracket - 1);
                    if (bracketContent.IndexOf('.') >= 0) // looks like a filename
                    {
                        entry.SourceLogFile = bracketContent;
                        line = line.Substring(closingBracket + 2);
                    }
                }
            }

            string payload;

            if (IsFileSyncFormat(line))
            {
                // ── Format B: MM/DD/YYYY HH:MM:SS [Thread:NNN]: {payload} ──────
                // Find ]: to locate the end of the thread prefix.
                const string threadEnd = "]: ";
                int threadEndIdx = line.IndexOf(threadEnd, StringComparison.Ordinal);
                if (threadEndIdx < 0)
                    return entry;

                // Extract thread ID from [Thread:NNN]
                const string threadTag = "[Thread:";
                int threadTagIdx = line.LastIndexOf(threadTag, threadEndIdx, StringComparison.Ordinal);
                if (threadTagIdx >= 0)
                    entry.ThreadId = line.Substring(threadTagIdx + threadTag.Length,
                        threadEndIdx - (threadTagIdx + threadTag.Length));

                payload = line.Substring(threadEndIdx + threadEnd.Length);
                entry.Level = "D"; // INV logs carry no explicit level — treat as Debug
            }
            else
            {
                // ── Format A: ISO colon-separated prefix ──────────────────────
                // Walk colon-separated fields without allocating a string array.
                // Format: {DateTime}: {Level}: {PID}: {TID}: {App}: {Area}: {payload}
                const string sep = ": ";
                int pos = 0;
                int fieldIndex = 0;
                int payloadStart = -1;

                while (pos < line.Length)
                {
                    int next = line.IndexOf(sep, pos, StringComparison.Ordinal);
                    if (next < 0 || fieldIndex >= 6)
                    {
                        if (fieldIndex == 6) payloadStart = pos;
                        break;
                    }

                    string field = line.Substring(pos, next - pos);

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

                if (fieldIndex == 6) payloadStart = pos;

                if (payloadStart < 0 || payloadStart >= line.Length)
                    return entry;

                payload = line.Substring(payloadStart);
            }

            // ── Shared tab-payload parsing ─────────────────────────────────────
            // Fields by index: 0=State  1=Module  2=SourceFile  3=ApiName  4=EntryType  5=EpochMs
            int[] tabPos = new int[7];
            int tabCount = 0;
            tabPos[0] = 0;
            for (int i = 0; i < payload.Length && tabCount < 6; i++)
            {
                if (payload[i] == '\t')
                    tabPos[++tabCount] = i + 1;
            }

            // Need at least 5 tabs to reach field 4 (EntryType)
            if (tabCount < TabFieldEntryType)
                return entry;

            int FieldEnd(int n) => (n + 1 <= tabCount) ? tabPos[n + 1] - 1 : payload.Length;

            // Field 4: EntryType ("ENTER" or "EXIT")
            string entryType = payload.Substring(tabPos[TabFieldEntryType],
                FieldEnd(TabFieldEntryType) - tabPos[TabFieldEntryType]).Trim();

            if (entryType != "ENTER" && entryType != "EXIT")
                return entry;

            entry.IsApiCall   = true;
            entry.IsCallEnter = entryType == "ENTER";
            entry.IsCallExit  = !entry.IsCallEnter;

            // Field 3: ApiName
            entry.ApiName = payload.Substring(tabPos[TabFieldApiName],
                FieldEnd(TabFieldApiName) - tabPos[TabFieldApiName]).Trim();

            // Field 2: SourceFile
            entry.SourceFile = payload.Substring(tabPos[TabFieldSourceFile],
                FieldEnd(TabFieldSourceFile) - tabPos[TabFieldSourceFile]).Trim();

            // Field 1: Module
            entry.Module = payload.Substring(tabPos[TabFieldModule],
                FieldEnd(TabFieldModule) - tabPos[TabFieldModule]).Trim();

            // Field 5: EpochMs — present when tabCount >= 5
            if (tabCount >= TabFieldEpochMs)
            {
                string epochStr = payload.Substring(tabPos[TabFieldEpochMs],
                    FieldEnd(TabFieldEpochMs) - tabPos[TabFieldEpochMs]).Trim();
                long.TryParse(epochStr, out long epochMs);
                entry.EpochMs = epochMs;
            }

            return entry;
        }

        // ── API list (flat, unique names) ─────────────────────────────────────
        /// <summary>
        /// Returns one <see cref="ApiCallNode"/> per unique API name found in the log.
        /// <see cref="ApiCallNode.LineNumbers"/> contains only the ENTER line for each
        /// invocation so that each call is counted exactly once.
        /// Invocations that have only an EXIT (no matching ENTER) are tracked separately
        /// in <see cref="ApiCallNode.ExitOnlyLines"/>.
        /// </summary>
        public List<ApiCallNode> BuildApiList(List<LogEntry> entries)
        {
            var map   = new Dictionary<string, ApiCallNode>(StringComparer.Ordinal);
            // Running count of unmatched ENTERs per API.
            // ENTER → depth++   |   EXIT when depth>0 → depth-- (matched, no node added)
            //                         EXIT when depth==0 → orphan EXIT → ExitOnlyLines
            var depth = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var entry in entries)
            {
                if (!entry.IsApiCall) continue;

                if (!map.TryGetValue(entry.ApiName, out var node))
                {
                    node = new ApiCallNode { ApiName = entry.ApiName };
                    map[entry.ApiName]   = node;
                    depth[entry.ApiName] = 0;
                }

                if (entry.IsCallEnter)
                {
                    node.LineNumbers.Add(entry.LineNumber);
                    depth[entry.ApiName]++;
                }
                else if (entry.IsCallExit)
                {
                    if (depth[entry.ApiName] > 0)
                        depth[entry.ApiName]--;          // normal matched EXIT — no extra node
                    else
                        node.ExitOnlyLines.Add(entry.LineNumber); // orphan EXIT with no ENTER
                }
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
            var roots     = new List<CallStackNode>();
            var stack     = new Stack<CallStackNode>();

            // P-05: per-name frame stacks give O(1) EXIT pre-check.
            // Before: every EXIT popped the whole stack looking for a match, then restored
            // on failure — O(depth) work per EXIT, O(N×depth) total.
            // Now: we skip the stack scan entirely when no matching ENTER exists in the
            // dictionary (common for partial/truncated logs), and for matched EXITs we still
            // pop down but without any allocations or restores.
            var nameFrames = new Dictionary<string, Stack<CallStackNode>>(StringComparer.Ordinal);

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

                    if (stack.Count == 0) roots.Add(node);
                    else                  stack.Peek().Children.Add(node);

                    stack.Push(node);

                    // Register in per-name stack for fast EXIT lookup.
                    if (!nameFrames.TryGetValue(entry.ApiName, out var nf))
                        nameFrames[entry.ApiName] = nf = new Stack<CallStackNode>();
                    nf.Push(node);
                }
                else if (entry.IsCallExit)
                {
                    if (stack.Count == 0) continue;

                    // O(1) pre-check: skip entirely if no matching ENTER is on the stack.
                    if (!nameFrames.TryGetValue(entry.ApiName, out var nf) || nf.Count == 0)
                        continue;

                    // A match exists — pop the main stack down to it.
                    // Intermediate nodes are intentionally orphaned (their EXIT was not seen).
                    while (stack.Count > 0)
                    {
                        var top = stack.Pop();

                        // Keep the per-name frame stacks consistent for every popped node.
                        if (nameFrames.TryGetValue(top.Label, out var topNf)
                            && topNf.Count > 0
                            && ReferenceEquals(topNf.Peek(), top))
                            topNf.Pop();

                        if (top.Label == entry.ApiName)
                        {
                            top.ExitLineNumber = entry.LineNumber;
                            top.ExitEpochMs    = entry.EpochMs;
                            if (top.EpochMs > 0 && entry.EpochMs > 0)
                                top.DurationMs = entry.EpochMs - top.EpochMs;
                            break;
                        }
                    }
                }
            }

            return roots;
        }

        // ── Per-file call tree (for merged logs) ───────────────────────────────
        /// <summary>
        /// Builds one independent call tree per source log file.
        /// When multiple files are merged, this prevents ENTER/EXIT pairs from
        /// different files cross-matching and producing a single interleaved tree.
        /// Each file's tree is returned as a synthetic root <see cref="CallStackNode"/>
        /// labelled with the filename. Falls back to <see cref="BuildCallTree"/> when
        /// all entries share the same (or empty) <see cref="LogEntry.SourceLogFile"/>.
        /// </summary>
        public List<CallStackNode> BuildCallTreeGroupedByFile(List<LogEntry> entries)
        {
            // Collect distinct, non-empty source file tags while preserving order
            var fileOrder = new List<string>();
            var fileGroups = new Dictionary<string, List<LogEntry>>(StringComparer.Ordinal);

            foreach (var entry in entries)
            {
                string key = string.IsNullOrEmpty(entry.SourceLogFile) ? string.Empty : entry.SourceLogFile;
                if (!fileGroups.TryGetValue(key, out var group))
                {
                    group = new List<LogEntry>();
                    fileGroups[key] = group;
                    fileOrder.Add(key);
                }
                group.Add(entry);
            }

            // Single file (or no tag) — normal tree, no synthetic wrapper
            if (fileOrder.Count <= 1)
                return BuildCallTree(entries);

            // Multiple files — build an independent tree per file
            var result = new List<CallStackNode>();
            foreach (var fileName in fileOrder)
            {
                var fileEntries = fileGroups[fileName];
                var fileRoots   = BuildCallTree(fileEntries);

                // Wrap the per-file roots under a labelled synthetic node.
                // BUG-B08: LineNumber = 0 so BuildCallStackNodeIndex skips it
                // (only nodes with LineNumber > 0 are indexed). Setting it to
                // fileRoots[0].LineNumber caused the first real child and the
                // synthetic root to share the same key, with the last writer
                // winning and producing wrong performance subtree filter ranges.
                var fileRoot = new CallStackNode
                {
                    Label           = string.IsNullOrEmpty(fileName) ? "(unknown file)" : fileName,
                    LineNumber      = 0, // intentionally 0 — synthetic wrapper, not a real call
                    Depth           = 0,
                    IsFileGroupRoot = true
                };
                foreach (var root in fileRoots)
                {
                    root.Depth++;
                    fileRoot.Children.Add(root);
                }
                result.Add(fileRoot);
            }
            return result;
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

        private static void CollectStats(CallStackNode node, Dictionary<string, ApiPerfStats> map,
            string sourceLogFile = null, int depth = 0)
        {
            if (depth > 500) return; // BUG-B07: guard against StackOverflowException on deep trees

            // BUG-10: propagate sourceLogFile from the node itself when the caller
            // does not supply one (single-file loads). Without this, every
            // ApiPerfStats.SourceLogFile was null for non-merged loads, making the
            // Performance tab's "Log File" column permanently blank.
            string effectiveFile = sourceLogFile ?? node.SourceFile;

            if (!map.TryGetValue(node.Label, out var stats))
            {
                stats = new ApiPerfStats { ApiName = node.Label, SourceFile = node.SourceFile, SourceLogFile = effectiveFile };
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
                CollectStats(child, map, effectiveFile, depth + 1);
        }

        // ── Per-file performance statistics (for merged logs) ─────────────────
        /// <summary>
        /// Builds aggregated performance statistics independently for each source log file.
        /// When multiple files are merged each file's stats are calculated in isolation so
        /// timings are not contaminated by cross-file call pairing.
        /// Each returned <see cref="ApiPerfStats"/> is tagged with its <see cref="ApiPerfStats.SourceLogFile"/>.
        /// Falls back to <see cref="BuildPerformanceStats"/> for single-file (or untagged) data.
        /// </summary>
        public List<ApiPerfStats> BuildPerformanceStatsGroupedByFile(List<LogEntry> entries)
        {
            // Group entries by source log file
            var fileOrder  = new List<string>();
            var fileGroups = new Dictionary<string, List<LogEntry>>(StringComparer.Ordinal);

            foreach (var entry in entries)
            {
                string key = string.IsNullOrEmpty(entry.SourceLogFile) ? string.Empty : entry.SourceLogFile;
                if (!fileGroups.TryGetValue(key, out var group))
                {
                    group = new List<LogEntry>();
                    fileGroups[key] = group;
                    fileOrder.Add(key);
                }
                group.Add(entry);
            }

            // Single file — fall back to normal (untagged) stats
            if (fileOrder.Count <= 1)
                return BuildPerformanceStats(BuildCallTree(entries));

            var result = new List<ApiPerfStats>();

            foreach (var fileName in fileOrder)
            {
                var fileEntries = fileGroups[fileName];
                var fileRoots   = BuildCallTree(fileEntries);

                var map = new Dictionary<string, ApiPerfStats>(StringComparer.Ordinal);
                foreach (var root in fileRoots)
                    CollectStats(root, map, fileName);

                var fileStats = new List<ApiPerfStats>(map.Values);
                fileStats.Sort((a, b) => b.TotalDurationMs.CompareTo(a.TotalDurationMs));
                result.AddRange(fileStats);
            }

            return result;
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
        /// <summary>Source log filename tag added by MergeLogService. Empty for single-file loads.</summary>
        public string SourceLogFile { get; set; }
    }

    /// <summary>One unique API name with all line numbers it appears on.</summary>
    public class ApiCallNode
    {
        public string    ApiName      { get; set; }
        /// <summary>Line numbers of ENTER calls (one per invocation).</summary>
        public List<int> LineNumbers  { get; } = new List<int>();
        /// <summary>EXIT lines that had no matching ENTER (orphan EXITs).</summary>
        public List<int> ExitOnlyLines { get; } = new List<int>();
        public int       FirstLine    => LineNumbers.Count > 0 ? LineNumbers[0]
                                      : ExitOnlyLines.Count > 0 ? ExitOnlyLines[0] : -1;
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
        /// <summary>
        /// True for synthetic root nodes created by <see cref="LogParserService.BuildCallTreeGroupedByFile"/>
        /// to represent a source log file. These nodes carry no timing data of their own;
        /// their children are the actual top-level calls from that file.
        /// </summary>
        public bool                IsFileGroupRoot { get; set; }
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
        /// <summary>Source log filename (from MergeLogService tag). Empty for single-file loads.</summary>
        public string SourceLogFile   { get; set; }
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