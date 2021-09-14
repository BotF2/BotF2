using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Supremacy.Utility
{
    public static class EventExtensions
    {
        public static void Raise(this EventHandler handler, object sender)
        {
            handler?.Invoke(sender, EventArgs.Empty);
        }

        public static void Raise(this EventHandler handler, object sender, EventArgs e)
        {
            handler?.Invoke(sender, e);
        }

        public static void Raise(this EventHandler<EventArgs> handler, object sender)
        {
            handler?.Invoke(sender, EventArgs.Empty);
        }

        public static void Raise<TEventArgs>(this EventHandler<TEventArgs> handler, object sender, TEventArgs e)
            where TEventArgs : EventArgs
        {
            handler?.Invoke(sender, e);
        }

        public static void Raise(this PropertyChangedEventHandler handler, object sender, string propertyName)
        {
            handler?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
        }

        public static void Raise(this PropertyChangedEventHandler handler, object sender, PropertyChangedEventArgs e)
        {
            handler?.Invoke(sender, e);
        }

        public static void Raise(this PropertyChangingEventHandler handler, object sender, string propertyName)
        {
            handler?.Invoke(sender, new PropertyChangingEventArgs(propertyName));
        }

        public static void Raise(this PropertyChangingEventHandler handler, object sender, PropertyChangingEventArgs e)
        {
            handler?.Invoke(sender, e);
        }

        #region NotifyCollectionChangedEventHandler Extensions
        public static void RaiseReset(this NotifyCollectionChangedEventHandler handler, object sender)
        {
            handler?.Invoke(sender, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public static void Raise(
            this NotifyCollectionChangedEventHandler handler,
            object sender,
            NotifyCollectionChangedEventArgs e)
        {
            handler?.Invoke(sender, e);
        }

        public static void Raise(
            this NotifyCollectionChangedEventHandler handler,
            object sender,
            NotifyCollectionChangedAction action,
            object item,
            int index)
        {
            Raise(
                handler,
                sender,
                new NotifyCollectionChangedEventArgs(
                    action,
                    item,
                    index));
        }

        public static void Raise(
            this NotifyCollectionChangedEventHandler handler,
            object sender,
            NotifyCollectionChangedAction action,
            IList items,
            int startingIndex)
        {
            Raise(
                handler,
                sender,
                new NotifyCollectionChangedEventArgs(
                    action,
                    items,
                    startingIndex));
        }

        public static void RaiseMove(
            this NotifyCollectionChangedEventHandler handler,
            object sender,
            IList changedItems,
            int index,
            int oldIndex)
        {
            Raise(
                handler,
                sender,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Move,
                    changedItems,
                    index,
                    oldIndex));
        }

        public static void RaiseMove(
            this NotifyCollectionChangedEventHandler handler,
            object sender,
            object changedItem,
            int index,
            int oldIndex)
        {
            Raise(
                handler,
                sender,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Move,
                    changedItem,
                    index,
                    oldIndex));
        }

        public static void RaiseReplace(
            this NotifyCollectionChangedEventHandler handler,
            object sender,
            object oldItem,
            object newItem,
            int index)
        {
            Raise(
                handler,
                sender,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace,
                    newItem,
                    oldItem,
                    index));
        }

        public static void RaiseReplace(
            this NotifyCollectionChangedEventHandler handler,
            object sender,
            IList oldItems,
            IList newItems,
            int startingIndex)
        {
            Raise(
                handler,
                sender,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace,
                    newItems,
                    oldItems,
                    startingIndex));
        }

        public static void RaiseAdd(
            this NotifyCollectionChangedEventHandler handler,
            object sender,
            object newItem)
        {
            Raise(
                handler,
                sender,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    newItem));
        }

        public static void RaiseRemove(
            this NotifyCollectionChangedEventHandler handler,
            object sender,
            object oldItem)
        {
            Raise(
                handler,
                sender,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    oldItem));
        }

        public static void RaiseAdd(
            this NotifyCollectionChangedEventHandler handler,
            object sender,
            IList newItems)
        {
            Raise(
                handler,
                sender,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    newItems));
        }

        public static void RaiseRemove(
            this NotifyCollectionChangedEventHandler handler,
            object sender,
            IList oldItems)
        {
            Raise(
                handler,
                sender,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    oldItems));
        }
        #endregion
    }
}