using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Supremacy.Resources;
using Supremacy.Types;
using Supremacy.Utility;

namespace Supremacy.Xna
{
    public enum ExitRunScopeBehavior
    {
        Stop,
        Suspend
    }

    public class XnaComponent
    {
        private static readonly MethodInfo _copySurfaceMethod;
        private readonly ExitRunScopeBehavior _exitRunScopeBehavior;
        private readonly StateScope _deviceTransitionScope;
        private readonly StateScope _suppressDrawScope;
        private readonly XnaClock _clock;
        private readonly XnaTime _time;
        private readonly XnaTimer _timer;
        private readonly StateScope _runScope;
        private IntPtr _frontBufferPointer;
        private DepthStencilBuffer _depthStencilBuffer;
        private DepthStencilBuffer _deviceDepthStencilBuffer;
        private int _updatesSinceRunningSlowly1 = 0x7fffffff;
        private int _updatesSinceRunningSlowly2 = 0x7fffffff;
        private bool _drawRunningSlowly;
        private readonly TimeSpan _maximumElapsedTime = TimeSpan.FromMilliseconds(500.0);
        private bool _suppressDraw;
        private bool _forceElapsedTimeToZero;
        private readonly bool _isFixedTimeStep;
        private bool _doneFirstUpdate;
        private TimeSpan _targetElapsedTime;
        private TimeSpan _totalGameTime;
        private TimeSpan _accumulatedElapsedGameTime;
        private TimeSpan _lastFrameElapsedGameTime;
        private TimeSpan _lastFrameElapsedRealTime;
        private Int32Rect _targetSize;
        private IGraphicsDeviceService _graphicsDeviceService;
        private bool _compositionTargetSetBackBufferRequired;
        private static FieldInfo _comPointerField;
        private bool _targetSizeChanged;
        private bool _clockSuspended;
        private bool _buffersCreated;

        public event EventHandler<CustomPresentEventArgs> CustomPresent;

        public event EventHandler FramePresented;

        public event EventHandler LoadingContent;
        public event EventHandler LoadedContent;
        public event EventHandler UnloadingContent;
        public event EventHandler UnloadedContent;

        public virtual event EventHandler<TargetSizeChangedEventArgs> TargetSizeChanged;

        static XnaComponent()
        {
            _copySurfaceMethod = typeof(GraphicsDevice).GetMethod("CopySurface", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public XnaComponent(
            bool enableDepthStencil,
            bool preferMultiSampling,
            bool preferAnisotropicFiltering,
            Int32Rect targetSize = default,
            ExitRunScopeBehavior exitRunScopeBehavior = ExitRunScopeBehavior.Stop)
        {
            _exitRunScopeBehavior = exitRunScopeBehavior;
            GraphicsOptions = new XnaGraphicsOptions(enableDepthStencil, preferAnisotropicFiltering, preferMultiSampling);
            _deviceTransitionScope = new StateScope();
            _suppressDrawScope = new StateScope();
            Services = new ServiceContainer();

            _maximumElapsedTime = TimeSpan.FromMilliseconds(500.0);
            _time = new XnaTime();
            _isFixedTimeStep = false;
            _updatesSinceRunningSlowly1 = 0x7fffffff;
            _updatesSinceRunningSlowly2 = 0x7fffffff;

            _clock = new XnaClock();
            _totalGameTime = TimeSpan.Zero;
            _accumulatedElapsedGameTime = TimeSpan.Zero;
            _lastFrameElapsedGameTime = TimeSpan.Zero;
            _targetElapsedTime = TimeSpan.FromTicks(166667);

            _timer = new XnaTimer();

            _targetSize = new Int32Rect(0, 0, Math.Max(1, targetSize.Width), Math.Max(1, targetSize.Height));

            _runScope = new StateScope(OnRunScopeIsWithinChanged);
        }

        public IDisposable Run()
        {
            return _runScope.Enter();
        }

        private void OnRunScopeIsWithinChanged()
        {
            if (_runScope.IsWithin)
            {
                RunOrResume();
                return;
            }

            if (_exitRunScopeBehavior == ExitRunScopeBehavior.Stop)
            {
                Stop();
            }
            else
            {
                Suspend();
            }
        }

        public bool IsRunning { get; private set; }

        public D3DImage RenderTargetImage { get; private set; }

        protected RenderTarget2D BackBuffer { get; private set; }

        protected RenderTarget2D FrontBuffer { get; private set; }

        protected internal Int32Rect TargetSize
        {
            get => _targetSize;
            set
            {
                if (value == _targetSize)
                {
                    return;
                }

                Int32Rect oldSize = _targetSize;

                _targetSize = value;
                _targetSizeChanged = true;

                EventHandler<TargetSizeChangedEventArgs> onTargetSizeChanged = TargetSizeChanged;
                onTargetSizeChanged?.Raise(this, new TargetSizeChangedEventArgs(oldSize, value));
            }
        }

        protected internal XnaGraphicsOptions GraphicsOptions { get; }

        public ServiceContainer Services { get; }

        protected bool IsDeviceInTransition => _deviceTransitionScope.IsWithin;

        protected internal GraphicsDeviceManager Graphics { get; private set; }

        protected internal ContentManager Content { get; private set; }

        private void OnTimerTick(object sender, EventArgs eventArgs)
        {
            Tick();
        }

        internal IDisposable BeginDeviceChange()
        {
            return _deviceTransitionScope.Enter();
        }

        protected virtual void BeginRun() { }
        protected virtual void EndRun() { }
        protected virtual void LoadContent() { }
        protected virtual void UnloadContent() { }
        protected virtual void Present(XnaTime time) { }

        protected virtual void Initialize()
        {
            if (_graphicsDeviceService != null && _graphicsDeviceService.GraphicsDevice != null)
            {
                DoLoadContent();
            }
        }

        private void DoLoadContent()
        {
            LoadingContent.Raise(this);
            LoadContent();
            LoadedContent.Raise(this);
        }

        private void DoUnloadContent()
        {
            UnloadingContent.Raise(this);
            UnloadContent();
            UnloadedContent.Raise(this);
        }

        private void PreInitialize()
        {
            _graphicsDeviceService = Graphics = XnaHelper.CreateDeviceManager(this);
            Content = new ContentManager(Services, ResourceManager.WorkingDirectory);

            HookDeviceEvents();

            Graphics.CreateDevice();

            RenderTargetImage = new D3DImage();
            RenderTargetImage.Lock();

            try
            {
                RenderTargetImage.SetBackBuffer(
                    D3DResourceType.IDirect3DSurface9,
                    _frontBufferPointer);
            }
            finally
            {
                RenderTargetImage.Unlock();
            }
        }

        protected virtual void CreateBuffers()
        {
            if (_buffersCreated)
            {
                return;
            }

            int width = _targetSize.Width;
            int height = _targetSize.Height;

            /*
             * Note that the front buffer doesn't need any multisampling/antialiasing (AA)
             * because all rendering is done to the back buffer, which is then copied to
             * the front buffer.  This works out well because D3DImage can only use AA with
             * an IDirect3DDevice9Ex, and XNA only supports IDirect3DDevice9.
             */
            GraphicsDevice device = Graphics.GraphicsDevice;
            if (device == null)
            {
                return;
            }

            FrontBuffer = new RenderTarget2D(
                device,
                width,
                height,
                1,
                device.PresentationParameters.BackBufferFormat,
                MultiSampleType.None,
                0);

            BackBuffer = new RenderTarget2D(
                device,
                width,
                height,
                1,
                device.PresentationParameters.BackBufferFormat,
                device.PresentationParameters.MultiSampleType,
                device.PresentationParameters.MultiSampleQuality);

            _frontBufferPointer = GetRenderTargetPointer(FrontBuffer);

            if (GraphicsOptions.EnableDepthStencil)
            {
                _depthStencilBuffer = new DepthStencilBuffer(
                    device,
                    width,
                    height,
                    device.DepthStencilBuffer.Format,
                    device.DepthStencilBuffer.MultiSampleType,
                    device.DepthStencilBuffer.MultiSampleQuality);

                _deviceDepthStencilBuffer = device.DepthStencilBuffer;

                device.DepthStencilBuffer = _depthStencilBuffer;
            }

            _buffersCreated = true;
        }

        protected virtual void DisposeBuffers()
        {
            if (!_buffersCreated)
            {
                return;
            }

            _frontBufferPointer = IntPtr.Zero;

            _compositionTargetSetBackBufferRequired = true;

            GraphicsDevice device = Graphics.GraphicsDevice;
            if (device != null && !device.IsDisposed)
            {
                device.SetRenderTarget(0, null);
            }

            FrontBuffer?.Dispose();

            if (FrontBuffer != null)
            {
                BackBuffer.Dispose();
            }

            FrontBuffer = null;
            BackBuffer = null;

            if (_deviceDepthStencilBuffer != null)
            {
                if (device != null && !device.IsDisposed)
                {
                    device.DepthStencilBuffer = _deviceDepthStencilBuffer;
                }
                else if (!_deviceDepthStencilBuffer.IsDisposed)
                {
                    _deviceDepthStencilBuffer.Dispose();
                }
            }

            _depthStencilBuffer?.Dispose();

            _depthStencilBuffer = null;
            _deviceDepthStencilBuffer = null;

            _buffersCreated = false;
        }

        protected virtual bool BeginDraw()
        {
            return Graphics == null || Graphics.BeginDraw();
        }

        private void HookDeviceEvents()
        {
            if (_graphicsDeviceService == null)
            {
                return;
            }

            _graphicsDeviceService.DeviceCreated += DeviceCreated;
            _graphicsDeviceService.DeviceResetting += DeviceResetting;
            _graphicsDeviceService.DeviceReset += DeviceReset;
            _graphicsDeviceService.DeviceDisposing += DeviceDisposing;
        }

        private void UnhookDeviceEvents()
        {
            if (_graphicsDeviceService == null)
            {
                return;
            }

            _graphicsDeviceService.DeviceCreated -= DeviceCreated;
            _graphicsDeviceService.DeviceResetting -= DeviceResetting;
            _graphicsDeviceService.DeviceReset -= DeviceReset;
            _graphicsDeviceService.DeviceDisposing -= DeviceDisposing;
        }

        private void DeviceCreated(object sender, EventArgs e)
        {
            CreateBuffers();
            DoLoadContent();
        }

        private void DeviceDisposing(object sender, EventArgs e)
        {
            Content.Unload();
            DisposeBuffers();
            DoUnloadContent();
        }

        private void DeviceReset(object sender, EventArgs e) { }
        private void DeviceResetting(object sender, EventArgs e) { }
        protected virtual void Dispose() { }

        public void Tick()
        {
            _clock.Step();

            bool suppressDraw = true;

            _time.TotalRealTime = _clock.CurrentTime;
            _time.ElapsedRealTime = _clock.ElapsedTime;
            _lastFrameElapsedRealTime += _clock.ElapsedTime;

            TimeSpan elapsedAdjustedTime = _clock.ElapsedAdjustedTime;
            if (elapsedAdjustedTime < TimeSpan.Zero)
            {
                elapsedAdjustedTime = TimeSpan.Zero;
            }

            if (_forceElapsedTimeToZero)
            {
                _time.ElapsedRealTime = _lastFrameElapsedRealTime = elapsedAdjustedTime = TimeSpan.Zero;
                _forceElapsedTimeToZero = false;
            }

            if (elapsedAdjustedTime > _maximumElapsedTime)
            {
                elapsedAdjustedTime = _maximumElapsedTime;
            }

            if (_isFixedTimeStep)
            {
                if (Math.Abs((long)(sbyte)(elapsedAdjustedTime.Ticks - _targetElapsedTime.Ticks)) < (_targetElapsedTime.Ticks >> 6))
                {
                    elapsedAdjustedTime = _targetElapsedTime;
                }

                _accumulatedElapsedGameTime += elapsedAdjustedTime;

                long progress = _accumulatedElapsedGameTime.Ticks / _targetElapsedTime.Ticks;

                _accumulatedElapsedGameTime = TimeSpan.FromTicks(_accumulatedElapsedGameTime.Ticks % _targetElapsedTime.Ticks);
                _lastFrameElapsedGameTime = TimeSpan.Zero;

                if (progress == 0L)
                {
                    return;
                }

                TimeSpan targetElapsedTime = _targetElapsedTime;

                if (progress > 1L)
                {
                    _updatesSinceRunningSlowly2 = _updatesSinceRunningSlowly1;
                    _updatesSinceRunningSlowly1 = 0;
                }
                else
                {
                    if (_updatesSinceRunningSlowly1 < 0x7fffffff)
                    {
                        _updatesSinceRunningSlowly1++;
                    }

                    if (_updatesSinceRunningSlowly2 < 0x7fffffff)
                    {
                        _updatesSinceRunningSlowly2++;
                    }
                }

                _drawRunningSlowly = _updatesSinceRunningSlowly2 < 20;

                while (progress > 0L)
                {
                    progress -= 1L;

                    try
                    {
                        _time.ElapsedGameTime = targetElapsedTime;
                        _time.TotalGameTime = _totalGameTime;
                        _time.IsRunningSlowly = _drawRunningSlowly;

                        Update(_time);

                        suppressDraw &= _suppressDraw || _suppressDrawScope.IsWithin;

                        _suppressDraw = false;
                    }
                    finally
                    {
                        _lastFrameElapsedGameTime += targetElapsedTime;
                        _totalGameTime += targetElapsedTime;
                    }
                }
            }
            else
            {
                TimeSpan elapsedTime = elapsedAdjustedTime;

                _drawRunningSlowly = false;
                _updatesSinceRunningSlowly1 = 0x7fffffff;
                _updatesSinceRunningSlowly2 = 0x7fffffff;

                try
                {
                    _time.ElapsedGameTime = _lastFrameElapsedGameTime = elapsedTime;
                    _time.TotalGameTime = _totalGameTime;
                    _time.IsRunningSlowly = false;

                    Update(_time);

                    suppressDraw &= _suppressDraw || _suppressDrawScope.IsWithin;

                    _suppressDraw = false;
                }
                finally
                {
                    _totalGameTime += elapsedTime;
                }
            }

            if (!suppressDraw)
            {
                PresentFrame();
            }
        }

        protected virtual void EndDraw()
        {
            if (Graphics == null)
            {
                return;
            }

            Graphics.EndDraw();
        }

        private void PresentFrame()
        {
            try
            {
                if (_doneFirstUpdate && BeginDraw())
                {
                    _time.TotalRealTime = _clock.CurrentTime;
                    _time.ElapsedRealTime = _lastFrameElapsedRealTime;
                    _time.TotalGameTime = _totalGameTime;
                    _time.ElapsedGameTime = _lastFrameElapsedGameTime;
                    _time.IsRunningSlowly = _drawRunningSlowly;

                    if (_targetSizeChanged)
                    {
                        DisposeBuffers();
                        CreateBuffers();
                        _targetSizeChanged = false;
                    }

                    Graphics.GraphicsDevice.SetRenderTarget(0, BackBuffer);

                    DoPresent();

                    CopySurface(BackBuffer, FrontBuffer);

                    Graphics.GraphicsDevice.SetRenderTarget(0, null);

                    Compose();
                    EndDraw();

                    FramePresented.Raise(this);
                }
            }
            finally
            {
                _lastFrameElapsedRealTime = TimeSpan.Zero;
                _lastFrameElapsedGameTime = TimeSpan.Zero;
            }
        }

        private void DoPresent()
        {
            EventHandler<CustomPresentEventArgs> customPresent = CustomPresent;
            if (customPresent != null)
            {
                CustomPresentEventArgs customPresentArgs = new CustomPresentEventArgs(_time);

                customPresent(this, customPresentArgs);

                if (!customPresentArgs.Handled)
                {
                    Present(_time);
                }
            }
            else
            {
                Present(_time);
            }
        }

        private void Compose()
        {
            if (RenderTargetImage == null)
            {
                return;
            }

            if (RenderTargetImage.IsFrontBufferAvailable &&
                Graphics.GraphicsDevice.GraphicsDeviceStatus == GraphicsDeviceStatus.Normal)
            {
                RenderTargetImage.Lock();

                if (_compositionTargetSetBackBufferRequired)
                {
                    RenderTargetImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, _frontBufferPointer);
                    _compositionTargetSetBackBufferRequired = false;
                }

                if (_frontBufferPointer != IntPtr.Zero)
                {
                    RenderTargetImage.AddDirtyRect(
                        new Int32Rect(
                            0,
                            0,
                            RenderTargetImage.PixelWidth,
                            RenderTargetImage.PixelHeight));
                }

                RenderTargetImage.Unlock();
            }
        }

        protected virtual void Update(XnaTime gameTime)
        {
            _doneFirstUpdate = true;
        }

        protected void SkipDraw()
        {
            _suppressDraw = true;
        }

        public IDisposable SuppressDraw()
        {
            return _suppressDrawScope.Enter();
        }

        protected void RunOrResume()
        {
            if (!IsRunning)
            {
                RunCore();
                return;
            }

            if (_clockSuspended)
            {
                _clock.Resume();
            }

            _clockSuspended = false;

            _timer.Start(Graphics.GraphicsDevice.GraphicsDeviceCapabilities.DeviceType == DeviceType.Hardware);
        }

        public void ResetElapsedTime()
        {
            _forceElapsedTimeToZero = true;
            _drawRunningSlowly = false;
            _updatesSinceRunningSlowly1 = 0x7fffffff;
            _updatesSinceRunningSlowly2 = 0x7fffffff;
        }

        private void RunCore()
        {
            try
            {
                if (IsRunning)
                {
                    return;
                }

                PreInitialize();
                Initialize();

                IsRunning = true;

                BeginRun();

                _time.ElapsedGameTime = TimeSpan.Zero;
                _time.ElapsedRealTime = TimeSpan.Zero;
                _time.TotalGameTime = _totalGameTime;
                _time.TotalRealTime = _clock.CurrentTime;
                _time.IsRunningSlowly = false;

                GraphicsDeviceManager deviceManager = Graphics;

                bool usingHardwareDevice = deviceManager != null &&
                                           deviceManager.GraphicsDevice.GraphicsDeviceCapabilities.DeviceType == DeviceType.Hardware;

                _timer.Tick += OnTimerTick;
                _timer.Start(usingHardwareDevice);

                Update(_time);

                _doneFirstUpdate = true;
            }
            catch (NoSuitableGraphicsDeviceException exception)
            {
                if (!ShowMissingRequirementMessage(exception))
                {
                    throw;
                }
            }
        }

        protected void Stop()
        {
            lock (this)
            {
                if (!IsRunning)
                {
                    return;
                }

                IsRunning = false;

                _timer.Tick -= OnTimerTick;
                _timer.Stop();

                if (_clockSuspended)
                {
                    _clock.Resume();
                }

                _clockSuspended = false;

                _clock.Reset();

                DisposeBuffers();

                Graphics?.Dispose();

                Graphics = null;

                Content?.Dispose();

                Content = null;

                UnhookDeviceEvents();

                EndRun();
            }

            Dispose();
        }

        private static bool ShowMissingRequirementMessage(Exception exception)
        {
            string message;

            if (exception is NoSuitableGraphicsDeviceException)
            {
                message = "No suitable graphics card found." + "\n\n" + exception.Message;

                object minimumPixelShaderProfile = exception.Data["MinimumPixelShaderProfile"];
                object minimumVertexShaderProfile = exception.Data["MinimumVertexShaderProfile"];

                if (minimumPixelShaderProfile is ShaderProfile && minimumVertexShaderProfile is ShaderProfile)
                {
                    string psName = GetShaderProfileName((ShaderProfile)minimumPixelShaderProfile);
                    string vsName = GetShaderProfileName((ShaderProfile)minimumVertexShaderProfile);

                    message = message + "\n\n" + string.Format(
                        "This program requires pixel shader {0} and vertex shader {1}.",
                        psName, vsName);
                }
            }
            else
            {
                return false;
            }

            _ = MessageBox.Show(
                Application.Current.MainWindow,
                message,
                string.Empty,
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Environment.Exit(-1);


            return true;

        }

        private static string GetShaderProfileName(ShaderProfile shaderProfile)
        {
            switch (shaderProfile)
            {
                case ShaderProfile.PS_1_1:
                    return "1.1";

                case ShaderProfile.PS_1_2:
                    return "1.2";

                case ShaderProfile.PS_1_3:
                    return "1.3";

                case ShaderProfile.PS_1_4:
                    return "1.4";

                case ShaderProfile.PS_2_0:
                    return "2.0";

                case ShaderProfile.PS_2_A:
                    return "2.0a";

                case ShaderProfile.PS_2_B:
                    return "2.0b";

                case ShaderProfile.PS_2_SW:
                    return "2.0sw";

                case ShaderProfile.PS_3_0:
                    return "3.0";

                case ShaderProfile.VS_1_1:
                    return "1.1";

                case ShaderProfile.VS_2_0:
                    return "2.0";

                case ShaderProfile.VS_2_A:
                    return "2.0a";

                case ShaderProfile.VS_2_SW:
                    return "2.0sw";

                case ShaderProfile.VS_3_0:
                    return "3.0";
            }
            return shaderProfile.ToString();
        }

        protected static unsafe IntPtr GetRenderTargetPointer(RenderTarget renderTarget)
        {
            if (_comPointerField == null)
            {
                _comPointerField = renderTarget.GetType().GetField("pComPtr", BindingFlags.Instance | BindingFlags.NonPublic);
                Debug.Assert(_comPointerField != null);
            }
            object pointer = _comPointerField.GetValue(renderTarget);
            return new IntPtr(Pointer.Unbox(pointer));
        }

        protected void CopySurface(RenderTarget2D source, RenderTarget2D destination)
        {
            _ = _copySurfaceMethod.Invoke(
                Graphics.GraphicsDevice,
                new object[] { GetRenderTargetPointer(BackBuffer), GetRenderTargetPointer(FrontBuffer) });
        }

        public void Suspend()
        {
            _clock.Suspend();
            _clockSuspended = true;
            _timer.Stop();
        }
    }
}