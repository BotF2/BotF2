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
            {
                return null;
            }

            ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(descendant);
            if (itemsControl != null)
            {
                object item = itemsControl.ItemContainerGenerator.ItemFromContainer(descendant);
                if (item != null)
                {
                    return item;
                }
            }

            DependencyObject parent = GetParent(descendant as FrameworkElement);

            return parent == null ? null : FindItemsControlItemFromVisualDescendant(parent);
        }

        public static DependencyObject GetParent(FrameworkElement frameworkElement)
        {
            return frameworkElement == null
                ? null
                : frameworkElement.Parent ??
                   frameworkElement.TemplatedParent ??
                   VisualTreeHelper.GetParent(frameworkElement) ??
                   LogicalTreeHelper.GetParent(frameworkElement);
        }
    }
}