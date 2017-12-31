using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

using Microsoft.Scripting;

using M = System.Dataflow;

namespace Supremacy.Scripting.Ast
{
    public class SourceSpanConverter : TypeConverter
    {
        private const char Delimiter = '#';

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return base.GetProperties(context, value, attributes);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || (sourceType == typeof(M.SourceSpan)));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((destinationType == typeof(string)) || (destinationType == typeof(M.SourceSpan)));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null)
                return base.ConvertFrom(context, culture, value);

            var stringValue = value as string;
            if (stringValue == null)
            {
                if (value is M.SourceSpan)
                {
                    var span = (M.SourceSpan)value;

                    return new SourceSpan(
                        new SourceLocation(
                            span.Start.Index,
                            span.Start.Line,
                            span.Start.Column),
                        new SourceLocation(
                            span.End.Index,
                            span.End.Line,
                            span.End.Column));
                }

                return base.ConvertFrom(context, culture, value);
            }

            if (stringValue == string.Empty)
                return null;

            var delimitedValues = stringValue.Split(new[] { Delimiter });
            if ((delimitedValues == null) || (delimitedValues.Length != 2))
                throw new FormatException("Malformed SourceSpan");

            var converter = SourceLocationConverter.Instance;
            var start = (SourceLocation)converter.ConvertFromInvariantString(context, delimitedValues[0]);

            return new SourceSpan(start, (SourceLocation)converter.ConvertFromInvariantString(context, delimitedValues[1]));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (!(value is SourceSpan) || 
                ((destinationType != typeof(string)) && (destinationType != typeof(M.SourceSpan))))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }

            var span = (SourceSpan)value;

            if (destinationType == typeof(M.SourceSpan))
            {
                return new M.SourceSpan(
                    new M.SourcePoint(
                        span.Start.Index,
                        span.Start.Line,
                        span.Start.Column),
                    new M.SourcePoint(
                        span.End.Index,
                        span.End.Line,
                        span.End.Column));
            }

            var converter = SourceLocationConverter.Instance;
            
            return (converter.ConvertToInvariantString(span.Start) + '#' + converter.ConvertToInvariantString(span.End));
        }
    }

    public sealed class SourceLocationConverter : TypeConverter
    {
        private static SourceLocationConverter _instance;

        public static SourceLocationConverter Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SourceLocationConverter();
                return _instance;
            }
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return base.GetProperties(context, value, attributes);
        }

        private static readonly Regex SourcePointRegex = new Regex(@"^\(((?<index>[^:,]+)\:)?(?<line>[^:,]+),(?<column>[^:,]+)\)$", RegexOptions.Compiled);

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return (destinationType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null)
                return base.ConvertFrom(context, culture, value);

            var stringValue = value as string;
            if (stringValue == null)
            {
                if (value is M.SourcePoint)
                {
                    var point = (M.SourcePoint)value;

                    return new SourceLocation(
                        point.Index,
                        point.Line,
                        point.Column);
                }

                return base.ConvertFrom(context, culture, value);
            }

            if (stringValue == string.Empty)
                return null;

            culture = CultureInfo.InvariantCulture;

            var match = SourcePointRegex.Match(stringValue);
            if (!match.Success)
                throw new FormatException("Malformed SourceLocation");

            var indexString = match.Groups["index"].Value;
            var lineString = match.Groups["line"].Value;
            var columnString = match.Groups["column"].Value;

            if (indexString != null)
            {
                return new SourceLocation(
                    int.Parse(indexString, culture),
                    int.Parse(lineString, culture),
                    int.Parse(columnString, culture));
            }

            return new SourceLocation(
                -1,
                int.Parse(lineString, culture),
                int.Parse(columnString, culture));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            string stringValue;
            
            if (!(value is SourceLocation) || 
                ((destinationType != typeof(string)) && (destinationType != typeof(M.SourcePoint))))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }

            culture = CultureInfo.InvariantCulture;

            var location = (SourceLocation)value;

            if (destinationType == typeof(M.SourcePoint))
            {
                return new M.SourcePoint(
                    location.Index,
                    location.Line,
                    location.Column);
            }

            if (location.Index != -1)
            {
                stringValue = string.Format(
                    culture,
                    "({2}:{0},{1})",
                    location.Line,
                    location.Column,
                    location.Index);
            }
            else
            {
                stringValue = string.Format(
                    culture,
                    "({0},{1})",
                    location.Line,
                    location.Column);
            }

            stringValue.IndexOf('#');

            return stringValue;
        }
    }
}