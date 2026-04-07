using System;
using System.Drawing;
using System.Windows.Forms;
using Cad3PLogBrowser.Services;

namespace Cad3PLogBrowser
{
    /// <summary>
    /// Application settings dialog — tab visibility, Grok URL,
    /// highlight colour, slow-call threshold, max file size guard.
    /// </summary>
    public partial class SettingsForm : Form
    {
        private readonly MainForm   _mainForm;
        private readonly AppSettings _settings;

        public SettingsForm(MainForm mainForm)
        {
            InitializeComponent();
            _mainForm = mainForm;
            _settings = mainForm.AppSettings;
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            // Tabs
            chkShowLog.Checked         = _mainForm.IsTabVisible(MainForm.TabId.Log);
            chkShowPerformance.Checked = _mainForm.IsTabVisible(MainForm.TabId.Performance);
            chkShowLogDetails.Checked  = _mainForm.IsTabVisible(MainForm.TabId.LogDetails);
            chkShowCallGraph.Checked   = _mainForm.IsTabVisible(MainForm.TabId.CallGraph);

            // Grok
            txtGrokUrl.Text = _settings.GrokUrl;

            // Highlight colour
            cmbHighlightColor.Items.Clear();
            foreach (string name in new[] { "Yellow", "Cyan", "LimeGreen", "Orange",
                                             "HotPink", "LightBlue", "Plum", "Gold" })
                cmbHighlightColor.Items.Add(name);
            cmbHighlightColor.SelectedItem = _settings.HighlightColorName;
            if (cmbHighlightColor.SelectedIndex < 0)
                cmbHighlightColor.SelectedIndex = 0;

            // Thresholds
            nudSlowCallMs.Value  = Math.Max(nudSlowCallMs.Minimum,
                Math.Min(nudSlowCallMs.Maximum, _settings.SlowCallThresholdMs));
            nudMaxFileMb.Value   = Math.Max(nudMaxFileMb.Minimum,
                Math.Min(nudMaxFileMb.Maximum, _settings.MaxFileSizeMbForListView));

            UpdateColourPreview();
        }

        private void cmbHighlightColor_SelectedIndexChanged(object sender, EventArgs e) =>
            UpdateColourPreview();

        private void UpdateColourPreview()
        {
            if (cmbHighlightColor.SelectedItem == null) return;
            try { panelColorPreview.BackColor = Color.FromName(cmbHighlightColor.SelectedItem.ToString()); }
            catch { panelColorPreview.BackColor = Color.Yellow; }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            // Apply tab visibility
            _mainForm.SetTabVisible(MainForm.TabId.Log,         chkShowLog.Checked);
            _mainForm.SetTabVisible(MainForm.TabId.Performance, chkShowPerformance.Checked);
            _mainForm.SetTabVisible(MainForm.TabId.LogDetails,  chkShowLogDetails.Checked);
            _mainForm.SetTabVisible(MainForm.TabId.CallGraph,   chkShowCallGraph.Checked);

            // Save other settings
            _settings.GrokUrl                = txtGrokUrl.Text.Trim();
            _settings.HighlightColorName     = cmbHighlightColor.SelectedItem?.ToString() ?? "Yellow";
            _settings.SlowCallThresholdMs    = (long)nudSlowCallMs.Value;
            _settings.MaxFileSizeMbForListView = (long)nudMaxFileMb.Value;
            _settings.Save();

            Hide();
        }

        private void CancelButton_Click(object sender, EventArgs e) => Hide();
    }
}
