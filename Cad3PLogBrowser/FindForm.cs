using System;
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

        private void FindButton_Click(object sender, EventArgs e)
        {
            PerformFind();
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                PerformFind();
        }

        private void PerformFind()
        {
            _owner.FindNext(SearchTextBox.Text, MatchCaseCheckBox.Checked);
        }

        private void Cancelbutton_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void FindFrm_Load(object sender, EventArgs e) { }
    }
}
