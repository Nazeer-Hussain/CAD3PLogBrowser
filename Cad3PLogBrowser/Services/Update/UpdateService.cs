namespace Cad3PLogBrowser.Services.Update
{
    using System;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Runtime.Serialization.Json;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles checking for, downloading, and applying application updates.
    /// </summary>
    /// <remarks>
    /// Update flow:
    /// 1. Fetch a small JSON manifest (with retry + timeout).
    /// 2. Compare manifest version with the running assembly version.
    /// 3. Optionally query download size via HTTP HEAD (ENH-5).
    /// 4. If newer: download the new EXE to a temp path with progress reporting.
    /// 5. Optionally verify SHA-256 hash (ENH-6) and PE magic bytes (ENH-2).
    /// 6. Write a batch script that waits (with limit) for the current process
    ///    to exit, copies the downloaded EXE over the running one, then restarts.
    /// 7. Launch the batch script and signal the app to close.
    /// </remarks>
    public class UpdateService
    {
        // ?? Events ????????????????????????????????????????????????????????????

        /// <summary>Raised periodically during download. Value is 0–100.</summary>
        public event Action<int> DownloadProgressChanged;

        /// <summary>
        /// Raised during download with (bytesReceived, totalBytes, speedBytesPerSec).
        /// totalBytes is -1 when the server did not send Content-Length.
        /// </summary>
        public event Action<long, long, long> DownloadStatsChanged;

        // ?? Constants ?????????????????????????????????????????????????????????

        /// <summary>HTTP timeout for manifest fetch and HEAD request (seconds).</summary>
        private const int FetchTimeoutSeconds = 15;

        /// <summary>Number of retry attempts for manifest fetch (ENH-7).</summary>
        private const int FetchRetryCount = 2;

        /// <summary>Delay between retries in seconds (ENH-7).</summary>
        private const int FetchRetryDelaySeconds = 5;

        /// <summary>Maximum seconds the batch updater will wait for the app to exit (BUG-4).</summary>
        private const int UpdaterWaitLimitSeconds = 60;

        // ?? Fields ????????????????????????????????????????????????????????????

        private readonly string _manifestUrl;
        private volatile WebClient _activeClient;

        // ?? Constructor ???????????????????????????????????????????????????????

        public UpdateService(string manifestUrl)
        {
            if (string.IsNullOrWhiteSpace(manifestUrl))
                throw new ArgumentNullException(UpdateServiceStrings.ErrorManifestUrlNullOrEmpty);
            _manifestUrl = manifestUrl;
        }

        // ?? Public API ????????????????????????????????????????????????????????

        /// <summary>
        /// Fetches the remote manifest with timeout and retry (ENH-7, BUG-2).
        /// Throws the last network/parse exception once retries are exhausted, so
        /// callers can tell "fetch failed" apart from "fetched fine, no update" —
        /// silently returning null for both made a real network error look
        /// identical to "you're already up to date" (G12).
        /// </summary>
        public UpdateManifest FetchManifest()
        {
            UpdateLogger.Log(UpdateServiceStrings.LogFetchManifestStarting, _manifestUrl);
            Exception lastEx = null;
            for (int attempt = 0; attempt <= FetchRetryCount; attempt++)
            {
                if (attempt > 0)
                {
                    UpdateLogger.Log(UpdateServiceStrings.LogFetchManifestRetry,
                        attempt, FetchRetryCount, FetchRetryDelaySeconds,
                        lastEx != null ? lastEx.Message : "?");
                    Thread.Sleep(FetchRetryDelaySeconds * 1000);
                }

                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(_manifestUrl);
                    request.Timeout   = FetchTimeoutSeconds * 1000;
                    request.UserAgent = GetUserAgent();
                    request.Method    = "GET";

                    using (var response = (HttpWebResponse)request.GetResponse())
                    using (var stream   = response.GetResponseStream())
                    using (var reader   = new StreamReader(stream, Encoding.UTF8))
                    {
                        string json = reader.ReadToEnd();
                        UpdateLogger.Log(UpdateServiceStrings.LogFetchManifestHttpResponse,
                            (int)response.StatusCode, json.Length);
                        var manifest = DeserializeManifest(json);
                        UpdateLogger.Log(UpdateServiceStrings.LogFetchManifestParsedOk, manifest.Version);
                        return manifest;
                    }
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    UpdateLogger.Log(UpdateServiceStrings.LogFetchManifestAttemptFailed, attempt, ex.Message);
                }
            }

            UpdateLogger.Log(UpdateServiceStrings.LogFetchManifestRetriesExhausted);
            throw lastEx;
        }

        /// <summary>Asynchronously fetches the manifest. Faults with the underlying
        /// exception on failure — see <see cref="FetchManifest"/>.</summary>
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
            {
                UpdateLogger.Log(UpdateServiceStrings.LogIsUpdateAvailableManifestNull);
                return false;
            }

            Version remote;
            if (!Version.TryParse(manifest.Version, out remote))
            {
                UpdateLogger.Log(UpdateServiceStrings.LogIsUpdateAvailableCannotParseVersion,
                    manifest.Version);
                return false;
            }

            Version current = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            bool available  = remote > current;
            UpdateLogger.Log(UpdateServiceStrings.LogIsUpdateAvailableResult,
                remote, current, available);
            return available;
        }

        /// <summary>
        /// Queries the download size via an HTTP HEAD request without downloading
        /// the file (ENH-5). Returns -1 if the server does not provide Content-Length
        /// or on any error.
        /// </summary>
        public long QueryDownloadSize(string downloadUrl)
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(downloadUrl);
                req.Method    = "HEAD";
                req.Timeout   = FetchTimeoutSeconds * 1000;
                req.UserAgent = GetUserAgent();

                using (var resp = (HttpWebResponse)req.GetResponse())
                    return resp.ContentLength;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>Async version of <see cref="QueryDownloadSize"/>.</summary>
        public Task<long> QueryDownloadSizeAsync(string downloadUrl)
        {
            return Task.Run<long>(() => QueryDownloadSize(downloadUrl));
        }

        /// <summary>
        /// Downloads the new EXE to a temp file with progress + speed reporting.
        /// Verifies SHA-256 (ENH-6) and PE magic bytes (ENH-2) before returning.
        /// Returns the temp path on success, or null on failure/cancellation/verification fail.
        /// </summary>
        public Task<string> DownloadUpdateAsync(string downloadUrl, string expectedSha256 = null)
        {
            return Task.Run<string>(() =>
            {
                string tempPath = Path.Combine(Path.GetTempPath(),
                    UpdateServiceStrings.UpdateFileNamePrefix + Guid.NewGuid().ToString("N") + UpdateServiceStrings.UpdateFileNameSuffix);

                try
                {
                    using (_activeClient = new WebClient())
                    {
                        _activeClient.Headers[HttpRequestHeader.UserAgent] = GetUserAgent();

                        // Speed tracking
                        var    speedTimer    = System.Diagnostics.Stopwatch.StartNew();

                        _activeClient.DownloadProgressChanged += (s, e) =>
                        {
                            // Progress percent
                            var pctHandler = DownloadProgressChanged;
                            if (pctHandler != null)
                                pctHandler(e.ProgressPercentage);

                            // Speed + ETA (ENH-1)
                            var statsHandler = DownloadStatsChanged;
                            if (statsHandler != null)
                            {
                                double elapsedSec = speedTimer.Elapsed.TotalSeconds;
                                long   speed      = elapsedSec > 0
                                    ? (long)(e.BytesReceived / elapsedSec)
                                    : 0;
                                statsHandler(e.BytesReceived, e.TotalBytesToReceive, speed);
                            }
                        };

                        var    waitHandle     = new ManualResetEventSlim(false);
                        Exception downloadError = null;

                        _activeClient.DownloadFileCompleted += (s, e) =>
                        {
                            downloadError = e.Error;
                            waitHandle.Set();
                        };

                        _activeClient.DownloadFileAsync(new Uri(downloadUrl), tempPath);
                        waitHandle.Wait();

                        if (downloadError != null)
                            throw downloadError;
                    }

                    _activeClient = null;

                    // ENH-2: Verify the file is a valid Windows PE executable
                    if (!IsValidPeFile(tempPath))
                    {
                        TryDeleteFile(tempPath);
                        return null;
                    }

                    // ENH-6: Verify SHA-256 hash when provided in the manifest
                    if (!string.IsNullOrWhiteSpace(expectedSha256))
                    {
                        string actualHash = ComputeSha256(tempPath);
                        if (!string.Equals(actualHash, expectedSha256.ToLowerInvariant(),
                                StringComparison.OrdinalIgnoreCase))
                        {
                            TryDeleteFile(tempPath);
                            return null;
                        }
                    }

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

        /// <summary>Cancels any in-progress download.</summary>
        public void CancelDownload()
        {
            try { _activeClient?.CancelAsync(); }
            catch { /* ignore */ }
        }

        /// <summary>
        /// Writes a self-replacement batch script, launches it detached, and returns
        /// the script path.
        ///
        /// Fixes applied:
        ///   BUG-3 — paths with % characters are escaped as %% in the SET lines.
        ///   BUG-4 — a counter limits the wait loop to <see cref="UpdaterWaitLimitSeconds"/> seconds.
        /// </summary>
        public string ApplyUpdate(string currentExePath, string newExePath, int currentPid)
        {
            string scriptPath = Path.Combine(Path.GetTempPath(),
                UpdateServiceStrings.UpdaterScriptNamePrefix + Guid.NewGuid().ToString("N") + UpdateServiceStrings.UpdaterScriptNameSuffix);

            // BUG-3: escape % as %% in the SET value (paths can contain %)
            string safeTarget = currentExePath.Replace(UpdateServiceStrings.PercentNormal, UpdateServiceStrings.PercentEscaped);
            string safeNew    = newExePath.Replace(UpdateServiceStrings.PercentNormal, UpdateServiceStrings.PercentEscaped);

            // BUG-4: add a counter so the wait loop terminates after the limit
            string script = string.Format(
                UpdateServiceStrings.BatchScriptTemplate,
                safeTarget,
                safeNew,
                currentPid,
                UpdaterWaitLimitSeconds);

            File.WriteAllText(scriptPath, script, Encoding.ASCII);

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName        = UpdateServiceStrings.CmdExecutable,
                Arguments       = string.Format(UpdateServiceStrings.CmdArgumentsFormat, scriptPath),
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
            return string.Format(UpdateServiceStrings.UserAgentFormat, ver.ToString(3));
        }

        private static void TryDeleteFile(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch { /* ignore */ }
        }

        /// <summary>
        /// ENH-2: Checks the first two bytes of a file for the MZ magic number (0x4D 0x5A)
        /// that marks a valid Windows Portable Executable.
        /// </summary>
        private static bool IsValidPeFile(string path)
        {
            try
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (fs.Length < 2) return false;
                    int b0 = fs.ReadByte();
                    int b1 = fs.ReadByte();
                    return b0 == 0x4D && b1 == 0x5A; // 'M' 'Z'
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// ENH-6: Computes the lowercase hex SHA-256 hash of a file.
        /// </summary>
        private static string ComputeSha256(string path)
        {
            using (var sha = SHA256.Create())
            using (var fs  = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte[] hash = sha.ComputeHash(fs);
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                    sb.AppendFormat(UpdateServiceStrings.Sha256HexFormat, b);
                return sb.ToString();
            }
        }
    }
}

