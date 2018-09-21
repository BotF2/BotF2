using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

using Microsoft.Xna.Framework.Graphics;

using Supremacy.Annotations;

using System.Linq;
using Supremacy.Utility;

namespace Supremacy.Xna
{
    public interface IGraphicsDeviceManager
    {
        bool BeginDraw();
        void CreateDevice();
        void EndDraw();
    }

    public class GraphicsDeviceManager : IGraphicsDeviceService, IDisposable, IServiceProvider, IGraphicsDeviceManager
    {
        public const int DefaultBackBufferHeight = 600;
        public const int DefaultBackBufferWidth = 800;

        public static readonly SurfaceFormat[] ValidAdapterFormats = new[]
                                                                     {
                                                                         SurfaceFormat.Bgr32,
                                                                         SurfaceFormat.Bgr555,
                                                                         SurfaceFormat.Bgr565,
                                                                         SurfaceFormat.Bgra1010102
                                                                     };

        public static readonly SurfaceFormat[] ValidBackBufferFormats = new[]
                                                                        {
                                                                            SurfaceFormat.Bgr565,
                                                                            SurfaceFormat.Bgr555,
                                                                            SurfaceFormat.Bgra5551,
                                                                            SurfaceFormat.Bgr32,
                                                                            SurfaceFormat.Color,
                                                                            SurfaceFormat.Bgra1010102
                                                                        };

        public static readonly DeviceType[] ValidDeviceTypes = new[]
                                                               {
                                                                   DeviceType.Hardware,
                                                                   DeviceType.Reference
                                                               };

        private static readonly TimeSpan _deviceLostSleepTime = TimeSpan.FromMilliseconds(50.0);

        private static readonly DepthFormat[] _depthFormatsWithoutStencil;
        private static readonly DepthFormat[] _depthFormatsWithStencil;
        private static readonly MultiSampleType[] _multiSampleTypes;

        private readonly XnaComponent _owner;

        private bool _allowMultiSampling;
        private SurfaceFormat _backBufferFormat = SurfaceFormat.Color;
        private bool _beginDrawOk;
        private DepthFormat _depthStencilFormat = DepthFormat.Depth24;
        private GraphicsDevice _device;
        private bool _inDeviceTransition;
        private bool _isDeviceDirty;
        private ShaderProfile _minimumPixelShaderProfile;
        private ShaderProfile _minimumVertexShaderProfile = ShaderProfile.VS_1_1;
        private int _resizedBackBufferHeight;
        private int _resizedBackBufferWidth;
        private bool _synchronizeWithVerticalRetrace = true;
        private bool _useResizedBackBuffer;
        private Int32Rect _clientBounds;

        public event EventHandler DeviceCreated;
        public event EventHandler DeviceDisposing;
        public event EventHandler DeviceReset;
        public event EventHandler DeviceResetting;
        public event EventHandler Disposed;
        public event EventHandler<PreparingDeviceSettingsEventArgs> PreparingDeviceSettings;

        static GraphicsDeviceManager()
        {
            _multiSampleTypes = new[]
                                {
                                    MultiSampleType.NonMaskable,
                                    MultiSampleType.SixteenSamples,
                                    MultiSampleType.FifteenSamples,
                                    MultiSampleType.FourteenSamples,
                                    MultiSampleType.ThirteenSamples,
                                    MultiSampleType.TwelveSamples,
                                    MultiSampleType.ElevenSamples,
                                    MultiSampleType.TenSamples,
                                    MultiSampleType.NineSamples,
                                    MultiSampleType.EightSamples,
                                    MultiSampleType.SevenSamples,
                                    MultiSampleType.SixSamples,
                                    MultiSampleType.FiveSamples,
                                    MultiSampleType.FourSamples,
                                    MultiSampleType.ThreeSamples,
                                    MultiSampleType.TwoSamples
                                };

            _depthFormatsWithStencil = new[]
                                       {
                                           DepthFormat.Depth24Stencil8,
                                           DepthFormat.Depth24Stencil4,
                                           DepthFormat.Depth24Stencil8Single,
                                           DepthFormat.Depth15Stencil1
                                       };

            _depthFormatsWithoutStencil = new[]
                                          {
                                              DepthFormat.Depth24,
                                              DepthFormat.Depth32,
                                              DepthFormat.Depth16
                                          };
        }

        public GraphicsDeviceManager([NotNull] XnaComponent owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            _owner = owner;
            _owner.Services.AddService(typeof(IGraphicsDeviceService), this);
            _owner.Services.AddService(typeof(IGraphicsDeviceManager), this);
            _owner.TargetSizeChanged += OnOwnerSizeChanged;

            UpdateClientBounds(_owner.TargetSize);
        }

        private void OnOwnerSizeChanged(object sender, TargetSizeChangedEventArgs e)
        {
            UpdateClientBounds(e.NewSize);
        }

        private void AddDevices(List<GraphicsDeviceInformation> foundDevices)
        {
            foreach (var adapter in GraphicsAdapter.Adapters)
            {
                foreach (var type in ValidDeviceTypes)
                {
                    try
                    {
                        if (!adapter.IsDeviceTypeAvailable(type))
                            continue;

                        var capabilities = adapter.GetCapabilities(type);

                        if (capabilities.DeviceCapabilities.IsDirect3D9Driver &&
                            IsValidShaderProfile(capabilities.MaxPixelShaderProfile, MinimumPixelShaderProfile) &&
                            IsValidShaderProfile(capabilities.MaxVertexShaderProfile, MinimumVertexShaderProfile))
                        {
                            var baseDeviceInfo = new GraphicsDeviceInformation
                                                 {
                                                     Adapter = adapter,
                                                     DeviceType = type,
                                                     PresentationParameters =
                                                     {
                                                         DeviceWindowHandle = GetDesktopWindow(),
                                                         EnableAutoDepthStencil = true,
                                                         BackBufferCount = 1,
                                                         PresentOptions = PresentOptions.None,
                                                         SwapEffect = SwapEffect.Discard,
                                                         FullScreenRefreshRateInHz = 0,
                                                         MultiSampleQuality = 0,
                                                         MultiSampleType = MultiSampleType.None,
                                                         IsFullScreen = false,
                                                         PresentationInterval = SynchronizeWithVerticalRetrace
                                                                                 ? PresentInterval.One
                                                                                 : PresentInterval.Immediate
                                                     }
                                                 };

                            AddDevices(
                                adapter,
                                type,
                                adapter.CurrentDisplayMode,
                                baseDeviceInfo,
                                foundDevices);
                        }
                    }
                    catch (DeviceNotSupportedException) {}
                }
            }
        }

        private void AddDevices(
            GraphicsAdapter adapter,
            DeviceType deviceType,
            DisplayMode mode,
            GraphicsDeviceInformation baseDeviceInfo,
            List<GraphicsDeviceInformation> foundDevices)
        {
            foreach (var backBufferFormat in ValidBackBufferFormats)
            {
                if (!adapter.CheckDeviceType(deviceType, mode.Format, backBufferFormat, false))
                    continue;

                var item = baseDeviceInfo.Clone();

                if (_useResizedBackBuffer)
                {
                    item.PresentationParameters.BackBufferWidth = _resizedBackBufferWidth;
                    item.PresentationParameters.BackBufferHeight = _resizedBackBufferHeight;
                }
                else
                {
                    item.PresentationParameters.BackBufferWidth = PreferredBackBufferWidth;
                    item.PresentationParameters.BackBufferHeight = PreferredBackBufferHeight;
                }

                item.PresentationParameters.BackBufferFormat = backBufferFormat;
                item.PresentationParameters.AutoDepthStencilFormat = ChooseDepthStencilFormat(adapter, deviceType, mode.Format);

                if (PreferMultiSampling)
                {
                    foreach (var multiSampleType in _multiSampleTypes)
                    {
                        var sampleType = multiSampleType;

                        if (!adapter.CheckDeviceMultiSampleType(deviceType, backBufferFormat, false, sampleType, out int qualityLevels))
                            continue;

                        var clone = item.Clone();

                        clone.PresentationParameters.MultiSampleType = sampleType;

                        if (!foundDevices.Contains(clone))
                            foundDevices.Add(clone);

                        break;
                    }
                }
                else if (!foundDevices.Contains(item))
                {
                    foundDevices.Add(item);
                }
            }
        }

        public void ApplyChanges()
        {
            if (_device != null && !_isDeviceDirty)
                return;

            ChangeDevice(false);
        }

        protected virtual bool CanResetDevice(GraphicsDeviceInformation newDeviceInfo)
        {
            if (_device.CreationParameters.DeviceType != newDeviceInfo.DeviceType)
                return false;

            return true;
        }

        private void ChangeDevice(bool forceCreate)
        {
            using (_owner.BeginDeviceChange())
            {
                _inDeviceTransition = true;

                try
                {
                    var deviceInformation = FindBestDevice();
                    var newDeviceRequired = true;

                    if (!forceCreate && _device != null)
                    {
                        OnPreparingDeviceSettings(
                            this,
                            new PreparingDeviceSettingsEventArgs(deviceInformation));

                        if (CanResetDevice(deviceInformation))
                        {
                            try
                            {
                                var clonedDeviceInformation = deviceInformation.Clone();

                                MassagePresentParameters(deviceInformation.PresentationParameters);
                                ValidateGraphicsDeviceInformation(deviceInformation);

                                _device.Reset(
                                    clonedDeviceInformation.PresentationParameters,
                                    clonedDeviceInformation.Adapter);

                                newDeviceRequired = false;
                            }
                            catch (Exception e)
                            {
                                GameLog.Client.General.Error(e);
                            }
                        }
                    }

                    if (newDeviceRequired)
                        CreateDevice(deviceInformation);

                    _isDeviceDirty = false;
                }
                finally
                {
                    _inDeviceTransition = false;
                }
            }
        }

        // ReSharper disable UnusedMember.Local
        private void CheckForAvailableSupportedHardware()
        {
            var deviceFound = false;
            var deviceIsDirect3D9Compatible = false;

            foreach (var adapter in GraphicsAdapter.Adapters)
            {
                if (!adapter.IsDeviceTypeAvailable(DeviceType.Hardware))
                    continue;

                deviceFound = true;

                var capabilities = adapter.GetCapabilities(DeviceType.Hardware);

                if (capabilities.MaxPixelShaderProfile != ShaderProfile.Unknown &&
                    capabilities.MaxPixelShaderProfile >= ShaderProfile.PS_1_1 &&
                    capabilities.DeviceCapabilities.IsDirect3D9Driver)
                {
                    deviceIsDirect3D9Compatible = true;
                    break;
                }
            }

            if (!deviceFound)
            {
                if (SystemParameters.IsRemoteSession)
                {
                    throw CreateNoSuitableGraphicsDeviceException(
                        "Direct3D is not available when you are using Remote Desktop.",
                        null);
                }
                throw CreateNoSuitableGraphicsDeviceException(
                    "Direct3D hardware acceleration is not available or has been disabled. " +
                    "Verify that a Direct3D enabled graphics device is installed and check " +
                    "the display properties to make sure hardware acceleration is set to Full.",
                    null);
            }

            if (!deviceIsDirect3D9Compatible)
            {
                throw CreateNoSuitableGraphicsDeviceException(
                    "Could not find a Direct3D device that has a Direct3D 9 level driver " +
                    "and supports pixel shader 1.1 or greater.",
                    null);
            }
        }
        // ReSharper restore UnusedMember.Local

        private DepthFormat ChooseDepthStencilFormat(GraphicsAdapter adapter, DeviceType deviceType, SurfaceFormat adapterFormat)
        {
            if (adapter.CheckDeviceFormat(
                deviceType,
                adapterFormat,
                TextureUsage.None,
                QueryUsages.None,
                ResourceType.DepthStencilBuffer,
                PreferredDepthStencilFormat))
            {
                return PreferredDepthStencilFormat;
            }

            DepthFormat depthFormat;

            if (Array.IndexOf(_depthFormatsWithStencil, PreferredDepthStencilFormat) >= 0)
            {
                depthFormat = ChooseDepthStencilFormatFromList(_depthFormatsWithStencil, adapter, deviceType, adapterFormat);

                if (depthFormat != DepthFormat.Unknown)
                    return depthFormat;
            }

            depthFormat = ChooseDepthStencilFormatFromList(_depthFormatsWithoutStencil, adapter, deviceType, adapterFormat);

            if (depthFormat != DepthFormat.Unknown)
                return depthFormat;

            return DepthFormat.Depth24;
        }

        private DepthFormat ChooseDepthStencilFormatFromList(
            DepthFormat[] availableFormats,
            GraphicsAdapter adapter,
            DeviceType deviceType,
            SurfaceFormat adapterFormat)
        {
            foreach (var depthFormat in availableFormats)
            {
                if (depthFormat != PreferredDepthStencilFormat &&
                    adapter.CheckDeviceFormat(
                        deviceType,
                        adapterFormat,
                        TextureUsage.None,
                        QueryUsages.None,
                        ResourceType.DepthStencilBuffer,
                        depthFormat))
                {
                    return depthFormat;
                }
            }
            return DepthFormat.Unknown;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetDesktopWindow();

        public void CreateDevice()
        {
            ChangeDevice(true);
        }

        private void CreateDevice(GraphicsDeviceInformation newInfo)
        {
            if (_device != null)
            {
                _device.Dispose();
                _device = null;
            }

            OnPreparingDeviceSettings(this, new PreparingDeviceSettingsEventArgs(newInfo));
            MassagePresentParameters(newInfo.PresentationParameters);

            try
            {
                ValidateGraphicsDeviceInformation(newInfo);

                var device = new GraphicsDevice(
                    newInfo.Adapter,
                    newInfo.DeviceType,
                    GetDesktopWindow(),
                    newInfo.PresentationParameters);

                _device = device;

                _device.DeviceResetting += HandleDeviceResetting;
                _device.DeviceReset += HandleDeviceReset;
                _device.DeviceLost += delegate {};
                _device.Disposing += HandleDisposing;
            }
            catch (DeviceNotSupportedException exception)
            {
                throw CreateNoSuitableGraphicsDeviceException(
                    "Direct3D 9 is not available. This could be the result of using " +
                    "Remote Desktop or a corrupted DirectX 9 installation.",
                    exception);
            }
            catch (DriverInternalErrorException exception)
            {
                throw CreateNoSuitableGraphicsDeviceException(
                    "The graphics driver returned a low-level error. Please recreate " +
                    "the device or reboot your computer.",
                    exception);
            }
            catch (ArgumentException exception)
            {
                throw CreateNoSuitableGraphicsDeviceException(
                    "The device creation parameters contain invalid configuration options.",
                    exception);
            }
            catch (Exception exception)
            {
                throw CreateNoSuitableGraphicsDeviceException(
                    "Unable to create the graphics device.",
                    exception);
            }

            OnDeviceCreated(this, EventArgs.Empty);
        }

        private Exception CreateNoSuitableGraphicsDeviceException(string message, Exception innerException)
        {
            var exception = new NoSuitableGraphicsDeviceException(message, innerException);

            exception.Data.Add("MinimumPixelShaderProfile", _minimumPixelShaderProfile);
            exception.Data.Add("MinimumVertexShaderProfile", _minimumVertexShaderProfile);

            return exception;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            if (_device != null)
            {
                _device.Dispose();
                _device = null;
            }

            _owner.Services.RemoveService(typeof(IGraphicsDeviceManager));
            _owner.Services.RemoveService(typeof(IGraphicsDeviceService));

            var handler = Disposed;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private bool EnsureDevice()
        {
            if (_device == null)
                return false;

            return EnsureDevicePlatform();
        }

        private bool EnsureDevicePlatform()
        {
            switch (_device.GraphicsDeviceStatus)
            {
                case GraphicsDeviceStatus.Lost:
                    {
                        Thread.Sleep((int)_deviceLostSleepTime.TotalMilliseconds);
                        return false;
                    }

                case GraphicsDeviceStatus.NotReset:
                    {
                        Thread.Sleep((int)_deviceLostSleepTime.TotalMilliseconds);

                        try { ChangeDevice(false); }
                        catch (DeviceLostException) { return false; }
                        catch { ChangeDevice(true); }

                        break;
                    }
            }

            return true;
        }

        protected virtual GraphicsDeviceInformation FindBestDevice()
        {
            return FindBestPlatformDevice();
        }

        private GraphicsDeviceInformation FindBestPlatformDevice()
        {
            var foundDevices = new List<GraphicsDeviceInformation>();

            AddDevices(foundDevices);

            if (PreferMultiSampling &&
                !foundDevices.Any(o => o.DeviceType == DeviceType.Hardware))
            {
                PreferMultiSampling = false;
                AddDevices(foundDevices);
            }

            if (foundDevices.Count == 0)
            {
                throw CreateNoSuitableGraphicsDeviceException(
                    "No compatible graphics device was found.",
                    null);
            }

            RankDevices(foundDevices);

            if (foundDevices.Count == 0)
            {
                throw CreateNoSuitableGraphicsDeviceException(
                    "None of the available graphics meet the necessary requirements.",
                    null);
            }

            return foundDevices[0];
        }

        protected void UpdateClientBounds(Int32Rect clientBounds)
        {
            if (clientBounds == _clientBounds)
                return;

            _clientBounds = clientBounds;

            if (_inDeviceTransition || (_clientBounds.Height == 0 && _clientBounds.Width == 0))
                return;

            _resizedBackBufferWidth = _clientBounds.Width;
            _resizedBackBufferHeight = _clientBounds.Height;
            _useResizedBackBuffer = true;

            ChangeDevice(true);
        }

        private void HandleDeviceReset(object sender, EventArgs e)
        {
            OnDeviceReset(this, EventArgs.Empty);
        }

        private void HandleDeviceResetting(object sender, EventArgs e)
        {
            OnDeviceResetting(this, EventArgs.Empty);
        }

        private void HandleDisposing(object sender, EventArgs e)
        {
            OnDeviceDisposing(this, EventArgs.Empty);
        }

        private static bool IsValidShaderProfile(ShaderProfile capsShaderProfile, ShaderProfile minimumShaderProfile)
        {
            if (capsShaderProfile == ShaderProfile.PS_2_B && minimumShaderProfile == ShaderProfile.PS_2_A)
                return false;

            return (capsShaderProfile >= minimumShaderProfile);
        }

        private static void MassagePresentParameters(PresentationParameters pp)
        {
            if (pp.BackBufferWidth == 0)
                pp.BackBufferWidth = 1;

            if (pp.BackBufferHeight == 0)
                pp.BackBufferHeight = 1;

            pp.IsFullScreen = false;
            pp.FullScreenRefreshRateInHz = 0;
        }

        public bool BeginDraw()
        {
            if (!EnsureDevice())
                return false;

            _beginDrawOk = true;

            return true;
        }

        public void EndDraw()
        {
            if (!_beginDrawOk || _device == null)
                return;

            try { _device.Present(); }
            catch (InvalidOperationException) {}
            catch (DeviceLostException) {}
            catch (DeviceNotResetException) {}
            catch (DriverInternalErrorException) {}
        }

        protected virtual void OnDeviceCreated(object sender, EventArgs args)
        {
            var handler = DeviceCreated;
            if (handler != null)
                handler(sender, args);
        }

        protected virtual void OnDeviceDisposing(object sender, EventArgs args)
        {
            var handler = DeviceDisposing;
            if (handler != null)
                handler(sender, args);
        }

        protected virtual void OnDeviceReset(object sender, EventArgs args)
        {
            var handler = DeviceReset;
            if (handler != null)
                handler(sender, args);
        }

        protected virtual void OnDeviceResetting(object sender, EventArgs args)
        {
            var handler = DeviceResetting;
            if (handler != null)
                handler(sender, args);
        }

        protected virtual void OnPreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs args)
        {
            var handler = PreparingDeviceSettings;
            if (handler != null)
                handler(sender, args);
        }

        protected virtual void RankDevices(List<GraphicsDeviceInformation> foundDevices)
        {
            RankDevicesPlatform(foundDevices);
        }

        private void RankDevicesPlatform(List<GraphicsDeviceInformation> foundDevices)
        {
            var index = 0;

            while (index < foundDevices.Count)
            {
                var deviceType = foundDevices[index].DeviceType;
                var adapter = foundDevices[index].Adapter;
                var presentationParameters = foundDevices[index].PresentationParameters;

                if (!adapter.CheckDeviceFormat(
                    deviceType,
                    adapter.CurrentDisplayMode.Format,
                    TextureUsage.None,
                    QueryUsages.PostPixelShaderBlending,
                    ResourceType.Texture2D,
                    presentationParameters.BackBufferFormat))
                {
                    foundDevices.RemoveAt(index);
                }
                else
                {
                    index++;
                }
            }

            foundDevices.Sort(new GraphicsDeviceInformationComparer(this));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static void ValidateGraphicsDeviceInformation(GraphicsDeviceInformation devInfo)
        {
            var adapter = devInfo.Adapter;
            var deviceType = devInfo.DeviceType;
            var presentationParameters = devInfo.PresentationParameters;
            var deviceFormat = adapter.CurrentDisplayMode.Format;
            var acceptedBackBufferFormat = presentationParameters.BackBufferFormat;

            if (!presentationParameters.IsFullScreen)
            {
                deviceFormat = adapter.CurrentDisplayMode.Format;

                if (SurfaceFormat.Unknown == presentationParameters.BackBufferFormat)
                    acceptedBackBufferFormat = deviceFormat;
            }

            if (Array.IndexOf(ValidBackBufferFormats, acceptedBackBufferFormat) == -1)
            {
                throw new ArgumentException(
                    "BackBufferFormat must be one of the following: " +
                    "Bgr565, Bgr555, Bgra5551, Bgr32, Color, or Bgra1010102.");
            }

            if (!adapter.CheckDeviceType(deviceType, deviceFormat, presentationParameters.BackBufferFormat, presentationParameters.IsFullScreen))
            {
                throw new ArgumentException(
                    "The selected BackBufferFormat and IsFullScreen value are " +
                    "not valid for the selected adapter format and device type.");
            }

            if (presentationParameters.BackBufferCount < 0 || presentationParameters.BackBufferCount > 3)
                throw new ArgumentException("BackBufferCount must be between 0 and 3.");

            if (presentationParameters.BackBufferCount > 1 && presentationParameters.SwapEffect == SwapEffect.Copy)
                throw new ArgumentException("When using SwapEffect.Copy, BackBufferCount must be one.");

            switch (presentationParameters.SwapEffect)
            {
                case SwapEffect.Discard:
                case SwapEffect.Flip:
                case SwapEffect.Copy:
                {
                    int qualityLevels;

                    if (!adapter.CheckDeviceMultiSampleType(
                        deviceType,
                        acceptedBackBufferFormat,
                        presentationParameters.IsFullScreen,
                        presentationParameters.MultiSampleType,
                        out qualityLevels))
                    {
                        throw new ArgumentException(
                            "The selected MultiSampleType is not compatible with the " +
                            "current BackBufferFormat and IsFullScreen value for the " +
                            "selected adapter.");
                    }

                    if (presentationParameters.MultiSampleQuality >= qualityLevels)
                    {
                        throw new ArgumentException(
                            "The selected MultiSampleQualityLevel value is invalid for the " +
                            "selected MultiSampleType.");
                    }

                    if (presentationParameters.MultiSampleType != MultiSampleType.None &&
                        presentationParameters.SwapEffect != SwapEffect.Discard)
                    {
                        throw new ArgumentException(
                            "Must use SwapEffect.Discard when enabling multisampling.");
                    }

                    if ((presentationParameters.PresentOptions & PresentOptions.DiscardDepthStencil) != PresentOptions.None &&
                        !presentationParameters.EnableAutoDepthStencil)
                    {
                        throw new ArgumentException(
                            "When PresentOptions.DiscardDepthStencil is set, " +
                            "EnabledAutoDepthStencil must be true.");
                    }

                    if (presentationParameters.EnableAutoDepthStencil)
                    {
                        if (!adapter.CheckDeviceFormat(
                            deviceType,
                            deviceFormat,
                            TextureUsage.None,
                            QueryUsages.None,
                            ResourceType.DepthStencilBuffer,
                            presentationParameters.AutoDepthStencilFormat))
                        {
                            throw new ArgumentException(
                                "The specified DepthStencilFormat is not supported as " +
                                "a depth/stencil format for the selected adapter.");
                        }

                        if (!adapter.CheckDepthStencilMatch(
                            deviceType,
                            deviceFormat,
                            acceptedBackBufferFormat,
                            presentationParameters.AutoDepthStencilFormat))
                        {
                            throw new ArgumentException(
                                "The specified DepthStencilFormat is not supported as " +
                                "a depth/stencil format when using the selected BackBufferFormat.");
                        }
                    }

                    if (!presentationParameters.IsFullScreen)
                    {
                        switch (presentationParameters.PresentationInterval)
                        {
                            case PresentInterval.Default:
                            case PresentInterval.One:
                            case PresentInterval.Immediate:
                                return;
                        }

                        throw new ArgumentException(
                            "When IsFullScreen is false, PresentationInterval must be " +
                            "one of the following: Default, Immediate, or One.");
                    }

                    break;
                }
                default:
                {
                    throw new ArgumentException(
                        "SwapEffect must be one of the following: SwapEffect.Copy, " +
                        "SwapEffect.Discard, or SwapEffect.Flip.");
                }
            }
        }

        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return _device;
            }
        }

        public ShaderProfile MinimumPixelShaderProfile
        {
            get
            {
                return _minimumPixelShaderProfile;
            }
            set
            {
                if (value < ShaderProfile.PS_1_1 || value > ShaderProfile.XPS_3_0)
                {
                    throw new ArgumentOutOfRangeException(
                        "value",
                        "Value must be a valid pixel shader profile.");
                }
                _minimumPixelShaderProfile = value;
                _isDeviceDirty = true;
            }
        }

        public ShaderProfile MinimumVertexShaderProfile
        {
            get
            {
                return _minimumVertexShaderProfile;
            }
            set
            {
                if (value < ShaderProfile.VS_1_1 || value > ShaderProfile.XVS_3_0)
                {
                    throw new ArgumentOutOfRangeException(
                        "value",
                        "Value must be a valid vertex shader profile.");
                }
                _minimumVertexShaderProfile = value;
                _isDeviceDirty = true;
            }
        }

        public bool PreferMultiSampling
        {
            get
            {
                return _allowMultiSampling;
            }
            set
            {
                _allowMultiSampling = value;
                _isDeviceDirty = true;
            }
        }

        public SurfaceFormat PreferredBackBufferFormat
        {
            get
            {
                return _backBufferFormat;
            }
            set
            {
                if (Array.IndexOf(ValidBackBufferFormats, value) == -1)
                {
                    throw new ArgumentOutOfRangeException(
                        "value",
                        "BackBufferFormat must be one of the following: Bgr565, Bgr555, " +
                        "Bgra5551, Bgr32, Color, or Bgra1010102.");
                }

                _backBufferFormat = value;
                _isDeviceDirty = true;
            }
        }

        public int PreferredBackBufferHeight
        {
            get { return _clientBounds.Height; }
        }

        public int PreferredBackBufferWidth
        {
            get { return _clientBounds.Width; }
        }

        public DepthFormat PreferredDepthStencilFormat
        {
            get
            {
                return _depthStencilFormat;
            }
            set
            {
                switch (value)
                {
                    case DepthFormat.Depth24Stencil8:
                    case DepthFormat.Depth24Stencil8Single:
                    case DepthFormat.Depth24Stencil4:
                    case DepthFormat.Depth24:
                    case DepthFormat.Depth32:
                    case DepthFormat.Depth16:
                    case DepthFormat.Depth15Stencil1:
                        _depthStencilFormat = value;
                        _isDeviceDirty = true;
                        return;
                }
                throw new ArgumentOutOfRangeException(
                    "value",
                    "The specified DepthStencilFormat is not supported as a depth/stencil " +
                    "format for the selected adapter.");
            }
        }

        public bool SynchronizeWithVerticalRetrace
        {
            get
            {
                return _synchronizeWithVerticalRetrace;
            }
            set
            {
                _synchronizeWithVerticalRetrace = value;
                _isDeviceDirty = true;
            }
        }

        private sealed class GraphicsDeviceInformationComparer : IComparer<GraphicsDeviceInformation>
        {
            private readonly GraphicsDeviceManager _graphics;

            public GraphicsDeviceInformationComparer([NotNull] GraphicsDeviceManager graphicsComponent)
            {
                if (graphicsComponent == null)
                    throw new ArgumentNullException("graphicsComponent");

                _graphics = graphicsComponent;
            }

            public int Compare(GraphicsDeviceInformation d1, GraphicsDeviceInformation d2)
            {
                float preferredAspectRatio;

                if (d1.DeviceType != d2.DeviceType)
                {
                    if (d1.DeviceType >= d2.DeviceType)
                        return 1;

                    return -1;
                }

                var parameters1 = d1.PresentationParameters;
                var parameters2 = d2.PresentationParameters;

                var formatRank1 = RankFormat(parameters1.BackBufferFormat);
                var formatRank2 = RankFormat(parameters2.BackBufferFormat);

                if (formatRank1 != formatRank2)
                {
                    if (formatRank1 >= formatRank2)
                        return 1;

                    return -1;
                }

                if (parameters1.MultiSampleType != parameters2.MultiSampleType)
                {
                    var multiSample1 = (parameters1.MultiSampleType == MultiSampleType.NonMaskable)
                                           ? ((int)MultiSampleType.SixteenSamples | (int)MultiSampleType.NonMaskable)
                                           : (int)parameters1.MultiSampleType;

                    var multiSample2 = (parameters2.MultiSampleType == MultiSampleType.NonMaskable)
                                           ? ((int)MultiSampleType.SixteenSamples | (int)MultiSampleType.NonMaskable)
                                           : (int)parameters2.MultiSampleType;

                    if (multiSample1 <= multiSample2)
                        return 1;

                    return -1;
                }

                if (parameters1.MultiSampleQuality != parameters2.MultiSampleQuality)
                {
                    if (parameters1.MultiSampleQuality <= parameters2.MultiSampleQuality)
                        return 1;

                    return -1;
                }

                if (_graphics.PreferredBackBufferWidth == 0 || _graphics.PreferredBackBufferHeight == 0)
                {
                    preferredAspectRatio = ((float)DefaultBackBufferWidth) / DefaultBackBufferHeight;
                }
                else
                {
                    preferredAspectRatio = ((float)_graphics.PreferredBackBufferWidth) / _graphics.PreferredBackBufferHeight;
                }

                var aspectRatio1 = ((float)parameters1.BackBufferWidth) / parameters1.BackBufferHeight;
                var aspectRatio2 = ((float)parameters2.BackBufferWidth) / parameters2.BackBufferHeight;

                var dRatio1 = Math.Abs(aspectRatio1 - preferredAspectRatio);
                var dRaio2 = Math.Abs(aspectRatio2 - preferredAspectRatio);

                if (Math.Abs(dRatio1 - dRaio2) > 0.2f)
                {
                    if (dRatio1 >= dRaio2)
                        return 1;

                    return -1;
                }

                int pixelCount1;
                int pixelCount2;

                if (_graphics.PreferredBackBufferWidth == 0 || _graphics.PreferredBackBufferHeight == 0)
                {
                    pixelCount1 = pixelCount2 = DefaultBackBufferWidth * DefaultBackBufferHeight;
                }
                else
                {
                    pixelCount1 = pixelCount2 = _graphics.PreferredBackBufferWidth * _graphics.PreferredBackBufferHeight;
                }

                var dPixels1 = Math.Abs((parameters1.BackBufferWidth * parameters1.BackBufferHeight) - pixelCount1);
                var dPixels2 = Math.Abs((parameters2.BackBufferWidth * parameters2.BackBufferHeight) - pixelCount2);

                if (dPixels1 != dPixels2)
                {
                    if (dPixels1 >= dPixels2)
                        return 1;
                    return -1;
                }

                if (d1.Adapter != d2.Adapter)
                {
                    if (d1.Adapter.IsDefaultAdapter)
                        return -1;

                    if (d2.Adapter.IsDefaultAdapter)
                        return 1;
                }

                return 0;
            }

            private int RankFormat(SurfaceFormat format)
            {
                var formatIndex = Array.IndexOf(ValidBackBufferFormats, format);

                if (formatIndex != -1)
                {
                    var preferredFormatIndex = Array.IndexOf(ValidBackBufferFormats, _graphics.PreferredBackBufferFormat);

                    if (preferredFormatIndex == -1)
                        return (ValidBackBufferFormats.Length - formatIndex);
                    if (formatIndex >= preferredFormatIndex)
                        return (formatIndex - preferredFormatIndex);
                }

                return 0x7fffffff;
            }
        }

        #region Implementation of IServiceProvider

        object IServiceProvider.GetService(Type serviceType)
        {
            if (serviceType == typeof(IGraphicsDeviceService))
                return this;
            return null;
        }

        #endregion
    }
}