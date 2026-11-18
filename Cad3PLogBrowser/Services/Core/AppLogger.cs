namespace Cad3PLogBrowser.Services.Core
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Lightweight append-only log for non-fatal failures that were previously
    /// swallowed silently (settings/bookmarks/recent-files persistence errors) --
    /// a locked or corrupted file in any of these paths used to fail with zero
    /// signal anywhere, making "my settings keep resetting" impossible to diagnose.
    /// Writes to %AppData%\CAD3PLogBrowser\app.log (max 200 KB, then rolled over).
    /// All methods are non-throwing -- a logging failure must never crash the app,
    /// mirroring Services.Update.UpdateLogger's proven pattern.
    /// </summary>
    public static class AppLogger
    {
        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CAD3PLogBrowser", "app.log");

        private const long MaxLogBytes = 200 * 1024; // 200 KB before roll-over

        public static void Log(string message)
        {
            try
            {
                string dir = Path.GetDirectoryName(LogPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                if (File.Exists(LogPath) && new FileInfo(LogPath).Length > MaxLogBytes)
                {
                    string archive = LogPath + ".old";
                    if (File.Exists(archive)) File.Delete(archive);
                    File.Move(LogPath, archive);
                }

                string line = string.Format("[{0:yyyy-MM-dd HH:mm:ss}] {1}{2}",
                    DateTime.UtcNow, message, Environment.NewLine);

                File.AppendAllText(LogPath, line, Encoding.UTF8);
            }
            catch { /* non-fatal */ }
        }

        public static void Log(string format, params object[] args)
        {
            Log(string.Format(format, args));
        }

        /// <summary>Returns the full path of the current log file.</summary>
        public static string LogFilePath => LogPath;
    }
}
