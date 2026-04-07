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
            this.chkShowPerformance = new System.Windows.Forms.CheckBox();
            this.chkShowLogDetails  = new System.Windows.Forms.CheckBox();
            this.chkShowCallGraph   = new System.Windows.Forms.CheckBox();
            this.grpGrok            = new System.Windows.Forms.GroupBox();
            this.lblGrokUrl         = new System.Windows.Forms.Label();
            this.txtGrokUrl         = new System.Windows.Forms.TextBox();
            this.grpAppearance      = new System.Windows.Forms.GroupBox();
            this.lblHighlight       = new System.Windows.Forms.Label();
            this.cmbHighlightColor  = new System.Windows.Forms.ComboBox();
            this.panelColorPreview  = new System.Windows.Forms.Panel();
            this.grpPerf            = new System.Windows.Forms.GroupBox();
            this.lblSlowCall        = new System.Windows.Forms.Label();
            this.nudSlowCallMs      = new System.Windows.Forms.NumericUpDown();
            this.lblMaxFile         = new System.Windows.Forms.Label();
            this.nudMaxFileMb       = new System.Windows.Forms.NumericUpDown();
            this.OkButton           = new System.Windows.Forms.Button();
            this.CancelButton       = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudSlowCallMs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxFileMb)).BeginInit();
            this.grpTabs.SuspendLayout();
            this.grpGrok.SuspendLayout();
            this.grpAppearance.SuspendLayout();
            this.grpPerf.SuspendLayout();
            this.SuspendLayout();
            // grpTabs
            this.grpTabs.Controls.Add(this.chkShowLog);
            this.grpTabs.Controls.Add(this.chkShowPerformance);
            this.grpTabs.Controls.Add(this.chkShowLogDetails);
            this.grpTabs.Controls.Add(this.chkShowCallGraph);
            this.grpTabs.Location = new System.Drawing.Point(12, 12);
            this.grpTabs.Size = new System.Drawing.Size(400, 80);
            this.grpTabs.Text = "Visible Tabs";
            this.chkShowLog.AutoSize = true; this.chkShowLog.Checked = true;
            this.chkShowLog.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowLog.Location = new System.Drawing.Point(16, 24); this.chkShowLog.Text = "Log";
            this.chkShowPerformance.AutoSize = true; this.chkShowPerformance.Checked = true;
            this.chkShowPerformance.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowPerformance.Location = new System.Drawing.Point(16, 50); this.chkShowPerformance.Text = "Performance";
            this.chkShowLogDetails.AutoSize = true; this.chkShowLogDetails.Checked = true;
            this.chkShowLogDetails.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowLogDetails.Location = new System.Drawing.Point(160, 24); this.chkShowLogDetails.Text = "Log Details";
            this.chkShowCallGraph.AutoSize = true; this.chkShowCallGraph.Checked = true;
            this.chkShowCallGraph.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowCallGraph.Location = new System.Drawing.Point(160, 50); this.chkShowCallGraph.Text = "Call Graph";
            // grpGrok
            this.grpGrok.Controls.Add(this.lblGrokUrl);
            this.grpGrok.Controls.Add(this.txtGrokUrl);
            this.grpGrok.Location = new System.Drawing.Point(12, 100);
            this.grpGrok.Size = new System.Drawing.Size(400, 60);
            this.grpGrok.Text = "Grok Integration";
            this.lblGrokUrl.AutoSize = true; this.lblGrokUrl.Location = new System.Drawing.Point(12, 24); this.lblGrokUrl.Text = "Grok URL:";
            this.txtGrokUrl.Location = new System.Drawing.Point(80, 21); this.txtGrokUrl.Size = new System.Drawing.Size(306, 22);
            // grpAppearance
            this.grpAppearance.Controls.Add(this.lblHighlight);
            this.grpAppearance.Controls.Add(this.cmbHighlightColor);
            this.grpAppearance.Controls.Add(this.panelColorPreview);
            this.grpAppearance.Location = new System.Drawing.Point(12, 168);
            this.grpAppearance.Size = new System.Drawing.Size(400, 60);
            this.grpAppearance.Text = "Appearance";
            this.lblHighlight.AutoSize = true; this.lblHighlight.Location = new System.Drawing.Point(12, 24); this.lblHighlight.Text = "Highlight color:";
            this.cmbHighlightColor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHighlightColor.Location = new System.Drawing.Point(108, 21); this.cmbHighlightColor.Size = new System.Drawing.Size(140, 24);
            this.cmbHighlightColor.SelectedIndexChanged += new System.EventHandler(this.cmbHighlightColor_SelectedIndexChanged);
            this.panelColorPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColorPreview.Location = new System.Drawing.Point(258, 21); this.panelColorPreview.Size = new System.Drawing.Size(60, 22);
            // grpPerf
            this.grpPerf.Controls.Add(this.lblSlowCall);
            this.grpPerf.Controls.Add(this.nudSlowCallMs);
            this.grpPerf.Controls.Add(this.lblMaxFile);
            this.grpPerf.Controls.Add(this.nudMaxFileMb);
            this.grpPerf.Location = new System.Drawing.Point(12, 236);
            this.grpPerf.Size = new System.Drawing.Size(400, 70);
            this.grpPerf.Text = "Performance Guards";
            this.lblSlowCall.AutoSize = true; this.lblSlowCall.Location = new System.Drawing.Point(12, 24); this.lblSlowCall.Text = "Slow call threshold (ms):";
            this.nudSlowCallMs.Location = new System.Drawing.Point(168, 22); this.nudSlowCallMs.Size = new System.Drawing.Size(80, 22);
            this.nudSlowCallMs.Minimum = 10; this.nudSlowCallMs.Maximum = 60000; this.nudSlowCallMs.Value = 1000;
            this.lblMaxFile.AutoSize = true; this.lblMaxFile.Location = new System.Drawing.Point(12, 48); this.lblMaxFile.Text = "Skip list view if file > (MB):";
            this.nudMaxFileMb.Location = new System.Drawing.Point(200, 46); this.nudMaxFileMb.Size = new System.Drawing.Size(80, 22);
            this.nudMaxFileMb.Minimum = 1; this.nudMaxFileMb.Maximum = 2000; this.nudMaxFileMb.Value = 50;
            // Buttons
            this.OkButton.Location = new System.Drawing.Point(248, 318); this.OkButton.Size = new System.Drawing.Size(75, 30);
            this.OkButton.Text = "&OK"; this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Location = new System.Drawing.Point(337, 318); this.CancelButton.Size = new System.Drawing.Size(75, 30);
            this.CancelButton.Text = "&Cancel"; this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // Form
            this.AcceptButton = this.OkButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(424, 360);
            this.Controls.Add(this.grpTabs); this.Controls.Add(this.grpGrok);
            this.Controls.Add(this.grpAppearance); this.Controls.Add(this.grpPerf);
            this.Controls.Add(this.OkButton); this.Controls.Add(this.CancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false;
            this.Name = "SettingsForm"; this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            ((System.ComponentModel.ISupportInitialize)(this.nudSlowCallMs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxFileMb)).EndInit();
            this.grpTabs.ResumeLayout(false); this.grpTabs.PerformLayout();
            this.grpGrok.ResumeLayout(false); this.grpGrok.PerformLayout();
            this.grpAppearance.ResumeLayout(false); this.grpAppearance.PerformLayout();
            this.grpPerf.ResumeLayout(false); this.grpPerf.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.GroupBox grpTabs, grpGrok, grpAppearance, grpPerf;
        private System.Windows.Forms.CheckBox chkShowLog, chkShowPerformance, chkShowLogDetails, chkShowCallGraph;
        private System.Windows.Forms.Label lblGrokUrl, lblHighlight, lblSlowCall, lblMaxFile;
        private System.Windows.Forms.TextBox txtGrokUrl;
        private System.Windows.Forms.ComboBox cmbHighlightColor;
        private System.Windows.Forms.Panel panelColorPreview;
        private System.Windows.Forms.NumericUpDown nudSlowCallMs, nudMaxFileMb;
        private System.Windows.Forms.Button OkButton, CancelButton;
    }
}
