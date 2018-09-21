using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using Supremacy.Client.Commands;
using Supremacy.Orbitals;

using System.Linq;
using Supremacy.Utility;

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
            var presentationModel = DataContext as GalaxyScreenPresentationModel;
            if ((presentationModel == null) || (presentationModel.InputMode != GalaxyScreenInputMode.RedeployShips))
                return;

            var originalSource = e.OriginalSource as DependencyObject;
            if (originalSource == null)
                return;

            var container = originalSource.FindVisualAncestorByType<ListViewItem>();
            if (container == null)
                return;

            var selectedShip = container.DataContext as ShipView;
            if (selectedShip == null)
                return;

            GalaxyScreenCommands.RemoveShipFromTaskForce.Execute(
                new RedeployShipCommandArgs(selectedShip.Source));
        }

        private void OnShipListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var presentationModel = DataContext as GalaxyScreenPresentationModel;
            if (presentationModel == null)
                return;

            presentationModel.SelectedShipsInTaskForce = ShipList.SelectedItems.OfType<ShipView>();
            GameLog.Client.General.DebugFormat("presentationModel.SelectedTaskForce.Name = {0}", presentationModel.SelectedTaskForce.Name);
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            var presentationModel = DataContext as GalaxyScreenPresentationModel;
            if (presentationModel == null)
            {
                e.Handled = true;
                return;
            }

            var selectedTaskForce = presentationModel.SelectedTaskForce.View;
            if ((selectedTaskForce == null) || !selectedTaskForce.IsOwned)
            {
                e.Handled = true;
                return;
            }

            var selectedShips = ShipList.SelectedItems.OfType<ShipView>().Select(o => o.Source);
            if (!selectedShips.Any())
            {
                e.Handled = true;
                return;
            }

            BindingOperations.ClearAllBindings(ScrapMenuItem);

            ScrapMenuItem.CommandParameter = new ScrapCommandArgs(selectedShips);

            ScrapMenuItem.SetBinding(
                MenuItem.IsCheckedProperty,
                new Binding
                {
                    Source = ScrapMenuItem.CommandParameter,
                    Path = new PropertyPath("IsChecked"),
                    Mode = BindingMode.TwoWay,
                    FallbackValue = false
                });

            base.OnContextMenuOpening(e);
        }
        #endregion
    }
}