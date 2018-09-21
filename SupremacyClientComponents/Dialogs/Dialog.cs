// DialogBase.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.ServiceLocation;

using System.Linq;
using Supremacy.Utility;

namespace Supremacy.Client.Dialogs
{
    public interface IDialog
    {
        bool? DialogResult { get; set; }
        string Header { get; set; }
        string SubHeader { get; set; }
        bool IsOpen { get; }
        bool IsModal { get; set; }
        bool IsActive { get; }
        bool ShowActivated { get; }
        bool HasBorder { get; }
        void Activate();
        void Show();
        void Close();
    }

    public class CancelRoutedEventArgs : RoutedEventArgs
    {
        public bool Cancel { get; set; }
        public CancelRoutedEventArgs() { }
        public CancelRoutedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
        public CancelRoutedEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source) { }
    }

    public delegate void CancelRoutedEventHandler(object sender, CanExecuteRoutedEventArgs e);

    public class Dialog : ContentControl, IDialog, IWeakEventListener
    {
        public static readonly RoutedCommand SetDialogResultCommand;
        public static readonly RoutedEvent ActivatedEvent;
        public static readonly RoutedEvent DeactivatedEvent;
        public static readonly RoutedEvent ClosingEvent;
        public static readonly RoutedEvent ClosedEvent;

        private static IRegionManager _rootRegionManager;

        private bool _isClosing;
        private bool _showingAsDialog;
        private bool? _dialogResult;

        private bool _settingFocus;
        private bool _setFocusOnContent;
        private bool _isFocusActivating;
        private DispatcherFrame _dispatcherFrame;

        #region Constructors and Finalizers
        static Dialog()
        {
            SetDialogResultCommand = new RoutedCommand("SetDialogResult", typeof(Dialog));

            ActivatedEvent = EventManager.RegisterRoutedEvent(
                "Activated",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(Dialog));

            DeactivatedEvent = EventManager.RegisterRoutedEvent(
                "Deactivated",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(Dialog));

            ClosingEvent = EventManager.RegisterRoutedEvent(
                "Closing",
                RoutingStrategy.Bubble,
                typeof(CancelRoutedEventHandler),
                typeof(Dialog));

            ClosedEvent = EventManager.RegisterRoutedEvent(
                "Closed",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(Dialog));

            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(Dialog),
                new FrameworkPropertyMetadata(typeof(Dialog)));

            HorizontalContentAlignmentProperty.OverrideMetadata(
                typeof(Dialog),
                new FrameworkPropertyMetadata(HorizontalAlignment.Stretch));

            VerticalContentAlignmentProperty.OverrideMetadata(
                typeof(Dialog),
                new FrameworkPropertyMetadata(VerticalAlignment.Stretch));

            HorizontalAlignmentProperty.OverrideMetadata(
                typeof(Dialog),
                new FrameworkPropertyMetadata(HorizontalAlignment.Center));

            VerticalAlignmentProperty.OverrideMetadata(
                typeof(Dialog),
                new FrameworkPropertyMetadata(VerticalAlignment.Center));

            FocusManager.IsFocusScopeProperty.OverrideMetadata(
                typeof(Dialog),
                new FrameworkPropertyMetadata(
                    null,
                    (o, value) => true));

            IsTabStopProperty.OverrideMetadata(
                typeof(Dialog),
                new FrameworkPropertyMetadata(false));

            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(
                typeof(Dialog),
                new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));

            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(
                typeof(Dialog),
                new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));

            KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata(
                typeof(Dialog),
                new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));

            EventManager.RegisterClassHandler(
                typeof(UIElement),
                AccessKeyManager.AccessKeyPressedEvent,
                new AccessKeyPressedEventHandler(OnAccessKeyPressed));

            var dialogCancelCommand = GetDialogCancelCommand();
            if (dialogCancelCommand != null)
            {
                CommandManager.RegisterClassCommandBinding(
                    typeof(Dialog),
                    new CommandBinding(dialogCancelCommand, ExecuteDialogCancelCommand, CanExecuteDialogCancelCommand));
            }
        }

        private static void CanExecuteDialogCancelCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((Dialog)sender).AllowCancel;
        }

        private static void ExecuteDialogCancelCommand(object sender, ExecutedRoutedEventArgs args)
        {
            ((Dialog)sender).OnDialogCancelCommand();
        }

        private static RoutedCommand GetDialogCancelCommand()
        {
            try
            {
                RuntimeHelpers.RunClassConstructor(typeof(Window).TypeHandle);

                var windowDialogCancelCommandField = typeof(Window).GetField(
                    "DialogCancelCommand",
                    BindingFlags.Static | BindingFlags.NonPublic);

                if (windowDialogCancelCommandField == null)
                    return null;

                return windowDialogCancelCommandField.GetValue(null) as RoutedCommand;
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
            }

            return null;
        }

        public Dialog()
        {
            if (!Designer.IsInDesignMode)
            {
                if (_rootRegionManager == null)
                    _rootRegionManager = ServiceLocator.Current.GetInstance<IRegionManager>();
            }

            CommandBindings.Add(
                new CommandBinding(
                    SetDialogResultCommand,
                    (s, a) => DialogResult = (bool?)new NullableBoolConverter().ConvertFrom(a.Parameter),
                    (s, a) => a.CanExecute = _showingAsDialog));

            InputBindings.Add(
                new KeyBinding(
                    GetDialogCancelCommand(),
                    Key.Escape,
                    ModifierKeys.None));

            FocusManager.SetIsFocusScope(this, true);
        }
        #endregion

        #region Events
        public event RoutedEventHandler Activated
        {
            add { AddHandler(ActivatedEvent, value); }
            remove { RemoveHandler(ActivatedEvent, value); }
        }
        #endregion

        #region Properties and Indexers
        protected IRegion DialogRegion { get; private set; }
        protected IRegionManager DialogRegionManager { get; private set; }
        #endregion

        #region Routed Event Handlers
        private static void OnAccessKeyPressed(object sender, AccessKeyPressedEventArgs e)
        {
            if (e.Handled || (e.Scope != null))
                return;

            var dialog = sender as Dialog;

            if (dialog == null)
            {
                var senderElement = sender as DependencyObject;
                if (senderElement != null)
                {
                    var target = _rootRegionManager.Regions[ClientRegions.ModalDialogs].ActiveViews.OfType<Dialog>().FirstOrDefault()
                                 ?? _rootRegionManager.Regions[ClientRegions.ModelessDialogs].ActiveViews.OfType<Dialog>().FirstOrDefault();
                    if (target == null)
                        return;
                    dialog = senderElement.FindVisualAncestorByType<Dialog>();
                    if ((dialog == null) || (dialog != target))
                    {
                        e.Scope = target;
                        e.Handled = true;
                        return;
                    }
                }
            }
            else
            {
                dialog = (Dialog)sender;
            }

            if ((dialog == null) || dialog.IsActive)
                return;

            e.Scope = dialog;
            e.Handled = true;
        }
        #endregion

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (e.NewFocus == this)
            {
                MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                e.Handled = true;
            }
            base.OnGotKeyboardFocus(e);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (!IsOpen)
            {
                base.OnLostKeyboardFocus(e); 
                return;
            }

            var dialogManager = ParentDialogManager;
            if ((dialogManager != null) && (dialogManager.ActiveDialog != this))
            {
                base.OnLostKeyboardFocus(e);
                return;
            }

            var newFocus = e.NewFocus as DependencyObject;
            if (newFocus != null)
            {
                if (newFocus == this ||
                    newFocus.IsVisualDescendantOf(this) ||
                    newFocus.IsLogicalDescendantOf(this))
                {
                    base.OnLostKeyboardFocus(e);
                    return;
                }

                /*
                 * The only time we show more than one dialog at a time is if a modal
                 * dialog is presented over a non-modal dialog.  If focus was transferred
                 * into a modal dialog, then that's fine.
                 */
                var parentDialog = newFocus.FindVisualAncestorByType<Dialog>();
                if (parentDialog != null &&
                    parentDialog.IsActive &&
                    parentDialog.IsModal)
                {
                    base.OnLostKeyboardFocus(e);
                    return;
                }

                var contextMenu = newFocus.FindLogicalAncestorByType<ContextMenu>();
                if (contextMenu != null &&
                    contextMenu.PlacementTarget != null &&
                    (contextMenu.PlacementTarget.IsVisualDescendantOf(this) ||
                     contextMenu.PlacementTarget.IsLogicalDescendantOf(this)))
                {
                    base.OnLostKeyboardFocus(e);
                    return;
                }
            }

            e.Handled = true;

            var oldFocus = e.OldFocus as UIElement;
            if ((oldFocus != null) && oldFocus.IsVisualDescendantOf(this) && oldFocus.IsVisible)
            {
                Keyboard.Focus(e.OldFocus);
            }
            else
            {
                Keyboard.Focus(this);

                var firstFocusableDescendant = this.FindFirstFocusableDescendant(false) as IInputElement;
                if (firstFocusableDescendant != null)
                    Keyboard.Focus(firstFocusableDescendant);
            }
        }

        #region Header (Dependency Property)
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                "Header",
                typeof(string),
                typeof(Dialog),
                new FrameworkPropertyMetadata(string.Empty));

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        #endregion

        #region SubHeader (Dependency Property)
        public static readonly DependencyProperty SubHeaderProperty =
            DependencyProperty.Register(
                "SubHeader",
                typeof(string),
                typeof(Dialog),
                new FrameworkPropertyMetadata(string.Empty));

        public string SubHeader
        {
            get { return (string)GetValue(SubHeaderProperty); }
            set { SetValue(SubHeaderProperty, value); }
        }
        #endregion

        #region ShowActivated (Dependency Property)
        public static readonly DependencyProperty ShowActivatedProperty =
            DependencyProperty.Register(
                "ShowActivated",
                typeof(bool),
                typeof(Dialog),
                new FrameworkPropertyMetadata(true));

        public bool ShowActivated
        {
            get { return (bool)GetValue(ShowActivatedProperty); }
            set { SetValue(ShowActivatedProperty, value); }
        }

        public void Activate()
        {
            if (!IsOpen)
                throw new InvalidOperationException("Cannot activate a dialog that has not been shown.");
            ActivateInternal();
        }

        private void ActivateInternal()
        {
            if (IsActive)
                return;
            DialogRegion.Activate(this);
        }

        public void Show()
        {
            if (_showingAsDialog)
                throw new InvalidOperationException("Cannot call Show() on a dialog that was opened with ShowDialog().");

            if (IsOpen)
            {
                if (ShowActivated)
                    ActivateInternal();
                return;
            }
            ShowInternal();
        }

        private void OnDialogCancelCommand()
        {
            if (_showingAsDialog)
                DialogResult = false;
            else
                Close();
        }

        protected DialogManager ParentDialogManager
        {
            get { return ItemsControl.ItemsControlFromItemContainer(this) as DialogManager; }
        }

        internal bool SetFocus()
        {
            if (_settingFocus)
                return false;

            if (!IsModal && _rootRegionManager.Regions[ClientRegions.ModalDialogs].ActiveViews.Any())
                return false;

            var focusedElement = Keyboard.FocusedElement as Dialog;
            var setFocusOnContent = ((focusedElement == this) || (focusedElement == null)) ||
                                    (focusedElement.ParentDialogManager != ParentDialogManager);

            _settingFocus = true;
            _setFocusOnContent = setFocusOnContent;

            try
            {
                Keyboard.Focus(this);
                return (Focus() || setFocusOnContent);
            }
            finally
            {
                _settingFocus = false;
                _setFocusOnContent = false;
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (((e.Source == this) || !IsActive) && SetFocus())
            {
                e.Handled = true;
            }
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnPreviewGotKeyboardFocus(e);

            if (_isFocusActivating)
                return;

            if ((e.Handled || (e.NewFocus != this)) || (IsActive || (ParentDialogManager == null)))
                return;

            _isFocusActivating = true;

            try
            {
                ActivateInternal();

                if (e.OldFocus != Keyboard.FocusedElement)
                {
                    e.Handled = true;
                }
                else if (_setFocusOnContent)
                {
                    var activeDialogPresenter = ParentDialogManager.ActiveDialogPresenter;
                    if (activeDialogPresenter != null)
                    {
                        ParentDialogManager.UpdateLayout();
                        if (activeDialogPresenter.MoveFocus(new TraversalRequest(FocusNavigationDirection.First)))
                        {
                            e.Handled = true;
                        }
                    }
                }
            }
            finally
            {
                _isFocusActivating = false;
            }
        }

        public bool? ShowDialog()
        {
            if (_showingAsDialog)
            {
                GameLog.Client.General.Error("Cannot call ShowDialog() on a dialog that was already opened with ShowDialog().");
            }

            if (IsOpen)
            {
                GameLog.Client.General.Error("Cannot call ShowDialog() on a dialog that is already open.");
            }

            _showingAsDialog = true;

            try
            {
                ShowInternal();
                ActivateInternal();
                DoShowDialog();
            }
            catch
            {
                _showingAsDialog = false;

                throw;
            }
            finally
            {
                if (Mouse.Captured == this)
                    Mouse.Capture(null);

                _showingAsDialog = false;
            }

            return _dialogResult;
        }

        private void DoShowDialog()
        {
            if (!_showingAsDialog)
                return;

            try
            {
                _dispatcherFrame = new DispatcherFrame();
                Dispatcher.PushFrame(_dispatcherFrame);
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
            }
        }

        private void ShowInternal()
        {
            var regionName = IsModal ? ClientRegions.ModalDialogs : ClientRegions.ModelessDialogs;
            var dialogRegion = _rootRegionManager.Regions[regionName];
            var regionManager = dialogRegion.Add(this, null, true);

            DialogRegion = dialogRegion;
            DialogRegionManager = regionManager;

            _dialogResult = null;

            CollectionChangedEventManager.AddListener(
                DialogRegion.ActiveViews,
                this);

            IsOpen = true;

            if (_showingAsDialog)
                return;

            if (ShowActivated)
                ActivateInternal();
        }

        public void Close()
        {
            if (!IsOpen)
                return;
            CloseInternal(true);
        }

        private void CloseInternal(bool raiseCloseEvents)
        {
            if (!IsOpen || _isClosing)
                return;

            _isClosing = true;

            if (raiseCloseEvents && !OnClosing())
            {
                _isClosing = false;
                return;
            }

            try
            {
                if (_showingAsDialog)
                    DoHideDialog();
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
            }

            IsActive = false;
            IsOpen = false;

            try
            {
                DialogRegion.Remove(this);
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
            }

            CollectionChangedEventManager.RemoveListener(
                DialogRegion.ActiveViews,
                this);

            DialogRegion = null;
            DialogRegionManager = null;

            _isClosing = false;

            if (raiseCloseEvents)
                OnClosed();
        }

        public event CancelRoutedEventHandler Closing
        {
            add { AddHandler(ClosingEvent, value); }
            remove { RemoveHandler(ClosingEvent, value); }
        }

        public event RoutedEventHandler Closed
        {
            add { AddHandler(ClosedEvent, value); }
            remove { RemoveHandler(ClosedEvent, value); }
        }

        protected void OnClosed()
        {
            RaiseEvent(new RoutedEventArgs(ClosedEvent, this));
        }

        protected bool OnClosing()
        {
            var cancelArgs = new CancelRoutedEventArgs(ClosingEvent, this);
            RaiseEvent(cancelArgs);
            return !cancelArgs.Cancel;
        }

        private void DoHideDialog()
        {

            if (_dispatcherFrame != null)
            {
                _dispatcherFrame.Continue = false;
                _dispatcherFrame = null;
            }

            if (!_dialogResult.HasValue)
                _dialogResult = false;

            _showingAsDialog = false;
        }
        #endregion

        #region IsModal (Dependency Property)
        public static readonly DependencyProperty IsModalProperty =
            DependencyProperty.Register(
                "IsModal",
                typeof(bool),
                typeof(Dialog),
                new FrameworkPropertyMetadata(false));

        public bool IsModal
        {
            get { return (bool)GetValue(IsModalProperty); }
            set { SetValue(IsModalProperty, value); }
        }
        #endregion

        #region IsOpen (Dependency Property)
        protected static readonly DependencyPropertyKey IsOpenPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "IsOpen",
                typeof(bool),
                typeof(Dialog),
                new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsOpenProperty = IsOpenPropertyKey.DependencyProperty;

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            protected set { SetValue(IsOpenPropertyKey, value); }
        }
        #endregion

        #region IsActive (Dependency Property)
        protected static readonly DependencyPropertyKey IsActivePropertyKey =
            DependencyProperty.RegisterReadOnly(
                "IsActive",
                typeof(bool),
                typeof(Dialog),
                new FrameworkPropertyMetadata(
                    false,
                    OnIsActiveChanged));

        private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dialog = d as Dialog;

            if (dialog == null)
                return;

            if ((bool)e.NewValue)
                dialog.RaiseEvent(new RoutedEventArgs(ActivatedEvent, dialog));
            else
                dialog.RaiseEvent(new RoutedEventArgs(DeactivatedEvent, dialog));
        }

        public static readonly DependencyProperty IsActiveProperty = IsActivePropertyKey.DependencyProperty;

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            protected internal set { SetValue(IsActivePropertyKey, value); }
        }
        #endregion

        #region HasBorder (Dependency Property)
        public static readonly DependencyProperty HasBorderProperty =
            DependencyProperty.Register(
                "HasBorder",
                typeof(bool),
                typeof(Dialog),
                new FrameworkPropertyMetadata(true));

        public bool HasBorder
        {
            get { return (bool)GetValue(HasBorderProperty); }
            set { SetValue(HasBorderProperty, value); }
        }
        #endregion

        #region AllowCancel (Dependency Property)
        public static readonly DependencyProperty AllowCancelProperty =
            DependencyProperty.Register(
                "AllowCancel",
                typeof(bool),
                typeof(Dialog),
                new FrameworkPropertyMetadata(true));

        public bool AllowCancel
        {
            get { return (bool)GetValue(AllowCancelProperty); }
            set { SetValue(AllowCancelProperty, value); }
        }
        #endregion

        #region DialogResult (Property)
        public bool? DialogResult
        {
            get { return _dialogResult; }
            set
            {
                if (!_showingAsDialog)
                    throw new InvalidOperationException("DialogResult can only be set when the dialog is shown with ShowDialog().");
                if (_dialogResult == value)
                    return;
                _dialogResult = value;
                if (!_isClosing)
                    Close();
            }
        }
        #endregion

        #region IWeakEventListener Implementation
        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!IsOpen)
                return true;

            var dialogRegion = DialogRegion;
            if (dialogRegion == null)
                return true;

            IsActive = dialogRegion.ActiveViews.Contains(this);
            return true;
        }
        #endregion
    }
}