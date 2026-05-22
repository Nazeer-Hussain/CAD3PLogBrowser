namespace Cad3PLogBrowser
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Cad3PLogBrowser.Services;
    using Cad3PLogBrowser.Services.Update;

    /// <summary>
    /// Displays information about an available update and lets the user
    /// download and install it or defer until later.
    /// </summary>
    internal class UpdateAvailableForm : Form
    {
        // ?? Controls ??????????????????????????????????????????????????????????
        private Label       _lblTitle;
        private Label       _lblCurrentVersion;
        private Label       _lblNewVersion;
        private Label       _lblReleaseNotesHeader;
        private RichTextBox _txtReleaseNotes;
        private ProgressBar _progressBar;
        private Label       _lblStatus;
        private Button      _btnUpdate;
        private Button      _btnLater;
        private Button      _btnCancel;

        // ?? State ?????????????????????????????????????????????????????????????
        private readonly UpdateManifest _manifest;
        private readonly UpdateService  _service;
        private bool _downloading = false;

        // ?? Constructor ???????????????????????????????????????????????????????

        public UpdateAvailableForm(UpdateManifest manifest, UpdateService service)
        {
            _manifest = manifest ?? throw new ArgumentNullException("manifest");
            _service  = service  ?? throw new ArgumentNullException("service");

            BuildUI();
            PopulateContent();
            ThemeManager.ApplyTheme(this);
        }

        // ?? UI construction ???????????????????????????????????????????????????

        private void BuildUI()
        {
            Text            = "Update Available";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;
            ClientSize      = new Size(480, 370);
            ShowInTaskbar   = false;

            _lblTitle = new Label
            {
                Font      = new Font(SystemFonts.DefaultFont.FontFamily, 12f, FontStyle.Bold),
                Location  = new Point(16, 16),
                Size      = new Size(448, 28),
                TextAlign = ContentAlignment.MiddleLeft
            };

            _lblCurrentVersion = new Label
            {
                AutoSize = true,
                Location = new Point(16, 52)
            };

            _lblNewVersion = new Label
            {
                AutoSize  = true,
                Location  = new Point(16, 72),
                ForeColor = Color.FromArgb(0, 128, 0)
            };

            _lblReleaseNotesHeader = new Label
            {
                AutoSize = true,
                Font     = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
                Location = new Point(16, 100),
                Text     = "Release Notes:"
            };

            _txtReleaseNotes = new RichTextBox
            {
                Location   = new Point(16, 120),
                Size       = new Size(448, 140),
                ReadOnly   = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor  = SystemColors.Window,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            _progressBar = new ProgressBar
            {
                Location = new Point(16, 272),
                Size     = new Size(448, 18),
                Visible  = false
            };

            _lblStatus = new Label
            {
                AutoSize  = true,
                Location  = new Point(16, 294),
                ForeColor = SystemColors.GrayText,
                Visible   = false
            };

            _btnUpdate = new Button
            {
                Text     = "&Update Now",
                Location = new Point(16, 326),
                Size     = new Size(120, 30),
                TabIndex = 0
            };

            _btnLater = new Button
            {
                Text     = "Later",
                Location = new Point(152, 326),
                Size     = new Size(80, 30),
                TabIndex = 1
            };

            _btnCancel = new Button
            {
                Text    = "Cancel Download",
                Location = new Point(16, 326),
                Size    = new Size(140, 30),
                Visible = false,
                TabIndex = 2
            };

            _btnUpdate.Click += OnUpdateClicked;
            _btnLater.Click  += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            _btnCancel.Click += OnCancelClicked;

            Controls.AddRange(new Control[]
            {
                _lblTitle, _lblCurrentVersion, _lblNewVersion,
                _lblReleaseNotesHeader, _txtReleaseNotes,
                _progressBar, _lblStatus,
                _btnUpdate, _btnLater, _btnCancel
            });

            AcceptButton = _btnUpdate;
            CancelButton = _btnLater;
        }

        private void PopulateContent()
        {
            Version current = Assembly.GetExecutingAssembly().GetName().Version;

            _lblTitle.Text          = "A new version is available!";
            _lblCurrentVersion.Text = string.Format("Installed version:  {0}", current);
            _lblNewVersion.Text     = string.Format("Available version:  {0}", _manifest.Version);
            _txtReleaseNotes.Text   = string.IsNullOrWhiteSpace(_manifest.ReleaseNotes)
                                        ? "(No release notes provided.)"
                                        : _manifest.ReleaseNotes;

            if (_manifest.Mandatory)
            {
                _btnLater.Enabled = false;
                _btnLater.Text    = "(Required)";
                CancelButton      = null;
            }
        }

        // ?? Button handlers ???????????????????????????????????????????????????

        private async void OnUpdateClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_manifest.DownloadUrl))
            {
                MessageBox.Show("The update manifest does not contain a download URL.\n" +
                    "Please visit the GitHub releases page to download manually.",
                    "Update", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetDownloadingState(true);

            _service.DownloadProgressChanged += OnDownloadProgress;

            string tempPath = await _service.DownloadUpdateAsync(_manifest.DownloadUrl);

            _service.DownloadProgressChanged -= OnDownloadProgress;

            if (tempPath == null)
            {
                SetDownloadingState(false);
                if (_downloading) // not cancelled by the user
                {
                    MessageBox.Show("Download failed. Please check your internet connection and try again.",
                        "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }

            // Hand off to batch-script updater and close
            string currentExe = Assembly.GetExecutingAssembly().Location;
            int    currentPid = Process.GetCurrentProcess().Id;

            try
            {
                _service.ApplyUpdate(currentExe, tempPath, currentPid);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Could not launch updater:\n{0}", ex.Message),
                    "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Signal the parent form to close the application
            DialogResult = DialogResult.OK;
            Close();
        }

        private void OnCancelClicked(object sender, EventArgs e)
        {
            _downloading = false;
            _service.CancelDownload();
            SetDownloadingState(false);
        }

        private void OnDownloadProgress(int percent)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action<int>)OnDownloadProgress, percent);
                return;
            }

            _progressBar.Value = Math.Min(percent, 100);
            _lblStatus.Text    = string.Format("Downloading... {0}%", percent);
        }

        // ?? Helpers ???????????????????????????????????????????????????????????

        private void SetDownloadingState(bool downloading)
        {
            _downloading         = downloading;
            _btnUpdate.Visible   = !downloading;
            _btnLater.Visible    = !downloading;
            _btnCancel.Visible   = downloading;
            _progressBar.Visible = downloading;
            _lblStatus.Visible   = downloading;

            if (downloading)
            {
                _progressBar.Value = 0;
                _lblStatus.Text    = "Preparing download...";
                CancelButton       = _btnCancel;
            }
            else
            {
                _progressBar.Value = 0;
                CancelButton       = _btnLater;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_downloading)
            {
                _service.CancelDownload();
            }
            base.OnFormClosing(e);
        }
    }
}
