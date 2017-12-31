namespace Supremacy.Client
{
    /// <summary>
    /// Represents a node within the interaction hierarchy.
    /// </summary>
    public interface IInteractionNode
    {
        /// <summary>
        /// Gets the UI element.
        /// </summary>
        /// <value>The UI element.</value>
        object UIElement { get; }

        /// <summary>
        /// Finds the parent of this node.
        /// </summary>
        /// <returns>The parent or null if not found.</returns>
        IInteractionNode FindParent();
    }
}