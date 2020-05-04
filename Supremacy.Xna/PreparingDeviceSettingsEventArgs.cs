using System;

using Supremacy.Annotations;

namespace Supremacy.Xna
{
    public class PreparingDeviceSettingsEventArgs : EventArgs
    {
        public PreparingDeviceSettingsEventArgs([NotNull] GraphicsDeviceInformation graphicsDeviceInformation)
        {
            GraphicsDeviceInformation = graphicsDeviceInformation ?? throw new ArgumentNullException("graphicsDeviceInformation");
        }

        public GraphicsDeviceInformation GraphicsDeviceInformation { get; }
    }
}