namespace Cad3PLogBrowser.Services.Update
{
    /// <summary>
    /// String constants for the UpdateService.
    /// Centralizes all log messages and user-facing strings for easier maintenance and potential localization.
    /// </summary>
    public static class UpdateServiceStrings
    {
        // ?? Log Messages ??????????????????????????????????????????????????????

        // FetchManifest
        public const string LogFetchManifestStarting = "FetchManifest: starting � URL={0}";
        public const string LogFetchManifestRetry = "FetchManifest: retry {0}/{1} after {2}s (last error: {3})";
        public const string LogFetchManifestHttpResponse = "FetchManifest: HTTP {0} � received {1} bytes";
        public const string LogFetchManifestParsedOk = "FetchManifest: parsed OK � remote version={0}";
        public const string LogFetchManifestAttemptFailed = "FetchManifest: attempt {0} failed � {1}";
        public const string LogFetchManifestRetriesExhausted = "FetchManifest: all retries exhausted � returning null";

        // IsUpdateAvailable
        public const string LogIsUpdateAvailableManifestNull = "IsUpdateAvailable: manifest is null or has no version � returning false";
        public const string LogIsUpdateAvailableCannotParseVersion = "IsUpdateAvailable: could not parse remote version '{0}' � returning false";
        public const string LogIsUpdateAvailableResult = "IsUpdateAvailable: remote={0}  current={1}  available={2}";

        // ?? User Agent ????????????????????????????????????????????????????????
        public const string UserAgentFormat = "Cad3PLogBrowser/{0} (Windows; .NET Framework 4.8)";

        // ?? File Names ????????????????????????????????????????????????????????
        public const string UpdateFileNamePrefix = "Cad3PLogBrowser_update_";
        public const string UpdateFileNameSuffix = ".zip";
        public const string UpdateExtractDirPrefix = "Cad3PLogBrowser_extract_";
        public const string UpdaterScriptNamePrefix = "Cad3PLogBrowser_updater_";
        public const string UpdaterScriptNameSuffix = ".bat";

        // ?? Error Messages ????????????????????????????????????????????????????
        public const string ErrorManifestUrlNullOrEmpty = "manifestUrl";

        // ?? Batch Script Template ?????????????????????????????????????????????
        // The portable release is a .zip (exe + config + Help folder), not a bare
        // exe, so applying an update is a directory copy (robocopy) rather than a
        // single-file copy. /IS /IT force robocopy to overwrite files that look
        // "the same" by size/timestamp -- without them, a file that happens to
        // match could be silently skipped, leaving a stale copy behind.
        public const string BatchScriptTemplate =
            "@echo off\r\n" +
            "set SOURCEDIR={0}\r\n" +
            "set TARGETDIR={1}\r\n" +
            "set TARGETEXE={2}\r\n" +
            "set PID={3}\r\n" +
            "set /a WAIT=0\r\n" +
            ":WAITLOOP\r\n" +
            "tasklist /FI \"PID eq %PID%\" 2>NUL | find /I \"%PID%\" >NUL\r\n" +
            "if not errorlevel 1 (\r\n" +
            "    timeout /T 1 /NOBREAK >NUL\r\n" +
            "    set /a WAIT+=1\r\n" +
            "    if %WAIT% lss {4} goto WAITLOOP\r\n" +
            ")\r\n" +
            "robocopy \"%SOURCEDIR%\" \"%TARGETDIR%\" /E /IS /IT /R:3 /W:1 /NFL /NDL /NJH /NJS /NC /NS >NUL\r\n" +
            "rmdir /S /Q \"%SOURCEDIR%\" >NUL 2>&1\r\n" +
            "start \"\" \"%TARGETDIR%\\%TARGETEXE%\"\r\n" +
            "del \"%~f0\"\r\n";

        // ?? Command Line Arguments ????????????????????????????????????????????
        public const string CmdExecutable = "cmd.exe";
        public const string CmdArgumentsFormat = "/C \"{0}\"";
        public const string StartProcessEmptyTitle = "";

        // ?? Validation Constants ??????????????????????????????????????????????
        public const string Sha256HexFormat = "{0:x2}";
        public const string PercentEscaped = "%%";
        public const string PercentNormal = "%";
    }
}
