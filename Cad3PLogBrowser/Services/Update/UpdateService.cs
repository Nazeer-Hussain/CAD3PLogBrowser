namespace Cad3PLogBrowser.Services.Update
{
    using System;
    using System.IO;
    using System.IO.Compression;
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
    /// 1. Fetch a small JSON manifest (with retry + timeout) from GitHub Pages.
    /// 2. Compare manifest version with the running assembly version.
    /// 3. Optionally query download size via HTTP HEAD (ENH-5).
    /// 4. If newer: download the portable .zip release (exe + config + Help
    ///    folder) to a temp file with progress reporting and a stall watchdog
    ///    (BUG-19 -- WebClient has no built-in timeout, so a stalled connection
    ///    previously hung forever with the progress bar frozen and no error).
    /// 5. Optionally verify SHA-256 hash (ENH-6) and the ZIP local-file-header
    ///    magic bytes (adapted from the old single-exe MZ check).
    /// 6. Extract the zip to a temp folder.
    /// 7. Write a batch script that waits (with limit) for the current process
    ///    to exit, robocopies the extracted folder over the install directory
    ///    (replacing the exe, config, and Help folder in one pass), relaunches,
    ///    then deletes itself.
    /// 8. Launch the batch script and signal the app to close.
    /// </remarks>
    public class UpdateService
    {
        // ── Events ────────────────────────────────────────────────────────────

        /// <summary>Raised periodically during download. Value is 0–100.</summary>
        public event Action<int> DownloadProgressChanged;

        /// <summary>
        /// Raised during download with (bytesReceived, totalBytes, speedBytesPerSec).
        /// totalBytes is -1 when the server did not send Content-Length.
        /// </summary>
        public event Action<long, long, long> DownloadStatsChanged;

        // ── Constants ─────────────────────────────────────────────────────────

        /// <summary>HTTP timeout for manifest fetch and HEAD request (seconds).</summary>
        private const int FetchTimeoutSeconds = 15;

        /// <summary>Number of retry attempts for manifest fetch (ENH-7).</summary>
        private const int FetchRetryCount = 2;

        /// <summary>Delay between retries in seconds (ENH-7).</summary>
        private const int FetchRetryDelaySeconds = 5;

        /// <summary>Maximum seconds the batch updater will wait for the app to exit (BUG-4).</summary>
        private const int UpdaterWaitLimitSeconds = 60;

        /// <summary>
        /// BUG-19: WebClient.DownloadFileAsync has no built-in timeout, unlike the
        /// HttpWebRequest-based manifest/HEAD calls above. If a connection stalls
        /// (dead proxy, firewall silently dropping the transfer, flaky corporate
        /// network -- observed directly in this app's own update.log), the old
        /// unconditional wait blocked the background thread and the "Update Now"
        /// UI forever with the progress bar frozen at 0% and no error ever shown.
        /// This caps how long the download may go with zero incoming bytes before
        /// it's treated as failed; it does not cap total time for a slow-but-
        /// progressing transfer.
        /// </summary>
        private const int DownloadStallTimeoutSeconds = 45;

        // ── Fields ────────────────────────────────────────────────────────────

        private readonly string _manifestUrl;
        private volatile WebClient _activeClient;

        /// <summary>Set by <see cref="DownloadUpdateAsync"/> on failure so the UI can show
        /// a more specific message than "download failed" (e.g. distinguishing a stall
        /// timeout from a verification failure). Null after a successful download.</summary>
        public string LastDownloadError { get; private set; }

        // ── Constructor ───────────────────────────────────────────────────────

        public UpdateService(string manifestUrl)
        {
            if (string.IsNullOrWhiteSpace(manifestUrl))
                throw new ArgumentNullException(UpdateServiceStrings.ErrorManifestUrlNullOrEmpty);
            _manifestUrl = manifestUrl;
        }

        // ── Public API ────────────────────────────────────────────────────────

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
        /// Downloads the new release .zip to a temp file with progress + speed
        /// reporting. Verifies SHA-256 (ENH-6) and the ZIP local-file-header magic
        /// bytes before returning. Returns the temp .zip path on success, or null
        /// on failure/cancellation/verification failure (see
        /// <see cref="LastDownloadError"/> for the reason).
        /// </summary>
        public Task<string> DownloadUpdateAsync(string downloadUrl, string expectedSha256 = null)
        {
            LastDownloadError = null;

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
                        var speedTimer = System.Diagnostics.Stopwatch.StartNew();

                        // BUG-19: tracks the last time ANY bytes arrived, so a stalled
                        // connection can be detected and cancelled instead of hanging forever.
                        var progressLock    = new object();
                        var lastProgressUtc = DateTime.UtcNow;

                        _activeClient.DownloadProgressChanged += (s, e) =>
                        {
                            lock (progressLock) lastProgressUtc = DateTime.UtcNow;

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

                        var       waitHandle    = new ManualResetEventSlim(false);
                        Exception downloadError = null;

                        _activeClient.DownloadFileCompleted += (s, e) =>
                        {
                            downloadError = e.Error;
                            waitHandle.Set();
                        };

                        _activeClient.DownloadFileAsync(new Uri(downloadUrl), tempPath);

                        // Poll in 1s slices instead of an unbounded Wait() and bail out
                        // once no bytes have arrived for DownloadStallTimeoutSeconds.
                        bool timedOut = false;
                        while (!waitHandle.Wait(1000))
                        {
                            TimeSpan idle;
                            lock (progressLock) idle = DateTime.UtcNow - lastProgressUtc;
                            if (idle.TotalSeconds >= DownloadStallTimeoutSeconds)
                            {
                                timedOut = true;
                                _activeClient.CancelAsync();
                                waitHandle.Wait(TimeSpan.FromSeconds(5)); // let the cancel's completion event fire
                                break;
                            }
                        }

                        if (timedOut)
                        {
                            LastDownloadError = string.Format(
                                "No data received for {0} seconds. Check your network connection, firewall, or proxy settings.",
                                DownloadStallTimeoutSeconds);
                            TryDeleteFile(tempPath);
                            return null;
                        }

                        if (downloadError != null)
                            throw downloadError;
                    }

                    _activeClient = null;

                    // Verify the file is a valid ZIP archive (local file header magic "PK\x03\x04")
                    if (!IsValidZipFile(tempPath))
                    {
                        LastDownloadError = "The downloaded file is not a valid update package.";
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
                            LastDownloadError = "The downloaded file's checksum did not match the expected value.";
                            TryDeleteFile(tempPath);
                            return null;
                        }
                    }

                    return tempPath;
                }
                catch (Exception ex)
                {
                    _activeClient = null;
                    TryDeleteFile(tempPath);
                    if (LastDownloadError == null)
                        LastDownloadError = ex.Message;
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
        /// Extracts the downloaded release .zip to a fresh temp directory. Returns
        /// the extracted directory path, or null if extraction fails (corrupt
        /// archive, disk full, etc.) -- the caller treats this the same as a
        /// download failure.
        /// </summary>
        public string ExtractUpdate(string zipPath)
        {
            string extractDir = Path.Combine(Path.GetTempPath(),
                UpdateServiceStrings.UpdateExtractDirPrefix + Guid.NewGuid().ToString("N"));

            try
            {
                Directory.CreateDirectory(extractDir);
                ZipFile.ExtractToDirectory(zipPath, extractDir);
                return extractDir;
            }
            catch
            {
                TryDeleteDirectory(extractDir);
                return null;
            }
            finally
            {
                TryDeleteFile(zipPath);
            }
        }

        /// <summary>
        /// Writes a self-replacement batch script, launches it detached, and returns
        /// the script path. The script waits for the current process to exit (with
        /// a time limit, BUG-4), robocopies the extracted release over the install
        /// directory (replacing the exe, config, and Help folder in one pass —
        /// unlike the old single-exe copy, this release ships multiple files),
        /// relaunches the app, then deletes itself.
        /// </summary>
        /// <param name="extractedDir">Directory containing the freshly-extracted release
        /// (see <see cref="ExtractUpdate"/>).</param>
        /// <param name="installDir">Directory the running app is installed in
        /// (<c>Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)</c>).</param>
        /// <param name="exeFileName">The exe's file name (no path), used to relaunch
        /// (e.g. "Cad3PLogBrowser.exe").</param>
        /// <param name="currentPid">PID of the running process, so the script waits
        /// for it to exit before overwriting its files.</param>
        public string ApplyUpdate(string extractedDir, string installDir, string exeFileName, int currentPid)
        {
            string scriptPath = Path.Combine(Path.GetTempPath(),
                UpdateServiceStrings.UpdaterScriptNamePrefix + Guid.NewGuid().ToString("N") + UpdateServiceStrings.UpdaterScriptNameSuffix);

            // BUG-3: escape % as %% in the SET value (paths can contain %)
            string safeSource = extractedDir.Replace(UpdateServiceStrings.PercentNormal, UpdateServiceStrings.PercentEscaped);
            string safeTarget = installDir.Replace(UpdateServiceStrings.PercentNormal, UpdateServiceStrings.PercentEscaped);

            string script = string.Format(
                UpdateServiceStrings.BatchScriptTemplate,
                safeSource,
                safeTarget,
                exeFileName,
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

        // ── Helpers ───────────────────────────────────────────────────────────

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

        private static void TryDeleteDirectory(string path)
        {
            try { if (Directory.Exists(path)) Directory.Delete(path, recursive: true); }
            catch { /* ignore */ }
        }

        /// <summary>
        /// Checks the first four bytes of a file for the ZIP local-file-header
        /// magic number (0x50 0x4B 0x03 0x04 = "PK\x03\x04") that marks a valid
        /// ZIP archive. Replaces the old single-exe MZ/PE check now that releases
        /// are distributed as a portable .zip.
        /// </summary>
        private static bool IsValidZipFile(string path)
        {
            try
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (fs.Length < 4) return false;
                    int b0 = fs.ReadByte();
                    int b1 = fs.ReadByte();
                    int b2 = fs.ReadByte();
                    int b3 = fs.ReadByte();
                    return b0 == 0x50 && b1 == 0x4B && b2 == 0x03 && b3 == 0x04; // 'P' 'K' 0x03 0x04
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
