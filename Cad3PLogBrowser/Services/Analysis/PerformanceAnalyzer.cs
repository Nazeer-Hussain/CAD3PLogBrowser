namespace Cad3PLogBrowser.Services.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cad3PLogBrowser.Services;

    /// <summary>
    /// Analyzes parsed log entries and the call tree to surface performance insights
    /// that the aggregate Performance tab doesn't show on its own: individual slow
    /// invocations, call-frequency ranking, and call-stack depth.
    /// </summary>
    public class PerformanceAnalyzer
    {
        /// <summary>
        /// Finds the top N slowest individual method calls.
        /// </summary>
        /// <remarks>
        /// This looks at individual invocations, not aggregate statistics.
        /// Useful for finding specific slow executions, not just slow methods in general.
        /// </remarks>
        public List<SlowCallInfo> FindTopSlowestCalls(List<LogEntry> logEntries, int topCount = 10)
        {
            if (logEntries == null || logEntries.Count == 0)
                return new List<SlowCallInfo>();

            var slowCalls = new List<SlowCallInfo>();

            // O(N) stack-based matching, keyed per API name so recursive calls are
            // correctly paired LIFO (same approach as the call-tree builder).
            var enterStacks = new Dictionary<string, Stack<LogEntry>>(StringComparer.Ordinal);

            foreach (var entry in logEntries)
            {
                if (!entry.IsApiCall || string.IsNullOrEmpty(entry.ApiName))
                    continue;

                if (entry.IsCallEnter)
                {
                    if (!enterStacks.TryGetValue(entry.ApiName, out var stack))
                        enterStacks[entry.ApiName] = stack = new Stack<LogEntry>();
                    stack.Push(entry);
                }
                else if (entry.IsCallExit)
                {
                    if (!enterStacks.TryGetValue(entry.ApiName, out var stack) || stack.Count == 0)
                        continue;

                    var enter = stack.Pop();
                    if (enter.EpochMs > 0 && entry.EpochMs >= enter.EpochMs)
                    {
                        slowCalls.Add(new SlowCallInfo
                        {
                            ApiName         = enter.ApiName,
                            EnterLineNumber = enter.LineNumber,
                            ExitLineNumber  = entry.LineNumber,
                            DurationMs      = entry.EpochMs - enter.EpochMs,
                            EpochMs         = enter.EpochMs
                        });
                    }
                }
            }

            return slowCalls
                .OrderByDescending(c => c.DurationMs)
                .Take(topCount)
                .ToList();
        }

        /// <summary>
        /// Finds the most frequently called methods.
        /// </summary>
        public List<FrequentCallInfo> FindMostFrequentlyCalled(List<LogEntry> logEntries, int topCount = 10)
        {
            if (logEntries == null || logEntries.Count == 0)
                return new List<FrequentCallInfo>();

            var callCounts = logEntries
                .Where(e => e.IsApiCall && e.IsCallEnter && !string.IsNullOrEmpty(e.ApiName))
                .GroupBy(e => e.ApiName)
                .Select(g => new FrequentCallInfo
                {
                    ApiName        = g.Key,
                    CallCount      = g.Count(),
                    PercentOfTotal = 0 // calculated below
                })
                .OrderByDescending(f => f.CallCount)
                .Take(topCount)
                .ToList();

            int totalCalls = logEntries.Count(e => e.IsApiCall && e.IsCallEnter);
            foreach (var info in callCounts)
            {
                info.PercentOfTotal = totalCalls > 0
                    ? (double)info.CallCount / totalCalls * 100
                    : 0;
            }

            return callCounts;
        }

        /// <summary>
        /// Analyzes call depth (maximum nesting level) across the call tree.
        /// </summary>
        public CallDepthAnalysis AnalyzeCallDepth(List<CallStackNode> rootNodes)
        {
            if (rootNodes == null || rootNodes.Count == 0)
            {
                return new CallDepthAnalysis
                {
                    MaxDepth      = 0,
                    AvgDepth      = 0,
                    DeepestChains = new List<DeepestChainInfo>()
                };
            }

            int maxDepth = 0;
            var allDepths = new List<int>();
            var deepestChains = new List<DeepestChainInfo>();

            foreach (var root in rootNodes)
                AnalyzeNodeDepth(root, allDepths, ref maxDepth, deepestChains);

            return new CallDepthAnalysis
            {
                MaxDepth      = maxDepth,
                AvgDepth      = allDepths.Count > 0 ? allDepths.Average() : 0,
                DeepestChains = deepestChains.Take(5).ToList()
            };
        }

        private void AnalyzeNodeDepth(
            CallStackNode node,
            List<int> allDepths,
            ref int maxDepth,
            List<DeepestChainInfo> deepestChains)
        {
            // Synthetic per-file group roots (multi-file merges) carry no timing/identity
            // of their own — they'd only pollute both the depth count and the chain text.
            if (!node.IsFileGroupRoot)
            {
                allDepths.Add(node.Depth);

                if (node.Depth > maxDepth)
                {
                    maxDepth = node.Depth;
                    deepestChains.Clear();
                    deepestChains.Add(new DeepestChainInfo { Chain = GetCallChain(node), LineNumber = node.LineNumber });
                }
                else if (node.Depth == maxDepth && node.Depth > 0)
                {
                    deepestChains.Add(new DeepestChainInfo { Chain = GetCallChain(node), LineNumber = node.LineNumber });
                }
            }

            foreach (var child in node.Children)
                AnalyzeNodeDepth(child, allDepths, ref maxDepth, deepestChains);
        }

        /// <summary>Builds the "A → B → C" call chain from the root down to <paramref name="node"/>.</summary>
        private static string GetCallChain(CallStackNode node)
        {
            var chain = new List<string>();
            var current = node;
            while (current != null)
            {
                if (!current.IsFileGroupRoot)
                    chain.Insert(0, current.Label);
                current = current.Parent;
            }
            return string.Join(" → ", chain);
        }
    }

    /// <summary>Represents a single slow call invocation. Used in the "Top Slowest Calls" view.</summary>
    public class SlowCallInfo
    {
        public string ApiName { get; set; }
        public int EnterLineNumber { get; set; }
        public int ExitLineNumber { get; set; }
        public long DurationMs { get; set; }
        /// <summary>Unix epoch milliseconds of the ENTER event, as parsed from the log.</summary>
        public long EpochMs { get; set; }
    }

    /// <summary>Represents a frequently called method. Used in the "Most Frequently Called" view.</summary>
    public class FrequentCallInfo
    {
        public string ApiName { get; set; }
        public int CallCount { get; set; }
        public double PercentOfTotal { get; set; }
    }

    /// <summary>One of the (up to 5) call chains tied for the maximum call-stack depth.</summary>
    public class DeepestChainInfo
    {
        public string Chain { get; set; }
        /// <summary>Log line number of the deepest node's ENTER — used to jump to/highlight it.</summary>
        public int LineNumber { get; set; }
    }

    /// <summary>Results of call depth analysis.</summary>
    public class CallDepthAnalysis
    {
        public int MaxDepth { get; set; }
        public double AvgDepth { get; set; }
        public List<DeepestChainInfo> DeepestChains { get; set; }
    }
}
