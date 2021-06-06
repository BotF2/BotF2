using System;

using Supremacy.Annotations;
using Supremacy.Resources;

namespace Supremacy.Client.OptionsPages
{
    /// <summary>
    /// Interaction logic for GraphicsOptionsPage.xaml
    /// </summary>
    public partial class GraphicsOptionsPage : IClientOptionsPage
    {
        private readonly IResourceManager _resourceManager;

        public GraphicsOptionsPage([NotNull] IResourceManager resourceManager)
        {
            if (resourceManager == null)
                throw new ArgumentNullException("resourceManager");
            _resourceManager = resourceManager;
            InitializeComponent();
        }

        public string Header => _resourceManager.GetString("SETTINGS_GRAPHICS_TAB");
    }
}
