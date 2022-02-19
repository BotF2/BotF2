using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Supremacy.Client.Controls
{
    [DesignTimeVisible(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class GameMenuItem : ContentControl
    {
        #region Dependency Properties

        public static readonly DependencyProperty IsContentEnabledProperty = DependencyProperty.Register(
            "IsContentEnabled",
            typeof(bool),
            typeof(GameMenuItem),
            new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty IsPopupOpenProperty = PopupControlService.IsPopupOpenProperty.AddOwner(
            typeof(GameMenuItem),
            new FrameworkPropertyMetadata(false));

        internal static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected",
            typeof(bool),
            typeof(GameMenuItem),
            new FrameworkPropertyMetadata(
                false,
                OnIsSelectedPropertyValueChanged));

        #endregion

        #region Routed Events

        internal static readonly RoutedEvent IsSelectedChangedEvent = EventManager.RegisterRoutedEvent(
            "IsSelectedChanged",
            RoutingStrategy.Bubble,
            typeof(EventHandler<RoutedPropertyChangedEventArgs<bool>>),
            typeof(GameMenuItem));

        #endregion

        static GameMenuItem()
        {
            IsTabStopProperty.OverrideMetadata(
                typeof(GameMenuItem),
                new FrameworkPropertyMetadata(false));

            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(GameMenuItem),
                new FrameworkPropertyMetadata(typeof(GameMenuItem)));

            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(
                typeof(GameMenuItem),
                new FrameworkPropertyMetadata(KeyboardNavigationMode.Continue));

            GameControlService.IsExternalContentSupportedProperty.OverrideMetadata(
                typeof(GameMenuItem),
                new FrameworkPropertyMetadata(false));
        }

        private GameMenuItem CurrentSibling
        {
            get
            {
                GameMenuItem currentSelection = null;

                if (LogicalParent is GameMenu parentMenu)
                {
                    currentSelection = parentMenu.CurrentSelection;
                }

                if (currentSelection == this)
                {
                    currentSelection = null;
                }

                return currentSelection;
            }
        }

        internal void FocusOrSelect()
        {
            if (!IsKeyboardFocusWithin)
            {
                _ = Focus();
            }

            if (!IsSelected)
            {
                IsSelected = true;
            }

            if (IsSelected && (!IsHighlighted))
            {
                IsHighlighted = true;
            }
        }

        private UIElement GetTargetElement()
        {
            UIElement element = null;
            if (GameControlService.GetIsExternalContentSupported(this))
            {
                element = this;
            }
            else if (VisualTreeHelper.GetChildrenCount(this) > 0)
            {
                // Look for a UIElement that is the child of the ContentPresenter (this will normally be a Button)
                if (VisualTreeHelper.GetChild(this, 0) is ContentPresenter presenter && VisualTreeHelper.GetChildrenCount(presenter) > 0)
                {
                    element = VisualTreeHelper.GetChild(presenter, 0) as UIElement;
                }
            }
            return element ?? this;
        }

        internal bool HasPopup
        {
            get
            {
                UIElement element = GetTargetElement();
                return PopupControlService.GetHasPopup(element);
            }
        }

        internal bool IsHighlighted
        {
            get
            {
                UIElement element = GetTargetElement();
                return GameControlService.GetIsHighlighted(element);
            }
            set
            {
                UIElement element = GetTargetElement();
                if (value)
                {
                    GameControlService.SetIsHighlighted(element, true);
                }
                else
                {
                    element.ClearValue(GameControlService.IsHighlightedProperty);
                }
            }
        }

        internal bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        internal object LogicalParent
        {
            get
            {
                if (Parent != null)
                {
                    return Parent;
                }

                return ItemsControl.ItemsControlFromItemContainer(this);
            }
        }

        private static void OnIsSelectedPropertyValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            GameMenuItem control = (GameMenuItem)obj;

            control.IsHighlighted = (bool)e.NewValue;

            if ((bool)e.OldValue)
            {
                if (control.IsPopupOpen && !(Keyboard.FocusedElement is ContextMenu))
                {
                    control.IsPopupOpen = false;
                }
            }

            control.RaiseEvent(
                new RoutedPropertyChangedEventArgs<bool>(
                    (bool)e.OldValue,
                    (bool)e.NewValue,
                    IsSelectedChangedEvent));
        }

        public bool IsContentEnabled
        {
            get => (bool)GetValue(IsContentEnabledProperty);
            set => SetValue(IsContentEnabledProperty, value);
        }

        public bool IsPopupOpen
        {
            get => (bool)GetValue(IsPopupOpenProperty);
            set => SetValue(IsPopupOpenProperty, value);
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            if (oldContent is DependencyObject)
            {
                BindingOperations.ClearBinding(this, PopupControlService.IsPopupOpenProperty);
                BindingOperations.ClearBinding(this, IsContentEnabledProperty);
                BindingOperations.ClearBinding(this, VisibilityProperty);

                // If the menu item's external content properties were bound, clear the bindings
                if (GameControlService.GetIsExternalContentSupported(this))
                {
                    BindingOperations.ClearBinding(this, GameControlService.ImageSourceLargeProperty);
                    BindingOperations.ClearBinding(this, GameControlService.ImageSourceSmallProperty);
                    BindingOperations.ClearBinding(this, GameControlService.LabelProperty);
                }
            }

            if (newContent is DependencyObject newContentObj)
            {
                // Update the IsTabStop property based on whether the child item keeps highlighted when focused
                IsTabStop = !CanUnhighlightWhenFocused(newContentObj);

                // Bind to IsPopupOpen
                Binding binding = new Binding
                {
                    Mode = BindingMode.TwoWay,
                    Source = newContent,
                    Path = new PropertyPath(PopupControlService.IsPopupOpenProperty)
                };

                _ = SetBinding(PopupControlService.IsPopupOpenProperty, binding);

                if (newContent is UIElement)
                {
                    // Bind to IsEnabled
                    binding = new Binding
                    {
                        Source = newContent,
                        Path = new PropertyPath("IsEnabled")
                    };

                    _ = SetBinding(IsContentEnabledProperty, binding);

                    // Bind to Visibility
                    binding = new Binding
                    {
                        Source = newContent,
                        Path = new PropertyPath("Visibility")
                    };

                    _ = SetBinding(VisibilityProperty, binding);
                }

                // Tell the menu item whether external content is supported based on the content's settings
                bool isExternalContentSupported = GameControlService.GetIsExternalContentSupported(newContentObj);

                GameControlService.SetIsExternalContentSupported(this, isExternalContentSupported);

                if (isExternalContentSupported)
                {
                    // Configure bindings
                    binding = new Binding
                    {
                        Source = newContent,
                        Path = new PropertyPath(GameControlService.ImageSourceLargeProperty)
                    };

                    _ = SetBinding(GameControlService.ImageSourceLargeProperty, binding);

                    binding = new Binding
                    {
                        Source = newContent,
                        Path = new PropertyPath(GameControlService.ImageSourceSmallProperty)
                    };

                    _ = SetBinding(GameControlService.ImageSourceSmallProperty, binding);

                    binding = new Binding
                    {
                        Source = newContent,
                        Path = new PropertyPath(GameControlService.LabelProperty)
                    };

                    _ = SetBinding(GameControlService.LabelProperty, binding);
                }
            }
            else
            {
                GameControlService.SetIsExternalContentSupported(this, false);
            }
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            base.OnGotKeyboardFocus(e);

            // Update whether the content is selected
            if (!e.Handled && e.NewFocus == this)
            {
                IsSelected = true;
            }
        }

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);

            // Update whether the content is selected
            if (IsKeyboardFocusWithin && !IsSelected)
            {
                IsSelected = true;
            }
        }

        private static bool CanUnhighlightWhenFocused(DependencyObject obj)
        {
            while (obj is Visual)
            {
                if (!GameMenu.GetCanUnhighlightWhenFocused(obj))
                {
                    return false;
                }

                obj = VisualTreeHelper.GetParent(obj);
            }
            return true;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            if (Keyboard.FocusedElement is UIElement focusedElement && !CanUnhighlightWhenFocused(focusedElement))
            {
                // Quit since the currently focused element doesn't allow highlight changes while focused
                return;
            }

            GameMenuItem currentSibling = CurrentSibling;
            if (currentSibling != null && currentSibling.IsPopupOpen)
            {
                currentSibling.IsHighlighted = false;

                if (IsContentEnabled)
                {
                    IsHighlighted = true;
                }
            }
            else if (IsContentEnabled)
            {
                if (!IsPopupOpen)
                {
                    FocusOrSelect();
                }
                else
                {
                    IsHighlighted = true;
                }
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            // If the focused element can lose menu item highlight...
            if (!(Keyboard.FocusedElement is UIElement focusedElement) || CanUnhighlightWhenFocused(focusedElement))
            {
                // If no popup is open
                if (!IsPopupOpen)
                {
                    if (IsSelected)
                    {
                        IsSelected = false;
                    }
                    else
                    {
                        IsHighlighted = false;
                    }

                    if (IsKeyboardFocusWithin)
                    {
                        ItemsControl control = ItemsControl.ItemsControlFromItemContainer(this);
                        if (control != null)
                        {
                            _ = control.Focus();
                        }
                    }
                }
            }
        }
    }
}