namespace Cad3PLogBrowser.Models
{
    /// <summary>
    /// Represents a single search match result.
    /// Used in the Find All Results window to display all matches for a search term.
    /// </summary>
    /// <remarks>
    /// When the user performs a "Find All" search, the application searches the entire log
    /// and creates one SearchResult object for each match found.
    /// These results are then displayed in a dedicated window (FindAllResultsForm)
    /// where users can click to jump to specific matches.
    /// </remarks>
    /// <example>
    /// Find All search for "error":
    /// 
    /// Find All Results for "error" - 3 matches found
    /// ?????????????????????????????????????????????????????????????????????????
    /// ? Line #  ? Log Text                                                    ?
    /// ?????????????????????????????????????????????????????????????????????????
    /// ?   42    ? 2025-01-15 10:23:45: E: File not found error                ?
    /// ?  128    ? 2025-01-15 10:24:12: W: Connection error, retrying...       ?
    /// ?  235    ? 2025-01-15 10:25:33: E: Fatal error: Database unavailable   ?
    /// ?????????????????????????????????????????????????????????????????????????
    /// 
    /// Each row is one SearchResult object.
    /// </example>
    public class SearchResult
    {
        /// <summary>
        /// Gets or sets the 1-based line number where this match was found.
        /// </summary>
        /// <remarks>
        /// Used for:
        /// - Displaying in the results grid
        /// - Jumping to the matching line when user double-clicks
        /// - Sorting results by line number
        /// </remarks>
        public int LineNumber { get; set; }

        /// <summary>
        /// Gets or sets the complete text of the log line containing the match.
        /// This is the full line, not just the matching portion.
        /// </summary>
        /// <remarks>
        /// Displayed in the results window so users can see the context.
        /// The matching term is typically highlighted within this text.
        /// May be very long - the results window should support horizontal scrolling.
        /// </remarks>
        public string LineText { get; set; }

        /// <summary>
        /// Gets or sets the zero-based character position where the match starts within the line.
        /// -1 if position tracking is not enabled.
        /// </summary>
        /// <remarks>
        /// Used for:
        /// - Highlighting the exact matching text in the results window
        /// - Implementing "highlight in context" features
        /// - Regex match positioning
        /// 
        /// Example:
        /// LineText = "Error: File not found"
        /// SearchTerm = "File"
        /// MatchPosition = 7 (zero-based index of 'F' in "File")
        /// </remarks>
        public int MatchPosition { get; set; }

        /// <summary>
        /// Gets or sets the length of the matching text in characters.
        /// -1 if length tracking is not enabled.
        /// </summary>
        /// <remarks>
        /// Used for:
        /// - Highlighting the exact matching text (need both position and length)
        /// - Regex variable-length matches
        /// 
        /// Example:
        /// SearchTerm = "File not found"
        /// MatchLength = 14
        /// 
        /// Combined with MatchPosition, this allows precise highlighting:
        /// LineText.Substring(MatchPosition, MatchLength) gives the exact match.
        /// </remarks>
        public int MatchLength { get; set; }

        /// <summary>
        /// Gets or sets the search term that produced this match.
        /// Stored for reference and display purposes.
        /// </summary>
        /// <remarks>
        /// Useful for:
        /// - Displaying in the results window title ("Find All Results for '{SearchTerm}'")
        /// - Re-highlighting if results window is refreshed
        /// - Distinguishing results from different searches if multiple windows are open
        /// </remarks>
        public string SearchTerm { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this match was found using regex.
        /// Affects how the match should be highlighted and interpreted.
        /// </summary>
        public bool IsRegexMatch { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this match was case-sensitive.
        /// Stored for informational purposes.
        /// </summary>
        public bool WasCaseSensitive { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchResult"/> class.
        /// Sets default values.
        /// </summary>
        public SearchResult()
        {
            MatchPosition = -1;
            MatchLength = -1;
        }

        /// <summary>
        /// Creates a new SearchResult with basic information (line number and text only).
        /// </summary>
        /// <param name="lineNumber">The 1-based line number.</param>
        /// <param name="lineText">The full text of the matching line.</param>
        /// <returns>A SearchResult with position information set to -1.</returns>
        /// <remarks>
        /// Use this when you don't need to track the exact match position
        /// (e.g., for simple text searches where you just want to show matching lines).
        /// </remarks>
        public static SearchResult CreateBasic(int lineNumber, string lineText)
        {
            return new SearchResult
            {
                LineNumber = lineNumber,
                LineText = lineText
            };
        }

        /// <summary>
        /// Creates a new SearchResult with complete position information.
        /// </summary>
        /// <param name="lineNumber">The 1-based line number.</param>
        /// <param name="lineText">The full text of the matching line.</param>
        /// <param name="matchPosition">Zero-based position where match starts.</param>
        /// <param name="matchLength">Length of the matching text.</param>
        /// <param name="searchTerm">The search term that produced this match.</param>
        /// <returns>A fully initialized SearchResult.</returns>
        /// <remarks>
        /// Use this for precise highlighting and regex matches.
        /// </remarks>
        public static SearchResult CreateDetailed(
            int lineNumber,
            string lineText,
            int matchPosition,
            int matchLength,
            string searchTerm)
        {
            return new SearchResult
            {
                LineNumber = lineNumber,
                LineText = lineText,
                MatchPosition = matchPosition,
                MatchLength = matchLength,
                SearchTerm = searchTerm
            };
        }

        /// <summary>
        /// Gets the matching text extracted from the line.
        /// Returns empty string if position information is not available.
        /// </summary>
        /// <returns>The exact text that matched the search term.</returns>
        /// <example>
        /// LineText = "Error: File not found"
        /// MatchPosition = 7
        /// MatchLength = 14
        /// GetMatchedText() returns "File not found"
        /// </example>
        public string GetMatchedText()
        {
            if (string.IsNullOrEmpty(LineText) || MatchPosition < 0 || MatchLength < 0)
                return string.Empty;

            if (MatchPosition + MatchLength > LineText.Length)
                return string.Empty; // Safety check

            return LineText.Substring(MatchPosition, MatchLength);
        }

        /// <summary>
        /// Gets the line text with the matching portion highlighted using markers.
        /// </summary>
        /// <param name="highlightStart">The marker to place before the match (default: "?").</param>
        /// <param name="highlightEnd">The marker to place after the match (default: "?").</param>
        /// <returns>
        /// The line text with the match surrounded by markers.
        /// If position info is not available, returns the original line text.
        /// </returns>
        /// <example>
        /// LineText = "Error: File not found"
        /// Returns: "Error: ?File not found?"
        /// </example>
        public string GetHighlightedText(string highlightStart = "?", string highlightEnd = "?")
        {
            if (MatchPosition < 0 || MatchLength < 0)
                return LineText;

            return LineText.Substring(0, MatchPosition) +
                   highlightStart +
                   LineText.Substring(MatchPosition, MatchLength) +
                   highlightEnd +
                   LineText.Substring(MatchPosition + MatchLength);
        }

        /// <summary>
        /// Gets a truncated version of the line text for display in limited space.
        /// Shows context around the match if possible.
        /// </summary>
        /// <param name="maxLength">Maximum length of the returned string.</param>
        /// <returns>Truncated text with "..." indicators if shortened.</returns>
        /// <remarks>
        /// If the match position is known, truncates to show context around the match.
        /// Otherwise, truncates from the beginning.
        /// </remarks>
        public string GetTruncatedText(int maxLength = 100)
        {
            if (string.IsNullOrEmpty(LineText) || LineText.Length <= maxLength)
                return LineText;

            if (MatchPosition < 0)
            {
                // No position info - truncate from start
                return LineText.Substring(0, maxLength - 3) + "...";
            }

            // Try to show context around the match
            int contextBefore = 20;
            int contextAfter = maxLength - contextBefore - MatchLength - 6; // 6 for "..." on both sides

            int startPos = System.Math.Max(0, MatchPosition - contextBefore);
            int endPos = System.Math.Min(LineText.Length, MatchPosition + MatchLength + contextAfter);

            string prefix = startPos > 0 ? "..." : "";
            string suffix = endPos < LineText.Length ? "..." : "";

            return prefix + LineText.Substring(startPos, endPos - startPos) + suffix;
        }

        /// <summary>
        /// Returns a string representation for debugging.
        /// </summary>
        /// <returns>String with line number and text preview.</returns>
        public override string ToString()
        {
            string preview = LineText != null && LineText.Length > 50
                ? LineText.Substring(0, 50) + "..."
                : LineText;
            return $"Line {LineNumber}: {preview}";
        }

        /// <summary>
        /// Compares two SearchResult objects for equality based on line number.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>True if both have the same line number.</returns>
        public override bool Equals(object obj)
        {
            if (obj is SearchResult other)
                return this.LineNumber == other.LineNumber;
            return false;
        }

        /// <summary>
        /// Gets a hash code based on the line number.
        /// </summary>
        /// <returns>Hash code for use in collections.</returns>
        public override int GetHashCode()
        {
            return LineNumber.GetHashCode();
        }
    }
}
