namespace Cad3PLogBrowser.Services.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cad3PLogBrowser.Models;
    using Cad3PLogBrowser.Utilities;

    /// <summary>
    /// Analyzes log entries to generate performance statistics and insights.
    /// Calculates timing metrics, identifies hotspots, and builds performance summaries.
    /// </summary>
    /// <remarks>
    /// This service performs performance analysis on parsed log data:
    /// - Calculates call counts, durations (total, avg, min, max)
    /// - Identifies self-time (excluding child calls)
    /// - Finds top slowest calls and most frequently called methods
    /// - Generates data for the Performance tab
    /// 
    /// All calculations are done in-memory for speed.
    /// </remarks>
    public class PerformanceAnalyzer
    {
        /// <summary>
        /// Analyzes log entries to generate performance statistics for all API methods.
        /// </summary>
        /// <param name="logEntries">All parsed log entries.</param>
        /// <returns>List of performance statistics, one per unique API method.</returns>
        /// <remarks>
        /// This method:
        /// 1. Groups entries by API name
        /// 2. Matches ENTER/EXIT pairs to calculate durations
        /// 3. Calculates aggregate statistics (count, total, avg, min, max)
        /// 4. Returns sorted list (by total duration descending by default)
        /// 
        /// Only entries with matched ENTER/EXIT pairs contribute to duration statistics.
        /// Unmatched calls are counted but have zero duration.
        /// </remarks>
        /// <example>
        /// var analyzer = new PerformanceAnalyzer();
        /// var stats = analyzer.AnalyzePerformance(logEntries);
        /// 
        /// // Display in grid
        /// foreach (var stat in stats)
        /// {
        ///     Console.WriteLine($"{stat.ApiName}: {stat.CallCount} calls, {stat.AvgDurationMs}ms avg");
        /// }
        /// </example>
        public List<PerformanceStatistics> AnalyzePerformance(List<LogEntry> logEntries)
        {
            if (logEntries == null || logEntries.Count == 0)
                return new List<PerformanceStatistics>();

            // Group API calls by name
            var apiGroups = logEntries
                .Where(e => e.IsApiCall && !string.IsNullOrEmpty(e.ApiName))
                .GroupBy(e => e.ApiName);

            var statsList = new List<PerformanceStatistics>();

            foreach (var group in apiGroups)
            {
                var stat = new PerformanceStatistics
                {
                    ApiName = group.Key,
                    SourceFile = group.FirstOrDefault()?.SourceFile
                };

                // Find all ENTER/EXIT pairs for this API
                var enters = group.Where(e => e.IsCallEnter).ToList();
                var exits = group.Where(e => e.IsCallExit).ToList();

                // Match pairs and calculate durations
                var durations = MatchEnterExitPairs(enters, exits, logEntries);

                // Add each execution to statistics
                foreach (var duration in durations)
                {
                    stat.AddExecution(duration.DurationMs, duration.EnterLineNumber, duration.IsMatched);
                }

                // Finalize averages
                stat.RecalculateAverages();

                statsList.Add(stat);
            }

            // Sort by total duration descending (slowest methods first)
            return statsList.OrderByDescending(s => s.TotalDurationMs).ToList();
        }

        /// <summary>
        /// Matches ENTER lines with their corresponding EXIT lines to calculate durations.
        /// </summary>
        /// <param name="enters">List of ENTER entries for a specific API.</param>
        /// <param name="exits">List of EXIT entries for the same API.</param>
        /// <param name="allEntries">All log entries (for timestamp lookup).</param>
        /// <returns>List of matched pairs with calculated durations.</returns>
        /// <remarks>
        /// Uses a stack-based algorithm to handle nested calls correctly.
        /// If an ENTER has no matching EXIT, the duration is 0 and IsMatched is false.
        /// </remarks>
        private List<CallDuration> MatchEnterExitPairs(
            List<LogEntry> enters,
            List<LogEntry> exits,
            List<LogEntry> allEntries)
        {
            var durations = new List<CallDuration>();
            var enterStack = new Stack<LogEntry>();

            // Process entries in order
            var apiEvents = enters.Concat(exits).OrderBy(e => e.LineNumber).ToList();

            foreach (var evt in apiEvents)
            {
                if (evt.IsCallEnter)
                {
                    enterStack.Push(evt);
                }
                else if (evt.IsCallExit && enterStack.Count > 0)
                {
                    var enter = enterStack.Pop();

                    // Calculate duration
                    long durationMs = 0;
                    if (enter.Timestamp.HasValue && evt.Timestamp.HasValue)
                    {
                        var timeSpan = evt.Timestamp.Value - enter.Timestamp.Value;
                        durationMs = (long)timeSpan.TotalMilliseconds;
                    }

                    durations.Add(new CallDuration
                    {
                        EnterLineNumber = enter.LineNumber,
                        ExitLineNumber = evt.LineNumber,
                        DurationMs = durationMs,
                        IsMatched = true
                    });
                }
            }

            // Handle unmatched ENTERs (no corresponding EXIT)
            while (enterStack.Count > 0)
            {
                var unmatchedEnter = enterStack.Pop();
                durations.Add(new CallDuration
                {
                    EnterLineNumber = unmatchedEnter.LineNumber,
                    ExitLineNumber = 0,
                    DurationMs = 0,
                    IsMatched = false
                });
            }

            return durations;
        }

        /// <summary>
        /// Finds the top N slowest individual method calls.
        /// </summary>
        /// <param name="logEntries">All log entries.</param>
        /// <param name="topCount">Number of results to return (e.g., top 10).</param>
        /// <returns>List of the slowest calls, sorted by duration descending.</returns>
        /// <remarks>
        /// This looks at individual invocations, not aggregate statistics.
        /// Useful for finding specific slow executions, not just slow methods in general.
        /// </remarks>
        public List<SlowCallInfo> FindTopSlowestCalls(List<LogEntry> logEntries, int topCount = 10)
        {
            if (logEntries == null || logEntries.Count == 0)
                return new List<SlowCallInfo>();

            var slowCalls = new List<SlowCallInfo>();

            // O(N) stack-based matching — consistent with MatchEnterExitPairs.
            // Keyed per API name so recursive calls are correctly paired LIFO.
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
                    if (enter.Timestamp.HasValue && entry.Timestamp.HasValue)
                    {
                        var duration = (entry.Timestamp.Value - enter.Timestamp.Value).TotalMilliseconds;
                        slowCalls.Add(new SlowCallInfo
                        {
                            ApiName        = enter.ApiName,
                            EnterLineNumber = enter.LineNumber,
                            ExitLineNumber  = entry.LineNumber,
                            DurationMs      = (long)duration,
                            Timestamp       = enter.Timestamp.Value
                        });
                    }
                }
            }

            // Return top N, sorted by duration descending
            return slowCalls
                .OrderByDescending(c => c.DurationMs)
                .Take(topCount)
                .ToList();
        }

        /// <summary>
        /// Finds the most frequently called methods.
        /// </summary>
        /// <param name="logEntries">All log entries.</param>
        /// <param name="topCount">Number of results to return.</param>
        /// <returns>List of most called methods with call counts.</returns>
        public List<FrequentCallInfo> FindMostFrequentlyCalled(List<LogEntry> logEntries, int topCount = 10)
        {
            if (logEntries == null || logEntries.Count == 0)
                return new List<FrequentCallInfo>();

            var callCounts = logEntries
                .Where(e => e.IsApiCall && e.IsCallEnter && !string.IsNullOrEmpty(e.ApiName))
                .GroupBy(e => e.ApiName)
                .Select(g => new FrequentCallInfo
                {
                    ApiName = g.Key,
                    CallCount = g.Count(),
                    PercentOfTotal = 0 // Calculated below
                })
                .OrderByDescending(f => f.CallCount)
                .Take(topCount)
                .ToList();

            // Calculate percentage of total calls
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
        /// Analyzes call depth (maximum nesting level in the call tree).
        /// </summary>
        /// <param name="rootNodes">Root nodes of the call tree.</param>
        /// <returns>Tuple of (maxDepth, avgDepth, deepestChain).</returns>
        public CallDepthAnalysis AnalyzeCallDepth(List<CallStackNode> rootNodes)
        {
            if (rootNodes == null || rootNodes.Count == 0)
            {
                return new CallDepthAnalysis
                {
                    MaxDepth = 0,
                    AvgDepth = 0,
                    DeepestChains = new List<string>()
                };
            }

            int maxDepth = 0;
            var allDepths = new List<int>();
            var deepestChains = new List<string>();

            foreach (var root in rootNodes)
            {
                AnalyzeNodeDepth(root, allDepths, ref maxDepth, deepestChains);
            }

            return new CallDepthAnalysis
            {
                MaxDepth = maxDepth,
                AvgDepth = allDepths.Count > 0 ? allDepths.Average() : 0,
                DeepestChains = deepestChains.Take(5).ToList()
            };
        }

        /// <summary>
        /// Recursively analyzes depth of a call tree node.
        /// </summary>
        private void AnalyzeNodeDepth(
            CallStackNode node,
            List<int> allDepths,
            ref int maxDepth,
            List<string> deepestChains)
        {
            allDepths.Add(node.Depth);

            if (node.Depth > maxDepth)
            {
                maxDepth = node.Depth;
                deepestChains.Clear();
                deepestChains.Add(node.GetCallChain());
            }
            else if (node.Depth == maxDepth)
            {
                deepestChains.Add(node.GetCallChain());
            }

            foreach (var child in node.Children)
            {
                AnalyzeNodeDepth(child, allDepths, ref maxDepth, deepestChains);
            }
        }
    }

    /// <summary>
    /// Represents a single slow call invocation.
    /// Used in "Top N Slowest Calls" view.
    /// </summary>
    public class SlowCallInfo
    {
        public string ApiName { get; set; }
        public int EnterLineNumber { get; set; }
        public int ExitLineNumber { get; set; }
        public long DurationMs { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Represents a frequently called method.
    /// Used in "Most Frequently Called" view.
    /// </summary>
    public class FrequentCallInfo
    {
        public string ApiName { get; set; }
        public int CallCount { get; set; }
        public double PercentOfTotal { get; set; }
    }

    /// <summary>
    /// Results of call depth analysis.
    /// </summary>
    public class CallDepthAnalysis
    {
        public int MaxDepth { get; set; }
        public double AvgDepth { get; set; }
        public List<string> DeepestChains { get; set; }
    }

    /// <summary>
    /// Internal helper class for tracking call durations.
    /// </summary>
    internal class CallDuration
    {
        public int EnterLineNumber { get; set; }
        public int ExitLineNumber { get; set; }
        public long DurationMs { get; set; }
        public bool IsMatched { get; set; }
    }
}
