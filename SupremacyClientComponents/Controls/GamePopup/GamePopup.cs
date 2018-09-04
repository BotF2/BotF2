using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

using Supremacy.Client.Markup;

namespace Supremacy.Client.Controls
{
    [ContentProperty("Child")]
    public class GamePopup : Control
    {
        internal static RoutedEventHandler CloseOnUnloadedHandler;

        public static readonly RoutedCommand TogglePopupVisibilityCommand = new RoutedCommand("TogglePopupVisibility", typeof(GamePopup));
        public static readonly RoutedCommand OpenPopupCommand = new RoutedCommand("OpenPopup", typeof(GamePopup));
        public static readonly RoutedCommand ClosePopupCommand = new RoutedCommand("ClosePopup", typeof(GamePopup));

        public static readonly DependencyProperty ChildProperty;
        public static readonly DependencyProperty IsOpenProperty;
        public static readonly DependencyProperty PlacementProperty;
        public static readonly DependencyProperty CustomPopupPlacementCallbackProperty;
        public static readonly DependencyProperty StaysOpenProperty;
        public static readonly DependencyProperty HorizontalOffsetProperty;
        public static readonly DependencyProperty VerticalOffsetProperty;
        public static readonly DependencyProperty PlacementTargetProperty;
        public static readonly DependencyProperty PlacementRectangleProperty;

        private GamePopupRoot _popupRoot;
        private bool _isCaptureHeld;

        #region Constructors and Finalizers

        static GamePopup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(GamePopup),
                new FrameworkPropertyMetadata(typeof(GamePopup)));

            FocusableProperty.OverrideMetadata(
                typeof(GamePopup),
                new FrameworkPropertyMetadata(false));

            IsTabStopProperty.OverrideMetadata(
                typeof(GamePopup),
                new FrameworkPropertyMetadata(false));

            ChildProperty = DependencyProperty.Register(
                "Child",
                typeof(UIElement),
                typeof(GamePopup),
                new FrameworkPropertyMetadata(
                    null,
                    OnChildChanged));

            IsOpenProperty = DependencyProperty.Register(
                "IsOpen",
                typeof(bool),
                typeof(GamePopup),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnIsOpenChanged,
                    CoerceIsOpen));

            PlacementProperty = DependencyProperty.Register(
                "Placement",
                typeof(PlacementMode),
                typeof(GamePopup),
                new FrameworkPropertyMetadata(
                    PlacementMode.Bottom,
                    OnPlacementChanged),
                IsValidPlacementMode);

            CustomPopupPlacementCallbackProperty = DependencyProperty.Register(
                "CustomPopupPlacementCallback",
                typeof(CustomPopupPlacementCallback),
                typeof(GamePopup),
                new FrameworkPropertyMetadata(null));

            StaysOpenProperty = DependencyProperty.Register(
                "StaysOpen",
                typeof(bool),
                typeof(GamePopup),
                new FrameworkPropertyMetadata(
                    true,
                    OnStaysOpenChanged));

            HorizontalOffsetProperty = DependencyProperty.Register(
                "HorizontalOffset",
                typeof(double),
                typeof(GamePopup),
                new FrameworkPropertyMetadata(
                    0.0,
                    OnOffsetChanged));

            VerticalOffsetProperty = DependencyProperty.Register(
                "VerticalOffset",
                typeof(double),
                typeof(GamePopup),
                new FrameworkPropertyMetadata(
                    0.0,
                    OnOffsetChanged));

            PlacementTargetProperty = DependencyProperty.Register(
                "PlacementTarget",
                typeof(UIElement),
                typeof(GamePopup),
                new FrameworkPropertyMetadata(
                    null,
                    OnPlacementTargetChanged));

            PlacementRectangleProperty = DependencyProperty.Register(
                "PlacementRectangle",
                typeof(Rect),
                typeof(GamePopup),
                new FrameworkPropertyMetadata(
                    Rect.Empty,
                    OnOffsetChanged));
        }

        #endregion

        #region Events

        #region Opened Event

        public static RoutedEvent OpenedEvent = EventManager.RegisterRoutedEvent(
            "Opened",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(GamePopup));

        public event RoutedEventHandler Opened
        {
            add { AddHandler(OpenedEvent, value); }
            remove { RemoveHandler(OpenedEvent, value); }
        }

        protected void OnOpened()
        {
            RaiseEvent(new RoutedEventArgs(OpenedEvent, this));
        }

        #endregion

        #region Closed Event

        public static RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent(
            "Closed",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(GamePopup));

        public event RoutedEventHandler Closed
        {
            add { AddHandler(ClosedEvent, value); }
            remove { RemoveHandler(ClosedEvent, value); }
        }

        protected void OnClosed()
        {
            RaiseEvent(new RoutedEventArgs(ClosedEvent, this));
        }

        #endregion

        #endregion

        #region Properties

        internal GamePopupRoot PopupRoot
        {
            get
            {
                EnsurePopupRoot();
                return _popupRoot;
            }
        }

        internal GamePopupSite RegisteredPopupSite { get; set; }
        internal GamePopupCloseReason LastCloseReason { get; set; }

        public UIElement Child
        {
            get { return (UIElement)GetValue(ChildProperty); }
            set { SetValue(ChildProperty, value); }
        }

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        public bool StaysOpen
        {
            get { return (bool)GetValue(StaysOpenProperty); }
            set { SetValue(StaysOpenProperty, value); }
        }

        public double HorizontalOffset
        {
            get { return (double)GetValue(HorizontalOffsetProperty); }
            set { SetValue(HorizontalOffsetProperty, value); }
        }

        public double VerticalOffset
        {
            get { return (double)GetValue(VerticalOffsetProperty); }
            set { SetValue(VerticalOffsetProperty, value); }
        }

        public PlacementMode Placement
        {
            get { return (PlacementMode)GetValue(PlacementProperty); }
            set { SetValue(PlacementProperty, value); }
        }

        public UIElement PlacementTarget
        {
            get { return (UIElement)GetValue(PlacementTargetProperty); }
            set { SetValue(PlacementTargetProperty, value); }
        }

        public Rect PlacementRectangle
        {
            get { return (Rect)GetValue(PlacementRectangleProperty); }
            set { SetValue(PlacementRectangleProperty, value); }
        }

        #endregion

        private static void OnPlacementTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var popup = (GamePopup)d;
            var oldValue = (UIElement)e.OldValue;
            var newValue = (UIElement)e.NewValue;

            if (popup.IsOpen)
            {
                popup.UpdatePlacementTargetRegistration(oldValue, newValue);
            }
            else
            {
                if (oldValue != null)
                    UnregisterPopupFromPlacementTarget(popup, oldValue);
                if (newValue != null)
                    RegisterPopupWithPlacementTarget(popup, newValue);
            }
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            var registeredPopupSite = RegisteredPopupSite;
            if (registeredPopupSite != null)
                registeredPopupSite.Popups.Remove(this);
        }

        private void UpdatePlacementTargetRegistration(UIElement oldValue, UIElement newValue)
        {
            if (oldValue != null)
                UnregisterPopupFromPlacementTarget(this, oldValue);
            if (newValue != null)
                RegisterPopupWithPlacementTarget(this, newValue);
        }

        private static void UnregisterPopupFromPlacementTarget(GamePopup popup, UIElement placementTarget)
        {
            var popupSite = placementTarget.FindVisualAncestorByType<GamePopupSite>();
            if (popupSite == null)
                return;

            popupSite.SizeChanged -= popup.OnPopupSiteSizeChanged;
            popupSite.Popups.Remove(popup);
        }

        private static void RegisterPopupWithPlacementTarget(GamePopup popup, UIElement placementTarget)
        {
            var popupSite = placementTarget.FindVisualAncestorByType<GamePopupSite>();
            if (popupSite == null)
                return;

            popupSite.Popups.Add(popup);
            popupSite.SizeChanged += popup.OnPopupSiteSizeChanged;
        }

        private void OnPopupSiteSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Reposition();
        }

        private static void OnStaysOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var popup = (GamePopup)d;

            if (!popup.IsOpen)
                return;

            if ((bool)e.NewValue)
                popup.ReleasePopupCapture();
            else
                popup.EstablishPopupCapture();
        }

        private void ReleasePopupCapture()
        {
            if (!_isCaptureHeld)
                return;

            if (Mouse.Captured == _popupRoot)
                Mouse.Capture(null);

            _isCaptureHeld = false;
        }

        private void EstablishPopupCapture()
        {
            if (_isCaptureHeld || _popupRoot == null || StaysOpen || Mouse.Captured != null)
                return;

            _isCaptureHeld = Mouse.Capture(_popupRoot, CaptureMode.SubTree);
        }

        private static void OnOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GamePopup)d).Reposition();
        }

        private static void OnPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GamePopup)d).Reposition();
        }

        private static bool IsValidPlacementMode(object o)
        {
            var mode = (PlacementMode)o;
            if (((((mode != PlacementMode.Absolute) && (mode != PlacementMode.AbsolutePoint)) &&
                  ((mode != PlacementMode.Bottom) && (mode != PlacementMode.Center))) &&
                 (((mode != PlacementMode.Mouse) && (mode != PlacementMode.MousePoint)) &&
                  ((mode != PlacementMode.Relative) && (mode != PlacementMode.RelativePoint)))) &&
                (((mode != PlacementMode.Right) && (mode != PlacementMode.Left)) && (mode != PlacementMode.Top)))
            {
                return (mode == PlacementMode.Custom);
            }
            return true;
        }

        private static object CoerceIsOpen(DependencyObject d, object value)
        {
            if ((bool)value)
            {
                var popup = (GamePopup)d;

                if (popup.RegisteredPopupSite == null)
                {
                    RegisterPopupWithPlacementTarget(popup, popup.PlacementTarget ?? popup);

                    if (popup.RegisteredPopupSite == null)
                        return false;
                }

                if (!popup.IsLoaded && VisualTreeHelper.GetParent(popup) != null)
                {
                    popup.RegisterToOpenOnLoad();
                    return false;
                }
            }
            return value;
        }

        private void RegisterToOpenOnLoad()
        {
            Loaded += OpenOnLoad;
        }

        private void OpenOnLoad(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.Input,
                (Action<DependencyProperty>)CoerceValue,
                IsOpenProperty);
        }

        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var popup = (GamePopup)d;
            var newValue = (bool)e.NewValue;
            var popupSite = popup.RegisteredPopupSite;

            if (popupSite == null)
            {
                RegisterPopupWithPlacementTarget(popup, popup);
                popupSite = popup.RegisteredPopupSite;
            }

            if (newValue)
            {
                if (popupSite == null)
                    throw new InvalidOperationException("GamePopup has not been registered with a GamePopupSite.");

                if (CloseOnUnloadedHandler == null)
                    CloseOnUnloadedHandler = CloseOnUnloaded;

                popup.Unloaded += CloseOnUnloadedHandler;

                popup.EnsurePopupRoot();
                popup._popupRoot.Child = popup.Child;
                popupSite.AddCanvasChild(popup._popupRoot);

                if (!popup.StaysOpen)
                    popup.EstablishPopupCapture();

                popup.Reposition();
                popup.OnOpened();
            }
            else
            {
                if (popupSite != null)
                    popupSite.RemoveCanvasChild(popup._popupRoot);

                popup._popupRoot.Child = null;

                if (CloseOnUnloadedHandler != null)
                    popup.Unloaded -= CloseOnUnloadedHandler;

                popup.OnClosed();
                popup.ReleasePopupCapture();
            }
        }

        private void EnsurePopupRoot()
        {
            if (_popupRoot != null)
                return;

            _popupRoot = new GamePopupRoot(this);

            AddLogicalChild(_popupRoot);

            _popupRoot.SetupLayoutBindings(this);

            _popupRoot.PreviewMouseLeftButtonDown += OnPopupRootPreviewMouseButton;
            _popupRoot.PreviewMouseLeftButtonUp += OnPopupRootPreviewMouseButton;
            _popupRoot.PreviewMouseRightButtonDown += OnPopupRootPreviewMouseButton;
            _popupRoot.PreviewMouseRightButtonUp += OnPopupRootPreviewMouseButton;
            _popupRoot.LostMouseCapture += OnPopupRootLostMouseCapture;

            PushTextRenderingMode();
        }

        private void PushTextRenderingMode()
        {
            if (_popupRoot == null || Child == null)
                return;

            var vs = DependencyPropertyHelper.GetValueSource(Child, TextOptions.TextRenderingModeProperty);
            if (vs.BaseValueSource <= BaseValueSource.Inherited)
                TextOptions.SetTextRenderingMode(_popupRoot, TextOptions.GetTextRenderingMode(this));
        }

        private static void CloseOnUnloaded(object sender, RoutedEventArgs e)
        {
            ((GamePopup)sender).IsOpen = false;
        }

        private static void OnChildChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var popup = (GamePopup)d;

            var oldValue = (UIElement)e.OldValue;
            var newValue = (UIElement)e.NewValue;

            if (popup._popupRoot != null && popup.IsOpen)
                popup._popupRoot.Child = newValue;

            if (oldValue != null)
                popup.RemoveLogicalChild(oldValue);

            if (newValue != null)
                popup.AddLogicalChild(newValue);

            popup.Reposition();
            popup.PushTextRenderingMode();
        }

        internal void Reposition()
        {
            if (!IsOpen)
                return;

            if (CheckAccess())
            {
                UpdatePosition();
            }
            else
            {
                Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    (Action)UpdatePosition);
            }
        }

        private void UpdatePosition()
        {
            if (_popupRoot != null)
            {
                _popupRoot.SetPosition(Placement, GetTarget(), new Point(0, 0));
                
                if (IsOpen && RegisteredPopupSite != null)
                    RegisteredPopupSite.InvalidateArrange();
            }
        }

        private UIElement GetTarget()
        {
            return PlacementTarget ??
                   this.FindLogicalAncestorByType<UIElement>() ??
                   this.FindVisualAncestorByType<UIElement>();
        }

        protected override DependencyObject GetUIParentCore()
        {
            // We already know we don't have a visual parent, check logical as well 
            if (Parent == null)
            {
                // Use the placement target as the logical parent while the popup is open
                var placementTarget = PlacementTarget;
                if (placementTarget != null && IsOpen)
                    return placementTarget;
            }

            return base.GetUIParentCore();
        }

        private void OnPopupRootPreviewMouseButton(object sender, MouseButtonEventArgs e)
        {
            // We should only react to mouse buttons if we are in an auto close mode (where we have capture)
            if (!_isCaptureHeld || StaysOpen)
                return;

            Debug.Assert(Mouse.Captured == _popupRoot, "_isCaptureHeld == true, but Mouse.Captured != _popupRoot");

            // If we got a mouse press/release and the mouse isn't on the popup (popup root), dismiss. 
            // When captured to subtree, source will be the captured element for events outside the popup.
            if (_popupRoot == null || e.OriginalSource != _popupRoot)
                return;

            // When we have capture we will get all mouse button up/down messages.
            // We should close if the press was outside.  The MouseButtonEventArgs don't tell whether we get this
            // message because we have capture or if it was legit, so we have to do a hit test. 
            if (_popupRoot.InputHitTest(e.GetPosition(_popupRoot)) != null)
                return;

            // The hit test didn't find any element; that means the click happened outside the popup. 
            SetCurrentValue(IsOpenProperty, KnownBoxes.False);
        }

        private void OnPopupRootLostMouseCapture(object sender, MouseEventArgs e)
        {
            var root = (GamePopupRoot)sender;

            // Try to accomplish "subcapture" -- allowing elements within our
            // subtree to take mouse capture and reclaim it when they lose capture. 
            if (root.Popup.StaysOpen)
                return;

            // Use the same technique employed in ComoboBox.OnLostMouseCapture to allow another control in the
            // application to temporarily take capture and then take it back afterwards. 
            var captured = Mouse.Captured as DependencyObject;
            if (captured == root)
                return;

            if (e.OriginalSource == root)
            {
                if (captured == null || !IsDescendant(root, captured))
                    root.Popup.SetCurrentValue(IsOpenProperty, KnownBoxes.False);
                else
                    _isCaptureHeld = false;
            }
            else
            {
                if (IsDescendant(root, e.OriginalSource as DependencyObject))
                {
                    // Take capture if one of our children gave up capture 
                    if (root.Popup.IsOpen && captured == null)
                    {
                        root.Popup.EstablishPopupCapture();
                        e.Handled = true;
                    }
                }
                else
                {
                    // If Mouse.Captured != null then it's not someone in the subtree underneath us. 
                    // In this case, someone has taken capture and we should close.
                    root.SetCurrentValue(IsOpenProperty, KnownBoxes.False);
                }
            }
        }

        internal static bool IsDescendant(DependencyObject reference, DependencyObject node)
        {
            var success = false;
            var current = node;

            while (current != null)
            {
                if (current == reference)
                {
                    success = true;
                    break;
                }

                // Find popup if current is a GamePopupRoot 
                var popupRoot = current as GamePopupRoot;
                if (popupRoot != null)
                {
                    //Now Popup does not have a visual link to its parent (for context menu) 
                    //it is stored in its parent's arraylist (DP)
                    //so we get its parent by looking at PlacementTarget 
                    var popup = popupRoot.Parent as Popup;

                    current = popup;

                    if (popup != null)
                    {
                        // Try the poup Parent 
                        current = popup.Parent;

                        // Otherwise fall back to placement target 
                        if (current == null)
                            current = popup.PlacementTarget;
                    }
                }
                else // Otherwise walk tree
                {
                    current = FindParent(current);
                }
            }

            return success;
        }

        internal static DependencyObject FindParent(DependencyObject o)
        {
            var visual = (DependencyObject)(o as Visual) ?? o as Visual3D;
            var contentElement = visual == null ? o as ContentElement : null;

            if (contentElement != null)
            {
                o = ContentOperations.GetParent(contentElement);
                
                if (o != null)
                    return o;
                
                var frameworkContentElement = contentElement as FrameworkContentElement;
                if (frameworkContentElement != null)
                    return frameworkContentElement.Parent;

                return null;
            }
            
            if (visual != null)
                return VisualTreeHelper.GetParent(visual);

            return null;
        }
    }
}