namespace Cad3PLogBrowser.Services.Export
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Windows.Forms;
    using Cad3PLogBrowser.Models;
    using Cad3PLogBrowser.Utilities;

    /// <summary>
    /// Handles all export operations including CSV, text, Excel, and image exports.
    /// Consolidates export logic that was previously scattered in MainForm.
    /// </summary>
    /// <remarks>
    /// This service coordinates all export operations:
    /// - Filtered log entries to text/log files
    /// - Performance statistics to CSV
    /// - Call graph visualization to PNG/JPEG/BMP
    /// - Selected tree branches to files
    /// 
    /// Progress reporting is done via callbacks to keep the UI responsive.
    /// </remarks>
    public class ExportService
    {
        private readonly CsvExporter _csvExporter;
        private readonly ImageExporter _imageExporter;
        private readonly XlsxExporter _xlsxExporter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportService"/> class.
        /// Creates instances of specialized exporter classes.
        /// </summary>
        public ExportService()
        {
            _csvExporter = new CsvExporter();
            _imageExporter = new ImageExporter();
            _xlsxExporter = new XlsxExporter();
        }

        /// <summary>
        /// Exports filtered log entries to a text file.
        /// Includes a header with metadata about the export (source file, filters applied, etc.).
        /// </summary>
        /// <param name="entries">Log entries to export (already filtered).</param>
        /// <param name="filePath">Destination file path.</param>
        /// <param name="sourceFilePath">Original log file path (for header).</param>
        /// <param name="filterDescription">Description of filters applied (for header).</param>
        /// <param name="progressCallback">Optional callback for progress updates (percentage, message).</param>
        /// <exception cref="IOException">Thrown if file cannot be written.</exception>
        /// <example>
        /// exportService.ExportFilteredLogs(
        ///     filteredEntries,
        ///     "C:\\Logs\\filtered.log",
        ///     "C:\\Logs\\original.log",
        ///     "Text: 'error', Duration > 1000ms",
        ///     (progress, msg) => UpdateUI(progress, msg)
        /// );
        /// </example>
        public void ExportFilteredLogs(
            List<LogEntry> entries,
            string filePath,
            string sourceFilePath,
            string filterDescription,
            Action<int, string> progressCallback = null)
        {
            if (entries == null)
                throw new ArgumentNullException(nameof(entries));

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty", nameof(filePath));

            progressCallback?.Invoke(0, "Preparing export...");

            using (var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
            {
                // Write header
                writer.WriteLine("================================================================");
                writer.WriteLine(string.Format("Exported from: {0}", Constants.Application.Name));
                writer.WriteLine(string.Format("Export date: {0:yyyy-MM-dd HH:mm:ss}", DateTime.Now));
                writer.WriteLine(string.Format("Source file: {0}", sourceFilePath ?? "N/A"));
                writer.WriteLine(string.Format("Filters applied: {0}", filterDescription ?? "None"));
                writer.WriteLine(string.Format("Total lines exported: {0:N0}", entries.Count));
                writer.WriteLine("================================================================");
                writer.WriteLine();

                // Write entries with progress updates.
                // D4: write RawText (the original log line) rather than Text which
                // may contain UI-injected duration annotations like "[142 ms]".
                for (int i = 0; i < entries.Count; i++)
                {
                    writer.WriteLine(entries[i].RawText ?? entries[i].Text);

                    // Update progress every N lines
                    if (i % Constants.Performance.ProgressUpdateInterval == 0)
                    {
                        int progress = (int)((i / (double)entries.Count) * 100);
                        progressCallback?.Invoke(progress, $"Exporting... {i:N0}/{entries.Count:N0} lines");
                    }
                }
            }

            progressCallback?.Invoke(100, "Export complete");
        }

        /// <summary>
        /// Exports performance statistics to a CSV file.
        /// </summary>
        /// <param name="statistics">List of performance statistics to export.</param>
        /// <param name="filePath">Destination CSV file path.</param>
        /// <exception cref="IOException">Thrown if file cannot be written.</exception>
        /// <example>
        /// exportService.ExportPerformanceToCsv(perfStats, "C:\\Reports\\performance.csv");
        /// </example>
        public void ExportPerformanceToCsv(List<PerformanceStatistics> statistics, string filePath)
        {
            _csvExporter.ExportPerformanceStatistics(statistics, filePath);
        }

        /// <summary>
        /// Exports a tree branch to CSV (method name, depth, duration).
        /// </summary>
        /// <param name="rootNode">The root node of the branch to export.</param>
        /// <param name="filePath">Destination CSV file path.</param>
        /// <exception cref="IOException">Thrown if file cannot be written.</exception>
        public void ExportTreeBranchToCsv(CallStackNode rootNode, string filePath)
        {
            _csvExporter.ExportTreeBranch(rootNode, filePath);
        }

        /// <summary>
        /// Exports a call graph panel as an image file.
        /// Supports PNG, JPEG, and BMP formats based on file extension.
        /// </summary>
        /// <param name="callGraphPanel">The panel control to export.</param>
        /// <param name="filePath">Destination image file path.</param>
        /// <param name="width">Image width in pixels (default: panel width or 800, whichever is larger).</param>
        /// <param name="height">Image height in pixels (default: panel height or 600, whichever is larger).</param>
        /// <exception cref="ArgumentNullException">Thrown if callGraphPanel is null.</exception>
        /// <exception cref="IOException">Thrown if image cannot be saved.</exception>
        /// <example>
        /// exportService.ExportCallGraphAsImage(
        ///     callGraphPanel,
        ///     "C:\\Reports\\callgraph.png",
        ///     1920,
        ///     1080
        /// );
        /// </example>
        public void ExportCallGraphAsImage(
            Control callGraphPanel,
            string filePath,
            int width = 0,
            int height = 0)
        {
            _imageExporter.ExportControlAsImage(callGraphPanel, filePath, width, height);
        }

        /// <summary>
        /// Exports log entries for a specific tree branch (from ENTER to EXIT).
        /// </summary>
        /// <param name="allLogLines">All log file lines (raw text).</param>
        /// <param name="methodName">Name of the method to export.</param>
        /// <param name="filePath">Destination file path.</param>
        /// <returns>Number of lines exported.</returns>
        /// <remarks>
        /// Finds the first ENTER for the method and exports all lines until matching EXIT.
        /// Handles nested calls correctly by tracking depth.
        /// </remarks>
        public int ExportSelectedBranch(List<string> allLogLines, string methodName, string filePath)
        {
            if (allLogLines == null || allLogLines.Count == 0)
                throw new ArgumentException("No log lines to export", nameof(allLogLines));

            var branchLines = new List<string>();
            bool inBranch = false;
            int depth = 0;

            // B7: Use tab-field parsing (same approach as LogParserService) instead of
            // plain line.Contains() which fires on log message text that merely mentions
            // the ENTER/EXIT keywords, causing premature termination or over-collection.
            // A genuine ENTER/EXIT line has the keyword at tab-field index 4 (EntryType).
            foreach (var line in allLogLines)
            {
                bool isEnter = IsApiEntryLine(line, "ENTER");
                bool isExit  = IsApiEntryLine(line, "EXIT");
                bool isForMethod = line.Contains(methodName);

                if (!inBranch)
                {
                    if (isEnter && isForMethod)
                    {
                        inBranch = true;
                        depth = 1;
                        branchLines.Add(line);
                    }
                }
                else
                {
                    branchLines.Add(line);

                    if (isEnter) depth++;

                    if (isExit)
                    {
                        depth--;
                        if (depth == 0)
                            break; // found the matching EXIT � done
                    }
                }
            }

            File.WriteAllLines(filePath, branchLines);
            return branchLines.Count;
        }

        /// <summary>
        /// G7: exports a set of already-extracted branch log lines to an .xlsx workbook,
        /// one raw line per row, so they can be filtered/sorted in Excel.
        /// </summary>
        /// <param name="branchLines">Log lines to export (one row per line).</param>
        /// <param name="filePath">Destination .xlsx file path.</param>
        public void ExportBranchToXlsx(IList<string> branchLines, string filePath)
        {
            _xlsxExporter.ExportLines(branchLines, filePath, "Log Line");
        }

        /// <summary>
        /// Returns true only when the line's tab-field[4] (EntryType) exactly matches
        /// <paramref name="keyword"/> ("ENTER" or "EXIT").
        /// Avoids false positives from log message text containing those words.
        /// </summary>
        private static bool IsApiEntryLine(string line, string keyword)
        {
            // Count to the 5th tab (index 4 in 0-based tab-field array).
            // Payload starts after the 7th colon-separated prefix field.
            int tabCount = 0;
            int payloadStart = -1;

            // Find payload start: skip past the [filename] tag if present.
            int scanStart = 0;
            if (line.Length > 2 && line[0] == '[')
            {
                int cb = line.IndexOf("] ", StringComparison.Ordinal);
                if (cb > 1 && line.Substring(1, cb - 1).IndexOf('.') >= 0)
                    scanStart = cb + 2;
            }

            // Locate the payload after the 7th ": " separator (colon-field index 6).
            int colonFields = 0;
            for (int i = scanStart; i < line.Length - 1; i++)
            {
                if (line[i] == ':' && line[i + 1] == ' ')
                {
                    colonFields++;
                    if (colonFields == 6) { payloadStart = i + 2; break; }
                    i++; // skip the space
                }
            }
            if (payloadStart < 0) return false;

            // Walk to tab field 4 in the payload.
            for (int i = payloadStart; i < line.Length; i++)
            {
                if (line[i] == '\t')
                {
                    tabCount++;
                    if (tabCount == 4)
                    {
                        // Compare field 4 to keyword
                        int fieldStart = i + 1;
                        int fieldEnd   = line.IndexOf('\t', fieldStart);
                        if (fieldEnd < 0) fieldEnd = line.Length;
                        string field = line.Substring(fieldStart, fieldEnd - fieldStart).Trim();
                        return string.Equals(field, keyword, StringComparison.Ordinal);
                    }
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Specialized exporter for CSV format.
    /// Handles proper CSV escaping and formatting.
    /// </summary>
    public class CsvExporter
    {
        /// <summary>
        /// Exports performance statistics to CSV format.
        /// </summary>
        /// <param name="statistics">List of performance statistics.</param>
        /// <param name="filePath">Destination CSV file path.</param>
        public void ExportPerformanceStatistics(List<PerformanceStatistics> statistics, string filePath)
        {
            if (statistics == null)
                throw new ArgumentNullException(nameof(statistics));

            using (var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
            {
                // Write CSV header
                writer.WriteLine("API Name,Calls,Total (ms),Avg (ms),Min (ms),Max (ms),Self (ms),Source File");

                // Write data rows
                foreach (var stat in statistics)
                {
                    var row = new[]
                    {
                        stat.ApiName.EscapeCsv(),
                        stat.CallCount.ToString(),
                        stat.TotalDurationMs.ToString(),
                        stat.AvgDurationMs.ToString(),
                        stat.MinDurationMs.ToString(),
                        stat.MaxDurationMs.ToString(),
                        stat.SelfDurationMs.ToString(),
                        (stat.SourceFile ?? "").EscapeCsv()
                    };

                    writer.WriteLine(string.Join(",", row));
                }
            }
        }

        /// <summary>
        /// Exports a tree branch to CSV format (method, depth, duration).
        /// </summary>
        /// <param name="rootNode">Root of the branch to export.</param>
        /// <param name="filePath">Destination CSV file path.</param>
        public void ExportTreeBranch(CallStackNode rootNode, string filePath)
        {
            if (rootNode == null)
                throw new ArgumentNullException(nameof(rootNode));

            var rows = new List<string> { "Method,Depth,Duration (ms)" };

            // Recursively collect all nodes
            CollectBranchRows(rootNode, rows, 0);

            File.WriteAllLines(filePath, rows);
        }

        /// <summary>
        /// Recursively collects CSV rows for a tree branch.
        /// </summary>
        private void CollectBranchRows(CallStackNode node, List<string> rows, int depth)
        {
            var row = $"{node.Label.EscapeCsv()},{depth},{node.DurationMs}";
            rows.Add(row);

            foreach (var child in node.Children)
            {
                CollectBranchRows(child, rows, depth + 1);
            }
        }
    }

    /// <summary>
    /// Specialized exporter for Excel (.xlsx) format.
    /// </summary>
    /// <remarks>
    /// The project has no NuGet dependencies (old-style .csproj, zero PackageReference
    /// entries) and pulling in a full library like ClosedXML just to write a single flat
    /// sheet would be a heavy, unjustified addition. An .xlsx file is simply a ZIP archive
    /// of a handful of small XML parts (the Open Packaging Convention), so this writes that
    /// structure directly using System.IO.Compression, which the project already references.
    /// Cell text is written as inline strings (t="inlineStr") to avoid needing a
    /// sharedStrings.xml part.
    /// </remarks>
    public class XlsxExporter
    {
        private const string ContentTypesXml =
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
            "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
            "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
            "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
            "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>" +
            "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>" +
            "</Types>";

        private const string RootRelsXml =
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
            "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
            "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>" +
            "</Relationships>";

        private const string WorkbookRelsXml =
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
            "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
            "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>" +
            "</Relationships>";

        /// <summary>
        /// Writes <paramref name="lines"/> to a single-column worksheet, one row per line.
        /// </summary>
        /// <param name="lines">Raw text lines; each becomes one row in column A.</param>
        /// <param name="filePath">Destination .xlsx file path.</param>
        /// <param name="columnHeader">Header text for column A.</param>
        public void ExportLines(IList<string> lines, string filePath, string columnHeader)
        {
            if (lines == null)
                throw new ArgumentNullException(nameof(lines));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty", nameof(filePath));

            string workbookXml =
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" " +
                "xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">" +
                "<sheets><sheet name=\"Log Snippet\" sheetId=\"1\" r:id=\"rId1\"/></sheets>" +
                "</workbook>";

            // Delete-then-create: ZipFile requires the destination not to already exist.
            if (File.Exists(filePath))
                File.Delete(filePath);

            using (var archive = ZipFile.Open(filePath, ZipArchiveMode.Create))
            {
                AddEntry(archive, "[Content_Types].xml", ContentTypesXml);
                AddEntry(archive, "_rels/.rels", RootRelsXml);
                AddEntry(archive, "xl/workbook.xml", workbookXml);
                AddEntry(archive, "xl/_rels/workbook.xml.rels", WorkbookRelsXml);
                AddEntry(archive, "xl/worksheets/sheet1.xml", BuildSheetXml(columnHeader, lines));
            }
        }

        private static string BuildSheetXml(string columnHeader, IList<string> lines)
        {
            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sb.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>");

            AppendRow(sb, 1, columnHeader);
            for (int i = 0; i < lines.Count; i++)
            {
                AppendRow(sb, i + 2, lines[i]);
            }

            sb.Append("</sheetData></worksheet>");
            return sb.ToString();
        }

        private static void AppendRow(StringBuilder sb, int rowNumber, string text)
        {
            sb.Append("<row r=\"").Append(rowNumber).Append("\">")
              .Append("<c r=\"A").Append(rowNumber).Append("\" t=\"inlineStr\"><is><t xml:space=\"preserve\">")
              .Append(EscapeXml(text))
              .Append("</t></is></c></row>");
        }

        /// <summary>
        /// Escapes XML-reserved characters and strips control characters that are
        /// invalid in XML 1.0 (log lines occasionally contain stray control bytes).
        /// </summary>
        private static string EscapeXml(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            var sb = new StringBuilder(text.Length);
            foreach (char c in text)
            {
                switch (c)
                {
                    case '&': sb.Append("&amp;"); break;
                    case '<': sb.Append("&lt;"); break;
                    case '>': sb.Append("&gt;"); break;
                    default:
                        if (c < 0x20 && c != '\t' && c != '\n' && c != '\r')
                            sb.Append(' '); // invalid XML 1.0 control character
                        else
                            sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        private static void AddEntry(ZipArchive archive, string entryName, string content)
        {
            ZipArchiveEntry entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
            using (var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false)))
            {
                writer.Write(content);
            }
        }
    }

    /// <summary>
    /// Specialized exporter for image formats (PNG, JPEG, BMP).
    /// </summary>
    public class ImageExporter
    {
        /// <summary>
        /// Exports a WinForms control as an image file.
        /// Format is determined by the file extension.
        /// </summary>
        /// <param name="control">The control to export (e.g., Panel, PictureBox).</param>
        /// <param name="filePath">Destination image file path.</param>
        /// <param name="width">Image width (0 = use control width or 800, whichever is larger).</param>
        /// <param name="height">Image height (0 = use control height or 600, whichever is larger).</param>
        /// <exception cref="ArgumentNullException">Thrown if control is null.</exception>
        public void ExportControlAsImage(Control control, string filePath, int width = 0, int height = 0)
        {
            if (control == null)
                throw new ArgumentNullException(nameof(control));

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty", nameof(filePath));

            // Determine dimensions
            int imageWidth = width > 0 ? width : Math.Max(800, control.Width);
            int imageHeight = height > 0 ? height : Math.Max(600, control.Height);

            // Determine format from file extension
            ImageFormat format = GetImageFormatFromExtension(filePath);

            // Create bitmap and draw control
            using (var bitmap = new Bitmap(imageWidth, imageHeight))
            {
                var rect = new Rectangle(0, 0, imageWidth, imageHeight);
                control.DrawToBitmap(bitmap, rect);

                // Save to file
                bitmap.Save(filePath, format);
            }
        }

        /// <summary>
        /// Determines the image format from a file extension.
        /// </summary>
        /// <param name="filePath">File path with extension.</param>
        /// <returns>ImageFormat enum value (PNG, JPEG, or BMP).</returns>
        private ImageFormat GetImageFormatFromExtension(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return ImageFormat.Jpeg;

                case ".bmp":
                    return ImageFormat.Bmp;

                case ".png":
                default:
                    return ImageFormat.Png;
            }
        }
    }
}
