using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Cad3PLogBrowser.Models.Comparison;
using Cad3PLogBrowser.Services;
using Cad3PLogBrowser.Services.Comparison;

namespace Cad3PLogBrowser.UI
{
    /// <summary>
    /// Professional log comparison viewer with BeyondCompare-style interface.
    /// </summary>
    public partial class CompareLogsForm : Form
    {
        private readonly LogParserService _parserService;
        private CompareOptions _compareOptions;
        private TreeComparer _comparer;
        private DifferenceNavigator _navigator;
        private List<TreeDifference> _differences;
        private string _leftFilePath;
        private string _rightFilePath;
        private readonly Services.Analysis.LogDiffService _diffService;
        private readonly DifferenceHighlighter _highlighter = new DifferenceHighlighter();
        private List<Services.LogEntry> _leftEntries;
        private List<Services.LogEntry> _rightEntries;

        // ── Synchronized scrolling ─────────────────────────────────────────────
        // TreeView has no native "scrolled" event, so a lightweight poll is the
        // simplest way to catch every cause of movement (wheel, scrollbar drag,
        // keyboard, expand/collapse) uniformly.
        private readonly System.Windows.Forms.Timer _scrollSyncTimer;
        private TreeNode _lastLeftTopNode;
        private TreeNode _lastRightTopNode;

        public CompareLogsForm()
        {
            InitializeComponent();

            _parserService = new LogParserService();
            _compareOptions = CompareOptions.CreateDefaultLogOptions();
            _comparer = new TreeComparer();
            _differences = new List<TreeDifference>();
            _diffService = new Services.Analysis.LogDiffService();

            InitializeNavigator();
            ConfigureTreeViews();
            UpdateNavigationButtons();
            SetFormIcon();
            ApplyIcons();
            AddHeaderAccentBars();

            _scrollSyncTimer = new System.Windows.Forms.Timer { Interval = 80 };
            _scrollSyncTimer.Tick += ScrollSyncTimer_Tick;
            _scrollSyncTimer.Start();
            this.FormClosed += (s, e) => _scrollSyncTimer.Stop();

            // G2: this window never applied the app's Light/Dark theme at all —
            // Load (not the constructor) so the handle/controls exist first, same
            // guard used by every other themed dialog in the app.
            this.Load += (s, e) => ThemeManager.ApplyTheme(this);
        }

        /// <summary>
        /// Keeps the left and right trees scrolled to the same visible row position.
        /// Runs continuously but only acts while "Synchronize Scrolling" is checked.
        /// </summary>
        private void ScrollSyncTimer_Tick(object sender, EventArgs e)
        {
            if (leftTreeView.Nodes.Count == 0 || rightTreeView.Nodes.Count == 0) return;

            if (!syncScrollMenuItem.Checked)
            {
                _lastLeftTopNode  = leftTreeView.TopNode;
                _lastRightTopNode = rightTreeView.TopNode;
                return;
            }

            if (!ReferenceEquals(leftTreeView.TopNode, _lastLeftTopNode))
            {
                int idx = VisibleIndexOf(leftTreeView, leftTreeView.TopNode);
                var target = NodeAtVisibleIndex(rightTreeView, idx);
                if (target != null) rightTreeView.TopNode = target;
            }
            else if (!ReferenceEquals(rightTreeView.TopNode, _lastRightTopNode))
            {
                int idx = VisibleIndexOf(rightTreeView, rightTreeView.TopNode);
                var target = NodeAtVisibleIndex(leftTreeView, idx);
                if (target != null) leftTreeView.TopNode = target;
            }

            _lastLeftTopNode  = leftTreeView.TopNode;
            _lastRightTopNode = rightTreeView.TopNode;
        }

        /// <summary>Counts visible (non-collapsed-away) nodes from the top of the tree down to <paramref name="target"/>.</summary>
        private static int VisibleIndexOf(TreeView tv, TreeNode target)
        {
            if (target == null || tv.Nodes.Count == 0) return 0;
            int idx = 0;
            var node = tv.Nodes[0];
            while (node != null && node != target)
            {
                node = node.NextVisibleNode;
                idx++;
            }
            return idx;
        }

        /// <summary>The node at the given visible row offset from the top of the tree, or the last node if the tree is shorter.</summary>
        private static TreeNode NodeAtVisibleIndex(TreeView tv, int index)
        {
            if (tv.Nodes.Count == 0) return null;
            var node = tv.Nodes[0];
            while (node.NextVisibleNode != null && index > 0)
            {
                node = node.NextVisibleNode;
                index--;
            }
            return node;
        }

        /// <summary>
        /// Thin colour-coded strip above each header label — a familiar diff-tool
        /// cue that distinguishes the Left/Right sides at a glance (matches the
        /// app's existing accent palette: blue for primary, amber for secondary).
        /// </summary>
        private void AddHeaderAccentBars()
        {
            leftHeaderPanel.Controls.Add(new Panel
            {
                Dock = DockStyle.Top, Height = 3,
                BackColor = Color.FromArgb(0, 122, 204)
            });
            rightHeaderPanel.Controls.Add(new Panel
            {
                Dock = DockStyle.Top, Height = 3,
                BackColor = Color.FromArgb(215, 140, 20)
            });
        }

        /// <summary>Assigns generated toolbar/menu icons — mirrors MainForm's icon-application convention.</summary>
        private void ApplyIcons()
        {
            var sz  = IconGenerator.IconSize.Medium; // toolbar
            var msz = IconGenerator.IconSize.Small;  // menu

            // Toolbar
            browseLeftButton.Image  = IconGenerator.CreateOpenIcon(sz);
            browseRightButton.Image = IconGenerator.CreateOpenIcon(sz);
            compareButton.Image     = IconGenerator.CreateCompareIcon(sz);
            firstDiffButton.Image   = IconGenerator.CreateDiffFirstIcon(sz);
            prevDiffButton.Image    = IconGenerator.CreateDiffPrevIcon(sz);
            nextDiffButton.Image    = IconGenerator.CreateDiffNextIcon(sz);
            lastDiffButton.Image    = IconGenerator.CreateDiffLastIcon(sz);
            optionsButton.Image     = IconGenerator.CreateSettingsIcon(sz);

            // File menu
            browseLeftMenuItem.Image  = IconGenerator.CreateOpenIcon(msz);
            browseRightMenuItem.Image = IconGenerator.CreateOpenIcon(msz);
            compareMenuItem.Image     = IconGenerator.CreateCompareIcon(msz);
            swapFilesMenuItem.Image   = IconGenerator.CreateSwapIcon(msz);
            closeMenuItem.Image       = IconGenerator.CreateExitIcon(msz);

            // Navigation menu
            firstDiffMenuItem.Image = IconGenerator.CreateDiffFirstIcon(msz);
            prevDiffMenuItem.Image  = IconGenerator.CreateDiffPrevIcon(msz);
            nextDiffMenuItem.Image  = IconGenerator.CreateDiffNextIcon(msz);
            lastDiffMenuItem.Image  = IconGenerator.CreateDiffLastIcon(msz);

            // View menu
            expandAllMenuItem.Image   = IconGenerator.CreateExpandIcon(msz);
            collapseAllMenuItem.Image = IconGenerator.CreateCollapseIcon(msz);
            optionsMenuItem.Image     = IconGenerator.CreateSettingsIcon(msz);
        }

        /// <summary>Opens the comparison window pre-loaded with two known file paths and runs the comparison immediately.</summary>
        public CompareLogsForm(string leftFilePath, string rightFilePath)
            : this()
        {
            _leftFilePath  = leftFilePath;
            _rightFilePath = rightFilePath;
            leftTitleLabel.Text  = string.Format("Left: {0}", Path.GetFileName(leftFilePath));
            rightTitleLabel.Text = string.Format("Right: {0}", Path.GetFileName(rightFilePath));
            UpdateCompareButtonState();

            LoadAndCompareFiles(leftFilePath, rightFilePath);
        }

        public CompareLogsForm(TreeView leftTreeView, TreeView rightTreeView, string leftTitle, string rightTitle)
            : this()
        {
            if (leftTreeView != null && rightTreeView != null)
            {
                CopyTreeView(leftTreeView, this.leftTreeView);
                CopyTreeView(rightTreeView, this.rightTreeView);

                leftTitleLabel.Text = leftTitle ?? "Left Tree";
                rightTitleLabel.Text = rightTitle ?? "Right Tree";

                browseLeftMenuItem.Visible = false;
                browseRightMenuItem.Visible = false;

                PerformComparison();
            }
        }

        private void SetFormIcon()
        {
            try
            {
                var mainForm = Application.OpenForms.OfType<MainForm>().FirstOrDefault();
                if (mainForm != null && mainForm.Icon != null)
                {
                    this.Icon = mainForm.Icon;
                }
            }
            catch
            {
                // Use default icon if copy fails
            }
        }

        private void InitializeNavigator()
        {
            _navigator = new DifferenceNavigator(leftTreeView, rightTreeView);
            _navigator.Navigated += Navigator_Navigated;
        }

        private void ConfigureTreeViews()
        {
            leftTreeView.HideSelection = false;
            leftTreeView.Font = new Font("Consolas", 9F);
            leftTreeView.ShowLines = true;
            leftTreeView.ShowPlusMinus = true;
            leftTreeView.ShowRootLines = true;
            leftTreeView.FullRowSelect = false;

            rightTreeView.HideSelection = false;
            rightTreeView.Font = new Font("Consolas", 9F);
            rightTreeView.ShowLines = true;
            rightTreeView.ShowPlusMinus = true;
            rightTreeView.ShowRootLines = true;
            rightTreeView.FullRowSelect = false;

            leftTreeView.AfterSelect += TreeView_AfterSelect;
            rightTreeView.AfterSelect += TreeView_AfterSelect;
        }

        private void browseLeftButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _leftFilePath = openFileDialog.FileName;
                leftTitleLabel.Text = string.Format("Left: {0}", Path.GetFileName(_leftFilePath));
                UpdateCompareButtonState();
            }
        }

        private void browseRightButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _rightFilePath = openFileDialog.FileName;
                rightTitleLabel.Text = string.Format("Right: {0}", Path.GetFileName(_rightFilePath));
                UpdateCompareButtonState();
            }
        }

        private void UpdateCompareButtonState()
        {
            bool canCompare = !string.IsNullOrWhiteSpace(_leftFilePath) && 
                            !string.IsNullOrWhiteSpace(_rightFilePath);
            compareButton.Enabled = canCompare;
            compareMenuItem.Enabled = canCompare;
        }

        private void compareButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_leftFilePath) || string.IsNullOrWhiteSpace(_rightFilePath))
            {
                MessageBox.Show("Please select both files to compare.", "Files Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!File.Exists(_leftFilePath))
            {
                MessageBox.Show(string.Format("Left file not found:\n{0}", _leftFilePath), "File Not Found",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(_rightFilePath))
            {
                MessageBox.Show(string.Format("Right file not found:\n{0}", _rightFilePath), "File Not Found",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LoadAndCompareFiles(_leftFilePath, _rightFilePath);
        }

        private void LoadAndCompareFiles(string leftPath, string rightPath)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                compareButton.Enabled = false;
                compareMenuItem.Enabled = false;
                statusLabel.Text = "Loading files...";
                progressBar.Visible = true;
                progressBar.Style = ProgressBarStyle.Marquee;
                Application.DoEvents();

                leftTreeView.Nodes.Clear();
                var leftLines = File.ReadAllLines(leftPath);
                _leftEntries = _parserService.Parse(leftLines);
                var leftCallTree = _parserService.BuildCallTree(_leftEntries);
                PopulateTreeView(leftTreeView, leftCallTree);

                rightTreeView.Nodes.Clear();
                var rightLines = File.ReadAllLines(rightPath);
                _rightEntries = _parserService.Parse(rightLines);
                var rightCallTree = _parserService.BuildCallTree(_rightEntries);
                PopulateTreeView(rightTreeView, rightCallTree);

                PerformComparison();
                UpdatePerformanceDiff();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error loading or comparing files:\n{0}", ex.Message), "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                compareButton.Enabled = true;
                compareMenuItem.Enabled = true;
                progressBar.Visible = false;
            }
        }

        private void PopulateTreeView(TreeView treeView, List<Services.CallStackNode> callTree)
        {
            treeView.BeginUpdate();
            try
            {
                foreach (var node in callTree)
                {
                    var treeNode = CreateTreeNode(node);
                    treeView.Nodes.Add(treeNode);
                }
            }
            finally
            {
                treeView.EndUpdate();
            }
        }

        private TreeNode CreateTreeNode(Services.CallStackNode callNode)
        {
            var treeNode = new TreeNode(callNode.Label) { Tag = callNode };

            foreach (var child in callNode.Children)
            {
                treeNode.Nodes.Add(CreateTreeNode(child));
            }

            return treeNode;
        }

        private void PerformComparison()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                statusLabel.Text = "Comparing trees...";
                Application.DoEvents();

                var leftLogNode = LogNodeFactory.FromTreeView(leftTreeView);
                var rightLogNode = LogNodeFactory.FromTreeView(rightTreeView);

                _differences = _comparer.Compare(leftLogNode, rightLogNode, _compareOptions);

                LogNodeFactory.MapTreeNodesToDifferences(_differences);

                // Colour each tree node to match its difference type — same palette
                // already used in the difference list below, so the two views agree.
                _highlighter.ClearHighlighting(leftTreeView);
                _highlighter.ClearHighlighting(rightTreeView);
                _highlighter.HighlightDifferences(_differences);

                _navigator.Differences = _differences;

                PopulateDifferenceList();
                UpdateStatistics();
                UpdateNavigationButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error during comparison:\n{0}", ex.Message), "Comparison Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void PopulateDifferenceList()
        {
            differenceListView.BeginUpdate();
            try
            {
                differenceListView.Items.Clear();

                for (int i = 0; i < _differences.Count; i++)
                {
                    var diff = _differences[i];
                    diff.Index = i;

                    var item = new ListViewItem(new[] {
                        (i + 1).ToString(),
                        diff.Type.ToString(),
                        diff.Path,
                        diff.OldValue ?? "",
                        diff.NewValue ?? ""
                    });

                    item.BackColor = GetListViewColorForType(diff.Type);
                    item.Tag = diff;
                    differenceListView.Items.Add(item);
                }
            }
            finally
            {
                differenceListView.EndUpdate();
            }
        }

        private Color GetListViewColorForType(DifferenceType type)
        {
            switch (type)
            {
                case DifferenceType.TextMismatch:
                    return Color.FromArgb(255, 255, 200);
                case DifferenceType.ChildCountMismatch:
                    return Color.FromArgb(255, 230, 180);
                case DifferenceType.MissingInRight:
                    return Color.FromArgb(255, 200, 200);
                case DifferenceType.ExtraInRight:
                    return Color.FromArgb(200, 255, 200);
                default:
                    return Color.White;
            }
        }

        private void UpdateStatistics()
        {
            int textMismatchCount = 0;
            int missingInRightCount = 0;
            int extraInRightCount = 0;
            int childCountMismatchCount = 0;

            foreach (var diff in _differences)
            {
                switch (diff.Type)
                {
                    case DifferenceType.TextMismatch:
                        textMismatchCount++;
                        break;
                    case DifferenceType.MissingInRight:
                        missingInRightCount++;
                        break;
                    case DifferenceType.ExtraInRight:
                        extraInRightCount++;
                        break;
                    case DifferenceType.ChildCountMismatch:
                        childCountMismatchCount++;
                        break;
                }
            }

            statusLabel.Text = string.Format(
                "Changed: {0} | Missing: {1} | Added: {2} | Count Diff: {3} | Total: {4}",
                textMismatchCount,
                missingInRightCount,
                extraInRightCount,
                childCountMismatchCount,
                _differences.Count);
        }

        // ── E6: Performance Delta (log comparison / diff) ─────────────────────
        private void UpdatePerformanceDiff()
        {
            if (_leftEntries == null || _rightEntries == null) return;

            string labelA = string.IsNullOrEmpty(_leftFilePath) ? "Left" : Path.GetFileName(_leftFilePath);
            string labelB = string.IsNullOrEmpty(_rightFilePath) ? "Right" : Path.GetFileName(_rightFilePath);
            var result = _diffService.Compare(_leftEntries, labelA, _rightEntries, labelB);

            perfDiffListView.BeginUpdate();
            try
            {
                perfDiffListView.Items.Clear();

                int faster = 0, slower = 0;
                foreach (var entry in result.InBoth)
                {
                    string status = entry.AvgTimeDeltaMs > 0 ? "Slower" : entry.AvgTimeDeltaMs < 0 ? "Faster" : "Unchanged";
                    if (entry.AvgTimeDeltaMs > 0) slower++; else if (entry.AvgTimeDeltaMs < 0) faster++;

                    var item = new ListViewItem(new[]
                    {
                        entry.ApiName,
                        entry.StatsA.AvgDurationMs.ToString(),
                        entry.StatsB.AvgDurationMs.ToString(),
                        (entry.AvgTimeDeltaMs > 0 ? "+" : "") + entry.AvgTimeDeltaMs,
                        status
                    });
                    item.BackColor = entry.AvgTimeDeltaMs > 0 ? Color.FromArgb(255, 220, 220)
                                    : entry.AvgTimeDeltaMs < 0 ? Color.FromArgb(220, 255, 220)
                                    : Color.White;
                    perfDiffListView.Items.Add(item);
                }

                foreach (var entry in result.OnlyInA)
                {
                    var item = new ListViewItem(new[] { entry.ApiName, entry.StatsA.AvgDurationMs.ToString(), "—", "—", "Removed" });
                    item.BackColor = Color.FromArgb(230, 230, 230);
                    perfDiffListView.Items.Add(item);
                }

                foreach (var entry in result.OnlyInB)
                {
                    var item = new ListViewItem(new[] { entry.ApiName, "—", entry.StatsB.AvgDurationMs.ToString(), "—", "New" });
                    item.BackColor = Color.FromArgb(220, 235, 255);
                    perfDiffListView.Items.Add(item);
                }

                perfDiffSummaryLabel.Text = string.Format(
                    "Comparing {0} vs {1}: {2} method(s) slower, {3} faster, {4} new, {5} removed.",
                    labelA, labelB, slower, faster, result.OnlyInB.Count, result.OnlyInA.Count);
            }
            finally
            {
                perfDiffListView.EndUpdate();
            }
        }

        private void UpdateNavigationButtons()
        {
            bool hasDifferences = _differences != null && _differences.Count > 0;
            firstDiffButton.Enabled = hasDifferences;
            prevDiffButton.Enabled = hasDifferences;
            nextDiffButton.Enabled = hasDifferences;
            lastDiffButton.Enabled = hasDifferences;
            firstDiffMenuItem.Enabled = hasDifferences;
            prevDiffMenuItem.Enabled = hasDifferences;
            nextDiffMenuItem.Enabled = hasDifferences;
            lastDiffMenuItem.Enabled = hasDifferences;
        }

        private void firstDiffButton_Click(object sender, EventArgs e)
        {
            _navigator.NavigateFirst();
        }

        private void prevDiffButton_Click(object sender, EventArgs e)
        {
            _navigator.NavigatePrevious();
        }

        private void nextDiffButton_Click(object sender, EventArgs e)
        {
            _navigator.NavigateNext();
        }

        private void lastDiffButton_Click(object sender, EventArgs e)
        {
            _navigator.NavigateLast();
        }

        private void optionsButton_Click(object sender, EventArgs e)
        {
            // Find the MainForm to pass to SettingsForm
            var mainForm = Application.OpenForms.OfType<MainForm>().FirstOrDefault();
            if (mainForm == null)
            {
                // Fallback: use the old CompareOptionsDialog if MainForm is not available
                using (var optionsDialog = new CompareOptionsDialog(_compareOptions))
                {
                    if (optionsDialog.ShowDialog(this) == DialogResult.OK)
                    {
                        _compareOptions = optionsDialog.Options;

                        if (leftTreeView.Nodes.Count > 0 && rightTreeView.Nodes.Count > 0)
                        {
                            PerformComparison();
                        }
                    }
                }
                return;
            }

            // Use the unified SettingsForm with the Comparison tab
            using (var settingsDialog = new SettingsForm(mainForm, SettingsForm.TabIndexComparison))
            {
                if (settingsDialog.ShowDialog(this) == DialogResult.OK)
                {
                    _compareOptions = settingsDialog.CompareOptions;

                    if (leftTreeView.Nodes.Count > 0 && rightTreeView.Nodes.Count > 0)
                    {
                        PerformComparison();
                    }
                }
            }
        }

        private void differenceListView_DoubleClick(object sender, EventArgs e)
        {
            if (differenceListView.SelectedItems.Count > 0)
            {
                var item = differenceListView.SelectedItems[0];
                var diff = item.Tag as TreeDifference;

                if (diff != null)
                {
                    _navigator.NavigateToIndex(diff.Index);
                }
            }
        }

        private void Navigator_Navigated(object sender, DifferenceNavigationEventArgs e)
        {
            string positionInfo = string.Format("Difference {0}/{1}: {2} - {3}",
                e.Index + 1, _differences.Count, e.Difference.Type, e.Difference.Path);
            statusLabel.Text = positionInfo;

            if (e.Index >= 0 && e.Index < differenceListView.Items.Count)
            {
                differenceListView.Items[e.Index].Selected = true;
                differenceListView.Items[e.Index].EnsureVisible();
                differenceListView.Focus();
            }

            // Difference navigation already positions both trees on the matching
            // diff node — rebaseline so the scroll-sync timer doesn't immediately
            // try to "correct" one side using a stale visible-row index.
            _lastLeftTopNode  = leftTreeView.TopNode;
            _lastRightTopNode = rightTreeView.TopNode;
        }

        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
        }

        private void CopyTreeView(TreeView source, TreeView target)
        {
            target.BeginUpdate();
            try
            {
                target.Nodes.Clear();
                foreach (TreeNode node in source.Nodes)
                {
                    target.Nodes.Add(CloneTreeNode(node));
                }
            }
            finally
            {
                target.EndUpdate();
            }
        }

        private TreeNode CloneTreeNode(TreeNode source)
        {
            var clone = new TreeNode(source.Text)
            {
                Tag = source.Tag,
                ImageIndex = source.ImageIndex,
                SelectedImageIndex = source.SelectedImageIndex
            };

            foreach (TreeNode child in source.Nodes)
            {
                clone.Nodes.Add(CloneTreeNode(child));
            }

            return clone;
        }

        private void closeMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void expandAllMenuItem_Click(object sender, EventArgs e)
        {
            leftTreeView.ExpandAll();
            rightTreeView.ExpandAll();
        }

        private void collapseAllMenuItem_Click(object sender, EventArgs e)
        {
            leftTreeView.CollapseAll();
            rightTreeView.CollapseAll();
        }

        private void swapFilesMenuItem_Click(object sender, EventArgs e)
        {
            var tempPath = _leftFilePath;
            _leftFilePath = _rightFilePath;
            _rightFilePath = tempPath;

            var tempLabel = leftTitleLabel.Text;
            leftTitleLabel.Text = rightTitleLabel.Text;
            rightTitleLabel.Text = tempLabel;

            var tempNodes = new List<TreeNode>();
            foreach (TreeNode node in leftTreeView.Nodes)
            {
                tempNodes.Add(CloneTreeNode(node));
            }

            leftTreeView.Nodes.Clear();
            foreach (TreeNode node in rightTreeView.Nodes)
            {
                leftTreeView.Nodes.Add(CloneTreeNode(node));
            }

            rightTreeView.Nodes.Clear();
            foreach (TreeNode node in tempNodes)
            {
                rightTreeView.Nodes.Add(node);
            }

            if (_differences.Count > 0)
            {
                PerformComparison();
            }
        }
    }
}
