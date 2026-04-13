using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Cad3PLogBrowser.Services;

namespace Cad3PLogBrowser.Managers
{
    /// <summary>
    /// Professional welcome panel shown when visualization tabs are empty.
    /// Provides clear instructions and visual guidance.
    /// </summary>
    public class VisualizationWelcomePanel : UserControl
    {
        private readonly string _title;
        private readonly string _description;
        private readonly string[] _instructions;
        private readonly Color _accentColor;

        public VisualizationWelcomePanel(string title, string description, string[] instructions, Color accentColor)
        {
            _title = title;
            _description = description;
            _instructions = instructions;
            _accentColor = accentColor;

            this.DoubleBuffered = true;
            this.Dock = DockStyle.Fill;
            this.BackColor = ThemeManager.BackgroundColor;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Calculate center position
            int centerX = this.Width / 2;
            int centerY = this.Height / 2;

            // Draw icon/symbol at top
            using (var iconFont = new Font("Segoe UI", 32f, FontStyle.Bold))
            using (var iconBrush = new SolidBrush(_accentColor))
            {
                var iconSize = g.MeasureString(_title[0].ToString(), iconFont);
                g.DrawString(_title[0].ToString(), iconFont, iconBrush,
                    centerX - iconSize.Width / 2, centerY - 150);
            }

            // Draw title
            using (var titleFont = new Font("Segoe UI", 18f, FontStyle.Bold))
            using (var titleBrush = new SolidBrush(ThemeManager.ForegroundColor))
            {
                var titleSize = g.MeasureString(_title, titleFont);
                g.DrawString(_title, titleFont, titleBrush,
                    centerX - titleSize.Width / 2, centerY - 100);
            }

            // Draw description
            using (var descFont = new Font("Segoe UI", 11f))
            using (var descBrush = new SolidBrush(Color.FromArgb(150, ThemeManager.ForegroundColor)))
            {
                var descSize = g.MeasureString(_description, descFont);
                g.DrawString(_description, descFont, descBrush,
                    centerX - descSize.Width / 2, centerY - 65);
            }

            // Draw separator line
            using (var pen = new Pen(_accentColor, 2))
            {
                g.DrawLine(pen, centerX - 100, centerY - 30, centerX + 100, centerY - 30);
            }

            // Draw instructions
            int y = centerY - 10;
            using (var instructFont = new Font("Segoe UI", 10f))
            using (var instructBrush = new SolidBrush(ThemeManager.ForegroundColor))
            {
                foreach (var instruction in _instructions)
                {
                    var size = g.MeasureString(instruction, instructFont);
                    g.DrawString(instruction, instructFont, instructBrush,
                        centerX - size.Width / 2, y);
                    y += 30;
                }
            }

            // Draw "Get Started" hint at bottom
            using (var hintFont = new Font("Segoe UI", 9f, FontStyle.Italic))
            using (var hintBrush = new SolidBrush(Color.FromArgb(100, ThemeManager.ForegroundColor)))
            {
                string hint = "Open a log file (Ctrl+O) to begin analysis";
                var hintSize = g.MeasureString(hint, hintFont);
                g.DrawString(hint, hintFont, hintBrush,
                    centerX - hintSize.Width / 2, this.Height - 50);
            }
        }
    }
}
