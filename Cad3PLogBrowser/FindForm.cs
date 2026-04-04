using System.Windows.Forms;

namespace Cad3PLogBrowser
{
    public partial class FindForm : Form
    {
        private readonly mainFrm _owner;

        public FindForm(mainFrm owner)
        {
            InitializeComponent();
            _owner = owner;
        }

        // Called by the Find Next menu shortcut (Ctrl+F3) when the dialog is open.
        public void TriggerFindNext()
        {
            PerformFind();
        }

        private void FindButton_Click(object sender, System.EventArgs e) =>
            PerformFind();

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) PerformFind();
        }

        private void PerformFind()
        {
            // Add the term to history so the combo box acts as a search history.
            string term = SearchTextBox.Text;
            if (!string.IsNullOrEmpty(term) && !SearchTextBox.Items.Contains(term))
                SearchTextBox.Items.Insert(0, term);

            _owner.FindNext(term, MatchCaseCheckBox.Checked);
        }

        private void Cancelbutton_Click(object sender, System.EventArgs e) => Hide();

        private void FindFrm_Load(object sender, System.EventArgs e) { }
    }
}
