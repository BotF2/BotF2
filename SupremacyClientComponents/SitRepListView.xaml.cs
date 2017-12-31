// SitRepListView.xaml.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.Windows.Controls;
using System.Windows.Input;

using Microsoft.Practices.ServiceLocation;
using Supremacy.Client.Context;

namespace Supremacy.Client
{
    /// <summary>
    /// Interaction logic for SitRepListView.xaml
    /// </summary>

    public partial class SitRepListView : UserControl
    {
        private readonly IAppContext _appContext;

        public SitRepListView()
        {
            _appContext = ServiceLocator.Current.GetInstance<IAppContext>();
            
            InitializeComponent();

            DataContext = _appContext.LocalPlayerEmpire.SitRepEntries;
        }

        public IAppContext AppContext
        {
            get { return _appContext; }
        }

        void SitRepListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //SitRepEntry selection = SitRepListBox.SelectedItem as SitRepEntry;
            //if (selection != null)
            //{
            //    if (selection is ResearchCompleteSitRepEntry)
            //    {
            //        GameWindow.Current.ResearchScreen.SelectApplication(
            //            ((ResearchCompleteSitRepEntry)selection).Application);
            //        GameWindow.Current.ShowResearchScreen();
            //    }
            //    else if (selection is NewColonySitRepEntry)
            //    {
            //        GameWindow.Current.SystemScreen.Colony =
            //            ((NewColonySitRepEntry)selection).Colony;
            //        GameWindow.Current.ShowSystemScreen();
            //    }
            //    else if (selection is BuildQueueEmptySitRepEntry)
            //    {
            //        GameWindow.Current.SystemScreen.Colony =
            //            ((BuildQueueEmptySitRepEntry)selection).Colony;
            //        GameWindow.Current.ShowSystemScreen();
            //    }
            //    else if (selection is ItemBuiltSitRepEntry)
            //    {
            //        ItemBuiltSitRepEntry entry = (ItemBuiltSitRepEntry)selection;
            //        Sector sector = GameContext.Current.Universe.Map[entry.Location];
            //        if ((entry.ItemType is ShipDesign) || (entry.ItemType is StationDesign))
            //        {                        
            //            GameWindow.Current.GalaxyScreen.Model.SelectedSector = sector;
            //            GalaxyScreenCommands.CenterOnSector.Execute(sector);
            //            GameWindow.Current.ShowGalaxyScreen();
            //        }
            //        else if ((sector.System != null) 
            //            && sector.System.HasColony
            //            && (sector.System.Colony.Owner == entry.Owner))
            //        {
            //            GameWindow.Current.SystemScreen.Colony = sector.System.Colony;
            //            GameWindow.Current.ShowSystemScreen();
            //        }
            //    }
            //    else if (selection is StarvationSitRepEntry)
            //    {
            //        StarvationSitRepEntry entry = (StarvationSitRepEntry)selection;
            //        if ((entry.System != null)
            //            && entry.System.HasColony
            //            && (entry.System.Colony.Owner == entry.Owner))
            //        {
            //            GameWindow.Current.SystemScreen.Colony = entry.System.Colony;
            //            GameWindow.Current.ShowSystemScreen();
            //        }
            //    }
            //}
        }
    }
}