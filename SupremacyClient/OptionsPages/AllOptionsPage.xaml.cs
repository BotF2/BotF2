using System;
using System.Windows.Input;
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
            if (resourceManager == null)
                throw new ArgumentNullException("resourceManager");
            _resourceManager = resourceManager;
            InitializeComponent();
        }

        public string Header
        {
            get { return _resourceManager.GetString("SETTINGS_ALL_TAB"); }
        }
    }
}
