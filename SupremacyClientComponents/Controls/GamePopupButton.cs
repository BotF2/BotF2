using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace Supremacy.Client.Controls
{
    /*
    [TemplatePart(Name = "PART_Popup", Type = typeof(GamePopup))]
    [TemplatePart(Name = "PART_PopupResizeGrip", Type = typeof(Thumb))]
    public class GamePopupButton : GameButtonBase, IGamePopupAnchor
    {
        public static readonly DependencyProperty IsPopupOpenProperty = PopupControlService.IsPopupOpenProperty.AddOwner(typeof(GamePopupButton));

        private GamePopup _popup;
        private Thumb _resizeGrip;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _popup = GetTemplateChild("PART_Popup") as GamePopup;
            _resizeGrip = GetTemplateChild("PART_PopupResizeGrip") as Thumb;

            this.HasPopup = _popup != null;
        }

        protected override void OnClick(ExecuteRoutedEventArgs e)
        {
            if (e.Handled)
                return;

            this.IsPopupOpen = !this.IsPopupOpen;
        }

        #region Implementation of IGamePopupAnchor

        bool IGamePopupAnchor.IgnoreNextLeftRelease { get; set; }

        bool IGamePopupAnchor.IgnoreNextRightRelease { get; set; }

        public bool IsPopupOpen
        {
            get { return (bool)GetValue(IsPopupOpenProperty); }
            set { SetValue(IsPopupOpenProperty, value); }
        }

        GamePopupCloseReason IGamePopupAnchor.LastCloseReason { get; set; }

        public void OnPopupClosed()
        {
        }

        public void OnPopupOpened()
        {
        }

        public bool OnPopupOpening()
        {
            return true;
        }

        public GamePopup Popup
        {
            get { return _popup; }
        }

        bool IGamePopupAnchor.PopupOpenedWithMouse { get; set; }

        Thumb IGamePopupAnchor.ResizeGrip
        {
            get { return _resizeGrip; }
        }

        #endregion
    }
*/

    [ContentProperty("PopupContent")]
    [TemplatePart(Name = PopupPartName, Type = typeof(Popup))]
    [TemplatePart(Name = ResizeGripPartName, Type = typeof(Thumb))]
    public abstract class PopupButtonBase : GameButtonBase, IGamePopupAnchor
    {
        private const string PopupPartName = "PART_Popup";
        private const string ResizeGripPartName = "PART_ResizeGrip";

        private readonly FlagManager _flags = new FlagManager();
        private GamePopupCloseReason _popupLastCloseReason = GamePopupCloseReason.Unknown;

        #region Routed Events

        public static readonly RoutedEvent PopupClosedEvent = PopupControlService.PopupClosedEvent.AddOwner(typeof(PopupButtonBase));
        public static readonly RoutedEvent PopupOpenedEvent = PopupControlService.PopupOpenedEvent.AddOwner(typeof(PopupButtonBase));
        public static readonly RoutedEvent PopupOpeningEvent = PopupControlService.PopupOpeningEvent.AddOwner(typeof(PopupButtonBase));

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty AutoDisableWhenPopupContentIsDisabledProperty = DependencyProperty.Register(
            "AutoDisableWhenPopupContentIsDisabled", typeof(bool), typeof(PopupButtonBase), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty CustomPopupPlacementCallbackProperty = PopupControlService.CustomPopupPlacementCallbackProperty.AddOwner(typeof(PopupButtonBase));

        public static readonly DependencyProperty IsPopupOpenProperty = PopupControlService.IsPopupOpenProperty.AddOwner(
            typeof(PopupButtonBase), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty PopupContentProperty = PopupControlService.PopupContentProperty.AddOwner(typeof(PopupButtonBase));

        public static readonly DependencyProperty PopupContentTemplateProperty = PopupControlService.PopupContentTemplateProperty.AddOwner(typeof(PopupButtonBase));

        public static readonly DependencyProperty PopupContentTemplateSelectorProperty = PopupControlService.PopupContentTemplateSelectorProperty.AddOwner(typeof(PopupButtonBase));

        public static readonly DependencyProperty PopupHasBorderProperty = PopupControlService.PopupHasBorderProperty.AddOwner(typeof(PopupButtonBase));

        public static readonly DependencyProperty PopupHorizontalOffsetProperty = PopupControlService.PopupHorizontalOffsetProperty.AddOwner(typeof(PopupButtonBase));

        public static readonly DependencyProperty PopupPlacementProperty = PopupControlService.PopupPlacementProperty.AddOwner(
            typeof(PopupButtonBase), new FrameworkPropertyMetadata(PlacementMode.Bottom));

        public static readonly DependencyProperty PopupPlacementRectangleProperty = PopupControlService.PopupPlacementRectangleProperty.AddOwner(typeof(PopupButtonBase));

        public static readonly DependencyProperty PopupPlacementTargetProperty = PopupControlService.PopupPlacementTargetProperty.AddOwner(typeof(PopupButtonBase));

        public static readonly DependencyProperty PopupResizeModeProperty = PopupControlService.PopupResizeModeProperty.AddOwner(typeof(PopupButtonBase));

        public static readonly DependencyProperty PopupVerticalOffsetProperty = PopupControlService.PopupVerticalOffsetProperty.AddOwner(typeof(PopupButtonBase));

        #endregion

        #region Flags Management

        [Flags]
        private enum Flags
        {
            PopupIgnoreNextLeftRelease = 0x20,
            PopupIgnoreNextRightRelease = 0x40,
            PopupOpenedWithMouse = 0x80,
        }

        private class FlagManager
        {
            private Flags _flags;

            internal bool GetFlag(Flags flag)
            {
                return ((_flags & flag) == flag);
            }

            internal void SetFlag(Flags flag, bool set)
            {
                if (set)
                    _flags |= flag;
                else
                    _flags &= (~flag);
            }
        }

        #endregion

        [Description("Occurs when the value of the IsPopupOpen property changes.")]
        public event RoutedEventHandler PopupClosed
        {
            add { AddHandler(PopupClosedEvent, value); }
            remove { RemoveHandler(PopupClosedEvent, value); }
        }

        [Description("Occurs when the value of the IsPopupOpen property changes.")]
        public event RoutedEventHandler PopupOpened
        {
            add { AddHandler(PopupOpenedEvent, value); }
            remove { RemoveHandler(PopupOpenedEvent, value); }
        }

        [Description("Occurs before the value of the IsPopupOpen property changes.")]
        public event EventHandler<CancelRoutedEventArgs> PopupOpening
        {
            add { AddHandler(PopupOpeningEvent, value); }
            remove { RemoveHandler(PopupOpeningEvent, value); }
        }

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static PopupButtonBase()
        {
            PopupControlService.HasPopupProperty.OverrideMetadata(typeof(PopupButtonBase), new FrameworkPropertyMetadata(true));
        }

        #region IPopupAnchor Implementation

        bool IGamePopupAnchor.IgnoreNextLeftRelease
        {
            get { return _flags.GetFlag(Flags.PopupIgnoreNextLeftRelease); }
            set { _flags.SetFlag(Flags.PopupIgnoreNextLeftRelease, value); }
        }

        bool IGamePopupAnchor.IgnoreNextRightRelease
        {
            get { return _flags.GetFlag(Flags.PopupIgnoreNextRightRelease); }
            set { _flags.SetFlag(Flags.PopupIgnoreNextRightRelease, value); }
        }

        GamePopupCloseReason IGamePopupAnchor.LastCloseReason
        {
            get { return _popupLastCloseReason; }
            set { _popupLastCloseReason = value; }
        }

        void IGamePopupAnchor.OnPopupClosed()
        {
            OnPopupClosed();
        }

        void IGamePopupAnchor.OnPopupOpened()
        {
            OnPopupOpened();
        }

        bool IGamePopupAnchor.OnPopupOpening()
        {
            return OnPopupOpening();
        }

        GamePopup IGamePopupAnchor.Popup
        {
            get { return (GamePopup)GetTemplateChild(PopupPartName); }
        }

        bool IGamePopupAnchor.PopupOpenedWithMouse
        {
            get { return _flags.GetFlag(Flags.PopupOpenedWithMouse); }
            set { _flags.SetFlag(Flags.PopupOpenedWithMouse, value); }
        }

        Thumb IGamePopupAnchor.ResizeGrip
        {
            get { return (Thumb)GetTemplateChild(ResizeGripPartName); }
        }

        #endregion

        // NON-PUBLIC PROCEDURES

        public bool AutoDisableWhenPopupContentIsDisabled
        {
            get { return (bool)GetValue(AutoDisableWhenPopupContentIsDisabledProperty); }
            set { SetValue(AutoDisableWhenPopupContentIsDisabledProperty, value); }
        }

        [Bindable(false)]
        public CustomPopupPlacementCallback CustomPopupPlacementCallback
        {
            get { return (CustomPopupPlacementCallback)GetValue(CustomPopupPlacementCallbackProperty); }
            set { SetValue(CustomPopupPlacementCallbackProperty, value); }
        }

        public bool IsPopupOpen
        {
            get { return (bool)GetValue(IsPopupOpenProperty); }
            set { SetValue(IsPopupOpenProperty, value); }
        }

        protected override IEnumerator LogicalChildren
        {
            get
            {
                IList children = new List<object>();

                var enumerator = base.LogicalChildren;
                if (enumerator != null)
                {
                    enumerator.Reset();
                    while (enumerator.MoveNext())
                        children.Add(enumerator.Current);
                }

                if (PopupContent is DependencyObject)
                    children.Add(PopupContent);

                if (children.Count > 0)
                    return children.GetEnumerator();

                return null;
            }
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            // Don't allow the popup to close if focus is still in the popup (prevents popup from closing when non-focusable popup content is clicked)
            // Originally added in build 480 with just IsKeyboardFocusWithin check as:
            //   "Fixed a bug where popup button popups may close when clicking on a non-focusable control in them"
            // But there were problems with SplitButtons retaining focus when clicked discovered in later builds, so further conditions added in build 485
            if ((IsKeyboardFocusWithin) && (IsPopupOpen))
            {
                var popup = ((IGamePopupAnchor)this).Popup;
                if (popup != null && popup.Child != null)
                {
                    var hitTarget = popup.Child.InputHitTest(e.GetPosition(popup.Child));
                    // If the mouse is over the popup's child control, don't call the base method
                    if (hitTarget != null)
                        return;
                }
            }

            // Call the base method
            base.OnLostMouseCapture(e);
        }

        protected virtual void OnPopupClosed()
        {
            RaiseEvent(new RoutedEventArgs(PopupClosedEvent, this));
        }

        protected virtual void OnPopupOpened()
        {
            RaiseEvent(new RoutedEventArgs(PopupOpenedEvent, this));
        }

        protected virtual bool OnPopupOpening()
        {
            var e = new CancelRoutedEventArgs(PopupOpeningEvent, this);
            RaiseEvent(e);
            return !e.Cancel;
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            // Call the base method
            base.OnVisualParentChanged(oldParent);

            // Coerce values again
            CoerceValue(PopupPlacementProperty);
            CoerceValue(PopupPlacementTargetProperty);
            CoerceValue(PopupVerticalOffsetProperty);
        }

        public object PopupContent
        {
            get { return GetValue(PopupContentProperty); }
            set { SetValue(PopupContentProperty, value); }
        }

        public DataTemplate PopupContentTemplate
        {
            get { return (DataTemplate)GetValue(PopupContentTemplateProperty); }
            set { SetValue(PopupContentTemplateProperty, value); }
        }

        public DataTemplateSelector PopupContentTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(PopupContentTemplateSelectorProperty); }
            set { SetValue(PopupContentTemplateSelectorProperty, value); }
        }

        public bool PopupHasBorder
        {
            get { return (bool)GetValue(PopupHasBorderProperty); }
            set { SetValue(PopupHasBorderProperty, value); }
        }

        [TypeConverter(typeof(LengthConverter))]
        public double PopupHorizontalOffset
        {
            get { return (double)GetValue(PopupHorizontalOffsetProperty); }
            set { SetValue(PopupHorizontalOffsetProperty, value); }
        }

        public PlacementMode PopupPlacement
        {
            get { return (PlacementMode)GetValue(PopupPlacementProperty); }
            set { SetValue(PopupPlacementProperty, value); }
        }

        public Rect PopupPlacementRectangle
        {
            get { return (Rect)GetValue(PopupPlacementRectangleProperty); }
            set { SetValue(PopupPlacementRectangleProperty, value); }
        }

        public UIElement PopupPlacementTarget
        {
            get { return (UIElement)GetValue(PopupPlacementTargetProperty); }
            set { SetValue(PopupPlacementTargetProperty, value); }
        }

        public ControlResizeMode PopupResizeMode
        {
            get { return (ControlResizeMode)GetValue(PopupResizeModeProperty); }
            set { SetValue(PopupResizeModeProperty, value); }
        }

        [TypeConverter(typeof(LengthConverter))]
        public double PopupVerticalOffset
        {
            get { return (double)GetValue(PopupVerticalOffsetProperty); }
            set { SetValue(PopupVerticalOffsetProperty, value); }
        }
    }

    public class GamePopupButton : PopupButtonBase
    {
        private readonly FlagManager _flags = new FlagManager();

        // NESTED TYPES

        #region Flags Management

        [Flags]
        private enum Flags
        {
            IgnoreNextMouseDown = 0x1,

            ForceStaysOpenOnClick = 0x2,
        }

        private class FlagManager
        {
            private Flags _flags;

            internal bool GetFlag(Flags flag)
            {
                return ((_flags & flag) == flag);
            }

            internal void SetFlag(Flags flag, bool set)
            {
                if (set)
                    _flags |= flag;
                else
                    _flags &= (~flag);
            }
        }

        #endregion

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static GamePopupButton()
        {
            ClickModeProperty.OverrideMetadata(typeof(GamePopupButton), new FrameworkPropertyMetadata(ClickMode.Press));
            StaysOpenOnClickProperty.OverrideMetadata(typeof(GamePopupButton), new FrameworkPropertyMetadata(false, null, CoerceStaysOpenOnClickPropertyValue));
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GamePopupButton), new FrameworkPropertyMetadata(typeof(GamePopupButton)));
        }

        private static object CoerceStaysOpenOnClickPropertyValue(DependencyObject obj, object value)
        {
            var control = (GamePopupButton)obj;
            if (control._flags.GetFlag(Flags.ForceStaysOpenOnClick))
                return true;
            return value;
        }

        protected override void OnClick(ExecuteRoutedEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            // Ignore the base code so that a command is not raised

            if (IsPopupOpen)
            {
                // Only close the popup if not a menu item
                if (Context != GameControlContext.MenuItem)
                    IsPopupOpen = false;
            }
            else
            {
                // Fire the command
                GameCommand.ExecuteCommandSource(this);

                // Flag if the popup is being opened with the mouse
                ((IGamePopupAnchor)this).PopupOpenedWithMouse = (e.Reason == ExecuteReason.Mouse);

                // Open the popup
                IsPopupOpen = true;
            }
        }

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            // Call the base method
            base.OnIsKeyboardFocusWithinChanged(e);

            if (e.NewValue.Equals(false))
            {
                // Flag to not ignore the next mouse down (see notes where this flag is set for an explanation of why it is done)
                _flags.SetFlag(Flags.IgnoreNextMouseDown, false);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            // If ignoring the next mouse down, eat the mouse down
            var ignoreMouseDown = _flags.GetFlag(Flags.IgnoreNextMouseDown);
            _flags.SetFlag(Flags.IgnoreNextMouseDown, false);

            if (!e.Handled && IsMouseOver)
            {
                // If there is a popup open, see if the click was on a disabled menu item and if so, handle the mouse down
                if (!ignoreMouseDown && IsPopupOpen && !IsMouseDirectlyOver && e.Source is GameMenu)
                {
                    var presenter = e.OriginalSource as ContentPresenter;
                    if (presenter != null && VisualTreeHelper.GetChildrenCount(presenter) > 0)
                    {
                        var presenterContent = VisualTreeHelper.GetChild(presenter, 0) as UIElement;
                        if (presenterContent != null && !presenterContent.IsEnabled)
                            ignoreMouseDown = true;
                    }
                }

                if (ignoreMouseDown)
                {
                    e.Handled = true;
                    return;
                }
            }

            // Call the base method
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            // Temporarily force the StaysOpenOnClick property to true to prevent keyboard loss of focus
            _flags.SetFlag(Flags.ForceStaysOpenOnClick, true);
            try
            {
                CoerceValue(StaysOpenOnClickProperty);
                base.OnMouseLeftButtonUp(e);
            }
            finally
            {
                _flags.SetFlag(Flags.ForceStaysOpenOnClick, false);
                CoerceValue(StaysOpenOnClickProperty);
            }
        }

        protected override void OnPopupClosed()
        {
            // Added IsKeyboardFocusWithin check below to work around issue where IgnoreNextMouseDown would not be reset.
            if (((IGamePopupAnchor)this).LastCloseReason == GamePopupCloseReason.ClickThrough && IsKeyboardFocusWithin)
            {
                // Flag to ignore the next mouse down since the mouse was clicked somewhere...
                //   If the mouse was clicked outside of this control hierarchy, the lose focus code will reset this flag to false
                _flags.SetFlag(Flags.IgnoreNextMouseDown, true);
            }

            // Call the base method
            base.OnPopupClosed();
        }

        protected override void OnPreviewClick(ExecuteRoutedEventArgs e)
        {
            // Don't raise an event, otherwise ContextMenu parents will close
        }

        protected override void UpdateCanExecute()
        {
            var commandCanExecute = Command == null || Command == ApplicationCommands.NotACommand || GameCommand.CanExecuteCommandSource(this);
            CanExecute = commandCanExecute && (!AutoDisableWhenPopupContentIsDisabled || (PopupControlService.IsPopupAnchorPopupEnabled(this)));
        }
    }
}