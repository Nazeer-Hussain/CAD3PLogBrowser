using System;
using System.Windows.Forms;
using Cad3PLogBrowser.Services;

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
            ThemeManager.ApplyTheme(this);
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            int? minDuration = chkEnableDuration.Checked ? (int?)nudMinDuration.Value : null;
            DateTime? fromTime = chkEnableTimeRange.Checked ? (DateTime?)dtpFromTime.Value : null;
            DateTime? toTime = chkEnableTimeRange.Checked ? (DateTime?)dtpToTime.Value : null;

            _mainForm.ApplyFilter(FilterTextBox.Text, MatchCaseCheckBox.Checked, minDuration, fromTime, toTime);
            Hide();
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            _mainForm.ApplyFilter(string.Empty, false, null, null, null);
            chkEnableDuration.Checked = false;
            chkEnableTimeRange.Checked = false;
            Hide();
        }

        private void CloseButton_Click(object sender, EventArgs e) => Hide();

        private void FilterForm_Load(object sender, EventArgs e) { }
    }
}
