using System;
using Supremacy.Annotations;
using Supremacy.Resources;

namespace Supremacy.Client.OptionsPages
{
    /// <summary>
    /// Interaction logic for AllOptionsPage.xaml
    /// </summary>
    public partial class AllOptionsPage : IClientOptionsPage
    {
        private readonly IResourceManager _resourceManager;

        public AllOptionsPage([NotNull] IResourceManager resourceManager)
        {
            _resourceManager = resourceManager ?? throw new ArgumentNullException("resourceManager");
            InitializeComponent();
        }

        public string Header => _resourceManager.GetString("SETTINGS_ALL_TAB");
    }
}
