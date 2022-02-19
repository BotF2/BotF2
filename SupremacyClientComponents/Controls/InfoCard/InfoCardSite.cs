using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Supremacy.Client.Controls
{
    public class InfoCardSite : Decorator
    {
        #region Fields
        private readonly List<InfoCardHost> _infoCardHosts;
        private readonly Canvas _canvas;
        private readonly InfoCardCollection _openInfoCardsCore;
        #endregion

        #region Constructors and Finalizers
        static InfoCardSite()
        {
            EventManager.RegisterClassHandler(
                typeof(InfoCardSite),
                InfoCardActivatedEvent,
                (EventHandler<InfoCardEventArgs>)OnInfoCardActivatedEvent);

            EventManager.RegisterClassHandler(
                typeof(InfoCardSite),
                InfoCardClosedEvent,
                (EventHandler<InfoCardEventArgs>)OnInfoCardClosedEvent);

            EventManager.RegisterClassHandler(
                typeof(InfoCardSite),
                InfoCardClosingEvent,
                (EventHandler<InfoCardEventArgs>)OnInfoCardClosingEvent);

            EventManager.RegisterClassHandler(
                typeof(InfoCardSite),
                InfoCardDeactivatedEvent,
                (EventHandler<InfoCardEventArgs>)OnInfoCardDeactivatedEvent);

            EventManager.RegisterClassHandler(
                typeof(InfoCardSite),
                InfoCardOpenedEvent,
                (EventHandler<InfoCardEventArgs>)OnInfoCardOpenedEvent);

            EventManager.RegisterClassHandler(
                typeof(InfoCardSite),
                InfoCardOpeningEvent,
                (EventHandler<InfoCardEventArgs>)OnInfoCardOpeningEvent);
        }

        public InfoCardSite()
        {
            _infoCardHosts = new List<InfoCardHost>();
            InfoCards = new InfoCardCollection();
            InfoCards.CollectionChanged += OnInfoCardsCollectionChanged;

            _canvas = new Canvas();

            AddVisualChild(_canvas);
            AddLogicalChild(_canvas);

            _openInfoCardsCore = new InfoCardCollection();
            OpenInfoCards = new ReadOnlyInfoCardCollection(InfoCards);

            AddHandler(LoadedEvent, (RoutedEventHandler)OnLoaded);
        }

        private void OnInfoCardsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ProcessCommonInfoCardsCollectionChanged(e);
        }

        private void ProcessCommonInfoCardsCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            ValidateUniqueIds();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (object item in e.NewItems)
                    {
                        if (item is InfoCard infoCard)
                        {
                            UpdateRegisteredInfoCardSite(infoCard, true);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    if (!DesignerProperties.GetIsInDesignMode(this))
                    {
                        throw new NotSupportedException();
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (object item in e.OldItems)
                    {
                        if (!(item is InfoCard infoCard))
                        {
                            continue;
                        }

                        if (infoCard.IsOpen)
                        {
                            _ = infoCard.Close();
                        }

                        UnregisterInfoCardSiteObject(infoCard);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (!DesignerProperties.GetIsInDesignMode(this))
                    {
                        throw new NotSupportedException();
                    }

                    break;
            }
        }

        private void ValidateUniqueIds()
        {
            Dictionary<Guid, bool> uniqueIds = new Dictionary<Guid, bool>();

            foreach (InfoCard infoCard in InfoCards)
            {
                if (uniqueIds.ContainsKey(infoCard.UniqueId))
                {
                    throw new InvalidOperationException();
                }

                uniqueIds[infoCard.UniqueId] = true;
            }
        }

        #endregion

        #region Events

        #region InfoCardActivated Event
        public static readonly RoutedEvent InfoCardActivatedEvent = EventManager.RegisterRoutedEvent(
            "InfoCardActivated",
            RoutingStrategy.Bubble,
            typeof(EventHandler<InfoCardEventArgs>),
            typeof(InfoCardSite));

        public event EventHandler<InfoCardEventArgs> InfoCardActivated
        {
            add { AddHandler(InfoCardActivatedEvent, value); }
            remove { RemoveHandler(InfoCardActivatedEvent, value); }
        }
        #endregion

        #region InfoCardDeactivated Event
        public static readonly RoutedEvent InfoCardDeactivatedEvent = EventManager.RegisterRoutedEvent(
            "InfoCardDeactivated",
            RoutingStrategy.Bubble,
            typeof(EventHandler<InfoCardEventArgs>),
            typeof(InfoCardSite));

        public event EventHandler<InfoCardEventArgs> InfoCardDeactivated
        {
            add { AddHandler(InfoCardDeactivatedEvent, value); }
            remove { RemoveHandler(InfoCardDeactivatedEvent, value); }
        }
        #endregion

        #region InfoCardClosing Event
        public static readonly RoutedEvent InfoCardClosingEvent = EventManager.RegisterRoutedEvent(
            "InfoCardClosing",
            RoutingStrategy.Bubble,
            typeof(EventHandler<InfoCardEventArgs>),
            typeof(InfoCardSite));

        public event EventHandler<InfoCardEventArgs> InfoCardClosing
        {
            add { AddHandler(InfoCardClosingEvent, value); }
            remove { RemoveHandler(InfoCardClosingEvent, value); }
        }
        #endregion

        #region InfoCardClosed Event
        public static readonly RoutedEvent InfoCardClosedEvent = EventManager.RegisterRoutedEvent(
            "InfoCardClosed",
            RoutingStrategy.Bubble,
            typeof(EventHandler<InfoCardEventArgs>),
            typeof(InfoCardSite));

        public event EventHandler<InfoCardEventArgs> InfoCardClosed
        {
            add { AddHandler(InfoCardClosedEvent, value); }
            remove { RemoveHandler(InfoCardClosedEvent, value); }
        }
        #endregion

        #region InfoCardOpening Event
        public static readonly RoutedEvent InfoCardOpeningEvent = EventManager.RegisterRoutedEvent(
            "InfoCardOpening",
            RoutingStrategy.Bubble,
            typeof(EventHandler<InfoCardEventArgs>),
            typeof(InfoCardSite));

        public event EventHandler<InfoCardEventArgs> InfoCardOpening
        {
            add { AddHandler(InfoCardOpeningEvent, value); }
            remove { RemoveHandler(InfoCardOpeningEvent, value); }
        }
        #endregion

        #region InfoCardOpened Event
        public static readonly RoutedEvent InfoCardOpenedEvent = EventManager.RegisterRoutedEvent(
            "InfoCardOpened",
            RoutingStrategy.Bubble,
            typeof(EventHandler<InfoCardEventArgs>),
            typeof(InfoCardSite));

        public event EventHandler<InfoCardEventArgs> InfoCardOpened
        {
            add { AddHandler(InfoCardOpenedEvent, value); }
            remove { RemoveHandler(InfoCardOpenedEvent, value); }
        }
        #endregion

        #region InfoCardPinned Event
        public static readonly RoutedEvent InfoCardPinnedEvent = EventManager.RegisterRoutedEvent(
            "InfoCardPinned",
            RoutingStrategy.Bubble,
            typeof(EventHandler<InfoCardEventArgs>),
            typeof(InfoCardSite));

        public event EventHandler<InfoCardEventArgs> InfoCardPinned
        {
            add { AddHandler(InfoCardPinnedEvent, value); }
            remove { RemoveHandler(InfoCardPinnedEvent, value); }
        }
        #endregion

        #region InfoCardUnpinned Event
        public static readonly RoutedEvent InfoCardUnpinnedEvent = EventManager.RegisterRoutedEvent(
            "InfoCardUnpinned",
            RoutingStrategy.Bubble,
            typeof(EventHandler<InfoCardEventArgs>),
            typeof(InfoCardSite));

        public event EventHandler<InfoCardEventArgs> InfoCardUnpinned
        {
            add { AddHandler(InfoCardUnpinnedEvent, value); }
            remove { RemoveHandler(InfoCardUnpinnedEvent, value); }
        }
        #endregion

        #endregion

        #region Properties

        #region HasOpenInfoCards Property
        public bool HasOpenInfoCards => OpenInfoCards.Any();
        #endregion

        #region OpenInfoCards Property
        public ReadOnlyInfoCardCollection OpenInfoCards { get; }
        #endregion

        #region CanInfoCardsPin Property
        public static readonly DependencyProperty CanInfoCardsPinProperty = DependencyProperty.Register(
            "CanInfoCardsPin",
            typeof(bool),
            typeof(InfoCardSite),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public bool CanInfoCardsPin
        {
            get => (bool)GetValue(CanInfoCardsPinProperty);
            set => SetValue(CanInfoCardsPinProperty, value);
        }
        #endregion

        #region InfoCardSite Attached Property
        internal static readonly DependencyPropertyKey InfoCardSitePropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "InfoCardSite",
                typeof(InfoCardSite),
                typeof(InfoCardSite),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.Inherits,
                    OnInfoCardSitePropertyValueChanged));

        internal static DependencyProperty InfoSiteInfoCardProperty = InfoCardSitePropertyKey.DependencyProperty;

        private static void OnInfoCardSitePropertyValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is InfoCardSite oldInfoCardSite)
            {
                oldInfoCardSite.UnregisterInfoCardSiteObject(d);
            }

            if (e.NewValue is InfoCardSite newInfoCardSite)
            {
                newInfoCardSite.RegisterInfoCardSiteObject(d);
            }
        }

        public static InfoCardSite GetInfoCardSite(DependencyObject o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            return o.GetValue(InfoSiteInfoCardProperty) as InfoCardSite;
        }

        internal static void SetInfoCardSite(DependencyObject o, InfoCardSite value)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            o.SetValue(InfoCardSitePropertyKey, value);
            if (o is InfoCard)
            {
                o.CoerceValue(InfoCard.CanPinProperty);
            }
        }
        #endregion

        #region InfoCards Property
        public InfoCardCollection InfoCards { get; }
        #endregion

        #region ActiveInfoCard Property
        internal static readonly DependencyPropertyKey ActiveInfoCardPropertyKey = DependencyProperty.RegisterReadOnly(
            "ActiveInfoCard",
            typeof(InfoCard),
            typeof(InfoCardSite),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.None,
                OnActiveInfoCardPropertyValueChanged));

        public static readonly DependencyProperty ActiveInfoCardProperty = ActiveInfoCardPropertyKey.DependencyProperty;

        public InfoCard ActiveInfoCard
        {
            get => (InfoCard)GetValue(ActiveInfoCardProperty);
            internal set => SetValue(ActiveInfoCardPropertyKey, value);
        }

        private static void OnActiveInfoCardPropertyValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((InfoCardSite)d).OnActiveInfoCardChanged(e.OldValue as InfoCard, e.NewValue as InfoCard);
        }
        #endregion

        #region LastActiveInfoCard Property
        internal static readonly DependencyPropertyKey LastActiveInfoCardPropertyKey = DependencyProperty.RegisterReadOnly(
            "LastActiveInfoCard",
            typeof(InfoCard),
            typeof(InfoCardSite),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty LastActiveInfoCardProperty = LastActiveInfoCardPropertyKey.DependencyProperty;

        public InfoCard LastActiveInfoCard
        {
            get => (InfoCard)GetValue(LastActiveInfoCardProperty);
            internal set => SetValue(LastActiveInfoCardPropertyKey, value);
        }
        #endregion

        #region UseHostedInfoCardWindows Property
        public static readonly DependencyProperty UseHostedInfoCardWindowsProperty =
            DependencyProperty.Register(
                "UseHostedInfoCardWindows",
                typeof(bool),
                typeof(InfoCardSite),
                new FrameworkPropertyMetadata(
                    false,
                    OnUseHostedInfoCardWindowsPropertyValueChanged));

        private static void OnUseHostedInfoCardWindowsPropertyValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((InfoCardSite)d).CloseInfoCardWindows(InfoCardCloseReason.InfoCardWindowClosed);
        }

        public bool UseHostedInfoCardWindows
        {
            get => (bool)GetValue(UseHostedInfoCardWindowsProperty);
            set => SetValue(UseHostedInfoCardWindowsProperty, value);
        }
        #endregion

        #region UniqueId Property
        internal static readonly DependencyPropertyKey UniqueIdPropertyKey = DependencyProperty.RegisterReadOnly(
            "UniqueId",
            typeof(Guid),
            typeof(InfoCardSite),
            new FrameworkPropertyMetadata(
                Guid.Empty,
                FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty UniqueIdProperty = UniqueIdPropertyKey.DependencyProperty;
        #endregion

        #region InactiveInfoCardFadeOpacity Property
        public static readonly DependencyProperty InactiveInfoCardFadeOpacityProperty = DependencyProperty.Register(
            "InactiveInfoCardFadeOpacity",
            typeof(double),
            typeof(InfoCardSite),
            new FrameworkPropertyMetadata(
                0.5,
                FrameworkPropertyMetadataOptions.None));

        public double InactiveInfoCardFadeOpacity
        {
            get => (double)GetValue(InactiveInfoCardFadeOpacityProperty);
            set => SetValue(InactiveInfoCardFadeOpacityProperty, value);
        }
        #endregion

        #region IsInactiveInfoCardFadeEnabled Property
        public static readonly DependencyProperty IsInactiveInfoCardFadeEnabledProperty = DependencyProperty.Register(
            "IsInactiveInfoCardFadeEnabled",
            typeof(bool),
            typeof(InfoCardSite),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None,
                OnIsInactiveInfoCardFadeEnabledChanged));

        private static void OnIsInactiveInfoCardFadeEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InfoCardSite infoCardSite = (InfoCardSite)d;
            foreach (InfoCard infoCard in infoCardSite.InfoCards)
            {
                infoCard.ResetFade();
            }
        }

        public bool IsInactiveInfoCardFadeEnabled
        {
            get => (bool)GetValue(IsInactiveInfoCardFadeEnabledProperty);
            set => SetValue(IsInactiveInfoCardFadeEnabledProperty, value);
        }
        #endregion

        #region InactiveInfoCardFadeDuration Property
        public static readonly DependencyProperty InactiveInfoCardFadeDurationProperty = DependencyProperty.Register(
            "InactiveInfoCardFadeDuration",
            typeof(Duration),
            typeof(InfoCardSite),
            new FrameworkPropertyMetadata(
                new Duration(TimeSpan.FromSeconds(0.5)),
                FrameworkPropertyMetadataOptions.None));

        public Duration InactiveInfoCardFadeDuration
        {
            get => (Duration)GetValue(InactiveInfoCardFadeDurationProperty);
            set => SetValue(InactiveInfoCardFadeDurationProperty, value);
        }
        #endregion

        #region InactiveInfoCardFadeDelay Property
        public static readonly DependencyProperty InactiveInfoCardFadeDelayProperty = DependencyProperty.Register(
            "InactiveInfoCardFadeDelay",
            typeof(TimeSpan),
            typeof(InfoCardSite),
            new FrameworkPropertyMetadata(
                TimeSpan.FromSeconds(0.5),
                FrameworkPropertyMetadataOptions.None));

        public TimeSpan InactiveInfoCardFadeDelay
        {
            get => (TimeSpan)GetValue(InactiveInfoCardFadeDelayProperty);
            set => SetValue(InactiveInfoCardFadeDelayProperty, value);
        }
        #endregion

        #endregion

        #region Protected Methods
        protected virtual void OnInfoCardActivated(InfoCardEventArgs e) { }
        protected virtual void OnInfoCardClosed(InfoCardEventArgs e) { }
        protected virtual void OnInfoCardClosing(InfoCardEventArgs e) { }
        protected virtual void OnInfoCardDeactivated(InfoCardEventArgs e) { }
        protected virtual void OnInfoCardOpened(InfoCardEventArgs e) { }
        protected virtual void OnInfoCardOpening(InfoCardEventArgs e) { }

        protected virtual void OnActiveInfoCardChanged(InfoCard oldValue, InfoCard newValue)
        {
            if (oldValue != null)
            {
                RaiseInfoCardDeactivatedEvent(oldValue);
            }

            if (newValue != null)
            {
                RaiseInfoCardActivatedEvent(newValue);
            }
        }

        private void RaiseInfoCardActivatedEvent(InfoCard newValue)
        {
            RaiseEvent(new InfoCardEventArgs(newValue, InfoCardActivatedEvent, this));
            newValue.RaiseActivatedEvent();
        }

        private void RaiseInfoCardDeactivatedEvent(InfoCard oldValue)
        {
            RaiseEvent(new InfoCardEventArgs(oldValue, InfoCardDeactivatedEvent, this));
            oldValue.RaiseDeactivatedEvent();
        }
        #endregion

        #region Internal Methods
        internal InfoCard GetInfoCardFromHost(InfoCardHost host)
        {
            return host.Content as InfoCard;
        }

        internal bool Close(InfoCard infoCard, InfoCardCloseReason closeReason, bool force)
        {
            if ((infoCard == null) || (!infoCard.IsOpen))
            {
                return false;
            }

            // Raise closing event
            InfoCardEventArgs closingEventArgs = new InfoCardEventArgs(infoCard, InfoCardClosingEvent, this);
            RaiseEvent(closingEventArgs);
            infoCard.RaiseClosingEvent();

            if (!force && closingEventArgs.Cancel)
            {
                return false;
            }

            EventNotifier notifier = new EventNotifier();
            notifier.Subscribe(infoCard);

            infoCard.Measure(new Size(0, 0));
            infoCard.ClearValue(Expander.IsExpandedProperty);
            infoCard.ClearValue(InfoCard.IsPinnedProperty);

            InfoCardHost infoCardHost = InfoCardHost.GetInfoCardHost(infoCard);
            if (infoCardHost != null)
            {
                infoCardHost.Content = null;
            }

            notifier.RaiseEvents();

            if (InfoCardService.GetUnregisterInfoCardOnClose(infoCard))
            {
                _ = InfoCards.Remove(infoCard);
            }

            RaiseEvent(new InfoCardEventArgs(infoCard, InfoCardClosedEvent, this));
            infoCard.RaiseClosedEvent();

            infoCard.LastCloseReason = closeReason;

            return true;
        }

        internal void AddCanvasChild(UIElement element)
        {
            if (!_canvas.Children.Contains(element))
            {
                _ = _canvas.Children.Add(element);
            }
        }

        internal void BringToFront(UIElement element)
        {
            int zIndex = 0;

            foreach (UIElement child in _canvas.Children.OfType<UIElement>())
            {
                if (child is IInfoCardWindow)
                {
                    zIndex = Math.Max(zIndex, Panel.GetZIndex(child));
                }
            }

            Panel.SetZIndex(element, zIndex + 1);
        }

        internal void RemoveCanvasChild(UIElement element)
        {
            if (_canvas.Children.Contains(element))
            {
                _canvas.Children.Remove(element);
            }
        }

        internal void UpdateOpenInfoCards()
        {
            _openInfoCardsCore.BeginUpdate();
            try
            {
                _openInfoCardsCore.Clear();
                _openInfoCardsCore.AddRange(OpenInfoCards.Where(o => o.IsOpen));
            }
            finally
            {
                _openInfoCardsCore.EndUpdate();
            }
        }
        #endregion

        #region Private Methods
        private static void OnInfoCardActivatedEvent(object sender, InfoCardEventArgs e)
        {
            ((InfoCardSite)sender).OnInfoCardActivated(e);
        }

        private static void OnInfoCardClosedEvent(object sender, InfoCardEventArgs e)
        {
            ((InfoCardSite)sender).OnInfoCardClosed(e);
        }

        private static void OnInfoCardClosingEvent(object sender, InfoCardEventArgs e)
        {
            ((InfoCardSite)sender).OnInfoCardClosing(e);
        }

        private static void OnInfoCardDeactivatedEvent(object sender, InfoCardEventArgs e)
        {
            ((InfoCardSite)sender).OnInfoCardDeactivated(e);
        }

        private static void OnInfoCardOpenedEvent(object sender, InfoCardEventArgs e)
        {
            ((InfoCardSite)sender).OnInfoCardOpened(e);
        }

        private static void OnInfoCardOpeningEvent(object sender, InfoCardEventArgs e)
        {
            ((InfoCardSite)sender).OnInfoCardOpening(e);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateOpenInfoCards();
            CommandManager.InvalidateRequerySuggested();
        }

        private void UpdateRegisteredInfoCardSite(InfoCard infoCard, bool register)
        {
            if (register)
            {
                if (infoCard.RegisteredInfoCardSite != this)
                {
                    if (infoCard.RegisteredInfoCardSite != null)
                    {
                        throw new InvalidOperationException(SR.InfoCardAlreadyRegistered);
                    }

                    infoCard.RegisteredInfoCardSite = this;
                }
            }
            else if (infoCard.RegisteredInfoCardSite == this)
            {
                infoCard.RegisteredInfoCardSite = null;
            }
        }

        private void RegisterInfoCard(InfoCard infoCard)
        {
            UpdateRegisteredInfoCardSite(infoCard, true);

            if (!InfoCards.Contains(infoCard))
            {
                InfoCards.Add(infoCard);
            }
        }

        private void RegisterInfoCardSiteObject(DependencyObject o)
        {
            if (o is InfoCard infoCard)
            {
                if (!infoCard.IsContainerForItem)
                {
                    RegisterInfoCard(infoCard);
                }
            }
            else
            {
                if (o is InfoCardHost infoCardHost)
                {
                    _infoCardHosts.Add(infoCardHost);

                    infoCardHost.LayoutChanged += OnInfoCardHostLayoutChanged;
                    infoCardHost.Activated += OnInfoCardHostActivated;
                }
            }
        }

        private void OnInfoCardHostActivated(object sender, EventArgs e)
        {
            InfoCardHost container = (InfoCardHost)sender;
            _ = _infoCardHosts.Remove(container);
            _infoCardHosts.Insert(0, container);
        }

        private void OnInfoCardHostLayoutChanged(object sender, RoutedEventArgs e)
        {
            // If a rafting host...
            if (sender is InfoCardHost container)
            {
                CloseInfoCardWindowIfEmpty(container);

                if (container.Content == null)
                {
                    container.InfoCardSite = null;
                }
            }

            ClearInternalCache();
        }

        private void CloseInfoCardWindowIfEmpty(InfoCardHost container)
        {
            IInfoCardWindow window = container.InfoCardWindow;
            if (!HasVisualContent(container) && (window != null) && (!window.IsClosing))
            {
                window.Close(InfoCardCloseReason.InfoCardWindowClosed);
            }
        }

        internal bool HasVisualContent(InfoCardHost infoCardHost)
        {
            return GetInfoCardFromHost(infoCardHost) != null;
        }

        private void ClearInternalCache()
        {
            if ((LastActiveInfoCard != null) && (!LastActiveInfoCard.IsOpen))
            {
                LastActiveInfoCard = null;
            }

            if ((ActiveInfoCard != null) && ((!ActiveInfoCard.IsOpen) || (!ActiveInfoCard.IsKeyboardFocusWithin)))
            {
                ActiveInfoCard = null;
            }
        }

        private void UnregisterInfoCardSiteObject(DependencyObject o)
        {
            if (!(o is InfoCardHost infoCardHost))
            {
                return;
            }

            _ = _infoCardHosts.Remove(infoCardHost);

            infoCardHost.LayoutChanged -= OnInfoCardHostLayoutChanged;
            infoCardHost.Activated -= OnInfoCardHostActivated;
        }
        #endregion

        #region InfoCard Window Management Methods
        internal IInfoCardWindow[] GetInfoCardWindows()
        {
            return _infoCardHosts.Where(o => o.InfoCardWindow != null).Select(o => o.InfoCardWindow).ToArray();
        }

        internal void CloseInfoCardWindows(InfoCardCloseReason closeReason)
        {
            IInfoCardWindow[] infoCardWindows = GetInfoCardWindows();
            foreach (IInfoCardWindow infoCardWindow in infoCardWindows)
            {
                if (!infoCardWindow.IsClosing)
                {
                    infoCardWindow.Close(closeReason);
                }
            }
        }

        internal InfoCardHost CreateInfoCardHost()
        {
            return new InfoCardHost { InfoCardSite = this };
        }

        protected internal virtual IInfoCardWindow CreateRaftingWindow(InfoCardHost infoCardHost)
        {
            if (UseHostedInfoCardWindows)
            {
                return new InfoCardWindowControl(infoCardHost);
            }

            return CreateRaftingWindowForWindows(infoCardHost);
        }

        private IInfoCardWindow CreateRaftingWindowForWindows(InfoCardHost infoCardHost)
        {
            InfoCardWindow window = new InfoCardWindow(infoCardHost);

            _ = window.SetBinding(
                CanInfoCardsPinProperty,
                InfoCardHelper.CreateBinding(this, CanInfoCardsPinProperty));

            _ = window.SetBinding(
                DataContextProperty,
                InfoCardHelper.CreateBinding(this, DataContextProperty));

            return window;
        }

        internal void OpenInfoCardCore(InfoCard infoCard)
        {
            IInfoCardWindow infoCardWindow;

            InfoCardHost infoCardHost = InfoCardHost.GetInfoCardHost(infoCard);
            if ((infoCardHost != null) && infoCardHost.IsVisible)
            {
                infoCardWindow = infoCardHost.InfoCardWindow;
                if (infoCardWindow != null)
                {
                    return;
                }
            }

            if (infoCardHost == null)
            {
                infoCardHost = CreateInfoCardHost();
                infoCardHost.Location = infoCard.Location;
            }

            infoCardHost.Content = infoCard;

            infoCardWindow = CreateRaftingWindow(infoCardHost);
            infoCardWindow.SnapToScreen();

            infoCardWindow.Show();
            _ = infoCardWindow.Activate();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == IsVisibleProperty)
            {
                UpdateInfoCardWindows((bool)e.NewValue);
            }
        }

        private void UpdateInfoCardWindows(bool isVisible)
        {
            if (isVisible)
            {
                foreach (InfoCard infoCard in InfoCards)
                {
                    if ((infoCard.LastCloseReason == InfoCardCloseReason.InfoCardSiteUnloaded) && !infoCard.IsOpen)
                    {
                        infoCard.Open();
                    }
                }
            }
            else
            {
                _ = Dispatcher.BeginInvoke(
                    (Action)
                    delegate
                    {
                        if (IsVisible)
                        {
                            return;
                        }

                        if (!UseHostedInfoCardWindows)
                        {
                            CloseInfoCardWindows(InfoCardCloseReason.InfoCardSiteUnloaded);
                        }
                    },
                    DispatcherPriority.Send);
            }
        }

        internal void RaiseOpenedEvent(InfoCard infoCard)
        {
            RaiseEvent(new InfoCardEventArgs(infoCard, InfoCardOpenedEvent, this));
            infoCard.RaiseOpenedEvent();
        }

        internal void RaiseOpeningEvent(InfoCard infoCard)
        {
            RaiseEvent(new InfoCardEventArgs(infoCard, InfoCardOpeningEvent, this));
            infoCard.RaiseOpeningEvent();
        }

        internal void OpenInfoCard(InfoCard infoCard)
        {
            if ((infoCard == null) || infoCard.IsOpen)
            {
                return;
            }

            RaiseOpeningEvent(infoCard);
            OpenInfoCardCore(infoCard);
            RaiseOpenedEvent(infoCard);
        }
        #endregion

        #region Visual Child Enumeration
        protected override int VisualChildrenCount => base.VisualChildrenCount + 1;

        protected override Visual GetVisualChild(int index)
        {
            if (index < base.VisualChildrenCount)
            {
                return base.GetVisualChild(index);
            }

            return _canvas;
        }
        #endregion

        #region Measure and Arrange Overrides
        protected override Size ArrangeOverride(Size finalSize)
        {
            _canvas.Arrange(new Rect(new Point(), finalSize));
            return base.ArrangeOverride(finalSize);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _canvas.Measure(constraint);
            return base.MeasureOverride(constraint);
        }
        #endregion
    }
}