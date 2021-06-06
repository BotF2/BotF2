using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

using Supremacy.Client.Interop;

namespace Supremacy.Client.Controls
{
    public class InfoCardWindow : Window, IInfoCardWindow
    {
        #region Fields
        private Matrix _transformToDevice;
        private InfoCardCloseReason _closeReason;
        #endregion

        #region Constructors and Finalizers
		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static InfoCardWindow()
        {
            ShowInTaskbarProperty.OverrideMetadata(
                typeof(InfoCardWindow),
                new FrameworkPropertyMetadata(false));

            SizeToContentProperty.OverrideMetadata(
                typeof(InfoCardWindow),
                new FrameworkPropertyMetadata(SizeToContent.WidthAndHeight));

            WindowStyleProperty.OverrideMetadata(
                typeof(InfoCardWindow),
                new FrameworkPropertyMetadata(WindowStyle.None));

            ResizeModeProperty.OverrideMetadata(
                typeof(InfoCardWindow),
                new FrameworkPropertyMetadata(ResizeMode.NoResize));

            AllowsTransparencyProperty.OverrideMetadata(
                typeof(InfoCardWindow),
                new FrameworkPropertyMetadata(true));

            BackgroundProperty.OverrideMetadata(
                typeof(InfoCardWindow),
                new FrameworkPropertyMetadata(Brushes.Transparent));

		    EventManager.RegisterClassHandler(
		        typeof(InfoCard),
		        InfoCard.PinnedEvent,
		        (RoutedEventHandler)OnInfoCardPinned);

            EventManager.RegisterClassHandler(
                typeof(InfoCard),
                InfoCard.UnpinnedEvent,
                (RoutedEventHandler)OnInfoCardUnpinned);
        }

        private static void OnInfoCardPinned(object sender, RoutedEventArgs e)
        {
            InfoCard infoCard = (InfoCard)sender;
            if (infoCard == null)
                return;

            InfoCardWindow window = InfoCardHost.GetInfoCardWindow(infoCard) as InfoCardWindow;
            if (window == null)
                return;
            window.ShowInTaskbar = true;
            window.Topmost = true;
        }

        private static void OnInfoCardUnpinned(object sender, RoutedEventArgs e)
        {
            InfoCard infoCard = (InfoCard)sender;
            if (infoCard == null)
                return;

            InfoCardWindow window = InfoCardHost.GetInfoCardWindow(infoCard) as InfoCardWindow;
            if (window == null)
                return;

            window.ShowInTaskbar = false;
            window.Topmost = false;
        }

        public InfoCardWindow(InfoCardHost container)
        {
			if (container == null)
				throw new ArgumentNullException("container");

			InfoCardHost.SetInfoCardWindow(this, this);

		    WindowStyle = WindowStyle.None;
		    ResizeMode = ResizeMode.NoResize;
		    AllowsTransparency = true;
            Background = Brushes.Transparent;
            Width = 0;
            Height = 0;
            SizeToContent = SizeToContent.WidthAndHeight;

			InfoCardHost = container;

            Window ownerWindow = GetWindow(InfoCardSite);
            if (ownerWindow != null)
                Owner = ownerWindow;

            InfoCard infoCard = (InfoCard)container.Content;
            Point location = container.Location ?? new Point(0, 0);

            _transformToDevice = Matrix.Identity;

            Window targetWindow = GetWindow(infoCard.TargetElement);
            if (targetWindow != null)
            {
                _transformToDevice.OffsetX = targetWindow.Left;
                _transformToDevice.OffsetY = targetWindow.Top;
                location = _transformToDevice.Transform(location);
            }

            Setup(location);

            Loaded += OnLoaded;
		}
        #endregion

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            InvalidateMeasure();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            if (SizeToContent == SizeToContent.WidthAndHeight)
                SizeToContent = SizeToContent.Height;
        }

        #region IsClosing Property
        private static readonly DependencyPropertyKey IsClosingPropertyKey = DependencyProperty.RegisterReadOnly(
            "IsClosing",
            typeof(bool),
            typeof(InfoCardWindow),
            new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsClosingProperty = IsClosingPropertyKey.DependencyProperty;

        public bool IsClosing
        {
            get { return (bool)GetValue(IsClosingProperty); }
            private set { SetValue(IsClosingPropertyKey, value); }
        }
        #endregion

        #region Implementation of IInfoCardWindow
        public void Close(InfoCardCloseReason reason)
        {
            _closeReason = reason;

            Close();
        }

        public InfoCardSite InfoCardSite => InfoCardHost.InfoCardSite;


        public Point Location
        {
            get
            {
                Point location = new Point(Left, Top);
                InfoCard infoCard = InfoCardHost.Content as InfoCard;
                if (infoCard != null)
                {
                    Window targetWindow = GetWindow(infoCard.TargetElement);
                    if (targetWindow != null)
                    {
                        _transformToDevice = Matrix.Identity;
                        _transformToDevice.OffsetX = targetWindow.Left;
                        _transformToDevice.OffsetY = targetWindow.Top;
                    }
                }
                location = _transformToDevice.Transform(location);
                return location;
            }
        }

        public InfoCardHost InfoCardHost
        {
            get { return Content as InfoCardHost; }
            private set { Content = value; }
        }

        public void Setup(Point? position)
        {
            if (!position.HasValue)
                return;
            Left = position.Value.X;
            Top = position.Value.Y;
        }

        public void SnapToScreen()
        {
            Rect bounds = NativeMethods.EnsureBoundsOnScreen(
                new Rect(
                    Left,
                    Top,
                    Width,
                    Height));

            if (Left != bounds.Left)
                Left = bounds.Left;
            if (Top != bounds.Top)
                Top = bounds.Top;
        }

        void IInfoCardWindow.DragMove()
        {
            DragMove();
        }
        #endregion

        #region Window Close Method Overrides
        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);

            InfoCard popup = InfoCardSite.GetInfoCardFromHost(InfoCardHost);
            if ((popup != null) && !popup.IsPinned)
                popup.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            InfoCardSite popupSite = InfoCardSite;
            if (popupSite != null)
            {
                Window window = GetWindow(popupSite);
                if ((window != null) && !window.IsActive)
                    window.Activate();
            }

            InfoCardHost = null;

            base.OnClosed(e);

            BindingOperations.ClearAllBindings(this);

            IsClosing = false;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (e.Cancel)
            {
                _closeReason = InfoCardCloseReason.InfoCardWindowClosed;
                return;
            }

            IsClosing = true;

            bool cancel = false;

            InfoCardSite infoCardSite = InfoCardSite;
            if (infoCardSite != null)
            {
                InfoCard popup = infoCardSite.GetInfoCardFromHost(InfoCardHost);
                if (popup != null)
                    cancel |= !infoCardSite.Close(popup, _closeReason, false);
            }

            _closeReason = InfoCardCloseReason.InfoCardWindowClosed;

            if (!cancel)
            {
                Owner = null;
                return;
            }

            e.Cancel = true;
            IsClosing = false;
            return;
        }
        #endregion
    }
}