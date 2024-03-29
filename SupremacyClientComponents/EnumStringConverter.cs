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
                    @"Resources\Data\EnumStrings.txt"));
        }
        #endregion

        #region IValueConverter Members
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            bool parameterIsTypeName = false;

            string typeName = null;
            string valueName = null;

            if (value is Enum enumValue)
            {
                typeName = enumValue.GetType().Name;
                valueName = enumValue.ToString();
            }


            if (typeName == null)
            {
                if (value is string stringValue)
                {
                    try
                    {
                        string[] parts = value.ToString().Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length < 2)
                        {
                            typeName = parameter as string;

                            if (typeName == null)
                            {
                                return value;
                            }

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
            {
                return value;
            }

            if (!EnumTables.TryGetValue(typeName, valueName, 0, out string result))
            {
                return value;
            }

            if (!parameterIsTypeName &&
                result != null &&
                parameter != null &&
                string.Equals(parameter.ToString(), "UpperCase"))
            {
                result = result.ToUpperInvariant();
            }

            if (result != null)
            {
                result = result.Replace("&", AccessText ? "_" : "");
            }

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
                Type enumType = value as Type ?? Type.GetType(value.ToString());
                if (enumType != null)
                {
                    return Enum.GetValues(enumType);
                }
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
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
            _enumTables = GameContext.Current != null
                ? GameContext.Current.Tables.EnumTables
                : Designer.IsInDesignMode
                    ? TableMap.ReadFromFile(
                                    Path.Combine(
                                        PathHelper.GetWorkingDirectory(),
                                        @"Resources\Data\EnumStrings.txt"))
                    : TableMap.ReadFromFile(
                                    Path.Combine(
                                        Environment.CurrentDirectory,
                                        @"Resources\Data\EnumStrings.txt"));
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
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
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