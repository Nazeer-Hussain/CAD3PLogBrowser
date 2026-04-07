using System;
using System.Windows.Forms;

namespace Cad3PLogBrowser
{
    /// <summary>
    /// Application settings dialog.
    /// Currently exposes tab visibility toggles.
    /// Extend with additional settings as needed.
    /// </summary>
    public partial class SettingsForm : Form
    {
        private readonly MainForm _mainForm;

        public SettingsForm(MainForm mainForm)
        {
            InitializeComponent();
            _mainForm = mainForm;
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            chkShowLog.Checked         = _mainForm.IsTabVisible(MainForm.TabId.Log);
            chkShowPerformance.Checked = _mainForm.IsTabVisible(MainForm.TabId.Performance);
            chkShowLogDetails.Checked  = _mainForm.IsTabVisible(MainForm.TabId.LogDetails);
            chkShowCallGraph.Checked   = _mainForm.IsTabVisible(MainForm.TabId.CallGraph);
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            _mainForm.SetTabVisible(MainForm.TabId.Log,         chkShowLog.Checked);
            _mainForm.SetTabVisible(MainForm.TabId.Performance, chkShowPerformance.Checked);
            _mainForm.SetTabVisible(MainForm.TabId.LogDetails,  chkShowLogDetails.Checked);
            _mainForm.SetTabVisible(MainForm.TabId.CallGraph,   chkShowCallGraph.Checked);
            Hide();
        }

        private void CancelButton_Click(object sender, EventArgs e) => Hide();
    }
}
