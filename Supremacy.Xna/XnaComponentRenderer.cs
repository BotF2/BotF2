using System;
using System.Windows;
using System.Windows.Controls;

namespace Supremacy.Xna
{
    public class XnaComponentRenderer : Control
    {
        private XnaComponent _component;
        private IDisposable _runHandle;
        private bool _subscribedToPresentEvent;

        public XnaComponentRenderer(XnaComponent component = null)
        {
            _component = component;

            IsVisibleChanged += OnIsVisibleChanged;
            SizeChanged += OnSizeChanged;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        protected XnaComponent Component
        {
            get => _component;
            set
            {
                if (value == _component)
                {
                    return;
                }

                EndRun();

                _component = value;

                if (IsLoaded && IsVisible)
                {
                    BeginRun();
                }
            }
        }

        private void OnFramePresented(object sender, EventArgs eventArgs)
        {
            InvalidateVisual();
        }

        protected virtual bool ManageComponentTargetSize => true;

        private void BeginRun()
        {
            IDisposable oldRunHandle = _runHandle;

            if (_component != null)
            {
                if (!_subscribedToPresentEvent)
                {
                    _component.FramePresented += OnFramePresented;
                    _subscribedToPresentEvent = true;
                }

                _runHandle = _component.Run();
            }
            else
            {
                _runHandle = null;
            }

            oldRunHandle?.Dispose();
        }

        private void EndRun()
        {
            if (_runHandle == null)
            {
                return;
            }

            if (_subscribedToPresentEvent && _component != null)
            {
                _component.FramePresented -= OnFramePresented;
                _subscribedToPresentEvent = false;
            }

            _runHandle.Dispose();
            _runHandle = null;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            BeginRun();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            EndRun();
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                BeginRun();
            }
            else
            {
                EndRun();
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ManageComponentTargetSize)
            {
                UpdateTargetSize();
            }
        }

        protected void UpdateTargetSize()
        {
            double multiplier = 1.0;

            Window window = Window.GetWindow(this);
            if (window != null)
            {
                multiplier = TransformToAncestor(window).TransformBounds(new Rect(1, 1, 1, 1)).Width;
            }

            _component.TargetSize = new Int32Rect(
                0,
                0,
                (int)Math.Max(1, ActualWidth * multiplier),
                (int)Math.Max(1, ActualHeight * multiplier));
        }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            XnaComponent component = _component;
            if (component == null)
            {
                return;
            }

            System.Windows.Interop.D3DImage renderTargetImage = component.RenderTargetImage;
            if (renderTargetImage != null && renderTargetImage.IsFrontBufferAvailable)
            {
                drawingContext.DrawImage(renderTargetImage, new Rect(RenderSize));
            }
        }
    }
}