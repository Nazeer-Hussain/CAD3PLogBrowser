using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cad3PLogBrowser.Models;

namespace Cad3PLogBrowser.Services.Analysis
{
    /// <summary>
    /// Offline / rule-based canned-response generator (L1-L6 features).
    ///
    /// This service NEVER calls out to any AI provider — it produces deterministic,
    /// statistics-driven "sample" text entirely on-device. It exists purely as a
    /// fallback used by <see cref="Cad3PLogBrowser.Managers.AiAssistantPanel"/> when
    /// the real AI service (<see cref="Cad3PLogBrowser.AI.Services.AIService"/>) is
    /// disabled, unconfigured, or offline, so the UI always has something useful to
    /// show. Callers are responsible for clearly labeling this output as a SAMPLE
    /// response, not real AI analysis.
    /// </summary>
    public class AiLogService
    {
        public bool IsConfigured => true; // offline always works

        // ── L1: Summarize ─────────────────────────────────────────────────────
        public Task<string> SummarizeAsync(AggregateStats stats, List<ApiPerfStats> perfStats)
        {
            return Task.FromResult(OfflineSummarize(stats, perfStats));
        }

        private string OfflineSummarize(AggregateStats stats, List<ApiPerfStats> perfStats)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== LOG SESSION SUMMARY ===\n");
            string health = stats.ErrorCount > 10 ? "CRITICAL" :
                            (stats.ErrorCount > 0 || stats.WarningCount > 10) ? "WARNING" : "HEALTHY";
            sb.AppendLine($"Status: {health}");
            sb.AppendLine($"Total Lines: {stats.TotalLines:N0}");
            sb.AppendLine($"Errors: {stats.ErrorCount} | Warnings: {stats.WarningCount}");
            sb.AppendLine($"API Calls: {stats.TotalApiCalls:N0} ({stats.UniqueApiCount} unique)");
            sb.AppendLine($"Max Call Depth: {stats.MaxCallDepth}");
            sb.AppendLine($"Session Duration: {stats.SessionDurationMs:N0} ms\n");
            if (perfStats != null && perfStats.Count > 0)
            {
                // Sorted descending by TotalDurationMs by ConvertPerfStats — index directly.
                int top5 = Math.Min(5, perfStats.Count);
                bool hasSlowCall = false;
                for (int i = 0; i < top5; i++)
                    if (perfStats[i].TotalDurationMs > 1000) { hasSlowCall = true; break; }

                if (hasSlowCall)
                {
                    sb.AppendLine("PERFORMANCE CONCERNS:");
                    for (int i = 0; i < top5; i++)
                    {
                        var p = perfStats[i];
                        if (p.TotalDurationMs > 1000)
                            sb.AppendLine($"  {p.ApiName}: {p.TotalDurationMs:N0} ms total ({p.CallCount} calls)");
                    }
                    sb.AppendLine();
                }
                sb.AppendLine("Top 5 Slowest APIs:");
                for (int i = 0; i < top5; i++)
                {
                    var p = perfStats[i];
                    sb.AppendLine($"  {p.ApiName}: {p.TotalDurationMs:N0} ms (avg {p.AvgDurationMs:N0} ms, {p.CallCount} calls)");
                }
            }
            sb.AppendLine("\nRECOMMENDATIONS:");
            if (stats.ErrorCount > 0)    sb.AppendLine($"  Investigate {stats.ErrorCount} error(s) — use F8 to navigate");
            if (stats.WarningCount > 5)  sb.AppendLine($"  Review {stats.WarningCount} warning(s) — use Shift+F8");
            if (stats.MaxCallDepth > 20) sb.AppendLine($"  Call depth of {stats.MaxCallDepth} may indicate recursion");
            if (perfStats != null && perfStats.Any(p => p.TotalDurationMs > 5000))
                sb.AppendLine("  Multiple slow operations detected — check Performance tab");
            return sb.ToString();
        }

        // ── L2: Natural Language Search ───────────────────────────────────────
        public Task<string> NlSearchAsync(string question, AggregateStats stats, List<ApiPerfStats> perfStats)
        {
            return Task.FromResult(OfflineNlSearch(question, stats, perfStats));
        }

        private string OfflineNlSearch(string question, AggregateStats stats, List<ApiPerfStats> perfStats)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Question: \"{question}\"\n");
            string q = question.ToLowerInvariant();
            if (q.Contains("error") || q.Contains("fail"))
            {
                sb.AppendLine($"Error Analysis:");
                sb.AppendLine($"  Total errors: {stats.ErrorCount}");
                if (stats.ErrorCount > 0) sb.AppendLine("  Use F8 to navigate through errors");
                else sb.AppendLine("  No errors detected in this log session");
            }
            else if (q.Contains("slow") || q.Contains("performance") || q.Contains("bottleneck"))
            {
                sb.AppendLine("Performance Analysis:");
                if (perfStats != null && perfStats.Count > 0)
                {
                    // Sorted descending by TotalDurationMs by ConvertPerfStats — index directly.
                    sb.AppendLine("  Slowest operations:");
                    int top3 = Math.Min(3, perfStats.Count);
                    for (int i = 0; i < top3; i++)
                    {
                        var p = perfStats[i];
                        sb.AppendLine(string.Format("    {0}: {1:N0} ms ({2} calls)",
                            p.ApiName, p.TotalDurationMs, p.CallCount));
                    }
                }
            }
            else if (q.Contains("warning"))
            {
                sb.AppendLine($"Warning Analysis:");
                sb.AppendLine($"  Total warnings: {stats.WarningCount}");
                if (stats.WarningCount > 0) sb.AppendLine("  Use Shift+F8 to navigate");
            }
            else if (q.Contains("api") || q.Contains("call"))
            {
                sb.AppendLine("API Call Analysis:");
                sb.AppendLine($"  Total API calls: {stats.TotalApiCalls:N0}");
                sb.AppendLine($"  Unique APIs: {stats.UniqueApiCount}");
                sb.AppendLine($"  Max call depth: {stats.MaxCallDepth}");
            }
            else
            {
                sb.AppendLine("Session Overview:");
                sb.AppendLine($"  Total lines: {stats.TotalLines:N0}");
                sb.AppendLine($"  Errors: {stats.ErrorCount} | Warnings: {stats.WarningCount}");
                sb.AppendLine($"  API calls: {stats.TotalApiCalls:N0} | Duration: {stats.SessionDurationMs:N0} ms");
            }
            return sb.ToString();
        }

        // ── L3: Anomaly Detection ─────────────────────────────────────────────
        public Task<string> DetectAnomaliesAsync(AggregateStats stats, List<ApiPerfStats> perfStats)
        {
            return Task.FromResult(OfflineDetectAnomalies(stats, perfStats));
        }

        private string OfflineDetectAnomalies(AggregateStats stats, List<ApiPerfStats> perfStats)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ANOMALY DETECTION RESULTS\n");
            bool found = false;
            if (stats.ErrorCount > 0)
            {
                double rate = (stats.ErrorCount / (double)stats.TotalLines) * 100;
                if (rate > 5) { sb.AppendLine($"HIGH ERROR RATE: {rate:F2}% ({stats.ErrorCount}/{stats.TotalLines:N0} lines)\n   Investigate immediately\n"); found = true; }
            }
            if (stats.WarningCount > 20) { sb.AppendLine($"HIGH WARNING COUNT: {stats.WarningCount} warnings\n   Review warning messages\n"); found = true; }
            if (stats.MaxCallDepth > 25) { sb.AppendLine($"DEEP CALL STACK: depth {stats.MaxCallDepth}\n   Check for recursion or excessive nesting\n"); found = true; }
            if (perfStats != null && perfStats.Count > 0)
            {
                // P4: single pass instead of three separate LINQ iterations
                // (Average + Where outliers + Where hotspots = 3x N allocations).
                double totalAvgMs    = 0;
                double totalCallsD   = 0;
                foreach (var p in perfStats)
                {
                    totalAvgMs  += p.AvgDurationMs;
                    totalCallsD += p.CallCount;
                }
                double avg      = totalAvgMs  / perfStats.Count;
                double avgCalls = totalCallsD / perfStats.Count;

                var outliers  = new List<ApiPerfStats>();
                var hotspots  = new List<ApiPerfStats>();
                foreach (var p in perfStats)
                {
                    if (p.AvgDurationMs > avg      * 10) outliers.Add(p);
                    if (p.CallCount     > avgCalls * 5)  hotspots.Add(p);
                }

                if (outliers.Count > 0)
                {
                    sb.AppendLine(string.Format("PERFORMANCE OUTLIERS: {0} API(s) with 10x+ average duration", outliers.Count));
                    int show = Math.Min(5, outliers.Count);
                    for (int i = 0; i < show; i++)
                        sb.AppendLine(string.Format("   {0}: {1:N0} ms avg (session avg {2:N0} ms)",
                            outliers[i].ApiName, outliers[i].AvgDurationMs, avg));
                    sb.AppendLine("   Profile these methods for optimization\n");
                    found = true;
                }
                if (hotspots.Count > 0)
                {
                    sb.AppendLine(string.Format("HOTSPOT METHODS: {0} API(s) called 5x+ more than average", hotspots.Count));
                    int show = Math.Min(5, hotspots.Count);
                    for (int i = 0; i < show; i++)
                        sb.AppendLine(string.Format("   {0}: {1} calls (avg {2:N0})",
                            hotspots[i].ApiName, hotspots[i].CallCount, avgCalls));
                    found = true;
                }
            }
            if (!found) sb.AppendLine("No significant anomalies detected.\nSession appears healthy with normal patterns.");
            return sb.ToString();
        }

        // ── L4: Root Cause Suggester ──────────────────────────────────────────
        public Task<string> SuggestRootCauseAsync(AggregateStats stats, List<ApiPerfStats> perfStats,
            int errorCount, int warningCount)
        {
            return Task.FromResult(OfflineRootCause(stats, perfStats, errorCount, warningCount));
        }

        private string OfflineRootCause(AggregateStats stats, List<ApiPerfStats> perfStats,
            int errors, int warnings)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ROOT CAUSE ANALYSIS\n");
            if (errors == 0 && warnings == 0) { sb.AppendLine("No errors or warnings to analyze."); return sb.ToString(); }
            if (errors > 0)
            {
                sb.AppendLine($"ERROR ANALYSIS ({errors} errors):");
                sb.AppendLine("  Possible causes: missing input data, network issues, resource constraints, unhandled exceptions");
                sb.AppendLine("  Next steps:");
                sb.AppendLine("  1. Use F8 to navigate to first error");
                sb.AppendLine("  2. Check error message in Log Details tab");
                sb.AppendLine("  3. Use Call Tree to see call context\n");
            }
            if (warnings > 0)
            {
                sb.AppendLine($"WARNING ANALYSIS ({warnings} warnings):");
                sb.AppendLine("  Common causes: deprecated API usage, configuration issues, performance degradation\n");
            }
            if (perfStats != null && perfStats.Any(p => p.AvgDurationMs > 1000))
            {
                sb.AppendLine("PERFORMANCE ISSUES:");
                // Sorted descending by TotalDurationMs by ConvertPerfStats — index directly.
                sb.AppendLine("Performance impact:");
                int top3rc = Math.Min(3, perfStats.Count);
                for (int i = 0; i < top3rc; i++)
                {
                    var p = perfStats[i];
                    if (p.AvgDurationMs > 1000)
                    {
                        sb.AppendLine($"  {p.ApiName}: {p.AvgDurationMs:N0} ms avg");
                        sb.AppendLine("    Likely causes: I/O operations, database queries, external API calls");
                    }
                }
            }
            return sb.ToString();
        }

        // ── L5: Bug Report Generator ──────────────────────────────────────────
        public Task<string> GenerateBugReportAsync(AggregateStats stats,
            List<ApiPerfStats> perfStats, string appVersion)
        {
            return Task.FromResult(OfflineBugReport(stats, perfStats, appVersion));
        }

        private string OfflineBugReport(AggregateStats stats, List<ApiPerfStats> perfStats, string version)
        {
            var sb = new StringBuilder();
            sb.AppendLine("## Bug Report\n");
            sb.AppendLine($"**Version:** {version}");
            sb.AppendLine($"**Session:** {stats.TotalLines:N0} lines, {stats.SessionDurationMs:N0} ms\n");
            sb.AppendLine($"**Summary:** {stats.ErrorCount} error(s), {stats.WarningCount} warning(s) detected.\n");
            sb.AppendLine("**Performance Impact:**");
            if (perfStats != null && perfStats.Count > 0)
            {
                // Sorted descending by TotalDurationMs by ConvertPerfStats — index directly.
                int top5 = Math.Min(5, perfStats.Count);
                for (int i = 0; i < top5; i++)
                {
                    var p = perfStats[i];
                    sb.AppendLine($"- {p.ApiName}: {p.TotalDurationMs:N0} ms total ({p.CallCount} calls)");
                }
            }
            sb.AppendLine($"\n**Priority:** {(stats.ErrorCount > 10 ? "HIGH" : stats.ErrorCount > 0 ? "MEDIUM" : "LOW")}");
            sb.AppendLine("\n_Tip: Enable Claude API in Settings for an enhanced AI-generated bug report._");
            return sb.ToString();
        }

        // ── L6: Conversational Chat ───────────────────────────────────────────
        public Task<string> ChatAsync(string userMessage,
            List<(string role, string content)> history,
            AggregateStats stats, List<ApiPerfStats> perfStats)
        {
            return Task.FromResult(OfflineNlSearch(userMessage, stats, perfStats));
        }

        // ── L5 (offline): Performance Insights ───────────────────────────────
        public Task<string> AnalyzePerformanceAsync(List<ApiPerfStats> perfStats)
        {
            var sb = new StringBuilder();
            sb.AppendLine("PERFORMANCE INSIGHTS\n");
            if (perfStats == null || !perfStats.Any()) { sb.AppendLine("No performance data available."); return Task.FromResult(sb.ToString()); }
            long total = perfStats.Sum(p => p.TotalDurationMs);
            sb.AppendLine($"Total tracked time: {total:N0} ms");
            sb.AppendLine($"Total calls: {perfStats.Sum(p => p.CallCount):N0}\n");
            sb.AppendLine("Top 10 Time Consumers:");
            // Sorted descending by TotalDurationMs by ConvertPerfStats — index directly.
            int top10ap = Math.Min(10, perfStats.Count);
            for (int i = 0; i < top10ap; i++)
            {
                var p = perfStats[i];
                double pct = total > 0 ? (p.TotalDurationMs / (double)total) * 100 : 0;
                sb.AppendLine($"  {p.ApiName}");
                sb.AppendLine($"    Total: {p.TotalDurationMs:N0} ms ({pct:F1}%) | Calls: {p.CallCount} | Avg: {p.AvgDurationMs:N0} ms | Max: {p.MaxDurationMs:N0} ms");
            }
            return Task.FromResult(sb.ToString());
        }

        // ── L6 (offline): Pattern Recognition ────────────────────────────────
        public Task<string> FindPatternsAsync(List<LogEntry> entries)
        {
            var sb = new StringBuilder();
            sb.AppendLine("PATTERN RECOGNITION\n");
            if (entries == null || !entries.Any()) { sb.AppendLine("No log entries to analyze."); return Task.FromResult(sb.ToString()); }

            // P-07: single combined pass instead of 5 separate LINQ scans over the same list.
            int total        = entries.Count;
            int firstHalfEnd = total / 2;
            int errFirst = 0, errSecond = 0;
            var errorPrefixes  = new Dictionary<string, int>(StringComparer.Ordinal);
            var apiCallCounts  = new Dictionary<string, int>(StringComparer.Ordinal);

            for (int i = 0; i < total; i++)
            {
                var e = entries[i];
                if (e.Level == "E")
                {
                    string key = (e.RawText ?? "").Length > 80
                        ? e.RawText.Substring(0, 80) : e.RawText ?? "";
                    errorPrefixes.TryGetValue(key, out int n);
                    errorPrefixes[key] = n + 1;

                    if (i < firstHalfEnd) errFirst++; else errSecond++;
                }
                if (e.IsApiCall && !string.IsNullOrEmpty(e.ApiName))
                {
                    apiCallCounts.TryGetValue(e.ApiName, out int n);
                    apiCallCounts[e.ApiName] = n + 1;
                }
            }

            bool found = false;

            // Repeated errors
            var repeated = new List<KeyValuePair<string, int>>();
            foreach (var kv in errorPrefixes)
                if (kv.Value > 1) repeated.Add(kv);
            repeated.Sort((a, b) => b.Value.CompareTo(a.Value));
            if (repeated.Count > 0)
            {
                sb.AppendLine("REPEATED ERRORS:");
                for (int i = 0; i < Math.Min(3, repeated.Count); i++)
                    sb.AppendLine(string.Format("  Occurs {0}x: {1}...", repeated[i].Value, repeated[i].Key));
                found = true;
            }

            // Escalating error pattern
            if (total > 100 && errSecond > errFirst * 2)
            {
                sb.AppendLine(string.Format("ESCALATING PATTERN: Errors increase over time: {0} -> {1}", errFirst, errSecond));
                sb.AppendLine("  May indicate degrading system state");
                found = true;
            }

            // High-frequency API calls
            var burst = new List<KeyValuePair<string, int>>();
            foreach (var kv in apiCallCounts)
                if (kv.Value > 100) burst.Add(kv);
            burst.Sort((a, b) => b.Value.CompareTo(a.Value));
            if (burst.Count > 0)
            {
                sb.AppendLine("HIGH-FREQUENCY CALLS:");
                for (int i = 0; i < Math.Min(3, burst.Count); i++)
                    sb.AppendLine(string.Format("  {0}: called {1} times", burst[i].Key, burst[i].Value));
                sb.AppendLine("  May indicate polling, retries, or loops");
                found = true;
            }

            if (!found) sb.AppendLine("No concerning patterns detected. Log activity appears normal.");
            return Task.FromResult(sb.ToString());
        }

        // ── Router for generic query ──────────────────────────────────────────
        public Task<string> AnalyzeAsync(string query, AggregateStats stats,
            List<ApiPerfStats> perfStats, List<LogEntry> entries)
        {
            string q = query.ToLowerInvariant();
            if (q.Contains("pattern") || q.Contains("repeat"))  return FindPatternsAsync(entries);
            if (q.Contains("performance") || q.Contains("slow")) return AnalyzePerformanceAsync(perfStats);
            if (q.Contains("anomaly") || q.Contains("unusual"))  return DetectAnomaliesAsync(stats, perfStats);
            if (q.Contains("root cause") || q.Contains("why"))   return SuggestRootCauseAsync(stats, perfStats, stats.ErrorCount, stats.WarningCount);
            if (q.Contains("bug") || q.Contains("report"))       return GenerateBugReportAsync(stats, perfStats, "");
            if (q.Contains("summary") || q.Contains("overview")) return SummarizeAsync(stats, perfStats);
            return NlSearchAsync(query, stats, perfStats);
        }

        // ── Static helpers used by MainForm ───────────────────────────────────
        public static AggregateStats BuildAggregateStats(List<LogEntry> entries, List<ApiPerfStats> perfStats)
        {
            // BUG-04: single pass replaces 5 separate LINQ scans (Count×4 + Distinct)
            // that previously re-iterated entries[N] five times on every AI call.
            int depth = 0, maxDepth = 0;
            int errorCount = 0, warningCount = 0, apiCallCount = 0;
            var uniqueApis = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);

            foreach (var e in entries)
            {
                if (e.Level == "E") errorCount++;
                else if (e.Level == "W") warningCount++;

                if (e.IsApiCall)
                {
                    apiCallCount++;
                    if (!string.IsNullOrEmpty(e.ApiName)) uniqueApis.Add(e.ApiName);
                    if (e.IsCallEnter) { if (++depth > maxDepth) maxDepth = depth; }
                    else if (e.IsCallExit && depth > 0) depth--;
                }
            }

            var stats = new AggregateStats
            {
                TotalLines     = entries.Count,
                ErrorCount     = errorCount,
                WarningCount   = warningCount,
                TotalApiCalls  = apiCallCount,
                UniqueApiCount = uniqueApis.Count,
                MaxCallDepth   = maxDepth
            };
            if (perfStats != null && perfStats.Any())
            {
                stats.SessionDurationMs = perfStats.Sum(p => p.TotalDurationMs);
                stats.MaxCallDurationMs = perfStats.Max(p => p.MaxDurationMs);
            }
            return stats;
        }

        /// <summary>
        /// Returns a copy of <paramref name="stats"/> sorted descending by
        /// <see cref="ApiPerfStats.TotalDurationMs"/>.
        ///
        /// Sorting here — once, at the AI boundary — guarantees that every
        /// offline method that indexes into the first N entries (OfflineSummarize,
        /// OfflineNlSearch, OfflineRootCause, OfflineBugReport,
        /// AnalyzePerformanceAsync, BuildStructuredSummary) always sees the
        /// globally slowest APIs, regardless of:
        ///   * which column the user has sorted the Performance grid by, or
        ///   * merged sessions where BuildPerformanceStatsGroupedByFile produces
        ///     per-file sorted blocks that are NOT globally sorted when concatenated.
        /// </summary>
        public static List<ApiPerfStats> ConvertPerfStats(List<ApiPerfStats> stats)
        {
            if (stats == null || stats.Count == 0)
                return new List<ApiPerfStats>();

            var sorted = new List<ApiPerfStats>(stats);
            sorted.Sort((a, b) => b.TotalDurationMs.CompareTo(a.TotalDurationMs));
            return sorted;
        }
    }
}
