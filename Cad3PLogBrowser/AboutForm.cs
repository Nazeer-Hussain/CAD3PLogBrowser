using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Cad3PLogBrowser.Services;

namespace Cad3PLogBrowser
{
    /// <summary>Displays application version, copyright, and description information.</summary>
    internal class AboutForm : Form
    {
        public AboutForm() { BuildUI(); }

        private void BuildUI()
        {
            SuspendLayout();

            // All positions derived from constants — easy to adjust.
            const int margin = 16;
            const int logoSz = 80;
            const int lx     = margin + logoSz + margin; // 112 — left of text column
            const int lw     = 360;                       // text column width
            const int ly     = margin;
            const int gap    = 24;
            const int descH  = 70;
            const int btnW   = 88;
            const int btnH   = 28;
            const int formW  = lx + lw + margin;                              // 488
            const int formH  = ly + gap * 4 + descH + margin + btnH + margin; // ~272

            Text            = string.Format(UI.AppStrings.AboutFormTitle, AssemblyTitle);
            ClientSize      = new Size(formW, formH);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            ShowIcon        = false;
            ShowInTaskbar   = false;
            StartPosition   = FormStartPosition.CenterParent;

            // ?? Logo ??????????????????????????????????????????????????????????
            var logo = new PictureBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode    = PictureBoxSizeMode.Zoom,
                Location    = new Point(margin, margin),
                Size        = new Size(logoSz, logoSz),
            };
            try { logo.Image = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location)?.ToBitmap(); }
            catch { /* no icon - leave blank */ }

            // ?? Info labels ???????????????????????????????????????????????????
            var lblProduct = MakeLabel(AssemblyProduct,                                  lx, ly,              lw, bold: true);
            var lblVersion = MakeLabel(string.Format(UI.AppStrings.AboutLabelVersion, AssemblyVersion), lx, ly + gap,        lw);
            var lblCopy    = MakeLabel(AssemblyCopyright,                                lx, ly + gap * 2,    lw);
            var lblCompany = MakeLabel(AssemblyCompany,                                  lx, ly + gap * 3,    lw);

            // Description — Label is used instead of TextBox/RichTextBox because
            // TextBoxBase..ctor() calls Font.GetHeight()->GetDC(NULL) which throws
            // "Parameter is not valid" when the system DC cache is exhausted.
            var lblDesc = new Label
            {
                Text      = AssemblyDescription,
                Location  = new Point(lx, ly + gap * 4),
                Size      = new Size(lw, descH),
                AutoSize  = false,
                TextAlign = ContentAlignment.TopLeft,
            };

            // ?? OK button (bottom-right) ??????????????????????????????????????
            var btnOk = new Button
            {
                Text         = UI.AppStrings.AboutButtonOK,
                DialogResult = DialogResult.Cancel,
                Size         = new Size(btnW, btnH),
                Location     = new Point(formW - margin - btnW, formH - margin - btnH),
            };
            AcceptButton = btnOk;
            CancelButton = btnOk;

            Controls.AddRange(new Control[] { logo, lblProduct, lblVersion, lblCopy, lblCompany, lblDesc, btnOk });

            ResumeLayout(false);

            Load += (s, e) => ThemeManager.ApplyTheme(this);
        }

        private static Label MakeLabel(string text, int x, int y, int width, bool bold = false)
        {
            return new Label
            {
                Text      = text,
                Location  = new Point(x, y),
                Size      = new Size(width, 22),
                AutoSize  = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Font      = bold ? new Font(SystemFonts.DefaultFont, FontStyle.Bold)
                                 : SystemFonts.DefaultFont,
            };
        }

        // ?? Assembly attribute helpers ?????????????????????????????????????????

        private string AssemblyTitle
        {
            get
            {
                object[] a = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (a.Length > 0)
                {
                    string t = ((AssemblyTitleAttribute)a[0]).Title;
                    if (!string.IsNullOrEmpty(t)) return t;
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
            }
        }

        private string AssemblyVersion
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(3); }
        }

        private string AssemblyDescription
        {
            get
            {
                object[] a = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                return a.Length == 0 ? "" : ((AssemblyDescriptionAttribute)a[0]).Description;
            }
        }

        private string AssemblyProduct
        {
            get
            {
                object[] a = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                return a.Length == 0 ? "" : ((AssemblyProductAttribute)a[0]).Product;
            }
        }

        private string AssemblyCopyright
        {
            get
            {
                object[] a = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                return a.Length == 0 ? "" : ((AssemblyCopyrightAttribute)a[0]).Copyright;
            }
        }

        private string AssemblyCompany
        {
            get
            {
                object[] a = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                return a.Length == 0 ? "" : ((AssemblyCompanyAttribute)a[0]).Company;
            }
        }
    }
}
