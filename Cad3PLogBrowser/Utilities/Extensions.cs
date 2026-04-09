namespace Cad3PLogBrowser.Utilities
{
    using System;
    using System.Drawing;
    using System.Text.RegularExpressions;
    using Cad3PLogBrowser.Models;

    /// <summary>
    /// Extension methods for common operations throughout the application.
    /// These methods add functionality to existing types without modifying them.
    /// </summary>
    /// <remarks>
    /// Extension methods improve code readability and reduce repetition.
    /// They allow you to call methods as if they were part of the original type.
    /// 
    /// Example:
    /// Instead of: StringHelper.SafeSubstring(text, 0, 10)
    /// You can write: text.SafeSubstring(0, 10)
    /// </remarks>
    public static class Extensions
    {
        /// <summary>
        /// Safely extracts a substring without throwing exceptions for invalid indices.
        /// Returns empty string if the operation is not valid.
        /// </summary>
        /// <param name="text">The source string.</param>
        /// <param name="startIndex">The zero-based starting character position.</param>
        /// <param name="length">The number of characters to extract.</param>
        /// <returns>
        /// The extracted substring, or empty string if:
        /// - Text is null or empty
        /// - startIndex is negative or beyond the string length
        /// - length is negative
        /// </returns>
        /// <remarks>
        /// This is safer than String.Substring() which throws ArgumentOutOfRangeException.
        /// Useful when working with unpredictable log file content.
        /// </remarks>
        /// <example>
        /// string text = "Hello World";
        /// 
        /// // Safe - returns "Hello"
        /// text.SafeSubstring(0, 5);
        /// 
        /// // Safe - returns "" instead of throwing exception
        /// text.SafeSubstring(20, 5);
        /// text.SafeSubstring(-1, 5);
        /// 
        /// // Automatically truncates if length exceeds string
        /// text.SafeSubstring(6, 100); // Returns "World" (not exception)
        /// </example>
        public static string SafeSubstring(this string text, int startIndex, int length)
        {
            // Validate input
            if (string.IsNullOrEmpty(text) || startIndex < 0 || startIndex >= text.Length || length < 0)
                return string.Empty;

            // Truncate length if it would exceed the string
            int safeLength = Math.Min(length, text.Length - startIndex);
            return text.Substring(startIndex, safeLength);
        }

        /// <summary>
        /// Formats a duration in milliseconds as a human-readable string.
        /// Automatically chooses appropriate units (ms, sec, min).
        /// </summary>
        /// <param name="durationMs">Duration in milliseconds.</param>
        /// <returns>
        /// Formatted string with appropriate units:
        /// - "X ms" if less than 1 second
        /// - "X.X sec" if less than 60 seconds
        /// - "X.X min" if 60 seconds or more
        /// </returns>
        /// <example>
        /// 50L.FormatDuration()    // Returns "50 ms"
        /// 1500L.FormatDuration()  // Returns "1.5 sec"
        /// 90000L.FormatDuration() // Returns "1.5 min"
        /// </example>
        public static string FormatDuration(this long durationMs)
        {
            if (durationMs < 1000)
                return $"{durationMs} ms";
            else if (durationMs < 60000)
                return $"{durationMs / 1000.0:F1} sec";
            else
                return $"{durationMs / 60000.0:F1} min";
        }

        /// <summary>
        /// Escapes a string for safe use in CSV files.
        /// Wraps in quotes and escapes internal quotes if necessary.
        /// </summary>
        /// <param name="value">The string to escape.</param>
        /// <returns>
        /// CSV-safe string:
        /// - Empty string if value is null or empty
        /// - Original string if it doesn't contain special characters
        /// - Quoted and escaped string if it contains comma, quote, or newline
        /// </returns>
        /// <remarks>
        /// CSV special characters that require quoting:
        /// - Comma (,) - field separator
        /// - Quote (") - must be doubled ("")
        /// - Newline (\n) - line separator
        /// </remarks>
        /// <example>
        /// "Hello".EscapeCsv()                  // Returns "Hello"
        /// "Hello, World".EscapeCsv()           // Returns "\"Hello, World\""
        /// "Say \"Hi\"".EscapeCsv()             // Returns "\"Say \"\"Hi\"\"\""
        /// "Line1\nLine2".EscapeCsv()           // Returns "\"Line1\nLine2\""
        /// </example>
        public static string EscapeCsv(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            // Check if escaping is needed
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                // Escape internal quotes by doubling them
                string escaped = value.Replace("\"", "\"\"");
                // Wrap in quotes
                return $"\"{escaped}\"";
            }

            return value;
        }

        /// <summary>
        /// Gets the background color appropriate for a log level.
        /// Used for color-coding log entries in the UI.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <returns>
        /// Color based on severity:
        /// - Error: LightCoral (red tint)
        /// - Warning: LightGoldenrodYellow (amber tint)
        /// - Info/Debug: SystemColors.Window (default background)
        /// </returns>
        /// <example>
        /// LogLevel.Error.GetBackgroundColor()   // Returns Color.LightCoral
        /// LogLevel.Warning.GetBackgroundColor() // Returns Color.LightGoldenrodYellow
        /// LogLevel.Info.GetBackgroundColor()    // Returns SystemColors.Window
        /// </example>
        public static Color GetBackgroundColor(this LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Error:
                    return Color.LightCoral; // Red tint

                case LogLevel.Warning:
                    return Color.LightGoldenrodYellow; // Amber tint

                case LogLevel.Info:
                case LogLevel.Debug:
                default:
                    return SystemColors.Window; // Default (white or theme)
            }
        }

        /// <summary>
        /// Gets the foreground color for a call duration (for color-coding tree nodes).
        /// </summary>
        /// <param name="durationMs">The duration in milliseconds.</param>
        /// <returns>
        /// Color based on speed:
        /// - Green if duration &lt; 100ms (fast)
        /// - DarkOrange if duration 100-500ms (medium)
        /// - Red if duration &gt; 500ms (slow)
        /// </returns>
        /// <remarks>
        /// Uses thresholds from Constants.Performance:
        /// - FastCallThresholdMs = 100
        /// - SlowCallThresholdMs = 500
        /// </remarks>
        /// <example>
        /// 50L.GetDurationColor()   // Returns Color.Green
        /// 250L.GetDurationColor()  // Returns Color.DarkOrange
        /// 1000L.GetDurationColor() // Returns Color.Red
        /// </example>
        public static Color GetDurationColor(this long durationMs)
        {
            if (durationMs < Constants.Performance.FastCallThresholdMs)
                return Color.FromArgb(0, 128, 0); // Green

            if (durationMs < Constants.Performance.SlowCallThresholdMs)
                return Color.FromArgb(204, 102, 0); // DarkOrange (Amber)

            return Color.FromArgb(200, 0, 0); // Red
        }

        /// <summary>
        /// Checks if a string contains another string, case-insensitive.
        /// </summary>
        /// <param name="source">The string to search in.</param>
        /// <param name="value">The string to search for.</param>
        /// <returns>True if source contains value (case-insensitive).</returns>
        /// <remarks>
        /// This is a common operation, making it an extension improves readability.
        /// </remarks>
        /// <example>
        /// "Hello World".ContainsIgnoreCase("WORLD") // Returns true
        /// "Hello World".ContainsIgnoreCase("world") // Returns true
        /// "Hello World".ContainsIgnoreCase("xyz")   // Returns false
        /// </example>
        public static bool ContainsIgnoreCase(this string source, string value)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(value))
                return false;

            return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Truncates a string to a maximum length, adding "..." if truncated.
        /// </summary>
        /// <param name="text">The string to truncate.</param>
        /// <param name="maxLength">Maximum length including the "..." suffix.</param>
        /// <returns>
        /// Original string if shorter than maxLength,
        /// otherwise truncated string with "..." appended.
        /// </returns>
        /// <example>
        /// "Hello World".Truncate(20)  // Returns "Hello World"
        /// "Hello World".Truncate(8)   // Returns "Hello..."
        /// "Hello World".Truncate(5)   // Returns "He..."
        /// </example>
        public static string Truncate(this string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            if (maxLength <= 3)
                return "...";

            return text.Substring(0, maxLength - 3) + "...";
        }

        /// <summary>
        /// Validates if a string is a valid regular expression pattern.
        /// </summary>
        /// <param name="pattern">The regex pattern to validate.</param>
        /// <param name="errorMessage">Output parameter containing the error message if invalid.</param>
        /// <returns>True if valid regex pattern; false otherwise.</returns>
        /// <remarks>
        /// Used before applying regex searches to provide friendly error messages.
        /// </remarks>
        /// <example>
        /// string error;
        /// if (!"[invalid(".IsValidRegex(out error))
        /// {
        ///     MessageBox.Show($"Invalid regex: {error}");
        /// }
        /// </example>
        public static bool IsValidRegex(this string pattern, out string errorMessage)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                errorMessage = "Pattern is empty";
                return false;
            }

            try
            {
                Regex.Match("", pattern);
                errorMessage = null;
                return true;
            }
            catch (ArgumentException ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Converts a string to an integer safely, returning a default value if conversion fails.
        /// </summary>
        /// <param name="text">The string to convert.</param>
        /// <param name="defaultValue">The value to return if conversion fails (default: 0).</param>
        /// <returns>Parsed integer or default value.</returns>
        /// <example>
        /// "42".ToIntOrDefault()      // Returns 42
        /// "abc".ToIntOrDefault()     // Returns 0
        /// "abc".ToIntOrDefault(-1)   // Returns -1
        /// </example>
        public static int ToIntOrDefault(this string text, int defaultValue = 0)
        {
            if (int.TryParse(text, out int result))
                return result;
            return defaultValue;
        }

        /// <summary>
        /// Converts a string to a long integer safely, returning a default value if conversion fails.
        /// </summary>
        /// <param name="text">The string to convert.</param>
        /// <param name="defaultValue">The value to return if conversion fails (default: 0).</param>
        /// <returns>Parsed long integer or default value.</returns>
        public static long ToLongOrDefault(this string text, long defaultValue = 0)
        {
            if (long.TryParse(text, out long result))
                return result;
            return defaultValue;
        }

        /// <summary>
        /// Checks if a string is null, empty, or contains only whitespace.
        /// This is the opposite of string.IsNullOrWhiteSpace().
        /// </summary>
        /// <param name="text">The string to check.</param>
        /// <returns>True if string has meaningful content.</returns>
        /// <example>
        /// "Hello".HasValue()    // Returns true
        /// "".HasValue()         // Returns false
        /// "   ".HasValue()      // Returns false
        /// null.HasValue()       // Returns false
        /// </example>
        public static bool HasValue(this string text)
        {
            return !string.IsNullOrWhiteSpace(text);
        }

        /// <summary>
        /// Repeats a string a specified number of times.
        /// </summary>
        /// <param name="text">The string to repeat.</param>
        /// <param name="count">Number of times to repeat.</param>
        /// <returns>The repeated string.</returns>
        /// <example>
        /// "-".Repeat(10)     // Returns "----------"
        /// "ab".Repeat(3)     // Returns "ababab"
        /// "x".Repeat(0)      // Returns ""
        /// </example>
        public static string Repeat(this string text, int count)
        {
            if (string.IsNullOrEmpty(text) || count <= 0)
                return string.Empty;

            return string.Concat(System.Linq.Enumerable.Repeat(text, count));
        }

        /// <summary>
        /// Formats a file size in bytes as a human-readable string.
        /// </summary>
        /// <param name="bytes">Size in bytes.</param>
        /// <returns>
        /// Formatted string with appropriate units:
        /// - "X bytes" if less than 1KB
        /// - "X.X KB" if less than 1MB
        /// - "X.X MB" if less than 1GB
        /// - "X.X GB" if 1GB or more
        /// </returns>
        /// <example>
        /// 512L.FormatFileSize()          // Returns "512 bytes"
        /// 2048L.FormatFileSize()         // Returns "2.0 KB"
        /// 5242880L.FormatFileSize()      // Returns "5.0 MB"
        /// 1073741824L.FormatFileSize()   // Returns "1.0 GB"
        /// </example>
        public static string FormatFileSize(this long bytes)
        {
            const long KB = 1024;
            const long MB = KB * 1024;
            const long GB = MB * 1024;

            if (bytes < KB)
                return $"{bytes} bytes";
            else if (bytes < MB)
                return $"{bytes / (double)KB:F1} KB";
            else if (bytes < GB)
                return $"{bytes / (double)MB:F1} MB";
            else
                return $"{bytes / (double)GB:F1} GB";
        }
    }
}
