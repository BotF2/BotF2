using System;

using Supremacy.Messaging.Internal;

namespace Supremacy.Messaging
{
    public class ChannelSubscription<T>
        : ChannelSubscriptionBase<T>
    {
        private IObserver<T> _subscriber;

        public ChannelSubscription(ChannelThreadOption threadOption, IObserver<T> subscriber) :
            base(threadOption)
        {
            Guard.ArgumentNotNull(subscriber, "subscriber");
            _subscriber = subscriber;
        }

        #region Overrides
        public override IObserver<T> Subscriber
        {
            get { return _subscriber; }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            if (_subscriber != null)
                ((Channel<T>)Channel<T>.Public).Unsubscribe(this);

            _subscriber = null;
        }
        #endregion
    }
}