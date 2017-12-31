// DoubleExtensions.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

namespace Supremacy.Types
{
    public static class DoubleExtensions
    {
        public static bool IsVeryCloseTo(this double source, double other)
        { 
            if (source == other)
            {
                return true;
            }
            double num = source - other;
            if (num < 1.53E-06)
            {
                return (num > -1.53E-06);
            }
            return false;
        }
    }
}
