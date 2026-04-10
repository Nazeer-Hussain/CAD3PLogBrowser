using System;
using System.Windows.Forms;
using Cad3PLogBrowser.Services;
using Cad3PLogBrowser.Models;

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
            var criteria = new FilterCriteria
            {
                SearchText = FilterTextBox.Text,
                IsCaseSensitive = MatchCaseCheckBox.Checked,
                MinimumDurationMs = chkEnableDuration.Checked ? (int?)nudMinDuration.Value : null,
                FromTime = chkEnableTimeRange.Checked ? (DateTime?)dtpFromTime.Value : null,
                ToTime = chkEnableTimeRange.Checked ? (DateTime?)dtpToTime.Value : null,
                ThreadId = string.IsNullOrWhiteSpace(txtThreadId.Text) ? null : txtThreadId.Text.Trim(),
                Level = cmbLogLevel.SelectedIndex > 0 ? (LogLevel?)(cmbLogLevel.SelectedIndex - 1) : null
            };

            _mainForm.ApplyFilter(criteria);
            Hide();
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            FilterTextBox.Text = string.Empty;
            MatchCaseCheckBox.Checked = false;
            chkEnableDuration.Checked = false;
            chkEnableTimeRange.Checked = false;
            txtThreadId.Text = string.Empty;
            cmbLogLevel.SelectedIndex = 0;

            _mainForm.ClearFilter();
            Hide();
        }

        private void CloseButton_Click(object sender, EventArgs e) => Hide();

        private void FilterForm_Load(object sender, EventArgs e) { }
    }
}
