using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Windows.Data;

namespace Supremacy.Client.Events
{
    /*public sealed class DependecyPropertyChangedEventManager : WeakEventManager
    {
        public static void AddListener(object source, DependencyProperty dependencyProperty, IWeakEventListener listener)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (dependencyProperty == null)
                throw new ArgumentNullException("dependencyProperty");
            if (listener == null)
                throw new ArgumentNullException("listener");

            CurrentManager.PrivateAddListener(
                source,
                listener,
                dependencyProperty);
        }

        public static void RemoveListener(object source, DependencyProperty dependencyProperty, IWeakEventListener listener)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (dependencyProperty == null)
                throw new ArgumentNullException("dependencyProperty");
            if (listener == null)
                throw new ArgumentNullException("listener");

            CurrentManager.PrivateRemoveListener(
                source,
                listener,
                dependencyProperty);
        }

        private void PrivateAddListener(object source, IWeakEventListener listener, DependencyProperty dependencyProperty)
        {
            using (base.WriteLock)
            {
                var dictionary = base[source] as Dictionary<DependencyProperty, DependencyPropertyChangedWeakEventListenerRecord>;
                if (dictionary == null)
                {
                    dictionary = new Dictionary<DependencyProperty, DependencyPropertyChangedWeakEventListenerRecord>();
                    base[source] = dictionary;
                }
                
                DependencyPropertyChangedWeakEventListenerRecord record;

                if (!dictionary.TryGetValue(dependencyProperty, out record))
                {
                    record = new DependencyPropertyChangedWeakEventListenerRecord(this, source, dependencyProperty);
                    dictionary[dependencyProperty] = record;
                }
                
                record.Add(listener);

                base.ScheduleCleanup();
            }
        }

        private void PrivateRemoveListener(object source, IWeakEventListener listener, DependencyProperty dependencyProperty)
        {
            using (base.WriteLock)
            {
                var dictionary = base[source] as Dictionary<DependencyProperty, DependencyPropertyChangedWeakEventListenerRecord>;
                if (dictionary == null)
                    return;

                DependencyPropertyChangedWeakEventListenerRecord record;

                if (!dictionary.TryGetValue(dependencyProperty, out record))
                    return;
                
                record.Remove(listener);

                if (record.IsEmpty)
                    dictionary.Remove(dependencyProperty);

                if (dictionary.Count == 0)
                    base.Remove(source);
            }
        }

        protected override bool Purge(object source, object data, bool purgeAll)
        {
            var removedAnyEntries = false;
            var dictionary = (Dictionary<DependencyProperty, DependencyPropertyChangedWeakEventListenerRecord>)data;
            var keys = dictionary.Keys.ToList();

            foreach (var key in keys)
            {
                var isEmpty = (purgeAll || (source == null));

                DependencyPropertyChangedWeakEventListenerRecord record;
                if (!dictionary.TryGetValue(key, out record))
                    continue;

                if (!isEmpty)
                {
                    if (record.Purge())
                        removedAnyEntries = true;
                    isEmpty = record.IsEmpty;
                }

                if (!isEmpty)
                    continue;

                record.StopListening();

                if (!purgeAll)
                    dictionary.Remove(key);
            }

            if (dictionary.Count == 0)
            {
                removedAnyEntries = true;
                if (source != null)
                    base.Remove(source);
            }

            return removedAnyEntries;
        }

        public static void RemoveListener(object source, IWeakEventListener listener, DependencyProperty dependencyProperty)
        {
            CurrentManager.PrivateRemoveListener(source, listener, dependencyProperty);
        }

        protected override void StartListening(object source) {}

        protected override void StopListening(object source) {}

        private static DependecyPropertyChangedEventManager CurrentManager
        {
            get
            {
                var managerType = typeof(DependecyPropertyChangedEventManager);
                var manager = (DependecyPropertyChangedEventManager)GetCurrentManager(managerType);
                if (manager == null)
                {
                    manager = new DependecyPropertyChangedEventManager();
                    SetCurrentManager(managerType, manager);
                }
                return manager;
            }
        }

        private class DependencyPropertyChangedWeakEventListenerRecord
        {
            private readonly DependecyPropertyChangedEventManager _manager;
            private readonly DependencyPropertyDescriptor _dependencyPropertyDescriptor;
            private readonly WeakReference _source = new WeakReference(null);

            private ListenerList _listeners;

            internal DependencyPropertyChangedWeakEventListenerRecord(DependecyPropertyChangedEventManager manager, object source, DependencyProperty dependencyProperty)
            {
                if (manager == null)
                    throw new ArgumentNullException("manager");
                if (source == null)
                    throw new ArgumentNullException("source");
                if (dependencyProperty == null)
                    throw new ArgumentNullException("dependencyProperty");

                _listeners = new ListenerList();
                _listenerWrappers = new ConcurrentWeakDictionaryStrongValues<IWeakEventListener, WeakEventListenerWrapper>();
                _manager = manager;
                _source.Target = source;
                _dependencyPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(dependencyProperty, source.GetType());
                _dependencyPropertyDescriptor.AddValueChanged(source, HandleEvent);
            }

            internal void Add(IWeakEventListener listener)
            {
                ListenerList.PrepareForWriting(ref _listeners);

                var wrappedListener = listener as WeakEventListenerWrapper ??
                                      new WeakEventListenerWrapper(listener);

                if (!ReferenceEquals(listener, wrappedListener))
                    _listenerWrappers[listener] = wrappedListener;

                _listeners.Add(wrappedListener);
            }

            private void HandleEvent(object sender, EventArgs e)
            {
                using (_manager.ReadLock)
                {
                    _listeners.BeginUse();
                }
                try
                {
                    _manager.DeliverEventToList(sender, e, _listeners);
                }
                finally
                {
                    _listeners.EndUse();
                }
            }

            internal bool Purge()
            {
                ListenerList.PrepareForWriting(ref _listeners);

                var purgedAny = false;
                
                for (var i = 0; i < _listeners.Count; i++)
                {
                    var listener = _listeners[i] as WeakEventListenerWrapper;
                    if (listener == null)
                        continue;

                    if (listener.IsTargetAlive)
                        continue;

                    purgedAny = true;
                    Remove(_listeners[i--]);
                }

                return purgedAny || _listeners.Purge();
            }

            private readonly ConcurrentWeakDictionaryStrongValues<IWeakEventListener, WeakEventListenerWrapper> _listenerWrappers;

            internal void Remove(IWeakEventListener listener)
            {
                if (listener == null)
                    return;

                WeakEventListenerWrapper wrappedListener;

                if (!_listenerWrappers.TryPopValue(listener, out wrappedListener))
                    return;

                ListenerList.PrepareForWriting(ref _listeners);

                _listeners.Remove(wrappedListener);

                if (_listeners.IsEmpty)
                    StopListening();
            }

            internal void StopListening()
            {
                var target = _source.Target;
                if (target == null)
                    return;

                _source.Target = null;
                _dependencyPropertyDescriptor.RemoveValueChanged(target, HandleEvent);
            }

            internal bool IsEmpty
            {
                get { return _listeners.IsEmpty; }
            }
        }

        private sealed class WeakEventListenerWrapper : IWeakEventListener
        {
            private readonly WeakReference _wrappedListenerReference;

            public WeakEventListenerWrapper([NotNull] IWeakEventListener wrappedListener)
            {
                if (wrappedListener == null)
                    throw new ArgumentNullException("wrappedListener");
                _wrappedListenerReference = new WeakReference(wrappedListener);
            }

            public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
            {
                if (_wrappedListenerReference.IsAlive)
                    return true;

                var wrappedListener = _wrappedListenerReference.Target as IWeakEventListener;
                if (wrappedListener == null)
                    return true;

                return wrappedListener.ReceiveWeakEvent(managerType, sender, e);
            }

            public bool IsTargetAlive
            {
                get { return _wrappedListenerReference.IsAlive; }
            }
        }
    }*/

    public sealed class DependecyPropertyChangedEventManager : WeakEventManager
    {
        public static void AddListener(DependencyObject source, DependencyProperty dependencyProperty, IWeakEventListener listener)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (dependencyProperty == null)
                throw new ArgumentNullException("dependencyProperty");
            if (listener == null)
                throw new ArgumentNullException("listener");

            CurrentManager.PrivateAddListener(
                source,
                listener,
                dependencyProperty);
        }

        public static void RemoveListener(DependencyObject source, DependencyProperty dependencyProperty, IWeakEventListener listener)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (dependencyProperty == null)
                throw new ArgumentNullException("dependencyProperty");
            if (listener == null)
                throw new ArgumentNullException("listener");

            CurrentManager.PrivateRemoveListener(
                source,
                listener,
                dependencyProperty);
        }

        private void PrivateAddListener(DependencyObject source, IWeakEventListener listener, DependencyProperty dependencyProperty)
        {
            using (WriteLock)
            {
                var dictionary = base[source] as Dictionary<DependencyProperty, DependencyPropertyChangedWeakEventListenerRecord>;
                if (dictionary == null)
                {
                    dictionary = new Dictionary<DependencyProperty, DependencyPropertyChangedWeakEventListenerRecord>();
                    base[source] = dictionary;
                }

                DependencyPropertyChangedWeakEventListenerRecord record;

                if (!dictionary.TryGetValue(dependencyProperty, out record))
                {
                    record = new DependencyPropertyChangedWeakEventListenerRecord(this, source, dependencyProperty);
                    dictionary[dependencyProperty] = record;
                }

                record.Add(listener);

                ScheduleCleanup();
            }
        }

        private void PrivateRemoveListener(DependencyObject source, IWeakEventListener listener, DependencyProperty dependencyProperty)
        {
            using (WriteLock)
            {
                var dictionary = base[source] as Dictionary<DependencyProperty, DependencyPropertyChangedWeakEventListenerRecord>;
                if (dictionary == null)
                    return;

                DependencyPropertyChangedWeakEventListenerRecord record;

                if (!dictionary.TryGetValue(dependencyProperty, out record))
                    return;

                record.Remove(listener);

                if (record.IsEmpty)
                    dictionary.Remove(dependencyProperty);

                if (dictionary.Count == 0)
                    Remove(source);
            }
        }

        protected override bool Purge(object source, object data, bool purgeAll)
        {
            var removedAnyEntries = false;
            var dictionary = (Dictionary<DependencyProperty, DependencyPropertyChangedWeakEventListenerRecord>)data;
            var keys = dictionary.Keys.ToList();

            foreach (var key in keys)
            {
                var isEmpty = (purgeAll || (source == null));

                DependencyPropertyChangedWeakEventListenerRecord record;

                if (!dictionary.TryGetValue(key, out record))
                    continue;

                if (!isEmpty)
                {
                    if (record.Purge())
                        removedAnyEntries = true;

                    isEmpty = record.IsEmpty;
                }

                if (!isEmpty)
                    continue;

                record.StopListening();

                if (!purgeAll)
                    dictionary.Remove(key);
            }

            if (dictionary.Count == 0)
            {
                removedAnyEntries = true;

                if (source != null)
                    Remove(source);
            }

            return removedAnyEntries;
        }

        public static void RemoveListener(DependencyObject source, IWeakEventListener listener, DependencyProperty dependencyProperty)
        {
            CurrentManager.PrivateRemoveListener(source, listener, dependencyProperty);
        }

        protected override void StartListening(object source) { }

        protected override void StopListening(object source) { }

        private static DependecyPropertyChangedEventManager CurrentManager
        {
            get
            {
                var managerType = typeof(DependecyPropertyChangedEventManager);
                var manager = (DependecyPropertyChangedEventManager)GetCurrentManager(managerType);
                if (manager == null)
                {
                    manager = new DependecyPropertyChangedEventManager();
                    SetCurrentManager(managerType, manager);
                }
                return manager;
            }
        }

        private class DependencyPropertyChangedWeakEventListenerRecord : DependencyObject
        {
            #region ObservedValue Property
            private static readonly DependencyProperty ObservedValueProperty = DependencyProperty.Register(
                "ObservedValue",
                typeof(object),
                typeof(DependencyPropertyChangedWeakEventListenerRecord),
                new PropertyMetadata((d, e) => ((DependencyPropertyChangedWeakEventListenerRecord)d).OnObservedValueChanged()));

            private void OnObservedValueChanged()
            {
                if (_isConstructing)
                    return;

                if (!_source.IsAlive)
                    return;

                var source = _source.Target;
                if (source == null)
                    return;

                HandleEvent(source, EventArgs.Empty);
            }
            #endregion

            private readonly DependecyPropertyChangedEventManager _manager;
            private readonly WeakReference _source = new WeakReference(null);
            private readonly bool _isConstructing;
            private ListenerList _listeners;

            internal DependencyPropertyChangedWeakEventListenerRecord(DependecyPropertyChangedEventManager manager, DependencyObject source, DependencyProperty dependencyProperty)
            {
                if (manager == null)
                    throw new ArgumentNullException("manager");
                if (source == null)
                    throw new ArgumentNullException("source");
                if (dependencyProperty == null)
                    throw new ArgumentNullException("dependencyProperty");

                _listeners = new ListenerList();
                _manager = manager;
                _source.Target = source;

                _isConstructing = true;

                try
                {
                    BindingOperations.SetBinding(
                        this,
                        ObservedValueProperty,
                        new Binding
                        {
                            Source = source,
                            Path = new PropertyPath(dependencyProperty),
                            Mode = BindingMode.OneWay
                        });

                    CoerceValue(ObservedValueProperty);
                }
                finally
                {
                    _isConstructing = false;
                }
            }

            internal void Add(IWeakEventListener listener)
            {
                ListenerList.PrepareForWriting(ref _listeners);

                _listeners.Add(listener);
            }

            private void HandleEvent(object sender, EventArgs e)
            {
                using (_manager.ReadLock)
                {
                    _listeners.BeginUse();
                }
                try
                {
                    _manager.DeliverEventToList(sender, e, _listeners);
                }
                finally
                {
                    _listeners.EndUse();
                }
            }

            internal bool Purge()
            {
                ListenerList.PrepareForWriting(ref _listeners);

                if (!_source.IsAlive)
                {
                    while (_listeners.Count > 0)
                        Remove(_listeners[0]);

                    return true;
                }

                return _listeners.Purge();
            }

            internal void Remove(IWeakEventListener listener)
            {
                if (listener == null)
                    return;

                ListenerList.PrepareForWriting(ref _listeners);

                _listeners.Remove(listener);

                if (_listeners.IsEmpty)
                    StopListening();
            }

            internal void StopListening()
            {
                var target = _source.Target;
                if (target == null)
                    return;

                _source.Target = null;

                BindingOperations.ClearBinding(this, ObservedValueProperty);
            }

            internal bool IsEmpty
            {
                get { return _listeners.IsEmpty; }
            }
        }
    }
}