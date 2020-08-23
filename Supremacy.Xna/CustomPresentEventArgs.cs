using System;

using Supremacy.Annotations;

namespace Supremacy.Xna
{
    public sealed class CustomPresentEventArgs : EventArgs
    {
        public CustomPresentEventArgs([NotNull] XnaTime time)
        {
            Time = time ?? throw new ArgumentNullException("time");
        }

        public XnaTime Time { get; }

        public bool Handled { get; set; }
    }
}