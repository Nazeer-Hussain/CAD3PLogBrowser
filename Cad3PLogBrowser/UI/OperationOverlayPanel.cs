using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Cad3PLogBrowser.Services;

namespace Cad3PLogBrowser
{
    /// <summary>
    /// A semi-transparent centred overlay that shows while a long operation runs.
    /// Usage:
    ///   overlay.Show("Loading…");          // marquee
    ///   overlay.SetProgress(42, "42 %");   // determinate
    ///   overlay.Hide();
    /// </summary>
    internal sealed class OperationOverlayPanel : Panel
    {
        // ?? Layout constants ?????????????????????????????????????????????????
        private const int CardW      = 360;
        private const int CardH      = 110;
        private const int PadX       = 24;
        private const int PadY       = 18;
        private const int BarH       = 8;
        private const int CornerR    = 10;

        // ?? Child controls ???????????????????????????????????????????????????
        private readonly Label       _label;
        private readonly ProgressBar _bar;
        private readonly Label       _subLabel;   // "ESC to cancel" hint

        // ?? Card rectangle (centred in this panel) ???????????????????????????
        private Rectangle _card;

        // ??????????????????????????????????????????????????????????????????????
        public OperationOverlayPanel()
        {
            // Panel itself is full-size, click-through background
            Dock        = DockStyle.Fill;
            BackColor   = Color.Transparent;
            Visible     = false;

            // Must be on top of siblings
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            // ?? Operation name label ??????????????????????????????????????????
            _label = new Label
            {
                AutoSize  = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Font      = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(241, 241, 241),
            };

            // ?? Progress bar ??????????????????????????????????????????????????
            _bar = new ProgressBar
            {
                Style   = ProgressBarStyle.Marquee,
                Height  = BarH,
                Minimum = 0,
                Maximum = 100,
                MarqueeAnimationSpeed = 25,
            };

            // ?? ESC hint ??????????????????????????????????????????????????????
            _subLabel = new Label
            {
                AutoSize  = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Font      = new Font("Segoe UI", 8f, FontStyle.Regular, GraphicsUnit.Point),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(160, 160, 160),
                Text      = "Press ESC to cancel",
            };

            Controls.Add(_label);
            Controls.Add(_bar);
            Controls.Add(_subLabel);

            // Re-centre when resized
            Resize    += (s, e) => PositionCard();
            VisibleChanged += (s, e) => { if (Visible) PositionCard(); };
        }

        // ?? Public API ????????????????????????????????????????????????????????

        /// <summary>Displays the overlay in marquee (indeterminate) mode.</summary>
        public void Show(string operationName)
        {
            _label.Text           = operationName;
            _bar.Style            = ProgressBarStyle.Marquee;
            _bar.MarqueeAnimationSpeed = 25;
            _subLabel.Visible     = true;
            Visible               = true;
            BringToFront();
        }

        /// <summary>Switches to determinate mode and updates progress 0–100.</summary>
        public void SetProgress(int percent, string statusText)
        {
            _bar.Style   = ProgressBarStyle.Blocks;
            _bar.Value   = Math.Max(0, Math.Min(100, percent));
            _label.Text  = statusText;
        }

        /// <summary>Hides the overlay.</summary>
        public new void Hide()
        {
            Visible = false;
        }

        /// <summary>Re-applies theme colours to the overlay.</summary>
        public void UpdateTheme()
        {
            Invalidate();  // triggers OnPaintBackground ? redraws card
            bool dark = ThemeManager.CurrentTheme == ThemeManager.Theme.Dark;
            _label.ForeColor    = dark ? Color.FromArgb(241, 241, 241) : Color.FromArgb(30, 30, 30);
            _subLabel.ForeColor = dark ? Color.FromArgb(160, 160, 160) : Color.FromArgb(100, 100, 100);
            _bar.ForeColor      = Color.FromArgb(0, 122, 204);
        }

        // ?? Layout ????????????????????????????????????????????????????????????

        private void PositionCard()
        {
            if (Width == 0 || Height == 0) return;

            int cx = (Width  - CardW) / 2;
            int cy = (Height - CardH) / 2;
            _card = new Rectangle(cx, cy, CardW, CardH);

            // Label: top portion of card
            _label.SetBounds(cx + PadX, cy + PadY,
                             CardW - PadX * 2, 26);

            // Progress bar: middle
            _bar.SetBounds(cx + PadX, cy + PadY + 30,
                           CardW - PadX * 2, BarH);

            // Sub-label: bottom
            _subLabel.SetBounds(cx + PadX, cy + PadY + 50,
                                CardW - PadX * 2, 20);

            Invalidate();
        }

        // ?? Custom painting: dim backdrop + rounded card ??????????????????????

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Semi-transparent dim over everything behind us
            using (var dimBrush = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
                e.Graphics.FillRectangle(dimBrush, ClientRectangle);

            if (_card.Width == 0) return;

            bool dark = ThemeManager.CurrentTheme == ThemeManager.Theme.Dark;
            Color cardBack   = dark ? Color.FromArgb(45, 45, 48) : Color.FromArgb(250, 250, 252);
            Color cardBorder = Color.FromArgb(0, 122, 204);   // VS blue accent always

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Drop shadow (simple offset fill)
            using (var shadowBrush = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
            {
                var shadow = Rectangle.Inflate(_card, 2, 2);
                shadow.Offset(3, 3);
                FillRoundedRect(e.Graphics, shadowBrush, shadow, CornerR);
            }

            // Card background
            using (var cardBrush = new SolidBrush(cardBack))
                FillRoundedRect(e.Graphics, cardBrush, _card, CornerR);

            // Accent top border (2 px blue line at top of card)
            using (var accentPen = new Pen(cardBorder, 2f))
            {
                int r = CornerR;
                e.Graphics.DrawLine(accentPen,
                    _card.X + r, _card.Y + 1,
                    _card.Right - r, _card.Y + 1);
            }

            // Card outline
            using (var borderPen = new Pen(Color.FromArgb(80, cardBorder), 1f))
                DrawRoundedRect(e.Graphics, borderPen, _card, CornerR);
        }

        protected override void OnPaint(PaintEventArgs e) { /* children paint themselves */ }

        // ?? Helpers ???????????????????????????????????????????????????????????

        private static void FillRoundedRect(Graphics g, Brush brush, Rectangle r, int radius)
        {
            using (var path = RoundedRectPath(r, radius))
                g.FillPath(brush, path);
        }

        private static void DrawRoundedRect(Graphics g, Pen pen, Rectangle r, int radius)
        {
            using (var path = RoundedRectPath(r, radius))
                g.DrawPath(pen, path);
        }

        private static GraphicsPath RoundedRectPath(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X,             r.Y,              d, d, 180, 90);
            path.AddArc(r.Right - d,     r.Y,              d, d, 270, 90);
            path.AddArc(r.Right - d,     r.Bottom - d,     d, d,   0, 90);
            path.AddArc(r.X,             r.Bottom - d,     d, d,  90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
