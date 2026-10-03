using Supremacy.Client.Context;

namespace Supremacy.Client.Views.GalaxyScreen
{
    /// <summary>
    /// Interaction logic for Borg_ColorInfoScreen.xaml
    /// </summary>
    public partial class Borg_ColorInfoScreen
    {
        private readonly IAppContext _appContext;

        public Borg_ColorInfoScreen(IAppContext appContext)
        {
            InitializeComponent();

            _appContext = appContext;
        }
    }
}
