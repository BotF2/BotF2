using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Supremacy.Utility
{
    public static class Shuffle
    {
        public static void ShuffleInPlace<T>(this IList<T> source)
        {
            System.Random rng = new System.Random();
            if (source == null) throw new ArgumentNullException("source");

            for (int i = 0; i < source.Count - 1; i++)
            {
                int j = rng.Next(i, source.Count);

                T temp = source[j];
                source[j] = source[i];
                source[i] = temp;
            }
        }
    }
}
