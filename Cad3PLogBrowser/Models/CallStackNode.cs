namespace Cad3PLogBrowser.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a single node in the call tree hierarchy.
    /// Models the call stack as a tree structure showing which methods call which.
    /// </summary>
    /// <remarks>
    /// The Call Tree displays the hierarchical call structure from the log file.
    /// Each node represents one method invocation with its context:
    /// - Parent node = the method that called this method
    /// - Child nodes = methods called by this method
    /// 
    /// This creates a visual representation of the execution flow.
    /// </remarks>
    /// <example>
    /// Log sequence:
    /// Line 10: "Application::Start [ENTER]"
    /// Line 11:   "Database::Connect [ENTER]"
    /// Line 12:   "Database::Connect [EXIT]"
    /// Line 13:   "UI::ShowWindow [ENTER]"
    /// Line 14:   "UI::ShowWindow [EXIT]"
    /// Line 15: "Application::Start [EXIT]"
    /// 
    /// Creates tree:
    /// Application::Start [142 ms]
    /// ??? Database::Connect [45 ms]
    /// ??? UI::ShowWindow [89 ms]
    /// </example>
    public class CallStackNode
    {
        /// <summary>
        /// Gets or sets the display label for this node.
        /// Typically the method name in format "ClassName::MethodName".
        /// </summary>
        /// <remarks>
        /// This is the text shown in the tree node before the duration overlay.
        /// Example: "CADSystem::OpenFile" becomes "CADSystem::OpenFile [142 ms]" in the UI.
        /// </remarks>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the source code file where this method is defined.
        /// Null if source information is not available in the log.
        /// </summary>
        /// <remarks>
        /// Some log formats include source file information.
        /// Example: "C:\Source\CADSystem\FileOperations.cs"
        /// Used in tooltip and for "jump to source" feature.
        /// </remarks>
        public string SourceFile { get; set; }

        /// <summary>
        /// Gets or sets the line number in the log file where this method's ENTER was found.
        /// This is a 1-based line number in the log file (not the source code).
        /// </summary>
        /// <remarks>
        /// Used for:
        /// - Clicking tree node to jump to this line in the log view
        /// - Matching ENTER with EXIT
        /// - Generating tooltips
        /// </remarks>
        public int LineNumber { get; set; }

        /// <summary>
        /// Gets or sets the line number in the log file where this method's EXIT was found.
        /// Zero if no matching EXIT was found (unmatched call).
        /// </summary>
        /// <remarks>
        /// Used for:
        /// - Calculating execution duration
        /// - Determining if call is matched (has both ENTER and EXIT)
        /// - Coloring node (green checkmark if matched, red cross if not)
        /// - "Jump to matching ENTER/EXIT" feature
        /// </remarks>
        public int ExitLineNumber { get; set; }

        /// <summary>
        /// Gets or sets the execution duration of this method in milliseconds.
        /// Calculated as: (EXIT timestamp - ENTER timestamp).
        /// Zero if no matching EXIT was found.
        /// </summary>
        /// <remarks>
        /// Used for:
        /// - Displaying "[142 ms]" in the tree node text
        /// - Color coding: green (&lt;100ms), amber (100-500ms), red (&gt;500ms)
        /// - Performance analysis and hotspot detection
        /// - Filtering by duration threshold
        /// </remarks>
        public long DurationMs { get; set; }

        /// <summary>
        /// Gets or sets the depth level of this node in the call tree.
        /// Root level = 0, first children = 1, etc.
        /// </summary>
        /// <remarks>
        /// Used for:
        /// - Indentation in exported text format
        /// - Call depth analysis
        /// - Detecting deep recursion
        /// </remarks>
        public int Depth { get; set; }

        /// <summary>
        /// Gets or sets the list of child nodes (methods called by this method).
        /// Empty list if this method doesn't call any other logged methods.
        /// </summary>
        /// <remarks>
        /// Each child represents a method that was called while this method was executing.
        /// The order of children matches the chronological order in the log file.
        /// </remarks>
        public List<CallStackNode> Children { get; set; }

        /// <summary>
        /// Gets or sets the parent node (the method that called this method).
        /// Null for root-level nodes (top-level entry points).
        /// </summary>
        /// <remarks>
        /// Used for:
        /// - Navigating up the call tree
        /// - Calculating self-time (time excluding children)
        /// - Displaying full call chain in tooltips
        /// </remarks>
        public CallStackNode Parent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this ENTER call had no matching EXIT
        /// (e.g. log was truncated or the API exited abnormally).
        /// When true the node is still surfaced in the tree but marked as unclosed.
        /// </summary>
        public bool IsUnclosed { get; set; }

        /// <summary>
        /// Gets a value indicating whether this call has a matching ENTER/EXIT pair.
        /// True if ExitLineNumber > 0.
        /// </summary>
        /// <value>
        /// True if EXIT was found; false if only ENTER exists.
        /// </value>
        /// <remarks>
        /// Determines the icon shown in the tree:
        /// - ? (green checkmark) if IsMatched = true
        /// - ? (red cross) if IsMatched = false
        /// </remarks>
        public bool IsMatched
        {
            get { return ExitLineNumber > 0; }
        }

        /// <summary>
        /// Gets the self-time: duration of this method excluding time spent in child calls.
        /// Measured in milliseconds.
        /// </summary>
        /// <value>
        /// DurationMs minus sum of all children's DurationMs.
        /// Zero if not matched or if children took longer (shouldn't happen but defensive).
        /// </value>
        /// <remarks>
        /// Self-time shows how much time was spent in this method's own code,
        /// not in methods it called. Useful for identifying where actual work is done.
        /// 
        /// Example:
        /// - Method A takes 100ms total
        /// - Calls Method B which takes 60ms
        /// - Self-time of A = 40ms (100 - 60)
        /// </remarks>
        public long SelfTimeMs
        {
            get
            {
                if (!IsMatched || Children == null || Children.Count == 0)
                    return DurationMs;

                long childrenTotal = 0;
                foreach (var child in Children)
                {
                    childrenTotal += child.DurationMs;
                }

                long selfTime = DurationMs - childrenTotal;
                return selfTime > 0 ? selfTime : 0; // Defensive: shouldn't be negative
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CallStackNode"/> class.
        /// Sets default values for collections.
        /// </summary>
        public CallStackNode()
        {
            Children = new List<CallStackNode>();
            Depth = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CallStackNode"/> class with a parent.
        /// Automatically sets depth based on parent.
        /// </summary>
        /// <param name="parent">The parent node that called this method.</param>
        public CallStackNode(CallStackNode parent) : this()
        {
            Parent = parent;
            Depth = parent != null ? parent.Depth + 1 : 0;
        }

        /// <summary>
        /// Gets the full display text for this node including duration overlay.
        /// </summary>
        /// <returns>
        /// String in format:
        /// - "MethodName [142 ms]" if matched
        /// - "MethodName [&lt;1 ms]" if matched with duration &lt; 1ms
        /// - "MethodName [? ms]" if not matched
        /// </returns>
        public string GetDisplayText()
        {
            if (IsUnclosed)
                return $"{Label}  [unclosed]";
            else if (!IsMatched)
                return $"{Label}  [? ms]";
            else if (DurationMs == 0)
                return $"{Label}  [<1 ms]";
            else
                return $"{Label}  [{DurationMs} ms]";
        }

        /// <summary>
        /// Gets the full call chain from root to this node.
        /// </summary>
        /// <returns>String showing the call path, e.g., "A ? B ? C ? D"</returns>
        /// <remarks>
        /// Used in tooltips and error messages to show context.
        /// Example: "Application::Start ? Database::Connect ? Connection::Open"
        /// </remarks>
        public string GetCallChain()
        {
            var chain = new List<string>();
            var current = this;

            while (current != null)
            {
                chain.Insert(0, current.Label);
                current = current.Parent;
            }

            return string.Join(" ? ", chain);
        }

        /// <summary>
        /// Recursively counts all descendant nodes (children, grandchildren, etc.).
        /// </summary>
        /// <returns>Total number of nodes in the subtree rooted at this node.</returns>
        public int CountDescendants()
        {
            int count = Children.Count;
            foreach (var child in Children)
            {
                count += child.CountDescendants();
            }
            return count;
        }

        /// <summary>
        /// Returns a detailed string representation for debugging.
        /// </summary>
        /// <returns>String with key properties and call chain.</returns>
        public override string ToString()
        {
            return $"{Label} (Ln {LineNumber}): {DurationMs}ms, {Children.Count} children, Depth {Depth}";
        }
    }
}
