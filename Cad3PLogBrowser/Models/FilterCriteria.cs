namespace Cad3PLogBrowser.Models
{
    using System;

    /// <summary>
    /// Encapsulates all filter criteria that can be applied to log entries.
    /// This class holds all the parameters needed to filter a log file's content.
    /// </summary>
    /// <remarks>
    /// Multiple criteria can be combined - all active criteria must match (AND logic).
    /// Example: Filter for "error" text AND duration > 1000ms AND time range 10:00-11:00
    /// would only show error lines that took more than 1 second between 10 and 11 AM.
    /// </remarks>
    public class FilterCriteria
    {
        /// <summary>
        /// Gets or sets the text to search for in log lines.
        /// Empty or null means no text filter is applied.
        /// </summary>
        /// <remarks>
        /// Behavior depends on <see cref="UseRegex"/> and <see cref="IsCaseSensitive"/> properties.
        /// Example: "OpenFile" will match lines containing "OpenFile" (case depends on settings).
        /// </remarks>
        public string SearchText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the search is case-sensitive.
        /// Default is false (case-insensitive matching).
        /// </summary>
        /// <example>
        /// If true: "ERROR" will NOT match "error"
        /// If false: "ERROR" WILL match "error"
        /// </example>
        public bool IsCaseSensitive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use regular expression pattern matching.
        /// Default is false (plain text matching).
        /// </summary>
        /// <remarks>
        /// When true, <see cref="SearchText"/> is treated as a regex pattern.
        /// Example patterns:
        /// - "Open.*File" matches "OpenFile", "OpenMyFile", "Open_File"
        /// - "\d{4}-\d{2}-\d{2}" matches date patterns like "2025-01-15"
        /// - "^Error:" matches lines starting with "Error:"
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// If the regex pattern in <see cref="SearchText"/> is invalid, 
        /// the filter operation will throw an exception.
        /// </exception>
        public bool UseRegex { get; set; }

        /// <summary>
        /// Gets or sets the minimum duration in milliseconds for call duration filtering.
        /// Null means no duration filter is applied.
        /// </summary>
        /// <remarks>
        /// Only applies to API call lines that have duration information (matched ENTER/EXIT pairs).
        /// Example: If set to 1000, only shows calls that took 1 second or longer.
        /// Lines without duration information will be excluded when this filter is active.
        /// </remarks>
        public int? MinimumDurationMs { get; set; }

        /// <summary>
        /// Gets or sets the start time for time range filtering.
        /// Null means no start time limit (show from beginning of log).
        /// </summary>
        /// <remarks>
        /// Only the time portion is used (date is ignored).
        /// Example: If set to 10:00:00, only shows entries at or after 10 AM.
        /// Lines without timestamp information will be excluded when this filter is active.
        /// </remarks>
        public DateTime? FromTime { get; set; }

        /// <summary>
        /// Gets or sets the end time for time range filtering.
        /// Null means no end time limit (show until end of log).
        /// </summary>
        /// <remarks>
        /// Only the time portion is used (date is ignored).
        /// Example: If set to 11:00:00, only shows entries at or before 11 AM.
        /// Lines without timestamp information will be excluded when this filter is active.
        /// </remarks>
        public DateTime? ToTime { get; set; }

        /// <summary>
        /// Gets or sets the thread ID to filter by.
        /// Null or empty means no thread filtering is applied.
        /// </summary>
        /// <remarks>
        /// Only shows log entries from the specified thread.
        /// Useful for analyzing multi-threaded applications or isolating specific thread behavior.
        /// Example: "Thread-1234" or "0x1A2B"
        /// </remarks>
        public string ThreadId { get; set; }

        /// <summary>
        /// Gets or sets the log level to filter by.
        /// Null means no log level filtering is applied (show all levels).
        /// </summary>
        /// <remarks>
        /// Only shows entries matching the specified log level.
        /// Example: If set to <see cref="LogLevel.Error"/>, only error lines are shown.
        /// </remarks>
        /// <seealso cref="LogLevel"/>
        public LogLevel? Level { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether any filter is currently active.
        /// This is a computed property based on whether any filter criteria is set.
        /// </summary>
        /// <value>
        /// True if at least one filter criterion is set; otherwise, false.
        /// </value>
        public bool IsActive
        {
            get
            {
                return !string.IsNullOrWhiteSpace(SearchText) ||
                       MinimumDurationMs.HasValue ||
                       FromTime.HasValue ||
                       ToTime.HasValue ||
                       !string.IsNullOrWhiteSpace(ThreadId) ||
                       Level.HasValue;
            }
        }

        /// <summary>
        /// Gets a human-readable description of the active filters.
        /// Used for displaying filter status in the UI.
        /// </summary>
        /// <returns>
        /// A formatted string describing all active filters, or "No filters active" if none are set.
        /// </returns>
        /// <example>
        /// Output examples:
        /// - "No filters active"
        /// - "Text: 'error' (case-sensitive)"
        /// - "Duration > 1000ms, Time: 10:00-11:00"
        /// - "Thread: Thread-1234, Level: Error"
        /// </example>
        public string GetDescription()
        {
            if (!IsActive)
                return "No filters active";

            var parts = new System.Collections.Generic.List<string>();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var textFilter = $"Text: '{SearchText}'";
                if (IsCaseSensitive) textFilter += " (case-sensitive)";
                if (UseRegex) textFilter += " (regex)";
                parts.Add(textFilter);
            }

            if (MinimumDurationMs.HasValue)
                parts.Add($"Duration > {MinimumDurationMs}ms");

            if (FromTime.HasValue || ToTime.HasValue)
            {
                var timeFilter = "Time: ";
                if (FromTime.HasValue)
                    timeFilter += FromTime.Value.ToString("HH:mm:ss");
                else
                    timeFilter += "start";

                timeFilter += "-";

                if (ToTime.HasValue)
                    timeFilter += ToTime.Value.ToString("HH:mm:ss");
                else
                    timeFilter += "end";

                parts.Add(timeFilter);
            }

            if (!string.IsNullOrWhiteSpace(ThreadId))
                parts.Add($"Thread: {ThreadId}");

            if (Level.HasValue)
                parts.Add($"Level: {Level.Value}");

            return string.Join(", ", parts);
        }

        /// <summary>
        /// Creates a new instance with no filters applied (all properties null/default).
        /// </summary>
        /// <returns>An empty FilterCriteria object.</returns>
        public static FilterCriteria CreateEmpty()
        {
            return new FilterCriteria();
        }

        /// <summary>
        /// Clears all filter criteria, resetting the object to its default state.
        /// </summary>
        public void Clear()
        {
            SearchText = null;
            IsCaseSensitive = false;
            UseRegex = false;
            MinimumDurationMs = null;
            FromTime = null;
            ToTime = null;
            ThreadId = null;
            Level = null;
        }

        /// <summary>
        /// Returns a string representation of this filter criteria for debugging purposes.
        /// </summary>
        /// <returns>A description of the active filters.</returns>
        public override string ToString()
        {
            return GetDescription();
        }
    }
}
