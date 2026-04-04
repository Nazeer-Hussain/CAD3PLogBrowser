using System;
using System.Collections.Generic;

namespace Cad3PLogBrowser.Services
{
    /// <summary>
    /// Stateful search and filter engine for a loaded list of log lines.
    /// Keeps track of the last search position to support Find Next.
    /// </summary>
    public class SearchService
    {
        // ── State ─────────────────────────────────────────────────────────────
        private int _lastFoundIndex = -1;
        private string _lastSearchTerm = string.Empty;
        private StringComparison _lastComparison = StringComparison.OrdinalIgnoreCase;

        // ── Reset ─────────────────────────────────────────────────────────────
        /// <summary>Call this whenever a new file is loaded to clear search position.</summary>
        public void Reset()
        {
            _lastFoundIndex = -1;
            _lastSearchTerm = string.Empty;
        }

        // ── Find ──────────────────────────────────────────────────────────────
        /// <summary>
        /// Searches forward from the last match position for <paramref name="searchTerm"/>
        /// in <paramref name="lines"/>. Wraps around to the beginning.
        /// </summary>
        /// <returns>
        /// The zero-based index of the matched line, or -1 if not found.
        /// </returns>
        public int FindNext(IList<string> lines, string searchTerm, bool matchCase)
        {
            if (lines == null || lines.Count == 0 || string.IsNullOrEmpty(searchTerm))
                return -1;

            var comp = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            // Reset position when the search term or case-mode changes.
            if (!string.Equals(searchTerm, _lastSearchTerm, comp) || comp != _lastComparison)
            {
                _lastFoundIndex = -1;
                _lastSearchTerm = searchTerm;
                _lastComparison = comp;
            }

            int start = _lastFoundIndex + 1;
            for (int i = 0; i < lines.Count; i++)
            {
                int idx = (start + i) % lines.Count;
                if (lines[idx].IndexOf(searchTerm, comp) >= 0)
                {
                    _lastFoundIndex = idx;
                    return idx;
                }
            }

            return -1;
        }

        // ── Filter ────────────────────────────────────────────────────────────
        /// <summary>
        /// Returns all lines (with their original 1-based line numbers) that contain
        /// <paramref name="filterText"/>. If <paramref name="filterText"/> is empty or
        /// whitespace, returns all lines unmodified.
        /// </summary>
        public List<FilteredLine> Filter(IList<string> allLines, string filterText, bool matchCase)
        {
            var result = new List<FilteredLine>();

            if (string.IsNullOrWhiteSpace(filterText))
            {
                for (int i = 0; i < allLines.Count; i++)
                    result.Add(new FilteredLine(i + 1, allLines[i]));
                return result;
            }

            var comp = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            for (int i = 0; i < allLines.Count; i++)
            {
                if (allLines[i].IndexOf(filterText, comp) >= 0)
                    result.Add(new FilteredLine(i + 1, allLines[i]));
            }

            return result;
        }

        // ── Copy helper ───────────────────────────────────────────────────────
        /// <summary>
        /// Joins <paramref name="lines"/> into a single newline-delimited string
        /// suitable for the clipboard.
        /// </summary>
        public string JoinForClipboard(IEnumerable<string> lines)
        {
            return string.Join(Environment.NewLine, lines);
        }
    }

    /// <summary>A log line paired with its original 1-based line number.</summary>
    public class FilteredLine
    {
        public int LineNumber { get; }
        public string Text { get; }

        public FilteredLine(int lineNumber, string text)
        {
            LineNumber = lineNumber;
            Text = text;
        }
    }
}
