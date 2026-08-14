namespace Cad3PLogBrowser.UI
{
    using System;
    using System.Windows.Forms;
    using Cad3PLogBrowser.Models.Comparison;
    using Cad3PLogBrowser.Services;

    /// <summary>
    /// Dialog for configuring comparison options.
    /// </summary>
    public partial class CompareOptionsDialog : Form
    {
        private CompareOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompareOptionsDialog"/> class.
        /// </summary>
        /// <param name="currentOptions">The current comparison options to edit.</param>
        public CompareOptionsDialog(CompareOptions currentOptions)
        {
            InitializeComponent();
            Icon = AppIcon.Get();

            _options = currentOptions ?? CompareOptions.CreateDefaultLogOptions();
            LoadOptions();

            // G2: never themed — same gap as the parent CompareLogsForm.
            this.Load += (s, e) => ThemeManager.ApplyTheme(this);
        }

        /// <summary>
        /// Gets the configured comparison options.
        /// </summary>
        public CompareOptions Options
        {
            get { return _options; }
        }

        /// <summary>
        /// Loads the current options into the UI controls.
        /// </summary>
        private void LoadOptions()
        {
            chkIgnoreCase.Checked = _options.IgnoreCase;
            chkIgnoreWhitespace.Checked = _options.IgnoreWhitespace;
            chkIgnoreTimestamps.Checked = _options.IgnoreTimestamps;
            chkIgnoreGuids.Checked = _options.IgnoreGuids;
            chkTrimText.Checked = _options.TrimText;
            chkUseRegex.Checked = _options.UseRegexIgnorePatterns;
            txtRegexPattern.Text = _options.RegexIgnorePattern ?? string.Empty;
            txtRegexPattern.Enabled = _options.UseRegexIgnorePatterns;
        }

        /// <summary>
        /// Saves the UI control values back to the options object.
        /// </summary>
        private void SaveOptions()
        {
            _options.IgnoreCase = chkIgnoreCase.Checked;
            _options.IgnoreWhitespace = chkIgnoreWhitespace.Checked;
            _options.IgnoreTimestamps = chkIgnoreTimestamps.Checked;
            _options.IgnoreGuids = chkIgnoreGuids.Checked;
            _options.TrimText = chkTrimText.Checked;
            _options.UseRegexIgnorePatterns = chkUseRegex.Checked;
            _options.RegexIgnorePattern = txtRegexPattern.Text;
        }

        /// <summary>
        /// Handles the OK button click.
        /// </summary>
        private void btnOK_Click(object sender, EventArgs e)
        {
            SaveOptions();
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Handles the Cancel button click.
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Handles the Default button click.
        /// </summary>
        private void btnDefault_Click(object sender, EventArgs e)
        {
            _options = CompareOptions.CreateDefaultLogOptions();
            LoadOptions();
        }

        /// <summary>
        /// Handles the Strict button click.
        /// </summary>
        private void btnStrict_Click(object sender, EventArgs e)
        {
            _options = CompareOptions.CreateStrictOptions();
            LoadOptions();
        }

        /// <summary>
        /// Handles the Use Regex checkbox change.
        /// </summary>
        private void chkUseRegex_CheckedChanged(object sender, EventArgs e)
        {
            txtRegexPattern.Enabled = chkUseRegex.Checked;
        }

        #region Designer Code

        private System.ComponentModel.IContainer components = null;
        private CheckBox chkIgnoreCase;
        private CheckBox chkIgnoreWhitespace;
        private CheckBox chkIgnoreTimestamps;
        private CheckBox chkIgnoreGuids;
        private CheckBox chkTrimText;
        private CheckBox chkUseRegex;
        private TextBox txtRegexPattern;
        private Button btnOK;
        private Button btnCancel;
        private Button btnDefault;
        private Button btnStrict;
        private Label lblRegexPattern;
        private GroupBox grpOptions;
        private GroupBox grpPresets;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.grpOptions = new System.Windows.Forms.GroupBox();
            this.lblRegexPattern = new System.Windows.Forms.Label();
            this.txtRegexPattern = new System.Windows.Forms.TextBox();
            this.chkUseRegex = new System.Windows.Forms.CheckBox();
            this.chkTrimText = new System.Windows.Forms.CheckBox();
            this.chkIgnoreGuids = new System.Windows.Forms.CheckBox();
            this.chkIgnoreTimestamps = new System.Windows.Forms.CheckBox();
            this.chkIgnoreWhitespace = new System.Windows.Forms.CheckBox();
            this.chkIgnoreCase = new System.Windows.Forms.CheckBox();
            this.grpPresets = new System.Windows.Forms.GroupBox();
            this.btnStrict = new System.Windows.Forms.Button();
            this.btnDefault = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.grpOptions.SuspendLayout();
            this.grpPresets.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpOptions
            // 
            this.grpOptions.Controls.Add(this.lblRegexPattern);
            this.grpOptions.Controls.Add(this.txtRegexPattern);
            this.grpOptions.Controls.Add(this.chkUseRegex);
            this.grpOptions.Controls.Add(this.chkTrimText);
            this.grpOptions.Controls.Add(this.chkIgnoreGuids);
            this.grpOptions.Controls.Add(this.chkIgnoreTimestamps);
            this.grpOptions.Controls.Add(this.chkIgnoreWhitespace);
            this.grpOptions.Controls.Add(this.chkIgnoreCase);
            this.grpOptions.Location = new System.Drawing.Point(12, 12);
            this.grpOptions.Name = "grpOptions";
            this.grpOptions.Size = new System.Drawing.Size(460, 220);
            this.grpOptions.TabIndex = 0;
            this.grpOptions.TabStop = false;
            this.grpOptions.Text = "Comparison Options";
            // 
            // lblRegexPattern
            // 
            this.lblRegexPattern.AutoSize = true;
            this.lblRegexPattern.Location = new System.Drawing.Point(40, 180);
            this.lblRegexPattern.Name = "lblRegexPattern";
            this.lblRegexPattern.Size = new System.Drawing.Size(83, 13);
            this.lblRegexPattern.TabIndex = 7;
            this.lblRegexPattern.Text = "Regex Pattern:";
            // 
            // txtRegexPattern
            // 
            this.txtRegexPattern.Location = new System.Drawing.Point(130, 177);
            this.txtRegexPattern.Name = "txtRegexPattern";
            this.txtRegexPattern.Size = new System.Drawing.Size(320, 20);
            this.txtRegexPattern.TabIndex = 6;
            // 
            // chkUseRegex
            // 
            this.chkUseRegex.AutoSize = true;
            this.chkUseRegex.Location = new System.Drawing.Point(20, 155);
            this.chkUseRegex.Name = "chkUseRegex";
            this.chkUseRegex.Size = new System.Drawing.Size(250, 17);
            this.chkUseRegex.TabIndex = 5;
            this.chkUseRegex.Text = "Use custom regex pattern to ignore text";
            this.chkUseRegex.UseVisualStyleBackColor = true;
            this.chkUseRegex.CheckedChanged += new System.EventHandler(this.chkUseRegex_CheckedChanged);
            // 
            // chkTrimText
            // 
            this.chkTrimText.AutoSize = true;
            this.chkTrimText.Location = new System.Drawing.Point(20, 128);
            this.chkTrimText.Name = "chkTrimText";
            this.chkTrimText.Size = new System.Drawing.Size(275, 17);
            this.chkTrimText.TabIndex = 4;
            this.chkTrimText.Text = "Trim leading and trailing whitespace before comparing";
            this.chkTrimText.UseVisualStyleBackColor = true;
            // 
            // chkIgnoreGuids
            // 
            this.chkIgnoreGuids.AutoSize = true;
            this.chkIgnoreGuids.Location = new System.Drawing.Point(20, 101);
            this.chkIgnoreGuids.Name = "chkIgnoreGuids";
            this.chkIgnoreGuids.Size = new System.Drawing.Size(290, 17);
            this.chkIgnoreGuids.TabIndex = 3;
            this.chkIgnoreGuids.Text = "Ignore GUIDs (useful for session IDs, transaction IDs)";
            this.chkIgnoreGuids.UseVisualStyleBackColor = true;
            // 
            // chkIgnoreTimestamps
            // 
            this.chkIgnoreTimestamps.AutoSize = true;
            this.chkIgnoreTimestamps.Checked = true;
            this.chkIgnoreTimestamps.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIgnoreTimestamps.Location = new System.Drawing.Point(20, 74);
            this.chkIgnoreTimestamps.Name = "chkIgnoreTimestamps";
            this.chkIgnoreTimestamps.Size = new System.Drawing.Size(350, 17);
            this.chkIgnoreTimestamps.TabIndex = 2;
            this.chkIgnoreTimestamps.Text = "Ignore timestamps and durations (essential for log file comparison)";
            this.chkIgnoreTimestamps.UseVisualStyleBackColor = true;
            // 
            // chkIgnoreWhitespace
            // 
            this.chkIgnoreWhitespace.AutoSize = true;
            this.chkIgnoreWhitespace.Location = new System.Drawing.Point(20, 47);
            this.chkIgnoreWhitespace.Name = "chkIgnoreWhitespace";
            this.chkIgnoreWhitespace.Size = new System.Drawing.Size(275, 17);
            this.chkIgnoreWhitespace.TabIndex = 1;
            this.chkIgnoreWhitespace.Text = "Ignore whitespace differences (normalize spaces)";
            this.chkIgnoreWhitespace.UseVisualStyleBackColor = true;
            // 
            // chkIgnoreCase
            // 
            this.chkIgnoreCase.AutoSize = true;
            this.chkIgnoreCase.Location = new System.Drawing.Point(20, 20);
            this.chkIgnoreCase.Name = "chkIgnoreCase";
            this.chkIgnoreCase.Size = new System.Drawing.Size(200, 17);
            this.chkIgnoreCase.TabIndex = 0;
            this.chkIgnoreCase.Text = "Ignore case (case-insensitive comparison)";
            this.chkIgnoreCase.UseVisualStyleBackColor = true;
            // 
            // grpPresets
            // 
            this.grpPresets.Controls.Add(this.btnStrict);
            this.grpPresets.Controls.Add(this.btnDefault);
            this.grpPresets.Location = new System.Drawing.Point(12, 238);
            this.grpPresets.Name = "grpPresets";
            this.grpPresets.Size = new System.Drawing.Size(460, 60);
            this.grpPresets.TabIndex = 1;
            this.grpPresets.TabStop = false;
            this.grpPresets.Text = "Presets";
            // 
            // btnStrict
            // 
            this.btnStrict.Location = new System.Drawing.Point(240, 20);
            this.btnStrict.Name = "btnStrict";
            this.btnStrict.Size = new System.Drawing.Size(200, 28);
            this.btnStrict.TabIndex = 1;
            this.btnStrict.Text = "Strict (Consider Everything)";
            this.btnStrict.UseVisualStyleBackColor = true;
            this.btnStrict.Click += new System.EventHandler(this.btnStrict_Click);
            // 
            // btnDefault
            // 
            this.btnDefault.Location = new System.Drawing.Point(20, 20);
            this.btnDefault.Name = "btnDefault";
            this.btnDefault.Size = new System.Drawing.Size(200, 28);
            this.btnDefault.TabIndex = 0;
            this.btnDefault.Text = "Default (Recommended for Logs)";
            this.btnDefault.UseVisualStyleBackColor = true;
            this.btnDefault.Click += new System.EventHandler(this.btnDefault_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(316, 310);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 28);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(397, 310);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 28);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // CompareOptionsDialog
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(484, 350);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.grpPresets);
            this.Controls.Add(this.grpOptions);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CompareOptionsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Comparison Options";
            this.grpOptions.ResumeLayout(false);
            this.grpOptions.PerformLayout();
            this.grpPresets.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion
    }
}
