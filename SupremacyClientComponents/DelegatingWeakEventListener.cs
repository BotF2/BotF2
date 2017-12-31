using System;
using System.Windows;

namespace Supremacy.Client
{
    public class DelegatingWeakEventListener<TArgs> : IWeakEventListener where TArgs : EventArgs
    {
        private readonly EventHandler<TArgs> _handler;

        public DelegatingWeakEventListener(EventHandler<TArgs> handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");
            _handler = handler;
        }

        #region Implementation of IWeakEventListener
        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            var args = e as TArgs;
            if (args == null)
                return false;
            _handler(sender, args);
            return true;
        }
        #endregion
    }
}