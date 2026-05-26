namespace Cad3PLogBrowser.Services.Update
{
    using System;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles checking for, downloading, and applying application updates.
    /// </summary>
    /// <remarks>
    /// Update flow:
    /// 1. Fetch a small JSON manifest from a known URL (GitHub raw or releases).
    /// 2. Compare manifest version with the running assembly version.
    /// 3. If newer: download the new EXE to a temp path.
    /// 4. Write a batch script that waits for the current process to exit,
    ///    copies the downloaded EXE over the running one, then restarts.
    /// 5. Launch the batch script and signal the app to close.
    /// </remarks>
    public class UpdateService
    {
        // ?? Events ????????????????????????????????????????????????????????????

        /// <summary>Raised periodically during download. Value is 0–100.</summary>
        public event Action<int> DownloadProgressChanged;

        // ?? Fields ????????????????????????????????????????????????????????????

        private readonly string _manifestUrl;
        private WebClient _activeClient;

        // ?? Constructor ???????????????????????????????????????????????????????

        public UpdateService(string manifestUrl)
        {
            if (string.IsNullOrWhiteSpace(manifestUrl))
                throw new ArgumentNullException("manifestUrl");

            _manifestUrl = manifestUrl;
        }

        // ?? Public API ????????????????????????????????????????????????????????

        /// <summary>
        /// Fetches the remote manifest and returns it, or null on any failure.
        /// Non-throwing by design — callers should treat null as "no update available".
        /// </summary>
        public UpdateManifest FetchManifest()
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.UserAgent] = GetUserAgent();
                    string json = client.DownloadString(_manifestUrl);
                    return DeserializeManifest(json);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Asynchronously fetches the manifest. Returns null on failure.
        /// </summary>
        public Task<UpdateManifest> FetchManifestAsync()
        {
            return Task.Run<UpdateManifest>(() => FetchManifest());
        }

        /// <summary>
        /// Returns true when <paramref name="manifest"/> describes a version
        /// newer than the currently running assembly.
        /// </summary>
        public bool IsUpdateAvailable(UpdateManifest manifest)
        {
            if (manifest == null || string.IsNullOrWhiteSpace(manifest.Version))
                return false;

            Version remote;
            if (!Version.TryParse(manifest.Version, out remote))
                return false;

            Version current = Assembly.GetExecutingAssembly().GetName().Version;
            return remote > current;
        }

        /// <summary>
        /// Downloads the new EXE to a temporary file.
        /// Reports progress via <see cref="DownloadProgressChanged"/>.
        /// Returns the temp path on success, or null on failure/cancellation.
        /// </summary>
        public Task<string> DownloadUpdateAsync(string downloadUrl)
        {
            return Task.Run<string>(() =>
            {
                string tempPath = Path.Combine(Path.GetTempPath(),
                    "Cad3PLogBrowser_update_" + Guid.NewGuid().ToString("N") + ".exe");

                try
                {
                    using (_activeClient = new WebClient())
                    {
                        _activeClient.Headers[HttpRequestHeader.UserAgent] = GetUserAgent();

                        _activeClient.DownloadProgressChanged += (s, e) =>
                        {
                            var handler = DownloadProgressChanged;
                            if (handler != null)
                                handler(e.ProgressPercentage);
                        };

                        // Synchronous wait inside the background task
                        var tcs = new System.Threading.ManualResetEventSlim(false);
                        Exception downloadError = null;

                        _activeClient.DownloadFileCompleted += (s, e) =>
                        {
                            downloadError = e.Error;
                            tcs.Set();
                        };

                        _activeClient.DownloadFileAsync(new Uri(downloadUrl), tempPath);
                        tcs.Wait();

                        if (downloadError != null)
                            throw downloadError;
                    }

                    _activeClient = null;
                    return tempPath;
                }
                catch
                {
                    _activeClient = null;
                    TryDeleteFile(tempPath);
                    return null;
                }
            });
        }

        /// <summary>
        /// Cancels any in-progress download.
        /// </summary>
        public void CancelDownload()
        {
            try { _activeClient?.CancelAsync(); }
            catch { /* ignore */ }
        }

        /// <summary>
        /// Writes a self-replacement batch script to disk, launches it, and
        /// returns the script path so the caller can optionally delete it later.
        ///
        /// The batch script:
        ///   1. Waits until the current process exits (max ~30 s).
        ///   2. Copies <paramref name="newExePath"/> over <paramref name="currentExePath"/>.
        ///   3. Restarts the application.
        ///   4. Deletes itself.
        /// </summary>
        /// <param name="currentExePath">Full path of the currently running EXE.</param>
        /// <param name="newExePath">Full path of the downloaded replacement EXE.</param>
        /// <param name="currentPid">Process ID of the running app (to wait for).</param>
        /// <returns>Path of the launched batch script.</returns>
        public string ApplyUpdate(string currentExePath, string newExePath, int currentPid)
        {
            string scriptPath = Path.Combine(Path.GetTempPath(),
                "Cad3PLogBrowser_updater_" + Guid.NewGuid().ToString("N") + ".bat");

            // Escape paths for batch (wrap in quotes; do not embed % characters)
            string script = string.Format(
                "@echo off\r\n" +
                "set TARGET={0}\r\n" +
                "set NEWEXE={1}\r\n" +
                "set PID={2}\r\n" +
                ":WAITLOOP\r\n" +
                "tasklist /FI \"PID eq %PID%\" 2>NUL | find /I \"%PID%\" >NUL\r\n" +
                "if not errorlevel 1 (\r\n" +
                "    timeout /T 1 /NOBREAK >NUL\r\n" +
                "    goto WAITLOOP\r\n" +
                ")\r\n" +
                "copy /Y \"%NEWEXE%\" \"%TARGET%\" >NUL\r\n" +
                "del \"%NEWEXE%\" >NUL 2>&1\r\n" +
                "start \"\" \"%TARGET%\"\r\n" +
                "del \"%~f0\"\r\n",
                currentExePath,
                newExePath,
                currentPid);

            File.WriteAllText(scriptPath, script, Encoding.ASCII);

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName        = "cmd.exe",
                Arguments       = string.Format("/C \"{0}\"", scriptPath),
                WindowStyle     = System.Diagnostics.ProcessWindowStyle.Hidden,
                CreateNoWindow  = true,
                UseShellExecute = false
            };

            System.Diagnostics.Process.Start(psi);
            return scriptPath;
        }

        // ?? Helpers ???????????????????????????????????????????????????????????

        private static UpdateManifest DeserializeManifest(string json)
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            var ser   = new DataContractJsonSerializer(typeof(UpdateManifest));
            using (var ms = new MemoryStream(bytes))
                return (UpdateManifest)ser.ReadObject(ms);
        }

        private static string GetUserAgent()
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            return string.Format("Cad3PLogBrowser/{0} (Windows; .NET Framework 4.8)", ver.ToString(3));
        }

        private static void TryDeleteFile(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch { /* ignore */ }
        }
    }
}
