using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cad3PLogBrowser.Properties;
using Cad3PLogBrowser.Services;
using Cad3PLogBrowser.Services.Navigation;

namespace Cad3PLogBrowser
{
    public partial class MainForm : Form
    {
        // ── Services ──────────────────────────────────────────────────────────
        private readonly LogFileService    _logFileService;
        private readonly SearchService     _searchService;
        private readonly SettingsService   _settingsService;
        private readonly LogParserService  _parserService;
        private readonly CallGraphService  _callGraphService;
        private readonly Services.Core.MergeLogService _mergeLogService;
        private Services.Analysis.AiLogService _aiService;
        private Managers.AiAssistantPanel _aiPanel;
        private OperationOverlayPanel      _overlay;
        private UI.LineInspectorPanel      _lineInspector;
        private TabPage _aiTab;
        private readonly BookmarkService   _bookmarkService;

        // ── State ─────────────────────────────────────────────────────────────
        private string            _currentFilePath = string.Empty;
        private bool              _isLoading       = false;
        private List<string>      _allLines        = new List<string>();
        private List<ApiCallNode> _apiNodes        = new List<ApiCallNode>();
        private AppSettings       _appSettings;
        private List<Services.LogEntry>  _lastEntries      = new List<Services.LogEntry>(); // for ENTER/EXIT jump
        private bool              _isFormLoaded     = false; // Flag to prevent saving during initialization
        /// <summary>
        /// Tracks the real on-disk paths of every file that has been merged into the current session.
        /// Populated on every merge so that subsequent drop-merges can re-merge from disk instead of
        /// writing in-memory tagged lines to a temp file (which would overwrite the original tags).
        /// </summary>
        private List<string>      _mergedSourcePaths = new List<string>();

        // Feature C2: Lazy loading for large trees
        private const int LAZY_LOAD_THRESHOLD = 50000; // Enable lazy loading for 50k+ nodes
        private Dictionary<TreeNode, List<CallStackNode>> _lazyChildrenMap = new Dictionary<TreeNode, List<CallStackNode>>();
        // Cached font for lazy-load placeholder nodes — avoids a GDI Font
        // allocation on every tree node during expand (Issue 1).
        private Font _lazyPlaceholderFont;

        // D-18: cached context menus — built once, reused on every right-click to avoid
        // allocating ~15 GDI objects per click (previous code created a new ContextMenuStrip
        // and all its ToolStripMenuItems on every mouse-up event).
        private ContextMenuStrip     _callTreeContextMenu;
        private ToolStripSeparator   _callTreeGrokSep;
        private ToolStripMenuItem    _callTreeGrokItem;

        // ── Cancellation support for long-running operations ──────────────────
        private CancellationTokenSource _cancellationTokenSource;
        private string _currentOperation = string.Empty;

        // Feature B10: Error/Warning navigation
        private List<int>         _errorLines       = new List<int>();
        private List<int>         _warningLines     = new List<int>();
        private int               _currentErrorIndex   = -1;
        private int               _currentWarningIndex = -1;

        // ── Tab identifiers (used by SettingsForm) ────────────────────────────
        public enum TabId { Log, Raw, Performance, LogDetails, CallGraph, FlameGraph, Timeline }

        /// <summary>Returns whether the given tab is currently visible.</summary>
        public bool IsTabVisible(TabId id) => mainTabControl.TabPages.Contains(GetTab(id));

        /// <summary>Shows or hides the given tab and keeps View menu in sync.</summary>
        public void SetTabVisible(TabId id, bool visible)
        {
            var tab = GetTab(id);
            var menuItem = GetTabMenuItem(id);
            if (menuItem != null) menuItem.Checked = visible;
            // The CheckedChanged handler on the menu item calls SetTabVisible(TabPage, bool)
            // so we only need to toggle if there's no menu item
            else SetTabVisible(tab, visible);
        }

        private TabPage GetTab(TabId id)
        {
            switch (id)
            {
                case TabId.Log:         return logTab;
                case TabId.Raw:         return rawTab;
                case TabId.Performance: return performanceTab;
                case TabId.LogDetails:  return logDetailTab;
                case TabId.CallGraph:   return callGraphTab;
                case TabId.FlameGraph:  return flameGraphTab;
                case TabId.Timeline:    return timelineTab;
                default:                return logTab;
            }
        }

        private ToolStripMenuItem GetTabMenuItem(TabId id)
        {
            switch (id)
            {
                case TabId.Log:         return showLogTabMenuItem;
                case TabId.Raw:         return showRawTabMenuItem;
                case TabId.Performance: return showPerformanceTabMenuItem;
                case TabId.LogDetails:  return showLogDetailsTabMenuItem;
                case TabId.CallGraph:   return showCallGraphMenuItem;
                case TabId.FlameGraph:  return showFlameGraphTabMenuItem;
                case TabId.Timeline:    return showTimelineTabMenuItem;
                default:                return null;
            }
        }

        /// <summary>Returns the canonical tab order so re-shown tabs land at their original position.</summary>
        private TabPage[] GetCanonicalTabOrder() => new[]
        {
            logTab, rawTab, performanceTab, logDetailTab,
            callGraphTab, flameGraphTab, timelineTab, _aiTab
        };

        private void SetTabVisible(TabPage tab, bool visible)
        {
            if (tab == null) return;

            bool currentlyVisible = mainTabControl.TabPages.Contains(tab);
            if (visible == currentlyVisible) return;

            if (visible)
            {
                // Bug #14: insert at the canonical position so tab order is stable.
                int insertAt = 0;
                foreach (TabPage canonical in GetCanonicalTabOrder())
                {
                    if (ReferenceEquals(canonical, tab)) break;
                    if (mainTabControl.TabPages.Contains(canonical)) insertAt++;
                }

                if (insertAt >= mainTabControl.TabPages.Count)
                    mainTabControl.TabPages.Add(tab);
                else
                    mainTabControl.TabPages.Insert(insertAt, tab);
                return;
            }

            if (mainTabControl.TabPages.Count <= 1)
            {
                if (ReferenceEquals(tab, logTab))         showLogTabMenuItem.Checked         = true;
                if (ReferenceEquals(tab, rawTab))         showRawTabMenuItem.Checked         = true;
                if (ReferenceEquals(tab, performanceTab)) showPerformanceTabMenuItem.Checked = true;
                if (ReferenceEquals(tab, logDetailTab))   showLogDetailsTabMenuItem.Checked  = true;
                if (ReferenceEquals(tab, callGraphTab))   showCallGraphMenuItem.Checked      = true;
                if (ReferenceEquals(tab, flameGraphTab))  showFlameGraphTabMenuItem.Checked  = true;
                if (ReferenceEquals(tab, timelineTab))    showTimelineTabMenuItem.Checked    = true;
                return;
            }

            mainTabControl.TabPages.Remove(tab);
        }

        private void showLogTabMenuItem_CheckedChanged(object sender, EventArgs e) =>
            SetTabVisible(logTab,         showLogTabMenuItem.Checked);

        private void showRawTabMenuItem_CheckedChanged(object sender, EventArgs e) =>
            SetTabVisible(rawTab,         showRawTabMenuItem.Checked);

        private void showPerformanceTabMenuItem_CheckedChanged(object sender, EventArgs e) =>
            SetTabVisible(performanceTab, showPerformanceTabMenuItem.Checked);

        private void showLogDetailsTabMenuItem_CheckedChanged(object sender, EventArgs e) =>
            SetTabVisible(logDetailTab,   showLogDetailsTabMenuItem.Checked);

        private void showCallGraphMenuItem_CheckedChanged(object sender, EventArgs e) =>
            SetTabVisible(callGraphTab,   showCallGraphMenuItem.Checked);

        private void showCallTreeMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (showCallTreeMenuItem.Checked)
            {
                // Show Call Tree, hide API Tree
                CallTreeButton.Checked = true;
                ApiTreeButton.Checked = false;
                showApiTreeMenuItem.Checked = false;
            }
        }

        private void showApiTreeMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (showApiTreeMenuItem.Checked)
            {
                // Show API Tree, hide Call Tree
                ApiTreeButton.Checked = true;
                CallTreeButton.Checked = false;
                showCallTreeMenuItem.Checked = false;
            }
        }

        private ToolStripMenuItem _recentFilesMenuItem;
        private ToolStripSeparator _recentFilesSeparator;
        private ToolStripButton _themeToggleButton;

        private void AddThemeToggleButton()
        {
            // Create theme toggle button with proper icons
            _themeToggleButton = new ToolStripButton
            {
                ToolTipText = Resources.TOOLTIP_THEME_TOGGLE_DEFAULT,
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                AutoSize = true
            };
            _themeToggleButton.Click += ThemeToggleButton_Click;

            // Add to toolbar before settings button
            int settingsIndex = mainToolStrip.Items.IndexOf(SettingsButton);
            if (settingsIndex >= 0)
            {
                mainToolStrip.Items.Insert(settingsIndex, _themeToggleButton);
                mainToolStrip.Items.Insert(settingsIndex + 1, new ToolStripSeparator());
            }
            else
            {
                mainToolStrip.Items.Add(new ToolStripSeparator());
                mainToolStrip.Items.Add(_themeToggleButton);
            }

            UpdateThemeButtonIcon();
        }

        private void ThemeToggleButton_Click(object sender, EventArgs e)
        {
            _appSettings.Theme = _appSettings.Theme == "Dark" ? "Light" : "Dark";
            ApplyThemeWithOverlay();
            UpdateThemeButtonIcon();
        }

        private Bitmap _sunIcon;
        private Bitmap _moonIcon;

        private void UpdateThemeButtonIcon()
        {
            if (_themeToggleButton == null) return;

            bool isDark = _appSettings.Theme == "Dark";

            // Generate once and reuse
            if (_sunIcon == null || _moonIcon == null)
                IconGenerator.GenerateThemeIcons(IconGenerator.IconSize.Medium, out _sunIcon, out _moonIcon);

            _themeToggleButton.Image = isDark ? _sunIcon : _moonIcon;
            _themeToggleButton.ToolTipText = isDark
                ? Resources.TOOLTIP_THEME_TOGGLE_TO_LIGHT
                : Resources.TOOLTIP_THEME_TOGGLE_TO_DARK;
        }

        private void BuildMruMenu()
        {
            if (_recentFilesMenuItem != null)
            {
                // Snapshot into an array before disposing: ToolStripItem.Dispose() removes
                // the item from its parent collection as a side-effect, which would mutate
                // DropDownItems mid-foreach and throw InvalidOperationException.
                var children = new ToolStripItem[_recentFilesMenuItem.DropDownItems.Count];
                _recentFilesMenuItem.DropDownItems.CopyTo(children, 0);
                foreach (var child in children)
                    child.Dispose();

                fileMenuItem.DropDownItems.Remove(_recentFilesMenuItem);
                _recentFilesMenuItem.Dispose();
                _recentFilesMenuItem = null;
            }
            if (_recentFilesSeparator != null)
            {
                fileMenuItem.DropDownItems.Remove(_recentFilesSeparator);
                _recentFilesSeparator.Dispose();
                _recentFilesSeparator = null;
            }

            if (_appSettings.RecentFiles.Count == 0) return;

            // Create separator and Recent Files submenu
            _recentFilesSeparator = new ToolStripSeparator();
            _recentFilesMenuItem = new ToolStripMenuItem(Resources.MENU_RECENT_FILES);

            // Add each recent file
            for (int i = 0; i < _appSettings.RecentFiles.Count && i < 10; i++)
            {
                string filePath = _appSettings.RecentFiles[i];
                string fileName = Path.GetFileName(filePath);
                string menuText = string.Format("{0}. {1}", i + 1, fileName);

                var menuItem = new ToolStripMenuItem(menuText)
                {
                    Tag = filePath,
                    ToolTipText = filePath
                };
                menuItem.Click += RecentFileMenuItem_Click;
                _recentFilesMenuItem.DropDownItems.Add(menuItem);
            }

            // Add Clear Recent Files option
            if (_recentFilesMenuItem.DropDownItems.Count > 0)
            {
                _recentFilesMenuItem.DropDownItems.Add(new ToolStripSeparator());
                var clearItem = new ToolStripMenuItem(Resources.MENU_CLEAR_RECENT_FILES);
                clearItem.Click += (s, e) =>
                {
                    _appSettings.RecentFiles.Clear();
                    _appSettings.Save();
                    BuildMruMenu();
                };
                _recentFilesMenuItem.DropDownItems.Add(clearItem);
            }

            // Insert before the Exit menu item
            int exitIndex = fileMenuItem.DropDownItems.IndexOf(fileSeparatorBeforeExit);
            if (exitIndex >= 0)
            {
                fileMenuItem.DropDownItems.Insert(exitIndex, _recentFilesSeparator);
                fileMenuItem.DropDownItems.Insert(exitIndex + 1, _recentFilesMenuItem);
            }
        }

        private void RecentFileMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem && menuItem.Tag is string filePath)
            {
                if (File.Exists(filePath))
                {
                    LoadFileAsync(filePath);
                }
                else
                {
                    MessageBox.Show(
                        string.Format(Resources.MSG_FILE_NOT_FOUND_REMOVED, filePath),
                        Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _appSettings.RecentFiles.Remove(filePath);
                    _appSettings.Save();
                    BuildMruMenu();
                }
            }
        }

        // Measured pixel width of the widest log line — used to hold the horizontal scrollbar
        // across resizes without re-measuring on every resize event.
        private int _logTextColumnContentWidth = 0;
        private struct VirtualLogLine
        {
            public int    LineNumber;
            public string Text;
            public Color  BackColour;
        }
        private List<VirtualLogLine> _virtualLines = new List<VirtualLogLine>();
        // O(1) lookup: 1-based line number → virtual list index
        private Dictionary<int, int> _lineIndexMap = new Dictionary<int, int>();
        private bool _performanceViewNeedsRefresh = false;

        // P-06: thin IList<string> view over _virtualLines so FindNext never
        // has to allocate and copy a full List<string> on every F3 press.
        private VirtualLineTextList _virtualLineTexts;

        /// <summary>
        /// Zero-allocation read-only IList&lt;string&gt; view over the Text fields of
        /// a List&lt;VirtualLogLine&gt;.  Avoids the O(N) copy in FindNext (P-06).
        /// </summary>
        private sealed class VirtualLineTextList : System.Collections.Generic.IList<string>
        {
            private readonly List<VirtualLogLine> _src;
            internal VirtualLineTextList(List<VirtualLogLine> src) { _src = src; }
            public string this[int i]
            {
                get => _src[i].Text;
                set => throw new NotSupportedException();
            }
            public int  Count      => _src.Count;
            public bool IsReadOnly => true;
            public bool Contains(string item) { foreach (var v in _src) if (v.Text == item) return true; return false; }
            public int  IndexOf(string item)  { for (int i = 0; i < _src.Count; i++) if (_src[i].Text == item) return i; return -1; }
            public void CopyTo(string[] array, int idx) { for (int i = 0; i < _src.Count; i++) array[idx + i] = _src[i].Text; }
            public System.Collections.Generic.IEnumerator<string> GetEnumerator()
            { foreach (var v in _src) yield return v.Text; }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
            public void Add(string item)            => throw new NotSupportedException();
            public void Clear()                     => throw new NotSupportedException();
            public void Insert(int i, string item)  => throw new NotSupportedException();
            public bool Remove(string item)         => throw new NotSupportedException();
            public void RemoveAt(int i)             => throw new NotSupportedException();
        }

        // ── Construction ──────────────────────────────────────────────────────
        public MainForm()
        {
            InitializeComponent();

            _appSettings = AppSettings.Load();

            // Centred operation overlay — must be added before other setup so BringToFront works
            _overlay = new OperationOverlayPanel();
            Controls.Add(_overlay);
            _overlay.BringToFront();

            // Replace logDetailBox with the structured Line Inspector
            _lineInspector = new UI.LineInspectorPanel();
            logDetailTab.Controls.Clear();
            logDetailTab.Controls.Add(_lineInspector);
            _settingsService  = new SettingsService(_appSettings);
            _searchService    = new SearchService();
            _parserService    = new LogParserService();
            _callGraphService = new CallGraphService();
            _mergeLogService  = new Services.Core.MergeLogService();
            _aiService        = new Services.Analysis.AiLogService(
                _appSettings.ClaudeApiKey, _appSettings.UseClaudeApi, _appSettings.ClaudeModel);
            _logFileService   = new LogFileService(this);
            _bookmarkService  = new Services.Navigation.BookmarkService();
            _logFileService.FileChangedOnDisk += OnFileChangedOnDisk;

            RestoreSettings();
            InitTreeViews();
            InitAiPanel();
            BuildMruMenu();
            AddThemeToggleButton();
            ApplyTheme();

            // Ensure search box is on top (above trees)
            treeSearchTextBox.BringToFront();

            // Load saved font preferences
            LoadLogFont();

            // Initialize tree search placeholder
            InitializeTreeSearchBox();

            // Apply toolbar visibility from settings
            showToolbarMenuItem.Checked = _appSettings.ShowToolbar;
            mainToolStrip.Visible = _appSettings.ShowToolbar;

            // Apply status bar visibility from settings
            showStatusBarMenuItem.Checked = _appSettings.ShowStatusBar;
            mainStatusStrip.Visible = _appSettings.ShowStatusBar;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Force layout after form is fully loaded and sized
            LayoutTrees();

            // Set up F1 help key handling for context-sensitive help
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown_Help;

            // Wire context-menu export events on the visualisation panels (once)
            if (timelinePanel != null)
                timelinePanel.ExportImageRequested += (s, ev) => exportTimelineMenuItem_Click(s, ev);
            if (flameGraphPanel != null)
                flameGraphPanel.ExportImageRequested += (s, ev) => exportFlameGraphMenuItem_Click(s, ev);

            // PERFORMANCE: lazily re-render the performance tab when it becomes visible,
            // so ApplyTheme() can skip it when it is not in the foreground.
            mainTabControl.SelectedIndexChanged += (s, ev) =>
            {
                if (mainTabControl.SelectedTab == performanceTab
                    && _apiPerfStats != null && _apiPerfStats.Count > 0
                    && _performanceViewNeedsRefresh)
                {
                    _performanceViewNeedsRefresh = false;
                    RenderPerformanceRows(_apiPerfStats, _lastTotalLines);
                }
            };
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Force layout again when form is shown to ensure correct positioning
            LayoutTrees();

            // Restore the start-up tab chosen by the user in Settings
            ApplyInitialView();
        }

        /// <summary>
        /// Handle F1 key for context-sensitive help.
        /// </summary>
        private void MainForm_KeyDown_Help(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                e.Handled = true;
                ShowContextSensitiveHelp();
            }
        }

        /// <summary>
        /// Shows context-sensitive help based on currently active control or tab.
        /// </summary>
        private void ShowContextSensitiveHelp()
        {
            string section = null;

            // Determine context based on active tab
            if (mainTabControl.SelectedTab == logTab)
                section = "log-view";
            else if (mainTabControl.SelectedTab == performanceTab)
                section = "performance";
            else if (mainTabControl.SelectedTab == callGraphTab)
                section = "call-graph";
            else if (mainTabControl.SelectedTab == flameGraphTab)
                section = "flame-graph";
            else if (mainTabControl.SelectedTab == timelineTab)
                section = "timeline";
            else if (mainTabControl.SelectedTab == logDetailTab)
                section = "log-details";

            // Check if tree panel has focus
            if (CallTree.Focused || ApiTree.Focused)
                section = "tree-view";

            ShowUserGuide(section);
        }

        // ── Public API ────────────────────────────────────────────────────────
        public string ActiveFilePath { get { return _currentFilePath; } }
        public AppSettings AppSettings { get { return _appSettings; } }

        public void OpenFilePath(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                LoadFileAsync(filePath);
        }

        private void ApplyThemeWithOverlay()
        {
            // Theme switching walks every control and redraws trees — show the
            // overlay so the user sees something is happening during the wait.
            string opName = _appSettings.Theme == "Dark" ? "Switching to Dark Theme…" : "Switching to Light Theme…";
            StartOperation(opName);   // shows overlay + DoEvents so it paints
            try
            {
                ApplyTheme();
            }
            finally
            {
                EndOperation();
            }
        }

        public void ApplyTheme()
        {
            // PERFORMANCE: Suppress redraws on the form-level handle while we restyle.
            // NativeMethods.SuppressRedraw is also called inside ThemeManager.ApplyTheme,
            // but doing it here too blocks any paint triggered by SuspendLayout itself.
            this.SuspendLayout();

            try
            {
                // Set the theme based on settings
                var theme = _appSettings.Theme == "Dark" ? ThemeManager.Theme.Dark : ThemeManager.Theme.Light;
                ThemeManager.SetTheme(theme);

                // Apply to main form (WM_SETREDRAW suppressed inside ApplyTheme)
                ThemeManager.ApplyTheme(this);

                // Manually update visualization panels (they handle their own child controls)
                flameGraphPanel?.UpdateTheme();
                timelinePanel?.UpdateTheme();
                _aiPanel?.UpdateTheme();
                _overlay?.UpdateTheme();
                _lineInspector?.ApplyTheme();

                // Issue 4 Fix: apply dark/light theme colors to the tree views to prevent text truncation
                bool isDarkTheme = _appSettings.Theme == "Dark";
                Color treeBack = isDarkTheme ? Color.FromArgb(28, 30, 38)    : SystemColors.Window;
                Color treeFore = isDarkTheme ? Color.FromArgb(210, 215, 230) : SystemColors.WindowText;
                Color treeLine = isDarkTheme ? Color.FromArgb(60, 65, 80)    : SystemColors.GrayText;
                foreach (var tv in new[] { ApiTree, CallTree })
                {
                    tv.BackColor = treeBack;
                    tv.ForeColor = treeFore;
                    tv.LineColor = treeLine;
                    tv.Invalidate();
                }

                // Apply icon size
                ApplyIconSize();

                // Refresh tree layout after theme/icon change
                LayoutTrees();

                // P-08: update pre-stored BackColour values for the new theme, then
                // invalidate.  This replaces the old on-demand text.Contains() check in
                // RetrieveVirtualItem and also makes bookmark/highlight colours visible.
                if (logListView.VirtualMode && _virtualLines.Count > 0)
                    RefreshVirtualLineColours();

                // Apply theme to raw text view
                if (rawTextBox != null)
                {
                    bool isDark = ThemeManager.CurrentTheme == ThemeManager.Theme.Dark;
                    rawTextBox.BackColor = ThemeManager.ControlBackgroundColor;
                    rawTextBox.ForeColor = ThemeManager.ForegroundColor;
                }

                // Refresh the call graph panel
                if (callGraphPanel != null)
                {
                    callGraphPanel.Invalidate();
                }

                // Refresh the performance view with theme-aware colors
                // PERFORMANCE: only re-render if the perf tab is actually visible to avoid
                // rebuilding the full ListView on every theme toggle when user is elsewhere.
                if (_apiPerfStats != null && _apiPerfStats.Count > 0)
                {
                    if (performanceView.Visible)
                    {
                        _performanceViewNeedsRefresh = false;
                        RenderPerformanceRows(_apiPerfStats, _lastTotalLines);
                    }
                    else
                    {
                        // Mark dirty so the tab's SelectedIndexChanged handler re-renders lazily.
                        _performanceViewNeedsRefresh = true;
                    }
                }
            }
            finally
            {
                // Always resume layout even if error occurs
                this.ResumeLayout(true);
            }
        }

        private void UpdateLogColors()
        {
            // Update the virtual lines with theme-appropriate colors
            for (int i = 0; i < _virtualLines.Count; i++)
            {
                var line = _virtualLines[i];
                string text = line.Text;

                if (text.Contains("ERROR") || text.Contains("EXCEPTION"))
                    line.BackColour = ThemeManager.ErrorBackgroundColor;
                else if (text.Contains("WARNING") || text.Contains("WARN"))
                    line.BackColour = ThemeManager.WarningBackgroundColor;
                else
                    line.BackColour = ThemeManager.BackgroundColor;

                _virtualLines[i] = line;
            }
        }

        public void ApplyToolbarVisibility()
        {
            mainToolStrip.Visible = _appSettings.ShowToolbar;
            showToolbarMenuItem.Checked = _appSettings.ShowToolbar;
            mainStatusStrip.Visible = _appSettings.ShowStatusBar;
            showStatusBarMenuItem.Checked = _appSettings.ShowStatusBar;
        }

        public void ApplyFontSettings()
        {
            LoadLogFont();
            // Refresh the log view to apply new font
            if (logListView.VirtualMode && _virtualLines.Count > 0)
            {
                logListView.Invalidate();
            }
        }

        // ── Settings ──────────────────────────────────────────────────────────
        private void RestoreSettings()
        {
            // Feature 1a/1b/1c: Restore window state
            if (_appSettings.WindowLeft >= 0 && _appSettings.WindowTop >= 0)
            {
                // Restore saved position
                this.StartPosition = FormStartPosition.Manual;
                this.Left = _appSettings.WindowLeft;
                this.Top = _appSettings.WindowTop;
                this.Width = _appSettings.WindowWidth;
                this.Height = _appSettings.WindowHeight;

                // Validate position is on-screen
                if (!IsPositionOnScreen(this.Left, this.Top))
                {
                    this.StartPosition = FormStartPosition.CenterScreen;
                }

                if (_appSettings.WindowState == "Maximized")
                {
                    this.WindowState = FormWindowState.Maximized;
                }
            }
            else
            {
                // Feature 1c: No saved settings → maximize
                this.WindowState = FormWindowState.Maximized;
            }

            // Feature 2a/2b: Default splitter to 30% if not set
            int dist = _settingsService.LoadSplitterDistance();

            if (dist > 0)
            {
                mainSplitContainer.SplitterDistance = dist;
            }
            else if (_appSettings.SplitterDistance > 0)
            {
                mainSplitContainer.SplitterDistance = _appSettings.SplitterDistance;
            }
            // else: will be set to 30% in MainForm_Load after layout is ready

            // Feature A3: Default to PTC_LOG_DIR environment variable if set
            string ptcLogDir = Environment.GetEnvironmentVariable("PTC_LOG_DIR");
            if (!string.IsNullOrEmpty(ptcLogDir) && Directory.Exists(ptcLogDir))
            {
                openLogFileDialog.InitialDirectory = ptcLogDir;
            }
            else
            {
                string lastDir = _settingsService.LoadLastDirectory();
                if (!string.IsNullOrEmpty(lastDir) && Directory.Exists(lastDir))
                    openLogFileDialog.InitialDirectory = lastDir;
            }
        }

        private bool IsPositionOnScreen(int left, int top)
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.Contains(left, top))
                    return true;
            }
            return false;
        }

        /// <summary>Selects the start-up tab and tree view that were saved in Settings.</summary>
        private void ApplyInitialView()
        {
            // Apply default tree view (Call Tree vs API Tree)
            if (_appSettings.DefaultTreeView == "Api")
                ShowApiTree();
            else
                ShowCallTree();

            // Map the human-readable setting string to a tab page
            TabPage target = null;
            switch (_appSettings.InitialView ?? "Log")
            {
                case "Log":         target = logTab;          break;
                case "Raw":         target = rawTab;          break;
                case "Performance": target = performanceTab;  break;
                case "Log Details": target = logDetailTab;    break;
                case "Call Graph":  target = callGraphTab;    break;
                case "Flame Graph": target = flameGraphTab;   break;
                case "Timeline":    target = timelineTab;     break;
                case "AI Assistant": target = _aiTab;          break;
                // Legacy values saved by older builds
                case "LogView":     target = logTab;          break;
                case "ApiView":     target = logTab;          break;
            }
            if (target != null && mainTabControl.TabPages.Contains(target))
                mainTabControl.SelectedTab = target;
        }

        private void SaveSettings()
        {
            try
            {
                // Update all settings in AppSettings object first (no I/O)
                _appSettings.SplitterDistance = mainSplitContainer.SplitterDistance;

                if (!string.IsNullOrEmpty(_currentFilePath))
                    _appSettings.InitialDirectory = GetSafeDirectory(_currentFilePath);

                // Feature 1a/1b: Save window state
                if (this.WindowState == FormWindowState.Normal)
                {
                    _appSettings.WindowLeft = this.Left;
                    _appSettings.WindowTop = this.Top;
                    _appSettings.WindowWidth = this.Width;
                    _appSettings.WindowHeight = this.Height;
                    _appSettings.WindowState = "Normal";
                }
                else if (this.WindowState == FormWindowState.Maximized)
                {
                    _appSettings.WindowState = "Maximized";
                    // Keep last normal position/size for when user un-maximizes
                }

                // Save everything in one operation (single I/O)
                _appSettings.Save();
            }
            catch
            {
                // Non-fatal: settings save failure should not prevent exit
            }
        }

        // ── Status bar ────────────────────────────────────────────────────────
        private string _activeFilterText = "";

        private void UpdateStatusBar()
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                StatusFileName.Text = StatusLineCount.Text = StatusSelection.Text = "";
                return;
            }

            // Build the file portion of the status text.
            // For merged sessions _currentFilePath looks like "[Merged: a.txt, b.txt]".
            // Show every contributing filename so the user always knows what is loaded.
            string filePart;
            if (_currentFilePath.StartsWith("[Merged:"))
            {
                // Extract the comma-separated names from "[Merged: a.txt, b.txt]"
                string inner = _currentFilePath
                    .TrimStart('[')
                    .TrimEnd(']')
                    .Substring("Merged:".Length)
                    .Trim();
                // inner is already "a.txt, b.txt, ..." — just label it clearly
                filePart = "Merged: " + inner;
            }
            else
            {
                // Single file — show the full path so the user can see the directory too
                filePart = _currentFilePath;
            }

            string fileInfo = string.Format("{0}  |  {1:N0} lines", filePart, _allLines.Count);

            if (_errorLines.Count > 0 || _warningLines.Count > 0)
            {
                fileInfo += string.Format("  |  {0} errors, {1} warnings",
                    _errorLines.Count, _warningLines.Count);
            }

            StatusFileName.Text = fileInfo;

            int total   = _allLines.Count;
            int visible = _virtualLines.Count;

            // Show filter status
            if (total != visible && !string.IsNullOrEmpty(_activeFilterText))
            {
                StatusLineCount.Text = string.Format(Resources.STATUS_FILTER_ACTIVE,
                    _activeFilterText, visible, total);
            }
            else if (total != visible)
            {
                StatusLineCount.Text = string.Format(Resources.STATUS_SHOWING_LINES, visible, total);
            }
            else
            {
                StatusLineCount.Text = "";
            }
        }

        private void UpdateSelectionStatus()
        {
            if (logListView.SelectedIndices.Count == 0) { StatusSelection.Text = ""; return; }
            int idx = logListView.SelectedIndices[0];

            // Feature G5: Show selected line info with more detail
            string lineNum = _virtualLines[idx].LineNumber.ToString();
            string preview = _virtualLines[idx].Text;
            if (preview.Length > 60) preview = preview.Substring(0, 57) + "...";

            StatusSelection.Text = string.Format(Resources.STATUS_SELECTION_INFO, lineNum, preview);
        }

        // ── Tree view init ────────────────────────────────────────────────────
        private void InitTreeViews()
        {
            // Feature C3: Build icon list and assign to both trees
            BuildTreeIconList();
            ApiTree.ImageList = treeIconList;
            CallTree.ImageList = treeIconList;

            ApiTree.ShowLines = ApiTree.ShowPlusMinus = true;
            ApiTree.HideSelection = false;
            ApiTree.ShowNodeToolTips = true;
            CallTree.ShowLines = CallTree.ShowPlusMinus = true;
            CallTree.ShowNodeToolTips = true;
            CallTree.HideSelection = false;

            // FullRowSelect ensures the selection highlight spans the full row width
            // so there is no narrow-highlight artefact in dark theme.
            // DrawMode stays Normal: Windows owns all per-node rendering, which is
            // fast, clip-correct, and honours individual TreeNode.ForeColor /
            // NodeFont settings without any managed per-node callback overhead.
            ApiTree.FullRowSelect  = true;
            CallTree.FullRowSelect = true;

            CallTreeButton.CheckedChanged += (s, e) => SyncTreeVisibility();
            ApiTreeButton.CheckedChanged  += (s, e) => SyncTreeVisibility();

            // Add context menu handlers for both trees
            ApiTree.MouseUp  += ApiTree_MouseUpForSorting;
            CallTree.MouseUp += CallTree_MouseUp;

            SyncTreeVisibility();
        }

        private void ApiTree_MouseUpForSorting(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Select node under cursor
                var node = ApiTree.GetNodeAt(e.Location);
                if (node != null) ApiTree.SelectedNode = node;

                // API Tree specific context menu
                var contextMenu = new ContextMenuStrip();

                // ═══ SORTING OPTIONS (API Tree only - PRIMARY FEATURE) ═══
                var sortByName = new ToolStripMenuItem(Resources.MENU_SORT_BY_NAME);
                sortByName.Click += (s, ev) => ChangeApiSorting(ApiSortMode.ByName);
                sortByName.Checked = (_apiSortMode == ApiSortMode.ByName);
                contextMenu.Items.Add(sortByName);

                var sortByCount = new ToolStripMenuItem(Resources.MENU_SORT_BY_COUNT);
                sortByCount.Click += (s, ev) => ChangeApiSorting(ApiSortMode.ByCount);
                sortByCount.Checked = (_apiSortMode == ApiSortMode.ByCount);
                contextMenu.Items.Add(sortByCount);

                var sortByLine = new ToolStripMenuItem(Resources.MENU_SORT_BY_LINE);
                sortByLine.Click += (s, ev) => ChangeApiSorting(ApiSortMode.ByFirstLine);
                sortByLine.Checked = (_apiSortMode == ApiSortMode.ByFirstLine);
                contextMenu.Items.Add(sortByLine);

                contextMenu.Items.Add(new ToolStripSeparator());

                // ═══ COPY ACTIONS ═══
                contextMenu.Items.Add(Resources.MENU_COPY_API_NAME, null, (s, ev) => 
                {
                    var n = ApiTree.SelectedNode;
                    if (n != null) Clipboard.SetText(GetMethodNameFromNode(n));
                });
                contextMenu.Items.Add(Resources.MENU_INSPECT_LINE, null, (s, ev) => InspectSelectedLine());
                ((ToolStripMenuItem)contextMenu.Items[contextMenu.Items.Count - 1]).ShortcutKeyDisplayString = "F12";

                contextMenu.Items.Add(new ToolStripSeparator());

                // ═══ COMMON TREE ACTIONS ═══
                contextMenu.Items.Add(Resources.MENU_EXPAND_ALL_SHORTCUT, null, (s, ev) => ApiTree.ExpandAll());
                contextMenu.Items.Add(Resources.MENU_COLLAPSE_ALL_SHORTCUT, null, (s, ev) => 
                {
                    ApiTree.CollapseAll();
                    if (ApiTree.Nodes.Count > 0) ApiTree.Nodes[0].Expand();
                });

                contextMenu.Items.Add(new ToolStripSeparator());

                // ═══ CROSS-REFERENCE ═══
                contextMenu.Items.Add(Resources.MENU_SHOW_IN_CALL_TREE, null, (s, ev) => 
                {
                    var n = ApiTree.SelectedNode;
                    if (n != null)
                    {
                        ShowCallTree();
                        FindAndSelectCallTreeNode(GetMethodNameFromNode(n));
                    }
                });

                // ═══ SEARCH IN GROK (if configured) ═══
                if (!string.IsNullOrEmpty(_appSettings?.GrokUrl))
                {
                    contextMenu.Items.Add(new ToolStripSeparator());
                    contextMenu.Items.Add(Resources.MENU_SEARCH_IN_GROK, null, treeContextSearchInGrokMenuItem_Click);
                }

                // Apply dark theme if needed
                if (ThemeManager.CurrentTheme == ThemeManager.Theme.Dark)
                {
                    contextMenu.Renderer = new ToolStripProfessionalRenderer(new DarkContextMenuColorTable());
                    contextMenu.BackColor = Color.FromArgb(45, 45, 48);
                    contextMenu.ForeColor = Color.FromArgb(241, 241, 241);
                }

                contextMenu.Show(ApiTree, e.Location);
            }
        }

        private void ChangeApiSorting(ApiSortMode newMode)
        {
            _apiSortMode = newMode;

            // Refresh API Tree
            if (_apiNodes != null && _apiNodes.Count > 0)
            {
                PopulateApiTree(_apiNodes);
            }
        }

        private void BuildTreeIconList()
        {
            var imgList = treeIconList;
            imgList.Images.Clear();

            // Feature C3: Flat-style icons for checkmark and cross
            // Icon 0: green checkmark (flat style)
            var checkBmp = new System.Drawing.Bitmap(16, 16);
            using (var g = System.Drawing.Graphics.FromImage(checkBmp))
            {
                g.Clear(System.Drawing.Color.Transparent);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(34, 139, 34), 2.5f)
                    { StartCap = System.Drawing.Drawing2D.LineCap.Round,
                      EndCap   = System.Drawing.Drawing2D.LineCap.Round })
                {
                    g.DrawLines(pen, new[] {
                        new System.Drawing.PointF(2, 8),
                        new System.Drawing.PointF(6, 12),
                        new System.Drawing.PointF(14, 3)
                    });
                }
            }
            imgList.Images.Add(checkBmp);

            // Icon 1: red cross (flat style)
            var crossBmp = new System.Drawing.Bitmap(16, 16);
            using (var g = System.Drawing.Graphics.FromImage(crossBmp))
            {
                g.Clear(System.Drawing.Color.Transparent);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(220, 20, 60), 2.5f)
                    { StartCap = System.Drawing.Drawing2D.LineCap.Round,
                      EndCap   = System.Drawing.Drawing2D.LineCap.Round })
                {
                    g.DrawLine(pen, 3, 3, 13, 13);
                    g.DrawLine(pen, 13, 3, 3, 13);
                }
            }
            imgList.Images.Add(crossBmp);
        }

        // ── Tree population ───────────────────────────────────────────────────
        private void PopulateTrees(List<string> lines)
        {
            var entries   = _parserService.Parse(lines);
            var apiNodes  = _parserService.BuildApiList(entries);
            var callTree  = _parserService.BuildCallTree(entries);
            var perfStats = _parserService.BuildPerformanceStats(callTree);
            var graph     = _callGraphService.Build(entries);
            PopulateTreesFromData(entries, apiNodes, callTree, perfStats, graph);
        }

        private void PopulateTreesFromData(
            List<Services.LogEntry>     entries,
            List<ApiCallNode>           apiNodes,
            List<CallStackNode>         callTree,
            List<ApiPerfStats>          perfStats,
            Services.CallGraph          graph)
        {
            PopulateTreesFromData(entries, apiNodes, callTree, perfStats, graph, null);
        }

        private void PopulateTreesFromData(
            List<Services.LogEntry>                        entries,
            List<ApiCallNode>                              apiNodes,
            List<CallStackNode>                            callTree,
            List<ApiPerfStats>                             perfStats,
            Services.CallGraph                             graph,
            List<(string FileName, Services.CallGraph Graph)> fileGraphs)
        {
            _lastEntries  = entries;
            _apiNodes     = apiNodes;

            // Pre-compute matched status for all APIs in one O(N) pass
            _matchedApiCache = BuildMatchedApiCache(entries);

            PopulateApiTree(_apiNodes);
            PopulateCallTree(callTree);
            PopulatePerformanceTab(perfStats, _allLines.Count);

            // Load call graph: per-file when available, otherwise single merged graph
            if (fileGraphs != null && fileGraphs.Count > 1)
                callGraphPanel.LoadGraphs(fileGraphs);
            else
                callGraphPanel.LoadGraph(graph);

            // Load flame graph and timeline
            if (flameGraphPanel != null)
                flameGraphPanel.LoadCallStack(callTree);

            if (timelinePanel != null)
            {
                timelinePanel.LoadCallStack(callTree);
                timelinePanel.TimelineEntrySelected += TimelinePanel_EntrySelected;
            }

            // Feature 3a/3b: Auto-select topmost node after load
            SelectDefaultTreeNode();
        }

        // Feature 3a/3b: Auto-select first node in active tree
        private void SelectDefaultTreeNode()
        {
            // Feature 3a: Call Tree auto-select
            if (CallTree.Visible && CallTree.Nodes.Count > 0)
            {
                var root = CallTree.Nodes[0]; // "Call Tree" root node
                if (root.Nodes.Count > 0)
                {
                    CallTree.SelectedNode = root.Nodes[0]; // First call
                    CallTree.SelectedNode.EnsureVisible();
                }
            }

            // Feature 3b: API Tree auto-select
            if (ApiTree.Visible && ApiTree.Nodes.Count > 0)
            {
                var root = ApiTree.Nodes[0]; // "API Tree" root node
                if (root.Nodes.Count > 0)
                {
                    ApiTree.SelectedNode = root.Nodes[0]; // First API
                    ApiTree.SelectedNode.EnsureVisible();
                }
            }
        }

        private void PopulateApiTree(List<ApiCallNode> apiNodes)
        {
            // D6: Sort based on current mode
            var sorted = new System.Collections.Generic.List<ApiCallNode>(apiNodes);
            switch (_apiSortMode)
            {
                case ApiSortMode.ByCount:
                    sorted.Sort((a, b) => b.LineNumbers.Count.CompareTo(a.LineNumbers.Count)); break;
                case ApiSortMode.ByFirstLine:
                    sorted.Sort((a, b) => a.FirstLine.CompareTo(b.FirstLine)); break;
                default:
                    sorted.Sort((a, b) => string.Compare(a.ApiName, b.ApiName, StringComparison.OrdinalIgnoreCase)); break;
            }
            apiNodes = sorted;

            ApiTree.BeginUpdate();
            ApiTree.Nodes.Clear();

            // Root node: "API Tree"
            string sortLabel = _apiSortMode == ApiSortMode.ByCount ? Resources.TREE_SORT_LABEL_COUNT
                             : _apiSortMode == ApiSortMode.ByFirstLine ? Resources.TREE_SORT_LABEL_LINE
                             : Resources.TREE_SORT_LABEL_NAME;
            var root = new TreeNode(Resources.TREE_LABEL_API_TREE + sortLabel) { Tag = -1 };

            foreach (var node in apiNodes)
            {
                // Check if all occurrences have matching ENTER/EXIT pairs
                bool allMatched = AreAllApiCallsMatched(node.ApiName);
                int totalCalls = node.LineNumbers.Count + node.ExitOnlyLines.Count;
                string apiLabel = string.Format("{0}  ({1} calls)", node.ApiName, totalCalls);
                var apiRoot = new TreeNode(apiLabel)
                {
                    Tag             = node.FirstLine,
                    ImageIndex      = allMatched ? 0 : 1,
                    SelectedImageIndex = allMatched ? 0 : 1
                };

                // Children: one per ENTER invocation
                foreach (int lineNo in node.LineNumbers)
                {
                    var child = new TreeNode(string.Format("{0} — Ln {1}", node.ApiName, lineNo))
                    {
                        Tag                = lineNo,
                        ImageIndex         = allMatched ? 0 : 1,
                        SelectedImageIndex = allMatched ? 0 : 1
                    };
                    apiRoot.Nodes.Add(child);
                }

                // EXIT-only children (orphan EXITs with no matching ENTER)
                foreach (int lineNo in node.ExitOnlyLines)
                {
                    var child = new TreeNode(string.Format("{0} — Ln {1}  [EXIT only]", node.ApiName, lineNo))
                    {
                        Tag                = lineNo,
                        ImageIndex         = 1,    // always unmatched
                        SelectedImageIndex = 1
                    };
                    apiRoot.Nodes.Add(child);
                }
                root.Nodes.Add(apiRoot);
            }

            ApiTree.Nodes.Add(root);
            // Start collapsed (only root expanded)
            root.Expand();

            ApiTree.EndUpdate();

            // Scroll to top and select the first API node
            if (root.Nodes.Count > 0)
            {
                root.EnsureVisible();
                ApiTree.SelectedNode = root.Nodes[0];
                ApiTree.SelectedNode.EnsureVisible();
            }
        }

        // Cache of API name → all-matched status, built in one pass during tree population
        private Dictionary<string, bool> _matchedApiCache = new Dictionary<string, bool>(StringComparer.Ordinal);

        private Dictionary<string, bool> BuildMatchedApiCache(List<Services.LogEntry> entries)
        {
            var enters = new Dictionary<string, int>(StringComparer.Ordinal);
            var exits  = new Dictionary<string, int>(StringComparer.Ordinal);
            if (entries != null)
            {
                foreach (var e in entries)
                {
                    if (!e.IsApiCall || string.IsNullOrEmpty(e.ApiName)) continue;
                    if (e.IsCallEnter) { enters.TryGetValue(e.ApiName, out int n); enters[e.ApiName] = n + 1; }
                    else if (e.IsCallExit) { exits.TryGetValue(e.ApiName, out int n); exits[e.ApiName] = n + 1; }
                }
            }
            var cache = new Dictionary<string, bool>(StringComparer.Ordinal);
            foreach (var kv in enters)
                cache[kv.Key] = kv.Value == (exits.TryGetValue(kv.Key, out int ex) ? ex : 0);
            return cache;
        }

        private bool AreAllApiCallsMatched(string apiName)
        {
            if (_matchedApiCache.TryGetValue(apiName, out bool matched)) return matched;
            return true; // no data → assume matched
        }

        private void PopulateCallTree(List<CallStackNode> roots)
        {
            CallTree.BeginUpdate();
            CallTree.Nodes.Clear();
            _lazyChildrenMap.Clear(); // Clear lazy load cache

            // Always unsubscribe first so we never accumulate duplicate handlers
            // across file reloads (each reload would otherwise add another subscription).
            CallTree.BeforeExpand -= CallTree_BeforeExpand;

            // Root node: "Call Tree"
            var rootNode = new TreeNode(Resources.TREE_LABEL_CALL_TREE) { Tag = -1 };
            rootNode.NodeFont = new System.Drawing.Font(CallTree.Font, System.Drawing.FontStyle.Bold);

            // Feature C2: Check total node count for lazy loading
            int totalNodes = CountTotalNodes(roots);
            bool useLazyLoading = totalNodes > LAZY_LOAD_THRESHOLD;

            if (useLazyLoading)
            {
                StatusFileName.Text = string.Format(Resources.STATUS_LARGE_TREE_LAZY_LOADING, totalNodes);
            }

            foreach (var root in roots)
                rootNode.Nodes.Add(BuildTreeNode(root, useLazyLoading));

            CallTree.Nodes.Add(rootNode);
            // Issue Fix: Start collapsed (only root expanded)
            rootNode.Expand();
            // Don't expand first level - let users expand as needed

            CallTree.EndUpdate();

            // Wire up before expand event for lazy loading — exactly once (unsubscribed above).
            if (useLazyLoading)
            {
                CallTree.BeforeExpand += CallTree_BeforeExpand;
            }
        }

        // C2: Count total nodes in call tree (depth-capped to avoid StackOverflow)
        private int CountTotalNodes(List<CallStackNode> nodes, int depth = 0)
        {
            if (depth > 500) return 0; // guard against pathologically deep trees
            int count = 0;
            foreach (var node in nodes)
            {
                count++;
                count += CountTotalNodes(node.Children, depth + 1);
            }
            return count;
        }

        // C2: Lazy loading handler - load children when node is expanded
        private void CallTree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node == null) return;
            if (!_lazyChildrenMap.ContainsKey(e.Node)) return;

            var children = _lazyChildrenMap[e.Node];

            // Issue 1: suppress per-node repaints while loading children;
            // show wait cursor and status text so the user sees feedback.
            Cursor prev = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            StatusFileName.Text = string.Format(Resources.STATUS_LOADED_CHILDREN,
                children.Count, GetMethodNameFromNode(e.Node));

            CallTree.BeginUpdate();
            try
            {
                e.Node.Nodes.Clear();
                foreach (var child in children)
                    e.Node.Nodes.Add(BuildTreeNode(child, useLazyLoading: true));
                _lazyChildrenMap.Remove(e.Node);
            }
            finally
            {
                CallTree.EndUpdate();
                Cursor.Current = prev;
            }
        }

        // C2: Overload for backward compatibility (non-lazy)
        private TreeNode BuildTreeNode(CallStackNode csNode)
        {
            return BuildTreeNode(csNode, useLazyLoading: false);
        }

        // C2: Build tree node with optional lazy loading
        private TreeNode BuildTreeNode(CallStackNode csNode, bool useLazyLoading)
        {
            bool matched = csNode.ExitLineNumber > 0;

            // Feature C3: Duration overlay with color coding
            string label = csNode.Label;
            if (csNode.DurationMs > 0)
                label = string.Format("{0}  [{1} ms]", label, csNode.DurationMs);
            else if (matched)
                label = string.Format("{0}  [<1 ms]", label);
            else
                label = string.Format("{0}  [? ms]", label);

            string tooltip = string.Format(
                Resources.TREE_NODE_TOOLTIP_FORMAT,
                csNode.Label,
                csNode.SourceFile ?? "-",
                csNode.LineNumber,
                matched ? csNode.ExitLineNumber.ToString() : Resources.TREE_NODE_EXIT_NOT_FOUND,
                csNode.DurationMs);

            // ImageIndex: 0 = checkmark (matched), 1 = cross (unmatched)
            int imgIdx = matched ? 0 : 1;
            var tn = new TreeNode(label)
            {
                Tag                = csNode.LineNumber,
                ToolTipText        = tooltip,
                ImageIndex         = imgIdx,
                SelectedImageIndex = imgIdx
            };

            // Feature C3: Color coding by duration (green < 100ms, amber 100-500ms, red > 500ms)
            if (csNode.DurationMs > 0)
            {
                const int FAST_MS = 100;
                const int SLOW_MS = 500;

                if (csNode.DurationMs < FAST_MS)
                    tn.ForeColor = Color.FromArgb(0, 128, 0);      // Green
                else if (csNode.DurationMs < SLOW_MS)
                    tn.ForeColor = Color.FromArgb(204, 102, 0);    // Amber
                else
                    tn.ForeColor = Color.FromArgb(200, 0, 0);      // Red
            }

            // C2: Lazy loading - only add placeholder if node has children
            if (csNode.Children != null && csNode.Children.Count > 0)
            {
                if (useLazyLoading)
                {
                    // Lazily init the placeholder font once (avoids a GDI allocation per node).
                    if (_lazyPlaceholderFont == null)
                        _lazyPlaceholderFont = new Font(CallTree.Font, FontStyle.Italic);

                    var placeholder = new TreeNode(Resources.TREE_LAZY_LOAD_PLACEHOLDER)
                    {
                        Tag       = -2,
                        ForeColor = Color.Gray,
                        NodeFont  = _lazyPlaceholderFont
                    };
                    tn.Nodes.Add(placeholder);
                    _lazyChildrenMap[tn] = csNode.Children;
                }
                else
                {
                    // Normal loading - add all children immediately
                    foreach (var child in csNode.Children)
                        tn.Nodes.Add(BuildTreeNode(child, useLazyLoading));
                }
            }

            return tn;
        }

        // ── Performance tab ───────────────────────────────────────────────────
        // ── API tree sort state (D6) ─────────────────────────────────────────
        private enum ApiSortMode { ByName, ByCount, ByFirstLine }
        private ApiSortMode _apiSortMode = ApiSortMode.ByName;

        // ── Performance tab sort state ────────────────────────────────────────
        // Column indices (with Log File column at position 1):
        //   0=API Name  1=Log File  2=Calls  3=Total  4=Avg  5=Min  6=Max  7=Self  8=Source File
        private int  _perfSortColumn    = 3; // default: Total ms
        private bool _perfSortAscending = false;

        private void PopulatePerformanceTab(List<ApiPerfStats> stats, int totalLines)
        {
            // Wire sortable column headers once
            if (!_perfHeaderWired)
            {
                performanceView.ColumnClick += PerformanceView_ColumnClick;
                performanceView.HeaderStyle  = ColumnHeaderStyle.Clickable;
                _perfHeaderWired = true;
            }

            RenderPerformanceRows(stats, totalLines);
        }

        private bool _perfHeaderWired = false;
        private List<ApiPerfStats> _apiPerfStats = new List<ApiPerfStats>();
        private int _lastTotalLines = 0;

        private void PerformanceView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == _perfSortColumn)
                _perfSortAscending = !_perfSortAscending;
            else
            {
                _perfSortColumn    = e.Column;
                // Name and Log File default to ascending; timing columns default to descending
                _perfSortAscending = (e.Column == 0 || e.Column == 1);
            }
            RenderPerformanceRows(_apiPerfStats, _lastTotalLines);
        }

        private void RenderPerformanceRows(List<ApiPerfStats> stats, int totalLines)
        {
            _apiPerfStats   = stats;
            _lastTotalLines = totalLines;

            // Sort all rows by the selected column (flat — no grouping headers)
            var sorted = new List<ApiPerfStats>(stats);
            sorted.Sort((a, b) =>
            {
                int cmp;
                switch (_perfSortColumn)
                {
                    case 0:  cmp = string.Compare(a.ApiName,      b.ApiName,      StringComparison.OrdinalIgnoreCase); break;
                    case 1:  cmp = string.Compare(a.SourceLogFile ?? "", b.SourceLogFile ?? "", StringComparison.OrdinalIgnoreCase); break;
                    case 2:  cmp = a.CallCount.CompareTo(b.CallCount);             break;
                    case 3:  cmp = a.TotalDurationMs.CompareTo(b.TotalDurationMs); break;
                    case 4:  cmp = a.AvgDurationMs.CompareTo(b.AvgDurationMs);     break;
                    case 5:  cmp = a.MinDurationMs.CompareTo(b.MinDurationMs);     break;
                    case 6:  cmp = a.MaxDurationMs.CompareTo(b.MaxDurationMs);     break;
                    case 7:  cmp = a.SelfDurationMs.CompareTo(b.SelfDurationMs);   break;
                    default: cmp = a.TotalDurationMs.CompareTo(b.TotalDurationMs); break;
                }
                return _perfSortAscending ? cmp : -cmp;
            });

            performanceView.BeginUpdate();
            performanceView.Items.Clear();

            // Summary row (spans all columns)
            long sumTotal = 0; int sumCalls = 0;
            foreach (var s in stats) { sumTotal += s.TotalDurationMs; sumCalls += s.TimedCallCount; }

            var summary = new ListViewItem(Resources.PERF_SUMMARY_ROW_LABEL);
            summary.SubItems.Add(string.Empty);                                                       // Log File
            summary.SubItems.Add(sumCalls.ToString());                                                // Calls
            summary.SubItems.Add(sumTotal.ToString());                                                // Total
            summary.SubItems.Add("-"); summary.SubItems.Add("-");
            summary.SubItems.Add("-"); summary.SubItems.Add("-");
            summary.SubItems.Add(string.Format(Resources.PERF_SUMMARY_STATS, stats.Count, totalLines)); // Self / summary

            if (ThemeManager.CurrentTheme == ThemeManager.Theme.Dark)
            {
                summary.BackColor = Color.FromArgb(50, 70, 90);
                summary.ForeColor = Color.FromArgb(220, 220, 220);
            }
            else
            {
                summary.BackColor = Color.FromArgb(210, 225, 255);
                summary.ForeColor = SystemColors.ControlText;
            }
            summary.Font = new System.Drawing.Font(performanceView.Font, System.Drawing.FontStyle.Bold);
            performanceView.Items.Add(summary);

            long threshold = _appSettings?.SlowCallThresholdMs ?? 1000;
            foreach (var s in sorted)
                performanceView.Items.Add(BuildPerfRow(s, threshold));

            performanceView.EndUpdate();
        }

        private ListViewItem BuildPerfRow(ApiPerfStats s, long threshold)
        {
            // Column order: Name(0), LogFile(1), Calls(2), Total(3), Avg(4), Min(5), Max(6), Self(7), Source(8)
            var item = new ListViewItem(s.ApiName);
            item.SubItems.Add(s.SourceLogFile ?? string.Empty);
            item.SubItems.Add(s.CallCount.ToString());
            item.SubItems.Add(s.TotalDurationMs > 0 ? s.TotalDurationMs.ToString() : "-");
            item.SubItems.Add(s.AvgDurationMs   > 0 ? s.AvgDurationMs.ToString()   : "-");
            item.SubItems.Add(s.MinDurationMs   >= 0 ? s.MinDurationMs.ToString()  : "-");
            item.SubItems.Add(s.MaxDurationMs   > 0 ? s.MaxDurationMs.ToString()   : "-");
            item.SubItems.Add(s.SelfDurationMs  > 0 ? s.SelfDurationMs.ToString()  : "-");
            item.SubItems.Add(s.SourceFile      ?? string.Empty);

            if (s.TotalDurationMs >= threshold)
            {
                item.BackColor = ThemeManager.ErrorBackgroundColor;
                item.ForeColor = ThemeManager.ForegroundColor;
            }
            else if (s.TotalDurationMs >= threshold / 10)
            {
                item.BackColor = ThemeManager.WarningBackgroundColor;
                item.ForeColor = ThemeManager.ForegroundColor;
            }
            else
            {
                item.BackColor = ThemeManager.BackgroundColor;
                item.ForeColor = ThemeManager.ForegroundColor;
            }
            return item;
        }

        // ── Tree visibility ───────────────────────────────────────────────────
        // D6: Sort API tree by name / call count / first line
        public void SortApiTreeBy(string mode)
        {
            switch (mode)
            {
                case "count":    _apiSortMode = ApiSortMode.ByCount;    break;
                case "line":     _apiSortMode = ApiSortMode.ByFirstLine; break;
                default:         _apiSortMode = ApiSortMode.ByName;     break;
            }
            if (_apiNodes != null && _apiNodes.Count > 0)
                PopulateApiTree(_apiNodes);
        }

        private void SyncTreeVisibility()
        {
            bool showCall = CallTreeButton.Checked;
            bool showApi  = ApiTreeButton.Checked;

            // Make trees mutually exclusive - only one can be visible at a time
            if (showCall && showApi)
            {
                // Both checked - determine which was just clicked
                if (!CallTree.Visible)
                {
                    // Call Tree was just checked, hide API Tree
                    showApi = false;
                    ApiTreeButton.Checked = false;
                }
                else
                {
                    // API Tree was just checked, hide Call Tree
                    showCall = false;
                    CallTreeButton.Checked = false;
                }
            }

            // Ensure at least one tree is always visible
            if (!showCall && !showApi)
            {
                // Default to Call Tree if both are unchecked
                showCall = true;
                CallTreeButton.Checked = true;
            }

            CallTree.Visible = showCall;
            ApiTree.Visible  = showApi;

            // When the API tree becomes visible, ensure the root node is selected and in view
            if (showApi && ApiTree.Nodes.Count > 0 && ApiTree.SelectedNode == null)
            {
                ApiTree.SelectedNode = ApiTree.Nodes[0];
                ApiTree.Nodes[0].EnsureVisible();
            }

            // Update View menu items
            showCallTreeMenuItem.Checked = showCall;
            showApiTreeMenuItem.Checked = showApi;

            // Ensure search box stays on top
            treeSearchTextBox.BringToFront();

            LayoutTrees();
        }

        // Helper methods for tree switching
        private void ShowApiTree()
        {
            // Ensure only API Tree is visible
            CallTreeButton.Checked = false;
            ApiTreeButton.Checked = true;
        }

        private void ShowCallTree()
        {
            // Ensure only Call Tree is visible
            ApiTreeButton.Checked = false;
            CallTreeButton.Checked = true;
        }

        private void SyncTabVisibility()
        {
            // Hide tabs feature removed - tabs are always visible with proper labels
            // mainTabControl.Appearance = TabAppearance.Normal;
        }

        private void LayoutTrees()
        {
            // NOTE: Manual layout - search box at top, trees below
            // This is simpler and more reliable than Dock which was causing issues

            if (mainSplitContainer?.Panel1 == null) return;

            int panelWidth = mainSplitContainer.Panel1.ClientSize.Width;
            int panelHeight = mainSplitContainer.Panel1.ClientSize.Height;

            // Position search box at top with 3px padding
            treeSearchTextBox.Location = new Point(3, 3);
            treeSearchTextBox.Width = panelWidth - 6;

            // Position trees below search box (at Y=31 to give 6px spacing after 22px textbox)
            int treeY = 31;
            int treeHeight = panelHeight - treeY - 3; // Leave 3px at bottom
            int treeWidth = panelWidth - 6;

            if (CallTree.Visible)
            {
                CallTree.SetBounds(3, treeY, treeWidth, treeHeight);
            }

            if (ApiTree.Visible)
            {
                ApiTree.SetBounds(3, treeY, treeWidth, treeHeight);
            }
        }

        private void Panel1_Resize(object sender, EventArgs e)
        {
            LayoutTrees();
        }

        // ── Tree → scroll log ─────────────────────────────────────────────────
        private void ApiTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ScrollLogToLine(e.Node?.Tag);

            // If the Log Details tab is the active tab, auto-inspect the selected line.
            if (mainTabControl.SelectedTab == logDetailTab
                && e.Node?.Tag is int ln
                && _lineIndexMap.TryGetValue(ln, out int idx))
                ShowLogDetail(idx);
            ShowApiDetails(e.Node);  // D3: show invocation details

            // D5: Cross-reference - highlight matching node in Call Tree if both visible
            if (e.Node != null && CallTree.Nodes.Count > 0)
            {
                string methodName = GetMethodNameFromNode(e.Node);
                if (!string.IsNullOrEmpty(methodName))
                {
                    // Don't trigger recursive selection events
                    CallTree.AfterSelect -= CallTree_AfterSelect;
                    TryHighlightInCallTree(methodName);
                    CallTree.AfterSelect += CallTree_AfterSelect;
                }
            }
        }

        // D3: API Invocation Details Panel
        private void ShowApiDetails(TreeNode node)
        {
            if (node == null || logDetailBox == null) return;
            if (node.Tag == null || (node.Tag is int t && t < 0)) return;

            // Find the API name from this node or its parent
            string apiName = null;
            var n = node;
            while (n != null)
            {
                string lbl = n.Text;
                // API root nodes have format "ApiName  (N calls)"
                if (lbl.Contains("  (") && lbl.Contains(" calls)"))
                {
                    apiName = lbl.Substring(0, lbl.IndexOf("  ("));
                    break;
                }
                // Child nodes have format "ApiName — Ln N"
                if (lbl.Contains(" — Ln "))
                {
                    apiName = lbl.Substring(0, lbl.IndexOf(" — Ln "));
                    break;
                }
                n = n.Parent;
            }

            if (string.IsNullOrEmpty(apiName)) return;

            // Find matching API stats from performance data
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(string.Format(Resources.API_DETAILS_HEADER, apiName));
            sb.AppendLine();

            // Basic invocation info from _apiNodes
            var apiNode = _apiNodes?.Find(a => a.ApiName == apiName);
            if (apiNode != null)
            {
                sb.AppendLine(string.Format(Resources.API_DETAILS_TOTAL_INVOCATIONS, apiNode.LineNumbers.Count));
                sb.AppendLine(string.Format(Resources.API_DETAILS_FIRST_OCCURRENCE, apiNode.FirstLine));
                sb.AppendLine();
                sb.AppendLine(Resources.API_DETAILS_INVOCATION_LINES);
                foreach (int ln in apiNode.LineNumbers)
                    sb.AppendLine(string.Format(Resources.API_DETAILS_LINE_INDENTED, ln));
            }

            // Match/unmatch status
            bool matched = AreAllApiCallsMatched(apiName);
            sb.AppendLine();
            sb.AppendLine(string.Format(Resources.API_DETAILS_ENTER_EXIT_MATCHED, 
                matched ? Resources.API_DETAILS_MATCHED_YES : Resources.API_DETAILS_MATCHED_NO));

            if (logDetailBox.InvokeRequired)
                logDetailBox.Invoke((Action)(() => logDetailBox.Text = sb.ToString()));
            else
                logDetailBox.Text = sb.ToString();
        }

        private void ApiTree_Click(object sender, EventArgs e) { }
        private void ApiTree_MouseClick(object sender, MouseEventArgs e) { }

        private void CallTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ScrollLogToLine(e.Node?.Tag);

            // If the Log Details tab is the active tab, auto-inspect the selected line.
            if (mainTabControl.SelectedTab == logDetailTab
                && e.Node?.Tag is int ln
                && _lineIndexMap.TryGetValue(ln, out int idx))
                ShowLogDetail(idx);

            // D5: Cross-reference - highlight matching node in API Tree if both have data
            if (e.Node != null && ApiTree.Nodes.Count > 0)
            {
                string methodName = GetMethodNameFromNode(e.Node);
                if (!string.IsNullOrEmpty(methodName))
                {
                    ApiTree.AfterSelect -= ApiTree_AfterSelect;
                    TryHighlightInApiTree(methodName);
                    ApiTree.AfterSelect += ApiTree_AfterSelect;
                }
            }
        }

        private void ScrollLogToLine(object tag)
        {
            if (!(tag is int lineNumber)) return;
            ScrollLogToLine(lineNumber);
        }

        private void ScrollLogToLine(int lineNumber)
        {
            if (!_lineIndexMap.TryGetValue(lineNumber, out int idx)) return;
            if (idx < 0 || idx >= logListView.VirtualListSize) return;

            // Feature H1: Show 10 previous lines by scrolling appropriately
            int scrollToIdx = Math.Max(0, idx - 10);
            logListView.EnsureVisible(scrollToIdx);
            logListView.EnsureVisible(idx); // Make sure selected line is visible

            logListView.SelectedIndices.Clear();
            logListView.SelectedIndices.Add(idx);
            logListView.Focus();
        }

        // ── Log Details panel ─────────────────────────────────────────────────
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectionStatus();

            // If the Log Details tab is active, auto-inspect the newly selected row.
            if (mainTabControl.SelectedTab == logDetailTab
                && logListView.SelectedIndices.Count > 0)
                ShowLogDetail(logListView.SelectedIndices[0]);
        }

        private void ShowLogDetail(int idx)
        {
            if (idx < 0 || idx >= _virtualLines.Count || _lineInspector == null) return;

            // Convert _virtualLines to the panel's simple InspectorLine type
            var lines = new List<UI.LineInspectorPanel.InspectorLine>(_virtualLines.Count);
            foreach (var vl in _virtualLines)
                lines.Add(new UI.LineInspectorPanel.InspectorLine { LineNumber = vl.LineNumber, Text = vl.Text });

            _lineInspector.Inspect(lines, idx, _lastEntries);

            // Switch to Log Details tab
            if (mainTabControl != null && logDetailTab != null
                && mainTabControl.TabPages.Contains(logDetailTab))
                mainTabControl.SelectedTab = logDetailTab;
        }

        /// <summary>Inspects the currently selected log-list row (F12 / context menu).</summary>
        private void InspectSelectedLine()
        {
            if (logListView.SelectedIndices.Count == 0) return;
            ShowLogDetail(logListView.SelectedIndices[0]);
        }

        private void contextInspectLineMenuItem_Click(object sender, EventArgs e) =>
            InspectSelectedLine();

        // ── #7: Virtual mode handler ──────────────────────────────────────────
        private void listView1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex < 0 || e.ItemIndex >= _virtualLines.Count)
            {
                e.Item = new ListViewItem();
                return;
            }
            var vl   = _virtualLines[e.ItemIndex];
            var item = new ListViewItem(vl.LineNumber.ToString());
            item.SubItems.Add(vl.Text);

            // P-08: use the pre-computed BackColour (bookmark / highlight / level colour).
            // The previous code re-ran text.Contains("ERROR") etc. on every paint call which
            // (a) was slower than a struct field read and (b) used the wrong search pattern
            // causing bookmark and search-highlight colours to be invisible.
            // After a theme change RefreshVirtualLineColours() updates the stored colours.
            item.BackColor = vl.BackColour;
            item.ForeColor = ThemeManager.ForegroundColor;
            e.Item = item;
        }

        /// <summary>
        /// Updates the BackColour of every virtual line after a theme change.
        /// Bookmark and search-highlight colours are preserved; only the theme-dependent
        /// level colours (Error / Warning / Default) are refreshed.
        /// Called from ApplyThemeWithOverlay so the ListView immediately reflects the
        /// new theme without an extra full repopulation (P-08).
        /// </summary>
        private void RefreshVirtualLineColours()
        {
            if (_virtualLines == null || _virtualLines.Count == 0) return;

            int highlightArgb    = _appSettings?.HighlightColor.ToArgb() ?? Color.Yellow.ToArgb();
            int bookmarkDarkArgb  = Color.FromArgb(0, 70, 130).ToArgb();
            int bookmarkLightArgb = Color.FromArgb(200, 230, 255).ToArgb();

            for (int i = 0; i < _virtualLines.Count; i++)
            {
                var vl = _virtualLines[i];
                int argb = vl.BackColour.ToArgb();

                // Skip user-set colours (bookmarks and search highlights) — these are
                // theme-independent and should not be overwritten.
                if (argb == bookmarkDarkArgb || argb == bookmarkLightArgb
                    || argb == highlightArgb) continue;

                // Re-evaluate the theme-dependent level colour.
                Color fresh = GetLineColour(vl.Text);
                if (argb != fresh.ToArgb())
                    _virtualLines[i] = new VirtualLogLine
                    {
                        LineNumber = vl.LineNumber,
                        Text       = vl.Text,
                        BackColour = fresh
                    };
            }
            if (logListView != null) logListView.Invalidate();
        }

        // ── #8: Call Graph reset button ───────────────────────────────────────
        private void callGraphResetButton_Click(object sender, EventArgs e) =>
            callGraphPanel.ResetView();

        private void callGraphToggleButton_Click(object sender, EventArgs e)
        {
            callGraphPanel.ToggleViewMode();
            callGraphToggleButton.Text = callGraphPanel.IsStructuralView
                ? "⇄ Weighted View"
                : "⇄ Structural View";
        }

        // ── Drag-and-drop ─────────────────────────────────────────────────────
        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files == null || files.Length == 0) return;

            string droppedFile = files[0];

            // If no file is currently open, just open the dropped file directly
            if (string.IsNullOrEmpty(_currentFilePath) || _allLines.Count == 0)
            {
                LoadFileAsync(droppedFile);
                return;
            }

            // A file is already open — ask the user what to do
            using (var dlg = new Form())
            {
                dlg.Text            = "Open Dropped File";
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.MinimizeBox     = false;
                dlg.MaximizeBox     = false;
                dlg.ClientSize      = new Size(360, 130);

                var lbl = new Label
                {
                    Text      = string.Format("A file is already open.\nWhat would you like to do with:\n{0}",
                                    Path.GetFileName(droppedFile)),
                    AutoSize  = false,
                    Size      = new Size(340, 55),
                    Location  = new Point(10, 10)
                };

                var btnMerge = new Button
                {
                    Text     = "Merge",
                    Size     = new Size(100, 30),
                    Location = new Point(30, 80),
                    DialogResult = DialogResult.Yes
                };

                var btnOpen = new Button
                {
                    Text     = "Open Independently",
                    Size     = new Size(140, 30),
                    Location = new Point(145, 80),
                    DialogResult = DialogResult.No
                };

                var btnCancel = new Button
                {
                    Text     = "Cancel",
                    Size     = new Size(70, 30),
                    Location = new Point(295, 80),
                    DialogResult = DialogResult.Cancel
                };

                dlg.Controls.AddRange(new Control[] { lbl, btnMerge, btnOpen, btnCancel });
                dlg.AcceptButton = btnMerge;
                dlg.CancelButton = btnCancel;

                var result = dlg.ShowDialog(this);

                if (result == DialogResult.No)
                {
                    // Open independently
                    LoadFileAsync(droppedFile);
                }
                else if (result == DialogResult.Yes)
                {
                    // Merge dropped file into the already-open file
                    MergeDroppedFileAsync(droppedFile);
                }
                // Cancel: do nothing
            }
        }

        private async void MergeDroppedFileAsync(string droppedFile)
        {
            StartOperation(string.Format(Resources.OPERATION_MERGING_FILES, 2));

            try
            {
                List<string> merged;

                bool currentIsMerge = _currentFilePath.StartsWith("[Merged:");

                if (!currentIsMerge && File.Exists(_currentFilePath))
                {
                    // Simple case: current session is a single file still on disk
                    merged = await _mergeLogService.MergeAsync(new[] { _currentFilePath, droppedFile });
                    _mergedSourcePaths = new List<string> { _currentFilePath, droppedFile };
                }
                else if (currentIsMerge && _mergedSourcePaths.Count > 0
                         && _mergedSourcePaths.TrueForAll(File.Exists))
                {
                    // All original source files still exist on disk — re-merge cleanly
                    // so original [filename] tags are preserved and no temp file is needed.
                    var allPaths = new List<string>(_mergedSourcePaths) { droppedFile };
                    merged = await _mergeLogService.MergeAsync(allPaths);
                    _mergedSourcePaths = allPaths;
                }
                else
                {
                    // Original files are no longer accessible (e.g. already-merged from memory).
                    // Merge the new file into the existing tagged in-memory lines without
                    // re-tagging them, so original [filename] prefixes are preserved.
                    merged = await _mergeLogService.MergeTaggedWithNewFilesAsync(
                        _allLines, new[] { droppedFile });
                    _mergedSourcePaths.Add(droppedFile);
                }

                // Build the display name from the real file names we know about
                _currentFilePath = "[Merged: " + string.Join(", ",
                    _mergedSourcePaths.ConvertAll(Path.GetFileName)) + "]";

                _allLines = merged;
                _searchService.Reset();
                ClearHighlighting();

                UpdateStatusProgress(25, Resources.STATUS_PROCESSING_MERGED_DATA);
                await Task.Delay(10);
                PopulateVirtualListView(_allLines);

                UpdateStatusProgress(50, Resources.STATUS_BUILDING_MERGED_TREE);
                await Task.Delay(10);
                var mergeEntries = await Task.Run(() => _parserService.Parse(_allLines));
                var mApiTask     = Task.Run(() => _parserService.BuildApiList(mergeEntries));
                var mCallTask    = Task.Run(() => _parserService.BuildCallTreeGroupedByFile(mergeEntries));
                await Task.WhenAll(mApiTask, mCallTask);
                var mCallTree    = mCallTask.Result;

                UpdateStatusProgress(75, "Building performance stats...");
                var mPerfStats   = await Task.Run(() => _parserService.BuildPerformanceStatsGroupedByFile(mergeEntries));

                UpdateStatusProgress(90, "Building call graph...");
                var mFileGraphs  = await Task.Run(() => _callGraphService.BuildGroupedByFile(mergeEntries));
                var mGraph       = mFileGraphs.Count == 1 ? mFileGraphs[0].Graph : _callGraphService.Build(mergeEntries);

                UpdateStatusProgress(98, "Populating views...");
                PopulateTreesFromData(mergeEntries, mApiTask.Result, mCallTree, mPerfStats, mGraph, mFileGraphs);
                UpdateStatusProgress(100, "Merge complete.");

                SetDocumentLoaded(true);
                FileStatus.Image = IconGenerator.CreateStatusOkIcon(IconGenerator.IconSize.Small);
                UpdateStatusBar();

                MessageBox.Show(
                    string.Format(Resources.MSG_MERGE_SUCCESSFUL, _mergedSourcePaths.Count, merged.Count),
                    Resources.DIALOG_TITLE_MERGE_COMPLETE, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Resources.MSG_MERGE_FAILED, ex.Message),
                    Resources.DIALOG_TITLE_MERGE_LOGS, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                EndOperation();
            }
        }

        // ── File loading ──────────────────────────────────────────────────────
        private async void LoadFileAsync(string filePath, int restoreTopIndex = -1)
        {
            if (_isLoading) return;
            _isLoading = true;
            SetDocumentLoaded(false);
            FileStatus.Image = IconGenerator.CreateStatusLoadingIcon(IconGenerator.IconSize.Small);
            FileLoadProgress.Visible = true;
            FileLoadProgress.Value = 0;
            StatusFileName.Text = Resources.STATUS_LOADING;
            _overlay.Show(Resources.STATUS_LOADING);

            // Persist bookmarks for the previous file before we discard its context.
            try { _bookmarkService.SaveBookmarks(); } catch { /* non-fatal */ }

            try
            {
                // Read file with progress updates
                var lines = await _logFileService.ReadLinesAsync(filePath, (progress, message) =>
                {
                    // Update UI on UI thread
                    this.Invoke((Action)(() =>
                    {
                        FileLoadProgress.Value = progress;
                        StatusFileName.Text = message;
                        _overlay.SetProgress(progress, message);
                    }));
                });

                _allLines        = lines;
                _currentFilePath = filePath;
                _searchService.Reset();
                ClearHighlighting(); // Clear any previous search highlights

                // Load bookmarks for this file
                _bookmarkService.LoadBookmarks(filePath);

                // Populate views with progress
                UpdateStatusProgress(20, Resources.STATUS_PROCESSING_LOG_DATA);
                await Task.Delay(10);

                PopulateVirtualListView(_allLines);
                // D-01: restore scroll position now that VirtualListSize is set.
                if (restoreTopIndex > 0 && restoreTopIndex < _virtualLines.Count)
                    logListView.EnsureVisible(restoreTopIndex);
                PopulateRawView(_allLines);

                UpdateStatusProgress(35, Resources.STATUS_BUILDING_CALL_TREE);
                await Task.Delay(10);

                // Parse and build all tree data in parallel on background threads
                var entries      = await Task.Run(() => _parserService.Parse(_allLines));
                var apiNodesTask = Task.Run(() => _parserService.BuildApiList(entries));
                var callTreeTask = Task.Run(() => _parserService.BuildCallTree(entries));
                await Task.WhenAll(apiNodesTask, callTreeTask);

                UpdateStatusProgress(65, "Building performance statistics...");
                var callTree  = callTreeTask.Result;
                var perfStats = await Task.Run(() => _parserService.BuildPerformanceStats(callTree));

                UpdateStatusProgress(80, "Building call graph...");
                var graph = await Task.Run(() => _callGraphService.Build(entries));

                UpdateStatusProgress(95, "Populating views...");
                // Populate UI with the pre-built data (must be on UI thread)
                PopulateTreesFromData(entries, apiNodesTask.Result, callTree, perfStats, graph);
                UpdateStatusProgress(100, "Load complete.");

                SetDocumentLoaded(true);
                FileStatus.Image = IconGenerator.CreateStatusOkIcon(IconGenerator.IconSize.Small);
                _logFileService.WatchFile(filePath);
                UpdateStatusBar();
                _appSettings.AddRecentFile(filePath);
                BuildMruMenu();
            }
            catch (UnauthorizedAccessException ex) { ShowLoadError(filePath, Resources.LOAD_ERROR_ACCESS_DENIED, ex.Message); }
            catch (IOException ex)                 { ShowLoadError(filePath, Resources.LOAD_ERROR_FILE_READ, ex.Message); }
            catch (Exception ex)                   { ShowLoadError(filePath, Resources.LOAD_ERROR_UNEXPECTED, ex.Message); }
            finally
            {
                FileLoadProgress.Visible = false;
                FileLoadProgress.Value = 0;
                _isLoading = false;
                _overlay.Hide();
            }
        }

        // ── #7: Virtual mode population ───────────────────────────────────────
        /// <summary>
        /// Builds the backing store for virtual mode. No ListViewItems are created here —
        /// items are produced on demand in RetrieveVirtualItem. This makes loading
        /// 500k-line files near-instant.
        /// </summary>
        private void PopulateRawView(IList<string> lines)
        {
            if (rawTextBox == null) return;
            const int MaxRawLines = 50_000;
            bool truncated = lines.Count > MaxRawLines;
            int count = Math.Min(lines.Count, MaxRawLines);

            // P-03: build the text then load into the RTB with WM_SETREDRAW suppressed
            // and use AppendText instead of .Text= so the control parses the RTF once
            // rather than re-parsing the whole document on every incremental append.
            var sb = new System.Text.StringBuilder(count * 80);
            for (int i = 0; i < count; i++) sb.AppendLine(lines[i]);
            if (truncated)
                sb.AppendLine(string.Format("[... {0:N0} more lines not shown — file exceeds raw view limit ...]",
                              lines.Count - MaxRawLines));

            Services.NativeMethods.SuppressRedraw(rawTextBox);
            try
            {
                rawTextBox.Clear();
                rawTextBox.AppendText(sb.ToString());
            }
            finally
            {
                Services.NativeMethods.ResumeRedraw(rawTextBox);
            }
        }

        private void PopulateVirtualListView(IList<string> lines)
        {
            _virtualLines = new List<VirtualLogLine>(lines.Count);
            _lineIndexMap = new Dictionary<int, int>(lines.Count);

            // Feature B10: Index errors and warnings
            _errorLines.Clear();
            _warningLines.Clear();
            _currentErrorIndex = -1;
            _currentWarningIndex = -1;

            for (int i = 0; i < lines.Count; i++)
            {
                int lineNumber = i + 1;
                Color backColor = GetLineColour(lines[i]);

                // D-04: apply a theme-aware tint for bookmarked lines so the
                // colour reads correctly in both Light and Dark themes.
                if (_bookmarkService.IsBookmarked(lineNumber))
                {
                    backColor = ThemeManager.CurrentTheme == ThemeManager.Theme.Dark
                        ? Color.FromArgb(0, 70, 130)    // dark-blue tint for dark theme
                        : Color.FromArgb(200, 230, 255); // light-blue tint for light theme
                }

                _virtualLines.Add(new VirtualLogLine
                {
                    LineNumber = lineNumber,
                    Text       = lines[i],
                    BackColour = backColor
                });
                _lineIndexMap[lineNumber] = i;

                // Feature B10: Index error and warning lines
                if (!string.IsNullOrEmpty(lines[i]))
                {
                    int first = lines[i].IndexOf(": ", StringComparison.Ordinal);
                    if (first >= 0 && first + 3 < lines[i].Length)
                    {
                        char level = lines[i][first + 2];
                        if (level == 'E') _errorLines.Add(i);
                        else if (level == 'W') _warningLines.Add(i);
                    }
                }
            }

            // Safety check: ensure logListView is initialized
            if (logListView != null)
            {
                logListView.VirtualListSize = _virtualLines.Count;
                logListView.Invalidate();
                MeasureAndStoreMaxLineWidth();
                AutoResizeLogListColumns();
            }
            // Rebuild the zero-alloc text wrapper for FindNext (P-06).
            _virtualLineTexts = new VirtualLineTextList(_virtualLines);
            UpdateStatusBar();
        }

        private void PopulateVirtualListViewFiltered(IList<FilteredLine> filtered)
        {
            _virtualLines = new List<VirtualLogLine>(filtered.Count);
            _lineIndexMap = new Dictionary<int, int>(filtered.Count);

            // Rebuild error/warning navigation indices for the filtered view so that
            // F8 / Ctrl+F8 navigation jumps to the correct rows after a filter is applied.
            _errorLines.Clear();
            _warningLines.Clear();
            _currentErrorIndex   = -1;
            _currentWarningIndex = -1;

            for (int i = 0; i < filtered.Count; i++)
            {
                var fl = filtered[i];
                _virtualLines.Add(new VirtualLogLine
                {
                    LineNumber = fl.LineNumber,
                    Text       = fl.Text,
                    BackColour = GetLineColour(fl.Text)
                });
                _lineIndexMap[fl.LineNumber] = i;

                // Index error and warning rows in the filtered view.
                if (!string.IsNullOrEmpty(fl.Text))
                {
                    int first = fl.Text.IndexOf(": ", StringComparison.Ordinal);
                    if (first >= 0 && first + 3 < fl.Text.Length)
                    {
                        char level = fl.Text[first + 2];
                        if (level == 'E') _errorLines.Add(i);
                        else if (level == 'W') _warningLines.Add(i);
                    }
                }
            }

            // Safety check: ensure logListView is initialized
            if (logListView != null)
            {
                logListView.VirtualListSize = _virtualLines.Count;
                logListView.Invalidate();

                // Issue Fix: Auto-resize columns to fit content
                MeasureAndStoreMaxLineWidth();
                AutoResizeLogListColumns();
            }
            // Rebuild zero-alloc text wrapper (P-06).
            _virtualLineTexts = new VirtualLineTextList(_virtualLines);

            UpdateStatusBar();
        }

        // ── Icon Size Management ──────────────────────────────────────────────
        private void ApplyIconSize()
        {
            IconGenerator.IconSize sz;
            switch (_appSettings.ToolbarIconSize)
            {
                case "Small":
                    sz = IconGenerator.IconSize.Small;
                    mainToolStrip.ImageScalingSize = new Size(16, 16);
                    break;
                case "Large":
                    sz = IconGenerator.IconSize.Large;
                    mainToolStrip.ImageScalingSize = new Size(32, 32);
                    break;
                default:
                    sz = IconGenerator.IconSize.Medium;
                    mainToolStrip.ImageScalingSize = new Size(24, 24);
                    break;
            }

            // Menu size — always 16px regardless of toolbar setting
            var msz = IconGenerator.IconSize.Small;

            // ── Toolbar buttons ───────────────────────────────────────────────
            OpenButton.Image             = IconGenerator.CreateOpenIcon(sz);
            SaveButton.Image             = IconGenerator.CreateSaveIcon(sz);
            SaveToXLSButton.Image        = IconGenerator.CreateExportXlsIcon(sz);
            RefreshButton.Image          = IconGenerator.CreateReloadIcon(sz);
            CopyButton.Image             = IconGenerator.CreateCopyIcon(sz);
            FindButton.Image             = IconGenerator.CreateFindIcon(sz);
            FindNextButton.Image         = IconGenerator.CreateFindNextIcon(sz);
            FilterButton.Image           = IconGenerator.CreateFilterIcon(sz);
            ExpandAllButton.Image        = IconGenerator.CreateExpandIcon(sz);
            ExpandAllButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            CollapseAllButton.Image      = IconGenerator.CreateCollapseIcon(sz);
            CollapseAllButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            JumpToMatchingButton.Image   = IconGenerator.CreateJumpMatchIcon(sz);
            JumpToMatchingButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            CallTreeButton.Image         = IconGenerator.CreateCallTreeIcon(sz);
            ApiTreeButton.Image          = IconGenerator.CreateApiTreeIcon(sz);
            SettingsButton.Image         = IconGenerator.CreateSettingsIcon(sz);
            ShowHelpButton.Image         = IconGenerator.CreateHelpIcon(sz);

            // Navigation error/warning buttons
            prevErrorButton.Image   = IconGenerator.CreatePrevErrorIcon(sz);
            prevErrorButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            nextErrorButton.Image   = IconGenerator.CreateNextErrorIcon(sz);
            nextErrorButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            prevWarningButton.Image = IconGenerator.CreatePrevWarningIcon(sz);
            prevWarningButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            nextWarningButton.Image = IconGenerator.CreateNextWarningIcon(sz);
            nextWarningButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;

            // ── File menu ─────────────────────────────────────────────────────
            openMenuItem.Image                 = IconGenerator.CreateOpenIcon(msz);
            saveAsMenuItem.Image               = IconGenerator.CreateSaveIcon(msz);
            exportFilteredLogsMenuItem.Image   = IconGenerator.CreateExportXlsIcon(msz);
            exportPerformanceMenuItem.Image    = IconGenerator.CreateExportCsvIcon(msz);
            exportTreeJsonMenuItem.Image       = IconGenerator.CreateExportJsonIcon(msz);
            exportTreeXmlMenuItem.Image        = IconGenerator.CreateExportXmlIcon(msz);
            exportTimelineMenuItem.Image       = IconGenerator.CreateExportImageIcon(msz);
            exportFlameGraphMenuItem.Image     = IconGenerator.CreateExportImageIcon(msz);
            mergeLogsMenuItem.Image            = IconGenerator.CreateMergeLogsIcon(msz);
            reloadMenuItem.Image               = IconGenerator.CreateReloadIcon(msz);
            exitMenuItem.Image                 = IconGenerator.CreateExitIcon(msz);

            // ── Edit menu ─────────────────────────────────────────────────────
            copyMenuItem.Image             = IconGenerator.CreateCopyIcon(msz);
            copyWithHeadersMenuItem.Image  = IconGenerator.CreateCopyHeadersIcon(msz);
            findMenuItem.Image             = IconGenerator.CreateFindIcon(msz);
            findNextMenuItem.Image         = IconGenerator.CreateFindNextIcon(msz);
            findAllMenuItem.Image          = IconGenerator.CreateFindAllIcon(msz);
            filterMenuItem.Image           = IconGenerator.CreateFilterIcon(msz);
            clearFilterMenuItem.Image      = IconGenerator.CreateClearFilterIcon(msz);
            expandAllMenuItem.Image        = IconGenerator.CreateExpandIcon(msz);
            collapseAllMenuItem.Image      = IconGenerator.CreateCollapseIcon(msz);
            jumpToMatchingMenuItem.Image   = IconGenerator.CreateJumpMatchIcon(msz);
            jumpToLineMenuItem.Image       = IconGenerator.CreateJumpLineIcon(msz);
            toggleBookmarkMenuItem.Image   = IconGenerator.CreateBookmarkIcon(msz);
            nextBookmarkMenuItem.Image     = IconGenerator.CreateBookmarkNextIcon(msz);
            previousBookmarkMenuItem.Image = IconGenerator.CreateBookmarkPrevIcon(msz);
            showBookmarksMenuItem.Image    = IconGenerator.CreateBookmarkShowIcon(msz);
            clearBookmarksMenuItem.Image   = IconGenerator.CreateBookmarkClearIcon(msz);

            // ── View menu ─────────────────────────────────────────────────────
            showCallTreeMenuItem.Image     = IconGenerator.CreateCallTreeIcon(msz);
            showApiTreeMenuItem.Image      = IconGenerator.CreateApiTreeIcon(msz);
            tabsMenuItem.Image             = IconGenerator.CreateTabIcon(msz);
            selectFontMenuItem.Image       = IconGenerator.CreateFontIcon(msz);
            showToolbarMenuItem.Image      = IconGenerator.CreateToolbarIcon(msz);
            showLogTabMenuItem.Image          = IconGenerator.CreateTabLogIcon(msz);
            showRawTabMenuItem.Image          = IconGenerator.CreateTabRawIcon(msz);
            showPerformanceTabMenuItem.Image          = IconGenerator.CreateTabPerformanceIcon(msz);
            showLogDetailsTabMenuItem.Image          = IconGenerator.CreateTabLogDetailsIcon(msz);
            showCallGraphMenuItem.Image     = IconGenerator.CreateTabCallGraphIcon(msz);
            showFlameGraphTabMenuItem.Image = IconGenerator.CreateTabFlameGraphIcon(msz);
            showTimelineTabMenuItem.Image   = IconGenerator.CreateTabTimelineIcon(msz);

            // ── Options menu ──────────────────────────────────────────────────
            settingsMenuItem.Image         = IconGenerator.CreateSettingsIcon(msz);

            // ── Help menu ─────────────────────────────────────────────────────
            viewHelpMenuItem.Image         = IconGenerator.CreateHelpIcon(msz);
            keyboardShortcutsMenuItem.Image = IconGenerator.CreateKeyboardIcon(msz);
            aboutMenuItem.Image            = IconGenerator.CreateAboutIcon(msz);
            checkForUpdatesMenuItem.Image  = IconGenerator.CreateCheckUpdatesIcon(msz);
            reportErrorsMenuItem.Image     = IconGenerator.CreateReportErrorsIcon(msz);

            // ── Log view context menu ─────────────────────────────────────────
            contextCopyMenuItem.Image          = IconGenerator.CreateCopyIcon(msz);
            contextCopyWithHeadersMenuItem.Image = IconGenerator.CreateCopyHeadersIcon(msz);
            contextFindMenuItem.Image          = IconGenerator.CreateFindIcon(msz);
            contextFilterMenuItem.Image        = IconGenerator.CreateFilterIcon(msz);
            contextExpandAllMenuItem.Image     = IconGenerator.CreateExpandIcon(msz);
            contextCollapseAllMenuItem.Image   = IconGenerator.CreateCollapseIcon(msz);
            contextJumpToMatchingMenuItem.Image = IconGenerator.CreateJumpMatchIcon(msz);
            contextRefreshMenuItem.Image       = IconGenerator.CreateReloadIcon(msz);

            // ── Tree context menu ─────────────────────────────────────────────
            treeContextCopyNodeNameMenuItem.Image   = IconGenerator.CreateCopyIcon(msz);
            treeContextCopySubtreeMenuItem.Image    = IconGenerator.CreateCopyHeadersIcon(msz);
            treeContextExpandAllMenuItem.Image      = IconGenerator.CreateExpandIcon(msz);
            treeContextCollapseAllMenuItem.Image    = IconGenerator.CreateCollapseIcon(msz);
            treeContextJumpToMatchingMenuItem.Image = IconGenerator.CreateJumpMatchIcon(msz);
            treeContextSaveBranchMenuItem.Image     = IconGenerator.CreateSaveBranchIcon(msz);
            treeContextExportBranchCsvMenuItem.Image = IconGenerator.CreateExportCsvIcon(msz);
            treeContextSearchInGrokMenuItem.Image   = IconGenerator.CreateGrokIcon(msz);
            treeContextShowInOtherTreeMenuItem.Image = IconGenerator.CreateShowInTreeIcon(msz);
        }

        // ── Tab Icons ─────────────────────────────────────────────────────────
        /// <summary>
        /// Assigns a distinct Segoe MDL2 icon to every tab in mainTabControl.
        /// Creates a 16×16 ImageList owned by the form (disposed with the form).
        /// Safe to call multiple times — replaces the previous ImageList.
        /// </summary>
        private void ApplyTabIcons()
        {
            var sz = IconGenerator.IconSize.Small;

            // Build a fresh ImageList every call (handles theme changes)
            var il = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize  = new Size(16, 16),
                TransparentColor = Color.Transparent
            };

            // Index 0 – Log
            il.Images.Add("log",        IconGenerator.CreateTabLogIcon(sz));
            // Index 1 – Raw
            il.Images.Add("raw",        IconGenerator.CreateTabRawIcon(sz));
            // Index 2 – Performance
            il.Images.Add("perf",       IconGenerator.CreateTabPerformanceIcon(sz));
            // Index 3 – Log Details
            il.Images.Add("details",    IconGenerator.CreateTabLogDetailsIcon(sz));
            // Index 4 – Call Graph
            il.Images.Add("callgraph",  IconGenerator.CreateTabCallGraphIcon(sz));
            // Index 5 – Flame Graph
            il.Images.Add("flame",      IconGenerator.CreateTabFlameGraphIcon(sz));
            // Index 6 – Timeline
            il.Images.Add("timeline",   IconGenerator.CreateTabTimelineIcon(sz));
            // Index 7 – AI Assistant
            il.Images.Add("ai",         IconGenerator.CreateTabAiIcon(sz));
            // Index 8 – generic fallback
            il.Images.Add("generic",    IconGenerator.CreateTabIcon(sz));

            // Dispose the old ImageList before replacing it
            var oldIl = mainTabControl.ImageList;
            mainTabControl.ImageList = il;
            oldIl?.Dispose();

            // Assign by key so order-independence is guaranteed
            logTab.ImageKey         = "log";
            rawTab.ImageKey         = "raw";
            performanceTab.ImageKey = "perf";
            logDetailTab.ImageKey   = "details";
            callGraphTab.ImageKey   = "callgraph";
            flameGraphTab.ImageKey  = "flame";
            timelineTab.ImageKey    = "timeline";

            // Dynamic tabs added at runtime
            if (_aiTab != null) _aiTab.ImageKey = "ai";
        }

        private static Color GetLineColour(string line)
        {
            // Use the actual log level code (2nd colon-separated field: E=Error, W=Warning)
            if (string.IsNullOrEmpty(line)) return ThemeManager.BackgroundColor;
            // Format: "{datetime}: {Level}: ..."  — level is always at index 1 after ": " split
            int first = line.IndexOf(": ", StringComparison.Ordinal);
            if (first >= 0 && first + 3 < line.Length)
            {
                char level = line[first + 2];
                if (level == 'E') return ThemeManager.ErrorBackgroundColor;
                if (level == 'W') return ThemeManager.WarningBackgroundColor;
            }
            return ThemeManager.BackgroundColor;
        }

        // Issue Fix: Auto-resize ListView columns to fit content
        private void AutoResizeLogListColumns()
        {
            if (logListView == null || logListView.Columns.Count < 2) return;

            logListView.Columns[0].Width = 80; // Fixed width for line numbers

            int viewWidth = logListView.ClientSize.Width - logListView.Columns[0].Width - SystemInformation.VerticalScrollBarWidth;

            // colLogText must be at least as wide as the widest content line so that
            // the ListView shows a horizontal scrollbar when lines overflow the view.
            // It may also grow to fill the view when content is narrower.
            logListView.Columns[1].Width = Math.Max(viewWidth, _logTextColumnContentWidth);
        }

        /// <summary>
        /// Finds the widest log line and stores its pixel width in
        /// <see cref="_logTextColumnContentWidth"/> so the log column scrollbar is correct.
        ///
        /// P-04: previous implementation called TextRenderer.MeasureText up to 10 000 times
        /// (one GDI call per sample).  The new approach runs a single O(N) pass to find the
        /// longest line by character count (cheap string-length comparison), then makes
        /// exactly ONE GDI call to measure that one line.  For monospace fonts (Consolas)
        /// the longest character-count line is also the widest in pixels.
        /// </summary>
        private void MeasureAndStoreMaxLineWidth()
        {
            _logTextColumnContentWidth = 0;
            if (_virtualLines == null || _virtualLines.Count == 0) return;

            // Find the line with the most characters — O(N), no GDI calls.
            string widest = null;
            int maxLen = 0;
            for (int i = 0; i < _virtualLines.Count; i++)
            {
                string t = _virtualLines[i].Text;
                if (t != null && t.Length > maxLen) { maxLen = t.Length; widest = t; }
            }

            if (widest == null) return;

            // Single GDI call for the one widest line.
            _logTextColumnContentWidth = TextRenderer.MeasureText(widest, logListView.Font).Width + 12;
        }

        private void ShowLoadError(string filePath, string reason, string detail)
        {
            SetDocumentLoaded(false);
            FileStatus.Image = IconGenerator.CreateStatusErrorIcon(IconGenerator.IconSize.Small);
            MessageBox.Show(
                string.Format("{0}:\n{1}\n\nFile: {2}", reason, detail, filePath),
                Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void OnFileChangedOnDisk(object sender, EventArgs e)
        {
            if (!_isLoading) FileStatus.Image = IconGenerator.CreateStatusErrorIcon(IconGenerator.IconSize.Small);
        }

        // ── UI state ──────────────────────────────────────────────────────────
        private void SetDocumentLoaded(bool loaded)
        {
            saveAsMenuItem.Enabled   = SaveButton.Enabled    = loaded;
            reloadMenuItem.Enabled   = RefreshButton.Enabled = loaded;
            copyMenuItem.Enabled     = CopyButton.Enabled    = loaded;
            findMenuItem.Enabled     = findNextMenuItem.Enabled
                                     = FindButton.Enabled    = loaded;
            filterMenuItem.Enabled   = FilterButton.Enabled  = loaded;
            FileStatus.Enabled       = loaded;
            FileLoadProgress.Enabled = loaded;

            // Feature I1: Enable export filtered logs menu item (will be added to designer)
            if (exportFilteredLogsMenuItem != null)
                exportFilteredLogsMenuItem.Enabled = loaded;
        }

        // ── File menu ─────────────────────────────────────────────────────────

        // ── Path helpers (PathTooLongException guard) ─────────────────────────
        // _currentFilePath can be a synthetic display string such as
        //   "[Merged: file1.log, file2.log, ...]"
        // which is not a real filesystem path and can exceed 260 characters.
        // Calling Path.GetDirectoryName / GetFileName on it throws
        // PathTooLongException on .NET Framework 4.8.  All three helpers below
        // return a safe fallback whenever the path is virtual or too long.

        private static string GetSafeDirectory(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (path.StartsWith("[") || path.Length >= 260) return null;
            try { return Path.GetDirectoryName(path); }
            catch { return null; }
        }

        private static string GetSafeBaseName(string path)
        {
            if (string.IsNullOrEmpty(path)) return "log";
            if (path.StartsWith("[") || path.Length >= 260) return "merged";
            try { return Path.GetFileNameWithoutExtension(path); }
            catch { return "log"; }
        }

        private static string GetSafeFileName(string path)
        {
            if (string.IsNullOrEmpty(path)) return "log";
            if (path.StartsWith("[") || path.Length >= 260) return "merged";
            try { return Path.GetFileName(path); }
            catch { return "log"; }
        }

        private void openMenuItem_Click(object sender, EventArgs e)
        {
            if (openLogFileDialog.ShowDialog() == DialogResult.OK)
                LoadFileAsync(openLogFileDialog.FileName);
        }

        private void OpenButton_Click(object sender, EventArgs e) =>
            openMenuItem_Click(sender, e);

        private void saveAsMenuItem_Click(object sender, EventArgs e)
        {
            // Save the ENTER→EXIT block for the currently selected tree node.
            // Default filename: {original-basename}{snippet-suffix}.log
            TreeView activeTree = CallTreeButton.Checked ? CallTree : ApiTree;
            if (activeTree?.SelectedNode == null)
            {
                MessageBox.Show("Please select a node in the tree first.",
                    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            TreeNode node       = activeTree.SelectedNode;
            string methodName   = GetMethodNameFromNode(node);
            int    enterLine    = (node.Tag is int t && t > 0) ? t : -1;
            List<string> lines  = ExtractBranchLines(enterLine, methodName);

            if (lines.Count == 0)
            {
                MessageBox.Show(string.Format(Resources.ERR_NO_ENTER_EXIT_PAIR, methodName),
                    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string baseName    = string.IsNullOrEmpty(_currentFilePath)
                ? methodName.Replace("::", "_")
                : GetSafeBaseName(_currentFilePath);
            string defaultName = baseName + (_appSettings.SaveSnippetSuffix ?? "_snippet") + ".log";

            using (var dlg = new SaveFileDialog())
            {
                dlg.Title            = Resources.DIALOG_TITLE_SAVE_BRANCH ?? "Save Selected Branch";
                dlg.Filter           = Resources.FILE_FILTER_LOG_SAVE;
                dlg.FileName         = defaultName;
                dlg.InitialDirectory = string.IsNullOrEmpty(_currentFilePath)
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : GetSafeDirectory(_currentFilePath);

                if (dlg.ShowDialog() != DialogResult.OK) return;

                try
                {
                    // Show status-bar progress while writing (visible for large branches)
                    FileLoadProgress.Style   = ProgressBarStyle.Blocks;
                    FileLoadProgress.Visible = true;
                    FileLoadProgress.Value   = 0;
                    StatusFileName.Text      = string.Format("Saving {0} lines...", lines.Count);

                    File.WriteAllLines(dlg.FileName, lines);

                    FileLoadProgress.Value = 100;
                    MessageBox.Show(
                        string.Format(Resources.MSG_BRANCH_SAVED_TO, lines.Count, dlg.FileName),
                        Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(Resources.ERR_SAVE_BRANCH_FAILED, ex.Message),
                        Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    FileLoadProgress.Visible = false;
                    FileLoadProgress.Value   = 0;
                    UpdateStatusBar();
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e) =>
            saveAsMenuItem_Click(sender, e);

        private void refreshMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            // D-01: capture scroll position BEFORE the async load, then restore it
            // inside LoadFileAsync once PopulateVirtualListView has run — not via
            // BeginInvoke which fires before the async method completes.
            int topIndex = logListView.TopItem != null ? logListView.TopItem.Index : 0;
            LoadFileAsync(_currentFilePath, restoreTopIndex: topIndex);
        }

        private void RefreshButton_Click(object sender, EventArgs e) =>
            refreshMenuItem_Click(sender, e);

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e) =>
            refreshMenuItem_Click(sender, e);

        private void reloadMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentFilePath))
                LoadFileAsync(_currentFilePath);
        }

        // A6: Merge multiple log files (time-sorted)
        private async void mergeLogsMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title       = Resources.FILE_DIALOG_SELECT_LOGS_TO_MERGE;
                dlg.Filter      = Resources.FILE_DIALOG_FILTER_LOGS;
                dlg.Multiselect = true;
                dlg.InitialDirectory = openLogFileDialog.InitialDirectory;

                if (dlg.ShowDialog() != DialogResult.OK || dlg.FileNames.Length < 2)
                {
                    if (dlg.FileNames.Length == 1)
                    {
                        MessageBox.Show(Resources.MSG_SELECT_TWO_FILES_TO_MERGE,
                            Resources.DIALOG_TITLE_MERGE_LOGS, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return;
                }

                // Feature A6: Merge logs implementation
                StartOperation(string.Format(Resources.OPERATION_MERGING_FILES, dlg.FileNames.Length));

                try
                {
                    // Step 1 of 5: Read and sort files (background)
                    UpdateStatusProgress(5, string.Format("Reading {0} files...", dlg.FileNames.Length));
                    var merged = await _mergeLogService.MergeAsync(dlg.FileNames);

                    // Update current file path to show merged state
                    _currentFilePath = "[Merged: " + string.Join(", ",
                        System.Array.ConvertAll(dlg.FileNames,
                            p => System.IO.Path.GetFileName(p))) + "]";
                    // Track real paths so subsequent drop-merges can re-merge from disk
                    _mergedSourcePaths = new List<string>(dlg.FileNames);

                    _allLines = merged;
                    _searchService.Reset();
                    ClearHighlighting();

                    // Step 2 of 5: Populate log list
                    UpdateStatusProgress(25, Resources.STATUS_PROCESSING_MERGED_DATA);
                    await Task.Delay(10);
                    PopulateVirtualListView(_allLines);

                    // Step 3 of 5: Parse log entries
                    UpdateStatusProgress(40, Resources.STATUS_BUILDING_MERGED_TREE);
                    await Task.Delay(10);
                    var mergeEntries  = await Task.Run(() => _parserService.Parse(_allLines));

                    // Step 4 of 5: Build trees and graphs in parallel
                    UpdateStatusProgress(60, "Building call trees and graphs...");
                    var mApiTask  = Task.Run(() => _parserService.BuildApiList(mergeEntries));
                    var mCallTask = Task.Run(() => _parserService.BuildCallTreeGroupedByFile(mergeEntries));
                    await Task.WhenAll(mApiTask, mCallTask);
                    var mCallTree  = mCallTask.Result;
                    var mPerfStats = await Task.Run(() => _parserService.BuildPerformanceStatsGroupedByFile(mergeEntries));

                    // Step 5 of 5: Build call graph and populate UI
                    UpdateStatusProgress(80, "Building call graph...");
                    var mFileGraphs = await Task.Run(() => _callGraphService.BuildGroupedByFile(mergeEntries));
                    var mGraph      = mFileGraphs.Count == 1 ? mFileGraphs[0].Graph : _callGraphService.Build(mergeEntries);

                    UpdateStatusProgress(95, "Populating views...");
                    PopulateTreesFromData(mergeEntries, mApiTask.Result, mCallTree, mPerfStats, mGraph, mFileGraphs);
                    UpdateStatusProgress(100, "Merge complete.");

                    SetDocumentLoaded(true);
                    FileStatus.Image = IconGenerator.CreateStatusOkIcon(IconGenerator.IconSize.Small);
                    UpdateStatusBar();

                    MessageBox.Show(
                        string.Format(Resources.MSG_MERGE_SUCCESSFUL,
                            dlg.FileNames.Length, merged.Count),
                        Resources.DIALOG_TITLE_MERGE_COMPLETE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        string.Format(Resources.MSG_MERGE_FAILED, ex.Message),
                        Resources.DIALOG_TITLE_MERGE_LOGS, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    EndOperation();
                }
            }
        }

        private void exitMenuItem_Click(object sender, EventArgs e) => Close();

        // ── Edit menu ─────────────────────────────────────────────────────────
        // MOVED: copyMenuItem_Click is now in Feature 1 section below

        // ── Feature I1: Export Filtered Logs ──────────────────────────────────
        private void exportFilteredLogsMenuItem_Click(object sender, EventArgs e)
        {
            // Export the Performance tab statistics to a SpreadsheetML (.xls) file.
            if (_apiPerfStats == null || _apiPerfStats.Count == 0)
            {
                MessageBox.Show("No performance data to export. Load a log file first.",
                    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string baseName = string.IsNullOrEmpty(_currentFilePath)
                ? "performance"
                : GetSafeBaseName(_currentFilePath);

            using (var dlg = new SaveFileDialog())
            {
                dlg.Title  = "Export Performance to XLS";
                dlg.Filter = "Excel Workbook (*.xls)|*.xls|All files (*.*)|*.*";
                dlg.FileName = baseName + "_performance.xls";
                dlg.InitialDirectory = string.IsNullOrEmpty(_currentFilePath)
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : GetSafeDirectory(_currentFilePath);

                if (dlg.ShowDialog() != DialogResult.OK) return;

                try
                {
                    WritePerformanceXls(dlg.FileName, _apiPerfStats);
                    MessageBox.Show(
                        $"Performance data exported to:\n{dlg.FileName}\n\n{_apiPerfStats.Count} rows written.",
                        Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Export failed: {ex.Message}", Resources.TITLE,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Writes performance statistics to a SpreadsheetML XML file readable by Excel 97-2003+.
        /// No external library required — pure XML.
        /// </summary>
        private static void WritePerformanceXls(string path, IList<ApiPerfStats> stats)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
            sb.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
            sb.AppendLine("  xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");
            sb.AppendLine(" <Styles>");
            sb.AppendLine("  <Style ss:ID=\"Header\"><Font ss:Bold=\"1\"/><Interior ss:Color=\"#17375E\" ss:Pattern=\"Solid\"/><Font ss:Color=\"#FFFFFF\" ss:Bold=\"1\"/></Style>");
            sb.AppendLine("  <Style ss:ID=\"Red\"><Interior ss:Color=\"#FFCCCC\" ss:Pattern=\"Solid\"/></Style>");
            sb.AppendLine("  <Style ss:ID=\"Amber\"><Interior ss:Color=\"#FFF3CC\" ss:Pattern=\"Solid\"/></Style>");
            sb.AppendLine(" </Styles>");
            sb.AppendLine(" <Worksheet ss:Name=\"Performance\">");
            sb.AppendLine("  <Table>");

            // Header row
            sb.AppendLine("   <Row>");
            foreach (string h in new[] { "API Name", "Calls", "Total (ms)", "Avg (ms)", "Min (ms)", "Max (ms)", "Self (ms)", "Source File" })
            {
                sb.AppendLine($"    <Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">{XmlEsc(h)}</Data></Cell>");
            }
            sb.AppendLine("   </Row>");

            // Data rows
            foreach (var s in stats)
            {
                string style = s.TotalDurationMs >= 1000 ? "Red" :
                               s.TotalDurationMs >= 100  ? "Amber" : "";
                string sAttr = string.IsNullOrEmpty(style) ? "" : $" ss:StyleID=\"{style}\"";
                sb.AppendLine("   <Row>");
                Cell(sb, "String",  s.ApiName,                    sAttr);
                Cell(sb, "Number",  s.CallCount.ToString(),        sAttr);
                Cell(sb, "Number",  s.TotalDurationMs.ToString(),  sAttr);
                Cell(sb, "Number",  s.AvgDurationMs.ToString(),    sAttr);
                Cell(sb, "Number",  (s.MinDurationMs < 0 ? 0 : s.MinDurationMs).ToString(), sAttr);
                Cell(sb, "Number",  s.MaxDurationMs.ToString(),    sAttr);
                Cell(sb, "Number",  s.SelfDurationMs.ToString(),   sAttr);
                Cell(sb, "String",  s.SourceFile ?? "",            sAttr);
                sb.AppendLine("   </Row>");
            }

            sb.AppendLine("  </Table>");
            sb.AppendLine(" </Worksheet>");
            sb.AppendLine("</Workbook>");

            File.WriteAllText(path, sb.ToString(), System.Text.Encoding.UTF8);
        }

        private static void Cell(System.Text.StringBuilder sb, string type, string value, string styleAttr)
        {
            sb.AppendLine($"    <Cell{styleAttr}><Data ss:Type=\"{type}\">{XmlEsc(value)}</Data></Cell>");
        }

        private static string XmlEsc(string s) =>
            string.IsNullOrEmpty(s) ? "" :
            s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

        // MOVED: CopyButton_Click is now in Feature 1 section below

        // MOVED: contextCopyWithHeadersMenuItem_Click is now in Feature 3 section below

        private FindForm _findForm;

        private void findMenuItem_Click(object sender, EventArgs e)
        {
            if (_findForm == null || _findForm.IsDisposed)
            {
                _findForm = new FindForm(this);
                _findForm.Left = Right - _findForm.Width - 20;
                _findForm.Top  = Top + 80;
            }
            _findForm.Show();
            _findForm.BringToFront();
        }

        private void FindButton_Click(object sender, EventArgs e) =>
            findMenuItem_Click(sender, e);

        private void FindNextButton_Click(object sender, EventArgs e)
        {
            if (_findForm != null)
                _findForm.TriggerFindNext();
        }

        // Feature B8: Highlight search results
        private string _lastHighlightTerm = "";
        private bool _lastHighlightMatchCase = false;

        public void FindNext(string searchTerm, bool matchCase, bool useRegex = false)
        {
            if (_virtualLines.Count == 0 || string.IsNullOrEmpty(searchTerm)) return;

            // P-06: use the zero-alloc wrapper instead of copying all text into a new List<string>.
            if (_virtualLineTexts == null) _virtualLineTexts = new VirtualLineTextList(_virtualLines);
            int idx = _searchService.FindNext(_virtualLineTexts, searchTerm, matchCase, useRegex);
            if (idx >= 0)
            {
                // Feature B8: Apply highlighting when search term changes
                if (searchTerm != _lastHighlightTerm || matchCase != _lastHighlightMatchCase)
                {
                    HighlightSearchResults(searchTerm, matchCase, useRegex);
                    _lastHighlightTerm = searchTerm;
                    _lastHighlightMatchCase = matchCase;
                }

                logListView.SelectedIndices.Clear();
                logListView.SelectedIndices.Add(idx);
                logListView.EnsureVisible(idx);
                ShowLogDetail(idx);
            }
            else
            {
                MessageBox.Show(string.Format(Resources.ERR_NOT_FOUND, searchTerm),
                    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Feature B8: Highlight all search results in the log view
        private void HighlightSearchResults(string searchTerm, bool matchCase, bool useRegex = false)
        {
            if (string.IsNullOrEmpty(searchTerm)) { ClearHighlighting(); return; }
            if (_virtualLines == null || _virtualLines.Count == 0) return;

            try
            {
                // Pre-compile regex once outside the loop (Bug #11 fix).
                System.Text.RegularExpressions.Regex rx = null;
                StringComparison comparison = matchCase
                    ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

                if (useRegex)
                {
                    var opts = matchCase
                        ? System.Text.RegularExpressions.RegexOptions.None
                        : System.Text.RegularExpressions.RegexOptions.IgnoreCase;
                    rx = new System.Text.RegularExpressions.Regex(searchTerm, opts);
                }

                Color highlight = _appSettings.HighlightColor;

                for (int i = 0; i < _virtualLines.Count; i++)
                {
                    var vl = _virtualLines[i];

                    bool matches = rx != null
                        ? rx.IsMatch(vl.Text)
                        : vl.Text.IndexOf(searchTerm, comparison) >= 0;

                    // Bug #10: only overwrite the background when the line actually
                    // matches — leave bookmarked (blue) and level-coloured lines alone.
                    if (matches)
                    {
                        _virtualLines[i] = new VirtualLogLine
                        {
                            LineNumber = vl.LineNumber,
                            Text       = vl.Text,
                            BackColour = highlight
                        };
                    }
                }

                logListView.Invalidate();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(string.Format(Resources.ERR_INVALID_REGEX, ex.Message),
                    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ClearHighlighting()
        {
            if (_virtualLines == null || _virtualLines.Count == 0)
            {
                _lastHighlightTerm = "";
                return;
            }

            // Bug #10: restore each line to either its bookmark colour or its level colour
            // so that bookmarked lines keep their highlight after a search is cleared.
            Color bookmarkColour = ThemeManager.CurrentTheme == ThemeManager.Theme.Dark
                ? Color.FromArgb(0, 70, 130)
                : Color.FromArgb(200, 230, 255);

            for (int i = 0; i < _virtualLines.Count; i++)
            {
                var vl = _virtualLines[i];
                Color back = _bookmarkService.IsBookmarked(vl.LineNumber)
                    ? bookmarkColour
                    : GetLineColour(vl.Text);

                _virtualLines[i] = new VirtualLogLine
                {
                    LineNumber = vl.LineNumber,
                    Text       = vl.Text,
                    BackColour = back
                };
            }

            _lastHighlightTerm = "";
            if (logListView != null) logListView.Invalidate();
        }

        private void findNextMenuItem_Click(object sender, EventArgs e)
        {
            if (_findForm != null && !_findForm.IsDisposed)
                _findForm.TriggerFindNext();
            else
                findMenuItem_Click(sender, e);
        }

        // ── B9: Jump to matching ENTER/EXIT ───────────────────────────────────
        public void JumpToMatchingPair()
        {
            if (_virtualLines.Count == 0 || _lastEntries.Count == 0) return;
            if (logListView.SelectedIndices.Count == 0) return;

            int selectedIdx  = logListView.SelectedIndices[0];
            int selectedLine = _virtualLines[selectedIdx].LineNumber;

            // Find the entry at this line
            LogEntry current = null;
            foreach (var e in _lastEntries)
                if (e.LineNumber == selectedLine && e.IsApiCall) { current = e; break; }

            if (current == null) { MessageBox.Show(Resources.MSG_NOT_API_CALL, Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            int targetLine = -1;
            if (current.IsCallEnter)
            {
                // Find matching EXIT
                int depth = 0;
                foreach (var e in _lastEntries)
                {
                    if (e.LineNumber <= selectedLine) { if (e.IsApiCall && e.ApiName == current.ApiName && e.IsCallEnter) depth++; continue; }
                    if (!e.IsApiCall || e.ApiName != current.ApiName) continue;
                    if (e.IsCallEnter) depth++;
                    else if (e.IsCallExit) { depth--; if (depth == 0) { targetLine = e.LineNumber; break; } }
                }
            }
            else if (current.IsCallExit)
            {
                // Find matching ENTER by going backwards
                int depth = 0;
                for (int i = _lastEntries.Count - 1; i >= 0; i--)
                {
                    var e = _lastEntries[i];
                    if (e.LineNumber >= selectedLine) { if (e.IsApiCall && e.ApiName == current.ApiName && e.IsCallExit) depth++; continue; }
                    if (!e.IsApiCall || e.ApiName != current.ApiName) continue;
                    if (e.IsCallExit) depth++;
                    else if (e.IsCallEnter) { depth--; if (depth == 0) { targetLine = e.LineNumber; break; } }
                }
            }

            if (targetLine > 0)
                ScrollLogToLine(targetLine);
            else
                MessageBox.Show(Resources.MSG_NO_MATCHING_PAIR, Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Operation Progress and Cancellation Support ───────────────────────
        private void StartOperation(string operationName)
        {
            _currentOperation = operationName;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            // Show progress bar and update status
            FileLoadProgress.Style = ProgressBarStyle.Marquee;
            FileLoadProgress.Visible = true;
            StatusFileName.Text = string.Format(Resources.STATUS_OPERATION_IN_PROGRESS, operationName);

            // Disable menu items during operation (must be before DoEvents to block re-entrancy)
            SetOperationInProgress(true);

            // Show centred overlay and flush all pending paint messages so the overlay
            // is physically visible on screen before any synchronous work begins.
            _overlay.Show(operationName);
            Application.DoEvents();
        }

        /// <summary>
        /// Updates the status-bar progress bar with a deterministic percentage and message.
        /// Switches FileLoadProgress to Blocks mode on the first call so the user sees
        /// actual progress instead of a marquee stripe.
        /// </summary>
        private void UpdateStatusProgress(int percent, string message)
        {
            if (FileLoadProgress.Style != ProgressBarStyle.Blocks)
                FileLoadProgress.Style = ProgressBarStyle.Blocks;

            FileLoadProgress.Value = Math.Max(0, Math.Min(100, percent));
            StatusFileName.Text    = message;
            _overlay.SetProgress(percent, message);
        }

        private void EndOperation()
        {
            FileLoadProgress.Visible = false;
            FileLoadProgress.Style = ProgressBarStyle.Blocks;
            StatusFileName.Text = string.Empty;
            _currentOperation = string.Empty;

            // Hide centred overlay
            _overlay.Hide();

            // Re-enable menu items
            SetOperationInProgress(false);

            UpdateStatusBar();
        }

        private void CancelCurrentOperation()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                StatusFileName.Text = string.Format(Resources.STATUS_OPERATION_CANCELLED, _currentOperation);
            }
        }

        private void StatusFileName_Click(object sender, EventArgs e)
        {
            // Allow users to click status bar to cancel operations
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                CancelCurrentOperation();
            }
        }

        private void SetOperationInProgress(bool inProgress)
        {
            // Disable/enable menu items that could conflict with operations
            expandAllMenuItem.Enabled = !inProgress;
            collapseAllMenuItem.Enabled = !inProgress;
            filterMenuItem.Enabled = !inProgress;
            findMenuItem.Enabled = !inProgress;
            findNextMenuItem.Enabled = !inProgress;
            saveAsMenuItem.Enabled = !inProgress;
            exportFilteredLogsMenuItem.Enabled = !inProgress;
            openMenuItem.Enabled = !inProgress;
            reloadMenuItem.Enabled = !inProgress;

            // Disable/enable corresponding toolbar buttons
            ExpandAllButton.Enabled = !inProgress;
            CollapseAllButton.Enabled = !inProgress;
            FilterButton.Enabled = !inProgress;
            FindButton.Enabled = !inProgress;
            FindNextButton.Enabled = !inProgress;
            SaveButton.Enabled = !inProgress;
            SaveToXLSButton.Enabled = !inProgress;
            OpenButton.Enabled = !inProgress;
            RefreshButton.Enabled = !inProgress;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Handle ESC key to cancel operations
            if (keyData == Keys.Escape && _cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                CancelCurrentOperation();
                return true;
            }

            // Handle error/warning navigation shortcuts
            switch (keyData)
            {
                case Keys.Control | Keys.T:                      // Toggle Theme
                    ThemeToggleButton_Click(this, EventArgs.Empty);
                    return true;

                case Keys.F12:                                   // Inspect selected line
                    InspectSelectedLine();
                    return true;

                case Keys.F8:                                    // Next Error
                    NavigateToNextError();
                    return true;
                case Keys.Shift | Keys.F8:                       // Previous Error
                    NavigateToPreviousError();
                    return true;
                case Keys.Control | Keys.F8:                     // Next Warning
                    NavigateToNextWarning();
                    return true;
                case Keys.Control | Keys.Shift | Keys.F8:       // Previous Warning
                    NavigateToPreviousWarning();
                    return true;

                // Bookmark shortcuts
                case Keys.Control | Keys.B:                      // Toggle Bookmark
                    ToggleBookmarkOnCurrentLine();
                    return true;
                case Keys.F2:                                    // Next Bookmark
                    NavigateToNextBookmark();
                    return true;
                case Keys.Shift | Keys.F2:                       // Previous Bookmark
                    NavigateToPreviousBookmark();
                    return true;
                case Keys.Control | Keys.Shift | Keys.B:        // Show Bookmarks
                    ShowBookmarkList();
                    return true;
                case Keys.Control | Keys.Shift | Keys.Delete:   // Clear All Bookmarks
                    ClearAllBookmarks();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ── C1: Expand / Collapse all ─────────────────────────────────────────
        public void ExpandAllTrees()
        {
            StartOperation(Resources.OPERATION_EXPANDING_ALL_NODES);

            try
            {
                var token = _cancellationTokenSource.Token;
                int count = 0;

                CallTree.BeginUpdate();
                foreach (var n in CollectAllNodes(CallTree.Nodes))
                {
                    token.ThrowIfCancellationRequested();
                    n.Expand();
                    if (++count % 100 == 0) Application.DoEvents();
                }
                CallTree.EndUpdate();

                token.ThrowIfCancellationRequested();

                count = 0;
                ApiTree.BeginUpdate();
                foreach (var n in CollectAllNodes(ApiTree.Nodes))
                {
                    token.ThrowIfCancellationRequested();
                    n.Expand();
                    if (++count % 100 == 0) Application.DoEvents();
                }
                ApiTree.EndUpdate();
            }
            catch (OperationCanceledException)
            {
                StatusFileName.Text = Resources.STATUS_EXPAND_CANCELLED;
            }
            finally
            {
                EndOperation();
            }
        }

        private List<TreeNode> CollectAllNodes(TreeNodeCollection nodes)
        {
            var list = new List<TreeNode>();
            CollectAllNodesRecursive(nodes, list);
            return list;
        }

        private void CollectAllNodesRecursive(TreeNodeCollection nodes, List<TreeNode> list)
        {
            foreach (TreeNode node in nodes)
            {
                list.Add(node);
                if (node.Nodes.Count > 0)
                    CollectAllNodesRecursive(node.Nodes, list);
            }
        }

        public async void CollapseAllTrees()
        {
            StartOperation(Resources.OPERATION_COLLAPSING_ALL_NODES);
            try
            {
                var token = _cancellationTokenSource.Token;

                UpdateStatusProgress(10, "Collapsing Call Tree...");
                CallTree.BeginUpdate();
                CallTree.CollapseAll();
                foreach (TreeNode n in CallTree.Nodes) n.Expand();
                CallTree.EndUpdate();

                await System.Threading.Tasks.Task.Yield(); // let status bar repaint
                token.ThrowIfCancellationRequested();

                UpdateStatusProgress(60, "Collapsing API Tree...");
                ApiTree.BeginUpdate();
                ApiTree.CollapseAll();
                foreach (TreeNode n in ApiTree.Nodes) n.Expand();
                ApiTree.EndUpdate();

                await System.Threading.Tasks.Task.Yield();
                UpdateStatusProgress(100, "Collapsed.");
            }
            catch (OperationCanceledException)
            {
                StatusFileName.Text = Resources.STATUS_COLLAPSE_CANCELLED;
            }
            finally
            {
                EndOperation();
            }
        }

        // Feature C1: Menu event handlers
        private void expandAllMenuItem_Click(object sender, EventArgs e) =>
            ExpandAllTrees();

        private void collapseAllMenuItem_Click(object sender, EventArgs e) =>
            CollapseAllTrees();

        private void jumpToMatchingMenuItem_Click(object sender, EventArgs e) =>
            JumpToMatchingPair();

        // ── Feature B10: Error/Warning Navigation ─────────────────────────────
        public void NavigateToNextError()
        {
            if (_errorLines.Count == 0)
            {
                MessageBox.Show(Resources.MSG_NO_ERRORS, Resources.TITLE, 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _currentErrorIndex = (_currentErrorIndex + 1) % _errorLines.Count;
            int lineIdx = _errorLines[_currentErrorIndex];
            logListView.EnsureVisible(lineIdx);
            logListView.SelectedIndices.Clear();
            logListView.SelectedIndices.Add(lineIdx);
            logListView.Focus();
        }

        public void NavigateToPreviousError()
        {
            if (_errorLines.Count == 0)
            {
                MessageBox.Show(Resources.MSG_NO_ERRORS, Resources.TITLE, 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _currentErrorIndex--;
            if (_currentErrorIndex < 0) _currentErrorIndex = _errorLines.Count - 1;
            int lineIdx = _errorLines[_currentErrorIndex];
            logListView.EnsureVisible(lineIdx);
            logListView.SelectedIndices.Clear();
            logListView.SelectedIndices.Add(lineIdx);
            logListView.Focus();
        }

        public void NavigateToNextWarning()
        {
            if (_warningLines.Count == 0)
            {
                MessageBox.Show(Resources.MSG_NO_WARNINGS, Resources.TITLE, 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _currentWarningIndex = (_currentWarningIndex + 1) % _warningLines.Count;
            int lineIdx = _warningLines[_currentWarningIndex];
            logListView.EnsureVisible(lineIdx);
            logListView.SelectedIndices.Clear();
            logListView.SelectedIndices.Add(lineIdx);
            logListView.Focus();
        }

        public void NavigateToPreviousWarning()
        {
            if (_warningLines.Count == 0)
            {
                MessageBox.Show(Resources.MSG_NO_WARNINGS, Resources.TITLE, 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _currentWarningIndex--;
            if (_currentWarningIndex < 0) _currentWarningIndex = _warningLines.Count - 1;
            int lineIdx = _warningLines[_currentWarningIndex];
            logListView.EnsureVisible(lineIdx);
            logListView.SelectedIndices.Clear();
            logListView.SelectedIndices.Add(lineIdx);
            logListView.Focus();
        }

        // Feature B10: Toolbar button click handlers
        private void prevErrorButton_Click(object sender, EventArgs e) =>
            NavigateToPreviousError();

        private void nextErrorButton_Click(object sender, EventArgs e) =>
            NavigateToNextError();

        private void prevWarningButton_Click(object sender, EventArgs e) =>
            NavigateToPreviousWarning();

        private void nextWarningButton_Click(object sender, EventArgs e) =>
            NavigateToNextWarning();

        public void ApplyFilter(string filterText, bool matchCase, int? minDuration = null, 
            DateTime? fromTime = null, DateTime? toTime = null)
        {
            var criteria = new Models.FilterCriteria
            {
                SearchText = filterText,
                IsCaseSensitive = matchCase,
                MinimumDurationMs = minDuration,
                FromTime = fromTime,
                ToTime = toTime
            };

            ApplyFilter(criteria);
        }

        public async void ApplyFilter(Models.FilterCriteria criteria)
        {
            if (criteria == null || !criteria.IsActive)
            {
                ClearFilter();
                return;
            }

            StartOperation(string.Format(Resources.OPERATION_FILTERING, _allLines.Count));

            try
            {
                var allLinesCopy = _allLines; // capture reference; immutable during filter

                // Duration filter: compute on the UI thread (uses _lastEntries which is
                // only written on the UI thread) before handing off to Task.Run.
                HashSet<int> durationQualifiedLines = criteria.MinimumDurationMs.HasValue
                    ? BuildDurationQualifiedLines(criteria.MinimumDurationMs.Value)
                    : null;

                var filtered = await Task.Run(() =>
                {
                    var token = _cancellationTokenSource.Token;
                    var result = new List<FilteredLine>();

                    for (int i = 0; i < allLinesCopy.Count; i++)
                    {
                        if (i % 1000 == 0)
                        {
                            token.ThrowIfCancellationRequested();
                            int progress = (int)((i / (double)allLinesCopy.Count) * 100);
                            this.Invoke((Action)(() =>
                            {
                                FileLoadProgress.Style = ProgressBarStyle.Blocks;
                                FileLoadProgress.Value = progress;
                                var msg = string.Format(Resources.STATUS_FILTERING_PROGRESS,
                                    progress, i, allLinesCopy.Count);
                                StatusFileName.Text = msg;
                                _overlay.SetProgress(progress, msg);
                            }));
                        }

                        string line = allLinesCopy[i];
                        if (MatchesFilterRaw(line, criteria, durationQualifiedLines, i + 1))
                            result.Add(new FilteredLine(i + 1, line));
                    }
                    return result;
                });

                _activeFilterText = criteria.GetDescription();
                PopulateVirtualListViewFiltered(filtered);
                ClearHighlighting();

                StatusFileName.Text = string.Format(Resources.STATUS_FILTER_APPLIED,
                    filtered.Count, _allLines.Count);
            }
            catch (OperationCanceledException)
            {
                StatusFileName.Text = Resources.STATUS_FILTER_CANCELLED;
                ClearFilter();
            }
            finally
            {
                EndOperation();
            }
        }

        // Feature B3: Clear filter and show all lines
        public void ClearFilter()
        {
            _activeFilterText = "";
            PopulateVirtualListView(_allLines);
            ClearHighlighting(); // Clear search highlights when filter is cleared
        }

        // ── Pre-compiled regex for ISO 8601 timestamp (supports both T and space separator) ──
        private static readonly System.Text.RegularExpressions.Regex _filterTimeRegex =
            new System.Text.RegularExpressions.Regex(
                @"(\d{4}-\d{2}-\d{2}[T ]\d{2}:\d{2}:\d{2}(?:[.,]\d{1,3})?Z?)",
                System.Text.RegularExpressions.RegexOptions.Compiled);

        private bool MatchesFilterRaw(string line, Models.FilterCriteria criteria,
            HashSet<int> durationQualifiedLines, int lineNumber1Based)
        {
            // ── Text filter — honours UseRegex and IsCaseSensitive ────────────
            if (!string.IsNullOrWhiteSpace(criteria.SearchText))
            {
                if (criteria.UseRegex)
                {
                    try
                    {
                        var opts = criteria.IsCaseSensitive
                            ? System.Text.RegularExpressions.RegexOptions.None
                            : System.Text.RegularExpressions.RegexOptions.IgnoreCase;
                        if (!System.Text.RegularExpressions.Regex.IsMatch(line, criteria.SearchText, opts))
                            return false;
                    }
                    catch (ArgumentException)
                    {
                        return false; // invalid regex pattern → exclude
                    }
                }
                else
                {
                    var comparison = criteria.IsCaseSensitive
                        ? StringComparison.Ordinal
                        : StringComparison.OrdinalIgnoreCase;
                    if (line.IndexOf(criteria.SearchText, comparison) < 0)
                        return false;
                }
            }

            // ── Duration filter — evaluated via pre-computed ENTER/EXIT pairs ─
            if (durationQualifiedLines != null)
            {
                if (!durationQualifiedLines.Contains(lineNumber1Based))
                    return false;
            }

            // ── Time-range filter — uses ISO 8601 timestamp in line header ────
            if (criteria.FromTime.HasValue || criteria.ToTime.HasValue)
            {
                var m = _filterTimeRegex.Match(line);
                if (!m.Success) return false;
                if (!DateTime.TryParse(m.Groups[1].Value, null,
                    System.Globalization.DateTimeStyles.RoundtripKind, out DateTime lineTime))
                    return false;
                var t = lineTime.TimeOfDay;
                if (criteria.FromTime.HasValue && t < criteria.FromTime.Value.TimeOfDay) return false;
                if (criteria.ToTime.HasValue   && t > criteria.ToTime.Value.TimeOfDay)   return false;
            }

            // ── ThreadId filter ───────────────────────────────────────────────
            // Log format (field 3, 0-based, split on ": "):
            //   {datetime}: {Level}: {PID}: {TID}: {App}: {Area}: {payload}
            if (!string.IsNullOrWhiteSpace(criteria.ThreadId))
            {
                string tid = ParseRawThreadId(line);
                if (!string.Equals(tid, criteria.ThreadId.Trim(), StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            // ── Log level filter ──────────────────────────────────────────────
            if (criteria.Level.HasValue)
            {
                if (ParseLogLevel(line) != criteria.Level.Value)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Extracts the Thread-ID from the 4th colon-separated field of a raw log line.
        /// Handles merged log lines that start with "[filename] ".
        /// </summary>
        private static string ParseRawThreadId(string line)
        {
            if (string.IsNullOrEmpty(line)) return null;

            // Strip merged-log prefix "[filename] " so field positions are correct.
            string parseable = line;
            if (line.Length > 2 && line[0] == '[')
            {
                int cb = line.IndexOf("] ", StringComparison.Ordinal);
                if (cb > 0)
                {
                    string tag = line.Substring(1, cb - 1);
                    if (tag.IndexOf('.') >= 0) // looks like a filename
                        parseable = line.Substring(cb + 2);
                }
            }

            // Split on ": " — TID is field[3]
            var parts = parseable.Split(new[] { ": " }, 5, StringSplitOptions.None);
            return parts.Length >= 4 ? parts[3].Trim() : null;
        }

        /// <summary>
        /// Parses log level from a log line.
        /// </summary>
        private Models.LogLevel ParseLogLevel(string line)
        {
            if (string.IsNullOrEmpty(line))
                return Models.LogLevel.Info;

            // Look for log level indicator (2nd field after first colon)
            int first = line.IndexOf(": ", StringComparison.Ordinal);
            if (first >= 0 && first + 3 < line.Length)
            {
                char level = line[first + 2];
                switch (level)
                {
                    case 'E': return Models.LogLevel.Error;
                    case 'W': return Models.LogLevel.Warning;
                    case 'I': return Models.LogLevel.Info;
                    case 'D': return Models.LogLevel.Debug;
                }
            }

            return Models.LogLevel.Info;
        }

        /// <summary>
        /// Walks the already-parsed <see cref="_lastEntries"/> list and returns a set of
        /// 1-based line numbers for ENTER lines whose matched call duration is at or above
        /// <paramref name="minDurationMs"/>.  Must be called on the UI thread.
        /// </summary>
        private HashSet<int> BuildDurationQualifiedLines(long minDurationMs)
        {
            var result = new HashSet<int>();
            if (_lastEntries == null || _lastEntries.Count == 0) return result;

            var stack = new Stack<Services.LogEntry>();
            foreach (var entry in _lastEntries)
            {
                if (!entry.IsApiCall) continue;

                if (entry.IsCallEnter)
                {
                    stack.Push(entry);
                }
                else if (entry.IsCallExit && stack.Count > 0)
                {
                    // Walk the stack to find the matching ENTER (handles unmatched pairs).
                    var temp = new Stack<Services.LogEntry>();
                    bool found = false;
                    while (stack.Count > 0 && !found)
                    {
                        var top = stack.Pop();
                        if (top.ApiName == entry.ApiName)
                        {
                            // Compute duration from EpochMs timestamps.
                            if (top.EpochMs > 0 && entry.EpochMs >= top.EpochMs)
                            {
                                long durationMs = entry.EpochMs - top.EpochMs;
                                if (durationMs >= minDurationMs)
                                    result.Add(top.LineNumber);
                            }
                            found = true;
                        }
                        else
                        {
                            temp.Push(top);
                        }
                    }
                    // Restore non-matching frames.
                    while (temp.Count > 0) stack.Push(temp.Pop());
                }
            }
            return result;
        }

        private void filterMenuItem_Click(object sender, EventArgs e)
        {
            using (var filterDialog = new FilterForm(this))
                filterDialog.ShowDialog(this);
        }

        private void FilterButton_Click(object sender, EventArgs e) =>
            filterMenuItem_Click(sender, e);

        private void filterToolStripMenuItem_Click(object sender, EventArgs e) =>
            filterMenuItem_Click(sender, e);

        // ── Options / Help ────────────────────────────────────────────────────
        private void settingsMenuItem_Click(object sender, EventArgs e)
        {
            using (var settingsDialog = new SettingsForm(this))
            {
                if (settingsDialog.ShowDialog(this) == DialogResult.OK)
                {
                    SetTabVisible(TabId.Log,         _appSettings.ShowLogTab);
                    SetTabVisible(TabId.Raw,         _appSettings.ShowRawTab);
                    SetTabVisible(TabId.Performance, _appSettings.ShowPerformanceTab);
                    SetTabVisible(TabId.LogDetails,  _appSettings.ShowLogDetailsTab);
                    SetTabVisible(TabId.CallGraph,   _appSettings.ShowCallGraphTab);
                    SetTabVisible(TabId.FlameGraph,  _appSettings.ShowFlameGraphTab);
                    SetTabVisible(TabId.Timeline,    _appSettings.ShowTimelineTab);
                    // AI tab is a dynamic TabPage — toggle directly
                    if (_aiTab != null && mainTabControl != null)
                    {
                        bool showAi = _appSettings.ShowAiTab;
                        if (showAi && !mainTabControl.TabPages.Contains(_aiTab))
                            mainTabControl.TabPages.Add(_aiTab);
                        else if (!showAi && mainTabControl.TabPages.Contains(_aiTab))
                            mainTabControl.TabPages.Remove(_aiTab);
                    }
                    ApplyThemeWithOverlay();
                    ApplyToolbarVisibility();
                    ApplyFontSettings();
                    // Propagate updated AI settings (including configurable model) to the service.
                    _aiService?.UpdateConfig(_appSettings.ClaudeApiKey, _appSettings.UseClaudeApi,
                                             _appSettings.ClaudeModel);
                }
                // Refresh AI service after settings may have changed
                RefreshAiService();
            }
        }

        private void SettingsButton_Click(object sender, EventArgs e) =>
            settingsMenuItem_Click(sender, e);

        private void aboutMenuItem_Click(object sender, EventArgs e)
        {
            using (var aboutDialog = new AboutForm())
                aboutDialog.ShowDialog(this);
        }

        private void checkForUpdatesMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Open GitHub releases page
                System.Diagnostics.Process.Start("https://github.com/Nazeer-Hussain/CAD3PLogBrowser/releases");
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Resources.ERR_OPEN_UPDATES_FAILED, ex.Message), 
                    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void reportErrorsMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Open GitHub issues page
                System.Diagnostics.Process.Start("https://github.com/Nazeer-Hussain/CAD3PLogBrowser/issues/new");
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Resources.ERR_OPEN_ISSUES_FAILED, ex.Message), 
                    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void viewHelpMenuItem_Click(object sender, EventArgs e)
        {
            ShowUserGuide();
        }

        /// <summary>
        /// Opens the User Guide HTML file in default browser.
        /// Provides context-sensitive help based on current active control/tab.
        /// </summary>
        private void ShowUserGuide(string section = null)
        {
            try
            {
                string helpFilePath = Path.Combine(Application.StartupPath, "Help", "UserGuide.html");

                if (File.Exists(helpFilePath))
                {
                    // Add section anchor if specified for context-sensitive help
                    string url = helpFilePath;
                    if (!string.IsNullOrEmpty(section))
                    {
                        url = helpFilePath + "#" + section;
                    }

                    // Open HTML help file in default browser
                    System.Diagnostics.Process.Start(url);
                }
                else
                {
                    // If help file doesn't exist, show inline help dialog
                    ShowInlineHelpDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Resources.ERR_OPEN_HELP_FAILED, ex.Message), 
                    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Shows inline help dialog when UserGuide.html is not available.
        /// </summary>
        private void ShowInlineHelpDialog()
        {
            var helpForm = new Form
            {
                Text = Resources.DIALOG_TITLE_QUICK_HELP,
                Size = new Size(700, 600),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.Sizable,
                MinimizeBox = false,
                MaximizeBox = true,
                ShowIcon = false
            };

            var rtb = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.None,
                Padding = new Padding(20),
                Text = GetQuickHelpContent()
            };

            // Apply current theme to help dialog
            ThemeManager.ApplyTheme(helpForm);

            helpForm.Controls.Add(rtb);
            helpForm.ShowDialog(this);
        }

        /// <summary>
        /// Generates quick help content with overview and keyboard shortcuts.
        /// Used by both inline help and F1 context help.
        /// </summary>
        private string GetQuickHelpContent()
        {
            return "═══════════════════════════════════════════════════════════════════\r\n" +
                   "                 CAD 3P LOG BROWSER - QUICK HELP                      \r\n" +
                   "═══════════════════════════════════════════════════════════════════\r\n\r\n" +
                   "OVERVIEW\r\n" +
                   "─────────────────────────────────────────────────────────────────\r\n" +
                   "CAD 3P Log Browser is a powerful tool for viewing and analyzing\r\n" +
                   "CAD log files with advanced features like call trees, performance\r\n" +
                   "analysis, flame graphs, and more.\r\n\r\n" +
                   "GETTING STARTED\r\n" +
                   "─────────────────────────────────────────────────────────────────\r\n" +
                   "1. Open Log File: File → Open (Ctrl+O)\r\n" +
                   "2. View call structure in Call Tree (left panel)\r\n" +
                   "3. Analyze performance in Performance tab\r\n" +
                   "4. Use Find (Ctrl+F) and Filter (Ctrl+I) to narrow down\r\n\r\n" +
                   "KEY FEATURES\r\n" +
                   "─────────────────────────────────────────────────────────────────\r\n" +
                   "• Call Tree: Hierarchical view of API calls with ENTER/EXIT matching\r\n" +
                   "• API Tree: Grouped view of all API invocations\r\n" +
                   "• Performance Tab: Statistics on API call counts and durations\r\n" +
                   "• Call Graph: Visual dependency graph\r\n" +
                   "• Flame Graph: Performance visualization\r\n" +
                   "• Timeline: Chronological event view\r\n" +
                   "• Find & Filter: Powerful search with regex and criteria\r\n" +
                   "• Bookmarks: Mark important lines for quick access\r\n" +
                   "• Dark Theme: Eye-friendly professional appearance\r\n" +
                   "• AI Assistant: Log analysis and anomaly detection\r\n\r\n" +
                   GetKeyboardShortcutsContent() + "\r\n" +
                   "TIPS & TRICKS\r\n" +
                   "─────────────────────────────────────────────────────────────────\r\n" +
                   "• Use Tree Search: Type in the search box above trees to filter\r\n" +
                   "• Right-click menus: Context menus on trees and log view\r\n" +
                   "• Virtual mode: Handles 500k+ line files smoothly\r\n" +
                   "• Recent files: Last 10 files in File menu\r\n" +
                   "• Export options: Save to CSV, JSON, XML, or images\r\n" +
                   "• Performance guards: Warnings for large files and slow calls\r\n\r\n" +
                   "TROUBLESHOOTING\r\n" +
                   "─────────────────────────────────────────────────────────────────\r\n" +
                   "• File not loading? Check PTC_LOG_DIR environment variable\r\n" +
                   "• Slow performance? Enable lazy loading in settings\r\n" +
                   "• Can't find API? Use API Tree and search function\r\n" +
                   "• Unmatched ENTER/EXIT? Red X icon in tree indicates no match\r\n\r\n" +
                   "FOR MORE INFORMATION\r\n" +
                   "─────────────────────────────────────────────────────────────────\r\n" +
                   "Full documentation: Help → View User Guide (F1)\r\n" +
                   "GitHub: https://github.com/Nazeer-Hussain/CAD3PLogBrowser\r\n" +
                   "Report issues: Help → Report Errors\r\n" +
                   "Updates: Help → Check for Updates\r\n\r\n" +
                   "═══════════════════════════════════════════════════════════════════\r\n";
        }

        private void helpMenuItem_Click(object sender, EventArgs e)
        {
            // This should not be called - removed from designer
            ShowKeyboardShortcutsDialog();
        }

        // Feature G4: Comprehensive Keyboard Shortcuts Dialog
        private void keyboardShortcutsMenuItem_Click(object sender, EventArgs e)
        {
            ShowKeyboardShortcutsDialog();
        }

        private void ShowKeyboardShortcutsDialog()
        {
            var helpForm = new Form
            {
                Text = Resources.DIALOG_TITLE_KEYBOARD_SHORTCUTS,
                Size = new System.Drawing.Size(650, 550),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false, MinimizeBox = false,
                ShowIcon = false
            };

            var rtb = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new System.Drawing.Font("Consolas", 9f),
                BorderStyle = BorderStyle.None,
                Padding = new Padding(15),
                Text = "═══════════════════════════════════════════════════════════════════\r\n" +
                       "           CAD 3P LOG BROWSER — KEYBOARD SHORTCUTS\r\n" +
                       "═══════════════════════════════════════════════════════════════════\r\n\r\n" +
                       GetKeyboardShortcutsContent() + 
                       "TIPS\r\n" +
                       "─────────────────────────────────────────────────────────────────\r\n" +
                       "  •  Drag and drop a log file onto the window to open it\r\n" +
                       "  •  Click any tree node to jump to that line in the log\r\n" +
                       "  •  Select lines then Save As to save a trimmed log\r\n" +
                       "  •  ERROR lines are highlighted red, WARN in amber\r\n" +
                       "  •  Tree node colors: Green (fast), Amber (medium), Red (slow)\r\n" +
                       "  •  ✓ icon = matched ENTER/EXIT,  ✗ icon = unmatched\r\n" +
                       "  •  Duration overlay: [142 ms], [<1 ms], or [? ms] if unmatched\r\n" +
                       "  •  Virtual mode: handles 500k+ line log files smoothly\r\n" +
                       "  •  Recent Files: Last 10 opened files in File menu\r\n" +
                       "  •  Status bar shows: File info | Filter state | Selection preview\r\n\r\n" +
                       "═══════════════════════════════════════════════════════════════════\r\n"
            };

            // Apply current theme
            ThemeManager.ApplyTheme(helpForm);

            helpForm.Controls.Add(rtb);
            helpForm.ShowDialog(this);
        }

        /// <summary>
        /// Generates keyboard shortcuts content.
        /// Shared between Quick Help and Keyboard Shortcuts dialogs to avoid duplication.
        /// </summary>
        private string GetKeyboardShortcutsContent()
        {
            return "FILE OPERATIONS\r\n" +
                   "Ctrl+O              Open log file\r\n" +
                   "Ctrl+S              Save As (selection or all visible lines)\r\n" +
                   "Ctrl+Shift+E        Export Filtered Logs\r\n" +
                   "F5                  Refresh (reload, keep scroll position)\r\n" +
                   "Ctrl+R              Reload File from Disk\r\n" +
                   "Alt+F4              Exit\r\n\r\n" +
                   "EDITING & SEARCH\r\n" +
                   "Ctrl+C              Copy selected lines\r\n" +
                   "Ctrl+F              Find / Search\r\n" +
                   "F3                  Find Next\r\n" +
                   "Ctrl+I              Filter log entries\r\n\r\n" +
                   "TREE NAVIGATION\r\n" +
                   "Ctrl+E              Expand All (Call Tree & API Tree)\r\n" +
                   "Ctrl+W              Collapse All (keeps root nodes expanded)\r\n" +
                   "Ctrl+G              Jump to Matching ENTER/EXIT pair\r\n" +
                   "Ctrl+J              Jump to line number\r\n\r\n" +
                   "ERROR & WARNING NAVIGATION\r\n" +
                   "F8                  Next Error\r\n" +
                   "Shift+F8            Previous Error\r\n" +
                   "Ctrl+F8             Next Warning\r\n" +
                   "Ctrl+Shift+F8       Previous Warning\r\n\r\n" +
                   "BOOKMARKS\r\n" +
                   "Ctrl+B              Toggle bookmark on current line\r\n" +
                   "F2                  Next bookmark\r\n" +
                   "Shift+F2            Previous bookmark\r\n" +
                   "Ctrl+Shift+B        Show all bookmarks\r\n" +
                   "Ctrl+Shift+Del      Clear all bookmarks\r\n\r\n" +
                   "VIEW OPTIONS\r\n" +
                   "Ctrl+T              Toggle theme (Dark/Light)\r\n" +
                   "Ctrl+1              Show Call Tree\r\n" +
                   "Ctrl+2              Show API Tree\r\n\r\n" +
                   "HELP\r\n" +
                   "F1                  View Help / User Guide\r\n" +
                   "Ctrl+K              Keyboard Shortcuts (this dialog)\r\n\r\n" +
                   "CALL GRAPH TAB\r\n" +
                   "Scroll Wheel        Zoom in/out\r\n" +
                   "Click & Drag        Pan view\r\n" +
                   "Hover Node          Highlight edges\r\n" +
                   "Edge Thickness      = Call frequency\r\n" +
                   "Reset View Button   Restore default zoom/pan\r\n\r\n";
        }

        // ── Form lifecycle ────────────────────────────────────────────────────
        // Feature B10: Keyboard shortcuts for error/warning navigation

        private void MainForm_Load(object sender, EventArgs e)
        {
            SetDocumentLoaded(false);
            LayoutTrees();

            // Feature 2a: Set default splitter to 30% only on first run (no saved value)
            // Check if this is the first run (no saved splitter distance)
            if (_appSettings.SplitterDistance <= 0)
            {
                // First run - calculate 30% default
                int defaultSplitter = (int)(this.ClientSize.Width * 0.3);
                if (defaultSplitter > mainSplitContainer.Panel1MinSize && 
                    defaultSplitter < this.ClientSize.Width - mainSplitContainer.Panel2MinSize)
                {
                    mainSplitContainer.SplitterDistance = defaultSplitter;
                }
            }
            // else: RestoreSettings already set the splitter distance from saved value

            logTab.Text         = Resources.TAB_LOG;
            rawTab.Text         = Resources.TAB_RAW;
            performanceTab.Text = Resources.TAB_PERFORMANCE;
            logDetailTab.Text   = Resources.TAB_LOG_DETAILS;
            callGraphTab.Text   = Resources.TAB_CALL_GRAPH;

            ApplyTabIcons();

            // CRITICAL FIX: Restore splitter distance AFTER all window layout is complete
            // Use BeginInvoke to run after the message queue is processed
            if (_appSettings.SplitterDistance > 0)
            {
                int savedDistance = _appSettings.SplitterDistance;
                this.BeginInvoke((Action)(() =>
                {
                    mainSplitContainer.SplitterDistance = savedDistance;

                    // NOW form is fully loaded - enable saving
                    _isFormLoaded = true;
                }));
            }
            else
            {
                // No saved value - just enable saving
                _isFormLoaded = true;
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                SaveSettings();
            }
            catch { /* Non-fatal */ }

            try
            {
                _logFileService?.Dispose();
            }
            catch { /* Non-fatal */ }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Persist bookmarks for the current file before closing.
            try { _bookmarkService.SaveBookmarks(); } catch { /* non-fatal */ }

            // Save search history before closing
            try
            {
                SaveSearchHistory();
            }
            catch { /* Non-fatal */ }

            // Stop file watching immediately to prevent blocking on close
            try
            {
                _logFileService?.StopWatching();
            }
            catch { /* Non-fatal */ }

            // PERFORMANCE FIX: Clear large data structures before form disposal
            // This significantly speeds up form closing with large log files
            try
            {
                // Suspend layout to speed up clearing
                CallTree.BeginUpdate();
                ApiTree.BeginUpdate();

                // Clear tree views (can have thousands of nodes)
                CallTree.Nodes.Clear();
                ApiTree.Nodes.Clear();

                CallTree.EndUpdate();
                ApiTree.EndUpdate();

                // Clear virtual list view
                logListView.VirtualListSize = 0;
                _virtualLines.Clear();

                // Clear large collections
                _allLines.Clear();
                _apiNodes.Clear();
                _lastEntries.Clear();
                _errorLines.Clear();
                _warningLines.Clear();
            }
            catch
            {
                /* Non-fatal */
            }
        }

        // Issue Fix: Auto-resize ListView columns when form resizes
        private void logListView_Resize(object sender, EventArgs e)
        {
            // Re-use the already-measured content width; only adjust the column
            // to fill the view when it has grown larger than the content.
            AutoResizeLogListColumns();
        }

        private void MainForm_ResizeBegin(object sender, EventArgs e) { }
        private void MainForm_ResizeEnd(object sender, EventArgs e) => LayoutTrees();
        private void MainForm_Resize(object sender, EventArgs e) { }
        private void MainForm_SizeChanged(object sender, EventArgs e) { }
        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            LayoutTrees();

            // Only save splitter distance if form is fully loaded (not during initialization)
            if (!_isFormLoaded)
                return;

            // Save splitter distance immediately to in-memory settings
            if (_appSettings != null && mainSplitContainer != null)
            {
                _appSettings.SplitterDistance = mainSplitContainer.SplitterDistance;
            }
        }
        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e) { }
        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e) { }

        // ── Context menu ──────────────────────────────────────────────────────
        private void listView1_MouseUp(object sender, MouseEventArgs e) { }

        private void listView1_MouseUp_1(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                logContextMenu.Show(logListView, e.Location);
        }

        private void CallTree_MouseClick(object sender, MouseEventArgs e) { }

        private void CallTree_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            var node = CallTree.GetNodeAt(e.Location);
            if (node != null) CallTree.SelectedNode = node;

            // D-18: build once, reuse on every right-click.
            if (_callTreeContextMenu == null)
                _callTreeContextMenu = BuildCallTreeContextMenu();

            // Update the one dynamic item whose visibility depends on settings.
            bool hasGrok = !string.IsNullOrEmpty(_appSettings?.GrokUrl);
            _callTreeGrokSep.Visible  = hasGrok;
            _callTreeGrokItem.Visible = hasGrok;

            ApplyContextMenuTheme(_callTreeContextMenu);
            _callTreeContextMenu.Show(CallTree, e.Location);
        }

        /// <summary>
        /// Builds the Call Tree context menu exactly once.  Lambdas read
        /// CallTree.SelectedNode at click-time so no stale-capture issue.
        /// </summary>
        private ContextMenuStrip BuildCallTreeContextMenu()
        {
            var menu = new ContextMenuStrip();

            menu.Items.Add(Resources.MENU_COPY_METHOD_NAME, null, (s, ev) =>
            {
                var n = CallTree.SelectedNode;
                if (n != null) Clipboard.SetText(GetMethodNameFromNode(n));
            });

            menu.Items.Add(Resources.MENU_COPY_SUBTREE, null, (s, ev) =>
            {
                var n = CallTree.SelectedNode;
                if (n != null)
                {
                    var sb = new System.Text.StringBuilder();
                    AppendSubtreeText(n, sb, 0);
                    Clipboard.SetText(sb.ToString());
                }
            });

            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(Resources.MENU_JUMP_TO_MATCHING, null, (s, ev) => JumpToMatchingPair());
            menu.Items.Add(Resources.MENU_INSPECT_LINE, null, (s, ev) => InspectSelectedLine());
            ((ToolStripMenuItem)menu.Items[menu.Items.Count - 1]).ShortcutKeyDisplayString = "F12";

            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(Resources.MENU_SAVE_BRANCH, null, treeContextSaveBranchMenuItem_Click);
            menu.Items.Add(Resources.MENU_EXPORT_BRANCH_CSV, null, treeContextExportBranchCsvMenuItem_Click);

            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(Resources.MENU_EXPAND_ALL_SHORTCUT, null, (s, ev) => CallTree.ExpandAll());
            menu.Items.Add(Resources.MENU_COLLAPSE_ALL_SHORTCUT, null, (s, ev) =>
            {
                CallTree.CollapseAll();
                if (CallTree.Nodes.Count > 0) CallTree.Nodes[0].Expand();
            });

            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(Resources.MENU_SHOW_IN_API_TREE, null, (s, ev) =>
            {
                var n = CallTree.SelectedNode;
                if (n != null) { ShowApiTree(); FindAndSelectApiTreeNode(GetMethodNameFromNode(n)); }
            });

            // Grok item — always present, Visible toggled on each show.
            _callTreeGrokSep  = new ToolStripSeparator { Visible = false };
            _callTreeGrokItem = new ToolStripMenuItem(Resources.MENU_SEARCH_IN_GROK) { Visible = false };
            _callTreeGrokItem.Click += treeContextSearchInGrokMenuItem_Click;
            menu.Items.Add(_callTreeGrokSep);
            menu.Items.Add(_callTreeGrokItem);

            return menu;
        }

        /// <summary>Applies dark/light theme colours to a context menu in-place.</summary>
        private static void ApplyContextMenuTheme(ContextMenuStrip menu)
        {
            if (ThemeManager.CurrentTheme == ThemeManager.Theme.Dark)
            {
                menu.Renderer  = new ToolStripProfessionalRenderer(new DarkContextMenuColorTable());
                menu.BackColor = Color.FromArgb(45, 45, 48);
                menu.ForeColor = Color.FromArgb(241, 241, 241);
            }
            else
            {
                menu.Renderer  = new ToolStripProfessionalRenderer();
                menu.BackColor = SystemColors.Menu;
                menu.ForeColor = SystemColors.MenuText;
            }
        }

        // Dark theme context menu color table
        private class DarkContextMenuColorTable : ProfessionalColorTable
        {
            public override Color MenuItemSelected => Color.FromArgb(62, 62, 64);
            public override Color MenuItemSelectedGradientBegin => Color.FromArgb(62, 62, 64);
            public override Color MenuItemSelectedGradientEnd => Color.FromArgb(62, 62, 64);
            public override Color MenuItemBorder => Color.FromArgb(0, 122, 204);
            public override Color MenuBorder => Color.FromArgb(63, 63, 70);
            public override Color ToolStripDropDownBackground => Color.FromArgb(45, 45, 48);
            public override Color ImageMarginGradientBegin => Color.FromArgb(45, 45, 48);
            public override Color ImageMarginGradientMiddle => Color.FromArgb(45, 45, 48);
            public override Color ImageMarginGradientEnd => Color.FromArgb(45, 45, 48);
        }

        // C6/J3: Tree context menu handlers
        private TreeNode GetActiveTreeSelectedNode()
        {
            if (CallTree.Visible && CallTree.SelectedNode != null) return CallTree.SelectedNode;
            if (ApiTree.Visible && ApiTree.SelectedNode != null) return ApiTree.SelectedNode;
            return null;
        }

        private string GetMethodNameFromNode(TreeNode node)
        {
            if (node == null) return string.Empty;
            string text = node.Text;
            // Strip duration suffix like " [142 ms]" or call count like " (3 calls)"
            int bracket = text.IndexOf(" [");
            if (bracket > 0) return text.Substring(0, bracket).Trim();
            int paren = text.IndexOf(" (");
            if (paren > 0) return text.Substring(0, paren).Trim();
            // Strip line number suffix like " — Ln 123"
            int dash = text.IndexOf(" \u2014 ");
            if (dash > 0) return text.Substring(0, dash).Trim();
            return text.Trim();
        }

        // ── Branch extraction helpers ─────────────────────────────────────────
        // Log format: ...: Area: State\tModule\tSourceFile\tApiName\tENTER\tEpochMs
        // ENTER/EXIT are tab-delimited fields, NOT [ENTER]/[EXIT] in brackets.

        private static bool IsApiEnterLine(string line) =>
            line.Contains("\tENTER\t") || line.TrimEnd().EndsWith("\tENTER");

        private static bool IsApiExitLine(string line) =>
            line.Contains("\tEXIT\t") || line.TrimEnd().EndsWith("\tEXIT");

        /// <summary>
        /// Extracts the log lines from an ENTER to its matching EXIT (inclusive),
        /// correctly handling nested calls of the same method.
        /// Scans _allLines (not filtered) so active filters cannot hide the ENTER/EXIT.
        /// </summary>
        /// <param name="enterLine1Based">
        ///   1-based line number of the ENTER line (from the node's Tag).
        ///   Pass -1 to fall back to searching by methodName.
        /// </param>
        private List<string> ExtractBranchLines(int enterLine1Based, string methodName)
        {
            var lines = new List<string>();
            if (_allLines == null || _allLines.Count == 0) return lines;

            // Determine the 0-based start index.
            int startIdx = -1;
            if (enterLine1Based > 0 && enterLine1Based <= _allLines.Count)
            {
                // Use the exact line the parser recorded for this call
                startIdx = enterLine1Based - 1;
            }
            else if (!string.IsNullOrEmpty(methodName))
            {
                // Fall back: scan for first ENTER line that contains the method name
                for (int i = 0; i < _allLines.Count; i++)
                {
                    string t = _allLines[i];
                    if (IsApiEnterLine(t) && t.Contains(methodName))
                    {
                        startIdx = i;
                        break;
                    }
                }
            }

            if (startIdx < 0) return lines;

            int depth = 0;
            for (int i = startIdx; i < _allLines.Count; i++)
            {
                string t = _allLines[i];
                lines.Add(t);

                if (IsApiEnterLine(t)) depth++;
                if (IsApiExitLine(t))
                {
                    depth--;
                    if (depth <= 0) break;
                }
            }

            // If we hit end-of-file before the matching EXIT, still return what we have
            // (unmatched ENTER) rather than showing an error for incomplete traces.
            return lines;
        }

        // J3: Search in Grok
        private void treeContextSearchInGrokMenuItem_Click(object sender, EventArgs e)
        {
            var node = GetActiveTreeSelectedNode();
            if (node == null) return;

            string methodName = GetMethodNameFromNode(node);
            string grokUrl = _appSettings?.GrokUrl?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(grokUrl))
            {
                MessageBox.Show(Resources.MSG_GROK_NOT_CONFIGURED,
                    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string url = grokUrl + Uri.EscapeDataString(methodName);
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Resources.ERR_BROWSER_FAILED, ex.Message),
                    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Copy node name
        private void treeContextCopyNodeNameMenuItem_Click(object sender, EventArgs e)
        {
            var node = GetActiveTreeSelectedNode();
            if (node == null) return;
            Clipboard.SetText(GetMethodNameFromNode(node));
        }

        // Copy subtree recursively as indented text
        private void treeContextCopySubtreeMenuItem_Click(object sender, EventArgs e)
        {
            var node = GetActiveTreeSelectedNode();
            if (node == null) return;
            var sb = new System.Text.StringBuilder();
            AppendSubtreeText(node, sb, 0);
            Clipboard.SetText(sb.ToString());
        }

        private void AppendSubtreeText(TreeNode node, System.Text.StringBuilder sb, int depth)
        {
            if (depth > 500) return; // guard against StackOverflow
            sb.AppendLine(new string(' ', depth * 2) + node.Text);
            foreach (TreeNode child in node.Nodes)
                AppendSubtreeText(child, sb, depth + 1);
        }

        // Export branch to CSV
        private void treeContextExportBranchCsvMenuItem_Click(object sender, EventArgs e)
        {
            var node = GetActiveTreeSelectedNode();
            if (node == null) return;

            using (var dlg = new SaveFileDialog())
            {
                dlg.Title = Resources.DIALOG_TITLE_EXPORT_BRANCH;
                dlg.Filter = Resources.FILE_FILTER_CSV_FILES;
                dlg.FileName = GetMethodNameFromNode(node).Replace("::", "_") + Resources.FILENAME_SUFFIX_BRANCH_CSV;

                if (dlg.ShowDialog() != DialogResult.OK) return;

                var rows = new List<string> { Resources.CSV_HEADER_BRANCH };
                CollectBranchCsvRows(node, rows, 0);
                File.WriteAllLines(dlg.FileName, rows);

                MessageBox.Show(string.Format(Resources.MSG_BRANCH_EXPORTED_TO, dlg.FileName),
                    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void CollectBranchCsvRows(TreeNode node, List<string> rows, int depth)
        {
            if (depth > 500) return; // guard against StackOverflow
            string name = GetMethodNameFromNode(node);
            string duration = "";
            int b1 = node.Text.IndexOf('[');
            int b2 = node.Text.IndexOf(" ms]");
            if (b1 >= 0 && b2 > b1)
                duration = node.Text.Substring(b1 + 1, b2 - b1 - 1).Trim();
            rows.Add(string.Format("\"{0}\",{1},{2}", name, depth, duration));
            foreach (TreeNode child in node.Nodes)
                CollectBranchCsvRows(child, rows, depth + 1);
        }

        // Show in other tree (cross-reference)
        private void treeContextShowInOtherTreeMenuItem_Click(object sender, EventArgs e)
        {
            var node = GetActiveTreeSelectedNode();
            if (node == null) return;

            string methodName = GetMethodNameFromNode(node);

            if (CallTree.Visible)
            {
                // Switch to API Tree and find matching node
                ShowApiTree();
                FindAndSelectApiTreeNode(methodName);
            }
            else
            {
                // Switch to Call Tree and find matching node
                ShowCallTree();
                FindAndSelectCallTreeNode(methodName);
            }
        }

        // D5: Try to highlight matching node in API Tree (without switching views)
        private void TryHighlightInApiTree(string methodName)
        {
            if (string.IsNullOrEmpty(methodName) || ApiTree.Nodes.Count == 0)
                return;

            // Search for matching API node
            foreach (TreeNode root in ApiTree.Nodes)
            {
                foreach (TreeNode apiNode in root.Nodes)
                {
                    if (GetMethodNameFromNode(apiNode).Equals(methodName, StringComparison.OrdinalIgnoreCase))
                    {
                        // Highlight but don't scroll - just show it's selected
                        ApiTree.SelectedNode = apiNode;
                        return;
                    }
                }
            }
        }

        // D5: Try to highlight matching node in Call Tree (without switching views)
        private void TryHighlightInCallTree(string methodName)
        {
            if (string.IsNullOrEmpty(methodName) || CallTree.Nodes.Count == 0)
                return;

            // Search for first matching call tree node
            foreach (TreeNode root in CallTree.Nodes)
            {
                if (FindAndHighlightInTree(root.Nodes, methodName))
                    return;
            }
        }

        private bool FindAndHighlightInTree(TreeNodeCollection nodes, string methodName)
        {
            foreach (TreeNode n in nodes)
            {
                if (GetMethodNameFromNode(n).Equals(methodName, StringComparison.OrdinalIgnoreCase))
                {
                    CallTree.SelectedNode = n;
                    return true;
                }
                // Search children recursively (depth-first)
                if (FindAndHighlightInTree(n.Nodes, methodName))
                    return true;
            }
            return false;
        }

        private void FindAndSelectApiTreeNode(string methodName)
        {
            foreach (TreeNode root in ApiTree.Nodes)
            {
                foreach (TreeNode apiNode in root.Nodes)
                {
                    if (GetMethodNameFromNode(apiNode).Equals(methodName, StringComparison.OrdinalIgnoreCase))
                    {
                        ApiTree.SelectedNode = apiNode;
                        apiNode.EnsureVisible();
                        return;
                    }
                }
            }
        }

        private void FindAndSelectCallTreeNode(string methodName)
        {
            foreach (TreeNode root in CallTree.Nodes)
            {
                if (FindNodeInTree(root.Nodes, methodName))
                    return;
            }
        }

        private bool FindNodeInTree(TreeNodeCollection nodes, string methodName)
        {
            foreach (TreeNode n in nodes)
            {
                if (GetMethodNameFromNode(n).Equals(methodName, StringComparison.OrdinalIgnoreCase))
                {
                    CallTree.SelectedNode = n;
                    n.EnsureVisible();
                    return true;
                }
                if (FindNodeInTree(n.Nodes, methodName))
                    return true;
            }
            return false;
        }

        private void logWatcher_Changed(object sender, System.IO.FileSystemEventArgs e) { }

        // ── Feature I3: Export Performance to CSV ────────────────────────────
        private void exportPerformanceMenuItem_Click(object sender, EventArgs e)
        {
            if (performanceView.Items.Count == 0)
            {
                MessageBox.Show(Resources.ERR_NO_PERFORMANCE_DATA,
                    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = Resources.FILE_FILTER_CSV_FILES;
                dialog.FileName = GetSafeBaseName(_currentFilePath) + Resources.FILENAME_SUFFIX_PERFORMANCE_CSV;
                dialog.InitialDirectory = string.IsNullOrEmpty(_currentFilePath) 
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : GetSafeDirectory(_currentFilePath);

                if (dialog.ShowDialog() != DialogResult.OK) return;

                try
                {
                    using (var writer = new StreamWriter(dialog.FileName))
                    {
                        // Write header
                        writer.WriteLine(Resources.CSV_HEADER_PERFORMANCE);

                        // Write data (skip summary row if present)
                        foreach (ListViewItem item in performanceView.Items)
                        {
                            if (item.Text == "──── TOTAL ────") continue;

                            var values = new string[8];
                            values[0] = EscapeCsv(item.Text);
                            for (int i = 0; i < item.SubItems.Count && i < 8; i++)
                                values[i] = EscapeCsv(item.SubItems[i].Text);

                            writer.WriteLine(string.Join(",", values));
                        }
                    }

                    MessageBox.Show(string.Format(Resources.MSG_PERFORMANCE_EXPORTED_TO, dialog.FileName),
                        Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(Resources.ERR_EXPORT_PERFORMANCE_FAILED, ex.Message),
                        Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }

        // ── Feature B9: Jump to Line Number ──────────────────────────────────
        private void jumpToLineMenuItem_Click(object sender, EventArgs e)
        {
            if (_virtualLines.Count == 0)
            {
                MessageBox.Show(Resources.ERR_NO_FILE_LOADED, Resources.TITLE, 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string input = Microsoft.VisualBasic.Interaction.InputBox(
                string.Format(Resources.PROMPT_ENTER_LINE_NUMBER, _virtualLines.Count),
                Resources.DIALOG_TITLE_JUMP_TO_LINE,
                "",
                -1, -1);

            if (string.IsNullOrWhiteSpace(input)) return;

            if (int.TryParse(input.Trim(), out int lineNum))
            {
                if (lineNum >= 1 && lineNum <= _virtualLines.Count)
                {
                    int index = lineNum - 1;
                    logListView.EnsureVisible(index);
                    logListView.SelectedIndices.Clear();
                    logListView.SelectedIndices.Add(index);
                    // Do NOT call logListView.Items[index] — it returns null in virtual mode.
                }
                else
                {
                    MessageBox.Show(string.Format(Resources.ERR_LINE_NUMBER_OUT_OF_RANGE, _virtualLines.Count),
                        Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show(Resources.ERR_INVALID_LINE_NUMBER, Resources.TITLE,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ── Feature I2: Save Selected Branch ─────────────────────────────────
        private void treeContextSaveBranchMenuItem_Click(object sender, EventArgs e)
        {
            TreeView activeTree = CallTreeButton.Checked ? CallTree : ApiTree;
            if (activeTree?.SelectedNode == null) return;

            TreeNode node      = activeTree.SelectedNode;
            string methodName  = GetMethodNameFromNode(node);
            int    enterLine   = (node.Tag is int t && t > 0) ? t : -1;
            List<string> lines = ExtractBranchLines(enterLine, methodName);

            if (lines.Count == 0)
            {
                MessageBox.Show(string.Format(Resources.ERR_NO_ENTER_EXIT_PAIR, methodName),
                    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = Resources.FILE_FILTER_LOG_SAVE;
                dialog.FileName = methodName.Replace("::", "_") + _appSettings.SaveSnippetSuffix + ".log";
                dialog.InitialDirectory = string.IsNullOrEmpty(_currentFilePath)
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : GetSafeDirectory(_currentFilePath);

                if (dialog.ShowDialog() != DialogResult.OK) return;

                try
                {
                    File.WriteAllLines(dialog.FileName, lines);
                    MessageBox.Show(string.Format(Resources.MSG_BRANCH_SAVED_TO, lines.Count, dialog.FileName),
                        Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(Resources.ERR_SAVE_BRANCH_FAILED, ex.Message),
                        Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ── Feature: Show/Hide Toolbar ───────────────────────────────────────
        private void showToolbarMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            mainToolStrip.Visible = showToolbarMenuItem.Checked;
            _appSettings.ShowToolbar = showToolbarMenuItem.Checked;
        }

        private void showStatusBarMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            mainStatusStrip.Visible = showStatusBarMenuItem.Checked;
            _appSettings.ShowStatusBar = showStatusBarMenuItem.Checked;
        }

        // ── Feature B7: Find All Results Window ──────────────────────────────
        private void findAllMenuItem_Click(object sender, EventArgs e)
        {
            if (_virtualLines.Count == 0)
            {
                MessageBox.Show(Resources.ERR_NO_FILE_LOADED, Resources.TITLE, 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Prompt for search term
            string searchTerm = Microsoft.VisualBasic.Interaction.InputBox(
                Resources.PROMPT_ENTER_SEARCH_TERM,
                Resources.DIALOG_TITLE_FIND_ALL,
                "",
                -1, -1);

            if (string.IsNullOrWhiteSpace(searchTerm)) return;

            // Search all lines
            var results = new List<FindResult>();
            for (int i = 0; i < _virtualLines.Count; i++)
            {
                if (_virtualLines[i].Text.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    results.Add(new FindResult
                    {
                        LineNumber = i + 1,
                        LineText = _virtualLines[i].Text
                    });
                }
            }

            if (results.Count == 0)
            {
                MessageBox.Show(string.Format(Resources.ERR_NO_MATCHES_FOUND, searchTerm),
                    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Show results window
            var resultsForm = new FindAllResultsForm(this, results, searchTerm);
            resultsForm.Show(this);
        }

        public void JumpToLine(int lineNumber)
        {
            if (lineNumber < 1 || lineNumber > _virtualLines.Count) return;

            int index = lineNumber - 1;
            logListView.EnsureVisible(index);
            logListView.SelectedIndices.Clear();
            logListView.SelectedIndices.Add(index);
            // NOTE: logListView.Items[index] returns null in virtual mode — do NOT set
            // FocusedItem; selecting via SelectedIndices is sufficient for virtual lists.
            Focus();
        }

        // ── Feature F6: Export Call Graph as PNG ─────────────────────────────
        private void callGraphExportButton_Click(object sender, EventArgs e)
        {
            if (callGraphPanel == null) return;

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = Resources.FILE_FILTER_IMAGE_FILES;
                dialog.FileName = GetSafeBaseName(_currentFilePath) + Resources.FILENAME_SUFFIX_CALLGRAPH_PNG;
                dialog.InitialDirectory = string.IsNullOrEmpty(_currentFilePath)
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : GetSafeDirectory(_currentFilePath);

                if (dialog.ShowDialog() != DialogResult.OK) return;

                try
                {
                    // Create high-resolution bitmap
                    int width = Math.Max(800, callGraphPanel.Width);
                    int height = Math.Max(600, callGraphPanel.Height);

                    using (var bitmap = new Bitmap(width, height))
                    {
                        callGraphPanel.DrawToBitmap(bitmap, new Rectangle(0, 0, width, height));

                        // Determine format from extension
                        string ext = Path.GetExtension(dialog.FileName).ToLowerInvariant();
                        var format = System.Drawing.Imaging.ImageFormat.Png;

                        if (ext == ".jpg" || ext == ".jpeg")
                            format = System.Drawing.Imaging.ImageFormat.Jpeg;
                        else if (ext == ".bmp")
                            format = System.Drawing.Imaging.ImageFormat.Bmp;

                        bitmap.Save(dialog.FileName, format);
                    }

                    MessageBox.Show(string.Format(Resources.MSG_CALL_GRAPH_EXPORTED_TO, dialog.FileName),
                        Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(Resources.ERR_EXPORT_CALL_GRAPH_FAILED, ex.Message),
                        Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ── Feature B5: Duration Threshold Filter ────────────────────────────
        private bool CheckDurationFilter(string line, int minDurationMs)
        {
            // Look for duration pattern: [XX ms] or similar
            var match = System.Text.RegularExpressions.Regex.Match(line, @"\[(\d+)\s*ms\]");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int duration))
            {
                return duration >= minDurationMs;
            }
            return false; // No duration found, exclude from filter
        }

        // ── Feature B4: Time Range Filter ────────────────────────────────────
        private bool CheckTimeRangeFilter(string line, DateTime? fromTime, DateTime? toTime)
        {
            // Parse timestamp from log line (assumes format: YYYY-MM-DD HH:MM:SS)
            var match = System.Text.RegularExpressions.Regex.Match(line, @"(\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2})");
            if (!match.Success) return false;

            if (DateTime.TryParse(match.Groups[1].Value, out DateTime lineTime))
            {
                var timeOnly = lineTime.TimeOfDay;

                if (fromTime.HasValue && timeOnly < fromTime.Value.TimeOfDay)
                    return false;

                if (toTime.HasValue && timeOnly > toTime.Value.TimeOfDay)
                    return false;

                return true;
            }

            return false;
        }

        // ═══════════════════════════════════════════════════════════════════════
        // FEATURE 1 & 3: Copy Menu Item Handlers (CRITICAL)
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Handles copy menu item click - copies selected log lines to clipboard.
        /// </summary>
        private void copyMenuItem_Click(object sender, EventArgs e)
        {
            CopySelectedLinesToClipboard(includeHeaders: false);
        }

        /// <summary>
        /// Handles context menu copy click.
        /// </summary>
        private void contextCopyMenuItem_Click(object sender, EventArgs e)
        {
            CopySelectedLinesToClipboard(includeHeaders: false);
        }

        /// <summary>
        /// Handles toolbar copy button click.
        /// </summary>
        private void CopyButton_Click(object sender, EventArgs e)
        {
            CopySelectedLinesToClipboard(includeHeaders: false);
        }

        /// <summary>
        /// Handles copy with headers menu item click.
        /// </summary>
        private void contextCopyWithHeadersMenuItem_Click(object sender, EventArgs e)
        {
            CopySelectedLinesToClipboard(includeHeaders: true);
        }

        // (contextCopyWithHeadersMenuItem_Click combined above)

        /// <summary>
        /// Copies selected log lines to clipboard.
        /// </summary>
        /// <param name="includeHeaders">If true, includes "Line #\tLog Text" header.</param>
        private void CopySelectedLinesToClipboard(bool includeHeaders)
        {
            try
            {
                if (logListView.SelectedIndices.Count == 0)
                {
                    StatusFileName.Text = Resources.STATUS_NO_LINES_SELECTED;
                    return;
                }

                var sb = new System.Text.StringBuilder();

                // Add header if requested
                if (includeHeaders)
                {
                    sb.AppendLine("Line #\tLog Text");
                    sb.AppendLine("------\t--------");
                }

                // Copy selected indices to array and sort
                var indices = new int[logListView.SelectedIndices.Count];
                logListView.SelectedIndices.CopyTo(indices, 0);
                Array.Sort(indices);

                // Get text for each selected line
                foreach (int index in indices)
                {
                    if (index >= 0 && index < _virtualLines.Count)
                    {
                        var line = _virtualLines[index];
                        if (includeHeaders)
                        {
                            // Tab-separated format: Line# \t Text
                            sb.AppendLine(string.Format("{0}\t{1}", line.LineNumber, line.Text));
                        }
                        else
                        {
                            // Just the log text
                            sb.AppendLine(line.Text);
                        }
                    }
                }

                // Copy to clipboard
                if (sb.Length > 0)
                {
                    Clipboard.SetText(sb.ToString());

                    // Update status bar
                    string message = includeHeaders 
                        ? string.Format(Resources.MSG_COPIED_WITH_HEADERS, indices.Length)
                        : string.Format(Resources.MSG_COPIED_WITHOUT_HEADERS, indices.Length);

                    StatusFileName.Text = message;

                    // Clear status after 3 seconds — guard against form disposal
                    // before the tick fires (Bug #17).
                    var timer = new System.Windows.Forms.Timer { Interval = 3000 };
                    timer.Tick += (s, args) =>
                    {
                        timer.Stop();
                        timer.Dispose();
                        if (!IsDisposed && !Disposing)
                            StatusFileName.Text = GetSafeFileName(_currentFilePath);
                    };
                    timer.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Resources.ERR_COPY_FAILED, ex.Message),
                    Resources.TITLE, 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // FEATURE 2: Search History Persistence (B6)
        // ═══════════════════════════════════════════════════════════════════════

        private const int MAX_SEARCH_HISTORY = 20;

        /// <summary>
        /// Saves search history to JSON file.
        /// Call this when FindForm is closing or app is exiting.
        /// </summary>
        private void SaveSearchHistory()
        {
            try
            {
                var searchHistory = new List<string>();

                // Get search history from app settings if available
                if (_appSettings != null && _appSettings.SearchHistory != null)
                {
                    searchHistory = _appSettings.SearchHistory;
                }

                // Limit to MAX_SEARCH_HISTORY items
                if (searchHistory.Count > MAX_SEARCH_HISTORY)
                    searchHistory = searchHistory.Take(MAX_SEARCH_HISTORY).ToList();

                // Save settings
                if (_appSettings != null)
                {
                    _appSettings.SearchHistory = searchHistory;
                    _appSettings.Save();
                }
            }
            catch (Exception ex)
            {
                // Don't show error to user - just log it
                System.Diagnostics.Debug.WriteLine(string.Format("Failed to save search history: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Adds a search term to history.
        /// </summary>
        public void AddSearchHistory(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return;

            if (_appSettings.SearchHistory == null)
                _appSettings.SearchHistory = new List<string>();

            // Remove if already exists
            _appSettings.SearchHistory.Remove(searchTerm);

            // Add to beginning
            _appSettings.SearchHistory.Insert(0, searchTerm);

            // Limit size
            if (_appSettings.SearchHistory.Count > MAX_SEARCH_HISTORY)
                _appSettings.SearchHistory = _appSettings.SearchHistory.Take(MAX_SEARCH_HISTORY).ToList();
        }

        /// <summary>
        /// Gets search history.
        /// </summary>
        public List<string> GetSearchHistory()
        {
            return _appSettings?.SearchHistory ?? new List<string>();
        }

        // ═══════════════════════════════════════════════════════════════════════
        // FEATURE 4: Tree Search/Filter (C5)
        // ═══════════════════════════════════════════════════════════════════════

        private string _treeSearchText = string.Empty;
        private const string TREE_SEARCH_PLACEHOLDER = "Search tree nodes...";

        /// <summary>
        /// Handler for tree search textbox - filters tree nodes in real-time.
        /// </summary>
        private void treeSearchTextBox_TextChanged(object sender, EventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Ignore if showing placeholder
                if (textBox.ForeColor == SystemColors.GrayText)
                    return;

                FilterTreeNodes(textBox.Text);
            }
        }

        /// <summary>
        /// Handles Enter event - removes placeholder text.
        /// </summary>
        private void treeSearchTextBox_Enter(object sender, EventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Text == TREE_SEARCH_PLACEHOLDER)
                {
                    textBox.Text = "";
                    textBox.ForeColor = SystemColors.WindowText;
                }
            }
        }

        /// <summary>
        /// Handles Leave event - shows placeholder if empty.
        /// </summary>
        private void treeSearchTextBox_Leave(object sender, EventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = TREE_SEARCH_PLACEHOLDER;
                    textBox.ForeColor = SystemColors.GrayText;
                }
            }
        }

        /// <summary>
        /// Filters tree nodes based on search text.
        /// Matching nodes are highlighted in yellow.
        /// </summary>
        /// <param name="searchText">Text to search for in node names.</param>
        private void FilterTreeNodes(string searchText)
        {
            _treeSearchText = searchText.ToLowerInvariant();

            TreeView activeTree = CallTree.Visible ? CallTree : ApiTree;

            if (string.IsNullOrWhiteSpace(_treeSearchText))
            {
                // Show all nodes
                ShowAllTreeNodes(activeTree);
                return;
            }

            // Filter nodes
            activeTree.BeginUpdate();

            foreach (TreeNode rootNode in activeTree.Nodes)
            {
                FilterTreeNodeRecursive(rootNode);
            }

            activeTree.EndUpdate();
        }

        /// <summary>
        /// Recursively filters tree nodes.
        /// A node is visible if it or any of its children match the search text.
        /// _treeSearchText is already lower-case; compare without allocating per-node.
        /// </summary>
        /// <returns>True if this node or any child matches.</returns>
        private bool FilterTreeNodeRecursive(TreeNode node)
        {
            bool hasMatch = false;
            bool nodeMatches = node.Text.IndexOf(_treeSearchText, StringComparison.OrdinalIgnoreCase) >= 0;

            // Check children first
            foreach (TreeNode child in node.Nodes)
            {
                if (FilterTreeNodeRecursive(child))
                    hasMatch = true;
            }

            // Show node if it matches or has matching children
            if (nodeMatches || hasMatch)
            {
                node.BackColor = nodeMatches ? Color.Yellow : Color.Transparent;
                // Expand matching nodes to show context
                if (hasMatch && !nodeMatches)
                    node.Expand();
                return true;
            }
            else
            {
                node.BackColor = Color.Transparent;
                node.Collapse();
                return false;
            }
        }

        /// <summary>
        /// Shows all tree nodes (clears filter).
        /// </summary>
        private void ShowAllTreeNodes(TreeView tree)
        {
            tree.BeginUpdate();

            foreach (TreeNode rootNode in tree.Nodes)
            {
                ClearTreeNodeFilter(rootNode);
            }

            tree.EndUpdate();
        }

        /// <summary>
        /// Recursively clears filter highlighting from nodes.
        /// </summary>
        private void ClearTreeNodeFilter(TreeNode node)
        {
            node.BackColor = Color.Transparent;

            foreach (TreeNode child in node.Nodes)
            {
                ClearTreeNodeFilter(child);
            }
        }

        /// <summary>
        /// Initializes the tree search textbox with placeholder text.
        /// </summary>
        private void InitializeTreeSearchBox()
        {
            if (treeSearchTextBox != null)
            {
                treeSearchTextBox.Text = Resources.TREE_SEARCH_PLACEHOLDER;
                treeSearchTextBox.ForeColor = SystemColors.GrayText;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // FEATURE: Bookmark Lines (2.8)
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Toggles bookmark on currently selected line.
        /// Keyboard shortcut: Ctrl+B
        /// </summary>
        private void ToggleBookmarkOnCurrentLine()
        {
            if (logListView.SelectedIndices.Count == 0)
            {
                StatusFileName.Text = Resources.STATUS_NO_LINE_SELECTED;
                return;
            }

            int selectedIndex = logListView.SelectedIndices[0];
            if (selectedIndex < 0 || selectedIndex >= _virtualLines.Count)
                return;

            int lineNumber = _virtualLines[selectedIndex].LineNumber;
            bool added = _bookmarkService.ToggleBookmark(lineNumber);

            // Update visual indication (mark with special color)
            RefreshBookmarkVisuals();

            string message = added 
                ? string.Format(Resources.MSG_BOOKMARK_ADDED, lineNumber)
                : string.Format(Resources.MSG_BOOKMARK_REMOVED, lineNumber);

            StatusFileName.Text = message;

            // Save bookmarks
            _bookmarkService.SaveBookmarks();
        }

        /// <summary>
        /// Navigates to the next bookmark.
        /// Keyboard shortcut: F2
        /// </summary>
        private void NavigateToNextBookmark()
        {
            if (_bookmarkService.Count == 0)
            {
                StatusFileName.Text = Resources.MSG_NO_BOOKMARKS_PRESS_CTRL_B;
                return;
            }

            int currentLine = 1;
            if (logListView.SelectedIndices.Count > 0)
            {
                int idx = logListView.SelectedIndices[0];
                currentLine = _virtualLines[idx].LineNumber;
            }

            int nextBookmark = _bookmarkService.GetNextBookmark(currentLine);
            if (nextBookmark > 0)
            {
                JumpToLine(nextBookmark);
                StatusFileName.Text = string.Format(Resources.MSG_BOOKMARK_COUNT, 
                    _bookmarkService.GetAllBookmarksSorted().IndexOf(nextBookmark) + 1,
                    _bookmarkService.Count);
            }
        }

        /// <summary>
        /// Navigates to the previous bookmark.
        /// Keyboard shortcut: Shift+F2
        /// </summary>
        private void NavigateToPreviousBookmark()
        {
            if (_bookmarkService.Count == 0)
            {
                StatusFileName.Text = Resources.MSG_NO_BOOKMARKS_PRESS_CTRL_B;
                return;
            }

            int currentLine = _virtualLines.Count;
            if (logListView.SelectedIndices.Count > 0)
            {
                int idx = logListView.SelectedIndices[0];
                currentLine = _virtualLines[idx].LineNumber;
            }

            int prevBookmark = _bookmarkService.GetPreviousBookmark(currentLine);
            if (prevBookmark > 0)
            {
                JumpToLine(prevBookmark);
                StatusFileName.Text = string.Format(Resources.MSG_BOOKMARK_COUNT, 
                    _bookmarkService.GetAllBookmarksSorted().IndexOf(prevBookmark) + 1,
                    _bookmarkService.Count);
            }
        }

        /// <summary>
        /// Clears all bookmarks.
        /// </summary>
        private void ClearAllBookmarks()
        {
            if (_bookmarkService.Count == 0)
                return;

            var result = MessageBox.Show(
                string.Format(Resources.PROMPT_CLEAR_BOOKMARKS, _bookmarkService.Count),
                Resources.DIALOG_TITLE_CLEAR_BOOKMARKS,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _bookmarkService.ClearAllBookmarks();
                _bookmarkService.SaveBookmarks();
                RefreshBookmarkVisuals();
                StatusFileName.Text = Resources.MSG_ALL_BOOKMARKS_CLEARED;
            }
        }

        /// <summary>
        /// Refreshes visual indication of bookmarked lines.
        /// Bookmarked lines show with a blue background.
        /// </summary>
        private void RefreshBookmarkVisuals()
        {
            if (_virtualLines.Count == 0)
                return;

            for (int i = 0; i < _virtualLines.Count; i++)
            {
                var vl = _virtualLines[i];
                int lineNum = vl.LineNumber;

                if (_bookmarkService.IsBookmarked(lineNum))
                {
                    // Mark with blue background
                    _virtualLines[i] = new VirtualLogLine
                    {
                        LineNumber = vl.LineNumber,
                        Text = vl.Text,
                        BackColour = Color.FromArgb(200, 230, 255) // Light blue
                    };
                }
                else if (vl.BackColour == Color.FromArgb(200, 230, 255))
                {
                    // Remove bookmark color
                    _virtualLines[i] = new VirtualLogLine
                    {
                        LineNumber = vl.LineNumber,
                        Text = vl.Text,
                        BackColour = GetLineColour(vl.Text)
                    };
                }
            }

            logListView.Invalidate();
        }

        /// <summary>
        /// Shows a list of all bookmarks.
        /// </summary>
        private void ShowBookmarkList()
        {
            if (_bookmarkService.Count == 0)
            {
                MessageBox.Show(Resources.ERR_NO_BOOKMARKS,
                    Resources.DIALOG_TITLE_BOOKMARKS, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var bookmarks = _bookmarkService.GetAllBookmarksSorted();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(string.Format(Resources.BOOKMARKS_LIST_HEADER, bookmarks.Count));
            sb.AppendLine();

            foreach (var lineNum in bookmarks)
            {
                // Find the line text
                _lineIndexMap.TryGetValue(lineNum, out int idx);
                if (idx >= 0 && idx < _virtualLines.Count && _virtualLines[idx].LineNumber == lineNum)
                {
                    string text = _virtualLines[idx].Text;
                    if (text.Length > 80)
                        text = text.Substring(0, 77) + "...";
                    sb.AppendLine(string.Format(Resources.BOOKMARK_LINE_FORMAT, lineNum, text));
                }
                else
                {
                    sb.AppendLine(string.Format(Resources.BOOKMARK_LINE_SIMPLE, lineNum));
                }
            }

            MessageBox.Show(sb.ToString(), Resources.DIALOG_TITLE_BOOKMARKS, 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // FEATURE: Timeline Panel Event Handler
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Handles timeline entry selection - jumps to the line in log view.
        /// </summary>
        private void TimelinePanel_EntrySelected(object sender, Managers.TimelinePanel.TimelineEntry entry)
        {
            if (entry != null && entry.LineNumber > 0)
            {
                ScrollLogToLine(entry.LineNumber);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // FEATURE: Export Tree as JSON/XML
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Exports call tree as JSON.
        /// </summary>
        private void exportTreeJsonMenuItem_Click(object sender, EventArgs e)
        {
            if (_lastEntries == null || _lastEntries.Count == 0)
            {
                MessageBox.Show(Resources.ERR_NO_CALL_TREE_DATA,
                    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = Resources.FILE_FILTER_JSON_FILES;
                dlg.FileName = GetSafeBaseName(_currentFilePath) + Resources.FILENAME_SUFFIX_CALLTREE_JSON;
                dlg.InitialDirectory = string.IsNullOrEmpty(_currentFilePath)
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : GetSafeDirectory(_currentFilePath);

                if (dlg.ShowDialog() != DialogResult.OK) return;

                try
                {
                    var callTree = _parserService.BuildCallTree(_lastEntries);
                    var exporter = new Services.Export.TreeExportService();
                    exporter.ExportToJson(callTree, dlg.FileName);

                    MessageBox.Show(string.Format(Resources.MSG_CALL_TREE_EXPORTED_JSON, dlg.FileName),
                        Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(Resources.ERR_EXPORT_TREE_FAILED, ex.Message),
                        Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Exports call tree as XML.
        /// </summary>
        private void exportTreeXmlMenuItem_Click(object sender, EventArgs e)
        {
            if (_lastEntries == null || _lastEntries.Count == 0)
            {
                MessageBox.Show(Resources.ERR_NO_CALL_TREE_DATA,
                    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = Resources.FILE_FILTER_XML_FILES;
                dlg.FileName = GetSafeBaseName(_currentFilePath) + Resources.FILENAME_SUFFIX_CALLTREE_XML;
                dlg.InitialDirectory = string.IsNullOrEmpty(_currentFilePath)
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : GetSafeDirectory(_currentFilePath);

                if (dlg.ShowDialog() != DialogResult.OK) return;

                try
                {
                    var callTree = _parserService.BuildCallTree(_lastEntries);
                    var exporter = new Services.Export.TreeExportService();
                    exporter.ExportToXml(callTree, dlg.FileName);

                    MessageBox.Show(string.Format(Resources.MSG_CALL_TREE_EXPORTED_XML, dlg.FileName),
                        Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(Resources.ERR_EXPORT_TREE_FAILED, ex.Message),
                        Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Exports timeline visualization as an image.
        /// </summary>
        private void exportTimelineMenuItem_Click(object sender, EventArgs e)
        {
            if (timelinePanel == null || _lastEntries == null || _lastEntries.Count == 0)
            {
                MessageBox.Show(Resources.ERR_NO_TIMELINE_DATA,
                    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = Resources.FILE_FILTER_IMAGE_FILES;
                dlg.FileName = GetSafeBaseName(_currentFilePath) + Resources.FILENAME_SUFFIX_TIMELINE_PNG;
                dlg.InitialDirectory = string.IsNullOrEmpty(_currentFilePath)
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : GetSafeDirectory(_currentFilePath);

                if (dlg.ShowDialog() != DialogResult.OK) return;

                try
                {
                    var image = timelinePanel.ExportAsImage(1920, 1080);

                    // Determine format from extension
                    string ext = Path.GetExtension(dlg.FileName).ToLowerInvariant();
                    var format = System.Drawing.Imaging.ImageFormat.Png;

                    if (ext == ".jpg" || ext == ".jpeg")
                        format = System.Drawing.Imaging.ImageFormat.Jpeg;
                    else if (ext == ".bmp")
                        format = System.Drawing.Imaging.ImageFormat.Bmp;

                    image.Save(dlg.FileName, format);
                    image.Dispose();

                    MessageBox.Show(string.Format(Resources.MSG_TIMELINE_EXPORTED, dlg.FileName),
                        Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(Resources.ERR_EXPORT_TIMELINE_FAILED, ex.Message),
                        Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Exports flame graph visualization as an image.
        /// </summary>
        private void exportFlameGraphMenuItem_Click(object sender, EventArgs e)
        {
            if (flameGraphPanel == null || _lastEntries == null || _lastEntries.Count == 0)
            {
                MessageBox.Show(Resources.ERR_NO_FLAME_GRAPH_DATA,
                    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = Resources.FILE_FILTER_IMAGE_FILES;
                dlg.FileName = GetSafeBaseName(_currentFilePath) + Resources.FILENAME_SUFFIX_FLAMEGRAPH_PNG;
                dlg.InitialDirectory = string.IsNullOrEmpty(_currentFilePath)
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : GetSafeDirectory(_currentFilePath);

                if (dlg.ShowDialog() != DialogResult.OK) return;

                try
                {
                    var image = flameGraphPanel.ExportAsImage(1920, 1080);

                    // Determine format from extension
                    string ext = Path.GetExtension(dlg.FileName).ToLowerInvariant();
                    var format = System.Drawing.Imaging.ImageFormat.Png;

                    if (ext == ".jpg" || ext == ".jpeg")
                        format = System.Drawing.Imaging.ImageFormat.Jpeg;
                    else if (ext == ".bmp")
                        format = System.Drawing.Imaging.ImageFormat.Bmp;

                    image.Save(dlg.FileName, format);
                    image.Dispose();

                    MessageBox.Show(string.Format(Resources.MSG_FLAME_GRAPH_EXPORTED, dlg.FileName),
                        Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(Resources.ERR_EXPORT_FLAME_GRAPH_FAILED, ex.Message),
                        Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // BOOKMARK MENU HANDLERS
        // ═══════════════════════════════════════════════════════════════════════

        private void toggleBookmarkMenuItem_Click(object sender, EventArgs e)
        {
            ToggleBookmarkOnCurrentLine();
        }

        private void nextBookmarkMenuItem_Click(object sender, EventArgs e)
        {
            NavigateToNextBookmark();
        }

        private void previousBookmarkMenuItem_Click(object sender, EventArgs e)
        {
            NavigateToPreviousBookmark();
        }

        private void showBookmarksMenuItem_Click(object sender, EventArgs e)
        {
            ShowBookmarkList();
        }

        private void clearBookmarksMenuItem_Click(object sender, EventArgs e)
        {
            ClearAllBookmarks();
        }

        // ═══════════════════════════════════════════════════════════════════════
        // COPY WITH HEADERS MENU HANDLER
        // ═══════════════════════════════════════════════════════════════════════

        private void copyWithHeadersMenuItem_Click(object sender, EventArgs e)
        {
            CopySelectedLinesToClipboard(includeHeaders: true);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // CLEAR FILTER MENU HANDLER
        // ═══════════════════════════════════════════════════════════════════════

        private void clearFilterMenuItem_Click(object sender, EventArgs e)
        {
            ClearFilter();
        }

        // ═══════════════════════════════════════════════════════════════════════
        // TAB VISIBILITY HANDLERS
        // ═══════════════════════════════════════════════════════════════════════

        private void showFlameGraphTabMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (flameGraphTab != null && mainTabControl != null)
            {
                if (showFlameGraphTabMenuItem.Checked)
                {
                    if (!mainTabControl.TabPages.Contains(flameGraphTab))
                        mainTabControl.TabPages.Add(flameGraphTab);
                }
                else
                {
                    if (mainTabControl.TabPages.Contains(flameGraphTab))
                        mainTabControl.TabPages.Remove(flameGraphTab);
                }
            }
        }

        private void showTimelineTabMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (timelineTab != null && mainTabControl != null)
            {
                if (showTimelineTabMenuItem.Checked)
                {
                    if (!mainTabControl.TabPages.Contains(timelineTab))
                        mainTabControl.TabPages.Add(timelineTab);
                }
                else
                {
                    if (mainTabControl.TabPages.Contains(timelineTab))
                        mainTabControl.TabPages.Remove(timelineTab);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // FEATURE 5: Font Selection (H5)
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Handler for View → Select Font menu item.
        /// </summary>
        private void selectFontMenuItem_Click(object sender, EventArgs e)
        {
            SelectLogFont();
        }

        /// <summary>
        /// Shows font selection dialog and applies chosen font to log view.
        /// </summary>
        private void SelectLogFont()
        {
            try
            {
                // Set current font in dialog
                logFontDialog.Font = logListView.Font;

                if (logFontDialog.ShowDialog() == DialogResult.OK)
                {
                    // Apply font to log list view
                    logListView.Font = logFontDialog.Font;

                    // Save to settings
                    if (_appSettings != null)
                    {
                        _appSettings.LogFontFamily = logFontDialog.Font.FontFamily.Name;
                        _appSettings.LogFontSize = logFontDialog.Font.Size;
                        _appSettings.LogFontStyle = logFontDialog.Font.Style;

                        // Save settings
                        _appSettings.Save();
                    }

                    StatusFileName.Text = string.Format(Resources.MSG_FONT_CHANGED, 
                        logFontDialog.Font.Name, logFontDialog.Font.Size);

                    // Clear status after 3 seconds
                    var timer = new System.Windows.Forms.Timer();
                    timer.Interval = 3000;
                    timer.Tick += (s, args) =>
                    {
                        StatusFileName.Text = GetSafeFileName(_currentFilePath);
                        timer.Stop();
                        timer.Dispose();
                    };
                    timer.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Resources.ERR_FONT_CHANGE_FAILED, ex.Message), 
                    Resources.TITLE, 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Loads saved font from settings.
        /// Call this during form initialization.
        /// </summary>
        private void LoadLogFont()
        {
            try
            {
                if (_appSettings != null && 
                    !string.IsNullOrEmpty(_appSettings.LogFontFamily))
                {
                    var font = new Font(
                        _appSettings.LogFontFamily,
                        _appSettings.LogFontSize > 0 ? _appSettings.LogFontSize : 9.0f,
                        _appSettings.LogFontStyle);

                    logListView.Font = font;
                }
            }
            catch (Exception ex)
            {
                // Use default font if loading fails
                System.Diagnostics.Debug.WriteLine(string.Format("Failed to load font: {0}", ex.Message));
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // FEATURE F4: Dependency Graph Initialization
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Initializes the dependency graph tab and panel programmatically.
        /// Creates tab, panel, and reset button.
        /// NOTE: Currently commented out - files need to be added to csproj first.
        /// TO ENABLE: 
        /// 1. Close solution in Visual Studio
        /// 2. Edit Cad3PLogBrowser.csproj
        /// 3. Add these lines after FlameGraphPanel:
        ///    <Compile Include="Managers\DependencyGraphPanel.cs">
        // ═══════════════════════════════════════════════════════════════════════
        // FEATURES L2-L6: AI Assistant Initialization
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Initializes AI Assistant panel and tab.
        /// Creates AI panel with chat interface and analysis capabilities.
        /// </summary>
        private void InitAiPanel()
        {
            // Create AI tab
            _aiTab = new TabPage(Resources.TAB_NAME_AI_ASSISTANT)
            {
                Name = "aiTab",
                UseVisualStyleBackColor = true
            };

            // Create AI panel
            _aiPanel = new Managers.AiAssistantPanel()
            {
                Dock = DockStyle.Fill,
                Name = "aiAssistantPanel"
            };

            // Initialize AI service
            _aiService = new Services.Analysis.AiLogService(_appSettings?.ClaudeApiKey ?? "", _appSettings?.UseClaudeApi ?? false);

            // Wire up events
            _aiPanel.QuerySubmitted              += AiPanel_QuerySubmitted;
            _aiPanel.SummarizeRequested          += AiPanel_SummarizeRequested;
            _aiPanel.DetectAnomaliesRequested    += AiPanel_DetectAnomaliesRequested;
            _aiPanel.AnalyzePerformanceRequested += AiPanel_AnalyzePerformanceRequested;
            _aiPanel.FindPatternsRequested       += AiPanel_FindPatternsRequested;
            _aiPanel.RootCauseRequested          += AiPanel_RootCauseRequested;
            _aiPanel.BugReportRequested          += AiPanel_BugReportRequested;
            _aiPanel.ChatMessageSubmitted        += AiPanel_ChatMessageSubmitted;
            _aiPanel.SetApiMode(_appSettings?.UseClaudeApi ?? false);

            // Add panel to tab
            _aiTab.Controls.Add(_aiPanel);

            // Add tab to main control
            if (mainTabControl != null)
            {
                mainTabControl.TabPages.Add(_aiTab);
                ApplyTabIcons();   // re-run so the new tab gets its icon
            }

            // Create View menu item
            var showAiMenuItem = new ToolStripMenuItem(Resources.MENU_SHOW_AI_ASSISTANT)
            {
                Name        = "showAiMenuItem",
                CheckOnClick = true,
                Checked      = true,
                Image        = IconGenerator.CreateTabAiIcon(IconGenerator.IconSize.Small)
            };
            showAiMenuItem.CheckedChanged += (s, e) =>
            {
                if (_aiTab != null && mainTabControl != null)
                {
                    if (showAiMenuItem.Checked)
                    {
                        if (!mainTabControl.TabPages.Contains(_aiTab))
                            mainTabControl.TabPages.Add(_aiTab);
                    }
                    else
                    {
                        if (mainTabControl.TabPages.Contains(_aiTab))
                            mainTabControl.TabPages.Remove(_aiTab);
                    }
                }
            };

            // Add to View menu
            if (tabsMenuItem != null)
            {
                tabsMenuItem.DropDownItems.Add(showAiMenuItem);
            }

            // Apply initial theme
            _aiPanel?.UpdateTheme();
        }

        private async void AiPanel_QuerySubmitted(object sender, string query)
        {
            if (_aiService == null || _aiPanel == null || _lastEntries == null) return;

            try
            {
                _aiPanel.ShowThinking(true);
                var stats = Services.Analysis.AiLogService.BuildAggregateStats(_lastEntries, 
                    Services.Analysis.AiLogService.ConvertPerfStats(_apiPerfStats));
                var perfStats = Services.Analysis.AiLogService.ConvertPerfStats(_apiPerfStats);
                var response = await _aiService.AnalyzeAsync(query, stats, perfStats, _lastEntries);
                _aiPanel.ShowResponse(response);
            }
            catch (Exception ex)
            {
                _aiPanel.ShowError(string.Format(Resources.ERR_AI_QUERY_FAILED, ex.Message));
            }
            finally
            {
                _aiPanel.ShowThinking(false);
            }
        }

        private async void AiPanel_SummarizeRequested(object sender, EventArgs e)
        {
            if (_aiService == null || _aiPanel == null || _lastEntries == null) return;

            try
            {
                _aiPanel.ShowThinking(true);
                var stats = Services.Analysis.AiLogService.BuildAggregateStats(_lastEntries, 
                    Services.Analysis.AiLogService.ConvertPerfStats(_apiPerfStats));
                var perfStats = Services.Analysis.AiLogService.ConvertPerfStats(_apiPerfStats);
                var summary = await _aiService.SummarizeAsync(stats, perfStats);
                _aiPanel.ShowResponse(summary);
            }
            catch (Exception ex)
            {
                _aiPanel.ShowError(string.Format(Resources.ERR_AI_SUMMARIZE_FAILED, ex.Message));
            }
            finally
            {
                _aiPanel.ShowThinking(false);
            }
        }

        private async void AiPanel_DetectAnomaliesRequested(object sender, EventArgs e)
        {
            if (_aiService == null || _aiPanel == null || _lastEntries == null) return;

            try
            {
                _aiPanel.ShowThinking(true);
                var stats = Services.Analysis.AiLogService.BuildAggregateStats(_lastEntries, 
                    Services.Analysis.AiLogService.ConvertPerfStats(_apiPerfStats));
                var perfStats = Services.Analysis.AiLogService.ConvertPerfStats(_apiPerfStats);
                var anomalies = await _aiService.DetectAnomaliesAsync(stats, perfStats);
                _aiPanel.ShowResponse(anomalies);
            }
            catch (Exception ex)
            {
                _aiPanel.ShowError(string.Format(Resources.ERR_AI_ANOMALY_DETECTION_FAILED, ex.Message));
            }
            finally
            {
                _aiPanel.ShowThinking(false);
            }
        }

        private async void AiPanel_RootCauseRequested(object sender, EventArgs e)
        {
            if (_aiService == null || _aiPanel == null || _lastEntries == null) return;
            try
            {
                _aiPanel.ShowThinking(true);
                var stats = Services.Analysis.AiLogService.BuildAggregateStats(_lastEntries, Services.Analysis.AiLogService.ConvertPerfStats(_apiPerfStats));
                var result = await _aiService.SuggestRootCauseAsync(stats, Services.Analysis.AiLogService.ConvertPerfStats(_apiPerfStats), stats.ErrorCount, stats.WarningCount);
                _aiPanel.ShowResponse(result);
            }
            catch (Exception ex) { _aiPanel.ShowError(string.Format(Resources.ERR_AI_ROOT_CAUSE_FAILED, ex.Message)); }
            finally { _aiPanel.ShowThinking(false); }
        }

        private async void AiPanel_BugReportRequested(object sender, EventArgs e)
        {
            if (_aiService == null || _aiPanel == null || _lastEntries == null) return;
            try
            {
                _aiPanel.ShowThinking(true);
                var stats = Services.Analysis.AiLogService.BuildAggregateStats(_lastEntries, Services.Analysis.AiLogService.ConvertPerfStats(_apiPerfStats));
                string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
                var result = await _aiService.GenerateBugReportAsync(stats, Services.Analysis.AiLogService.ConvertPerfStats(_apiPerfStats), version);
                _aiPanel.ShowResponse(result);
            }
            catch (Exception ex) { _aiPanel.ShowError(string.Format(Resources.ERR_AI_BUG_REPORT_FAILED, ex.Message)); }
            finally { _aiPanel.ShowThinking(false); }
        }

        private async void AiPanel_ChatMessageSubmitted(object sender, string message)
        {
            if (_aiService == null || _aiPanel == null || _lastEntries == null) return;
            try
            {
                _aiPanel.ShowThinking(true);
                var stats = Services.Analysis.AiLogService.BuildAggregateStats(_lastEntries, Services.Analysis.AiLogService.ConvertPerfStats(_apiPerfStats));
                var result = await _aiService.ChatAsync(message, _aiPanel.ChatHistory, stats, Services.Analysis.AiLogService.ConvertPerfStats(_apiPerfStats));
                _aiPanel.AppendChatTurn("assistant", result);
            }
            catch (Exception ex) { _aiPanel.ShowError(string.Format(Resources.ERR_AI_CHAT_FAILED, ex.Message)); }
            finally { _aiPanel.ShowThinking(false); }
        }

        /// <summary>Call after settings are saved so AI service picks up new key/mode.</summary>
        public void RefreshAiService()
        {
            _aiService?.UpdateConfig(_appSettings?.ClaudeApiKey ?? "", _appSettings?.UseClaudeApi ?? false);
            _aiPanel?.SetApiMode(_appSettings?.UseClaudeApi ?? false);
        }

        private async void AiPanel_AnalyzePerformanceRequested(object sender, EventArgs e)
        {
            if (_aiService == null || _aiPanel == null || _apiPerfStats == null) return;

            try
            {
                _aiPanel.ShowThinking(true);
                var perfStats = Services.Analysis.AiLogService.ConvertPerfStats(_apiPerfStats);
                var analysis = await _aiService.AnalyzePerformanceAsync(perfStats);
                _aiPanel.ShowResponse(analysis);
            }
            catch (Exception ex)
            {
                _aiPanel.ShowError(string.Format(Resources.ERR_AI_PERFORMANCE_ANALYSIS_FAILED, ex.Message));
            }
            finally
            {
                _aiPanel.ShowThinking(false);
            }
        }

        private async void AiPanel_FindPatternsRequested(object sender, EventArgs e)
        {
            if (_aiService == null || _aiPanel == null || _lastEntries == null) return;

            try
            {
                _aiPanel.ShowThinking(true);
                var patterns = await _aiService.FindPatternsAsync(_lastEntries);
                _aiPanel.ShowResponse(patterns);
            }
            catch (Exception ex)
            {
                _aiPanel.ShowError(string.Format(Resources.ERR_AI_PATTERN_FINDING_FAILED, ex.Message));
            }
            finally
            {
                _aiPanel.ShowThinking(false);
            }
        }
    }
}
