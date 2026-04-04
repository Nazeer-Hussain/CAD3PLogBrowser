using System;
using System.Windows.Forms;

namespace Cad3PLogBrowser
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var mainForm = new mainFrm();

            if (args.Length > 0)
                mainForm.OpenFilePath(args[0]);

            Application.Run(mainForm);
        }
    }
}
