using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows.Forms;

namespace Cad3PLogBrowser.Services
{
    /// <summary>
    /// Typed application settings stored as JSON in the user's AppData folder.
    /// All config is portable — no registry writes (except splitter position kept for
    /// backwards compat via SettingsService).
    /// </summary>
    public class AppSettings
    {
        // ── File handling ─────────────────────────────────────────────────────
        public List<string> RecentFiles      { get; set; } = new List<string>();
        public int          MaxRecentFiles   { get; set; } = 10;
        public string       InitialDirectory { get; set; } = "";

        // ── UI preferences ────────────────────────────────────────────────────
        public string  HighlightColorName   { get; set; } = "Yellow";
        public string  InitialView          { get; set; } = "LogView"; // "LogView" | "ApiView"
        public string  SaveSnippetSuffix    { get; set; } = "_snippet";
        public int     SplitterDistance     { get; set; } = 285;
        public bool    ShowLogTab           { get; set; } = true;
        public bool    ShowPerformanceTab   { get; set; } = true;
        public bool    ShowLogDetailsTab    { get; set; } = true;
        public bool    ShowCallGraphTab     { get; set; } = true;

        // ── Performance guards ─────────────────────────────────────────────────
        public long    MaxFileSizeMbForListView { get; set; } = 50; // skip list if > N MB
        public long    SlowCallThresholdMs      { get; set; } = 1000;

        // ── Grok integration ──────────────────────────────────────────────────
        public string  GrokUrl { get; set; } = "";

        // ── Helpers ───────────────────────────────────────────────────────────
        [System.Runtime.Serialization.IgnoreDataMember]
        public Color HighlightColor
        {
            get
            {
                try { return Color.FromName(HighlightColorName); }
                catch { return Color.Yellow; }
            }
        }

        // ── Persistence ───────────────────────────────────────────────────────
        private static string SettingsFilePath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CAD3PLogBrowser", "settings.json");

        public static AppSettings Load()
        {
            try
            {
                string path = SettingsFilePath;
                if (!File.Exists(path)) return new AppSettings();

                var bytes = File.ReadAllBytes(path);
                var ser   = new DataContractJsonSerializer(typeof(AppSettings));
                using (var ms = new MemoryStream(bytes))
                    return (AppSettings)ser.ReadObject(ms);
            }
            catch
            {
                return new AppSettings();
            }
        }

        public void Save()
        {
            try
            {
                string dir = Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                var ser = new DataContractJsonSerializer(typeof(AppSettings));
                using (var ms = new MemoryStream())
                {
                    ser.WriteObject(ms, this);
                    File.WriteAllBytes(SettingsFilePath, ms.ToArray());
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
        }
    }
}
