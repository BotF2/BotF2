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

        private object _trueValue = True;
        private object _falseValue = False;

        public object TrueValue
        {
            get { return _trueValue; }
            set { _trueValue = value; }
        }

        public object FalseValue
        {
            get { return _falseValue; }
            set { _falseValue = value; }
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SystemBonus? bonuses = value as SystemBonus?;
            if (!bonuses.HasValue)
                return FalseValue;

            SystemBonus? comparand = parameter as SystemBonus?;
            if (!comparand.HasValue)
            {
                string comparandString = parameter as string;
                if (comparandString == null)
                    return FalseValue;

                comparand = EnumHelper.Parse<SystemBonus>(comparandString);
                if (!comparand.HasValue)
                    return FalseValue;
            }

            if ((bonuses.Value & comparand.Value) == comparand.Value)
                return TrueValue;

            return FalseValue;
        }
    }
}