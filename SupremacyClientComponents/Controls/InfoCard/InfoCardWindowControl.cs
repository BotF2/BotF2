using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Supremacy.Client.Controls
{
    public class InfoCardWindowControl : InlineWindow, IInfoCardWindow
    {
        #region Fields
        private InfoCardCloseReason _closeReason = InfoCardCloseReason.InfoCardWindowClosed;
        private Window _popupSiteWindow;
        #endregion

        #region Constructors and Finalizers
        static InfoCardWindowControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(InfoCardWindowControl),
                new FrameworkPropertyMetadata(typeof(InfoCardWindowControl)));

            VisibilityProperty.OverrideMetadata(
                typeof(InfoCardWindowControl),
                new FrameworkPropertyMetadata(Visibility.Collapsed));

            DropShadowColorProperty.OverrideMetadata(
                typeof(InfoCardWindowControl),
                new FrameworkPropertyMetadata(Color.FromArgb(0x38, 0, 0, 0)));

            HasDropShadowProperty.OverrideMetadata(
                typeof(InfoCardWindowControl),
                new FrameworkPropertyMetadata(true));

            WindowStyleProperty.OverrideMetadata(
                typeof(InfoCardWindowControl),
                new FrameworkPropertyMetadata(WindowStyle.None));

            ResizeModeProperty.OverrideMetadata(
                typeof(InfoCardWindowControl),
                new FrameworkPropertyMetadata(ResizeMode.NoResize));
        }

        public InfoCardWindowControl(InfoCardHost container)
        {
            InfoCardHost = container ?? throw new ArgumentNullException("container");

            InfoCardHost.SetInfoCardWindow(this, this);

            Setup(container.Location);
        }
        #endregion

        #region Implementation of IInfoCardWindow
        private event EventHandler PopupWindowActivatedEvent;
        private event EventHandler PopupWindowClosedEvent;
        private event EventHandler PopupWindowLocationChangedEvent;

        /// <summary>
        /// Occurs when the window is activated.
        /// </summary>
        event EventHandler IInfoCardWindow.Activated
        {
            add { PopupWindowActivatedEvent += value; }
            remove { PopupWindowActivatedEvent -= value; }
        }

        void IInfoCardWindow.Close(InfoCardCloseReason reason)
        {
            _closeReason = reason;
            Close();
        }

        event EventHandler IInfoCardWindow.Closed
        {
            add { PopupWindowClosedEvent += value; }
            remove { PopupWindowClosedEvent -= value; }
        }

        public InfoCardSite InfoCardSite => InfoCardHost.InfoCardSite;

        public bool IsClosing { get; private set; }

        public Point Location => new Point(Left, Top);

        event EventHandler IInfoCardWindow.LocationChanged
        {
            add { PopupWindowLocationChangedEvent += value; }
            remove { PopupWindowLocationChangedEvent -= value; }
        }

        public InfoCardHost InfoCardHost
        {
            get => Content as InfoCardHost;
            private set => Content = value;
        }

        public void Setup(Point? position)
        {
            if (!position.HasValue)
            {
                return;
            }

            Left = position.Value.X;
            Top = position.Value.Y;
        }

        public void SnapToScreen()
        {
            InfoCardSite popupSite = InfoCardSite;
            if (popupSite == null)
            {
                return;
            }

            Rect bounds = new Rect(Left, Top, Width, Height);
            Rect workingArea = new Rect(new Point(), popupSite.RenderSize);

            if (bounds.Right > workingArea.Right)
            {
                bounds.Offset(workingArea.Right - bounds.Right, 0);
            }

            if (bounds.Left < workingArea.Left)
            {
                bounds.Offset(workingArea.Left - bounds.Left, 0);
            }

            if (bounds.Bottom > workingArea.Bottom)
            {
                bounds.Offset(0, workingArea.Bottom - bounds.Bottom);
            }

            if (bounds.Top < workingArea.Top)
            {
                bounds.Offset(0, workingArea.Top - bounds.Top);
            }

            if (Left != bounds.Left)
            {
                Left = bounds.Left;
            }

            if (Top != bounds.Top)
            {
                Top = bounds.Top;
            }
        }

        bool IInfoCardWindow.Activate()
        {
            return (bool)Dispatcher.Invoke(
                (Func<bool>)base.Activate,
                DispatcherPriority.Input);
        }

        void IInfoCardWindow.DragMove()
        {
            DragMove();
        }
        #endregion

        protected override void OnActivated(RoutedEventArgs e)
        {
            base.OnActivated(e);

            PopupWindowActivatedEvent?.Invoke(this, EventArgs.Empty);

            Color dropShadowColor = DropShadowColor;
            dropShadowColor.A *= 2;
            DropShadowColor = dropShadowColor;

            // Bring this window to the front
            InfoCardSite popupSite = InfoCardSite;
            if (popupSite != null)
            {
                popupSite.BringToFront(this);
            }
        }

        protected override void OnOpened(RoutedEventArgs e)
        {
            base.OnOpened(e);

            InfoCardSite popupSite = InfoCardSite;
            if (popupSite != null)
            {
                popupSite.AddCanvasChild(this);
                _popupSiteWindow = Window.GetWindow(popupSite);
                if (_popupSiteWindow != null)
                {
                    _popupSiteWindow.PreviewMouseDown += OnPopupSitePreviewMouseDown;
                }
            }

            Visibility = Visibility.Visible;
        }

        private void OnPopupSitePreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            InfoCardSite popupSite = InfoCardSite;
            if (popupSite == null)
            {
                return;
            }

            InfoCard popup = popupSite.GetInfoCardFromHost(InfoCardHost);
            if ((popup == null) || popup.IsPinned)
            {
                return;
            }

            if ((popupSite.InputHitTest(e.GetPosition(popupSite)) is DependencyObject hitTestResult) && this.IsVisualAncestorOf(hitTestResult))
            {
                return;
            }

            _ = popup.Close();
        }

        protected override void OnClosed(RoutedEventArgs e)
        {
            if (_popupSiteWindow != null)
            {
                _popupSiteWindow.PreviewMouseDown -= OnPopupSitePreviewMouseDown;
                _popupSiteWindow = null;
            }

            Visibility = Visibility.Collapsed;

            InfoCardSite popupSite = InfoCardSite;
            if (popupSite != null)
            {
                popupSite.RemoveCanvasChild(this);
            }

            InfoCardHost = null;

            base.OnClosed(e);

            PopupWindowClosedEvent?.Invoke(this, EventArgs.Empty);

            IsClosing = false;
        }

        protected override void OnClosing(CancelRoutedEventArgs e)
        {
            base.OnClosing(e);

            if (e.Cancel)
            {
                _closeReason = InfoCardCloseReason.InfoCardWindowClosed;
                return;
            }

            IsClosing = true;

            bool cancel = false;

            InfoCardSite popupSite = InfoCardSite;
            if (popupSite != null)
            {
                InfoCard popup = popupSite.GetInfoCardFromHost(InfoCardHost);
                if (popup != null)
                {
                    cancel |= !popupSite.Close(popup, _closeReason, false);
                }
            }

            if (cancel)
            {
                e.Cancel = true;
                IsClosing = false;
                return;
            }

            // Reset the close reason
            _closeReason = InfoCardCloseReason.InfoCardWindowClosed;
        }

        protected override void OnDeactivated(RoutedEventArgs e)
        {
            base.OnDeactivated(e);

            ClearValue(DropShadowColorProperty);

            if (!IsVisible)
            {
                return;
            }

            InfoCard popup = InfoCardSite.GetInfoCardFromHost(InfoCardHost);
            if ((popup != null) && !popup.IsPinned)
            {
                _ = popup.Close();
            }
        }

        /// <summary>
        /// Raises the <see cref="InlineWindow.LocationChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="RoutedEventArgs"/> that contains the event data.</param>
        protected override void OnLocationChanged(RoutedEventArgs e)
        {
            base.OnLocationChanged(e);

            PopupWindowLocationChangedEvent?.Invoke(this, EventArgs.Empty);

            Canvas.SetLeft(this, Left);
            Canvas.SetTop(this, Top);
        }
    }
}