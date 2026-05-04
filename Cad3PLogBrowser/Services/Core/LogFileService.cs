using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cad3PLogBrowser.Services
{
    /// <summary>
    /// Handles all file I/O for log files: async reading, writing, and file-change watching.
    /// </summary>
    public class LogFileService : IDisposable
    {
        // ── Events ────────────────────────────────────────────────────────────
        /// <summary>Raised (on the UI thread) when the watched file changes on disk.</summary>
        public event EventHandler FileChangedOnDisk;

        // ── Fields ────────────────────────────────────────────────────────────
        private FileSystemWatcher _watcher;
        private readonly System.ComponentModel.ISynchronizeInvoke _syncObject;

        // ── Construction ──────────────────────────────────────────────────────
        /// <param name="syncObject">Usually the main form; marshals watcher events to the UI thread.</param>
        public LogFileService(System.ComponentModel.ISynchronizeInvoke syncObject)
        {
            _syncObject = syncObject;
        }

        // ── Read ──────────────────────────────────────────────────────────────
        /// <summary>
        /// Reads all lines from <paramref name="filePath"/> asynchronously.
        /// Uses FileShare.ReadWrite so log files still being written can be opened.
        /// </summary>
        /// <param name="filePath">Path to the log file</param>
        /// <param name="progressCallback">Optional callback for progress updates (percentage 0-100, message)</param>
        public Task<List<string>> ReadLinesAsync(string filePath, Action<int, string> progressCallback = null)
        {
            return Task.Run(() =>
            {
                var lines = new List<string>();
                var fileInfo = new FileInfo(filePath);
                long fileSize = fileInfo.Length;
                long bytesRead = 0;
                int lastReportedProgress = 0;

                using (var stream = new FileStream(filePath, FileMode.Open,
                    FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(stream, Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: true))
                {
                    string line;
                    int lineCount = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        lines.Add(line);
                        lineCount++;

                        // Report progress every ~5%
                        if (progressCallback != null && fileSize > 0)
                        {
                            bytesRead = stream.Position;
                            int progress = (int)((bytesRead * 100) / fileSize);
                            if (progress > lastReportedProgress + 5 || progress >= 100)
                            {
                                lastReportedProgress = progress;
                                progressCallback(progress, $"Reading: {lineCount:N0} lines");
                            }
                        }
                    }
                }
                return lines;
            });
        }

        // ── Write ─────────────────────────────────────────────────────────────
        /// <summary>
        /// Writes <paramref name="lines"/> to <paramref name="filePath"/> as UTF-8 text.
        /// Lines are streamed directly to disk to avoid loading the entire content into memory.
        /// </summary>
        /// <param name="filePath">Path to save the file</param>
        /// <param name="lines">Lines to write</param>
        /// <param name="progressCallback">Optional callback for progress updates (percentage 0-100, message)</param>
        public void WriteLines(string filePath, IEnumerable<string> lines, Action<int, string> progressCallback = null)
        {
            var linesList = lines as IList<string> ?? lines.ToList();
            int totalLines = linesList.Count;
            int linesWritten = 0;
            int lastReportedProgress = 0;

            // Stream lines directly to disk — avoids allocating a single large StringBuilder
            // that can cause memory pressure or OOM on large log files.
            using (var writer = new StreamWriter(filePath, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                foreach (var line in linesList)
                {
                    writer.WriteLine(line);
                    linesWritten++;

                    // Report progress every ~10%
                    if (progressCallback != null && totalLines > 1000)
                    {
                        int progress = (linesWritten * 100) / totalLines;
                        if (progress > lastReportedProgress + 10 || linesWritten == totalLines)
                        {
                            lastReportedProgress = progress;
                            progressCallback(progress, $"Writing: {linesWritten:N0}/{totalLines:N0} lines");
                        }
                    }
                }
            }

            if (progressCallback != null)
                progressCallback(100, "Complete");
        }

        // ── Watch ─────────────────────────────────────────────────────────────
        /// <summary>
        /// Starts watching <paramref name="filePath"/> for size/write changes.
        /// Raises <see cref="FileChangedOnDisk"/> when a change is detected.
        /// </summary>
        public void WatchFile(string filePath)
        {
            StopWatching();

            _watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(filePath),
                Filter = Path.GetFileName(filePath),
                NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite,
                SynchronizingObject = _syncObject,
                EnableRaisingEvents = true
            };

            _watcher.Changed += OnWatcherChanged;
        }

        public void StopWatching()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Changed -= OnWatcherChanged;
                _watcher.Dispose();
                _watcher = null;
            }
        }

        private void OnWatcherChanged(object sender, FileSystemEventArgs e)
        {
            FileChangedOnDisk?.Invoke(this, EventArgs.Empty);
        }

        // ── Disposal ──────────────────────────────────────────────────────────
        public void Dispose()
        {
            StopWatching();
        }
    }
}
