using System;
using System.Collections.Generic;
using System.IO;
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
        public Task<List<string>> ReadLinesAsync(string filePath)
        {
            return Task.Run(() =>
            {
                var lines = new List<string>();
                using (var stream = new FileStream(filePath, FileMode.Open,
                    FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(stream, Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: true))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                        lines.Add(line);
                }
                return lines;
            });
        }

        // ── Write ─────────────────────────────────────────────────────────────
        /// <summary>
        /// Writes <paramref name="lines"/> to <paramref name="filePath"/> as UTF-8 text.
        /// </summary>
        public void WriteLines(string filePath, IEnumerable<string> lines)
        {
            var sb = new StringBuilder();
            foreach (var line in lines)
                sb.AppendLine(line);
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
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
