using System;
using System.ComponentModel;
using System.Globalization;

using log4net;
using log4net.Core;
using Supremacy.Utility;

namespace Supremacy.Client
{
    public sealed class LogLevelConverter : TypeConverter
    {
        private static LogLevelConverter _instance;

        public static LogLevelConverter Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LogLevelConverter();
                return _instance;
            }
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(LogManager.GetRepository().LevelMap.AllLevels);
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
            var stringValue = value as string;

            if (string.IsNullOrWhiteSpace(stringValue))
                return null;

            try
            {
                return LogManager.GetRepository().LevelMap[stringValue];
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
            }

            return null;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var level = value as Level;
            if (level == null)
                return null;

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Level.Error.Name.ToLower());
        }
    }
}
