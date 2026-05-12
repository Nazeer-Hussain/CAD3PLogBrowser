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
        /// Writes directly to a StreamWriter so the entire document is never
        /// held in memory — avoids OutOfMemoryException on large call trees.
        /// </summary>
        /// <param name="callStack">Root nodes of the call stack.</param>
        /// <param name="filePath">Output file path.</param>
        public void ExportToJson(List<CallStackNode> callStack, string filePath)
        {
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8, 65536))
            {
                writer.WriteLine("{");
                writer.WriteLine("  \"callStack\": [");

                for (int i = 0; i < callStack.Count; i++)
                {
                    ExportNodeToJson(callStack[i], writer, 2);
                    if (i < callStack.Count - 1)
                        writer.WriteLine(",");
                }

                writer.WriteLine();
                writer.WriteLine("  ]");
                writer.WriteLine("}");
            }
        }

        private void ExportNodeToJson(CallStackNode node, StreamWriter writer, int indent)
        {
            string ind  = new string(' ', indent);
            string ind2 = new string(' ', indent + 2);

            writer.WriteLine(ind  + "{");
            writer.WriteLine(ind2 + "\"method\": \""     + EscapeJson(node.Label) + "\",");
            writer.WriteLine(ind2 + "\"lineNumber\": "   + node.LineNumber + ",");
            writer.WriteLine(ind2 + "\"duration\": "     + node.DurationMs + ",");
            writer.WriteLine(ind2 + "\"exitLine\": "     + node.ExitLineNumber + ",");

            if (!string.IsNullOrEmpty(node.SourceFile))
                writer.WriteLine(ind2 + "\"sourceFile\": \"" + EscapeJson(node.SourceFile) + "\",");

            if (node.Children.Count > 0)
            {
                writer.WriteLine(ind2 + "\"children\": [");

                for (int i = 0; i < node.Children.Count; i++)
                {
                    ExportNodeToJson(node.Children[i], writer, indent + 4);
                    if (i < node.Children.Count - 1)
                        writer.WriteLine(",");
                }

                writer.WriteLine();
                writer.WriteLine(ind2 + "]");
            }
            else
            {
                writer.WriteLine(ind2 + "\"children\": []");
            }

            writer.Write(ind + "}");
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
        /// Streams directly to disk to avoid holding all rows in memory.
        /// </summary>
        public void ExportToCsv(List<CallStackNode> callStack, string filePath)
        {
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8, 65536))
            {
                writer.WriteLine("Depth,Method,LineNumber,ExitLine,Duration(ms),SourceFile");

                foreach (var node in callStack)
                    ExportNodeToCsv(node, 0, writer);
            }
        }

        private void ExportNodeToCsv(CallStackNode node, int depth, StreamWriter writer)
        {
            writer.WriteLine(string.Format("{0},\"{1}\",{2},{3},{4},\"{5}\"",
                depth,
                EscapeCsv(node.Label),
                node.LineNumber,
                node.ExitLineNumber,
                node.DurationMs,
                EscapeCsv(node.SourceFile ?? "")));

            foreach (var child in node.Children)
                ExportNodeToCsv(child, depth + 1, writer);
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
