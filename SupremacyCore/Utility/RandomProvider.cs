// RandomProvider.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Annotations;

namespace Supremacy.Utility
{
    public static class RandomProvider
    {
        [ThreadStatic]
        private static Random _random;

        public static Random Shared
        {
            get
            {
                if (_random == null)
                    _random = new MersenneTwister();
                
                return _random;
            }
        }

        public static object NextEnum(Type enumType)
        {
            var values = Enum.GetValues(enumType);
            return values.GetValue(Shared.Next(values.Length));
        }

        public static T NextEnum<T>() where T : struct 
        {
            var values = EnumUtilities.GetValues<T>();
            return values[Shared.Next(values.Count)];
        }

        public static T NextEnum<T>([NotNull] this Random random) where T : struct 
        {
            if (random == null)
                throw new ArgumentNullException("random");

            var values = EnumUtilities.GetValues<T>();
            return values[random.Next(values.Count)];
        }
    }
}
