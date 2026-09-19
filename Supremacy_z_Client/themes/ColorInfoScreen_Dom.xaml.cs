using Supremacy.Client.Context;

namespace Supremacy.Client.Views.GalaxyScreen
{
    /// <summary>
    /// Interaction logic for CheatMenu.xaml
    /// </summary>
    public partial class Dom_ColorInfoScreen
    {
        private readonly IAppContext _appContext;

        public Dom_ColorInfoScreen(IAppContext appContext)
        {
            InitializeComponent();

            _appContext = appContext;
        }
    }
}
