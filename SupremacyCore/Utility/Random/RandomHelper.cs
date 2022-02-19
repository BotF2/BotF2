using System;

namespace Supremacy.Utility
{
    public static class RandomHelper
    {
        /// <summary>
        /// Random number with standard Gaussian distribution
        /// </summary>
        /// <returns></returns>
        public static double Gaussian()
        {
            double U = RandomProvider.Shared.NextDouble();
            double V = RandomProvider.Shared.NextDouble();
            return Math.Sin(2 * Math.PI * V) * Math.Sqrt(-2 * Math.Log(1 - U));
        }

        /// <summary>
        /// Random integer between 0 and N-1
        /// </summary>
        /// <param name="N"></param>
        /// <returns></returns>
        public static int Random(int N)
        {
            return (int)(RandomProvider.Shared.NextDouble() * N);
        }

        /// <summary>
        /// Random number with Gaussian distribution of mean mu and stddev sigma
        /// </summary>
        /// <param name="mu"></param>
        /// <param name="sigma"></param>
        /// <returns></returns>
        public static double Gaussian(double mu, double sigma)
        {
            return mu + sigma * Gaussian();
        }

        /// <summary>
        /// Roll a dice with numSides, and return the number
        /// that was rolled
        /// </summary>
        /// <param name="numSides"></param>
        /// <returns></returns>
        public static int Roll(int numSides)
        {
            return RandomProvider.Shared.Next(numSides) + 1;
        }

        /// <summary>
        /// Roll a dice with numSides, and return whether that number
        /// was equal to numSides
        /// For example, 1 is a 100% chance, 2 is 50%, 10 is 10%, 100 is 1%
        /// </summary>
        /// <param name="numSides"></param>
        /// <returns></returns>
        public static bool Chance(int numSides)
        {
            return Roll(numSides) == numSides;
        }
    }
}
