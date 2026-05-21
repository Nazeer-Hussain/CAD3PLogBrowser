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
            // Wire global handlers BEFORE anything else so no exception ever
            // silently terminates the process. Covers async void, background
            // threads, and WinForms message-pump exceptions.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException                        += OnThreadException;
            AppDomain.CurrentDomain.UnhandledException        += OnUnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var mainForm = new MainForm();

            if (args.Length > 0)
                mainForm.OpenFilePath(args[0]);

            Application.Run(mainForm);
        }

        // ?? Unhandled exception sinks ?????????????????????????????????????????

        private static void OnThreadException(object sender,
            System.Threading.ThreadExceptionEventArgs e) =>
            HandleFatalException(e.Exception);

        private static void OnUnhandledException(object sender,
            UnhandledExceptionEventArgs e) =>
            HandleFatalException(e.ExceptionObject as Exception);

        private static void HandleFatalException(Exception ex)
        {
            try
            {
                // Write details to a per-session crash log so subsequent crashes don't
                // overwrite earlier ones and the file is user-identifiable.
                // D10: use a timestamp-based filename instead of the shared static name.
                string logPath = Path.Combine(
                    Path.GetTempPath(),
                    string.Format("Cad3PLogBrowser_{0}.err",
                        DateTime.Now.ToString("yyyyMMdd_HHmmss")));
                File.AppendAllText(logPath,
                    string.Format("[{0}] Unhandled exception:{1}{2}{1}{1}",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Environment.NewLine,
                        ex != null ? ex.ToString() : "(null exception)"));

                MessageBox.Show(
                    string.Format(
                        "An unexpected error occurred:\n\n{0}\n\nDetails have been written to:\n{1}\n\nThe application will continue running.",
                        ex != null ? ex.Message : "Unknown error",
                        logPath),
                    "Unexpected Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch
            {
                // Last resort – swallow if even the error handler fails.
            }
        }
    }
}

