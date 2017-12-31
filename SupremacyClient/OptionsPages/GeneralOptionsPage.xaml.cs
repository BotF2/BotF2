using System;

using Supremacy.Annotations;
using Supremacy.Resources;

namespace Supremacy.Client.OptionsPages
{
    /// <summary>
    /// Interaction logic for GeneralOptionsPage.xaml
    /// </summary>
    public partial class GeneralOptionsPage : IClientOptionsPage
    {
        private readonly IResourceManager _resourceManager;

        public GeneralOptionsPage([NotNull] IResourceManager resourceManager)
        {
            if (resourceManager == null)
                throw new ArgumentNullException("resourceManager");
            _resourceManager = resourceManager;
            InitializeComponent();
        }

        public string Header
        {
            get { return _resourceManager.GetString("SETTINGS_GENERAL_TAB"); }
        }
    }
}
