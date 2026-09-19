
using Supremacy.Client.Context;

namespace Supremacy.Client.Views.GalaxyScreen
{
    /// <summary>
    /// Interaction logic for ColonyInfoScreen.xaml
    /// </summary>
    public partial class ColonyInfoScreen
    {
        private readonly IAppContext _appContext;

        public ColonyInfoScreen(IAppContext appContext)
        {
            InitializeComponent();

            _appContext = appContext;
        }
    }
}
