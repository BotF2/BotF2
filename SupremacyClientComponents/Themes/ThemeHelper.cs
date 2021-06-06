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

            IAppContext appContext = ServiceLocator.Current.GetInstance<IAppContext>();
            IClientApplication clientApplication = ServiceLocator.Current.GetInstance<IClientApplication>();

            if (clientApplication?.IsShuttingDown != false)
                return false;

            string theme = appContext?.LocalPlayer?.Empire?.Key;
            if (theme == null)
                return false;

            Uri themeUri = new Uri(
                $"/SupremacyClient;Component/themes/{theme}/Theme.xaml",
                UriKind.RelativeOrAbsolute);

            string _text = "including" + themeUri.ToString();
            GameLog.Client.UIDetails.DebugFormat(_text);
            Console.WriteLine(_text);

            try
            {
                SharedResourceDictionary sharedResources = new SharedResourceDictionary();

                sharedResources.BeginInit();

                try { sharedResources.Source = themeUri; }
                finally { sharedResources.EndInit(); }

                resources = sharedResources;

                return true;
            }
            catch (Exception e)
            {
                GameLog.Client.GameData.ErrorFormat("ThemeHelper.cs: problem at try sharedResources Exception {0} {1}", e.Message, e.StackTrace);
            }

            resources = null;
            return false;
        }
    }
}