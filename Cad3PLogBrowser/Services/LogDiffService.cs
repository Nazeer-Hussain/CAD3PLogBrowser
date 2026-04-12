using System;
using System.Collections.Generic;
using System.Linq;
using Cad3PLogBrowser.Models;

namespace Cad3PLogBrowser.Services.Analysis
{
    /// <summary>
    /// E6 — Log Comparison / Diff.
    /// Compares two sets of parsed log entries by API call sequences,
    /// timing, and unique-to-each log entries.
    /// </summary>
    public class LogDiffService
    {
        public LogDiffResult Compare(
            List<LogEntry> linesA, string labelA,
            List<LogEntry> linesB, string labelB)
        {
            var result = new LogDiffResult { LabelA = labelA, LabelB = labelB };

            var apisA = GetApiMap(linesA);
            var apisB = GetApiMap(linesB);

            var allApis = new HashSet<string>(apisA.Keys);
            allApis.UnionWith(apisB.Keys);

            foreach (var api in allApis.OrderBy(a => a))
            {
                bool inA = apisA.TryGetValue(api, out var statsA);
                bool inB = apisB.TryGetValue(api, out var statsB);

                if (inA && !inB)
                    result.OnlyInA.Add(new DiffEntry { ApiName = api, StatsA = statsA });
                else if (!inA && inB)
                    result.OnlyInB.Add(new DiffEntry { ApiName = api, StatsB = statsB });
                else
                {
                    var entry = new DiffEntry
                    {
                        ApiName = api, StatsA = statsA, StatsB = statsB,
                        CallCountDelta   = statsB.CallCount    - statsA.CallCount,
                        AvgTimeDeltaMs   = statsB.AvgDurationMs - statsA.AvgDurationMs,
                        TotalTimeDeltaMs = statsB.TotalDurationMs - statsA.TotalDurationMs
                    };
                    result.InBoth.Add(entry);
                }
            }

            // Sort InBoth by biggest total time delta descending
            result.InBoth.Sort((a, b) =>
                Math.Abs(b.TotalTimeDeltaMs).CompareTo(Math.Abs(a.TotalTimeDeltaMs)));

            return result;
        }

        private static Dictionary<string, ApiStats> GetApiMap(List<LogEntry> entries)
        {
            var map = new Dictionary<string, ApiStats>(StringComparer.Ordinal);
            var stack = new Stack<(string api, long enterMs)>();

            foreach (var entry in entries)
            {
                if (!entry.IsApiCall) continue;
                if (entry.IsCallEnter)
                    stack.Push((entry.ApiName, entry.EpochMs));
                else if (entry.IsCallExit && stack.Count > 0 &&
                         stack.Peek().api == entry.ApiName)
                {
                    var (api, enterMs) = stack.Pop();
                    long dur = entry.EpochMs - enterMs;
                    if (!map.TryGetValue(api, out var s))
                        map[api] = s = new ApiStats { ApiName = api };
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

    public class ApiStats
    {
        public string ApiName        { get; set; }
        public int    CallCount      { get; set; }
        public long   TotalDurationMs { get; set; }
        public long   AvgDurationMs  { get; set; }
        public long   MinDurationMs  { get; set; } = -1;
        public long   MaxDurationMs  { get; set; }
    }

    public class DiffEntry
    {
        public string   ApiName          { get; set; }
        public ApiStats StatsA           { get; set; }
        public ApiStats StatsB           { get; set; }
        public int      CallCountDelta   { get; set; }
        public long     AvgTimeDeltaMs   { get; set; }
        public long     TotalTimeDeltaMs { get; set; }
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
