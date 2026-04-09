namespace Cad3PLogBrowser.Services.UI
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using Cad3PLogBrowser.Utilities;

    /// <summary>
    /// Manages the application status bar, providing progress updates and status messages.
    /// Centralizes all status bar update logic that was previously in MainForm.
    /// </summary>
    /// <remarks>
    /// The status bar displays:
    /// - Left: File status indicator (red/yellow/green ball icon)
    /// - Center-Left: Progress bar for long operations
    /// - Center: Status message (file name, operation status, filter info)
    /// - Center-Right: Line count
    /// - Right: Selected line preview
    /// 
    /// This manager ensures consistent status bar behavior across the application.
    /// </remarks>
    public class StatusBarManager
    {
        // StatusStrip components
        private readonly StatusStrip _statusStrip;
        private readonly ToolStripStatusLabel _fileStatusLabel;
        private readonly ToolStripProgressBar _progressBar;
        private readonly ToolStripStatusLabel _statusMessageLabel;
        private readonly ToolStripStatusLabel _lineCountLabel;
        private readonly ToolStripStatusLabel _selectionLabel;

        /// <summary>
        /// Gets or sets the current operation name (for cancellation messages).
        /// </summary>
        public string CurrentOperation { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusBarManager"/> class.
        /// </summary>
        /// <param name="statusStrip">The main status strip control.</param>
        /// <exception cref="ArgumentNullException">Thrown if statusStrip is null.</exception>
        /// <remarks>
        /// The status strip must have at least 5 items in a specific order:
        /// [0] File status icon
        /// [1] Progress bar
        /// [2] Status message
        /// [3] Line count
        /// [4] Selection preview
        /// </remarks>
        public StatusBarManager(StatusStrip statusStrip)
        {
            _statusStrip = statusStrip ?? throw new ArgumentNullException(nameof(statusStrip));

            // Get references to status strip items (assumes they exist in designer)
            if (statusStrip.Items.Count >= 5)
            {
                _fileStatusLabel = statusStrip.Items[0] as ToolStripStatusLabel;
                _progressBar = statusStrip.Items[1] as ToolStripProgressBar;
                _statusMessageLabel = statusStrip.Items[2] as ToolStripStatusLabel;
                _lineCountLabel = statusStrip.Items[3] as ToolStripStatusLabel;
                _selectionLabel = statusStrip.Items[4] as ToolStripStatusLabel;
            }
        }

        /// <summary>
        /// Shows a progress bar with marquee style for operations of unknown duration.
        /// </summary>
        /// <param name="message">Message to display (e.g., "Loading file...").</param>
        /// <param name="allowCancel">Whether to show "ESC to cancel" hint.</param>
        /// <remarks>
        /// Marquee style is used when we don't know how long an operation will take.
        /// The progress bar animates continuously without showing a specific percentage.
        /// </remarks>
        public void ShowProgress(string message, bool allowCancel = true)
        {
            if (_progressBar != null)
            {
                _progressBar.Style = ProgressBarStyle.Marquee;
                _progressBar.Visible = true;
            }

            if (_statusMessageLabel != null)
            {
                string cancelHint = allowCancel ? " (ESC or click here to cancel)" : "";
                _statusMessageLabel.Text = message + cancelHint;
            }
        }

        /// <summary>
        /// Updates progress bar with a specific percentage and message.
        /// </summary>
        /// <param name="percentage">Progress percentage (0-100).</param>
        /// <param name="message">Status message to display.</param>
        /// <remarks>
        /// Use this when you can calculate progress (e.g., processing 500 of 1000 lines = 50%).
        /// The progress bar switches from marquee to blocks style automatically.
        /// </remarks>
        /// <example>
        /// for (int i = 0; i < totalLines; i++)
        /// {
        ///     ProcessLine(i);
        ///     
        ///     if (i % 100 == 0)
        ///     {
        ///         int percent = (i * 100) / totalLines;
        ///         statusBarManager.UpdateProgress(percent, $"Processing line {i} of {totalLines}");
        ///     }
        /// }
        /// </example>
        public void UpdateProgress(int percentage, string message)
        {
            if (_progressBar != null)
            {
                _progressBar.Style = ProgressBarStyle.Blocks;
                _progressBar.Value = Math.Min(100, Math.Max(0, percentage)); // Clamp to 0-100
                _progressBar.Visible = true;
            }

            if (_statusMessageLabel != null)
            {
                _statusMessageLabel.Text = message;
            }
        }

        /// <summary>
        /// Hides the progress bar and clears the status message.
        /// Call this when an operation completes successfully.
        /// </summary>
        public void HideProgress()
        {
            if (_progressBar != null)
            {
                _progressBar.Visible = false;
                _progressBar.Value = 0;
            }

            if (_statusMessageLabel != null)
            {
                _statusMessageLabel.Text = string.Empty;
            }

            CurrentOperation = string.Empty;
        }

        /// <summary>
        /// Updates the file status indicator icon (red/yellow/green ball).
        /// </summary>
        /// <param name="status">Status level: "error" (red), "warning" (yellow), "ok" (green).</param>
        /// <remarks>
        /// The icon provides quick visual feedback:
        /// - Red: File load error or file changed on disk
        /// - Yellow: File loading in progress
        /// - Green: File loaded successfully
        /// </remarks>
        public void SetFileStatus(string status)
        {
            if (_fileStatusLabel == null)
                return;

            // Note: This assumes red_ball, yellow, green_ball images exist in Resources
            // In a real implementation, these would be set via the image property
            string tooltipText;
            switch (status)
            {
                case "error":
                    tooltipText = "File error or changed on disk";
                    break;
                case "warning":
                    tooltipText = "File loading...";
                    break;
                case "ok":
                    tooltipText = "File loaded successfully";
                    break;
                default:
                    tooltipText = "Ready";
                    break;
            }
            _fileStatusLabel.ToolTipText = tooltipText;
        }

        /// <summary>
        /// Updates the line count display (e.g., "4,231 lines").
        /// </summary>
        /// <param name="totalLines">Total number of lines in the log file.</param>
        /// <param name="visibleLines">Number of lines currently visible after filtering (optional).</param>
        /// <remarks>
        /// If filtering is active and visibleLines is provided, shows both:
        /// "1,234 of 4,231 lines (filtered)"
        /// 
        /// Otherwise shows just the total:
        /// "4,231 lines"
        /// </remarks>
        public void UpdateLineCount(int totalLines, int? visibleLines = null)
        {
            if (_lineCountLabel == null)
                return;

            if (visibleLines.HasValue && visibleLines.Value != totalLines)
            {
                _lineCountLabel.Text = $"{visibleLines:N0} of {totalLines:N0} lines (filtered)";
            }
            else
            {
                _lineCountLabel.Text = $"{totalLines:N0} line{(totalLines != 1 ? "s" : "")}";
            }
        }

        /// <summary>
        /// Updates the selection preview showing details of the currently selected line.
        /// </summary>
        /// <param name="lineNumber">Selected line number.</param>
        /// <param name="preview">Preview text (first N characters of the line).</param>
        /// <remarks>
        /// Example display: "Line 142: CADSystemUnigraphics::setReadOnly  142ms"
        /// Truncated to fit in the status bar (typically ~50 characters).
        /// </remarks>
        public void UpdateSelection(int lineNumber, string preview)
        {
            if (_selectionLabel == null)
                return;

            if (lineNumber > 0)
            {
                string truncated = preview.Truncate(50);
                _selectionLabel.Text = $"Line {lineNumber}: {truncated}";
            }
            else
            {
                _selectionLabel.Text = string.Empty;
            }
        }

        /// <summary>
        /// Clears the selection preview.
        /// </summary>
        public void ClearSelection()
        {
            if (_selectionLabel != null)
            {
                _selectionLabel.Text = string.Empty;
            }
        }

        /// <summary>
        /// Shows a temporary status message that disappears after a timeout.
        /// Useful for confirmation messages like "Copied to clipboard".
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="durationMs">How long to show the message (default: 3000ms = 3 seconds).</param>
        /// <example>
        /// statusBarManager.ShowTemporaryMessage("Copied 10 lines to clipboard.", 2000);
        /// </example>
        public void ShowTemporaryMessage(string message, int durationMs = 3000)
        {
            if (_statusMessageLabel == null)
                return;

            _statusMessageLabel.Text = message;

            // Clear after timeout
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = durationMs;
            timer.Tick += (s, e) =>
            {
                _statusMessageLabel.Text = string.Empty;
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }

        /// <summary>
        /// Shows a warning message in amber color.
        /// Used for important notifications that don't require user action.
        /// </summary>
        /// <param name="message">Warning message to display.</param>
        /// <example>
        /// statusBarManager.ShowWarning("? Large log — performance updates may be slow");
        /// </example>
        public void ShowWarning(string message)
        {
            if (_statusMessageLabel == null)
                return;

            _statusMessageLabel.Text = message;
            _statusMessageLabel.ForeColor = Color.DarkOrange;
        }

        /// <summary>
        /// Shows an error message in red color.
        /// </summary>
        /// <param name="message">Error message to display.</param>
        public void ShowError(string message)
        {
            if (_statusMessageLabel == null)
                return;

            _statusMessageLabel.Text = message;
            _statusMessageLabel.ForeColor = Color.Red;
        }

        /// <summary>
        /// Resets status message color to default.
        /// </summary>
        public void ResetMessageColor()
        {
            if (_statusMessageLabel != null)
            {
                _statusMessageLabel.ForeColor = SystemColors.ControlText;
            }
        }

        /// <summary>
        /// Updates all status bar components with comprehensive file information.
        /// </summary>
        /// <param name="fileName">Current file name (or empty if no file loaded).</param>
        /// <param name="totalLines">Total lines in file.</param>
        /// <param name="visibleLines">Visible lines after filtering (null if no filter).</param>
        /// <param name="filterDescription">Description of active filters.</param>
        public void UpdateFullStatus(
            string fileName,
            int totalLines,
            int? visibleLines = null,
            string filterDescription = null)
        {
            // Update status message with file name or filter info
            if (_statusMessageLabel != null)
            {
                if (!string.IsNullOrWhiteSpace(filterDescription))
                {
                    _statusMessageLabel.Text = $"Filter: {filterDescription}";
                }
                else if (!string.IsNullOrWhiteSpace(fileName))
                {
                    _statusMessageLabel.Text = fileName;
                }
                else
                {
                    _statusMessageLabel.Text = "Ready";
                }
            }

            // Update line count
            UpdateLineCount(totalLines, visibleLines);
        }
    }
}
