using System;
using System.Windows;

namespace Supremacy.Client
{
    public class DelegatingWeakRoutedEventListener : IWeakEventListener
    {
        private readonly RoutedEventHandler _handler;

        internal DelegatingWeakRoutedEventListener(RoutedEventHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");
            _handler = handler;
        }

        #region Implementation of IWeakEventListener
        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            RoutedEventArgs args = e as RoutedEventArgs;
            if (args == null)
                return false;
            _handler(sender, args);
            return true;
        }
        #endregion
    }
}