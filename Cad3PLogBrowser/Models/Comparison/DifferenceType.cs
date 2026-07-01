namespace Cad3PLogBrowser.Models.Comparison
{
    /// <summary>
    /// Specifies the type of difference found during tree comparison.
    /// </summary>
    /// <remarks>
    /// This enum is designed to be extensible. Future versions may add:
    /// - NodeMoved: Node exists in both trees but at different positions
    /// - PropertyMismatch: Node exists but has different properties
    /// - TagMismatch: Node tag data differs
    /// - OrderChanged: Child order differs
    /// </remarks>
    public enum DifferenceType
    {
        /// <summary>
        /// The text content of matching nodes differs.
        /// </summary>
        /// <example>
        /// Left: "OpenFile [100 ms]"
        /// Right: "OpenFile [200 ms]"
        /// </example>
        TextMismatch,

        /// <summary>
        /// The number of child nodes differs between left and right.
        /// </summary>
        /// <example>
        /// Left node has 3 children, Right node has 5 children.
        /// </example>
        ChildCountMismatch,

        /// <summary>
        /// A node exists in the left tree but is missing in the right tree.
        /// </summary>
        /// <example>
        /// Left tree contains "Database::Connect" but right tree does not have this node at this position.
        /// </example>
        MissingInRight,

        /// <summary>
        /// A node exists in the right tree but is missing in the left tree.
        /// </summary>
        /// <example>
        /// Right tree contains "Cache::Initialize" but left tree does not have this node at this position.
        /// </example>
        ExtraInRight,

        /// <summary>
        /// A node is completely identical in both trees (used for filtering/display purposes).
        /// </summary>
        Identical
    }
}
