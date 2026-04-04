using System;
using System.Windows.Forms;

namespace Cad3PLogBrowser.Services
{
    /// <summary>
    /// Typed wrapper around the per-user registry store (Application.UserAppDataRegistry).
    /// All reads are safe on first-run (missing keys return defaults).
    /// All writes are non-fatal (failures are silently swallowed).
    /// </summary>
    public class SettingsService
    {
        // ── Keys ──────────────────────────────────────────────────────────────
        private const string KeySplitter = "LastSplitter";
        private const string KeyLastDir  = "LastDirectory";

        // ── Defaults ──────────────────────────────────────────────────────────
        private const int DefaultSplitterDistance = 285;

        // ── Read ──────────────────────────────────────────────────────────────
        /// <summary>Returns the saved splitter distance, or the default if not set.</summary>
        public int LoadSplitterDistance()
        {
            return ReadInt(KeySplitter, DefaultSplitterDistance);
        }

        /// <summary>Returns the last directory the user opened a file from.</summary>
        public string LoadLastDirectory()
        {
            return ReadString(KeyLastDir, string.Empty);
        }

        // ── Write ─────────────────────────────────────────────────────────────
        public void SaveSplitterDistance(int distance)
        {
            WriteValue(KeySplitter, distance.ToString());
        }

        public void SaveLastDirectory(string directory)
        {
            WriteValue(KeyLastDir, directory);
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private int ReadInt(string key, int defaultValue)
        {
            try
            {
                var raw = Application.UserAppDataRegistry.GetValue(key);
                if (raw != null && int.TryParse(raw.ToString(), out int value) && value > 0)
                    return value;
            }
            catch { }
            return defaultValue;
        }

        private string ReadString(string key, string defaultValue)
        {
            try
            {
                var raw = Application.UserAppDataRegistry.GetValue(key);
                if (raw != null)
                    return raw.ToString();
            }
            catch { }
            return defaultValue;
        }

        private void WriteValue(string key, string value)
        {
            try
            {
                Application.UserAppDataRegistry.SetValue(key, value);
            }
            catch { /* Non-fatal: registry write failure should not crash the app */ }
        }
    }
}
