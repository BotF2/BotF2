using System;
using System.Windows;

namespace Supremacy.Xna
{
    [Serializable]
    public class TargetSizeChangedEventArgs : EventArgs
    {
        public TargetSizeChangedEventArgs(Int32Rect oldSize, Int32Rect newSize)
        {
            OldSize = oldSize;
            NewSize = newSize;
        }

        public Int32Rect NewSize { get; }

        public Int32Rect OldSize { get; }
    }
}