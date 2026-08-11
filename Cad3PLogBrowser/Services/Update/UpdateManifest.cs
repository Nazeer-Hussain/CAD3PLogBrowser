namespace Cad3PLogBrowser.Services.Update
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents the remote version manifest JSON file hosted on the project's
    /// GitHub Pages site (docs/version.json in the repo). Expected format:
    /// {
    ///   "version": "4.0.2",
    ///   "downloadUrl": "https://nazeer-hussain.github.io/CAD3PLogBrowser/downloads/Cad3PLogBrowser-4.0.2.zip",
    ///   "releaseNotes": "Bug fixes and performance improvements.",
    ///   "mandatory": false,
    ///   "sha256": "a3f2...e9b1",
    ///   "downloadSizeBytes": 4567890
    /// }
    /// downloadUrl now points to a portable .zip (exe + config + Help folder), not
    /// a bare exe -- see UpdateService.ExtractUpdate/ApplyUpdate.
    /// </summary>
    [DataContract]
    public class UpdateManifest
    {
        /// <summary>Latest available version string (e.g. "3.1.0.0").</summary>
        [DataMember(Name = "version")]
        public string Version { get; set; }

        /// <summary>Direct URL to download the new release's portable .zip.</summary>
        [DataMember(Name = "downloadUrl")]
        public string DownloadUrl { get; set; }

        /// <summary>Human-readable release notes shown in the update dialog.</summary>
        [DataMember(Name = "releaseNotes")]
        public string ReleaseNotes { get; set; }

        /// <summary>
        /// When true the update dialog does not offer a "Later" button.
        /// Use for critical security patches.
        /// </summary>
        [DataMember(Name = "mandatory")]
        public bool Mandatory { get; set; }

        /// <summary>
        /// Optional SHA-256 hex digest of the EXE (ENH-6 / BUG-3 protection).
        /// When present the downloaded file is verified before ApplyUpdate is called.
        /// Leave null/empty to skip verification.
        /// </summary>
        [DataMember(Name = "sha256")]
        public string Sha256 { get; set; }

        /// <summary>
        /// Optional size of the download in bytes.
        /// Used to show "X.X MB" in the update dialog before downloading (ENH-5).
        /// 0 means unknown.
        /// </summary>
        [DataMember(Name = "downloadSizeBytes")]
        public long DownloadSizeBytes { get; set; }
    }
}
