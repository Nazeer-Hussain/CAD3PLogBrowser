using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cad3PLogBrowser.Models;

namespace Cad3PLogBrowser.Services.Analysis
{
    /// <summary>
    /// L1-L6 - AI Features for intelligent log analysis.
    /// Provides offline, heuristic-based analysis of log statistics and patterns.
    /// No external API calls are made — all analysis runs entirely offline.
    /// </summary>
    public class AiLogService
    {
        /// <summary>
        /// Always false: this service performs offline heuristic analysis only.
        /// No external AI API is called regardless of any configured API key.
        /// </summary>
        public bool IsConfigured => false;

        // ?? L1: AI Log Summarizer ????????????????????????????????????????????
        public Task<string> SummarizeAsync(AggregateStats stats, List<ApiPerfStats> perfStats)
        {
            var summary = new StringBuilder();
            summary.AppendLine("=== LOG SESSION SUMMARY ===\n");

            // Overall health assessment
            string health = "HEALTHY ?";
            if (stats.ErrorCount > 10) health = "CRITICAL ?";
            else if (stats.ErrorCount > 0 || stats.WarningCount > 10) health = "WARNING ??";

            summary.AppendLine($"Status: {health}");
            summary.AppendLine($"Total Lines: {stats.TotalLines:N0}");
            summary.AppendLine($"Errors: {stats.ErrorCount} | Warnings: {stats.WarningCount}");
            summary.AppendLine($"API Calls: {stats.TotalApiCalls:N0} ({stats.UniqueApiCount} unique)");
            summary.AppendLine($"Max Call Depth: {stats.MaxCallDepth}");
            summary.AppendLine($"Session Duration: {stats.SessionDurationMs:N0} ms");
            summary.AppendLine();

            // Notable slow calls
            if (perfStats != null && perfStats.Any())
            {
                var slowest = perfStats.OrderByDescending(p => p.TotalDurationMs).Take(5).ToList();
                if (slowest.Any(p => p.TotalDurationMs > 1000))
                {
                    summary.AppendLine("?? PERFORMANCE CONCERNS:");
                    foreach (var p in slowest.Where(x => x.TotalDurationMs > 1000))
                    {
                        summary.AppendLine($"  • {p.ApiName}: {p.TotalDurationMs:N0} ms total ({p.CallCount} calls, avg {p.AvgDurationMs:N0} ms)");
                    }
                    summary.AppendLine();
                }

                summary.AppendLine("Top 5 Slowest APIs:");
                foreach (var p in slowest)
                {
                    summary.AppendLine($"  {p.ApiName}: {p.TotalDurationMs:N0} ms (avg {p.AvgDurationMs:N0} ms, {p.CallCount} calls)");
                }
                summary.AppendLine();
            }

            // Recommendations
            summary.AppendLine("?? RECOMMENDATIONS:");
            if (stats.ErrorCount > 0)
                summary.AppendLine($"  • Investigate {stats.ErrorCount} error(s) - use F8 to navigate");
            if (stats.WarningCount > 5)
                summary.AppendLine($"  • Review {stats.WarningCount} warning(s) - use Ctrl+F8 to navigate");
            if (stats.MaxCallDepth > 20)
                summary.AppendLine($"  • Call depth of {stats.MaxCallDepth} may indicate recursive issues");
            if (perfStats != null && perfStats.Any(p => p.TotalDurationMs > 5000))
                summary.AppendLine("  • Multiple slow operations detected - consider optimization");

            return Task.FromResult(summary.ToString());
        }

        // ?? L2: Natural Language Search ??????????????????????????????????????
        public Task<string> NlSearchAsync(string userQuestion, AggregateStats stats, List<ApiPerfStats> perfStats)
        {
            var response = new StringBuilder();
            response.AppendLine($"?? Question: \"{userQuestion}\"\n");

            string questionLower = userQuestion.ToLowerInvariant();

            // Handle common questions
            if (questionLower.Contains("error") || questionLower.Contains("fail"))
            {
                response.AppendLine($"?? Error Analysis:");
                response.AppendLine($"  • Total errors found: {stats.ErrorCount}");
                if (stats.ErrorCount > 0)
                {
                    response.AppendLine($"  • Use F8 to navigate through errors");
                    response.AppendLine($"  • Export with filters to isolate error lines");
                }
                else
                {
                    response.AppendLine($"  • No errors detected in this log session ?");
                }
            }
            else if (questionLower.Contains("slow") || questionLower.Contains("performance") || questionLower.Contains("bottleneck"))
            {
                response.AppendLine($"? Performance Analysis:");
                if (perfStats != null && perfStats.Any())
                {
                    var slowest = perfStats.OrderByDescending(p => p.TotalDurationMs).Take(3).ToList();
                    response.AppendLine($"  • Slowest operations:");
                    foreach (var p in slowest)
                    {
                        response.AppendLine($"    - {p.ApiName}: {p.TotalDurationMs:N0} ms ({p.CallCount} calls)");
                    }
                    response.AppendLine($"  • Check Performance tab for detailed metrics");
                }
            }
            else if (questionLower.Contains("warning"))
            {
                response.AppendLine($"?? Warning Analysis:");
                response.AppendLine($"  • Total warnings: {stats.WarningCount}");
                if (stats.WarningCount > 0)
                {
                    response.AppendLine($"  • Use Ctrl+F8 to navigate through warnings");
                }
            }
            else if (questionLower.Contains("api") || questionLower.Contains("call"))
            {
                response.AppendLine($"?? API Call Analysis:");
                response.AppendLine($"  • Total API calls: {stats.TotalApiCalls:N0}");
                response.AppendLine($"  • Unique APIs: {stats.UniqueApiCount}");
                response.AppendLine($"  • Max call depth: {stats.MaxCallDepth}");
                response.AppendLine($"  • View API Tree for detailed breakdown");
            }
            else
            {
                // General statistics
                response.AppendLine($"?? Session Overview:");
                response.AppendLine($"  • Total lines: {stats.TotalLines:N0}");
                response.AppendLine($"  • Errors: {stats.ErrorCount} | Warnings: {stats.WarningCount}");
                response.AppendLine($"  • API calls: {stats.TotalApiCalls:N0}");
                response.AppendLine($"  • Duration: {stats.SessionDurationMs:N0} ms");
            }

            return Task.FromResult(response.ToString());
        }

        // ?? L3: Anomaly Detection ????????????????????????????????????????????
        public Task<string> DetectAnomaliesAsync(AggregateStats stats, List<ApiPerfStats> perfStats)
        {
            var anomalies = new StringBuilder();
            anomalies.AppendLine("?? ANOMALY DETECTION RESULTS\n");

            bool foundAnomalies = false;

            // Check for high error rates
            if (stats.ErrorCount > 0)
            {
                double errorRate = (stats.ErrorCount / (double)stats.TotalLines) * 100;
                if (errorRate > 5)
                {
                    anomalies.AppendLine($"?? HIGH ERROR RATE: {errorRate:F2}% ({stats.ErrorCount}/{stats.TotalLines:N0} lines)");
                    anomalies.AppendLine($"   Recommendation: Investigate root cause immediately\n");
                    foundAnomalies = true;
                }
            }

            // Check for excessive warnings
            if (stats.WarningCount > 20)
            {
                double warnRate = (stats.WarningCount / (double)stats.TotalLines) * 100;
                anomalies.AppendLine($"?? HIGH WARNING COUNT: {stats.WarningCount} warnings ({warnRate:F2}%)");
                anomalies.AppendLine($"   Recommendation: Review warning messages\n");
                foundAnomalies = true;
            }

            // Check for deep call stacks
            if (stats.MaxCallDepth > 25)
            {
                anomalies.AppendLine($"?? DEEP CALL STACK: Maximum depth of {stats.MaxCallDepth}");
                anomalies.AppendLine($"   Recommendation: Check for recursion or excessive nesting\n");
                foundAnomalies = true;
            }

            // Check for performance outliers
            if (perfStats != null && perfStats.Any())
            {
                double avgDuration = perfStats.Average(p => (double)p.AvgDurationMs);
                var outliers = perfStats.Where(p => p.AvgDurationMs > avgDuration * 10).ToList();

                if (outliers.Any())
                {
                    anomalies.AppendLine($"? PERFORMANCE OUTLIERS: {outliers.Count} API(s) with 10x+ average duration");
                    foreach (var p in outliers.Take(5))
                    {
                        anomalies.AppendLine($"   • {p.ApiName}: {p.AvgDurationMs:N0} ms avg (vs session avg {avgDuration:N0} ms)");
                    }
                    anomalies.AppendLine($"   Recommendation: Profile these methods for optimization\n");
                    foundAnomalies = true;
                }

                // Check for unbalanced call counts
                double avgCalls = perfStats.Average(p => (double)p.CallCount);
                var hotspots = perfStats.Where(p => p.CallCount > avgCalls * 5).ToList();

                if (hotspots.Any())
                {
                    anomalies.AppendLine($"?? HOTSPOT METHODS: {hotspots.Count} API(s) called 5x+ more than average");
                    foreach (var p in hotspots.Take(5))
                    {
                        anomalies.AppendLine($"   • {p.ApiName}: {p.CallCount} calls (vs avg {avgCalls:N0})");
                    }
                    anomalies.AppendLine($"   Recommendation: Review call patterns\n");
                    foundAnomalies = true;
                }
            }

            if (!foundAnomalies)
            {
                anomalies.AppendLine("? No significant anomalies detected.");
                anomalies.AppendLine("\nSession appears healthy with normal patterns.");
            }

            return Task.FromResult(anomalies.ToString());
        }

        // ?? L4: Root Cause Analysis ??????????????????????????????????????????
        public Task<string> SuggestRootCauseAsync(AggregateStats stats, List<ApiPerfStats> perfStats, 
            int errorCount, int warningCount)
        {
            var analysis = new StringBuilder();
            analysis.AppendLine("?? ROOT CAUSE ANALYSIS\n");

            if (errorCount == 0 && warningCount == 0)
            {
                analysis.AppendLine("? No errors or warnings to analyze.");
                return Task.FromResult(analysis.ToString());
            }

            // Error root cause suggestions
            if (errorCount > 0)
            {
                analysis.AppendLine($"ERROR ANALYSIS ({errorCount} errors):");
                analysis.AppendLine($"  Possible causes:");
                analysis.AppendLine($"  • Missing or invalid input data");
                analysis.AppendLine($"  • Network connectivity issues");
                analysis.AppendLine($"  • Resource constraints (memory/disk)");
                analysis.AppendLine($"  • Unhandled exceptions in API calls");
                analysis.AppendLine();
                analysis.AppendLine($"  Next steps:");
                analysis.AppendLine($"  1. Use F8 to navigate to first error");
                analysis.AppendLine($"  2. Check error message and stack trace");
                analysis.AppendLine($"  3. Use Call Tree to see context");
                analysis.AppendLine($"  4. Export filtered errors for detailed review");
                analysis.AppendLine();
            }

            // Warning analysis
            if (warningCount > 0)
            {
                analysis.AppendLine($"WARNING ANALYSIS ({warningCount} warnings):");
                analysis.AppendLine($"  Common causes:");
                analysis.AppendLine($"  • Deprecated API usage");
                analysis.AppendLine($"  • Configuration issues");
                analysis.AppendLine($"  • Performance degradation");
                analysis.AppendLine($"  • Resource usage approaching limits");
                analysis.AppendLine();
            }

            // Performance-related root causes
            if (perfStats != null && perfStats.Any(p => p.AvgDurationMs > 1000))
            {
                analysis.AppendLine($"PERFORMANCE ISSUES:");
                var slow = perfStats.Where(p => p.AvgDurationMs > 1000).OrderByDescending(p => p.TotalDurationMs).Take(3);
                foreach (var p in slow)
                {
                    analysis.AppendLine($"  • {p.ApiName}: {p.AvgDurationMs:N0} ms avg");
                    analysis.AppendLine($"    Likely causes:");
                    analysis.AppendLine($"    - I/O operations (disk/network)");
                    analysis.AppendLine($"    - Database queries");
                    analysis.AppendLine($"    - External API calls");
                    analysis.AppendLine($"    - Complex computations");
                }
                analysis.AppendLine();
            }

            analysis.AppendLine("?? TIP: Use Dependency Graph to visualize call relationships");

            return Task.FromResult(analysis.ToString());
        }

        // ?? L5: Performance Insights ?????????????????????????????????????????
        public Task<string> AnalyzePerformanceAsync(List<ApiPerfStats> perfStats)
        {
            var insights = new StringBuilder();
            insights.AppendLine("? PERFORMANCE INSIGHTS\n");

            if (perfStats == null || !perfStats.Any())
            {
                insights.AppendLine("No performance data available.");
                return Task.FromResult(insights.ToString());
            }

            // Calculate session metrics
            long totalDuration = perfStats.Sum(p => p.TotalDurationMs);
            double avgCallDuration = perfStats.Average(p => (double)p.AvgDurationMs);
            int totalCalls = perfStats.Sum(p => p.CallCount);

            insights.AppendLine($"?? Session Metrics:");
            insights.AppendLine($"  • Total time: {totalDuration:N0} ms");
            insights.AppendLine($"  • Average call duration: {avgCallDuration:N0} ms");
            insights.AppendLine($"  • Total calls: {totalCalls:N0}");
            insights.AppendLine();

            // Top 10 by total duration
            insights.AppendLine($"?? Top 10 Time Consumers:");
            var topByTotal = perfStats.OrderByDescending(p => p.TotalDurationMs).Take(10);
            foreach (var p in topByTotal)
            {
                double percentage = (p.TotalDurationMs / (double)totalDuration) * 100;
                insights.AppendLine($"  • {p.ApiName}");
                insights.AppendLine($"    Total: {p.TotalDurationMs:N0} ms ({percentage:F1}% of session)");
                insights.AppendLine($"    Calls: {p.CallCount} | Avg: {p.AvgDurationMs:N0} ms | Max: {p.MaxDurationMs:N0} ms");
            }
            insights.AppendLine();

            // Slowest individual calls
            var slowestAvg = perfStats.OrderByDescending(p => p.AvgDurationMs).Take(5);
            insights.AppendLine($"?? Slowest Average Duration:");
            foreach (var p in slowestAvg)
            {
                insights.AppendLine($"  • {p.ApiName}: {p.AvgDurationMs:N0} ms avg");
            }
            insights.AppendLine();

            // Most frequently called
            var mostFrequent = perfStats.OrderByDescending(p => p.CallCount).Take(5);
            insights.AppendLine($"?? Most Frequently Called:");
            foreach (var p in mostFrequent)
            {
                insights.AppendLine($"  • {p.ApiName}: {p.CallCount} calls");
            }

            return Task.FromResult(insights.ToString());
        }

        // ?? L6: Pattern Recognition ??????????????????????????????????????????
        public Task<string> FindPatternsAsync(List<LogEntry> entries)
        {
            var patterns = new StringBuilder();
            patterns.AppendLine("?? PATTERN RECOGNITION\n");

            if (entries == null || !entries.Any())
            {
                patterns.AppendLine("No log entries to analyze.");
                return Task.FromResult(patterns.ToString());
            }

            bool foundPatterns = false;

            // Pattern 1: Repeated errors
            var errorEntries = entries.Where(e => e.Level == "E").ToList();
            var errorGroups = new List<IGrouping<string, LogEntry>>();

            if (errorEntries.Any())
            {
                errorGroups = errorEntries
                    .GroupBy(e => e.RawText != null && e.RawText.Length > 100 ? e.RawText.Substring(0, 100) : e.RawText ?? "")
                    .Where(g => g.Count() > 1)
                    .OrderByDescending(g => g.Count())
                    .Take(3)
                    .ToList();

                if (errorGroups.Any())
                {
                    patterns.AppendLine($"?? REPEATED ERRORS:");
                    foreach (var group in errorGroups)
                    {
                        patterns.AppendLine($"  • Occurs {group.Count()} times: {group.Key}...");
                    }
                    patterns.AppendLine();
                    foundPatterns = true;
                }
            }

            // Pattern 2: Time-based patterns
            int firstHalfErrors = 0;
            int secondHalfErrors = 0;

            if (entries.Count > 100)
            {
                firstHalfErrors = entries.Take(entries.Count / 2).Count(e => e.Level == "E");
                secondHalfErrors = entries.Skip(entries.Count / 2).Count(e => e.Level == "E");

                if (secondHalfErrors > firstHalfErrors * 2)
                {
                    patterns.AppendLine($"?? ESCALATING PATTERN:");
                    patterns.AppendLine($"  • Errors increase over time: {firstHalfErrors} ? {secondHalfErrors}");
                    patterns.AppendLine($"  • May indicate degrading system state");
                    patterns.AppendLine();
                    foundPatterns = true;
                }
            }

            // Pattern 3: Call frequency patterns
            var apiCalls = entries.Where(e => e.IsApiCall).GroupBy(e => e.ApiName).ToList();
            var burst = apiCalls.Where(g => g.Count() > 100).OrderByDescending(g => g.Count()).Take(3).ToList();

            if (burst.Any())
            {
                patterns.AppendLine($"?? HIGH-FREQUENCY CALLS:");
                foreach (var group in burst)
                {
                    patterns.AppendLine($"  • {group.Key}: called {group.Count()} times");
                }
                patterns.AppendLine($"  • May indicate polling, retries, or loops");
                patterns.AppendLine();
                foundPatterns = true;
            }

            if (!foundPatterns)
            {
                patterns.AppendLine("? No concerning patterns detected.");
                patterns.AppendLine("   Log activity appears normal.");
            }

            return Task.FromResult(patterns.ToString());
        }

        // ?? General Analysis ??????????????????????????????????????????????????
        public Task<string> AnalyzeAsync(string query, AggregateStats stats, List<ApiPerfStats> perfStats, List<LogEntry> entries)
        {
            // Route to appropriate analysis based on query
            string queryLower = query.ToLowerInvariant();

            if (queryLower.Contains("pattern") || queryLower.Contains("repeat"))
                return FindPatternsAsync(entries);
            else if (queryLower.Contains("performance") || queryLower.Contains("slow"))
                return AnalyzePerformanceAsync(perfStats);
            else if (queryLower.Contains("anomaly") || queryLower.Contains("unusual"))
                return DetectAnomaliesAsync(stats, perfStats);
            else if (queryLower.Contains("error") || queryLower.Contains("warning"))
                return SuggestRootCauseAsync(stats, perfStats, stats.ErrorCount, stats.WarningCount);
            else if (queryLower.Contains("summary") || queryLower.Contains("overview"))
                return SummarizeAsync(stats, perfStats);
            else
                return NlSearchAsync(query, stats, perfStats);
        }

        // ?? Helper: Build statistics from log entries ????????????????????????
        public static AggregateStats BuildAggregateStats(List<LogEntry> entries, List<ApiPerfStats> perfStats)
        {
            var stats = new AggregateStats
            {
                TotalLines = entries.Count,
                ErrorCount = entries.Count(e => e.Level == "E"),
                WarningCount = entries.Count(e => e.Level == "W"),
                TotalApiCalls = entries.Count(e => e.IsApiCall),
                UniqueApiCount = entries.Where(e => e.IsApiCall).Select(e => e.ApiName).Distinct().Count(),
                MaxCallDepth = CalculateMaxCallDepth(entries)
            };

            if (perfStats != null && perfStats.Any())
            {
                stats.SessionDurationMs = perfStats.Sum(p => p.TotalDurationMs);
                stats.MaxCallDurationMs = perfStats.Max(p => p.MaxDurationMs);
            }

            return stats;
        }

        private static int CalculateMaxCallDepth(List<LogEntry> entries)
        {
            int maxDepth = 0;
            int currentDepth = 0;

            foreach (var entry in entries)
            {
                if (entry.IsApiCall)
                {
                    if (entry.IsCallEnter)
                    {
                        currentDepth++;
                        if (currentDepth > maxDepth)
                            maxDepth = currentDepth;
                    }
                    else if (entry.IsCallExit)
                    {
                        currentDepth--;
                    }
                }
            }

            return maxDepth;
        }

        // ?? Helper: No conversion needed - ApiPerfStats is used directly ?????
        public static List<ApiPerfStats> ConvertPerfStats(List<ApiPerfStats> apiStats)
        {
            return apiStats ?? new List<ApiPerfStats>();
        }
    }
}
