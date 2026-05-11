using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Cad3PLogBrowser.Models;

namespace Cad3PLogBrowser.Services.Analysis
{
    /// <summary>
    /// L1-L6 — AI Features (Option B hybrid approach).
    ///
    /// DEFAULT (offline): All methods work without any configuration using
    /// rule-based analysis on log statistics. No data leaves the machine.
    ///
    /// ENHANCED (Claude API): When UseClaudeApi=true and a ClaudeApiKey is
    /// provided in Settings, every method upgrades to a real Claude API call.
    /// Only structured summaries (method names, counts, durations) are sent —
    /// NEVER raw log lines.
    /// </summary>
    public class AiLogService
    {
        // ── Configuration ─────────────────────────────────────────────────────
        private string _apiKey;
        private bool   _useApi;
        // D-10: model name is user-configurable so the app does not hard-break
        // when Anthropic deprecates the previously baked-in version string.
        private string _model;

        private const string ApiUrl        = "https://api.anthropic.com/v1/messages";
        private const string DefaultModel  = "claude-sonnet-4-20250514";

        public bool IsApiEnabled => _useApi && !string.IsNullOrWhiteSpace(_apiKey);
        public bool IsConfigured => true; // offline always works

        public AiLogService(string apiKey = "", bool useClaudeApi = false, string model = null)
        {
            _apiKey = apiKey ?? string.Empty;
            _useApi = useClaudeApi;
            _model  = string.IsNullOrWhiteSpace(model) ? DefaultModel : model;
        }

        /// <summary>Call after user changes settings so the running instance picks up new values.</summary>
        public void UpdateConfig(string apiKey, bool useClaudeApi, string model = null)
        {
            _apiKey = apiKey ?? string.Empty;
            _useApi = useClaudeApi;
            _model  = string.IsNullOrWhiteSpace(model) ? DefaultModel : model;
        }

        // ── L1: Summarize ─────────────────────────────────────────────────────
        public async Task<string> SummarizeAsync(AggregateStats stats, List<ApiPerfStats> perfStats)
        {
            if (IsApiEnabled)
            {
                string prompt =
                    "You are a software performance analyst. Provide a concise plain-English " +
                    "summary (4-6 sentences) covering: overall health, notable slow calls, " +
                    "error/warning count, and top concerns.\n\n" +
                    BuildStructuredSummary(stats, perfStats);
                return await CallClaudeAsync(prompt) ?? OfflineSummarize(stats, perfStats);
            }
            return OfflineSummarize(stats, perfStats);
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
            if (perfStats != null && perfStats.Any())
            {
                var slowest = perfStats.OrderByDescending(p => p.TotalDurationMs).Take(5).ToList();
                if (slowest.Any(p => p.TotalDurationMs > 1000))
                {
                    sb.AppendLine("PERFORMANCE CONCERNS:");
                    foreach (var p in slowest.Where(x => x.TotalDurationMs > 1000))
                        sb.AppendLine($"  {p.ApiName}: {p.TotalDurationMs:N0} ms total ({p.CallCount} calls)");
                    sb.AppendLine();
                }
                sb.AppendLine("Top 5 Slowest APIs:");
                foreach (var p in slowest)
                    sb.AppendLine($"  {p.ApiName}: {p.TotalDurationMs:N0} ms (avg {p.AvgDurationMs:N0} ms, {p.CallCount} calls)");
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
        public async Task<string> NlSearchAsync(string question, AggregateStats stats, List<ApiPerfStats> perfStats)
        {
            if (IsApiEnabled)
            {
                string prompt =
                    "Answer this developer question about their application log session. " +
                    "Use ONLY the structured data provided — do not invent details. " +
                    "If the data is insufficient, say so.\n\n" +
                    $"Question: {question}\n\n" + BuildStructuredSummary(stats, perfStats);
                return await CallClaudeAsync(prompt) ?? OfflineNlSearch(question, stats, perfStats);
            }
            return OfflineNlSearch(question, stats, perfStats);
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
                if (perfStats != null && perfStats.Any())
                {
                    sb.AppendLine("  Slowest operations:");
                    foreach (var p in perfStats.OrderByDescending(x => x.TotalDurationMs).Take(3))
                        sb.AppendLine($"    {p.ApiName}: {p.TotalDurationMs:N0} ms ({p.CallCount} calls)");
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
        public async Task<string> DetectAnomaliesAsync(AggregateStats stats, List<ApiPerfStats> perfStats)
        {
            if (IsApiEnabled)
            {
                string prompt =
                    "You are a performance anomaly detector. Identify statistical anomalies " +
                    "(e.g. calls 10x the average, unexpectedly high errors, deep call stacks). " +
                    "List each with a brief explanation and priority.\n\n" +
                    BuildStructuredSummary(stats, perfStats);
                return await CallClaudeAsync(prompt) ?? OfflineDetectAnomalies(stats, perfStats);
            }
            return OfflineDetectAnomalies(stats, perfStats);
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
            if (perfStats != null && perfStats.Any())
            {
                double avg = perfStats.Average(p => (double)p.AvgDurationMs);
                var outliers = perfStats.Where(p => p.AvgDurationMs > avg * 10).ToList();
                if (outliers.Any())
                {
                    sb.AppendLine($"PERFORMANCE OUTLIERS: {outliers.Count} API(s) with 10x+ average duration");
                    foreach (var p in outliers.Take(5))
                        sb.AppendLine($"   {p.ApiName}: {p.AvgDurationMs:N0} ms avg (session avg {avg:N0} ms)");
                    sb.AppendLine("   Profile these methods for optimization\n");
                    found = true;
                }
                double avgCalls = perfStats.Average(p => (double)p.CallCount);
                var hotspots = perfStats.Where(p => p.CallCount > avgCalls * 5).ToList();
                if (hotspots.Any())
                {
                    sb.AppendLine($"HOTSPOT METHODS: {hotspots.Count} API(s) called 5x+ more than average");
                    foreach (var p in hotspots.Take(5))
                        sb.AppendLine($"   {p.ApiName}: {p.CallCount} calls (avg {avgCalls:N0})");
                    found = true;
                }
            }
            if (!found) sb.AppendLine("No significant anomalies detected.\nSession appears healthy with normal patterns.");
            return sb.ToString();
        }

        // ── L4: Root Cause Suggester ──────────────────────────────────────────
        public async Task<string> SuggestRootCauseAsync(AggregateStats stats, List<ApiPerfStats> perfStats,
            int errorCount, int warningCount)
        {
            if (IsApiEnabled)
            {
                string prompt =
                    "You are a root cause analysis expert. The log shows " +
                    $"{errorCount} errors and {warningCount} warnings. " +
                    "Suggest the 2-3 most likely root causes and what to investigate first.\n\n" +
                    BuildStructuredSummary(stats, perfStats,
                        $"{errorCount} errors, {warningCount} warnings");
                return await CallClaudeAsync(prompt) ?? OfflineRootCause(stats, perfStats, errorCount, warningCount);
            }
            return OfflineRootCause(stats, perfStats, errorCount, warningCount);
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
                foreach (var p in perfStats.Where(p => p.AvgDurationMs > 1000).OrderByDescending(p => p.TotalDurationMs).Take(3))
                {
                    sb.AppendLine($"  {p.ApiName}: {p.AvgDurationMs:N0} ms avg");
                    sb.AppendLine("    Likely causes: I/O operations, database queries, external API calls");
                }
            }
            return sb.ToString();
        }

        // ── L5: Bug Report Generator ──────────────────────────────────────────
        public async Task<string> GenerateBugReportAsync(AggregateStats stats,
            List<ApiPerfStats> perfStats, string appVersion)
        {
            if (IsApiEnabled)
            {
                string prompt =
                    "Generate a concise bug report in Markdown format. Include: " +
                    "Summary, Observed Behaviour (from API patterns), Performance Impact, " +
                    "and Recommended Priority.\n\n" +
                    BuildStructuredSummary(stats, perfStats, $"App version: {appVersion}");
                return await CallClaudeAsync(prompt) ?? OfflineBugReport(stats, perfStats, appVersion);
            }
            return OfflineBugReport(stats, perfStats, appVersion);
        }

        private string OfflineBugReport(AggregateStats stats, List<ApiPerfStats> perfStats, string version)
        {
            var sb = new StringBuilder();
            sb.AppendLine("## Bug Report\n");
            sb.AppendLine($"**Version:** {version}");
            sb.AppendLine($"**Session:** {stats.TotalLines:N0} lines, {stats.SessionDurationMs:N0} ms\n");
            sb.AppendLine($"**Summary:** {stats.ErrorCount} error(s), {stats.WarningCount} warning(s) detected.\n");
            sb.AppendLine("**Performance Impact:**");
            if (perfStats != null && perfStats.Any())
                foreach (var p in perfStats.OrderByDescending(x => x.TotalDurationMs).Take(5))
                    sb.AppendLine($"- {p.ApiName}: {p.TotalDurationMs:N0} ms total ({p.CallCount} calls)");
            sb.AppendLine($"\n**Priority:** {(stats.ErrorCount > 10 ? "HIGH" : stats.ErrorCount > 0 ? "MEDIUM" : "LOW")}");
            sb.AppendLine("\n_Tip: Enable Claude API in Settings for an enhanced AI-generated bug report._");
            return sb.ToString();
        }

        // ── L6: Conversational Chat ───────────────────────────────────────────
        public async Task<string> ChatAsync(string userMessage,
            List<(string role, string content)> history,
            AggregateStats stats, List<ApiPerfStats> perfStats)
        {
            if (IsApiEnabled)
            {
                // Build messages array for multi-turn conversation
                var messages = new StringBuilder("[");
                foreach (var (role, msg) in history)
                {
                    if (messages.Length > 1) messages.Append(",");
                    messages.Append($"{{\"role\":\"{EscJson(role)}\",\"content\":\"{EscJson(msg)}\"}}");
                }
                if (messages.Length > 1) messages.Append(",");
                messages.Append($"{{\"role\":\"user\",\"content\":\"{EscJson(userMessage)}\"}}");
                messages.Append("]");

                string system =
                    "You are a helpful log analysis assistant. Answer concisely. " +
                    "You have access to structured log session data only — no raw log text.\n\n" +
                    BuildStructuredSummary(stats, perfStats);

                return await CallClaudeAsync(null, system, messages.ToString())
                    ?? OfflineNlSearch(userMessage, stats, perfStats);
            }
            return OfflineNlSearch(userMessage, stats, perfStats);
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
            foreach (var p in perfStats.OrderByDescending(p => p.TotalDurationMs).Take(10))
            {
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
            bool found = false;
            var errors = entries.Where(e => e.Level == "E").ToList();
            if (errors.Any())
            {
                var repeated = errors
                    .GroupBy(e => (e.RawText ?? "").Length > 80 ? e.RawText.Substring(0, 80) : e.RawText ?? "")
                    .Where(g => g.Count() > 1).OrderByDescending(g => g.Count()).Take(3).ToList();
                if (repeated.Any())
                {
                    sb.AppendLine("REPEATED ERRORS:");
                    foreach (var g in repeated) sb.AppendLine($"  Occurs {g.Count()}x: {g.Key}...");
                    found = true;
                }
            }
            if (entries.Count > 100)
            {
                int firstHalf  = entries.Take(entries.Count / 2).Count(e => e.Level == "E");
                int secondHalf = entries.Skip(entries.Count / 2).Count(e => e.Level == "E");
                if (secondHalf > firstHalf * 2)
                {
                    sb.AppendLine($"ESCALATING PATTERN: Errors increase over time: {firstHalf} -> {secondHalf}");
                    sb.AppendLine("  May indicate degrading system state");
                    found = true;
                }
            }
            var burst = entries.Where(e => e.IsApiCall).GroupBy(e => e.ApiName)
                .Where(g => g.Count() > 100).OrderByDescending(g => g.Count()).Take(3).ToList();
            if (burst.Any())
            {
                sb.AppendLine("HIGH-FREQUENCY CALLS:");
                foreach (var g in burst) sb.AppendLine($"  {g.Key}: called {g.Count()} times");
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

        // ── Helpers ───────────────────────────────────────────────────────────
        /// <summary>
        /// Builds the structured data payload sent to Claude.
        /// NEVER includes raw log lines — only aggregated statistics.
        /// </summary>
        private static string BuildStructuredSummary(AggregateStats stats,
            List<ApiPerfStats> perfStats, string extra = "")
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Structured Log Session Data (no raw log content) ===");
            sb.AppendLine($"Total lines: {stats.TotalLines}");
            sb.AppendLine($"Errors: {stats.ErrorCount}  Warnings: {stats.WarningCount}");
            sb.AppendLine($"API calls: {stats.TotalApiCalls}  Unique: {stats.UniqueApiCount}");
            sb.AppendLine($"Max depth: {stats.MaxCallDepth}  Max single call: {stats.MaxCallDurationMs} ms");
            sb.AppendLine($"Session duration: {stats.SessionDurationMs} ms");
            if (perfStats != null && perfStats.Any())
            {
                sb.AppendLine("\nTop 10 APIs by total time:");
                foreach (var p in perfStats.OrderByDescending(x => x.TotalDurationMs).Take(10))
                    sb.AppendLine($"  {p.ApiName}: total={p.TotalDurationMs}ms avg={p.AvgDurationMs}ms min={p.MinDurationMs}ms max={p.MaxDurationMs}ms calls={p.CallCount}");
            }
            if (!string.IsNullOrEmpty(extra)) sb.AppendLine($"\nContext: {extra}");
            return sb.ToString();
        }

        private async Task<string> CallClaudeAsync(string userPrompt,
            string system = "You are a log analysis assistant.",
            string messagesJson = null)
        {
            try
            {
                if (messagesJson == null)
                    messagesJson = $"[{{\"role\":\"user\",\"content\":\"{EscJson(userPrompt)}\"}}]";

                string body = $"{{\"model\":\"{_model}\",\"max_tokens\":1000,\"system\":\"{EscJson(system)}\",\"messages\":{messagesJson}}}";

                // Use WebClient instead of HttpClient (compatible with .NET Framework 4.8 without extra packages)
                using (var client = new WebClient())
                {
                    client.Headers.Add("x-api-key", _apiKey);
                    client.Headers.Add("anthropic-version", "2023-06-01");
                    client.Headers.Add("Content-Type", "application/json");

                    string raw = await Task.Run(() => client.UploadString(ApiUrl, body));

                    // Parse "text" from response
                    int start = raw.IndexOf("\"text\":\"", StringComparison.Ordinal);
                    if (start < 0) return null;
                    start += 8;
                    int end = FindJsonStringEnd(raw, start);
                    if (end < 0) return null;
                    return raw.Substring(start, end - start)
                        .Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\\\", "\\");
                }
            }
            catch (WebException wex)
            {
                try
                {
                    using (var reader = new System.IO.StreamReader(wex.Response.GetResponseStream()))
                    {
                        string error = reader.ReadToEnd();
                        return $"[Claude API error: {error}]";
                    }
                }
                catch
                {
                    return $"[Claude API unavailable: {wex.Message}]";
                }
            }
            catch (Exception ex)
            {
                return $"[Claude API unavailable: {ex.Message}]";
            }
        }

        private static string EscJson(string s) =>
            (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"")
                     .Replace("\n", "\\n").Replace("\r", "").Replace("\t", "\\t");

        private static int FindJsonStringEnd(string json, int start)
        {
            for (int i = start; i < json.Length; i++)
            {
                if (json[i] == '\\') { i++; continue; }
                if (json[i] == '"') return i;
            }
            return -1;
        }

        // ── Static helpers used by MainForm ───────────────────────────────────
        public static AggregateStats BuildAggregateStats(List<LogEntry> entries, List<ApiPerfStats> perfStats)
        {
            int depth = 0, maxDepth = 0;
            foreach (var e in entries)
            {
                if (!e.IsApiCall) continue;
                if (e.IsCallEnter) { if (++depth > maxDepth) maxDepth = depth; }
                else if (e.IsCallExit && depth > 0) depth--;
            }
            var stats = new AggregateStats
            {
                TotalLines    = entries.Count,
                ErrorCount    = entries.Count(e => e.Level == "E"),
                WarningCount  = entries.Count(e => e.Level == "W"),
                TotalApiCalls = entries.Count(e => e.IsApiCall),
                UniqueApiCount = entries.Where(e => e.IsApiCall).Select(e => e.ApiName).Distinct().Count(),
                MaxCallDepth  = maxDepth
            };
            if (perfStats != null && perfStats.Any())
            {
                stats.SessionDurationMs  = perfStats.Sum(p => p.TotalDurationMs);
                stats.MaxCallDurationMs  = perfStats.Max(p => p.MaxDurationMs);
            }
            return stats;
        }

        public static List<ApiPerfStats> ConvertPerfStats(List<ApiPerfStats> stats) =>
            stats ?? new List<ApiPerfStats>();
    }
}
