// SimpleCaseConverter.cs
// 
// Copyright (c) 2011 Mike Strobel
// 
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
// 
// All other rights reserved.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

using System.Windows.Markup;

namespace Supremacy.Client.Data
{
    public sealed class SimpleCaseConverter : Freezable, IValueConverter
    {
        public static readonly DependencyProperty CaseProperty =
            DependencyProperty.Register(
                "Case",
                typeof(object),
                typeof(SimpleCaseConverter),
                new PropertyMetadata(default(object)));

        public static readonly DependencyProperty IfMatchProperty =
            DependencyProperty.Register(
                "IfMatch",
                typeof(object),
                typeof(SimpleCaseConverter),
                new PropertyMetadata(default(object)));

        public static readonly DependencyProperty ElseProperty =
            DependencyProperty.Register(
                "Else",
                typeof(object),
                typeof(SimpleCaseConverter),
                new PropertyMetadata(default(object)));

        public object Case
        {
            get { return GetValue(CaseProperty); }
            set { SetValue(CaseProperty, value); }
        }

        public object IfMatch
        {
            get { return GetValue(IfMatchProperty); }
            set { SetValue(IfMatchProperty, value); }
        }

        public object Else
        {
            get { return GetValue(ElseProperty); }
            set { SetValue(ElseProperty, value); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object comparand = Case;
            bool isMatch = Equals(value, comparand);

            if (!isMatch && comparand != null && value != null)
            {
                object convertedComparand = ValueConversionHelper.Convert(comparand, value.GetType(), null, culture);
                if (convertedComparand != DependencyProperty.UnsetValue)
                {
                    isMatch = Equals(value, convertedComparand);
                }
                else
                {
                    object convertedValue = ValueConversionHelper.Convert(value, comparand.GetType(), null, culture);
                    if (convertedValue != DependencyProperty.UnsetValue)
                        isMatch = Equals(convertedValue, comparand);
                }
            }

            object convertedResult = ValueConversionHelper.Convert(
                isMatch ? IfMatch : Else,
                targetType,
                parameter,
                culture);

            return convertedResult;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new SimpleCaseConverter();
        }
    }

    [MarkupExtensionReturnType(typeof(SimpleCaseConverter))]
    public sealed class SimpleCaseExtension : MarkupExtension
    {
        public object Case { get; set; }

        public object IfMatch { get; set; }

        public object Else { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new SimpleCaseConverter
            {
                Case = Case,
                IfMatch = IfMatch,
                Else = Else
            };
        }
    }
}