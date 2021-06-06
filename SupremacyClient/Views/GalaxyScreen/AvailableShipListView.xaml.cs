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
            GalaxyScreenPresentationModel presentationModel = DataContext as GalaxyScreenPresentationModel;
            if ((presentationModel == null) || (presentationModel.InputMode != GalaxyScreenInputMode.RedeployShips))
                return;

            DependencyObject originalSource = e.OriginalSource as DependencyObject;
            if (originalSource == null)
                return;

            ListViewItem container = originalSource.FindVisualAncestorByType<ListViewItem>();
            if (container == null)
                return;

            ShipView selectedShip = container.DataContext as ShipView;
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
            FrameworkElement targetElement = TargetElement as FrameworkElement;
            if (targetElement == null)
                return;

            List<Ship> ships;

            ListView targetItemsControl = null;

            FrameworkElement sourceElement = ExtractElement(obj);
            ListView sourceListView = sourceElement as ListView;
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
                FleetViewWrapper sourceFleetWrapper = sourceElement.DataContext as FleetViewWrapper;
                if (sourceFleetWrapper != null)
                {
                    ships = sourceFleetWrapper.View.Ships.Select(o => o.Source).ToList();
                }
                else
                {
                    ShipView shipView = sourceElement.DataContext as ShipView;
                    if (shipView == null)
                        return;
                    ships = new List<Ship> { shipView.Source };
                }
            }

            FleetViewWrapper targetFleetWrapper = targetElement.DataContext as FleetViewWrapper;
            if (targetFleetWrapper == null)
                return;

            if (targetItemsControl != null)
                targetItemsControl.SelectedItems.Clear();

            foreach (Ship ship in ships)
            {
                // works    GameLog.Print("ship.Name = {0}", ship.Name);
                GalaxyScreenCommands.AddShipToTaskForce.Execute(
                    new RedeployShipCommandArgs(
                        ship,
                        targetFleetWrapper.View.Source));
                if (targetItemsControl != null)
                    targetItemsControl.SelectedItems.Add(ship);
            }
        }
    

        public UIElement TargetElement { get; set; }

        public bool ApplyMouseOffset => false;

        public UIElement GetVisualFeedback(IDataObject obj)
        {
            FrameworkElement element = ExtractElement(obj);
            ListView listView = element as ListView;

            UIElement visual;

            if (listView != null)
            {
                List<FrameworkElement> selectedItems = listView.Items
                    .OfType<object>()
                    .Select(o => listView.ItemContainerGenerator.ContainerFromItem(o))
                    .Where(Selector.GetIsSelected)
                    .Cast<FrameworkElement>()
                    .ToList();

                if (selectedItems.Count == 1)
                {
                    FrameworkElement selectedItem = selectedItems[0];
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
                    Canvas canvas = new Canvas
                    {
                        Width = selectedItems[0].ActualWidth + ((selectedItems.Count - 1) * 4),
                        Height = selectedItems[0].ActualHeight + ((selectedItems.Count - 1) * 4)
                    };
                    for (int i = selectedItems.Count - 1; i >= 0; i--)
                    {
                        FrameworkElement selectedItem = selectedItems[i];
                        Rectangle rect = new Rectangle
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
            FrameworkElement element = ExtractElement(obj);
            if (element == null)
                return;

            List<Ship> ships;

            ListView listView = element as ListView;
            if (listView != null)
            {
                ships = listView.SelectedItems.OfType<Ship>().ToList();
                //GameLog.Print("ships.Count = {0}", ships.Count);
            }
            else
            {
                FleetViewWrapper fleetViewWrapper = element.DataContext as FleetViewWrapper;
                if (fleetViewWrapper != null)
                {
                    ships = fleetViewWrapper.View.Ships.Select(o => o.Source).ToList();
                    //GameLog.Print("fleetViewWrapper - ships.Count = {0}", ships.Count);
                }
                else
                {
                    ShipView shipView = element.DataContext as ShipView;
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

            for (int i = 1; i < ships.Count; i++)
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

        public DragDropEffects SupportedEffects => DragDropEffects.Move;

        public UIElement SourceElement { get; set; }

        public DataObject GetDataObject(UIElement draggedElement)
        {
            DataObject data = new DataObject(SupportedFormat.Name, draggedElement);
            return data;
        }

        public void FinishDrag(UIElement draggedElement, DragDropEffects finalEffects) { }

        public bool IsDraggable(UIElement draggedElement)
        {
            FrameworkElement draggedFrameworkElement = draggedElement as FrameworkElement;
            if (draggedFrameworkElement == null)
                return false;

            ListBox draggedListBox = draggedFrameworkElement.FindVisualAncestorByType<ListBox>();
            if (draggedListBox != null)
            {
                UIElement sourceItem = Mouse.DirectlyOver as UIElement;
                if (sourceItem == null)
                    return false;
                ListBoxItem listBoxItem = sourceItem.FindVisualAncestorByType<ListBoxItem>();
                if (listBoxItem == null)
                    return false;
            }

            IAppContext appContext = ServiceLocator.Current.GetInstance<IAppContext>();
            if (appContext == null)
                return false;

            Game.CivilizationManager localPlayerEmpire = appContext.LocalPlayerEmpire;
            if (localPlayerEmpire == null)
                return false;

            FleetViewWrapper fleetViewWrapper = draggedFrameworkElement.DataContext as FleetViewWrapper;
            if (fleetViewWrapper != null)
                return (fleetViewWrapper.View.Source.OwnerID == localPlayerEmpire.CivilizationID);

            ShipView shipView = draggedFrameworkElement.DataContext as ShipView;
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