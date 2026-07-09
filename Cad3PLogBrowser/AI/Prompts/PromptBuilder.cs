using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cad3PLogBrowser.AI.Abstractions;

namespace Cad3PLogBrowser.AI.Prompts
{
    /// <summary>
    /// Builds prompts for AI analysis by combining system prompts, context, and user queries.
    /// </summary>
    public class PromptBuilder : IPromptBuilder
    {
        private readonly ITokenEstimator _tokenEstimator;
        private const int DefaultMaxContextTokens = 100000;

        public PromptBuilder(ITokenEstimator tokenEstimator = null)
        {
            _tokenEstimator = tokenEstimator;
        }

        /// <summary>
        /// Builds a complete prompt for analysis tasks.
        /// </summary>
        public string BuildAnalysisPrompt(string analysisType, string userQuery, 
            IEnumerable<IContextProvider> contextProviders)
        {
            var sb = new StringBuilder();

            // Add context information
            if (contextProviders != null && contextProviders.Any())
            {
                foreach (var provider in contextProviders.Where(p => p.HasContext))
                {
                    sb.AppendLine($"## {provider.Description}");
                    sb.AppendLine();

                    // Get context (may be truncated if too large)
                    var context = provider.GetContextAsync().Result;
                    sb.AppendLine(context);
                    sb.AppendLine();
                }
            }

            // Add user query
            if (!string.IsNullOrWhiteSpace(userQuery))
            {
                sb.AppendLine("## User Request");
                sb.AppendLine(userQuery);
            }
            else
            {
                // Use default query based on analysis type
                sb.AppendLine($"## Task");
                sb.AppendLine(GetDefaultQueryForAnalysis(analysisType));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Builds a comparison prompt for comparing two logs.
        /// </summary>
        public string BuildComparisonPrompt(string oldContext, string newContext, string userQuery = null)
        {
            var sb = new StringBuilder();

            sb.AppendLine("## Original Log (Baseline)");
            sb.AppendLine();
            sb.AppendLine(oldContext);
            sb.AppendLine();

            sb.AppendLine("## New Log (Current)");
            sb.AppendLine();
            sb.AppendLine(newContext);
            sb.AppendLine();

            sb.AppendLine("## Task");
            if (!string.IsNullOrWhiteSpace(userQuery))
            {
                sb.AppendLine(userQuery);
            }
            else
            {
                sb.AppendLine(@"Compare these two logs and identify:
1. New errors or warnings that appeared
2. Errors or warnings that were fixed
3. Performance differences (slower or faster operations)
4. Changes in execution flow
5. Overall assessment (improvement, regression, or neutral)

Focus on significant changes. Ignore minor timing variations.");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Builds a chat prompt with conversation history.
        /// </summary>
        public string BuildChatPrompt(string userMessage, List<ChatMessage> history, 
            IEnumerable<IContextProvider> contextProviders)
        {
            var sb = new StringBuilder();

            // Add context if this is the first message
            if (history == null || history.Count == 0)
            {
                if (contextProviders != null && contextProviders.Any())
                {
                    sb.AppendLine("## Log Context");
                    sb.AppendLine();

                    foreach (var provider in contextProviders.Where(p => p.HasContext))
                    {
                        var context = provider.GetContextAsync().Result;
                        sb.AppendLine($"### {provider.Description}");
                        sb.AppendLine(context);
                        sb.AppendLine();
                    }
                }
            }

            // User's current message
            sb.AppendLine(userMessage);

            return sb.ToString();
        }

        /// <summary>
        /// Gets the system prompt for log analysis tasks.
        /// </summary>
        public string GetSystemPrompt(string taskType = "general")
        {
            return SystemPrompts.GetSystemPrompt(taskType);
        }

        /// <summary>
        /// Estimates the total tokens in a built prompt.
        /// </summary>
        public int EstimatePromptTokens(string prompt)
        {
            if (_tokenEstimator != null)
            {
                return _tokenEstimator.EstimateTokenCount(prompt);
            }

            // Fallback: rough estimation (1 token ? 4 characters)
            return prompt.Length / 4;
        }

        private string GetDefaultQueryForAnalysis(string analysisType)
        {
            switch (analysisType?.ToLowerInvariant())
            {
                case "summarize":
                    return "Provide a concise summary of this log file, highlighting the most important information.";

                case "rootcause":
                    return "Analyze this log and identify the root cause of any failures or issues.";

                case "performance":
                    return "Analyze the performance characteristics and identify bottlenecks or slow operations.";

                case "timeline":
                    return "Create a timeline of significant events in chronological order.";

                case "errors":
                    return "List and explain all errors found in this log.";

                case "warnings":
                    return "List and explain all warnings found in this log.";

                case "crash":
                    return "Analyze the crash and determine what caused the application to fail.";

                case "fix":
                    return "Suggest specific fixes for the issues identified in this log.";

                default:
                    return "Analyze this log file and provide insights.";
            }
        }

        /// <summary>
        /// Truncates context to fit within token limits.
        /// </summary>
        public string TruncateContext(string context, int maxTokens)
        {
            if (_tokenEstimator == null)
            {
                // Fallback: truncate by characters (rough estimate)
                int maxChars = maxTokens * 4;
                if (context.Length <= maxChars)
                    return context;

                return context.Substring(0, maxChars) + 
                    "\n\n[... Content truncated to fit token limit ...]";
            }

            return _tokenEstimator.TruncateToTokenLimit(context, maxTokens);
        }

        /// <summary>
        /// Builds a structured summary prompt for large logs.
        /// Instead of sending raw log lines, sends aggregated statistics.
        /// </summary>
        public string BuildStructuredSummaryPrompt(Dictionary<string, object> stats)
        {
            var sb = new StringBuilder();

            sb.AppendLine("## Log Statistics");
            sb.AppendLine();

            foreach (var kvp in stats)
            {
                sb.AppendLine($"**{kvp.Key}**: {kvp.Value}");
            }

            sb.AppendLine();
            sb.AppendLine("Based on these statistics, provide a concise analysis and identify any concerns.");

            return sb.ToString();
        }
    }
}
