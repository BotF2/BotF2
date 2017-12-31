using System;

using Supremacy.Annotations;

namespace Supremacy.Xna
{
    public class PreparingDeviceSettingsEventArgs : EventArgs
    {
        private readonly GraphicsDeviceInformation _graphicsDeviceInformation;

        public PreparingDeviceSettingsEventArgs([NotNull] GraphicsDeviceInformation graphicsDeviceInformation)
        {
            if (graphicsDeviceInformation == null)
                throw new ArgumentNullException("graphicsDeviceInformation");

            _graphicsDeviceInformation = graphicsDeviceInformation;
        }

        public GraphicsDeviceInformation GraphicsDeviceInformation
        {
            get { return _graphicsDeviceInformation; }
        }
    }
}