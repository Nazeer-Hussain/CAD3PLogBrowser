namespace Cad3PLogBrowser.Services.Export
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportService"/> class.
        /// Creates instances of specialized exporter classes.
        /// </summary>
        public ExportService()
        {
            _csvExporter = new CsvExporter();
            _imageExporter = new ImageExporter();
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

            // Find the method's ENTER and collect all lines until matching EXIT
            foreach (var line in allLogLines)
            {
                // Check if this is the ENTER line for our method
                if (line.Contains(Constants.Parsing.EnterKeyword) && line.Contains(methodName))
                {
                    inBranch = true;
                    depth = 1;
                    branchLines.Add(line);
                }
                else if (inBranch)
                {
                    branchLines.Add(line);

                    // Track depth for nested calls
                    if (line.Contains(Constants.Parsing.EnterKeyword))
                        depth++;

                    if (line.Contains(Constants.Parsing.ExitKeyword))
                    {
                        depth--;
                        if (depth == 0)
                        {
                            // Found matching EXIT - we're done
                            break;
                        }
                    }
                }
            }

            // Write to file
            File.WriteAllLines(filePath, branchLines);
            return branchLines.Count;
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
