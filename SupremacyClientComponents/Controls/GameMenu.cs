using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using ActiproSoftware.Windows.Media;

namespace Supremacy.Client.Controls
{
    /// <summary>
    /// Represents a menu control that can appear on a popup.
    /// </summary>
    /// <remarks>
    /// For detailed documentation on this control's features and how to use them, please see the 
    /// <a href="../Topics/Ribbon/Controls/Miscellaneous/Menu.html">Menu</a> documentation topic. 
    /// </remarks>
    public class GameMenu : GameItemsControl
    {

        private GameMenuItem _currentSelection;
        private DispatcherTimer _mouseOverTimer;

        #region Dependency Properties

        public static readonly DependencyProperty IsMenuItemInputGestureTextVisibleProperty = DependencyProperty.RegisterAttached(
            "IsMenuItemInputGestureTextVisible",
            typeof(bool),
            typeof(GameMenu),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.Inherits));

        public static readonly DependencyProperty CanUnhighlightWhenFocusedProperty = DependencyProperty.RegisterAttached(
            "CanUnhighlightWhenFocused",
            typeof(bool),
            typeof(GameMenu),
            new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty ItemVariantSizeProperty = DependencyProperty.Register(
            "ItemVariantSize",
            typeof(VariantSize),
            typeof(GameMenu),
            new FrameworkPropertyMetadata(
                VariantSize.Medium,
                OnItemVariantSizePropertyValueChanged));

        #endregion

        static GameMenu()
        {
            // Override property defaults
            IsTabStopProperty.OverrideMetadata(typeof(GameMenu), new FrameworkPropertyMetadata(false));
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GameMenu), new FrameworkPropertyMetadata(typeof(GameMenu)));
            KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata(typeof(GameMenu), new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));
            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(GameMenu), new FrameworkPropertyMetadata(KeyboardNavigationMode.Continue));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(GameMenu), new FrameworkPropertyMetadata(KeyboardNavigationMode.Continue));
            FocusableProperty.OverrideMetadata(typeof(GameMenu), new FrameworkPropertyMetadata(true));

            // Apply to native controls
            CanUnhighlightWhenFocusedProperty.OverrideMetadata(typeof(ComboBox), new FrameworkPropertyMetadata(false));
            CanUnhighlightWhenFocusedProperty.OverrideMetadata(typeof(System.Windows.Controls.Primitives.TextBoxBase), new FrameworkPropertyMetadata(false));

            // Attach to routed events
            EventManager.RegisterClassHandler(typeof(GameMenu), GameMenuItem.IsSelectedChangedEvent, new EventHandler<RoutedPropertyChangedEventArgs<bool>>(OnMenuItemIsSelectedChangedEvent));
        }

        internal GameMenuItem CurrentSelection
        {
            get => _currentSelection;
            set
            {
                bool isKeyboardFocused = false;

                if (_currentSelection != null)
                {
                    isKeyboardFocused = _currentSelection.IsKeyboardFocused;
                    _currentSelection.IsSelected = false;
                }

                _currentSelection = value;

                if (_currentSelection != null)
                {
                    _currentSelection.IsSelected = true;
                    if (isKeyboardFocused)
                    {
                        _ = _currentSelection.Focus();
                    }
                }
            }
        }

        private GameMenuItem HighlightedMenuItem => Items
                    .Cast<object>()
                    .Select((t, index) => ItemContainerGenerator.ContainerFromIndex(index) as GameMenuItem)
                    .FirstOrDefault(menuItem => menuItem != null && menuItem.IsHighlighted);

        private DispatcherTimer MouseOverTimer
        {
            get
            {
                if (_mouseOverTimer == null)
                {
                    // Create the timer
                    _mouseOverTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(SystemParameters.MenuShowDelay) };
                    _mouseOverTimer.Tick += OnMouseOverTimerTick;
                }
                return _mouseOverTimer;
            }
        }

        private static void OnItemVariantSizePropertyValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            GameMenu control = (GameMenu)obj;

            foreach (object item in control.Items)
            {
                if (!(item is IVariantControl childControl))
                {
                    childControl = control.ItemContainerGenerator.ContainerFromItem(item) as IVariantControl;
                }

                if (childControl != null)
                {
                    childControl.VariantSize = control.ItemVariantSize;
                }
            }
        }

        private static void OnMenuItemIsSelectedChangedEvent(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            if (!(e.OriginalSource is GameMenuItem originalSource))
            {
                return;
            }

            GameMenu control = (GameMenu)sender;

            if (e.NewValue)
            {
                if (control.CurrentSelection != originalSource &&
                    originalSource.LogicalParent == control)
                {
                    if (control.CurrentSelection != null && control.CurrentSelection.IsPopupOpen)
                    {
                        control.CurrentSelection.IsPopupOpen = false;
                    }

                    control.CurrentSelection = originalSource;
                }
            }
            else if (control.CurrentSelection == originalSource)
            {
                control.CurrentSelection = null;
            }

            e.Handled = true;
        }

        private void OnMouseOverTimerTick(object sender, EventArgs e)
        {
            MouseOverTimer.Stop();

            // Quit if the menu is hidden since the last timer start
            if (!IsVisible)
            {
                return;
            }

            // Select the menu item under the mouse and show its popup if it has one
            GameMenuItem highlightedMenuItem = HighlightedMenuItem;
            if (highlightedMenuItem == null)
            {
                return;
            }

            if (highlightedMenuItem != CurrentSelection)
            {
                highlightedMenuItem.FocusOrSelect();
            }

            if (highlightedMenuItem.HasPopup)
            {
                highlightedMenuItem.IsPopupOpen = true;
            }
        }

        public static bool GetCanUnhighlightWhenFocused(DependencyObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return (bool)obj.GetValue(CanUnhighlightWhenFocusedProperty);
        }

        public static void SetCanUnhighlightWhenFocused(DependencyObject obj, bool value)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            obj.SetValue(CanUnhighlightWhenFocusedProperty, value);
        }

        protected sealed override DependencyObject GetContainerForItemOverride()
        {
            return new GameMenuItem();
        }

        protected sealed override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is GameMenuItem;
        }

        public static bool GetIsMenuItemInputGestureTextVisible(DependencyObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return (bool)obj.GetValue(IsMenuItemInputGestureTextVisibleProperty);
        }

        public static void SetIsMenuItemInputGestureTextVisible(DependencyObject obj, bool value)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            obj.SetValue(IsMenuItemInputGestureTextVisibleProperty, value);
        }

        public VariantSize ItemVariantSize
        {
            get => (VariantSize)GetValue(ItemVariantSizeProperty);
            set => SetValue(ItemVariantSizeProperty, value);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            base.OnKeyDown(e);

            if (!e.Handled)
            {
                Key key = e.Key;
                if (FlowDirection == FlowDirection.RightToLeft)
                {
                    switch (key)
                    {
                        case Key.Left:
                            key = Key.Right;
                            break;
                        case Key.Right:
                            key = Key.Left;
                            break;
                    }
                }

                switch (key)
                {
                    case Key.Left:
                        if (CurrentSelection != null && CurrentSelection.IsPopupOpen)
                        {
                            // Hide a child menu item's popup
                            e.Handled = true;
                            CurrentSelection.IsPopupOpen = false;
                        }
                        else
                        {
                            IGamePopupAnchor popupAnchor = PopupControlService.GetParentPopupAnchor(this);
                            if (popupAnchor != null && popupAnchor.IsPopupOpen && !PopupControlService.IsTopLevel(popupAnchor))
                            {
                                // Close the parent popup
                                e.Handled = true;
                                _ = PopupControlService.Current.ClosePopup(popupAnchor, GamePopupCloseReason.EscapeKeyPressed);
                            }
                        }
                        break;

                    case Key.Right:
                        if (CurrentSelection != null && !CurrentSelection.IsPopupOpen && CurrentSelection.HasPopup)
                        {
                            // Show a child menu item's popup
                            e.Handled = true;
                            CurrentSelection.IsPopupOpen = true;

                            // Focus the first item in a child Menu
                            _ = Dispatcher.BeginInvoke(
                                DispatcherPriority.Send,
                                (Action)
                                (() =>
                                 {
                                     if (Keyboard.FocusedElement is GameMenu menu)
                                     {
                                         _ = menu.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                                     }
                                 }));
                        }
                        break;
                }
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            // If the IsVisible property changes...
            if (e.Property == IsVisibleProperty)
            {
                if (!(bool)e.NewValue)
                {
                    // The menu is hiding so clear the current selection
                    CurrentSelection = null;
                }

                // If the Menu is in a ContextMenu, ensure focus moves out of the context menu and into this Menu
                // after it is displayed... otherwise arrow keys won't work
                _ = Dispatcher.BeginInvoke(
                    DispatcherPriority.Send,
                    (Action)
                    (() =>
                     {
                         if (Keyboard.FocusedElement is ContextMenu contextMenuParent &&
                             VisualTreeHelperExtended.GetAncestor(this, typeof(ContextMenu)) == contextMenuParent)
                         {
                             _ = Focus();
                         }
                     }));
            }

            base.OnPropertyChanged(e);
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            base.OnItemsChanged(e);

            // If a variant control is added, update its variant size
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                    foreach (object item in e.NewItems)
                    {
                        IVariantControl control = item as IVariantControl ??
                                      ItemContainerGenerator.ContainerFromItem(item) as IVariantControl;
                        if (control == null)
                        {
                            continue;
                        }

                        control.Context = GameControlContext.MenuItem;
                        control.VariantSize = ItemVariantSize;
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    foreach (object item in Items)
                    {
                        IVariantControl control = item as IVariantControl ??
                                      ItemContainerGenerator.ContainerFromItem(item) as IVariantControl;
                        if (control == null)
                        {
                            continue;
                        }

                        control.Context = GameControlContext.MenuItem;
                        control.VariantSize = ItemVariantSize;
                    }
                    break;
            }
        }

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);

            if ((bool)e.NewValue)
            {
                return;
            }

            // Clear the selection if a context menu is not displayed
            if (CurrentSelection != null && !(Keyboard.FocusedElement is ContextMenu))
            {
                CurrentSelection = null;
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            // Clear the current selection if appropriate
            if (IsMouseOver || CurrentSelection == null)
            {
                return;
            }

            if (CurrentSelection.IsKeyboardFocusWithin)
            {
                if (CurrentSelection.IsPopupOpen)
                {
                    CurrentSelection.IsHighlighted = true;
                }
            }
            else if (!CurrentSelection.IsPopupOpen)
            {
                CurrentSelection = null;
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            // Restart the mouse over timer
            MouseOverTimer.Stop();
            MouseOverTimer.Start();

            base.OnPreviewMouseMove(e);
        }
    }
}