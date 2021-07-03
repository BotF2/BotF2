using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using Supremacy.Annotations;

namespace Supremacy.Client.Controls
{
    public class InfoCard : Expander
    {
        private readonly bool _isContainerForItem;
        private readonly InfoCardFader _fader;

        public static readonly RoutedCommand CloseCommand = new RoutedCommand("Close", typeof(InfoCard));
        public static readonly RoutedCommand PinCommand = new RoutedCommand("Pin", typeof(InfoCard));
        public static readonly RoutedCommand UnpinCommand = new RoutedCommand("Unpin", typeof(InfoCard));

        static InfoCard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(InfoCard),
                new FrameworkPropertyMetadata(typeof(InfoCard)));

            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(
                typeof(InfoCard),
                new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));

            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(
                typeof(InfoCard),
                new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));

            CommandManager.RegisterClassInputBinding(
                typeof(InfoCard),
                new KeyBinding(
                    CloseCommand,
                    new KeyGesture(Key.Escape)));

            CommandManager.RegisterClassCommandBinding(
                typeof(InfoCard),
                new CommandBinding(
                    CloseCommand,
                    ExecuteCloseCommand));

            CommandManager.RegisterClassCommandBinding(
                typeof(InfoCard),
                new CommandBinding(
                    PinCommand,
                    ExecutePinCommand,
                    CanExecutePinCommand));

            CommandManager.RegisterClassCommandBinding(
                typeof(InfoCard),
                new CommandBinding(
                    UnpinCommand,
                    ExecuteUnpinCommand,
                    CanExecuteUnpinCommand));
        }

        private static void ExecuteCloseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            InfoCard infoCard = (InfoCard)sender;
            if (!infoCard.IsOpen)
            {
                return;
            }

            _ = infoCard.Close();
        }

        private static void ExecutePinCommand(object sender, ExecutedRoutedEventArgs e)
        {
            InfoCard infoCard = (InfoCard)sender;
            if (!infoCard.CanPin)
            {
                return;
            }

            infoCard.IsPinned = true;
        }

        private static void CanExecutePinCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            InfoCard infoCard = (InfoCard)sender;
            e.CanExecute = infoCard.CanPin && !infoCard.IsPinned;
            e.Handled = true;
        }

        private static void ExecuteUnpinCommand(object sender, ExecutedRoutedEventArgs e)
        {
            InfoCard infoCard = (InfoCard)sender;
            if (!infoCard.IsPinned)
            {
                return;
            }

            infoCard.IsPinned = false;
        }

        private static void CanExecuteUnpinCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((InfoCard)sender).IsPinned;
            e.Handled = true;
        }

        public InfoCard() : this(null) { }

        public InfoCard([CanBeNull] IInfoCardSubject subject) : this(false)
        {
            Subject = subject;
        }

        internal InfoCard(bool isContainerForItem)
        {
            _fader = new InfoCardFader();
            _isContainerForItem = isContainerForItem;

            UniqueId = Guid.NewGuid();
            LastFocusedDateTime = DateTime.MinValue;

            CoerceValue(CanPinProperty);
        }

        internal void ResetFade()
        {
            _fader.StopFade();

            if (IsVisible && !IsMouseOver)
            {
                _fader.StartFade(this);
            }
        }

        internal DateTime LastFocusedDateTime { get; set; }

        protected internal IInputElement FocusedElement { get; set; }

        public InfoCardSite InfoCardSite => InfoCardSite.GetInfoCardSite(this) ?? RegisteredInfoCardSite;

        internal InfoCardSite RegisteredInfoCardSite { get; set; }

        internal bool IsContainerForItem => _isContainerForItem;

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);

            if (IsKeyboardFocusWithin)
            {
                LastFocusedDateTime = DateTime.Now;
            }
            else if (!IsPinned)
            {
                _ = Close();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Handled || !IsOpen)
            {
                return;
            }

            if (e.Key == Key.Escape)
            {
                _ = Close();
            }
        }

        #region Activated Event
        public static RoutedEvent ActivatedEvent = EventManager.RegisterRoutedEvent(
            "Activated",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(InfoCard));

        public event RoutedEventHandler Activated
        {
            add { AddHandler(ActivatedEvent, value); }
            remove { RemoveHandler(ActivatedEvent, value); }
        }

        internal void RaiseActivatedEvent()
        {
            RaiseEvent(new RoutedEventArgs(ActivatedEvent, this));
        }
        #endregion

        #region Deactivated Event
        public static RoutedEvent DeactivatedEvent = EventManager.RegisterRoutedEvent(
            "Deactivated",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(InfoCard));

        public event RoutedEventHandler Deactivated
        {
            add { AddHandler(DeactivatedEvent, value); }
            remove { RemoveHandler(DeactivatedEvent, value); }
        }

        internal void RaiseDeactivatedEvent()
        {
            RaiseEvent(new RoutedEventArgs(DeactivatedEvent, this));
        }
        #endregion

        #region CustomPlacementCallback Property
        public static readonly DependencyProperty CustomPlacementCallbackProperty =
            InfoCardService.CustomInfoCardPlacementCallbackProperty.AddOwner(typeof(InfoCard));

        public CustomInfoCardPlacementCallback CustomPlacementCallback
        {
            get => (CustomInfoCardPlacementCallback)GetValue(CustomPlacementCallbackProperty);
            set => SetValue(CustomPlacementCallbackProperty, value);
        }
        #endregion

        #region Opening Event
        public static RoutedEvent OpeningEvent = EventManager.RegisterRoutedEvent(
            "Opening",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(InfoCard));

        public event RoutedEventHandler Opening
        {
            add { AddHandler(OpeningEvent, value); }
            remove { RemoveHandler(OpeningEvent, value); }
        }

        internal void RaiseOpeningEvent()
        {
            RaiseEvent(new RoutedEventArgs(OpeningEvent, this));
        }
        #endregion

        #region Opened Event
        public static RoutedEvent OpenedEvent = EventManager.RegisterRoutedEvent(
            "Opened",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(InfoCard));

        public event RoutedEventHandler Opened
        {
            add { AddHandler(OpenedEvent, value); }
            remove { RemoveHandler(OpenedEvent, value); }
        }

        internal void RaiseOpenedEvent()
        {
            RaiseEvent(new RoutedEventArgs(OpenedEvent, this));
        }
        #endregion

        #region Closing Event
        public static RoutedEvent ClosingEvent = EventManager.RegisterRoutedEvent(
            "Closing",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(InfoCard));

        public event RoutedEventHandler Closing
        {
            add { AddHandler(ClosingEvent, value); }
            remove { RemoveHandler(ClosingEvent, value); }
        }

        internal void RaiseClosingEvent()
        {
            RaiseEvent(new RoutedEventArgs(ClosingEvent, this));
        }
        #endregion

        #region Closed Event
        public static RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent(
            "Closed",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(InfoCard));

        public event RoutedEventHandler Closed
        {
            add { AddHandler(ClosedEvent, value); }
            remove { RemoveHandler(ClosedEvent, value); }
        }

        internal void RaiseClosedEvent()
        {
            RaiseEvent(new RoutedEventArgs(ClosedEvent, this));
        }
        #endregion

        #region Pinned Event
        public static RoutedEvent PinnedEvent = EventManager.RegisterRoutedEvent(
            "Pinned",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(InfoCard));

        public event RoutedEventHandler Pinned
        {
            add { AddHandler(PinnedEvent, value); }
            remove { RemoveHandler(PinnedEvent, value); }
        }

        private void RaisePinnedEvent()
        {
            RaiseEvent(new RoutedEventArgs(PinnedEvent, this));
        }
        #endregion

        #region Unpinned Event
        public static RoutedEvent UnpinnedEvent = EventManager.RegisterRoutedEvent(
            "Unpinned",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(InfoCard));

        public event RoutedEventHandler Unpinned
        {
            add { AddHandler(UnpinnedEvent, value); }
            remove { RemoveHandler(UnpinnedEvent, value); }
        }

        private void RaiseUnpinnedEvent()
        {
            RaiseEvent(new RoutedEventArgs(UnpinnedEvent, this));
        }
        #endregion

        #region SubjectChanged Event
        public static RoutedEvent SubjectChangedEvent = EventManager.RegisterRoutedEvent(
            "SubjectChanged",
            RoutingStrategy.Direct,
            typeof(EventHandler<PropertyChangedRoutedEventArgs<IInfoCardSubject>>),
            typeof(InfoCard));

        public event EventHandler<PropertyChangedRoutedEventArgs<IInfoCardSubject>> SubjectChanged
        {
            add { AddHandler(SubjectChangedEvent, value); }
            remove { RemoveHandler(SubjectChangedEvent, value); }
        }

        private void OnSubjectChanged(IInfoCardSubject oldValue, IInfoCardSubject newValue)
        {
            RaiseEvent(
                new PropertyChangedRoutedEventArgs<IInfoCardSubject>(
                    SubjectChangedEvent,
                    oldValue,
                    newValue,
                    this));
        }
        #endregion

        #region TargetElement Property
        internal static readonly DependencyProperty TargetElementProperty = DependencyProperty.Register(
            "TargetElement",
            typeof(UIElement),
            typeof(InfoCard),
            new FrameworkPropertyMetadata((d, e) => ((InfoCard)d).UpdateInfoCardBindings()));

        internal UIElement TargetElement
        {
            get => (UIElement)GetValue(TargetElementProperty);
            set => SetValue(TargetElementProperty, value);
        }
        #endregion

        #region IsCurrentlyOpen Property
        internal static readonly DependencyProperty IsCurrentlyOpenProperty =
            DependencyProperty.RegisterAttached(
                "IsCurrentlyOpen",
                typeof(bool),
                typeof(InfoCard),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.Inherits,
                    OnIsCurrentlyOpenPropertyValueChanged));

        private static void OnIsCurrentlyOpenPropertyValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InfoCard infoCard)
            {
                infoCard.IsOpen = (bool)e.NewValue;
            }
        }

        internal static bool GetIsCurrentlyOpen(DependencyObject d)
        {
            if (d == null)
            {
                throw new ArgumentNullException("d");
            }

            return (bool)d.GetValue(IsCurrentlyOpenProperty);
        }

        internal static void SetIsCurrentlyOpen(DependencyObject d, bool value)
        {
            if (d == null)
            {
                throw new ArgumentNullException("d");
            }

            d.SetValue(IsCurrentlyOpenProperty, value);
        }
        #endregion

        #region LastCloseReason Property
        internal static readonly DependencyProperty LastCloseReasonProperty =
            DependencyProperty.Register(
                "LastCloseReason",
                typeof(InfoCardCloseReason),
                typeof(InfoCard),
                new FrameworkPropertyMetadata(InfoCardCloseReason.Other));

        internal InfoCardCloseReason LastCloseReason
        {
            get => (InfoCardCloseReason)GetValue(LastCloseReasonProperty);
            set => SetValue(LastCloseReasonProperty, value);
        }
        #endregion

        #region IsOpen Property
        protected static readonly DependencyPropertyKey IsOpenPropertyKey = DependencyProperty.RegisterReadOnly(
            "IsOpen",
            typeof(bool),
            typeof(InfoCardSite),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty IsOpenProperty = IsOpenPropertyKey.DependencyProperty;

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            protected set => SetValue(IsOpenPropertyKey, value);
        }
        #endregion

        #region CanPin Property
        public static readonly DependencyProperty CanPinProperty = DependencyProperty.Register(
            "CanPin",
            typeof(bool),
            typeof(InfoCard),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None,
                OnCanPinPropertyValueChanged,
                CoerceCanPinPropertyValue));

        private static void OnCanPinPropertyValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InfoCard infoCard = (InfoCard)d;
            if (infoCard.IsPinned && !(bool)e.NewValue)
            {
                infoCard.CoerceValue(IsPinnedProperty);
            }
        }

        private static object CoerceCanPinPropertyValue(DependencyObject d, object baseValue)
        {
            bool? localValue = (bool?)baseValue;
            if (localValue.HasValue)
            {
                return localValue.Value;
            }

            InfoCardSite infoCardSite = ((InfoCard)d).InfoCardSite;
            if (infoCardSite != null)
            {
                return infoCardSite.CanInfoCardsPin;
            }

            return true;
        }

        public bool CanPin
        {
            get => (bool)GetValue(CanPinProperty);
            set => SetValue(CanPinProperty, value);
        }
        #endregion

        #region IsPinned Property
        public static readonly DependencyProperty IsPinnedProperty = DependencyProperty.Register(
            "IsPinned",
            typeof(bool),
            typeof(InfoCard),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None,
                OnIsPinnedPropertyValueChanged,
                CoerceIsPinnedProperty));

        private static object CoerceIsPinnedProperty(DependencyObject d, object baseValue)
        {
            InfoCard infoCard = (InfoCard)d;
            if (!infoCard.CanPin)
            {
                return false;
            }

            return baseValue;
        }

        private static void OnIsPinnedPropertyValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InfoCard infoCard = (InfoCard)d;

            if ((bool)e.NewValue)
            {
                infoCard.RaisePinnedEvent();
            }
            else
            {
                infoCard.RaiseUnpinnedEvent();
                if (!infoCard.IsKeyboardFocusWithin)
                {
                    _ = infoCard.Close();
                }
            }
        }

        public bool IsPinned
        {
            get => (bool)GetValue(IsPinnedProperty);
            set => SetValue(IsPinnedProperty, value);
        }
        #endregion

        #region UniqueId Property
        public Guid UniqueId
        {
            get => (Guid)GetValue(InfoCardSite.UniqueIdProperty);
            internal set => SetValue(InfoCardSite.UniqueIdPropertyKey, value);
        }
        #endregion

        #region Location Property
        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register(
            "Location",
            typeof(Point?),
            typeof(InfoCard),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.None,
                OnLocationPropertyChanged));

        private static void OnLocationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Point? newValue = (Point?)e.NewValue;
            if (!newValue.HasValue)
            {
                return;
            }

            InfoCardHost infoCardHost = InfoCardHost.GetInfoCardHost(d);
            if (infoCardHost != null)
            {
                infoCardHost.Location = newValue;
            }
        }

        public Point? Location
        {
            get => (Point?)GetValue(LocationProperty);
            set => SetValue(LocationProperty, value);
        }
        #endregion

        #region LastFocusedComparer Class
        public class LastFocusedComparer : IComparer
        {
            private readonly IList _collection;

            internal LastFocusedComparer(IList collection)
            {
                _collection = collection ?? throw new ArgumentNullException("collection");
            }

            int IComparer.Compare(object x, object y)
            {
                int result = ((InfoCard)y).LastFocusedDateTime.CompareTo(((InfoCard)x).LastFocusedDateTime);
                if (result != 0)
                {
                    return result;
                }

                return _collection.IndexOf(x).CompareTo(_collection.IndexOf(y));
            }
        }
        #endregion

        #region Footer Property
        public static readonly DependencyProperty FooterProperty = DependencyProperty.Register(
            "Footer",
            typeof(object),
            typeof(InfoCard),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.None,
                (d, e) => d.SetValue(HasFooterPropertyKey, e.NewValue != null)));

        public object Footer
        {
            get => GetValue(FooterProperty);
            set => SetValue(FooterProperty, value);
        }
        #endregion

        #region FooterTemplate Property
        public static readonly DependencyProperty FooterTemplateProperty = DependencyProperty.Register(
            "FooterTemplate",
            typeof(DataTemplate),
            typeof(InfoCard),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.None));

        public DataTemplate FooterTemplate
        {
            get => (DataTemplate)GetValue(FooterTemplateProperty);
            set => SetValue(FooterTemplateProperty, value);
        }
        #endregion

        #region FooterTemplateSelector Property
        public static readonly DependencyProperty FooterTemplateSelectorProperty = DependencyProperty.Register(
            "FooterTemplateSelector",
            typeof(DataTemplateSelector),
            typeof(InfoCard),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.None));

        public DataTemplateSelector FooterTemplateSelector
        {
            get => (DataTemplateSelector)GetValue(FooterTemplateSelectorProperty);
            set => SetValue(FooterTemplateSelectorProperty, value);
        }
        #endregion

        #region FooterStringFormat Property
        public static readonly DependencyProperty FooterStringFormatProperty = DependencyProperty.Register(
            "FooterStringFormat",
            typeof(string),
            typeof(InfoCard),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.None));

        public string FooterStringFormat
        {
            get => (string)GetValue(FooterStringFormatProperty);
            set => SetValue(FooterStringFormatProperty, value);
        }
        #endregion

        #region HasFooter Property
        protected static readonly DependencyPropertyKey HasFooterPropertyKey = DependencyProperty.RegisterReadOnly(
            "HasFooter",
            typeof(bool),
            typeof(InfoCard),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty HasFooterProperty = HasFooterPropertyKey.DependencyProperty;

        public bool HasFooter
        {
            get => (bool)GetValue(HasFooterProperty);
            protected set => SetValue(HasFooterPropertyKey, value);
        }
        #endregion

        #region Subject Property
        public static readonly DependencyProperty SubjectProperty = DependencyProperty.Register(
            "Subject",
            typeof(IInfoCardSubject),
            typeof(InfoCard),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.None,
                OnSubjectChanged));

        private static void OnSubjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InfoCard infoCard = (InfoCard)d;

            IInfoCardSubject oldValue = e.OldValue as IInfoCardSubject;
            if (oldValue != null)
            {
                oldValue.DataChanged -= infoCard.OnSubjectDataChanged;
            }

            IInfoCardSubject newValue = e.NewValue as IInfoCardSubject;
            if (newValue != null)
            {
                newValue.DataChanged += infoCard.OnSubjectDataChanged;
            }

            infoCard.UpdateInfoCardBindings();
            infoCard.OnSubjectChanged(oldValue, newValue);
        }

        private void UpdateInfoCardBindings()
        {
            UIElement targetElement = TargetElement;
            IInfoCardSubject infoCardSubject = Subject;

            if ((infoCardSubject == null) || (targetElement == null))
            {
                BindingOperations.ClearBinding(this, HeaderProperty);
                BindingOperations.ClearBinding(this, FooterProperty);
                BindingOperations.ClearBinding(this, ContentProperty);

                BindingOperations.ClearBinding(this, HeaderTemplateProperty);
                BindingOperations.ClearBinding(this, FooterTemplateProperty);
                BindingOperations.ClearBinding(this, ContentTemplateProperty);

                BindingOperations.ClearBinding(this, HeaderStringFormatProperty);
                BindingOperations.ClearBinding(this, FooterStringFormatProperty);
                BindingOperations.ClearBinding(this, ContentStringFormatProperty);
            }
            else
            {
                _ = SetBinding(
                    HeaderProperty,
                    new Binding
                    {
                        Source = targetElement,
                        Path = new PropertyPath(InfoCardService.InfoCardHeaderProperty),
                        Mode = BindingMode.OneWay
                    });

                _ = SetBinding(
                    FooterProperty,
                    new Binding
                    {
                        Source = targetElement,
                        Path = new PropertyPath(InfoCardService.InfoCardFooterProperty),
                        Mode = BindingMode.OneWay
                    });

                _ = SetBinding(
                    ContentProperty,
                    new Binding
                    {
                        Source = targetElement,
                        Path = new PropertyPath(InfoCardService.InfoCardContentProperty),
                        Mode = BindingMode.OneWay
                    });

                _ = SetBinding(
                    HeaderTemplateProperty,
                    new Binding
                    {
                        Source = targetElement,
                        Path = new PropertyPath(InfoCardService.InfoCardHeaderTemplateProperty),
                        Mode = BindingMode.OneWay
                    });

                _ = SetBinding(
                    FooterTemplateProperty,
                    new Binding
                    {
                        Source = targetElement,
                        Path = new PropertyPath(InfoCardService.InfoCardFooterTemplateProperty),
                        Mode = BindingMode.OneWay
                    });

                _ = SetBinding(
                    ContentTemplateProperty,
                    new Binding
                    {
                        Source = targetElement,
                        Path = new PropertyPath(InfoCardService.InfoCardContentTemplateProperty),
                        Mode = BindingMode.OneWay
                    });

                _ = SetBinding(
                    HeaderStringFormatProperty,
                    new Binding
                    {
                        Source = targetElement,
                        Path = new PropertyPath(InfoCardService.InfoCardHeaderStringFormatProperty),
                        Mode = BindingMode.OneWay
                    });

                _ = SetBinding(
                    FooterStringFormatProperty,
                    new Binding
                    {
                        Source = targetElement,
                        Path = new PropertyPath(InfoCardService.InfoCardFooterStringFormatProperty),
                        Mode = BindingMode.OneWay
                    });

                _ = SetBinding(
                    ContentStringFormatProperty,
                    new Binding
                    {
                        Source = targetElement,
                        Path = new PropertyPath(InfoCardService.InfoCardContentStringFormatProperty),
                        Mode = BindingMode.OneWay
                    });

            }
        }

        private void OnSubjectDataChanged(object sender, EventArgs e)
        {
            if ((!(sender is IInfoCardSubject subject)) || (subject.Data == null))
            {
                if (IsOpen)
                {
                    _ = Close();
                }

                ClearValue(DataContextProperty);
            }
        }

        public IInfoCardSubject Subject
        {
            get => (IInfoCardSubject)GetValue(SubjectProperty);
            set => SetValue(SubjectProperty, value);
        }
        #endregion

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (e.Handled)
            {
                return;
            }

            IInfoCardWindow window = InfoCardHost.GetInfoCardWindow(this);
            if (window != null)
            {
                _ = window.Activate();
                window.DragMove();
                e.Handled = true;
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            _fader.StopFade();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if (!IsVisible)
            {
                return;
            }
            //
            // HACK: Use DragMoving and DragMoved events to inform an InfoCard that it
            //       is being dragged.
            //
            IInfoCardWindow infoCardWindow = InfoCardHost.GetInfoCardWindow(this);
            if (infoCardWindow != null && infoCardWindow == Mouse.Captured)
            {
                return;
            }

            if (IsPinned)
            {
                _fader.StartFade(this);
            }
            else if (!IsKeyboardFocusWithin)
            {
                _ = Close();
            }
        }

        public void Open()
        {
            InfoCardSite infoCardSite = InfoCardSite;
            if (infoCardSite == null)
            {
                throw new InvalidOperationException(SR.InfoCardNotRegistered);
            }

            infoCardSite.OpenInfoCard(this);
        }

        public bool Close()
        {
            InfoCardSite infoCardSite = InfoCardSite;
            if (infoCardSite != null)
            {
                return infoCardSite.Close(this, InfoCardCloseReason.Other, false);
            }

            return false;
        }
    }
}