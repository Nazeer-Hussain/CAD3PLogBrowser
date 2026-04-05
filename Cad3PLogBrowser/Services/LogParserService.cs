using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Cad3PLogBrowser.Services
{
    /// <summary>
    /// Parses raw log lines into structured <see cref="LogEntry"/> objects and builds
    /// two tree structures from them:
    ///   - API call list  : flat, one entry per unique API name
    ///   - Call stack tree: hierarchical, reflecting call depth via indentation/brackets
    ///
    /// Detection heuristics (extend as you learn the real log format):
    ///   API call   : line contains a token that looks like  SomeName(  or  [SomeName]
    ///   Call enter : line contains "=>" or "-->" or starts with "ENTER"
    ///   Call exit  : line contains "<=" or "<--" or starts with "EXIT" or "RETURN"
    ///   Depth      : number of leading spaces / 2  (adjust multiplier to taste)
    /// </summary>
    public class LogParserService
    {
        // ── Heuristic patterns (tune to the real log format) ──────────────────
        private static readonly Regex ApiCallPattern =
            new Regex(@"\b([A-Z][A-Za-z0-9_]+)\s*\(", RegexOptions.Compiled);

        private static readonly Regex BracketApiPattern =
            new Regex(@"\[([A-Z][A-Za-z0-9_:]+)\]", RegexOptions.Compiled);

        private static readonly Regex EnterPattern =
            new Regex(@"(=>|-->|\bENTER\b|\bCALL\b)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex ExitPattern =
            new Regex(@"(<=|<--|\bEXIT\b|\bRETURN\b|\bRET\b)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // ── Public parse entry point ──────────────────────────────────────────
        /// <summary>Converts raw string lines into structured <see cref="LogEntry"/> list.</summary>
        public List<LogEntry> Parse(IList<string> rawLines)
        {
            var entries = new List<LogEntry>(rawLines.Count);
            for (int i = 0; i < rawLines.Count; i++)
            {
                string raw = rawLines[i];
                entries.Add(new LogEntry
                {
                    LineNumber  = i + 1,
                    RawText     = raw,
                    ApiName     = ExtractApiName(raw),
                    IsCallEnter = EnterPattern.IsMatch(raw),
                    IsCallExit  = ExitPattern.IsMatch(raw),
                    Depth       = MeasureDepth(raw)
                });
            }
            return entries;
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
                if (string.IsNullOrEmpty(entry.ApiName)) continue;

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
        /// Builds a hierarchical call stack tree from the parsed entries.
        /// Uses indentation depth and ENTER/EXIT markers to determine parent-child relationships.
        /// Returns a list of root-level <see cref="CallStackNode"/> objects.
        /// </summary>
        public List<CallStackNode> BuildCallTree(List<LogEntry> entries)
        {
            var roots   = new List<CallStackNode>();
            var stack   = new Stack<CallStackNode>();

            foreach (var entry in entries)
            {
                if (!entry.IsCallEnter && string.IsNullOrEmpty(entry.ApiName))
                    continue;

                var node = new CallStackNode
                {
                    Label      = string.IsNullOrEmpty(entry.ApiName) ? entry.RawText.Trim() : entry.ApiName,
                    LineNumber = entry.LineNumber,
                    Depth      = entry.Depth
                };

                // Pop stack until we find a node at a shallower depth.
                while (stack.Count > 0 && stack.Peek().Depth >= entry.Depth)
                    stack.Pop();

                if (stack.Count == 0)
                    roots.Add(node);
                else
                    stack.Peek().Children.Add(node);

                if (entry.IsCallEnter || !entry.IsCallExit)
                    stack.Push(node);
            }

            return roots;
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private static string ExtractApiName(string line)
        {
            var m = ApiCallPattern.Match(line);
            if (m.Success) return m.Groups[1].Value;

            m = BracketApiPattern.Match(line);
            if (m.Success) return m.Groups[1].Value;

            return null;
        }

        private static int MeasureDepth(string line)
        {
            int spaces = 0;
            foreach (char c in line)
            {
                if (c == ' ')       spaces++;
                else if (c == '\t') spaces += 4;
                else break;
            }
            return spaces / 2;
        }
    }

    // ── Data models ───────────────────────────────────────────────────────────

    public class LogEntry
    {
        public int    LineNumber  { get; set; }
        public string RawText    { get; set; }
        public string ApiName    { get; set; }
        public bool   IsCallEnter { get; set; }
        public bool   IsCallExit  { get; set; }
        public int    Depth       { get; set; }
    }

    public class ApiCallNode
    {
        public string      ApiName     { get; set; }
        public List<int>   LineNumbers { get; } = new List<int>();
        /// <summary>First line number this API appears on — used to jump to it in the log view.</summary>
        public int         FirstLine   => LineNumbers.Count > 0 ? LineNumbers[0] : -1;
        public override string ToString() =>
            string.Format("{0}  ({1}×)", ApiName, LineNumbers.Count);
    }

    public class CallStackNode
    {
        public string               Label      { get; set; }
        public int                  LineNumber  { get; set; }
        public int                  Depth       { get; set; }
        public List<CallStackNode>  Children   { get; } = new List<CallStackNode>();
        public override string ToString() => Label;
    }
}
