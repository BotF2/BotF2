using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Supremacy.Client.Controls
{
    public enum ControlResizeMode
    {
        None = 0,
        Horizontal,
        Vertical,
        Both,
    }

    public class PopupControlService
    {
        private readonly List<IGamePopupAnchor> _popupAnchors; // Represents a stack with items on top of stack at index 0

        private static PopupControlService _current;

        #region Routed Events
        public static readonly RoutedEvent PopupClosedEvent = EventManager.RegisterRoutedEvent(
            "PopupClosed",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(PopupControlService));

        public static readonly RoutedEvent PopupOpenedEvent = EventManager.RegisterRoutedEvent(
            "PopupOpened",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(PopupControlService));

        public static readonly RoutedEvent PopupOpeningEvent = EventManager.RegisterRoutedEvent(
            "PopupOpening",
            RoutingStrategy.Bubble,
            typeof(EventHandler<CancelRoutedEventArgs>),
            typeof(PopupControlService));
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty CustomPopupPlacementCallbackProperty =
            DependencyProperty.RegisterAttached(
                "CustomPopupPlacementCallback",
                typeof(CustomPopupPlacementCallback),
                typeof(PopupControlService),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty HasPopupProperty = DependencyProperty.RegisterAttached(
            "HasPopup",
            typeof(bool),
            typeof(PopupControlService),
            new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsPopupOpenProperty =
            DependencyProperty.RegisterAttached(
                "IsPopupOpen",
                typeof(bool),
                typeof(PopupControlService),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnIsPopupOpenPropertyValueChanged));

        public static readonly DependencyProperty PopupContentProperty =
            DependencyProperty.RegisterAttached(
                "PopupContent",
                typeof(object),
                typeof(PopupControlService),
                new FrameworkPropertyMetadata(
                    null,
                    OnPopupContentPropertyValueChanged));

        public static readonly DependencyProperty PopupContentTemplateProperty =
            DependencyProperty.RegisterAttached(
                "PopupContentTemplate",
                typeof(DataTemplate),
                typeof(PopupControlService),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty PopupContentTemplateSelectorProperty =
            DependencyProperty.RegisterAttached(
                "PopupContentTemplateSelector",
                typeof(DataTemplateSelector),
                typeof(PopupControlService),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty PopupHasBorderProperty =
            DependencyProperty.RegisterAttached(
                "PopupHasBorder",
                typeof(bool),
                typeof(PopupControlService),
                new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty PopupHorizontalOffsetProperty =
            DependencyProperty.RegisterAttached(
                "PopupHorizontalOffset",
                typeof(double),
                typeof(PopupControlService),
                new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty PopupPlacementProperty =
            DependencyProperty.RegisterAttached(
                "PopupPlacement",
                typeof(PlacementMode),
                typeof(PopupControlService),
                new FrameworkPropertyMetadata(PlacementMode.Bottom));

        public static readonly DependencyProperty PopupPlacementRectangleProperty =
            DependencyProperty.RegisterAttached(
                "PopupPlacementRectangle",
                typeof(Rect),
                typeof(PopupControlService),
                new FrameworkPropertyMetadata(Rect.Empty));

        public static readonly DependencyProperty PopupPlacementTargetProperty =
            DependencyProperty.RegisterAttached(
                "PopupPlacementTarget",
                typeof(UIElement),
                typeof(PopupControlService),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty PopupResizeModeProperty =
            DependencyProperty.RegisterAttached(
                "PopupResizeMode",
                typeof(ControlResizeMode),
                typeof(PopupControlService),
                new FrameworkPropertyMetadata(ControlResizeMode.None));

        public static readonly DependencyProperty PopupVerticalOffsetProperty =
            DependencyProperty.RegisterAttached(
                "PopupVerticalOffset",
                typeof(double),
                typeof(PopupControlService),
                new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty StaysOpenOnClickProperty =
            DependencyProperty.RegisterAttached(
                "StaysOpenOnClick",
                typeof(bool),
                typeof(PopupControlService),
                new
                    FrameworkPropertyMetadata(false));
        #endregion

        #region Constructors and Finalizers
        internal PopupControlService()
        {
            _popupAnchors = new List<IGamePopupAnchor>();

            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                AttachToInputManager();
            }
        }
        #endregion

        private static void AttachToInputManager()
        {
            InputManager.Current.PostProcessInput += OnPostProcessInput;
        }

        internal static PopupControlService Current
        {
            get
            {
                EnsureCreated();
                return _current;
            }
        }

        internal static void EnsureCreated()
        {
            if (_current == null)
            {
                _current = new PopupControlService();
            }
        }

        internal WeakReference IgnoreNextMouseDownOver { get; set; }

        private static void OnPostProcessInput(object sender, ProcessInputEventArgs e)
        {
            if (e.StagingItem.Input.RoutedEvent == Mouse.PreviewMouseMoveEvent)
            {
                InfoCardService.Current.ProcessPreviewMouseMoveForInfoCard((MouseEventArgs)e.StagingItem.Input);
            }
            else if (e.StagingItem.Input.RoutedEvent == Keyboard.PreviewKeyDownEvent)
            {
                InfoCardService.Current.ProcessPreviewKeyDownForInfoCard((KeyEventArgs)e.StagingItem.Input);
            }
        }

        private void CloseAllPopupsCore(GamePopupCloseReason closeReason)
        {
            while (_popupAnchors.Count > 0)
            {
                if (!CloseTopmostPopup(closeReason))
                {
                    return;
                }
            }
        }

        internal bool ClosePopup(IGamePopupAnchor popupAnchor, GamePopupCloseReason closeReason)
        {
            // If the popup is not currently being managed, simply call the core close code on it
            if (!_popupAnchors.Contains(popupAnchor))
            {
                return false;
            }

            while (_popupAnchors.Count > 0)
            {
                bool isRootPopupAnchor = RootPopupAnchor == popupAnchor;
                bool isTargetPopupAnchor = TopmostPopupAnchor == popupAnchor;

                if (!CloseTopmostPopup(closeReason))
                {
                    return false;
                }

                if (!isTargetPopupAnchor)
                {
                    continue;
                }

                // Focus the anchor if the escape key was pressed and the popup was not originally opened with the mouse
                if ((closeReason == GamePopupCloseReason.EscapeKeyPressed) &&
                    (!isRootPopupAnchor || !popupAnchor.PopupOpenedWithMouse) &&
                    popupAnchor.Focusable)
                {
                    _ = popupAnchor.Focus();
                }

                return true;
            }

            return false;
        }

        private void ClosePopupCore(IGamePopupAnchor popupAnchor, GamePopupCloseReason closeReason)
        {
            // Update the close reason (must happen before changing mouse capture)
            if (popupAnchor.LastCloseReason == GamePopupCloseReason.Unknown)
            {
                popupAnchor.LastCloseReason = closeReason;
            }

            // Get the popup host
            FrameworkElement popupHost = GetPopupHost(popupAnchor);

            // Update the captured popup
            if ((popupHost != null) && (Mouse.Captured == popupHost))
            {
                _ = _popupAnchors.Count > 0
                    ? Mouse.Capture(GetPopupHost(_popupAnchors[_popupAnchors.Count - 1]), CaptureMode.SubTree)
                    : Mouse.Capture(null);
            }

            if (popupAnchor.IsPopupOpen)
            {
                popupAnchor.IsPopupOpen = false;
            }

            if (popupHost != null)
            {
                popupHost.KeyDown -= OnKeyDown;
                popupHost.LostMouseCapture -= OnLostMouseCapture;
                popupHost.PreviewKeyDown -= OnPreviewKeyDown;

                popupHost.RemoveHandler(
                    Mouse.PreviewMouseDownOutsideCapturedElementEvent,
                    (MouseButtonEventHandler)OnClickThrough);

                popupHost.RemoveHandler(
                    Mouse.PreviewMouseUpOutsideCapturedElementEvent,
                    (MouseButtonEventHandler)OnClickThrough);
            }

            if ((popupAnchor is UIElement popupAnchorElement) && (_popupAnchors.Count == 0))
            {
                popupAnchorElement.IsKeyboardFocusWithinChanged -= OnIsKeyboardFocusWithinChanged;
            }

            return;
        }

        internal bool CloseTopmostPopup(GamePopupCloseReason closeReason)
        {
            // Quit if there are no open popups
            if (_popupAnchors.Count == 0)
            {
                return false;
            }

            // Get the topmost popup
            IGamePopupAnchor popupAnchor = _popupAnchors[0];
            _popupAnchors.RemoveAt(0);
            ClosePopupCore(popupAnchor, closeReason);

            return true;
        }

        private static DependencyObject FindParent(DependencyObject node)
        {
            if (node.GetType().Name == "GamePopupRoot")
            {
                if (((FrameworkElement)node).Parent is GamePopup popup)
                {
                    if (popup.Parent != null)
                    {
                        return popup.Parent;
                    }

                    return popup.TemplatedParent ?? popup.PlacementTarget;
                }
            }

            if (node.IsVisual())
            {
                DependencyObject parent = VisualTreeHelper.GetParent(node);
                if (parent != null)
                {
                    return parent;
                }
            }

            return LogicalTreeHelper.GetParent(node);
        }

        internal static IGamePopupAnchor GetParentPopupAnchor(DependencyObject node)
        {
            while (node != null)
            {
                node = FindParent(node);

                if (node is IGamePopupAnchor popupAnchor)
                {
                    return popupAnchor;
                }
            }
            return null;
        }

        private static FrameworkElement GetPopupHost(IGamePopupAnchor popupAnchor)
        {
            if (popupAnchor != null && popupAnchor.Popup != null)
            {
                return popupAnchor.Popup.PopupRoot;
            }

            return null;
        }

        private static bool HasCapture(IGamePopupAnchor popupAnchor)
        {
            FrameworkElement popupHost = GetPopupHost(popupAnchor);
            return (popupHost != null) && (Mouse.Captured == popupHost);
        }

        private static bool IsDescendant(DependencyObject ancestorNode, DependencyObject node)
        {
            return node.IsVisualDescendantOf(ancestorNode);
        }

        internal static bool IsPopupAnchorPopupEnabled(IGamePopupAnchor popupAnchor)
        {
            if (!(popupAnchor is DependencyObject popupAnchorObj))
            {
                return true;
            }

            if (!(GetPopupContent(popupAnchorObj) is UIElement popupContent))
            {
                return true;
            }

            // Recurse through logical children... if there are none found, assume enabled
            bool isEnabled = false;
            if (!IsPopupAnchorPopupEnabledRecursive(popupContent, ref isEnabled))
            {
                isEnabled = true;
            }

            return isEnabled;
        }

        private static bool IsPopupAnchorPopupEnabledRecursive(UIElement element, ref bool isEnabled)
        {
            bool validControlFound = false;

            // Check for an items control
            if (element is ItemsControl itemsControl)
            {
                for (int index = 0; index < itemsControl.Items.Count; index++)
                {
                    UIElement childElement = itemsControl.Items[index] as UIElement ??
                                       itemsControl.ItemContainerGenerator.ContainerFromIndex(index) as UIElement;

                    if (childElement == null)
                    {
                        continue;
                    }

                    if (!(childElement is IGameControl gameControl))
                    {
                        continue;
                    }

                    if (gameControl.Command != null)
                    {
                        // If using ItemsSource, the visual tree may not yet be built so force the parent element as the target
                        isEnabled = VisualTreeHelper.GetParent(childElement) == null
                            ? GameCommand.CanExecuteCommandSource(gameControl, element)
                            : GameCommand.CanExecuteCommandSource(gameControl);
                    }
                    else
                    {
                        isEnabled = childElement.IsEnabled;
                    }

                    validControlFound = true;

                    if (isEnabled)
                    {
                        return true;
                    }
                }
            }

            System.Collections.IEnumerable children = LogicalTreeHelper.GetChildren(element);
            foreach (object child in children)
            {
                if (!(child is UIElement childElement))
                {
                    continue;
                }

                if (childElement is IGameControl gameControl)
                {
                    isEnabled = gameControl.Command != null ? GameCommand.CanExecuteCommandSource(gameControl) : childElement.IsEnabled;

                    validControlFound = true;

                    if (isEnabled)
                    {
                        return true;
                    }
                }

                if (childElement is IGamePopupAnchor)
                {
                    continue;
                }

                // Recurse and quit afterwards if an enabled 
                validControlFound |= IsPopupAnchorPopupEnabledRecursive(childElement, ref isEnabled);

                if (isEnabled)
                {
                    return validControlFound;
                }
            }

            return validControlFound;
        }

        internal static bool IsPopupBelowAnchor(IGamePopupAnchor popupAnchor)
        {
            if ((popupAnchor is UIElement popupAnchorControl) && (popupAnchor.Popup != null))
            {
                UIElement popupChild = popupAnchor.Popup.Child;
                if (popupChild != null)
                {
                    Point popupAnchorLocation = popupAnchorControl.PointToScreen(
                        new Point(
                            0,
                            popupAnchorControl.RenderSize.Height / 2));

                    Point popupControlLocation = popupChild.PointToScreen(
                        new Point(
                            0,
                            popupChild.RenderSize.Height / 2));

                    return popupAnchorLocation.Y < popupControlLocation.Y;
                }
            }
            return true;
        }

        internal static bool IsTopLevel(IGamePopupAnchor popupAnchor)
        {
            return GetParentPopupAnchor(popupAnchor as DependencyObject) == null;
        }

        private void OnClickThrough(object sender, MouseButtonEventArgs e)
        {
            IgnoreNextMouseDownOver = null;

            if (!(sender is GamePopupRoot popupHost))
            {
                return;
            }

            if ((!(popupHost.Popup.TemplatedParent is IGamePopupAnchor popupAnchor)) ||
                ((e.ChangedButton != MouseButton.Left) && (e.ChangedButton != MouseButton.Right)) ||
                !HasCapture(popupAnchor))
            {
                return;
            }

            if (popupHost.InputHitTest(e.GetPosition(popupHost)) is DependencyObject hitTarget && popupHost.IsLogicalAncestorOf(hitTarget))
            {
                return;
            }

            bool closePopup = true;
            if (e.ButtonState == MouseButtonState.Released)
            {
                if ((e.ChangedButton == MouseButton.Left) && popupAnchor.IgnoreNextLeftRelease)
                {
                    popupAnchor.IgnoreNextLeftRelease = false;
                    closePopup = false;
                }
                else if ((e.ChangedButton == MouseButton.Right) && popupAnchor.IgnoreNextRightRelease)
                {
                    popupAnchor.IgnoreNextRightRelease = false;
                    closePopup = false;
                }
            }

            if (closePopup)
            {
                _ = ClosePopup(popupAnchor, GamePopupCloseReason.ClickThrough);
            }
        }

        private void OnContextMenuClosed(object sender, RoutedEventArgs e)
        {
            ContextMenu contextMenu = (ContextMenu)sender;

            contextMenu.RemoveHandler(
                GameControlService.PreviewClickEvent,
                (RoutedEventHandler)OnContextMenuItemPreviewClick);

            contextMenu.Closed -= OnContextMenuClosed;
        }

        private void OnContextMenuItemPreviewClick(object sender, RoutedEventArgs e)
        {
            // Close all popups since a button was clicked on the context menu
            CloseAllPopupsCore(GamePopupCloseReason.IsPopupOpenChanged);
        }

        private void OnIsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // If keyboard focus is no longer in the popup hierarchy...
            if ((bool)e.NewValue)
            {
                return;
            }

            if ((Keyboard.FocusedElement is ContextMenu contextMenu) && contextMenu.IsOpen)
            {
                contextMenu.AddHandler(
                    GameControlService.PreviewClickEvent,
                    (RoutedEventHandler)OnContextMenuItemPreviewClick);

                contextMenu.Closed += OnContextMenuClosed;

                // Prevent the context menu from closing the popup
                return;
            }

            // Close all popups
            CloseAllPopupsCore(GamePopupCloseReason.LostKeyboardFocusWithin);
        }

        private static void OnIsPopupOpenPropertyValueChanged(
            DependencyObject o,
            DependencyPropertyChangedEventArgs e)
        {
            if (!(o is IGamePopupAnchor popupAnchor))
            {
                return;
            }

            if (e.NewValue.Equals(true))
            {
                // Raise a cancelable opening event
                if (!popupAnchor.OnPopupOpening())
                {
                    // Opening was cancelled
                    SetIsPopupOpen(o, false);
                    return;
                }

                UIElement element = popupAnchor.Popup?.Child;
                if (element != null)
                {
                    if ((popupAnchor is DependencyObject popupAnchorObj) && (element.DesiredSize.Width == 0))
                    {
                        // The first time a Popup is created, it doesn't have its visual tree generated yet...
                        // so in this case we place the content in a temporary ContentPresenter for measurement purposes
                        ContentPresenter presenter = new ContentPresenter
                        {
                            Content = GetPopupContent(popupAnchorObj),
                            ContentTemplate = GetPopupContentTemplate(popupAnchorObj),
                            ContentTemplateSelector = GetPopupContentTemplateSelector(popupAnchorObj)
                        };
                        element = presenter;
                        element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    }
                }

                if ((element != null) && (element.Visibility != Visibility.Collapsed))
                {
                    // Update the minimum width and add to the total height
                    element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                }

                // Start tracking the popup 
                _ = Current.OpenPopup(popupAnchor);

                // Raise an opened event
                popupAnchor.OnPopupOpened();
            }
            else
            {
                // Ensure the popup is closed
                _ = Current.ClosePopup(popupAnchor, GamePopupCloseReason.IsPopupOpenChanged);

                // Raise a closed event
                popupAnchor.OnPopupClosed();
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Escape:
                    // Close the popup
                    if (sender is FrameworkElement popupHost)
                    {
                        if (popupHost.TemplatedParent is IGamePopupAnchor popupAnchor)
                        {
                            e.Handled = true;
                            _ = ClosePopup(popupAnchor, GamePopupCloseReason.EscapeKeyPressed);
                        }
                    }
                    break;
            }
        }

        private void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            IGamePopupAnchor popupAnchor = sender as IGamePopupAnchor ?? GetParentPopupAnchor(sender as DependencyObject);
            if (popupAnchor == null)
            {
                return;
            }

            FrameworkElement popupHost = GetPopupHost(popupAnchor);
            if ((popupHost == null) || (Mouse.Captured == popupHost))
            {
                return;
            }

            if (e.OriginalSource == popupHost)
            {
                if ((Mouse.Captured == null) ||
                    ((Mouse.Captured != popupAnchor) &&
                     !IsDescendant(popupHost, Mouse.Captured as DependencyObject)))
                {
                    _ = ClosePopup(popupAnchor, GamePopupCloseReason.LostMouseCapture);
                }
            }
            else if (IsDescendant(popupHost, e.OriginalSource as DependencyObject))
            {
                if (popupAnchor.IsPopupOpen && (Mouse.Captured == null))
                {
                    _ = Mouse.Capture(popupHost, CaptureMode.SubTree);
                    e.Handled = true;
                }
            }
            else
            {
                _ = ClosePopup(popupAnchor, GamePopupCloseReason.LostMouseCapture);
            }
        }

        private static void OnPopupContentPropertyValueChanged(
            DependencyObject o,
            DependencyPropertyChangedEventArgs e)
        {
            if (!(o is ILogicalParent control))
            {
                return;
            }

            if ((e.OldValue is DependencyObject oldContent) && (oldContent.GetLogicalParent() == control))
            {
                control.RemoveLogicalChild(oldContent);
            }

            if ((e.NewValue is DependencyObject newContent) && (newContent.GetLogicalParent() == null))
            {
                control.AddLogicalChild(newContent);
            }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // If the Alt or F10 key s pressed, close all popups
            if ((e.Key == Key.System) &&
                ((e.SystemKey == Key.LeftAlt) || (e.SystemKey == Key.RightAlt) || (e.SystemKey == Key.F10)))
            {
                CloseAllPopupsCore(GamePopupCloseReason.SystemMenuKeyPressed);
            }
        }

        internal bool OpenPopup(IGamePopupAnchor popupAnchor)
        {
            if (popupAnchor == null)
            {
                return false;
            }

            IGamePopupAnchor parentPopupAnchor = GetParentPopupAnchor(popupAnchor as DependencyObject);
            while ((parentPopupAnchor != null) && (!parentPopupAnchor.IsPopupOpen))
            {
                // Only use parent popup anchors that have open popups since there are cases like where Game has a minimized popup open 
                //   and a control inside an uncollapsed Group is clicked... without this, the Game minimized popup is closed inappropriately
                parentPopupAnchor = GetParentPopupAnchor(parentPopupAnchor as DependencyObject);
            }
            if (parentPopupAnchor != null)
            {
                // Close any popup anchors that don't share the same ancestry
                while (_popupAnchors.Count > 0)
                {
                    // Quit since a common ancestor was found
                    if (TopmostPopupAnchor == parentPopupAnchor)
                    {
                        break;
                    }

                    // Close the topmost popup
                    _ = CloseTopmostPopup(GamePopupCloseReason.OtherPopupOpened);
                }
            }
            else
            {
                // Close all open popups since the new one has no parent
                CloseAllPopupsCore(GamePopupCloseReason.OtherPopupOpened);
            }

            // Get the popup host
            FrameworkElement popupHost = GetPopupHost(popupAnchor);
            if (popupHost == null)
            {
                return false;
            }

            popupAnchor.IgnoreNextLeftRelease = Mouse.LeftButton == MouseButtonState.Pressed;
            popupAnchor.IgnoreNextRightRelease = Mouse.RightButton == MouseButtonState.Pressed;

            // Reset the close reason
            popupAnchor.LastCloseReason = GamePopupCloseReason.Unknown;

            // Add the popup to the stack
            _popupAnchors.Insert(0, popupAnchor);

            // If the popup is not open...
            if (!popupAnchor.IsPopupOpen)
            {
                // Open the popup
                popupAnchor.IsPopupOpen = true;
            }

            // Attach to events
            popupHost.KeyDown += OnKeyDown;
            popupHost.LostMouseCapture += OnLostMouseCapture;
            popupHost.PreviewKeyDown += OnPreviewKeyDown;

            if (popupAnchor is UIElement popupAnchorElement)
            {
                // If this is the first popup, attach to the root popup's keyboard focus change event
                if (_popupAnchors.Count == 1)
                {
                    popupAnchorElement.IsKeyboardFocusWithinChanged += OnIsKeyboardFocusWithinChanged;
                }

                // Focus the popup
                _ = popupAnchorElement.Dispatcher.BeginInvoke(
                    DispatcherPriority.Input,
                    (Action)
                    delegate
                    {
                        popupAnchorElement.UpdateLayout();

                        if (popupAnchor.Popup == null || !popupAnchor.IsPopupOpen)
                        {
                            return;
                        }

                        UIElement focusableElement = popupAnchor.Popup.Child.FindFirstFocusableDescendant<UIElement>();
                        if (focusableElement is GamePopupContentPresenter)
                        {
                            focusableElement = focusableElement.FindFirstFocusableDescendant<UIElement>() ??
                                               focusableElement;
                        }

                        if (focusableElement != null)
                        {
                            _ = focusableElement.Focus();
                        }

                        _ = Mouse.Capture(popupHost, CaptureMode.SubTree);

                        popupHost.AddHandler(
                            Mouse.PreviewMouseDownOutsideCapturedElementEvent,
                            (MouseButtonEventHandler)OnClickThrough);

                        popupHost.AddHandler(
                            Mouse.PreviewMouseUpOutsideCapturedElementEvent,
                            (MouseButtonEventHandler)OnClickThrough);
                    });
            }

            return true;
        }

        private IGamePopupAnchor RootPopupAnchor => (_popupAnchors.Count > 0) ? _popupAnchors[_popupAnchors.Count - 1] : null;

        internal IGamePopupAnchor TopmostPopupAnchor => (_popupAnchors.Count > 0) ? _popupAnchors[0] : null;

        public static void CloseAllPopups(GamePopupCloseReason closeReason)
        {
            Current.CloseAllPopupsCore(closeReason);
        }

        public static CustomPopupPlacementCallback GetCustomPopupPlacementCallback(DependencyObject o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            return (CustomPopupPlacementCallback)o.GetValue(CustomPopupPlacementCallbackProperty);
        }

        public static void SetCustomPopupPlacementCallback(DependencyObject o, CustomPopupPlacementCallback value)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            o.SetValue(CustomPopupPlacementCallbackProperty, value);
        }

        public static bool GetHasPopup(DependencyObject o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            return (bool)o.GetValue(HasPopupProperty);
        }

        public static void SetHasPopup(DependencyObject o, bool value)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            o.SetValue(HasPopupProperty, value);
        }

        public static bool GetIsPopupOpen(DependencyObject o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            return (bool)o.GetValue(IsPopupOpenProperty);
        }

        public static void SetIsPopupOpen(DependencyObject o, bool value)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            o.SetValue(IsPopupOpenProperty, value);
        }

        public static object GetPopupContent(DependencyObject o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            return o.GetValue(PopupContentProperty);
        }

        public static void SetPopupContent(DependencyObject o, object value)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            o.SetValue(PopupContentProperty, value);
        }

        public static DataTemplate GetPopupContentTemplate(DependencyObject o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            return (DataTemplate)o.GetValue(PopupContentTemplateProperty);
        }

        public static void SetPopupContentTemplate(DependencyObject o, DataTemplate value)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            o.SetValue(PopupContentTemplateProperty, value);
        }

        public static DataTemplateSelector GetPopupContentTemplateSelector(DependencyObject o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            return (DataTemplateSelector)o.GetValue(PopupContentTemplateSelectorProperty);
        }

        public static void SetPopupContentTemplateSelector(DependencyObject o, DataTemplateSelector value)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            o.SetValue(PopupContentTemplateSelectorProperty, value);
        }

        public static double GetPopupHorizontalOffset(DependencyObject o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            return (double)o.GetValue(PopupHorizontalOffsetProperty);
        }

        public static void SetPopupHorizontalOffset(DependencyObject o, double value)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            o.SetValue(PopupHorizontalOffsetProperty, value);
        }

        public static PlacementMode GetPopupPlacement(DependencyObject o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            return (PlacementMode)o.GetValue(PopupPlacementProperty);
        }

        public static void SetPopupPlacement(DependencyObject o, PlacementMode value)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            o.SetValue(PopupPlacementProperty, value);
        }

        public static Rect GetPopupPlacementRectangle(DependencyObject o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            return (Rect)o.GetValue(PopupPlacementRectangleProperty);
        }

        public static void SetPopupPlacementRectangle(DependencyObject o, Rect value)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            o.SetValue(PopupPlacementRectangleProperty, value);
        }

        public static UIElement GetPopupPlacementTarget(DependencyObject o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            return (UIElement)o.GetValue(PopupPlacementTargetProperty);
        }

        public static void SetPopupPlacementTarget(DependencyObject o, UIElement value)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            o.SetValue(PopupPlacementTargetProperty, value);
        }

        public static ControlResizeMode GetPopupResizeMode(DependencyObject o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            return (ControlResizeMode)o.GetValue(PopupResizeModeProperty);
        }

        public static void SetPopupResizeMode(DependencyObject o, ControlResizeMode value)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            o.SetValue(PopupResizeModeProperty, value);
        }

        public static double GetPopupVerticalOffset(DependencyObject o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            return (double)o.GetValue(PopupVerticalOffsetProperty);
        }

        public static void SetPopupVerticalOffset(DependencyObject o, double value)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            o.SetValue(PopupVerticalOffsetProperty, value);
        }

        public static bool GetStaysOpenOnClick(DependencyObject o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            return (bool)o.GetValue(StaysOpenOnClickProperty);
        }

        public static void SetStaysOpenOnClick(DependencyObject o, bool value)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            o.SetValue(StaysOpenOnClickProperty, value);
        }
    }
}