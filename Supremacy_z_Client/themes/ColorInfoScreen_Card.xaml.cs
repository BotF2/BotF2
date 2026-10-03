using Supremacy.Client.Context;

namespace Supremacy.Client.Views.GalaxyScreen
{
    /// <summary>
    /// Interaction logic for Card_ColorInfoScreen.xaml
    /// </summary>
    public partial class Card_ColorInfoScreen
    {
        private readonly IAppContext _appContext;

        public Card_ColorInfoScreen(IAppContext appContext)
        {
            InitializeComponent();

            _appContext = appContext;
        }
    }
}
