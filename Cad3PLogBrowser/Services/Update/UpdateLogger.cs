namespace Cad3PLogBrowser.Services.Update
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Lightweight append-only log for the auto-update subsystem.
    /// Writes to %AppData%\CAD3PLogBrowser\update.log (max 200 KB, then rolled over).
    /// All methods are non-throwing — a logging failure must never crash the app.
    /// </summary>
    public static class UpdateLogger
    {
        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CAD3PLogBrowser", "update.log");

        private const long MaxLogBytes = 200 * 1024; // 200 KB before roll-over

        public static void Log(string message)
        {
            try
            {
                string dir = Path.GetDirectoryName(LogPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                // Roll over when file exceeds the size limit
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
