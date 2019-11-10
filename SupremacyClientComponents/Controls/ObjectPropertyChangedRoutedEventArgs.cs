using System.Windows;

namespace Supremacy.Client.Controls
{
    public class ObjectPropertyChangedRoutedEventArgs : PropertyChangedRoutedEventArgs<object>
    {
        public ObjectPropertyChangedRoutedEventArgs(object oldValue, object newValue) : base(oldValue, newValue) { }

        public ObjectPropertyChangedRoutedEventArgs(RoutedEvent routedEvent, object oldValue, object newValue)
            : base(routedEvent, oldValue, newValue) { }
    }
}