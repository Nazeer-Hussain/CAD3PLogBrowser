namespace Cad3PLogBrowser.Services.Search
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Cad3PLogBrowser.Models;
    using Cad3PLogBrowser.Utilities;

    /// <summary>
    /// Handles filtering of log entries based on various criteria.
    /// This service extracts all filter logic that was previously in MainForm.
    /// </summary>
    /// <remarks>
    /// The FilterService applies multiple filter criteria to log entries using AND logic.
    /// For example, if text filter AND duration filter are both active, only entries
    /// matching BOTH criteria will be included in the results.
    /// 
    /// This is a stateless service - it doesn't hold any data between calls.
    /// All state is passed in via parameters or the FilterCriteria object.
    /// </remarks>
    public class FilterService
    {
        // D2/P2: cached Regex to avoid recompiling on every filter operation.
        // Invalidated whenever the pattern or options change.
        private Regex  _cachedRegex;
        private string _cachedRegexPattern;
        private RegexOptions _cachedRegexOptions = (RegexOptions)(-1);

        private Regex GetOrBuildRegex(string pattern, RegexOptions options)
        {
            if (_cachedRegex != null &&
                _cachedRegexPattern == pattern &&
                _cachedRegexOptions == options)
                return _cachedRegex;

            _cachedRegex        = new Regex(pattern, options | RegexOptions.Compiled);
            _cachedRegexPattern = pattern;
            _cachedRegexOptions = options;
            return _cachedRegex;
        }

        /// <summary>
        /// Applies multiple filters to a collection of log entries.
        /// All active filters must match for an entry to be included (AND logic).
        /// </summary>
        /// <param name="allEntries">Complete list of log entries to filter.</param>
        /// <param name="criteria">Filter criteria to apply. If null or inactive, returns all entries.</param>
        /// <returns>Filtered list of entries matching all active criteria.</returns>
        /// <exception cref="ArgumentException">Thrown if regex pattern in criteria is invalid.</exception>
        /// <example>
        /// var criteria = new FilterCriteria 
        /// { 
        ///     SearchText = "error",
        ///     MinimumDurationMs = 1000,
        ///     Level = LogLevel.Error
        /// };
        /// 
        /// var filtered = filterService.ApplyFilters(allEntries, criteria);
        /// // Returns only error entries containing "error" that took > 1 second
        /// </example>
        public List<LogEntry> ApplyFilters(List<LogEntry> allEntries, FilterCriteria criteria)
        {
            // If no entries or no filter, return all
            if (allEntries == null || allEntries.Count == 0)
                return new List<LogEntry>();

            if (criteria == null || !criteria.IsActive)
                return new List<LogEntry>(allEntries); // Return copy

            var results = new List<LogEntry>();

            // Apply filters to each entry
            foreach (var entry in allEntries)
            {
                if (MatchesAllFilters(entry, criteria))
                {
                    results.Add(entry);
                }
            }

            return results;
        }

        /// <summary>
        /// Checks if a log entry matches all active filters in the criteria.
        /// </summary>
        /// <param name="entry">The log entry to check.</param>
        /// <param name="criteria">The filter criteria containing all filter settings.</param>
        /// <returns>True if the entry matches all active filters.</returns>
        /// <remarks>
        /// This method applies each filter in sequence. If any filter doesn't match,
        /// it returns false immediately (short-circuit evaluation for performance).
        /// </remarks>
        private bool MatchesAllFilters(LogEntry entry, FilterCriteria criteria)
        {
            // Text filter
            if (!string.IsNullOrWhiteSpace(criteria.SearchText))
            {
                if (!MatchesTextFilter(entry, criteria.SearchText, criteria.IsCaseSensitive, criteria.UseRegex))
                    return false;
            }

            // Duration filter
            if (criteria.MinimumDurationMs.HasValue)
            {
                if (!MatchesDurationFilter(entry.Text, criteria.MinimumDurationMs.Value))
                    return false;
            }

            // Time range filter
            if (criteria.FromTime.HasValue || criteria.ToTime.HasValue)
            {
                if (!MatchesTimeRangeFilter(entry, criteria.FromTime, criteria.ToTime))
                    return false;
            }

            // Thread ID filter
            if (!string.IsNullOrWhiteSpace(criteria.ThreadId))
            {
                if (!MatchesThreadFilter(entry, criteria.ThreadId))
                    return false;
            }

            // Log level filter
            if (criteria.Level.HasValue)
            {
                if (entry.Level != criteria.Level.Value)
                    return false;
            }

            return true; // Passed all filters
        }

        /// <summary>
        /// Checks if a log entry matches the text filter.
        /// Supports both plain text and regular expression matching.
        /// </summary>
        /// <param name="entry">The log entry to check.</param>
        /// <param name="searchText">The text or regex pattern to search for.</param>
        /// <param name="caseSensitive">Whether the search is case-sensitive.</param>
        /// <param name="useRegex">Whether to use regular expression matching.</param>
        /// <returns>True if the entry matches the text filter.</returns>
        /// <exception cref="ArgumentException">Thrown if useRegex is true and searchText is not a valid regex.</exception>
        private bool MatchesTextFilter(LogEntry entry, string searchText, bool caseSensitive, bool useRegex)
        {
            if (string.IsNullOrEmpty(entry.Text))
                return false;

            if (useRegex)
            {
                // Regex matching — use cached instance to avoid recompiling on every entry
                var options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                try
                {
                    var regex = GetOrBuildRegex(searchText, options);
                    return regex.IsMatch(entry.Text);
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException(string.Format("Invalid regular expression: {0}", searchText), ex);
                }
            }
            else
            {
                // Plain text matching
                return caseSensitive
                    ? entry.Text.Contains(searchText)
                    : entry.Text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

        /// <summary>
        /// Checks if a log entry matches the duration filter.
        /// Only entries with duration information can match this filter.
        /// </summary>
        /// <param name="logText">The log line text to extract duration from.</param>
        /// <param name="minDurationMs">Minimum duration in milliseconds.</param>
        /// <returns>True if the entry has duration >= minDurationMs.</returns>
        /// <remarks>
        /// Looks for duration pattern like "[142 ms]" in the log text.
        /// If no duration is found, the entry is excluded (returns false).
        /// This is intentional - duration filter only shows entries WITH duration info.
        /// </remarks>
        private bool MatchesDurationFilter(string logText, int minDurationMs)
        {
            if (string.IsNullOrEmpty(logText))
                return false;

            // Look for duration pattern: [XXX ms]
            var match = Regex.Match(logText, Constants.Parsing.DurationRegexPattern);
            if (match.Success && int.TryParse(match.Groups[1].Value, out int duration))
            {
                return duration >= minDurationMs;
            }

            return false; // No duration found - exclude from filter results
        }

        /// <summary>
        /// Checks if a log entry matches the time range filter.
        /// Only the time portion of the timestamp is used (date is ignored).
        /// </summary>
        /// <param name="entry">The log entry to check.</param>
        /// <param name="fromTime">Start time (null = no start limit).</param>
        /// <param name="toTime">End time (null = no end limit).</param>
        /// <returns>True if the entry's time falls within the specified range.</returns>
        /// <remarks>
        /// If the entry has no timestamp, it's excluded from results.
        /// This allows filtering by time of day across multiple log file dates.
        /// Example: Show only entries between 10:00 AM and 11:00 AM.
        /// </remarks>
        private bool MatchesTimeRangeFilter(LogEntry entry, DateTime? fromTime, DateTime? toTime)
        {
            if (!entry.Timestamp.HasValue)
                return false; // No timestamp - exclude

            var entryTime = entry.Timestamp.Value.TimeOfDay;

            // Check start time
            if (fromTime.HasValue && entryTime < fromTime.Value.TimeOfDay)
                return false;

            // Check end time
            if (toTime.HasValue && entryTime > toTime.Value.TimeOfDay)
                return false;

            return true;
        }

        /// <summary>
        /// Checks if a log entry matches the thread ID filter.
        /// Case-insensitive exact match.
        /// </summary>
        /// <param name="entry">The log entry to check.</param>
        /// <param name="threadId">The thread ID to filter by.</param>
        /// <returns>True if the entry is from the specified thread.</returns>
        private bool MatchesThreadFilter(LogEntry entry, string threadId)
        {
            if (string.IsNullOrWhiteSpace(entry.ThreadId))
                return false;

            return entry.ThreadId.Equals(threadId, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets a count of entries that match each filter criterion individually.
        /// Useful for showing "X matches" for each filter before applying them all.
        /// </summary>
        /// <param name="allEntries">All log entries.</param>
        /// <param name="criteria">Filter criteria.</param>
        /// <returns>Dictionary with filter names and match counts.</returns>
        /// <example>
        /// var counts = filterService.GetFilterMatchCounts(entries, criteria);
        /// // counts["text"] = 42
        /// // counts["duration"] = 15
        /// // counts["combined"] = 8  (entries matching ALL filters)
        /// </example>
        public Dictionary<string, int> GetFilterMatchCounts(List<LogEntry> allEntries, FilterCriteria criteria)
        {
            var counts = new Dictionary<string, int>();

            if (allEntries == null || allEntries.Count == 0 || criteria == null)
                return counts;

            // Count matches for each individual filter
            if (!string.IsNullOrWhiteSpace(criteria.SearchText))
            {
                int textMatches = 0;
                foreach (var e in allEntries)
                {
                    if (MatchesTextFilter(e, criteria.SearchText, criteria.IsCaseSensitive, criteria.UseRegex))
                        textMatches++;
                }
                counts["text"] = textMatches;
            }

            if (criteria.MinimumDurationMs.HasValue)
            {
                int durationMatches = 0;
                foreach (var e in allEntries)
                {
                    if (MatchesDurationFilter(e.Text, criteria.MinimumDurationMs.Value))
                        durationMatches++;
                }
                counts["duration"] = durationMatches;
            }

            if (criteria.FromTime.HasValue || criteria.ToTime.HasValue)
            {
                int timeMatches = 0;
                foreach (var e in allEntries)
                {
                    if (MatchesTimeRangeFilter(e, criteria.FromTime, criteria.ToTime))
                        timeMatches++;
                }
                counts["timeRange"] = timeMatches;
            }

            if (!string.IsNullOrWhiteSpace(criteria.ThreadId))
            {
                int threadMatches = 0;
                foreach (var e in allEntries)
                {
                    if (MatchesThreadFilter(e, criteria.ThreadId))
                        threadMatches++;
                }
                counts["thread"] = threadMatches;
            }

            if (criteria.Level.HasValue)
            {
                int levelMatches = 0;
                foreach (var e in allEntries)
                {
                    if (e.Level == criteria.Level.Value)
                        levelMatches++;
                }
                counts["level"] = levelMatches;
            }

            // Count entries matching ALL filters
            counts["combined"] = ApplyFilters(allEntries, criteria).Count;

            return counts;
        }
    }
}
