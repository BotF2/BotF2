using System.Windows;

namespace Supremacy.Client.Controls
{
    public enum ExecuteReason
    {
        Mouse,
        Keyboard,
        LostKeyboardFocus,
        Unknown
    }

    public class ExecuteRoutedEventArgs : RoutedEventArgs
    {
        public ExecuteRoutedEventArgs(ExecuteReason reason)
        {
            Reason = reason;
        }

        public ExecuteReason Reason { get; set; }
    }
}