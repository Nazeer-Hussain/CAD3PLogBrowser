using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

        // ── #7 Virtual mode backing store ─────────────────────────────────────
        // Each VirtualLogLine holds exactly what the ListView needs for one row.
        private struct VirtualLogLine
        {
            public string LineNumber;
            public string Text;
            public Color  BackColour;
        }
        private List<VirtualLogLine> _virtualLines = new List<VirtualLogLine>();

        // ── Log-level colours ─────────────────────────────────────────────────
        private static readonly Color ColourError = Color.FromArgb(255, 220, 220);
        private static readonly Color ColourWarn  = Color.FromArgb(255, 243, 205);
        private static readonly Color ColourInfo  = SystemColors.Window;

        // ── Construction ──────────────────────────────────────────────────────
        public MainForm()
        {
            InitializeComponent();

            _settingsService  = new SettingsService();
            _searchService    = new SearchService();
            _parserService    = new LogParserService();
            _callGraphService = new CallGraphService();
            _logFileService   = new LogFileService(this);
            _logFileService.FileChangedOnDisk += OnFileChangedOnDisk;

            RestoreSettings();
            InitTreeViews();
        }

        // ── Public API ────────────────────────────────────────────────────────
        public string ActiveFilePath { get { return _currentFilePath; } }

        public void OpenFilePath(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                LoadFileAsync(filePath);
        }

        // ── Settings ──────────────────────────────────────────────────────────
        private void RestoreSettings()
        {
            int dist = _settingsService.LoadSplitterDistance();
            if (dist > 0) mainSplitContainer.SplitterDistance = dist;

            string lastDir = _settingsService.LoadLastDirectory();
            if (!string.IsNullOrEmpty(lastDir) && Directory.Exists(lastDir))
                openLogFileDialog.InitialDirectory = lastDir;
        }

        private void SaveSettings()
        {
            _settingsService.SaveSplitterDistance(mainSplitContainer.SplitterDistance);
            if (!string.IsNullOrEmpty(_currentFilePath))
                _settingsService.SaveLastDirectory(Path.GetDirectoryName(_currentFilePath));
        }

        // ── Status bar ────────────────────────────────────────────────────────
        private void UpdateStatusBar()
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                StatusFileName.Text = StatusLineCount.Text = StatusSelection.Text = "";
                return;
            }
            StatusFileName.Text = Path.GetFileName(_currentFilePath);
            int total   = _allLines.Count;
            int visible = _virtualLines.Count;
            StatusLineCount.Text = total == visible
                ? string.Format("Lines: {0}", total)
                : string.Format("Lines: {0} / {1}", visible, total);
        }

        private void UpdateSelectionStatus()
        {
            if (logListView.SelectedIndices.Count == 0) { StatusSelection.Text = ""; return; }
            int idx = logListView.SelectedIndices[0];
            StatusSelection.Text = string.Format("Ln {0}", _virtualLines[idx].LineNumber);
        }

        // ── Tree view init ────────────────────────────────────────────────────
        private void InitTreeViews()
        {
            ApiTree.ShowLines = ApiTree.ShowPlusMinus = true;
            ApiTree.HideSelection = false;
            CallTree.ShowLines = CallTree.ShowPlusMinus = true;
            CallTree.ShowNodeToolTips = true;
            CallTree.HideSelection = false;

            CallTreeButton.CheckedChanged       += (s, e) => SyncTreeVisibility();
            ApiTreeButton.CheckedChanged        += (s, e) => SyncTreeVisibility();
            HideTabsButton.CheckedChanged       += (s, e) => SyncTabVisibility();
            showCallTreeMenuItem.CheckedChanged += (s, e) => SyncTreeVisibility();
            showApiListMenuItem.CheckedChanged  += (s, e) => SyncTreeVisibility();
            hideAllMenuItem.Click += (s, e) =>
            {
                HideTabsButton.Checked = !HideTabsButton.Checked;
                SyncTabVisibility();
            };

            SyncTreeVisibility();
        }

        // ── Tree population ───────────────────────────────────────────────────
        private void PopulateTrees(List<string> lines)
        {
            var entries  = _parserService.Parse(lines);
            _apiNodes    = _parserService.BuildApiList(entries);
            var callTree = _parserService.BuildCallTree(entries);
            var graph    = _callGraphService.Build(entries);

            PopulateApiTree(_apiNodes);
            PopulateCallTree(callTree);
            PopulatePerformanceTab(_apiNodes, lines.Count);
            callGraphPanel.LoadGraph(graph);
        }

        private void PopulateApiTree(List<ApiCallNode> apiNodes)
        {
            ApiTree.BeginUpdate();
            ApiTree.Nodes.Clear();
            foreach (var node in apiNodes)
            {
                var tn = new TreeNode(node.ToString()) { Tag = node.FirstLine };
                foreach (int lineNo in node.LineNumbers)
                    tn.Nodes.Add(new TreeNode(string.Format("Line {0}", lineNo)) { Tag = lineNo });
                ApiTree.Nodes.Add(tn);
            }
            ApiTree.EndUpdate();
        }

        private void PopulateCallTree(List<CallStackNode> roots)
        {
            CallTree.BeginUpdate();
            CallTree.Nodes.Clear();
            foreach (var root in roots)
                CallTree.Nodes.Add(BuildTreeNode(root));
            // Expand the first level so the tree is immediately useful
            foreach (TreeNode root in CallTree.Nodes)
                root.Expand();
            CallTree.EndUpdate();
        }

        private static TreeNode BuildTreeNode(CallStackNode csNode)
        {
            // Node label: ApiName [duration ms]  (Ln enter→exit)
            string label = csNode.Label;
            if (csNode.DurationMs > 0)
                label = string.Format("{0}  [{1} ms]", label, csNode.DurationMs);
            else if (csNode.ExitLineNumber > 0)
                label = string.Format("{0}  [<1 ms]", label);

            string tooltip = string.Format(
                "API: {0}\nSource: {1}\nENTER line: {2}\nEXIT line: {3}\nDuration: {4} ms",
                csNode.Label,
                csNode.SourceFile ?? "-",
                csNode.LineNumber,
                csNode.ExitLineNumber > 0 ? csNode.ExitLineNumber.ToString() : "?",
                csNode.DurationMs);

            var tn = new TreeNode(label)
            {
                Tag         = csNode.LineNumber,
                ToolTipText = tooltip
            };

            foreach (var child in csNode.Children)
                tn.Nodes.Add(BuildTreeNode(child));

            return tn;
        }

        // ── Performance tab ───────────────────────────────────────────────────
        private void PopulatePerformanceTab(List<ApiCallNode> apiNodes, int totalLines)
        {
            performanceView.BeginUpdate();
            performanceView.Items.Clear();

            var summary = new ListViewItem("── Summary ──");
            summary.SubItems.Add(string.Format("{0} unique APIs", apiNodes.Count));
            summary.SubItems.Add(string.Format("{0} total lines", totalLines));
            summary.BackColor = Color.FromArgb(230, 230, 255);
            performanceView.Items.Add(summary);

            var sorted = new List<ApiCallNode>(apiNodes);
            sorted.Sort((a, b) => b.LineNumbers.Count.CompareTo(a.LineNumbers.Count));
            foreach (var node in sorted)
            {
                var item = new ListViewItem(node.ApiName);
                item.SubItems.Add(node.LineNumbers.Count.ToString());
                item.SubItems.Add(node.FirstLine.ToString());
                performanceView.Items.Add(item);
            }
            performanceView.EndUpdate();
        }

        // ── Tree visibility ───────────────────────────────────────────────────
        private void SyncTreeVisibility()
        {
            bool showCall = CallTreeButton.Checked || showCallTreeMenuItem.Checked;
            bool showApi  = ApiTreeButton.Checked  || showApiListMenuItem.Checked;
            CallTree.Visible = showCall;
            ApiTree.Visible  = showApi;
            showCallTreeMenuItem.Checked = CallTreeButton.Checked = showCall;
            showApiListMenuItem.Checked  = ApiTreeButton.Checked  = showApi;
            LayoutTrees();
        }

        private void SyncTabVisibility()
        {
            mainTabControl.Appearance = HideTabsButton.Checked
                ? TabAppearance.FlatButtons : TabAppearance.Normal;
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
            // In virtual mode we search _virtualLines for a matching line number
            int idx = _virtualLines.FindIndex(v => v.LineNumber == lineNumber.ToString());
            if (idx < 0 || idx >= logListView.VirtualListSize) return;
            logListView.EnsureVisible(idx);
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

            try
            {
                var lines = await _logFileService.ReadLinesAsync(filePath);
                _allLines        = lines;
                _currentFilePath = filePath;
                _searchService.Reset();

                PopulateVirtualListView(_allLines);
                PopulateTrees(_allLines);

                SetDocumentLoaded(true);
                FileStatus.Image = Resources.green_ball;
                _logFileService.WatchFile(filePath);
                UpdateStatusBar();
            }
            catch (UnauthorizedAccessException ex) { ShowLoadError(filePath, "Access denied", ex.Message); }
            catch (IOException ex)                 { ShowLoadError(filePath, "File read error", ex.Message); }
            catch (Exception ex)                   { ShowLoadError(filePath, "Unexpected error", ex.Message); }
            finally
            {
                FileLoadProgress.Visible = false;
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
            for (int i = 0; i < lines.Count; i++)
            {
                _virtualLines.Add(new VirtualLogLine
                {
                    LineNumber = (i + 1).ToString(),
                    Text       = lines[i],
                    BackColour = GetLineColour(lines[i])
                });
            }
            logListView.VirtualListSize = _virtualLines.Count;
            logListView.Invalidate();
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
            logListView.VirtualListSize = _virtualLines.Count;
            logListView.Invalidate();
            UpdateStatusBar();
        }

        private static Color GetLineColour(string line)
        {
            // Use the actual log level code (2nd colon-separated field: E=Error, W=Warning)
            if (string.IsNullOrEmpty(line)) return ColourInfo;
            // Format: "{datetime}: {Level}: ..."  — level is always at index 1 after ": " split
            int first = line.IndexOf(": ", StringComparison.Ordinal);
            if (first >= 0 && first + 3 < line.Length)
            {
                char level = line[first + 2];
                if (level == 'E') return ColourError;
                if (level == 'W') return ColourWarn;
            }
            return ColourInfo;
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
            refreshMenuItem.Enabled  = reloadMenuItem.Enabled
                                     = RefreshButton.Enabled = loaded;
            copyMenuItem.Enabled     = CopyButton.Enabled    = loaded;
            findMenuItem.Enabled     = findNextMenuItem.Enabled
                                     = FindButton.Enabled    = loaded;
            filterMenuItem.Enabled   = FilterButton.Enabled  = loaded;
            FileStatus.Enabled       = loaded;
            FileLoadProgress.Enabled = loaded;
        }

        // ── File menu ─────────────────────────────────────────────────────────
        private void openMenuItem_Click(object sender, EventArgs e)
        {
            if (openLogFileDialog.ShowDialog() == DialogResult.OK)
                LoadFileAsync(openLogFileDialog.FileName);
        }

        private void OpenButton_Click(object sender, EventArgs e) =>
            openMenuItem_Click(sender, e);

        private void saveAsMenuItem_Click(object sender, EventArgs e)
        {
            if (_virtualLines.Count == 0) return;
            if (saveLogFileDialog.ShowDialog() != DialogResult.OK) return;
            try
            {
                var lines = new List<string>();
                if (logListView.SelectedIndices.Count > 0)
                    foreach (int idx in logListView.SelectedIndices)
                        lines.Add(_virtualLines[idx].Text);
                else
                    foreach (var vl in _virtualLines)
                        lines.Add(vl.Text);

                _logFileService.WriteLines(saveLogFileDialog.FileName, lines);
                MessageBox.Show(string.Format("{0} line(s) saved.", lines.Count),
                    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Could not save file:\n{0}", ex.Message),
                    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        public void FindNext(string searchTerm, bool matchCase)
        {
            if (_virtualLines.Count == 0 || string.IsNullOrEmpty(searchTerm)) return;

            var visibleLines = new List<string>(_virtualLines.Count);
            foreach (var vl in _virtualLines) visibleLines.Add(vl.Text);

            int idx = _searchService.FindNext(visibleLines, searchTerm, matchCase);
            if (idx >= 0)
            {
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

        private void findNextMenuItem_Click(object sender, EventArgs e)
        {
            if (_findForm != null && !_findForm.IsDisposed)
                _findForm.TriggerFindNext();
            else
                findMenuItem_Click(sender, e);
        }

        public void ApplyFilter(string filterText, bool matchCase)
        {
            var filtered = _searchService.Filter(_allLines, filterText, matchCase);
            PopulateVirtualListViewFiltered(filtered);
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
            using (var settingsDialog = new SettingsForm())
                settingsDialog.ShowDialog(this);
        }

        private void SettingsButton_Click(object sender, EventArgs e) =>
            settingsMenuItem_Click(sender, e);

        private void aboutMenuItem_Click(object sender, EventArgs e)
        {
            using (var aboutDialog = new AboutForm())
                aboutDialog.ShowDialog(this);
        }

        private void helpMenuItem_Click(object sender, EventArgs e)
        {
            var helpForm = new Form
            {
                Text = "Help — CAD3P Log Browser",
                Size = new System.Drawing.Size(520, 440),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false, MinimizeBox = false
            };
            var rtb = new RichTextBox
            {
                Dock = DockStyle.Fill, ReadOnly = true,
                Font = new System.Drawing.Font("Segoe UI", 9.5f),
                Text =
                    "CAD3P Log Browser — Keyboard Shortcuts\r\n" +
                    "═══════════════════════════════════════\r\n\r\n" +
                    "Ctrl+O      Open log file\r\n" +
                    "Ctrl+S      Save As (selection or all visible)\r\n" +
                    "F5          Refresh (reload, keep scroll position)\r\n" +
                    "Ctrl+R      Reload (reset to top)\r\n" +
                    "Ctrl+C      Copy selected lines\r\n" +
                    "Ctrl+F      Find\r\n" +
                    "F3          Find Next\r\n" +
                    "Ctrl+I      Filter\r\n" +
                    "Ctrl+T      Toggle Call Tree\r\n" +
                    "Ctrl+L      Toggle API List\r\n" +
                    "Ctrl+H      Hide/Show Tabs\r\n" +
                    "Ctrl+E      Settings\r\n" +
                    "Alt+F4      Exit\r\n\r\n" +
                    "Call Graph tab\r\n" +
                    "══════════════\r\n" +
                    "• Scroll wheel to zoom in/out.\r\n" +
                    "• Click and drag to pan.\r\n" +
                    "• Hover a node to highlight its edges.\r\n" +
                    "• Edge thickness = call frequency.\r\n" +
                    "• Reset View button restores default zoom/pan.\r\n\r\n" +
                    "Tips\r\n" +
                    "════\r\n" +
                    "• Drag and drop a log file onto the window to open it.\r\n" +
                    "• Click any tree node to jump to that line in the log.\r\n" +
                    "• Select lines then Save As to save a trimmed log.\r\n" +
                    "• ERROR/FATAL lines are highlighted red, WARN in amber.\r\n" +
                    "• Virtual mode: the log list handles 500k+ lines smoothly.\r\n"
            };
            helpForm.Controls.Add(rtb);
            helpForm.ShowDialog(this);
        }

        // ── Form lifecycle ────────────────────────────────────────────────────
        private void MainForm_Load(object sender, EventArgs e)
        {
            SetDocumentLoaded(false);
            LayoutTrees();
            logTab.Text = "Log";
            performanceTab.Text = "Performance";
            logDetailTab.Text = "Log Details";
            callGraphTab.Text = "Call Graph";
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveSettings();
            _logFileService.Dispose();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) { }
        private void MainForm_ResizeBegin(object sender, EventArgs e) { }
        private void MainForm_ResizeEnd(object sender, EventArgs e) => LayoutTrees();
        private void MainForm_Resize(object sender, EventArgs e) { }
        private void MainForm_SizeChanged(object sender, EventArgs e) { }
        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e) => LayoutTrees();
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
                logContextMenu.Show(ApiTree, e.Location);
        }

        private void CallTree_MouseClick(object sender, MouseEventArgs e) { }

        private void CallTree_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                logContextMenu.Show(CallTree, e.Location);
        }

        private void logWatcher_Changed(object sender, System.IO.FileSystemEventArgs e) { }
    }
}
