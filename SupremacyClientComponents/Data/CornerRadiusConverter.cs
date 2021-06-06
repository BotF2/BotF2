using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

using Supremacy.Client.Controls;

namespace Supremacy.Client.Data
{
    [ValueConversion(typeof(double), typeof(CornerRadius))]
    [ValueConversion(typeof(CornerRadius), typeof(CornerRadius))]
    public sealed class CornerRadiusConverter : ValueConverter<CornerRadiusConverter>
    {
        public Sides Sides { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double? doubleValue = value as double?;
            if (doubleValue.HasValue)
                value = new CornerRadius(doubleValue.Value);

            CornerRadius? cornerRadius = value as CornerRadius?;
            if (!cornerRadius.HasValue)
                return base.Convert(value, targetType, parameter, culture);

            return new CornerRadius
            {
                TopLeft = ((Sides & Sides.Left) == Sides.Left) && ((Sides & Sides.Top) == Sides.Top) ? cornerRadius.Value.BottomLeft : 0d,
                TopRight = ((Sides & Sides.Right) == Sides.Right) && ((Sides & Sides.Top) == Sides.Top) ? cornerRadius.Value.TopRight : 0d,
                BottomRight = ((Sides & Sides.Right) == Sides.Right) && ((Sides & Sides.Bottom) == Sides.Bottom) ? cornerRadius.Value.BottomRight : 0d,
                BottomLeft = ((Sides & Sides.Left) == Sides.Left) && ((Sides & Sides.Bottom) == Sides.Bottom) ? cornerRadius.Value.BottomLeft : 0d,
            };
        }
    }
}