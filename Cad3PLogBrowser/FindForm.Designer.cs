namespace Cad3PLogBrowser
{
    partial class FindForm
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
            this.SearchTextBox = new System.Windows.Forms.ComboBox();
            this.MatchCaseCheckBox = new System.Windows.Forms.CheckBox();
            this.UseRegexCheckBox = new System.Windows.Forms.CheckBox();
            this.MatchCountLabel = new System.Windows.Forms.Label();
            this.PreviousButton = new System.Windows.Forms.Button();
            this.FindNextButton = new System.Windows.Forms.Button();
            this.CloseButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = UI.AppStrings.FindLabelSearchFor;
            // 
            // SearchTextBox
            // 
            this.SearchTextBox.FormattingEnabled = true;
            this.SearchTextBox.Location = new System.Drawing.Point(15, 37);
            this.SearchTextBox.Name = "SearchTextBox";
            this.SearchTextBox.Size = new System.Drawing.Size(495, 24);
            this.SearchTextBox.TabIndex = 1;
            this.SearchTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SearchTextBox_KeyDown);
            // 
            // MatchCaseCheckBox
            // 
            this.MatchCaseCheckBox.AutoSize = true;
            this.MatchCaseCheckBox.Location = new System.Drawing.Point(15, 75);
            this.MatchCaseCheckBox.Name = "MatchCaseCheckBox";
            this.MatchCaseCheckBox.Size = new System.Drawing.Size(100, 21);
            this.MatchCaseCheckBox.TabIndex = 2;
            this.MatchCaseCheckBox.Text = UI.AppStrings.FindCheckMatchCase;
            // 
            // UseRegexCheckBox
            // 
            this.UseRegexCheckBox.AutoSize = true;
            this.UseRegexCheckBox.Location = new System.Drawing.Point(140, 75);
            this.UseRegexCheckBox.Name = "UseRegexCheckBox";
            this.UseRegexCheckBox.Size = new System.Drawing.Size(170, 21);
            this.UseRegexCheckBox.TabIndex = 3;
            this.UseRegexCheckBox.Text = UI.AppStrings.FindCheckUseRegex;
            //
            // MatchCountLabel
            //
            this.MatchCountLabel.AutoSize = true;
            this.MatchCountLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.MatchCountLabel.Location = new System.Drawing.Point(15, 118);
            this.MatchCountLabel.Name = "MatchCountLabel";
            this.MatchCountLabel.Size = new System.Drawing.Size(0, 17);
            this.MatchCountLabel.TabIndex = 4;
            //
            // PreviousButton
            //
            this.PreviousButton.Location = new System.Drawing.Point(205, 110);
            this.PreviousButton.Name = "PreviousButton";
            this.PreviousButton.Size = new System.Drawing.Size(95, 35);
            this.PreviousButton.TabIndex = 5;
            this.PreviousButton.Text = "&Previous";
            this.PreviousButton.UseVisualStyleBackColor = true;
            this.PreviousButton.Click += new System.EventHandler(this.PreviousButton_Click);
            //
            // FindNextButton
            //
            this.FindNextButton.Location = new System.Drawing.Point(310, 110);
            this.FindNextButton.Name = "FindNextButton";
            this.FindNextButton.Size = new System.Drawing.Size(95, 35);
            this.FindNextButton.TabIndex = 6;
            this.FindNextButton.Text = UI.AppStrings.FindButtonFindNext;
            this.FindNextButton.UseVisualStyleBackColor = true;
            this.FindNextButton.Click += new System.EventHandler(this.FindNextButton_Click);
            //
            // CloseButton
            //
            this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CloseButton.Location = new System.Drawing.Point(415, 110);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(95, 35);
            this.CloseButton.TabIndex = 7;
            this.CloseButton.Text = UI.AppStrings.FindButtonClose;
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // FindForm
            // 
            this.AcceptButton = this.FindNextButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CloseButton;
            this.ClientSize = new System.Drawing.Size(525, 160);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.FindNextButton);
            this.Controls.Add(this.PreviousButton);
            this.Controls.Add(this.MatchCountLabel);
            this.Controls.Add(this.UseRegexCheckBox);
            this.Controls.Add(this.MatchCaseCheckBox);
            this.Controls.Add(this.SearchTextBox);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FindForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = UI.AppStrings.FindFormTitle;
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FindForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FindForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox SearchTextBox;
        private System.Windows.Forms.CheckBox MatchCaseCheckBox;
        private System.Windows.Forms.CheckBox UseRegexCheckBox;
        private System.Windows.Forms.Label MatchCountLabel;
        private System.Windows.Forms.Button PreviousButton;
        private System.Windows.Forms.Button FindNextButton;
        private System.Windows.Forms.Button CloseButton;
    }
}
