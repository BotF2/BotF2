using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Types;

using System.Linq;

using Supremacy.Utility;

namespace Supremacy.Client.Controls
{
    public delegate Point CustomInfoCardPlacementCallback(Size infoCardSize, DependencyObject ownerElement, Point defaultLocation);

    public class InfoCardService
    {
        private readonly WeakDictionary<Window, InfoCardSite> _generatedSites;
        private readonly WeakDictionary<IInfoCardSubject, InfoCard> _infoCardCache;

        private InfoCard _currentInfoCard;
        private WeakReference _lastChecked;
        private WeakReference _lastMouseDirectlyOver;
        private WeakReference _lastMouseOverWithInfoCard;
        private bool _quickShow;
        private DispatcherTimer _infoCardTimer;

        internal const int BetweenShowDelay = 100;
        internal const int InitialShowDelay = 900;
        internal const int ShowDuration = 20000;

        private static InfoCardService _current;

        #region Routed Events
        internal static readonly RoutedEvent FindInfoCardEvent = EventManager.RegisterRoutedEvent(
            "FindInfoCard",
            RoutingStrategy.Bubble,
            typeof(FindInfoCardEventHandler),
            typeof(InfoCardService));

        public static readonly RoutedEvent InfoCardClosingEvent = EventManager.RegisterRoutedEvent(
            "InfoCardClosing",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(InfoCardService));

        public static readonly RoutedEvent InfoCardOpeningEvent = EventManager.RegisterRoutedEvent(
            "InfoCardOpening",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(InfoCardService));
        #endregion

        #region Dependency Properties

        #region InfoCard Attached Property
        public static readonly DependencyProperty InfoCardProperty = DependencyProperty.RegisterAttached(
            "InfoCard",
            typeof(InfoCard),
            typeof(InfoCardService));
        #endregion

        #region InfoCardSubject Attached Property
        public static readonly DependencyProperty InfoCardSubjectProperty = DependencyProperty.RegisterAttached(
            "InfoCardSubject",
            typeof(IInfoCardSubject),
            typeof(InfoCardService),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.None));

        public static IInfoCardSubject GetInfoCardSubject(DependencyObject source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return (IInfoCardSubject)source.GetValue(InfoCardSubjectProperty);
        }

        public static void SetInfoCardSubject(DependencyObject source, IInfoCardSubject value)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            source.SetValue(InfoCardSubjectProperty, value);
        }
        #endregion

        #region InfoCardHeader Attached Property
        public static readonly DependencyProperty InfoCardHeaderProperty = DependencyProperty.RegisterAttached(
            "InfoCardHeader",
            typeof(object),
            typeof(InfoCardService),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.None));

        public static object GetInfoCardHeader(DependencyObject source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return source.GetValue(InfoCardHeaderProperty);
        }

        public static void SetInfoCardHeader(DependencyObject source, object value)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            source.SetValue(InfoCardHeaderProperty, value);
        }
        #endregion

        #region InfoCardFooter Attached Property
        public static readonly DependencyProperty InfoCardFooterProperty = DependencyProperty.RegisterAttached(
            "InfoCardFooter",
            typeof(object),
            typeof(InfoCardService),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.None));

        public static object GetInfoCardFooter(DependencyObject source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return source.GetValue(InfoCardFooterProperty);
        }

        public static void SetInfoCardFooter(DependencyObject source, object value)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            source.SetValue(InfoCardFooterProperty, value);
        }
        #endregion

        #region InfoCardContent Attached Property
        public static readonly DependencyProperty InfoCardContentProperty = DependencyProperty.RegisterAttached(
            "InfoCardContent",
            typeof(object),
            typeof(InfoCardService),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.None));

        public static object GetInfoCardContent(DependencyObject source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return source.GetValue(InfoCardContentProperty);
        }

        public static void SetInfoCardContent(DependencyObject source, object value)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            source.SetValue(InfoCardContentProperty, value);
        }
        #endregion

        #region InfoCardHeaderTemplate Attached Property
        public static readonly DependencyProperty InfoCardHeaderTemplateProperty = DependencyProperty.RegisterAttached(
            "InfoCardHeaderTemplate",
            typeof(DataTemplate),
            typeof(InfoCardService),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.None));

        public static DataTemplate GetInfoCardHeaderTemplate(DependencyObject source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return (DataTemplate)source.GetValue(InfoCardHeaderTemplateProperty);
        }

        public static void SetInfoCardHeaderTemplate(DependencyObject source, DataTemplate value)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            source.SetValue(InfoCardHeaderTemplateProperty, value);
        }
        #endregion

        #region InfoCardFooterTemplate Attached Property
        public static readonly DependencyProperty InfoCardFooterTemplateProperty = DependencyProperty.RegisterAttached(
            "InfoCardFooterTemplate",
            typeof(DataTemplate),
            typeof(InfoCardService),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.None));

        public static DataTemplate GetInfoCardFooterTemplate(DependencyObject source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return (DataTemplate)source.GetValue(InfoCardFooterTemplateProperty);
        }

        public static void SetInfoCardFooterTemplate(DependencyObject source, DataTemplate value)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            source.SetValue(InfoCardFooterTemplateProperty, value);
        }
        #endregion

        #region InfoCardContentTemplate Attached Property
        public static readonly DependencyProperty InfoCardContentTemplateProperty = DependencyProperty.RegisterAttached(
            "InfoCardContentTemplate",
            typeof(DataTemplate),
            typeof(InfoCardService),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.None));

        public static DataTemplate GetInfoCardContentTemplate(DependencyObject source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return (DataTemplate)source.GetValue(InfoCardContentTemplateProperty);
        }

        public static void SetInfoCardContentTemplate(DependencyObject source, DataTemplate value)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            source.SetValue(InfoCardContentTemplateProperty, value);
        }
        #endregion

        #region InfoCardHeaderStringFormat Attached Property
        public static readonly DependencyProperty InfoCardHeaderStringFormatProperty = DependencyProperty.RegisterAttached(
            "InfoCardHeaderStringFormat",
            typeof(string),
            typeof(InfoCardService),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.None));

        public static string GetInfoCardHeaderStringFormat(DependencyObject source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return (string)source.GetValue(InfoCardHeaderStringFormatProperty);
        }

        public static void SetInfoCardHeaderStringFormat(DependencyObject source, string value)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            source.SetValue(InfoCardHeaderStringFormatProperty, value);
        }
        #endregion

        #region InfoCardFooterStringFormat Attached Property
        public static readonly DependencyProperty InfoCardFooterStringFormatProperty = DependencyProperty.RegisterAttached(
            "InfoCardFooterStringFormat",
            typeof(string),
            typeof(InfoCardService),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.None));

        public static string GetInfoCardFooterStringFormat(DependencyObject source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return (string)source.GetValue(InfoCardFooterStringFormatProperty);
        }

        public static void SetInfoCardFooterStringFormat(DependencyObject source, string value)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            source.SetValue(InfoCardFooterStringFormatProperty, value);
        }
        #endregion

        #region InfoCardContentStringFormat Attached Property
        public static readonly DependencyProperty InfoCardContentStringFormatProperty = DependencyProperty.RegisterAttached(
            "InfoCardContentStringFormat",
            typeof(string),
            typeof(InfoCardService),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.None));

        public static string GetInfoCardContentStringFormat(DependencyObject source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return (string)source.GetValue(InfoCardContentStringFormatProperty);
        }

        public static void SetInfoCardContentStringFormat(DependencyObject source, string value)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            source.SetValue(InfoCardContentStringFormatProperty, value);
        }
        #endregion

        #region CustomInfoCardPlacementCallback Attached Property
        public static readonly DependencyProperty CustomInfoCardPlacementCallbackProperty = DependencyProperty.RegisterAttached(
            "CustomInfoCardPlacementCallback",
            typeof(CustomInfoCardPlacementCallback),
            typeof(InfoCardService),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.None));

        public static CustomInfoCardPlacementCallback GetCustomInfoCardPlacementCallback(DependencyObject source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return (CustomInfoCardPlacementCallback)source.GetValue(CustomInfoCardPlacementCallbackProperty);
        }

        public static void SetCustomInfoCardPlacementCallback(DependencyObject source, CustomInfoCardPlacementCallback value)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            source.SetValue(CustomInfoCardPlacementCallbackProperty, value);
        }
        #endregion

        #region UnregisterInfoCardOnClose Attached Property
        internal static readonly DependencyProperty UnregisterInfoCardOnCloseProperty = DependencyProperty.
            RegisterAttached(
            "UnregisterInfoCardOnClose",
            typeof(bool),
            typeof(InfoCardService),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None));

        internal static bool GetUnregisterInfoCardOnClose(InfoCard source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return (bool)source.GetValue(UnregisterInfoCardOnCloseProperty);
        }

        internal static void SetUnregisterInfoCardOnClose(InfoCard source, bool value)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            source.SetValue(UnregisterInfoCardOnCloseProperty, value);
        }
        #endregion

        #endregion

        // NESTED TYPES

        #region FindInfoCardEventArgs class
        internal class FindInfoCardEventArgs : RoutedEventArgs
        {
            // OBJECT

            public FindInfoCardEventArgs() : base(FindInfoCardEvent) {}

            // PUBLIC PROCEDURES

            protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
            {
                ((FindInfoCardEventHandler)genericHandler)(genericTarget, this);
            }

            public bool KeepCurrentActive { get; set; }

            public DependencyObject TargetElement { get; set; }
        }
        #endregion

        #region FindInfoCardEventHandler delegate
        internal delegate void FindInfoCardEventHandler(object sender, FindInfoCardEventArgs e);
        #endregion

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static InfoCardService()
        {
            // Register event handlers
            EventManager.RegisterClassHandler(
                typeof(UIElement),
                FindInfoCardEvent,
                new FindInfoCardEventHandler(OnFindInfoCard));

            EventManager.RegisterClassHandler(
                typeof(ContentElement),
                FindInfoCardEvent,
                new FindInfoCardEventHandler(OnFindInfoCard));

            PopupControlService.EnsureCreated();
        }

        private InfoCardService()
        {
            _generatedSites = new WeakDictionary<Window, InfoCardSite>();
            _infoCardCache = new WeakDictionary<IInfoCardSubject, InfoCard>(
                new DelegatingEqualityComparer<IInfoCardSubject>(
                    (x, y) => (x == null) ? (y == null) : Equals(x.Data, y.Data),
                    s => (s.Data != null) ? s.Data.GetHashCode() : 0));
        }

        internal static InfoCardService Current
        {
            get
            {
                if (_current == null)
                    _current = new InfoCardService();

                return _current;
            }
        }

        public ReadOnlyCollection<InfoCard> InfoCards
        {
            get
            {
                _infoCardCache.RemoveCollectedEntries();
                return new ReadOnlyCollection<InfoCard>(_infoCardCache.Values.ToList());
            }
        }

        private static Visual FindContentElementParent(ContentElement ce)
        {
            var visual = (Visual)null;
            var parent = (DependencyObject)ce;

            while (parent != null)
            {
                visual = parent as Visual;
                if (visual != null)
                    return visual;

                ce = parent as ContentElement;
                if (ce == null)
                    return visual;

                parent = ContentOperations.GetParent(ce);

                if (parent != null)
                    continue;

                var element = ce as FrameworkContentElement;
                if (element != null)
                    parent = element.Parent;
            }

            return visual;
        }

        internal static bool HasInfoCard(DependencyObject obj)
        {
            return (GetInfoCardSubject(obj) != null);
        }

        private void InspectElementForInfoCard(DependencyObject obj)
        {
            var objectToInspect = obj;

            if (!IsOnContextMenu(obj) &&
                (LocateNearestInfoCard(ref obj)))
            {
                if (obj != null)
                {
                    if (LastMouseOverWithInfoCard != null)
                        RaiseInfoCardClosingEvent(true);

                    LastChecked = objectToInspect;
                    LastMouseOverWithInfoCard = obj;

                    var lastQuickShow = _quickShow;

                    ResetInfoCardTimer();

                    if (lastQuickShow)
                    {
                        _quickShow = false;
                        RaiseInfoCardOpeningEvent();
                    }
                    else
                    {
                        InfoCardTimer = new DispatcherTimer(DispatcherPriority.Normal)
                                             {
                                                 Interval = TimeSpan.FromMilliseconds(InitialShowDelay),
                                                 Tag = true
                                             };
                        InfoCardTimer.Tick += OnRaiseInfoCardOpeningEvent;
                        InfoCardTimer.Start();
                    }
                }
                else if (LastMouseOverWithInfoCard != null)
                {
                    // 4/3/2009 - Ensure the object to inspect is a descendant of the current target (0F5-10AE9FE9-990A)
                    while ((objectToInspect != null) &&
                           (objectToInspect != _currentInfoCard) &&
                           (objectToInspect != LastMouseOverWithInfoCard))
                    {
                        objectToInspect = objectToInspect.GetVisualParent();
                    }

                    // If the current target is not a parent of the object to inspect, close the screen tip
                    if (objectToInspect == null)
                    {
                        RaiseInfoCardClosingEvent(true);
                        LastMouseOverWithInfoCard = null;
                    }
                }
            }
            else if (!IsOnInfoCard(obj))
            {
                LastMouseOverWithInfoCard = null;
            }
        }

        private static bool IsOnContextMenu(DependencyObject obj)
        {
            if (obj != null)
                return (obj.FindVisualAncestorByType<ContextMenu>() != null);
            return false;
        }

        private static bool IsOnInfoCard(DependencyObject obj)
        {
            if (obj != null)
            {
                var infoCard = obj.FindVisualAncestorByType<InfoCardWindow>();
                if (infoCard == null)
                    return false;
                return (infoCard == InfoCardHost.GetInfoCardWindow(Current._currentInfoCard));
            }
            return false;
        }

        private DependencyObject LastChecked
        {
            get
            {
                if (_lastChecked != null)
                {
                    var target = (DependencyObject)_lastChecked.Target;
                    if (target != null)
                        return target;
                    _lastChecked = null;
                }
                return null;
            }
            set
            {
                if (value == null)
                    _lastChecked = null;
                else if (_lastChecked == null)
                    _lastChecked = new WeakReference(value);
                else
                    _lastChecked.Target = value;
            }
        }

        private IInputElement LastMouseDirectlyOver
        {
            get
            {
                if (_lastMouseDirectlyOver != null)
                {
                    var target = (IInputElement)_lastMouseDirectlyOver.Target;
                    if (target != null)
                        return target;
                    _lastMouseDirectlyOver = null;
                }
                return null;
            }
            set
            {
                if (value == null)
                    _lastMouseDirectlyOver = null;
                else if (_lastMouseDirectlyOver == null)
                    _lastMouseDirectlyOver = new WeakReference(value);
                else
                    _lastMouseDirectlyOver.Target = value;
            }
        }

        private DependencyObject LastMouseOverWithInfoCard
        {
            get
            {
                if (_lastMouseOverWithInfoCard != null)
                {
                    var target = (DependencyObject)_lastMouseOverWithInfoCard.Target;
                    if (target != null)
                        return target;
                    _lastMouseOverWithInfoCard = null;
                }
                return null;
            }
            set
            {
                if ((_lastMouseOverWithInfoCard != null) && (_lastMouseOverWithInfoCard.IsAlive) &&
                    (_lastMouseOverWithInfoCard.Target is IInputElement))
                    ((IInputElement)_lastMouseOverWithInfoCard.Target).MouseLeave -= OnMouseLeave;

                if (value == null)
                    _lastMouseOverWithInfoCard = null;
                else if (_lastMouseOverWithInfoCard == null)
                    _lastMouseOverWithInfoCard = new WeakReference(value);
                else
                    _lastMouseOverWithInfoCard.Target = value;

                if ((_lastMouseOverWithInfoCard != null) && (_lastMouseOverWithInfoCard.IsAlive) &&
                    (_lastMouseOverWithInfoCard.Target is IInputElement))
                    ((IInputElement)_lastMouseOverWithInfoCard.Target).MouseLeave += OnMouseLeave;
            }
        }

        private static bool LocateNearestInfoCard(ref DependencyObject obj)
        {
            var element = obj as IInputElement;
            if (element != null)
            {
                var e = new FindInfoCardEventArgs();
                element.RaiseEvent(e);
                if (e.TargetElement != null)
                {
                    obj = e.TargetElement;
                    return true;
                }
                if (e.KeepCurrentActive)
                {
                    obj = null;
                    return true;
                }
            }
            return false;
        }

        private static void OnFindInfoCard(object sender, FindInfoCardEventArgs e)
        {
            if (e.TargetElement != null)
                return;

            var obj = sender as DependencyObject;
            if (obj == null)
                return;

            if (Current.StopLookingForInfoCard(obj))
            {
                e.Handled = true;
                e.KeepCurrentActive = true;
            }
            else if (InfoCardIsEnabled(obj))
            {
                e.TargetElement = obj;
                e.Handled = true;
            }
        }

        private void OnBetweenShowDelay(object sender, EventArgs e)
        {
            ResetInfoCardTimer();
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            if ((_currentInfoCard != null) && (_currentInfoCard.IsPinned || _currentInfoCard.IsKeyboardFocusWithin))
                return;

            var directlyOver = Mouse.DirectlyOver as DependencyObject;
            if ((directlyOver != null) && (directlyOver.FindVisualAncestorByType<InfoCard>() == _currentInfoCard))
                return;

            // Close any active screen tip
            RaiseInfoCardClosingEvent(true);
        }

        private void OnMouseMove(IInputElement directlyOver)
        {
            var directlyOverObj = directlyOver as DependencyObject;
            if ((directlyOverObj != null) && (directlyOverObj.FindVisualAncestorByType<InfoCard>() != null))
                return;
            if (directlyOver != LastMouseDirectlyOver)
            {
                LastMouseDirectlyOver = directlyOver;
                if (directlyOver != LastMouseOverWithInfoCard)
                    InspectElementForInfoCard(directlyOver as DependencyObject);
            }
        }

        private void OnRaiseInfoCardOpeningEvent(object sender, EventArgs e)
        {
            RaiseInfoCardOpeningEvent();
        }

        internal void ProcessPreviewKeyDownForInfoCard(KeyEventArgs e)
        {
            
        }

        internal void ProcessPreviewMouseMoveForInfoCard(MouseEventArgs e)
        {
            // Get the element that the mouse is directly over
            var directlyOver = Mouse.PrimaryDevice.DirectlyOver;
            var position = e.GetPosition(directlyOver);

            // If the mouse is over a disabled element, it won't be returned by DirectlyOver (only the internal RawDirectlyOver gives that)
            //   so search on our own
            var directlyOverVisual = directlyOver as Visual;
            if (directlyOverVisual != null)
            {
                var result = VisualTreeHelper.HitTest(directlyOverVisual, position);
                if (result != null)
                {
                    directlyOverVisual = result.VisualHit as Visual;
                    if ((directlyOverVisual != null) && (directlyOverVisual != directlyOver))
                    {
                        directlyOver = directlyOverVisual as IInputElement ?? directlyOver;
                        position = e.GetPosition(directlyOver);
                    }
                }
            }

            var element = directlyOver as UIElement;
            if ((element == null) ||
                (((position.X >= 0) && (position.X < element.RenderSize.Width)) &&
                 ((position.Y >= 0) && (position.Y < element.RenderSize.Height))))
            {
                OnMouseMove(directlyOver);
            }
        }

        private void RaiseInfoCardClosingEvent(bool reset)
        {
            ResetInfoCardTimer();
            if (reset)
            {
                LastChecked = null;
            }
            var lastMouseOver = LastMouseOverWithInfoCard;
            if ((lastMouseOver != null) && (_currentInfoCard != null))
            {
                var isOpen = _currentInfoCard.IsOpen;

                if (_currentInfoCard.IsPinned)
                    return;

                try
                {
                    if (isOpen)
                    {
                        var element = lastMouseOver as IInputElement;
                        if (element != null)
                            element.RaiseEvent(new RoutedEventArgs(InfoCardClosingEvent, this));
                    }
                }
                finally
                {
                    if (isOpen)
                    {
                        var infoCardSite = _currentInfoCard.RegisteredInfoCardSite;
                        if (infoCardSite != null)
                            infoCardSite.InfoCards.Remove(_currentInfoCard);

                        _currentInfoCard.Close();
                        _quickShow = true;
                        InfoCardTimer = new DispatcherTimer(DispatcherPriority.Normal)
                                             {
                                                 Interval = TimeSpan.FromMilliseconds(BetweenShowDelay)
                                             };
                        InfoCardTimer.Tick += OnBetweenShowDelay;
                        InfoCardTimer.Start();
                    }

                    _currentInfoCard = null;
                }
            }
        }

        private void RaiseInfoCardOpeningEvent()
        {
            ResetInfoCardTimer();

            var lastMouseOver = LastMouseOverWithInfoCard;
            if (lastMouseOver != null)
            {
                var showInfoCard = true;
                var inputElement = lastMouseOver as IInputElement;
                if (inputElement != null)
                {
                    // Raise the screen tip opening event
                    var e = new RoutedEventArgs(InfoCardOpeningEvent, this);
                    inputElement.RaiseEvent(e);
                    showInfoCard = !e.Handled;
                }
                if (showInfoCard)
                {
                    if ((_currentInfoCard != null) && !_currentInfoCard.IsOpen)
                        RetireInfoCard(_currentInfoCard);

                    _currentInfoCard = CreateInfoCard(lastMouseOver);

                    if (_currentInfoCard != null)
                    {
                        var targetElement = lastMouseOver as UIElement;

                        _currentInfoCard.TargetElement = targetElement;

                        var infoCardPosition = Mouse.GetPosition(inputElement);
                        var infoCardSite = _currentInfoCard.RegisteredInfoCardSite ??
                                           lastMouseOver.FindVisualAncestorByType<InfoCardSite>();

                        if (infoCardSite == null)
                        {
                            var window = Window.GetWindow(lastMouseOver);
                            if (window != null)
                            {
                                if (!_generatedSites.TryGetValue(window, out infoCardSite))
                                {
                                    infoCardSite = new InfoCardSite();
                                    _generatedSites[window] = infoCardSite;
                                }
                            }
                            else
                            {
                                RetireInfoCard(_currentInfoCard);
                                _currentInfoCard = null;
                                return;
                            }
                        }

                        if (!_currentInfoCard.IsOpen)
                        {
                            if (!infoCardSite.InfoCards.Contains(_currentInfoCard))
                            {
                                SetUnregisterInfoCardOnClose(_currentInfoCard, true);
                                infoCardSite.InfoCards.Add(_currentInfoCard);
                            }
                        }

                        if (infoCardSite.IsLoaded)
                        {
                            var targetVisual = targetElement;
                            if (targetVisual != null)
                            {
                                var transformToVisual = targetVisual.TransformToVisual(infoCardSite);
                                if (transformToVisual != null)
                                    infoCardPosition = transformToVisual.Transform(infoCardPosition);
                            }
                        }

                        if (targetElement != null)
                        {
                            var customPlacementCallback = _currentInfoCard.CustomPlacementCallback ??
                                                          GetCustomInfoCardPlacementCallback(targetElement);
                            if (customPlacementCallback != null)
                            {
                                _currentInfoCard.UpdateLayout();
                                infoCardPosition = customPlacementCallback(
                                    _currentInfoCard.RenderSize,
                                    targetElement,
                                    infoCardPosition);
                            }
                        }

                        _currentInfoCard.Location = new Point(
                            DoubleUtil.DoubleToInt(infoCardPosition.X),
                            DoubleUtil.DoubleToInt(infoCardPosition.Y));

                        if (_currentInfoCard.IsOpen)
                        {
                            var infoCardWindow = InfoCardHost.GetInfoCardWindow(_currentInfoCard);
                            if (infoCardWindow != null)
                            {
                                infoCardWindow.Setup(_currentInfoCard.Location);
                                infoCardWindow.Activate();
                            }
                            return;
                        }
                        
                        _currentInfoCard.Open();
                    }
                }
            }
        }

        private void ResetInfoCardTimer()
        {
            if (_infoCardTimer == null)
                return;
            _infoCardTimer.Stop();
            _infoCardTimer = null;
            _quickShow = false;
        }

        private static bool InfoCardIsEnabled(DependencyObject obj)
        {
            return ((obj != null) && HasInfoCard(obj));
        }

        private DispatcherTimer InfoCardTimer
        {
            get { return _infoCardTimer; }
            set
            {
                ResetInfoCardTimer();
                _infoCardTimer = value;
            }
        }

        internal bool StopLookingForInfoCard(DependencyObject obj)
        {
            if ((LastChecked == obj) ||
                (LastMouseOverWithInfoCard == obj) ||
                (_currentInfoCard == obj) ||
                WithinCurrentInfoCard(obj))
            {
                return true;
            }
            return false;
        }

        // ReSharper disable UnusedMember.Local
        private bool IsInfoCardOpen(DependencyObject obj)
        {
            if (obj == null)
                return false;

            var infoCardSubject = GetInfoCardSubject(obj);
            if (infoCardSubject == null)
                return false;

            InfoCard infoCard;
            return _infoCardCache.TryGetValue(infoCardSubject, out infoCard) && infoCard.IsOpen;
        }
        // ReSharper restore UnusedMember.Local

        private bool WithinCurrentInfoCard(DependencyObject obj)
        {
            if (_currentInfoCard != null)
            {
                var visual = obj as Visual;
                if (visual == null)
                {
                    var element = obj as ContentElement;
                    if (element != null)
                        visual = FindContentElementParent(element);
                }

                if (visual != null)
                    return visual.IsDescendantOf(_currentInfoCard);
            }
            return false;
        }

        // PUBLIC PROCEDURES

        public static InfoCard CurrentInfoCard
        {
            get
            {
                return Current._currentInfoCard;
            }
        }

        private InfoCard CreateInfoCard(DependencyObject obj)
        {
            var infoCardSubject = GetInfoCardSubject(obj);
            if (infoCardSubject == null)
                return null;

            InfoCard infoCard;
            if (_infoCardCache.TryGetValue(infoCardSubject, out infoCard))
                return infoCard;

            infoCard = infoCardSubject.CreateInfoCard();
            infoCard.Closed += OnInfoCardClosed;
            infoCard.SubjectChanged += OnInfoCardSubjectChanged;
            _infoCardCache[infoCardSubject] = infoCard;

            return infoCard;
        }

        private void OnInfoCardSubjectChanged(object sender, PropertyChangedRoutedEventArgs<IInfoCardSubject> e)
        {
            var oldSubject = e.OldValue;
            if (oldSubject == null)
                return;

            InfoCard infoCard;
            if (!_infoCardCache.TryGetValue(oldSubject, out infoCard))
                return;

            _infoCardCache.Remove(oldSubject);

            var newSubject = e.NewValue;
            if (newSubject != null)
                _infoCardCache[newSubject] = infoCard;
            else if (infoCard.IsOpen)
                infoCard.Close();
            else
                RetireInfoCard(infoCard);
        }

        private void OnInfoCardClosed(object sender, RoutedEventArgs e)
        {
            var infoCard = sender as InfoCard;
            if (infoCard == null)
                return;

            RetireInfoCard(infoCard);
        }

        private void RetireInfoCard([NotNull] InfoCard infoCard)
        {
            if (infoCard == null)
                throw new ArgumentNullException("infoCard");

            infoCard.Closed -= OnInfoCardClosed;
            infoCard.SubjectChanged -= OnInfoCardSubjectChanged;

            var infoCardSubject = infoCard.Subject;
            if (infoCardSubject != null)
                _infoCardCache.Remove(infoCard.Subject);

            _infoCardCache.RemoveCollectedEntries();

            infoCard.ClearValue(InfoCard.SubjectProperty);
        }

        public static InfoCard GetInfoCard(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            return obj.GetValue(InfoCardProperty) as InfoCard;
        }

        public static void SetInfoCard(DependencyObject obj, InfoCard value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            obj.SetValue(InfoCardProperty, value);
        }
    }
}