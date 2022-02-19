// RandomProvider.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

namespace Supremacy.Utility
{
    public static class RandomProvider
    {
        [ThreadStatic]
        private static Random _random;

        /// <summary>
        /// A shared <see cref="Random"/> for RNG utilities to use
        /// </summary>
        public static Random Shared
        {
            get
            {
                if (_random == null)
                {
                    _random = new MersenneTwister();
                }

                return _random;
            }
        }
    }
}
