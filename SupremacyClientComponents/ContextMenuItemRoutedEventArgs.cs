using System.Windows;
using System.Windows.Controls;

namespace Supremacy.Client
{
    /// <summary>
    /// Provides event arguments for a <see cref="ContextMenu"/>-related routed event.
    /// </summary>
    public class ContextMenuItemRoutedEventArgs : ItemRoutedEventArgs<ContextMenu>
    {
        /// <summary>
        /// Initializes a new instance of the <c>ContextMenuItemRoutedEventArgs</c> class.
        /// </summary>
        /// <param name="item">The item that is the focus of this event.</param>
        public ContextMenuItemRoutedEventArgs(ContextMenu item) : base(item) {}

        /// <summary>
        /// Initializes a new instance of the <c>ContextMenuItemRoutedEventArgs</c> class.
        /// </summary>
        /// <param name="item">The item that is the focus of this event.</param>
        /// <param name="routedEvent">The routed event identifier for this event arguments instance.</param>
        public ContextMenuItemRoutedEventArgs(ContextMenu item, RoutedEvent routedEvent) : base(item, routedEvent) {}

        /// <summary>
        /// Initializes a new instance of the <c>ContextMenuItemRoutedEventArgs</c> class. 
        /// </summary>
        /// <param name="item">The item that is the focus of this event.</param>
        /// <param name="routedEvent">The routed event identifier for this event arguments instance.</param>
        /// <param name="source">An alternate source that will be reported when the event is handled.</param>
        public ContextMenuItemRoutedEventArgs(ContextMenu item, RoutedEvent routedEvent, object source)
            : base(item, routedEvent, source) {}
    }
}