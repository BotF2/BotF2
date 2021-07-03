using System;

using Supremacy.Client.Controls;

namespace Supremacy.Client.Data
{
    public static class MathHelper
    {
        public static double Range(double value, double minValue, double maxValue)
        {
            return Math.Min(maxValue, Math.Max(minValue, value));
        }

        public static int Range(int value, int minValue, int maxValue)
        {
            return Math.Min(maxValue, Math.Max(minValue, value));
        }

        public static double Round(RoundMode mode, double value)
        {
            switch (mode)
            {
                case RoundMode.Ceiling:
                case RoundMode.CeilingToEven:
                case RoundMode.CeilingToOdd:

                    value = Math.Ceiling(value);

                    if ((mode == RoundMode.CeilingToEven) && (value % 2 == 1))
                    {
                        value++;
                    }

                    if ((mode == RoundMode.CeilingToOdd) && (value % 2 == 0))
                    {
                        value++;
                    }

                    break;
                case RoundMode.Floor:
                case RoundMode.FloorToEven:
                case RoundMode.FloorToOdd:

                    value = Math.Floor(value);

                    if ((mode == RoundMode.FloorToEven) && (value % 2 == 1))
                    {
                        value--;
                    }

                    if ((mode == RoundMode.FloorToOdd) && (value % 2 == 0))
                    {
                        value--;
                    }

                    break;
                case RoundMode.Round:

                    value = Math.Round(value);
                    break;
                case RoundMode.RoundToEven:
                    {
                        double roundedValue = Math.Round(value);
                        if (roundedValue % 2 == 0)
                        {
                            value = roundedValue;
                        }
                        else if (value == roundedValue)
                        {
                            value++;
                        }
                        else
                        {
                            value = roundedValue + (roundedValue < value ? 1 : -1);
                        }
                        break;
                    }
                case RoundMode.RoundToOdd:
                    {
                        double roundedValue = Math.Round(value);
                        if (roundedValue % 2 == 1)
                        {
                            value = roundedValue;
                        }
                        else if (value == roundedValue)
                        {
                            value++;
                        }
                        else
                        {
                            value = roundedValue + (roundedValue < value ? 1 : -1);
                        }
                        break;
                    }
            }
            return value;
        }
    }
}