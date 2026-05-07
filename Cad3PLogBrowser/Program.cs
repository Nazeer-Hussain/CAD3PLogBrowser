using System;
using System.IO;
using System.Windows.Forms;

namespace Cad3PLogBrowser
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Wire up global exception handlers BEFORE anything else so no crash
            // goes unnoticed — exceptions from async void, background threads, and
            // the WinForms message-pump are all caught and written to a log file.
            Application.ThreadException += OnThreadException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var mainForm = new MainForm();

            if (args.Length > 0)
                mainForm.OpenFilePath(args[0]);

            Application.Run(mainForm);
        }

        private static void OnThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            HandleUnexpectedException(e.Exception);
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleUnexpectedException(e.ExceptionObject as Exception);
        }

        private static void HandleUnexpectedException(Exception ex)
        {
            try
            {
                string logPath = Path.Combine(Path.GetTempPath(), "Cad3PLogBrowser.err");
                string entry = string.Format(
                    "[{0}] Unhandled exception:{1}{2}{1}{1}",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Environment.NewLine,
                    ex?.ToString() ?? "(null)");
                File.AppendAllText(logPath, entry);

                string message = string.Format(
                    "An unexpected error occurred:\n\n{0}\n\nDetails written to:\n{1}",
                    ex?.Message ?? "Unknown error", logPath);

                MessageBox.Show(message, "Unexpected Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                // Last resort – if even the error handler fails, swallow silently.
            }
        }
    }
}

