using System;

using Supremacy.Annotations;

namespace Supremacy.Xna
{
    public sealed class CustomPresentEventArgs : EventArgs
    {
        private readonly XnaTime _time;

        public CustomPresentEventArgs([NotNull] XnaTime time)
        {
            if (time == null)
                throw new ArgumentNullException("time");
            _time = time;
        }

        public XnaTime Time
        {
            get { return _time; }
        }

        public bool Handled { get; set; }
    }
}