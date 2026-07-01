namespace Cad3PLogBrowser.Services.Comparison
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using Cad3PLogBrowser.Models.Comparison;

    /// <summary>
    /// Provides navigation functionality for moving through tree comparison differences.
    /// Similar to Visual Studio's "Next Difference" / "Previous Difference" feature.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DifferenceNavigator manages navigation state and coordinates:
    /// - TreeView scrolling
    /// - Node selection
    /// - Node expansion
    /// - Synchronized left/right navigation
    /// </para>
    /// 
    /// <para><b>Features:</b></para>
    /// <list type="bullet">
    /// <item>Navigate forward/backward through differences</item>
    /// <item>Jump to first/last difference</item>
    /// <item>Track current position</item>
    /// <item>Synchronized left/right tree positioning</item>
    /// <item>Automatic node expansion for visibility</item>
    /// </list>
    /// </remarks>
    public class DifferenceNavigator
    {
        private List<TreeDifference> _differences;
        private int _currentIndex;
        private TreeView _leftTreeView;
        private TreeView _rightTreeView;
        private DifferenceHighlighter _highlighter;

        /// <summary>
        /// Initializes a new instance of the <see cref="DifferenceNavigator"/> class.
        /// </summary>
        /// <param name="leftTreeView">The left TreeView control.</param>
        /// <param name="rightTreeView">The right TreeView control.</param>
        public DifferenceNavigator(TreeView leftTreeView, TreeView rightTreeView)
        {
            _leftTreeView = leftTreeView ?? throw new ArgumentNullException(nameof(leftTreeView));
            _rightTreeView = rightTreeView ?? throw new ArgumentNullException(nameof(rightTreeView));
            _highlighter = new DifferenceHighlighter();
            _differences = new List<TreeDifference>();
            _currentIndex = -1;
        }

        /// <summary>
        /// Gets or sets the list of differences to navigate.
        /// </summary>
        /// <value>The complete list of TreeDifference objects.</value>
        /// <remarks>
        /// Set this property after comparison to enable navigation.
        /// Resets current index to -1 (before first difference).
        /// </remarks>
        public List<TreeDifference> Differences
        {
            get { return _differences; }
            set
            {
                _differences = value ?? new List<TreeDifference>();
                _currentIndex = -1;
            }
        }

        /// <summary>
        /// Gets the current difference index (0-based).
        /// </summary>
        /// <value>
        /// The index of the current difference, or -1 if no difference is selected.
        /// </value>
        public int CurrentIndex
        {
            get { return _currentIndex; }
        }

        /// <summary>
        /// Gets the total number of differences.
        /// </summary>
        public int TotalDifferences
        {
            get { return _differences?.Count ?? 0; }
        }

        /// <summary>
        /// Gets a value indicating whether there are any differences to navigate.
        /// </summary>
        public bool HasDifferences
        {
            get { return TotalDifferences > 0; }
        }

        /// <summary>
        /// Gets the current difference object.
        /// </summary>
        /// <value>The TreeDifference at the current index, or null if no difference is selected.</value>
        public TreeDifference CurrentDifference
        {
            get
            {
                if (_currentIndex >= 0 && _currentIndex < TotalDifferences)
                {
                    return _differences[_currentIndex];
                }
                return null;
            }
        }

        /// <summary>
        /// Navigates to the next difference.
        /// </summary>
        /// <returns>True if navigation succeeded; false if already at last difference.</returns>
        /// <remarks>
        /// If currently at the last difference, this method returns false and does not wrap around.
        /// </remarks>
        public bool NavigateNext()
        {
            if (!HasDifferences)
                return false;

            if (_currentIndex < TotalDifferences - 1)
            {
                _currentIndex++;
                NavigateToDifference(_currentIndex);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Navigates to the previous difference.
        /// </summary>
        /// <returns>True if navigation succeeded; false if already at first difference.</returns>
        /// <remarks>
        /// If currently before the first difference, navigates to the first difference.
        /// </remarks>
        public bool NavigatePrevious()
        {
            if (!HasDifferences)
                return false;

            if (_currentIndex > 0)
            {
                _currentIndex--;
                NavigateToDifference(_currentIndex);
                return true;
            }
            else if (_currentIndex < 0)
            {
                // If before first, go to first
                _currentIndex = 0;
                NavigateToDifference(_currentIndex);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Navigates to the first difference.
        /// </summary>
        /// <returns>True if navigation succeeded; false if no differences exist.</returns>
        public bool NavigateFirst()
        {
            if (!HasDifferences)
                return false;

            _currentIndex = 0;
            NavigateToDifference(_currentIndex);
            return true;
        }

        /// <summary>
        /// Navigates to the last difference.
        /// </summary>
        /// <returns>True if navigation succeeded; false if no differences exist.</returns>
        public bool NavigateLast()
        {
            if (!HasDifferences)
                return false;

            _currentIndex = TotalDifferences - 1;
            NavigateToDifference(_currentIndex);
            return true;
        }

        /// <summary>
        /// Navigates to a specific difference by index.
        /// </summary>
        /// <param name="index">The 0-based index of the difference to navigate to.</param>
        /// <returns>True if navigation succeeded; false if index is out of range.</returns>
        public bool NavigateToIndex(int index)
        {
            if (index < 0 || index >= TotalDifferences)
                return false;

            _currentIndex = index;
            NavigateToDifference(_currentIndex);
            return true;
        }

        /// <summary>
        /// Performs the actual navigation to the specified difference.
        /// </summary>
        /// <param name="index">The index of the difference to navigate to.</param>
        /// <remarks>
        /// This method:
        /// 1. Expands parent nodes to make the target visible
        /// 2. Selects the target nodes in both trees
        /// 3. Scrolls both trees to show the target nodes
        /// 4. Ensures both trees are synchronized
        /// </remarks>
        private void NavigateToDifference(int index)
        {
            if (index < 0 || index >= TotalDifferences)
                return;

            TreeDifference diff = _differences[index];

            // Suspend layout for smoother updates
            _leftTreeView.BeginUpdate();
            _rightTreeView.BeginUpdate();

            try
            {
                // Clear previous selections
                _leftTreeView.SelectedNode = null;
                _rightTreeView.SelectedNode = null;

                // Navigate left tree
                if (diff.LeftTreeNode != null)
                {
                    _highlighter.ExpandToNode(diff.LeftTreeNode);
                    _leftTreeView.SelectedNode = diff.LeftTreeNode;
                    diff.LeftTreeNode.EnsureVisible();

                    // Scroll to center if possible
                    ScrollToNode(_leftTreeView, diff.LeftTreeNode);
                }

                // Navigate right tree
                if (diff.RightTreeNode != null)
                {
                    _highlighter.ExpandToNode(diff.RightTreeNode);
                    _rightTreeView.SelectedNode = diff.RightTreeNode;
                    diff.RightTreeNode.EnsureVisible();

                    // Scroll to center if possible
                    ScrollToNode(_rightTreeView, diff.RightTreeNode);
                }
            }
            finally
            {
                _leftTreeView.EndUpdate();
                _rightTreeView.EndUpdate();
            }

            // Raise navigation event
            OnNavigated(new DifferenceNavigationEventArgs(diff, index, TotalDifferences));
        }

        /// <summary>
        /// Scrolls a TreeView to position the specified node near the center of the visible area.
        /// </summary>
        /// <param name="treeView">The TreeView to scroll.</param>
        /// <param name="node">The node to center.</param>
        /// <remarks>
        /// This provides a better user experience than EnsureVisible alone,
        /// which only guarantees the node is somewhere in the viewport.
        /// </remarks>
        private void ScrollToNode(TreeView treeView, TreeNode node)
        {
            if (treeView == null || node == null)
                return;

            // EnsureVisible does basic scrolling
            node.EnsureVisible();

            // Try to center the node by expanding nodes above and below
            // This is a heuristic approach since TreeView doesn't expose scroll position directly
            int visibleCount = treeView.VisibleCount;
            int targetOffset = visibleCount / 3; // Position node in upper third for readability

            TreeNode current = node;
            for (int i = 0; i < targetOffset && current.PrevVisibleNode != null; i++)
            {
                current = current.PrevVisibleNode;
            }

            if (current != null)
            {
                current.EnsureVisible();
            }
        }

        /// <summary>
        /// Gets a formatted string describing the current navigation position.
        /// </summary>
        /// <returns>A string like "Difference 5 of 23" or "No differences" if none exist.</returns>
        public string GetPositionText()
        {
            if (!HasDifferences)
            {
                return "No differences";
            }

            if (_currentIndex < 0)
            {
                return $"{TotalDifferences} difference(s) found";
            }

            return $"Difference {_currentIndex + 1} of {TotalDifferences}";
        }

        /// <summary>
        /// Occurs when navigation to a difference completes.
        /// </summary>
        public event EventHandler<DifferenceNavigationEventArgs> Navigated;

        /// <summary>
        /// Raises the Navigated event.
        /// </summary>
        /// <param name="e">Event arguments containing navigation details.</param>
        protected virtual void OnNavigated(DifferenceNavigationEventArgs e)
        {
            Navigated?.Invoke(this, e);
        }
    }

    /// <summary>
    /// Provides data for the Navigated event.
    /// </summary>
    public class DifferenceNavigationEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DifferenceNavigationEventArgs"/> class.
        /// </summary>
        /// <param name="difference">The difference that was navigated to.</param>
        /// <param name="index">The index of the difference.</param>
        /// <param name="total">The total number of differences.</param>
        public DifferenceNavigationEventArgs(TreeDifference difference, int index, int total)
        {
            Difference = difference;
            Index = index;
            Total = total;
        }

        /// <summary>
        /// Gets the difference that was navigated to.
        /// </summary>
        public TreeDifference Difference { get; private set; }

        /// <summary>
        /// Gets the 0-based index of the difference.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Gets the total number of differences.
        /// </summary>
        public int Total { get; private set; }
    }
}
