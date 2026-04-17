using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Cad3PLogBrowser.Services;

namespace Cad3PLogBrowser
{
    /// <summary>
    /// Application settings dialog — TabControl layout with six organised pages.
    /// Every AppSettings property has a corresponding control here, every control
    /// has a default, and all values are saved then restored on next startup.
    /// </summary>
    public partial class SettingsForm : Form
    {
        private readonly MainForm    _mainForm;
        private readonly AppSettings _settings;

        // ── Appearance ────────────────────────────────────────────────────────
        private ComboBox  cmbTheme, cmbIconSize, cmbHighlightColor;
        private CheckBox  chkShowToolbar;
        private Panel     panelColorPreview;

        // ── Tabs & Layout ─────────────────────────────────────────────────────
        private CheckBox  chkShowLog, chkShowRaw, chkShowPerformance, chkShowLogDetails;
        private CheckBox  chkShowCallGraph, chkShowFlameGraph, chkShowTimeline;
        private ComboBox  cmbInitialView;
        private ComboBox  cmbDefaultTreeView;

        // ── Log Font ──────────────────────────────────────────────────────────
        private ComboBox       cmbFontFamily;
        private NumericUpDown  nudFontSize;
        private CheckBox       chkFontBold, chkFontItalic;

        // ── Files & Behavior ──────────────────────────────────────────────────
        private TextBox        txtInitialDir;
        private NumericUpDown  nudMaxRecentFiles;
        private TextBox        txtSnippetSuffix;

        // ── Performance ───────────────────────────────────────────────────────
        private NumericUpDown  nudSlowCallMs, nudMaxFileMb;

        // ── Integration ───────────────────────────────────────────────────────
        private TextBox   txtGrokUrl, txtClaudeApiKey;
        private CheckBox  chkUseClaudeApi;

        // ── Buttons ───────────────────────────────────────────────────────────
        private Button OkButton, CancelBtn, btnResetDefaults;

        // ─────────────────────────────────────────────────────────────────────
        public SettingsForm(MainForm mainForm)
        {
            _mainForm = mainForm;
            _settings = mainForm.AppSettings;
            BuildUi();
            LoadCurrentSettings();
            ThemeManager.ApplyTheme(this);
        }

        // ── UI Construction ───────────────────────────────────────────────────
        private void BuildUi()
        {
            Text             = "Settings";
            ClientSize       = new Size(556, 450);
            FormBorderStyle  = FormBorderStyle.FixedDialog;
            MaximizeBox      = false;
            MinimizeBox      = false;
            ShowInTaskbar    = false;
            StartPosition    = FormStartPosition.CenterParent;
            AutoScaleMode    = AutoScaleMode.Font;
            AutoScaleDimensions = new SizeF(8f, 16f);

            var tabs = new TabControl
            {
                Location = new Point(12, 10),
                Size     = new Size(532, 390),
            };

            tabs.TabPages.Add(BuildAppearanceTab());
            tabs.TabPages.Add(BuildTabsLayoutTab());
            tabs.TabPages.Add(BuildFontTab());
            tabs.TabPages.Add(BuildFilesTab());
            tabs.TabPages.Add(BuildPerformanceTab());
            tabs.TabPages.Add(BuildIntegrationTab());

            // Bottom buttons
            btnResetDefaults = Btn("Reset to Defaults", 12,   414, 140, 28);
            OkButton         = Btn("&OK",               338,  414,  90, 28);
            CancelBtn        = Btn("&Cancel",            438,  414,  90, 28);

            OkButton.DialogResult     = DialogResult.OK;
            CancelBtn.DialogResult    = DialogResult.Cancel;
            OkButton.Click           += (s, e) => OkButton_Click();
            btnResetDefaults.Click   += (s, e) => ResetToDefaults();

            AcceptButton = OkButton;
            CancelButton = CancelBtn;

            Controls.Add(tabs);
            Controls.Add(btnResetDefaults);
            Controls.Add(OkButton);
            Controls.Add(CancelBtn);
        }

        // ── TAB: Appearance ───────────────────────────────────────────────────
        private TabPage BuildAppearanceTab()
        {
            var tp = Tab("Appearance");

            cmbTheme = AddRow(tp, "Theme:", 22, out _);
            cmbTheme.Items.AddRange(new object[] { "Light", "Dark" });

            cmbIconSize = AddRow(tp, "Toolbar icon size:", 58, out _);
            cmbIconSize.Items.AddRange(new object[] { "Small", "Medium", "Large" });

            // Show toolbar on its own row, aligned with the control column
            chkShowToolbar = new CheckBox
            {
                AutoSize = true, Text = "Show toolbar", Checked = true,
                Location = new Point(175, 94)
            };
            tp.Controls.Add(chkShowToolbar);
            Lbl(tp, "Toolbar visible:", 12, 97);

            cmbHighlightColor = AddRow(tp, "Highlight colour:", 130, out _);
            foreach (string n in new[] { "Yellow","Cyan","LimeGreen","Orange","HotPink","LightBlue","Plum","Gold" })
                cmbHighlightColor.Items.Add(n);
            cmbHighlightColor.SelectedIndexChanged += (s, e) => UpdateColourPreview();

            panelColorPreview = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location    = new Point(370, 127),
                Size        = new Size(52, 24)
            };
            tp.Controls.Add(panelColorPreview);

            return tp;
        }

        // ── TAB: Tabs & Layout ────────────────────────────────────────────────
        private TabPage BuildTabsLayoutTab()
        {
            var tp = Tab("Tabs & Layout");

            var grp = new GroupBox { Text = "Visible Tabs",
                Location = new Point(12, 10), Size = new Size(498, 100), TabStop = false };

            chkShowLog         = Chk(grp, "Log View",     14, 24);
            chkShowRaw         = Chk(grp, "Raw",         120, 24);
            chkShowPerformance = Chk(grp, "Performance", 200, 24);
            chkShowLogDetails  = Chk(grp, "Log Details", 330, 24);
            chkShowCallGraph   = Chk(grp, "Call Graph",   14, 58);
            chkShowFlameGraph  = Chk(grp, "Flame Graph", 120, 58);
            chkShowTimeline    = Chk(grp, "Timeline",    250, 58);
            tp.Controls.Add(grp);

            cmbInitialView = AddRow(tp, "Start-up tab:", 126, out _);
            cmbInitialView.Items.AddRange(new object[]
            {
                "Log", "Raw", "Performance", "Log Details",
                "Call Graph", "Flame Graph", "Timeline"
            });

            cmbDefaultTreeView = AddRow(tp, "Default tree view:", 162, out _);
            cmbDefaultTreeView.Items.AddRange(new object[] { "Call Tree", "API Tree" });
            cmbDefaultTreeView.Size = new Size(130, 24);

            return tp;
        }

        // ── TAB: Log Font ─────────────────────────────────────────────────────
        private TabPage BuildFontTab()
        {
            var tp = Tab("Log Font");

            cmbFontFamily = AddRow(tp, "Font family:", 22, out _);
            foreach (var f in new[] { "Consolas","Courier New","Lucida Console","DejaVu Sans Mono","Source Code Pro" })
                cmbFontFamily.Items.Add(f);

            nudFontSize = new NumericUpDown { Location = new Point(175, 55), Size = new Size(72, 23),
                DecimalPlaces = 1, Increment = 0.5m, Minimum = 6, Maximum = 24, Value = 9 };
            Lbl(tp, "Font size (pt):", 12, 58);
            tp.Controls.Add(nudFontSize);

            chkFontBold   = new CheckBox { AutoSize = true, Text = "Bold",   Location = new Point(175, 92) };
            chkFontItalic = new CheckBox { AutoSize = true, Text = "Italic", Location = new Point(255, 92) };
            tp.Controls.Add(chkFontBold);
            tp.Controls.Add(chkFontItalic);

            var btnPrev = Btn("Preview Font...", 175, 126, 160, 28);
            btnPrev.Click += (s, e) => PreviewFont();
            tp.Controls.Add(btnPrev);

            return tp;
        }

        // ── TAB: Files & Behavior ─────────────────────────────────────────────
        private TabPage BuildFilesTab()
        {
            var tp = Tab("Files & Behavior");

            Lbl(tp, "Default open folder:", 12, 26);
            txtInitialDir = new TextBox { Location = new Point(175, 22), Size = new Size(248, 23) };
            tp.Controls.Add(txtInitialDir);

            var btnBrowse = Btn("Browse…", 432, 21, 68, 25);
            btnBrowse.Click += (s, e) => BrowseFolder();
            tp.Controls.Add(btnBrowse);

            nudMaxRecentFiles = AddNud(tp, "Max recent files:", 58, 5, 20, 10);
            txtSnippetSuffix  = AddTxt(tp, "Snippet file suffix:", 94, "_snippet", 160);

            return tp;
        }

        // ── TAB: Performance ──────────────────────────────────────────────────
        private TabPage BuildPerformanceTab()
        {
            var tp = Tab("Performance");

            nudSlowCallMs = AddNud(tp, "Slow call threshold:", 22, 10, 60000, 1000);
            Lbl(tp, "ms   (highlights slow calls in the tree)", 290, 26);

            nudMaxFileMb = AddNud(tp, "Skip list view if file >", 58, 1, 2000, 50);
            Lbl(tp, "MB   (use Raw tab for very large files)", 290, 62);

            return tp;
        }

        // ── TAB: Integration ──────────────────────────────────────────────────
        private TabPage BuildIntegrationTab()
        {
            var tp = Tab("Integration");

            Lbl(tp, "Grok URL:", 12, 26);
            txtGrokUrl = new TextBox { Location = new Point(175, 22), Size = new Size(336, 23) };
            tp.Controls.Add(txtGrokUrl);

            Lbl(tp, "Claude API Key:", 12, 62);
            txtClaudeApiKey = new TextBox
            {
                Location = new Point(175, 58), Size = new Size(336, 23),
                UseSystemPasswordChar = true
            };
            tp.Controls.Add(txtClaudeApiKey);

            chkUseClaudeApi = new CheckBox
            {
                AutoSize = true, Location = new Point(175, 92),
                Text = "Enable Claude AI integration (requires key above)"
            };
            tp.Controls.Add(chkUseClaudeApi);

            return tp;
        }

        // ── Load / Save ───────────────────────────────────────────────────────
        private void LoadCurrentSettings()
        {
            // Appearance
            cmbTheme.SelectedItem     = _settings.Theme ?? "Light";
            if (cmbTheme.SelectedIndex < 0) cmbTheme.SelectedIndex = 0;
            cmbIconSize.SelectedItem  = _settings.ToolbarIconSize ?? "Medium";
            if (cmbIconSize.SelectedIndex < 0) cmbIconSize.SelectedIndex = 1;
            chkShowToolbar.Checked    = _settings.ShowToolbar;
            cmbHighlightColor.SelectedItem = _settings.HighlightColorName ?? "Yellow";
            if (cmbHighlightColor.SelectedIndex < 0) cmbHighlightColor.SelectedIndex = 0;
            UpdateColourPreview();

            // Tabs & Layout
            chkShowLog.Checked         = _mainForm.IsTabVisible(MainForm.TabId.Log);
            chkShowRaw.Checked         = _mainForm.IsTabVisible(MainForm.TabId.Raw);
            chkShowPerformance.Checked = _mainForm.IsTabVisible(MainForm.TabId.Performance);
            chkShowLogDetails.Checked  = _mainForm.IsTabVisible(MainForm.TabId.LogDetails);
            chkShowCallGraph.Checked   = _mainForm.IsTabVisible(MainForm.TabId.CallGraph);
            chkShowFlameGraph.Checked  = _mainForm.IsTabVisible(MainForm.TabId.FlameGraph);
            chkShowTimeline.Checked    = _mainForm.IsTabVisible(MainForm.TabId.Timeline);
            cmbInitialView.SelectedItem = _settings.InitialView ?? "Log";
            if (cmbInitialView.SelectedIndex < 0) cmbInitialView.SelectedIndex = 0;
            cmbDefaultTreeView.SelectedItem = _settings.DefaultTreeView == "Api" ? "API Tree" : "Call Tree";
            if (cmbDefaultTreeView.SelectedIndex < 0) cmbDefaultTreeView.SelectedIndex = 0;

            // Log Font
            cmbFontFamily.SelectedItem = _settings.LogFontFamily ?? "Consolas";
            if (cmbFontFamily.SelectedIndex < 0) cmbFontFamily.SelectedIndex = 0;
            nudFontSize.Value  = (decimal)Math.Max(6f, Math.Min(24f, _settings.LogFontSize));
            chkFontBold.Checked   = (_settings.LogFontStyle & FontStyle.Bold)   != 0;
            chkFontItalic.Checked = (_settings.LogFontStyle & FontStyle.Italic) != 0;

            // Files & Behavior
            txtInitialDir.Text         = _settings.InitialDirectory ?? "";
            nudMaxRecentFiles.Value    = Math.Max(5, Math.Min(20, _settings.MaxRecentFiles));
            txtSnippetSuffix.Text      = _settings.SaveSnippetSuffix ?? "_snippet";

            // Performance
            nudSlowCallMs.Value = Math.Max(nudSlowCallMs.Minimum,
                Math.Min(nudSlowCallMs.Maximum, _settings.SlowCallThresholdMs));
            nudMaxFileMb.Value  = Math.Max(nudMaxFileMb.Minimum,
                Math.Min(nudMaxFileMb.Maximum, _settings.MaxFileSizeMbForListView));

            // Integration
            txtGrokUrl.Text         = _settings.GrokUrl ?? "";
            txtClaudeApiKey.Text    = _settings.ClaudeApiKey ?? "";
            chkUseClaudeApi.Checked = _settings.UseClaudeApi;
        }

        private void OkButton_Click()
        {
            // Appearance
            _settings.Theme            = cmbTheme.SelectedItem?.ToString() ?? "Light";
            _settings.ToolbarIconSize  = cmbIconSize.SelectedItem?.ToString() ?? "Medium";
            _settings.ShowToolbar      = chkShowToolbar.Checked;
            _settings.HighlightColorName = cmbHighlightColor.SelectedItem?.ToString() ?? "Yellow";

            // Tabs & Layout
            _settings.ShowLogTab         = chkShowLog.Checked;
            _settings.ShowRawTab         = chkShowRaw.Checked;
            _settings.ShowPerformanceTab = chkShowPerformance.Checked;
            _settings.ShowLogDetailsTab  = chkShowLogDetails.Checked;
            _settings.ShowCallGraphTab   = chkShowCallGraph.Checked;
            _settings.ShowFlameGraphTab  = chkShowFlameGraph.Checked;
            _settings.ShowTimelineTab    = chkShowTimeline.Checked;
            _settings.InitialView        = cmbInitialView.SelectedItem?.ToString() ?? "Log";
            _settings.DefaultTreeView    = cmbDefaultTreeView.SelectedItem?.ToString() == "API Tree" ? "Api" : "Call";

            // Log Font
            _settings.LogFontFamily = cmbFontFamily.SelectedItem?.ToString() ?? "Consolas";
            _settings.LogFontSize   = (float)nudFontSize.Value;
            var style = FontStyle.Regular;
            if (chkFontBold.Checked)   style |= FontStyle.Bold;
            if (chkFontItalic.Checked) style |= FontStyle.Italic;
            _settings.LogFontStyle  = style;

            // Files & Behavior
            _settings.InitialDirectory  = txtInitialDir.Text.Trim();
            _settings.MaxRecentFiles    = (int)nudMaxRecentFiles.Value;
            _settings.SaveSnippetSuffix = txtSnippetSuffix.Text.Trim();

            // Performance
            _settings.SlowCallThresholdMs      = (long)nudSlowCallMs.Value;
            _settings.MaxFileSizeMbForListView  = (long)nudMaxFileMb.Value;

            // Integration
            _settings.GrokUrl      = txtGrokUrl.Text.Trim();
            _settings.ClaudeApiKey = txtClaudeApiKey.Text.Trim();
            _settings.UseClaudeApi = chkUseClaudeApi.Checked;

            _settings.Save();
        }

        private void ResetToDefaults()
        {
            if (MessageBox.Show(
                    "Reset all settings to their default values?",
                    "Reset to Defaults",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes) return;

            var def = new AppSettings();
            // Temporarily replace so LoadCurrentSettings picks them up
            _settings.Theme               = def.Theme;
            _settings.ToolbarIconSize     = def.ToolbarIconSize;
            _settings.ShowToolbar         = def.ShowToolbar;
            _settings.HighlightColorName  = def.HighlightColorName;
            _settings.ShowLogTab          = def.ShowLogTab;
            _settings.ShowRawTab          = def.ShowRawTab;
            _settings.ShowPerformanceTab  = def.ShowPerformanceTab;
            _settings.ShowLogDetailsTab   = def.ShowLogDetailsTab;
            _settings.ShowCallGraphTab    = def.ShowCallGraphTab;
            _settings.ShowFlameGraphTab   = def.ShowFlameGraphTab;
            _settings.ShowTimelineTab     = def.ShowTimelineTab;
            _settings.InitialView         = def.InitialView;
            _settings.DefaultTreeView     = def.DefaultTreeView;
            _settings.LogFontFamily       = def.LogFontFamily;
            _settings.LogFontSize         = def.LogFontSize;
            _settings.LogFontStyle        = def.LogFontStyle;
            _settings.InitialDirectory    = def.InitialDirectory;
            _settings.MaxRecentFiles      = def.MaxRecentFiles;
            _settings.SaveSnippetSuffix   = def.SaveSnippetSuffix;
            _settings.SlowCallThresholdMs       = def.SlowCallThresholdMs;
            _settings.MaxFileSizeMbForListView  = def.MaxFileSizeMbForListView;
            _settings.GrokUrl             = def.GrokUrl;
            // Note: API key and UseClaudeApi are NOT reset (security/convenience)
            LoadCurrentSettings();
        }

        // ── Event handlers ────────────────────────────────────────────────────
        private void UpdateColourPreview()
        {
            if (cmbHighlightColor.SelectedItem == null) return;
            try { panelColorPreview.BackColor = Color.FromName(cmbHighlightColor.SelectedItem.ToString()); }
            catch { panelColorPreview.BackColor = Color.Yellow; }
        }

        private void BrowseFolder()
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description  = "Select default folder for opening log files";
                dlg.SelectedPath = txtInitialDir.Text.Trim();
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    txtInitialDir.Text = dlg.SelectedPath;
            }
        }

        private void PreviewFont()
        {
            try
            {
                var style = FontStyle.Regular;
                if (chkFontBold.Checked)   style |= FontStyle.Bold;
                if (chkFontItalic.Checked) style |= FontStyle.Italic;
                using (var f = new Font(cmbFontFamily.SelectedItem?.ToString() ?? "Consolas",
                                        (float)nudFontSize.Value, style))
                {
                    MessageBox.Show(
                        "ABCDEFGHIJKLMNOPQRSTUVWXYZ\nabcdefghijklmnopqrstuvwxyz\n0123456789\n{}[]()<>+-*/=",
                        "Font Preview — " + f.Name,
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot create font: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Build helpers ─────────────────────────────────────────────────────
        private static TabPage Tab(string text)
        {
            return new TabPage(text) { Padding = new Padding(10), UseVisualStyleBackColor = true };
        }
        private static ComboBox AddRow(TabPage tp, string label, int y, out Label lbl)
        {
            lbl = Lbl(tp, label, 12, y + 3);
            var cmb = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FormattingEnabled = true,
                Location = new Point(175, y),
                Size = new Size(180, 24)
            };
            tp.Controls.Add(cmb);
            return cmb;
        }
        private static Label Lbl(TabPage tp, string text, int x, int y)
        {
            var l = new Label { AutoSize = true, Location = new Point(x, y), Text = text };
            tp.Controls.Add(l);
            return l;
        }
        private static CheckBox Chk(GroupBox grp, string text, int x, int y)
        {
            var c = new CheckBox { AutoSize = true, Location = new Point(x, y),
                Text = text, Checked = true, CheckState = CheckState.Checked };
            grp.Controls.Add(c);
            return c;
        }
        private static NumericUpDown AddNud(TabPage tp, string label, int y, decimal min, decimal max, decimal val)
        {
            Lbl(tp, label, 12, y + 3);
            var n = new NumericUpDown
            {
                Location = new Point(175, y), Size = new Size(100, 23),
                Minimum = min, Maximum = max, Value = val
            };
            tp.Controls.Add(n);
            return n;
        }
        private static TextBox AddTxt(TabPage tp, string label, int y, string def, int w)
        {
            Lbl(tp, label, 12, y + 3);
            var t = new TextBox { Location = new Point(175, y), Size = new Size(w, 23), Text = def };
            tp.Controls.Add(t);
            return t;
        }
        private static Button Btn(string text, int x, int y, int w, int h)
        {
            return new Button
            {
                Text = text, Location = new Point(x, y), Size = new Size(w, h),
                UseVisualStyleBackColor = true
            };
        }
    }
}
