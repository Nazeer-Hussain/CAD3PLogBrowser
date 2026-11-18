using System;
using System.Reflection;
using System.Windows.Forms;
using Cad3PLogBrowser.Properties;
using Cad3PLogBrowser.Services;
using Cad3PLogBrowser.UI;

namespace Cad3PLogBrowser
{
    /// <summary>
    /// Modeless find dialog. Delegates all search logic to <see cref="MainForm.FindNext"/>.
    /// Maintains a history of recent search terms in the combo box.
    /// </summary>
    public partial class FindForm : Form
    {
        private readonly MainForm _mainForm;
        private readonly ToolTip _regexErrorToolTip = new ToolTip();

        private enum FindTargetType
        {
            Log,
            Performance,
            CallGraph,
            FlameGraph,
            Timeline,
            Heatmap,
            Ai
        }

        private sealed class FindTarget
        {
            public FindTargetType Type { get; set; }
            public object Instance { get; set; }
        }

        public FindForm(MainForm mainForm)
        {
            InitializeComponent();
            Icon = AppIcon.Get();
            _mainForm = mainForm;

            LoadSearchHistory();
            SearchTextBox.TextChanged += (s, e) => ValidateRegexLive();
            UseRegexCheckBox.CheckedChanged += (s, e) => ValidateRegexLive();
        }

        private bool ValidateRegexLive()
        {
            if (UseRegexCheckBox.Checked &&
                !_mainForm.TryValidateRegex(SearchTextBox.Text, MatchCaseCheckBox.Checked, out string error))
            {
                SearchTextBox.BackColor = ThemeManager.ErrorBackgroundColor;
                _regexErrorToolTip.SetToolTip(SearchTextBox, "Invalid regular expression: " + error);
                return false;
            }

            SearchTextBox.BackColor = ThemeManager.InputBackgroundColor;
            _regexErrorToolTip.SetToolTip(SearchTextBox, string.Empty);
            return true;
        }

        private static object GetField(object owner, string fieldName)
        {
            if (owner == null) return null;
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            return owner.GetType().GetField(fieldName, flags)?.GetValue(owner);
        }

        private bool TryGetActiveFindTarget(out FindTarget target)
        {
            target = new FindTarget { Type = FindTargetType.Log, Instance = null };

            var tabControl = GetField(_mainForm, "mainTabControl") as TabControl;
            if (tabControl == null || tabControl.SelectedTab == null)
                return true;

            var selected = tabControl.SelectedTab;

            var performanceTab = GetField(_mainForm, "performanceTab") as TabPage;
            if (selected == performanceTab)
            {
                target.Type = FindTargetType.Performance;
                target.Instance = GetField(_mainForm, "performanceView") as ListView;
                return target.Instance != null;
            }

            var callGraphTab = GetField(_mainForm, "callGraphTab") as TabPage;
            if (selected == callGraphTab)
            {
                target.Type = FindTargetType.CallGraph;
                target.Instance = GetField(_mainForm, "callGraphPanel");
                return target.Instance != null;
            }

            var flameGraphTab = GetField(_mainForm, "flameGraphTab") as TabPage;
            if (selected == flameGraphTab)
            {
                target.Type = FindTargetType.FlameGraph;
                target.Instance = GetField(_mainForm, "flameGraphPanel");
                return target.Instance != null;
            }

            var timelineTab = GetField(_mainForm, "timelineTab") as TabPage;
            if (selected == timelineTab)
            {
                target.Type = FindTargetType.Timeline;
                target.Instance = GetField(_mainForm, "timelinePanel");
                return target.Instance != null;
            }

            var heatmapTab = GetField(_mainForm, "heatmapTab") as TabPage;
            if (selected == heatmapTab)
            {
                target.Type = FindTargetType.Heatmap;
                target.Instance = GetField(_mainForm, "heatmapPanel");
                return target.Instance != null;
            }

            var aiTab = GetField(_mainForm, "_aiTab") as TabPage;
            if (selected == aiTab)
            {
                target.Type = FindTargetType.Ai;
                target.Instance = GetField(_mainForm, "_aiPanel");
                return target.Instance != null;
            }

            target.Type = FindTargetType.Log;
            target.Instance = null;
            return true;
        }

        private static bool IsPerformanceMatch(ListViewItem item, string term, bool useRegex,
            System.Text.RegularExpressions.Regex rx, StringComparison comparison)
        {
            if (item == null || item.Tag == null) return false;

            for (int i = 0; i < item.SubItems.Count; i++)
            {
                string text = item.SubItems[i].Text ?? string.Empty;
                if (useRegex ? rx.IsMatch(text) : text.IndexOf(term, comparison) >= 0)
                    return true;
            }

            return false;
        }

        private int FindInPerformance(ListView performanceView, string term, bool matchCase, bool useRegex, bool forward)
        {
            if (performanceView.Items.Count == 0 || string.IsNullOrEmpty(term)) return -1;

            try
            {
                var comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                var rx = useRegex
                    ? new System.Text.RegularExpressions.Regex(
                        term,
                        matchCase ? System.Text.RegularExpressions.RegexOptions.None : System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                    : null;

                int count = performanceView.Items.Count;
                int start = performanceView.SelectedIndices.Count > 0
                    ? performanceView.SelectedIndices[0]
                    : (forward ? -1 : count);

                for (int step = 1; step <= count; step++)
                {
                    int idx = forward
                        ? (start + step + count) % count
                        : (start - step + count) % count;

                    if (!IsPerformanceMatch(performanceView.Items[idx], term, useRegex, rx, comparison))
                        continue;

                    performanceView.SelectedIndices.Clear();
                    performanceView.SelectedIndices.Add(idx);
                    performanceView.Items[idx].EnsureVisible();
                    performanceView.Focus();
                    return idx;
                }
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(string.Format(Resources.ERR_INVALID_REGEX, ex.Message),
                    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return -1;
            }

            MessageBox.Show(string.Format(Resources.ERR_NOT_FOUND, term),
                Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return -1;
        }

        private int CountPerformanceMatches(ListView performanceView, string term, bool matchCase,
            bool useRegex, int currentIndex, out int rank)
        {
            rank = 0;
            if (performanceView.Items.Count == 0 || string.IsNullOrEmpty(term)) return 0;

            try
            {
                var comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                var rx = useRegex
                    ? new System.Text.RegularExpressions.Regex(
                        term,
                        matchCase ? System.Text.RegularExpressions.RegexOptions.None : System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                    : null;

                int total = 0;
                for (int i = 0; i < performanceView.Items.Count; i++)
                {
                    if (!IsPerformanceMatch(performanceView.Items[i], term, useRegex, rx, comparison))
                        continue;

                    total++;
                    if (i == currentIndex) rank = total;
                }

                return total;
            }
            catch
            {
                return 0;
            }
        }

        private static int InvokeFind(object target, string methodName,
            string term, bool matchCase, bool useRegex, bool forward)
        {
            if (target == null) return -1;

            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
            if (method == null) return -1;

            var result = method.Invoke(target, new object[] { term, matchCase, useRegex, forward });
            return result is int idx ? idx : -1;
        }

        private static int InvokeCount(object target, string methodName,
            string term, bool matchCase, bool useRegex, int currentIndex, out int rank)
        {
            rank = 0;
            if (target == null) return 0;

            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
            if (method == null) return 0;

            object[] args = { term, matchCase, useRegex, currentIndex, 0 };
            var result = method.Invoke(target, args);
            rank = args[4] is int r ? r : 0;
            return result is int total ? total : 0;
        }

        /// <summary>Triggers Find Next using the current search term — called by the menu shortcut.</summary>
        public void TriggerFindNext() => PerformFind(forward: true);

        /// <summary>Triggers Find Previous using the current search term — called by Shift+F3.</summary>
        public void TriggerFindPrev() => PerformFind(forward: false);

        private void FindNextButton_Click(object sender, EventArgs e) => PerformFind(forward: true);

        private void PreviousButton_Click(object sender, EventArgs e) => PerformFind(forward: false);

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) PerformFind(forward: !e.Shift);
        }

        private void PerformFind(bool forward = true)
        {
            string term = SearchTextBox.Text;

            if (!ValidateRegexLive())
            {
                MatchCountLabel.Text = string.Empty;
                return;
            }

            if (!string.IsNullOrEmpty(term))
            {
                _mainForm.AddSearchHistory(term);

                if (!SearchTextBox.Items.Contains(term))
                    SearchTextBox.Items.Insert(0, term);

                while (SearchTextBox.Items.Count > 20)
                    SearchTextBox.Items.RemoveAt(SearchTextBox.Items.Count - 1);
            }

            TryGetActiveFindTarget(out var target);

            int foundIndex = -1;
            bool handledNotFoundMessage = false;

            switch (target.Type)
            {
                case FindTargetType.Performance:
                    foundIndex = FindInPerformance((ListView)target.Instance, term, MatchCaseCheckBox.Checked, UseRegexCheckBox.Checked, forward);
                    handledNotFoundMessage = true;
                    break;

                case FindTargetType.CallGraph:
                    foundIndex = InvokeFind(target.Instance, "FindNode", term, MatchCaseCheckBox.Checked, UseRegexCheckBox.Checked, forward);
                    break;

                case FindTargetType.FlameGraph:
                    foundIndex = InvokeFind(target.Instance, "FindFunction", term, MatchCaseCheckBox.Checked, UseRegexCheckBox.Checked, forward);
                    break;

                case FindTargetType.Timeline:
                    foundIndex = InvokeFind(target.Instance, "FindEntry", term, MatchCaseCheckBox.Checked, UseRegexCheckBox.Checked, forward);
                    break;

                case FindTargetType.Heatmap:
                    foundIndex = InvokeFind(target.Instance, "FindApi", term, MatchCaseCheckBox.Checked, UseRegexCheckBox.Checked, forward);
                    break;

                case FindTargetType.Ai:
                    foundIndex = InvokeFind(target.Instance, "FindInConversation", term, MatchCaseCheckBox.Checked, UseRegexCheckBox.Checked, forward);
                    break;

                default:
                    foundIndex = forward
                        ? _mainForm.FindNext(term, MatchCaseCheckBox.Checked, UseRegexCheckBox.Checked)
                        : _mainForm.FindPrev(term, MatchCaseCheckBox.Checked, UseRegexCheckBox.Checked);
                    handledNotFoundMessage = true;
                    break;
            }

            if (!handledNotFoundMessage && !string.IsNullOrEmpty(term) && foundIndex < 0)
            {
                MessageBox.Show(string.Format(Resources.ERR_NOT_FOUND, term),
                    Resources.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            UpdateMatchCount(term, foundIndex, target);
        }

        private void UpdateMatchCount(string term, int foundIndex, FindTarget target)
        {
            if (string.IsNullOrEmpty(term))
            {
                MatchCountLabel.Text = string.Empty;
                return;
            }

            int total;
            int rank;

            switch (target.Type)
            {
                case FindTargetType.Performance:
                    total = CountPerformanceMatches((ListView)target.Instance, term, MatchCaseCheckBox.Checked,
                        UseRegexCheckBox.Checked, foundIndex, out rank);
                    break;

                case FindTargetType.CallGraph:
                    total = InvokeCount(target.Instance, "CountNodeMatches", term, MatchCaseCheckBox.Checked,
                        UseRegexCheckBox.Checked, foundIndex, out rank);
                    break;

                case FindTargetType.FlameGraph:
                    total = InvokeCount(target.Instance, "CountFunctionMatches", term, MatchCaseCheckBox.Checked,
                        UseRegexCheckBox.Checked, foundIndex, out rank);
                    break;

                case FindTargetType.Timeline:
                    total = InvokeCount(target.Instance, "CountEntryMatches", term, MatchCaseCheckBox.Checked,
                        UseRegexCheckBox.Checked, foundIndex, out rank);
                    break;

                case FindTargetType.Heatmap:
                    total = InvokeCount(target.Instance, "CountApiMatches", term, MatchCaseCheckBox.Checked,
                        UseRegexCheckBox.Checked, foundIndex, out rank);
                    break;

                case FindTargetType.Ai:
                    total = InvokeCount(target.Instance, "CountConversationMatches", term, MatchCaseCheckBox.Checked,
                        UseRegexCheckBox.Checked, foundIndex, out rank);
                    break;

                default:
                    total = _mainForm.CountMatches(term, MatchCaseCheckBox.Checked, UseRegexCheckBox.Checked,
                        foundIndex, out rank);
                    break;
            }

            MatchCountLabel.Text = total == 0
                ? "No matches"
                : (rank > 0 ? string.Format("Match {0} of {1}", rank, total) : string.Format("{0} matches", total));
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            _mainForm.ClearHighlighting();
            Hide();
        }

        private void FindForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _mainForm.ClearHighlighting();
        }

        private void FindForm_Load(object sender, EventArgs e)
        {
            ThemeManager.ApplyTheme(this);
        }

        private void LoadSearchHistory()
        {
            try
            {
                var history = _mainForm.GetSearchHistory();
                if (history != null && history.Count > 0)
                {
                    SearchTextBox.Items.Clear();
                    foreach (var term in history)
                    {
                        if (!string.IsNullOrWhiteSpace(term))
                            SearchTextBox.Items.Add(term);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Failed to load search history: {0}", ex.Message));
            }
        }
    }
}
