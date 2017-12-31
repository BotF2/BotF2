using System;
using System.Windows;

namespace Supremacy.Xna
{
    [Serializable]
    public class TargetSizeChangedEventArgs : EventArgs
    {
        private readonly Int32Rect _oldSize;
        private readonly Int32Rect _newSize;

        public TargetSizeChangedEventArgs(Int32Rect oldSize, Int32Rect newSize)
        {
            _oldSize = oldSize;
            _newSize = newSize;
        }

        public Int32Rect NewSize
        {
            get { return _newSize; }
        }

        public Int32Rect OldSize
        {
            get { return _oldSize; }
        }
    }
}