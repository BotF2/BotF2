using System.Windows;

namespace Supremacy.Client.Controls
{
    public class ObjectItemRoutedEventArgs : ItemRoutedEventArgs<object>
    {
        public ObjectItemRoutedEventArgs(object item)
            : base(item) {}

        public ObjectItemRoutedEventArgs(object item, RoutedEvent routedEvent)
            : base(item, routedEvent) {}

        public ObjectItemRoutedEventArgs(object item, RoutedEvent routedEvent, object source)
            : base(item, routedEvent, source) {}
    }
}