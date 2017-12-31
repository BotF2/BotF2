using System;

using Supremacy.Messaging.Internal;

namespace Supremacy.Messaging
{
    public static class ChannelExtensions
    {
        public static IDisposable Subscribe<T>(
            this IChannel<T> source,
            Action<T> onNext = null,
            Action<Exception> onError = null,
            Action onCompleted = null,
            ChannelThreadOption threadOption = ChannelThreadOption.BackgroundThread,
            bool useWeakReference = false)
        {
            Guard.ArgumentNotNull(source, "source");
            return source.Subscribe(new RelayObserver<T>(onNext, onError, onCompleted), threadOption, useWeakReference);
        }
        public static IDisposable Subscribe<T>(
            this IChannel<T> source,
            IObserver<T> observer,
            ChannelThreadOption threadOption = ChannelThreadOption.BackgroundThread,
            bool useWeakReference = false)
        {
            Guard.ArgumentNotNull(source, "source");
            return source.Subscribe(observer, threadOption, useWeakReference);
        }
    }

    public class RelayObserver<T>
     : IObserver<T>
    {
        private readonly Action<T> _onNext;
        private readonly Action<Exception> _onError;
        private readonly Action _onCompleted;

        public RelayObserver(Action<T> onNext = null, Action<Exception> onError = null, Action onCompleted = null)
        {
            _onNext = onNext;
            _onError = onError;
            _onCompleted = onCompleted;
        }

        protected bool IsStopped { get; set; }

        #region IObserver<T> Members

        public void OnCompleted()
        {
            if (IsStopped || _onCompleted == null)
                return;

            IsStopped = true;

            _onCompleted();
        }

        public void OnError(Exception exception)
        {
            Guard.ArgumentNotNull(exception, "exception");

            if (IsStopped)
                return;

            IsStopped = true;

            if (_onError != null)
                _onError(exception);
            else
                throw exception;
        }

        public void OnNext(T value)
        {
            if (IsStopped || _onNext == null)
                return;

            _onNext(value);
        }
        #endregion
    }
}