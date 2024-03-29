﻿using System;
using System.Globalization;

namespace Supremacy.Client.Data
{
    public enum ComparisonType
    {
        Equal,
        NotEqual,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual
    }

    public class ComparisonConverter : BooleanValueConverter
    {
        public ComparisonType ComparisonType { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return FalseValue;
            }

            if (ComparisonType == ComparisonType.Equal)
            {
                return Equals(value, parameter) ? TrueValue : FalseValue;
            }

            if (ComparisonType == ComparisonType.NotEqual)
            {
                return !Equals(value, parameter) ? TrueValue : FalseValue;
            }

            double left = System.Convert.ToDouble(value);
            double right = System.Convert.ToDouble(parameter);

            switch (ComparisonType)
            {
                case ComparisonType.GreaterThan:
                    return left > right ? TrueValue : FalseValue;
                case ComparisonType.LessThan:
                    return left < right ? TrueValue : FalseValue;
                case ComparisonType.GreaterThanOrEqual:
                    return left >= right ? TrueValue : FalseValue;
                case ComparisonType.LessThanOrEqual:
                    return left <= right ? TrueValue : FalseValue;
            }

            return FalseValue;
        }
    }
}