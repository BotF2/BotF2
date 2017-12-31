using System;
using System.ComponentModel;
using System.Globalization;

namespace Supremacy.Client.Controls
{
    [TypeConverter(typeof(VariantSizeConverter))]
    public enum VariantSize
    {
        Collapsed = 0,
        Small,
        Medium,
        Large,
    }

    public class VariantSizeConverter : EnumConverter {

        public VariantSizeConverter()
            : base(typeof(VariantSize)) {}

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var variantSize = value as VariantSize?;
            if (!variantSize.HasValue)
                return 0d;

            switch (variantSize.Value)
            {
                case VariantSize.Collapsed:
                    return 0d;

                case VariantSize.Large:
                    return 24d;

                default:
                case VariantSize.Small:
                case VariantSize.Medium:
                    return 16d;
            }
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(double))
                return true;

            return base.CanConvertTo(context, destinationType);
        }
    }
}