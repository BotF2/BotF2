using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using ActiproSoftware.Windows.Media.Animation;
using Supremacy.Annotations;
using Supremacy.Client.Controls;
using Supremacy.Client.DragDrop;
using Supremacy.Combat;
using Supremacy.Orbitals;
using Supremacy.Utility;
using Supremacy.Client.Context;

namespace Supremacy.Client.Views
{
    public enum AssaultGroup
    {
        Invalid = -1,
        Standby,
        Attack,
        LandTroops,
        Destroyed
    }

    public partial class SystemAssaultScreen : ISystemAssaultScreenView
    {
        #region AssaultGroup Attached Dependency Property

        public static readonly DependencyProperty AssaultGroupProperty = DependencyProperty.RegisterAttached(
            "AssaultGroup",
            typeof(AssaultGroup),
            typeof(SystemAssaultScreen),
            new PropertyMetadata(AssaultGroup.Invalid));

        public static void SetAssaultGroup(DependencyObject target, AssaultGroup value)
        {
            target.SetValue(AssaultGroupProperty, value);
        }

        public static AssaultGroup GetAssaultGroup(DependencyObject target)
        {
            return (AssaultGroup)target.GetValue(AssaultGroupProperty);
        }

        #endregion

        public SystemAssaultScreen()
        {
            InitializeComponent();

            ItemsControlTransitionSelector.SetTransitionSelector(
                _actionTabs,
                new ItemsControlTransitionSelector(
                    _actionTabs,
                    new SlideTransition { IsFromContentPushed = true, Direction = TransitionDirection.Backward   },
                    new SlideTransition { IsFromContentPushed = true, Direction = TransitionDirection.Forward }));

            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is SystemAssaultScreenViewModel oldModel)
            {
                oldModel.StateChanged -= OnModelStateChanged;
                oldModel.SelectedActionChanged -= OnModelSelectedActionChanged;
            }

            if (e.NewValue is SystemAssaultScreenViewModel newModel)
            {
                newModel.StateChanged += OnModelStateChanged;
                newModel.SelectedActionChanged += OnModelSelectedActionChanged;
                OnModelSelectedActionChanged(null, EventArgs.Empty);
                OnModelStateChanged(null, EventArgs.Empty);
            }
        }

        private void OnModelSelectedActionChanged(object sender, EventArgs e)
        {
            var action = Model.SelectedAction;
            if (action == null)
                return;

            switch (action.Value)
            {
                case InvasionAction.AttackOrbitalDefenses:
                    _actionTabs.SelectedItem = _chooseActionTab;
                    break;
                case InvasionAction.BombardPlanet:
                    _actionTabs.SelectedItem = _bombardActionTab;
                    break;
                case InvasionAction.UnloadAllOrdinance:
                    _actionTabs.SelectedItem = _chooseActionTab;
                    break;
                case InvasionAction.LandTroops:
                    _actionTabs.SelectedItem = _landTroopsTab;
                    break;
                case InvasionAction.StandDown:
                    _actionTabs.SelectedItem = _chooseActionTab;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnModelStateChanged(object sender, EventArgs eventArgs)
        {
            var state = Model.State;
            if (state == null)
                return;

            if (state == SystemAssaultScreenState.AwaitingPlayerOrders)
            {
                _actionTabs.SelectedItem = _chooseActionTab;
                return;
            }

            if (state == SystemAssaultScreenState.Finished)
            {
                _actionTabs.SelectedItem = _combatOverTab;
                return;
            }
        }

        #region IsActive Property

        [field: NonSerialized]
        public event EventHandler IsActiveChanged;

        private bool _isActive;

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (Equals(value, _isActive))
                    return;

                _isActive = value;

                OnIsActiveChanged();
            }
        }

        protected virtual void OnIsActiveChanged()
        {
            IsActiveChanged.Raise(this);
        }

        #endregion

        #region AppContext Property

        [field: NonSerialized]
        public event EventHandler AppContextChanged;

        private IAppContext _appContext;

        public IAppContext AppContext
        {
            get { return _appContext; }
            set
            {
                if (Equals(value, _appContext))
                    return;

                _appContext = value;

                OnAppContextChanged();
            }
        }

        protected virtual void OnAppContextChanged()
        {
            AppContextChanged.Raise(this);
        }

        #endregion

        #region Model Property

        [field: NonSerialized]
        public event EventHandler ModelChanged;

        public SystemAssaultScreenViewModel Model
        {
            get { return DataContext as SystemAssaultScreenViewModel; }
            set
            {
                if (Equals(value, DataContext))
                    return;

                DataContext = value;

                OnModelChanged();
            }
        }

        protected virtual void OnModelChanged()
        {
            ModelChanged.Raise(this);
        }

        #endregion

        public void OnCreated() { }

        public void OnDestroyed() { }

        private void OnChangeOrdersButtonClick(object sender, ExecuteRoutedEventArgs e)
        {
            if (Model.State != SystemAssaultScreenState.AwaitingPlayerOrders)
                return;

            Model.SelectedAction = null;

            _actionTabs.SelectedItem = _chooseActionTab;
        }
    }

    public sealed class ItemsControlTransitionSelector : TransitionSelector
    {
        private readonly ItemsControl _itemsControl;
        private readonly Transition _backTransition;
        private readonly Transition _forwardTransition;

        public static readonly DependencyProperty TransitionSelectorProperty = DependencyProperty.RegisterAttached(
            "TransitionSelector",
            typeof(TransitionSelector),
            typeof(ItemsControlTransitionSelector));

        public static void SetTransitionSelector(DependencyObject target, TransitionSelector value)
        {
            target.SetValue(TransitionSelectorProperty, value);
        }

        public static TransitionSelector GetTransitionSelector(DependencyObject target)
        {
            return (TransitionSelector)target.GetValue(TransitionSelectorProperty);
        }

        public ItemsControlTransitionSelector([NotNull] ItemsControl itemsControl, [NotNull] Transition backTransition, [NotNull] Transition forwardTransition)
        {
            _itemsControl = itemsControl ?? throw new ArgumentNullException("itemsControl");
            _backTransition = backTransition ?? throw new ArgumentNullException("backTransition");
            _forwardTransition = forwardTransition ?? throw new ArgumentNullException("forwardTransition");
        }

        public override Transition SelectTransition(TransitionPresenter presenter, object fromContent, object toContent)
        {
            var fromIndex = _itemsControl.Items.IndexOf(fromContent);
            var toIndex = _itemsControl.Items.IndexOf(toContent);

            if (fromIndex < 0)
            {
                if (fromContent is DependencyObject fromElement)
                {
                    var fromContainer = _itemsControl.ContainerFromElement(fromElement);
                    if (fromContainer == null)
                    {
                        fromContainer = fromElement.FindLogicalAncestor(
                            o =>
                            {
                                var itemsControl = ItemsControl.ItemsControlFromItemContainer(o);
                                return itemsControl == _itemsControl;
                            });
                    }

                    if (fromContainer != null)
                    {
                        fromIndex = _itemsControl.ItemContainerGenerator.IndexFromContainer(fromContainer);

                        if (fromIndex < 0)
                            fromIndex = _itemsControl.Items.IndexOf(fromContainer);
                    }
                }
            }
            
            if (toIndex < 0)
            {
                if (toContent is DependencyObject toElement)
                {
                    var toContainer = _itemsControl.ContainerFromElement(toElement);
                    if (toContainer == null)
                    {
                        toContainer = toElement.FindLogicalAncestor(
                            o =>
                            {
                                var itemsControl = ItemsControl.ItemsControlFromItemContainer(o);
                                return itemsControl == _itemsControl;
                            });
                    }

                    if (toContainer != null)
                    {
                        toIndex = _itemsControl.ItemContainerGenerator.IndexFromContainer(toContainer);

                        if (toIndex < 0)
                            toIndex = _itemsControl.Items.IndexOf(toContainer);
                    }
                }
            }
            if (fromIndex < 0 || toIndex < 0)
                return _forwardTransition;

            if (fromIndex < toIndex)
                return _forwardTransition;

            return _backTransition;
        }
    }

    internal class AssaultGroupDropTargetAdvisor : IDropTargetAdvisor
    {
        internal static readonly DataFormat CombatUnitUIContainerFormat = DataFormats.GetDataFormat("Supremacy.Client.Views.SystemAssaultScreen.CombatUnitUIContainer");

        #region IDropTargetAdvisor Members

        public bool IsValidDataObject(IDataObject obj)
        {
            if (!obj.GetDataPresent(CombatUnitUIContainerFormat.Name))
                return false;


            return CanDrop(obj, out List<CombatUnit> draggedUnits, out ICommand dropCommand);
        }

        private bool CanDrop(IDataObject obj, out List<CombatUnit> draggedUnits, out ICommand dropCommand)
        {
            dropCommand = null;
            draggedUnits = new List<CombatUnit>();

            if (!(TargetElement is FrameworkElement targetElement))
                return false;

            var targetListBox = targetElement.FindLogicalAncestorByType<ListBox>();
            if (targetListBox == null)
                return false;

            var view = targetElement.FindLogicalAncestorByType<SystemAssaultScreen>();
            if (view == null || view.Model == null)
                return false;

            if (!(ExtractElement(obj) is DependencyObject sourceElement))
                return false;

            var assaultGroup = SystemAssaultScreen.GetAssaultGroup(targetListBox);

            if (!(sourceElement is ListBox sourceListBox))
            {
                sourceListBox = sourceElement.FindLogicalAncestorByType<ListBox>();

                if (sourceListBox == null)
                    return false;

                draggedUnits.AddRange(sourceListBox.SelectedItems.OfType<CombatUnit>().Where(o => !o.IsDestroyed));
            }
            else
            {
                var combatUnit = sourceElement.GetValue(FrameworkElement.DataContextProperty) as CombatUnit;
                if (combatUnit == null || combatUnit.IsDestroyed)
                    return false;

                draggedUnits.Add(combatUnit);
            }

            if (draggedUnits.Count == 0)
                return false;

            switch (assaultGroup)
            {
                case AssaultGroup.Invalid:
                    goto default;
                case AssaultGroup.Standby:
                    dropCommand = view.Model.StandbyOrderCommand;
                    break;
                case AssaultGroup.Attack:
                    dropCommand = view.Model.AttackOrderCommand;
                    break;
                case AssaultGroup.LandTroops:
                    dropCommand = view.Model.LandTroopsOrderCommand;
                    return !draggedUnits.Select(o => o.Source).OfType<Ship>().Any(o => o.ShipType != ShipType.Transport);
                case AssaultGroup.Destroyed:
                    goto default;
                default:
                    return false;
            }

            return true;
        }

        public virtual void OnDropCompleted(IDataObject obj, Point dropPoint)
        {
            if (!(TargetElement is FrameworkElement targetElement))
                return;

            var targetListBox = targetElement.FindLogicalAncestorByType<ListBox>();
            if (targetListBox == null)
                return;

            var targetGroup = SystemAssaultScreen.GetAssaultGroup(targetListBox);
            if (targetGroup == AssaultGroup.Invalid || targetGroup == AssaultGroup.Destroyed)
                return;


            if (!CanDrop(obj, out List<CombatUnit> draggedUnits, out ICommand dropCommand))
                return;

            if (!draggedUnits.All(o => dropCommand.CanExecute(o)))
                return;

            draggedUnits.ForEach(o => dropCommand.Execute(o));
        }

        public UIElement TargetElement { get; set; }

        public bool ApplyMouseOffset => false;

        public UIElement GetVisualFeedback(IDataObject obj)
        {
            var element = ExtractElement(obj);

            UIElement visual;

            if (element is ListBox listBox)
            {
                var selectedItems = listBox.Items
                    .OfType<object>()
                    .Select(o => listBox.ItemContainerGenerator.ContainerFromItem(o))
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

                    for (var i = selectedItems.Count - 1; i >= 0; i--)
                    {
                        var selectedItem = selectedItems[i];

                        var rectangle = new Rectangle
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

                        Canvas.SetLeft(rectangle, i * 4);
                        Canvas.SetTop(rectangle, i * 4);

                        canvas.Children.Add(rectangle);
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
            return obj.GetData(CombatUnitUIContainerFormat.Name) as FrameworkElement;
        }
    }

    internal class AssaultGroupDragSourceAdvisor : IDragSourceAdvisor
    {
        #region IDragSourceAdvisor Members

        public DragDropEffects SupportedEffects => DragDropEffects.Move;

        public UIElement SourceElement { get; set; }

        public DataObject GetDataObject(UIElement draggedElement)
        {
            var data = new DataObject(AssaultGroupDropTargetAdvisor.CombatUnitUIContainerFormat.Name, draggedElement);
            return data;
        }

        public void FinishDrag(UIElement draggedElement, DragDropEffects finalEffects) { }

        public bool IsDraggable(UIElement draggedElement)
        {
            if (!(draggedElement is FrameworkElement draggedFrameworkElement))
                return false;

            var draggedListBox = draggedFrameworkElement.FindVisualAncestorByType<ListBox>();
            if (draggedListBox != null)
            {
                if (!(Mouse.DirectlyOver is UIElement sourceItem))
                    return false;

                var listBoxItem = sourceItem.FindVisualAncestorByType<ListBoxItem>();
                if (listBoxItem == null)
                    return false;
            }

            var view = draggedListBox.FindLogicalAncestorByType<SystemAssaultScreen>();
            if (view == null || view.Model == null || view.AppContext == null)
                return false;

            var localPlayerEmpire = view.AppContext.LocalPlayerEmpire;
            if (localPlayerEmpire == null)
                return false;

            var combatUnit = draggedFrameworkElement.DataContext as CombatUnit;
            if (combatUnit == null)
                return false;

            return (combatUnit.Source.OwnerID == localPlayerEmpire.CivilizationID);
        }

        public UIElement GetTopContainer()
        {
            return SourceElement.FindVisualRoot() as UIElement ??
                   Application.Current.MainWindow.Content as UIElement ??
                   Application.Current.MainWindow;
        }

        #endregion
    }
}
