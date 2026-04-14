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
            this.lblTheme           = new System.Windows.Forms.Label();
            this.cmbTheme           = new System.Windows.Forms.ComboBox();
            this.lblHighlight       = new System.Windows.Forms.Label();
            this.cmbHighlightColor  = new System.Windows.Forms.ComboBox();
            this.panelColorPreview  = new System.Windows.Forms.Panel();
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

            // ── Layout constants ──────────────────────────────────────────────
            // Form: ClientSize 544 × 800
            // Groups: x=12, width=520, inner padding x=14
            // Label column: x=14, width=140  (right-aligned text, anchored)
            // Control column: x=162, width=340
            // Row height: 30 px  (label/control height 23 px, gap 7 px)
            // Group title row offset: 24 px from group top
            // Groups stacked with 10 px gap

            // ── grpTabs  (y=10, h=110) ────────────────────────────────────────
            this.grpTabs.Controls.Add(this.chkShowLog);
            this.grpTabs.Controls.Add(this.chkShowRaw);
            this.grpTabs.Controls.Add(this.chkShowPerformance);
            this.grpTabs.Controls.Add(this.chkShowLogDetails);
            this.grpTabs.Controls.Add(this.chkShowCallGraph);
            this.grpTabs.Controls.Add(this.chkShowFlameGraph);
            this.grpTabs.Controls.Add(this.chkShowTimeline);
            this.grpTabs.Location  = new System.Drawing.Point(12, 10);
            this.grpTabs.Size      = new System.Drawing.Size(520, 100);
            this.grpTabs.TabStop   = false;
            this.grpTabs.Text      = "Visible Tabs";

            // Row 1 of checkboxes — y=26
            this.chkShowLog.AutoSize  = true;
            this.chkShowLog.Checked   = true;
            this.chkShowLog.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowLog.Location  = new System.Drawing.Point(16, 26);
            this.chkShowLog.Text      = "Log View";

            this.chkShowRaw.AutoSize  = true;
            this.chkShowRaw.Checked   = true;
            this.chkShowRaw.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowRaw.Location  = new System.Drawing.Point(110, 26);
            this.chkShowRaw.Text      = "Raw";

            this.chkShowPerformance.AutoSize  = true;
            this.chkShowPerformance.Checked   = true;
            this.chkShowPerformance.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowPerformance.Location  = new System.Drawing.Point(190, 26);
            this.chkShowPerformance.Text      = "Performance";

            this.chkShowLogDetails.AutoSize  = true;
            this.chkShowLogDetails.Checked   = true;
            this.chkShowLogDetails.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowLogDetails.Location  = new System.Drawing.Point(310, 26);
            this.chkShowLogDetails.Text      = "Log Details";

            // Row 2 of checkboxes — y=58
            this.chkShowCallGraph.AutoSize  = true;
            this.chkShowCallGraph.Checked   = true;
            this.chkShowCallGraph.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowCallGraph.Location  = new System.Drawing.Point(16, 58);
            this.chkShowCallGraph.Text      = "Call Graph";

            this.chkShowFlameGraph.AutoSize  = true;
            this.chkShowFlameGraph.Checked   = true;
            this.chkShowFlameGraph.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowFlameGraph.Location  = new System.Drawing.Point(126, 58);
            this.chkShowFlameGraph.Text      = "Flame Graph";

            this.chkShowTimeline.AutoSize  = true;
            this.chkShowTimeline.Checked   = true;
            this.chkShowTimeline.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowTimeline.Location  = new System.Drawing.Point(248, 58);
            this.chkShowTimeline.Text      = "Timeline";

            // ── grpGrok  (y=120, h=115) ───────────────────────────────────────
            this.grpGrok.Controls.Add(this.lblGrokUrl);
            this.grpGrok.Controls.Add(this.txtGrokUrl);
            this.grpGrok.Controls.Add(this.lblClaudeApiKey);
            this.grpGrok.Controls.Add(this.txtClaudeApiKey);
            this.grpGrok.Controls.Add(this.chkUseClaudeApi);
            this.grpGrok.Location  = new System.Drawing.Point(12, 120);
            this.grpGrok.Size      = new System.Drawing.Size(520, 115);
            this.grpGrok.TabStop   = false;
            this.grpGrok.Text      = "External Integration";

            this.lblGrokUrl.AutoSize  = true;
            this.lblGrokUrl.Location  = new System.Drawing.Point(14, 28);
            this.lblGrokUrl.Size      = new System.Drawing.Size(140, 17);
            this.lblGrokUrl.Text      = "Grok URL:";
            this.lblGrokUrl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.txtGrokUrl.Location  = new System.Drawing.Point(162, 25);
            this.txtGrokUrl.Size      = new System.Drawing.Size(342, 23);

            this.lblClaudeApiKey.AutoSize  = true;
            this.lblClaudeApiKey.Location  = new System.Drawing.Point(14, 58);
            this.lblClaudeApiKey.Text      = "Claude API Key:";
            this.lblClaudeApiKey.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.txtClaudeApiKey.Location            = new System.Drawing.Point(162, 55);
            this.txtClaudeApiKey.Size                = new System.Drawing.Size(342, 23);
            this.txtClaudeApiKey.UseSystemPasswordChar = true;

            this.chkUseClaudeApi.AutoSize = true;
            this.chkUseClaudeApi.Location = new System.Drawing.Point(162, 84);
            this.chkUseClaudeApi.Text     = "Enable Claude API (requires key above)";
            this.chkUseClaudeApi.Font     = new System.Drawing.Font("Segoe UI", 8.5f);

            // ── grpAppearance  (y=245, h=140) ─────────────────────────────────
            this.grpAppearance.Controls.Add(this.lblTheme);
            this.grpAppearance.Controls.Add(this.cmbTheme);
            this.grpAppearance.Controls.Add(this.lblHighlight);
            this.grpAppearance.Controls.Add(this.cmbHighlightColor);
            this.grpAppearance.Controls.Add(this.panelColorPreview);
            this.grpAppearance.Controls.Add(this.lblIconSize);
            this.grpAppearance.Controls.Add(this.cmbIconSize);
            this.grpAppearance.Controls.Add(this.chkShowToolbar);
            this.grpAppearance.Location  = new System.Drawing.Point(12, 245);
            this.grpAppearance.Size      = new System.Drawing.Size(520, 140);
            this.grpAppearance.TabStop   = false;
            this.grpAppearance.Text      = "Appearance";

            // Row 1 — Theme  y=26
            this.lblTheme.AutoSize  = true;
            this.lblTheme.Location  = new System.Drawing.Point(14, 28);
            this.lblTheme.Text      = "Theme:";
            this.lblTheme.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.cmbTheme.DropDownStyle    = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTheme.FormattingEnabled = true;
            this.cmbTheme.Location         = new System.Drawing.Point(162, 25);
            this.cmbTheme.Size             = new System.Drawing.Size(160, 24);

            // Row 2 — Highlight colour  y=58
            this.lblHighlight.AutoSize  = true;
            this.lblHighlight.Location  = new System.Drawing.Point(14, 60);
            this.lblHighlight.Text      = "Highlight colour:";
            this.lblHighlight.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.cmbHighlightColor.DropDownStyle    = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHighlightColor.FormattingEnabled = true;
            this.cmbHighlightColor.Location         = new System.Drawing.Point(162, 57);
            this.cmbHighlightColor.Size             = new System.Drawing.Size(160, 24);
            this.cmbHighlightColor.SelectedIndexChanged += new System.EventHandler(this.cmbHighlightColor_SelectedIndexChanged);

            this.panelColorPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColorPreview.Location    = new System.Drawing.Point(332, 57);
            this.panelColorPreview.Size        = new System.Drawing.Size(60, 24);

            // Row 3 — Icon size + Show toolbar  y=90
            this.lblIconSize.AutoSize  = true;
            this.lblIconSize.Location  = new System.Drawing.Point(14, 92);
            this.lblIconSize.Text      = "Toolbar icon size:";
            this.lblIconSize.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.cmbIconSize.DropDownStyle    = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbIconSize.FormattingEnabled = true;
            this.cmbIconSize.Location         = new System.Drawing.Point(162, 89);
            this.cmbIconSize.Size             = new System.Drawing.Size(120, 24);

            this.chkShowToolbar.AutoSize  = true;
            this.chkShowToolbar.Checked   = true;
            this.chkShowToolbar.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowToolbar.Location  = new System.Drawing.Point(300, 91);
            this.chkShowToolbar.Text      = "Show Toolbar";

            // ── grpFont  (y=395, h=110) ───────────────────────────────────────
            this.grpFont.Controls.Add(this.lblFontFamily);
            this.grpFont.Controls.Add(this.cmbFontFamily);
            this.grpFont.Controls.Add(this.lblFontSize);
            this.grpFont.Controls.Add(this.nudFontSize);
            this.grpFont.Controls.Add(this.chkFontBold);
            this.grpFont.Controls.Add(this.chkFontItalic);
            this.grpFont.Controls.Add(this.btnFontPreview);
            this.grpFont.Location  = new System.Drawing.Point(12, 395);
            this.grpFont.Size      = new System.Drawing.Size(520, 110);
            this.grpFont.TabStop   = false;
            this.grpFont.Text      = "Log Font";

            // Row 1 — Font family + size  y=26
            this.lblFontFamily.AutoSize  = true;
            this.lblFontFamily.Location  = new System.Drawing.Point(14, 28);
            this.lblFontFamily.Text      = "Font family:";
            this.lblFontFamily.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.cmbFontFamily.DropDownStyle    = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFontFamily.FormattingEnabled = true;
            this.cmbFontFamily.Location         = new System.Drawing.Point(162, 25);
            this.cmbFontFamily.Size             = new System.Drawing.Size(180, 24);

            this.lblFontSize.AutoSize  = true;
            this.lblFontSize.Location  = new System.Drawing.Point(352, 28);
            this.lblFontSize.Text      = "Size:";
            this.lblFontSize.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.nudFontSize.DecimalPlaces = 1;
            this.nudFontSize.Increment     = new decimal(new int[] { 5, 0, 0, 65536 });
            this.nudFontSize.Location      = new System.Drawing.Point(392, 25);
            this.nudFontSize.Maximum       = new decimal(new int[] { 24, 0, 0, 0 });
            this.nudFontSize.Minimum       = new decimal(new int[] { 6, 0, 0, 0 });
            this.nudFontSize.Size          = new System.Drawing.Size(72, 23);
            this.nudFontSize.Value         = new decimal(new int[] { 9, 0, 0, 0 });

            // Row 2 — Style + preview button  y=58
            this.chkFontBold.AutoSize = true;
            this.chkFontBold.Location = new System.Drawing.Point(162, 60);
            this.chkFontBold.Text     = "Bold";

            this.chkFontItalic.AutoSize = true;
            this.chkFontItalic.Location = new System.Drawing.Point(232, 60);
            this.chkFontItalic.Text     = "Italic";

            this.btnFontPreview.Location             = new System.Drawing.Point(310, 56);
            this.btnFontPreview.Size                 = new System.Drawing.Size(154, 28);
            this.btnFontPreview.Text                 = "Preview Font...";
            this.btnFontPreview.UseVisualStyleBackColor = true;
            this.btnFontPreview.Click += new System.EventHandler(this.btnFontPreview_Click);

            // ── grpBehavior  (y=515, h=120) ───────────────────────────────────
            this.grpBehavior.Controls.Add(this.lblInitialView);
            this.grpBehavior.Controls.Add(this.cmbInitialView);
            this.grpBehavior.Controls.Add(this.lblSnippetSuffix);
            this.grpBehavior.Controls.Add(this.txtSnippetSuffix);
            this.grpBehavior.Controls.Add(this.lblMaxRecentFiles);
            this.grpBehavior.Controls.Add(this.nudMaxRecentFiles);
            this.grpBehavior.Location  = new System.Drawing.Point(12, 515);
            this.grpBehavior.Size      = new System.Drawing.Size(520, 120);
            this.grpBehavior.TabStop   = false;
            this.grpBehavior.Text      = "Behavior";

            // Row 1 — Initial view  y=26
            this.lblInitialView.AutoSize  = true;
            this.lblInitialView.Location  = new System.Drawing.Point(14, 28);
            this.lblInitialView.Text      = "Initial view:";
            this.lblInitialView.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.cmbInitialView.DropDownStyle    = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbInitialView.FormattingEnabled = true;
            this.cmbInitialView.Location         = new System.Drawing.Point(162, 25);
            this.cmbInitialView.Size             = new System.Drawing.Size(160, 24);

            // Row 2 — Snippet suffix  y=58
            this.lblSnippetSuffix.AutoSize  = true;
            this.lblSnippetSuffix.Location  = new System.Drawing.Point(14, 60);
            this.lblSnippetSuffix.Text      = "Snippet suffix:";
            this.lblSnippetSuffix.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.txtSnippetSuffix.Location = new System.Drawing.Point(162, 57);
            this.txtSnippetSuffix.Size     = new System.Drawing.Size(160, 23);

            // Row 3 — Max recent files  y=88
            this.lblMaxRecentFiles.AutoSize  = true;
            this.lblMaxRecentFiles.Location  = new System.Drawing.Point(14, 90);
            this.lblMaxRecentFiles.Text      = "Max recent files:";
            this.lblMaxRecentFiles.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.nudMaxRecentFiles.Location = new System.Drawing.Point(162, 87);
            this.nudMaxRecentFiles.Maximum  = new decimal(new int[] { 20, 0, 0, 0 });
            this.nudMaxRecentFiles.Minimum  = new decimal(new int[] { 5,  0, 0, 0 });
            this.nudMaxRecentFiles.Size     = new System.Drawing.Size(80, 23);
            this.nudMaxRecentFiles.Value    = new decimal(new int[] { 10, 0, 0, 0 });

            // ── grpPerf  (y=645, h=92) ────────────────────────────────────────
            this.grpPerf.Controls.Add(this.lblSlowCall);
            this.grpPerf.Controls.Add(this.nudSlowCallMs);
            this.grpPerf.Controls.Add(this.lblMaxFile);
            this.grpPerf.Controls.Add(this.nudMaxFileMb);
            this.grpPerf.Location  = new System.Drawing.Point(12, 645);
            this.grpPerf.Size      = new System.Drawing.Size(520, 92);
            this.grpPerf.TabStop   = false;
            this.grpPerf.Text      = "Performance Guards";

            // Row 1 — Slow call  y=26
            this.lblSlowCall.AutoSize  = true;
            this.lblSlowCall.Location  = new System.Drawing.Point(14, 28);
            this.lblSlowCall.Text      = "Slow call threshold (ms):";
            this.lblSlowCall.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.nudSlowCallMs.Location = new System.Drawing.Point(202, 25);
            this.nudSlowCallMs.Maximum  = new decimal(new int[] { 60000, 0, 0, 0 });
            this.nudSlowCallMs.Minimum  = new decimal(new int[] { 10,    0, 0, 0 });
            this.nudSlowCallMs.Size     = new System.Drawing.Size(100, 23);
            this.nudSlowCallMs.Value    = new decimal(new int[] { 1000,  0, 0, 0 });

            // Row 2 — Max file size  y=56
            this.lblMaxFile.AutoSize  = true;
            this.lblMaxFile.Location  = new System.Drawing.Point(14, 58);
            this.lblMaxFile.Text      = "Skip list view if file > (MB):";
            this.lblMaxFile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.nudMaxFileMb.Location = new System.Drawing.Point(202, 55);
            this.nudMaxFileMb.Maximum  = new decimal(new int[] { 2000, 0, 0, 0 });
            this.nudMaxFileMb.Minimum  = new decimal(new int[] { 1,    0, 0, 0 });
            this.nudMaxFileMb.Size     = new System.Drawing.Size(100, 23);
            this.nudMaxFileMb.Value    = new decimal(new int[] { 50,   0, 0, 0 });

            // ── OK / Cancel buttons  (y=751) ──────────────────────────────────
            this.OkButton.Location             = new System.Drawing.Point(336, 751);
            this.OkButton.Name                 = "OkButton";
            this.OkButton.Size                 = new System.Drawing.Size(90, 32);
            this.OkButton.TabIndex             = 100;
            this.OkButton.Text                 = "&OK";
            this.OkButton.UseVisualStyleBackColor = true;
            this.OkButton.Click += new System.EventHandler(this.OkButton_Click);

            this.CancelBtn.DialogResult        = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBtn.Location            = new System.Drawing.Point(442, 751);
            this.CancelBtn.Name                = "CancelBtn";
            this.CancelBtn.Size                = new System.Drawing.Size(90, 32);
            this.CancelBtn.TabIndex            = 101;
            this.CancelBtn.Text                = "&Cancel";
            this.CancelBtn.UseVisualStyleBackColor = true;
            this.CancelBtn.Click += new System.EventHandler(this.CancelButton_Click);

            // ── Form ──────────────────────────────────────────────────────────
            this.AcceptButton       = this.OkButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode      = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton       = this.CancelBtn;
            this.ClientSize         = new System.Drawing.Size(544, 795);
            this.Controls.Add(this.grpTabs);
            this.Controls.Add(this.grpGrok);
            this.Controls.Add(this.grpAppearance);
            this.Controls.Add(this.grpFont);
            this.Controls.Add(this.grpBehavior);
            this.Controls.Add(this.grpPerf);
            this.Controls.Add(this.OkButton);
            this.Controls.Add(this.CancelBtn);
            this.FormBorderStyle    = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox        = false;
            this.MinimizeBox        = false;
            this.Name               = "SettingsForm";
            this.Padding            = new System.Windows.Forms.Padding(12, 10, 12, 10);
            this.ShowInTaskbar      = false;
            this.StartPosition      = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text               = "Settings";
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

