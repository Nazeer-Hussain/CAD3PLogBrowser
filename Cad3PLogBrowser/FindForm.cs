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

        public FindForm(MainForm mainForm)
        {
            InitializeComponent();
            _mainForm = mainForm;
            ThemeManager.ApplyTheme(this);

            // Load search history from settings
            LoadSearchHistory();
        }

        /// <summary>Triggers Find Next using the current search term — called by the menu shortcut.</summary>
        public void TriggerFindNext() => PerformFind(forward: true);

        /// <summary>Triggers Find Previous using the current search term — called by Shift+F3.</summary>
        public void TriggerFindPrev() => PerformFind(forward: false);

        private void FindNextButton_Click(object sender, System.EventArgs e) => PerformFind(forward: true);

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) PerformFind(forward: true);
        }

        private void PerformFind(bool forward = true)
        {
            string term = SearchTextBox.Text;
            if (!string.IsNullOrEmpty(term))
            {
                _mainForm.AddSearchHistory(term);

                if (!SearchTextBox.Items.Contains(term))
                    SearchTextBox.Items.Insert(0, term);

                while (SearchTextBox.Items.Count > 20)
                    SearchTextBox.Items.RemoveAt(SearchTextBox.Items.Count - 1);
            }

            if (forward)
                _mainForm.FindNext(term, MatchCaseCheckBox.Checked, UseRegexCheckBox.Checked);
            else
                _mainForm.FindPrev(term, MatchCaseCheckBox.Checked, UseRegexCheckBox.Checked);
        }

        private void CloseButton_Click(object sender, System.EventArgs e) => Hide();

        private void FindForm_Load(object sender, System.EventArgs e) { }

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
