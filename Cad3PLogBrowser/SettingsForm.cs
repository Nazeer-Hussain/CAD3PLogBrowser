using System;
using System.Windows.Forms;

namespace Cad3PLogBrowser
{
    /// <summary>
    /// Application settings dialog.
    /// Currently a placeholder — extend with real settings as needed.
    /// </summary>
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, EventArgs e) => Hide();

        private void CancelButton_Click(object sender, EventArgs e) => Hide();
    }
}
