namespace Cad3PLogBrowser.AI.Abstractions
{
    /// <summary>
    /// Estimates token counts for different AI providers.
    /// Different providers use different tokenization methods.
    /// </summary>
    public interface ITokenEstimator
    {
        /// <summary>
        /// Estimates the number of tokens in the given text.
        /// </summary>
        int EstimateTokenCount(string text);

        /// <summary>
        /// Truncates text to fit within the specified token limit.
        /// </summary>
        string TruncateToTokenLimit(string text, int maxTokens);

        /// <summary>
        /// Provider-specific tokenizer name (e.g., "cl100k_base" for GPT-4).
        /// </summary>
        string TokenizerName { get; }
    }
}
