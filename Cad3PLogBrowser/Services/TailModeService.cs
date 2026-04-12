using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Cad3PLogBrowser.Services.Core
{
    /// <summary>
    /// A4 — Auto-Reload / Tail Mode.
    /// Polls the log file for new content and fires <see cref="NewLinesAppended"/>
    /// when lines are added. Like `tail -f` behaviour.
    /// </summary>
    public class TailModeService : IDisposable
    {
        public event EventHandler<string[]> NewLinesAppended;

        private Timer     _pollTimer;
        private string    _filePath;
        private long      _lastFileSize;
        private bool      _active;
        private readonly int _intervalMs;

        public bool IsActive => _active;

        public TailModeService(int pollIntervalMs = 1000)
        {
            _intervalMs = pollIntervalMs;
            _pollTimer  = new Timer { Interval = pollIntervalMs };
            _pollTimer.Tick += PollFile;
        }

        public void Start(string filePath, long currentFileSize)
        {
            _filePath     = filePath;
            _lastFileSize = currentFileSize;
            _active       = true;
            _pollTimer.Start();
        }

        public void Stop()
        {
            _pollTimer.Stop();
            _active = false;
        }

        private void PollFile(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_filePath) || !File.Exists(_filePath)) return;
            try
            {
                var info = new FileInfo(_filePath);
                if (info.Length <= _lastFileSize) return;

                using (var stream = new FileStream(_filePath, FileMode.Open,
                    FileAccess.Read, FileShare.ReadWrite))
                {
                    stream.Seek(_lastFileSize, SeekOrigin.Begin);
                    using (var reader = new StreamReader(stream, Encoding.UTF8,
                        detectEncodingFromByteOrderMarks: false))
                    {
                        var newLines = new System.Collections.Generic.List<string>();
                        string line;
                        while ((line = reader.ReadLine()) != null)
                            newLines.Add(line);

                        if (newLines.Count > 0)
                            NewLinesAppended?.Invoke(this, newLines.ToArray());
                    }
                }
                _lastFileSize = info.Length;
            }
            catch { /* File may be locked briefly — skip this tick */ }
        }

        public void Dispose()
        {
            Stop();
            _pollTimer?.Dispose();
        }
    }
}
