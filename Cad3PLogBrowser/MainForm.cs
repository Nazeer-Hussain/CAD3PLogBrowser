using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cad3PLogBrowser.Properties;
using Cad3PLogBrowser.Services;

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

        // ── State ─────────────────────────────────────────────────────────────
        private string            _currentFilePath = string.Empty;
        private bool              _isLoading       = false;
        private List<string>      _allLines        = new List<string>();
        private List<ApiCallNode> _apiNodes        = new List<ApiCallNode>();
        private AppSettings       _appSettings;
        private HashSet<int>      _bookmarkedLines  = new HashSet<int>(); // 1-based line numbers
        private List<LogEntry>    _lastEntries      = new List<LogEntry>(); // for ENTER/EXIT jump
        private bool              _isFormLoaded     = false; // Flag to prevent saving during initialization

        // ── Cancellation support for long-running operations ──────────────────
        private CancellationTokenSource _cancellationTokenSource;
        private string _currentOperation = string.Empty;

        // Feature B10: Error/Warning navigation
        private List<int>         _errorLines       = new List<int>();
        private List<int>         _warningLines     = new List<int>();
        private int               _currentErrorIndex   = -1;
        private int               _currentWarningIndex = -1;

        // ── Tab identifiers (used by SettingsForm) ────────────────────────────
        public enum TabId { Log, Performance, LogDetails, CallGraph }

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
                case TabId.Performance: return performanceTab;
                case TabId.LogDetails:  return logDetailTab;
                case TabId.CallGraph:   return callGraphTab;
                default:                return logTab;
            }
        }

        private ToolStripMenuItem GetTabMenuItem(TabId id)
        {
            switch (id)
            {
                case TabId.Log:         return showTab1MenuItem;
                case TabId.Performance: return showTab2MenuItem;
                case TabId.LogDetails:  return showTab3MenuItem;
                case TabId.CallGraph:   return showTab4MenuItem;
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
            _recentFilesMenuItem = new ToolStripMenuItem("Recent &Files");

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
                var clearItem = new ToolStripMenuItem("&Clear Recent Files");
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
                        string.Format("File not found:\n{0}\n\nRemoving from recent files list.", filePath),
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
            _logFileService   = new LogFileService(this);
            _logFileService.FileChangedOnDisk += OnFileChangedOnDisk;

            RestoreSettings();
            InitTreeViews();
            BuildMruMenu();
            ApplyTheme();
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
            // Set the theme based on settings
            var theme = _appSettings.Theme == "Dark" ? ThemeManager.Theme.Dark : ThemeManager.Theme.Light;
            ThemeManager.SetTheme(theme);

            // Apply to main form
            ThemeManager.ApplyTheme(this);

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
            if (_lastPerfStats != null && _lastPerfStats.Count > 0)
            {
                RenderPerformanceRows(_lastPerfStats, _lastTotalLines);
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
                StatusLineCount.Text = string.Format("Filter: '{0}'  |  Showing {1:N0} / {2:N0} lines",
                    _activeFilterText, visible, total);
            }
            else if (total != visible)
            {
                StatusLineCount.Text = string.Format("Showing {0:N0} / {1:N0} lines", visible, total);
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

            StatusSelection.Text = string.Format("Line {0}: {1}", lineNum, preview);
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
            CallTree.ShowLines = CallTree.ShowPlusMinus = true;
            CallTree.ShowNodeToolTips = true;
            CallTree.HideSelection = false;

            CallTreeButton.CheckedChanged       += (s, e) => SyncTreeVisibility();
            ApiTreeButton.CheckedChanged        += (s, e) => SyncTreeVisibility();

            SyncTreeVisibility();
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
            ApiTree.BeginUpdate();
            ApiTree.Nodes.Clear();

            // Root node: "API Tree"
            var root = new TreeNode("API Tree") { Tag = -1 };
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

            // Root node: "Call Tree"
            var rootNode = new TreeNode("Call Tree") { Tag = -1 };
            rootNode.NodeFont = new System.Drawing.Font(CallTree.Font, System.Drawing.FontStyle.Bold);

            foreach (var root in roots)
                rootNode.Nodes.Add(BuildTreeNode(root));

            CallTree.Nodes.Add(rootNode);
            // Issue Fix: Start collapsed (only root expanded)
            rootNode.Expand();
            // Don't expand first level - let users expand as needed

            CallTree.EndUpdate();
        }

        private static TreeNode BuildTreeNode(CallStackNode csNode)
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
                "API: {0}\r\nSource: {1}\r\nENTER line: {2}\r\nEXIT line: {3}\r\nDuration: {4} ms",
                csNode.Label,
                csNode.SourceFile ?? "-",
                csNode.LineNumber,
                matched ? csNode.ExitLineNumber.ToString() : "? (no EXIT found)",
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

            foreach (var child in csNode.Children)
                tn.Nodes.Add(BuildTreeNode(child));

            return tn;
        }

        // ── Performance tab ───────────────────────────────────────────────────
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
        private List<ApiPerfStats> _lastPerfStats = new List<ApiPerfStats>();
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
            RenderPerformanceRows(_lastPerfStats, _lastTotalLines);
        }

        private void RenderPerformanceRows(List<ApiPerfStats> stats, int totalLines)
        {
            _lastPerfStats  = stats;
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

            var summary = new ListViewItem("── Summary ──");
            summary.SubItems.Add(sumCalls.ToString());
            summary.SubItems.Add(sumTotal.ToString());
            summary.SubItems.Add("-"); summary.SubItems.Add("-");
            summary.SubItems.Add("-"); summary.SubItems.Add("-");
            summary.SubItems.Add(string.Format("{0} unique APIs  |  {1} lines", stats.Count, totalLines));

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
            int h = mainSplitContainer.Panel1.ClientSize.Height;
            int w = mainSplitContainer.Panel1.ClientSize.Width;
            bool showCall = CallTree.Visible;
            bool showApi  = ApiTree.Visible;

            if (showCall && showApi)
            {
                int half = h / 2;
                ApiTree.SetBounds(0, 0, w, half);
                CallTree.SetBounds(0, half, w, h - half);
            }
            else if (showCall) CallTree.SetBounds(0, 0, w, h);
            else if (showApi)  ApiTree.SetBounds(0, 0, w, h);
        }

        // ── Tree → scroll log ─────────────────────────────────────────────────
        private void ApiTree_AfterSelect(object sender, TreeViewEventArgs e) =>
            ScrollLogToLine(e.Node?.Tag);

        private void ApiTree_Click(object sender, EventArgs e) { }
        private void ApiTree_MouseClick(object sender, MouseEventArgs e) { }

        private void CallTree_AfterSelect(object sender, TreeViewEventArgs e) =>
            ScrollLogToLine(e.Node?.Tag);

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
            logDetailBox.Text = string.Format("Line {0}:\r\n\r\n{1}",
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
            StatusFileName.Text = "Loading...";

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

                // Show processing message
                StatusFileName.Text = "Processing log data...";
                FileLoadProgress.Value = 0;

                // Give UI a chance to update
                await Task.Delay(10);

                // Populate views with progress
                PopulateVirtualListView(_allLines);
                FileLoadProgress.Value = 33;
                StatusFileName.Text = "Building call tree...";
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
            catch (UnauthorizedAccessException ex) { ShowLoadError(filePath, "Access denied", ex.Message); }
            catch (IOException ex)                 { ShowLoadError(filePath, "File read error", ex.Message); }
            catch (Exception ex)                   { ShowLoadError(filePath, "Unexpected error", ex.Message); }
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
                _virtualLines.Add(new VirtualLogLine
                {
                    LineNumber = (i + 1).ToString(),
                    Text       = lines[i],
                    BackColour = GetLineColour(lines[i])
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

            StartOperation($"Saving {lines.Count:N0} lines");

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
                            StatusFileName.Text = $"{message} (Press ESC to cancel)";
                        }));
                    });
                });

                MessageBox.Show($"{lines.Count:N0} line(s) saved.", Resources.TITLE, 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                StatusFileName.Text = "Save operation cancelled.";
                MessageBox.Show("Save operation was cancelled.", Resources.TITLE, 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save file:\n{ex.Message}", Resources.TITLE, 
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

        private void exitMenuItem_Click(object sender, EventArgs e) => Close();

        // ── Edit menu ─────────────────────────────────────────────────────────
        private void copyMenuItem_Click(object sender, EventArgs e)
        {
            if (logListView.SelectedIndices.Count == 0) return;
            var lines = new List<string>();
            foreach (int idx in logListView.SelectedIndices)
                lines.Add(_virtualLines[idx].Text);
            Clipboard.SetText(_searchService.JoinForClipboard(lines));
        }

        // ── Feature I1: Export Filtered Logs ──────────────────────────────────
        private void exportFilteredLogsMenuItem_Click(object sender, EventArgs e)
        {
            if (_virtualLines.Count == 0)
            {
                MessageBox.Show("No log data to export.", Resources.TITLE,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dlg = new SaveFileDialog())
            {
                dlg.Title = "Export Filtered Logs";
                dlg.Filter = "Log files (*.log)|*.log|Text files (*.txt)|*.txt|All files (*.*)|*.*";
                dlg.FileName = Path.GetFileNameWithoutExtension(_currentFilePath ?? "filtered") + "_filtered.log";

                if (!string.IsNullOrEmpty(_currentFilePath))
                    dlg.InitialDirectory = Path.GetDirectoryName(_currentFilePath);

                if (dlg.ShowDialog() != DialogResult.OK) return;

                FileLoadProgress.Visible = true;
                FileLoadProgress.Value = 0;
                StatusFileName.Text = "Exporting filtered logs...";

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
                        ? "all lines" 
                        : string.Format("filtered lines (filter: '{0}')", _activeFilterText);

                    MessageBox.Show(
                        string.Format("{0:N0} {1} exported successfully.\n\nFile: {2}",
                            lines.Count, filterInfo, dlg.FileName),
                        Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    FileLoadProgress.Visible = false;
                    UpdateStatusBar();
                    MessageBox.Show(string.Format("Failed to export file:\n{0}", ex.Message),
                        Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CopyButton_Click(object sender, EventArgs e) =>
            copyMenuItem_Click(sender, e);

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

        public void FindNext(string searchTerm, bool matchCase)
        {
            if (_virtualLines.Count == 0 || string.IsNullOrEmpty(searchTerm)) return;

            var visibleLines = new List<string>(_virtualLines.Count);
            foreach (var vl in _virtualLines) visibleLines.Add(vl.Text);

            int idx = _searchService.FindNext(visibleLines, searchTerm, matchCase);
            if (idx >= 0)
            {
                // Feature B8: Apply highlighting when search term changes
                if (searchTerm != _lastHighlightTerm || matchCase != _lastHighlightMatchCase)
                {
                    HighlightSearchResults(searchTerm, matchCase);
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
                MessageBox.Show(string.Format("'{0}' not found.", searchTerm),
                    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Feature B8: Highlight all search results in the log view
        private void HighlightSearchResults(string searchTerm, bool matchCase)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                ClearHighlighting();
                return;
            }

            // Safety check: ensure _virtualLines is initialized
            if (_virtualLines == null || _virtualLines.Count == 0)
                return;

            var comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            // Update background colors for highlighted lines
            for (int i = 0; i < _virtualLines.Count; i++)
            {
                var vl = _virtualLines[i];
                if (vl.Text.IndexOf(searchTerm, comparison) >= 0)
                {
                    // Highlight color: Light yellow
                    _virtualLines[i] = new VirtualLogLine
                    {
                        LineNumber = vl.LineNumber,
                        Text = vl.Text,
                        BackColour = Color.FromArgb(255, 255, 200)
                    };
                }
            }

            if (logListView != null)
                logListView.Invalidate();
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

            if (current == null) { MessageBox.Show("Selected line is not an API call line.", Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

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
                MessageBox.Show("No matching pair found.", Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            StatusFileName.Text = $"{operationName}... (ESC or click here to cancel)";

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
                StatusFileName.Text = $"{_currentOperation} cancelled.";
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
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ── C1: Expand / Collapse all ─────────────────────────────────────────
        public async void ExpandAllTrees()
        {
            StartOperation("Expanding all nodes");

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
                StatusFileName.Text = "Expand operation cancelled.";
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
            StartOperation("Collapsing all nodes");

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
                StatusFileName.Text = "Collapse operation cancelled.";
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
                MessageBox.Show("No errors found in this log file.", Resources.TITLE, 
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
                MessageBox.Show("No errors found in this log file.", Resources.TITLE, 
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
                MessageBox.Show("No warnings found in this log file.", Resources.TITLE, 
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
                MessageBox.Show("No warnings found in this log file.", Resources.TITLE, 
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

        public async void ApplyFilter(string filterText, bool matchCase)
        {
            if (string.IsNullOrWhiteSpace(filterText))
            {
                ClearFilter();
                return;
            }

            StartOperation($"Filtering ({_allLines.Count:N0} lines)");

            try
            {
                var filtered = await Task.Run(() =>
                {
                    var token = _cancellationTokenSource.Token;
                    var results = new List<FilteredLine>();

                    for (int i = 0; i < _allLines.Count; i++)
                    {
                        if (i % 1000 == 0) // Check cancellation every 1000 lines
                        {
                            token.ThrowIfCancellationRequested();

                            // Update progress
                            int progress = (int)((i / (double)_allLines.Count) * 100);
                            this.Invoke((Action)(() => 
                            {
                                FileLoadProgress.Style = ProgressBarStyle.Blocks;
                                FileLoadProgress.Value = progress;
                                StatusFileName.Text = $"Filtering... {progress}% ({i:N0}/{_allLines.Count:N0} lines)";
                            }));
                        }

                        string line = _allLines[i];
                        bool matches = matchCase 
                            ? line.Contains(filterText)
                            : line.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0;

                        if (matches)
                        {
                            results.Add(new FilteredLine(i + 1, line));
                        }
                    }

                    return results;
                });

                _activeFilterText = filterText;
                PopulateVirtualListViewFiltered(filtered);
                ClearHighlighting();

                StatusFileName.Text = $"Filter applied: {filtered.Count:N0} of {_allLines.Count:N0} lines match.";
            }
            catch (OperationCanceledException)
            {
                StatusFileName.Text = "Filter operation cancelled.";
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
                settingsDialog.ShowDialog(this);
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
                MessageBox.Show($"Could not open updates page:\n{ex.Message}", 
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
                MessageBox.Show($"Could not open issues page:\n{ex.Message}", 
                    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void viewHelpMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Try to open CHM help file if it exists
                string helpFilePath = Path.Combine(Application.StartupPath, "Cad3PLogBrowser.chm");

                if (File.Exists(helpFilePath))
                {
                    System.Diagnostics.Process.Start(helpFilePath);
                }
                else
                {
                    // If CHM doesn't exist, show keyboard shortcuts as fallback
                    MessageBox.Show(
                        "Help file (Cad3PLogBrowser.chm) not found.\n\n" +
                        "Press Ctrl+K to view keyboard shortcuts,\n" +
                        "or visit the GitHub repository for documentation.",
                        Resources.TITLE, 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open help file:\n{ex.Message}", 
                    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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
                Text = "Keyboard Shortcuts — WWGM CAD 3P Log Browser",
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
                    "       WWGM CAD 3P LOG BROWSER — KEYBOARD SHORTCUTS\r\n" +
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

            logTab.Text = "Log";
            performanceTab.Text = "Performance";
            logDetailTab.Text = "Log Details";
            callGraphTab.Text = "Call Graph";

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

        private void ApiTree_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var node = ApiTree.GetNodeAt(e.Location);
                if (node != null) ApiTree.SelectedNode = node;
                treeContextMenu.Show(ApiTree, e.Location);
            }
        }

        private void CallTree_MouseClick(object sender, MouseEventArgs e) { }

        private void CallTree_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var node = CallTree.GetNodeAt(e.Location);
                if (node != null) CallTree.SelectedNode = node;
                treeContextMenu.Show(CallTree, e.Location);
            }
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
                MessageBox.Show(
                    "Please configure the Grok URL in Options > Settings first.\n\n" +
                    "Example: https://grok.example.com/search?q=",
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
                MessageBox.Show($"Failed to open browser:\n{ex.Message}",
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
                dlg.Title = "Export Branch to CSV";
                dlg.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                dlg.FileName = GetMethodNameFromNode(node).Replace("::", "_") + "_branch.csv";

                if (dlg.ShowDialog() != DialogResult.OK) return;

                var rows = new List<string> { "Method,Depth,Duration_ms" };
                CollectBranchCsvRows(node, rows, 0);
                File.WriteAllLines(dlg.FileName, rows);

                MessageBox.Show($"Branch exported to:\n{dlg.FileName}",
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
    }
}
