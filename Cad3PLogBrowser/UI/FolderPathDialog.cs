using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Cad3PLogBrowser.UI
{
    /// <summary>
    /// A folder-picker dialog that, unlike the plain <see cref="FolderBrowserDialog"/>,
    /// also lets the user type or paste a path directly instead of only navigating
    /// the tree view.
    /// </summary>
    internal sealed class FolderPathDialog : Form
    {
        private readonly TextBox _pathTextBox;
        private readonly Button _browseButton;
        private readonly Button _okButton;
        private readonly Button _cancelButton;
        private readonly Label _promptLabel;

        public string SelectedPath => _pathTextBox.Text.Trim();

        public FolderPathDialog(string prompt, string initialPath)
        {
            Text = "Select Folder";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(520, 124);

            _promptLabel = new Label
            {
                AutoSize = false,
                Location = new Point(12, 12),
                Size = new Size(496, 34),
                Text = prompt
            };

            _pathTextBox = new TextBox
            {
                Location = new Point(12, 50),
                Size = new Size(414, 23),
                Text = initialPath ?? string.Empty
            };

            _browseButton = new Button
            {
                Location = new Point(432, 49),
                Size = new Size(76, 25),
                Text = "Browse�"
            };
            _browseButton.Click += BrowseButton_Click;

            _okButton = new Button
            {
                Location = new Point(352, 86),
                Size = new Size(75, 26),
                Text = "OK",
                DialogResult = DialogResult.OK
            };
            _okButton.Click += OkButton_Click;

            _cancelButton = new Button
            {
                Location = new Point(433, 86),
                Size = new Size(75, 26),
                Text = "Cancel",
                DialogResult = DialogResult.Cancel
            };

            Controls.Add(_promptLabel);
            Controls.Add(_pathTextBox);
            Controls.Add(_browseButton);
            Controls.Add(_okButton);
            Controls.Add(_cancelButton);

            AcceptButton = _okButton;
            CancelButton = _cancelButton;
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = _promptLabel.Text;
                if (!string.IsNullOrEmpty(_pathTextBox.Text) && Directory.Exists(_pathTextBox.Text))
                    dlg.SelectedPath = _pathTextBox.Text;

                if (dlg.ShowDialog(this) == DialogResult.OK)
                    _pathTextBox.Text = dlg.SelectedPath;
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            string path = _pathTextBox.Text.Trim();
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                MessageBox.Show(this,
                    string.Format("\"{0}\" is not a valid folder path.", path),
                    "Select Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
            }
        }
    }
}
