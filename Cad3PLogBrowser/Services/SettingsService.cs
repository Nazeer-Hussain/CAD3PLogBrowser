using System;
using System.IO;
using Microsoft.Win32;

namespace Cad3PLogBrowser.Services
{
    /// <summary>
    /// Settings service that uses JSON file storage instead of Windows Registry.
    /// Provides migration from old registry-based settings for backward compatibility.
    /// All settings are now stored in %AppData%\CAD3PLogBrowser\settings.json via AppSettings.
    /// </summary>
    public class SettingsService
    {
        // ── Registry migration keys (for backward compatibility) ─────────────
        private const string RegistryKeyPath = @"Software\CAD3PLogBrowser";
        private const string KeySplitter = "LastSplitter";
        private const string KeyLastDir = "LastDirectory";

        // ── Defaults ──────────────────────────────────────────────────────────
        private const int DefaultSplitterDistance = 285;

        private readonly AppSettings _appSettings;
        private bool _migrationAttempted = false;

        public SettingsService()
        {
            _appSettings = AppSettings.Load();

            // One-time migration from registry to JSON
            MigrateFromRegistryIfNeeded();
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

        // ── Migration from Registry ───────────────────────────────────────────
        /// <summary>
        /// One-time migration from old registry-based settings to JSON.
        /// This ensures existing users don't lose their settings.
        /// </summary>
        private void MigrateFromRegistryIfNeeded()
        {
            if (_migrationAttempted)
                return;

            _migrationAttempted = true;

            try
            {
                // Check if we have any settings in JSON already
                bool hasJsonSettings = _appSettings.SplitterDistance > 0 || 
                                       !string.IsNullOrEmpty(_appSettings.InitialDirectory);

                if (hasJsonSettings)
                {
                    // Already migrated or new user, skip migration
                    return;
                }

                // Try to read from old registry location
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false))
                {
                    if (key == null)
                    {
                        // No registry settings to migrate
                        return;
                    }

                    // Migrate splitter distance
                    var splitterValue = key.GetValue(KeySplitter);
                    if (splitterValue != null && int.TryParse(splitterValue.ToString(), out int splitter) && splitter > 0)
                    {
                        _appSettings.SplitterDistance = splitter;
                    }

                    // Migrate last directory
                    var lastDirValue = key.GetValue(KeyLastDir);
                    if (lastDirValue != null && !string.IsNullOrEmpty(lastDirValue.ToString()))
                    {
                        _appSettings.InitialDirectory = lastDirValue.ToString();
                    }

                    // Save migrated settings to JSON
                    _appSettings.Save();

                    // Optional: Clean up registry after successful migration
                    // Uncomment the following line if you want to remove old registry entries
                    // CleanupRegistrySettings();
                }
            }
            catch
            {
                // Migration failure is non-fatal - app will use defaults
            }
        }

        /// <summary>
        /// Optional: Removes old registry settings after successful migration.
        /// Uncomment the call in MigrateFromRegistryIfNeeded() to enable.
        /// </summary>
        private void CleanupRegistrySettings()
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(RegistryKeyPath, false);
            }
            catch
            {
                // Cleanup failure is non-fatal
            }
        }
    }
}
