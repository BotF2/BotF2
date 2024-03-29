using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.Windows;

using Supremacy.Annotations;

namespace Supremacy.Client
{
    public sealed class GenericWeakEventManager : WeakEventManager
    {
        public static void AddListener(object source, string eventName, IWeakEventListener listener)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (eventName == null)
            {
                throw new ArgumentNullException("eventName");
            }

            if (listener == null)
            {
                throw new ArgumentNullException("listener");
            }

            CurrentManager.PrivateAddListener(
                source,
                listener,
                FindEvent(source, eventName));
        }

        public static void RemoveListener(object source, string eventName, IWeakEventListener listener)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (eventName == null)
            {
                throw new ArgumentNullException("eventName");
            }

            if (listener == null)
            {
                throw new ArgumentNullException("listener");
            }

            CurrentManager.PrivateRemoveListener(
                source,
                listener,
                FindEvent(source, eventName));
        }

        private static EventDescriptor FindEvent(object source, string eventName)
        {
            return TypeDescriptor.GetEvents(source)[eventName];
        }

        public static void AddListener(object source, EventDescriptor eventDescriptor, IWeakEventListener listener)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (eventDescriptor == null)
            {
                throw new ArgumentNullException("eventDescriptor");
            }

            if (listener == null)
            {
                throw new ArgumentNullException("listener");
            }

            CurrentManager.PrivateAddListener(
                source,
                listener,
                eventDescriptor);
        }

        public static void RemoveListener(object source, EventDescriptor eventDescriptor, IWeakEventListener listener)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (eventDescriptor == null)
            {
                throw new ArgumentNullException("eventDescriptor");
            }

            if (listener == null)
            {
                throw new ArgumentNullException("listener");
            }

            CurrentManager.PrivateRemoveListener(
                source,
                listener,
                eventDescriptor);
        }

        private void PrivateAddListener(object source, IWeakEventListener listener, EventDescriptor eventDescriptor)
        {
            using (WriteLock)
            {
                if (!(base[source] is Dictionary<EventDescriptor, WeakEventListenerRecord> dictionary))
                {
                    dictionary = new Dictionary<EventDescriptor, WeakEventListenerRecord>();
                    base[source] = dictionary;
                }


                if (!dictionary.TryGetValue(eventDescriptor, out WeakEventListenerRecord record))
                {
                    record = new WeakEventListenerRecord(this, source, eventDescriptor);
                    dictionary[eventDescriptor] = record;
                }

                record.Add(listener);

                ScheduleCleanup();
            }
        }

        private void PrivateRemoveListener(object source, IWeakEventListener listener, EventDescriptor eventDescriptor)
        {
            using (WriteLock)
            {
                if (!(base[source] is Dictionary<EventDescriptor, WeakEventListenerRecord> dictionary))
                {
                    return;
                }


                if (!dictionary.TryGetValue(eventDescriptor, out WeakEventListenerRecord record))
                {
                    return;
                }

                record.Remove(listener);

                if (record.IsEmpty)
                {
                    _ = dictionary.Remove(eventDescriptor);
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
            Dictionary<EventDescriptor, WeakEventListenerRecord> dictionary = (Dictionary<EventDescriptor, WeakEventListenerRecord>)data;
            List<EventDescriptor> keys = dictionary.Keys.ToList();

            foreach (EventDescriptor key in keys)
            {
                bool isEmpty = purgeAll || (source == null);

                if (!dictionary.TryGetValue(key, out WeakEventListenerRecord record))
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

        public static void RemoveListener(object source, IWeakEventListener listener, EventDescriptor eventDescriptor)
        {
            CurrentManager.PrivateRemoveListener(source, listener, eventDescriptor);
        }

        protected override void StartListening(object source) { }

        protected override void StopListening(object source) { }

        private static GenericWeakEventManager CurrentManager
        {
            get
            {
                Type managerType = typeof(GenericWeakEventManager);
                GenericWeakEventManager manager = (GenericWeakEventManager)GetCurrentManager(managerType);
                if (manager == null)
                {
                    manager = new GenericWeakEventManager();
                    SetCurrentManager(managerType, manager);
                }
                return manager;
            }
        }

        private class WeakEventListenerRecord
        {
            private static readonly Lazy<MethodInfo> HandlerMethod;

            private readonly GenericWeakEventManager _manager;
            private readonly EventDescriptor _eventDescriptor;
            private readonly WeakReference _source = new WeakReference(null);
            private readonly Delegate _handlerDelegate;

            private ListenerList _listeners;

            static WeakEventListenerRecord()
            {
                HandlerMethod = new Lazy<MethodInfo>(
                    () => typeof(WeakEventListenerRecord).GetMethod(
                        "HandleEvent",
                        BindingFlags.Instance | BindingFlags.NonPublic));
            }

            internal WeakEventListenerRecord(GenericWeakEventManager manager, object source, EventDescriptor eventDescriptor)
            {
                _listeners = new ListenerList();
                _manager = manager ?? throw new ArgumentNullException("manager");
                _source.Target = source ?? throw new ArgumentNullException("source");
                _eventDescriptor = eventDescriptor ?? throw new ArgumentNullException("eventDescriptor");

                Type eventType = eventDescriptor.EventType;

                _handlerDelegate = Delegate.CreateDelegate(eventType, this, HandlerMethod.Value);

                if (_handlerDelegate == null)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Could not bind weak event listener to event '{0}'.",
                            eventDescriptor.DisplayName));
                }

                _eventDescriptor.AddEventHandler(source, _handlerDelegate);
            }

            internal void Add(IWeakEventListener listener)
            {
                _ = ListenerList.PrepareForWriting(ref _listeners);
                _listeners.Add(listener);
            }

            [UsedImplicitly]
            private void HandleEvent(object sender, EventArgs e)
            {
                using (_manager.ReadLock)
                {
                    _ = _listeners.BeginUse();
                }

                try { _manager.DeliverEventToList(sender, e, _listeners); }
                finally { _listeners.EndUse(); }
            }

            internal bool Purge()
            {
                _ = ListenerList.PrepareForWriting(ref _listeners);
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
                _eventDescriptor.RemoveEventHandler(target, _handlerDelegate);
            }

            internal bool IsEmpty => _listeners.IsEmpty;
        }
    }
}