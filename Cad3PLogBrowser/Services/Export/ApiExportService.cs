using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cad3PLogBrowser.Models;

namespace Cad3PLogBrowser.Services.Export
{
    /// <summary>
    /// D7 — Export API List to CSV.
    /// I5 — Screenshot / Snapshot helper (returns a Bitmap from any Control).
    /// </summary>
    public class ApiExportService
    {
        // D7: Export API list
        public void ExportApiListToCsv(List<ApiCallNode> apiNodes, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("API Name,Call Count,First Line,All Lines");
            foreach (var node in apiNodes)
            {
                string lines = string.Join(";", node.LineNumbers);
                sb.AppendLine(string.Format("{0},{1},{2},{3}",
                    CsvEscape(node.ApiName),
                    node.LineNumbers.Count,
                    node.FirstLine,
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
