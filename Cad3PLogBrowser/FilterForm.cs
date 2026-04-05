using System;
using System.Windows.Forms;

namespace Cad3PLogBrowser
{
    /// <summary>
    /// Dialog for filtering the log view to lines containing a search term.
    /// Applies the filter directly to the owning <see cref="MainForm"/>.
    /// </summary>
    public partial class FilterForm : Form
    {
        private readonly MainForm _mainForm;

        public FilterForm(MainForm mainForm)
        {
            InitializeComponent();
            _mainForm = mainForm;
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            _mainForm.ApplyFilter(FilterTextBox.Text, MatchCaseCheckBox.Checked);
            Hide();
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            FilterTextBox.Clear();
            _mainForm.ApplyFilter(string.Empty, false);
            Hide();
        }

        private void CloseButton_Click(object sender, EventArgs e) => Hide();

        private void FilterForm_Load(object sender, EventArgs e) { }
    }
}
