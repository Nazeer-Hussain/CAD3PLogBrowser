namespace Cad3PLogBrowser.Models
{
    using System.Drawing;

    /// <summary>
    /// Represents a single row in the virtual list view that displays log entries.
    /// Optimized for efficient UI rendering with minimal memory overhead.
    /// </summary>
    /// <remarks>
    /// The log view uses virtual mode to handle large files (500k+ lines) efficiently.
    /// Instead of creating ListView items for all lines upfront, items are created
    /// on-demand when they become visible (the RetrieveVirtualItem event).
    /// 
    /// This struct stores exactly what the ListView needs for each row:
    /// - Line number (as formatted string, ready to display)
    /// - Text content (the actual log line)
    /// - Background color (for error highlighting, search highlighting, etc.)
    /// 
    /// Using a struct instead of a class reduces memory allocation and GC pressure.
    /// </remarks>
    /// <example>
    /// Virtual list view population:
    /// 
    /// // Step 1: Parse log file and create VirtualLogLine array
    /// var virtualLines = new List&lt;VirtualLogLine&gt;();
    /// for (int i = 0; i < logEntries.Count; i++)
    /// {
    ///     var entry = logEntries[i];
    ///     virtualLines.Add(new VirtualLogLine
    ///     {
    ///         LineNumber = (i + 1).ToString(),
    ///         Text = entry.Text,
    ///         BackgroundColor = GetColorForLevel(entry.Level)
    ///     });
    /// }
    /// 
    /// // Step 2: Set virtual list size
    /// logListView.VirtualListSize = virtualLines.Count;
    /// 
    /// // Step 3: Handle RetrieveVirtualItem event
    /// void OnRetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
    /// {
    ///     var vl = virtualLines[e.ItemIndex];
    ///     var item = new ListViewItem(vl.LineNumber);
    ///     item.SubItems.Add(vl.Text);
    ///     item.BackColor = vl.BackgroundColor;
    ///     e.Item = item;
    /// }
    /// </example>
    public struct VirtualLogLine
    {
        /// <summary>
        /// Gets or sets the line number as a formatted string.
        /// Pre-formatted for display (e.g., "1", "42", "12345").
        /// </summary>
        /// <remarks>
        /// Stored as string (not int) because:
        /// - ListView displays strings, avoiding repeated ToString() calls
        /// - Can include formatting (e.g., "  42" with padding if needed)
        /// - Slightly faster rendering since no conversion needed
        /// 
        /// Always displayed in the first column of the list view.
        /// Right-aligned for easier reading of large line numbers.
        /// </remarks>
        public string LineNumber { get; set; }

        /// <summary>
        /// Gets or sets the complete text content of the log line.
        /// This is the raw line exactly as it appears in the log file.
        /// </summary>
        /// <remarks>
        /// Displayed in the second column of the list view.
        /// May be very long (1000+ characters in some logs).
        /// ListView handles horizontal scrolling automatically.
        /// 
        /// No truncation or modification is done - users see the exact log content.
        /// </remarks>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the background color for this row.
        /// Used for visual highlighting and categorization.
        /// </summary>
        /// <remarks>
        /// Common uses:
        /// - Error lines: LightCoral (red tint)
        /// - Warning lines: LightGoldenrodYellow (amber tint)
        /// - Search results: Yellow
        /// - Filter matches: PowderBlue
        /// - Normal lines: SystemColors.Window (white or theme background)
        /// 
        /// Color is applied to the entire row (both columns).
        /// ForeColor (text color) can be adjusted separately if needed.
        /// </remarks>
        public Color BackgroundColor { get; set; }

        /// <summary>
        /// Creates a new VirtualLogLine with all properties set.
        /// </summary>
        /// <param name="lineNumber">The line number as a string.</param>
        /// <param name="text">The log line text.</param>
        /// <param name="backgroundColor">The background color for highlighting.</param>
        /// <returns>A fully initialized VirtualLogLine.</returns>
        /// <example>
        /// var line = VirtualLogLine.Create("42", "ERROR: File not found", Color.LightCoral);
        /// </example>
        public static VirtualLogLine Create(string lineNumber, string text, Color backgroundColor)
        {
            return new VirtualLogLine
            {
                LineNumber = lineNumber,
                Text = text,
                BackgroundColor = backgroundColor
            };
        }

        /// <summary>
        /// Creates a new VirtualLogLine with default (normal) background color.
        /// </summary>
        /// <param name="lineNumber">The line number as a string.</param>
        /// <param name="text">The log line text.</param>
        /// <returns>A VirtualLogLine with white/theme background.</returns>
        /// <example>
        /// var line = VirtualLogLine.CreateNormal("42", "INFO: Application started");
        /// </example>
        public static VirtualLogLine CreateNormal(string lineNumber, string text)
        {
            return new VirtualLogLine
            {
                LineNumber = lineNumber,
                Text = text,
                BackgroundColor = SystemColors.Window
            };
        }

        /// <summary>
        /// Creates a new VirtualLogLine from a LogEntry model.
        /// Automatically determines background color based on log level.
        /// </summary>
        /// <param name="entry">The log entry to convert.</param>
        /// <returns>A VirtualLogLine ready for display.</returns>
        /// <remarks>
        /// This is a convenience method for converting from the domain model (LogEntry)
        /// to the display model (VirtualLogLine).
        /// </remarks>
        public static VirtualLogLine FromLogEntry(LogEntry entry)
        {
            Color bgColor = SystemColors.Window;

            // Set color based on log level
            switch (entry.Level)
            {
                case LogLevel.Error:
                    bgColor = Color.LightCoral; // Red tint
                    break;
                case LogLevel.Warning:
                    bgColor = Color.LightGoldenrodYellow; // Amber tint
                    break;
                // Debug and Info use default background
            }

            return new VirtualLogLine
            {
                LineNumber = entry.LineNumber.ToString(),
                Text = entry.Text,
                BackgroundColor = bgColor
            };
        }

        /// <summary>
        /// Updates the background color of this line (e.g., for highlighting).
        /// </summary>
        /// <param name="newColor">The new background color to apply.</param>
        /// <returns>A new VirtualLogLine with the updated color.</returns>
        /// <remarks>
        /// Since this is a struct, it's immutable by convention.
        /// This method returns a new instance with the changed color.
        /// 
        /// Used for:
        /// - Applying search result highlighting
        /// - Applying filter highlighting
        /// - Clearing highlighting
        /// </remarks>
        /// <example>
        /// // Highlight search results in yellow
        /// if (line.Text.Contains(searchTerm))
        /// {
        ///     line = line.WithBackgroundColor(Color.Yellow);
        /// }
        /// </example>
        public VirtualLogLine WithBackgroundColor(Color newColor)
        {
            return new VirtualLogLine
            {
                LineNumber = this.LineNumber,
                Text = this.Text,
                BackgroundColor = newColor
            };
        }

        /// <summary>
        /// Returns a preview of this line for debugging (first 50 chars of text).
        /// </summary>
        /// <returns>String showing line number and text preview.</returns>
        public override string ToString()
        {
            string preview = Text != null && Text.Length > 50 
                ? Text.Substring(0, 50) + "..." 
                : Text;
            return $"Line {LineNumber}: {preview}";
        }

        /// <summary>
        /// Checks if this line's text contains a search term.
        /// </summary>
        /// <param name="searchTerm">The term to search for.</param>
        /// <param name="caseSensitive">Whether the search is case-sensitive.</param>
        /// <returns>True if the text contains the search term.</returns>
        /// <remarks>
        /// Convenience method for search/filter operations.
        /// </remarks>
        public bool Contains(string searchTerm, bool caseSensitive = false)
        {
            if (string.IsNullOrEmpty(Text) || string.IsNullOrEmpty(searchTerm))
                return false;

            return caseSensitive
                ? Text.Contains(searchTerm)
                : Text.IndexOf(searchTerm, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
