// Dimension.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Runtime.InteropServices;

namespace Supremacy.Types
{
    [Serializable]
    [StructLayout(LayoutKind.Auto)]
    public struct Dimension
    {
        private int width;
        private int height;

        public int Width
        {
            get { return width; }
            set { width = Math.Max(0, value); }
        }

        public int Height
        {
            get { return height; }
            set { height = Math.Max(0, value); }
        }

        public Dimension(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public override int GetHashCode()
        {
            return ((width << 8) | height);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            Dimension other = (Dimension)obj;
            return ((other.width == width)
                && (other.height == height));
        }

        public static bool operator ==(Dimension a, Dimension b)
        {
            return ((a.width == b.width) && (a.height == b.height));
        }

        public static bool operator !=(Dimension a, Dimension b)
        {
            return !(a == b);
        }
    }
}
