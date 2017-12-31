using System;
using System.Collections.Generic;

namespace Supremacy.Messaging
{
    public interface IChannel<T>
        : ISubject<T>
    {
        void OnNext(T value, bool asynchronously);
        void OnError(Exception exception, bool asynchronously);

        IDisposable Subscribe(IObserver<T> subscriber, ChannelThreadOption threadOption);
        IDisposable Subscribe(IObserver<T> subscriber, bool useWeakReference);
        IDisposable Subscribe(IObserver<T> subscriber, ChannelThreadOption threadOption, bool useWeakReference);
    }
}
