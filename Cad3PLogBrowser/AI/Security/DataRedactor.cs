using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Cad3PLogBrowser.AI.Security
{
    /// <summary>
    /// Redacts sensitive information from text before sending to AI providers.
    /// Helps protect user privacy and comply with data protection policies.
    /// </summary>
    public class DataRedactor
    {
        private readonly List<RedactionRule> _rules;
        private readonly Dictionary<string, string> _replacementCache;

        public DataRedactor()
        {
            _rules = new List<RedactionRule>();
            _replacementCache = new Dictionary<string, string>();
            InitializeDefaultRules();
        }

        private void InitializeDefaultRules()
        {
            // Email addresses
            AddRule("EmailAddress", 
                @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b",
                "[EMAIL_REDACTED]");

            // IP addresses (IPv4)
            AddRule("IPAddress",
                @"\b(?:\d{1,3}\.){3}\d{1,3}\b",
                "[IP_REDACTED]");

            // Windows file paths (C:\, \\server\, etc.)
            AddRule("FilePath",
                @"(?:[A-Z]:\\|\\\\)[^\s\n]+",
                "[PATH_REDACTED]",
                caseSensitive: false);

            // Computer names (common patterns)
            AddRule("ComputerName",
                @"\b(?:DESKTOP|LAPTOP|PC|WORKSTATION|SERVER|WIN)-[A-Z0-9-]+\b",
                "[COMPUTER_REDACTED]",
                caseSensitive: false);

            // User names (in Windows paths)
            AddRule("UserName",
                @"\\Users\\([^\\]+)\\",
                @"\Users\[USER_REDACTED]\",
                caseSensitive: false);

            // API keys (common patterns)
            AddRule("ApiKey",
                @"\b(?:api[_-]?key|apikey|api[_-]?secret|token)[\""']?\s*[:=]\s*[\""']?([A-Za-z0-9_-]{20,})",
                "[API_KEY_REDACTED]",
                caseSensitive: false);

            // UUIDs/GUIDs
            AddRule("GUID",
                @"\b[A-Fa-f0-9]{8}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{12}\b",
                "[GUID_REDACTED]");
        }

        /// <summary>
        /// Adds a custom redaction rule.
        /// </summary>
        public void AddRule(string name, string pattern, string replacement, bool caseSensitive = true)
        {
            var options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
            _rules.Add(new RedactionRule
            {
                Name = name,
                Pattern = new Regex(pattern, options | RegexOptions.Compiled),
                Replacement = replacement
            });
        }

        /// <summary>
        /// Removes a redaction rule by name.
        /// </summary>
        public void RemoveRule(string name)
        {
            _rules.RemoveAll(r => r.Name == name);
        }

        /// <summary>
        /// Redacts sensitive information from text.
        /// </summary>
        public string Redact(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            string redacted = text;

            foreach (var rule in _rules)
            {
                redacted = rule.Pattern.Replace(redacted, rule.Replacement);
            }

            return redacted;
        }

        /// <summary>
        /// Redacts text with consistent replacement (same value always replaced with same token).
        /// Useful for maintaining relationships in the redacted text.
        /// </summary>
        public string RedactWithConsistentReplacement(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            string redacted = text;

            foreach (var rule in _rules)
            {
                var matches = rule.Pattern.Matches(redacted);
                foreach (Match match in matches)
                {
                    string original = match.Value;

                    if (!_replacementCache.TryGetValue(original, out string replacement))
                    {
                        // Generate consistent replacement token
                        int hashCode = Math.Abs(original.GetHashCode());
                        replacement = $"[{rule.Name}_{hashCode % 10000:D4}]";
                        _replacementCache[original] = replacement;
                    }

                    redacted = redacted.Replace(original, replacement);
                }
            }

            return redacted;
        }

        /// <summary>
        /// Clears the replacement cache.
        /// </summary>
        public void ClearCache()
        {
            _replacementCache.Clear();
        }

        /// <summary>
        /// Gets a summary of redactions performed.
        /// </summary>
        public RedactionSummary GetRedactionSummary(string originalText, string redactedText)
        {
            var summary = new RedactionSummary();

            foreach (var rule in _rules)
            {
                int count = rule.Pattern.Matches(originalText).Count;
                if (count > 0)
                {
                    summary.RedactionsByType[rule.Name] = count;
                    summary.TotalRedactions += count;
                }
            }

            summary.OriginalLength = originalText.Length;
            summary.RedactedLength = redactedText.Length;

            return summary;
        }

        private class RedactionRule
        {
            public string Name { get; set; }
            public Regex Pattern { get; set; }
            public string Replacement { get; set; }
        }

        public class RedactionSummary
        {
            public Dictionary<string, int> RedactionsByType { get; set; } = new Dictionary<string, int>();
            public int TotalRedactions { get; set; }
            public int OriginalLength { get; set; }
            public int RedactedLength { get; set; }

            public override string ToString()
            {
                if (TotalRedactions == 0)
                    return "No sensitive data redacted.";

                var parts = new List<string>();
                foreach (var kvp in RedactionsByType)
                {
                    parts.Add($"{kvp.Key}: {kvp.Value}");
                }

                return $"Redacted {TotalRedactions} items ({string.Join(", ", parts)})";
            }
        }
    }
}
