using System;
using System.Collections.Generic;
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
        private string       _currentFilePath = string.Empty;
        private bool         _isLoading       = false;
        private List<string> _allLines        = new List<string>();

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
            if (dist > 0)
                splitContainer1.SplitterDistance = dist;

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

        // ── Tree view initialisation ──────────────────────────────────────────
        private void InitTreeViews()
        {
            ApiTree.ShowLines    = true;
            ApiTree.ShowPlusMinus = true;
            ApiTree.HideSelection = false;

            CallTree.ShowLines    = true;
            CallTree.ShowPlusMinus = true;
            CallTree.HideSelection = false;

            // Wire up toolbar toggle buttons.
            CallTreeButton.CheckedChanged += (s, e) => SyncTreeVisibility();
            ApiTreeButton.CheckedChanged  += (s, e) => SyncTreeVisibility();
            HideTabsButton.CheckedChanged += (s, e) => SyncTabVisibility();

            // Wire up View menu items.
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
            var entries     = _parserService.Parse(lines);
            var apiList     = _parserService.BuildApiList(entries);
            var callTree    = _parserService.BuildCallTree(entries);

            PopulateApiTree(apiList);
            PopulateCallTree(callTree);
        }

        private void PopulateApiTree(List<ApiCallNode> apiNodes)
        {
            ApiTree.BeginUpdate();
            ApiTree.Nodes.Clear();

            foreach (var node in apiNodes)
            {
                var tn = new TreeNode(node.ToString())
                {
                    Tag = node.FirstLine   // line number to jump to on click
                };
                // Sub-nodes: each call occurrence with its line number.
                foreach (int lineNo in node.LineNumbers)
                {
                    var child = new TreeNode(string.Format("Line {0}", lineNo))
                    {
                        Tag = lineNo
                    };
                    tn.Nodes.Add(child);
                }
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

        // ── Tree visibility toggles ───────────────────────────────────────────
        private void SyncTreeVisibility()
        {
            bool showCall = CallTreeButton.Checked || showCallTreeMenuItem.Checked;
            bool showApi  = ApiTreeButton.Checked  || showApiListMenuItem.Checked;

            CallTree.Visible = showCall;
            ApiTree.Visible  = showApi;

            // Keep menu and toolbar in sync.
            showCallTreeMenuItem.Checked = CallTreeButton.Checked = showCall;
            showApiListMenuItem.Checked  = ApiTreeButton.Checked  = showApi;

            LayoutTrees();
        }

        private void SyncTabVisibility()
        {
            bool hide = HideTabsButton.Checked;
            tabControl1.Appearance = hide
                ? TabAppearance.FlatButtons   // hides tab headers
                : TabAppearance.Normal;
        }

        /// <summary>Stack the two trees vertically inside Panel1, splitting evenly.</summary>
        private void LayoutTrees()
        {
            int panelH = splitContainer1.Panel1.ClientSize.Height;
            int panelW = splitContainer1.Panel1.ClientSize.Width;

            bool showCall = CallTree.Visible;
            bool showApi  = ApiTree.Visible;

            if (showCall && showApi)
            {
                int half = panelH / 2;
                ApiTree.SetBounds(0, 0, panelW, half);
                CallTree.SetBounds(0, half, panelW, panelH - half);
            }
            else if (showCall)
            {
                CallTree.SetBounds(0, 0, panelW, panelH);
            }
            else if (showApi)
            {
                ApiTree.SetBounds(0, 0, panelW, panelH);
            }
        }

        // ── Tree node click → scroll log view ────────────────────────────────
        private void ApiTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ScrollLogToLine(e.Node?.Tag);
        }

        private void ApiTree_Click(object sender, EventArgs e) { }
        private void ApiTree_MouseClick(object sender, MouseEventArgs e) { }

        private void CallTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ScrollLogToLine(e.Node?.Tag);
        }

        private void ScrollLogToLine(object tag)
        {
            if (tag == null) return;
            if (!(tag is int lineNumber)) return;

            // ListView items are 0-based; line numbers are 1-based.
            int idx = lineNumber - 1;
            if (idx < 0 || idx >= listView1.Items.Count) return;

            listView1.SelectedItems.Clear();
            listView1.Items[idx].Selected = true;
            listView1.Items[idx].EnsureVisible();
            listView1.Focus();
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
            }
            catch (UnauthorizedAccessException ex)
            {
                ShowLoadError(filePath, "Access denied", ex.Message);
            }
            catch (IOException ex)
            {
                ShowLoadError(filePath, "File read error", ex.Message);
            }
            catch (Exception ex)
            {
                ShowLoadError(filePath, "Unexpected error", ex.Message);
            }
            finally
            {
                FileLoadProgress.Visible = false;
                _isLoading = false;
            }
        }

        private void PopulateListView(IList<string> lines)
        {
            listView1.BeginUpdate();
            listView1.Items.Clear();
            for (int i = 0; i < lines.Count; i++)
            {
                var item = new ListViewItem((i + 1).ToString());
                item.SubItems.Add(lines[i]);
                listView1.Items.Add(item);
            }
            listView1.EndUpdate();
        }

        private void PopulateListViewFiltered(IList<FilteredLine> filtered)
        {
            listView1.BeginUpdate();
            listView1.Items.Clear();
            foreach (var fl in filtered)
            {
                var item = new ListViewItem(fl.LineNumber.ToString());
                item.SubItems.Add(fl.Text);
                listView1.Items.Add(item);
            }
            listView1.EndUpdate();
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
            if (!_isLoading)
                FileStatus.Image = Resources.red_ball;
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

        private void saveAsMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count == 0) return;
            if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;

            try
            {
                var lines = new List<string>();
                foreach (ListViewItem item in listView1.Items)
                    lines.Add(item.SubItems[1].Text);

                _logFileService.WriteLines(saveFileDialog1.FileName, lines);
                MessageBox.Show("File saved successfully.", Resources.TITLE,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void helpMenuItem_Click(object sender, EventArgs e) { }

        // ── Form lifecycle ────────────────────────────────────────────────────
        private void mainFrm_Load(object sender, EventArgs e)
        {
            SetDocumentLoaded(false);
            LayoutTrees();
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

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e) =>
            LayoutTrees();

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e) { }
        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e) { }

        // ── Context menu ──────────────────────────────────────────────────────
        private void listView1_MouseUp(object sender, MouseEventArgs e) { }
        private void listView1_SelectedIndexChanged(object sender, EventArgs e) { }

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
