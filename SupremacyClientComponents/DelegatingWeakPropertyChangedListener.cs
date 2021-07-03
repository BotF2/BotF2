using System;
using System.ComponentModel;
using System.Windows;

namespace Supremacy.Client
{
    public class DelegatingWeakPropertyChangedListener : IWeakEventListener
    {
        private readonly PropertyChangedEventHandler _handler;

        public DelegatingWeakPropertyChangedListener(PropertyChangedEventHandler handler)
        {
            _handler = handler ?? throw new ArgumentNullException("handler");
        }

        #region Implementation of IWeakEventListener
        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!(e is PropertyChangedEventArgs args))
            {
                return false;
            }

            _handler(sender, args);
            return true;
        }
        #endregion
    }
}