using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Supremacy.Client.Controls
{
    public class ScrollViewerItemsPresenter : ItemsPresenter
    {
        static ScrollViewerItemsPresenter()
        {
            KeyboardNavigation.IsTabStopProperty.OverrideMetadata(
                typeof(ScrollViewerItemsPresenter),
                new FrameworkPropertyMetadata(false));

            FocusableProperty.OverrideMetadata(
                typeof(ScrollViewerItemsPresenter),
                new FrameworkPropertyMetadata(true));
        }

        internal static void ProcessKeyDown(FrameworkElement presenter, KeyEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }

            FocusNavigationDirection direction = FocusNavigationDirection.Next;
            switch (e.Key)
            {
                case Key.Down:
                    direction = FocusNavigationDirection.Down;
                    break;
                case Key.Left:
                    direction = presenter.FlowDirection == FlowDirection.LeftToRight
                                     ? FocusNavigationDirection.Left
                                     : FocusNavigationDirection.Right;
                    break;
                case Key.Right:
                    direction = presenter.FlowDirection == FlowDirection.LeftToRight
                                     ? FocusNavigationDirection.Right
                                     : FocusNavigationDirection.Left;
                    break;
                case Key.Up:
                    direction = FocusNavigationDirection.Up;
                    break;
            }

            if (direction == FocusNavigationDirection.Next)
            {
                return;
            }

            if (!(Keyboard.FocusedElement is UIElement focusedElement))
            {
                return;
            }

            if (!(focusedElement.PredictFocus(direction) is IInputElement predictedFocus))
            {
                return;
            }

            e.Handled = true;
            _ = predictedFocus.Focus();
            return;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            ProcessKeyDown(this, e);
            base.OnKeyDown(e);
        }
    }
}