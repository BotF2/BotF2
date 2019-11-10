using System;

namespace Supremacy.Messaging
{
    public abstract class ChannelSubscriptionBase<T>
        : IDisposable
    {
        private readonly ChannelThreadOption _threadOption;

        protected ChannelSubscriptionBase(ChannelThreadOption threadOption)
        {
            _threadOption = threadOption;
        }

        #region Properties
        public abstract IObserver<T> Subscriber { get; }

        public ChannelThreadOption ThreadOption
        {
            get { return _threadOption; }
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Overridable
        protected virtual void Dispose(bool disposing) { }
        #endregion
    }
}