namespace Cad3PLogBrowser.Models
{
    /// <summary>
    /// Performance metrics for a single API method.
    /// Used in the Performance tab to display statistical analysis of method executions.
    /// </summary>
    /// <remarks>
    /// The Performance tab shows a grid of all methods with their execution statistics.
    /// Each row in the grid is represented by one PerformanceStatistics object.
    /// Users can sort by any column to identify performance bottlenecks.
    /// </remarks>
    /// <example>
    /// Performance tab display:
    /// 
    /// | API Name              | Calls | Total (ms) | Avg (ms) | Min (ms) | Max (ms) | Self (ms) | Source File       |
    /// |-----------------------|-------|------------|----------|----------|----------|-----------|-------------------|
    /// | CADSystem::OpenFile   |   3   |    426     |   142    |   89     |   215    |    83     | FileOperations.cs |
    /// | Database::Query       |  15   |   1523     |   101    |   45     |   342    |   101     | Database.cs       |
    /// | UI::Render            |  50   |    523     |    10    |    5     |    23    |     8     | Renderer.cs       |
    /// </example>
    public class PerformanceStatistics
    {
        /// <summary>
        /// Gets or sets the name of the API method.
        /// Format: "ClassName::MethodName" or "Namespace.ClassName::MethodName".
        /// </summary>
        /// <example>
        /// - "CADSystem::OpenFile"
        /// - "ProToolkit.Drawing::CreateView"
        /// - "Assembly::AddComponent"
        /// </example>
        public string ApiName { get; set; }

        /// <summary>
        /// Gets or sets the source file where this method is defined.
        /// Null or empty if source information is not available.
        /// </summary>
        /// <remarks>
        /// Displayed in the rightmost column of the Performance tab.
        /// Used for "jump to source" feature and grouping related methods.
        /// Example: "CADSystem\FileOperations.cs"
        /// </remarks>
        public string SourceFile { get; set; }

        /// <summary>
        /// Gets or sets the total number of times this method was called.
        /// Only counts calls with matched ENTER/EXIT pairs.
        /// </summary>
        /// <remarks>
        /// High call counts may indicate:
        /// - Frequently used utility methods
        /// - Methods in tight loops
        /// - Potential optimization targets if each call is expensive
        /// </remarks>
        public int CallCount { get; set; }

        /// <summary>
        /// Gets or sets the sum of all execution durations for this method.
        /// Measured in milliseconds.
        /// </summary>
        /// <remarks>
        /// Calculated as: sum of (EXIT timestamp - ENTER timestamp) for all matched calls.
        /// This is the total time spent in this method across all invocations,
        /// INCLUDING time spent in methods it calls.
        /// 
        /// Used for:
        /// - Identifying methods that consume the most overall time
        /// - Calculating average duration
        /// - Comparing relative cost of different methods
        /// </remarks>
        public long TotalDurationMs { get; set; }

        /// <summary>
        /// Gets or sets the average execution duration per call.
        /// Measured in milliseconds.
        /// </summary>
        /// <remarks>
        /// Calculated as: TotalDurationMs / CallCount
        /// 
        /// Most useful metric for comparing methods:
        /// - Shows typical performance expectation
        /// - Smooths out outliers
        /// - Easier to compare than total (which depends on call count)
        /// 
        /// Sort by this column to find consistently slow methods.
        /// </remarks>
        public long AvgDurationMs { get; set; }

        /// <summary>
        /// Gets or sets the minimum execution duration among all calls.
        /// Measured in milliseconds.
        /// </summary>
        /// <remarks>
        /// The fastest execution of this method (best case).
        /// 
        /// Useful for:
        /// - Identifying baseline performance
        /// - Detecting caching effects (first call slow, subsequent fast)
        /// - Understanding best-case scenario
        /// 
        /// Large gap between Min and Max may indicate:
        /// - Caching/warm-up effects
        /// - Varying input complexity
        /// - External dependencies (network, disk I/O)
        /// </remarks>
        public long MinDurationMs { get; set; }

        /// <summary>
        /// Gets or sets the maximum execution duration among all calls.
        /// Measured in milliseconds.
        /// </summary>
        /// <remarks>
        /// The slowest execution of this method (worst case).
        /// 
        /// Often indicates:
        /// - Performance bottlenecks
        /// - Edge cases with complex input
        /// - First-time initialization overhead
        /// - External timeouts or delays
        /// 
        /// Highlighted in red if exceeds SlowCallThresholdMs (default 500ms).
        /// </remarks>
        public long MaxDurationMs { get; set; }

        /// <summary>
        /// Gets or sets the "self time" - duration excluding time spent in child method calls.
        /// Measured in milliseconds.
        /// </summary>
        /// <remarks>
        /// Calculated as: TotalDurationMs - (sum of TotalDurationMs of all direct children)
        /// 
        /// This isolates the method's OWN cost from the cost of methods it calls.
        /// 
        /// Example:
        /// - Method A takes 1000ms total
        /// - Calls Method B (600ms), Method C (300ms)
        /// - Self time of A = 1000 - 600 - 300 = 100ms
        /// - Only 100ms is A's own code; 900ms is waiting for B and C
        /// 
        /// Low self-time = method is mostly a coordinator
        /// High self-time = method does significant work itself
        /// 
        /// Sort by this column to find where actual computation happens.
        /// </remarks>
        public long SelfDurationMs { get; set; }

        /// <summary>
        /// Gets or sets the first line number where this method appears in the log.
        /// Used for quick navigation.
        /// </summary>
        public int FirstCallLineNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all calls to this method have matching ENTER/EXIT pairs.
        /// False if any call is unmatched (missing EXIT).
        /// </summary>
        public bool AllCallsMatched { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceStatistics"/> class.
        /// Sets default values.
        /// </summary>
        public PerformanceStatistics()
        {
            AllCallsMatched = true;
            MinDurationMs = long.MaxValue; // Will be replaced with actual min
        }

        /// <summary>
        /// Updates this statistics object with data from a single method execution.
        /// Used during log parsing to incrementally build statistics.
        /// </summary>
        /// <param name="durationMs">Duration of this single execution.</param>
        /// <param name="lineNumber">Line number where this execution started.</param>
        /// <param name="isMatched">Whether this execution has matching ENTER/EXIT.</param>
        /// <remarks>
        /// Call this method once for each matched ENTER/EXIT pair during parsing.
        /// After all calls are processed, call RecalculateAverages() to finalize statistics.
        /// </remarks>
        public void AddExecution(long durationMs, int lineNumber, bool isMatched)
        {
            CallCount++;
            TotalDurationMs += durationMs;

            if (durationMs < MinDurationMs)
                MinDurationMs = durationMs;

            if (durationMs > MaxDurationMs)
                MaxDurationMs = durationMs;

            if (FirstCallLineNumber == 0)
                FirstCallLineNumber = lineNumber;

            if (!isMatched)
                AllCallsMatched = false;
        }

        /// <summary>
        /// Recalculates derived statistics (averages) after all executions have been added.
        /// Must be called after all AddExecution() calls are complete.
        /// </summary>
        public void RecalculateAverages()
        {
            if (CallCount > 0)
            {
                AvgDurationMs = TotalDurationMs / CallCount;
            }
            else
            {
                AvgDurationMs = 0;
                MinDurationMs = 0; // Reset from MaxValue if no calls
            }
        }

        /// <summary>
        /// Gets a formatted display string for the total duration.
        /// </summary>
        /// <returns>Human-readable duration string (e.g., "1.5 sec", "142 ms").</returns>
        public string GetTotalDurationDisplay()
        {
            if (TotalDurationMs >= 60000)
                return $"{TotalDurationMs / 60000.0:F1} min";
            else if (TotalDurationMs >= 1000)
                return $"{TotalDurationMs / 1000.0:F1} sec";
            else
                return $"{TotalDurationMs} ms";
        }

        /// <summary>
        /// Gets a color indication based on average duration.
        /// Used for conditional formatting in the Performance tab.
        /// </summary>
        /// <returns>
        /// "Fast" if AvgDurationMs &lt; 100ms (green),
        /// "Slow" if AvgDurationMs &gt; 500ms (red),
        /// "Medium" otherwise (amber).
        /// </returns>
        public string GetPerformanceCategory()
        {
            if (AvgDurationMs < 100)
                return "Fast";
            else if (AvgDurationMs > 500)
                return "Slow";
            else
                return "Medium";
        }

        /// <summary>
        /// Returns a detailed string representation for debugging.
        /// </summary>
        /// <returns>String with all key metrics.</returns>
        public override string ToString()
        {
            return $"{ApiName}: {CallCount} calls, Avg: {AvgDurationMs}ms, Total: {TotalDurationMs}ms, Self: {SelfDurationMs}ms";
        }
    }
}
