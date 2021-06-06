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
                throw new ArgumentNullException("source");
            if (eventName == null)
                throw new ArgumentNullException("eventName");
            if (listener == null)
                throw new ArgumentNullException("listener");

            CurrentManager.PrivateAddListener(
                source,
                listener,
                FindEvent(source, eventName));
        }

        public static void RemoveListener(object source, string eventName, IWeakEventListener listener)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (eventName == null)
                throw new ArgumentNullException("eventName");
            if (listener == null)
                throw new ArgumentNullException("listener");

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
                throw new ArgumentNullException("source");
            if (eventDescriptor == null)
                throw new ArgumentNullException("eventDescriptor");
            if (listener == null)
                throw new ArgumentNullException("listener");

            CurrentManager.PrivateAddListener(
                source,
                listener,
                eventDescriptor);
        }

        public static void RemoveListener(object source, EventDescriptor eventDescriptor, IWeakEventListener listener)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (eventDescriptor == null)
                throw new ArgumentNullException("eventDescriptor");
            if (listener == null)
                throw new ArgumentNullException("listener");

            CurrentManager.PrivateRemoveListener(
                source,
                listener,
                eventDescriptor);
        }

        private void PrivateAddListener(object source, IWeakEventListener listener, EventDescriptor eventDescriptor)
        {
            using (WriteLock)
            {
                var dictionary = base[source] as Dictionary<EventDescriptor, WeakEventListenerRecord>;
                if (dictionary == null)
                {
                    dictionary = new Dictionary<EventDescriptor, WeakEventListenerRecord>();
                    base[source] = dictionary;
                }

                WeakEventListenerRecord record;

                if (!dictionary.TryGetValue(eventDescriptor, out record))
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
                var dictionary = base[source] as Dictionary<EventDescriptor, WeakEventListenerRecord>;
                if (dictionary == null)
                    return;

                WeakEventListenerRecord record;

                if (!dictionary.TryGetValue(eventDescriptor, out record))
                    return;

                record.Remove(listener);

                if (record.IsEmpty)
                    dictionary.Remove(eventDescriptor);

                if (dictionary.Count == 0)
                    Remove(source);
            }
        }

        protected override bool Purge(object source, object data, bool purgeAll)
        {
            var removedAnyEntries = false;
            var dictionary = (Dictionary<EventDescriptor, WeakEventListenerRecord>)data;
            var keys = dictionary.Keys.ToList();

            foreach (var key in keys)
            {
                var isEmpty = (purgeAll || (source == null));

                WeakEventListenerRecord record;
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
                var managerType = typeof(GenericWeakEventManager);
                var manager = (GenericWeakEventManager)GetCurrentManager(managerType);
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
                if (manager == null)
                    throw new ArgumentNullException("manager");
                if (source == null)
                    throw new ArgumentNullException("source");
                if (eventDescriptor == null)
                    throw new ArgumentNullException("eventDescriptor");

                _listeners = new ListenerList();
                _manager = manager;
                _source.Target = source;
                _eventDescriptor = eventDescriptor;

                var eventType = eventDescriptor.EventType;

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
                ListenerList.PrepareForWriting(ref _listeners);
                _listeners.Add(listener);
            }

            [UsedImplicitly]
            private void HandleEvent(object sender, EventArgs e)
            {
                using (_manager.ReadLock)
                    _listeners.BeginUse();

                try { _manager.DeliverEventToList(sender, e, _listeners); }
                finally { _listeners.EndUse(); }
            }

            internal bool Purge()
            {
                ListenerList.PrepareForWriting(ref _listeners);
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
                _eventDescriptor.RemoveEventHandler(target, _handlerDelegate);
            }

            internal bool IsEmpty => _listeners.IsEmpty;
        }
    }
}