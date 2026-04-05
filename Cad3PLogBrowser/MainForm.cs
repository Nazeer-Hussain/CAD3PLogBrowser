using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Cad3PLogBrowser.Properties;
using Cad3PLogBrowser.Services;

namespace Cad3PLogBrowser
{
    public partial class mainFrm : Form
    {
        // ── Services ──────────────────────────────────────────────────────────
        private readonly LogFileService   _logFileService;
        private readonly SearchService    _searchService;
        private readonly SettingsService  _settingsService;
        private readonly LogParserService _parserService;

        // ── State ─────────────────────────────────────────────────────────────
        private string            _currentFilePath = string.Empty;
        private bool              _isLoading       = false;
        private List<string>      _allLines        = new List<string>();
        private List<ApiCallNode> _apiNodes        = new List<ApiCallNode>();

        // ── Log-level colours (improvement #10) ───────────────────────────────
        private static readonly Color ColourError  = Color.FromArgb(255, 220, 220);
        private static readonly Color ColourWarn   = Color.FromArgb(255, 243, 205);
        private static readonly Color ColourInfo   = SystemColors.Window;

        // ── Construction ──────────────────────────────────────────────────────
        public mainFrm()
        {
            InitializeComponent();

            _settingsService = new SettingsService();
            _searchService   = new SearchService();
            _parserService   = new LogParserService();
            _logFileService  = new LogFileService(this);
            _logFileService.FileChangedOnDisk += OnFileChangedOnDisk;

            RestoreSettings();
            InitTreeViews();
            InitListView();
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
            if (dist > 0) splitContainer1.SplitterDistance = dist;

            string lastDir = _settingsService.LoadLastDirectory();
            if (!string.IsNullOrEmpty(lastDir) && Directory.Exists(lastDir))
                openFileDialog1.InitialDirectory = lastDir;
        }

        private void SaveSettings()
        {
            _settingsService.SaveSplitterDistance(splitContainer1.SplitterDistance);
            if (!string.IsNullOrEmpty(_currentFilePath))
                _settingsService.SaveLastDirectory(Path.GetDirectoryName(_currentFilePath));
        }

        // ── #10: ListView colour-coding init ──────────────────────────────────
        private void InitListView()
        {
            listView1.OwnerDraw = false; // let Windows paint; we set BackColor per item
        }

        // ── #1: Status bar ────────────────────────────────────────────────────
        private void UpdateStatusBar()
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                StatusFileName.Text  = "";
                StatusLineCount.Text = "";
                StatusSelection.Text = "";
                return;
            }

            StatusFileName.Text = Path.GetFileName(_currentFilePath);

            int total    = _allLines.Count;
            int visible  = listView1.Items.Count;
            StatusLineCount.Text = total == visible
                ? string.Format("Lines: {0}", total)
                : string.Format("Lines: {0} / {1}", visible, total);
        }

        private void UpdateSelectionStatus()
        {
            if (listView1.SelectedItems.Count == 0)
            {
                StatusSelection.Text = "";
                return;
            }
            int lineNo = listView1.SelectedItems[0].Index + 1;
            StatusSelection.Text = string.Format("Ln {0}", lineNo);
        }

        // ── Tree view init ────────────────────────────────────────────────────
        private void InitTreeViews()
        {
            ApiTree.ShowLines     = true;
            ApiTree.ShowPlusMinus = true;
            ApiTree.HideSelection = false;
            CallTree.ShowLines    = true;
            CallTree.ShowPlusMinus = true;
            CallTree.HideSelection = false;

            CallTreeButton.CheckedChanged += (s, e) => SyncTreeVisibility();
            ApiTreeButton.CheckedChanged  += (s, e) => SyncTreeVisibility();
            HideTabsButton.CheckedChanged += (s, e) => SyncTabVisibility();
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

            PopulateApiTree(_apiNodes);
            PopulateCallTree(callTree);
            PopulatePerformanceTab(_apiNodes, lines.Count);
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
            CallTree.EndUpdate();
        }

        private static TreeNode BuildTreeNode(CallStackNode csNode)
        {
            var tn = new TreeNode(csNode.Label) { Tag = csNode.LineNumber };
            foreach (var child in csNode.Children)
                tn.Nodes.Add(BuildTreeNode(child));
            return tn;
        }

        // ── #2: Performance tab ───────────────────────────────────────────────
        private void PopulatePerformanceTab(List<ApiCallNode> apiNodes, int totalLines)
        {
            performanceView.BeginUpdate();
            performanceView.Items.Clear();

            // Summary row
            var summary = new ListViewItem("── Summary ──");
            summary.SubItems.Add(string.Format("{0} unique APIs", apiNodes.Count));
            summary.SubItems.Add(string.Format("{0} total lines", totalLines));
            summary.BackColor = Color.FromArgb(230, 230, 255);
            performanceView.Items.Add(summary);

            // Sort by call count descending (top callers first)
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

        // ── Tree visibility toggles ───────────────────────────────────────────
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
            bool hide = HideTabsButton.Checked;
            tabControl1.Appearance = hide ? TabAppearance.FlatButtons : TabAppearance.Normal;
        }

        private void LayoutTrees()
        {
            int h = splitContainer1.Panel1.ClientSize.Height;
            int w = splitContainer1.Panel1.ClientSize.Width;
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

        // ── Tree → scroll log + show details ─────────────────────────────────
        private void ApiTree_AfterSelect(object sender, TreeViewEventArgs e) =>
            ScrollLogToLine(e.Node?.Tag);

        private void ApiTree_Click(object sender, EventArgs e) { }
        private void ApiTree_MouseClick(object sender, MouseEventArgs e) { }

        private void CallTree_AfterSelect(object sender, TreeViewEventArgs e) =>
            ScrollLogToLine(e.Node?.Tag);

        private void ScrollLogToLine(object tag)
        {
            if (!(tag is int lineNumber)) return;
            int idx = lineNumber - 1;
            if (idx < 0 || idx >= listView1.Items.Count) return;
            listView1.SelectedItems.Clear();
            listView1.Items[idx].Selected = true;
            listView1.Items[idx].EnsureVisible();
            listView1.Focus();
            ShowLogDetail(idx);
        }

        // ── #3: Log Details panel ─────────────────────────────────────────────
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectionStatus();
            if (listView1.SelectedItems.Count > 0)
                ShowLogDetail(listView1.SelectedItems[0].Index);
        }

        private void ShowLogDetail(int idx)
        {
            if (idx < 0 || idx >= listView1.Items.Count) return;
            string lineText = listView1.Items[idx].SubItems[1].Text;
            int lineNo = idx + 1;
            logDetailBox.Text = string.Format("Line {0}:\r\n\r\n{1}", lineNo, lineText);
        }

        // ── #6: Drag-and-drop file open ───────────────────────────────────────
        private void mainFrm_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        }

        private void mainFrm_DragDrop(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null && files.Length > 0)
                LoadFileAsync(files[0]);
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

                PopulateListView(_allLines);
                PopulateTrees(_allLines);

                SetDocumentLoaded(true);
                FileStatus.Image = Resources.green_ball;
                _logFileService.WatchFile(filePath);
                UpdateStatusBar();
                tabPage1.Text = "Log";
                tabPage3.Text = "Performance";
                tabPage4.Text = "Log Details";
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

        // ── #10: Colour-coded ListView population ─────────────────────────────
        private void PopulateListView(IList<string> lines)
        {
            listView1.BeginUpdate();
            listView1.Items.Clear();
            for (int i = 0; i < lines.Count; i++)
            {
                var item = new ListViewItem((i + 1).ToString());
                item.SubItems.Add(lines[i]);
                item.BackColor = GetLineColour(lines[i]);
                listView1.Items.Add(item);
            }
            listView1.EndUpdate();
            UpdateStatusBar();
        }

        private void PopulateListViewFiltered(IList<FilteredLine> filtered)
        {
            listView1.BeginUpdate();
            listView1.Items.Clear();
            foreach (var fl in filtered)
            {
                var item = new ListViewItem(fl.LineNumber.ToString());
                item.SubItems.Add(fl.Text);
                item.BackColor = GetLineColour(fl.Text);
                listView1.Items.Add(item);
            }
            listView1.EndUpdate();
            UpdateStatusBar();
        }

        private static Color GetLineColour(string line)
        {
            if (line == null) return ColourInfo;
            string u = line.ToUpperInvariant();
            if (u.Contains("ERROR") || u.Contains("FATAL") || u.Contains("EXCEPTION"))
                return ColourError;
            if (u.Contains("WARN"))
                return ColourWarn;
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

        // ── File watcher callback ─────────────────────────────────────────────
        private void OnFileChangedOnDisk(object sender, EventArgs e)
        {
            if (!_isLoading) FileStatus.Image = Resources.red_ball;
        }

        // ── UI state helper ───────────────────────────────────────────────────
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
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                LoadFileAsync(openFileDialog1.FileName);
        }

        private void OpenButton_Click(object sender, EventArgs e) =>
            openMenuItem_Click(sender, e);

        // ── #4: Save Selected (from tree selection) ───────────────────────────
        private void saveAsMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count == 0) return;
            if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;
            try
            {
                // If tree node(s) selected → save only those lines; else save all visible.
                var lines = new List<string>();
                if (listView1.SelectedItems.Count > 0)
                {
                    foreach (ListViewItem item in listView1.SelectedItems)
                        lines.Add(item.SubItems[1].Text);
                }
                else
                {
                    foreach (ListViewItem item in listView1.Items)
                        lines.Add(item.SubItems[1].Text);
                }
                _logFileService.WriteLines(saveFileDialog1.FileName, lines);
                MessageBox.Show(
                    string.Format("{0} line(s) saved.", lines.Count),
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
            int topIndex = listView1.TopItem != null ? listView1.TopItem.Index : 0;
            LoadFileAsync(_currentFilePath);
            BeginInvoke((Action)(() =>
            {
                if (topIndex < listView1.Items.Count)
                    listView1.EnsureVisible(topIndex);
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
            if (listView1.SelectedItems.Count == 0) return;
            var lines = new List<string>();
            foreach (ListViewItem item in listView1.SelectedItems)
                lines.Add(item.SubItems[1].Text);
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
            if (listView1.Items.Count == 0 || string.IsNullOrEmpty(searchTerm)) return;
            var visibleLines = new List<string>();
            foreach (ListViewItem item in listView1.Items)
                visibleLines.Add(item.SubItems[1].Text);

            int idx = _searchService.FindNext(visibleLines, searchTerm, matchCase);
            if (idx >= 0)
            {
                listView1.SelectedItems.Clear();
                listView1.Items[idx].Selected = true;
                listView1.Items[idx].EnsureVisible();
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
            PopulateListViewFiltered(filtered);
        }

        private void filterMenuItem_Click(object sender, EventArgs e)
        {
            using (var filterFrm = new FilterFrm(this))
                filterFrm.ShowDialog(this);
        }

        private void FilterButton_Click(object sender, EventArgs e) =>
            filterMenuItem_Click(sender, e);

        private void filterToolStripMenuItem_Click(object sender, EventArgs e) =>
            filterMenuItem_Click(sender, e);

        // ── Options / Help ────────────────────────────────────────────────────
        private void settingsMenuItem_Click(object sender, EventArgs e)
        {
            using (var settingsFrm = new SettingsFrm())
                settingsFrm.ShowDialog(this);
        }

        private void SettingsButton_Click(object sender, EventArgs e) =>
            settingsMenuItem_Click(sender, e);

        private void aboutMenuItem_Click(object sender, EventArgs e)
        {
            using (var abtBox = new AbtBox())
                abtBox.ShowDialog(this);
        }

        // ── #5: Help content ──────────────────────────────────────────────────
        private void helpMenuItem_Click(object sender, EventArgs e)
        {
            var help = new Form
            {
                Text = "Help — CAD3P Log Browser",
                Size = new System.Drawing.Size(520, 420),
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
                    "Ctrl+S      Save As\r\n" +
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
                    "Tips\r\n" +
                    "════\r\n" +
                    "• Drag and drop a log file onto the window to open it.\r\n" +
                    "• Click any tree node to jump to that line in the log.\r\n" +
                    "• Select lines then Save As to save a trimmed log.\r\n" +
                    "• ERROR/FATAL lines are highlighted red, WARN in amber.\r\n" +
                    "• The Performance tab shows API call frequency.\r\n" +
                    "• The Log Details tab shows the full selected line.\r\n"
            };
            help.Controls.Add(rtb);
            help.ShowDialog(this);
        }

        // ── Form lifecycle ────────────────────────────────────────────────────
        private void mainFrm_Load(object sender, EventArgs e)
        {
            SetDocumentLoaded(false);
            LayoutTrees();
            tabPage1.Text = "Log";
            tabPage3.Text = "Performance";
            tabPage4.Text = "Log Details";
        }

        private void mainFrm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveSettings();
            _logFileService.Dispose();
        }

        private void mainFrm_FormClosing(object sender, FormClosingEventArgs e) { }
        private void mainFrm_ResizeBegin(object sender, EventArgs e) { }
        private void mainFrm_ResizeEnd(object sender, EventArgs e) => LayoutTrees();
        private void mainFrm_Resize(object sender, EventArgs e) { }
        private void mainFrm_SizeChanged(object sender, EventArgs e) { }
        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e) => LayoutTrees();
        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e) { }
        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e) { }

        // ── Context menu ──────────────────────────────────────────────────────
        private void listView1_MouseUp(object sender, MouseEventArgs e) { }

        private void listView1_MouseUp_1(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                contextMenuStrip1.Show(listView1, e.Location);
        }

        private void ApiTree_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                contextMenuStrip1.Show(ApiTree, e.Location);
        }

        private void CallTree_MouseClick(object sender, MouseEventArgs e) { }

        private void CallTree_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                contextMenuStrip1.Show(CallTree, e.Location);
        }

        private void logWatcher_Changed(object sender, System.IO.FileSystemEventArgs e) { }
    }
}
