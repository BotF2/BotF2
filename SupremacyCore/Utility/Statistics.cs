// Statistics.cs
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
    public static class Statistics
    {
        // fractional error in math formula less than 1.2 * 10 ^ -7.
        // although subject to catastrophic cancellation when z in very close to 0
        public static double Erf(double z)
        {
            double t = 1.0 / (1.0 + 0.5 * Math.Abs(z));

            // use Horner's method
            double ans = 1 - t * Math.Exp(-z * z - 1.26551223 +
                                                t * (1.00002368 +
                                                t * (0.37409196 +
                                                t * (0.09678418 +
                                                t * (-0.18628806 +
                                                t * (0.27886807 +
                                                t * (-1.13520398 +
                                                t * (1.48851587 +
                                                t * (-0.82215223 +
                                                t * 0.17087277)))))))));
            if (z >= 0)
            {
                return ans;
            }
            else
            {
                return -ans;
            }
        }

        // fractional error less than x.xx * 10 ^ -4.
        public static double Erf2(double z)
        {
            double t = 1.0 / (1.0 + 0.47047 * Math.Abs(z));
            double poly = t * (0.3480242 + t * (-0.0958798 + t * 0.7478556));
            double ans = 1.0 - poly * Math.Exp(-z * z);
            if (z >= 0)
            {
                return ans;
            }
            else
            {
                return -ans;
            }
        }

        public static double Phi(double x)
        {
            return Math.Exp(-0.5 * x * x) / Math.Sqrt(2 * Math.PI);
        }

        public static double Phi(double x, double mu, double sigma)
        {
            return Phi((x - mu) / sigma) / sigma;
        }

        // accurate with absolute error less than 8 * 10^-16
        // Reference: http://www.jstatsoft.org/v11/i04/v11i04.pdf
        public static double Phi2(double z)
        {
            if (z > 8.0)
            {
                return 1.0;    // needed for large values of z
            }

            if (z < -8.0)
            {
                return 0.0;    // probably not needed
            }

            double sum = 0.0, term = z;
            for (int i = 3; sum + term != sum; i += 2)
            {
                sum += term;
                term = term * z * z / i;
            }
            return 0.5 + sum * Phi(z);
        }

        // cumulative normal distribution
        public static double CPhi(double z)
        {
            return 0.5 * (1.0 + Erf(z / Math.Sqrt(2.0)));
        }

        // cumulative normal distribution with mean mu and std deviation sigma
        public static double CPhi(double z, double mu, double sigma)
        {
            return CPhi((z - mu) / sigma);
        }
    }
}
