using System.Windows;

namespace Supremacy.Client.Controls
{
    /// <summary>
    /// Provides event arguments for <see cref="InfoCard"/>-related events.
    /// </summary>
    public class InfoCardEventArgs : CancelRoutedEventArgs
    {
        private readonly InfoCard _infoCard;

        /// <summary>
        /// Initializes a new instance of the <c>InfoCardEventArgs</c> class.
        /// </summary>
        /// <param name="infoCard">The <see cref="InfoCard"/> that is the focus of this event.</param>
        public InfoCardEventArgs(InfoCard infoCard) : this(infoCard, null, null) {}

        /// <summary>
        /// Initializes a new instance of the <c>InfoCardEventArgs</c> class.
        /// </summary>
        /// <param name="infoCard">The <see cref="InfoCard"/> that is the focus of this event.</param>
        /// <param name="routedEvent">The routed event identifier for this event arguments instance.</param>
        public InfoCardEventArgs(InfoCard infoCard, RoutedEvent routedEvent) : this(infoCard, routedEvent, null) {}

        /// <summary>
        /// Initializes a new instance of the <c>InfoCardEventArgs</c> class. 
        /// </summary>
        /// <param name="infoCard">The <see cref="InfoCard"/> that is the focus of this event.</param>
        /// <param name="routedEvent">The routed event identifier for this event arguments instance.</param>
        /// <param name="source">An alternate source that will be reported when the event is handled.</param>
        public InfoCardEventArgs(InfoCard infoCard, RoutedEvent routedEvent, object source)
            : base(routedEvent, source)
        {
            // Initialize parameters
            _infoCard = infoCard;
        }

        /// <summary>
        /// Gets or sets the <see cref="InfoCard"/> that is the focus of this event.
        /// </summary>
        /// <value>The <see cref="InfoCard"/> that is the focus of this event.</value>
        public InfoCard InfoCard
        {
            get { return _infoCard; }
        }
    }
}