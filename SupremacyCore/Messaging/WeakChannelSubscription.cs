using System;

using Supremacy.Messaging.Internal;

namespace Supremacy.Messaging
{
    public class WeakChannelSubscription<T>
         : ChannelSubscriptionBase<T>
    {
        WeakReference _subscriber;

        public WeakChannelSubscription(ChannelThreadOption threadOption, IObserver<T> subscriber)
         : base(threadOption)
        {
            Guard.ArgumentNotNull(subscriber, "subscriber");
            _subscriber = new WeakReference(subscriber);
        }

        #region Overrides

        public override IObserver<T> Subscriber
        {
            get
            {
                if (_subscriber == null || !_subscriber.IsAlive)
                {
                    return null;
                }

                IObserver<T> subscriberObj = _subscriber.Target as IObserver<T>;
                return subscriberObj;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_subscriber != null)
                {
                    ((Channel<T>)Channel<T>.Public).Unsubscribe(this);
                }
                _subscriber = null;
            }
        }

        #endregion

    }
}