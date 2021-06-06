using Supremacy.Messaging.Internal;
using System;
using System.Collections.Generic;
using System.Concurrency;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;

namespace Supremacy.Messaging
{
    public sealed class Channel<T>
        : Channel, IChannel<T>
    {
        #region Declarations
        private const string ChannelCannotbeClosed = "Channel of type '{0}' cannot be closed (via OnCompleted)";
        private const string ChannelWithkeyExists = "Private Channel of '{0}' with key '{1}' already exists";

        private static readonly Channel<T> _publicChannel;
        private static readonly Dictionary<string, IChannel<T>> _privateChannels;
        private static readonly Object _channelsLock = new object();

        private readonly Object _instanceLock = new Object();
        private readonly List<ChannelSubscriptionBase<T>> _subscriptions;
        #endregion

        #region Constructor
        static Channel()
        {
            _privateChannels = new Dictionary<string, IChannel<T>>(StringComparer.InvariantCultureIgnoreCase);
            _publicChannel = new Channel<T>();
        }

        private Channel()
        {
            _subscriptions = new List<ChannelSubscriptionBase<T>>();
        }
        #endregion

        #region Public Properties
        public static IChannel<T> Public => _publicChannel;

        public static Channel<T> Private => _publicChannel;

        public IChannel<T> this[string key]
        {
            get
            {
                Guard.ArgumentNotNullOrWhiteSpace(key, "key");

                lock (_channelsLock)
                {
                    IChannel<T> channel;

                    if (_privateChannels.TryGetValue(key, out channel))
                        return channel;
                }

                return CreateChannel(key);
            }
        }
        #endregion

        #region Private Channels
        private static IChannel<T> CreateChannel(string key)
        {
            //Guard.ArgumentNotNullOrWhiteSpace(key, "key");

            lock (_channelsLock)
            {
                if (_privateChannels.ContainsKey(key))
                {
                    throw new InvalidOperationException(
                        string.Format(
                            ChannelWithkeyExists,
                            typeof(T).FullName,
                            key));
                }

                var channel = new Channel<T>();
                _privateChannels.Add(key, channel);
                return channel;
            }
        }
        #endregion

        #region IChannel<T> Members
        void IObserver<T>.OnNext(T value)
        {
            OnNextInternal(value);
        }

        void IObserver<T>.OnError(Exception exception)
        {
            OnErrorInternal(exception);
        }

        void IObserver<T>.OnCompleted()
        {
            throw new InvalidOperationException(string.Format(ChannelCannotbeClosed, typeof(T).FullName));
        }

        IDisposable IObservable<T>.Subscribe(IObserver<T> subscriber)
        {
            return OnSubscribeInternal(subscriber, ChannelThreadOption.PublisherThread, false);
        }
        #endregion

        #region IChannel<T> Extensions
        IDisposable IChannel<T>.Subscribe(IObserver<T> subscriber, bool useWeakReference)
        {
            return OnSubscribeInternal(subscriber, ChannelThreadOption.PublisherThread, useWeakReference);
        }

        IDisposable IChannel<T>.Subscribe(IObserver<T> subscriber, ChannelThreadOption threadOption)
        {
            return OnSubscribeInternal(subscriber, threadOption, false);
        }

        IDisposable IChannel<T>.Subscribe(
            IObserver<T> subscriber, ChannelThreadOption threadOption, bool useWeakReference)
        {
            return OnSubscribeInternal(subscriber, threadOption, useWeakReference);
        }
        #endregion

        #region Internal
        internal void Unsubscribe(ChannelSubscriptionBase<T> subscription)
        {
            Guard.ArgumentNotNull(subscription, "subscription");

            lock (_instanceLock)
            {
                var subscribers = _subscriptions.ToArray();

                foreach (var subscriber in subscribers)
                {
                    if (subscriber.Subscriber == null)
                        _subscriptions.Remove(subscriber);

                    else if (ReferenceEquals(subscriber, subscription))
                        _subscriptions.Remove(subscriber);
                }
            }
        }
        #endregion

        #region Helpers
        private IDisposable OnSubscribeInternal(
            IObserver<T> subscriber,
            ChannelThreadOption threadOption,
            bool useWeakReference)
        {
            Guard.ArgumentNotNull(subscriber, "subscriber");

            ChannelSubscriptionBase<T> subscription;

            if (useWeakReference)
                subscription = new WeakChannelSubscription<T>(threadOption, subscriber);
            else
                subscription = new ChannelSubscription<T>(threadOption, subscriber);

            lock (_instanceLock)
            {
                // we add it to our collection
                _subscriptions.Add(subscription);

#if DEBUG && WRITETOCONSOLE
                Debug.WriteLine(string.Format("Subscribed, Total Subscribers in Channel {0}: {1}", typeof(T).FullName, _subscriptions.Count));
#endif
            }

            // we return
            return subscription;
        }

        private void OnNextInternal(T value)
        {
            ChannelSubscriptionBase<T>[] subscriptionsList;

            lock (_instanceLock)
            {
                if (_subscriptions.Count == 0)
                    return;

                subscriptionsList = _subscriptions.ToArray();
            }

            PublishInternal(value, subscriptionsList);
        }

        private void OnErrorInternal(Exception exception)
        {
            Guard.ArgumentNotNull(exception, "exception");

            ChannelSubscriptionBase<T>[] subscriptionsList;

            lock (_instanceLock)
            {
                if (_subscriptions.Count == 0)
                    return;

                subscriptionsList = _subscriptions.ToArray();
            }

            PublishErrorInternal(exception, subscriptionsList);
        }

        private void PublishInternal(T value, ChannelSubscriptionBase<T>[] subscriptionsList)
        {
            var deadSubscriptions = new Lazy<List<ChannelSubscriptionBase<T>>>(false);

            Array.Sort(subscriptionsList, (a, b) => a.ThreadOption.CompareTo(b.ThreadOption));

            foreach (var subscription in subscriptionsList)
            {
                var subscriber = subscription.Subscriber;

                if (subscriber == null)
                {
                    deadSubscriptions.Value.Add(subscription);
                    continue;
                }

                try
                {
                    IScheduler scheduler;

                    switch (subscription.ThreadOption)
                    {
                        case ChannelThreadOption.PublisherThread:
                            scheduler = Scheduler.Immediate;
                            break;

                        case ChannelThreadOption.UIThread:
                            scheduler = Scheduler.Dispatcher;
                            break;

                        case ChannelThreadOption.BackgroundThread:
                            scheduler = Scheduler.TaskPool;
                            break;

                        default:
                            throw new NotSupportedException();
                    }

                    var waitEvent = new ChannelEvent();

                    scheduler.Schedule(
                        () =>
                        {
                            subscriber.OnNext(value);
                            waitEvent.OnCompleted();
                        });

                    waitEvent.WaitOne();
                }
                catch
                {
                    //if (Debugger.IsAttached)
                    //    Debugger.Break();
                    //throw;
                }
            }

            if (deadSubscriptions.IsValueCreated &&
                deadSubscriptions.Value.Count > 0)
            {
                PurgeSubscriptions(deadSubscriptions.Value);
            }
        }

        private void PublishErrorInternal(Exception error, IEnumerable<ChannelSubscriptionBase<T>> subscriptionsList)
        {
            var deadSubscriptions = new Lazy<List<ChannelSubscriptionBase<T>>>();

            foreach (var subscription in subscriptionsList)
            {
                var subscriber = subscription.Subscriber;

                if (subscriber == null)
                {
                    deadSubscriptions.Value.Add(subscription);
                    continue;
                }
                
                try
                {
                    IScheduler scheduler;

                    switch (subscription.ThreadOption)
                    {
                        case ChannelThreadOption.PublisherThread:
                            scheduler = Scheduler.Immediate;
                            break;

                        case ChannelThreadOption.UIThread:
                            scheduler = Scheduler.Dispatcher;
                            break;

                        case ChannelThreadOption.BackgroundThread:
                            scheduler = Scheduler.TaskPool;
                            break;

                        default:
                            throw new NotSupportedException();
                    }

                    var waitEvent = new ChannelEvent();

                    scheduler.Schedule(
                        () =>
                        {
                            subscriber.OnError(error);
                            waitEvent.OnCompleted();
                        });

                    waitEvent.WaitOne();
                }
                catch
                {
                    if (Debugger.IsAttached)
                        Debugger.Break();
                    throw;
                }
            }

            if (deadSubscriptions.IsValueCreated &&
                deadSubscriptions.Value.Count > 0)
            {
                PurgeSubscriptions(deadSubscriptions.Value);
            }
        }

        private void PurgeSubscriptions(IEnumerable<ChannelSubscriptionBase<T>> subscriptions)
        {
            lock (_instanceLock)
            {
                foreach (var subscription in subscriptions)
                    _subscriptions.Remove(subscription);
            }
        }
        #endregion

        #region IChannel<T> Extensions
        void IChannel<T>.OnNext(T value, bool asynchronously)
        {
            if (asynchronously)
                Scheduler.ThreadPool.Schedule(() => OnNextInternal(value));
            else
                OnNextInternal(value);
        }

        void IChannel<T>.OnError(Exception exception, bool asynchronously)
        {
            Guard.ArgumentNotNull(exception, "exception");

            if (asynchronously)
                Scheduler.ThreadPool.Schedule(() => OnErrorInternal(exception));
            else
                OnErrorInternal(exception);
        }
        #endregion

        private sealed class ChannelEvent
        {
            public ChannelEvent(TimeSpan? timeout = null)
            {
                _syncLock = new object();
                _timeout = timeout;
                _eventClosed = false;

                if (Scheduler.Dispatcher.Dispatcher.CheckAccess())
                {
                    _frame = new DispatcherFrame();
                    _usingDispatcher = true;
                }
                else
                {
                    _event = new ManualResetEvent(false);
                    _usingDispatcher = false;
                }
            }

            public void OnCompleted()
            {
                lock (SyncLock)
                {
                    if (_eventClosed)
                        return;

                    if (_usingDispatcher)
                        _frame.Continue = false;
                    else
                        _event.Set();
                }
            }

            public void WaitOne()
            {
                if (_usingDispatcher)
                {
                    DispatcherTimer timer = null;

                    if (_timeout.HasValue)
                    {    
                        timer = new DispatcherTimer(
                            _timeout.Value,
                            DispatcherPriority.Send,
                            (sender, args) => OnCompleted(),
                            Scheduler.Dispatcher.Dispatcher);

                        timer.Start();
                    }

                    Dispatcher.PushFrame(_frame);

                    if (timer != null)
                        timer.Stop();
                }
                else
                {
                    _event.WaitOne(
                        _timeout.HasValue ? _timeout.Value : TimeSpan.FromMilliseconds(-1),
                        false);
                }

                lock (SyncLock)
                {
                    if (_eventClosed)
                        return;

                    /* 
                     * Close the event immediately instead of waiting for a GC because the Dispatcher
                     * may be a high-activity component, and and we could run out of events.
                     */
                    if (!_usingDispatcher)
                        _event.Close();

                    _eventClosed = true;
                }
            }

            private object SyncLock => _syncLock;

            private readonly object _syncLock;
            private readonly TimeSpan? _timeout;
            private readonly ManualResetEvent _event;
            private readonly DispatcherFrame _frame;
            private readonly bool _usingDispatcher;
            private bool _eventClosed;
        }

    }
}