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
        }

        protected void InjectThemeResources()
        {
            ResourceDictionary themeResources;

            if (ThemeHelper.TryLoadThemeResources(out themeResources))
            {
                Resources.MergedDictionaries.Add(themeResources);
            }
        }
    }
}