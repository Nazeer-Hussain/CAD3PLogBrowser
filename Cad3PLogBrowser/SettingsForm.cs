using Cad3PLogBrowser.Services;
using Cad3PLogBrowser.AI.Models;
using Cad3PLogBrowser.AI.Security;
using Cad3PLogBrowser.AI.Services;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

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
        private CheckBox  chkShowCallGraph, chkShowFlameGraph, chkShowTimeline, chkShowAiTab;
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
        private NumericUpDown  nudSlowCallMs, nudFastCallMs, nudMaxFileMb;
        private CheckBox       chkFilterPerfOnTreeSelect;

        // ── Integration ───────────────────────────────────────────────────────
        private TextBox   txtGrokUrl, txtClaudeApiKey;
        private CheckBox  chkUseClaudeApi;

        // ── AI Settings ───────────────────────────────────────────────────────
        private CheckBox chkEnableAI;
        private ComboBox cmbAIProvider;
        private TextBox txtAIApiKey;
        private Button btnShowHideAIKey;
        private ComboBox cmbAIModel;
        private TrackBar trackAITemperature;
        private Label lblAITemperatureValue;
        private NumericUpDown numAIMaxTokens;
        private CheckBox chkAIStreaming;
        private CheckBox chkAIRedactData;
        private CheckBox chkAIRememberConversation;
        private NumericUpDown numAIMaxMessages;
        private TextBox txtOllamaServerUrl;
        private ComboBox cmbOllamaModel;
        private Button btnTestAIConnection;
        private Label lblAIStatus;
        private AISettings _aiSettings;

        // ── Updates (ENH-4) ───────────────────────────────────────────────────
        private CheckBox      chkCheckOnStartup;
        private NumericUpDown nudUpdateIntervalDays;
        private TextBox       txtManifestUrl;
        private Label         lblLastChecked;
        private Label         lblSkippedVersion;

        // ── Buttons ───────────────────────────────────────────────────────────
        private Button OkButton, CancelBtn, btnResetDefaults;

        // ─────────────────────────────────────────────────────────────────────
        public SettingsForm(MainForm mainForm)
        {
            InitializeComponent();

            _mainForm = mainForm;
            _settings = mainForm.AppSettings;
            _aiSettings = AISettingsService.Load();
            BuildUi();
            LoadCurrentSettings();

            // NOTE: ThemeManager.ApplyTheme moved to OnShown to avoid premature handle creation.
            // UpdateColourPreview is also called in OnShown after theme application.
        }

        protected override void OnShown(System.EventArgs e)
        {
            base.OnShown(e);

            // Apply theme now that the form and all controls are fully created.
            ThemeManager.ApplyTheme(this);

            // UpdateColourPreview must be called AFTER ApplyTheme because the theme
            // walk overwrites every Panel's BackColor, including panelColorPreview.
            UpdateColourPreview();
        }

        // ── UI Construction ───────────────────────────────────────────────────
        private void BuildUi()
        {
            Text             = "Settings";
            ClientSize       = new Size(556, 600);
            FormBorderStyle  = FormBorderStyle.FixedDialog;
            MaximizeBox      = false;
            MinimizeBox      = false;
            ShowInTaskbar    = false;
            StartPosition    = FormStartPosition.CenterParent;
            AutoScaleMode    = AutoScaleMode.Font;
            AutoScaleDimensions = new SizeF(8f, 16f);
            Font             = new Font("Segoe UI", 9f);

            var tabs = new TabControl
            {
                Location = new Point(12, 10),
                Size     = new Size(532, 540),
            };

            tabs.TabPages.Add(BuildAppearanceTab());
            tabs.TabPages.Add(BuildTabsLayoutTab());
            tabs.TabPages.Add(BuildFontTab());
            tabs.TabPages.Add(BuildFilesTab());
            tabs.TabPages.Add(BuildPerformanceTab());
            tabs.TabPages.Add(BuildAIAndIntegrationTab());
            tabs.TabPages.Add(BuildUpdatesTab());   // ENH-4

            // Bottom buttons
            btnResetDefaults = Btn("Reset to Defaults", 12,   564, 140, 28);
            OkButton         = Btn("&OK",               338,  564,  90, 28);
            CancelBtn        = Btn("&Cancel",            438,  564,  90, 28);

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

            var grp = new GroupBox { 
                Text = "Visible Tabs",
                Location = new Point(12, 10), 
                Size = new Size(498, 136), 
                TabStop = false,
                Font = new Font("Segoe UI", 9f)
            };

            // Row 1: Log, Raw, Performance, Log Details
            chkShowLog         = Chk(grp, "Log View",      14,  24);
            chkShowRaw         = Chk(grp, "Raw",           120,  24);
            chkShowPerformance = Chk(grp, "Performance",   200,  24);
            chkShowLogDetails  = Chk(grp, "Log Details",   330,  24);
            // Row 2: Call Graph, Flame Graph, Timeline
            chkShowCallGraph   = Chk(grp, "Call Graph",     14,  58);
            chkShowFlameGraph  = Chk(grp, "Flame Graph",   120,  58);
            chkShowTimeline    = Chk(grp, "Timeline",       250,  58);
            // Row 3: AI Assistant
            chkShowAiTab       = Chk(grp, "AI Assistant",   14,  92);
            tp.Controls.Add(grp);

            cmbInitialView = AddRow(tp, "Start-up tab:", 160, out _);
            cmbInitialView.Items.AddRange(new object[]
            {
                "Log", "Raw", "Performance", "Log Details",
                "Call Graph", "Flame Graph", "Timeline", "AI Assistant"
            });

            cmbDefaultTreeView = AddRow(tp, "Default tree view:", 196, out _);
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

            nudFastCallMs = AddNud(tp, "Fast call threshold:", 22, 1, 60000, 100);
            Lbl(tp, "ms   (green — below this is fast)", 290, 26);

            nudSlowCallMs = AddNud(tp, "Slow call threshold:", 58, 10, 60000, 1000);
            Lbl(tp, "ms   (red — above this is slow, amber is in between)", 290, 62);

            nudMaxFileMb = AddNud(tp, "Skip list view if file >", 94, 1, 2000, 50);
            Lbl(tp, "MB   (use Raw tab for very large files)", 290, 98);

            chkFilterPerfOnTreeSelect = new CheckBox
            {
                AutoSize = true,
                Location = new Point(12, 136),
                Text     = "Auto-filter Performance tab when a Call Tree node is selected",
            };
            Lbl(tp, "", 12, 140); // spacer
            tp.Controls.Add(chkFilterPerfOnTreeSelect);

            var hint = new Label
            {
                AutoSize  = false,
                Location  = new Point(30, 160),
                Size      = new Size(480, 34),
                Text      = "When OFF, use the Call Tree right-click menu to filter manually.",
                ForeColor = SystemColors.GrayText,
                Font      = new Font("Segoe UI", 8.5f)
            };
            tp.Controls.Add(hint);

            return tp;
        }

        // ── TAB: AI & Integration ─────────────────────────────────────────────
        private TabPage BuildAIAndIntegrationTab()
        {
            var tp = Tab("AI & Integration");

            // ═══ AI Settings Section ═══
            var grpAI = new GroupBox 
            { 
                Text = "AI Provider", 
                Location = new Point(12, 10), 
                Size = new Size(498, 165),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };

            // Enable AI checkbox
            chkEnableAI = new CheckBox
            {
                Text = "Enable AI Features",
                Location = new Point(10, 22),
                Size = new Size(200, 20),
                Checked = true
            };
            chkEnableAI.CheckedChanged += (s, e) => UpdateAIControlsState();
            grpAI.Controls.Add(chkEnableAI);

            // Provider selection
            grpAI.Controls.Add(new Label { 
                Text = "Provider:", 
                Location = new Point(10, 51), 
                AutoSize = true,
                Font = new Font("Segoe UI", 9f)
            });
            cmbAIProvider = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(100, 48),
                Size = new Size(380, 24)
            };
            cmbAIProvider.Items.AddRange(new object[] 
            { 
                "Mock (Testing)", 
                "Anthropic Claude", 
                "GitHub Copilot",
                "Ollama (Self-Hosted)",
                "OpenAI (Coming Soon)", 
                "Azure OpenAI (Coming Soon)", 
                "Google Gemini (Coming Soon)" 
            });
            cmbAIProvider.SelectedIndex = 0;
            cmbAIProvider.SelectedIndexChanged += (s, e) => UpdateAIProviderFields();
            grpAI.Controls.Add(cmbAIProvider);

            // API Key for cloud providers
            grpAI.Controls.Add(new Label { 
                Text = "API Key:", 
                Location = new Point(10, 81), 
                AutoSize = true,
                Font = new Font("Segoe UI", 9f)
            });
            txtAIApiKey = new TextBox
            {
                Location = new Point(100, 78),
                Size = new Size(330, 23),
                UseSystemPasswordChar = true
            };
            grpAI.Controls.Add(txtAIApiKey);

            btnShowHideAIKey = new Button
            {
                Text = "Show",
                Location = new Point(435, 78),
                Size = new Size(50, 25),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8f)
            };
            btnShowHideAIKey.Click += (s, e) =>
            {
                txtAIApiKey.UseSystemPasswordChar = !txtAIApiKey.UseSystemPasswordChar;
                btnShowHideAIKey.Text = txtAIApiKey.UseSystemPasswordChar ? "Show" : "Hide";
            };
            grpAI.Controls.Add(btnShowHideAIKey);

            // Ollama server URL (shown only for Ollama provider)
            grpAI.Controls.Add(new Label { 
                Text = "Server URL:", 
                Location = new Point(10, 111), 
                AutoSize = true,
                Font = new Font("Segoe UI", 9f)
            });
            txtOllamaServerUrl = new TextBox
            {
                Location = new Point(100, 108),
                Size = new Size(380, 23),
                Text = "http://localhost:11434",
                Visible = false
            };
            grpAI.Controls.Add(txtOllamaServerUrl);

            // Ollama model selection
            grpAI.Controls.Add(new Label { 
                Text = "Model:", 
                Location = new Point(10, 141), 
                AutoSize = true,
                Font = new Font("Segoe UI", 9f)
            });
            cmbOllamaModel = new ComboBox
            {
                Location = new Point(100, 138),
                Size = new Size(200, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Visible = false
            };
            cmbOllamaModel.Items.AddRange(new object[] { "llama3", "codellama", "mistral", "phi3" });
            cmbOllamaModel.SelectedIndex = 0;
            grpAI.Controls.Add(cmbOllamaModel);

            // Model for cloud providers
            cmbAIModel = new ComboBox
            {
                Location = new Point(100, 138),
                Size = new Size(200, 24),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbAIModel.Items.Add("(Select provider first)");
            cmbAIModel.SelectedIndex = 0;
            grpAI.Controls.Add(cmbAIModel);

            tp.Controls.Add(grpAI);

            // ═══ Model Configuration Section ═══
            var grpModel = new GroupBox 
            { 
                Text = "Model Configuration", 
                Location = new Point(12, 182), 
                Size = new Size(498, 110),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };

            // Temperature
            grpModel.Controls.Add(new Label { 
                Text = "Temperature:", 
                Location = new Point(10, 25), 
                AutoSize = true,
                Font = new Font("Segoe UI", 9f)
            });
            trackAITemperature = new TrackBar
            {
                Location = new Point(100, 22),
                Size = new Size(310, 45),
                Minimum = 0,
                Maximum = 20,
                Value = 7,
                TickFrequency = 1
            };
            trackAITemperature.ValueChanged += (s, e) =>
            {
                lblAITemperatureValue.Text = (trackAITemperature.Value / 10.0).ToString("0.0");
            };
            grpModel.Controls.Add(trackAITemperature);

            lblAITemperatureValue = new Label
            {
                Text = "0.7",
                Location = new Point(415, 27),
                Size = new Size(65, 20),
                TextAlign = ContentAlignment.MiddleRight
            };
            grpModel.Controls.Add(lblAITemperatureValue);

            var lblTempHelp = new Label
            {
                Text = "Lower = focused, Higher = creative",
                Location = new Point(100, 62),
                Size = new Size(380, 18),
                ForeColor = SystemColors.GrayText,
                Font = new Font("Segoe UI", 8.5f)
            };
            grpModel.Controls.Add(lblTempHelp);

            // Max tokens
            grpModel.Controls.Add(new Label { 
                Text = "Max Tokens:", 
                Location = new Point(10, 85), 
                AutoSize = true,
                Font = new Font("Segoe UI", 9f)
            });
            numAIMaxTokens = new NumericUpDown
            {
                Location = new Point(100, 82),
                Size = new Size(100, 23),
                Minimum = 100,
                Maximum = 200000,
                Value = 4096,
                Increment = 100
            };
            grpModel.Controls.Add(numAIMaxTokens);

            // Streaming
            chkAIStreaming = new CheckBox
            {
                Text = "Enable streaming",
                Location = new Point(210, 84),
                Size = new Size(140, 20),
                Checked = true
            };
            grpModel.Controls.Add(chkAIStreaming);

            tp.Controls.Add(grpModel);

            // ═══ Privacy & Conversation Section ═══
            var grpPrivacy = new GroupBox 
            { 
                Text = "Privacy & Conversation", 
                Location = new Point(12, 299), 
                Size = new Size(498, 75),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };

            chkAIRedactData = new CheckBox
            {
                Text = "Redact sensitive data (emails, IPs, paths)",
                Location = new Point(10, 22),
                Size = new Size(400, 20),
                Checked = true
            };
            grpPrivacy.Controls.Add(chkAIRedactData);

            chkAIRememberConversation = new CheckBox
            {
                Text = "Remember conversation history",
                Location = new Point(10, 47),
                Size = new Size(220, 20),
                Checked = true
            };
            chkAIRememberConversation.CheckedChanged += (s, e) => UpdateAIControlsState();
            grpPrivacy.Controls.Add(chkAIRememberConversation);

            grpPrivacy.Controls.Add(new Label { 
                Text = "Max messages:", 
                Location = new Point(240, 49), 
                AutoSize = true,
                Font = new Font("Segoe UI", 9f)
            });
            numAIMaxMessages = new NumericUpDown
            {
                Location = new Point(345, 46),
                Size = new Size(70, 23),
                Minimum = 5,
                Maximum = 100,
                Value = 20
            };
            grpPrivacy.Controls.Add(numAIMaxMessages);

            tp.Controls.Add(grpPrivacy);

            // ═══ Legacy Integration Section ═══
            var grpLegacy = new GroupBox 
            { 
                Text = "Legacy Integration (Deprecated)", 
                Location = new Point(12, 381), 
                Size = new Size(498, 100),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };

            grpLegacy.Controls.Add(new Label { 
                Text = "Grok URL:", 
                Location = new Point(10, 25), 
                AutoSize = true,
                Font = new Font("Segoe UI", 9f)
            });
            txtGrokUrl = new TextBox
            { 
                Location = new Point(100, 22), 
                Size = new Size(380, 23) 
            };
            grpLegacy.Controls.Add(txtGrokUrl);

            grpLegacy.Controls.Add(new Label { 
                Text = "Claude Key:", 
                Location = new Point(10, 55), 
                AutoSize = true,
                Font = new Font("Segoe UI", 9f)
            });
            txtClaudeApiKey = new TextBox
            {
                Location = new Point(100, 52),
                Size = new Size(380, 23),
                UseSystemPasswordChar = true
            };
            grpLegacy.Controls.Add(txtClaudeApiKey);

            chkUseClaudeApi = new CheckBox
            {
                Text = "Enable legacy Claude integration",
                Location = new Point(100, 77),
                Size = new Size(380, 20)
            };
            grpLegacy.Controls.Add(chkUseClaudeApi);

            var lblDeprecated = new Label
            {
                Text = "WARNING: Use 'Anthropic Claude' provider above instead",
                Location = new Point(10, 77),
                AutoSize = false,
                Size = new Size(480, 20),
                ForeColor = Color.DarkOrange,
                Font = new Font("Segoe UI", 8f)
            };
            grpLegacy.Controls.Add(lblDeprecated);

            tp.Controls.Add(grpLegacy);

            // ═══ Connection Testing ═══
            btnTestAIConnection = new Button
            {
                Text = "Test AI Connection",
                Location = new Point(12, 488),
                Size = new Size(140, 28)
            };
            btnTestAIConnection.Click += async (s, e) => await TestAIConnection();
            tp.Controls.Add(btnTestAIConnection);

            lblAIStatus = new Label
            {
                Text = "",
                Location = new Point(160, 493),
                Size = new Size(350, 20),
                ForeColor = Color.DarkGreen
            };
            tp.Controls.Add(lblAIStatus);

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
            chkShowAiTab.Checked       = _settings.ShowAiTab;
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
            nudFastCallMs.Value = Math.Max(nudFastCallMs.Minimum,
                Math.Min(nudFastCallMs.Maximum, _settings.FastCallThresholdMs));
            nudSlowCallMs.Value = Math.Max(nudSlowCallMs.Minimum,
                Math.Min(nudSlowCallMs.Maximum, _settings.SlowCallThresholdMs));
            nudMaxFileMb.Value  = Math.Max(nudMaxFileMb.Minimum,
                Math.Min(nudMaxFileMb.Maximum, _settings.MaxFileSizeMbForListView));
            chkFilterPerfOnTreeSelect.Checked = _settings.FilterPerfOnTreeSelect;

            // Integration
            txtGrokUrl.Text         = _settings.GrokUrl ?? "";
            txtClaudeApiKey.Text    = _settings.ClaudeApiKey ?? "";
            chkUseClaudeApi.Checked = _settings.UseClaudeApi;

            // Updates (ENH-4)
            chkCheckOnStartup.Checked    = _settings.CheckForUpdatesOnStartup;
            nudUpdateIntervalDays.Value  = Math.Max(0, Math.Min(365, _settings.UpdateCheckIntervalDays));
            txtManifestUrl.Text          = _settings.UpdateManifestUrl ?? "";
            lblLastChecked.Text          = _settings.LastUpdateCheck == DateTime.MinValue
                ? "Last checked:  never"
                : string.Format("Last checked:  {0:yyyy-MM-dd HH:mm} UTC", _settings.LastUpdateCheck);
            lblSkippedVersion.Text       = string.IsNullOrEmpty(_settings.SkippedVersion)
                ? "Skipped version:  (none)"
                : string.Format("Skipped version:  {0}", _settings.SkippedVersion);

            // AI Settings
            chkEnableAI.Checked = _aiSettings.EnableAI;

            // Map AIProviderType to combo index (accounting for Ollama at index 3)
            int providerIndex = 0;
            switch (_aiSettings.SelectedProvider)
            {
                case AIProviderType.Mock: providerIndex = 0; break;
                case AIProviderType.Anthropic: providerIndex = 1; break;
                case AIProviderType.GitHubCopilot: providerIndex = 2; break;
                case AIProviderType.Ollama: providerIndex = 3; break;
                default: providerIndex = 0; break;
            }
            cmbAIProvider.SelectedIndex = providerIndex;

            txtAIApiKey.Text = _aiSettings.GetCurrentApiKey();
            txtOllamaServerUrl.Text = _aiSettings.OllamaServerUrl ?? "http://localhost:11434";

            if (cmbOllamaModel.Items.Count > 0)
            {
                var ollamaModel = _aiSettings.OllamaModel ?? "llama3";
                var modelIndex = cmbOllamaModel.FindStringExact(ollamaModel);
                cmbOllamaModel.SelectedIndex = modelIndex >= 0 ? modelIndex : 0;
            }

            if (cmbAIModel.Items.Count > 0)
            {
                var currentModel = _aiSettings.GetCurrentModel();
                var modelIndex = cmbAIModel.FindStringExact(currentModel);
                if (modelIndex >= 0)
                    cmbAIModel.SelectedIndex = modelIndex;
                else
                    cmbAIModel.SelectedIndex = 0;
            }

            trackAITemperature.Value = (int)(_aiSettings.Temperature * 10);
            lblAITemperatureValue.Text = _aiSettings.Temperature.ToString("0.0");
            numAIMaxTokens.Value = Math.Max(100, Math.Min(200000, _aiSettings.MaxTokens));
            chkAIStreaming.Checked = _aiSettings.EnableStreaming;
            chkAIRedactData.Checked = _aiSettings.RedactSensitiveData;
            chkAIRememberConversation.Checked = _aiSettings.RememberConversation;
            numAIMaxMessages.Value = Math.Max(5, Math.Min(100, _aiSettings.MaxConversationMessages));

            UpdateAIProviderFields();
            UpdateAIControlsState();
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
            _settings.ShowAiTab          = chkShowAiTab.Checked;
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
            _settings.FastCallThresholdMs      = (int)nudFastCallMs.Value;
            _settings.SlowCallThresholdMs      = (long)nudSlowCallMs.Value;
            _settings.MaxFileSizeMbForListView  = (long)nudMaxFileMb.Value;
            _settings.FilterPerfOnTreeSelect    = chkFilterPerfOnTreeSelect.Checked;

            // Integration
            _settings.GrokUrl      = txtGrokUrl.Text.Trim();
            _settings.ClaudeApiKey = txtClaudeApiKey.Text.Trim();
            _settings.UseClaudeApi = chkUseClaudeApi.Checked;

            // Updates (ENH-4)
            _settings.CheckForUpdatesOnStartup = chkCheckOnStartup.Checked;
            _settings.UpdateCheckIntervalDays  = (int)nudUpdateIntervalDays.Value;
            // Guard: never persist an empty URL — fall back to the default so the
            // UpdateService constructor (which throws on whitespace) can never crash.
            string manifestUrl = txtManifestUrl.Text.Trim();
            _settings.UpdateManifestUrl = string.IsNullOrWhiteSpace(manifestUrl)
                ? AppSettings.DefaultUpdateManifestUrl
                : manifestUrl;
            // Reflect the resolved value back into the text box so the user can
            // see that the blank field was replaced with the default.
            txtManifestUrl.Text = _settings.UpdateManifestUrl;

            // Save AI Settings
            SaveAISettings();
            AISettingsService.Save(_aiSettings);

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
            _settings.ShowAiTab           = def.ShowAiTab;
            _settings.InitialView         = def.InitialView;
            _settings.DefaultTreeView     = def.DefaultTreeView;
            _settings.LogFontFamily       = def.LogFontFamily;
            _settings.LogFontSize         = def.LogFontSize;
            _settings.LogFontStyle        = def.LogFontStyle;
            _settings.InitialDirectory    = def.InitialDirectory;
            _settings.MaxRecentFiles      = def.MaxRecentFiles;
            _settings.SaveSnippetSuffix   = def.SaveSnippetSuffix;
            _settings.FastCallThresholdMs       = def.FastCallThresholdMs;
            _settings.SlowCallThresholdMs       = def.SlowCallThresholdMs;
            _settings.MaxFileSizeMbForListView  = def.MaxFileSizeMbForListView;
            _settings.FilterPerfOnTreeSelect    = def.FilterPerfOnTreeSelect;
            _settings.GrokUrl             = def.GrokUrl;
            // Note: API key and UseClaudeApi are NOT reset (security/convenience)
            // Updates — reset to defaults but preserve LastUpdateCheck and SkippedVersion
            _settings.CheckForUpdatesOnStartup = def.CheckForUpdatesOnStartup;
            _settings.UpdateCheckIntervalDays  = def.UpdateCheckIntervalDays;
            _settings.UpdateManifestUrl        = def.UpdateManifestUrl;
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

        // ── TAB: Updates (ENH-4) ──────────────────────────────────────────────
        private TabPage BuildUpdatesTab()
        {
            var tp = Tab("Updates");

            chkCheckOnStartup = new CheckBox
            {
                AutoSize = true,
                Location = new Point(12, 22),
                Text     = "Check for updates automatically on startup"
            };
            tp.Controls.Add(chkCheckOnStartup);

            nudUpdateIntervalDays = AddNud(tp, "Check interval (days):", 58, 0, 365, 1);
            Lbl(tp, "  0 = every launch", 290, 62);

            Lbl(tp, "Manifest URL:", 12, 98);
            txtManifestUrl = new TextBox
            {
                Location = new Point(12, 114),
                Size     = new Size(500, 23)
            };
            tp.Controls.Add(txtManifestUrl);

            var hint = new Label
            {
                AutoSize  = false,
                Location  = new Point(12, 142),
                Size      = new Size(500, 18),
                Text      = "Leave as default unless you host your own update server.",
                ForeColor = SystemColors.GrayText,
                Font      = new Font("Segoe UI", 8.5f)
            };
            tp.Controls.Add(hint);

            lblLastChecked = new Label
            {
                AutoSize  = true,
                Location  = new Point(12, 170),
                ForeColor = SystemColors.GrayText
            };
            tp.Controls.Add(lblLastChecked);

            lblSkippedVersion = new Label
            {
                AutoSize  = true,
                Location  = new Point(12, 192),
                ForeColor = SystemColors.GrayText
            };
            tp.Controls.Add(lblSkippedVersion);

            var btnClearSkip = Btn("Clear Skipped Version", 12, 216, 160, 26);
            btnClearSkip.Click += (s, e) =>
            {
                _settings.SkippedVersion  = "";
                lblSkippedVersion.Text    = "Skipped version:  (none)";
            };
            tp.Controls.Add(btnClearSkip);

            var btnCheckNow = Btn("Check Now", 186, 216, 100, 26);
            btnCheckNow.Click += (s, e) =>
            {
                DialogResult = DialogResult.OK;
                OkButton_Click();
                Close();
                // MainForm.checkForUpdatesMenuItem_Click equivalent
                if (_mainForm != null)
                    _mainForm.TriggerUpdateCheck();
            };
            tp.Controls.Add(btnCheckNow);

            return tp;
        }

        // ── TAB: AI Settings ──────────────────────────────────────────────────
        private TabPage BuildAISettingsTab()
        {
            var tp = Tab("AI Settings");

            // Enable AI checkbox
            chkEnableAI = new CheckBox
            {
                Text = "Enable AI Features",
                Location = new Point(12, 22),
                Size = new Size(200, 20),
                Checked = true
            };
            chkEnableAI.CheckedChanged += (s, e) => UpdateAIControlsState();
            tp.Controls.Add(chkEnableAI);

            // Provider selection
            cmbAIProvider = AddRow(tp, "AI Provider:", 52, out _);
            cmbAIProvider.Items.AddRange(new object[] 
            { 
                "Mock (Testing)", 
                "Anthropic Claude", 
                "GitHub Copilot",
                "Ollama (Self-Hosted)",
                "OpenAI (Coming Soon)", 
                "Azure OpenAI (Coming Soon)", 
                "Google Gemini (Coming Soon)" 
            });
            cmbAIProvider.SelectedIndex = 0;
            cmbAIProvider.SelectedIndexChanged += (s, e) => UpdateAIProviderFields();

            // API Key for cloud providers
            Lbl(tp, "API Key:", 12, 91);
            txtAIApiKey = new TextBox
            {
                Location = new Point(175, 88),
                Size = new Size(270, 23),
                UseSystemPasswordChar = true
            };
            tp.Controls.Add(txtAIApiKey);

            btnShowHideAIKey = Btn("Show", 450, 88, 50, 25);
            btnShowHideAIKey.Font = new Font("Segoe UI", 8f);
            btnShowHideAIKey.Click += (s, e) =>
            {
                txtAIApiKey.UseSystemPasswordChar = !txtAIApiKey.UseSystemPasswordChar;
                btnShowHideAIKey.Text = txtAIApiKey.UseSystemPasswordChar ? "Show" : "Hide";
            };
            tp.Controls.Add(btnShowHideAIKey);

            // Ollama server URL (shown only for Ollama provider)
            Lbl(tp, "Ollama Server:", 12, 127);
            txtOllamaServerUrl = new TextBox
            {
                Location = new Point(175, 124),
                Size = new Size(320, 23),
                Text = "http://localhost:11434",
                Visible = false
            };
            tp.Controls.Add(txtOllamaServerUrl);

            // Ollama model selection
            Lbl(tp, "Ollama Model:", 12, 163);
            cmbOllamaModel = new ComboBox
            {
                Location = new Point(175, 160),
                Size = new Size(180, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Visible = false
            };
            cmbOllamaModel.Items.AddRange(new object[] { "llama3", "codellama", "mistral", "phi3" });
            cmbOllamaModel.SelectedIndex = 0;
            tp.Controls.Add(cmbOllamaModel);

            // Model for cloud providers
            cmbAIModel = AddRow(tp, "Model:", 124, out _);
            cmbAIModel.Items.Add("(Select provider first)");
            cmbAIModel.SelectedIndex = 0;

            // Temperature
            Lbl(tp, "Temperature:", 12, 163);
            trackAITemperature = new TrackBar
            {
                Location = new Point(175, 158),
                Size = new Size(270, 45),
                Minimum = 0,
                Maximum = 20,
                Value = 7,
                TickFrequency = 1
            };
            trackAITemperature.ValueChanged += (s, e) =>
            {
                lblAITemperatureValue.Text = (trackAITemperature.Value / 10.0).ToString("0.0");
            };
            tp.Controls.Add(trackAITemperature);

            lblAITemperatureValue = new Label
            {
                Text = "0.7",
                Location = new Point(450, 163),
                Size = new Size(45, 20),
                TextAlign = ContentAlignment.MiddleRight
            };
            tp.Controls.Add(lblAITemperatureValue);

            // Max tokens
            numAIMaxTokens = AddNud(tp, "Max Tokens:", 204, 100, 200000, 4096);
            numAIMaxTokens.Increment = 100;

            // Streaming
            chkAIStreaming = new CheckBox
            {
                Text = "Enable streaming responses",
                Location = new Point(175, 236),
                Size = new Size(300, 20),
                Checked = true
            };
            tp.Controls.Add(chkAIStreaming);

            // Privacy - Redact data
            chkAIRedactData = new CheckBox
            {
                Text = "Redact sensitive data (emails, IPs, paths)",
                Location = new Point(12, 268),
                Size = new Size(400, 20),
                Checked = true
            };
            tp.Controls.Add(chkAIRedactData);

            // Conversation settings
            chkAIRememberConversation = new CheckBox
            {
                Text = "Remember conversation history",
                Location = new Point(12, 296),
                Size = new Size(250, 20),
                Checked = true
            };
            chkAIRememberConversation.CheckedChanged += (s, e) => UpdateAIControlsState();
            tp.Controls.Add(chkAIRememberConversation);

            numAIMaxMessages = AddNud(tp, "Max messages:", 324, 5, 100, 20);

            // Test connection button
            btnTestAIConnection = Btn("Test Connection", 12, 352, 130, 28);
            btnTestAIConnection.Click += async (s, e) => await TestAIConnection();
            tp.Controls.Add(btnTestAIConnection);

            // Status label
            lblAIStatus = new Label
            {
                Text = "",
                Location = new Point(150, 357),
                Size = new Size(360, 20),
                ForeColor = Color.DarkGreen
            };
            tp.Controls.Add(lblAIStatus);

            return tp;
        }

        private void SaveAISettings()
        {
            _aiSettings.EnableAI = chkEnableAI.Checked;

            // Map combo index to AIProviderType
            var providerIndex = cmbAIProvider.SelectedIndex;
            if (providerIndex == 0) _aiSettings.SelectedProvider = AIProviderType.Mock;
            else if (providerIndex == 1) _aiSettings.SelectedProvider = AIProviderType.Anthropic;
            else if (providerIndex == 2) _aiSettings.SelectedProvider = AIProviderType.GitHubCopilot;
            else if (providerIndex == 3) _aiSettings.SelectedProvider = AIProviderType.Ollama;
            else _aiSettings.SelectedProvider = AIProviderType.None;

            _aiSettings.Temperature = trackAITemperature.Value / 10.0;
            _aiSettings.MaxTokens = (int)numAIMaxTokens.Value;
            _aiSettings.EnableStreaming = chkAIStreaming.Checked;
            _aiSettings.RedactSensitiveData = chkAIRedactData.Checked;
            _aiSettings.RememberConversation = chkAIRememberConversation.Checked;
            _aiSettings.MaxConversationMessages = (int)numAIMaxMessages.Value;

            // Save provider-specific settings
            switch (_aiSettings.SelectedProvider)
            {
                case AIProviderType.Anthropic:
                    _aiSettings.AnthropicApiKey = txtAIApiKey.Text.Trim();
                    _aiSettings.AnthropicModel = cmbAIModel.Text;
                    break;

                case AIProviderType.GitHubCopilot:
                    _aiSettings.GitHubCopilotApiToken = txtAIApiKey.Text.Trim();
                    _aiSettings.GitHubCopilotModel = cmbAIModel.Text;
                    break;

                case AIProviderType.Ollama:
                    _aiSettings.OllamaServerUrl = txtOllamaServerUrl.Text.Trim();
                    _aiSettings.OllamaModel = cmbOllamaModel.Text;
                    break;
            }
        }

        private void UpdateAIControlsState()
        {
            bool enabled = chkEnableAI.Checked;
            cmbAIProvider.Enabled = enabled;

            bool isOllama = cmbAIProvider.SelectedIndex == 3;
            bool needsApiKey = enabled && cmbAIProvider.SelectedIndex > 0 && !isOllama;

            txtAIApiKey.Enabled = needsApiKey;
            btnShowHideAIKey.Enabled = needsApiKey;
            txtOllamaServerUrl.Visible = enabled && isOllama;
            cmbOllamaModel.Visible = enabled && isOllama;

            cmbAIModel.Enabled = enabled && !isOllama;
            cmbAIModel.Visible = !isOllama;
            trackAITemperature.Enabled = enabled;
            numAIMaxTokens.Enabled = enabled;
            chkAIStreaming.Enabled = enabled;
            chkAIRedactData.Enabled = enabled;
            chkAIRememberConversation.Enabled = enabled;
            numAIMaxMessages.Enabled = enabled && chkAIRememberConversation.Checked;
            btnTestAIConnection.Enabled = enabled;
        }

        private void UpdateAIProviderFields()
        {
            var providerIndex = cmbAIProvider.SelectedIndex;

            // Map combo index to AIProviderType (accounting for Ollama at index 3)
            AIProviderType provider;
            if (providerIndex == 0) provider = AIProviderType.Mock;
            else if (providerIndex == 1) provider = AIProviderType.Anthropic;
            else if (providerIndex == 2) provider = AIProviderType.GitHubCopilot;
            else if (providerIndex == 3) provider = AIProviderType.Ollama;
            else provider = AIProviderType.None;

            cmbAIModel.Items.Clear();

            bool isOllama = providerIndex == 3;
            txtOllamaServerUrl.Visible = isOllama;
            cmbOllamaModel.Visible = isOllama;
            cmbAIModel.Visible = !isOllama;

            switch (provider)
            {
                case AIProviderType.Mock:
                    txtAIApiKey.Text = "";
                    cmbAIModel.Items.Add("mock-model-1.0");
                    break;

                case AIProviderType.Anthropic:
                    txtAIApiKey.Text = _aiSettings.AnthropicApiKey;
                    cmbAIModel.Items.AddRange(new object[] 
                    { 
                        "claude-3-5-sonnet-20241022",
                        "claude-3-opus-latest",
                        "claude-3-haiku-latest"
                    });
                    break;

                case AIProviderType.GitHubCopilot:
                    txtAIApiKey.Text = _aiSettings.GitHubCopilotApiToken;
                    cmbAIModel.Items.AddRange(new object[] 
                    { 
                        "gpt-4",
                        "gpt-4-turbo",
                        "gpt-3.5-turbo"
                    });
                    break;

                case AIProviderType.Ollama:
                    txtAIApiKey.Text = "";
                    txtOllamaServerUrl.Text = _aiSettings.OllamaServerUrl ?? "http://localhost:11434";
                    // Ollama uses separate combo
                    break;

                default:
                    txtAIApiKey.Text = "";
                    cmbAIModel.Items.Add("(Coming soon)");
                    break;
            }

            if (cmbAIModel.Items.Count > 0)
                cmbAIModel.SelectedIndex = 0;

            UpdateAIControlsState();
        }

        private async System.Threading.Tasks.Task TestAIConnection()
        {
            SaveAISettings();

            var aiService = new AIService(_aiSettings);

            if (!aiService.IsEnabled)
            {
                lblAIStatus.ForeColor = Color.DarkOrange;
                lblAIStatus.Text = "⚠ AI is disabled or not configured";
                return;
            }

            btnTestAIConnection.Enabled = false;
            btnTestAIConnection.Text = "Testing...";
            lblAIStatus.Text = "Testing connection...";
            lblAIStatus.ForeColor = Color.Blue;
            Cursor = Cursors.WaitCursor;

            try
            {
                var (success, message) = await aiService.TestConnectionAsync();

                if (success)
                {
                    lblAIStatus.ForeColor = Color.DarkGreen;
                    lblAIStatus.Text = "✓ Connection successful!";
                }
                else
                {
                    lblAIStatus.ForeColor = Color.DarkRed;
                    lblAIStatus.Text = "✗ Connection failed: " + message;
                }
            }
            catch (Exception ex)
            {
                lblAIStatus.ForeColor = Color.DarkRed;
                lblAIStatus.Text = "✗ Error: " + ex.Message;
            }
            finally
            {
                btnTestAIConnection.Enabled = true;
                btnTestAIConnection.Text = "Test Connection";
                Cursor = Cursors.Default;
            }
        }

        // ── Build helpers ─────────────────────────────────────────────────────
        private static TabPage Tab(string text)
        {
            return new TabPage(text) { 
                Padding = new Padding(10), 
                UseVisualStyleBackColor = true,
                Font = new Font("Segoe UI", 9f)
            };
        }
        private static ComboBox AddRow(TabPage tp, string label, int y, out Label lbl)
        {
            lbl = Lbl(tp, label, 12, y + 3);

            var cmb = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FormattingEnabled = true,
                Location = new Point(175, y),
                Size = new Size(180, 24),
                Font = new Font("Segoe UI", 9f)
            };
            tp.Controls.Add(cmb);
            return cmb;
        }
        private static Label Lbl(TabPage tp, string text, int x, int y)
        {
            var l = new Label { 
                AutoSize = true, 
                Location = new Point(x, y), 
                Text = text,
                Font = new Font("Segoe UI", 9f)
            };
            tp.Controls.Add(l);
            return l;
        }
        private static CheckBox Chk(GroupBox grp, string text, int x, int y)
        {
            var c = new CheckBox { 
                AutoSize = true, 
                Location = new Point(x, y),
                Text = text, 
                Checked = true, 
                CheckState = CheckState.Checked,
                Font = new Font("Segoe UI", 9f)
            };
            grp.Controls.Add(c);
            return c;
        }
        private static NumericUpDown AddNud(TabPage tp, string label, int y, decimal min, decimal max, decimal val)
        {
            Lbl(tp, label, 12, y + 3);
            var n = new NumericUpDown
            {
                Location = new Point(175, y), 
                Size = new Size(100, 23),
                Minimum = min, 
                Maximum = max, 
                Value = val,
                Font = new Font("Segoe UI", 9f)
            };
            tp.Controls.Add(n);
            return n;
        }
        private static TextBox AddTxt(TabPage tp, string label, int y, string def, int w)
        {
            Lbl(tp, label, 12, y + 3);
            var t = new TextBox { 
                Location = new Point(175, y), 
                Size = new Size(w, 23), 
                Text = def,
                Font = new Font("Segoe UI", 9f)
            };
            tp.Controls.Add(t);
            return t;
        }
        private static Button Btn(string text, int x, int y, int w, int h)
        {
            return new Button
            {
                Text = text, 
                Location = new Point(x, y), 
                Size = new Size(w, h),
                UseVisualStyleBackColor = true,
                Font = new Font("Segoe UI", 9f)
            };
        }
    }
}
