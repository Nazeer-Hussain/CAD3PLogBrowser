using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Cad3PLogBrowser
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            mainFrm mainfrm = new mainFrm();
            if (args.Length > 0)
            {
                mainfrm.activeFilename = args[0].ToString();
                mainfrm.OpenFilename ( args[0].ToString());
            }
            Application.Run(mainfrm);
        }
    }
}
