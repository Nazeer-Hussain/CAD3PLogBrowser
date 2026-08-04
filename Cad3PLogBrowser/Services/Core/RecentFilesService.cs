using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;

namespace Cad3PLogBrowser.Services.Core
{
    /// <summary>
    /// A3 — Recent Files list, persisted separately from the main settings file.
    /// Stored at %AppData%\CAD3PLogBrowser\recentfiles.json so it survives a
    /// settings.json reset/corruption independently.
    /// </summary>
    public class RecentFilesService
    {
        public List<string> RecentFiles { get; private set; } = new List<string>();

        /// <summary>Maximum number of entries to keep; caller sets this from AppSettings.MaxRecentFiles.</summary>
        public int MaxRecentFiles { get; set; } = 10;

        private static string FilePath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CAD3PLogBrowser", "recentfiles.json");

        private static string LegacySettingsPath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CAD3PLogBrowser", "settings.json");

        public void Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    // One-time migration: older versions stored this list inside settings.json.
                    MigrateFromLegacySettings();
                    return;
                }

                var bytes = File.ReadAllBytes(FilePath);
                var ser   = new DataContractJsonSerializer(typeof(List<string>));
                using (var ms = new MemoryStream(bytes))
                    RecentFiles = (List<string>)ser.ReadObject(ms) ?? new List<string>();
            }
            catch
            {
                // Missing/corrupt file: start with an empty list rather than crash.
                RecentFiles = new List<string>();
            }
        }

        /// <summary>
        /// A3: reads a "RecentFiles" array straight out of the legacy settings.json (without
        /// depending on an AppSettings.RecentFiles property, since that field no longer
        /// exists) and adopts it as the initial recentfiles.json, best-effort only.
        /// </summary>
        private void MigrateFromLegacySettings()
        {
            RecentFiles = new List<string>();
            try
            {
                if (!File.Exists(LegacySettingsPath)) return;

                string json = File.ReadAllText(LegacySettingsPath);
                var arrayMatch = Regex.Match(json, "\"RecentFiles\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline);
                if (!arrayMatch.Success) return;

                foreach (Match m in Regex.Matches(arrayMatch.Groups[1].Value, "\"((?:[^\"\\\\]|\\\\.)*)\""))
                {
                    string entry = m.Groups[1].Value
                        .Replace("\\\\", "\\").Replace("\\\"", "\"").Replace("\\/", "/");
                    if (!string.IsNullOrWhiteSpace(entry)) RecentFiles.Add(entry);
                }

                if (RecentFiles.Count > 0) Save();
            }
            catch
            {
                RecentFiles = new List<string>();
            }
        }

        public void Save()
        {
            try
            {
                string dir = Path.GetDirectoryName(FilePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                var ser = new DataContractJsonSerializer(typeof(List<string>));
                using (var ms = new MemoryStream())
                {
                    ser.WriteObject(ms, RecentFiles);
                    File.WriteAllBytes(FilePath, ms.ToArray());
                }
            }
            catch { /* Non-fatal */ }
        }

        public void AddRecentFile(string filePath)
        {
            RecentFiles.Remove(filePath);
            RecentFiles.Insert(0, filePath);
            while (RecentFiles.Count > MaxRecentFiles)
                RecentFiles.RemoveAt(RecentFiles.Count - 1);
            Save();
        }

        public void RemoveRecentFile(string filePath)
        {
            if (RecentFiles.Remove(filePath))
                Save();
        }

        public void ClearRecentFiles()
        {
            RecentFiles.Clear();
            Save();
        }
    }
}
