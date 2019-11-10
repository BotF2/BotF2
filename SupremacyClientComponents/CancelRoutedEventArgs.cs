using System.Windows;

namespace Supremacy.Client
{
    /// <summary>
    /// Provides event arguments for a cancelable routed event.
    /// </summary>
    public class CancelRoutedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <c>CancelRoutedEventArgs</c> class with the <see cref="Cancel"/> property set to <c>false</c>. 
        /// </summary>
        public CancelRoutedEventArgs() : this(false, null, null) { }

        /// <summary>
        /// Initializes a new instance of the <c>CancelRoutedEventArgs</c> class with the <see cref="Cancel"/> property set to <c>false</c>. 
        /// </summary>
        /// <param name="routedEvent">The routed event identifier for this event arguments instance.</param>
        public CancelRoutedEventArgs(RoutedEvent routedEvent) : this(false, routedEvent, null) { }

        /// <summary>
        /// Initializes a new instance of the <c>CancelRoutedEventArgs</c> class with the <see cref="Cancel"/> property set to <c>false</c>. 
        /// </summary>
        /// <param name="routedEvent">The routed event identifier for this event arguments instance.</param>
        /// <param name="source">An alternate source that will be reported when the event is handled.</param>
        public CancelRoutedEventArgs(RoutedEvent routedEvent, object source) : this(false, routedEvent, source) { }

        /// <summary>
        /// Initializes a new instance of the <c>CancelRoutedEventArgs</c> class with the <see cref="Cancel"/> property set to the given value. 
        /// </summary>
        /// <param name="cancel"><c>true</c> to cancel the event; otherwise, <c>false</c>.</param>
        public CancelRoutedEventArgs(bool cancel) : this(cancel, null, null) { }

        /// <summary>
        /// Initializes a new instance of the <c>CancelRoutedEventArgs</c> class with the <see cref="Cancel"/> property set to the given value. 
        /// </summary>
        /// <param name="cancel"><c>true</c> to cancel the event; otherwise, <c>false</c>.</param>
        /// <param name="routedEvent">The routed event identifier for this event arguments instance.</param>
        public CancelRoutedEventArgs(bool cancel, RoutedEvent routedEvent) : this(cancel, routedEvent, null) { }

        /// <summary>
        /// Initializes a new instance of the <c>CancelRoutedEventArgs</c> class with the <see cref="Cancel"/> property set to the given value. 
        /// </summary>
        /// <param name="cancel"><c>true</c> to cancel the event; otherwise, <c>false</c>.</param>
        /// <param name="routedEvent">The routed event identifier for this event arguments instance.</param>
        /// <param name="source">An alternate source that will be reported when the event is handled.</param>
        public CancelRoutedEventArgs(bool cancel, RoutedEvent routedEvent, object source)
            : base(routedEvent, source)
        {
            // Initialize parameters
            Cancel = cancel;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // PUBLIC PROCEDURES
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets a value indicating whether the event should be canceled.
        /// </summary>
        /// <value>
        /// <c>true</c> to cancel the event; otherwise, <c>false</c>.
        /// </value>
        public bool Cancel { get; set; }
    }
}