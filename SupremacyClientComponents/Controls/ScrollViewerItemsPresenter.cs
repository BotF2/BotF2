using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Supremacy.Client.Controls
{
    public class ScrollViewerItemsPresenter : ItemsPresenter
    {
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
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
                return;

            var direction = FocusNavigationDirection.Next;
            switch (e.Key)
            {
                case Key.Down:
                    direction = FocusNavigationDirection.Down;
                    break;
                case Key.Left:
                    direction = (presenter.FlowDirection == FlowDirection.LeftToRight
                                     ? FocusNavigationDirection.Left
                                     : FocusNavigationDirection.Right);
                    break;
                case Key.Right:
                    direction = (presenter.FlowDirection == FlowDirection.LeftToRight
                                     ? FocusNavigationDirection.Right
                                     : FocusNavigationDirection.Left);
                    break;
                case Key.Up:
                    direction = FocusNavigationDirection.Up;
                    break;
            }

            if (direction == FocusNavigationDirection.Next)
                return;

            var focusedElement = Keyboard.FocusedElement as UIElement;
            if (focusedElement == null)
                return;

            var predictedFocus = focusedElement.PredictFocus(direction) as IInputElement;
            if (predictedFocus == null)
                return;

            e.Handled = true;
            predictedFocus.Focus();
            return;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            ProcessKeyDown(this, e);
            base.OnKeyDown(e);
        }
    }
}