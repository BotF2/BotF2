﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using Supremacy.Client.Commands;
using Supremacy.Orbitals;

using System.Linq;

namespace Supremacy.Client.Views
{
    public partial class AssignedShipListView
    {
        #region Constructors and Finalizers
        public AssignedShipListView()
        {
            InitializeComponent();
            ShipList.MouseDoubleClick += OnShipListMouseDoubleClick;
        }
        #endregion

        #region Private Methods
        private void OnShipListMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((!(DataContext is GalaxyScreenPresentationModel presentationModel)) || (presentationModel.InputMode != GalaxyScreenInputMode.RedeployShips))
            {
                return;
            }

            if (!(e.OriginalSource is DependencyObject originalSource))
            {
                return;
            }

            ListViewItem container = originalSource.FindVisualAncestorByType<ListViewItem>();
            if (container == null)
            {
                return;
            }

            if (!(container.DataContext is ShipView selectedShip))
            {
                return;
            }

            GalaxyScreenCommands.RemoveShipFromTaskForce.Execute(
                new RedeployShipCommandArgs(selectedShip.Source));
        }

        private void OnShipListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(DataContext is GalaxyScreenPresentationModel presentationModel))
            {
                return;
            }

            presentationModel.SelectedShipsInTaskForce = ShipList.SelectedItems.OfType<ShipView>();
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            if (!(DataContext is GalaxyScreenPresentationModel presentationModel))
            {
                e.Handled = true;
                return;
            }

            FleetView selectedTaskForce = presentationModel.SelectedTaskForce.View;
            if ((selectedTaskForce == null) || !selectedTaskForce.IsOwned)
            {
                e.Handled = true;
                return;
            }

            System.Collections.Generic.IEnumerable<Ship> selectedShips = ShipList.SelectedItems.OfType<ShipView>().Select(o => o.Source);
            if (!selectedShips.Any())
            {
                e.Handled = true;
                return;
            }

            BindingOperations.ClearAllBindings(ScrapMenuItem);

            ScrapMenuItem.CommandParameter = new ScrapCommandArgs(selectedShips);

            foreach (Ship ship in selectedShips)
            {
                if (ship.OwnerID == presentationModel.AppContext.LocalPlayer.CivID)
                {
                    ScrapMenuItem.CommandParameter = new ScrapCommandArgs(selectedShips);

                    _ = ScrapMenuItem.SetBinding(
                        MenuItem.IsCheckedProperty,
                        new Binding
                        {
                            Source = ScrapMenuItem.CommandParameter,
                            Path = new PropertyPath("IsChecked"),
                            Mode = BindingMode.TwoWay,
                            FallbackValue = false
                        });
                }
            }

            base.OnContextMenuOpening(e);
        }
        #endregion
    }
}