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
            // NOTE: ThemeManager.ApplyTheme moved to Load event to avoid premature handle creation
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
                ThreadId = string.IsNullOrWhiteSpace(cmbThreadId.Text) ? null : cmbThreadId.Text.Trim(),
                Levels = GetCheckedLevels()
            };

            _mainForm.ApplyFilter(criteria);
            Hide();
        }

        /// <summary>B6: collects whichever level checkboxes are checked. Null (not an
        /// empty set) when none are checked, so FilterCriteria.IsActive treats "no boxes
        /// checked" the same as "no level filter" — i.e. show every level.</summary>
        private System.Collections.Generic.HashSet<LogLevel> GetCheckedLevels()
        {
            var levels = new System.Collections.Generic.HashSet<LogLevel>();
            if (chkLevelDebug.Checked)   levels.Add(LogLevel.Debug);
            if (chkLevelInfo.Checked)    levels.Add(LogLevel.Info);
            if (chkLevelWarning.Checked) levels.Add(LogLevel.Warning);
            if (chkLevelError.Checked)   levels.Add(LogLevel.Error);
            return levels.Count > 0 ? levels : null;
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            FilterTextBox.Text = string.Empty;
            MatchCaseCheckBox.Checked = false;
            chkEnableDuration.Checked = false;
            chkEnableTimeRange.Checked = false;
            cmbThreadId.Text = string.Empty;
            chkLevelDebug.Checked = false;
            chkLevelInfo.Checked = false;
            chkLevelWarning.Checked = false;
            chkLevelError.Checked = false;

            _mainForm.ClearFilter();
            Hide();
        }

        private void CloseButton_Click(object sender, EventArgs e) => Hide();

        private void FilterForm_Load(object sender, EventArgs e)
        {
            // Apply theme now that form and controls are fully created
            ThemeManager.ApplyTheme(this);

            // B7: populate the Thread ID combo with IDs actually detected in the
            // loaded log, and hide it entirely when the log has none (e.g. single-
            // threaded logs) rather than showing an always-empty dropdown.
            var threadIds = _mainForm.GetDetectedThreadIds();
            bool hasThreads = threadIds != null && threadIds.Count > 0;
            cmbThreadId.Items.Clear();
            if (hasThreads)
                cmbThreadId.Items.AddRange(threadIds.ToArray());

            lblThreadId.Visible = hasThreads;
            cmbThreadId.Visible = hasThreads;
        }
    }
}
