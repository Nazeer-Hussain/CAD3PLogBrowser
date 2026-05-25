namespace Cad3PLogBrowser.Services.Update
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents the remote version manifest JSON file hosted on GitHub.
    /// Expected format (version.json at repo root):
    /// {
    ///   "version": "2.1.0.0",
    ///   "downloadUrl": "https://github.com/.../releases/download/v2.1/Cad3PLogBrowser.exe",
    ///   "releaseNotes": "Bug fixes and performance improvements.",
    ///   "mandatory": false
    /// }
    /// </summary>
    [DataContract]
    public class UpdateManifest
    {
        /// <summary>Latest available version string (e.g. "2.1.0.0").</summary>
        [DataMember(Name = "version")]
        public string Version { get; set; }

        /// <summary>Direct URL to download the new EXE.</summary>
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
    }
}
