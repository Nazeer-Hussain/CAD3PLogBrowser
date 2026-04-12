using System;
using System.Collections.Generic;
using System.Linq;
using Cad3PLogBrowser.Models;
using Cad3PLogBrowser.Services.Analysis;

namespace Cad3PLogBrowser.Services.Analysis
{
    /// <summary>
    /// E5 — Aggregate Statistics Panel.
    /// Computes a summary of the entire log session in one pass.
    /// </summary>
    public class AggregateStatsService
    {
        public AggregateStats Compute(
            List<LogEntry>      entries,
            List<CallStackNode> callTree,
            int                 totalLines)
        {
            var stats = new AggregateStats { TotalLines = totalLines };

            // Counts from raw entries
            foreach (var e in entries)
            {
                if (e.Level == "E") stats.ErrorCount++;
                else if (e.Level == "W") stats.WarningCount++;
                if (e.IsCallEnter) stats.TotalApiCalls++;
            }

            // Unique APIs
            var uniqueApis = new HashSet<string>(StringComparer.Ordinal);
            foreach (var e in entries)
                if (e.IsApiCall && !string.IsNullOrEmpty(e.ApiName))
                    uniqueApis.Add(e.ApiName);
            stats.UniqueApiCount = uniqueApis.Count;

            // Timing from call tree
            var allDurations = new List<long>();
            CollectDurations(callTree, allDurations);

            if (allDurations.Count > 0)
            {
                stats.TotalTrackedMs   = allDurations.Sum();
                stats.MaxCallDurationMs = allDurations.Max();
                stats.AvgCallDurationMs = (long)(allDurations.Average());
                stats.TimedCallCount    = allDurations.Count;
            }

            // Max call depth
            stats.MaxCallDepth = MaxDepth(callTree, 0);

            // Session span from first/last epoch
            long firstMs = long.MaxValue, lastMs = 0;
            foreach (var e in entries)
            {
                if (e.EpochMs <= 0) continue;
                if (e.EpochMs < firstMs) firstMs = e.EpochMs;
                if (e.EpochMs > lastMs)  lastMs  = e.EpochMs;
            }
            if (firstMs != long.MaxValue && lastMs > firstMs)
                stats.SessionDurationMs = lastMs - firstMs;

            return stats;
        }

        private static void CollectDurations(List<CallStackNode> nodes, List<long> acc)
        {
            foreach (var n in nodes)
            {
                if (n.DurationMs > 0) acc.Add(n.DurationMs);
                CollectDurations(n.Children, acc);
            }
        }

        private static int MaxDepth(List<CallStackNode> nodes, int current)
        {
            int max = current;
            foreach (var n in nodes)
                max = Math.Max(max, MaxDepth(n.Children, current + 1));
            return max;
        }
    }

    public class AggregateStats
    {
        public int  TotalLines        { get; set; }
        public int  ErrorCount        { get; set; }
        public int  WarningCount      { get; set; }
        public int  TotalApiCalls     { get; set; }
        public int  UniqueApiCount    { get; set; }
        public int  TimedCallCount    { get; set; }
        public int  MaxCallDepth      { get; set; }
        public long TotalTrackedMs    { get; set; }
        public long MaxCallDurationMs { get; set; }
        public long AvgCallDurationMs { get; set; }
        public long SessionDurationMs { get; set; }
    }
}
