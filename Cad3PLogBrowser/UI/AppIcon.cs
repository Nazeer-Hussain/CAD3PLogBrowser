using System.Drawing;
using System.Reflection;

namespace Cad3PLogBrowser.UI
{
    /// <summary>
    /// Shared application icon (cad3plog.ico, baked into the EXE via
    /// ApplicationIcon) for dialogs that don't otherwise inherit MainForm's
    /// Designer-embedded Icon -- keeps every window's title bar/taskbar
    /// thumbnail consistent with the app's actual branding.
    /// </summary>
    internal static class AppIcon
    {
        private static Icon _cached;

        public static Icon Get()
        {
            if (_cached == null)
                _cached = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            return _cached;
        }
    }
}
