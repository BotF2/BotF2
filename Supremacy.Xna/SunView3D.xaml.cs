using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Supremacy.Universe;
using Supremacy.Utility;

using Color = Microsoft.Xna.Framework.Graphics.Color;
using Matrix = Microsoft.Xna.Framework.Matrix;

namespace Supremacy.Xna
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SunView3D : IGraphicsDeviceService, IServiceProvider
    {
        // ReSharper disable InconsistentNaming
        private D3DImage _d3dImage;
        private bool _compositionTargetSetBackBufferRequired;
        private Matrix _projectionMatrix;
        private GraphicsDeviceManager _deviceManager;
        private GraphicsDevice _device;
        private RenderTarget2D _backBuffer;
        private RenderTarget2D _tempBuffer;
        private RenderTarget2D _frontBuffer;
        private IntPtr _frontBufferPointer;
        private IntPtr _backBufferPointer;
        private IntPtr _tempBufferPointer;
        private MethodInfo _copySurfaceMethod;
        private ContentManager _contentManager;
        private bool _needResize;
        private Int32Rect _targetSize;
        private long _lastTime;
        private readonly Stopwatch _stopwatch;
        private float _dt;
        private PostProcessor _postProcessor;
        private bool _usePostProcessor;
        private Sun _sun;
        private DispatcherTimer _timer;
        // ReSharper restore InconsistentNaming

        #region StarType Property
        public static readonly DependencyProperty StarTypeProperty = DependencyProperty.Register(
            "StarType",
            typeof(StarType),
            typeof(SunView3D),
            new PropertyMetadata(Supremacy.Universe.StarType.Yellow));

        public StarType StarType
        {
            get { return (StarType)GetValue(StarTypeProperty); }
            set { SetValue(StarTypeProperty, value); }
        }
        #endregion

        public SunView3D()
        {
            InitializeComponent();

            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
            this.SizeChanged += OnSizeChanged;

            _stopwatch = new Stopwatch();
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

            if (_deviceManager.GraphicsDevice.GraphicsDeviceCapabilities.DeviceType == DeviceType.Hardware)
            {
                CompositionTarget.Rendering += OnCompositionTargetRendering;
            }
            else
            {
                _timer = XnaHelper.CreateRenderTimer();
                _timer.Tick += OnCompositionTargetRendering;
                _timer.Start();
            }

            _stopwatch.Start();
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
            _stopwatch.Stop();

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
            typeof(SunView3D),
            new FrameworkPropertyMetadata((d, e) => ((SunView3D)d).LoadSun()));

        public string ModelFile
        {
            get { return (string)GetValue(ModelFileProperty); }
            set { SetValue(ModelFileProperty, value); }
        }
        #endregion

        #region CameraDistanceMultiplier Property
        public static readonly DependencyProperty CameraDistanceMultiplierProperty = DependencyProperty.Register(
            "CameraDistanceMultiplier",
            typeof(float),
            typeof(SunView3D),
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
                _deviceManager.GraphicsDevice.GraphicsDeviceStatus == GraphicsDeviceStatus.Normal)
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
            var device = _deviceManager.GraphicsDevice;

            _frontBuffer = new RenderTarget2D(
                device,
                width,
                height,
                1,
                device.PresentationParameters.BackBufferFormat,
                MultiSampleType.None,
                0);

            _backBuffer = new RenderTarget2D(
                device,
                width,
                height,
                1,
                device.PresentationParameters.BackBufferFormat,
                device.PresentationParameters.MultiSampleType,
                device.PresentationParameters.MultiSampleQuality);

            _tempBuffer = new RenderTarget2D(
                device,
                width,
                height,
                1,
                device.PresentationParameters.BackBufferFormat,
                device.PresentationParameters.MultiSampleType,
                device.PresentationParameters.MultiSampleQuality);

            _frontBufferPointer = GetNativePointer(_frontBuffer);
            _backBufferPointer = GetNativePointer(_backBuffer);
            _tempBufferPointer = GetNativePointer(_tempBuffer);

            _compositionTargetSetBackBufferRequired = true;
        }

        private void DisposeBuffers()
        {
            _frontBufferPointer = IntPtr.Zero;
            _backBufferPointer = IntPtr.Zero;
            _tempBufferPointer = IntPtr.Zero;

            _compositionTargetSetBackBufferRequired = true;

            _device.SetRenderTarget(0, null);

            _frontBuffer.Dispose();
            _backBuffer.Dispose();
        }

        private void InitializeXna()
        {
            CreateDevice();

            _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                (float)Math.PI * 0.25f,
                (float)_targetSize.Width / _targetSize.Height,
                0.05f,
                1000f);

            _copySurfaceMethod = typeof(GraphicsDevice).GetMethod("CopySurface", BindingFlags.Instance | BindingFlags.NonPublic);

            CreateBuffers();
            CreateContentManager();
            LoadSun();
        }

        private void CreateContentManager()
        {
            _contentManager = new ContentManager(_deviceManager, Environment.CurrentDirectory);
            
            try
            {
                _postProcessor = new PostProcessor(_deviceManager.GraphicsDevice, _contentManager);
            }
            catch (Exception e)
            {
                _usePostProcessor = false;
                GameLog.Client.General.Error("An error occurred loading the HDR post-processor effects.", e);
            }
        }

        private void Reset()
        {
            Dispose();

            CreateDevice();
            CreateBuffers();
            CreateContentManager();
            LoadSun();
        }

        private void Dispose()
        {
            // Destroy previous data
            DisposeBuffers();

            _deviceManager.Dispose();
            _device.EvictManagedResources();
            _device.Dispose();
            _device = null;
            _contentManager.Unload();
            _contentManager.Dispose();
        }

        private void CreateDevice()
        {
            _device = XnaHelper.CreateDevice(_targetSize, false, false, false);

            _usePostProcessor = _device.GraphicsDeviceCapabilities.DeviceType == DeviceType.Hardware &&
                                _device.GraphicsDeviceCapabilities.MaxPixelShaderProfile >= ShaderProfile.PS_3_0;
        }

        private void Present()
        {
            var time = _stopwatch.ElapsedMilliseconds;

            _dt = (time - _lastTime) / 1000.0f;
            _lastTime = time;

            if (_lastTime != 0L)
                _sun.Update(TimeSpan.FromSeconds(_dt));

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
                var backupTempBuffer = _tempBuffer;

                CreateBuffers();

                backupFrontBuffer.Dispose();
                backupBackBuffer.Dispose();
                backupTempBuffer.Dispose();

                return;
            }

            _device.SetRenderTarget(0, _backBuffer);

            Clear();

            _sun.Render(this.StarType);

            if (_usePostProcessor)
            {
                _postProcessor.ToneMap(_backBuffer, _tempBuffer, _dt, false, true);
                _copySurfaceMethod.Invoke(_device, new object[] { _tempBufferPointer, _frontBufferPointer });
            }
            else
            {
                _copySurfaceMethod.Invoke(_device, new object[] { _backBufferPointer, _frontBufferPointer });
            }

            //_copySurfaceMethod.Invoke(_device, new object[] { _backBufferPointer, _frontBufferPointer });

            _device.SetRenderTarget(0, null);

            try { _device.Present(); }
            catch (DeviceLostException) {}
        }

        private void Clear()
        {
            _device.Clear(
                options: ClearOptions.Target,
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
            add { _device.DeviceReset += value; }
            remove { _device.DeviceReset -= value; }
        }

        event EventHandler IGraphicsDeviceService.DeviceResetting
        {
            add { _device.DeviceResetting += value; }
            remove { _device.DeviceResetting -= value; }
        }

        event EventHandler IGraphicsDeviceService.DeviceCreated
        {
            add { }
            remove { }
        }
        #endregion

        private void LoadSun()
        {
            var camera = new Camera(_projectionMatrix) { ViewPosition = new Vector3(-.0f, .0f, 1.2f) };

            _sun = new Sun();
            _sun.Create(this, _contentManager, camera);
        }
    }
}
