// File:TracesOptionsPage.xaml.cs
using System;
using Supremacy.Annotations;
using Supremacy.Resources;

namespace Supremacy.Client.OptionsPages
{
    /// <summary>
    /// Interaction logic for TracesOptionsPage.xaml
    /// </summary>
    public partial class TracesOptionsPage : IClientOptionsPage
    {
        private readonly IResourceManager _resourceManager;

        public TracesOptionsPage([NotNull] IResourceManager resourceManager)
        {
            _resourceManager = resourceManager ?? throw new ArgumentNullException("resourceManager");
            try
            {
                InitializeComponent();
            }
            catch
            {
                _ = System.Windows.MessageBox.Show("Problem with Traces-Screen (CTRL+Z)", "WARNING", System.Windows.MessageBoxButton.OK);
            }
        }

        public string Header => _resourceManager.GetString("SETTINGS_TRACES_TAB");
    }
}
