
using Supremacy.Client.Context;

namespace Supremacy.Client.Views.GalaxyScreen
{
    /// <summary>
    /// Interaction logic for ColorInfoScreen.xaml  // Screen to go an overview about used colors
    /// </summary>
    public partial class ColorInfoScreen
    {
        private readonly IAppContext _appContext;

        public ColorInfoScreen(IAppContext appContext)
        {
            InitializeComponent();

            _appContext = appContext;
        }
    }
}
