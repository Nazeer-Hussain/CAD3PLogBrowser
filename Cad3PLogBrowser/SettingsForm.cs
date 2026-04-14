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
            ThemeManager.ApplyTheme(this);
        }

        private void LoadCurrentSettings()
        {
            // Tabs
            chkShowLog.Checked         = _mainForm.IsTabVisible(MainForm.TabId.Log);
            chkShowRaw.Checked         = _mainForm.IsTabVisible(MainForm.TabId.Raw);
            chkShowPerformance.Checked = _mainForm.IsTabVisible(MainForm.TabId.Performance);
            chkShowLogDetails.Checked  = _mainForm.IsTabVisible(MainForm.TabId.LogDetails);
            chkShowCallGraph.Checked   = _mainForm.IsTabVisible(MainForm.TabId.CallGraph);
            chkShowFlameGraph.Checked  = _mainForm.IsTabVisible(MainForm.TabId.FlameGraph);
            chkShowTimeline.Checked    = _mainForm.IsTabVisible(MainForm.TabId.Timeline);

            // Grok & AI Integration
            txtGrokUrl.Text = _settings.GrokUrl;
            txtClaudeApiKey.Text      = _settings.ClaudeApiKey;
            if (chkUseClaudeApi != null)
                chkUseClaudeApi.Checked = _settings.UseClaudeApi;

            // Theme
            cmbTheme.Items.Clear();
            cmbTheme.Items.Add("Light");
            cmbTheme.Items.Add("Dark");
            cmbTheme.SelectedItem = _settings.Theme ?? "Light";
            if (cmbTheme.SelectedIndex < 0)
                cmbTheme.SelectedIndex = 0;

            // Icon Size
            cmbIconSize.Items.Clear();
            cmbIconSize.Items.Add("Small");
            cmbIconSize.Items.Add("Medium");
            cmbIconSize.Items.Add("Large");
            cmbIconSize.SelectedItem = _settings.ToolbarIconSize ?? "Medium";
            if (cmbIconSize.SelectedIndex < 0)
                cmbIconSize.SelectedIndex = 1;

            // Toolbar visibility
            chkShowToolbar.Checked = _settings.ShowToolbar;

            // Highlight colour
            cmbHighlightColor.Items.Clear();
            foreach (string name in new[] { "Yellow", "Cyan", "LimeGreen", "Orange",
                                             "HotPink", "LightBlue", "Plum", "Gold" })
                cmbHighlightColor.Items.Add(name);
            cmbHighlightColor.SelectedItem = _settings.HighlightColorName;
            if (cmbHighlightColor.SelectedIndex < 0)
                cmbHighlightColor.SelectedIndex = 0;

            // Font settings
            cmbFontFamily.Items.Clear();
            foreach (var family in new[] { "Consolas", "Courier New", "Lucida Console", "Monaco", "Menlo", "DejaVu Sans Mono", "Source Code Pro" })
                cmbFontFamily.Items.Add(family);
            cmbFontFamily.SelectedItem = _settings.LogFontFamily ?? "Consolas";
            if (cmbFontFamily.SelectedIndex < 0)
                cmbFontFamily.SelectedIndex = 0;

            nudFontSize.Value = (decimal)Math.Max(6.0f, Math.Min(24.0f, _settings.LogFontSize));
            chkFontBold.Checked = (_settings.LogFontStyle & FontStyle.Bold) == FontStyle.Bold;
            chkFontItalic.Checked = (_settings.LogFontStyle & FontStyle.Italic) == FontStyle.Italic;

            // Behavior settings
            cmbInitialView.Items.Clear();
            cmbInitialView.Items.Add("LogView");
            cmbInitialView.Items.Add("ApiView");
            cmbInitialView.SelectedItem = _settings.InitialView ?? "LogView";
            if (cmbInitialView.SelectedIndex < 0)
                cmbInitialView.SelectedIndex = 0;

            txtSnippetSuffix.Text = _settings.SaveSnippetSuffix ?? "_snippet";
            nudMaxRecentFiles.Value = Math.Max(5, Math.Min(20, _settings.MaxRecentFiles));

            // Performance thresholds
            nudSlowCallMs.Value  = Math.Max(nudSlowCallMs.Minimum,
                Math.Min(nudSlowCallMs.Maximum, _settings.SlowCallThresholdMs));
            nudMaxFileMb.Value   = Math.Max(nudMaxFileMb.Minimum,
                Math.Min(nudMaxFileMb.Maximum, _settings.MaxFileSizeMbForListView));

            UpdateColourPreview();
        }

        private void cmbHighlightColor_SelectedIndexChanged(object sender, EventArgs e) =>
            UpdateColourPreview();

        private void btnFontPreview_Click(object sender, EventArgs e)
        {
            try
            {
                FontStyle style = FontStyle.Regular;
                if (chkFontBold.Checked) style |= FontStyle.Bold;
                if (chkFontItalic.Checked) style |= FontStyle.Italic;

                string family = cmbFontFamily.SelectedItem?.ToString() ?? "Consolas";
                float size = (float)nudFontSize.Value;
                Font previewFont = new Font(family, size, style);

                MessageBox.Show(
                    $"Font Preview:\n\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nabcdefghijklmnopqrstuvwxyz\n0123456789\n{{}}[]()<>+-*/=",
                    "Font Preview - " + previewFont.Name,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                previewFont.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot create font preview: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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
            _mainForm.SetTabVisible(MainForm.TabId.Raw,         chkShowRaw.Checked);
            _mainForm.SetTabVisible(MainForm.TabId.Performance, chkShowPerformance.Checked);
            _mainForm.SetTabVisible(MainForm.TabId.LogDetails,  chkShowLogDetails.Checked);
            _mainForm.SetTabVisible(MainForm.TabId.CallGraph,   chkShowCallGraph.Checked);
            _mainForm.SetTabVisible(MainForm.TabId.FlameGraph,  chkShowFlameGraph.Checked);
            _mainForm.SetTabVisible(MainForm.TabId.Timeline,    chkShowTimeline.Checked);

            // Save Grok & AI settings
            _settings.GrokUrl                = txtGrokUrl.Text.Trim();
            _settings.ClaudeApiKey  = txtClaudeApiKey.Text.Trim();
            if (chkUseClaudeApi != null)
                _settings.UseClaudeApi = chkUseClaudeApi.Checked;

            // Save appearance settings
            _settings.Theme                  = cmbTheme.SelectedItem?.ToString() ?? "Light";
            _settings.ToolbarIconSize        = cmbIconSize.SelectedItem?.ToString() ?? "Medium";
            _settings.ShowToolbar            = chkShowToolbar.Checked;
            _settings.HighlightColorName     = cmbHighlightColor.SelectedItem?.ToString() ?? "Yellow";

            // Save font settings
            _settings.LogFontFamily          = cmbFontFamily.SelectedItem?.ToString() ?? "Consolas";
            _settings.LogFontSize            = (float)nudFontSize.Value;
            FontStyle fontStyle = FontStyle.Regular;
            if (chkFontBold.Checked) fontStyle |= FontStyle.Bold;
            if (chkFontItalic.Checked) fontStyle |= FontStyle.Italic;
            _settings.LogFontStyle           = fontStyle;

            // Save behavior settings
            _settings.InitialView            = cmbInitialView.SelectedItem?.ToString() ?? "LogView";
            _settings.SaveSnippetSuffix      = txtSnippetSuffix.Text.Trim();
            _settings.MaxRecentFiles         = (int)nudMaxRecentFiles.Value;

            // Save performance settings
            _settings.SlowCallThresholdMs    = (long)nudSlowCallMs.Value;
            _settings.MaxFileSizeMbForListView = (long)nudMaxFileMb.Value;

            _settings.Save();

            // Apply theme immediately
            _mainForm.ApplyTheme();

            // Apply toolbar visibility
            _mainForm.ApplyToolbarVisibility();

            // Apply font settings
            _mainForm.ApplyFontSettings();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
