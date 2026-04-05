using System;
using System.Reflection;
using System.Windows.Forms;

namespace Cad3PLogBrowser
{
    /// <summary>Displays application version, copyright, and description information.</summary>
    internal partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
            Text                        = string.Format("About {0}", AssemblyTitle);
            labelProductName.Text       = AssemblyProduct;
            labelVersion.Text           = string.Format("Version {0}", AssemblyVersion);
            labelCopyright.Text         = AssemblyCopyright;
            labelCompanyName.Text       = AssemblyCompany;
            textBoxDescription.Text     = AssemblyDescription;
        }

        #region Assembly attribute accessors

        public string AssemblyTitle
        {
            get
            {
                var attrs = Assembly.GetExecutingAssembly()
                    .GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attrs.Length > 0)
                {
                    var title = ((AssemblyTitleAttribute)attrs[0]).Title;
                    if (!string.IsNullOrEmpty(title)) return title;
                }
                return System.IO.Path.GetFileNameWithoutExtension(
                    Assembly.GetExecutingAssembly().Location);
            }
        }

        public string AssemblyVersion =>
            Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public string AssemblyDescription
        {
            get
            {
                var attrs = Assembly.GetExecutingAssembly()
                    .GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                return attrs.Length == 0 ? "" : ((AssemblyDescriptionAttribute)attrs[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                var attrs = Assembly.GetExecutingAssembly()
                    .GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                return attrs.Length == 0 ? "" : ((AssemblyProductAttribute)attrs[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                var attrs = Assembly.GetExecutingAssembly()
                    .GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                return attrs.Length == 0 ? "" : ((AssemblyCopyrightAttribute)attrs[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                var attrs = Assembly.GetExecutingAssembly()
                    .GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                return attrs.Length == 0 ? "" : ((AssemblyCompanyAttribute)attrs[0]).Company;
            }
        }

        #endregion

        private void AboutForm_Load(object sender, EventArgs e) { }
    }
}
