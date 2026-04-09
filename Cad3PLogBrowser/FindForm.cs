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
        }

        /// <summary>Triggers Find Next using the current search term — called by the menu shortcut.</summary>
        public void TriggerFindNext() => PerformFind();

        private void FindNextButton_Click(object sender, System.EventArgs e) => PerformFind();

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) PerformFind();
        }

        private void PerformFind()
        {
            string term = SearchTextBox.Text;
            if (!string.IsNullOrEmpty(term) && !SearchTextBox.Items.Contains(term))
                SearchTextBox.Items.Insert(0, term);

            _mainForm.FindNext(term, MatchCaseCheckBox.Checked, UseRegexCheckBox.Checked);
        }

        private void CloseButton_Click(object sender, System.EventArgs e) => Hide();

        private void FindForm_Load(object sender, System.EventArgs e) { }
    }
}
