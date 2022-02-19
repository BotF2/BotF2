using System;
using System.Globalization;
using System.Windows.Data;

using Supremacy.Universe;
using Supremacy.Utility;

namespace Supremacy.Client.Data
{
    [ValueConversion(typeof(SystemBonus), typeof(bool))]
    public class HasSystemBonusConverter : ValueConverter<HasSystemBonusConverter>
    {
        public static readonly object True = true;
        public static readonly object False = false;

        public object TrueValue { get; set; } = True;

        public object FalseValue { get; set; } = False;

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SystemBonus? bonuses = value as SystemBonus?;
            if (!bonuses.HasValue)
            {
                return FalseValue;
            }

            SystemBonus? comparand = parameter as SystemBonus?;
            if (!comparand.HasValue)
            {
                if (!(parameter is string comparandString))
                {
                    return FalseValue;
                }

                comparand = EnumHelper.Parse<SystemBonus>(comparandString);
                if (!comparand.HasValue)
                {
                    return FalseValue;
                }
            }

            if ((bonuses.Value & comparand.Value) == comparand.Value)
            {
                return TrueValue;
            }

            return FalseValue;
        }
    }
}