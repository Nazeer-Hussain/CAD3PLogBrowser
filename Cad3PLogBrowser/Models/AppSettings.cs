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
        // A3: the recent-files list itself lives in recentfiles.json (see
        // Services.Core.RecentFilesService), not here — only the preference below does.
        public int          MaxRecentFiles   { get; set; } = 10;
        public string       InitialDirectory { get; set; } = "";
        // G10: pre-populates the Report Errors dialog's recipient field; remembered
        // across sessions once the user fills it in once.
        public string       ErrorReportEmail { get; set; } = "";

        // ── UI preferences ────────────────────────────────────────────────────
        public string  HighlightColorName   { get; set; } = "Yellow";
        public string  InitialView          { get; set; } = "Log"; // matches SettingsForm combo items
        public string  SaveSnippetSuffix    { get; set; } = "_snippet";
        public int     SplitterDistance     { get; set; } = 285;
        public bool    ShowLogTab           { get; set; } = true;
        public bool    ShowPerformanceTab   { get; set; } = true;
        public bool    ShowLogDetailsTab    { get; set; } = true;
        public bool    ShowCallGraphTab     { get; set; } = true;
        public bool    ShowTimelineTab      { get; set; } = true;
        public bool    ShowHeatmapTab       { get; set; } = true;
        public bool    ShowAiTab            { get; set; } = true;
        public string  Theme                { get; set; } = "Light"; // "Light" | "Dark"
        public string  DefaultTreeView      { get; set; } = "Call";  // "Call" | "Api"
        // H2: whether the whole right-side tab panel is collapsed (View > Hide Tabs).
        public bool    HideRightPanel       { get; set; } = false;
        public string  ToolbarIconSize      { get; set; } = "Medium"; // "Small" | "Medium" | "Large"
        public bool    ShowToolbar          { get; set; } = true;
        public bool    ShowStatusBar        { get; set; } = true;

        // ── Font settings (Feature H5) ────────────────────────────────────────
        public string     LogFontFamily     { get; set; } = "Consolas";
        public float      LogFontSize       { get; set; } = 9.0f;
        public FontStyle  LogFontStyle      { get; set; } = FontStyle.Regular;

        // ── Search history (Feature B6) ───────────────────────────────────────
        public List<string> SearchHistory   { get; set; } = new List<string>();

        // ── Session restore (Feature A8) ──────────────────────────────────────
        /// <summary>When true, the last-opened file is reopened automatically on startup. Off by default.</summary>
        public bool         RestoreSessionOnStartup { get; set; } = false;
        /// <summary>Files that were open when the app was last closed.</summary>
        public List<string> LastSessionFiles        { get; set; } = new List<string>();

        // ── Auto-Reload / Tail Mode (Feature A4) ──────────────────────────────
        /// <summary>Master toggle for watching the open file for changes on disk. Mirrored by the View menu.</summary>
        public bool WatchFileChanges     { get; set; } = true;
        /// <summary>
        /// Seconds to wait after a change is detected before reloading automatically, with no prompt.
        /// 0 = manual: ask the user (Yes/No) instead of reloading automatically.
        /// </summary>
        public int  AutoReloadDelaySeconds { get; set; } = 0;

        // ── Jump to Source Code (Feature K4) ──────────────────────────────────
        /// <summary>Path to a custom editor executable. Empty = auto-detect (VS Code, then Visual Studio, then Notepad).</summary>
        public string       SourceEditorPath        { get; set; } = "";

        // ── Window state persistence (Feature 1a/1b/1c) ───────────────────────
        public int     WindowWidth          { get; set; } = 1024;
        public int     WindowHeight         { get; set; } = 768;
        public int     WindowLeft           { get; set; } = -1;  // -1 = not set
        public int     WindowTop            { get; set; } = -1;  // -1 = not set
        public string  WindowState          { get; set; } = "Normal"; // "Normal" | "Maximized"

        // ── Performance guards ─────────────────────────────────────────────────
        public long    MaxFileSizeMbForListView { get; set; } = 50; // skip list if > N MB
        public long    SlowCallThresholdMs      { get; set; } = 1000;
        // F5: user-configurable thresholds for call-tree colour coding.
        // FastCallThresholdMs  — calls below this are green  (default 100 ms)
        // SlowCallThresholdMs is reused as the upper bound    (default 1000 ms)
        // Calls between Fast..Slow are amber; calls above Slow are red.
        public int     FastCallThresholdMs      { get; set; } = 100;
        /// <summary>
        /// C2: call tree nodes only load lazily (placeholder + expand-on-demand)
        /// once the tree's total node count exceeds this threshold. Lower it for
        /// snappier expansion on slower machines, or raise it to always build the
        /// full tree eagerly for smaller/medium logs.
        /// </summary>
        public int     LazyLoadThreshold        { get; set; } = 50000;
        /// <summary>
        /// Maximum number of characters to load into RichTextBox controls (detail views).
        /// RichTextBox has a hard limit around 2GB but realistically becomes unstable above 32MB.
        /// Default: 10 million characters (~10MB of text).
        /// </summary>
        public int     MaxRichTextBoxChars      { get; set; } = 10_000_000;
        /// <summary>
        /// Maximum file size (in bytes) that will be loaded into RichTextBox controls.
        /// Files larger than this will show a placeholder message instead.
        /// Default: 50 MB.
        /// </summary>
        public long    MaxRichTextBoxFileSizeBytes { get; set; } = 50 * 1024 * 1024;
        /// <summary>
        /// When true, clicking a Call Tree node automatically filters the Performance
        /// tab to show only the API calls within that node's ENTER/EXIT scope.
        /// When false, the filter must be triggered manually via the context menu.
        /// </summary>
        public bool    FilterPerfOnTreeSelect   { get; set; } = true;

        // ── Auto-update ───────────────────────────────────────────────────────
        /// <summary>When true, checks for a new version once per <see cref="UpdateCheckIntervalDays"/> on startup.</summary>
        /// <summary>The canonical default manifest URL — used as a fallback whenever the
        /// user-configured value is blank.</summary>
        public const string DefaultUpdateManifestUrl =
            "https://raw.githubusercontent.com/Nazeer-Hussain/CAD3PLogBrowser/master/version.json";

        public bool     CheckForUpdatesOnStartup  { get; set; } = true;
        /// <summary>URL of the remote version manifest JSON.
        /// When blank the application falls back to <see cref="DefaultUpdateManifestUrl"/>.</summary>
        public string   UpdateManifestUrl         { get; set; } = DefaultUpdateManifestUrl;
        /// <summary>UTC timestamp of the last successful update check.</summary>
        public DateTime LastUpdateCheck           { get; set; } = DateTime.MinValue;
        /// <summary>Minimum number of days between automatic background checks.</summary>
        public int      UpdateCheckIntervalDays   { get; set; } = 1;
        /// <summary>
        /// Version string the user chose to skip (ENH-3).
        /// The update dialog is suppressed for this version until a newer one appears.
        /// Reset to empty to stop skipping.
        /// </summary>
        public string   SkippedVersion            { get; set; } = "";

        // ── Grok integration ──────────────────────────────────────────────────
        public string  GrokUrl          { get; set; } = "";
        public string  ClaudeApiKey     { get; set; } = "";  // L1-L6: AI features (never log raw content)
        public bool    UseClaudeApi     { get; set; } = false; // Option B: offline by default, real API when enabled
        // D-10: user-configurable model so the app does not hard-break when
        // Anthropic deprecates the baked-in version string.
        public string  ClaudeModel      { get; set; } = "claude-sonnet-4-20250514";

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

    }
}
