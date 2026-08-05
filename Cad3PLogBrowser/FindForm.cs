using System;
using System.Windows.Forms;
using Cad3PLogBrowser.Services;

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

        public FindForm(MainForm mainForm)
        {
            InitializeComponent();
            _mainForm = mainForm;
            // NOTE: ThemeManager.ApplyTheme moved to Load event to avoid premature handle creation

            // Load search history from settings
            LoadSearchHistory();

            // B2: validate the regex live as the user types, instead of only discovering
            // it's invalid after Find Next reports a misleading "not found".
            SearchTextBox.TextChanged += (s, e) => ValidateRegexLive();
            UseRegexCheckBox.CheckedChanged += (s, e) => ValidateRegexLive();
        }

        /// <summary>B2: tints the search box red and shows the regex error as a tooltip
        /// when "Use regular expression" is on and the pattern doesn't compile.</summary>
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

        /// <summary>Triggers Find Next using the current search term — called by the menu shortcut.</summary>
        public void TriggerFindNext() => PerformFind(forward: true);

        /// <summary>Triggers Find Previous using the current search term — called by Shift+F3.</summary>
        public void TriggerFindPrev() => PerformFind(forward: false);

        private void FindNextButton_Click(object sender, System.EventArgs e) => PerformFind(forward: true);

        private void PreviousButton_Click(object sender, System.EventArgs e) => PerformFind(forward: false);

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) PerformFind(forward: !e.Shift); // B1: Shift+Enter = Previous
        }

        private void PerformFind(bool forward = true)
        {
            string term = SearchTextBox.Text;

            // B2: an invalid regex isn't "not found" — stop here so the red-tinted box
            // and tooltip (already showing from live validation) aren't masked by that
            // misleading message, and clear any stale match count from a prior search.
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

            int foundIndex = forward
                ? _mainForm.FindNext(term, MatchCaseCheckBox.Checked, UseRegexCheckBox.Checked)
                : _mainForm.FindPrev(term, MatchCaseCheckBox.Checked, UseRegexCheckBox.Checked);

            UpdateMatchCount(term, foundIndex);
        }

        /// <summary>B1: shows "match N of M" (or "No matches") next to the buttons.</summary>
        private void UpdateMatchCount(string term, int foundIndex)
        {
            if (string.IsNullOrEmpty(term))
            {
                MatchCountLabel.Text = string.Empty;
                return;
            }

            int total = _mainForm.CountMatches(term, MatchCaseCheckBox.Checked, UseRegexCheckBox.Checked,
                foundIndex, out int rank);

            MatchCountLabel.Text = total == 0
                ? "No matches"
                : (rank > 0 ? string.Format("Match {0} of {1}", rank, total) : string.Format("{0} matches", total));
        }

        private void CloseButton_Click(object sender, System.EventArgs e) => Hide();

        private void FindForm_Load(object sender, System.EventArgs e)
        {
            // Apply theme now that form and controls are fully created
            ThemeManager.ApplyTheme(this);
        }

        /// <summary>
        /// Loads search history from MainForm settings.
        /// </summary>
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
