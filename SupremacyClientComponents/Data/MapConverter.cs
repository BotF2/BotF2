using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Supremacy.Client.Data
{
    /// <summary>
    /// An implementation of <see cref="IValueConverter"/> that converts from one set of values to another based on the contents of the
    /// <see cref="Mappings"/> collection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <c>MapConverter</c> converts from one set of values to another. The source and destination values are stored in instances of
    /// <see cref="Mapping"/> inside the <see cref="Mappings"/> collection. 
    /// </para>
    /// <para>
    /// If this converter is asked to convert a value for which it has no knowledge, it will use the <see cref="FallbackBehavior"/> to determine
    /// how to deal with the situation.
    /// </para>
    /// </remarks>
    /// <example>
    /// The following example shows a <c>MapConverter</c> being used to control the visibility of a <c>Label</c> based on a
    /// <c>CheckBox</c>:
    /// <code lang="xml">
    /// <![CDATA[
    /// <CheckBox x:Name="_checkBox"/>
    /// <Label Content="Here is the label.">
    ///   <Label.Visibility>
    ///     <Binding Path="IsChecked" ElementName="_checkBox" FallbackValue="Collapsed">
    ///       <Binding.Converter>
    ///         <MapConverter>
    ///           <Mapping From="True" To="Visible" />
    ///         </MapConverter>
    ///       </Binding.Converter>
    ///     </Binding>
    ///   </Label.Visibility>
    /// </Label>
    /// ]]>
    /// </code>
    /// </example>
    /// <example>
    /// The following example shows how a <c>MapConverter</c> can be used to convert between values of the <see cref="UriFormat"/>
    /// enumeration and human-readable strings. Notice how not all possible values are present in the mappings. The fallback behavior
    /// is set to <c>ReturnOriginalValue</c> to ensure that any conversion failures result in the original value being returned:
    /// <code lang="xml">
    /// <![CDATA[
    /// <Label>
    ///   <Label.Content>
    ///     <Binding Path="UriFormat">
    ///       <Binding.Converter>
    ///         <MapConverter FallbackBehavior="ReturnOriginalValue">
    ///           <Mapping From="{x:Static sys:UriFormat.SafeUnescaped}" To="Safe unescaped"/>
    ///           <Mapping From="{x:Static sys:UriFormat.UriEscaped}" To="URI escaped"/>
    ///         </MapConverter>
    ///       </Binding.Converter>
    ///     </Binding>
    ///   </Label.Content>
    /// </Label>
    /// ]]>
    /// </code>
    /// </example>
    [ContentProperty("Mappings")]
    [ValueConversion(typeof(object), typeof(object))]
    public class MapConverter : Freezable, IValueConverter
    {
        protected static readonly DependencyPropertyKey MappingsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "Mappings",
                typeof(Collection<Mapping>),
                typeof(MapConverter),
                new FrameworkPropertyMetadata());

        /// <summary>
        /// Identifies the <see cref="FallbackBehavior"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FallbackBehaviorProperty =
            DependencyProperty.Register(
                "FallbackBehavior",
                typeof(FallbackBehavior),
                typeof(MapConverter),
                new FrameworkPropertyMetadata(),
                ValidateFallbackValue);

        /// <summary>
        /// Identifies the <see cref="FallbackValue"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FallbackValueProperty =
            DependencyProperty.Register(
                "FallbackValue",
                typeof(object),
                typeof(MapConverter),
                new FrameworkPropertyMetadata());

        /// <summary>
        /// Identifies the <see cref="Mappings"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MappingsProperty = MappingsPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets or sets the fallback behavior for this <c>MapConverter</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The fallback behavior determines how this <c>MapConverter</c> treats failed conversions. <c>ReturnUnsetValue</c> (the default)
        /// specifies that any failed conversions should return <see cref="DependencyProperty.UnsetValue"/>, which can be used in combination with
        /// <c>Binding.FallbackValue</c> to default bindings to a specific value.
        /// </para>
        /// <para>
        /// Alternatively, <c>FallbackBehavior.ReturnOriginalValue</c> can be specified so that failed conversions result in the original value
        /// being returned. This is useful where mappings are only necessary for a subset of the total possible values. Mappings can be specified
        /// where necessary and other values can be returned as is by the <c>MapConverter</c> by setting the fallback behavior to
        /// <c>ReturnOriginalValue</c>.
        /// </para>
        /// </remarks>
        public FallbackBehavior FallbackBehavior
        {
            get { return (FallbackBehavior)GetValue(FallbackBehaviorProperty); }
            set { SetValue(FallbackBehaviorProperty, value); }
        }

        public object FallbackValue
        {
            get { return GetValue(FallbackValueProperty); }
            set { SetValue(FallbackValueProperty, value); }
        }

        /// <summary>
        /// Gets the collection of <see cref="Mapping"/>s configured for this <c>MapConverter</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Each <see cref="Mapping"/> defines a relationship between a source object (see <see cref="Mapping.From"/>) and a destination (see
        /// <see cref="Mapping.To"/>). The <c>MapConverter</c> uses these mappings whilst attempting to convert values.
        /// </para>
        /// </remarks>
        public Collection<Mapping> Mappings
        {
            get { return GetValue(MappingsProperty) as Collection<Mapping>; }
            private set
            {
                Debug.Assert(Mappings == null);
                SetValue(MappingsPropertyKey, value);
            }
        }

        /// <summary>
        /// Constructs an instance of <c>MapConverter</c>.
        /// </summary>
        public MapConverter()
        {
            Mappings = new Collection<Mapping>();
        }

        /// <summary>
        /// Attempts to convert the specified value.
        /// </summary>
        /// <param name="value">
        /// The value to convert.
        /// </param>
        /// <param name="targetType">
        /// The type of the binding target property.
        /// </param>
        /// <param name="parameter">
        /// The converter parameter to use.
        /// </param>
        /// <param name="culture">
        /// The culture to use in the converter.
        /// </param>
        /// <returns>
        /// A converted value.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mapping = FindMapping(value, false);
            if (mapping != null)
            {
                var convertedValue = ValueConversionHelper.Convert(mapping.To, targetType, parameter, culture);
                if (convertedValue != DependencyProperty.UnsetValue)
                    return convertedValue;
            }

            if (!this.HasDefaultValue(FallbackValueProperty))
                return FallbackValue;

            if (FallbackBehavior == FallbackBehavior.ReturnUnsetValue)
                return DependencyProperty.UnsetValue;

            return value;
        }

        /// <summary>
        /// Attempts to convert the specified value back.
        /// </summary>
        /// <param name="value">
        /// The value to convert.
        /// </param>
        /// <param name="targetType">
        /// The type of the binding target property.
        /// </param>
        /// <param name="parameter">
        /// The converter parameter to use.
        /// </param>
        /// <param name="culture">
        /// The culture to use in the converter.
        /// </param>
        /// <returns>
        /// A converted value.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mapping = FindMapping(value, true);
            if (mapping != null)
            {
                var convertedValue = ValueConversionHelper.Convert(mapping.From, targetType, parameter, culture);
                if (convertedValue != DependencyProperty.UnsetValue)
                    return convertedValue;
            }

            if (FallbackBehavior == FallbackBehavior.ReturnUnsetValue)
                return DependencyProperty.UnsetValue;

            return value;
        }

        private Mapping FindMapping(object value, bool reverse)
        {
            foreach (var mapping in Mappings)
            {
                var comparand = reverse ? mapping.To : mapping.From;
                if (comparand == null)
                {
                    if (value == null)
                        return mapping;
                    continue;
                }

                if (value == null)
                    continue;

                if (reverse)
                {
                    var convertedValue = ValueConversionHelper.Convert(value, comparand.GetType());
                    if (convertedValue == DependencyProperty.UnsetValue)
                    {
                        var convertedComparand = ValueConversionHelper.Convert(comparand, value.GetType());
                        if (convertedComparand != DependencyProperty.UnsetValue && Equals(convertedComparand, value))
                            return mapping;
                        continue;
                    }

                    if (Equals(convertedValue, comparand))
                        return mapping;
                }
                else
                {
                    var convertedComparand = ValueConversionHelper.Convert(comparand, value.GetType());
                    if (convertedComparand == DependencyProperty.UnsetValue)
                    {
                        var convertedValue = ValueConversionHelper.Convert(value, comparand.GetType());
                        if (convertedValue != DependencyProperty.UnsetValue && Equals(convertedValue, value))
                            return mapping;
                        continue;
                    }

                    if (Equals(value, convertedComparand))
                        return mapping;
                }
            }

            return null;
        }

        private static bool ValidateFallbackValue(object value)
        {
            return Equals(value, FallbackBehavior.ReturnOriginalValue) ||
                   Equals(value, FallbackBehavior.ReturnUnsetValue);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new MapConverter();
        }
    }

    /// <summary>
    /// Represents a mapping <see cref="From"/> one value <see cref="To"/> another.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="MapConverter"/> uses instances of this class to define mappings between one set of values and another.
    /// </para>
    /// </remarks>
    public class Mapping : DependencyObject
    {
        /// <summary>
        /// Identifies the <see cref="From"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FromProperty = DependencyProperty.Register(
            "From",
            typeof(object),
            typeof(Mapping));

        /// <summary>
        /// Identifies the <see cref="To"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ToProperty = DependencyProperty.Register(
            "To",
            typeof(object),
            typeof(Mapping));

        /// <summary>
        /// Gets or sets the source object for the mapping.
        /// </summary>
        [ConstructorArgument("from")]
        public object From
        {
            get { return GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }

        /// <summary>
        /// Gets or sets the destination object for the mapping.
        /// </summary>
        [ConstructorArgument("to")]
        public object To
        {
            get { return GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }

        /// <summary>
        /// Constructs a default instance of <c>Mapping</c>.
        /// </summary>
        public Mapping() { }

        /// <summary>
        /// Constructs an instance of <c>Mapping</c> with the specified <paramref name="from"/> and <paramref name="to"/> values.
        /// </summary>
        /// <param name="from">
        /// The value for the source in the mapping (see <see cref="From"/>).
        /// </param>
        /// <param name="to">
        /// The value for the destination in the mapping (see <see cref="To"/>).
        /// </param>
        public Mapping(object from, object to)
        {
            From = from;
            To = to;
        }
    }

    /// <summary>
    /// Defines possible fallback behaviors for the <see cref="MapConverter"/>.
    /// </summary>
    public enum FallbackBehavior
    {
        /// <summary>
        /// Specifies that <see cref="System.Windows.DependencyProperty.UnsetValue"/> should be returned when falling back.
        /// </summary>
        ReturnUnsetValue,
        /// <summary>
        /// Specifies that the value being converted should be returned when falling back.
        /// </summary>
        ReturnOriginalValue
    }
}