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
        private readonly Services.Analysis.DependencyGraphService _dependencyGraphService;
        private readonly Services.Core.MergeLogService _mergeLogService;
        private Services.Analysis.AiLogService _aiService;
        private Managers.AiAssistantPanel _aiPanel;
        private TabPage _aiTab;
        private readonly BookmarkService   _bookmarkService;

        // ── State ─────────────────────────────────────────────────────────────
        private string            _currentFilePath = string.Empty;
        private bool              _isLoading       = false;
        private List<string>      _allLines        = new List<string>();
        private List<ApiCallNode> _apiNodes        = new List<ApiCallNode>();
        private AppSettings       _appSettings;
        private HashSet<int>      _bookmarkedLines  = new HashSet<int>(); // 1-based line numbers
        private List<LogEntry>    _lastEntries      = new List<LogEntry>(); // for ENTER/EXIT jump
        private bool              _isFormLoaded     = false; // Flag to prevent saving during initialization

        // Feature C2: Lazy loading for large trees
        private const int LAZY_LOAD_THRESHOLD = 50000; // Enable lazy loading for 50k+ nodes
        private Dictionary<TreeNode, List<CallStackNode>> _lazyChildrenMap = new Dictionary<TreeNode, List<CallStackNode>>();

        // Feature F4: Dependency graph tab and panel
        private TabPage _dependencyGraphTab;
        private Managers.DependencyGraphPanel _dependencyGraphPanel;
        private Button _depGraphResetButton;
        private ToolStripMenuItem _showDependencyGraphMenuItem;


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
                case TabId.Log:         return showTab1MenuItem;
                case TabId.Raw:         return showTab2MenuItem;
                case TabId.Performance: return showTab3MenuItem;
                case TabId.LogDetails:  return showTab4MenuItem;
                case TabId.CallGraph:   return null; // No specific menu item
                case TabId.FlameGraph:  return showFlameGraphTabMenuItem;
                case TabId.Timeline:    return showTimelineTabMenuItem;
                default:                return null;
            }
        }

        private void SetTabVisible(TabPage tab, bool visible)
        {
            if (tab == null) return;

            bool currentlyVisible = mainTabControl.TabPages.Contains(tab);
            if (visible == currentlyVisible) return;

            if (visible)
            {
                mainTabControl.TabPages.Add(tab);
                return;
            }

            if (mainTabControl.TabPages.Count <= 1)
            {
                if (ReferenceEquals(tab, logTab)) showTab1MenuItem.Checked = true;
                if (ReferenceEquals(tab, performanceTab)) showTab2MenuItem.Checked = true;
                if (ReferenceEquals(tab, logDetailTab)) showTab3MenuItem.Checked = true;
                if (ReferenceEquals(tab, callGraphTab)) showTab4MenuItem.Checked = true;
                return;
            }

            mainTabControl.TabPages.Remove(tab);
        }

        private void showTab1MenuItem_CheckedChanged(object sender, EventArgs e) =>
            SetTabVisible(logTab, showTab1MenuItem.Checked);

        private void showTab2MenuItem_CheckedChanged(object sender, EventArgs e) =>
            SetTabVisible(performanceTab, showTab2MenuItem.Checked);

        private void showTab3MenuItem_CheckedChanged(object sender, EventArgs e) =>
            SetTabVisible(logDetailTab, showTab3MenuItem.Checked);

        private void showTab4MenuItem_CheckedChanged(object sender, EventArgs e) =>
            SetTabVisible(callGraphTab, showTab4MenuItem.Checked);

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
            // Create theme toggle button
            _themeToggleButton = new ToolStripButton
            {
                Text = "☀", // Sun icon for light theme (moon ☾ for dark)
                ToolTipText = Resources.TOOLTIP_THEME_TOGGLE_DEFAULT,
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = new Font("Segoe UI", 12f),
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
            // Toggle theme
            _appSettings.Theme = _appSettings.Theme == "Dark" ? "Light" : "Dark";
            ApplyTheme();
            UpdateThemeButtonIcon();
        }

        private void UpdateThemeButtonIcon()
        {
            if (_themeToggleButton != null)
            {
                // Sun ☀ for light theme (click to go to light), Moon ☾ for dark theme (click to go to dark)
                bool isDark = _appSettings.Theme == "Dark";
                _themeToggleButton.Text = isDark ? "☀" : "☾";
                _themeToggleButton.ToolTipText = isDark 
                    ? Resources.TOOLTIP_THEME_TOGGLE_TO_LIGHT 
                    : Resources.TOOLTIP_THEME_TOGGLE_TO_DARK;
                _themeToggleButton.ForeColor = isDark 
                    ? Color.FromArgb(255, 200, 0)  // Yellow sun in dark mode
                    : Color.FromArgb(100, 100, 150); // Blue moon in light mode
            }
        }

        private void BuildMruMenu()
        {
            // Remove existing Recent Files menu if present
            if (_recentFilesMenuItem != null)
            {
                fileMenuItem.DropDownItems.Remove(_recentFilesMenuItem);
                fileMenuItem.DropDownItems.Remove(_recentFilesSeparator);
            }

            // Only show if there are recent files
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

        // ── #7 Virtual mode backing store ─────────────────────────────────────
        // Each VirtualLogLine holds exactly what the ListView needs for one row.
        private struct VirtualLogLine
        {
            public string LineNumber;
            public string Text;
            public Color  BackColour;
        }
        private List<VirtualLogLine> _virtualLines = new List<VirtualLogLine>();

        // ── Construction ──────────────────────────────────────────────────────
        public MainForm()
        {
            InitializeComponent();

            _appSettings = AppSettings.Load();
            _settingsService  = new SettingsService(_appSettings);
            _searchService    = new SearchService();
            _parserService    = new LogParserService();
            _callGraphService = new CallGraphService();
            _dependencyGraphService = new Services.Analysis.DependencyGraphService();
            _mergeLogService  = new Services.Core.MergeLogService();
            _aiService        = new Services.Analysis.AiLogService();
            _logFileService   = new LogFileService(this);
            _bookmarkService  = new Services.Navigation.BookmarkService();
            _logFileService.FileChangedOnDisk += OnFileChangedOnDisk;

            RestoreSettings();
            InitTreeViews();
            InitDependencyGraphPanel();
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
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Force layout after form is fully loaded and sized
            LayoutTrees();

            // Set up F1 help key handling for context-sensitive help
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown_Help;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Force layout again when form is shown to ensure correct positioning
            LayoutTrees();
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

        public void ApplyTheme()
        {
            // EMERGENCY FIX: Suspend layout during theme change to prevent freeze
            this.SuspendLayout();

            try
            {
                // Set the theme based on settings
                var theme = _appSettings.Theme == "Dark" ? ThemeManager.Theme.Dark : ThemeManager.Theme.Light;
                ThemeManager.SetTheme(theme);

                // Apply to main form
                ThemeManager.ApplyTheme(this);

                // Manually update visualization panels (they handle their own child controls)
                flameGraphPanel?.UpdateTheme();
                timelinePanel?.UpdateTheme();

                // Apply icon size
                ApplyIconSize();

                // Refresh tree layout after theme/icon change
                LayoutTrees();

                // Update log-level colors based on theme
                UpdateLogColors();

                // Refresh the log view to apply new colors
                if (logListView.VirtualMode && _virtualLines.Count > 0)
                {
                    logListView.Invalidate();
                }

                // Refresh the call graph panel
                if (callGraphPanel != null)
                {
                    callGraphPanel.Invalidate();
                }

                // Refresh the performance view with theme-aware colors
                if (_apiPerfStats != null && _apiPerfStats.Count > 0)
                {
                    RenderPerformanceRows(_apiPerfStats, _lastTotalLines);
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

        private void SaveSettings()
        {
            try
            {
                // Update all settings in AppSettings object first (no I/O)
                _appSettings.SplitterDistance = mainSplitContainer.SplitterDistance;

                if (!string.IsNullOrEmpty(_currentFilePath))
                    _appSettings.InitialDirectory = Path.GetDirectoryName(_currentFilePath);

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

            // Feature G5: Enhanced status bar with file info
            // Feature B10: Include error/warning counts
            string fileInfo = string.Format("{0}  |  {1:N0} lines", 
                Path.GetFileName(_currentFilePath), _allLines.Count);

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
            string lineNum = _virtualLines[idx].LineNumber;
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
            ApiTree.ShowNodeToolTips = true; // Same as CallTree
            CallTree.ShowLines = CallTree.ShowPlusMinus = true;
            CallTree.ShowNodeToolTips = true;
            CallTree.HideSelection = false;

            CallTreeButton.CheckedChanged       += (s, e) => SyncTreeVisibility();
            ApiTreeButton.CheckedChanged        += (s, e) => SyncTreeVisibility();

            // Add context menu handlers for both trees
            ApiTree.MouseUp += ApiTree_MouseUpForSorting;
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
            _lastEntries  = entries;  // store for ENTER/EXIT jump feature
            _apiNodes     = _parserService.BuildApiList(entries);
            var callTree  = _parserService.BuildCallTree(entries);
            var perfStats = _parserService.BuildPerformanceStats(callTree);
            var graph     = _callGraphService.Build(entries);

            PopulateApiTree(_apiNodes);
            PopulateCallTree(callTree);
            PopulatePerformanceTab(perfStats, lines.Count);
            callGraphPanel.LoadGraph(graph);

            // F4: Load dependency graph
            if (_dependencyGraphPanel != null)
            {
                var depGraph = _dependencyGraphService.Build(entries);
                _dependencyGraphPanel.Load(depGraph);
            }

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
            root.NodeFont = new System.Drawing.Font(ApiTree.Font, System.Drawing.FontStyle.Bold);

            foreach (var node in apiNodes)
            {
                // Check if all occurrences have matching ENTER/EXIT pairs
                bool allMatched = AreAllApiCallsMatched(node.ApiName);
                // Feature C3: Show call count in API tree root node
                string apiLabel = string.Format("{0}  ({1} calls)", node.ApiName, node.LineNumbers.Count);
                var apiRoot = new TreeNode(apiLabel)
                {
                    Tag             = node.FirstLine,
                    ImageIndex      = allMatched ? 0 : 1,
                    SelectedImageIndex = allMatched ? 0 : 1
                };

                // Children: "ApiName — Ln N" per invocation
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
                root.Nodes.Add(apiRoot);
            }

            ApiTree.Nodes.Add(root);
            // Issue Fix: Start collapsed (only root expanded)
            root.Expand();
            // Don't expand first level - let users expand as needed

            ApiTree.EndUpdate();
        }

        private bool AreAllApiCallsMatched(string apiName)
        {
            if (_lastEntries == null) return true;
            int enters = 0, exits = 0;
            foreach (var e in _lastEntries)
            {
                if (!e.IsApiCall || e.ApiName != apiName) continue;
                if (e.IsCallEnter) enters++;
                else if (e.IsCallExit) exits++;
            }
            return enters == exits;
        }

        private void PopulateCallTree(List<CallStackNode> roots)
        {
            CallTree.BeginUpdate();
            CallTree.Nodes.Clear();
            _lazyChildrenMap.Clear(); // Clear lazy load cache

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

            // Wire up before expand event for lazy loading
            if (useLazyLoading)
            {
                CallTree.BeforeExpand += CallTree_BeforeExpand;
            }
        }

        // C2: Count total nodes in call tree
        private int CountTotalNodes(List<CallStackNode> nodes)
        {
            int count = 0;
            foreach (var node in nodes)
            {
                count++; // Count this node
                count += CountTotalNodes(node.Children); // Count children recursively
            }
            return count;
        }

        // C2: Lazy loading handler - load children when node is expanded
        private void CallTree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node == null) return;

            // Check if this node has lazy-loaded children waiting
            if (_lazyChildrenMap.ContainsKey(e.Node))
            {
                var children = _lazyChildrenMap[e.Node];

                // Remove placeholder
                e.Node.Nodes.Clear();

                // Add real children
                foreach (var child in children)
                {
                    e.Node.Nodes.Add(BuildTreeNode(child, useLazyLoading: true));
                }

                // Remove from map - children are now loaded
                _lazyChildrenMap.Remove(e.Node);

                StatusFileName.Text = string.Format(Resources.STATUS_LOADED_CHILDREN, children.Count, GetMethodNameFromNode(e.Node));
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
                    // Add placeholder node
                    var placeholder = new TreeNode(Resources.TREE_LAZY_LOAD_PLACEHOLDER)
                    {
                        Tag = -2, // Special tag for placeholder
                        ForeColor = Color.Gray,
                        NodeFont = new Font(CallTree.Font, FontStyle.Italic)
                    };
                    tn.Nodes.Add(placeholder);

                    // Store actual children for later loading
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
        private int  _perfSortColumn    = 2; // default: Total ms
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
                _perfSortAscending = e.Column == 0; // API name sorts ascending by default
            }
            RenderPerformanceRows(_apiPerfStats, _lastTotalLines);
        }

        private void RenderPerformanceRows(List<ApiPerfStats> stats, int totalLines)
        {
            _apiPerfStats  = stats;
            _lastTotalLines = totalLines;

            // Sort a copy
            var sorted = new List<ApiPerfStats>(stats);
            sorted.Sort((a, b) =>
            {
                int cmp;
                switch (_perfSortColumn)
                {
                    case 0:  cmp = string.Compare(a.ApiName, b.ApiName, StringComparison.OrdinalIgnoreCase); break;
                    case 1:  cmp = a.CallCount.CompareTo(b.CallCount);        break;
                    case 2:  cmp = a.TotalDurationMs.CompareTo(b.TotalDurationMs); break;
                    case 3:  cmp = a.AvgDurationMs.CompareTo(b.AvgDurationMs); break;
                    case 4:  cmp = a.MinDurationMs.CompareTo(b.MinDurationMs); break;
                    case 5:  cmp = a.MaxDurationMs.CompareTo(b.MaxDurationMs); break;
                    case 6:  cmp = a.SelfDurationMs.CompareTo(b.SelfDurationMs); break;
                    default: cmp = a.TotalDurationMs.CompareTo(b.TotalDurationMs); break;
                }
                return _perfSortAscending ? cmp : -cmp;
            });

            performanceView.BeginUpdate();
            performanceView.Items.Clear();

            // Summary row - use theme-aware color
            long sumTotal = 0; int sumCalls = 0;
            foreach (var s in stats) { sumTotal += s.TotalDurationMs; sumCalls += s.TimedCallCount; }

            var summary = new ListViewItem(Resources.PERF_SUMMARY_ROW_LABEL);
            summary.SubItems.Add(sumCalls.ToString());
            summary.SubItems.Add(sumTotal.ToString());
            summary.SubItems.Add("-"); summary.SubItems.Add("-");
            summary.SubItems.Add("-"); summary.SubItems.Add("-");
            summary.SubItems.Add(string.Format(Resources.PERF_SUMMARY_STATS, stats.Count, totalLines));

            // Theme-aware summary row color
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
            {
                var item = new ListViewItem(s.ApiName);
                item.SubItems.Add(s.CallCount.ToString());
                item.SubItems.Add(s.TotalDurationMs > 0 ? s.TotalDurationMs.ToString() : "-");
                item.SubItems.Add(s.AvgDurationMs   > 0 ? s.AvgDurationMs.ToString()   : "-");
                item.SubItems.Add(s.MinDurationMs   >= 0 ? s.MinDurationMs.ToString()   : "-");
                item.SubItems.Add(s.MaxDurationMs   > 0 ? s.MaxDurationMs.ToString()    : "-");
                item.SubItems.Add(s.SelfDurationMs  > 0 ? s.SelfDurationMs.ToString()   : "-");

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
                    // Use default theme background for normal items
                    item.BackColor = ThemeManager.BackgroundColor;
                    item.ForeColor = ThemeManager.ForegroundColor;
                }

                performanceView.Items.Add(item);
            }
            performanceView.EndUpdate();
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

            // Switch to Log Details tab to show it
            if (mainTabControl != null && logDetailTab != null &&
                mainTabControl.TabPages.Contains(logDetailTab))
                mainTabControl.SelectedTab = logDetailTab;
        }

        private void ApiTree_Click(object sender, EventArgs e) { }
        private void ApiTree_MouseClick(object sender, MouseEventArgs e) { }

        private void CallTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ScrollLogToLine(e.Node?.Tag);

            // D5: Cross-reference - highlight matching node in API Tree if both have data
            if (e.Node != null && ApiTree.Nodes.Count > 0)
            {
                string methodName = GetMethodNameFromNode(e.Node);
                if (!string.IsNullOrEmpty(methodName))
                {
                    // Don't trigger recursive selection events
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
            int idx = _virtualLines.FindIndex(v => v.LineNumber == lineNumber.ToString());
            if (idx < 0 || idx >= logListView.VirtualListSize) return;

            // Feature H1: Show 10 previous lines by scrolling appropriately
            int scrollToIdx = Math.Max(0, idx - 10);
            logListView.EnsureVisible(scrollToIdx);
            logListView.EnsureVisible(idx); // Make sure selected line is visible

            logListView.SelectedIndices.Clear();
            logListView.SelectedIndices.Add(idx);
            logListView.Focus();
            ShowLogDetail(idx);
        }

        // ── Log Details panel ─────────────────────────────────────────────────
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectionStatus();
            if (logListView.SelectedIndices.Count > 0)
                ShowLogDetail(logListView.SelectedIndices[0]);
        }

        private void ShowLogDetail(int idx)
        {
            if (idx < 0 || idx >= _virtualLines.Count) return;
            logDetailBox.Text = string.Format(Resources.LOG_DETAIL_FORMAT,
                _virtualLines[idx].LineNumber, _virtualLines[idx].Text);
        }

        // ── #7: Virtual mode handler ──────────────────────────────────────────
        private void listView1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex < 0 || e.ItemIndex >= _virtualLines.Count)
            {
                e.Item = new ListViewItem();
                return;
            }
            var vl   = _virtualLines[e.ItemIndex];
            var item = new ListViewItem(vl.LineNumber);
            item.SubItems.Add(vl.Text);
            item.BackColor = vl.BackColour;
            e.Item = item;
        }

        // ── #8: Call Graph reset button ───────────────────────────────────────
        private void callGraphResetButton_Click(object sender, EventArgs e) =>
            callGraphPanel.ResetView();

        // ── Drag-and-drop ─────────────────────────────────────────────────────
        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null && files.Length > 0) LoadFileAsync(files[0]);
        }

        // ── File loading ──────────────────────────────────────────────────────
        private async void LoadFileAsync(string filePath)
        {
            if (_isLoading) return;
            _isLoading = true;
            SetDocumentLoaded(false);
            FileStatus.Image = Resources.yellow;
            FileLoadProgress.Visible = true;
            FileLoadProgress.Value = 0;
            StatusFileName.Text = Resources.STATUS_LOADING;

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
                    }));
                });

                _allLines        = lines;
                _currentFilePath = filePath;
                _searchService.Reset();
                ClearHighlighting(); // Clear any previous search highlights

                // Load bookmarks for this file
                _bookmarkService.LoadBookmarks(filePath);

                // Show processing message
                StatusFileName.Text = Resources.STATUS_PROCESSING_LOG_DATA;
                FileLoadProgress.Value = 0;

                // Give UI a chance to update
                await Task.Delay(10);

                // Populate views with progress
                PopulateVirtualListView(_allLines);
                FileLoadProgress.Value = 33;
                StatusFileName.Text = Resources.STATUS_BUILDING_CALL_TREE;
                await Task.Delay(10);

                PopulateTrees(_allLines);
                FileLoadProgress.Value = 100;

                SetDocumentLoaded(true);
                FileStatus.Image = Resources.green_ball;
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
            }
        }

        // ── #7: Virtual mode population ───────────────────────────────────────
        /// <summary>
        /// Builds the backing store for virtual mode. No ListViewItems are created here —
        /// items are produced on demand in RetrieveVirtualItem. This makes loading
        /// 500k-line files near-instant.
        /// </summary>
        private void PopulateVirtualListView(IList<string> lines)
        {
            _virtualLines = new List<VirtualLogLine>(lines.Count);

            // Feature B10: Index errors and warnings
            _errorLines.Clear();
            _warningLines.Clear();
            _currentErrorIndex = -1;
            _currentWarningIndex = -1;

            for (int i = 0; i < lines.Count; i++)
            {
                int lineNumber = i + 1;
                Color backColor = GetLineColour(lines[i]);

                // Check if bookmarked - override color
                if (_bookmarkService.IsBookmarked(lineNumber))
                {
                    backColor = Color.FromArgb(200, 230, 255); // Light blue for bookmarks
                }

                _virtualLines.Add(new VirtualLogLine
                {
                    LineNumber = lineNumber.ToString(),
                    Text       = lines[i],
                    BackColour = backColor
                });

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
            }
            UpdateStatusBar();
        }

        private void PopulateVirtualListViewFiltered(IList<FilteredLine> filtered)
        {
            _virtualLines = new List<VirtualLogLine>(filtered.Count);
            foreach (var fl in filtered)
            {
                _virtualLines.Add(new VirtualLogLine
                {
                    LineNumber = fl.LineNumber.ToString(),
                    Text       = fl.Text,
                    BackColour = GetLineColour(fl.Text)
                });
            }

            // Safety check: ensure logListView is initialized
            if (logListView != null)
            {
                logListView.VirtualListSize = _virtualLines.Count;
                logListView.Invalidate();

                // Issue Fix: Auto-resize columns to fit content
                AutoResizeLogListColumns();
            }

            UpdateStatusBar();
        }

        // ── Icon Size Management ──────────────────────────────────────────────
        private void ApplyIconSize()
        {
            IconGenerator.IconSize iconSize;
            switch (_appSettings.ToolbarIconSize)
            {
                case "Small":
                    iconSize = IconGenerator.IconSize.Small;
                    mainToolStrip.ImageScalingSize = new Size(16, 16);
                    break;
                case "Large":
                    iconSize = IconGenerator.IconSize.Large;
                    mainToolStrip.ImageScalingSize = new Size(32, 32);
                    break;
                default: // Medium
                    iconSize = IconGenerator.IconSize.Medium;
                    mainToolStrip.ImageScalingSize = new Size(24, 24);
                    break;
            }

            // Generate and apply modern flat icons
            IconGenerator.GenerateAllIcons(iconSize, 
                out var openIcon, out var saveIcon, out var refreshIcon,
                out var copyIcon, out var findIcon, out var filterIcon,
                out var settingsIcon, out var helpIcon, out var expandIcon,
                out var collapseIcon, out var treeIcon, out var exportIcon,
                out var jumpIcon, out var errorIcon, out var warningIcon);

            // Apply icons to toolbar buttons
            OpenButton.Image = openIcon;
            SaveButton.Image = saveIcon;
            SaveToXLSButton.Image = exportIcon;
            RefreshButton.Image = refreshIcon;
            CopyButton.Image = copyIcon;
            FindButton.Image = findIcon;
            FindNextButton.Image = findIcon; // Reuse find icon
            FilterButton.Image = filterIcon;
            ExpandAllButton.Image = expandIcon;
            CollapseAllButton.Image = collapseIcon;
            CallTreeButton.Image = treeIcon;
            ApiTreeButton.Image = treeIcon; // Reuse tree icon
            SettingsButton.Image = settingsIcon;
            ShowHelpButton.Image = helpIcon;

            // Apply to error/warning navigation buttons (no images, use text)
            // These buttons use text display

            // Also apply to menu items for consistency
            openMenuItem.Image = openIcon;
            saveAsMenuItem.Image = saveIcon;
            exportFilteredLogsMenuItem.Image = exportIcon;
            reloadMenuItem.Image = refreshIcon;
            copyMenuItem.Image = copyIcon;
            findMenuItem.Image = findIcon;
            filterMenuItem.Image = filterIcon;
            settingsMenuItem.Image = settingsIcon;
            viewHelpMenuItem.Image = helpIcon;
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
            // Safety check: ensure logListView is initialized
            if (logListView == null || logListView.Columns.Count < 2) return;

            // Line number column: auto-resize to content
            logListView.Columns[0].Width = 80; // Fixed width for line numbers

            // Log text column: fill remaining space
            int remainingWidth = logListView.ClientSize.Width - logListView.Columns[0].Width - 4;
            if (remainingWidth > 0)
            {
                logListView.Columns[1].Width = remainingWidth;
            }
        }

        private void ShowLoadError(string filePath, string reason, string detail)
        {
            SetDocumentLoaded(false);
            FileStatus.Image = Resources.red_ball;
            MessageBox.Show(
                string.Format("{0}:\n{1}\n\nFile: {2}", reason, detail, filePath),
                Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void OnFileChangedOnDisk(object sender, EventArgs e)
        {
            if (!_isLoading) FileStatus.Image = Resources.red_ball;
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
        private void openMenuItem_Click(object sender, EventArgs e)
        {
            if (openLogFileDialog.ShowDialog() == DialogResult.OK)
                LoadFileAsync(openLogFileDialog.FileName);
        }

        private void OpenButton_Click(object sender, EventArgs e) =>
            openMenuItem_Click(sender, e);

        private async void saveAsMenuItem_Click(object sender, EventArgs e)
        {
            if (_virtualLines.Count == 0) return;
            if (saveLogFileDialog.ShowDialog() != DialogResult.OK) return;

            var lines = new List<string>();
            if (logListView.SelectedIndices.Count > 0)
            {
                foreach (int idx in logListView.SelectedIndices)
                    lines.Add(_virtualLines[idx].Text);
            }
            else
            {
                foreach (var vl in _virtualLines)
                    lines.Add(vl.Text);
            }

            StartOperation(string.Format(Resources.OPERATION_SAVING_LINES, lines.Count));

            try
            {
                await Task.Run(() =>
                {
                    var token = _cancellationTokenSource.Token;

                    _logFileService.WriteLines(saveLogFileDialog.FileName, lines, (progress, message) =>
                    {
                        token.ThrowIfCancellationRequested();

                        this.Invoke((Action)(() =>
                        {
                            FileLoadProgress.Style = ProgressBarStyle.Blocks;
                            FileLoadProgress.Value = progress;
                            StatusFileName.Text = string.Format(Resources.PROGRESS_PRESS_ESC_TO_CANCEL, message);
                        }));
                    });
                });

                MessageBox.Show(string.Format(Resources.MSG_FILE_SAVED, lines.Count), Resources.TITLE, 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                StatusFileName.Text = Resources.STATUS_SAVE_CANCELLED;
                MessageBox.Show(Resources.ERR_SAVE_CANCELLED, Resources.TITLE,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Resources.MSG_SAVE_ERROR, ex.Message), Resources.TITLE, 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                EndOperation();
            }
        }

        private void SaveButton_Click(object sender, EventArgs e) =>
            saveAsMenuItem_Click(sender, e);

        private void refreshMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            int topIndex = logListView.TopItem != null ? logListView.TopItem.Index : 0;
            LoadFileAsync(_currentFilePath);
            BeginInvoke((Action)(() =>
            {
                if (topIndex < logListView.VirtualListSize)
                    logListView.EnsureVisible(topIndex);
            }));
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
                    // Merge files time-sorted
                    var merged = await _mergeLogService.MergeAsync(dlg.FileNames);

                    // Update current file path to show merged state
                    _currentFilePath = "[Merged: " + string.Join(", ",
                        System.Array.ConvertAll(dlg.FileNames,
                            p => System.IO.Path.GetFileName(p))) + "]";

                    _allLines = merged;
                    _searchService.Reset();
                    ClearHighlighting();

                    // Load merged data into UI
                    StatusFileName.Text = Resources.STATUS_PROCESSING_MERGED_DATA;
                    FileLoadProgress.Value = 33;
                    await Task.Delay(10);

                    PopulateVirtualListView(_allLines);
                    FileLoadProgress.Value = 66;
                    StatusFileName.Text = Resources.STATUS_BUILDING_MERGED_TREE;
                    await Task.Delay(10);

                    PopulateTrees(_allLines);
                    FileLoadProgress.Value = 100;

                    SetDocumentLoaded(true);
                    FileStatus.Image = Resources.green_ball;
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
            if (_virtualLines.Count == 0)
            {
                MessageBox.Show(Resources.ERR_NO_DATA_TO_EXPORT, Resources.TITLE,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dlg = new SaveFileDialog())
            {
                dlg.Title = Resources.DIALOG_TITLE_SAVE_BRANCH;
                dlg.Filter = Resources.FILE_FILTER_LOG_FILES;
                dlg.FileName = Path.GetFileNameWithoutExtension(_currentFilePath ?? "filtered") + Resources.FILENAME_SUFFIX_FILTERED;

                if (!string.IsNullOrEmpty(_currentFilePath))
                    dlg.InitialDirectory = Path.GetDirectoryName(_currentFilePath);

                if (dlg.ShowDialog() != DialogResult.OK) return;

                FileLoadProgress.Visible = true;
                FileLoadProgress.Value = 0;
                StatusFileName.Text = Resources.STATUS_EXPORTING_LOGS;

                try
                {
                    var lines = new List<string>();
                    foreach (var vl in _virtualLines)
                        lines.Add(vl.Text);

                    // Save with progress callback
                    _logFileService.WriteLines(dlg.FileName, lines, (progress, message) =>
                    {
                        this.Invoke((Action)(() =>
                        {
                            FileLoadProgress.Value = progress;
                            StatusFileName.Text = message;
                        }));
                    });

                    FileLoadProgress.Visible = false;
                    UpdateStatusBar();

                    string filterInfo = string.IsNullOrEmpty(_activeFilterText) 
                        ? Resources.EXPORT_FILTER_INFO_ALL_LINES 
                        : string.Format(Resources.EXPORT_FILTER_INFO_FILTERED, _activeFilterText);

                    MessageBox.Show(
                        string.Format(Resources.MSG_EXPORT_FILTERED_SUCCESSFUL,
                            lines.Count, filterInfo, dlg.FileName),
                        Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    FileLoadProgress.Visible = false;
                    UpdateStatusBar();
                    MessageBox.Show(string.Format(Resources.ERR_EXPORT_FILE_FAILED, ex.Message),
                        Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

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

            var visibleLines = new List<string>(_virtualLines.Count);
            foreach (var vl in _virtualLines) visibleLines.Add(vl.Text);

            int idx = _searchService.FindNext(visibleLines, searchTerm, matchCase, useRegex);
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
            if (string.IsNullOrEmpty(searchTerm))
            {
                ClearHighlighting();
                return;
            }

            // Safety check: ensure _virtualLines is initialized
            if (_virtualLines == null || _virtualLines.Count == 0)
                return;

            try
            {
                if (useRegex)
                {
                    // Regex matching
                    var options = matchCase ? System.Text.RegularExpressions.RegexOptions.None 
                        : System.Text.RegularExpressions.RegexOptions.IgnoreCase;
                    var regex = new System.Text.RegularExpressions.Regex(searchTerm, options);

                    for (int i = 0; i < _virtualLines.Count; i++)
                    {
                        var vl = _virtualLines[i];
                        if (regex.IsMatch(vl.Text))
                        {
                            _virtualLines[i] = new VirtualLogLine
                            {
                                LineNumber = vl.LineNumber,
                                Text = vl.Text,
                                BackColour = _appSettings.HighlightColor
                            };
                        }
                    }
                }
                else
                {
                    // Standard string matching
                    var comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

                    for (int i = 0; i < _virtualLines.Count; i++)
                    {
                        var vl = _virtualLines[i];
                        if (vl.Text.IndexOf(searchTerm, comparison) >= 0)
                        {
                            _virtualLines[i] = new VirtualLogLine
                            {
                                LineNumber = vl.LineNumber,
                                Text = vl.Text,
                                BackColour = _appSettings.HighlightColor
                            };
                        }
                    }
                }

                logListView.Invalidate();
            }
            catch (ArgumentException ex) // Regex exception
            {
                MessageBox.Show(string.Format(Resources.ERR_INVALID_REGEX, ex.Message),
                    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ClearHighlighting()
        {
            // Safety check: ensure _virtualLines is initialized
            if (_virtualLines == null || _virtualLines.Count == 0)
            {
                _lastHighlightTerm = "";
                return;
            }

            // Restore original colors based on log level
            for (int i = 0; i < _virtualLines.Count; i++)
            {
                var vl = _virtualLines[i];
                _virtualLines[i] = new VirtualLogLine
                {
                    LineNumber = vl.LineNumber,
                    Text = vl.Text,
                    BackColour = GetLineColour(vl.Text)
                };
            }

            _lastHighlightTerm = "";
            if (logListView != null)
                logListView.Invalidate();
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
            int selectedLine = int.Parse(_virtualLines[selectedIdx].LineNumber);

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

            // Disable menu items during operation
            SetOperationInProgress(true);
        }

        private void EndOperation()
        {
            FileLoadProgress.Visible = false;
            FileLoadProgress.Style = ProgressBarStyle.Blocks;
            StatusFileName.Text = string.Empty;
            _currentOperation = string.Empty;

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
        public async void ExpandAllTrees()
        {
            StartOperation(Resources.OPERATION_EXPANDING_ALL_NODES);

            try
            {
                await Task.Run(() =>
                {
                    var token = _cancellationTokenSource.Token;

                    this.Invoke((Action)(() => CallTree.BeginUpdate()));
                    ExpandNodeRecursive(CallTree.Nodes, token);
                    this.Invoke((Action)(() => CallTree.EndUpdate()));

                    token.ThrowIfCancellationRequested();

                    this.Invoke((Action)(() => ApiTree.BeginUpdate()));
                    ExpandNodeRecursive(ApiTree.Nodes, token);
                    this.Invoke((Action)(() => ApiTree.EndUpdate()));
                });
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

        private void ExpandNodeRecursive(TreeNodeCollection nodes, CancellationToken token)
        {
            foreach (TreeNode node in nodes)
            {
                token.ThrowIfCancellationRequested();

                this.Invoke((Action)(() => node.Expand()));

                if (node.Nodes.Count > 0)
                {
                    ExpandNodeRecursive(node.Nodes, token);
                }
            }
        }

        public async void CollapseAllTrees()
        {
            StartOperation(Resources.OPERATION_COLLAPSING_ALL_NODES);

            try
            {
                await Task.Run(() =>
                {
                    var token = _cancellationTokenSource.Token;

                    this.Invoke((Action)(() =>
                    {
                        CallTree.BeginUpdate();
                        CallTree.CollapseAll();
                        // Keep root nodes expanded
                        foreach (TreeNode n in CallTree.Nodes) n.Expand();
                        CallTree.EndUpdate();
                    }));

                    token.ThrowIfCancellationRequested();

                    this.Invoke((Action)(() =>
                    {
                        ApiTree.BeginUpdate();
                        ApiTree.CollapseAll();
                        // Keep root nodes expanded
                        foreach (TreeNode n in ApiTree.Nodes) n.Expand();
                        ApiTree.EndUpdate();
                    }));
                });
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
            ShowLogDetail(lineIdx);
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
            ShowLogDetail(lineIdx);
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
            ShowLogDetail(lineIdx);
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
            ShowLogDetail(lineIdx);
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
                // Parse all lines to LogEntry objects for proper filtering
                var logEntries = new List<Models.LogEntry>();
                for (int i = 0; i < _allLines.Count; i++)
                {
                    logEntries.Add(new Models.LogEntry
                    {
                        LineNumber = i + 1,
                        Text = _allLines[i],
                        // Parse level from line if possible
                        Level = ParseLogLevel(_allLines[i])
                    });
                }

                var filtered = await Task.Run(() =>
                {
                    var token = _cancellationTokenSource.Token;
                    var filterService = new Services.Search.FilterService();
                    var filteredEntries = new List<Models.LogEntry>();

                    for (int i = 0; i < logEntries.Count; i++)
                    {
                        if (i % 1000 == 0) // Check cancellation every 1000 lines
                        {
                            token.ThrowIfCancellationRequested();

                            // Update progress
                            int progress = (int)((i / (double)logEntries.Count) * 100);
                            this.Invoke((Action)(() => 
                            {
                                FileLoadProgress.Style = ProgressBarStyle.Blocks;
                                FileLoadProgress.Value = progress;
                                StatusFileName.Text = string.Format(Resources.STATUS_FILTERING_PROGRESS, 
                                    progress, i, logEntries.Count);
                            }));
                        }

                        var entry = logEntries[i];

                        // Use FilterService for comprehensive filtering
                        if (MatchesFilter(entry, criteria))
                        {
                            filteredEntries.Add(entry);
                        }
                    }

                    return filteredEntries;
                });

                // Convert to FilteredLine format
                var filteredLines = filtered.Select(e => new FilteredLine(e.LineNumber, e.Text)).ToList();

                _activeFilterText = criteria.GetDescription();
                PopulateVirtualListViewFiltered(filteredLines);
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

        /// <summary>
        /// Checks if a log entry matches the filter criteria.
        /// Uses FilterService for comprehensive filtering.
        /// </summary>
        private bool MatchesFilter(Models.LogEntry entry, Models.FilterCriteria criteria)
        {
            // Text filter
            if (!string.IsNullOrWhiteSpace(criteria.SearchText))
            {
                var comparison = criteria.IsCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                if (entry.Text.IndexOf(criteria.SearchText, comparison) < 0)
                    return false;
            }

            // Duration filter
            if (criteria.MinimumDurationMs.HasValue)
            {
                if (!CheckDurationFilter(entry.Text, criteria.MinimumDurationMs.Value))
                    return false;
            }

            // Time range filter
            if (criteria.FromTime.HasValue || criteria.ToTime.HasValue)
            {
                if (!CheckTimeRangeFilter(entry.Text, criteria.FromTime, criteria.ToTime))
                    return false;
            }

            // Thread ID filter
            if (!string.IsNullOrWhiteSpace(criteria.ThreadId))
            {
                if (string.IsNullOrWhiteSpace(entry.ThreadId) ||
                    !entry.ThreadId.Equals(criteria.ThreadId, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            // Log level filter
            if (criteria.Level.HasValue)
            {
                if (entry.Level != criteria.Level.Value)
                    return false;
            }

            return true;
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
                settingsDialog.ShowDialog(this);
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
                // Try to open HTML help file
                string helpFilePath = Path.Combine(Application.StartupPath, "Help", "UserGuide.html");

                // Fallback to root directory if Help subfolder doesn't exist
                if (!File.Exists(helpFilePath))
                {
                    helpFilePath = Path.Combine(Application.StartupPath, "UserGuide.html");
                }

                // Fallback to documentation folder
                if (!File.Exists(helpFilePath))
                {
                    helpFilePath = Path.Combine(Application.StartupPath, "documentation", "UserGuide.html");
                }

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
                Text = "═══════════════════════════════════════════════════════════════════\r\n" +
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
                       "• Dark Theme: Eye-friendly professional appearance\r\n\r\n" +
                       "KEYBOARD SHORTCUTS\r\n" +
                       "─────────────────────────────────────────────────────────────────\r\n" +
                       "FILE OPERATIONS\r\n" +
                       "Ctrl+O              Open log file\r\n" +
                       "Ctrl+S              Save filtered/processed logs\r\n" +
                       "F5                  Reload current file\r\n" +
                       "Ctrl+Q              Quit application\r\n\r\n" +
                       "EDITING & SEARCH\r\n" +
                       "Ctrl+C              Copy selected lines\r\n" +
                       "Ctrl+F              Find text\r\n" +
                       "F3                  Find next occurrence\r\n" +
                       "Ctrl+I              Filter logs\r\n\r\n" +
                       "NAVIGATION\r\n" +
                       "Ctrl+E              Expand all tree nodes\r\n" +
                       "Ctrl+W              Collapse all tree nodes\r\n" +
                       "Ctrl+G              Jump to matching ENTER/EXIT\r\n" +
                       "Ctrl+J              Jump to line number\r\n" +
                       "F8                  Next error\r\n" +
                       "Shift+F8            Previous error\r\n" +
                       "F9                  Next warning\r\n" +
                       "Shift+F9            Previous warning\r\n\r\n" +
                       "BOOKMARKS\r\n" +
                       "Ctrl+B              Toggle bookmark on current line\r\n" +
                       "Ctrl+N              Next bookmark\r\n" +
                       "Ctrl+P              Previous bookmark\r\n" +
                       "Ctrl+Shift+B        Show all bookmarks\r\n" +
                       "Ctrl+Shift+C        Clear all bookmarks\r\n\r\n" +
                       "VIEW\r\n" +
                       "Ctrl+T              Toggle Call Tree\r\n" +
                       "Ctrl+L              Toggle API Tree\r\n" +
                       "Ctrl+Shift+S        Settings\r\n\r\n" +
                       "HELP\r\n" +
                       "F1                  View this help\r\n" +
                       "Ctrl+K              Keyboard shortcuts\r\n\r\n" +
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
                       "═══════════════════════════════════════════════════════════════════\r\n"
            };

            // Apply current theme to help dialog
            ThemeManager.ApplyTheme(helpForm);

            helpForm.Controls.Add(rtb);
            helpForm.ShowDialog(this);
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
                MaximizeBox = false, MinimizeBox = false
            };
            var rtb = new RichTextBox
            {
                Dock = DockStyle.Fill, ReadOnly = true,
                Font = new System.Drawing.Font("Consolas", 9f),
                BackColor = SystemColors.Window,
                Text =
                    "═══════════════════════════════════════════════════════════════════\r\n" +
                    "           CAD 3P LOG BROWSER — KEYBOARD SHORTCUTS\r\n" +
                    "═══════════════════════════════════════════════════════════════════\r\n\r\n" +
                    "FILE MENU\r\n" +
                    "─────────────────────────────────────────────────────────────────\r\n" +
                    "Ctrl+O              Open log file\r\n" +
                    "Ctrl+S              Save As (selection or all visible lines)\r\n" +
                    "Ctrl+Shift+E        Export Filtered Logs (save visible lines)\r\n" +
                    "F5                  Refresh (reload, keep scroll position)\r\n" +
                    "Ctrl+R              Reload File from Disk (reset to top)\r\n" +
                    "Alt+F4              Exit\r\n\r\n" +
                    "EDIT MENU\r\n" +
                    "─────────────────────────────────────────────────────────────────\r\n" +
                    "Ctrl+C              Copy selected lines\r\n" +
                    "Ctrl+F              Find / Search\r\n" +
                    "F3                  Find Next\r\n" +
                    "Ctrl+I              Filter log entries\r\n" +
                    "Ctrl+E              Expand All (Call Tree & API Tree)\r\n" +
                    "Ctrl+W              Collapse All (keeps root nodes expanded)\r\n" +
                    "Ctrl+G              Jump to Matching ENTER/EXIT pair\r\n\r\n" +
                    "NAVIGATION (Errors & Warnings)\r\n" +
                    "─────────────────────────────────────────────────────────────────\r\n" +
                    "F8                  Next Error\r\n" +
                    "Shift+F8            Previous Error\r\n" +
                    "Ctrl+F8             Next Warning\r\n" +
                    "Ctrl+Shift+F8       Previous Warning\r\n\r\n" +
                    "BOOKMARKS\r\n" +
                    "─────────────────────────────────────────────────────────────────\r\n" +
                    "Ctrl+B              Toggle Bookmark on current line\r\n" +
                    "F2                  Next Bookmark\r\n" +
                    "Shift+F2            Previous Bookmark\r\n" +
                    "Ctrl+Shift+B        Show all Bookmarks\r\n" +
                    "Ctrl+Shift+Del      Clear all Bookmarks\r\n\r\n" +
                    "VIEW MENU\r\n" +
                    "─────────────────────────────────────────────────────────────────\r\n" +
                    "Ctrl+T              Toggle Call Tree view\r\n" +
                    "Ctrl+L              Toggle API List view\r\n" +
                    "Ctrl+H              Hide/Show Tab panels\r\n\r\n" +
                    "OPTIONS MENU\r\n" +
                    "─────────────────────────────────────────────────────────────────\r\n" +
                    "Ctrl+Shift+S        Settings\r\n\r\n" +
                    "HELP MENU\r\n" +
                    "─────────────────────────────────────────────────────────────────\r\n" +
                    "F1                  View Help\r\n" +
                    "Ctrl+K              Keyboard Shortcuts (this dialog)\r\n\r\n" +
                    "CALL GRAPH TAB\r\n" +
                    "─────────────────────────────────────────────────────────────────\r\n" +
                    "  •  Scroll wheel to zoom in/out\r\n" +
                    "  •  Click and drag to pan\r\n" +
                    "  •  Hover a node to highlight its edges\r\n" +
                    "  •  Edge thickness = call frequency\r\n" +
                    "  •  Reset View button restores default zoom/pan\r\n\r\n" +
                    "TIPS & TRICKS\r\n" +
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
            helpForm.Controls.Add(rtb);
            helpForm.ShowDialog(this);
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

            logTab.Text = Resources.TAB_LOG;
            performanceTab.Text = Resources.TAB_PERFORMANCE;
            logDetailTab.Text = Resources.TAB_LOG_DETAILS;
            callGraphTab.Text = Resources.TAB_CALL_GRAPH;

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
            if (e.Button == MouseButtons.Right)
            {
                // Select node under cursor
                var node = CallTree.GetNodeAt(e.Location);
                if (node != null) CallTree.SelectedNode = node;

                // Call Tree specific context menu
                var contextMenu = new ContextMenuStrip();

                // ═══ COPY ACTIONS ═══
                contextMenu.Items.Add(Resources.MENU_COPY_METHOD_NAME, null, (s, ev) => 
                {
                    var n = CallTree.SelectedNode;
                    if (n != null) Clipboard.SetText(GetMethodNameFromNode(n));
                });

                contextMenu.Items.Add(Resources.MENU_COPY_SUBTREE, null, (s, ev) => 
                {
                    var n = CallTree.SelectedNode;
                    if (n != null)
                    {
                        var sb = new System.Text.StringBuilder();
                        AppendSubtreeText(n, sb, 0);
                        Clipboard.SetText(sb.ToString());
                    }
                });

                contextMenu.Items.Add(new ToolStripSeparator());

                // ═══ NAVIGATION (Call Tree only - ENTER/EXIT matching) ═══
                contextMenu.Items.Add(Resources.MENU_JUMP_TO_MATCHING, null, (s, ev) => JumpToMatchingPair());

                contextMenu.Items.Add(new ToolStripSeparator());

                // ═══ EXPORT (Call Tree only - branch operations) ═══
                contextMenu.Items.Add(Resources.MENU_SAVE_BRANCH, null, treeContextSaveBranchMenuItem_Click);
                contextMenu.Items.Add(Resources.MENU_EXPORT_BRANCH_CSV, null, treeContextExportBranchCsvMenuItem_Click);

                contextMenu.Items.Add(new ToolStripSeparator());

                // ═══ COMMON TREE ACTIONS ═══
                contextMenu.Items.Add(Resources.MENU_EXPAND_ALL_SHORTCUT, null, (s, ev) => CallTree.ExpandAll());
                contextMenu.Items.Add(Resources.MENU_COLLAPSE_ALL_SHORTCUT, null, (s, ev) => 
                {
                    CallTree.CollapseAll();
                    if (CallTree.Nodes.Count > 0) CallTree.Nodes[0].Expand();
                });

                contextMenu.Items.Add(new ToolStripSeparator());

                // ═══ CROSS-REFERENCE ═══
                contextMenu.Items.Add(Resources.MENU_SHOW_IN_API_TREE, null, (s, ev) => 
                {
                    var n = CallTree.SelectedNode;
                    if (n != null)
                    {
                        ShowApiTree();
                        FindAndSelectApiTreeNode(GetMethodNameFromNode(n));
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

                contextMenu.Show(CallTree, e.Location);
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
            int dash = text.IndexOf(" — ");
            if (dash > 0) return text.Substring(0, dash).Trim();
            return text.Trim();
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
            string name = GetMethodNameFromNode(node);
            // Extract duration from text like "MethodName [142 ms]"
            string duration = "";
            int b1 = node.Text.IndexOf('[');
            int b2 = node.Text.IndexOf(" ms]");
            if (b1 >= 0 && b2 > b1)
                duration = node.Text.Substring(b1 + 1, b2 - b1 - 1).Trim();
            rows.Add($"\"{name}\",{depth},{duration}");
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
                dialog.FileName = Path.GetFileNameWithoutExtension(_currentFilePath) + Resources.FILENAME_SUFFIX_PERFORMANCE_CSV;
                dialog.InitialDirectory = string.IsNullOrEmpty(_currentFilePath) 
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : Path.GetDirectoryName(_currentFilePath);

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
                    logListView.FocusedItem = logListView.Items[index];
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

            TreeNode selectedNode = activeTree.SelectedNode;
            string methodName = GetMethodNameFromNode(selectedNode);

            // Find the ENTER and EXIT lines for this method
            var branchLines = new List<string>();
            bool inBranch = false;
            int depth = 0;

            foreach (var line in _virtualLines)
            {
                string lineText = line.Text;

                if (lineText.Contains("[ENTER]") && lineText.Contains(methodName))
                {
                    inBranch = true;
                    depth = 1;
                    branchLines.Add(lineText);
                }
                else if (inBranch)
                {
                    branchLines.Add(lineText);

                    if (lineText.Contains("[ENTER]")) depth++;
                    if (lineText.Contains("[EXIT]"))
                    {
                        depth--;
                        if (depth == 0)
                            break;
                    }
                }
            }

            if (branchLines.Count == 0)
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
                    : Path.GetDirectoryName(_currentFilePath);

                if (dialog.ShowDialog() != DialogResult.OK) return;

                try
                {
                    File.WriteAllLines(dialog.FileName, branchLines);
                    MessageBox.Show(string.Format(Resources.MSG_BRANCH_SAVED_TO, branchLines.Count, dialog.FileName),
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
            _appSettings.Save();
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
            logListView.FocusedItem = logListView.Items[index];
            Focus();
        }

        // ── Feature F6: Export Call Graph as PNG ─────────────────────────────
        private void callGraphExportButton_Click(object sender, EventArgs e)
        {
            if (callGraphPanel == null) return;

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = Resources.FILE_FILTER_IMAGE_FILES;
                dialog.FileName = Path.GetFileNameWithoutExtension(_currentFilePath) + Resources.FILENAME_SUFFIX_CALLGRAPH_PNG;
                dialog.InitialDirectory = string.IsNullOrEmpty(_currentFilePath)
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : Path.GetDirectoryName(_currentFilePath);

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

                    // Clear status after 3 seconds
                    var timer = new System.Windows.Forms.Timer();
                    timer.Interval = 3000;
                    timer.Tick += (s, args) =>
                    {
                        StatusFileName.Text = Path.GetFileName(_currentFilePath);
                        timer.Stop();
                        timer.Dispose();
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

        private const string SEARCH_HISTORY_FILE = "search_history.json";
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
        /// </summary>
        /// <returns>True if this node or any child matches.</returns>
        private bool FilterTreeNodeRecursive(TreeNode node)
        {
            bool hasMatch = false;
            bool nodeMatches = node.Text.ToLowerInvariant().Contains(_treeSearchText);

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

            int lineNumber = int.Parse(_virtualLines[selectedIndex].LineNumber);
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
                currentLine = int.Parse(_virtualLines[idx].LineNumber);
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
                currentLine = int.Parse(_virtualLines[idx].LineNumber);
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
                int lineNum = int.Parse(vl.LineNumber);

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
                int idx = _virtualLines.FindIndex(v => v.LineNumber == lineNum.ToString());
                if (idx >= 0)
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
                dlg.FileName = Path.GetFileNameWithoutExtension(_currentFilePath) + Resources.FILENAME_SUFFIX_CALLTREE_JSON;
                dlg.InitialDirectory = string.IsNullOrEmpty(_currentFilePath)
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : Path.GetDirectoryName(_currentFilePath);

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
                dlg.FileName = Path.GetFileNameWithoutExtension(_currentFilePath) + Resources.FILENAME_SUFFIX_CALLTREE_XML;
                dlg.InitialDirectory = string.IsNullOrEmpty(_currentFilePath)
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : Path.GetDirectoryName(_currentFilePath);

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
                dlg.FileName = Path.GetFileNameWithoutExtension(_currentFilePath) + Resources.FILENAME_SUFFIX_TIMELINE_PNG;
                dlg.InitialDirectory = string.IsNullOrEmpty(_currentFilePath)
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : Path.GetDirectoryName(_currentFilePath);

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
                dlg.FileName = Path.GetFileNameWithoutExtension(_currentFilePath) + Resources.FILENAME_SUFFIX_FLAMEGRAPH_PNG;
                dlg.InitialDirectory = string.IsNullOrEmpty(_currentFilePath)
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : Path.GetDirectoryName(_currentFilePath);

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
                        StatusFileName.Text = Path.GetFileName(_currentFilePath);
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
        ///      <SubType>Component</SubType>
        ///    </Compile>
        ///    <Compile Include="Managers\AiAssistantPanel.cs">
        ///      <SubType>Component</SubType>
        ///    </Compile>
        /// 4. Uncomment field declarations in MainForm.cs (lines ~48-51)
        /// 5. Uncomment this method body
        /// 6. Uncomment call in constructor
        /// 7. Uncomment code in PopulateTrees
        /// </summary>
        private void InitDependencyGraphPanel()
        {
            // Create dependency graph tab
            _dependencyGraphTab = new TabPage(Resources.TAB_NAME_DEPENDENCY_GRAPH)
            {
                Name = "dependencyGraphTab",
                UseVisualStyleBackColor = true
            };

            // Create dependency graph panel
            _dependencyGraphPanel = new Managers.DependencyGraphPanel()
            {
                Dock = DockStyle.Fill,
                Name = "dependencyGraphPanel"
            };

            // Create reset view button
            _depGraphResetButton = new Button()
            {
                Text = Resources.BUTTON_RESET_VIEW,
                Size = new Size(100, 30),
                Location = new Point(10, 10),
                Name = "depGraphResetButton",
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            _depGraphResetButton.Click += (s, e) => _dependencyGraphPanel?.ResetView();

            // Add controls to tab
            _dependencyGraphTab.Controls.Add(_dependencyGraphPanel);
            _dependencyGraphTab.Controls.Add(_depGraphResetButton);

            // Add tab to main tab control
            if (mainTabControl != null)
            {
                mainTabControl.TabPages.Add(_dependencyGraphTab);
            }

            // Create View menu item for dependency graph
            _showDependencyGraphMenuItem = new ToolStripMenuItem(Resources.MENU_SHOW_DEPENDENCY_GRAPH)
            {
                Name = "showDependencyGraphMenuItem",
                CheckOnClick = true,
                Checked = true
            };
            _showDependencyGraphMenuItem.CheckedChanged += ShowDependencyGraphMenuItem_CheckedChanged;

            // Add to View → Tabs menu (after existing tab menu items)
            if (tabsMenuItem != null)
            {
                tabsMenuItem.DropDownItems.Add(_showDependencyGraphMenuItem);
            }
        }

        /// <summary>
        /// Handler for Show Dependency Graph menu item.
        /// </summary>
        private void ShowDependencyGraphMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (_dependencyGraphTab != null && mainTabControl != null)
            {
                if (_showDependencyGraphMenuItem.Checked)
                {
                    if (!mainTabControl.TabPages.Contains(_dependencyGraphTab))
                        mainTabControl.TabPages.Add(_dependencyGraphTab);
                }
                else
                {
                    if (mainTabControl.TabPages.Contains(_dependencyGraphTab))
                        mainTabControl.TabPages.Remove(_dependencyGraphTab);
                }
            }
        }

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
            }

            // Create View menu item
            var showAiMenuItem = new ToolStripMenuItem(Resources.MENU_SHOW_AI_ASSISTANT)
            {
                Name = "showAiMenuItem",
                CheckOnClick = true,
                Checked = true
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
