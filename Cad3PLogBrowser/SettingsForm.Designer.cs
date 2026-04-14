namespace Cad3PLogBrowser
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private void InitializeComponent()
        {
            this.grpTabs            = new System.Windows.Forms.GroupBox();
            this.chkShowLog         = new System.Windows.Forms.CheckBox();
            this.chkShowRaw         = new System.Windows.Forms.CheckBox();
            this.chkShowPerformance = new System.Windows.Forms.CheckBox();
            this.chkShowLogDetails  = new System.Windows.Forms.CheckBox();
            this.chkShowCallGraph   = new System.Windows.Forms.CheckBox();
            this.chkShowFlameGraph  = new System.Windows.Forms.CheckBox();
            this.chkShowTimeline    = new System.Windows.Forms.CheckBox();
            this.grpGrok            = new System.Windows.Forms.GroupBox();
            this.lblGrokUrl         = new System.Windows.Forms.Label();
            this.txtGrokUrl         = new System.Windows.Forms.TextBox();
            this.lblClaudeApiKey    = new System.Windows.Forms.Label();
            this.txtClaudeApiKey    = new System.Windows.Forms.TextBox();
            this.chkUseClaudeApi    = new System.Windows.Forms.CheckBox();
            this.grpAppearance      = new System.Windows.Forms.GroupBox();
            this.lblHighlight       = new System.Windows.Forms.Label();
            this.cmbHighlightColor  = new System.Windows.Forms.ComboBox();
            this.panelColorPreview  = new System.Windows.Forms.Panel();
            this.lblTheme           = new System.Windows.Forms.Label();
            this.cmbTheme           = new System.Windows.Forms.ComboBox();
            this.lblIconSize        = new System.Windows.Forms.Label();
            this.cmbIconSize        = new System.Windows.Forms.ComboBox();
            this.chkShowToolbar     = new System.Windows.Forms.CheckBox();
            this.grpFont            = new System.Windows.Forms.GroupBox();
            this.lblFontFamily      = new System.Windows.Forms.Label();
            this.cmbFontFamily      = new System.Windows.Forms.ComboBox();
            this.lblFontSize        = new System.Windows.Forms.Label();
            this.nudFontSize        = new System.Windows.Forms.NumericUpDown();
            this.chkFontBold        = new System.Windows.Forms.CheckBox();
            this.chkFontItalic      = new System.Windows.Forms.CheckBox();
            this.btnFontPreview     = new System.Windows.Forms.Button();
            this.grpBehavior        = new System.Windows.Forms.GroupBox();
            this.lblInitialView     = new System.Windows.Forms.Label();
            this.cmbInitialView     = new System.Windows.Forms.ComboBox();
            this.lblSnippetSuffix   = new System.Windows.Forms.Label();
            this.txtSnippetSuffix   = new System.Windows.Forms.TextBox();
            this.lblMaxRecentFiles  = new System.Windows.Forms.Label();
            this.nudMaxRecentFiles  = new System.Windows.Forms.NumericUpDown();
            this.grpPerf            = new System.Windows.Forms.GroupBox();
            this.lblSlowCall        = new System.Windows.Forms.Label();
            this.nudSlowCallMs      = new System.Windows.Forms.NumericUpDown();
            this.lblMaxFile         = new System.Windows.Forms.Label();
            this.nudMaxFileMb       = new System.Windows.Forms.NumericUpDown();
            this.OkButton           = new System.Windows.Forms.Button();
            this.CancelBtn          = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudFontSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxRecentFiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSlowCallMs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxFileMb)).BeginInit();
            this.grpTabs.SuspendLayout();
            this.grpGrok.SuspendLayout();
            this.grpAppearance.SuspendLayout();
            this.grpFont.SuspendLayout();
            this.grpBehavior.SuspendLayout();
            this.grpPerf.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpTabs
            // 
            this.grpTabs.Controls.Add(this.chkShowLog);
            this.grpTabs.Controls.Add(this.chkShowRaw);
            this.grpTabs.Controls.Add(this.chkShowPerformance);
            this.grpTabs.Controls.Add(this.chkShowLogDetails);
            this.grpTabs.Controls.Add(this.chkShowCallGraph);
            this.grpTabs.Controls.Add(this.chkShowFlameGraph);
            this.grpTabs.Controls.Add(this.chkShowTimeline);
            this.grpTabs.Location = new System.Drawing.Point(12, 12);
            this.grpTabs.Size = new System.Drawing.Size(500, 120);
            this.grpTabs.TabStop = false;
            this.grpTabs.Text = "Visible Tabs";
            // 
            // chkShowLog
            // 
            this.chkShowLog.AutoSize = true;
            this.chkShowLog.Checked = true;
            this.chkShowLog.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowLog.Location = new System.Drawing.Point(16, 28);
            this.chkShowLog.Name = "chkShowLog";
            this.chkShowLog.Size = new System.Drawing.Size(51, 21);
            this.chkShowLog.Text = "Log";
            // 
            // chkShowRaw
            // 
            this.chkShowRaw.AutoSize = true;
            this.chkShowRaw.Checked = true;
            this.chkShowRaw.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowRaw.Location = new System.Drawing.Point(16, 56);
            this.chkShowRaw.Name = "chkShowRaw";
            this.chkShowRaw.Size = new System.Drawing.Size(56, 21);
            this.chkShowRaw.Text = "Raw";
            // 
            // chkShowPerformance
            // 
            this.chkShowPerformance.AutoSize = true;
            this.chkShowPerformance.Checked = true;
            this.chkShowPerformance.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowPerformance.Location = new System.Drawing.Point(16, 84);
            this.chkShowPerformance.Name = "chkShowPerformance";
            this.chkShowPerformance.Size = new System.Drawing.Size(115, 21);
            this.chkShowPerformance.Text = "Performance";
            // 
            // chkShowLogDetails
            // 
            this.chkShowLogDetails.AutoSize = true;
            this.chkShowLogDetails.Checked = true;
            this.chkShowLogDetails.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowLogDetails.Location = new System.Drawing.Point(200, 28);
            this.chkShowLogDetails.Name = "chkShowLogDetails";
            this.chkShowLogDetails.Size = new System.Drawing.Size(103, 21);
            this.chkShowLogDetails.Text = "Log Details";
            // 
            // chkShowCallGraph
            // 
            this.chkShowCallGraph.AutoSize = true;
            this.chkShowCallGraph.Checked = true;
            this.chkShowCallGraph.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowCallGraph.Location = new System.Drawing.Point(200, 56);
            this.chkShowCallGraph.Name = "chkShowCallGraph";
            this.chkShowCallGraph.Size = new System.Drawing.Size(104, 21);
            this.chkShowCallGraph.Text = "Call Graph";
            // 
            // chkShowFlameGraph
            // 
            this.chkShowFlameGraph.AutoSize = true;
            this.chkShowFlameGraph.Checked = true;
            this.chkShowFlameGraph.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowFlameGraph.Location = new System.Drawing.Point(200, 84);
            this.chkShowFlameGraph.Name = "chkShowFlameGraph";
            this.chkShowFlameGraph.Size = new System.Drawing.Size(120, 21);
            this.chkShowFlameGraph.Text = "Flame Graph";
            // 
            // chkShowTimeline
            // 
            this.chkShowTimeline.AutoSize = true;
            this.chkShowTimeline.Checked = true;
            this.chkShowTimeline.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowTimeline.Location = new System.Drawing.Point(370, 28);
            this.chkShowTimeline.Name = "chkShowTimeline";
            this.chkShowTimeline.Size = new System.Drawing.Size(88, 21);
            this.chkShowTimeline.Text = "Timeline";
            // 
            // grpGrok
            // 
            this.grpGrok.Controls.Add(this.lblGrokUrl);
            this.grpGrok.Controls.Add(this.txtGrokUrl);
            this.grpGrok.Controls.Add(this.lblClaudeApiKey);
            this.grpGrok.Controls.Add(this.txtClaudeApiKey);
            this.grpGrok.Controls.Add(this.chkUseClaudeApi);
            this.grpGrok.Location = new System.Drawing.Point(12, 142);
            this.grpGrok.Size = new System.Drawing.Size(500, 100);
            this.grpGrok.TabStop = false;
            this.grpGrok.Text = "External Integration";
            // 
            // lblGrokUrl
            // 
            this.lblGrokUrl.AutoSize = true;
            this.lblGrokUrl.Location = new System.Drawing.Point(16, 28);
            this.lblGrokUrl.Name = "lblGrokUrl";
            this.lblGrokUrl.Size = new System.Drawing.Size(75, 17);
            this.lblGrokUrl.Text = "Grok URL:";
            // 
            // txtGrokUrl
            // 
            this.txtGrokUrl.Location = new System.Drawing.Point(130, 25);
            this.txtGrokUrl.Name = "txtGrokUrl";
            this.txtGrokUrl.Size = new System.Drawing.Size(350, 22);
            // 
            // lblClaudeApiKey
            // 
            this.lblClaudeApiKey.AutoSize = true;
            this.lblClaudeApiKey.Location = new System.Drawing.Point(16, 60);
            this.lblClaudeApiKey.Name = "lblClaudeApiKey";
            this.lblClaudeApiKey.Size = new System.Drawing.Size(115, 17);
            this.lblClaudeApiKey.Text = "Claude API Key:";
            // 
            // txtClaudeApiKey
            // 
            this.txtClaudeApiKey.Location = new System.Drawing.Point(130, 57);
            this.txtClaudeApiKey.Name = "txtClaudeApiKey";
            this.txtClaudeApiKey.Size = new System.Drawing.Size(350, 22);
            this.txtClaudeApiKey.UseSystemPasswordChar = true;
            // 
            // chkUseClaudeApi
            // 
            this.chkUseClaudeApi.AutoSize = true;
            this.chkUseClaudeApi.Location = new System.Drawing.Point(130, 83);
            this.chkUseClaudeApi.Name = "chkUseClaudeApi";
            this.chkUseClaudeApi.Text = "Enable Claude API (requires key above — enhances all AI features)";
            this.chkUseClaudeApi.Font = new System.Drawing.Font("Segoe UI", 8.5f);
            // 
            // grpAppearance
            // 
            this.grpAppearance.Controls.Add(this.lblTheme);
            this.grpAppearance.Controls.Add(this.cmbTheme);
            this.grpAppearance.Controls.Add(this.lblHighlight);
            this.grpAppearance.Controls.Add(this.cmbHighlightColor);
            this.grpAppearance.Controls.Add(this.panelColorPreview);
            this.grpAppearance.Controls.Add(this.lblIconSize);
            this.grpAppearance.Controls.Add(this.cmbIconSize);
            this.grpAppearance.Controls.Add(this.chkShowToolbar);
            this.grpAppearance.Location = new System.Drawing.Point(12, 252);
            this.grpAppearance.Size = new System.Drawing.Size(500, 135);
            this.grpAppearance.TabStop = false;
            this.grpAppearance.Text = "Appearance";
            // 
            // lblTheme
            // 
            this.lblTheme.AutoSize = true;
            this.lblTheme.Location = new System.Drawing.Point(16, 28);
            this.lblTheme.Name = "lblTheme";
            this.lblTheme.Size = new System.Drawing.Size(58, 17);
            this.lblTheme.Text = "Theme:";
            // 
            // cmbTheme
            // 
            this.cmbTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTheme.FormattingEnabled = true;
            this.cmbTheme.Location = new System.Drawing.Point(130, 25);
            this.cmbTheme.Name = "cmbTheme";
            this.cmbTheme.Size = new System.Drawing.Size(140, 24);
            // 
            // lblHighlight
            // 
            this.lblHighlight.AutoSize = true;
            this.lblHighlight.Location = new System.Drawing.Point(16, 60);
            this.lblHighlight.Name = "lblHighlight";
            this.lblHighlight.Size = new System.Drawing.Size(109, 17);
            this.lblHighlight.Text = "Highlight color:";
            // 
            // cmbHighlightColor
            // 
            this.cmbHighlightColor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHighlightColor.FormattingEnabled = true;
            this.cmbHighlightColor.Location = new System.Drawing.Point(130, 57);
            this.cmbHighlightColor.Name = "cmbHighlightColor";
            this.cmbHighlightColor.Size = new System.Drawing.Size(140, 24);
            this.cmbHighlightColor.SelectedIndexChanged += new System.EventHandler(this.cmbHighlightColor_SelectedIndexChanged);
            // 
            // panelColorPreview
            // 
            this.panelColorPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColorPreview.Location = new System.Drawing.Point(280, 57);
            this.panelColorPreview.Name = "panelColorPreview";
            this.panelColorPreview.Size = new System.Drawing.Size(70, 24);
            // 
            // lblIconSize
            // 
            this.lblIconSize.AutoSize = true;
            this.lblIconSize.Location = new System.Drawing.Point(16, 92);
            this.lblIconSize.Name = "lblIconSize";
            this.lblIconSize.Size = new System.Drawing.Size(119, 17);
            this.lblIconSize.Text = "Toolbar icon size:";
            // 
            // cmbIconSize
            // 
            this.cmbIconSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbIconSize.FormattingEnabled = true;
            this.cmbIconSize.Location = new System.Drawing.Point(150, 89);
            this.cmbIconSize.Name = "cmbIconSize";
            this.cmbIconSize.Size = new System.Drawing.Size(120, 24);
            // 
            // chkShowToolbar
            // 
            this.chkShowToolbar.AutoSize = true;
            this.chkShowToolbar.Checked = true;
            this.chkShowToolbar.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowToolbar.Location = new System.Drawing.Point(290, 90);
            this.chkShowToolbar.Name = "chkShowToolbar";
            this.chkShowToolbar.Size = new System.Drawing.Size(118, 21);
            this.chkShowToolbar.Text = "Show Toolbar";
            // 
            // grpFont
            // 
            this.grpFont.Controls.Add(this.lblFontFamily);
            this.grpFont.Controls.Add(this.cmbFontFamily);
            this.grpFont.Controls.Add(this.lblFontSize);
            this.grpFont.Controls.Add(this.nudFontSize);
            this.grpFont.Controls.Add(this.chkFontBold);
            this.grpFont.Controls.Add(this.chkFontItalic);
            this.grpFont.Controls.Add(this.btnFontPreview);
            this.grpFont.Location = new System.Drawing.Point(12, 397);
            this.grpFont.Size = new System.Drawing.Size(500, 105);
            this.grpFont.TabStop = false;
            this.grpFont.Text = "Log Font";
            // 
            // lblFontFamily
            // 
            this.lblFontFamily.AutoSize = true;
            this.lblFontFamily.Location = new System.Drawing.Point(16, 28);
            this.lblFontFamily.Name = "lblFontFamily";
            this.lblFontFamily.Size = new System.Drawing.Size(86, 17);
            this.lblFontFamily.Text = "Font Family:";
            // 
            // cmbFontFamily
            // 
            this.cmbFontFamily.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFontFamily.FormattingEnabled = true;
            this.cmbFontFamily.Location = new System.Drawing.Point(110, 25);
            this.cmbFontFamily.Name = "cmbFontFamily";
            this.cmbFontFamily.Size = new System.Drawing.Size(180, 24);
            // 
            // lblFontSize
            // 
            this.lblFontSize.AutoSize = true;
            this.lblFontSize.Location = new System.Drawing.Point(310, 28);
            this.lblFontSize.Name = "lblFontSize";
            this.lblFontSize.Size = new System.Drawing.Size(40, 17);
            this.lblFontSize.Text = "Size:";
            // 
            // nudFontSize
            // 
            this.nudFontSize.DecimalPlaces = 1;
            this.nudFontSize.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
            this.nudFontSize.Location = new System.Drawing.Point(360, 26);
            this.nudFontSize.Maximum = new decimal(new int[] { 24, 0, 0, 0 });
            this.nudFontSize.Minimum = new decimal(new int[] { 6, 0, 0, 0 });
            this.nudFontSize.Name = "nudFontSize";
            this.nudFontSize.Size = new System.Drawing.Size(80, 22);
            this.nudFontSize.Value = new decimal(new int[] { 9, 0, 0, 0 });
            // 
            // chkFontBold
            // 
            this.chkFontBold.AutoSize = true;
            this.chkFontBold.Location = new System.Drawing.Point(110, 60);
            this.chkFontBold.Name = "chkFontBold";
            this.chkFontBold.Size = new System.Drawing.Size(60, 21);
            this.chkFontBold.Text = "Bold";
            // 
            // chkFontItalic
            // 
            this.chkFontItalic.AutoSize = true;
            this.chkFontItalic.Location = new System.Drawing.Point(200, 60);
            this.chkFontItalic.Name = "chkFontItalic";
            this.chkFontItalic.Size = new System.Drawing.Size(61, 21);
            this.chkFontItalic.Text = "Italic";
            // 
            // btnFontPreview
            // 
            this.btnFontPreview.Location = new System.Drawing.Point(310, 56);
            this.btnFontPreview.Name = "btnFontPreview";
            this.btnFontPreview.Size = new System.Drawing.Size(130, 30);
            this.btnFontPreview.Text = "Preview Font...";
            this.btnFontPreview.UseVisualStyleBackColor = true;
            this.btnFontPreview.Click += new System.EventHandler(this.btnFontPreview_Click);
            // 
            // grpBehavior
            // 
            this.grpBehavior.Controls.Add(this.lblInitialView);
            this.grpBehavior.Controls.Add(this.cmbInitialView);
            this.grpBehavior.Controls.Add(this.lblSnippetSuffix);
            this.grpBehavior.Controls.Add(this.txtSnippetSuffix);
            this.grpBehavior.Controls.Add(this.lblMaxRecentFiles);
            this.grpBehavior.Controls.Add(this.nudMaxRecentFiles);
            this.grpBehavior.Location = new System.Drawing.Point(12, 512);
            this.grpBehavior.Size = new System.Drawing.Size(500, 115);
            this.grpBehavior.TabStop = false;
            this.grpBehavior.Text = "Behavior";
            // 
            // lblInitialView
            // 
            this.lblInitialView.AutoSize = true;
            this.lblInitialView.Location = new System.Drawing.Point(16, 28);
            this.lblInitialView.Name = "lblInitialView";
            this.lblInitialView.Size = new System.Drawing.Size(82, 17);
            this.lblInitialView.Text = "Initial View:";
            // 
            // cmbInitialView
            // 
            this.cmbInitialView.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbInitialView.FormattingEnabled = true;
            this.cmbInitialView.Location = new System.Drawing.Point(150, 25);
            this.cmbInitialView.Name = "cmbInitialView";
            this.cmbInitialView.Size = new System.Drawing.Size(150, 24);
            // 
            // lblSnippetSuffix
            // 
            this.lblSnippetSuffix.AutoSize = true;
            this.lblSnippetSuffix.Location = new System.Drawing.Point(16, 60);
            this.lblSnippetSuffix.Name = "lblSnippetSuffix";
            this.lblSnippetSuffix.Size = new System.Drawing.Size(106, 17);
            this.lblSnippetSuffix.Text = "Snippet Suffix:";
            // 
            // txtSnippetSuffix
            // 
            this.txtSnippetSuffix.Location = new System.Drawing.Point(150, 57);
            this.txtSnippetSuffix.Name = "txtSnippetSuffix";
            this.txtSnippetSuffix.Size = new System.Drawing.Size(150, 22);
            // 
            // lblMaxRecentFiles
            // 
            this.lblMaxRecentFiles.AutoSize = true;
            this.lblMaxRecentFiles.Location = new System.Drawing.Point(16, 86);
            this.lblMaxRecentFiles.Name = "lblMaxRecentFiles";
            this.lblMaxRecentFiles.Size = new System.Drawing.Size(125, 17);
            this.lblMaxRecentFiles.Text = "Max Recent Files:";
            // 
            // nudMaxRecentFiles
            // 
            this.nudMaxRecentFiles.Location = new System.Drawing.Point(150, 84);
            this.nudMaxRecentFiles.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            this.nudMaxRecentFiles.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            this.nudMaxRecentFiles.Name = "nudMaxRecentFiles";
            this.nudMaxRecentFiles.Size = new System.Drawing.Size(80, 22);
            this.nudMaxRecentFiles.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // 
            // grpPerf
            // 
            this.grpPerf.Controls.Add(this.lblSlowCall);
            this.grpPerf.Controls.Add(this.nudSlowCallMs);
            this.grpPerf.Controls.Add(this.lblMaxFile);
            this.grpPerf.Controls.Add(this.nudMaxFileMb);
            this.grpPerf.Location = new System.Drawing.Point(12, 637);
            this.grpPerf.Size = new System.Drawing.Size(500, 85);
            this.grpPerf.TabStop = false;
            this.grpPerf.Text = "Performance Guards";
            // 
            // lblSlowCall
            // 
            this.lblSlowCall.AutoSize = true;
            this.lblSlowCall.Location = new System.Drawing.Point(16, 28);
            this.lblSlowCall.Name = "lblSlowCall";
            this.lblSlowCall.Size = new System.Drawing.Size(166, 17);
            this.lblSlowCall.Text = "Slow call threshold (ms):";
            // 
            // nudSlowCallMs
            // 
            this.nudSlowCallMs.Location = new System.Drawing.Point(200, 26);
            this.nudSlowCallMs.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
            this.nudSlowCallMs.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            this.nudSlowCallMs.Name = "nudSlowCallMs";
            this.nudSlowCallMs.Size = new System.Drawing.Size(100, 22);
            this.nudSlowCallMs.Value = new decimal(new int[] { 1000, 0, 0, 0 });
            // 
            // lblMaxFile
            // 
            this.lblMaxFile.AutoSize = true;
            this.lblMaxFile.Location = new System.Drawing.Point(16, 56);
            this.lblMaxFile.Name = "lblMaxFile";
            this.lblMaxFile.Size = new System.Drawing.Size(175, 17);
            this.lblMaxFile.Text = "Skip list view if file > (MB):";
            // 
            // nudMaxFileMb
            // 
            this.nudMaxFileMb.Location = new System.Drawing.Point(200, 54);
            this.nudMaxFileMb.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            this.nudMaxFileMb.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.nudMaxFileMb.Name = "nudMaxFileMb";
            this.nudMaxFileMb.Size = new System.Drawing.Size(100, 22);
            this.nudMaxFileMb.Value = new decimal(new int[] { 50, 0, 0, 0 });
            // 
            // OkButton
            // 
            this.OkButton.Location = new System.Drawing.Point(320, 738);
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(90, 35);
            this.OkButton.TabIndex = 100;
            this.OkButton.Text = "&OK";
            this.OkButton.UseVisualStyleBackColor = true;
            this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // CancelBtn
            // 
            this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBtn.Location = new System.Drawing.Point(422, 738);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(90, 35);
            this.CancelBtn.TabIndex = 101;
            this.CancelBtn.Text = "&Cancel";
            this.CancelBtn.UseVisualStyleBackColor = true;
            this.CancelBtn.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.OkButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancelBtn;
            this.ClientSize = new System.Drawing.Size(524, 785);
            this.Controls.Add(this.grpTabs);
            this.Controls.Add(this.grpGrok);
            this.Controls.Add(this.grpAppearance);
            this.Controls.Add(this.grpFont);
            this.Controls.Add(this.grpBehavior);
            this.Controls.Add(this.grpPerf);
            this.Controls.Add(this.OkButton);
            this.Controls.Add(this.CancelBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            ((System.ComponentModel.ISupportInitialize)(this.nudFontSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxRecentFiles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSlowCallMs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxFileMb)).EndInit();
            this.grpTabs.ResumeLayout(false);
            this.grpTabs.PerformLayout();
            this.grpGrok.ResumeLayout(false);
            this.grpGrok.PerformLayout();
            this.grpAppearance.ResumeLayout(false);
            this.grpAppearance.PerformLayout();
            this.grpFont.ResumeLayout(false);
            this.grpFont.PerformLayout();
            this.grpBehavior.ResumeLayout(false);
            this.grpBehavior.PerformLayout();
            this.grpPerf.ResumeLayout(false);
            this.grpPerf.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.GroupBox grpTabs, grpGrok, grpAppearance, grpFont, grpBehavior, grpPerf;
        private System.Windows.Forms.CheckBox chkShowLog, chkShowRaw, chkShowPerformance, chkShowLogDetails;
        private System.Windows.Forms.CheckBox chkShowCallGraph, chkShowFlameGraph, chkShowTimeline;
        private System.Windows.Forms.Label lblGrokUrl, lblClaudeApiKey, lblTheme, lblHighlight, lblSlowCall, lblMaxFile, lblIconSize;
        private System.Windows.Forms.Label lblFontFamily, lblFontSize, lblInitialView, lblSnippetSuffix, lblMaxRecentFiles;
        private System.Windows.Forms.TextBox txtGrokUrl, txtClaudeApiKey, txtSnippetSuffix;
        private System.Windows.Forms.CheckBox chkUseClaudeApi;
        private System.Windows.Forms.ComboBox cmbTheme, cmbHighlightColor, cmbIconSize, cmbFontFamily, cmbInitialView;
        private System.Windows.Forms.Panel panelColorPreview;
        private System.Windows.Forms.NumericUpDown nudSlowCallMs, nudMaxFileMb, nudFontSize, nudMaxRecentFiles;
        private System.Windows.Forms.CheckBox chkShowToolbar, chkFontBold, chkFontItalic;
        private System.Windows.Forms.Button btnFontPreview;
        private System.Windows.Forms.Button OkButton, CancelBtn;
    }
}
