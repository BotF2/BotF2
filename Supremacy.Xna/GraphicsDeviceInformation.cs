using System;

using Microsoft.Xna.Framework.Graphics;

namespace Supremacy.Xna
{
    public class GraphicsDeviceInformation
    {
        private GraphicsAdapter _adapter = GraphicsAdapter.DefaultAdapter;
        private DeviceType _deviceType = DeviceType.Hardware;
        private PresentationParameters _presentationParameters = new PresentationParameters();

        public GraphicsDeviceInformation Clone()
        {
            var information = new GraphicsDeviceInformation
                              {
                                  _presentationParameters = _presentationParameters.Clone(),
                                  _adapter = _adapter,
                                  _deviceType = _deviceType
                              };
            return information;
        }

        public override bool Equals(object obj)
        {
            var information = obj as GraphicsDeviceInformation;
            if (information == null)
                return false;

            if (!information._adapter.Equals(_adapter))
                return false;

            if (!information._deviceType.Equals(_deviceType))
                return false;

            if (information.PresentationParameters.AutoDepthStencilFormat != PresentationParameters.AutoDepthStencilFormat)
                return false;

            if (information.PresentationParameters.BackBufferCount != PresentationParameters.BackBufferCount)
                return false;

            if (information.PresentationParameters.BackBufferFormat != PresentationParameters.BackBufferFormat)
                return false;

            if (information.PresentationParameters.BackBufferHeight != PresentationParameters.BackBufferHeight)
                return false;

            if (information.PresentationParameters.BackBufferWidth != PresentationParameters.BackBufferWidth)
                return false;

            if (information.PresentationParameters.DeviceWindowHandle != PresentationParameters.DeviceWindowHandle)
                return false;

            if (information.PresentationParameters.EnableAutoDepthStencil != PresentationParameters.EnableAutoDepthStencil)
                return false;

            if (information.PresentationParameters.FullScreenRefreshRateInHz != PresentationParameters.FullScreenRefreshRateInHz)
                return false;

            if (information.PresentationParameters.IsFullScreen != PresentationParameters.IsFullScreen)
                return false;

            if (information.PresentationParameters.MultiSampleQuality != PresentationParameters.MultiSampleQuality)
                return false;

            if (information.PresentationParameters.MultiSampleType != PresentationParameters.MultiSampleType)
                return false;

            if (information.PresentationParameters.PresentationInterval != PresentationParameters.PresentationInterval)
                return false;

            if (information.PresentationParameters.PresentOptions != PresentationParameters.PresentOptions)
                return false;

            if (information.PresentationParameters.SwapEffect != PresentationParameters.SwapEffect)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return ((_deviceType.GetHashCode() ^ _adapter.GetHashCode()) ^ _presentationParameters.GetHashCode());
        }

        public GraphicsAdapter Adapter
        {
            get { return _adapter; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _adapter = value;
            }
        }

        public DeviceType DeviceType
        {
            get { return _deviceType; }
            set { _deviceType = value; }
        }

        public PresentationParameters PresentationParameters
        {
            get { return _presentationParameters; }
            set { _presentationParameters = value; }
        }
    }
}