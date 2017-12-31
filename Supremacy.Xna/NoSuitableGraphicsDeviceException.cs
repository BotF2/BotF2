using System;

namespace Supremacy.Xna
{
    public class NoSuitableGraphicsDeviceException : ApplicationException
    {
        public NoSuitableGraphicsDeviceException(string message)
            : base(message) {}

        public NoSuitableGraphicsDeviceException(string message, Exception inner)
            : base(message, inner) {}
    }
}