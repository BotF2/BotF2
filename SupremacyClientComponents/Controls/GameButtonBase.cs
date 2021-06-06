using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace Supremacy.Client.Controls
{
    public abstract class GameButtonBase : GameControlBase
    {
        private readonly FlagManager _flags = new FlagManager(Flags.CanExecute);

        #region Routed Events
        public static readonly RoutedEvent CheckedEvent = EventManager.RegisterRoutedEvent(
            "Checked",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(GameButtonBase));

        public static readonly RoutedEvent IndeterminateEvent = EventManager.RegisterRoutedEvent(
            "Indeterminate",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(GameButtonBase));

        public static readonly RoutedEvent UncheckedEvent = EventManager.RegisterRoutedEvent(
            "Unchecked",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(GameButtonBase));
        #endregion

        #region Dependency Property Keys
        private static readonly DependencyPropertyKey IsPressedPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "IsPressed",
                typeof(bool),
                typeof(GameButtonBase),
                new FrameworkPropertyMetadata(false));
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty ClickModeProperty = DependencyProperty.Register(
            "ClickMode",
            typeof(ClickMode),
            typeof(GameButtonBase),
            new FrameworkPropertyMetadata(ClickMode.Release));

        public static readonly DependencyProperty HasPopupProperty =
            PopupControlService.HasPopupProperty.AddOwner(
                typeof(GameButtonBase),
                new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty InputGestureTextProperty =
            DependencyProperty.Register(
                "InputGestureText",
                typeof(string),
                typeof(GameButtonBase),
                new FrameworkPropertyMetadata(null, null, CoerceInputGestureTextPropertyValue));

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
            "IsChecked",
            typeof(bool?),
            typeof(GameButtonBase),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsCheckedPropertyValueChanged));

        public static readonly DependencyProperty IsPressedProperty = IsPressedPropertyKey.DependencyProperty;

        public static readonly DependencyProperty MenuItemDescriptionProperty =
            GameControlService.MenuItemDescriptionProperty.AddOwner(
                typeof(GameButtonBase),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty StaysOpenOnClickProperty =
            PopupControlService.StaysOpenOnClickProperty.AddOwner(
                typeof(GameButtonBase),
                new FrameworkPropertyMetadata(false));
        #endregion

        #region Flags Management
        [Flags]
        private enum Flags
        {
            CanExecute = 0x1,
            IsSpaceKeyDown = 0x2,
        }

        private class FlagManager
        {
            private Flags _flags;

            public FlagManager(Flags flags)
            {
                _flags = flags;
            }

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

        [Description("Occurs when the control is checked.")]
        public event RoutedEventHandler Checked
        {
            add { AddHandler(CheckedEvent, value); }
            remove { RemoveHandler(CheckedEvent, value); }
        }

        [Description("Occurs when the checked state of the control is indeterminate.")]
        public event RoutedEventHandler Indeterminate
        {
            add { AddHandler(IndeterminateEvent, value); }
            remove { RemoveHandler(IndeterminateEvent, value); }
        }

        [Description("Occurs when the control is unchecked.")]
        public event RoutedEventHandler Unchecked
        {
            add { AddHandler(UncheckedEvent, value); }
            remove { RemoveHandler(UncheckedEvent, value); }
        }

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static GameButtonBase()
        {
            KeyboardNavigation.AcceptsReturnProperty.OverrideMetadata(
                typeof(GameButtonBase), new FrameworkPropertyMetadata(true));

            EventManager.RegisterClassHandler(typeof(GameButtonBase), AccessKeyManager.AccessKeyPressedEvent, new AccessKeyPressedEventHandler(OnAccessKeyPressed));

        }

        private static void OnAccessKeyPressed(object sender, AccessKeyPressedEventArgs e)
        {
            if ((!e.Handled && (e.Scope == null)) && (e.Target == null))
            {
                e.Target = (UIElement)sender;
            }
        }

        internal bool CanExecute
        {
            get { return _flags.GetFlag(Flags.CanExecute); }
            set
            {
                if (_flags.GetFlag(Flags.CanExecute) == value)
                    return;
                _flags.SetFlag(Flags.CanExecute, value);
                CoerceValue(IsEnabledProperty);
            }
        }

        private static object CoerceInputGestureTextPropertyValue(DependencyObject o, object value)
        {
            GameButtonBase control = (GameButtonBase)o;
            string stringValue = value as string;

            RoutedCommand command;
            if ((string.IsNullOrEmpty(stringValue)) && ((command = control.Command as RoutedCommand) != null))
            {
                if (o.HasDefaultValue(InputGestureTextProperty))
                {
                    InputGestureCollection inputGestures = command.InputGestures;
                    if (inputGestures != null)
                    {
                        KeyGesture keyGesture = inputGestures.OfType<KeyGesture>().FirstOrDefault();
                        if (keyGesture != null)
                            return keyGesture.GetDisplayStringForCulture(CultureInfo.CurrentCulture);
                    }
                }
            }

            return value ?? string.Empty;
        }

        private bool HandleIsMouseOverChanged()
        {
            if (ClickMode != ClickMode.Hover)
                return false;

            if (IsMouseOver)
            {
                IsPressed = true;
                RaiseClickEvent(new ExecuteRoutedEventArgs(ExecuteReason.Mouse));
            }
            else
            {
                IsPressed = false;
            }

            return true;
        }

        internal static bool IsInMainFocusScope(DependencyObject o)
        {
            DependencyObject focusScope = FocusManager.GetFocusScope(o);
            if (focusScope != null)
                return (focusScope.GetVisualParent() == null);
            return true;
        }

        internal bool IsMouseOverBounds(MouseEventArgs e)
        {
            Rect bounds = new Rect(new Point(0, 0), RenderSize);
            return bounds.Contains(e.GetPosition(this));
        }

        private static void OnIsCheckedPropertyValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            GameButtonBase control = (GameButtonBase)o;
            bool? newValue = (bool?)e.NewValue;

            ICheckableCommandParameter parameter = control.CommandParameter as ICheckableCommandParameter;
            if (parameter != null)
                parameter.IsChecked = newValue;

            if (newValue.HasValue && newValue.Value)
                control.OnChecked();
            else if (newValue == false)
                control.OnUnchecked();
            else
                control.OnIndeterminate();
        }

        private void UpdateIsPressed()
        {
            Point point = Mouse.PrimaryDevice.GetPosition(this);
            if (((point.X >= 0) && (point.X <= ActualWidth)) && ((point.Y >= 0) && (point.Y <= ActualHeight)))
            {
                if (!IsPressed)
                    IsPressed = true;
            }
            else if (IsPressed)
            {
                IsPressed = false;
            }
        }

        public ClickMode ClickMode
        {
            get { return (ClickMode)GetValue(ClickModeProperty); }
            set { SetValue(ClickModeProperty, value); }
        }

        public bool HasPopup
        {
            get { return (bool)GetValue(HasPopupProperty); }
            protected set { SetValue(HasPopupProperty, value); }
        }

        public bool? IsChecked
        {
            get { return (bool?)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        [Localizability(LocalizationCategory.None)]
        public string InputGestureText
        {
            get { return (string)GetValue(InputGestureTextProperty); }
            set { SetValue(InputGestureTextProperty, value); }
        }

        protected override bool IsEnabledCore
        {
            get
            {
                if (base.IsEnabledCore)
                    return CanExecute;
                return false;
            }
        }

        public bool IsPressed
        {
            get { return (bool)GetValue(IsPressedProperty); }
            private set { SetValue(IsPressedPropertyKey, value); }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Size desiredSize = base.MeasureOverride(constraint);

            if (VariantSize == VariantSize.Large)
            {
                desiredSize.Width = Math.Ceiling(desiredSize.Width);
                if (desiredSize.Width % 2 == 1)
                    desiredSize.Width++;
            }

            return desiredSize;
        }

        [Localizability(LocalizationCategory.Text)]
        public string MenuItemDescription
        {
            get { return (string)GetValue(MenuItemDescriptionProperty); }
            set { SetValue(MenuItemDescriptionProperty, value); }
        }

        protected virtual void OnChecked()
        {
            RaiseEvent(new RoutedEventArgs(CheckedEvent, this));
        }

        protected override void OnClick(ExecuteRoutedEventArgs e)
        {
            base.OnClick(e);

            if ((ClickMode == ClickMode.Press) && IsMouseCaptured)
                ReleaseMouseCapture();

            if (!Focusable)
                UpdateCanExecute();
        }

        protected override void OnCommandChanged(ICommand oldCommand, ICommand newCommand)
        {
            base.OnCommandChanged(oldCommand, newCommand);

            CoerceValue(InputGestureTextProperty);
        }

        protected virtual void OnIndeterminate()
        {
            RaiseEvent(new RoutedEventArgs(IndeterminateEvent, this));
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (ClickMode == ClickMode.Hover)
                return;

            if (e.Key == Key.Space)
            {
                if (((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Alt)) != ModifierKeys.Alt) &&
                    !IsMouseCaptured &&
                    (e.OriginalSource == this))
                {
                    _flags.SetFlag(Flags.IsSpaceKeyDown, true);

                    IsPressed = true;
                    CaptureMouse();

                    if (ClickMode == ClickMode.Press)
                        RaiseClickEvent(new ExecuteRoutedEventArgs(ExecuteReason.Keyboard));

                    e.Handled = true;
                }
            }
            else if ((e.Key == Key.Return) && ((bool)GetValue(KeyboardNavigation.AcceptsReturnProperty)))
            {
                if (e.OriginalSource == this)
                {
                    _flags.SetFlag(Flags.IsSpaceKeyDown, false);

                    IsPressed = false;
                    if (IsMouseCaptured)
                        ReleaseMouseCapture();

                    RaiseClickEvent(new ExecuteRoutedEventArgs(ExecuteReason.Keyboard));

                    if ((IsKeyboardFocused) && !IsInMainFocusScope(this))
                    {
                        IGamePopupAnchor popupAnchor = PopupControlService.GetParentPopupAnchor(this);
                        if ((popupAnchor == null) ||
                            (popupAnchor == this) ||
                            !popupAnchor.IsPopupOpen ||
                            !StaysOpenOnClick)
                        {
                            GameControl.BlurFocus(this, false);
                        }
                    }

                    e.Handled = true;
                }
            }
            else if (_flags.GetFlag(Flags.IsSpaceKeyDown))
            {
                IsPressed = false;

                _flags.SetFlag(Flags.IsSpaceKeyDown, false);

                if (IsMouseCaptured)
                    ReleaseMouseCapture();
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (ClickMode == ClickMode.Hover)
                return;

            if (((e.Key == Key.Space) &&
                 (_flags.GetFlag(Flags.IsSpaceKeyDown)) &&
                 ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Alt)) != ModifierKeys.Alt)))
            {
                _flags.SetFlag(Flags.IsSpaceKeyDown, false);

                if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Released)
                {
                    bool isReleaseClick = (IsPressed && (ClickMode == ClickMode.Release));
                    if (IsMouseCaptured)
                        ReleaseMouseCapture();
                    if (isReleaseClick)
                        RaiseClickEvent(new ExecuteRoutedEventArgs(ExecuteReason.Keyboard));
                }
                else if (IsMouseCaptured)
                {
                    UpdateIsPressed();
                }

                e.Handled = true;
            }
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);

            if ((ClickMode == ClickMode.Hover) || (e.OriginalSource != this))
                return;

            if (IsPressed)
                IsPressed = false;
            if (IsMouseCaptured)
                ReleaseMouseCapture();

            _flags.SetFlag(Flags.IsSpaceKeyDown, false);
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);

            if ((e.OriginalSource == this) && (ClickMode != ClickMode.Hover) &&
                (!_flags.GetFlag(Flags.IsSpaceKeyDown)))
            {
                if ((IsKeyboardFocused) && (!IsInMainFocusScope(this)))
                {
                    IGamePopupAnchor popupAnchor = PopupControlService.GetParentPopupAnchor(this);
                    if ((popupAnchor == null) ||
                        (popupAnchor == this) ||
                        !popupAnchor.IsPopupOpen ||
                        !StaysOpenOnClick)
                    {
                        GameControl.BlurFocus(this, false);
                    }
                }
                IsPressed = false;
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            if (HandleIsMouseOverChanged())
                e.Handled = true;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if (HandleIsMouseOverChanged())
                e.Handled = true;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if ((e.ChangedButton == MouseButton.Left) && (ClickMode != ClickMode.Hover))
            {
                e.Handled = true;

                Focus();

                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    CaptureMouse();

                    if (IsMouseCaptured)
                    {
                        if (e.ButtonState == MouseButtonState.Pressed)
                        {
                            if ((!IsPressed) && (IsMouseOverBounds(e)))
                                IsPressed = true;
                        }
                        else
                        {
                            ReleaseMouseCapture();
                        }
                    }
                }

                if ((ClickMode == ClickMode.Press) && (IsMouseOverBounds(e)))
                {
                    bool onClickFailed = true;
                    try
                    {
                        RaiseClickEvent(new ExecuteRoutedEventArgs(ExecuteReason.Mouse));
                        onClickFailed = false;
                    }
                    finally
                    {
                        if (onClickFailed)
                        {
                            IsPressed = false;
                            ReleaseMouseCapture();
                        }
                    }
                }
            }

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if ((e.ChangedButton == MouseButton.Left) && (ClickMode != ClickMode.Hover))
            {
                e.Handled = true;

                bool isReleaseClick = !_flags.GetFlag(Flags.IsSpaceKeyDown) &&
                                     IsPressed &&
                                     (ClickMode == ClickMode.Release);

                if ((IsMouseCaptured) && (!_flags.GetFlag(Flags.IsSpaceKeyDown)))
                    ReleaseMouseCapture();
                if (isReleaseClick)
                    RaiseClickEvent(new ExecuteRoutedEventArgs(ExecuteReason.Mouse));
            }

            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if ((ClickMode == ClickMode.Hover) ||
                !IsMouseCaptured ||
                (Mouse.PrimaryDevice.LeftButton != MouseButtonState.Pressed) ||
                _flags.GetFlag(Flags.IsSpaceKeyDown))
            {
                return;
            }

            UpdateIsPressed();
            e.Handled = true;
        }

        protected override void OnPreviewClick(ExecuteRoutedEventArgs e)
        {
            IGamePopupAnchor popupAnchor = PopupControlService.GetParentPopupAnchor(this);

            if ((popupAnchor != null) && (popupAnchor.IsPopupOpen && !StaysOpenOnClick))
                PopupControlService.CloseAllPopups(GamePopupCloseReason.ControlClick);

            base.OnPreviewClick(e);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (IsMouseCaptured &&
                (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed) &&
                !_flags.GetFlag(Flags.IsSpaceKeyDown))
            {
                UpdateIsPressed();
            }
        }

        protected virtual void OnUnchecked()
        {
            RaiseEvent(new RoutedEventArgs(UncheckedEvent, this));
        }

        public bool StaysOpenOnClick
        {
            get { return (bool)GetValue(StaysOpenOnClickProperty); }
            set { SetValue(StaysOpenOnClickProperty, value); }
        }

        protected override void UpdateCanExecute()
        {
            if (Command != null)
                CanExecute = GameCommand.CanExecuteCommandSource(this);
            else
                CanExecute = true;

            ICheckableCommandParameter checkableParameter = CommandParameter as ICheckableCommandParameter;
            if ((checkableParameter == null) || !checkableParameter.Handled)
                return;

            IsChecked = checkableParameter.IsChecked;

            checkableParameter.Handled = false;
        }
    }
}