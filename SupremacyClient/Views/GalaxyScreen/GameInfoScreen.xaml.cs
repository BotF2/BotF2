using Supremacy.Client.Context;

namespace Supremacy.Client.Views.GalaxyScreen
{
    /// <summary>
    /// Interaction logic for GameInfoScreen.xaml
    /// </summary>
    public partial class GameInfoScreen
    {
        private readonly IAppContext _appContext;

        public GameInfoScreen(IAppContext appContext)
        {
            InitializeComponent();

            _appContext = appContext;
        }
    }
}
