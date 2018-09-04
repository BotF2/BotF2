// ValueConversionHelper.cs
// 
// Copyright (c) 2011 
// 
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
// 
// All other rights reserved.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

using System.IO.Packaging;

using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

using System.Windows.Navigation;

using Supremacy.Scripting.Utility;

namespace Supremacy.Client.Data
{
    public static class ValueConversionHelper
    {
        private static readonly ValueConverterTable _converterTable = new ValueConverterTable();

        /// <summary>
        /// Attempts to convert a value to the specified target type.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">An optional parameter to pass to the value converter.</param>
        /// <param name="culture">An optional culture (defaults to Invariant).</param>
        /// <returns>The converted value, or <see cref="DependencyProperty.UnsetValue"/> if the conversion failed.</returns>
        public static object Convert(object value, Type targetType, object parameter = null, CultureInfo culture = null)
        {
            if (IsNullValue(value))
                return NullValueForType(targetType);

            // If direct assignment is possible, the value is valid.
            var sourceType = value.GetType();
            if (targetType.IsAssignableFrom(sourceType))
                return value;

            if (culture == null)
                culture = CultureInfo.InvariantCulture;

            var converter = _converterTable[sourceType, targetType, false];
            if (converter == null)
            {
                converter = DefaultValueConverter.Create(sourceType, targetType, false);

                if (converter == null)
                    return DependencyProperty.UnsetValue;
                
                if (converter == DefaultValueConverter.ValueConverterNotNeeded)
                    return value;

                _converterTable.Add(sourceType, targetType, false, converter);
            }

            var result = converter.Convert(value, targetType, parameter, culture);

            if (!CoerceValidValue(ref result, targetType) || result == Binding.DoNothing || result == DependencyProperty.UnsetValue)
                return DependencyProperty.UnsetValue;

            return result;
        }

        #region Helper Methods

        private static bool CoerceValidValue(ref object value, Type targetType)
        {
            // Null values are valid for reference and Nullable types.
            if (IsNullValue(value))
            {
                if (!targetType.IsValueType || targetType.IsNullableType())
                {
                    value = NullValueForType(targetType);
                    return true;
                }
                return false;
            }

            // If direct assignment is possible, the value is valid.
            if (targetType.IsAssignableFrom(value.GetType()))
                return true;

            // Otherwise the value is invalid.
            return false;
        }

        private static bool IsNullValue(object value)
        {
            if (value == null)
                return true;

            if (System.Convert.IsDBNull(value))
                return true;

            return false;
        }

        private static object NullValueForType(Type type)
        {
            if (type == null)
                return null;

            if (!type.IsValueType)
                return null;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return null;

            return DependencyProperty.UnsetValue;
        }

        #endregion

        #region ValueConverterContext

        private sealed class ValueConverterContext : ITypeDescriptorContext, IUriContext
        {
            private static readonly Uri _baseUri = PackUriHelper.Create(new Uri("application://"));

            private DependencyObject _targetElement;
            private int _nestingLevel;
            private Uri _cachedBaseUri;

            public Uri BaseUri
            {
                get
                {
                    if (_cachedBaseUri == null)
                        _cachedBaseUri = _targetElement == null ? _baseUri : BaseUriHelper.GetBaseUri(_targetElement);
                    return _cachedBaseUri;
                }
                set { throw new NotSupportedException(); }
            }

            public IContainer Container
            {
                get { return null; }
            }

            public object Instance
            {
                get { return null; }
            }

            public PropertyDescriptor PropertyDescriptor
            {
                get { return null; }
            }

            public object GetService(Type serviceType)
            {
                return serviceType == typeof(IUriContext) ? this : null;
            }

            internal void SetTargetElement(DependencyObject target)
            {
                if (target != null)
                    ++_nestingLevel;
                else if (_nestingLevel > 0)
                    --_nestingLevel;

                Debug.Assert(_nestingLevel <= 1, "illegal to recurse/reenter ValueConverterContext.SetTargetElement()");

                _targetElement = target;
                _cachedBaseUri = null;
            }

            public void OnComponentChanged() {}

            public bool OnComponentChanging()
            {
                return false;
            }
        }

        #endregion

        #region DefaultValueConverter

        private class DefaultValueConverter
        {
            internal static readonly IValueConverter ValueConverterNotNeeded = new ObjectTargetConverter(typeof(object));

            protected readonly Type SourceType;
            protected readonly Type TargetType;

            private TypeConverter _typeConverter;
            private readonly bool _shouldConvertFrom;
            private readonly bool _shouldConvertTo;

            private static readonly Type StringType = typeof(String);

            protected DefaultValueConverter(
                TypeConverter typeConverter,
                Type sourceType,
                Type targetType,
                bool shouldConvertFrom,
                bool shouldConvertTo)
            {
                _typeConverter = typeConverter;
                _shouldConvertFrom = shouldConvertFrom;
                _shouldConvertTo = shouldConvertTo;

                SourceType = sourceType;
                TargetType = targetType;
            }

            /// <summary>
            /// Returns a ValueConverter suitable for converting between <paramref name="sourceType"/> and
            /// <paramref name="targetType"/>.
            /// </summary>
            /// <param name="sourceType">The source type.</param>
            /// <param name="targetType">The target type.</param>
            /// <param name="targetToSource">Indicates whether conversions are actually needed.</param>
            /// <param name="?"></param>
            /// <returns>
            /// A default value converter, <see cref="ValueConverterNotNeeded"/> if no converter is needed, or
            /// <c>null</c> if a converter could not be created.
            /// </returns>
            internal static IValueConverter Create(
                Type sourceType,
                Type targetType,
                bool targetToSource)
            {
                var sourceIsNullable = false;
                var targetIsNullable = false;

                // Sometimes no conversion is necessary.
                if (sourceType == targetType || (!targetToSource && targetType.IsAssignableFrom(sourceType)))
                    return ValueConverterNotNeeded;

                // The type converter for System.Object is useless.  It claims it can convert from string,
                // but then throws an exception when asked to do so.  We work around it.
                if (targetType == typeof(object))
                    return new ObjectTargetConverter(sourceType);

                if (sourceType == typeof(object))
                    return new ObjectSourceConverter(targetType);

                // Use System.Convert for well-known base types 
                if (SystemConvertConverter.CanConvert(sourceType, targetType))
                    return new SystemConvertConverter(sourceType, targetType);

                // Need to check for nullable types first, since NullableConverter is a bit over-eager;
                // TypeConverter for Nullable can convert, e.g., Nullable<DateTime> to string, but it ends
                // up doing a different conversion than the TypeConverter for the generic's inner type.
                var underlyingSourceType = Nullable.GetUnderlyingType(sourceType);
                if (underlyingSourceType != null)
                {
                    sourceType = underlyingSourceType;
                    sourceIsNullable = true;
                }

                var underlyingTargetType = Nullable.GetUnderlyingType(targetType);
                if (underlyingTargetType != null)
                {
                    targetType = underlyingTargetType;
                    targetIsNullable = true;
                }

                // Recursive call to try to find a converter for basic value types.
                if (sourceIsNullable || targetIsNullable)
                    return Create(sourceType, targetType, targetToSource);

                // Special case for converting IListSource to IList.
                if (typeof(IListSource).IsAssignableFrom(sourceType) &&
                    targetType.IsAssignableFrom(typeof(IList)))
                {
                    return new ListSourceConverter();
                }

                // Interfaces are best handled on a per-instance basis.  The type may
                // not implement the interface, but an instance of a derived type may. 
                if (sourceType.IsInterface || targetType.IsInterface)
                    return new InterfaceConverter(sourceType, targetType);

                // Try using the source's type converter.
                var typeConverter = GetConverter(sourceType);
                var canConvertTo = typeConverter != null && typeConverter.CanConvertTo(targetType);
                var canConvertFrom = typeConverter != null && typeConverter.CanConvertFrom(targetType);

                if ((canConvertTo || targetType.IsAssignableFrom(sourceType)) &&
                    (!targetToSource || canConvertFrom || sourceType.IsAssignableFrom(targetType)))
                {
                    return new SourceDefaultValueConverter(
                        typeConverter,
                        sourceType,
                        targetType,
                        targetToSource && canConvertFrom,
                        canConvertTo);
                }

                // If that doesn't work, try using the target's type converter .
                typeConverter = GetConverter(targetType);
                canConvertTo = (typeConverter != null) && typeConverter.CanConvertTo(sourceType);
                canConvertFrom = (typeConverter != null) && typeConverter.CanConvertFrom(sourceType);

                if ((canConvertFrom || targetType.IsAssignableFrom(sourceType)) &&
                    (!targetToSource || canConvertTo || sourceType.IsAssignableFrom(targetType)))
                {
                    return new TargetDefaultValueConverter(
                        typeConverter,
                        sourceType,
                        targetType,
                        canConvertFrom,
                        targetToSource && canConvertTo);
                }

                // Nothing worked; give up.
                return null;
            }

            private static TypeConverter GetConverter(Type type)
            {
                TypeConverter typeConverter = null;

                var knownType = XamlReader.GetWpfSchemaContext().GetXamlType(type);
                
                if (knownType != null && knownType.TypeConverter != null)
                    typeConverter = knownType.TypeConverter.ConverterInstance;

                if (typeConverter == null)
                    typeConverter = TypeDescriptor.GetConverter(type);

                return typeConverter;
            }

            internal static object TryParse(object o, Type targetType, CultureInfo culture)
            {
                // Some types have Parse methods that are more successful than their type converters
                // at converting strings.

                var result = DependencyProperty.UnsetValue;

                var stringValue = o as string;
                if (stringValue != null)
                {
                    try
                    {
                        MethodInfo mi;

                        if (culture != null && (mi = targetType.GetMethod("Parse",
                                                BindingFlags.Public | BindingFlags.Static,
                                                null,
                                                new[] { StringType, typeof(NumberStyles), typeof(IFormatProvider) },
                                                null))
                                    != null)
                        {
                            result = mi.Invoke(null, new object[] { stringValue, NumberStyles.Any, culture });
                        }
                        else if (culture != null && (mi = targetType.GetMethod("Parse",
                                                BindingFlags.Public | BindingFlags.Static,
                                                null,
                                                new[] { StringType, typeof(IFormatProvider) },
                                                null))
                                    != null)
                        {
                            result = mi.Invoke(null, new object[] { stringValue, culture });
                        }
                        else if ((mi = targetType.GetMethod("Parse",
                                                BindingFlags.Public | BindingFlags.Static,
                                                null,
                                                new[] { StringType },
                                                null))
                                    != null)
                        {
                            result = mi.Invoke(null, new object[] { stringValue });
                        }
                    }
                    catch (TargetInvocationException) {}
                }

                return result;
            }

            protected object ConvertFrom(object o, Type destinationType, DependencyObject targetElement, CultureInfo culture)
            {
                return ConvertHelper(o, destinationType, targetElement, culture, false);
            }

            protected object ConvertTo(object o, Type destinationType, DependencyObject targetElement, CultureInfo culture)
            {
                return ConvertHelper(o, destinationType, targetElement, culture, true);
            }

            protected void EnsureConverter(Type type)
            {
                if (_typeConverter == null)
                    _typeConverter = GetConverter(type);
            }

            private object ConvertHelper(object o, Type destinationType, DependencyObject targetElement, CultureInfo culture, bool isForward)
            {
                var value = DependencyProperty.UnsetValue;
                var needAssignment = (isForward ? !_shouldConvertTo : !_shouldConvertFrom);

                NotSupportedException savedException = null;

                if (!needAssignment)
                {
                    value = TryParse(o, destinationType, culture);

                    if (value == DependencyProperty.UnsetValue)
                    {
                        var context = new ValueConverterContext();

                        try
                        {
                            context.SetTargetElement(targetElement);

                            if (isForward)
                                value = _typeConverter.ConvertTo(context, culture, o, destinationType);
                            else
                                value = _typeConverter.ConvertFrom(context, culture, o);
                        }
                        catch (NotSupportedException ex)
                        {
                            needAssignment = true;
                            savedException = ex;
                        }
                        finally
                        {
                            context.SetTargetElement(null);
                        }
                    }
                }

                if (needAssignment && (o != null && destinationType.IsAssignableFrom(o.GetType()) || o == null && !destinationType.IsValueType))
                {
                    value = o;
                    needAssignment = false;
                }

                if (needAssignment && savedException != null)
                    throw savedException;

                return value;
            }
        }

        #endregion

        #region SourceDefaultValueConverter

        private sealed class SourceDefaultValueConverter : DefaultValueConverter, IValueConverter
        {
            public SourceDefaultValueConverter(
                TypeConverter typeConverter,
                Type sourceType,
                Type targetType,
                bool shouldConvertFrom,
                bool shouldConvertTo)
                : base(typeConverter, sourceType, targetType, shouldConvertFrom, shouldConvertTo) {}

            object IValueConverter.Convert(object o, Type type, object parameter, CultureInfo culture)
            {
                return ConvertTo(o, TargetType, parameter as DependencyObject, culture);
            }

            public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
            {
                return ConvertFrom(o, SourceType, parameter as DependencyObject, culture);
            }
        }

        #endregion

        #region TargetDefaultValueConverter

        private sealed class TargetDefaultValueConverter : DefaultValueConverter, IValueConverter
        {
            public TargetDefaultValueConverter(
                TypeConverter typeConverter,
                Type sourceType,
                Type targetType,
                bool shouldConvertFrom,
                bool shouldConvertTo)
                : base(typeConverter, sourceType, targetType, shouldConvertFrom, shouldConvertTo) {}

            object IValueConverter.Convert(object o, Type type, object parameter, CultureInfo culture)
            {
                return ConvertFrom(o, TargetType, parameter as DependencyObject, culture);
            }

            public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
            {
                return ConvertTo(o, SourceType, parameter as DependencyObject, culture);
            }
        }

        #endregion

        #region ObjectTargetConverter

        private sealed class ObjectTargetConverter : DefaultValueConverter, IValueConverter
        {
            public ObjectTargetConverter(Type sourceType)
                : base(null, sourceType, typeof(object), true, false)
            {
            }

            object IValueConverter.Convert(object o, Type type, object parameter, CultureInfo culture)
            {
                return o;
            }

            public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
            {
                if (o == null && !SourceType.IsValueType)
                    return null;

                if (o != null && SourceType.IsAssignableFrom(o.GetType()))
                    return o;

                if (SourceType == typeof(string))
                    return string.Format(culture, "{0}", new[] { o });

                EnsureConverter(SourceType);

                return ConvertFrom(o, SourceType, parameter as DependencyObject, culture);
            }
        }

        #endregion

        #region ObjectSourceConverter

        private sealed class ObjectSourceConverter : DefaultValueConverter, IValueConverter
        {
            public ObjectSourceConverter(Type targetType)
                : base(null, typeof(object), targetType, true, false) {}

            object IValueConverter.Convert(object o, Type type, object parameter, CultureInfo culture)
            {
                if (o != null && TargetType.IsAssignableFrom(o.GetType()))
                    return o;

                if (o == null && !TargetType.IsValueType)
                    return null;

                if (TargetType == typeof(string))
                    return string.Format(culture, "{0}", new[] { o });

                EnsureConverter(TargetType);

                return ConvertFrom(o, TargetType, parameter as DependencyObject, culture);
            }

            public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
            {
                return o;
            }
        }

        #endregion

        #region SystemConvertConverter

        private sealed class SystemConvertConverter : IValueConverter
        {
            private static readonly Type[] SupportedTypes = {
                                                                typeof(string), // put common types up front
                                                                typeof(int), typeof(long), typeof(float), typeof(double),
                                                                typeof(decimal), typeof(bool),
                                                                typeof(byte), typeof(short),
                                                                typeof(uint), typeof(ulong), typeof(ushort), typeof(sbyte) // non-CLS compliant types 
                                                            };

            private static readonly Type[] CharSupportedTypes = {
                                                                    typeof(string), // put common types up front
                                                                    typeof(int), typeof(long), typeof(byte), typeof(short),
                                                                    typeof(uint), typeof(ulong), typeof(ushort), typeof(sbyte) // non-CLS compliant types
                                                                };

            private readonly Type _sourceType;
            private readonly Type _targetType;

            public SystemConvertConverter(Type sourceType, Type targetType)
            {
                _sourceType = sourceType;
                _targetType = targetType;
            }

            object IValueConverter.Convert(object o, Type type, object parameter, CultureInfo culture)
            {
                return System.Convert.ChangeType(o, _targetType, culture);
            }

            public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
            {
                var parsedValue = DefaultValueConverter.TryParse(o, _sourceType, culture);

                return parsedValue != DependencyProperty.UnsetValue
                           ? parsedValue
                           : System.Convert.ChangeType(o, _sourceType, culture);
            }

            public static bool CanConvert(Type sourceType, Type targetType)
            {
                Debug.Assert(sourceType != targetType);

                // DateTime can only be converted to and from String type
                if (sourceType == typeof(DateTime))
                    return (targetType == typeof(string));

                if (targetType == typeof(DateTime))
                    return (sourceType == typeof(string));

                // Char can only be converted to a subset of supported types
                if (sourceType == typeof(char))
                    return CanConvertChar(targetType);

                if (targetType == typeof(char))
                    return CanConvertChar(sourceType);

                // Using nested loops is up to 40% more efficient than using one loop
                for (var i = 0; i < SupportedTypes.Length; ++i)
                {
                    if (sourceType == SupportedTypes[i])
                    {
                        ++i; // assuming (sourceType != targetType), start at next type 
                        for (; i < SupportedTypes.Length; ++i)
                        {
                            if (targetType == SupportedTypes[i])
                                return true;
                        }
                    }
                    else if (targetType == SupportedTypes[i])
                    {
                        ++i; // assuming (sourceType != targetType), start at next type 
                        for (; i < SupportedTypes.Length; ++i)
                        {
                            if (sourceType == SupportedTypes[i])
                                return true;
                        }
                    }
                }

                return false;
            }

            private static bool CanConvertChar(Type type)
            {
                for (var i = 0; i < CharSupportedTypes.Length; ++i)
                {
                    if (type == CharSupportedTypes[i])
                        return true;
                }
                return false;
            }
        }

        #endregion

        #region ListSourceConverter

        private sealed class ListSourceConverter : IValueConverter
        {
            object IValueConverter.Convert(object o, Type type, object parameter, CultureInfo culture)
            {
                var listSource = o as IListSource;
                return listSource != null ? listSource.GetList() : null;
            }

            public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
            {
                return null;
            }
        }

        #endregion

        #region InterfaceConverter

        private sealed class InterfaceConverter : IValueConverter
        {
            private readonly Type _sourceType;
            private readonly Type _targetType;

            internal InterfaceConverter(Type sourceType, Type targetType)
            {
                _sourceType = sourceType;
                _targetType = targetType;
            }

            object IValueConverter.Convert(object o, Type type, object parameter, CultureInfo culture)
            {
                return ConvertTo(o, _targetType);
            }

            public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
            {
                return ConvertTo(o, _sourceType);
            }

            private object ConvertTo(object o, Type type)
            {
                return type.IsInstanceOfType(o) ? o : null;
            }
        }

        #endregion

        #region ValueConverterTable

        private sealed class ValueConverterTable : Hashtable
        {
            public IValueConverter this[Type sourceType, Type targetType, bool targetToSource]
            {
                get { return (IValueConverter)base[new Key(sourceType, targetType, targetToSource)]; }
            }

            public void Add(Type sourceType, Type targetType, bool targetToSource, IValueConverter value)
            {
                base.Add(new Key(sourceType, targetType, targetToSource), value);
            }

            private struct Key
            {
                private readonly Type _sourceType;
                private readonly Type _targetType;
                private readonly bool _targetToSource;

                public Key(Type sourceType, Type targetType, bool targetToSource)
                {
                    _sourceType = sourceType;
                    _targetType = targetType;
                    _targetToSource = targetToSource;
                }

                public static bool operator ==(Key k1, Key k2)
                {
                    if (k1._sourceType == k2._sourceType && k1._targetType == k2._targetType)
                        return k1._targetToSource == k2._targetToSource;
                    return false;
                }

                public static bool operator !=(Key k1, Key k2)
                {
                    return !(k1 == k2);
                }

                public override int GetHashCode()
                {
                    return _sourceType.GetHashCode() + _targetType.GetHashCode();
                }

                public override bool Equals(object o)
                {
                    var key = o as Key?;
                    return key.HasValue && this == key.Value;
                }
            }
        }

        #endregion
    }
}