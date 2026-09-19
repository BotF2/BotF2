using Supremacy.Client.Context;
using Supremacy.Client.Services;
using Supremacy.Economy;
using Supremacy.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Supremacy.Client.Views.GalaxyScreen
{
    /// <summary>
    /// Interaction logic for CheatMenu.xaml
    /// </summary>
    public partial class ColorInfoScreen
    {
        private readonly IAppContext _appContext;

        public ColorInfoScreen(IAppContext appContext)
        {
            InitializeComponent();

            _appContext = appContext;
        }

        //private void OnGrantCreditsButtonClicked(object sender, RoutedEventArgs e)
        //{
        //    int amount = 0;
        //    if (int.TryParse(creditsAmount.Text, out amount))
        //    {
        //        _appContext.LocalPlayerEmpire.Credits.AdjustCurrent(amount);
        //        PlayerOrderService.Instance.AddOrder(new GiveCreditsOrder(_appContext.LocalPlayerEmpire, amount));
        //    }
        //}

        //private void OnGrantDilithiumButtonClicked(object sender, RoutedEventArgs e)
        //{
        //    int amount = 0;
        //    if (int.TryParse(dilithiumAmount.Text, out amount))
        //    {
        //        _appContext.LocalPlayerEmpire.Resources[ResourceType.Dilithium].AdjustCurrent(amount);
        //        PlayerOrderService.Instance.AddOrder(new GiveResourceOrder(_appContext.LocalPlayerEmpire, ResourceType.Dilithium, amount));
        //    }
        //}

        //private void OnGrantDeuteriumButtonClicked(object sender, RoutedEventArgs e)
        //{
        //    int amount = 0;
        //    if (int.TryParse(deuteriumAmount.Text, out amount))
        //    {
        //        _appContext.LocalPlayerEmpire.Resources[ResourceType.Deuterium].AdjustCurrent(amount);
        //        PlayerOrderService.Instance.AddOrder(new GiveResourceOrder(_appContext.LocalPlayerEmpire, ResourceType.Deuterium, amount));
        //    }
        //}

        //private void OnGrantDuraniumButtonClicked(object sender, RoutedEventArgs e)
        //{
        //    int amount = 0;
        //    if (int.TryParse(duraniumAmount.Text, out amount))
        //    {
        //        _appContext.LocalPlayerEmpire.Resources[ResourceType.RawMaterials].AdjustCurrent(amount);
        //        PlayerOrderService.Instance.AddOrder(new GiveResourceOrder(_appContext.LocalPlayerEmpire, ResourceType.RawMaterials, amount));
        //    }
        //}
    }
}
