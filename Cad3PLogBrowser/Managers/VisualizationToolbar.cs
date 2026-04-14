using System;
using System.Drawing;
using System.Windows.Forms;
using Cad3PLogBrowser.Services;

namespace Cad3PLogBrowser.Managers
{
    /// <summary>
    /// Professional toolbar for visualization panels (FlameGraph, Timeline).
    /// Provides quick access to common operations with modern UI.
    /// </summary>
    public class VisualizationToolbar : ToolStrip
    {
        private ToolStripButton _resetButton;
        private ToolStripButton _zoomInButton;
        private ToolStripButton _zoomOutButton;
        private ToolStripButton _fitButton;
        private ToolStripLabel _zoomLabel;
        private ToolStripButton _exportButton;
        private ToolStripButton _searchButton;
        private ToolStripTextBox _searchBox;

        public event EventHandler ResetClicked;
        public event EventHandler ZoomInClicked;
        public event EventHandler ZoomOutClicked;
        public event EventHandler FitToWindowClicked;
        public event EventHandler ExportClicked;
        public event EventHandler<string> SearchTextChanged;

        public VisualizationToolbar()
        {
            this.GripStyle = ToolStripGripStyle.Hidden;
            this.Renderer = new ToolStripProfessionalRenderer(new CustomColorTable());
            this.ImageScalingSize = new Size(16, 16);
            this.Padding = new Padding(4, 2, 4, 2);

            InitializeButtons();
            ApplyTheme();
        }

        private void InitializeButtons()
        {
            // Reset View button
            _resetButton = new ToolStripButton
            {
                Text = "Reset View",
                ToolTipText = "Reset zoom and pan to default view (Ctrl+0)",
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                Image = CreateResetIcon()
            };
            _resetButton.Click += (s, e) => ResetClicked?.Invoke(this, EventArgs.Empty);

            // Zoom In button
            _zoomInButton = new ToolStripButton
            {
                Text = "Zoom In",
                ToolTipText = "Zoom in (Ctrl++, Mouse Wheel Up)",
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                Image = CreateZoomInIcon()
            };
            _zoomInButton.Click += (s, e) => ZoomInClicked?.Invoke(this, EventArgs.Empty);

            // Zoom Out button
            _zoomOutButton = new ToolStripButton
            {
                Text = "Zoom Out",
                ToolTipText = "Zoom out (Ctrl+-, Mouse Wheel Down)",
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                Image = CreateZoomOutIcon()
            };
            _zoomOutButton.Click += (s, e) => ZoomOutClicked?.Invoke(this, EventArgs.Empty);

            // Fit to Window button
            _fitButton = new ToolStripButton
            {
                Text = "Fit to Window",
                ToolTipText = "Fit entire graph to window (Ctrl+F)",
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                Image = CreateFitIcon()
            };
            _fitButton.Click += (s, e) => FitToWindowClicked?.Invoke(this, EventArgs.Empty);

            // Zoom label
            _zoomLabel = new ToolStripLabel
            {
                Text = "100%",
                AutoSize = true,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 215)
            };

            // Export button
            _exportButton = new ToolStripButton
            {
                Text = "Export",
                ToolTipText = "Export to Speedscope JSON format",
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                Image = CreateExportIcon()
            };
            _exportButton.Click += (s, e) => ExportClicked?.Invoke(this, EventArgs.Empty);

            // Search box
            _searchBox = new ToolStripTextBox
            {
                Width = 150,
                ToolTipText = "Search for function names (Ctrl+F)"
            };
            _searchBox.TextChanged += (s, e) => SearchTextChanged?.Invoke(this, _searchBox.Text);

            // Search button
            _searchButton = new ToolStripButton
            {
                Text = "??",
                ToolTipText = "Search functions",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = new Font("Segoe UI", 9f)
            };

            // Add items to toolbar
            this.Items.Add(_resetButton);
            this.Items.Add(new ToolStripSeparator());
            this.Items.Add(_zoomInButton);
            this.Items.Add(_zoomOutButton);
            this.Items.Add(_fitButton);
            this.Items.Add(_zoomLabel);
            this.Items.Add(new ToolStripSeparator());
            this.Items.Add(_searchButton);
            this.Items.Add(_searchBox);
            this.Items.Add(new ToolStripSeparator());
            this.Items.Add(_exportButton);
        }

        public void UpdateZoomLevel(float zoom)
        {
            _zoomLabel.Text = $"{zoom * 100:F0}%";
        }

        public void ApplyTheme()
        {
            bool isDark = ThemeManager.CurrentTheme == ThemeManager.Theme.Dark;

            this.BackColor = isDark ? Color.FromArgb(45, 45, 48) : Color.FromArgb(245, 245, 245);
            this.ForeColor = isDark ? Color.FromArgb(220, 220, 220) : Color.FromArgb(30, 30, 30);

            _zoomLabel.ForeColor = Color.FromArgb(0, 120, 215);
            _searchBox.BackColor = isDark ? Color.FromArgb(37, 37, 38) : Color.White;
            _searchBox.ForeColor = isDark ? Color.FromArgb(220, 220, 220) : Color.FromArgb(30, 30, 30);
        }

        // Simple icon creation methods (using GDI+)
        private Image CreateResetIcon()
        {
            var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.FromArgb(100, 100, 100), 2))
                {
                    // Draw circular arrow
                    g.DrawArc(pen, 2, 2, 12, 12, 45, 270);
                    // Arrow head
                    g.DrawLine(pen, 8, 2, 11, 5);
                    g.DrawLine(pen, 8, 2, 5, 5);
                }
            }
            return bmp;
        }

        private Image CreateZoomInIcon()
        {
            var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.FromArgb(0, 120, 215), 2))
                {
                    g.DrawEllipse(pen, 2, 2, 10, 10);
                    g.DrawLine(pen, 5, 7, 9, 7); // Horizontal
                    g.DrawLine(pen, 7, 5, 7, 9); // Vertical (plus sign)
                    g.DrawLine(pen, 11, 11, 14, 14); // Handle
                }
            }
            return bmp;
        }

        private Image CreateZoomOutIcon()
        {
            var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.FromArgb(0, 120, 215), 2))
                {
                    g.DrawEllipse(pen, 2, 2, 10, 10);
                    g.DrawLine(pen, 5, 7, 9, 7); // Horizontal only (minus sign)
                    g.DrawLine(pen, 11, 11, 14, 14); // Handle
                }
            }
            return bmp;
        }

        private Image CreateFitIcon()
        {
            var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            {
                using (var pen = new Pen(Color.FromArgb(100, 100, 100), 2))
                {
                    g.DrawRectangle(pen, 2, 2, 12, 12);
                    // Arrows pointing outward
                    g.DrawLine(pen, 4, 4, 6, 6);
                    g.DrawLine(pen, 12, 4, 10, 6);
                    g.DrawLine(pen, 4, 12, 6, 10);
                    g.DrawLine(pen, 12, 12, 10, 10);
                }
            }
            return bmp;
        }

        private Image CreateExportIcon()
        {
            var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            {
                using (var pen = new Pen(Color.FromArgb(76, 175, 80), 2))
                {
                    // Arrow pointing up
                    g.DrawLine(pen, 8, 2, 8, 12);
                    g.DrawLine(pen, 8, 2, 5, 5);
                    g.DrawLine(pen, 8, 2, 11, 5);
                    // Base line
                    g.DrawLine(pen, 3, 14, 13, 14);
                }
            }
            return bmp;
        }

        private class CustomColorTable : System.Windows.Forms.ProfessionalColorTable
        {
            public override Color ToolStripGradientBegin => 
                ThemeManager.CurrentTheme == ThemeManager.Theme.Dark 
                    ? Color.FromArgb(45, 45, 48) : Color.FromArgb(245, 245, 245);

            public override Color ToolStripGradientEnd => 
                ThemeManager.CurrentTheme == ThemeManager.Theme.Dark 
                    ? Color.FromArgb(45, 45, 48) : Color.FromArgb(245, 245, 245);
        }
    }
}
