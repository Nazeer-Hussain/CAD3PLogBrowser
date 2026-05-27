namespace Cad3PLogBrowser
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Reflection;
    using System.Windows.Forms;
    using Cad3PLogBrowser.Services;
    using Cad3PLogBrowser.Services.Update;

    /// <summary>
    /// Displays information about an available update and lets the user
    /// download and install it, defer until later, or skip this version.
    /// </summary>
    internal class UpdateAvailableForm : Form
    {
        // ── Controls ──────────────────────────────────────────────────────────
        private Label       _lblTitle;
        private Label       _lblCurrentVersion;
        private Label       _lblNewVersion;
        private Label       _lblDownloadSize;
        private Label       _lblReleaseNotesHeader;
        // _txtReleaseNotes is intentionally NOT created in BuildUI.
        // RichTextBox..ctor() always calls set_Multiline(true) → AdjustHeight
        // → Font.GetHeight() → GetDC(null).  GetDC(null) can return NULL in
        // async-continuation contexts, causing ArgumentException from GDI+.
        // It is created safely in OnLoad() once the form HWND exists.
        private RichTextBox _txtReleaseNotes;
        private Panel       _releaseNotesContainer;  // placeholder until OnLoad
        private ProgressBar _progressBar;
        private Label       _lblStatus;
        private Button      _btnUpdate;
        private Button      _btnLater;
        private Button      _btnSkip;
        private Button      _btnCancel;

        // ── Owned fonts (disposed with the form) ──────────────────────────────
        private Font _fontTitle;
        private Font _fontBold;

        // ── State ─────────────────────────────────────────────────────────────
        private readonly UpdateManifest _manifest;
        private readonly UpdateService  _service;
        private bool _downloadActive  = false;
        private bool _cancelledByUser = false;

        // ── Result ────────────────────────────────────────────────────────────
        /// <summary>True when the user chose "Skip this version" (ENH-3).</summary>
        public bool UserSkippedVersion { get; private set; }

        // ?? Constructor ???????????????????????????????????????????????????????

        public UpdateAvailableForm(UpdateManifest manifest, UpdateService service)
        {
            _manifest = manifest ?? throw new ArgumentNullException("manifest");
            _service  = service  ?? throw new ArgumentNullException("service");

            BuildUI();
            PopulateContent();
            ThemeManager.ApplyTheme(this);

            // ENH-5: query size asynchronously so the dialog opens instantly
            if (_manifest.DownloadSizeBytes > 0)
            {
                ShowDownloadSize(_manifest.DownloadSizeBytes);
            }
            else if (!string.IsNullOrWhiteSpace(_manifest.DownloadUrl))
            {
                _lblDownloadSize.Text = "Checking download size...";
                var svc = _service;
                var url = _manifest.DownloadUrl;
                System.Threading.Tasks.Task.Run(() => svc.QueryDownloadSizeAsync(url))
                    .ContinueWith(t =>
                    {
                        if (!IsDisposed)
                            BeginInvoke((Action)(() => ShowDownloadSize(t.Result)));
                    });
            }
        }

        // ?? UI construction ???????????????????????????????????????????????????

        private void BuildUI()
        {
            // Create owned fonts using explicit family names so we never depend on
            // SystemFonts.DefaultFont whose underlying GDI object may already be
            // freed when the RichTextBox constructor queries it internally.
            _fontTitle = new Font("Segoe UI", 12f, FontStyle.Bold, GraphicsUnit.Point);
            _fontBold  = new Font("Segoe UI",  9f, FontStyle.Bold, GraphicsUnit.Point);

            SuspendLayout();

            Text            = "Update Available";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;
            ClientSize      = new Size(500, 400);
            ShowInTaskbar   = false;
            Font            = new Font("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);

            _lblTitle = new Label
            {
                Font      = _fontTitle,
                Location  = new Point(16, 14),
                Size      = new Size(468, 28),
                TextAlign = ContentAlignment.MiddleLeft
            };

            _lblCurrentVersion = new Label
            {
                AutoSize = true,
                Location = new Point(16, 50)
            };

            _lblNewVersion = new Label
            {
                AutoSize  = true,
                Location  = new Point(16, 70),
                ForeColor = Color.FromArgb(0, 128, 0)
            };

            _lblDownloadSize = new Label
            {
                AutoSize  = true,
                Location  = new Point(16, 90),
                ForeColor = SystemColors.GrayText
            };

            _lblReleaseNotesHeader = new Label
            {
                AutoSize = true,
                Font     = _fontBold,
                Location = new Point(16, 114),
                Text     = "Release Notes:"
            };

            // Plain Panel placeholder — no font measurement required.
            // The actual RichTextBox is created in OnLoad() once the HWND exists.
            _releaseNotesContainer = new Panel
            {
                Location    = new Point(16, 134),
                Size        = new Size(468, 148),
                BackColor   = SystemColors.Window,
                BorderStyle = BorderStyle.FixedSingle
            };

            _progressBar = new ProgressBar
            {
                Location = new Point(16, 292),
                Size     = new Size(468, 18),
                Visible  = false
            };

            _lblStatus = new Label
            {
                Location  = new Point(16, 314),
                Size      = new Size(468, 18),
                ForeColor = SystemColors.GrayText,
                Visible   = false
            };

            // ── Buttons ───────────────────────────────────────────────────────

            _btnUpdate = new Button
            {
                Text     = "&Update Now",
                Location = new Point(16, 358),
                Size     = new Size(120, 30),
                TabIndex = 0
            };

            _btnLater = new Button
            {
                Text     = "Later",
                Location = new Point(150, 358),
                Size     = new Size(80, 30),
                TabIndex = 1
            };

            _btnSkip = new Button
            {
                Text      = "Skip This Version",
                Location  = new Point(244, 358),
                Size      = new Size(130, 30),
                TabIndex  = 2,
                ForeColor = SystemColors.GrayText
            };

            _btnCancel = new Button
            {
                Text     = "Cancel Download",
                Location = new Point(16, 358),
                Size     = new Size(140, 30),
                Visible  = false,
                TabIndex = 3
            };

            _btnUpdate.Click += OnUpdateClicked;
            _btnLater.Click  += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            _btnSkip.Click   += OnSkipClicked;
            _btnCancel.Click += OnCancelClicked;

            Controls.AddRange(new System.Windows.Forms.Control[]
            {
                _lblTitle, _lblCurrentVersion, _lblNewVersion, _lblDownloadSize,
                _lblReleaseNotesHeader, _releaseNotesContainer,
                _progressBar, _lblStatus,
                _btnUpdate, _btnLater, _btnSkip, _btnCancel
            });

            AcceptButton = _btnUpdate;
            CancelButton = _btnLater;

            ResumeLayout(false);
        }

        private void PopulateContent()
        {
            Version current = Assembly.GetExecutingAssembly().GetName().Version;

            _lblTitle.Text          = "A new version is available!";
            _lblCurrentVersion.Text = string.Format("Installed version:  {0}", current.ToString(3));
            _lblNewVersion.Text     = string.Format("Available version:  {0}", _manifest.Version);
            // Release notes text is applied in OnLoad() once _txtReleaseNotes exists.

            if (_manifest.Mandatory)
            {
                _btnLater.Enabled = false;
                _btnSkip.Enabled  = false;
                _btnLater.Text    = "(Required)";
                CancelButton      = null;
            }
        }

        // ── Button handlers ───────────────────────────────────────────────────

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Form HWND is guaranteed to exist here, so RichTextBox..ctor()
            // can safely call AdjustHeight → Font.GetHeight() → GetDC(hwnd).
            _txtReleaseNotes = new RichTextBox
            {
                Dock        = DockStyle.Fill,
                ReadOnly    = true,
                BorderStyle = BorderStyle.None,   // outer panel supplies the border
                BackColor   = SystemColors.Window,
                ScrollBars  = RichTextBoxScrollBars.Vertical,
                Text        = string.IsNullOrWhiteSpace(_manifest.ReleaseNotes)
                                  ? "(No release notes provided.)"
                                  : _manifest.ReleaseNotes
            };
            _releaseNotesContainer.Controls.Add(_txtReleaseNotes);

            // Theme the new control to match the rest of the dialog.
            // ThemeManager.ApplyTheme only accepts Form, so mirror the form colours directly.
            _txtReleaseNotes.BackColor = this.BackColor;
            _txtReleaseNotes.ForeColor = this.ForeColor;
        }

        private async void OnUpdateClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_manifest.DownloadUrl))
            {
                MessageBox.Show(
                    "The update manifest does not contain a download URL.\n" +
                    "Please visit the GitHub releases page to download manually.",
                    "Update", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetDownloadingState(true);

            _service.DownloadProgressChanged += OnDownloadProgress;
            _service.DownloadStatsChanged    += OnDownloadStats;   // ENH-1

            // Pass expected SHA-256 so UpdateService can verify (ENH-6)
            string tempPath = await _service.DownloadUpdateAsync(
                _manifest.DownloadUrl, _manifest.Sha256);

            _service.DownloadProgressChanged -= OnDownloadProgress;
            _service.DownloadStatsChanged    -= OnDownloadStats;

            // BUG-1: capture cancel state BEFORE SetDownloadingState resets _downloadActive
            bool wasCancelled = _cancelledByUser;
            SetDownloadingState(false);

            if (tempPath == null)
            {
                if (!wasCancelled)
                {
                    // Genuine failure (network error or hash/PE verification failure)
                    MessageBox.Show(
                        "Download failed or the downloaded file failed verification.\n" +
                        "Please check your internet connection and try again.",
                        "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                // If cancelled by user: no message, just re-enable the buttons silently
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

            DialogResult = DialogResult.OK;
            Close();
        }

        private void OnCancelClicked(object sender, EventArgs e)
        {
            _cancelledByUser = true;
            _service.CancelDownload();
            // SetDownloadingState(false) is called from OnUpdateClicked after await returns
        }

        // ENH-3 ? Skip this version
        private void OnSkipClicked(object sender, EventArgs e)
        {
            UserSkippedVersion = true;
            DialogResult       = DialogResult.Ignore;
            Close();
        }

        // ENH-1 ? Progress percentage
        private void OnDownloadProgress(int percent)
        {
            if (InvokeRequired) { BeginInvoke((Action<int>)OnDownloadProgress, percent); return; }
            _progressBar.Value = Math.Min(percent, 100);
        }

        // ENH-1 ? Speed + ETA in the status label
        private void OnDownloadStats(long bytesReceived, long totalBytes, long speedBytesPerSec)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action<long, long, long>)OnDownloadStats, bytesReceived, totalBytes, speedBytesPerSec);
                return;
            }

            string received = FormatBytes(bytesReceived);
            string speed    = speedBytesPerSec > 0
                ? string.Format("{0}/s", FormatBytes(speedBytesPerSec))
                : "...";

            if (totalBytes > 0)
            {
                string total = FormatBytes(totalBytes);
                long   etaSec = speedBytesPerSec > 0
                    ? (totalBytes - bytesReceived) / speedBytesPerSec
                    : 0;
                string eta = etaSec > 0
                    ? string.Format(", {0} remaining", FormatSeconds(etaSec))
                    : "";
                _lblStatus.Text = string.Format(
                    "Downloading {0} / {1}  —  {2}{3}", received, total, speed, eta);
            }
            else
            {
                _lblStatus.Text = string.Format("Downloading {0}  —  {1}", received, speed);
            }
        }

        // ?? Helpers ???????????????????????????????????????????????????????????

        private void SetDownloadingState(bool downloading)
        {
            _downloadActive      = downloading;
            _cancelledByUser     = false;
            _btnUpdate.Visible   = !downloading;
            _btnLater.Visible    = !downloading;
            _btnSkip.Visible     = !downloading;
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

        // ENH-5 ? show formatted size below the version labels
        private void ShowDownloadSize(long bytes)
        {
            _lblDownloadSize.Text = bytes > 0
                ? string.Format("Download size:  {0}", FormatBytes(bytes))
                : "";
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024)        return string.Format("{0} B",        bytes);
            if (bytes < 1024 * 1024) return string.Format("{0:F1} KB",    bytes / 1024.0);
            return                          string.Format("{0:F1} MB",    bytes / (1024.0 * 1024));
        }

        private static string FormatSeconds(long seconds)
        {
            if (seconds < 60)   return string.Format("{0}s",       seconds);
            if (seconds < 3600) return string.Format("{0}m {1}s",  seconds / 60, seconds % 60);
            return                     string.Format("{0}h {1}m",  seconds / 3600, (seconds % 3600) / 60);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_downloadActive)
            {
                _cancelledByUser = true;
                _service.CancelDownload();
            }
            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fontTitle?.Dispose();
                _fontBold?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

