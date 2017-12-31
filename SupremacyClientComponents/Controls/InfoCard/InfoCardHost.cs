using System;
using System.Windows;
using System.Windows.Controls;

namespace Supremacy.Client.Controls
{
    public class InfoCardHost : ContentControl
    {
        #region Constructors and Finalizers
        public InfoCardHost()
        {
            UniqueId = Guid.NewGuid();
            SetInfoCardHost(this, this);
            InfoCard.SetIsCurrentlyOpen(this, true);
        }
        #endregion

        #region LayoutChanged Event
        public static readonly RoutedEvent LayoutChangedEvent = EventManager.RegisterRoutedEvent(
            "LayoutChanged",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(InfoCardHost));

        public event RoutedEventHandler LayoutChanged
        {
            add { AddHandler(LayoutChangedEvent, value); }
            remove { RemoveHandler(LayoutChangedEvent, value); }
        }
        #endregion

        #region InfoCardSite Property
        public InfoCardSite InfoCardSite
        {
            get { return InfoCardSite.GetInfoCardSite((this)); }
            internal set { InfoCardSite.SetInfoCardSite(this, value); }
        }
        #endregion

        #region InfoCardHost Property
        protected static readonly DependencyPropertyKey InfoCardHostPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "InfoCardHost",
            typeof(InfoCardHost),
            typeof(InfoCardHost),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.Inherits));

        public static readonly DependencyProperty InfoCardHostProperty = InfoCardHostPropertyKey.DependencyProperty;

        public static InfoCardHost GetInfoCardHost(DependencyObject d)
        {
            if (d == null)
                throw new ArgumentNullException("d");
            return (InfoCardHost)d.GetValue(InfoCardHostProperty);
        }

        protected static void SetInfoCardHost(DependencyObject d, InfoCardHost value)
        {
            if (d == null)
                throw new ArgumentNullException("d");
            d.SetValue(InfoCardHostPropertyKey, value);
        }
        #endregion

        #region InfoCardWindow Property
        private static readonly DependencyPropertyKey InfoCardWindowPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "InfoCardWindow",
            typeof(IInfoCardWindow),
            typeof(InfoCardHost),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.Inherits,
                OnInfoCardWindowPropertyChanged));

        internal static readonly DependencyProperty InfoCardWindowProperty = InfoCardWindowPropertyKey.DependencyProperty;

        private static void OnInfoCardWindowPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var container = d as InfoCardHost;
            if (container != null)
                container.OnInfoCardWindowChanged(e.OldValue as IInfoCardWindow, e.NewValue as IInfoCardWindow);
        }

        internal IInfoCardWindow InfoCardWindow
        {
            get { return GetInfoCardWindow(this); }
        }

        internal static IInfoCardWindow GetInfoCardWindow(DependencyObject d)
        {
            if (d == null)
                throw new ArgumentNullException("d");
            return (IInfoCardWindow)d.GetValue(InfoCardWindowProperty);
        }

        internal static void SetInfoCardWindow(DependencyObject d, IInfoCardWindow value)
        {
            if (d == null)
                throw new ArgumentNullException("d");
            d.SetValue(InfoCardWindowPropertyKey, value);
        }
        #endregion

        #region UniqueId Property
        public Guid UniqueId
        {
            get { return (Guid)GetValue(InfoCardSite.UniqueIdProperty); }
            internal set { SetValue(InfoCardSite.UniqueIdPropertyKey, value); }
        }
        #endregion

        public event EventHandler Activated;

        private void OnInfoCardActivated(object sender, EventArgs e)
        {
            Location = InfoCardWindow.Location;
            UpdateToolWindowInfoCardLocations();

            var handler = Activated;
            if (handler != null)
                handler(this, e);
        }

        private void OnInfoCardWindowChanged(IInfoCardWindow oldInfoCardWindow, IInfoCardWindow newInfoCardWindow)
        {
            if (oldInfoCardWindow != null)
            {
                oldInfoCardWindow.Activated -= OnInfoCardActivated;
                oldInfoCardWindow.LocationChanged -= OnInfoCardWindowLocationChanged;
            }

            if (newInfoCardWindow == null)
                return;

            newInfoCardWindow.Activated += OnInfoCardActivated;
            newInfoCardWindow.LocationChanged += OnInfoCardWindowLocationChanged;
        }

        private void OnInfoCardWindowLocationChanged(object sender, EventArgs e)
        {
            Location = InfoCardWindow.Location;
            UpdateToolWindowInfoCardLocations();
        }

        private void UpdateToolWindowInfoCardLocations()
        {
            var infoCard = InfoCardSite.GetInfoCardFromHost(this);
            if (infoCard != null)
                infoCard.Location = Location;
        }
        
        public Point? Location { get; internal set; }
    }
}