using System;

namespace Supremacy.Utility
{
    class ShipSuffixes
    {
        public static string Alphabetical(int number)
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            string suffix = "";

            if (number >= letters.Length)
                suffix += letters[number/ letters.Length - 1];

            suffix += letters[number % letters.Length];

            return suffix;
        }

        public static string Binary(int number)
        {
            return Convert.ToString(number, 2);
        }
    }
}
