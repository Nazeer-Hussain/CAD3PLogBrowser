namespace Cad3PLogBrowser.Models.Comparison
{
    using System.Windows.Forms;

    /// <summary>
    /// Represents a single difference found during tree comparison.
    /// Contains all information needed to display, navigate to, and understand the difference.
    /// </summary>
    /// <remarks>
    /// TreeDifference is the core output of the comparison engine.
    /// Each instance describes one specific difference between the left and right trees.
    /// 
    /// These objects are:
    /// - Created by TreeComparer during comparison
    /// - Stored in a list for navigation
    /// - Displayed in the difference results window
    /// - Used to highlight nodes in the tree views
    /// </remarks>
    public class TreeDifference
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TreeDifference"/> class.
        /// </summary>
        public TreeDifference()
        {
        }

        /// <summary>
        /// Gets or sets the type of difference.
        /// </summary>
        /// <value>
        /// A <see cref="DifferenceType"/> enum value indicating the nature of the difference.
        /// </value>
        /// <example>
        /// TextMismatch: Node text differs
        /// MissingInRight: Node exists in left but not in right
        /// </example>
        public DifferenceType Type { get; set; }

        /// <summary>
        /// Gets or sets the hierarchical path to this difference.
        /// </summary>
        /// <value>
        /// A forward-slash separated path from root to the difference location.
        /// </value>
        /// <example>
        /// "Root/Application::Start/Database::Connect"
        /// "Root/Parser/Error"
        /// </example>
        /// <remarks>
        /// Used for:
        /// - Displaying in the difference list
        /// - Filtering differences
        /// - Navigation context
        /// - Export to reports
        /// </remarks>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the LogNode from the left tree.
        /// </summary>
        /// <value>
        /// The left-side node involved in this difference. May be null if the node doesn't exist in the left tree.
        /// </value>
        /// <remarks>
        /// Null when Type == ExtraInRight (node only exists in right tree).
        /// </remarks>
        public LogNode LeftNode { get; set; }

        /// <summary>
        /// Gets or sets the LogNode from the right tree.
        /// </summary>
        /// <value>
        /// The right-side node involved in this difference. May be null if the node doesn't exist in the right tree.
        /// </value>
        /// <remarks>
        /// Null when Type == MissingInRight (node only exists in left tree).
        /// </remarks>
        public LogNode RightNode { get; set; }

        /// <summary>
        /// Gets or sets the WinForms TreeNode from the left TreeView.
        /// </summary>
        /// <value>
        /// The actual UI TreeNode for highlighting and navigation. May be null.
        /// </value>
        /// <remarks>
        /// This is populated by the UI layer after comparison.
        /// Used for:
        /// - Highlighting the node with color
        /// - Expanding parent nodes
        /// - Scrolling to the node
        /// - Selecting the node
        /// </remarks>
        public TreeNode LeftTreeNode { get; set; }

        /// <summary>
        /// Gets or sets the WinForms TreeNode from the right TreeView.
        /// </summary>
        /// <value>
        /// The actual UI TreeNode for highlighting and navigation. May be null.
        /// </value>
        public TreeNode RightTreeNode { get; set; }

        /// <summary>
        /// Gets or sets the original value (left-side content).
        /// </summary>
        /// <value>
        /// A human-readable string describing the left-side value.
        /// </value>
        /// <example>
        /// "OpenFile [100 ms]"
        /// "3 children"
        /// "(node missing)"
        /// </example>
        /// <remarks>
        /// This is a formatted, user-friendly representation for display in the results grid.
        /// </remarks>
        public string OldValue { get; set; }

        /// <summary>
        /// Gets or sets the new value (right-side content).
        /// </summary>
        /// <value>
        /// A human-readable string describing the right-side value.
        /// </value>
        /// <example>
        /// "OpenFile [200 ms]"
        /// "5 children"
        /// "(node added)"
        /// </example>
        public string NewValue { get; set; }

        /// <summary>
        /// Gets or sets a detailed description of the difference.
        /// </summary>
        /// <value>
        /// An optional longer explanation of the difference. May be null.
        /// </value>
        /// <remarks>
        /// Used for tooltips and detailed reports.
        /// </remarks>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the zero-based index of this difference in the overall difference list.
        /// </summary>
        /// <value>
        /// The sequence number (0 = first difference, 1 = second, etc.).
        /// </value>
        /// <remarks>
        /// Used for navigation: "Difference 5 of 23"
        /// </remarks>
        public int Index { get; set; }

        /// <summary>
        /// Returns a string representation of this difference for debugging.
        /// </summary>
        /// <returns>A formatted string describing the difference.</returns>
        public override string ToString()
        {
            return $"{Type} at {Path}: {OldValue} ? {NewValue}";
        }

        /// <summary>
        /// Gets a brief single-line summary of this difference.
        /// </summary>
        /// <returns>A concise description suitable for list display.</returns>
        public string GetSummary()
        {
            switch (Type)
            {
                case DifferenceType.TextMismatch:
                    return $"Text differs: '{OldValue}' vs '{NewValue}'";

                case DifferenceType.ChildCountMismatch:
                    return $"Child count differs: {OldValue} vs {NewValue}";

                case DifferenceType.MissingInRight:
                    return $"Node missing in right tree: '{OldValue}'";

                case DifferenceType.ExtraInRight:
                    return $"Extra node in right tree: '{NewValue}'";

                case DifferenceType.Identical:
                    return "Nodes are identical";

                default:
                    return $"{Type}: {OldValue} ? {NewValue}";
            }
        }
    }
}
