using System;
using System.Windows;
using System.Windows.Controls;

namespace Supremacy.Client.Controls
{
    public sealed class EnumValueItemTemplateSelector : DataTemplateSelector
    {
        public static readonly EnumValueItemTemplateSelector Instance = new EnumValueItemTemplateSelector();

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var frameworkElement = container as FrameworkElement;
            if (frameworkElement != null && item is Enum)
                return frameworkElement.TryFindResource(item) as DataTemplate;
            return null;
        }
    }
}