using System;
using System.Collections.Generic;

namespace Cad3PLogBrowser.AI.Abstractions
{
    /// <summary>
    /// Represents a response from an AI provider.
    /// </summary>
    public interface IAIResponse
    {
        /// <summary>The generated text response.</summary>
        string Content { get; set; }

        /// <summary>Whether the request was successful.</summary>
        bool Success { get; set; }

        /// <summary>Error message if the request failed.</summary>
        string ErrorMessage { get; set; }

        /// <summary>Number of tokens in the prompt.</summary>
        int? PromptTokens { get; set; }

        /// <summary>Number of tokens in the response.</summary>
        int? CompletionTokens { get; set; }

        /// <summary>Total tokens used (prompt + completion).</summary>
        int? TotalTokens { get; set; }

        /// <summary>Model that generated the response.</summary>
        string Model { get; set; }

        /// <summary>Time taken to generate the response.</summary>
        TimeSpan ElapsedTime { get; set; }

        /// <summary>Finish reason (e.g., "stop", "length", "content_filter").</summary>
        string FinishReason { get; set; }

        /// <summary>Provider-specific metadata.</summary>
        Dictionary<string, object> Metadata { get; set; }
    }

    /// <summary>
    /// Default implementation of IAIResponse.
    /// </summary>
    public class AIResponse : IAIResponse
    {
        public string Content { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public int? PromptTokens { get; set; }
        public int? CompletionTokens { get; set; }
        public int? TotalTokens { get; set; }
        public string Model { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public string FinishReason { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        public AIResponse()
        {
            Success = false;
            Metadata = new Dictionary<string, object>();
        }

        public static AIResponse CreateError(string errorMessage)
        {
            return new AIResponse
            {
                Success = false,
                ErrorMessage = errorMessage,
                Content = string.Empty
            };
        }

        public static AIResponse CreateSuccess(string content, int? promptTokens = null, 
            int? completionTokens = null, string model = null)
        {
            return new AIResponse
            {
                Success = true,
                Content = content,
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                TotalTokens = (promptTokens ?? 0) + (completionTokens ?? 0),
                Model = model
            };
        }
    }
}
