using System.Windows;

namespace Supremacy.Client
{
    public class PropertyChangedRoutedEventArgs<T> : RoutedEventArgs
    {
        private readonly T _newValue;
        private readonly T _oldValue;

        public PropertyChangedRoutedEventArgs(T oldValue, T newValue) : this(null, oldValue, newValue) { }

        public PropertyChangedRoutedEventArgs(RoutedEvent routedEvent, T oldValue, T newValue)
            : this(routedEvent, oldValue, newValue, null) { }

        public PropertyChangedRoutedEventArgs(RoutedEvent routedEvent, T oldValue, T newValue, object source)
            : base(routedEvent, source)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public T NewValue
        {
            get { return _newValue; }
        }

        public T OldValue
        {
            get { return _oldValue; }
        }
    }
}