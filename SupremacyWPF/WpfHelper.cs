using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Supremacy.Client
{
    public static class WpfHelper
    {
        public static object FindItemsControlItemFromVisualDescendant(DependencyObject descendant)
        {
            if (descendant == null)
                return null;

            var itemsControl = ItemsControl.ItemsControlFromItemContainer(descendant);
            if (itemsControl != null)
            {
                var item = itemsControl.ItemContainerGenerator.ItemFromContainer(descendant);
                if (item != null)
                    return item;
            }

            var parent = GetParent(descendant as FrameworkElement);

            if (parent == null)
                return null;

            return FindItemsControlItemFromVisualDescendant(parent);
        }

        public static DependencyObject GetParent(FrameworkElement frameworkElement)
        {
            if (frameworkElement == null)
                return null;

            return frameworkElement.Parent ??
                   frameworkElement.TemplatedParent ??
                   VisualTreeHelper.GetParent(frameworkElement) ??
                   LogicalTreeHelper.GetParent(frameworkElement);
        }
    }
}