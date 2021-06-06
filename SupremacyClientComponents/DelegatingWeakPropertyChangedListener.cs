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
            if (handler == null)
                throw new ArgumentNullException("handler");
            _handler = handler;
        }

        #region Implementation of IWeakEventListener
        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            PropertyChangedEventArgs args = e as PropertyChangedEventArgs;
            if (args == null)
                return false;
            _handler(sender, args);
            return true;
        }
        #endregion
    }
}