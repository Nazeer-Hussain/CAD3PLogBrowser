using System;
using System.Windows.Forms;

namespace Cad3PLogBrowser
{
    public partial class FilterFrm : Form
    {
        private readonly mainFrm _owner;

        public FilterFrm(mainFrm owner)
        {
            InitializeComponent();
            _owner = owner;
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            _owner.ApplyFilter(FilterTextBox.Text, MatchCaseCheckBox.Checked);
            Hide();
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            FilterTextBox.Clear();
            _owner.ApplyFilter(string.Empty, false);
            Hide();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void FilterFrm_Load(object sender, EventArgs e) { }
    }
}
