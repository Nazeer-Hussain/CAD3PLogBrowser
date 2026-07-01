namespace Cad3PLogBrowser.Models.Comparison
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a generic tree node for comparison purposes.
    /// This is a UI-independent data structure that can be built from any hierarchical source.
    /// </summary>
    /// <remarks>
    /// LogNode is designed to decouple the comparison engine from the UI layer.
    /// It can be constructed from:
    /// - WinForms TreeNode objects
    /// - CallStackNode objects
    /// - ApiCallNode objects
    /// - Any other hierarchical data structure
    /// 
    /// This separation allows the comparison engine to be:
    /// - Unit tested without UI dependencies
    /// - Reused in different UI frameworks
    /// - Used in batch/headless scenarios
    /// </remarks>
    public class LogNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogNode"/> class.
        /// </summary>
        public LogNode()
        {
            Children = new List<LogNode>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogNode"/> class with specified text.
        /// </summary>
        /// <param name="text">The display text for this node.</param>
        public LogNode(string text) : this()
        {
            Text = text;
        }

        /// <summary>
        /// Gets or sets the display text of this node.
        /// </summary>
        /// <value>
        /// The text content that will be compared. This should represent the logical content
        /// of the node without UI-specific decorations.
        /// </value>
        /// <example>
        /// "CADSystem::OpenFile [ENTER]"
        /// "Database::Connect"
        /// "Error: Connection timeout"
        /// </example>
        public string Text { get; set; }

        /// <summary>
        /// Gets the list of child nodes.
        /// </summary>
        /// <value>
        /// A list of child LogNode objects. Never null (initialized in constructor).
        /// Empty list if this is a leaf node.
        /// </value>
        /// <remarks>
        /// Child order is preserved and significant during comparison.
        /// The comparison engine compares children in sequential order.
        /// </remarks>
        public List<LogNode> Children { get; private set; }

        /// <summary>
        /// Gets or sets the source object that this LogNode was created from.
        /// </summary>
        /// <value>
        /// A reference to the original object (e.g., TreeNode, CallStackNode).
        /// This is used to map comparison results back to the UI.
        /// </value>
        /// <remarks>
        /// This property creates a bidirectional link:
        /// - Original object ? LogNode (via factory method)
        /// - LogNode ? Original object (via this property)
        /// 
        /// Allows the UI to:
        /// - Highlight the original TreeNode when a difference is found
        /// - Navigate to the original node in the tree
        /// - Display tooltips with original data
        /// </remarks>
        public object Source { get; set; }

        /// <summary>
        /// Gets or sets the full path to this node in the tree hierarchy.
        /// </summary>
        /// <value>
        /// A string representing the path from root to this node.
        /// Example: "Root/Application::Start/Database::Connect"
        /// </value>
        /// <remarks>
        /// This is populated during comparison and used for:
        /// - Displaying difference location in the results list
        /// - Navigation and filtering
        /// - Tooltips and status messages
        /// 
        /// Path format uses forward slash (/) as separator.
        /// </remarks>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets additional metadata associated with this node.
        /// </summary>
        /// <value>
        /// A dictionary of key-value pairs for extensible metadata.
        /// Null if no metadata is attached.
        /// </value>
        /// <remarks>
        /// This allows future extensions without breaking the core model.
        /// Potential uses:
        /// - Line numbers
        /// - Timestamps
        /// - Performance metrics
        /// - Source file references
        /// - Tags and categories
        /// </remarks>
        public Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Gets a value indicating whether this node is a leaf (has no children).
        /// </summary>
        public bool IsLeaf
        {
            get { return Children == null || Children.Count == 0; }
        }

        /// <summary>
        /// Gets the depth of the tree rooted at this node (maximum distance to any leaf).
        /// </summary>
        /// <returns>The depth (0 for leaf nodes, 1+ for branch nodes).</returns>
        public int GetDepth()
        {
            if (IsLeaf)
                return 0;

            int maxChildDepth = 0;
            foreach (var child in Children)
            {
                int childDepth = child.GetDepth();
                if (childDepth > maxChildDepth)
                    maxChildDepth = childDepth;
            }

            return maxChildDepth + 1;
        }

        /// <summary>
        /// Gets the total number of nodes in the tree rooted at this node (including this node).
        /// </summary>
        /// <returns>The total node count.</returns>
        public int GetNodeCount()
        {
            int count = 1; // Count this node

            foreach (var child in Children)
            {
                count += child.GetNodeCount();
            }

            return count;
        }

        /// <summary>
        /// Returns a string representation of this node for debugging purposes.
        /// </summary>
        /// <returns>A string containing the node's text and child count.</returns>
        public override string ToString()
        {
            return $"{Text} ({Children.Count} children)";
        }
    }
}
