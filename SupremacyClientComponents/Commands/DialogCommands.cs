using System.Windows.Input;

namespace Supremacy.Client.Commands
{
    public static class DialogCommands
    {
        public static readonly RoutedCommand AcceptCommand = new RoutedCommand("Accept", typeof(DialogCommands));
        public static readonly RoutedCommand CancelCommand = new RoutedCommand("Cancel", typeof(DialogCommands));
    }
}