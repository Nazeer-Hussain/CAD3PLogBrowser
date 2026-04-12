using System;
using System.Collections.Generic;
using System.Linq;
using Cad3PLogBrowser.Models;

namespace Cad3PLogBrowser.Services.Analysis
{
    /// <summary>
    /// E6 — Log Comparison / Diff.
    /// Compares two parsed log sets by API timing statistics.
    /// </summary>
    public class LogDiffService
    {
        public LogDiffResult Compare(
            List<LogEntry> entriesA, string labelA,
            List<LogEntry> entriesB, string labelB)
        {
            var result = new LogDiffResult { LabelA = labelA, LabelB = labelB };
            var mapA = BuildApiStats(entriesA);
            var mapB = BuildApiStats(entriesB);

            var all = new HashSet<string>(mapA.Keys);
            all.UnionWith(mapB.Keys);

            foreach (var api in all.OrderBy(a => a))
            {
                bool inA = mapA.TryGetValue(api, out var sA);
                bool inB = mapB.TryGetValue(api, out var sB);

                if (inA && !inB)       result.OnlyInA.Add(new DiffEntry { ApiName = api, StatsA = sA });
                else if (!inA && inB)  result.OnlyInB.Add(new DiffEntry { ApiName = api, StatsB = sB });
                else
                    result.InBoth.Add(new DiffEntry
                    {
                        ApiName          = api, StatsA = sA, StatsB = sB,
                        CallCountDelta   = sB.CallCount     - sA.CallCount,
                        AvgTimeDeltaMs   = sB.AvgDurationMs - sA.AvgDurationMs,
                        TotalTimeDeltaMs = sB.TotalDurationMs - sA.TotalDurationMs
                    });
            }

            result.InBoth.Sort((a, b) =>
                Math.Abs(b.TotalTimeDeltaMs).CompareTo(Math.Abs(a.TotalTimeDeltaMs)));

            return result;
        }

        private static Dictionary<string, DiffApiStats> BuildApiStats(List<LogEntry> entries)
        {
            var map   = new Dictionary<string, DiffApiStats>(StringComparer.Ordinal);
            var stack = new Stack<(string api, long enterMs)>();

            foreach (var e in entries)
            {
                if (!e.IsApiCall) continue;
                if (e.IsCallEnter) { stack.Push((e.ApiName, e.EpochMs)); continue; }
                if (e.IsCallExit && stack.Count > 0 && stack.Peek().api == e.ApiName)
                {
                    var (api, t0) = stack.Pop();
                    long dur = e.EpochMs - t0;
                    if (!map.TryGetValue(api, out var s))
                        map[api] = s = new DiffApiStats { ApiName = api };
                    s.CallCount++;
                    s.TotalDurationMs += dur;
                    if (dur < s.MinDurationMs || s.MinDurationMs < 0) s.MinDurationMs = dur;
                    if (dur > s.MaxDurationMs) s.MaxDurationMs = dur;
                }
            }
            foreach (var s in map.Values)
                s.AvgDurationMs = s.CallCount > 0 ? s.TotalDurationMs / s.CallCount : 0;
            return map;
        }
    }

    public class DiffApiStats
    {
        public string ApiName         { get; set; }
        public int    CallCount       { get; set; }
        public long   TotalDurationMs { get; set; }
        public long   AvgDurationMs   { get; set; }
        public long   MinDurationMs   { get; set; } = -1;
        public long   MaxDurationMs   { get; set; }
    }

    public class DiffEntry
    {
        public string      ApiName          { get; set; }
        public DiffApiStats StatsA          { get; set; }
        public DiffApiStats StatsB          { get; set; }
        public int          CallCountDelta   { get; set; }
        public long         AvgTimeDeltaMs   { get; set; }
        public long         TotalTimeDeltaMs { get; set; }
    }

    public class LogDiffResult
    {
        public string          LabelA  { get; set; }
        public string          LabelB  { get; set; }
        public List<DiffEntry> OnlyInA { get; } = new List<DiffEntry>();
        public List<DiffEntry> OnlyInB { get; } = new List<DiffEntry>();
        public List<DiffEntry> InBoth  { get; } = new List<DiffEntry>();
    }
}
