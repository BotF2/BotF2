using System;
using System.Collections.Generic;
using System.Windows;

namespace Supremacy.Client.Controls
{
    internal class EventNotifier
    {
        private readonly Dictionary<Guid, InfoCardHost> _popupHosts;

        internal EventNotifier()
        {
            _popupHosts = new Dictionary<Guid, InfoCardHost>();
        }

        internal void RaiseEvents()
        {
            foreach (InfoCardHost container in _popupHosts.Values)
                container.RaiseEvent(new RoutedEventArgs(InfoCardHost.LayoutChangedEvent, container));
        }

        internal void Subscribe(DependencyObject o)
        {
            if (o == null)
                return;

            InfoCardHost infoCardHost = InfoCardHost.GetInfoCardHost(o);
            if (infoCardHost != null)
                _popupHosts[infoCardHost.UniqueId] = infoCardHost;
        }
    }
}