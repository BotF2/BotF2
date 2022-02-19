using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Windows.Data;

namespace Supremacy.Client.Events
{
    public sealed class DependecyPropertyChangedEventManager : WeakEventManager
    {
        public static void AddListener(DependencyObject source, DependencyProperty dependencyProperty, IWeakEventListener listener)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (dependencyProperty == null)
            {
                throw new ArgumentNullException("dependencyProperty");
            }

            if (listener == null)
            {
                throw new ArgumentNullException("listener");
            }

            CurrentManager.PrivateAddListener(
                source,
                listener,
                dependencyProperty);
        }

        public static void RemoveListener(DependencyObject source, DependencyProperty dependencyProperty, IWeakEventListener listener)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (dependencyProperty == null)
            {
                throw new ArgumentNullException("dependencyProperty");
            }

            if (listener == null)
            {
                throw new ArgumentNullException("listener");
            }

            CurrentManager.PrivateRemoveListener(
                source,
                listener,
                dependencyProperty);
        }

        private void PrivateAddListener(DependencyObject source, IWeakEventListener listener, DependencyProperty dependencyProperty)
        {
            using (WriteLock)
            {
                if (!(base[source] is Dictionary<DependencyProperty, DependencyPropertyChangedWeakEventListenerRecord> dictionary))
                {
                    dictionary = new Dictionary<DependencyProperty, DependencyPropertyChangedWeakEventListenerRecord>();
                    base[source] = dictionary;
                }


                if (!dictionary.TryGetValue(dependencyProperty, out DependencyPropertyChangedWeakEventListenerRecord record))
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
                if (!(base[source] is Dictionary<DependencyProperty, DependencyPropertyChangedWeakEventListenerRecord> dictionary))
                {
                    return;
                }


                if (!dictionary.TryGetValue(dependencyProperty, out DependencyPropertyChangedWeakEventListenerRecord record))
                {
                    return;
                }

                record.Remove(listener);

                if (record.IsEmpty)
                {
                    _ = dictionary.Remove(dependencyProperty);
                }

                if (dictionary.Count == 0)
                {
                    Remove(source);
                }
            }
        }

        protected override bool Purge(object source, object data, bool purgeAll)
        {
            bool removedAnyEntries = false;
            Dictionary<DependencyProperty, DependencyPropertyChangedWeakEventListenerRecord> dictionary = (Dictionary<DependencyProperty, DependencyPropertyChangedWeakEventListenerRecord>)data;
            List<DependencyProperty> keys = dictionary.Keys.ToList();

            foreach (DependencyProperty key in keys)
            {
                bool isEmpty = purgeAll || (source == null);


                if (!dictionary.TryGetValue(key, out DependencyPropertyChangedWeakEventListenerRecord record))
                {
                    continue;
                }

                if (!isEmpty)
                {
                    if (record.Purge())
                    {
                        removedAnyEntries = true;
                    }

                    isEmpty = record.IsEmpty;
                }

                if (!isEmpty)
                {
                    continue;
                }

                record.StopListening();

                if (!purgeAll)
                {
                    _ = dictionary.Remove(key);
                }
            }

            if (dictionary.Count == 0)
            {
                removedAnyEntries = true;

                if (source != null)
                {
                    Remove(source);
                }
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
                Type managerType = typeof(DependecyPropertyChangedEventManager);
                DependecyPropertyChangedEventManager manager = (DependecyPropertyChangedEventManager)GetCurrentManager(managerType);
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
                {
                    return;
                }

                if (!_source.IsAlive)
                {
                    return;
                }

                object source = _source.Target;
                if (source == null)
                {
                    return;
                }

                HandleEvent(source, EventArgs.Empty);
            }
            #endregion

            private readonly DependecyPropertyChangedEventManager _manager;
            private readonly WeakReference _source = new WeakReference(null);
            private readonly bool _isConstructing;
            private ListenerList _listeners;

            internal DependencyPropertyChangedWeakEventListenerRecord(DependecyPropertyChangedEventManager manager, DependencyObject source, DependencyProperty dependencyProperty)
            {
                if (dependencyProperty == null)
                {
                    throw new ArgumentNullException("dependencyProperty");
                }

                _listeners = new ListenerList();
                _manager = manager ?? throw new ArgumentNullException("manager");
                _source.Target = source ?? throw new ArgumentNullException("source");

                _isConstructing = true;

                try
                {
                    _ = BindingOperations.SetBinding(
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
                _ = ListenerList.PrepareForWriting(ref _listeners);

                _listeners.Add(listener);
            }

            private void HandleEvent(object sender, EventArgs e)
            {
                using (_manager.ReadLock)
                {
                    _ = _listeners.BeginUse();
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
                _ = ListenerList.PrepareForWriting(ref _listeners);

                if (!_source.IsAlive)
                {
                    while (_listeners.Count > 0)
                    {
                        Remove(_listeners[0]);
                    }

                    return true;
                }

                return _listeners.Purge();
            }

            internal void Remove(IWeakEventListener listener)
            {
                if (listener == null)
                {
                    return;
                }

                _ = ListenerList.PrepareForWriting(ref _listeners);

                _listeners.Remove(listener);

                if (_listeners.IsEmpty)
                {
                    StopListening();
                }
            }

            internal void StopListening()
            {
                object target = _source.Target;
                if (target == null)
                {
                    return;
                }

                _source.Target = null;

                BindingOperations.ClearBinding(this, ObservedValueProperty);
            }

            internal bool IsEmpty => _listeners.IsEmpty;
        }
    }
}