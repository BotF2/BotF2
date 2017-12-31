using System;
using System.Concurrency;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Supremacy.Resources;

using Color = Microsoft.Xna.Framework.Graphics.Color;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Point = System.Windows.Point;

namespace Supremacy.Xna
{
    public sealed partial class ShipModelViewer : IServiceProvider, IGraphicsDeviceService
    {
        private const string ModelDirectory = @"Resources\Models";
        private const string XnaContentExtension = ".xnb";

        // ReSharper disable InconsistentNaming
        private readonly object _modelLock = new object();
        private readonly string _workingDirectory;

        private volatile Model _model;

        private D3DImage _d3dImage;
        private bool _compositionTargetSetBackBufferRequired;
        private Matrix _worldMatrix;
        private Matrix _viewMatrix;
        private Matrix _projectionMatrix;
        private GraphicsDevice _device;
        private RenderTarget2D _backBuffer;
        private RenderTarget2D _frontBuffer;
        private DepthStencilBuffer _depthStencilBuffer;
        private DepthStencilBuffer _deviceDepthStencilBuffer;
        private IntPtr _backBufferPointer;
        private IntPtr _frontBufferPointer;
        private ContentManager _contentManager;
        private MethodInfo _copySurfaceMethod;
        private bool _needResize;
        private Int32Rect _targetSize;
        private int _lastTime;
        private bool _isDragging;
        private Point _lastMousePosition;
        private IDisposable _modelLoadRequest;
        private DispatcherTimer _timer;

        // TODO: Localize these status messages.
        private string _loadFailureMessage = "Error Loading Model";
        private string _modelUnavailableMessage = "No Model Available";
        private string _loadingMessage = "Loading...";
        // ReSharper restore InconsistentNaming

        public ShipModelViewer()
        {
            InitializeComponent();

            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
            this.SizeChanged += OnSizeChanged;

            _workingDirectory = PathHelper.GetWorkingDirectory();

            _targetImage.MouseLeftButtonDown += OnTargetImageMouseLeftButtonDown;
            _targetImage.MouseLeftButtonUp += OnTargetImageMouseLeftButtonUp;
            _targetImage.MouseMove += OnTargetImageMouseMove;
            _targetImage.LostMouseCapture += OnTargetImageLostMouseCapture;
        }

        private void OnLoaded(object sender, EventArgs e)
        {
            UpdateTargetSize();

            InitializeXna();

            _d3dImage = new D3DImage();
            _d3dImage.Lock();

            try
            {
                _d3dImage.SetBackBuffer(
                    D3DResourceType.IDirect3DSurface9,
                    _frontBufferPointer);
            }
            finally
            {
                _d3dImage.Unlock();
            }

            _targetImage.Source = _d3dImage;

            if (_device.GraphicsDeviceCapabilities.DeviceType == DeviceType.Hardware)
            {
                CompositionTarget.Rendering += OnCompositionTargetRendering;
            }
            else
            {
                _timer = XnaHelper.CreateRenderTimer();
                _timer.Tick += OnCompositionTargetRendering;
                _timer.Start();
            }
        }

        private void UpdateTargetSize()
        {
            var multiplier = 1.0;

            var window = Window.GetWindow(this);
            if (window != null)
                multiplier = this.TransformToAncestor(window).TransformBounds(new Rect(1, 1, 1, 1)).Width;

            _targetSize = new Int32Rect(
                0,
                0,
                (int)(Math.Max(1, this.ActualWidth) * multiplier),
                (int)(Math.Max(1, this.ActualHeight) * multiplier));
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
            else
            {
                CompositionTarget.Rendering -= OnCompositionTargetRendering;
            }

            Dispose();
        }

        #region ModelFile Property
        public static readonly DependencyProperty ModelFileProperty = DependencyProperty.Register(
            "ModelFile",
            typeof(string),
            typeof(ShipModelViewer),
            new FrameworkPropertyMetadata((d, e) => ((ShipModelViewer)d).LoadModel()));

        public string ModelFile
        {
            get { return (string)GetValue(ModelFileProperty); }
            set { SetValue(ModelFileProperty, value); }
        }
        #endregion

        #region StatusMessage Property
        private static readonly DependencyPropertyKey StatusMessagePropertyKey = DependencyProperty.RegisterReadOnly(
            "StatusMessage",
            typeof(string),
            typeof(ShipModelViewer),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty StatusMessageProperty = StatusMessagePropertyKey.DependencyProperty;

        public string StatusMessage
        {
            get { return (string)GetValue(StatusMessageProperty); }
            private set { SetValue(StatusMessagePropertyKey, value); }
        }
        #endregion

        #region CameraDistanceMultiplier Property
        public static readonly DependencyProperty CameraDistanceMultiplierProperty = DependencyProperty.Register(
            "CameraDistanceMultiplier",
            typeof(float),
            typeof(ShipModelViewer),
            new FrameworkPropertyMetadata(
                3f,
                FrameworkPropertyMetadataOptions.None));

        public float CameraDistanceMultiplier
        {
            get { return (float)GetValue(CameraDistanceMultiplierProperty); }
            set { SetValue(CameraDistanceMultiplierProperty, value); }
        }
        #endregion

        private void OnCompositionTargetRendering(object sender, EventArgs eventArgs)
        {
            if (_d3dImage == null)
                return;

            this.Present();

            if (_d3dImage.IsFrontBufferAvailable &&
                _device.GraphicsDeviceStatus == GraphicsDeviceStatus.Normal)
            {
                _d3dImage.Lock();

                if (_compositionTargetSetBackBufferRequired)
                {
                    _d3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, _frontBufferPointer);
                    _compositionTargetSetBackBufferRequired = false;
                }

                if (_frontBufferPointer != IntPtr.Zero)
                {
                    _d3dImage.AddDirtyRect(
                        new Int32Rect(
                            0,
                            0,
                            _d3dImage.PixelWidth,
                            _d3dImage.PixelHeight));
                }

                _d3dImage.Unlock();
            }
        }

        private void CreateBuffers()
        {
            var width = _targetSize.Width;
            var height = _targetSize.Height;

            /*
             * Note that the front buffer doesn't need any multisampling/antialiasing (AA)
             * because all rendering is done to the back buffer, which is then copied to
             * the front buffer.  This works out well because D3DImage can only use AA with
             * an IDirect3DDevice9Ex, and XNA only supports IDirect3DDevice9.
             */
            _frontBuffer = new RenderTarget2D(
                _device,
                width,
                height,
                1,
                _device.PresentationParameters.BackBufferFormat,
                MultiSampleType.None, 
                0);

            _backBuffer = new RenderTarget2D(
                _device,
                width,
                height,
                1,
                _device.PresentationParameters.BackBufferFormat,
                _device.PresentationParameters.MultiSampleType,
                _device.PresentationParameters.MultiSampleQuality);

            _depthStencilBuffer = new DepthStencilBuffer(
                _device,
                width,
                height,
                _device.DepthStencilBuffer.Format,
                _device.DepthStencilBuffer.MultiSampleType,
                _device.DepthStencilBuffer.MultiSampleQuality);

            _device.DepthStencilBuffer = _depthStencilBuffer;

            _frontBufferPointer = GetNativePointer(_frontBuffer);
            _backBufferPointer = GetNativePointer(_backBuffer);

            _compositionTargetSetBackBufferRequired = true;
        }

        private void DisposeBuffers()
        {
            _frontBufferPointer = IntPtr.Zero;
            _backBufferPointer = IntPtr.Zero;

            _compositionTargetSetBackBufferRequired = true;

            _device.SetRenderTarget(0, null);
            _device.DepthStencilBuffer = _deviceDepthStencilBuffer;

            _frontBuffer.Dispose();
            _backBuffer.Dispose();
            _depthStencilBuffer.Dispose();
        }

        private void InitializeXna()
        {
            CreateDevice();

            var eye = new Vector3(0.0f, 0.0f, -35.0f);
            var at = new Vector3(0.0f, 0.0f, 0.0f);
            var up = new Vector3(0, 1.0f, 0.0f);

            _worldMatrix = Matrix.Identity;
            _viewMatrix = Matrix.CreateLookAt(eye, at, up);
            _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                (float)Math.PI * 0.25f,
                (float)_targetSize.Width / _targetSize.Height,
                1f,
                1000f);

            CreateBuffers();

            _copySurfaceMethod = typeof(GraphicsDevice).GetMethod("CopySurface", BindingFlags.Instance | BindingFlags.NonPublic);

            this.CreateContentManager();
            this.LoadModel();
        }

        private void CreateContentManager()
        {
            _contentManager = new ContentManager(this, ModelDirectory);
        }

        private void Reset()
        {
            Dispose();

            CreateDevice();
            CreateBuffers();
            CreateContentManager();
            LoadModel();
        }

        private void Dispose()
        {
            lock (_modelLock)
            {
                if (_modelLoadRequest != null)
                {
                    _modelLoadRequest.Dispose();
                    _modelLoadRequest = null;
                }
            }

            // Destroy previous data
            DisposeBuffers();

            _device.EvictManagedResources();
            _device.Dispose();
            _device = null;
            _contentManager.Unload();
            _contentManager.Dispose();
        }

        private void CreateDevice()
        {
            _device = XnaHelper.CreateDevice(
                _targetSize,
                true,
                true,
                true);

            if (_deviceDepthStencilBuffer != null &&
                _deviceDepthStencilBuffer != _device.DepthStencilBuffer &&
                !_deviceDepthStencilBuffer.IsDisposed)
            {
                _deviceDepthStencilBuffer.Dispose();
            }

            _deviceDepthStencilBuffer = _device.DepthStencilBuffer;
        }

        private void Present()
        {
            var now = Environment.TickCount;

            if (_lastTime != 0L && !_isDragging)
                _worldMatrix *= Matrix.CreateRotationY(-(float)(now - _lastTime) / 1000);

            _lastTime = now;

            if (_device.GraphicsDeviceStatus != GraphicsDeviceStatus.Normal)
            {
                while (_device.GraphicsDeviceStatus == GraphicsDeviceStatus.Lost)
                    Thread.Sleep(1);

                Reset();
                return;
            }

            // Do resize
            if (_needResize)
            {
                _needResize = false;

                var backupFrontBuffer = _frontBuffer;
                var backupBackBuffer = _backBuffer;
                var backupDepthStencilBuffer = _depthStencilBuffer;

                CreateBuffers();

                backupFrontBuffer.Dispose();
                backupBackBuffer.Dispose();
                backupDepthStencilBuffer.Dispose();

                return;
            }

            _device.SetRenderTarget(0, _backBuffer);
            _device.DepthStencilBuffer = _depthStencilBuffer;

            Clear();

            Model model;

            lock (_modelLock)
                model = _model;

            if (model != null)
            {
                // Draw the model. A model can have multiple meshes, so loop.

                foreach (var mesh in model.Meshes)
                {
                    // This is where the mesh orientation is set, as well 
                    // as our camera and projection.
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        effect.World = _worldMatrix;
                        effect.View = _viewMatrix;
                        effect.Projection = _projectionMatrix;
                    }
                    
                    // Draw the mesh, using the effects set above.
                    mesh.Draw();
                }
            }

            _copySurfaceMethod.Invoke(_device, new object[] { _backBufferPointer, _frontBufferPointer });

            _device.SetRenderTarget(0, null);
            _device.DepthStencilBuffer = _deviceDepthStencilBuffer;

            try { _device.Present(); }
            catch (DeviceLostException) {}
        }

        private void Clear()
        {
            _device.Clear(
                options: ClearOptions.Target | ClearOptions.DepthBuffer,
                color: Color.TransparentBlack,
                depth: 1.0f,
                stencil: 0);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateTargetSize();
            _needResize = true;
        }

        static unsafe IntPtr GetNativePointer(Microsoft.Xna.Framework.Graphics.RenderTarget renderTarget)
        {
            var comPointerField = renderTarget.GetType().GetField("pComPtr", BindingFlags.Instance | BindingFlags.NonPublic);
            Debug.Assert(comPointerField != null);
            var pointer = comPointerField.GetValue(renderTarget);
            return new IntPtr(Pointer.Unbox(pointer));
        }

        #region Implementation of IServiceProvider
        object IServiceProvider.GetService(Type serviceType)
        {
            return this;
        }
        #endregion

        #region Implementation of IGraphicsDeviceService
        GraphicsDevice IGraphicsDeviceService.GraphicsDevice
        {
            get { return _device; }
        }

        event EventHandler IGraphicsDeviceService.DeviceDisposing
        {
            add { _device.Disposing += value; }
            remove { _device.Disposing -= value; }
        }

        event EventHandler IGraphicsDeviceService.DeviceReset
        {
            add {_device.DeviceReset += value; }
            remove { _device.DeviceReset -= value; }
        }

        event EventHandler IGraphicsDeviceService.DeviceResetting
        {
            add { _device.DeviceResetting += value; }
            remove { _device.DeviceResetting -= value; }
        }

        event EventHandler IGraphicsDeviceService.DeviceCreated
        {
            add {}
            remove {}
        }
        #endregion

        private void LoadModel()
        {
            var modelFile = this.ModelFile;

            lock (_modelLock)
            {
                _model = null;

                if (_modelLoadRequest != null)
                {
                    _modelLoadRequest.Dispose();
                    _modelLoadRequest = null;
                }
            }

            if (string.IsNullOrWhiteSpace(modelFile))
            {
                this.StatusMessage = _modelUnavailableMessage;
                return;
            }

            try
            {
                modelFile = Path.Combine(ModelDirectory, modelFile);

                if (!Path.HasExtension(modelFile))
                    modelFile = modelFile + XnaContentExtension;

                modelFile = Path.Combine(
                    _workingDirectory,
                    ResourceManager.GetResourcePath(modelFile));
            }
            catch
            {
                modelFile = null;
            }

            if (modelFile == null ||
                !File.Exists(modelFile))
            {
                this.StatusMessage = _modelUnavailableMessage;
                return;
            }

            modelFile = modelFile.Substring(
                0,
                modelFile.Length - XnaContentExtension.Length);

            this.StatusMessage = _loadingMessage;

            if (_contentManager == null)
                return;

            lock (_modelLock)
            {
                _modelLoadRequest = ((Func<string, Model>)_contentManager.Load<Model>)
                    .ToAsync(Scheduler.ThreadPool)(modelFile)
                    .ObserveOnDispatcher()
                    .Subscribe(
                        model =>
                        {
                            //var boundingBox = XnaHelper.ComputeBoundingBox(model, Matrix.Identity);
                            //var offset = Math.Max(Math.Abs(boundingBox.Max.Z), Math.Abs(boundingBox.Min.Z)) / 2;

                            var radius = model.Meshes.Max(o => (o.BoundingSphere.Center - Vector3.Zero).Length() + o.BoundingSphere.Radius);
                            var cameraDistance = (radius * this.CameraDistanceMultiplier);//(float)((radius+offset) / Math.Tan(Math.PI / 8d)) + Math.Max(Math.Abs(boundingBox.Max.X), Math.Abs(boundingBox.Min.X)) + 1f;

                            var eye = new Vector3(0.0f, 0.0f, cameraDistance);
                            var at = new Vector3(0.0f, 0.0f, 0.0f);
                            var up = new Vector3(0, 1.0f, 0.0f);

                            ClearValue(StatusMessagePropertyKey);

                            lock (_modelLock)
                            {
                                if (_modelLoadRequest == null)
                                    return;

                                _modelLoadRequest = null;
                                _model = model;
                                _viewMatrix = Matrix.CreateLookAt(eye, at, up);
                            }
                        },
                        e =>
                        {
                            lock (_modelLock)
                            {
                                if (_modelLoadRequest == null)
                                    return;

                                _modelLoadRequest = null;

                                this.StatusMessage = _loadFailureMessage;
                            }
                        });
            }
        }

        private void OnTargetImageLostMouseCapture(object sender, MouseEventArgs e)
        {
            EndDrag();
        }

        private void OnTargetImageMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging)
                return;

            var mousePosition = e.GetPosition(_targetImage);

            var dX = mousePosition.X - _lastMousePosition.X;
            var dY = mousePosition.Y - _lastMousePosition.Y;

            _lastMousePosition = mousePosition;

            _worldMatrix *= Matrix.CreateRotationX(0.01f * (float)dY);
            _worldMatrix *= Matrix.CreateRotationY(0.01f * (float)dX);
        }

        private void OnTargetImageMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDragging)
                return;

            EndDrag();

            e.Handled = true;
        }

        private void EndDrag()
        {
            if (_targetImage.IsMouseCaptured)
                _targetImage.ReleaseMouseCapture();

            _isDragging = false;
            _worldMatrix = Matrix.Identity;
        }

        private void OnTargetImageMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_targetImage.CaptureMouse())
                return;

            _isDragging = true;
            _lastMousePosition = e.GetPosition(_targetImage);
            
            e.Handled = true;
        }
    }
}