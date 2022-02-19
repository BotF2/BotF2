using System;
using System.Windows;

namespace Supremacy.Client
{
    public class DelegatingWeakEventListener<TArgs> : IWeakEventListener where TArgs : EventArgs
    {
        private readonly EventHandler<TArgs> _handler;

        public DelegatingWeakEventListener(EventHandler<TArgs> handler)
        {
            _handler = handler ?? throw new ArgumentNullException("handler");
        }

        #region Implementation of IWeakEventListener
        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!(e is TArgs args))
            {
                return false;
            }

            _handler(sender, args);
            return true;
        }
        #endregion
    }
}