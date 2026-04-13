using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Cad3PLogBrowser.Services.Analysis;

namespace Cad3PLogBrowser.Services.Analysis
{
    /// <summary>
    /// L1–L6 — AI Features via Claude API.
    /// IMPORTANT: Only structured summaries (method names, counts, durations)
    /// are sent to the API. Raw log content is NEVER transmitted.
    /// </summary>
    public class AiLogService
    {
        private readonly string     _apiKey;
        private readonly HttpClient _http;
        private const string        Model   = "claude-sonnet-4-20250514";
        private const string        ApiUrl  = "https://api.anthropic.com/v1/messages";
        private const int           MaxToks = 1000;

        public AiLogService(string apiKey)
        {
            _apiKey = apiKey;
            _http   = new HttpClient();
            _http.DefaultRequestHeaders.Add("x-api-key", apiKey);
            _http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        }

        // ── Shared: build structured summary (NO raw log text) ────────────────
        private static string BuildSummary(AggregateStats stats,
            List<PerformanceStatistics> perfStats, string context = "")
        {
            var sb = new StringBuilder();
            sb.AppendLine("Log Session Summary (structured data only):");
            sb.AppendLine($"  Total lines: {stats.TotalLines}");
            sb.AppendLine($"  Error lines: {stats.ErrorCount}");
            sb.AppendLine($"  Warning lines: {stats.WarningCount}");
            sb.AppendLine($"  Total API calls: {stats.TotalApiCalls}");
            sb.AppendLine($"  Unique APIs: {stats.UniqueApiCount}");
            sb.AppendLine($"  Max call depth: {stats.MaxCallDepth}");
            sb.AppendLine($"  Session duration: {stats.SessionDurationMs} ms");
            sb.AppendLine($"  Max single call: {stats.MaxCallDurationMs} ms");

            if (perfStats != null && perfStats.Count > 0)
            {
                sb.AppendLine("\nTop 10 slowest APIs (ms):");
                foreach (var p in perfStats.OrderByDescending(p => p.TotalDurationMs).Take(10))
                    sb.AppendLine($"  {p.MethodName}: total={p.TotalDurationMs}ms " +
                                  $"avg={p.AverageMs}ms calls={p.CallCount}");
            }

            if (!string.IsNullOrEmpty(context))
                sb.AppendLine($"\nAdditional context: {context}");

            return sb.ToString();
        }

        // ── L1: AI Log Summarizer ─────────────────────────────────────────────
        public Task<string> SummarizeAsync(AggregateStats stats,
            List<PerformanceStatistics> perfStats)
        {
            string prompt =
                "You are a software performance analyst. Based on the structured log " +
                "session summary below, provide a concise plain-English summary " +
                "(3–5 sentences) covering: overall health, notable slow calls, " +
                "error/warning count, and any immediate concerns.\n\n" +
                BuildSummary(stats, perfStats);
            return CallApiAsync(prompt);
        }

        // ── L2: Natural Language Search ───────────────────────────────────────
        public Task<string> NlSearchAsync(string userQuestion, AggregateStats stats,
            List<PerformanceStatistics> perfStats)
        {
            string prompt =
                "You are a log analysis assistant. A developer is asking about their " +
                "application log session. Based ONLY on the structured summary below " +
                "(no raw log content is available), answer their question as specifically " +
                "as possible. If the data is insufficient, say so clearly.\n\n" +
                "Structured log data:\n" + BuildSummary(stats, perfStats) +
                $"\n\nDeveloper question: {userQuestion}";
            return CallApiAsync(prompt);
        }

        // ── L3: Anomaly Detection ─────────────────────────────────────────────
        public Task<string> DetectAnomaliesAsync(AggregateStats stats,
            List<PerformanceStatistics> perfStats)
        {
            string prompt =
                "You are a performance anomaly detector. Analyse the structured log " +
                "summary below and identify any statistical anomalies or suspicious " +
                "patterns (e.g. calls taking 100× the average, unexpectedly high error " +
                "counts, deep call stacks). List each anomaly with a brief explanation.\n\n" +
                BuildSummary(stats, perfStats);
            return CallApiAsync(prompt);
        }

        // ── L4: Root Cause Suggester ──────────────────────────────────────────
        public Task<string> SuggestRootCauseAsync(AggregateStats stats,
            List<PerformanceStatistics> perfStats, int errorCount, int warningCount)
        {
            string prompt =
                "You are a root cause analysis expert. The application log shows " +
                $"{errorCount} errors and {warningCount} warnings. Based on the " +
                "structured performance data below, suggest the 2–3 most likely root " +
                "causes of any issues and what a developer should investigate first.\n\n" +
                BuildSummary(stats, perfStats,
                    $"{errorCount} errors, {warningCount} warnings observed");
            return CallApiAsync(prompt);
        }

        // ── L5: Auto-Generate Bug Report ──────────────────────────────────────
        public Task<string> GenerateBugReportAsync(AggregateStats stats,
            List<PerformanceStatistics> perfStats, string appVersion)
        {
            string prompt =
                "Generate a concise bug report in Markdown format based on the " +
                "structured log analysis below. Include: Summary, Steps to Reproduce " +
                "(inferred from API call patterns), Expected vs Actual behaviour, " +
                "Performance Impact, and Recommended Priority.\n\n" +
                BuildSummary(stats, perfStats, $"Application version: {appVersion}");
            return CallApiAsync(prompt);
        }

        // ── L6: Conversational Log Assistant ──────────────────────────────────
        public Task<string> ChatAsync(string userMessage,
            List<(string role, string content)> history,
            AggregateStats stats, List<PerformanceStatistics> perfStats)
        {
            var messages = new List<object>();

            // System context: inject structured summary once
            string systemCtx = "You are a helpful log analysis assistant. " +
                "You have access to structured performance data from a log session " +
                "(no raw log text). Answer questions concisely and helpfully.\n\n" +
                "Log session data:\n" + BuildSummary(stats, perfStats);

            // Rebuild conversation history
            foreach (var (role, msg) in history)
                messages.Add(new { role, content = msg });

            messages.Add(new { role = "user", content = userMessage });

            return CallApiAsync(null, systemCtx, messages);
        }

        // ── Core API call ─────────────────────────────────────────────────────
        private async Task<string> CallApiAsync(string userPrompt,
            string systemPrompt = null,
            List<object> messages = null)
        {
            if (string.IsNullOrEmpty(_apiKey))
                return "⚠ No Claude API key configured. Add 'claudeApiKey' in Settings → AI.";

            try
            {
                if (messages == null)
                    messages = new List<object>
                        { new { role = "user", content = userPrompt } };

                var body = new
                {
                    model      = Model,
                    max_tokens = MaxToks,
                    system     = systemPrompt ?? "You are a log analysis assistant.",
                    messages
                };

                string json = SimpleJson(body);
                var resp = await _http.PostAsync(ApiUrl,
                    new StringContent(json, Encoding.UTF8, "application/json"));

                string raw = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode)
                    return $"API error {(int)resp.StatusCode}: {raw}";

                // Parse response text
                int start = raw.IndexOf("\"text\":\"", StringComparison.Ordinal);
                if (start < 0) return raw;
                start += 8;
                int end = raw.IndexOf("\",", start, StringComparison.Ordinal);
                if (end < 0) end = raw.IndexOf("\"}", start, StringComparison.Ordinal);
                if (end < 0) return raw;
                return raw.Substring(start, end - start)
                    .Replace("\\n", "\n").Replace("\\\"", "\"");
            }
            catch (Exception ex)
            {
                return $"Error calling Claude API: {ex.Message}";
            }
        }

        private static string SimpleJson(object obj)
        {
            // Minimal JSON serialiser — only handles the shapes we build above
            if (obj is string s) return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"")
                .Replace("\n", "\\n").Replace("\r", "") + "\"";
            if (obj is int i)  return i.ToString();
            if (obj is bool b) return b ? "true" : "false";
            if (obj is null)   return "null";

            var t = obj.GetType();
            if (t.IsArray || (t.IsGenericType && typeof(System.Collections.IEnumerable)
                .IsAssignableFrom(t)))
            {
                var items = new List<string>();
                foreach (var item in (System.Collections.IEnumerable)obj)
                    items.Add(SimpleJson(item));
                return "[" + string.Join(",", items) + "]";
            }

            var props = new List<string>();
            foreach (var prop in t.GetProperties())
                props.Add("\"" + prop.Name + "\":" + SimpleJson(prop.GetValue(obj)));
            return "{" + string.Join(",", props) + "}";
        }
    }
}
