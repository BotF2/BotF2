using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Supremacy.Client.Controls
{
    public class CheckedListBox : MultiSelector
    {
        public static readonly RoutedCommand SelectAllCommand;
        public static readonly RoutedCommand UnselectAllCommand;

        static CheckedListBox()
        {
            SelectAllCommand = new RoutedCommand("SelectAll", typeof(CheckedListBox));
            UnselectAllCommand = new RoutedCommand("UnselectAll", typeof(CheckedListBox));

            CommandManager.RegisterClassCommandBinding(
                typeof(CheckedListBox),
                new CommandBinding(
                    SelectAllCommand,
                    OnSelectAll));
            
            CommandManager.RegisterClassCommandBinding(
                 typeof(CheckedListBox),
                 new CommandBinding(
                     UnselectAllCommand,
                     OnUnselectAll));

            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CheckedListBox),
                new FrameworkPropertyMetadata(typeof(CheckedListBox)));
        }

        private static void OnSelectAll(object target, ExecutedRoutedEventArgs args)
        {
            CheckedListBox listBox = target as CheckedListBox;
            if (listBox != null)
                listBox.SelectAll();
        }

        private static void OnUnselectAll(object target, ExecutedRoutedEventArgs args)
        {
            CheckedListBox listBox = target as CheckedListBox;
            if (listBox != null)
                listBox.UnselectAll();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is CheckedListBoxItem || item is Separator;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new CheckedListBoxItem();
        }

        internal static bool ItemGetIsSelectable(object item)
        {
            return item != null && !(item is Separator);
        }

        internal static bool ElementGetIsSelectable(DependencyObject o)
        {
            if (o != null && ItemGetIsSelectable(o))
            {
                ItemsControl itemsControl = ItemsControlFromItemContainer(o);
                if (itemsControl != null)
                {
                    object item = itemsControl.ItemContainerGenerator.ItemFromContainer(o);
                    return item == o || ItemGetIsSelectable(item);
                }
            }
            return false;
        }
    }

    [DefaultEvent("Selected")]
    public class CheckedListBoxItem : ContentControl
    {
        public static readonly DependencyProperty IsSelectedProperty;

        public static readonly RoutedEvent SelectedEvent;
        public static readonly RoutedEvent UnselectedEvent;

        [Category("Appearance")]
        [Bindable(true)]
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        internal MultiSelector ParentSelector => ItemsControl.ItemsControlFromItemContainer(this) as MultiSelector;

        public event RoutedEventHandler Selected
        {
            add { AddHandler(SelectedEvent, value); }
            remove { RemoveHandler(SelectedEvent, value); }
        }

        public event RoutedEventHandler Unselected
        {
            add { AddHandler(UnselectedEvent, value); }
            remove { RemoveHandler(UnselectedEvent, value); }
        }

        static CheckedListBoxItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CheckedListBoxItem),
                new FrameworkPropertyMetadata(typeof(CheckedListBoxItem)));

            IsSelectedProperty = Selector.IsSelectedProperty.AddOwner(
                typeof(CheckedListBoxItem),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                    OnIsSelectedChanged));

            SelectedEvent = Selector.SelectedEvent.AddOwner(typeof(CheckedListBoxItem));
            UnselectedEvent = Selector.UnselectedEvent.AddOwner(typeof(CheckedListBoxItem));

            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(
                typeof(CheckedListBoxItem),
                new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));

            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(
                typeof(CheckedListBoxItem),
                new FrameworkPropertyMetadata(KeyboardNavigationMode.Local));
        }

        protected virtual void OnSelected(RoutedEventArgs e)
        {
            HandleIsSelectedChanged(true, e);
        }

        protected virtual void OnUnselected(RoutedEventArgs e)
        {
            HandleIsSelectedChanged(false, e);
        }

        private void HandleIsSelectedChanged(bool newValue, RoutedEventArgs e)
        {
            RaiseEvent(e);
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            ItemsControl itemsControl = (ItemsControl)null;

            if (VisualTreeHelper.GetParent(this) == null)
            {
                if (IsKeyboardFocusWithin)
                    itemsControl = ItemsControl.GetItemsOwner(oldParent);
            }
            else
            {
                MultiSelector parentSelector = ParentSelector;
                if (parentSelector != null && parentSelector.GroupStyle != null && !IsSelected)
                {
                    object t = parentSelector.ItemContainerGenerator.ItemFromContainer(this);
                    if (t != null && parentSelector.SelectedItems.Contains(t))
                        SetCurrentValue(IsSelectedProperty, true);
                }
            }

            base.OnVisualParentChanged(oldParent);

            if (itemsControl != null)
                itemsControl.Focus();
        }

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CheckedListBoxItem checkedListBoxItem = (CheckedListBoxItem)d;
            bool isSelected = (bool)e.NewValue;

            if (isSelected)
                checkedListBoxItem.OnSelected(new RoutedEventArgs(Selector.SelectedEvent, checkedListBoxItem));
            else
                checkedListBoxItem.OnUnselected(new RoutedEventArgs(Selector.UnselectedEvent, checkedListBoxItem));
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                HandleMouseButtonDown();
            }
            base.OnMouseLeftButtonDown(e);
        }

        private void HandleMouseButtonDown()
        {
            if (CheckedListBox.ElementGetIsSelectable(this) && Focus())
                IsSelected = !IsSelected;
        }

    }
}