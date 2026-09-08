namespace Cad3PLogBrowser.Services.Export
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Cad3PLogBrowser.Models;
    using Cad3PLogBrowser.Services.Analysis;

    /// <summary>
    /// Builds a single-file "Investigation Report" — a shareable snapshot of the
    /// current analysis session (source file, active filter, bookmarked evidence
    /// lines, aggregate stats, top slowest/frequent calls, call-depth analysis,
    /// exception groups, correlation IDs, and baseline anomalies) — in HTML or
    /// Markdown format.
    /// </summary>
    /// <remarks>
    /// This is intentionally a pure, dependency-free string builder (no NuGet PDF/DOCX
    /// libraries) so a report can be produced from plain data the caller (MainForm)
    /// already has in memory. HTML output can be turned into a PDF via the browser's
    /// Print > Save as PDF, matching the same approach used for the Analytics Report.
    /// </remarks>
    public class InvestigationReportService
    {
        /// <summary>All the data that can go into an investigation report. Any list may be null/empty.</summary>
        public class ReportData
        {
            public string SourceFilePath;
            public string ActiveFilterDescription;
            public AggregateStats Stats;
            public List<int> BookmarkedLines;
            /// <summary>Bookmarked line number -> raw log text, for evidence excerpts.</summary>
            public Func<int, string> GetLineText;
            public List<SlowCallInfo> TopSlowestCalls;
            public List<FrequentCallInfo> MostFrequentCalls;
            public CallDepthAnalysis CallDepth;
            public List<ExceptionGroup> ExceptionGroups;
            public Dictionary<string, List<int>> CorrelationIds;
            public List<AnomalyResult> Anomalies;
        }

        private static string Esc(string s) => System.Net.WebUtility.HtmlEncode(s ?? string.Empty);

        /// <summary>Builds a standalone, styled HTML investigation report.</summary>
        public string BuildHtml(ReportData data)
        {
            if (data == null) data = new ReportData();
            var sb = new StringBuilder();

            sb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\">");
            sb.Append("<title>Investigation Report — ").Append(Esc(SafeFileName(data.SourceFilePath))).Append("</title>");
            sb.Append(@"<style>
                body { font-family: 'Segoe UI', Arial, sans-serif; margin: 32px; color: #1a1a1a; }
                h1 { font-size: 20px; border-bottom: 2px solid #256b66; padding-bottom: 8px; }
                h2 { font-size: 15px; color: #256b66; margin-top: 28px; }
                table { border-collapse: collapse; width: 100%; margin-top: 8px; }
                th, td { border: 1px solid #ccc; padding: 6px 10px; font-size: 12px; text-align: left; }
                th { background: #256b66; color: #fff; }
                tr:nth-child(even) { background: #f5f5f5; }
                .meta { color: #666; font-size: 12px; margin-bottom: 16px; }
                .statgrid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 8px; margin-top: 8px; }
                .stat { border: 1px solid #ddd; border-radius: 4px; padding: 10px; }
                .stat .n { font-size: 20px; font-weight: bold; }
                .stat .l { font-size: 11px; color: #666; }
                .empty { color: #888; font-style: italic; font-size: 12px; }
                code.evidence { display: block; background: #f5f5f5; border: 1px solid #ddd; padding: 6px 8px;
                                 font-family: Consolas, monospace; font-size: 11.5px; white-space: pre-wrap;
                                 word-break: break-all; margin-top: 4px; }
                @media print { body { margin: 12mm; } }
            </style></head><body>");

            sb.Append("<h1>CAD 3P Log Browser — Investigation Report</h1>");
            sb.Append("<div class=\"meta\">Source: ").Append(Esc(data.SourceFilePath))
              .Append("<br>Generated: ").Append(Esc(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")))
              .Append("<br>Active filter: ").Append(string.IsNullOrEmpty(data.ActiveFilterDescription)
                  ? "None" : Esc(data.ActiveFilterDescription))
              .Append("</div>");

            var stats = data.Stats ?? new AggregateStats();
            sb.Append("<h2>Aggregate Statistics</h2><div class=\"statgrid\">");
            void Stat(string label, string value) =>
                sb.Append("<div class=\"stat\"><div class=\"n\">").Append(Esc(value))
                  .Append("</div><div class=\"l\">").Append(Esc(label)).Append("</div></div>");
            Stat("Total Lines", stats.TotalLines.ToString("N0"));
            Stat("Errors", stats.ErrorCount.ToString("N0"));
            Stat("Warnings", stats.WarningCount.ToString("N0"));
            Stat("API Calls", stats.TotalApiCalls.ToString("N0"));
            Stat("Unique APIs", stats.UniqueApiCount.ToString("N0"));
            Stat("Max Call Depth", stats.MaxCallDepth.ToString("N0"));
            sb.Append("</div>");

            // Bookmarked evidence
            sb.Append("<h2>Bookmarked Evidence</h2>");
            var bookmarks = data.BookmarkedLines ?? new List<int>();
            if (bookmarks.Count == 0)
            {
                sb.Append("<p class=\"empty\">No bookmarks in this session.</p>");
            }
            else
            {
                foreach (int line in bookmarks)
                {
                    string text = data.GetLineText != null ? data.GetLineText(line) : "";
                    sb.Append("<p><b>Line ").Append(line).Append(":</b>");
                    sb.Append("<code class=\"evidence\">").Append(Esc(text)).Append("</code></p>");
                }
            }

            // Top slowest calls
            sb.Append("<h2>Top Slowest Calls</h2>");
            var slowest = data.TopSlowestCalls ?? new List<SlowCallInfo>();
            if (slowest.Count == 0)
            {
                sb.Append("<p class=\"empty\">No timed calls available.</p>");
            }
            else
            {
                sb.Append("<table><tr><th>Method</th><th>Duration (ms)</th><th>Enter Line</th><th>Exit Line</th></tr>");
                foreach (var s in slowest)
                    sb.Append("<tr><td>").Append(Esc(s.ApiName)).Append("</td><td>").Append(s.DurationMs.ToString("N0"))
                      .Append("</td><td>").Append(s.EnterLineNumber).Append("</td><td>").Append(s.ExitLineNumber).Append("</td></tr>");
                sb.Append("</table>");
            }

            // Most frequently called
            sb.Append("<h2>Most Frequently Called</h2>");
            var frequent = data.MostFrequentCalls ?? new List<FrequentCallInfo>();
            if (frequent.Count == 0)
            {
                sb.Append("<p class=\"empty\">No call-frequency data available.</p>");
            }
            else
            {
                sb.Append("<table><tr><th>Method</th><th>Calls</th><th>% of Total</th></tr>");
                foreach (var f in frequent)
                    sb.Append("<tr><td>").Append(Esc(f.ApiName)).Append("</td><td>").Append(f.CallCount.ToString("N0"))
                      .Append("</td><td>").Append(f.PercentOfTotal.ToString("F1")).Append("%</td></tr>");
                sb.Append("</table>");
            }

            // Call depth
            sb.Append("<h2>Call Depth Analysis</h2>");
            if (data.CallDepth == null)
            {
                sb.Append("<p class=\"empty\">No call-tree data available.</p>");
            }
            else
            {
                sb.Append("<p>Max depth: <b>").Append(data.CallDepth.MaxDepth)
                  .Append("</b> &nbsp; Average depth: <b>").Append(data.CallDepth.AvgDepth.ToString("F1")).Append("</b></p>");
                var chains = data.CallDepth.DeepestChains ?? new List<DeepestChainInfo>();
                if (chains.Count > 0)
                {
                    sb.Append("<table><tr><th>Deepest Chains</th></tr>");
                    foreach (var c in chains)
                        sb.Append("<tr><td>").Append(Esc(c.Chain)).Append("</td></tr>");
                    sb.Append("</table>");
                }
            }

            // Exception groups
            sb.Append("<h2>Exception Groups</h2>");
            var groups = data.ExceptionGroups ?? new List<ExceptionGroup>();
            if (groups.Count == 0)
            {
                sb.Append("<p class=\"empty\">No exceptions detected.</p>");
            }
            else
            {
                sb.Append("<table><tr><th>Type</th><th>Count</th><th>First Seen (Line)</th><th>Last Seen (Line)</th></tr>");
                foreach (var g in groups)
                    sb.Append("<tr><td>").Append(Esc(g.ExceptionType)).Append("</td><td>").Append(g.Count)
                      .Append("</td><td>").Append(g.StartLine).Append("</td><td>").Append(g.EndLine).Append("</td></tr>");
                sb.Append("</table>");
            }

            // Correlation IDs
            sb.Append("<h2>Correlation / Request IDs</h2>");
            var correlations = data.CorrelationIds ?? new Dictionary<string, List<int>>();
            if (correlations.Count == 0)
            {
                sb.Append("<p class=\"empty\">No correlation/request IDs found.</p>");
            }
            else
            {
                sb.Append("<table><tr><th>Correlation / Request ID</th><th>Occurrences</th></tr>");
                foreach (var kv in correlations)
                    sb.Append("<tr><td>").Append(Esc(kv.Key)).Append("</td><td>").Append(kv.Value.Count).Append("</td></tr>");
                sb.Append("</table>");
            }

            // Anomalies
            sb.Append("<h2>Baseline Anomalies</h2>");
            var anomalies = data.Anomalies ?? new List<AnomalyResult>();
            if (anomalies.Count == 0)
            {
                sb.Append("<p class=\"empty\">No baseline anomalies (or no baseline saved).</p>");
            }
            else
            {
                sb.Append("<table><tr><th>Method</th><th>Baseline Avg (ms)</th><th>Current Avg (ms)</th>" +
                          "<th>Baseline Calls</th><th>Current Calls</th><th>Why Flagged</th></tr>");
                foreach (var a in anomalies)
                    sb.Append("<tr><td>").Append(Esc(a.ApiName)).Append("</td><td>").Append(a.BaselineAvgMs)
                      .Append("</td><td>").Append(a.CurrentAvgMs).Append("</td><td>").Append(a.BaselineCalls)
                      .Append("</td><td>").Append(a.CurrentCalls).Append("</td><td>").Append(Esc(a.Reason)).Append("</td></tr>");
                sb.Append("</table>");
            }

            sb.Append("</body></html>");
            return sb.ToString();
        }

        /// <summary>Builds the same report in GitHub-flavored Markdown, for easy pasting into tickets/wikis.</summary>
        public string BuildMarkdown(ReportData data)
        {
            if (data == null) data = new ReportData();
            var sb = new StringBuilder();

            sb.Append("# CAD 3P Log Browser — Investigation Report\n\n");
            sb.Append("- **Source:** ").Append(data.SourceFilePath).Append('\n');
            sb.Append("- **Generated:** ").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append('\n');
            sb.Append("- **Active filter:** ").Append(string.IsNullOrEmpty(data.ActiveFilterDescription)
                ? "None" : data.ActiveFilterDescription).Append("\n\n");

            var stats = data.Stats ?? new AggregateStats();
            sb.Append("## Aggregate Statistics\n\n");
            sb.Append("| Metric | Value |\n|---|---|\n");
            sb.Append("| Total Lines | ").Append(stats.TotalLines.ToString("N0")).Append(" |\n");
            sb.Append("| Errors | ").Append(stats.ErrorCount.ToString("N0")).Append(" |\n");
            sb.Append("| Warnings | ").Append(stats.WarningCount.ToString("N0")).Append(" |\n");
            sb.Append("| API Calls | ").Append(stats.TotalApiCalls.ToString("N0")).Append(" |\n");
            sb.Append("| Unique APIs | ").Append(stats.UniqueApiCount.ToString("N0")).Append(" |\n");
            sb.Append("| Max Call Depth | ").Append(stats.MaxCallDepth.ToString("N0")).Append(" |\n\n");

            sb.Append("## Bookmarked Evidence\n\n");
            var bookmarks = data.BookmarkedLines ?? new List<int>();
            if (bookmarks.Count == 0)
            {
                sb.Append("_No bookmarks in this session._\n\n");
            }
            else
            {
                foreach (int line in bookmarks)
                {
                    string text = data.GetLineText != null ? data.GetLineText(line) : "";
                    sb.Append("**Line ").Append(line).Append(":**\n```\n").Append(text).Append("\n```\n\n");
                }
            }

            sb.Append("## Top Slowest Calls\n\n");
            var slowest = data.TopSlowestCalls ?? new List<SlowCallInfo>();
            if (slowest.Count == 0)
            {
                sb.Append("_No timed calls available._\n\n");
            }
            else
            {
                sb.Append("| Method | Duration (ms) | Enter Line | Exit Line |\n|---|---|---|---|\n");
                foreach (var s in slowest)
                    sb.Append("| ").Append(s.ApiName).Append(" | ").Append(s.DurationMs.ToString("N0"))
                      .Append(" | ").Append(s.EnterLineNumber).Append(" | ").Append(s.ExitLineNumber).Append(" |\n");
                sb.Append('\n');
            }

            sb.Append("## Most Frequently Called\n\n");
            var frequent = data.MostFrequentCalls ?? new List<FrequentCallInfo>();
            if (frequent.Count == 0)
            {
                sb.Append("_No call-frequency data available._\n\n");
            }
            else
            {
                sb.Append("| Method | Calls | % of Total |\n|---|---|---|\n");
                foreach (var f in frequent)
                    sb.Append("| ").Append(f.ApiName).Append(" | ").Append(f.CallCount.ToString("N0"))
                      .Append(" | ").Append(f.PercentOfTotal.ToString("F1")).Append("% |\n");
                sb.Append('\n');
            }

            sb.Append("## Call Depth Analysis\n\n");
            if (data.CallDepth == null)
            {
                sb.Append("_No call-tree data available._\n\n");
            }
            else
            {
                sb.Append("- **Max depth:** ").Append(data.CallDepth.MaxDepth).Append('\n');
                sb.Append("- **Average depth:** ").Append(data.CallDepth.AvgDepth.ToString("F1")).Append("\n\n");
                var chains = data.CallDepth.DeepestChains ?? new List<DeepestChainInfo>();
                if (chains.Count > 0)
                {
                    sb.Append("**Deepest chains:**\n\n");
                    foreach (var c in chains)
                        sb.Append("- ").Append(c.Chain).Append('\n');
                    sb.Append('\n');
                }
            }

            sb.Append("## Exception Groups\n\n");
            var groups = data.ExceptionGroups ?? new List<ExceptionGroup>();
            if (groups.Count == 0)
            {
                sb.Append("_No exceptions detected._\n\n");
            }
            else
            {
                sb.Append("| Type | Count | First Seen (Line) | Last Seen (Line) |\n|---|---|---|---|\n");
                foreach (var g in groups)
                    sb.Append("| ").Append(g.ExceptionType).Append(" | ").Append(g.Count)
                      .Append(" | ").Append(g.StartLine).Append(" | ").Append(g.EndLine).Append(" |\n");
                sb.Append('\n');
            }

            sb.Append("## Correlation / Request IDs\n\n");
            var correlations = data.CorrelationIds ?? new Dictionary<string, List<int>>();
            if (correlations.Count == 0)
            {
                sb.Append("_No correlation/request IDs found._\n\n");
            }
            else
            {
                sb.Append("| Correlation / Request ID | Occurrences |\n|---|---|\n");
                foreach (var kv in correlations)
                    sb.Append("| ").Append(kv.Key).Append(" | ").Append(kv.Value.Count).Append(" |\n");
                sb.Append('\n');
            }

            sb.Append("## Baseline Anomalies\n\n");
            var anomalies = data.Anomalies ?? new List<AnomalyResult>();
            if (anomalies.Count == 0)
            {
                sb.Append("_No baseline anomalies (or no baseline saved)._\n\n");
            }
            else
            {
                sb.Append("| Method | Baseline Avg (ms) | Current Avg (ms) | Baseline Calls | Current Calls | Why Flagged |\n|---|---|---|---|---|---|\n");
                foreach (var a in anomalies)
                    sb.Append("| ").Append(a.ApiName).Append(" | ").Append(a.BaselineAvgMs).Append(" | ").Append(a.CurrentAvgMs)
                      .Append(" | ").Append(a.BaselineCalls).Append(" | ").Append(a.CurrentCalls).Append(" | ").Append(a.Reason).Append(" |\n");
            }

            return sb.ToString();
        }

        private static string SafeFileName(string path) =>
            string.IsNullOrEmpty(path) ? "untitled" : System.IO.Path.GetFileName(path);
    }
}
