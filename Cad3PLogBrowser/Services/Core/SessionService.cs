using System;
using System.Collections.Generic;
using System.IO;
using Cad3PLogBrowser.Services;

namespace Cad3PLogBrowser.Services.Core
{
    /// <summary>
    /// A8 — Session Restore. Saves and restores the list of last-opened files.
    /// Stored in AppSettings so it is portable (no registry).
    /// </summary>
    public class SessionService
    {
        private readonly AppSettings _settings;

        public SessionService(AppSettings settings)
        {
            _settings = settings;
        }

        /// <summary>Returns the list of files that were open in the last session.</summary>
        public List<string> GetLastSessionFiles()
        {
            var result = new List<string>();
            if (_settings.LastSessionFiles == null) return result;
            foreach (var f in _settings.LastSessionFiles)
                if (File.Exists(f)) result.Add(f);
            return result;
        }

        /// <summary>Persists the current set of open files for next-session restore.</summary>
        public void SaveSession(IEnumerable<string> openFiles)
        {
            _settings.LastSessionFiles = new List<string>(openFiles);
            _settings.Save();
        }
    }
}
