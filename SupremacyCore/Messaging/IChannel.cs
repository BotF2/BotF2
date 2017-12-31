using System;
using System.Collections.Generic;
using System.Concurrency;
using System.Linq;

namespace Supremacy.Messaging
{
    public interface IChannel<T>
        : ISubject<T> {}


    public sealed class Channel<T> : IChannel<T>
    {
        #region Declarations

        private const string ChannelCannotBeClosed = "Channel of type '{0}' cannot be closed (via OnCompleted)";
        private const string ChannelWithKeyExists = "Private Channel of '{0}' with key '{1}' already exists";

        private readonly static Channel<T> _publicChannel;
        private readonly static Dictionary<string, IChannel<T>> _privateChannels;
        private readonly static Object _channelsLock = new object();
        private readonly Subject<T> _subject;
        private readonly IObservable<T> _observable;

        #endregion

        #region Constructor

        static Channel()
        {
            _privateChannels = new Dictionary<string, IChannel<T>>(StringComparer.InvariantCultureIgnoreCase);
            _publicChannel = new Channel<T>();
        }

        private Channel()
        {
            _subject = new Subject<T>();
            _observable = _subject.ObserveOn(Scheduler.ThreadPool);
        }

        #endregion

        #region Public Properties

        public static IChannel<T> Public
        {
            get { return _publicChannel; }
        }

        public static Channel<T> Private
        {
            get { return _publicChannel; }
        }

        public IChannel<T> this[string key]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Key must be a non-null, non-empty string.");

                lock (_channelsLock)
                {
                    IChannel<T> privateChannel;
                    
                    if (_privateChannels.TryGetValue(key, out privateChannel))
                        return privateChannel;
                    
                    return CreateChannel(key);
                }
            }
        }

        #endregion

        #region Private Channels

        private static IChannel<T> CreateChannel(string key)
        {
            lock (_channelsLock)
            {
                IChannel<T> channel;

                if (_privateChannels.TryGetValue(key, out channel))
                {
                    throw new InvalidOperationException(
                        string.Format(
                            ChannelWithKeyExists,
                            typeof(T).FullName,
                            key));
                }

                channel = new Channel<T>();

                _privateChannels.Add(key, channel);

                return channel;
            }
        }

        #endregion

        #region IObserver<T> Members

        void IObserver<T>.OnNext(T value)
        {
            _subject.OnNext(value);
        }

        void IObserver<T>.OnError(Exception exception)
        {
            _subject.OnError(exception);
        }

        void IObserver<T>.OnCompleted()
        {
            throw new InvalidOperationException(
                string.Format(
                    ChannelCannotBeClosed,
                    typeof(T).FullName));
        }

        #endregion

        public static void Publish(T payload)
        {
            Channel<T>.Public.OnNext(payload);
        }

        public static void Publish(string key, T payload)
        {
            Channel<T>.Private[key].OnNext(payload);
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _observable.Subscribe(observer);
        }

        public IDisposable Subscribe(IScheduler scheduler, IObserver<T> observer)
        {
            return _subject.ObserveOn(scheduler).Subscribe(observer);
        }

        public IDisposable Subscribe(IScheduler scheduler, Action<T> onNext = null, Action<Exception> onError = null, Action onCompleted = null)
        {
            return _subject
                .ObserveOn(scheduler)
                .Subscribe(
                    onNext,
                    onError,
                    onCompleted);
        }
    }
}