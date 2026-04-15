using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cad3PLogBrowser.Services.Navigation
{
    /// <summary>
    /// Manages bookmarks for log lines.
    /// Allows users to mark important lines and quickly jump between them.
    /// Bookmarks are persisted per file.
    /// </summary>
    public class BookmarkService
    {
        // ??????????????????????????????????????????????????????????????????????
        // Fields
        // ??????????????????????????????????????????????????????????????????????

        private SortedSet<int> _bookmarkedLines = new SortedSet<int>();
        private string _currentFilePath = string.Empty;
        private int _currentBookmarkIndex = -1;

        // ??????????????????????????????????????????????????????????????????????
        // Properties
        // ??????????????????????????????????????????????????????????????????????

        /// <summary>Gets all bookmarked line numbers (1-based).</summary>
        public IReadOnlyCollection<int> BookmarkedLines => _bookmarkedLines;

        /// <summary>Gets the total number of bookmarks.</summary>
        public int Count => _bookmarkedLines.Count;

        // ??????????????????????????????????????????????????????????????????????
        // Public API
        // ??????????????????????????????????????????????????????????????????????

        /// <summary>
        /// Toggles bookmark on the specified line.
        /// </summary>
        /// <param name="lineNumber">1-based line number.</param>
        /// <returns>True if bookmark was added, false if removed.</returns>
        public bool ToggleBookmark(int lineNumber)
        {
            if (_bookmarkedLines.Contains(lineNumber))
            {
                _bookmarkedLines.Remove(lineNumber);
                return false;
            }
            else
            {
                _bookmarkedLines.Add(lineNumber);
                return true;
            }
        }

        /// <summary>
        /// Adds a bookmark at the specified line.
        /// </summary>
        public void AddBookmark(int lineNumber)
        {
            _bookmarkedLines.Add(lineNumber);
        }

        /// <summary>
        /// Removes bookmark from the specified line.
        /// </summary>
        public void RemoveBookmark(int lineNumber)
        {
            _bookmarkedLines.Remove(lineNumber);
        }

        /// <summary>
        /// Checks if a line is bookmarked.
        /// </summary>
        public bool IsBookmarked(int lineNumber)
        {
            return _bookmarkedLines.Contains(lineNumber);
        }

        /// <summary>
        /// Clears all bookmarks.
        /// </summary>
        public void ClearAllBookmarks()
        {
            _bookmarkedLines.Clear();
            _currentBookmarkIndex = -1;
        }

        /// <summary>
        /// Gets the next bookmarked line after the current line.
        /// Wraps around to the first bookmark if at the end.
        /// </summary>
        /// <param name="currentLine">Current line number (1-based).</param>
        /// <returns>Next bookmarked line, or -1 if no bookmarks.</returns>
        public int GetNextBookmark(int currentLine)
        {
            if (_bookmarkedLines.Count == 0)
                return -1;

            // SortedSet.GetViewBetween gives us lines > currentLine in O(log N)
            var after = _bookmarkedLines.GetViewBetween(currentLine + 1, int.MaxValue);
            if (after.Count > 0)
                return after.Min;

            // Wrap around
            return _bookmarkedLines.Min;
        }

        /// <summary>
        /// Gets the previous bookmarked line before the current line.
        /// Wraps around to the last bookmark if at the beginning.
        /// </summary>
        public int GetPreviousBookmark(int currentLine)
        {
            if (_bookmarkedLines.Count == 0)
                return -1;

            var before = _bookmarkedLines.GetViewBetween(int.MinValue, currentLine - 1);
            if (before.Count > 0)
                return before.Max;

            // Wrap around
            return _bookmarkedLines.Max;
        }

        /// <summary>
        /// Loads bookmarks for the specified file.
        /// </summary>
        public void LoadBookmarks(string filePath)
        {
            _currentFilePath = filePath;
            _bookmarkedLines.Clear();
            _currentBookmarkIndex = -1;

            if (string.IsNullOrEmpty(filePath))
                return;

            try
            {
                string bookmarkFile = GetBookmarkFilePath(filePath);

                if (File.Exists(bookmarkFile))
                {
                    var lines = File.ReadAllLines(bookmarkFile);
                    foreach (var line in lines)
                    {
                        if (int.TryParse(line.Trim(), out int lineNumber))
                        {
                            _bookmarkedLines.Add(lineNumber);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load bookmarks: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves bookmarks for the current file.
        /// </summary>
        public void SaveBookmarks()
        {
            if (string.IsNullOrEmpty(_currentFilePath))
                return;

            try
            {
                string bookmarkFile = GetBookmarkFilePath(_currentFilePath);
                string directory = Path.GetDirectoryName(bookmarkFile);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                // Save as simple text file (one line number per line)
                var lines = new string[_bookmarkedLines.Count];
                int idx = 0;
                foreach (var ln in _bookmarkedLines) lines[idx++] = ln.ToString();
                File.WriteAllLines(bookmarkFile, lines);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save bookmarks: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the bookmark file path for a given log file.
        /// </summary>
        private string GetBookmarkFilePath(string logFilePath)
        {
            string appDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CAD3PLogBrowser", "Bookmarks");

            // Create a safe filename from the log file path
            string safeFileName = Path.GetFileName(logFilePath) + ".bookmarks";

            return Path.Combine(appDataDir, safeFileName);
        }

        /// <summary>
        /// Gets all bookmarks as a sorted list.
        /// </summary>
        public List<int> GetAllBookmarksSorted()
        {
            return new List<int>(_bookmarkedLines); // already in ascending order
        }
    }
}
