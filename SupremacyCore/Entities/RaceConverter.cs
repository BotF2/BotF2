using System;
using System.ComponentModel;
using System.Globalization;

using Supremacy.Game;
using Supremacy.Utility;

namespace Supremacy.Entities
{
    public sealed class RaceConverter : TypeConverter
    {
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
            var stringValue = value as string;

            if (string.IsNullOrWhiteSpace(stringValue))
                return null;

            // ReSharper disable AssignNullToNotNullAttribute
            try
            {
                return GameContext.Current.Races[stringValue];
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
            var race = value as Race;
            if (race == null)
                return null;
            return race.Key;
        }
    }
}