namespace Cad3PLogBrowser.Models
{
    using System;

    /// <summary>
    /// Represents a single line from a log file with parsed metadata.
    /// This is the core data structure for representing log information throughout the application.
    /// </summary>
    /// <example>
    /// Example log line: "2025-01-15 10:23:45: I: CADSystem::OpenFile [ENTER]"
    /// Would be parsed into:
    /// - LineNumber = 42
    /// - Text = "2025-01-15 10:23:45: I: CADSystem::OpenFile [ENTER]"
    /// - Timestamp = 2025-01-15 10:23:45
    /// - Level = Info
    /// - IsApiCall = true
    /// - IsCallEnter = true
    /// - ApiName = "CADSystem::OpenFile"
    /// </example>
    public class LogEntry
    {
        /// <summary>
        /// Gets or sets the sequential line number in the log file.
        /// This is a 1-based index (first line = 1, not 0).
        /// </summary>
        /// <remarks>
        /// Used for:
        /// - Displaying line numbers in the UI
        /// - Jumping to specific lines
        /// - Matching ENTER/EXIT pairs
        /// </remarks>
        public int LineNumber { get; set; }

        /// <summary>
        /// Gets or sets the raw text content of the log line exactly as it appears in the file.
        /// This is the complete, unmodified line including all whitespace and formatting.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when this log entry was created.
        /// Null if the log line doesn't contain a parseable timestamp.
        /// </summary>
        /// <remarks>
        /// Timestamps are typically in format: "YYYY-MM-DD HH:MM:SS.fff"
        /// Used for:
        /// - Time range filtering
        /// - Chronological sorting
        /// - Performance analysis
        /// </remarks>
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the log severity level.
        /// </summary>
        /// <seealso cref="LogLevel"/>
        public LogLevel Level { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this line represents an API call (ENTER or EXIT).
        /// True if the line contains "[ENTER]" or "[EXIT]" keywords.
        /// </summary>
        /// <remarks>
        /// API calls are special log lines that mark the beginning and end of method executions.
        /// These are used to build the call tree and calculate execution times.
        /// </remarks>
        public bool IsApiCall { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is an ENTER line.
        /// True if the line contains "[ENTER]" keyword.
        /// </summary>
        /// <remarks>
        /// ENTER lines mark the beginning of a method execution.
        /// Example: "CADSystem::OpenFile [ENTER]"
        /// </remarks>
        public bool IsCallEnter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is an EXIT line.
        /// True if the line contains "[EXIT]" keyword.
        /// </summary>
        /// <remarks>
        /// EXIT lines mark the end of a method execution.
        /// Example: "CADSystem::OpenFile [EXIT]"
        /// Must be paired with a corresponding ENTER line to calculate duration.
        /// </remarks>
        public bool IsCallExit { get; set; }

        /// <summary>
        /// Gets or sets the name of the API method being called.
        /// Format is typically "ClassName::MethodName" or "Namespace.ClassName::MethodName".
        /// Null if this is not an API call line.
        /// </summary>
        /// <example>
        /// - "CADSystem::OpenFile"
        /// - "ProToolkit.Drawing::CreateView"
        /// - "Assembly::AddComponent"
        /// </example>
        public string ApiName { get; set; }

        /// <summary>
        /// Gets or sets the source code file path where this log entry originated.
        /// Null if not available in the log line.
        /// </summary>
        /// <remarks>
        /// Some log formats include the source file:
        /// Example: "at CADSystem.OpenFile() in C:\Source\CADSystem.cs:line 142"
        /// </remarks>
        public string SourceFile { get; set; }

        /// <summary>
        /// Gets or sets the thread identifier for this log entry.
        /// Null if thread information is not available in the log line.
        /// </summary>
        /// <remarks>
        /// Thread IDs are used for:
        /// - Filtering logs by specific thread
        /// - Analyzing multi-threaded behavior
        /// - Separating concurrent operations
        /// Example format: "Thread-1234" or "0x1A2B"
        /// </remarks>
        public string ThreadId { get; set; }

        /// <summary>
        /// Returns a string representation of this log entry for debugging purposes.
        /// </summary>
        /// <returns>A formatted string showing key properties of this log entry.</returns>
        public override string ToString()
        {
            string preview = Text ?? string.Empty;
            return $"Line {LineNumber}: [{Level}] {preview.Substring(0, Math.Min(50, preview.Length))}...";
        }
    }

    /// <summary>
    /// Defines the severity levels for log entries.
    /// These map to common log level indicators found in log files.
    /// </summary>
    /// <remarks>
    /// Log level indicators in files:
    /// - 'D' or 'Debug' ? Debug
    /// - 'I' or 'Info' ? Info
    /// - 'W' or 'Warn' or 'Warning' ? Warning
    /// - 'E' or 'Error' ? Error
    /// </remarks>
    public enum LogLevel
    {
        /// <summary>
        /// Debug-level messages used for detailed diagnostic information.
        /// Typically used during development and troubleshooting.
        /// </summary>
        Debug = 0,

        /// <summary>
        /// Informational messages that highlight the progress of the application.
        /// Normal operational messages that require no action.
        /// </summary>
        Info = 1,

        /// <summary>
        /// Warning messages that indicate a potentially harmful situation.
        /// The application can continue but something unexpected happened.
        /// </summary>
        Warning = 2,

        /// <summary>
        /// Error messages that indicate a failure in the application.
        /// An error occurred but the application can recover and continue running.
        /// </summary>
        Error = 3
    }
}
