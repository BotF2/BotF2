using System;
using System.Collections.Specialized;
using System.Windows;

namespace Supremacy.Client
{
    public class DelegatingWeakCollectionChangedListener : IWeakEventListener
    {
        private readonly NotifyCollectionChangedEventHandler _handler;

        public DelegatingWeakCollectionChangedListener(NotifyCollectionChangedEventHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");
            _handler = handler;
        }

        #region Implementation of IWeakEventListener
        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            var args = e as NotifyCollectionChangedEventArgs;
            if (args == null)
                return false;
            _handler(sender, args);
            return true;
        }
        #endregion
    }
}