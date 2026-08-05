namespace Cad3PLogBrowser
{
    partial class FilterForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.FilterTextBox = new System.Windows.Forms.ComboBox();
            this.MatchCaseCheckBox = new System.Windows.Forms.CheckBox();
            this.grpDuration = new System.Windows.Forms.GroupBox();
            this.chkEnableDuration = new System.Windows.Forms.CheckBox();
            this.lblMinDuration = new System.Windows.Forms.Label();
            this.nudMinDuration = new System.Windows.Forms.NumericUpDown();
            this.grpTimeRange = new System.Windows.Forms.GroupBox();
            this.chkEnableTimeRange = new System.Windows.Forms.CheckBox();
            this.lblFromTime = new System.Windows.Forms.Label();
            this.dtpFromTime = new System.Windows.Forms.DateTimePicker();
            this.lblToTime = new System.Windows.Forms.Label();
            this.dtpToTime = new System.Windows.Forms.DateTimePicker();
            this.grpThreadLevel = new System.Windows.Forms.GroupBox();
            this.lblThreadId = new System.Windows.Forms.Label();
            this.cmbThreadId = new System.Windows.Forms.ComboBox();
            this.lblLogLevel = new System.Windows.Forms.Label();
            this.chkLevelDebug = new System.Windows.Forms.CheckBox();
            this.chkLevelInfo = new System.Windows.Forms.CheckBox();
            this.chkLevelWarning = new System.Windows.Forms.CheckBox();
            this.chkLevelError = new System.Windows.Forms.CheckBox();
            this.ApplyButton = new System.Windows.Forms.Button();
            this.ClearButton = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            this.grpDuration.SuspendLayout();
            this.grpTimeRange.SuspendLayout();
            this.grpThreadLevel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinDuration)).BeginInit();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(160, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = UI.AppStrings.FilterLabelSearchText;
            // 
            // FilterTextBox
            // 
            this.FilterTextBox.FormattingEnabled = true;
            this.FilterTextBox.Location = new System.Drawing.Point(15, 37);
            this.FilterTextBox.Name = "FilterTextBox";
            this.FilterTextBox.Size = new System.Drawing.Size(510, 24);
            this.FilterTextBox.TabIndex = 1;
            // 
            // MatchCaseCheckBox
            // 
            this.MatchCaseCheckBox.AutoSize = true;
            this.MatchCaseCheckBox.Location = new System.Drawing.Point(15, 72);
            this.MatchCaseCheckBox.Name = "MatchCaseCheckBox";
            this.MatchCaseCheckBox.Size = new System.Drawing.Size(100, 21);
            this.MatchCaseCheckBox.TabIndex = 2;
            this.MatchCaseCheckBox.Text = UI.AppStrings.FilterCheckMatchCase;
            // 
            // grpDuration
            // 
            this.grpDuration.Controls.Add(this.chkEnableDuration);
            this.grpDuration.Controls.Add(this.lblMinDuration);
            this.grpDuration.Controls.Add(this.nudMinDuration);
            this.grpDuration.Location = new System.Drawing.Point(15, 108);
            this.grpDuration.Name = "grpDuration";
            this.grpDuration.Size = new System.Drawing.Size(250, 90);
            this.grpDuration.TabIndex = 3;
            this.grpDuration.TabStop = false;
            this.grpDuration.Text = "Duration Filter";
            // 
            // chkEnableDuration
            // 
            this.chkEnableDuration.AutoSize = true;
            this.chkEnableDuration.Location = new System.Drawing.Point(15, 25);
            this.chkEnableDuration.Name = "chkEnableDuration";
            this.chkEnableDuration.Size = new System.Drawing.Size(70, 21);
            this.chkEnableDuration.TabIndex = 0;
            this.chkEnableDuration.Text = "Enable";
            // 
            // lblMinDuration
            // 
            this.lblMinDuration.AutoSize = true;
            this.lblMinDuration.Location = new System.Drawing.Point(15, 56);
            this.lblMinDuration.Name = "lblMinDuration";
            this.lblMinDuration.Size = new System.Drawing.Size(130, 17);
            this.lblMinDuration.TabIndex = 1;
            this.lblMinDuration.Text = "Min duration (ms):";
            // 
            // nudMinDuration
            // 
            this.nudMinDuration.Location = new System.Drawing.Point(150, 54);
            this.nudMinDuration.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            this.nudMinDuration.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            this.nudMinDuration.Name = "nudMinDuration";
            this.nudMinDuration.Size = new System.Drawing.Size(85, 22);
            this.nudMinDuration.TabIndex = 2;
            this.nudMinDuration.Value = new decimal(new int[] { 1000, 0, 0, 0 });
            // 
            // grpTimeRange
            // 
            this.grpTimeRange.Controls.Add(this.chkEnableTimeRange);
            this.grpTimeRange.Controls.Add(this.lblFromTime);
            this.grpTimeRange.Controls.Add(this.dtpFromTime);
            this.grpTimeRange.Controls.Add(this.lblToTime);
            this.grpTimeRange.Controls.Add(this.dtpToTime);
            this.grpTimeRange.Location = new System.Drawing.Point(275, 108);
            this.grpTimeRange.Name = "grpTimeRange";
            this.grpTimeRange.Size = new System.Drawing.Size(250, 135);
            this.grpTimeRange.TabIndex = 4;
            this.grpTimeRange.TabStop = false;
            this.grpTimeRange.Text = "Time Range Filter";
            // 
            // chkEnableTimeRange
            // 
            this.chkEnableTimeRange.AutoSize = true;
            this.chkEnableTimeRange.Location = new System.Drawing.Point(15, 25);
            this.chkEnableTimeRange.Name = "chkEnableTimeRange";
            this.chkEnableTimeRange.Size = new System.Drawing.Size(70, 21);
            this.chkEnableTimeRange.TabIndex = 0;
            this.chkEnableTimeRange.Text = "Enable";
            // 
            // lblFromTime
            // 
            this.lblFromTime.AutoSize = true;
            this.lblFromTime.Location = new System.Drawing.Point(15, 56);
            this.lblFromTime.Name = "lblFromTime";
            this.lblFromTime.Size = new System.Drawing.Size(42, 17);
            this.lblFromTime.TabIndex = 1;
            this.lblFromTime.Text = "From:";
            // 
            // dtpFromTime
            // 
            this.dtpFromTime.CustomFormat = "HH:mm:ss";
            this.dtpFromTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpFromTime.Location = new System.Drawing.Point(65, 54);
            this.dtpFromTime.Name = "dtpFromTime";
            this.dtpFromTime.ShowUpDown = true;
            this.dtpFromTime.Size = new System.Drawing.Size(170, 22);
            this.dtpFromTime.TabIndex = 2;
            // 
            // lblToTime
            // 
            this.lblToTime.AutoSize = true;
            this.lblToTime.Location = new System.Drawing.Point(15, 90);
            this.lblToTime.Name = "lblToTime";
            this.lblToTime.Size = new System.Drawing.Size(28, 17);
            this.lblToTime.TabIndex = 3;
            this.lblToTime.Text = "To:";
            // 
            // dtpToTime
            // 
            this.dtpToTime.CustomFormat = "HH:mm:ss";
            this.dtpToTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpToTime.Location = new System.Drawing.Point(65, 88);
            this.dtpToTime.Name = "dtpToTime";
            this.dtpToTime.ShowUpDown = true;
            this.dtpToTime.Size = new System.Drawing.Size(170, 22);
            this.dtpToTime.TabIndex = 4;
            // 
            // grpThreadLevel
            // 
            this.grpThreadLevel.Controls.Add(this.lblThreadId);
            this.grpThreadLevel.Controls.Add(this.cmbThreadId);
            this.grpThreadLevel.Controls.Add(this.lblLogLevel);
            this.grpThreadLevel.Controls.Add(this.chkLevelDebug);
            this.grpThreadLevel.Controls.Add(this.chkLevelInfo);
            this.grpThreadLevel.Controls.Add(this.chkLevelWarning);
            this.grpThreadLevel.Controls.Add(this.chkLevelError);
            this.grpThreadLevel.Location = new System.Drawing.Point(15, 253);
            this.grpThreadLevel.Name = "grpThreadLevel";
            this.grpThreadLevel.Size = new System.Drawing.Size(510, 95);
            this.grpThreadLevel.TabIndex = 5;
            this.grpThreadLevel.TabStop = false;
            this.grpThreadLevel.Text = "Thread && Level Filter";
            // 
            // lblThreadId
            // 
            this.lblThreadId.AutoSize = true;
            this.lblThreadId.Location = new System.Drawing.Point(15, 30);
            this.lblThreadId.Name = "lblThreadId";
            this.lblThreadId.Size = new System.Drawing.Size(75, 17);
            this.lblThreadId.TabIndex = 0;
            this.lblThreadId.Text = UI.AppStrings.FilterLabelThreadId;
            // 
            // cmbThreadId
            //
            this.cmbThreadId.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cmbThreadId.Location = new System.Drawing.Point(105, 27);
            this.cmbThreadId.Name = "cmbThreadId";
            this.cmbThreadId.Size = new System.Drawing.Size(180, 23);
            this.cmbThreadId.TabIndex = 1;
            // 
            // lblLogLevel
            // 
            this.lblLogLevel.AutoSize = true;
            this.lblLogLevel.Location = new System.Drawing.Point(15, 60);
            this.lblLogLevel.Name = "lblLogLevel";
            this.lblLogLevel.Size = new System.Drawing.Size(75, 17);
            this.lblLogLevel.TabIndex = 2;
            this.lblLogLevel.Text = UI.AppStrings.FilterLabelLogLevel;
            // 
            // chkLevelDebug
            //
            // B6: independent checkboxes replace the single-select dropdown so any
            // combination of levels (e.g. Warning + Error) can be shown at once.
            this.chkLevelDebug.AutoSize = true;
            this.chkLevelDebug.Location = new System.Drawing.Point(105, 58);
            this.chkLevelDebug.Name = "chkLevelDebug";
            this.chkLevelDebug.Size = new System.Drawing.Size(70, 21);
            this.chkLevelDebug.TabIndex = 3;
            this.chkLevelDebug.Text = "Debug";
            //
            // chkLevelInfo
            //
            this.chkLevelInfo.AutoSize = true;
            this.chkLevelInfo.Location = new System.Drawing.Point(180, 58);
            this.chkLevelInfo.Name = "chkLevelInfo";
            this.chkLevelInfo.Size = new System.Drawing.Size(60, 21);
            this.chkLevelInfo.TabIndex = 4;
            this.chkLevelInfo.Text = "Info";
            //
            // chkLevelWarning
            //
            this.chkLevelWarning.AutoSize = true;
            this.chkLevelWarning.Location = new System.Drawing.Point(250, 58);
            this.chkLevelWarning.Name = "chkLevelWarning";
            this.chkLevelWarning.Size = new System.Drawing.Size(85, 21);
            this.chkLevelWarning.TabIndex = 5;
            this.chkLevelWarning.Text = "Warning";
            //
            // chkLevelError
            //
            this.chkLevelError.AutoSize = true;
            this.chkLevelError.Location = new System.Drawing.Point(345, 58);
            this.chkLevelError.Name = "chkLevelError";
            this.chkLevelError.Size = new System.Drawing.Size(70, 21);
            this.chkLevelError.TabIndex = 6;
            this.chkLevelError.Text = "Error";
            //
            // ApplyButton
            // 
            this.ApplyButton.Location = new System.Drawing.Point(217, 363);
            this.ApplyButton.Name = "ApplyButton";
            this.ApplyButton.Size = new System.Drawing.Size(95, 35);
            this.ApplyButton.TabIndex = 6;
            this.ApplyButton.Text = UI.AppStrings.FilterButtonApply;
            this.ApplyButton.UseVisualStyleBackColor = true;
            this.ApplyButton.Click += new System.EventHandler(this.ApplyButton_Click);
            // 
            // ClearButton
            // 
            this.ClearButton.Location = new System.Drawing.Point(323, 363);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new System.Drawing.Size(95, 35);
            this.ClearButton.TabIndex = 7;
            this.ClearButton.Text = UI.AppStrings.FilterButtonClear;
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Location = new System.Drawing.Point(429, 363);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(95, 35);
            this.buttonClose.TabIndex = 8;
            this.buttonClose.Text = UI.AppStrings.FilterButtonClose;
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // FilterForm
            // 
            this.AcceptButton = this.ApplyButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(540, 410);
            this.Controls.Add(this.grpThreadLevel);
            this.Controls.Add(this.grpTimeRange);
            this.Controls.Add(this.grpDuration);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.ClearButton);
            this.Controls.Add(this.ApplyButton);
            this.Controls.Add(this.MatchCaseCheckBox);
            this.Controls.Add(this.FilterTextBox);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FilterForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = UI.AppStrings.FilterFormTitle;
            this.Load += new System.EventHandler(this.FilterForm_Load);
            this.grpDuration.ResumeLayout(false);
            this.grpDuration.PerformLayout();
            this.grpTimeRange.ResumeLayout(false);
            this.grpTimeRange.PerformLayout();
            this.grpThreadLevel.ResumeLayout(false);
            this.grpThreadLevel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinDuration)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox FilterTextBox;
        private System.Windows.Forms.CheckBox MatchCaseCheckBox;
        private System.Windows.Forms.GroupBox grpDuration;
        private System.Windows.Forms.CheckBox chkEnableDuration;
        private System.Windows.Forms.Label lblMinDuration;
        private System.Windows.Forms.NumericUpDown nudMinDuration;
        private System.Windows.Forms.GroupBox grpTimeRange;
        private System.Windows.Forms.CheckBox chkEnableTimeRange;
        private System.Windows.Forms.Label lblFromTime;
        private System.Windows.Forms.DateTimePicker dtpFromTime;
        private System.Windows.Forms.Label lblToTime;
        private System.Windows.Forms.DateTimePicker dtpToTime;
        private System.Windows.Forms.GroupBox grpThreadLevel;
        private System.Windows.Forms.Label lblThreadId;
        private System.Windows.Forms.ComboBox cmbThreadId;
        private System.Windows.Forms.Label lblLogLevel;
        private System.Windows.Forms.CheckBox chkLevelDebug;
        private System.Windows.Forms.CheckBox chkLevelInfo;
        private System.Windows.Forms.CheckBox chkLevelWarning;
        private System.Windows.Forms.CheckBox chkLevelError;
        private System.Windows.Forms.Button ApplyButton;
        private System.Windows.Forms.Button ClearButton;
        private System.Windows.Forms.Button buttonClose;
    }
}
