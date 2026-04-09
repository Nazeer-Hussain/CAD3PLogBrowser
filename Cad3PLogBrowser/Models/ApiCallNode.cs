namespace Cad3PLogBrowser.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a unique API method with all its invocations.
    /// Used in the API Tree view to group multiple calls to the same method.
    /// </summary>
    /// <remarks>
    /// The API Tree displays a flat or hierarchical view of all unique methods called in the log.
    /// Each ApiCallNode represents one unique method, regardless of how many times it was called.
    /// The node tracks all invocations and calculates aggregate statistics.
    /// </remarks>
    /// <example>
    /// If a log file contains:
    /// - Line 10: "CADSystem::OpenFile [ENTER]"
    /// - Line 42: "CADSystem::OpenFile [ENTER]"
    /// - Line 99: "CADSystem::OpenFile [ENTER]"
    /// 
    /// This would create ONE ApiCallNode:
    /// - ApiName = "CADSystem::OpenFile"
    /// - TotalCalls = 3
    /// - LineNumbers = [10, 42, 99]
    /// - Statistics calculated from all three invocations
    /// </example>
    public class ApiCallNode
    {
        /// <summary>
        /// Gets or sets the unique name of the API method.
        /// Format is typically "ClassName::MethodName" or "Namespace.ClassName::MethodName".
        /// </summary>
        /// <example>
        /// - "CADSystem::OpenFile"
        /// - "ProToolkit.Drawing::CreateView"
        /// - "Assembly::AddComponent"
        /// </example>
        public string ApiName { get; set; }

        /// <summary>
        /// Gets or sets the total number of times this API was called in the log file.
        /// Counts all ENTER occurrences for this method.
        /// </summary>
        /// <remarks>
        /// Displayed in the API Tree as: "MethodName  (N calls)"
        /// </remarks>
        public int TotalCalls { get; set; }

        /// <summary>
        /// Gets or sets the list of line numbers where this API is invoked.
        /// Each entry is the line number of an ENTER statement.
        /// </summary>
        /// <remarks>
        /// Used for:
        /// - Creating child nodes in API Tree (one per invocation)
        /// - Jumping to specific invocations
        /// - Displaying invocation list in details panel
        /// </remarks>
        /// <example>
        /// If called on lines 10, 42, and 99:
        /// LineNumbers = [10, 42, 99]
        /// 
        /// Child nodes displayed as:
        /// - "CADSystem::OpenFile — Ln 10"
        /// - "CADSystem::OpenFile — Ln 42"
        /// - "CADSystem::OpenFile — Ln 99"
        /// </example>
        public List<int> LineNumbers { get; set; }

        /// <summary>
        /// Gets or sets the sum of all execution durations for this API across all calls.
        /// Measured in milliseconds. Zero if no duration information available.
        /// </summary>
        /// <remarks>
        /// Calculated by summing durations of all matched ENTER/EXIT pairs.
        /// Unmatched calls (missing EXIT) are excluded from this total.
        /// </remarks>
        public long TotalDurationMs { get; set; }

        /// <summary>
        /// Gets or sets the minimum execution duration among all calls to this API.
        /// Measured in milliseconds. Zero if no duration information available.
        /// </summary>
        /// <remarks>
        /// The fastest single execution of this method.
        /// Useful for identifying the baseline performance.
        /// </remarks>
        public long MinDurationMs { get; set; }

        /// <summary>
        /// Gets or sets the maximum execution duration among all calls to this API.
        /// Measured in milliseconds. Zero if no duration information available.
        /// </summary>
        /// <remarks>
        /// The slowest single execution of this method.
        /// Often indicates performance bottlenecks or edge cases.
        /// Colored red in the UI if exceeds SlowCallThresholdMs.
        /// </remarks>
        public long MaxDurationMs { get; set; }

        /// <summary>
        /// Gets or sets the average execution duration across all calls to this API.
        /// Measured in milliseconds. Zero if no duration information available.
        /// </summary>
        /// <remarks>
        /// Calculated as: TotalDurationMs / number of matched calls
        /// Provides a typical performance expectation for this method.
        /// </remarks>
        public double AvgDurationMs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all calls to this API have matching ENTER/EXIT pairs.
        /// True if every ENTER has a corresponding EXIT.
        /// </summary>
        /// <remarks>
        /// Used to determine icon in the tree:
        /// - ? (checkmark) if AllCallsMatched = true
        /// - ? (cross) if AllCallsMatched = false
        /// 
        /// False may indicate:
        /// - Log file was truncated
        /// - Application crashed during execution
        /// - Method is still running (for live logs)
        /// </remarks>
        public bool AllCallsMatched { get; set; }

        /// <summary>
        /// Gets or sets the source file where this API is defined.
        /// Null if source information is not available in the log.
        /// </summary>
        public string SourceFile { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiCallNode"/> class.
        /// Sets default values for collections.
        /// </summary>
        public ApiCallNode()
        {
            LineNumbers = new List<int>();
            AllCallsMatched = true; // Assume true until proven otherwise
        }

        /// <summary>
        /// Calculates statistics from a list of individual call durations.
        /// Updates TotalDurationMs, MinDurationMs, MaxDurationMs, and AvgDurationMs.
        /// </summary>
        /// <param name="durations">List of execution times in milliseconds for each call.</param>
        /// <remarks>
        /// This method should be called after parsing all log entries to compute aggregate statistics.
        /// Only includes durations from matched ENTER/EXIT pairs.
        /// </remarks>
        public void CalculateStatistics(List<long> durations)
        {
            if (durations == null || durations.Count == 0)
            {
                TotalDurationMs = 0;
                MinDurationMs = 0;
                MaxDurationMs = 0;
                AvgDurationMs = 0;
                return;
            }

            TotalDurationMs = 0;
            MinDurationMs = long.MaxValue;
            MaxDurationMs = 0;

            foreach (var duration in durations)
            {
                TotalDurationMs += duration;
                if (duration < MinDurationMs) MinDurationMs = duration;
                if (duration > MaxDurationMs) MaxDurationMs = duration;
            }

            AvgDurationMs = durations.Count > 0 ? (double)TotalDurationMs / durations.Count : 0;
        }

        /// <summary>
        /// Returns a formatted string showing the API name and call count.
        /// Used for displaying the root node in the API Tree.
        /// </summary>
        /// <returns>String in format: "MethodName  (N calls)"</returns>
        public string GetDisplayText()
        {
            return $"{ApiName}  ({TotalCalls} call{(TotalCalls != 1 ? "s" : "")})";
        }

        /// <summary>
        /// Returns a detailed string representation for debugging.
        /// </summary>
        /// <returns>String with all key properties.</returns>
        public override string ToString()
        {
            return $"{ApiName}: {TotalCalls} calls, Avg: {AvgDurationMs:F1}ms, Max: {MaxDurationMs}ms";
        }
    }
}
