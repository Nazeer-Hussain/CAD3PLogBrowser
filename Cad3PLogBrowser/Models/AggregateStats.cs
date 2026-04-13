using System;

namespace Cad3PLogBrowser.Models
{
    /// <summary>
    /// Aggregate statistics for an entire log session.
    /// Used by AI analysis features.
    /// </summary>
    public class AggregateStats
    {
        public int TotalLines { get; set; }
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public int TotalApiCalls { get; set; }
        public int UniqueApiCount { get; set; }
        public int MaxCallDepth { get; set; }
        public long SessionDurationMs { get; set; }
        public long MaxCallDurationMs { get; set; }
        public DateTime? SessionStart { get; set; }
        public DateTime? SessionEnd { get; set; }

        public AggregateStats()
        {
            TotalLines = 0;
            ErrorCount = 0;
            WarningCount = 0;
            TotalApiCalls = 0;
            UniqueApiCount = 0;
            MaxCallDepth = 0;
            SessionDurationMs = 0;
            MaxCallDurationMs = 0;
        }
    }
}
