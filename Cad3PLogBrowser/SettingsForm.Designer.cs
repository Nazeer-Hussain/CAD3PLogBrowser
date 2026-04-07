namespace Cad3PLogBrowser
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.grpTabs           = new System.Windows.Forms.GroupBox();
            this.chkShowLog        = new System.Windows.Forms.CheckBox();
            this.chkShowPerformance = new System.Windows.Forms.CheckBox();
            this.chkShowLogDetails = new System.Windows.Forms.CheckBox();
            this.chkShowCallGraph  = new System.Windows.Forms.CheckBox();
            this.OkButton          = new System.Windows.Forms.Button();
            this.CancelButton      = new System.Windows.Forms.Button();
            this.grpTabs.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpTabs
            // 
            this.grpTabs.Controls.Add(this.chkShowLog);
            this.grpTabs.Controls.Add(this.chkShowPerformance);
            this.grpTabs.Controls.Add(this.chkShowLogDetails);
            this.grpTabs.Controls.Add(this.chkShowCallGraph);
            this.grpTabs.Location = new System.Drawing.Point(12, 12);
            this.grpTabs.Name = "grpTabs";
            this.grpTabs.Size = new System.Drawing.Size(320, 130);
            this.grpTabs.TabIndex = 0;
            this.grpTabs.Text = "Visible Tabs";
            // 
            // chkShowLog
            // 
            this.chkShowLog.AutoSize = true;
            this.chkShowLog.Checked = true;
            this.chkShowLog.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowLog.Location = new System.Drawing.Point(16, 24);
            this.chkShowLog.Name = "chkShowLog";
            this.chkShowLog.Size = new System.Drawing.Size(42, 21);
            this.chkShowLog.TabIndex = 0;
            this.chkShowLog.Text = "Log";
            // 
            // chkShowPerformance
            // 
            this.chkShowPerformance.AutoSize = true;
            this.chkShowPerformance.Checked = true;
            this.chkShowPerformance.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowPerformance.Location = new System.Drawing.Point(16, 52);
            this.chkShowPerformance.Name = "chkShowPerformance";
            this.chkShowPerformance.Size = new System.Drawing.Size(100, 21);
            this.chkShowPerformance.TabIndex = 1;
            this.chkShowPerformance.Text = "Performance";
            // 
            // chkShowLogDetails
            // 
            this.chkShowLogDetails.AutoSize = true;
            this.chkShowLogDetails.Checked = true;
            this.chkShowLogDetails.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowLogDetails.Location = new System.Drawing.Point(16, 80);
            this.chkShowLogDetails.Name = "chkShowLogDetails";
            this.chkShowLogDetails.Size = new System.Drawing.Size(88, 21);
            this.chkShowLogDetails.TabIndex = 2;
            this.chkShowLogDetails.Text = "Log Details";
            // 
            // chkShowCallGraph
            // 
            this.chkShowCallGraph.AutoSize = true;
            this.chkShowCallGraph.Checked = true;
            this.chkShowCallGraph.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowCallGraph.Location = new System.Drawing.Point(180, 24);
            this.chkShowCallGraph.Name = "chkShowCallGraph";
            this.chkShowCallGraph.Size = new System.Drawing.Size(84, 21);
            this.chkShowCallGraph.TabIndex = 3;
            this.chkShowCallGraph.Text = "Call Graph";
            // 
            // OkButton
            // 
            this.OkButton.Location = new System.Drawing.Point(172, 158);
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(75, 30);
            this.OkButton.TabIndex = 1;
            this.OkButton.Text = "&OK";
            this.OkButton.UseVisualStyleBackColor = true;
            this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Location = new System.Drawing.Point(257, 158);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 30);
            this.CancelButton.TabIndex = 2;
            this.CancelButton.Text = "&Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.OkButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancelButton;
            this.ClientSize = new System.Drawing.Size(344, 200);
            this.Controls.Add(this.grpTabs);
            this.Controls.Add(this.OkButton);
            this.Controls.Add(this.CancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.grpTabs.ResumeLayout(false);
            this.grpTabs.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.GroupBox grpTabs;
        private System.Windows.Forms.CheckBox chkShowLog;
        private System.Windows.Forms.CheckBox chkShowPerformance;
        private System.Windows.Forms.CheckBox chkShowLogDetails;
        private System.Windows.Forms.CheckBox chkShowCallGraph;
        private System.Windows.Forms.Button OkButton;
        private System.Windows.Forms.Button CancelButton;
    }
}
