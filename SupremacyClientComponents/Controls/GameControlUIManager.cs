using System.Collections.Generic;
using System.Windows.Input;

namespace Supremacy.Client.Controls
{
    public static class GameCommandUIManager
    {
        private static Dictionary<ICommand, IGameCommandUIProvider> _linkData;

        internal static IGameCommandUIProvider GetUIProviderResolved(ICommand command)
        {
            if (command == null)
                return null;

            var uiProvider = command as IGameCommandUIProvider;
            if (uiProvider != null)
                return uiProvider;

            if (LinkData.TryGetValue(command, out uiProvider))
                return uiProvider;

            return null;
        }

        private static Dictionary<ICommand, IGameCommandUIProvider> LinkData
        {
            get
            {
                if (_linkData == null)
                    _linkData = new Dictionary<ICommand, IGameCommandUIProvider>();
                return _linkData;
            }
        }

        public static IGameCommandUIProvider GetUIProvider(ICommand command)
        {
            IGameCommandUIProvider uiProvider;
            if ((command != null) && (LinkData.TryGetValue(command, out uiProvider)))
                return uiProvider;
            return null;
        }

        public static void Register(ICommand command, IGameCommandUIProvider uiProvider)
        {
            var routedCommand = command as RoutedUICommand;
            var provider = uiProvider as GameCommandUIProvider;
            
            if ((uiProvider.Label == null) && (routedCommand != null) && (provider != null))
                provider.Label = routedCommand.Text;

            LinkData[command] = uiProvider;
        }

        public static void Unregister(ICommand command)
        {
            LinkData.Remove(command);
        }

        public static void UnregisterAll()
        {
            LinkData.Clear();
        }
    }
}