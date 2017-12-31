// RomanNumber.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Text;

namespace Supremacy.Utility
{
    public static class RomanNumber
    {
        private const string numerals = "IVXLCDM??";

        public static string Get(int number)
        {
            if (number < 1)
                throw new ArgumentOutOfRangeException("number", "number must be a positive integer");
            int e = 3;
            uint mod = 1000;
            StringBuilder sb = new StringBuilder();
            for (; 0 <= e; e--, mod /= 10)
            {
                uint current = ((uint)number / mod) % 10;
                if ((current % 5) == 4)
                {
                    sb.Append(numerals[e << 1]);
                    ++current;
                    if (current == 10)
                    {
                        sb.Append(numerals[(e << 1) + 2]);
                        continue;
                    }
                }
                if (current >= 5)
                {
                    sb.Append(numerals[(e << 1) + 1]);
                    current -= 5;
                }
                while (current > 0)
                {
                    sb.Append(numerals[e << 1]);
                    --current;
                }
            }
            return sb.ToString();
        }
    }
}
