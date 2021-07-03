using System;
using System.Windows;

namespace Supremacy.Client
{
    public class DelegatingWeakRoutedEventListener : IWeakEventListener
    {
        private readonly RoutedEventHandler _handler;

        internal DelegatingWeakRoutedEventListener(RoutedEventHandler handler)
        {
            _handler = handler ?? throw new ArgumentNullException("handler");
        }

        #region Implementation of IWeakEventListener
        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!(e is RoutedEventArgs args))
            {
                return false;
            }

            _handler(sender, args);
            return true;
        }
        #endregion
    }
}