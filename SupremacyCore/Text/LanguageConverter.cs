using Supremacy.Utility;
using System;
using System.ComponentModel;
using System.Globalization;

namespace Supremacy.Text
{
    public sealed class LanguageConverter : TypeConverter
    {
        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(CultureInfo.GetCultures(CultureTypes.NeutralCultures));
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return (destinationType == typeof(string));
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string stringValue = value as string;

            if (string.IsNullOrWhiteSpace(stringValue))
                return null;

            // ReSharper disable AssignNullToNotNullAttribute
            try
            {
                return CultureInfo.GetCultureInfo(stringValue);
            }
            catch (Exception e)
            {
                GameLog.Core.General.Error(e);
            }

            return null;
            // ReSharper restore AssignNullToNotNullAttribute
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            CultureInfo language = value as CultureInfo;
            if (language == null)
                return null;

            return language.Name;
        }
    }
}