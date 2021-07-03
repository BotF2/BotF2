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
            if (!(TargetElement is FrameworkElement targetElement))
            {
                return;
            }

            List<Ship> ships;

            ListView targetItemsControl = null;

            FrameworkElement sourceElement = ExtractElement(obj);
            if (sourceElement is ListView sourceListView)
            {
                targetItemsControl = TargetElement as ListView;
                if ((targetItemsControl != null) && (targetItemsControl == sourceListView))
                {
                    return;
                }

                ships = sourceListView.SelectedItems.OfType<Ship>().ToList();
                if (ships.Count == 0)
                {
                    ships = sourceListView.SelectedItems.OfType<ShipView>().Select(o => o.Source).ToList();
                    if (ships.Count == 0)
                    {
                        return;
                    }
                }
            }
            else
            {
                if (sourceElement.DataContext is FleetViewWrapper sourceFleetWrapper)
                {
                    ships = sourceFleetWrapper.View.Ships.Select(o => o.Source).ToList();
                }
                else
                {
                    if (!(sourceElement.DataContext is ShipView shipView))
                    {
                        return;
                    }

                    ships = new List<Ship> { shipView.Source };
                }
            }

            if (!(targetElement.DataContext is FleetViewWrapper targetFleetWrapper))
            {
                return;
            }

            if (targetItemsControl != null)
            {
                targetItemsControl.SelectedItems.Clear();
            }

            foreach (Ship ship in ships)
            {
                // works    GameLog.Print("ship.Name = {0}", ship.Name);
                GalaxyScreenCommands.AddShipToTaskForce.Execute(
                    new RedeployShipCommandArgs(
                        ship,
                        targetFleetWrapper.View.Source));
                if (targetItemsControl != null)
                {
                    _ = targetItemsControl.SelectedItems.Add(ship);
                }
            }
        }


        public UIElement TargetElement { get; set; }

        public bool ApplyMouseOffset => false;

        public UIElement GetVisualFeedback(IDataObject obj)
        {
            FrameworkElement element = ExtractElement(obj);

            UIElement visual;

            if (element is ListView listView)
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
                        _ = canvas.Children.Add(rect);
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
            {
                return;
            }

            List<Ship> ships;

            if (element is ListView listView)
            {
                ships = listView.SelectedItems.OfType<Ship>().ToList();
                //GameLog.Print("ships.Count = {0}", ships.Count);
            }
            else
            {
                if (element.DataContext is FleetViewWrapper fleetViewWrapper)
                {
                    ships = fleetViewWrapper.View.Ships.Select(o => o.Source).ToList();
                    //GameLog.Print("fleetViewWrapper - ships.Count = {0}", ships.Count);
                }
                else
                {
                    if (!(element.DataContext is ShipView shipView))
                    {
                        return;
                    }

                    ships = new List<Ship> { shipView.Source };
                    //GameLog.Print("New List - ships.Count = {0}", ships.Count);
                }
            }

            if (!ships.Any())
            {
                return;
            }

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
            if (!(draggedElement is FrameworkElement draggedFrameworkElement))
            {
                return false;
            }

            ListBox draggedListBox = draggedFrameworkElement.FindVisualAncestorByType<ListBox>();
            if (draggedListBox != null)
            {
                if (!(Mouse.DirectlyOver is UIElement sourceItem))
                {
                    return false;
                }

                ListBoxItem listBoxItem = sourceItem.FindVisualAncestorByType<ListBoxItem>();
                if (listBoxItem == null)
                {
                    return false;
                }
            }

            IAppContext appContext = ServiceLocator.Current.GetInstance<IAppContext>();
            if (appContext == null)
            {
                return false;
            }

            Game.CivilizationManager localPlayerEmpire = appContext.LocalPlayerEmpire;
            if (localPlayerEmpire == null)
            {
                return false;
            }

            if (draggedFrameworkElement.DataContext is FleetViewWrapper fleetViewWrapper)
            {
                return fleetViewWrapper.View.Source.OwnerID == localPlayerEmpire.CivilizationID;
            }

            if (draggedFrameworkElement.DataContext is ShipView shipView)
            {
                return shipView.Source.OwnerID == localPlayerEmpire.CivilizationID;
            }

            return false;
        }

        public UIElement GetTopContainer()
        {
            return Application.Current.MainWindow.Content as UIElement;
        }

        #endregion
    }
}