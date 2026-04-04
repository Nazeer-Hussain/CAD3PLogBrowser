using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cad3PLogBrowser.Properties;

namespace Cad3PLogBrowser
{
    public partial class mainFrm : Form
    {
        // ── State ────────────────────────────────────────────────────────────
        private string _currentFilePath = string.Empty;
        private bool _isLoading = false;
        private int _lastFoundIndex = -1;
        private string _lastSearchTerm = string.Empty;
        private List<string> _allLines = new List<string>();

        // ── Construction ─────────────────────────────────────────────────────
        public mainFrm()
        {
            InitializeComponent();
            RestoreSettings();
        }

        // ── Public API (used by Program.cs command-line arg) ──────────────────
        public string ActiveFilePath
        {
            get { return _currentFilePath; }
        }

        public void OpenFilePath(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                LoadFileAsync(filePath);
        }

        // ── Settings ─────────────────────────────────────────────────────────
        private void RestoreSettings()
        {
            try
            {
                var raw = Application.UserAppDataRegistry.GetValue("LastSplitter");
                if (raw != null && int.TryParse(raw.ToString(), out int dist) && dist > 0)
                    splitContainer1.SplitterDistance = dist;
            }
            catch
            {
                // First-run: registry key doesn't exist yet; leave default splitter position.
            }
        }

        private void SaveSettings()
        {
            try
            {
                Application.UserAppDataRegistry.SetValue("LastSplitter",
                    splitContainer1.SplitterDistance.ToString());
            }
            catch { /* Non-fatal */ }
        }

        // ── File loading (async) ──────────────────────────────────────────────
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
                var lines = await ReadLinesAsync(filePath);

                _allLines = lines;
                _currentFilePath = filePath;
                _lastFoundIndex = -1;
                _lastSearchTerm = string.Empty;

                PopulateListView(_allLines);

                SetDocumentLoaded(true);
                FileStatus.Image = Resources.green_ball;
                WatchFile(filePath);
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

        private static Task<List<string>> ReadLinesAsync(string filePath)
        {
            return Task.Run(() =>
            {
                var lines = new List<string>();
                // FileShare.ReadWrite allows reading logs still being written.
                using (var stream = new FileStream(filePath, FileMode.Open,
                    FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(stream, Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: true))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                        lines.Add(line);
                }
                return lines;
            });
        }

        private void PopulateListView(List<string> lines)
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

        private void ShowLoadError(string filePath, string reason, string detail)
        {
            SetDocumentLoaded(false);
            FileStatus.Image = Resources.red_ball;
            MessageBox.Show(
                string.Format("{0}:\n{1}\n\nFile: {2}", reason, detail, filePath),
                Resources.TITLE,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        // ── File watcher ──────────────────────────────────────────────────────
        private void WatchFile(string filePath)
        {
            try
            {
                logWatcher.EnableRaisingEvents = false;
                logWatcher.Path = Path.GetDirectoryName(filePath);
                logWatcher.Filter = Path.GetFileName(filePath);
                logWatcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite;
                logWatcher.EnableRaisingEvents = true;
            }
            catch { /* Non-fatal: user can refresh manually */ }
        }

        private void logWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            // SynchronizingObject = this is set in designer, so this runs on the UI thread.
            if (!_isLoading)
                FileStatus.Image = Resources.red_ball;
        }

        // ── UI state helper ───────────────────────────────────────────────────
        private void SetDocumentLoaded(bool loaded)
        {
            saveAsMenuItem.Enabled = SaveButton.Enabled = loaded;
            refreshMenuItem.Enabled = reloadMenuItem.Enabled = RefreshButton.Enabled = loaded;
            copyMenuItem.Enabled = CopyButton.Enabled = loaded;
            findMenuItem.Enabled = findNextMenuItem.Enabled = FindButton.Enabled = loaded;
            filterMenuItem.Enabled = FilterButton.Enabled = loaded;
            FileStatus.Enabled = loaded;
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
                var sb = new StringBuilder();
                foreach (ListViewItem item in listView1.Items)
                    sb.AppendLine(item.SubItems[1].Text);

                File.WriteAllText(saveFileDialog1.FileName, sb.ToString(), Encoding.UTF8);
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

            var sb = new StringBuilder();
            foreach (ListViewItem item in listView1.SelectedItems)
                sb.AppendLine(item.SubItems[1].Text);

            Clipboard.SetText(sb.ToString());
        }

        private void CopyButton_Click(object sender, EventArgs e) =>
            copyMenuItem_Click(sender, e);

        // Find – modeless dialog
        private FindForm _findForm;

        private void findMenuItem_Click(object sender, EventArgs e)
        {
            if (_findForm == null || _findForm.IsDisposed)
            {
                _findForm = new FindForm(this);
                _findForm.Left = Right - _findForm.Width - 20;
                _findForm.Top = Top + 80;
            }
            _findForm.Show();
            _findForm.BringToFront();
        }

        private void FindButton_Click(object sender, EventArgs e) =>
            findMenuItem_Click(sender, e);

        // FindNext – called by menu shortcut or FindForm
        public void FindNext(string searchTerm, bool matchCase)
        {
            if (listView1.Items.Count == 0 || string.IsNullOrEmpty(searchTerm)) return;

            var comp = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            if (!string.Equals(searchTerm, _lastSearchTerm, comp))
            {
                _lastFoundIndex = -1;
                _lastSearchTerm = searchTerm;
            }

            int start = _lastFoundIndex + 1;
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                int idx = (start + i) % listView1.Items.Count;
                if (listView1.Items[idx].SubItems[1].Text.IndexOf(searchTerm, comp) >= 0)
                {
                    listView1.SelectedItems.Clear();
                    listView1.Items[idx].Selected = true;
                    listView1.Items[idx].EnsureVisible();
                    _lastFoundIndex = idx;
                    return;
                }
            }

            MessageBox.Show(string.Format("'{0}' not found.", searchTerm), Resources.TITLE,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void findNextMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_lastSearchTerm))
                FindNext(_lastSearchTerm, matchCase: false);
            else
                findMenuItem_Click(sender, e);
        }

        // Filter
        private void filterMenuItem_Click(object sender, EventArgs e)
        {
            using (var filterFrm = new FilterFrm(this))
                filterFrm.ShowDialog(this);
        }

        private void FilterButton_Click(object sender, EventArgs e) =>
            filterMenuItem_Click(sender, e);

        private void filterToolStripMenuItem_Click(object sender, EventArgs e) =>
            filterMenuItem_Click(sender, e);

        // Called by FilterForm to apply or clear a filter
        public void ApplyFilter(string filterText, bool matchCase)
        {
            if (string.IsNullOrWhiteSpace(filterText))
            {
                PopulateListView(_allLines);
                return;
            }

            var comp = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            listView1.BeginUpdate();
            listView1.Items.Clear();

            for (int i = 0; i < _allLines.Count; i++)
            {
                if (_allLines[i].IndexOf(filterText, comp) >= 0)
                {
                    var item = new ListViewItem((i + 1).ToString());
                    item.SubItems.Add(_allLines[i]);
                    listView1.Items.Add(item);
                }
            }

            listView1.EndUpdate();
        }

        // ── Options / Help menus ──────────────────────────────────────────────
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
        private void mainFrm_Load(object sender, EventArgs e) =>
            SetDocumentLoaded(false);

        private void mainFrm_FormClosed(object sender, FormClosedEventArgs e) =>
            SaveSettings();

        private void mainFrm_FormClosing(object sender, FormClosingEventArgs e) { }
        private void mainFrm_ResizeBegin(object sender, EventArgs e) { }
        private void mainFrm_ResizeEnd(object sender, EventArgs e) { }
        private void mainFrm_Resize(object sender, EventArgs e) { }
        private void mainFrm_SizeChanged(object sender, EventArgs e) { }
        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e) { }
        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e) { }
        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e) { }

        // ── Context menu & tree stubs ─────────────────────────────────────────
        private void listView1_MouseUp(object sender, MouseEventArgs e) { }
        private void listView1_SelectedIndexChanged(object sender, EventArgs e) { }

        private void listView1_MouseUp_1(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                contextMenuStrip1.Show(listView1, e.Location);
        }

        private void ApiTree_AfterSelect(object sender, TreeViewEventArgs e) { }
        private void ApiTree_Click(object sender, EventArgs e) { }
        private void ApiTree_MouseClick(object sender, MouseEventArgs e) { }

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
    }
}
