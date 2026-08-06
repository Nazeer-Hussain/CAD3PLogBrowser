using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cad3PLogBrowser.Services;

namespace Cad3PLogBrowser.Services.Export
{
    /// <summary>
    /// D7 — Export API List to CSV.
    /// I5 — Screenshot / Snapshot helper (returns a Bitmap from any Control).
    /// </summary>
    public class ApiExportService
    {
        // D7: Export API list
        /// <param name="perfStatsByName">Optional Total/Avg/Min/Max timing per API name
        /// (the same stats the Performance tab and API Tree tooltips already show).
        /// APIs with no matched ENTER/EXIT pairs — or when this is omitted entirely —
        /// get blank timing columns rather than misleading zeros.</param>
        public void ExportApiListToCsv(List<ApiCallNode> apiNodes, string filePath,
            Dictionary<string, ApiPerfStats> perfStatsByName = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("API Name,Call Count,First Line,Total Time (ms),Avg Time (ms),Min Time (ms),Max Time (ms),All Lines");
            foreach (var node in apiNodes)
            {
                string lines = string.Join(";", node.LineNumbers);
                string totalMs = "", avgMs = "", minMs = "", maxMs = "";

                if (perfStatsByName != null
                    && perfStatsByName.TryGetValue(node.ApiName, out var stats)
                    && stats.TimedCallCount > 0)
                {
                    totalMs = stats.TotalDurationMs.ToString();
                    avgMs   = stats.AvgDurationMs.ToString();
                    minMs   = stats.MinDurationMs.ToString();
                    maxMs   = stats.MaxDurationMs.ToString();
                }

                sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                    CsvEscape(node.ApiName),
                    node.LineNumbers.Count,
                    node.FirstLine,
                    totalMs, avgMs, minMs, maxMs,
                    CsvEscape(lines)));
            }
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private static string CsvEscape(string s)
        {
            if (s == null) return "";
            if (s.Contains(",") || s.Contains("\"") || s.Contains("\n"))
                return "\"" + s.Replace("\"", "\"\"") + "\"";
            return s;
        }
    }
}
