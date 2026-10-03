using Supremacy.Client.Context;

namespace Supremacy.Client.Views.GalaxyScreen
{
    /// <summary>
    /// Interaction logic for Kling_ColorInfoScreen.xaml
    /// </summary>
    public partial class Fed_ColorInfoScreen
    {
        private readonly IAppContext _appContext;

        public Fed_ColorInfoScreen(IAppContext appContext)
        {
            InitializeComponent();

            _appContext = appContext;
        }
    }
}
