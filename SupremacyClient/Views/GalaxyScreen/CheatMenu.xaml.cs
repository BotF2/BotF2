using Supremacy.Client.Context;
using Supremacy.Client.Services;
using Supremacy.Economy;
using Supremacy.Game;
using System.Windows;

namespace Supremacy.Client.Views.GalaxyScreen
{
    /// <summary>
    /// Interaction logic for CheatMenu.xaml
    /// </summary>
    public partial class CheatMenu
    {
        private readonly IAppContext _appContext;

        public CheatMenu(IAppContext appContext)
        {
            InitializeComponent();

            _appContext = appContext;
        }

        private string CheatText
            {
            get { return "hello";}
            }

        private void OnGrantCreditsButtonClicked(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(creditsAmount.Text, out int amount))
            {
                _appContext.LocalPlayerEmpire.Credits.AdjustCurrent(amount);
                PlayerOrderService.Instance.AddOrder(new GiveCreditsOrder(_appContext.LocalPlayerEmpire, amount));
            }
        }

        private void OnGrantDilithiumButtonClicked(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(dilithiumAmount.Text, out int amount))
            {
                _appContext.LocalPlayerEmpire.Resources[ResourceType.Dilithium].AdjustCurrent(amount);
                PlayerOrderService.Instance.AddOrder(new GiveResourceOrder(_appContext.LocalPlayerEmpire, ResourceType.Dilithium, amount));
            }
        }

        private void OnGrantDeuteriumButtonClicked(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(deuteriumAmount.Text, out int amount))
            {
                _appContext.LocalPlayerEmpire.Resources[ResourceType.Deuterium].AdjustCurrent(amount);
                PlayerOrderService.Instance.AddOrder(new GiveResourceOrder(_appContext.LocalPlayerEmpire, ResourceType.Deuterium, amount));
            }
        }

        private void OnGrantDuraniumButtonClicked(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(duraniumAmount.Text, out int amount))
            {
                _appContext.LocalPlayerEmpire.Resources[ResourceType.RawMaterials].AdjustCurrent(amount);
                PlayerOrderService.Instance.AddOrder(new GiveResourceOrder(_appContext.LocalPlayerEmpire, ResourceType.RawMaterials, amount));
            }
        }
    }
}
