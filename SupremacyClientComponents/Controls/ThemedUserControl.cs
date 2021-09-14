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
            if (ThemeHelper.TryLoadThemeResources(out ResourceDictionary themeResources))
            {
                Resources.MergedDictionaries.Add(themeResources);
            }
        }
    }
}