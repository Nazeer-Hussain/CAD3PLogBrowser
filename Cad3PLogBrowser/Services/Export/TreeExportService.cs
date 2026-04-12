using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using CallStackNode = Cad3PLogBrowser.Services.CallStackNode;

namespace Cad3PLogBrowser.Services.Export
{
    /// <summary>
    /// Service for exporting tree structures (Call Tree, API Tree) to various formats.
    /// Supports JSON, XML, and CSV exports with full hierarchy preservation.
    /// </summary>
    public class TreeExportService
    {
        // ??????????????????????????????????????????????????????????????????????
        // JSON Export
        // ??????????????????????????????????????????????????????????????????????

        /// <summary>
        /// Exports call stack tree to JSON format.
        /// </summary>
        /// <param name="callStack">Root nodes of the call stack.</param>
        /// <param name="filePath">Output file path.</param>
        public void ExportToJson(List<CallStackNode> callStack, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("  \"callStack\": [");

            for (int i = 0; i < callStack.Count; i++)
            {
                ExportNodeToJson(callStack[i], sb, 2);
                if (i < callStack.Count - 1)
                    sb.AppendLine(",");
            }

            sb.AppendLine();
            sb.AppendLine("  ]");
            sb.AppendLine("}");

            File.WriteAllText(filePath, sb.ToString());
        }

        private void ExportNodeToJson(CallStackNode node, StringBuilder sb, int indent)
        {
            string indentStr = new string(' ', indent);

            sb.AppendLine($"{indentStr}{{");
            sb.AppendLine($"{indentStr}  \"method\": \"{EscapeJson(node.Label)}\",");
            sb.AppendLine($"{indentStr}  \"lineNumber\": {node.LineNumber},");
            sb.AppendLine($"{indentStr}  \"duration\": {node.DurationMs},");
            sb.AppendLine($"{indentStr}  \"exitLine\": {node.ExitLineNumber},");

            if (!string.IsNullOrEmpty(node.SourceFile))
                sb.AppendLine($"{indentStr}  \"sourceFile\": \"{EscapeJson(node.SourceFile)}\",");

            if (node.Children.Count > 0)
            {
                sb.AppendLine($"{indentStr}  \"children\": [");

                for (int i = 0; i < node.Children.Count; i++)
                {
                    ExportNodeToJson(node.Children[i], sb, indent + 4);
                    if (i < node.Children.Count - 1)
                        sb.AppendLine(",");
                }

                sb.AppendLine();
                sb.AppendLine($"{indentStr}  ]");
            }
            else
            {
                sb.AppendLine($"{indentStr}  \"children\": []");
            }

            sb.Append($"{indentStr}}}");
        }

        // ??????????????????????????????????????????????????????????????????????
        // XML Export
        // ??????????????????????????????????????????????????????????????????????

        /// <summary>
        /// Exports call stack tree to XML format.
        /// </summary>
        public void ExportToXml(List<CallStackNode> callStack, string filePath)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                Encoding = Encoding.UTF8
            };

            using (var writer = XmlWriter.Create(filePath, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("CallStack");

                foreach (var node in callStack)
                {
                    ExportNodeToXml(node, writer);
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        private void ExportNodeToXml(CallStackNode node, XmlWriter writer)
        {
            writer.WriteStartElement("Call");

            writer.WriteAttributeString("method", node.Label);
            writer.WriteAttributeString("line", node.LineNumber.ToString());
            writer.WriteAttributeString("duration", node.DurationMs.ToString());
            writer.WriteAttributeString("exitLine", node.ExitLineNumber.ToString());

            if (!string.IsNullOrEmpty(node.SourceFile))
                writer.WriteAttributeString("sourceFile", node.SourceFile);

            // Write children
            foreach (var child in node.Children)
            {
                ExportNodeToXml(child, writer);
            }

            writer.WriteEndElement();
        }

        // ??????????????????????????????????????????????????????????????????????
        // CSV Export (Flat representation)
        // ??????????????????????????????????????????????????????????????????????

        /// <summary>
        /// Exports call stack tree to CSV format (flattened hierarchy).
        /// </summary>
        public void ExportToCsv(List<CallStackNode> callStack, string filePath)
        {
            var lines = new List<string>
            {
                "Depth,Method,LineNumber,ExitLine,Duration(ms),SourceFile"
            };

            foreach (var node in callStack)
            {
                ExportNodeToCsv(node, 0, lines);
            }

            File.WriteAllLines(filePath, lines);
        }

        private void ExportNodeToCsv(CallStackNode node, int depth, List<string> lines)
        {
            string line = string.Format("{0},\"{1}\",{2},{3},{4},\"{5}\"",
                depth,
                EscapeCsv(node.Label),
                node.LineNumber,
                node.ExitLineNumber,
                node.DurationMs,
                EscapeCsv(node.SourceFile ?? ""));

            lines.Add(line);

            foreach (var child in node.Children)
            {
                ExportNodeToCsv(child, depth + 1, lines);
            }
        }

        // ??????????????????????????????????????????????????????????????????????
        // Helpers
        // ??????????????????????????????????????????????????????????????????????

        private string EscapeJson(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            return text.Replace("\\", "\\\\")
                      .Replace("\"", "\\\"")
                      .Replace("\n", "\\n")
                      .Replace("\r", "\\r")
                      .Replace("\t", "\\t");
        }

        private string EscapeCsv(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            if (text.Contains(",") || text.Contains("\"") || text.Contains("\n"))
                return text.Replace("\"", "\"\"");

            return text;
        }
    }
}
