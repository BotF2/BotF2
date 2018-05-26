using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Practices.ServiceLocation;

using Supremacy.Client.Commands;
using Supremacy.Client.DragDrop;
using Supremacy.Orbitals;


using System.Linq;
using Supremacy.Client.Context;
using Supremacy.Utility;

namespace Supremacy.Client.Views
{
    public partial class AvailableShipListView
    {
        #region Constructors and Finalizers
        public AvailableShipListView()
        {
            InitializeComponent();
        }
        #endregion

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

            GalaxyScreenCommands.AddShipToTaskForce.Execute(selectedShip.Source);
        }
    }

    public class TaskForceDropTargetAdvisor : IDropTargetAdvisor
    {
        private static readonly DataFormat SupportedFormat = DataFormats.GetDataFormat("TaskForceUIContainer");

        #region IDropTargetAdvisor Members

        public bool IsValidDataObject(IDataObject obj)
        {
            return obj.GetDataPresent(SupportedFormat.Name);
        }

        public virtual void OnDropCompleted(IDataObject obj, Point dropPoint)
        {
            var targetElement = TargetElement as FrameworkElement;
            if (targetElement == null)
                return;

            List<Ship> ships;

            ListView targetItemsControl = null;

            var sourceElement = ExtractElement(obj);
            var sourceListView = sourceElement as ListView;
            if (sourceListView != null)
            {
                targetItemsControl = TargetElement as ListView;
                if ((targetItemsControl != null) && (targetItemsControl == sourceListView))
                    return;

                ships = sourceListView.SelectedItems.OfType<Ship>().ToList();
                if (ships.Count == 0)
                {
                    ships = sourceListView.SelectedItems.OfType<ShipView>().Select(o => o.Source).ToList();
                    if (ships.Count == 0)
                        return;
                }
            }
            else
            {
                var sourceFleetWrapper = sourceElement.DataContext as FleetViewWrapper;
                if (sourceFleetWrapper != null)
                {
                    ships = sourceFleetWrapper.View.Ships.Select(o => o.Source).ToList();
                }
                else
                {
                    var shipView = sourceElement.DataContext as ShipView;
                    if (shipView == null)
                        return;
                    ships = new List<Ship> { shipView.Source };
                }
            }

            var targetFleetWrapper = targetElement.DataContext as FleetViewWrapper;
            if (targetFleetWrapper == null)
                return;

            if (targetItemsControl != null)
                targetItemsControl.SelectedItems.Clear();

            foreach (var ship in ships)
            {
                GameLog.Print("ship.Name = {0}", ship.Name);
                GalaxyScreenCommands.AddShipToTaskForce.Execute(
                    new RedeployShipCommandArgs(
                        ship,
                        targetFleetWrapper.View.Source));
                if (targetItemsControl != null)
                    targetItemsControl.SelectedItems.Add(ship);
            }
        }
    

        public UIElement TargetElement { get; set; }

        public bool ApplyMouseOffset
        {
            get { return false; }
        }

        public UIElement GetVisualFeedback(IDataObject obj)
        {
            var element = ExtractElement(obj);
            var listView = element as ListView;

            UIElement visual;

            if (listView != null)
            {
                var selectedItems = listView.Items
                    .OfType<object>()
                    .Select(o => listView.ItemContainerGenerator.ContainerFromItem(o))
                    .Where(Selector.GetIsSelected)
                    .Cast<FrameworkElement>()
                    .ToList();

                if (selectedItems.Count == 1)
                {
                    var selectedItem = selectedItems[0];
                    visual = new Rectangle
                    {
                        Height = selectedItem.ActualHeight,
                        Width = selectedItem.ActualWidth,
                        Opacity = 0.85,
                        IsHitTestVisible = false,
                        Fill = new VisualBrush(selectedItem)
                        {
                            AutoLayoutContent = false,
                            Stretch = Stretch.None,
                            AlignmentX = AlignmentX.Left,
                            AlignmentY = AlignmentY.Top
                        }
                    };
                }
                else
                {
                    var canvas = new Canvas
                    {
                        Width = selectedItems[0].ActualWidth + ((selectedItems.Count - 1) * 4),
                        Height = selectedItems[0].ActualHeight + ((selectedItems.Count - 1) * 4)
                    };
                    for (int i = selectedItems.Count - 1; i >= 0; i--)
                    {
                        var selectedItem = selectedItems[i];
                        var rect = new Rectangle
                        {
                            Height = selectedItem.ActualHeight,
                            Width = selectedItem.ActualWidth,
                            Opacity = 0.85,
                            IsHitTestVisible = false,
                            Fill = new VisualBrush(selectedItem)
                            {
                                AutoLayoutContent = false,
                                Stretch = Stretch.None,
                                AlignmentX = AlignmentX.Left,
                                AlignmentY = AlignmentY.Top
                            }
                        };
                        Canvas.SetLeft(rect, i * 4);
                        Canvas.SetTop(rect, i * 4);
                        canvas.Children.Add(rect);
                    }
                    visual = canvas;
                }
            }
            else
            {
                visual = new Rectangle
                {
                    Height = element.ActualHeight,
                    Width = element.ActualWidth,
                    Opacity = 0.85,
                    IsHitTestVisible = false,
                    Fill = new VisualBrush(element)
                    {
                        AutoLayoutContent = false,
                        Stretch = Stretch.None,
                        AlignmentX = AlignmentX.Left,
                        AlignmentY = AlignmentY.Top
                    }
                };
            }

            return visual;
        }

        public UIElement GetTopContainer()
        {
            return TargetElement;
        }

        #endregion

        protected static FrameworkElement ExtractElement(IDataObject obj)
        {
            return obj.GetData(SupportedFormat.Name) as FrameworkElement;
        }
    }

    public class NewTaskForceDropTargetAdvisor : TaskForceDropTargetAdvisor
    {
        public override void OnDropCompleted(IDataObject obj, Point dropPoint)
        {
            var element = ExtractElement(obj);
            if (element == null)
                return;

            List<Ship> ships;

            var listView = element as ListView;
            if (listView != null)
            {
                ships = listView.SelectedItems.OfType<Ship>().ToList();
                //GameLog.Print("ships.Count = {0}", ships.Count);
            }
            else
            {
                var fleetViewWrapper = element.DataContext as FleetViewWrapper;
                if (fleetViewWrapper != null)
                {
                    ships = fleetViewWrapper.View.Ships.Select(o => o.Source).ToList();
                    //GameLog.Print("fleetViewWrapper - ships.Count = {0}", ships.Count);
                }
                else
                {
                    var shipView = element.DataContext as ShipView;
                    if (shipView == null)
                        return;
                    ships = new List<Ship> { shipView.Source };
                    //GameLog.Print("New List - ships.Count = {0}", ships.Count);
                }
            }

            if (!ships.Any())
                return;

            GalaxyScreenCommands.RemoveShipFromTaskForce.Execute(
                new RedeployShipCommandArgs(ships[0]));

            for (var i = 1; i < ships.Count; i++)
            {
                //GameLog.Print("ships[i] = {0}, ships[0].Fleet = {1}", ships[i].Name, ships[0].Fleet.Name);
                GalaxyScreenCommands.AddShipToTaskForce.Execute(
                    new RedeployShipCommandArgs(
                        ships[i],
                        ships[0].Fleet));
            }
        }
    }


    public class TaskForceDragSourceAdvisor : IDragSourceAdvisor
    {
        private static readonly DataFormat SupportedFormat = DataFormats.GetDataFormat("TaskForceUIContainer");

        #region IDragSourceAdvisor Members

        public DragDropEffects SupportedEffects
        {
            get { return DragDropEffects.Move; }
        }

        public UIElement SourceElement { get; set; }

        public DataObject GetDataObject(UIElement draggedElement)
        {
            var data = new DataObject(SupportedFormat.Name, draggedElement);
            return data;
        }

        public void FinishDrag(UIElement draggedElement, DragDropEffects finalEffects) { }

        public bool IsDraggable(UIElement draggedElement)
        {
            var draggedFrameworkElement = draggedElement as FrameworkElement;
            if (draggedFrameworkElement == null)
                return false;

            var draggedListBox = draggedFrameworkElement.FindVisualAncestorByType<ListBox>();
            if (draggedListBox != null)
            {
                var sourceItem = Mouse.DirectlyOver as UIElement;
                if (sourceItem == null)
                    return false;
                var listBoxItem = sourceItem.FindVisualAncestorByType<ListBoxItem>();
                if (listBoxItem == null)
                    return false;
            }

            var appContext = ServiceLocator.Current.GetInstance<IAppContext>();
            if (appContext == null)
                return false;

            var localPlayerEmpire = appContext.LocalPlayerEmpire;
            if (localPlayerEmpire == null)
                return false;

            var fleetViewWrapper = draggedFrameworkElement.DataContext as FleetViewWrapper;
            if (fleetViewWrapper != null)
                return (fleetViewWrapper.View.Source.OwnerID == localPlayerEmpire.CivilizationID);

            var shipView = draggedFrameworkElement.DataContext as ShipView;
            if (shipView != null)
                return (shipView.Source.OwnerID == localPlayerEmpire.CivilizationID);

            return false;
        }

        public UIElement GetTopContainer()
        {
            return Application.Current.MainWindow.Content as UIElement;
        }

        #endregion
    }
}