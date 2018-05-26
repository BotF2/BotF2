using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Practices.Unity.Utility;

using Supremacy.Annotations;
using Supremacy.Client.Commands;
using Supremacy.Client.DragDrop;
using Supremacy.Orbitals;
using Supremacy.Client.Context;
using Supremacy.Utility;

namespace Supremacy.Client.Views
{
    public partial class TaskForceListView
    {
        private readonly IAppContext _appContext;

        private ListBoxItem _orderMenuTargetItem;

        #region Constructors and Finalizers
        public TaskForceListView([NotNull] IAppContext appContext)
        {
            if (appContext == null)
                throw new ArgumentNullException("appContext");
            
            _appContext = appContext;

            InitializeComponent();

            TaskForceList.ItemContainerGenerator.StatusChanged += OnItemContainerGeneratorStatusChanged;
        }

        private void OnItemContainerGeneratorStatusChanged(object sender, EventArgs e)
        {
            foreach (var item in TaskForceList.Items)
            {

                var container = TaskForceList.ItemContainerGenerator.ContainerFromItem(item);
                if (container == null)
                    continue;
                // doesn't work fine        GameLog.Print("container = {0}, item = {1}", container.ToString(), item.ToString());

                //works     GameLog.Print("TaskForceList.Items.Count = {0}", TaskForceList.Items.Count);

                if (DragDropManager.GetDropTargetAdvisor(container) == null)
                    DragDropManager.SetDropTargetAdvisor(container, new TaskForceDropTargetAdvisor());
                if (DragDropManager.GetDragSourceAdvisor(container) == null)
                    DragDropManager.SetDragSourceAdvisor(container, new TaskForceDragSourceAdvisor());
            }
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            var mouseTarget = InputHitTest(Mouse.GetPosition(this)) as DependencyObject;
            var targetListViewItem = mouseTarget.FindVisualAncestorByType<ListBoxItem>();
            if (targetListViewItem == null)
            {
                e.Handled = true;
                return;
            }

            var fleetView = targetListViewItem.DataContext as FleetViewWrapper;
            if (fleetView == null)
            {
                e.Handled = true;
                return;
            }

            _orderMenuTargetItem = targetListViewItem;
            _orderMenuTargetItem.SetValue(IsOrderMenuOpenedProperty, true);

            PopulateTaskForceOrderMenu(fleetView.View);

            TaskForceList.SelectedItem = null;

            base.OnContextMenuOpening(e);
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            var mouseTarget = InputHitTest(Mouse.GetPosition(this)) as DependencyObject;
            var targetListViewItem = mouseTarget.FindVisualAncestorByType<ListBoxItem>();
            if (targetListViewItem != null)
            {
                e.Handled = true;
                return;
            }
            base.OnMouseRightButtonDown(e);
        }

        private void PopulateTaskForceOrderMenu([NotNull] FleetView fleetView)
        {
            if (fleetView == null)
                throw new ArgumentNullException("fleetView");

            var orderMenu = ContextMenu;
            if (orderMenu != null)
                orderMenu.Items.Clear();

            if (fleetView.Source.OwnerID != _appContext.LocalPlayer.EmpireID)
                return;

            if (orderMenu == null)
            {
                orderMenu = new ContextMenu();
                ContextMenu = orderMenu;
            }

            if (fleetView.Source.CanCloak)
            {
                var cloakItem = new MenuItem
                                {
                                    IsCheckable = true,
                                    Header = "Cloak",
                                    Command = GalaxyScreenCommands.ToggleTaskForceCloak,
                                    CommandParameter = fleetView
                                };

                var cloakBinding = new Binding
                                   {
                                       Source = fleetView.Source,
                                       Path = new PropertyPath("IsCloaked", new object[0]),
                                       Mode = BindingMode.OneWay
                                   };

                cloakItem.SetBinding(MenuItem.IsCheckedProperty, cloakBinding);
                orderMenu.Items.Add(cloakItem);
                orderMenu.Items.Add(new Separator());
            }

            if (fleetView.Source.CanCamouflage)
            {
                var camouflagedItem = new MenuItem
                {
                    IsCheckable = true,
                    Header = "Camouflage",
                    Command = GalaxyScreenCommands.ToggleTaskForceCamouflage,
                    CommandParameter = fleetView
                };

                var camouflageBinding = new Binding
                {
                    Source = fleetView.Source,
                    Path = new PropertyPath("IsCamouflaged", new object[0]),
                    Mode = BindingMode.OneWay
                };

                camouflagedItem.SetBinding(MenuItem.IsCheckedProperty, camouflageBinding);
                orderMenu.Items.Add(camouflagedItem);
                orderMenu.Items.Add(new Separator());
            }

            foreach (var order in FleetOrders.GetAvailableOrders(fleetView.Source))
            {
                var orderItem = new MenuItem
                                {
                                    Header = order,
                                    Command = GalaxyScreenCommands.IssueTaskForceOrder,
                                    CommandParameter = new Pair<FleetView, FleetOrder>(fleetView, order)
                                };
                orderMenu.Items.Add(orderItem);
            }

            orderMenu.Closed += OnOrderMenuClosed;
        }

        private void OnOrderMenuClosed(object sender, RoutedEventArgs args)
        {
            var sourceMenu = (ContextMenu)args.Source;
            sourceMenu.Closed -= OnOrderMenuClosed;
            if (_orderMenuTargetItem == null)
                return;
            _orderMenuTargetItem.ClearValue(IsOrderMenuOpenedProperty);
            _orderMenuTargetItem = null;
        }
        #endregion

        #region IsOrderMenuOpened (Attached Property)
        public static readonly DependencyProperty IsOrderMenuOpenedProperty =
            DependencyProperty.RegisterAttached(
                "IsOrderMenuOpened",
                typeof(bool),
                typeof(TaskForceListView),
                new FrameworkPropertyMetadata(false));

        public static bool GetIsOrderMenuOpened(DependencyObject d)
        {
            return (bool)d.GetValue(IsOrderMenuOpenedProperty);
        }

        public static void SetIsOrderMenuOpened(DependencyObject d, bool value)
        {
            d.SetValue(IsOrderMenuOpenedProperty, value);
        }
        #endregion
        
    }
}