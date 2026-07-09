using System.Threading.Tasks;

namespace Cad3PLogBrowser.AI.Abstractions
{
    /// <summary>
    /// Provides context information to AI prompts.
    /// Examples: current log, selected lines, search results, comparison data.
    /// </summary>
    public interface IContextProvider
    {
        /// <summary>Unique identifier for this context provider.</summary>
        string ContextType { get; }

        /// <summary>Human-readable description of what this provider supplies.</summary>
        string Description { get; }

        /// <summary>Indicates whether this provider currently has valid context available.</summary>
        bool HasContext { get; }

        /// <summary>
        /// Gets the full context text to include in the AI prompt.
        /// May be large - consider chunking for large logs.
        /// </summary>
        Task<string> GetContextAsync();

        /// <summary>
        /// Gets a concise summary of the context (useful for displaying to users or token estimation).
        /// </summary>
        Task<string> GetSummaryAsync();

        /// <summary>
        /// Estimates the token count of the full context.
        /// </summary>
        Task<int> EstimateTokenCountAsync();

        /// <summary>
        /// Gets a truncated version of the context that fits within the specified token limit.
        /// </summary>
        Task<string> GetTruncatedContextAsync(int maxTokens);
    }
}
