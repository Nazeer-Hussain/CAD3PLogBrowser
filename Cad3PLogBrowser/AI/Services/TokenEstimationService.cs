using System;
using Cad3PLogBrowser.AI.Abstractions;

namespace Cad3PLogBrowser.AI.Services
{
    /// <summary>
    /// Simple token estimator using character-based approximation.
    /// For more accurate estimation, integrate tiktoken or provider-specific tokenizers.
    /// </summary>
    public class SimpleTokenEstimator : ITokenEstimator
    {
        private const double CharsPerToken = 4.0; // Rough approximation

        public string TokenizerName => "Simple";

        public int EstimateTokenCount(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            // Basic estimation: ~4 characters per token
            // This is a rough approximation. Real tokenizers vary by model.
            return (int)Math.Ceiling(text.Length / CharsPerToken);
        }

        public string TruncateToTokenLimit(string text, int maxTokens)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            int estimatedTokens = EstimateTokenCount(text);

            if (estimatedTokens <= maxTokens)
                return text;

            // Calculate how many characters we can keep
            int maxChars = (int)(maxTokens * CharsPerToken);

            if (text.Length <= maxChars)
                return text;

            // Truncate and add ellipsis
            string truncated = text.Substring(0, maxChars - 100);

            // Try to truncate at a line boundary for cleaner output
            int lastNewline = truncated.LastIndexOf('\n');
            if (lastNewline > maxChars - 500)
            {
                truncated = truncated.Substring(0, lastNewline);
            }

            return truncated + "\n\n[... Content truncated to fit within token limit ...]";
        }
    }

    /// <summary>
    /// More sophisticated token estimator that accounts for word boundaries and structure.
    /// </summary>
    public class SmartTokenEstimator : ITokenEstimator
    {
        private const double CharsPerToken = 4.0;
        private const double WhitespaceDiscount = 0.5; // Whitespace uses fewer tokens

        public string TokenizerName => "Smart";

        public int EstimateTokenCount(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            int totalChars = text.Length;
            int whitespaceChars = 0;

            foreach (char c in text)
            {
                if (char.IsWhiteSpace(c))
                    whitespaceChars++;
            }

            int contentChars = totalChars - whitespaceChars;

            // Content chars count fully, whitespace counts less
            double effectiveChars = contentChars + (whitespaceChars * WhitespaceDiscount);

            return (int)Math.Ceiling(effectiveChars / CharsPerToken);
        }

        public string TruncateToTokenLimit(string text, int maxTokens)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            int estimatedTokens = EstimateTokenCount(text);

            if (estimatedTokens <= maxTokens)
                return text;

            // Binary search for the right length
            int left = 0;
            int right = text.Length;
            int bestLength = 0;

            while (left <= right)
            {
                int mid = (left + right) / 2;
                string substring = text.Substring(0, mid);
                int tokens = EstimateTokenCount(substring);

                if (tokens <= maxTokens)
                {
                    bestLength = mid;
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }

            if (bestLength == 0)
                return string.Empty;

            string truncated = text.Substring(0, bestLength);

            // Truncate at line boundary
            int lastNewline = truncated.LastIndexOf('\n');
            if (lastNewline > bestLength * 0.9) // Within 10% of end
            {
                truncated = truncated.Substring(0, lastNewline);
            }

            return truncated + "\n\n[... Content truncated to fit within token limit ...]";
        }
    }
}
