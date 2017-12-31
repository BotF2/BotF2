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
            //GameLog.Client.GameData.DebugFormat("ThemeHelper.cs: theme=EmpireKey={0}", theme);
            if (theme == null)
                return false;

            var themeUri = new Uri(
                $"/SupremacyClient;Component/themes/{theme}/Theme.xaml",
                UriKind.RelativeOrAbsolute);

            // maybe causes a crash -GameLog.Client.GameData.DebugFormat("ThemeHelper.cs: themeUri={0}", themeUri.Scheme.ToString());

            try
            {
                var sharedResources = new SharedResourceDictionary();

                sharedResources.BeginInit();

                try { sharedResources.Source = themeUri; }
                finally { sharedResources.EndInit(); }

                resources = sharedResources;
                // maybe causes a crash - GameLog.Client.GameData.DebugFormat("ThemeHelper.cs: sharedResources={0}", sharedResources.Source.ToString());

                // working, but not fine   GameLog.Client.GameData.DebugFormat("ThemeHelper.cs: sharedResources is working for theme=EmpireKey={0}", theme);

                return true;
            }
            catch
            {
                GameLog.Client.GameData.DebugFormat("ThemeHelper.cs: problem at try sharedResources");
            }
            
            resources = null;
            return false;
        }
    }
}