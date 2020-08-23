using System;
using System.Windows;

using Microsoft.Practices.ServiceLocation;

using Supremacy.Client.Context;
using Supremacy.Client.Markup;
using Supremacy.Utility;

namespace Supremacy.Client.Themes
{
    public static class ThemeHelper
    {
        public static bool TryLoadThemeResources(out ResourceDictionary resources)
        {
            resources = null;

            var appContext = ServiceLocator.Current.GetInstance<IAppContext>();
            var clientApplication = ServiceLocator.Current.GetInstance<IClientApplication>();

            if (clientApplication?.IsShuttingDown != false)
                return false;

            var theme = appContext?.LocalPlayer?.Empire?.Key;
            if (theme == null)
                return false;

            var themeUri = new Uri(
                $"/SupremacyClient;Component/themes/{theme}/Theme.xaml",
                UriKind.RelativeOrAbsolute);

            try
            {
                var sharedResources = new SharedResourceDictionary();

                sharedResources.BeginInit();

                try { sharedResources.Source = themeUri; }
                finally { sharedResources.EndInit(); }

                resources = sharedResources;

                return true;
            }
            catch (Exception e)
            {
                GameLog.Client.GameData.DebugFormat("ThemeHelper.cs: problem at try sharedResources Exception {0} {1}", e.Message, e.StackTrace);
            }
            
            resources = null;
            return false;
        }
    }
}