namespace Cad3PLogBrowser.Utilities
{
    /// <summary>
    /// Application-wide constants.
    /// Centralizes all magic numbers, strings, and configuration values.
    /// </summary>
    /// <remarks>
    /// Benefits of using constants:
    /// - Easy to change values in one place
    /// - No magic numbers scattered throughout code
    /// - Clear names make code self-documenting
    /// - Compiler catches typos (vs string literals)
    /// </remarks>
    public static class Constants
    {
        /// <summary>
        /// Performance threshold constants used for color-coding and filtering.
        /// All values are in milliseconds.
        /// </summary>
        public static class Performance
        {
            /// <summary>
            /// Calls faster than this threshold are considered "fast" and colored green.
            /// Default: 100 milliseconds (0.1 seconds).
            /// </summary>
            /// <remarks>
            /// Fast calls are typically lightweight operations like getters, simple calculations, etc.
            /// </remarks>
            public const int FastCallThresholdMs = 100;

            /// <summary>
            /// Calls slower than this threshold are considered "slow" and colored red.
            /// Default: 500 milliseconds (0.5 seconds).
            /// </summary>
            /// <remarks>
            /// Slow calls may indicate performance bottlenecks that need investigation.
            /// Calls between FastCallThresholdMs and SlowCallThresholdMs are colored amber.
            /// </remarks>
            public const int SlowCallThresholdMs = 500;

            /// <summary>
            /// Maximum file size in megabytes for building the detailed list view.
            /// Files larger than this will skip list view generation for performance.
            /// Default: 50 MB.
            /// </summary>
            /// <remarks>
            /// Large files can cause performance issues when populating UI controls.
            /// If a file exceeds this size, a warning is shown and list view is disabled.
            /// The call tree and other views will still work.
            /// </remarks>
            public const int MaxFileSizeForListViewMb = 50;

            /// <summary>
            /// Number of lines to process between cancellation checks during long operations.
            /// Lower values = more responsive cancellation, but slightly slower processing.
            /// Default: 1000 lines.
            /// </summary>
            public const int CancellationCheckInterval = 1000;

            /// <summary>
            /// Number of lines to process between progress updates during long operations.
            /// Lower values = more frequent UI updates, but slightly slower processing.
            /// Default: 5000 lines.
            /// </summary>
            public const int ProgressUpdateInterval = 5000;
        }

        /// <summary>
        /// UI display constants that control visual behavior and layout.
        /// </summary>
        public static class UI
        {
            /// <summary>
            /// Number of lines to show before the selected line to provide context.
            /// Default: 10 lines.
            /// </summary>
            /// <remarks>
            /// When clicking a tree node, the log view scrolls to show the selected line
            /// plus this many lines before it for context.
            /// </remarks>
            public const int ContextLinesCount = 10;

            /// <summary>
            /// Default splitter position as a percentage of the window width (0.0 to 1.0).
            /// Default: 0.30 (30% for tree panel, 70% for log panel).
            /// </summary>
            /// <remarks>
            /// This provides a good balance: enough space for the tree without cramping the log view.
            /// User adjustments are saved and restored on next launch.
            /// </remarks>
            public const double DefaultSplitterPercent = 0.30;

            /// <summary>
            /// Maximum number of recent files to remember in the File > Recent Files menu.
            /// Default: 10 files.
            /// </summary>
            public const int MaxRecentFiles = 10;

            /// <summary>
            /// Minimum width in pixels for the tree panel (left side of splitter).
            /// Prevents users from making the panel too narrow to use.
            /// Default: 200 pixels.
            /// </summary>
            public const int MinTreePanelWidth = 200;

            /// <summary>
            /// Minimum width in pixels for the log panel (right side of splitter).
            /// Prevents users from making the panel too narrow to use.
            /// Default: 300 pixels.
            /// </summary>
            public const int MinLogPanelWidth = 300;

            /// <summary>
            /// Fixed width in pixels for the line number column in the log list view.
            /// Default: 80 pixels (enough for 7-8 digits).
            /// </summary>
            public const int LineNumberColumnWidth = 80;

            /// <summary>
            /// Screen height threshold in pixels below which the aggregate statistics panel auto-collapses.
            /// Default: 800 pixels.
            /// </summary>
            /// <remarks>
            /// On smaller screens, the statistics panel is auto-collapsed to maximize log viewing space.
            /// Users can manually expand it if needed.
            /// </remarks>
            public const int AutoCollapseStatsHeightThreshold = 800;
        }

        /// <summary>
        /// File and directory path constants.
        /// </summary>
        public static class Files
        {
            /// <summary>
            /// Settings file name stored in AppData folder.
            /// Contains all user preferences (theme, colors, window size, etc.).
            /// </summary>
            public const string SettingsFileName = "settings.json";

            /// <summary>
            /// Recent files list file name stored in AppData folder.
            /// Contains the list of recently opened log files for the MRU menu.
            /// </summary>
            public const string RecentFilesFileName = "recentfiles.json";

            /// <summary>
            /// Session state file name stored in AppData folder.
            /// Contains session information like open files for session restore on startup.
            /// </summary>
            public const string SessionFileName = "session.json";

            /// <summary>
            /// Search history file name stored in AppData folder.
            /// Contains previously used search terms for auto-completion.
            /// </summary>
            public const string SearchHistoryFileName = "searchhistory.json";

            /// <summary>
            /// Bookmark file suffix appended to log file names.
            /// Example: "app.log" ? "app.log.bookmarks.json"
            /// </summary>
            public const string BookmarkFileSuffix = ".bookmarks.json";

            /// <summary>
            /// Annotations file suffix appended to log file names.
            /// Example: "app.log" ? "app.log.notes.json"
            /// </summary>
            public const string AnnotationsFileSuffix = ".notes.json";

            /// <summary>
            /// Environment variable name for the default log directory.
            /// If set, the File > Open dialog starts in this directory.
            /// </summary>
            /// <remarks>
            /// Common in PTC/CAD environments. Example: PTC_LOG_DIR=C:\PTC\Logs
            /// Falls back to last-used directory if this variable is not set.
            /// </remarks>
            public const string LogDirectoryEnvVar = "PTC_LOG_DIR";

            /// <summary>
            /// Alternative environment variable name for the default log directory.
            /// Checked if PTC_LOG_DIR is not set.
            /// </summary>
            public const string AlternateLogDirectoryEnvVar = "VC_LOG_DIR";

            /// <summary>
            /// Default log file extension filter for open dialog.
            /// </summary>
            public const string LogFileFilter = "log files (*.log)|*.log|log files (*.log.*)|*.log.*|All files (*.*)|*.*";

            /// <summary>
            /// Application CHM help file name.
            /// Must be placed in the same directory as the executable.
            /// </summary>
            public const string HelpFileName = "Cad3PLogBrowser.chm";

            /// <summary>
            /// Error report file name in the temp directory.
            /// Used by the "Report Errors" feature to attach crash information to email.
            /// </summary>
            public const string ErrorReportFileName = "Cad3pLogBrowser.err";
        }

        /// <summary>
        /// Log parsing constants and keywords.
        /// </summary>
        public static class Parsing
        {
            /// <summary>
            /// Keyword that marks the beginning of an API call in log files.
            /// Example: "CADSystem::OpenFile [ENTER]"
            /// </summary>
            public const string EnterKeyword = "[ENTER]";

            /// <summary>
            /// Keyword that marks the end of an API call in log files.
            /// Example: "CADSystem::OpenFile [EXIT]"
            /// </summary>
            public const string ExitKeyword = "[EXIT]";

            /// <summary>
            /// Character that indicates an error log level in log lines.
            /// Typically the second colon-separated field: "timestamp: E: message"
            /// </summary>
            public const char ErrorLevel = 'E';

            /// <summary>
            /// Character that indicates a warning log level in log lines.
            /// Typically the second colon-separated field: "timestamp: W: message"
            /// </summary>
            public const char WarningLevel = 'W';

            /// <summary>
            /// Character that indicates an info log level in log lines.
            /// Typically the second colon-separated field: "timestamp: I: message"
            /// </summary>
            public const char InfoLevel = 'I';

            /// <summary>
            /// Character that indicates a debug log level in log lines.
            /// Typically the second colon-separated field: "timestamp: D: message"
            /// </summary>
            public const char DebugLevel = 'D';

            /// <summary>
            /// Default datetime format used in log files.
            /// Example: "2025-01-15 10:23:45.123"
            /// </summary>
            public const string DefaultDateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

            /// <summary>
            /// Regex pattern for matching timestamps in log lines.
            /// Matches: YYYY-MM-DD HH:MM:SS (with optional milliseconds)
            /// </summary>
            public const string TimestampRegexPattern = @"(\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}(?:\.\d{3})?)";

            /// <summary>
            /// Regex pattern for matching duration in log lines.
            /// Matches: [XXX ms] where XXX is one or more digits
            /// Example: "[142 ms]" or "[1523 ms]"
            /// </summary>
            public const string DurationRegexPattern = @"\[(\d+)\s*ms\]";

            /// <summary>
            /// Regex pattern for matching source file and line number.
            /// Matches patterns like: "at ClassName.Method() in C:\Path\File.cs:line 123"
            /// </summary>
            public const string SourceLocationRegexPattern = @"in\s+(.+?):line\s+(\d+)";
        }

        /// <summary>
        /// Application metadata constants.
        /// </summary>
        public static class Application
        {
            /// <summary>
            /// Application display name shown in window title and about dialog.
            /// </summary>
            public const string Name = "WWGM CAD 3P Log Browser";

            /// <summary>
            /// GitHub repository URL for update checks and issue reporting.
            /// </summary>
            public const string GitHubRepoUrl = "https://github.com/Nazeer-Hussain/CAD3PLogBrowser";

            /// <summary>
            /// URL for checking latest release version.
            /// </summary>
            public const string UpdateCheckUrl = "https://github.com/Nazeer-Hussain/CAD3PLogBrowser/releases/latest";

            /// <summary>
            /// URL for reporting issues.
            /// </summary>
            public const string IssueReportUrl = "https://github.com/Nazeer-Hussain/CAD3PLogBrowser/issues/new";

            /// <summary>
            /// Default Grok search URL template.
            /// {0} will be replaced with the search term.
            /// </summary>
            public const string DefaultGrokUrlTemplate = "https://grok.example.com/search?q={0}";
        }

        /// <summary>
        /// Color constants for UI theming.
        /// These are default values that can be overridden by user settings.
        /// </summary>
        public static class Colors
        {
            /// <summary>
            /// Default highlight color name for search results.
            /// </summary>
            public const string DefaultHighlightColor = "Yellow";

            /// <summary>
            /// Default highlight color name for filtered results.
            /// </summary>
            public const string DefaultFilterHighlightColor = "PowderBlue";

            /// <summary>
            /// Background color name for error lines.
            /// </summary>
            public const string ErrorBackgroundColor = "LightCoral";

            /// <summary>
            /// Background color name for warning lines.
            /// </summary>
            public const string WarningBackgroundColor = "LightGoldenrodYellow";

            /// <summary>
            /// Foreground color name for fast calls (duration &lt; 100ms).
            /// </summary>
            public const string FastCallColor = "Green";

            /// <summary>
            /// Foreground color name for medium calls (duration 100-500ms).
            /// </summary>
            public const string MediumCallColor = "DarkOrange";

            /// <summary>
            /// Foreground color name for slow calls (duration &gt; 500ms).
            /// </summary>
            public const string SlowCallColor = "Red";
        }

        /// <summary>
        /// Keyboard shortcut key combinations.
        /// Defined as constants for consistency across the application.
        /// </summary>
        public static class Shortcuts
        {
            /// <summary>
            /// Description shown in keyboard shortcuts dialog.
            /// </summary>
            public const string OpenFile = "Ctrl+O";
            public const string SaveAs = "Ctrl+S";
            public const string Refresh = "F5";
            public const string ReloadFromDisk = "Ctrl+R";
            public const string Copy = "Ctrl+C";
            public const string Find = "Ctrl+F";
            public const string FindNext = "F3";
            public const string Filter = "Ctrl+I";
            public const string ExpandAll = "Ctrl+E";
            public const string CollapseAll = "Ctrl+W";
            public const string JumpToMatching = "Ctrl+G";
            public const string NextError = "F8";
            public const string PreviousError = "Shift+F8";
            public const string NextWarning = "Ctrl+F8";
            public const string PreviousWarning = "Ctrl+Shift+F8";
            public const string KeyboardShortcuts = "Ctrl+K";
            public const string Help = "F1";
        }
    }
}
