using System;
using System.Threading.Tasks;
using Cad3PLogBrowser.AI.Abstractions;

namespace Cad3PLogBrowser.AI.Context
{
    /// <summary>
    /// Base class for context providers with common functionality.
    /// </summary>
    public abstract class ContextProviderBase : IContextProvider
    {
        protected ITokenEstimator TokenEstimator { get; }

        protected ContextProviderBase(ITokenEstimator tokenEstimator = null)
        {
            TokenEstimator = tokenEstimator;
        }

        public abstract string ContextType { get; }
        public abstract string Description { get; }
        public abstract bool HasContext { get; }

        public abstract Task<string> GetContextAsync();

        public virtual async Task<string> GetSummaryAsync()
        {
            if (!HasContext)
                return "No context available";

            string context = await GetContextAsync();

            // Default summary: first 200 characters
            if (context.Length <= 200)
                return context;

            return context.Substring(0, 197) + "...";
        }

        public virtual async Task<int> EstimateTokenCountAsync()
        {
            if (!HasContext)
                return 0;

            string context = await GetContextAsync();
            return EstimateTokens(context);
        }

        public virtual async Task<string> GetTruncatedContextAsync(int maxTokens)
        {
            if (!HasContext)
                return string.Empty;

            string context = await GetContextAsync();
            int tokens = EstimateTokens(context);

            if (tokens <= maxTokens)
                return context;

            // Truncate to fit within limit
            if (TokenEstimator != null)
            {
                return TokenEstimator.TruncateToTokenLimit(context, maxTokens);
            }

            // Fallback: simple character-based truncation (rough estimate)
            int maxChars = maxTokens * 4; // Approximate: 1 token ? 4 chars
            if (context.Length <= maxChars)
                return context;

            return context.Substring(0, maxChars) + 
                "\n\n[... Content truncated to fit within token limit ...]";
        }

        protected int EstimateTokens(string text)
        {
            if (TokenEstimator != null)
            {
                return TokenEstimator.EstimateTokenCount(text);
            }

            // Fallback estimation
            return text.Length / 4;
        }

        protected string FormatContext(string title, string content)
        {
            return $"## {title}\n\n{content}\n";
        }
    }
}
