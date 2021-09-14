using System;

namespace Supremacy.Messaging
{
    public abstract class ChannelSubscriptionBase<T>
        : IDisposable
    {
        protected ChannelSubscriptionBase(ChannelThreadOption threadOption)
        {
            ThreadOption = threadOption;
        }

        #region Properties
        public abstract IObserver<T> Subscriber { get; }

        public ChannelThreadOption ThreadOption { get; }
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