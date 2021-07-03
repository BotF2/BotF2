using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace Supremacy.Client.Controls
{
    public static class GameControl
    {
        internal static void BindToValue(DependencyObject target, DependencyProperty property, object value)
        {
            _ = BindingOperations.SetBinding(
                target,
                property,
                new Binding
                {
                    BindsDirectlyToSource = true,
                    Source = value
                });
        }

        internal static void BlurFocus(UIElement element, bool invoke)
        {
            if (invoke)
            {
                _ = element.Dispatcher.BeginInvoke(
                    DispatcherPriority.Send,
                    (Action)
                    (() =>
                     {
                         if (element.IsKeyboardFocusWithin)
                         {
                             _ = Keyboard.Focus(null);
                         }
                     }));
            }
            else
            {
                _ = Keyboard.Focus(null);
            }
        }
    }
}