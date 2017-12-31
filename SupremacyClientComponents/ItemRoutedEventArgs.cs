using System.Windows;

namespace Supremacy.Client
{
    /// <summary>
    /// Provides event arguments for an item-related routed event.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    public class ItemRoutedEventArgs<T> : RoutedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <c>ItemRoutedEventArgs</c> class.
        /// </summary>
        /// <param name="item">The item that is the focus of this event.</param>
        public ItemRoutedEventArgs(T item) : this(item, null, null) {}

        /// <summary>
        /// Initializes a new instance of the <c>ItemRoutedEventArgs</c> class.
        /// </summary>
        /// <param name="item">The item that is the focus of this event.</param>
        /// <param name="routedEvent">The routed event identifier for this event arguments instance.</param>
        public ItemRoutedEventArgs(T item, RoutedEvent routedEvent) : this(item, routedEvent, null) {}

        /// <summary>
        /// Initializes a new instance of the <c>ItemRoutedEventArgs</c> class. 
        /// </summary>
        /// <param name="item">The item that is the focus of this event.</param>
        /// <param name="routedEvent">The routed event identifier for this event arguments instance.</param>
        /// <param name="source">An alternate source that will be reported when the event is handled.</param>
        public ItemRoutedEventArgs(T item, RoutedEvent routedEvent, object source)
            : base(routedEvent, source)
        {
            // Initialize parameters
            Item = item;
        }

        /// <summary>
        /// Gets or sets the item that is the focus of this event.
        /// </summary>
        /// <value>The item that is the focus of this event.</value>
        public T Item { get; set; }
    }
}