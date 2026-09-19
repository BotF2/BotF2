using Supremacy.Client.Context;

namespace Supremacy.Client.Views.GalaxyScreen
{
    /// <summary>
    /// Interaction logic for Kling_ColorInfoScreen.xaml
    /// </summary>
    public partial class Ter_ColorInfoScreen
    {
        private readonly IAppContext _appContext;

        public Ter_ColorInfoScreen(IAppContext appContext)
        {
            InitializeComponent();

            _appContext = appContext;
        }
    }
}
