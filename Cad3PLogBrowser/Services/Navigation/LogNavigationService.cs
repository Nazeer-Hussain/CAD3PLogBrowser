namespace Cad3PLogBrowser.Services.Navigation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cad3PLogBrowser.Models;

    /// <summary>
    /// Handles navigation through log entries based on various criteria.
    /// Provides functionality for jumping to errors, warnings, specific lines, and search results.
    /// </summary>
    /// <remarks>
    /// This service manages navigation state (current position) and provides methods
    /// to move forward/backward through filtered subsets of log entries.
    /// 
    /// Features:
    /// - Navigate to next/previous error
    /// - Navigate to next/previous warning
    /// - Jump to specific line number
    /// - Jump to matching ENTER/EXIT pair
    /// - Wrap-around navigation (after last, goes to first)
    /// </remarks>
    public class LogNavigationService
    {
        // Navigation state
        private int _currentErrorIndex = -1;
        private int _currentWarningIndex = -1;

        // Cached lists of error and warning line indices
        private List<int> _errorLineIndices;
        private List<int> _warningLineIndices;

        /// <summary>
        /// Gets the total number of errors found in the current log.
        /// </summary>
        public int TotalErrors => _errorLineIndices?.Count ?? 0;

        /// <summary>
        /// Gets the total number of warnings found in the current log.
        /// </summary>
        public int TotalWarnings => _warningLineIndices?.Count ?? 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogNavigationService"/> class.
        /// </summary>
        public LogNavigationService()
        {
            _errorLineIndices = new List<int>();
            _warningLineIndices = new List<int>();
        }

        /// <summary>
        /// Indexes all errors and warnings from the log entries for fast navigation.
        /// This should be called whenever a new log file is loaded.
        /// </summary>
        /// <param name="entries">All log entries to index.</param>
        /// <remarks>
        /// This method scans all entries once and builds two lists:
        /// - Line indices of all error entries
        /// - Line indices of all warning entries
        /// 
        /// These lists are then used for fast forward/backward navigation.
        /// The indices are 0-based positions in the list view.
        /// </remarks>
        public void IndexErrorsAndWarnings(List<LogEntry> entries)
        {
            _errorLineIndices = new List<int>();
            _warningLineIndices = new List<int>();
            _currentErrorIndex = -1;
            _currentWarningIndex = -1;

            if (entries == null || entries.Count == 0)
                return;

            for (int i = 0; i < entries.Count; i++)
            {
                switch (entries[i].Level)
                {
                    case LogLevel.Error:
                        _errorLineIndices.Add(i);
                        break;

                    case LogLevel.Warning:
                        _warningLineIndices.Add(i);
                        break;
                }
            }
        }

        /// <summary>
        /// Finds the line index of the next error after the current position.
        /// Wraps around to the first error if at the end.
        /// </summary>
        /// <returns>
        /// Zero-based line index of the next error, or -1 if no errors exist.
        /// </returns>
        /// <remarks>
        /// Navigation wraps around: after the last error, goes to the first error.
        /// This allows users to cycle through all errors repeatedly.
        /// </remarks>
        /// <example>
        /// int errorLineIndex = navService.GetNextError();
        /// if (errorLineIndex >= 0)
        /// {
        ///     logListView.SelectedIndices.Clear();
        ///     logListView.SelectedIndices.Add(errorLineIndex);
        ///     logListView.EnsureVisible(errorLineIndex);
        /// }
        /// </example>
        public int GetNextError()
        {
            if (_errorLineIndices.Count == 0)
                return -1;

            // Move to next error (wrap around at end)
            _currentErrorIndex = (_currentErrorIndex + 1) % _errorLineIndices.Count;
            return _errorLineIndices[_currentErrorIndex];
        }

        /// <summary>
        /// Finds the line index of the previous error before the current position.
        /// Wraps around to the last error if at the beginning.
        /// </summary>
        /// <returns>
        /// Zero-based line index of the previous error, or -1 if no errors exist.
        /// </returns>
        public int GetPreviousError()
        {
            if (_errorLineIndices.Count == 0)
                return -1;

            // Move to previous error (wrap around at beginning)
            _currentErrorIndex--;
            if (_currentErrorIndex < 0)
                _currentErrorIndex = _errorLineIndices.Count - 1;

            return _errorLineIndices[_currentErrorIndex];
        }

        /// <summary>
        /// Finds the line index of the next warning after the current position.
        /// Wraps around to the first warning if at the end.
        /// </summary>
        /// <returns>
        /// Zero-based line index of the next warning, or -1 if no warnings exist.
        /// </returns>
        public int GetNextWarning()
        {
            if (_warningLineIndices.Count == 0)
                return -1;

            _currentWarningIndex = (_currentWarningIndex + 1) % _warningLineIndices.Count;
            return _warningLineIndices[_currentWarningIndex];
        }

        /// <summary>
        /// Finds the line index of the previous warning before the current position.
        /// Wraps around to the last warning if at the beginning.
        /// </summary>
        /// <returns>
        /// Zero-based line index of the previous warning, or -1 if no warnings exist.
        /// </returns>
        public int GetPreviousWarning()
        {
            if (_warningLineIndices.Count == 0)
                return -1;

            _currentWarningIndex--;
            if (_currentWarningIndex < 0)
                _currentWarningIndex = _warningLineIndices.Count - 1;

            return _warningLineIndices[_currentWarningIndex];
        }

        /// <summary>
        /// Resets navigation state (e.g., when a new log is loaded or filter is applied).
        /// </summary>
        public void ResetNavigationState()
        {
            _currentErrorIndex = -1;
            _currentWarningIndex = -1;
        }

        /// <summary>
        /// Gets a status message showing current navigation position.
        /// </summary>
        /// <param name="isError">True for error navigation, false for warning navigation.</param>
        /// <returns>
        /// String like "Error 3 of 7" or "Warning 1 of 12".
        /// Returns empty string if no errors/warnings exist.
        /// </returns>
        public string GetNavigationStatus(bool isError)
        {
            if (isError)
            {
                if (_errorLineIndices.Count == 0)
                    return string.Empty;
                return $"Error {_currentErrorIndex + 1} of {_errorLineIndices.Count}";
            }
            else
            {
                if (_warningLineIndices.Count == 0)
                    return string.Empty;
                return $"Warning {_currentWarningIndex + 1} of {_warningLineIndices.Count}";
            }
        }

        /// <summary>
        /// Validates if a line number is within valid range.
        /// </summary>
        /// <param name="lineNumber">1-based line number.</param>
        /// <param name="totalLines">Total number of lines in the log.</param>
        /// <returns>True if line number is valid (between 1 and totalLines).</returns>
        public bool IsValidLineNumber(int lineNumber, int totalLines)
        {
            return lineNumber >= 1 && lineNumber <= totalLines;
        }

        /// <summary>
        /// Converts a 1-based line number to a 0-based list view index.
        /// </summary>
        /// <param name="lineNumber">1-based line number (as users see it).</param>
        /// <returns>0-based index for accessing arrays/lists.</returns>
        /// <remarks>
        /// Users think in terms of line 1, 2, 3...
        /// But arrays/lists use 0-based indexing: 0, 1, 2...
        /// This method handles the conversion.
        /// </remarks>
        public int LineNumberToIndex(int lineNumber)
        {
            return lineNumber - 1;
        }

        /// <summary>
        /// Converts a 0-based list view index to a 1-based line number.
        /// </summary>
        /// <param name="index">0-based index in arrays/lists.</param>
        /// <returns>1-based line number for display to users.</returns>
        public int IndexToLineNumber(int index)
        {
            return index + 1;
        }
    }
}
