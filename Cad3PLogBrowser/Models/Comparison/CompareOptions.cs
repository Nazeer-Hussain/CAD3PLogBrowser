namespace Cad3PLogBrowser.Models.Comparison
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Configuration options for controlling tree comparison behavior.
    /// </summary>
    /// <remarks>
    /// This class provides fine-grained control over how nodes are compared.
    /// Options can be combined to create custom comparison rules.
    /// </remarks>
    public class CompareOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompareOptions"/> class with default settings.
        /// </summary>
        public CompareOptions()
        {
            IgnoreCase = false;
            IgnoreWhitespace = false;
            IgnoreTimestamps = true; // Default to true for log comparison
            IgnoreGuids = false;
            TrimText = false;
            UseRegexIgnorePatterns = false;
            RegexIgnorePattern = string.Empty;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to perform case-insensitive text comparison.
        /// </summary>
        /// <value>
        /// True to ignore case differences; false for case-sensitive comparison.
        /// Default is false.
        /// </value>
        /// <example>
        /// When true: "OpenFile" matches "openfile"
        /// </example>
        public bool IgnoreCase { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore leading/trailing whitespace differences.
        /// </summary>
        /// <value>
        /// True to ignore whitespace; false to consider whitespace significant.
        /// Default is false.
        /// </value>
        /// <example>
        /// When true: " OpenFile " matches "OpenFile"
        /// </example>
        public bool IgnoreWhitespace { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore timestamp differences in node text.
        /// </summary>
        /// <value>
        /// True to strip timestamps before comparison; false to include timestamps.
        /// Default is true (essential for comparing logs from different runs).
        /// </value>
        /// <remarks>
        /// This option uses pattern matching to detect and remove common timestamp formats:
        /// - ISO format: 2025-01-15 10:23:45.123
        /// - Unix timestamps: 1642245825
        /// - Relative times: [0.123s], [142 ms]
        /// - Date/time combinations
        /// </remarks>
        /// <example>
        /// Original: "2025-01-15 10:23:45: OpenFile [ENTER]"
        /// Normalized: "OpenFile [ENTER]"
        /// </example>
        public bool IgnoreTimestamps { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore GUID differences in node text.
        /// </summary>
        /// <value>
        /// True to strip GUIDs before comparison; false to include GUIDs.
        /// Default is false.
        /// </value>
        /// <remarks>
        /// Useful when comparing logs that contain session IDs or transaction IDs.
        /// Detects common GUID formats:
        /// - Hyphenated: 550e8400-e29b-41d4-a716-446655440000
        /// - Braced: {550e8400-e29b-41d4-a716-446655440000}
        /// </remarks>
        public bool IgnoreGuids { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to trim all text before comparison.
        /// </summary>
        /// <value>
        /// True to trim whitespace from both ends; false to preserve exact spacing.
        /// Default is false.
        /// </value>
        public bool TrimText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use custom regex patterns for ignoring text.
        /// </summary>
        /// <value>
        /// True to apply <see cref="RegexIgnorePattern"/>; false to skip regex filtering.
        /// Default is false.
        /// </value>
        /// <remarks>
        /// This provides maximum flexibility for custom comparison rules.
        /// The regex pattern is applied to each node's text before comparison.
        /// </remarks>
        public bool UseRegexIgnorePatterns { get; set; }

        /// <summary>
        /// Gets or sets the regular expression pattern to apply when <see cref="UseRegexIgnorePatterns"/> is true.
        /// </summary>
        /// <value>
        /// A valid .NET regular expression pattern. Empty string if not used.
        /// </value>
        /// <example>
        /// To ignore all numbers: @"\d+"
        /// To ignore thread IDs: @"Thread\[\d+\]"
        /// To ignore memory addresses: @"0x[0-9A-Fa-f]+"
        /// </example>
        public string RegexIgnorePattern { get; set; }

        /// <summary>
        /// Normalizes a text string according to the configured comparison options.
        /// </summary>
        /// <param name="text">The text to normalize.</param>
        /// <returns>The normalized text ready for comparison.</returns>
        /// <remarks>
        /// This method applies all enabled normalization rules in sequence:
        /// 1. Case normalization (if IgnoreCase)
        /// 2. Whitespace trimming (if TrimText)
        /// 3. Timestamp removal (if IgnoreTimestamps)
        /// 4. GUID removal (if IgnoreGuids)
        /// 5. Custom regex replacement (if UseRegexIgnorePatterns)
        /// 6. Whitespace normalization (if IgnoreWhitespace)
        /// </remarks>
        public string NormalizeText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text ?? string.Empty;

            string normalized = text;

            // Step 1: Case normalization
            if (IgnoreCase)
            {
                normalized = normalized.ToLowerInvariant();
            }

            // Step 2: Trim if requested
            if (TrimText)
            {
                normalized = normalized.Trim();
            }

            // Step 3: Remove timestamps
            if (IgnoreTimestamps)
            {
                // ISO datetime: 2025-01-15 10:23:45.123 or 2025-01-15T10:23:45.123Z
                normalized = Regex.Replace(normalized, @"\d{4}-\d{2}-\d{2}[T\s]\d{2}:\d{2}:\d{2}(\.\d{1,9})?(Z|[+-]\d{2}:\d{2})?", string.Empty);

                // Standalone dates: 2025-01-15, 01/15/2025, 15-Jan-2025
                normalized = Regex.Replace(normalized, @"\b\d{4}-\d{2}-\d{2}\b", string.Empty);
                normalized = Regex.Replace(normalized, @"\b\d{2}/\d{2}/\d{4}\b", string.Empty);
                normalized = Regex.Replace(normalized, @"\b\d{2}-[A-Za-z]{3}-\d{4}\b", string.Empty);

                // Time only: 10:23:45.123
                normalized = Regex.Replace(normalized, @"\b\d{1,2}:\d{2}:\d{2}(\.\d{1,9})?\b", string.Empty);

                // Duration markers: [142 ms], [1.5s], (250ms)
                normalized = Regex.Replace(normalized, @"[\[\(]\s*\d+(\.\d+)?\s*(ms|s|sec|seconds?|milliseconds?)\s*[\]\)]", string.Empty, RegexOptions.IgnoreCase);

                // Unix timestamps (10 digits)
                normalized = Regex.Replace(normalized, @"\b\d{10,13}\b", string.Empty);
            }

            // Step 4: Remove GUIDs
            if (IgnoreGuids)
            {
                // Standard GUID format with or without braces/parens
                normalized = Regex.Replace(normalized, @"[\{\(]?[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}[\}\)]?", string.Empty);

                // Compact GUID (32 hex chars)
                normalized = Regex.Replace(normalized, @"\b[0-9a-fA-F]{32}\b", string.Empty);
            }

            // Step 5: Apply custom regex pattern
            if (UseRegexIgnorePatterns && !string.IsNullOrEmpty(RegexIgnorePattern))
            {
                try
                {
                    normalized = Regex.Replace(normalized, RegexIgnorePattern, string.Empty);
                }
                catch (ArgumentException)
                {
                    // Invalid regex pattern, skip this step
                }
            }

            // Step 6: Normalize whitespace
            if (IgnoreWhitespace)
            {
                // Replace multiple spaces with single space
                normalized = Regex.Replace(normalized, @"\s+", " ");
                normalized = normalized.Trim();
            }

            return normalized;
        }

        /// <summary>
        /// Creates a default CompareOptions instance configured for log file comparison.
        /// </summary>
        /// <returns>A CompareOptions instance with timestamp and whitespace ignoring enabled.</returns>
        public static CompareOptions CreateDefaultLogOptions()
        {
            return new CompareOptions
            {
                IgnoreCase = false,
                IgnoreWhitespace = true,
                IgnoreTimestamps = true,
                IgnoreGuids = false,
                TrimText = true,
                UseRegexIgnorePatterns = false
            };
        }

        /// <summary>
        /// Creates a strict CompareOptions instance that considers all differences significant.
        /// </summary>
        /// <returns>A CompareOptions instance with all normalization disabled.</returns>
        public static CompareOptions CreateStrictOptions()
        {
            return new CompareOptions
            {
                IgnoreCase = false,
                IgnoreWhitespace = false,
                IgnoreTimestamps = false,
                IgnoreGuids = false,
                TrimText = false,
                UseRegexIgnorePatterns = false
            };
        }
    }
}
