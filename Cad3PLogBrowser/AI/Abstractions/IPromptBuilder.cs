using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cad3PLogBrowser.AI.Abstractions
{
    /// <summary>
    /// Builds AI prompts by combining system prompts, user queries, and context.
    /// </summary>
    public interface IPromptBuilder
    {
        /// <summary>
        /// Builds a complete prompt for analysis tasks.
        /// </summary>
        Task<string> BuildAnalysisPromptAsync(string analysisType, string userQuery, IEnumerable<IContextProvider> contextProviders);

        /// <summary>
        /// Builds a comparison prompt for comparing two logs.
        /// </summary>
        string BuildComparisonPrompt(string oldContext, string newContext, string userQuery = null);

        /// <summary>
        /// Builds a chat prompt with conversation history.
        /// </summary>
        Task<string> BuildChatPromptAsync(string userMessage, List<ChatMessage> history, IEnumerable<IContextProvider> contextProviders);

        /// <summary>
        /// Gets the system prompt for log analysis tasks.
        /// </summary>
        string GetSystemPrompt(string taskType = "general");

        /// <summary>
        /// Estimates the total tokens in a built prompt.
        /// </summary>
        int EstimatePromptTokens(string prompt);
    }
}
