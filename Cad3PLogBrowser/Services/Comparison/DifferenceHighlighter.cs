namespace Cad3PLogBrowser.Services.Comparison
{
    using System.Drawing;
    using System.Windows.Forms;
    using Cad3PLogBrowser.Models.Comparison;

    /// <summary>
    /// Applies visual highlighting to TreeView nodes based on comparison differences.
    /// </summary>
    /// <remarks>
    /// This class handles all visual aspects of displaying comparison results:
    /// - Color coding by difference type
    /// - Consistent color scheme
    /// - Node expansion for visibility
    /// - Background and foreground color management
    /// 
    /// Design follows the Visual Studio diff viewer color scheme for familiarity.
    /// </remarks>
    public class DifferenceHighlighter
    {
        // Color scheme inspired by Visual Studio and Beyond Compare
        private static readonly Color COLOR_IDENTICAL = Color.White;
        private static readonly Color COLOR_TEXT_MISMATCH = Color.FromArgb(255, 255, 200); // Light yellow
        private static readonly Color COLOR_MISSING = Color.FromArgb(255, 200, 200);      // Light red
        private static readonly Color COLOR_EXTRA = Color.FromArgb(200, 255, 200);        // Light green
        private static readonly Color COLOR_CHILD_MISMATCH = Color.FromArgb(255, 230, 180); // Light orange

        /// <summary>
        /// Initializes a new instance of the <see cref="DifferenceHighlighter"/> class.
        /// </summary>
        public DifferenceHighlighter()
        {
        }

        /// <summary>
        /// Highlights a TreeNode based on the difference type.
        /// </summary>
        /// <param name="treeNode">The TreeNode to highlight. If null, method returns without action.</param>
        /// <param name="differenceType">The type of difference to visualize.</param>
        /// <remarks>
        /// Color scheme:
        /// - Identical: White (no highlighting)
        /// - TextMismatch: Light yellow
        /// - MissingInRight: Light red (left tree)
        /// - ExtraInRight: Light green (right tree)
        /// - ChildCountMismatch: Light orange
        /// </remarks>
        public void HighlightNode(TreeNode treeNode, DifferenceType differenceType)
        {
            if (treeNode == null)
                return;

            Color backColor = GetColorForDifferenceType(differenceType);

            treeNode.BackColor = backColor;

            // Set appropriate foreground color for readability
            if (backColor.GetBrightness() > 0.7f)
            {
                treeNode.ForeColor = Color.Black;
            }
            else
            {
                treeNode.ForeColor = Color.White;
            }
        }

        /// <summary>
        /// Highlights all nodes referenced in the differences list.
        /// </summary>
        /// <param name="differences">The list of differences to visualize.</param>
        /// <remarks>
        /// This method applies highlighting to both left and right tree nodes.
        /// It handles cases where a node may appear in multiple differences
        /// (e.g., both text mismatch and child count mismatch).
        /// </remarks>
        public void HighlightDifferences(System.Collections.Generic.List<TreeDifference> differences)
        {
            if (differences == null)
                return;

            foreach (var diff in differences)
            {
                // Highlight left tree node
                if (diff.LeftTreeNode != null)
                {
                    HighlightNode(diff.LeftTreeNode, diff.Type);
                }

                // Highlight right tree node
                if (diff.RightTreeNode != null)
                {
                    // For missing/extra nodes, use the appropriate color
                    var rightType = diff.Type;
                    if (diff.Type == DifferenceType.MissingInRight)
                    {
                        // Don't highlight right node if it doesn't exist
                        continue;
                    }

                    HighlightNode(diff.RightTreeNode, rightType);
                }
            }
        }

        /// <summary>
        /// Clears all highlighting from a TreeView.
        /// </summary>
        /// <param name="treeView">The TreeView to clear. If null, method returns without action.</param>
        /// <remarks>
        /// This resets all nodes to default colors (white background, black text).
        /// Call this before applying new highlighting or when closing the comparison view.
        /// </remarks>
        public void ClearHighlighting(TreeView treeView)
        {
            if (treeView == null)
                return;

            ClearNodeHighlighting(treeView.Nodes);
        }

        /// <summary>
        /// Recursively clears highlighting from a collection of TreeNodes.
        /// </summary>
        /// <param name="nodes">The node collection to clear.</param>
        private void ClearNodeHighlighting(TreeNodeCollection nodes)
        {
            if (nodes == null)
                return;

            foreach (TreeNode node in nodes)
            {
                node.BackColor = COLOR_IDENTICAL;
                node.ForeColor = Color.Black;

                // Recursively clear children
                ClearNodeHighlighting(node.Nodes);
            }
        }

        /// <summary>
        /// Expands all parent nodes of a given TreeNode to make it visible.
        /// </summary>
        /// <param name="treeNode">The node to make visible.</param>
        /// <remarks>
        /// This method walks up the tree hierarchy expanding each parent.
        /// Essential for navigation - ensures the target node is visible and accessible.
        /// </remarks>
        public void ExpandToNode(TreeNode treeNode)
        {
            if (treeNode == null)
                return;

            TreeNode current = treeNode.Parent;

            while (current != null)
            {
                if (!current.IsExpanded)
                {
                    current.Expand();
                }
                current = current.Parent;
            }

            // Ensure the node itself is visible
            treeNode.EnsureVisible();
        }

        /// <summary>
        /// Expands both left and right tree nodes to make a difference visible.
        /// </summary>
        /// <param name="difference">The difference to make visible.</param>
        /// <remarks>
        /// This is a convenience method that calls ExpandToNode for both sides.
        /// Ensures synchronized visibility in side-by-side comparison view.
        /// </remarks>
        public void ExpandToDifference(TreeDifference difference)
        {
            if (difference == null)
                return;

            if (difference.LeftTreeNode != null)
            {
                ExpandToNode(difference.LeftTreeNode);
            }

            if (difference.RightTreeNode != null)
            {
                ExpandToNode(difference.RightTreeNode);
            }
        }

        /// <summary>
        /// Gets the appropriate background color for a difference type.
        /// </summary>
        /// <param name="type">The difference type.</param>
        /// <returns>A Color value appropriate for the difference type.</returns>
        private Color GetColorForDifferenceType(DifferenceType type)
        {
            switch (type)
            {
                case DifferenceType.TextMismatch:
                    return COLOR_TEXT_MISMATCH;

                case DifferenceType.ChildCountMismatch:
                    return COLOR_CHILD_MISMATCH;

                case DifferenceType.MissingInRight:
                    return COLOR_MISSING;

                case DifferenceType.ExtraInRight:
                    return COLOR_EXTRA;

                case DifferenceType.Identical:
                    return COLOR_IDENTICAL;

                default:
                    return COLOR_IDENTICAL;
            }
        }

        /// <summary>
        /// Creates a legend panel showing the color scheme.
        /// </summary>
        /// <returns>A Panel containing color swatches and labels.</returns>
        /// <remarks>
        /// This can be added to the comparison form to help users understand the colors.
        /// </remarks>
        public Panel CreateLegendPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                BorderStyle = BorderStyle.FixedSingle
            };

            int x = 10;
            AddLegendItem(panel, ref x, "Identical", COLOR_IDENTICAL);
            AddLegendItem(panel, ref x, "Text Differs", COLOR_TEXT_MISMATCH);
            AddLegendItem(panel, ref x, "Missing", COLOR_MISSING);
            AddLegendItem(panel, ref x, "Extra", COLOR_EXTRA);
            AddLegendItem(panel, ref x, "Child Count Differs", COLOR_CHILD_MISMATCH);

            return panel;
        }

        /// <summary>
        /// Helper method to add a legend item to the panel.
        /// </summary>
        private void AddLegendItem(Panel panel, ref int x, string label, Color color)
        {
            // Color swatch
            var swatch = new Panel
            {
                Left = x,
                Top = 5,
                Width = 20,
                Height = 20,
                BackColor = color,
                BorderStyle = BorderStyle.FixedSingle
            };
            panel.Controls.Add(swatch);
            x += 25;

            // Label
            var labelControl = new Label
            {
                Left = x,
                Top = 7,
                Width = 100,
                Height = 20,
                Text = label,
                AutoSize = true
            };
            panel.Controls.Add(labelControl);
            x += labelControl.Width + 20;
        }
    }
}
