// ActivatorExtension.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Supremacy.Client
{
    /// <summary>
    /// Represents a single property value for the <see cref="ActivatorExtension"/>.
    /// </summary>
    [ContentProperty("Value")]
    public class ActivatorSetter : DependencyObject
    {
        #region Properties

        /// <summary>
        /// Gets or sets the property name.
        /// </summary>
        /// <value>The property name.</value>
        public string Name
        {
            get => (string)GetValue(NameProperty);
            set => SetValue(NameProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="Name"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(ActivatorSetter), new PropertyMetadata());

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        /// <value>The property value.</value>
        public object Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="Value"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(ActivatorSetter), new PropertyMetadata());

        #endregion
    }

    [ContentProperty("TypeArguments")]
    public class ActivatorTypeExtension : MarkupExtension
    {
        #region Fields

        private Type _closedType;

        #endregion

        #region Constructors

        public ActivatorTypeExtension()
        {
            _typeArguments = new List<Type>();

        }

        public ActivatorTypeExtension(string typeName)
            : this()
        {
            NotNull(typeName);

            _typeName = typeName;
        }

        public ActivatorTypeExtension(string typeName, Type typeArgument1)
            : this(typeName)
        {
            NotNull(typeArgument1);

            TypeArgument1 = typeArgument1;
        }

        public ActivatorTypeExtension(string typeName, Type typeArgument1, Type typeArgument2)
            : this(typeName, typeArgument1)
        {
            NotNull(typeArgument2);

            TypeArgument2 = typeArgument2;
        }

        public ActivatorTypeExtension(string typeName, Type typeArgument1, Type typeArgument2, Type typeArgument3)
            : this(typeName, typeArgument1, typeArgument2)
        {
            NotNull(typeArgument3);

            TypeArgument3 = typeArgument3;
        }

        public ActivatorTypeExtension(string typeName, Type typeArgument1, Type typeArgument2, Type typeArgument3, Type typeArgument4)
            : this(typeName, typeArgument1, typeArgument2, typeArgument3)
        {
            NotNull(typeArgument4);

            TypeArgument4 = typeArgument4;
        }

        #endregion

        #region Properties

        private string _typeName;
        [ConstructorArgument("typeName")]
        public string TypeName
        {
            get => _typeName;
            set
            {
                NotNull(value);

                _typeName = value;
                _type = null;
            }
        }

        private Type _type;
        public Type Type
        {
            get => _type;
            set
            {
                NotNull(value);

                _type = value;
                _typeName = null;
            }
        }

        private readonly List<Type> _typeArguments;
        public IList<Type> TypeArguments => _typeArguments;

        [ConstructorArgument("typeArgument1")]
        public Type TypeArgument1
        {
            get => GetTypeArgument(0);
            set => SetTypeArgument(0, value);
        }

        [ConstructorArgument("typeArgument2")]
        public Type TypeArgument2
        {
            get => GetTypeArgument(1);
            set => SetTypeArgument(1, value);
        }

        [ConstructorArgument("typeArgument3")]
        public Type TypeArgument3
        {
            get => GetTypeArgument(2);
            set => SetTypeArgument(2, value);
        }

        [ConstructorArgument("typeArgument4")]
        public Type TypeArgument4
        {
            get => GetTypeArgument(3);
            set => SetTypeArgument(3, value);
        }

        #endregion

        #region Methods

        private static void NotNull(object o)
        {
            if (o == null)
            {
                throw new ArgumentNullException();
            }
        }

        private Type GetTypeArgument(int index)
        {
            return index < _typeArguments.Count ? _typeArguments[index] : null;
        }

        private void SetTypeArgument(int index, Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException();
            }

            if (index > _typeArguments.Count)
            {
                throw new ArgumentOutOfRangeException("Type Arguments need to be specified in the right order.");
            }

            if (index == _typeArguments.Count)
            {
                _typeArguments.Add(type);
            }
            else
            {
                _typeArguments[index] = type;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_typeName == null && _type == null)
            {
                throw new InvalidOperationException("Must specify Type or TypeName.");
            }

            Type type = _type;
            Type[] typeArguments = _typeArguments.TakeWhile(t => t != null).ToArray();

            if (_closedType == null)
            {
                if (type == null)
                {
                    // resolve using type name
                    if (!(serviceProvider.GetService(typeof(IXamlTypeResolver)) is IXamlTypeResolver typeResolver))
                    {
                        throw new InvalidOperationException("Cannot retrieve IXamlTypeResolver.");
                    }

                    // check that the number of generic arguments match
                    string typeName = _typeName;
                    if (typeArguments.Length > 0)
                    {
                        int genericsMarkerIndex = typeName.LastIndexOf('`');
                        if (genericsMarkerIndex < 0)
                        {
                            typeName = string.Format("{0}`{1}", typeName, typeArguments.Length);
                        }
                        else
                        {
                            bool validArgumentCount = false;
                            if (genericsMarkerIndex < typeName.Length)
                            {
                                if (int.TryParse(typeName.Substring(genericsMarkerIndex + 1), out int typeArgumentCount))
                                {
                                    validArgumentCount = true;
                                }
                            }

                            if (!validArgumentCount)
                            {
                                throw new InvalidOperationException("Invalid type argument count in type name.");
                            }
                        }
                    }

                    type = typeResolver.Resolve(typeName);
                    if (type == null)
                    {
                        throw new InvalidOperationException("Invalid type name.");
                    }
                }
                else if (type.IsGenericTypeDefinition && type.GetGenericArguments().Length != typeArguments.Length)
                {
                    throw new InvalidOperationException("Invalid type argument count in type.");
                }

                // build closed type
                _closedType = typeArguments.Length > 0 && type.IsGenericTypeDefinition ? type.MakeGenericType(typeArguments) : type;
            }

            return _closedType;
        }

        #endregion
    }
    /// <summary>
    /// Implements a markup extension that creates types (including generics) at runtime.
    /// </summary>
    [ContentProperty("PropertyValues")]
    public class ActivatorExtension : MarkupExtension
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivatorExtension"/> class.
        /// </summary>
        public ActivatorExtension()
        {
            PropertyValues = new List<ActivatorSetter>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivatorExtension"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public ActivatorExtension(Type type)
            : this()
        {
            Type = type;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the type to create.
        /// </summary>
        /// <value>The type to create.</value>
        [ConstructorArgument("type")]
        public Type Type { get; set; }

        /// <summary>
        /// Gets the property values.
        /// <remarks>
        /// This is the content property, so it can be specified in XAML directly beneath the object.
        /// </remarks>
        /// </summary>
        /// <value>The property values.</value>
        public List<ActivatorSetter> PropertyValues { get; }

        /// <summary>
        /// Gets the created object.
        /// </summary>
        /// <value>The created object.</value>
        public object Value
        {
            get
            {
                if (Type == null)
                {
                    throw new InvalidOperationException("Type was not specified.");
                }

                object value = Activator.CreateInstance(Type);

                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(Type);
                foreach (ActivatorSetter propertyValue in PropertyValues)
                {
                    PropertyDescriptor property = properties[propertyValue.Name];
                    if (property == null)
                    {
                        throw new XamlParseException(string.Format("Invalid property name '{0}'.", propertyValue.Name));
                    }
                    DependencyPropertyDescriptor dpDescriptor = DependencyPropertyDescriptor.FromProperty(property);
                    BindingBase binding = BindingOperations.GetBindingBase(
                        propertyValue, ActivatorSetter.ValueProperty);

                    // if the property is data-bound, transfer the binding
                    if (dpDescriptor != null && binding != null)
                    {
                        BindingOperations.ClearBinding(propertyValue, ActivatorSetter.ValueProperty);
                        _ = BindingOperations.SetBinding(
                            (DependencyObject)value, dpDescriptor.DependencyProperty, binding);
                    }
                    else if (propertyValue.Value != null)
                    {
                        Type propertyValueType = propertyValue.Value.GetType();
                        // if the value is assignable, assign it
                        if (property.PropertyType.IsAssignableFrom(propertyValueType))
                        {
                            property.SetValue(value, propertyValue.Value);
                        }
                        // try to use a type converter to get the value
                        else if (property.Converter.CanConvertFrom(propertyValueType))
                        {
                            try
                            {
                                property.SetValue(value, property.Converter.ConvertFrom(propertyValue.Value));
                            }
                            catch (FormatException ex)
                            {
                                throw new XamlParseException("Cannot convert value.", ex);
                            }
                        }
                    }
                    // if the property is nullable and the assigned value is null, assign null
                    else if (property.PropertyType.IsClass ||
                             (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                    {
                        property.SetValue(value, null);
                    }
                }
                return value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the constructed object.
        /// </summary>
        /// <param name="serviceProvider">Object that can provide services for the markup extension.</param>
        /// <returns>
        /// The object value to set on the property where the extension is applied.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Value;
        }

        #endregion
    }
}