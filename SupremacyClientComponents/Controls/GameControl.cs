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
            BindingOperations.SetBinding(
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
                element.Dispatcher.BeginInvoke(
                    DispatcherPriority.Send,
                    (Action)
                    (() =>
                     {
                         if (element.IsKeyboardFocusWithin)
                             Keyboard.Focus(null);
                     }));
            }
            else
            {
                Keyboard.Focus(null);
            }
        }
    }
}