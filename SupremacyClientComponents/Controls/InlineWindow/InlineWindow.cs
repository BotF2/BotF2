using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Supremacy.Utility;

namespace Supremacy.Client.Controls
{
    /// <summary>
    /// Represents a <see cref="ContentControl"/> that renders like a normal <see cref="Window"/> however can be used in XBAP or as 
    /// a container for MDI.
    /// </summary>
    [TemplatePart(Name = IconPartName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = TitleBarPartName, Type = typeof(FrameworkElement))]
    public class InlineWindow : ContentControl
    {
        private ResizeOperation _currentResizeOperation = ResizeOperation.None;
        private Point _dragMouseOffset;
        private Size _dragSize;
        private bool _isMoving;
        private FrameworkElement _icon;
        private FrameworkElement _titleBar;

        #region Commands
        public static readonly RoutedCommand CloseCommand = new RoutedCommand("Close", typeof(InlineWindow));
        public static readonly RoutedCommand MinimizeCommand = new RoutedCommand("Minimize", typeof(InlineWindow));
        public static readonly RoutedCommand MaximizeCommand = new RoutedCommand("Maximize", typeof(InlineWindow));
        public static readonly RoutedCommand RestoreCommand = new RoutedCommand("Restore", typeof(InlineWindow));
        #endregion

        #region Routed Events
        /// <summary>
        /// Identifies the <see cref="Activated"/> routed event.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="Activated"/> routed event.</value>
        public static readonly RoutedEvent ActivatedEvent = EventManager.RegisterRoutedEvent(
            "Activated", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InlineWindow));

        /// <summary>
        /// Identifies the <see cref="Closed"/> routed event.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="Closed"/> routed event.</value>
        public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent(
            "Closed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InlineWindow));

        /// <summary>
        /// Identifies the <see cref="Closing"/> routed event.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="Closing"/> routed event.</value>
        public static readonly RoutedEvent ClosingEvent = EventManager.RegisterRoutedEvent(
            "Closing", RoutingStrategy.Bubble, typeof(EventHandler<CancelRoutedEventArgs>), typeof(InlineWindow));

        /// <summary>
        /// Identifies the <see cref="Deactivated"/> routed event.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="Deactivated"/> routed event.</value>
        public static readonly RoutedEvent DeactivatedEvent = EventManager.RegisterRoutedEvent(
            "Deactivated", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InlineWindow));

        /// <summary>
        /// Identifies the <see cref="DragMoved"/> routed event.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="DragMoved"/> routed event.</value>
        public static readonly RoutedEvent DragMovedEvent = EventManager.RegisterRoutedEvent(
            "DragMoved", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InlineWindow));

        /// <summary>
        /// Identifies the <see cref="DragMoving"/> routed event.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="DragMoving"/> routed event.</value>
        public static readonly RoutedEvent DragMovingEvent = EventManager.RegisterRoutedEvent(
            "DragMoving", RoutingStrategy.Bubble, typeof(EventHandler<CancelRoutedEventArgs>), typeof(InlineWindow));

        /// <summary>
        /// Identifies the <see cref="LocationChanged"/> routed event.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="LocationChanged"/> routed event.</value>
        public static readonly RoutedEvent LocationChangedEvent = EventManager.RegisterRoutedEvent(
            "LocationChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InlineWindow));

        /// <summary>
        /// Identifies the <see cref="Opened"/> routed event.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="Opened"/> routed event.</value>
        public static readonly RoutedEvent OpenedEvent = EventManager.RegisterRoutedEvent(
            "Opened", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InlineWindow));

        /// <summary>
        /// Identifies the <see cref="StateChanged"/> routed event.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="StateChanged"/> routed event.</value>
        public static readonly RoutedEvent StateChangedEvent = EventManager.RegisterRoutedEvent(
            "StateChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InlineWindow));

        /// <summary>
        /// Identifies the <see cref="TitleBarContextMenuOpening"/> routed event.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="TitleBarContextMenuOpening"/> routed event.</value>
        public static readonly RoutedEvent TitleBarContextMenuOpeningEvent =
            EventManager.RegisterRoutedEvent(
                "TitleBarContextMenuOpening",
                RoutingStrategy.Bubble,
                typeof(EventHandler<ContextMenuItemRoutedEventArgs>),
                typeof(InlineWindow));

        /// <summary>
        /// Identifies the <see cref="TitleBarDoubleClick"/> routed event.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="TitleBarDoubleClick"/> routed event.</value>
        public static readonly RoutedEvent TitleBarDoubleClickEvent =
            EventManager.RegisterRoutedEvent(
                "TitleBarDoubleClick",
                RoutingStrategy.Bubble,
                typeof(EventHandler<CancelRoutedEventArgs>),
                typeof(InlineWindow));
        #endregion

        #region Dependency Property Keys
        private static readonly DependencyPropertyKey IsActivePropertyKey =
            DependencyProperty.RegisterReadOnly(
                "IsActive",
                typeof(bool),
                typeof(InlineWindow),
                new FrameworkPropertyMetadata(false, OnIsActivePropertyChanged));
        #endregion

        #region Dependency Properties
        /// <summary>
        /// Identifies the <see cref="CanClose"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="CanClose"/> dependency property.</value>
        public static readonly DependencyProperty CanCloseProperty = DependencyProperty.Register(
            "CanClose", typeof(bool), typeof(InlineWindow), new FrameworkPropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="CornerRadius"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="CornerRadius"/> dependency property.</value>
        public static readonly DependencyProperty CornerRadiusProperty =
            Border.CornerRadiusProperty.AddOwner(
                typeof(InlineWindow), new FrameworkPropertyMetadata(new CornerRadius(0)));

        /// <summary>
        /// Identifies the <see cref="DropShadowZOffset"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="DropShadowZOffset"/> dependency property.</value>
        public static readonly DependencyProperty DropShadowZOffsetProperty =
            DependencyProperty.Register(
                "DropShadowZOffset", typeof(double), typeof(InlineWindow), new FrameworkPropertyMetadata(6.0));

        /// <summary>
        /// Identifies the <see cref="DropShadowColor"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="DropShadowColor"/> dependency property.</value>
        public static readonly DependencyProperty DropShadowColorProperty =
            DependencyProperty.Register(
                "DropShadowColor",
                typeof(Color),
                typeof(InlineWindow),
                new FrameworkPropertyMetadata(Color.FromArgb(0x71, 0x0, 0x0, 0x0)));

        /// <summary>
        /// Identifies the <see cref="HasCloseButton"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="HasCloseButton"/> dependency property.</value>
        public static readonly DependencyProperty HasCloseButtonProperty = DependencyProperty.Register(
            "HasCloseButton", typeof(bool), typeof(InlineWindow), new FrameworkPropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="HasDropShadow"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="HasDropShadow"/> dependency property.</value>
        public static readonly DependencyProperty HasDropShadowProperty = DependencyProperty.Register(
            "HasDropShadow", typeof(bool), typeof(InlineWindow), new FrameworkPropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="HasIcon"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="HasIcon"/> dependency property.</value>
        public static readonly DependencyProperty HasIconProperty = DependencyProperty.Register(
            "HasIcon", typeof(bool), typeof(InlineWindow), new FrameworkPropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="HasMaximizeButton"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="HasMaximizeButton"/> dependency property.</value>
        public static readonly DependencyProperty HasMaximizeButtonProperty =
            DependencyProperty.Register(
                "HasMaximizeButton", typeof(bool), typeof(InlineWindow), new FrameworkPropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="HasMinimizeButton"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="HasMinimizeButton"/> dependency property.</value>
        public static readonly DependencyProperty HasMinimizeButtonProperty =
            DependencyProperty.Register(
                "HasMinimizeButton", typeof(bool), typeof(InlineWindow), new FrameworkPropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="HasRestoreButton"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="HasRestoreButton"/> dependency property.</value>
        public static readonly DependencyProperty HasRestoreButtonProperty =
            DependencyProperty.Register(
                "HasRestoreButton", typeof(bool), typeof(InlineWindow), new FrameworkPropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="Icon"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="Icon"/> dependency property.</value>
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon", typeof(ImageSource), typeof(InlineWindow), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="IsActive"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="IsActive"/> dependency property.</value>
        public static readonly DependencyProperty IsActiveProperty = IsActivePropertyKey.DependencyProperty;

        /// <summary>
        /// Identifies the <see cref="IsTitleBarTextShadowEnabled"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="IsTitleBarTextShadowEnabled"/> dependency property.</value>
        public static readonly DependencyProperty IsTitleBarTextShadowEnabledProperty =
            DependencyProperty.Register(
                "IsTitleBarTextShadowEnabled", typeof(bool), typeof(InlineWindow), new FrameworkPropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="Left"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="Left"/> dependency property.</value>
        public static readonly DependencyProperty LeftProperty = DependencyProperty.Register(
            "Left", typeof(double), typeof(InlineWindow), new FrameworkPropertyMetadata(0.0, OnLeftPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="RestoreBounds"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="RestoreBounds"/> dependency property.</value>
        public static readonly DependencyProperty RestoreBoundsProperty = DependencyProperty.Register(
            "RestoreBounds", typeof(Rect), typeof(InlineWindow), new FrameworkPropertyMetadata(Rect.Empty));

        /// <summary>
        /// Identifies the <see cref="ResizeMode"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="ResizeMode"/> dependency property.</value>
        public static readonly DependencyProperty ResizeModeProperty = DependencyProperty.Register(
            "ResizeMode",
            typeof(ResizeMode),
            typeof(InlineWindow),
            new FrameworkPropertyMetadata(ResizeMode.CanResize));

        /// <summary>
        /// Identifies the <see cref="Title"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="Title"/> dependency property.</value>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(InlineWindow), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="TitleBarFontWeight"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="TitleBarFontWeight"/> dependency property.</value>
        public static readonly DependencyProperty TitleBarFontWeightProperty =
            DependencyProperty.Register(
                "TitleBarFontWeight",
                typeof(FontWeight),
                typeof(InlineWindow),
                new FrameworkPropertyMetadata(FontWeights.Normal));

        /// <summary>
        /// Identifies the <see cref="Top"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="Top"/> dependency property.</value>
        public static readonly DependencyProperty TopProperty = DependencyProperty.Register(
            "Top", typeof(double), typeof(InlineWindow), new FrameworkPropertyMetadata(0.0, OnTopPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="WindowState"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="WindowState"/> dependency property.</value>
        public static readonly DependencyProperty WindowStateProperty = DependencyProperty.Register(
            "WindowState",
            typeof(WindowState),
            typeof(InlineWindow),
            new FrameworkPropertyMetadata(WindowState.Normal, OnWindowStatePropertyChanged));

        /// <summary>
        /// Identifies the <see cref="WindowStyle"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="WindowStyle"/> dependency property.</value>
        public static readonly DependencyProperty WindowStyleProperty = DependencyProperty.Register(
            "WindowStyle",
            typeof(WindowStyle),
            typeof(InlineWindow),
            new FrameworkPropertyMetadata(WindowStyle.SingleBorderWindow));
        #endregion

        // Part names
        private const string IconPartName = "PART_Icon";
        private const string TitleBarPartName = "PART_TitleBar";

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // NESTED TYPES
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        #region ResizeOperation
        /// <summary>
        /// Specifies a resize operation.
        /// </summary>
        internal enum ResizeOperation
        {
            None,
            West,
            East,
            North,
            NorthWest,
            NorthEast,
            South,
            SouthWest,
            SouthEast
        }
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // EVENTS
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Occurs when the window gains focus within.
        /// </summary>
        [Description("Occurs when the window gains focus within.")]
        public event RoutedEventHandler Activated
        {
            add { AddHandler(ActivatedEvent, value); }
            remove { RemoveHandler(ActivatedEvent, value); }
        }

        /// <summary>
        /// Occurs when the window is about to close.
        /// </summary>
        [Description("Occurs when the window is about to close.")]
        public event RoutedEventHandler Closed
        {
            add { AddHandler(ClosedEvent, value); }
            remove { RemoveHandler(ClosedEvent, value); }
        }

        /// <summary>
        /// Occurs directly after <see cref="Close"/> is called, and can be handled to cancel window closure.
        /// </summary>
        [Description("Occurs directly after Close is called, and can be handled to cancel window closure.")]
        public event EventHandler<CancelRoutedEventArgs> Closing
        {
            add { AddHandler(ClosingEvent, value); }
            remove { RemoveHandler(ClosingEvent, value); }
        }

        /// <summary>
        /// Occurs when the window loses focus within.
        /// </summary>
        [Description("Occurs when the window loses focus within.")]
        public event RoutedEventHandler Deactivated
        {
            add { AddHandler(DeactivatedEvent, value); }
            remove { RemoveHandler(DeactivatedEvent, value); }
        }

        /// <summary>
        /// Occurs after the window has been moved from a drag.
        /// </summary>
        [Description("Occurs after the window has been moved from a drag.")]
        public event RoutedEventHandler DragMoved
        {
            add { AddHandler(DragMovedEvent, value); }
            remove { RemoveHandler(DragMovedEvent, value); }
        }

        /// <summary>
        /// Occurs before the window is moved from a drag, and can be handled to cancel the drag.
        /// </summary>
        [Description("Occurs before the window is moved from a drag, and can be handled to cancel the drag.")]
        public event EventHandler<CancelRoutedEventArgs> DragMoving
        {
            add { AddHandler(DragMovingEvent, value); }
            remove { RemoveHandler(DragMovingEvent, value); }
        }

        /// <summary>
        /// Occurs when the window is moved.
        /// </summary>
        [Description("Occurs when the window is moved.")]
        public event RoutedEventHandler LocationChanged
        {
            add { AddHandler(LocationChangedEvent, value); }
            remove { RemoveHandler(LocationChangedEvent, value); }
        }

        /// <summary>
        /// Occurs when the window is about to open.
        /// </summary>
        [Description("Occurs when the window is about to open.")]
        public event RoutedEventHandler Opened
        {
            add { AddHandler(OpenedEvent, value); }
            remove { RemoveHandler(OpenedEvent, value); }
        }

        /// <summary>
        /// Occurs after the window's <see cref="WindowState"/> property has changed.
        /// </summary>
        [Description("Occurs after the window's WindowState property has changed.")]
        public event RoutedEventHandler StateChanged
        {
            add { AddHandler(StateChangedEvent, value); }
            remove { RemoveHandler(StateChangedEvent, value); }
        }

        /// <summary>
        /// Occurs when the title-bar should display a context menu.
        /// </summary>
        [Description("Occurs when the title-bar should display a context menu.")]
        public event EventHandler<ContextMenuItemRoutedEventArgs> TitleBarContextMenuOpening
        {
            add { AddHandler(TitleBarContextMenuOpeningEvent, value); }
            remove { RemoveHandler(TitleBarContextMenuOpeningEvent, value); }
        }

        /// <summary>
        /// Occurs when the title-bar is double-clicked, and can be handled to cancel the default action.
        /// </summary>
        [Description("Occurs when the title-bar is double-clicked, and can be handled to cancel the default action.")]
        public event EventHandler<CancelRoutedEventArgs> TitleBarDoubleClick
        {
            add { AddHandler(TitleBarDoubleClickEvent, value); }
            remove { RemoveHandler(TitleBarDoubleClickEvent, value); }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // OBJECT
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes the <c>InlineWindow</c> class.
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"),
         SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static InlineWindow()
        {
            // Override default properties
            BorderThicknessProperty.OverrideMetadata(
                typeof(InlineWindow), 
                new FrameworkPropertyMetadata(new Thickness(4)));

            IsTabStopProperty.OverrideMetadata(
                typeof(InlineWindow), 
                new FrameworkPropertyMetadata(false));

            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(
                typeof(InlineWindow), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
            KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata(
                typeof(InlineWindow), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(
                typeof(InlineWindow), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
            FocusableProperty.OverrideMetadata(typeof(InlineWindow), new FrameworkPropertyMetadata(false));

            // Register event handlers
            EventManager.RegisterClassHandler(
                typeof(InlineWindow), ActivatedEvent, new RoutedEventHandler(OnActivatedEvent));
            EventManager.RegisterClassHandler(typeof(InlineWindow), ClosedEvent, new RoutedEventHandler(OnClosedEvent));
            EventManager.RegisterClassHandler(
                typeof(InlineWindow), ClosingEvent, new EventHandler<CancelRoutedEventArgs>(OnClosingEvent));
            EventManager.RegisterClassHandler(
                typeof(InlineWindow), DeactivatedEvent, new RoutedEventHandler(OnDeactivatedEvent));
            EventManager.RegisterClassHandler(
                typeof(InlineWindow), DragMovedEvent, new RoutedEventHandler(OnDragMovedEvent));
            EventManager.RegisterClassHandler(
                typeof(InlineWindow), DragMovingEvent, new EventHandler<CancelRoutedEventArgs>(OnDragMovingEvent));
            EventManager.RegisterClassHandler(
                typeof(InlineWindow), LocationChangedEvent, new RoutedEventHandler(OnLocationChangedEvent));
            EventManager.RegisterClassHandler(typeof(InlineWindow), OpenedEvent, new RoutedEventHandler(OnOpenedEvent));
            EventManager.RegisterClassHandler(
                typeof(InlineWindow), StateChangedEvent, new RoutedEventHandler(OnStateChangedEvent));
            EventManager.RegisterClassHandler(
                typeof(InlineWindow),
                TitleBarContextMenuOpeningEvent,
                new EventHandler<ContextMenuItemRoutedEventArgs>(OnTitleBarContextMenuOpeningEvent));
            EventManager.RegisterClassHandler(
                typeof(InlineWindow),
                TitleBarDoubleClickEvent,
                new EventHandler<CancelRoutedEventArgs>(OnTitleBarDoubleClickEvent));

            // Command bindings
            CommandManager.RegisterClassCommandBinding(
                typeof(InlineWindow), new CommandBinding(CloseCommand, OnCloseCommandExecuted, OnCloseCommandCanExecute));
            CommandManager.RegisterClassCommandBinding(
                typeof(InlineWindow),
                new CommandBinding(MaximizeCommand, OnMaximizeCommandExecuted, OnMaximizeCommandCanExecute));
            CommandManager.RegisterClassCommandBinding(
                typeof(InlineWindow),
                new CommandBinding(MinimizeCommand, OnMinimizeCommandExecuted, OnMinimizeCommandCanExecute));
            CommandManager.RegisterClassCommandBinding(
                typeof(InlineWindow),
                new CommandBinding(RestoreCommand, OnRestoreCommandExecuted, OnRestoreCommandCanExecute));
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // COMMAND HANDLERS
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Occurs when the <see cref="RoutedCommand"/> needs to determine whether it can execute.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A <see cref="CanExecuteRoutedEventArgs"/> that contains the event data.</param>
        private static void OnCloseCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var container = (InlineWindow)sender;
            e.CanExecute = container.CanClose;
            e.Handled = true;
        }

        /// <summary>
        /// Occurs when the <see cref="RoutedCommand"/> is executed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An <see cref="ExecutedRoutedEventArgs"/> that contains the event data.</param>
        private static void OnCloseCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var container = (InlineWindow)sender;
            container.Close();
        }

        /// <summary>
        /// Occurs when the <see cref="RoutedCommand"/> needs to determine whether it can execute.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A <see cref="CanExecuteRoutedEventArgs"/> that contains the event data.</param>
        private static void OnMaximizeCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var container = (InlineWindow)sender;
            e.CanExecute = container.CanMaximizeResolved;
            e.Handled = true;
        }

        /// <summary>
        /// Occurs when the <see cref="RoutedCommand"/> is executed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An <see cref="ExecutedRoutedEventArgs"/> that contains the event data.</param>
        private static void OnMaximizeCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var container = (InlineWindow)sender;
            container.WindowState = WindowState.Maximized;
        }

        /// <summary>
        /// Occurs when the <see cref="RoutedCommand"/> needs to determine whether it can execute.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A <see cref="CanExecuteRoutedEventArgs"/> that contains the event data.</param>
        private static void OnMinimizeCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var container = (InlineWindow)sender;
            e.CanExecute = container.CanMinimizeResolved;
            e.Handled = true;
        }

        /// <summary>
        /// Occurs when the <see cref="RoutedCommand"/> is executed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An <see cref="ExecutedRoutedEventArgs"/> that contains the event data.</param>
        private static void OnMinimizeCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var container = (InlineWindow)sender;
            container.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Occurs when the <see cref="RoutedCommand"/> needs to determine whether it can execute.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A <see cref="CanExecuteRoutedEventArgs"/> that contains the event data.</param>
        private static void OnRestoreCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var container = (InlineWindow)sender;
            e.CanExecute = container.CanRestoreResolved;
            e.Handled = true;
        }

        /// <summary>
        /// Occurs when the <see cref="RoutedCommand"/> is executed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An <see cref="ExecutedRoutedEventArgs"/> that contains the event data.</param>
        private static void OnRestoreCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var container = (InlineWindow)sender;
            container.WindowState = WindowState.Normal;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // EVENT SINKS
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Calls the <see cref="OnActivated"/> method.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> that contains the event data.</param>
        private static void OnActivatedEvent(object sender, RoutedEventArgs e)
        {
            var window = (InlineWindow)sender;
            window.OnActivated(e);
        }

        /// <summary>
        /// Calls the <see cref="OnClosed"/> method.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> that contains the event data.</param>
        private static void OnClosedEvent(object sender, RoutedEventArgs e)
        {
            var window = (InlineWindow)sender;
            window.OnClosed(e);
        }

        /// <summary>
        /// Calls the <see cref="OnClosing"/> method.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The <see cref="CancelRoutedEventArgs"/> that contains the event data.</param>
        private static void OnClosingEvent(object sender, CancelRoutedEventArgs e)
        {
            var window = (InlineWindow)sender;
            window.OnClosing(e);
        }

        /// <summary>
        /// Calls the <see cref="OnDeactivated"/> method.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> that contains the event data.</param>
        private static void OnDeactivatedEvent(object sender, RoutedEventArgs e)
        {
            var window = (InlineWindow)sender;
            window.OnDeactivated(e);
        }

        /// <summary>
        /// Calls the <see cref="OnDragMoved"/> method.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> that contains the event data.</param>
        private static void OnDragMovedEvent(object sender, RoutedEventArgs e)
        {
            var window = (InlineWindow)sender;
            window.OnDragMoved(e);
        }

        /// <summary>
        /// Calls the <see cref="OnDragMoving"/> method.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The <see cref="CancelRoutedEventArgs"/> that contains the event data.</param>
        private static void OnDragMovingEvent(object sender, CancelRoutedEventArgs e)
        {
            var window = (InlineWindow)sender;
            window.OnDragMoving(e);
        }

        /// <summary>
        /// Calls the <see cref="OnLocationChanged"/> method.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> that contains the event data.</param>
        private static void OnLocationChangedEvent(object sender, RoutedEventArgs e)
        {
            var window = (InlineWindow)sender;
            window.OnLocationChanged(e);
        }

        /// <summary>
        /// Calls the <see cref="OnOpened"/> method.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> that contains the event data.</param>
        private static void OnOpenedEvent(object sender, RoutedEventArgs e)
        {
            var window = (InlineWindow)sender;
            window.OnOpened(e);
        }

        /// <summary>
        /// Calls the <see cref="OnStateChanged"/> method.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> that contains the event data.</param>
        private static void OnStateChangedEvent(object sender, RoutedEventArgs e)
        {
            var window = (InlineWindow)sender;
            window.OnStateChanged(e);
        }

        /// <summary>
        /// Occurs when the title bar context menu is opening.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The <see cref="ContextMenuItemRoutedEventArgs"/> that contains the event data.</param>
        private static void OnTitleBarContextMenuOpeningEvent(object sender, ContextMenuItemRoutedEventArgs e)
        {
            var window = (InlineWindow)sender;
            window.OnTitleBarContextMenuOpening(e);
        }

        /// <summary>
        /// Calls the <see cref="OnTitleBarDoubleClick"/> method.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The <see cref="CancelRoutedEventArgs"/> that contains the event data.</param>
        private static void OnTitleBarDoubleClickEvent(object sender, CancelRoutedEventArgs e)
        {
            var window = (InlineWindow)sender;
            window.OnTitleBarDoubleClick(e);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // NON-PUBLIC PROCEDURES
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets whether the window can maximize.
        /// </summary>
        /// <value>
        /// <c>true</c> if the window can maximize; otherwise, <c>false</c>.
        /// </value>
        internal bool CanMaximizeResolved
        {
            get { return ResolveMaximize(ResizeMode, WindowState, WindowStyle); }
        }

        /// <summary>
        /// Gets whether the window can minimize.
        /// </summary>
        /// <value>
        /// <c>true</c> if the window can minimize; otherwise, <c>false</c>.
        /// </value>
        internal bool CanMinimizeResolved
        {
            get { return ResolveMinimize(ResizeMode, WindowState, WindowStyle); }
        }

        /// <summary>
        /// Gets whether the window can minimize.
        /// </summary>
        /// <value>
        /// <c>true</c> if the window can minimize; otherwise, <c>false</c>.
        /// </value>
        private bool CanRestoreResolved
        {
            get
            {
                switch (ResizeMode)
                {
                    case ResizeMode.CanMinimize:
                    case ResizeMode.CanResize:
                    case ResizeMode.CanResizeWithGrip:
                        switch (WindowState)
                        {
                            case WindowState.Maximized:
                            case WindowState.Minimized:
                                return true;
                        }
                        break;
                }
                return false;
            }
        }

        /// <summary>
        /// Returns the mouse location during a drag.
        /// </summary>
        /// <returns>The mouse location during a drag.</returns>
        private Point GetMouseLocation()
        {
            var parentElement = GetParentElement();
            if (parentElement != null)
            {
                // Get the mouse location relative to the parent
                var location = Mouse.GetPosition(parentElement);

                // Get the bounds
                var bounds = new Rect(new Point(), parentElement.RenderSize);

                // Ensure location stays in bounds
                if (location.X >= bounds.Right)
                    location.X = bounds.Right - 1;
                if (location.X < bounds.Left)
                    location.X = bounds.Left;
                if (location.Y >= bounds.Bottom)
                    location.Y = bounds.Bottom - 1;
                if (location.Y < bounds.Top)
                    location.Y = bounds.Top;

                return location;
            }
            return new Point();
        }

        /// <summary>
        /// Returns the parent element.
        /// </summary>
        /// <returns>The parent element.</returns>
        private UIElement GetParentElement()
        {
            var startControl = TemplatedParent ?? (DependencyObject)(Parent as UserControl) ?? this;
            IInputElement parent = VisualTreeHelper.GetParent(startControl) as Panel;
            return parent as UIElement;
        }

        /// <summary>
        /// Hit tests the specified <see cref="Point"/> to see if it might cause a resize operation.
        /// </summary>
        /// <param name="point">The <see cref="Point"/> to hit test.</param>
        /// <returns>
        /// The resize operation that can be caused by dragging at the <see cref="Point"/>.
        /// </returns>
        private ResizeOperation HitTestResizeOperation(Point point)
        {
            // If in a normal state...
            if (WindowState == WindowState.Normal)
            {
                // Get whether to allow resizing
                var canResize = false;
                switch (ResizeMode)
                {
                    case ResizeMode.CanResize:
                    case ResizeMode.CanResizeWithGrip:
                        canResize = true;
                        break;
                }

                // If the point is within the bounds of the window...
                if ((canResize) && (point.X >= 0) && (point.X < ActualWidth) && (point.Y >= 0) &&
                    (point.Y < ActualHeight))
                {
                    var borderThickness = BorderThickness;

                    // If the point is within a border of the window...
                    if ((point.X < borderThickness.Left) || (point.X >= ActualWidth - borderThickness.Right) ||
                        (point.Y < borderThickness.Top) || (point.Y >= ActualHeight - borderThickness.Bottom))
                    {
                        if ((point.X >= 0) && (point.X < borderThickness.Left))
                        {
                            // Over the left border (SystemParameters.CaptionHeight throws security exception in XBAP)
                            if ((point.Y >= 0) && (point.Y < borderThickness.Top +
                                                             (BrowserInteropHelper.IsBrowserHosted
                                                                  ? 0
                                                                  : (WindowStyle == WindowStyle.ToolWindow
                                                                         ? SystemParameters.SmallCaptionHeight
                                                                         : SystemParameters.CaptionHeight))))
                                return ResizeOperation.NorthWest;
                            if ((point.Y >= ActualHeight - borderThickness.Bottom) &&
                                (point.Y < ActualHeight))
                                return ResizeOperation.SouthWest;
                            return ResizeOperation.West;
                        }
                        if ((point.X >= ActualWidth - borderThickness.Right) && (point.X < ActualWidth))
                        {
                            // Over the right border (SystemParameters.CaptionHeight throws security exception in XBAP)
                            if ((point.Y >= 0) && (point.Y < borderThickness.Top +
                                                             (BrowserInteropHelper.IsBrowserHosted
                                                                  ? 0
                                                                  : (WindowStyle == WindowStyle.ToolWindow
                                                                         ? SystemParameters.SmallCaptionHeight
                                                                         : SystemParameters.CaptionHeight))))
                            {
                                return ResizeOperation.NorthEast;
                            }
                            
                            if ((point.Y >= ActualHeight - borderThickness.Bottom) && (point.Y < ActualHeight))
                                return ResizeOperation.SouthEast;
                            
                            return ResizeOperation.East;
                        }
                        if ((point.Y >= 0) && (point.Y < borderThickness.Top))
                        {
                            // Over the top
                            return ResizeOperation.North;
                        }
                        if ((point.Y >= ActualHeight - borderThickness.Bottom) && (point.Y < ActualHeight))
                        {
                            // Over the bottom
                            return ResizeOperation.South;
                        }
                    }
                }
            }

            return ResizeOperation.None;
        }

        /// <summary>
        /// Occurs when the left mouse button is pressed on the icon.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A <see cref="MouseButtonEventArgs"/> that contains the event data.</param>
        private void OnIconMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!e.Handled)
            {
                switch (e.ClickCount)
                {
                    case 1:
                        // Activate
                        Activate();

                        // Raise an event
                        if (null != _titleBar)
                        {
                            // Get current context menu
                            var contextMenu = _titleBar.ContextMenu;
                            if (null != contextMenu)
                            {
                                contextMenu.Placement = PlacementMode.Bottom;
                                contextMenu.PlacementTarget = _icon;
                            }

                            var eventArgs = new ContextMenuItemRoutedEventArgs(
                                contextMenu, TitleBarContextMenuOpeningEvent, this);
                            RaiseEvent(eventArgs);
                            e.Handled = eventArgs.Handled;

                            // Update the context menu
                            if (contextMenu != eventArgs.Item)
                            {
                                contextMenu = _titleBar.ContextMenu = eventArgs.Item;
                                if (null != contextMenu)
                                {
                                    contextMenu.Placement = PlacementMode.Bottom;
                                    contextMenu.PlacementTarget = _icon;
                                }
                            }

                            if (null != contextMenu)
                                contextMenu.IsOpen = true;

                            e.Handled = true;
                        }
                        break;
                    case 2:
                        if (CanClose)
                            Close();
                        e.Handled = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Occurs when the <see cref="IsActiveProperty"/> value is changed.
        /// </summary>
        /// <param name="obj">The <see cref="DependencyObject"/> whose property is changed.</param>
        /// <param name="e">A <see cref="DependencyPropertyChangedEventArgs"/> that contains the event data.</param>
        private static void OnIsActivePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var window = (InlineWindow)obj;

            // Raise an event
            if (window.IsKeyboardFocusWithin)
                window.RaiseEvent(new RoutedEventArgs(ActivatedEvent, window));
            else
                window.RaiseEvent(new RoutedEventArgs(DeactivatedEvent, window));
        }

        /// <summary>
        /// Occurs when the <see cref="LeftProperty"/> value is changed.
        /// </summary>
        /// <param name="obj">The <see cref="DependencyObject"/> whose property is changed.</param>
        /// <param name="e">A <see cref="DependencyPropertyChangedEventArgs"/> that contains the event data.</param>
        private static void OnLeftPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var window = (InlineWindow)obj;
            window.RaiseEvent(new RoutedEventArgs(LocationChangedEvent, window));
        }

        /// <summary>
        /// Occurs when the context menu should be opened for the title bar.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A <see cref="MouseButtonEventArgs"/> that contains the event data.</param>
        private void OnTitleBarContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // Activate
            Activate();

            // Get current context menu
            var titleBarElement = (FrameworkElement)sender;
            var contextMenu = titleBarElement.ContextMenu;
            if (null != contextMenu)
            {
                contextMenu.Placement = PlacementMode.MousePoint;
                contextMenu.PlacementTarget = null;
            }

            // Raise an event
            var eventArgs = new ContextMenuItemRoutedEventArgs(contextMenu, TitleBarContextMenuOpeningEvent, this);
            RaiseEvent(eventArgs);
            e.Handled = eventArgs.Handled;

            // Update the context menu
            if (contextMenu != eventArgs.Item)
                titleBarElement.ContextMenu = eventArgs.Item;
        }

        /// <summary>
        /// Occurs when the left mouse button is pressed on the title bar.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A <see cref="MouseButtonEventArgs"/> that contains the event data.</param>
        private void OnTitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!e.Handled)
            {
                switch (e.ClickCount)
                {
                    case 1:
                        // Activate
                        Activate();

                        if (WindowState != WindowState.Maximized)
                        {
                            // Start a drag
                            DragMove();
                        }
                        break;
                    case 2:
                    {
                        // Raise an event
                        var eventArgs = new CancelRoutedEventArgs(TitleBarDoubleClickEvent, this);
                        RaiseEvent(eventArgs);
                        if (eventArgs.Cancel)
                            break;

                        // Toggle the state of the window
                        if (WindowStyle != WindowStyle.ToolWindow)
                        {
                            switch (ResizeMode)
                            {
                                case ResizeMode.CanMinimize:
                                    if (WindowState != WindowState.Normal)
                                        ToggleWindowState();
                                    break;
                                case ResizeMode.NoResize:
                                    // Do nothing
                                    break;
                                default:
                                    ToggleWindowState();
                                    break;
                            }
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Occurs when the <see cref="TopProperty"/> value is changed.
        /// </summary>
        /// <param name="obj">The <see cref="DependencyObject"/> whose property is changed.</param>
        /// <param name="e">A <see cref="DependencyPropertyChangedEventArgs"/> that contains the event data.</param>
        private static void OnTopPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var window = (InlineWindow)obj;
            window.RaiseEvent(new RoutedEventArgs(LocationChangedEvent, window));
        }

        /// <summary>
        /// Occurs when the <see cref="WindowStateProperty"/> value is changed.
        /// </summary>
        /// <param name="obj">The <see cref="DependencyObject"/> whose property is changed.</param>
        /// <param name="e">A <see cref="DependencyPropertyChangedEventArgs"/> that contains the event data.</param>
        private static void OnWindowStatePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var window = (InlineWindow)obj;

            var oldState = (WindowState)e.OldValue;
            var newState = (WindowState)e.NewValue;

            var isKeyboardFocusWithin = window.IsKeyboardFocusWithin;

            // If leaving a normal state...
            if (oldState == WindowState.Normal)
            {
                // Update restore bounds
                if ((!double.IsNaN(window.Width)) && (!double.IsNaN(window.Height)))
                    window.RestoreBounds = new Rect(window.Left, window.Top, window.Width, window.Height);
            }

            // Raise an event
            window.RaiseEvent(new RoutedEventArgs(StateChangedEvent, window));

            if ((newState == WindowState.Normal) && (!window.RestoreBounds.IsEmpty))
            {
                // Restoring... use restore bounds
                window.Left = window.RestoreBounds.Left;
                window.Top = window.RestoreBounds.Top;
                window.Width = window.RestoreBounds.Width;
                window.Height = window.RestoreBounds.Height;
            }

            // If keyboard focus was within... activate again
            if (isKeyboardFocusWithin)
            {
                window.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    (DispatcherOperationCallback)delegate
                                                 {
                                                     window.Activate();
                                                     return null;
                                                 },
                    null);
            }
        }

        /// <summary>
        /// Processes a drag move.
        /// </summary>
        private void ProcessDragMove()
        {
            // Get the new location
            var newLocation = GetMouseLocation();
            newLocation.X -= _dragMouseOffset.X;
            newLocation.Y -= _dragMouseOffset.Y;

            var bounds = new Rect(newLocation.X, newLocation.Y, _dragSize.Width, _dragSize.Height);

            bounds = UpdateBounds(bounds, ResizeOperation.None);

            // Update the bounds
            Left = bounds.X;
            Top = bounds.Y;
        }

        /// <summary>
        /// Processes a drag resize.
        /// </summary>
        private void ProcessDragResize()
        {
            // Get the current mouse location
            var newLocation = GetMouseLocation();

            // Initialize
            var width = _dragSize.Width;
            var height = _dragSize.Height;
            var isMinWidth = (width <= MinWidth);
            var isMinHeight = (height <= MinHeight);
            var bounds = new Rect(new Point(Left, Top), RenderSize);

            // Get width/height change
            switch (_currentResizeOperation)
            {
                case ResizeOperation.East:
                    if ((!isMinWidth) || (newLocation.X >= bounds.Right))
                        width += (newLocation.X - _dragMouseOffset.X);
                    break;
                case ResizeOperation.North:
                    if ((!isMinHeight) || (newLocation.Y < bounds.Top))
                        height -= (newLocation.Y - _dragMouseOffset.Y);
                    break;
                case ResizeOperation.NorthEast:
                    if ((!isMinWidth) || (newLocation.X >= bounds.Right))
                        width += (newLocation.X - _dragMouseOffset.X);
                    if ((!isMinHeight) || (newLocation.Y < bounds.Top))
                        height -= (newLocation.Y - _dragMouseOffset.Y);
                    break;
                case ResizeOperation.NorthWest:
                    if ((!isMinWidth) || (newLocation.X < bounds.Left))
                        width -= (newLocation.X - _dragMouseOffset.X);
                    if ((!isMinHeight) || (newLocation.Y < bounds.Top))
                        height -= (newLocation.Y - _dragMouseOffset.Y);
                    break;
                case ResizeOperation.South:
                    if ((!isMinHeight) || (newLocation.Y >= bounds.Bottom))
                        height += (newLocation.Y - _dragMouseOffset.Y);
                    break;
                case ResizeOperation.SouthEast:
                    if ((!isMinWidth) || (newLocation.X >= bounds.Right))
                        width += (newLocation.X - _dragMouseOffset.X);
                    if ((!isMinHeight) || (newLocation.Y >= bounds.Bottom))
                        height += (newLocation.Y - _dragMouseOffset.Y);
                    break;
                case ResizeOperation.SouthWest:
                    if ((!isMinWidth) || (newLocation.X < bounds.Left))
                        width -= (newLocation.X - _dragMouseOffset.X);
                    if ((!isMinHeight) || (newLocation.Y >= bounds.Bottom))
                        height += (newLocation.Y - _dragMouseOffset.Y);
                    break;
                case ResizeOperation.West:
                    if ((!isMinWidth) || (newLocation.X < bounds.Left))
                        width -= (newLocation.X - _dragMouseOffset.X);
                    break;
            }

            // Update bounds
            bounds.Width = Math.Max(MinWidth, Math.Min(MaxWidth, width));
            bounds.Height = Math.Max(MinHeight, Math.Min(MaxHeight, height));

            // Get deltas
            var deltaX = (bounds.Width - ActualWidth);
            var deltaY = (bounds.Height - ActualHeight);

            // Change location
            switch (_currentResizeOperation)
            {
                case ResizeOperation.North:
                    bounds.Y -= deltaY;
                    break;
                case ResizeOperation.NorthEast:
                    bounds.Y -= deltaY;
                    break;
                case ResizeOperation.NorthWest:
                    bounds.X -= deltaX;
                    bounds.Y -= deltaY;
                    break;
                case ResizeOperation.SouthWest:
                    bounds.X -= deltaX;
                    break;
                case ResizeOperation.West:
                    bounds.X -= deltaX;
                    break;
            }

            bounds = UpdateBounds(bounds, _currentResizeOperation);

            // Update the bounds
            if (!DoubleUtil.AreClose(Left, bounds.X))
                Left = bounds.X;
            if (!DoubleUtil.AreClose(Top, bounds.Y))
                Top = bounds.Y;
            if (!DoubleUtil.AreClose(ActualWidth, bounds.Width))
                Width = bounds.Width;
            if (!DoubleUtil.AreClose(ActualHeight, bounds.Height))
                Height = bounds.Height;
        }

        /// <summary>
        /// Determines a window can be maximized based on the specified parameters.
        /// </summary>
        /// <param name="resizeMode">The resize mode.</param>
        /// <param name="windowState">State of the window.</param>
        /// <param name="windowStyle">The window style.</param>
        /// <returns><c>true</c> if the window can maximize; otherwise, <c>false</c>.</returns>
        private static bool ResolveMaximize(ResizeMode resizeMode, WindowState windowState, WindowStyle windowStyle)
        {
            switch (resizeMode)
            {
                case ResizeMode.CanResize:
                case ResizeMode.CanResizeWithGrip:
                    switch (windowState)
                    {
                        case WindowState.Minimized:
                        case WindowState.Normal:
                            return (windowStyle != WindowStyle.ToolWindow);
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// Determines a window can be minimized based on the specified parameters.
        /// </summary>
        /// <param name="resizeMode">The resize mode.</param>
        /// <param name="windowState">State of the window.</param>
        /// <param name="windowStyle">The window style.</param>
        /// <returns><c>true</c> if the window can minimize; otherwise, <c>false</c>.</returns>
        private static bool ResolveMinimize(ResizeMode resizeMode, WindowState windowState, WindowStyle windowStyle)
        {
            switch (resizeMode)
            {
                case ResizeMode.CanMinimize:
                case ResizeMode.CanResize:
                case ResizeMode.CanResizeWithGrip:
                    switch (windowState)
                    {
                        case WindowState.Maximized:
                        case WindowState.Normal:
                            return (windowStyle != WindowStyle.ToolWindow);
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// Starts a drag operation.
        /// </summary>
        private void StartDrag()
        {
            // Ensure the left mouse button is pressed
            if (Mouse.LeftButton != MouseButtonState.Pressed)
                MessageBox.Show("Drag'n'Drop: The left mouse button must be down.", "Warning", MessageBoxButton.OK);

            // Raise an event
            var e = new CancelRoutedEventArgs(DragMovingEvent, this);
            RaiseEvent(e);
            if (e.Cancel)
                return;

            // Quit if the mouse cannot be captured
            if (!Mouse.Capture(this, CaptureMode.SubTree))
                return;

            // Get the mouse location
            _dragMouseOffset = Mouse.GetPosition(this);
            _dragSize = new Size(
                (double.IsNaN(Width) ? ActualWidth : Width),
                (double.IsNaN(Height) ? ActualHeight : Height));

            // Adjust for possible scale transform if there is a parent
            var parentElement = GetParentElement();
            if (parentElement != null)
            {
                _dragMouseOffset = Mouse.GetPosition(parentElement);
                _dragMouseOffset.X -= Left;
                _dragMouseOffset.Y -= Top;
            }

            // Flag that we are moving
            _isMoving = true;
        }

        /// <summary>
        /// Starts a resize operation.
        /// </summary>
        /// <param name="resizeOperation">A <see cref="ResizeOperation"/> indicating the resize operation to start.</param>
        private void StartResize(ResizeOperation resizeOperation)
        {
            // Ensure the left mouse button is pressed
            if (Mouse.LeftButton != MouseButtonState.Pressed)
                throw new InvalidOperationException("The left mouse button must be down.");

            // Quit if the mouse cannot be captured
            if (!CaptureMouse())
                return;

            // Get the mouse location and current size
            _dragMouseOffset = GetMouseLocation();
            _dragSize = new Size(
                double.IsNaN(Width) ? ActualWidth : Width,
                double.IsNaN(Height) ? ActualHeight : Height);

            // Flag that we are resizing
            _currentResizeOperation = resizeOperation;
        }

        /// <summary>
        /// Stops any current drag or resize operation.
        /// </summary>
        private void StopDragResize()
        {
            // Quit if not dragging or resizing
            if (!_isMoving && _currentResizeOperation == ResizeOperation.None)
                return;

            if (_isMoving)
            {
                // Raise an event
                RaiseEvent(new RoutedEventArgs(DragMovedEvent, this));
            }

            // Stop drag/resize
            _isMoving = false;
            _currentResizeOperation = ResizeOperation.None;

            // Ensure mouse capture is released
            if (IsMouseCaptured)
                ReleaseMouseCapture();
        }

        /// <summary>
        /// Updates the specified bounds. Override this method in derived classes to implement custom logic for the bounds.
        /// </summary>
        /// <param name="bounds">The bounds.</param>
        /// <param name="resizeOperation">The resize operation.</param>
        /// <returns>The updated bounds.</returns>
        internal virtual Rect UpdateBounds(Rect bounds, ResizeOperation resizeOperation)
        {
            return bounds;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // PUBLIC PROCEDURES
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Attempts to bring the window to the foreground and activates it.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the window was successfully activated; otherwise, <c>false</c>.
        /// </returns>
        public bool Activate()
        {
            if (!IsKeyboardFocusWithin)
            {
                var inputElement = Keyboard.Focus(this) as DependencyObject;
                if (inputElement != null && inputElement.IsVisualDescendantOf(this))
                    return true;
                return MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
            }
            return true;
        }

        /// <summary>
        /// Gets or sets whether the window can close.
        /// </summary>
        /// <value>
        /// <c>true</c> if the window can close; otherwise, <c>false</c>.
        /// The default value is <c>true</c>.
        /// </value>
        public bool CanClose
        {
            get { return (bool)GetValue(CanCloseProperty); }
            set { SetValue(CanCloseProperty, value); }
        }

        /// <summary>
        /// Closes the window.
        /// </summary>
        public void Close()
        {
            // Quit if already invisible
            if (!IsVisible)
                return;

            // Raise an event
            var e = new CancelRoutedEventArgs(ClosingEvent, this);
            RaiseEvent(e);
            if (e.Cancel)
                return;

            // Raise an event
            RaiseEvent(new RoutedEventArgs(ClosedEvent, this));
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Windows.CornerRadius"/> of the window.
        /// </summary>
        /// <value>
        /// The <see cref="System.Windows.CornerRadius"/> of the window.
        /// The default value is <c>0</c>.
        /// </value>
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        /// <summary>
        /// Allows a window to be dragged by a mouse with its left button down over an exposed area of the window's client rectangle.
        /// </summary>
        public void DragMove()
        {
            StartDrag();
        }

        /// <summary>
        /// Gets or sets the base <see cref="Color"/> to use for drawing the drop-shadow.
        /// </summary>
        /// <value>
        /// The base <see cref="Color"/> to use for drawing the drop-shadow.
        /// The default value is <c>#71808080</c>.
        /// </value>
        public Color DropShadowColor
        {
            get { return (Color)GetValue(DropShadowColorProperty); }
            set { SetValue(DropShadowColorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the z-offset of the drop shadow.
        /// </summary>
        /// <value>
        /// The z-offset of the drop shadow.
        /// The default value is <c>6</c>.
        /// </value>
        public double DropShadowZOffset
        {
            get { return (double)GetValue(DropShadowZOffsetProperty); }
            set { SetValue(DropShadowZOffsetProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the window is capable of displaying a close button.
        /// </summary>
        /// <value>
        /// <c>true</c> if the window is capable of displaying a close button; otherwise, <c>false</c>.
        /// The default value is <c>true</c>.
        /// </value>
        public bool HasCloseButton
        {
            get { return (bool)GetValue(HasCloseButtonProperty); }
            set { SetValue(HasCloseButtonProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the window has a drop shadow.
        /// </summary>
        /// <value>
        /// <c>true</c> if the window has a drop shadow; otherwise, <c>false</c>.
        /// The default value is <c>false</c>.
        /// </value>
        public bool HasDropShadow
        {
            get { return (bool)GetValue(HasDropShadowProperty); }
            set { SetValue(HasDropShadowProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the window is capable of displaying an icon.
        /// </summary>
        /// <value>
        /// <c>true</c> if the window is capable of displaying an icon; otherwise, <c>false</c>.
        /// The default value is <c>true</c>.
        /// </value>
        /// <remarks>
        /// By default, the <see cref="Icon"/> is shown by all styles except <c>WindowStyle.ToolWindow</c>.
        /// To show the <see cref="Icon"/> when using the <c>WindowStyle.ToolWindow</c> style,
        /// this property must be explicitly set to <c>true</c>.
        /// </remarks>
        public bool HasIcon
        {
            get { return (bool)GetValue(HasIconProperty); }
            set { SetValue(HasIconProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the window is capable of displaying a maximize button.
        /// </summary>
        /// <value>
        /// <c>true</c> if the window is capable of displaying a maximize button; otherwise, <c>false</c>.
        /// The default value is <c>true</c>.
        /// </value>
        public bool HasMaximizeButton
        {
            get { return (bool)GetValue(HasMaximizeButtonProperty); }
            set { SetValue(HasMaximizeButtonProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the window is capable of displaying a minimize button.
        /// </summary>
        /// <value>
        /// <c>true</c> if the window is capable of displaying a minimize button; otherwise, <c>false</c>.
        /// The default value is <c>true</c>.
        /// </value>
        public bool HasMinimizeButton
        {
            get { return (bool)GetValue(HasMinimizeButtonProperty); }
            set { SetValue(HasMinimizeButtonProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the window is capable of displaying a restore button.
        /// </summary>
        /// <value>
        /// <c>true</c> if the window is capable of displaying a restore button; otherwise, <c>false</c>.
        /// The default value is <c>true</c>.
        /// </value>
        public bool HasRestoreButton
        {
            get { return (bool)GetValue(HasRestoreButtonProperty); }
            set { SetValue(HasRestoreButtonProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="ImageSource"/> displayed as the window icon.
        /// </summary>
        /// <value>
        /// The <see cref="ImageSource"/> displayed as the window icon.
        /// The default value is <see langword="null"/>.
        /// </value>
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        /// <summary>
        /// Gets whether the window is currently active.
        /// </summary>
        /// <value>
        /// <c>true</c> if the window is currently active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            private set { SetValue(IsActivePropertyKey, value); }
        }

        /// <summary>
        /// Gets or sets whether the title bar text should draw using a shadow.
        /// </summary>
        /// <value>
        /// <c>true</c> if the title bar text should draw using a shadow; otherwise, <c>false</c>.
        /// The default value is <c>false</c>.
        /// </value>
        public bool IsTitleBarTextShadowEnabled
        {
            get { return (bool)GetValue(IsTitleBarTextShadowEnabledProperty); }
            set { SetValue(IsTitleBarTextShadowEnabledProperty, value); }
        }

        /// <summary>
        /// Gets or sets the position of the window's left edge, in relation to its container.
        /// </summary>
        /// <value>The position of the window's left edge, in relation to its container.</value>
        public double Left
        {
            get { return (double)GetValue(LeftProperty); }
            set { SetValue(LeftProperty, value); }
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="Activated"/> attached event reaches an element in its route that is derived from this class.
        /// Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">A <c>RoutedEventArgs</c> that contains the event data.</param>
        /// <remarks>
        /// This method has no default implementation. 
        /// Because an intermediate class in the inheritance might implement this method, 
        /// we recommend that you call the base implementation in your implementation.
        /// </remarks>
        protected virtual void OnActivated(RoutedEventArgs e) { }

        /// <summary>
        /// Invoked whenever application code or internal processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            // Call the base method
            base.OnApplyTemplate();

            if (_icon != null)
                _titleBar.MouseLeftButtonDown -= OnIconMouseLeftButtonDown;
            _icon = GetTemplateChild(IconPartName) as FrameworkElement;
            if (_icon != null)
                _icon.MouseLeftButtonDown += OnIconMouseLeftButtonDown;

            if (_titleBar != null)
            {
                _titleBar.ContextMenuOpening -= OnTitleBarContextMenuOpening;
                _titleBar.MouseLeftButtonDown -= OnTitleBarMouseLeftButtonDown;
            }
            _titleBar = GetTemplateChild(TitleBarPartName) as FrameworkElement;
            if (_titleBar != null)
            {
                _titleBar.ContextMenuOpening += OnTitleBarContextMenuOpening;
                _titleBar.MouseLeftButtonDown += OnTitleBarMouseLeftButtonDown;
            }
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="Closed"/> attached event reaches an element in its route that is derived from this class.
        /// Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">A <c>RoutedEventArgs</c> that contains the event data.</param>
        /// <remarks>
        /// This method has no default implementation. 
        /// Because an intermediate class in the inheritance might implement this method, 
        /// we recommend that you call the base implementation in your implementation.
        /// </remarks>
        protected virtual void OnClosed(RoutedEventArgs e) { }

        /// <summary>
        /// Invoked when an unhandled <see cref="Closing"/> attached event reaches an element in its route that is derived from this class.
        /// Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">A <c>CancelRoutedEventArgs</c> that contains the event data.</param>
        /// <remarks>
        /// This method has no default implementation. 
        /// Because an intermediate class in the inheritance might implement this method, 
        /// we recommend that you call the base implementation in your implementation.
        /// </remarks>
        protected virtual void OnClosing(CancelRoutedEventArgs e) { }

        /// <summary>
        /// Invoked when an unhandled <see cref="Deactivated"/> attached event reaches an element in its route that is derived from this class.
        /// Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">A <c>RoutedEventArgs</c> that contains the event data.</param>
        /// <remarks>
        /// This method has no default implementation. 
        /// Because an intermediate class in the inheritance might implement this method, 
        /// we recommend that you call the base implementation in your implementation.
        /// </remarks>
        protected virtual void OnDeactivated(RoutedEventArgs e) { }

        /// <summary>
        /// Invoked when an unhandled <see cref="DragMoved"/> attached event reaches an element in its route that is derived from this class.
        /// Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">A <c>RoutedEventArgs</c> that contains the event data.</param>
        /// <remarks>
        /// This method has no default implementation. 
        /// Because an intermediate class in the inheritance might implement this method, 
        /// we recommend that you call the base implementation in your implementation.
        /// </remarks>
        protected virtual void OnDragMoved(RoutedEventArgs e) { }

        /// <summary>
        /// Invoked when an unhandled <see cref="DragMoving"/> attached event reaches an element in its route that is derived from this class.
        /// Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">A <c>CancelRoutedEventArgs</c> that contains the event data.</param>
        /// <remarks>
        /// This method has no default implementation. 
        /// Because an intermediate class in the inheritance might implement this method, 
        /// we recommend that you call the base implementation in your implementation.
        /// </remarks>
        protected virtual void OnDragMoving(CancelRoutedEventArgs e) { }

        /// <summary>
        /// Reports that the <c>IsKeyboardFocusWithin</c> property changed. 
        /// </summary>
        /// <param name="e">A <c>DependencyPropertyChangedEventArgs</c> that contains the event data.</param>
        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            // Call the base method
            base.OnIsKeyboardFocusWithinChanged(e);

            // Update the IsActive properties
            IsActive = IsKeyboardFocusWithin;
            InlineWindowTitleBarButton.SetIsActive(this, IsKeyboardFocusWithin);
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="LocationChanged"/> attached event reaches an element in its route that is derived from this class.
        /// Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">A <c>RoutedEventArgs</c> that contains the event data.</param>
        /// <remarks>
        /// This method has no default implementation. 
        /// Because an intermediate class in the inheritance might implement this method, 
        /// we recommend that you call the base implementation in your implementation.
        /// </remarks>
        protected virtual void OnLocationChanged(RoutedEventArgs e) { }

        /// <summary>
        /// Invoked when an unhandled <see cref="Mouse.LostMouseCaptureEvent"/> attached event reaches an element in its route 
        /// that is derived from this class.
        /// </summary>
        /// <param name="e">The <see cref="MouseEventArgs"/> that contains event data.</param>
        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            // Call the base method
            base.OnLostMouseCapture(e);

            // Stop any current drag/resize
            StopDragResize();
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="UIElement"/>.<see cref="UIElement.MouseLeftButtonDown"/> attached event is raised on this element. 
        /// Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">A <see cref="MouseButtonEventArgs"/> that contains the event data.</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            // Call the base method
            base.OnMouseLeftButtonDown(e);

            // Ensure the window contains focus...
            if (!e.Handled)
            {
                Activate();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="UIElement"/>.<see cref="UIElement.MouseLeftButtonUp"/> attached event is raised on this element. 
        /// Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">A <see cref="MouseButtonEventArgs"/> that contains the event data.</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            // Call the base method
            base.OnMouseLeftButtonUp(e);

            // Stop any current drag/resize
            StopDragResize();
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="UIElement"/>.<see cref="UIElement.MouseMove"/> attached event is raised on this element. 
        /// Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            // Call the base method
            base.OnMouseMove(e);

            // Process a drag move or resize
            if (_isMoving)
                ProcessDragMove();
            else if (_currentResizeOperation != ResizeOperation.None)
                ProcessDragResize();
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="Opened"/> attached event reaches an element in its route that is derived from this class.
        /// Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">A <c>RoutedEventArgs</c> that contains the event data.</param>
        /// <remarks>
        /// This method has no default implementation. 
        /// Because an intermediate class in the inheritance might implement this method, 
        /// we recommend that you call the base implementation in your implementation.
        /// </remarks>
        protected virtual void OnOpened(RoutedEventArgs e) { }

        /// <summary>
        /// Invoked when an unhandled <see cref="UIElement.PreviewMouseLeftButtonDown"/> attached event is raised on this element. 
        /// Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">A <see cref="MouseButtonEventArgs"/> that contains the event data.</param>
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            // Hit test for a resize operation
            var operation = HitTestResizeOperation(e.GetPosition(this));
            if (operation != ResizeOperation.None)
            {
                // Start a resize operation
                StartResize(operation);
                e.Handled = true;
            }

            // Call the base method
            base.OnPreviewMouseLeftButtonDown(e);
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="Mouse"/>.<see cref="Mouse.PreviewMouseMoveEvent"/> attached event is raised on this element. 
        /// Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            // If no button is pressed...
            if ((e.LeftButton == MouseButtonState.Released) && (e.RightButton == MouseButtonState.Released))
            {
                // Hit test for a resize operation
                var operation = HitTestResizeOperation(e.GetPosition(this));
                switch (operation)
                {
                    case ResizeOperation.West:
                    case ResizeOperation.East:
                        Cursor = Cursors.SizeWE;
                        e.Handled = true;
                        break;
                    case ResizeOperation.NorthWest:
                    case ResizeOperation.SouthEast:
                        Cursor = (FlowDirection == FlowDirection.LeftToRight
                                           ? Cursors.SizeNWSE
                                           : Cursors.SizeNESW);
                        e.Handled = true;
                        break;
                    case ResizeOperation.North:
                    case ResizeOperation.South:
                        Cursor = Cursors.SizeNS;
                        e.Handled = true;
                        break;
                    case ResizeOperation.NorthEast:
                    case ResizeOperation.SouthWest:
                        Cursor = (FlowDirection == FlowDirection.LeftToRight
                                           ? Cursors.SizeNESW
                                           : Cursors.SizeNWSE);
                        e.Handled = true;
                        break;
                    default:
                        if (Cursor != null)
                            Cursor = null;
                        break;
                }
            }

            // Call the base method
            base.OnPreviewMouseMove(e);
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="StateChanged"/> attached event reaches an element in its route that is derived from this class.
        /// Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">A <c>RoutedEventArgs</c> that contains the event data.</param>
        /// <remarks>
        /// This method has no default implementation. 
        /// Because an intermediate class in the inheritance might implement this method, 
        /// we recommend that you call the base implementation in your implementation.
        /// </remarks>
        protected virtual void OnStateChanged(RoutedEventArgs e) { }

        /// <summary>
        /// Invoked when an unhandled <see cref="TitleBarContextMenuOpening"/> attached event reaches an element in its route that is derived from this class.
        /// Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">A <c>ContextMenuItemRoutedEventArgs</c> that contains the event data.</param>
        /// <remarks>
        /// This method has no default implementation. 
        /// Because an intermediate class in the inheritance might implement this method, 
        /// we recommend that you call the base implementation in your implementation.
        /// </remarks>
        protected virtual void OnTitleBarContextMenuOpening(ContextMenuItemRoutedEventArgs e) { }

        /// <summary>
        /// Invoked when an unhandled <see cref="TitleBarDoubleClick"/> attached event reaches an element in its route that is derived from this class.
        /// Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">A <c>CancelRoutedEventArgs</c> that contains the event data.</param>
        /// <remarks>
        /// This method has no default implementation. 
        /// Because an intermediate class in the inheritance might implement this method, 
        /// we recommend that you call the base implementation in your implementation.
        /// </remarks>
        protected virtual void OnTitleBarDoubleClick(CancelRoutedEventArgs e) { }

        /// <summary>
        /// Gets a <see cref="Rect"/> indicating the size and location of a window before being either minimized or maximized.
        /// </summary>
        /// <value>A <see cref="Rect"/> indicating the size and location of a window before being either minimized or maximized.</value>
        public Rect RestoreBounds
        {
            get { return (Rect)GetValue(RestoreBoundsProperty); }
            private set { SetValue(RestoreBoundsProperty, value); }
        }

        /// <summary>
        /// Gets or sets a <see cref="System.Windows.ResizeMode"/> indicating how the window is able to resize.
        /// </summary>
        /// <value>
        /// A <see cref="System.Windows.ResizeMode"/> indicating how the window is able to resize.
        /// The default value is <c>CanResize</c>.
        /// </value>
        public ResizeMode ResizeMode
        {
            get { return (ResizeMode)GetValue(ResizeModeProperty); }
            set { SetValue(ResizeModeProperty, value); }
        }

        /// <summary>
        /// Shows the window.
        /// </summary>
        public void Show()
        {
            // Quit if already visible
            if (IsVisible)
                return;

            // Raise an event
            RaiseEvent(new RoutedEventArgs(OpenedEvent, this));
        }

        /// <summary>
        /// Gets or sets the title of the window.
        /// </summary>
        /// <value>The title of the window.</value>
        [Localizability(LocalizationCategory.Title)]
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="FontWeight"/> of the title.
        /// </summary>
        /// <value>
        /// The <see cref="FontWeight"/> of the title.
        /// The default value is <c>Normal</c>.
        /// </value>
        public FontWeight TitleBarFontWeight
        {
            get { return (FontWeight)GetValue(TitleBarFontWeightProperty); }
            set { SetValue(TitleBarFontWeightProperty, value); }
        }

        /// <summary>
        /// Toggles the window's state based on its current state.
        /// </summary>
        /// <remarks>
        /// This method is generally called when the title bar is double-clicked.
        /// </remarks>
        public void ToggleWindowState()
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }

        /// <summary>
        /// Gets or sets the position of the window's top edge, in relation to its container.
        /// </summary>
        /// <value>The position of the window's top edge, in relation to its container.</value>
        public double Top
        {
            get { return (double)GetValue(TopProperty); }
            set { SetValue(TopProperty, value); }
        }

        /// <summary>
        /// Gets or sets a <see cref="System.Windows.WindowState"/> indicating the current state of the window.
        /// </summary>
        /// <value>
        /// A <see cref="System.Windows.WindowState"/> indicating the current state of the window.
        /// The default value is <c>Normal</c>.
        /// </value>
        public WindowState WindowState
        {
            get { return (WindowState)GetValue(WindowStateProperty); }
            set { SetValue(WindowStateProperty, value); }
        }

        /// <summary>
        /// Gets or sets a <see cref="System.Windows.WindowStyle"/> indicating the border style of the window.
        /// </summary>
        /// <value>
        /// A <see cref="System.Windows.WindowStyle"/> indicating the border style of the window.
        /// The default value is <c>SingleBorderWindow</c>.
        /// </value>
        public WindowStyle WindowStyle
        {
            get { return (WindowStyle)GetValue(WindowStyleProperty); }
            set { SetValue(WindowStyleProperty, value); }
        }
    }
}