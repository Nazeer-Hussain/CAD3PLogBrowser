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
    /// All config is portable — no registry writes.
    /// Location: %AppData%\CAD3PLogBrowser\settings.json
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
        public string  Theme                { get; set; } = "Light"; // "Light" | "Dark"
        public string  ToolbarIconSize      { get; set; } = "Medium"; // "Small" | "Medium" | "Large"
        public bool    ShowToolbar          { get; set; } = true;

        // ── Font settings (Feature H5) ────────────────────────────────────────
        public string     LogFontFamily     { get; set; } = "Consolas";
        public float      LogFontSize       { get; set; } = 9.0f;
        public FontStyle  LogFontStyle      { get; set; } = FontStyle.Regular;

        // ── Search history (Feature B6) ───────────────────────────────────────
        public List<string> SearchHistory   { get; set; } = new List<string>();

        // ── Window state persistence (Feature 1a/1b/1c) ───────────────────────
        public int     WindowWidth          { get; set; } = 1024;
        public int     WindowHeight         { get; set; } = 768;
        public int     WindowLeft           { get; set; } = -1;  // -1 = not set
        public int     WindowTop            { get; set; } = -1;  // -1 = not set
        public string  WindowState          { get; set; } = "Normal"; // "Normal" | "Maximized"

        // ── Performance guards ─────────────────────────────────────────────────
        public long    MaxFileSizeMbForListView { get; set; } = 50; // skip list if > N MB
        public long    SlowCallThresholdMs      { get; set; } = 1000;

        // ── Grok integration ──────────────────────────────────────────────────
        public string  GrokUrl          { get; set; } = "";
        public string  ClaudeApiKey     { get; set; } = "";  // L1-L6: AI features (never log raw content)

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
