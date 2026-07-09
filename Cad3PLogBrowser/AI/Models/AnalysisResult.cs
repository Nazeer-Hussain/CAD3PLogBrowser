using System;
using System.Collections.Generic;

namespace Cad3PLogBrowser.AI.Models
{
    /// <summary>
    /// Represents the result of an AI analysis operation.
    /// </summary>
    public class AnalysisResult
    {
        public bool Success { get; set; }
        public string Content { get; set; }
        public string ErrorMessage { get; set; }
        public AnalysisType Type { get; set; }
        public DateTime Timestamp { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public int? TokensUsed { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        public AnalysisResult()
        {
            Timestamp = DateTime.UtcNow;
            Metadata = new Dictionary<string, object>();
        }

        public static AnalysisResult CreateSuccess(string content, AnalysisType type)
        {
            return new AnalysisResult
            {
                Success = true,
                Content = content,
                Type = type,
                Timestamp = DateTime.UtcNow
            };
        }

        public static AnalysisResult CreateError(string errorMessage, AnalysisType type)
        {
            return new AnalysisResult
            {
                Success = false,
                ErrorMessage = errorMessage,
                Type = type,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Types of analysis that can be performed.
    /// </summary>
    public enum AnalysisType
    {
        Summarize,
        RootCause,
        ExplainException,
        Timeline,
        Performance,
        CompareLog,
        FindErrors,
        FindWarnings,
        ExplainCrash,
        SuggestFix,
        Custom
    }
}
