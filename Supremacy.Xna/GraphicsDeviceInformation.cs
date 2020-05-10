using System;

using Microsoft.Xna.Framework.Graphics;

namespace Supremacy.Xna
{
    public class GraphicsDeviceInformation
    {
        private GraphicsAdapter _adapter = GraphicsAdapter.DefaultAdapter;

        public GraphicsDeviceInformation Clone()
        {
            GraphicsDeviceInformation information = new GraphicsDeviceInformation
            {
                PresentationParameters = PresentationParameters.Clone(),
                _adapter = _adapter,
                DeviceType = DeviceType
            };
            return information;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is GraphicsDeviceInformation information))
            {
                return false;
            }

            if (!information._adapter.Equals(_adapter))
            {
                return false;
            }

            if (!information.DeviceType.Equals(DeviceType))
            {
                return false;
            }

            if (information.PresentationParameters.AutoDepthStencilFormat != PresentationParameters.AutoDepthStencilFormat)
            {
                return false;
            }

            if (information.PresentationParameters.BackBufferCount != PresentationParameters.BackBufferCount)
            {
                return false;
            }

            if (information.PresentationParameters.BackBufferFormat != PresentationParameters.BackBufferFormat)
            {
                return false;
            }

            if (information.PresentationParameters.BackBufferHeight != PresentationParameters.BackBufferHeight)
            {
                return false;
            }

            if (information.PresentationParameters.BackBufferWidth != PresentationParameters.BackBufferWidth)
            {
                return false;
            }

            if (information.PresentationParameters.DeviceWindowHandle != PresentationParameters.DeviceWindowHandle)
            {
                return false;
            }

            if (information.PresentationParameters.EnableAutoDepthStencil != PresentationParameters.EnableAutoDepthStencil)
            {
                return false;
            }

            if (information.PresentationParameters.FullScreenRefreshRateInHz != PresentationParameters.FullScreenRefreshRateInHz)
            {
                return false;
            }

            if (information.PresentationParameters.IsFullScreen != PresentationParameters.IsFullScreen)
            {
                return false;
            }

            if (information.PresentationParameters.MultiSampleQuality != PresentationParameters.MultiSampleQuality)
            {
                return false;
            }

            if (information.PresentationParameters.MultiSampleType != PresentationParameters.MultiSampleType)
            {
                return false;
            }

            if (information.PresentationParameters.PresentationInterval != PresentationParameters.PresentationInterval)
            {
                return false;
            }

            if (information.PresentationParameters.PresentOptions != PresentationParameters.PresentOptions)
            {
                return false;
            }

            return information.PresentationParameters.SwapEffect == PresentationParameters.SwapEffect;
        }

        public override int GetHashCode()
        {
            return DeviceType.GetHashCode() ^ _adapter.GetHashCode() ^ PresentationParameters.GetHashCode();
        }

        public GraphicsAdapter Adapter
        {
            get => _adapter;
            set => _adapter = value ?? throw new ArgumentNullException("value");
        }

        public DeviceType DeviceType { get; set; } = DeviceType.Hardware;

        public PresentationParameters PresentationParameters { get; set; } = new PresentationParameters();
    }
}