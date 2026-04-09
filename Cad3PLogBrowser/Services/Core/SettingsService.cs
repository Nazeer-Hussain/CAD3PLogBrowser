using System;

namespace Cad3PLogBrowser.Services
{
    /// <summary>
    /// Settings service that uses JSON file storage exclusively.
    /// All settings are stored in %AppData%\CAD3PLogBrowser\settings.json via AppSettings.
    /// No registry dependency - fully portable and modern.
    /// </summary>
    public class SettingsService
    {
        // ── Defaults ──────────────────────────────────────────────────────────
        private const int DefaultSplitterDistance = 285;

        private readonly AppSettings _appSettings;

        public SettingsService(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        // ── Read ──────────────────────────────────────────────────────────────
        /// <summary>Returns the saved splitter distance, or the default if not set.</summary>
        public int LoadSplitterDistance()
        {
            if (_appSettings.SplitterDistance > 0)
                return _appSettings.SplitterDistance;
            return DefaultSplitterDistance;
        }

        /// <summary>Returns the last directory the user opened a file from.</summary>
        public string LoadLastDirectory()
        {
            return _appSettings.InitialDirectory ?? string.Empty;
        }

        // ── Write ─────────────────────────────────────────────────────────────
        public void SaveSplitterDistance(int distance)
        {
            _appSettings.SplitterDistance = distance;
            _appSettings.Save();
        }

        public void SaveLastDirectory(string directory)
        {
            _appSettings.InitialDirectory = directory;
            _appSettings.Save();
        }
    }
}
