// EnumStringConverter.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

using Supremacy.Client.Data;
using Supremacy.Data;
using Supremacy.Game;
using Supremacy.Resources;
using Supremacy.Utility;

namespace Supremacy.Client
{
    [ValueConversion(typeof(string), typeof(string))]
    public class EnumStringConverter : ValueConverter<EnumStringConverter>
    {
        #region Fields
        private static readonly TableMap EnumTables;
        #endregion

        #region Properties
        public bool AccessText { get; set; }
        #endregion

        #region Constructors
        static EnumStringConverter()
        {
            EnumTables = TableMap.ReadFromFile(
                Path.Combine(
                    PathHelper.GetWorkingDirectory(),
                    @"Resources\Tables\EnumStrings.txt"));
        }
        #endregion

        #region IValueConverter Members
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var enumValue = value as Enum;
            var parameterIsTypeName = false;

            var typeName = (string)null;
            var valueName = (string)null;

            if (enumValue != null)
            {
                typeName = enumValue.GetType().Name;
                valueName = enumValue.ToString();
            }

            string result;

            if (typeName == null)
            {
                var stringValue = value as string;
                if (stringValue != null)
                {
                    try
                    {
                        var parts = value.ToString().Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length < 2)
                        {
                            typeName = parameter as string;

                            if (typeName == null)
                                return value;

                            parameterIsTypeName = true;
                        }
                        else
                        {
                            typeName = parts[parts.Length - 2];
                            valueName = parts[parts.Length - 1];
                        }
                    }
                    catch
                    {
                        typeName = null;
                        valueName = null;
                    }
                }
            }

            if (typeName == null)
                return value;

            if (!EnumTables.TryGetValue(typeName, valueName, 0, out result))
                return value;

            if (!parameterIsTypeName &&
                result != null &&
                parameter != null &&
                string.Equals(parameter.ToString(), "UpperCase"))
            {
                result = result.ToUpperInvariant();
            }

            if (result != null)
                result = result.Replace("&", AccessText ? "_" : "");

            return result;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
        #endregion
    }

    [ValueConversion(typeof(string), typeof(Array))]
    public class EnumValueCollectionConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var enumType = value as Type ?? Type.GetType(value.ToString());
                if (enumType != null)
                    return Enum.GetValues(enumType);
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
        #endregion
    }

    [ValueConversion(typeof(string), typeof(string))]
    public class AltEnumStringConverter : IValueConverter
    {
        #region Fields
        private readonly TableMap _enumTables;
        #endregion

        #region Constructors
        public AltEnumStringConverter()
        {
            if (GameContext.Current != null)
            {
                _enumTables = GameContext.Current.Tables.EnumTables;
            }
            else if (Designer.IsInDesignMode)
            {
                _enumTables = TableMap.ReadFromFile(
                    Path.Combine(
                        PathHelper.GetWorkingDirectory(),
                        @"Resources\Tables\EnumStrings.txt"));
            }
            else
            {
                _enumTables = TableMap.ReadFromFile(
                    Path.Combine(
                        Environment.CurrentDirectory,
                        @"Resources\Tables\EnumStrings.txt"));
            }
        }
        #endregion

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object result = value;
            try
            {
                string memberName = value.ToString();
                string typeName = parameter.ToString();
                if ((memberName != null) && (typeName != null))
                {
                    result = _enumTables[typeName][memberName][0];
                }
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
        #endregion
    }
}