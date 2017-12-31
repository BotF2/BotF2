using System.Windows;
using System.Windows.Controls;

using Supremacy.Client.Themes;

namespace Supremacy.Client.Controls
{
    public class ThemedUserControl : UserControl
    {
        public ThemedUserControl()
        {
            InjectThemeResources();
            //working   GameLog.Client.GameData.DebugFormat("ThemedUserControl.cs: InjectThemeResources");
        }

        protected void InjectThemeResources()
        {
            ResourceDictionary themeResources;

            if (ThemeHelper.TryLoadThemeResources(out themeResources))
            {
                Resources.MergedDictionaries.Add(themeResources);
                // not working fine  GameLog.Client.GameData.DebugFormat("ThemedUserControl.cs: themeResources={0}", themeResources);
            }
        }
    }
}