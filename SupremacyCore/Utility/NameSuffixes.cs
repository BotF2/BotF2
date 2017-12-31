namespace Supremacy.Utility
{
    class NameSuffixes
    {
        public static string GetFromNumber(int number)
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var suffix = "";

            if (number >= letters.Length)
                suffix += letters[number/ letters.Length - 1];

            suffix += letters[number % letters.Length];

            return suffix;
        }
    }
}
