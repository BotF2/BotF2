// GenericExtension.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Markup;

namespace Supremacy.Client
{
    [ContentProperty("TypeArguments")]
    [MarkupExtensionReturnType(typeof(object))]
    public class GenericExtension : MarkupExtension
    {
        public Collection<Type> TypeArguments { get; } = new Collection<Type>();

        public string TypeName { get; set; }

        // Constructors
        public GenericExtension() { }

        public GenericExtension(string typeName)
        {
            TypeName = typeName;
        }

        // ProvideValue, which returns an object instance of the constructed generic design
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (!(serviceProvider.GetService(typeof(IXamlTypeResolver)) is IXamlTypeResolver xamlTypeResolver))
            {
                throw new Exception("The Generic markup extension requires an IXamlTypeResolver service provider");
            }

            // Get e.g. "Collection`1" design
            Type genericType = xamlTypeResolver.Resolve(
                TypeName + "`" + TypeArguments.Count.ToString());

            // Get an array of the design arguments
            Type[] typeArgumentArray = new Type[TypeArguments.Count];
            TypeArguments.CopyTo(typeArgumentArray, 0);

            // Create the conrete design, e.g. Collection<String>
            Type constructedType = genericType.MakeGenericType(typeArgumentArray);

            // Create an instance of that design
            return Activator.CreateInstance(constructedType);
        }
    }

    [ContentProperty("TypeArguments")]
    [MarkupExtensionReturnType(typeof(Type))]
    public class GenericTypeExtension : MarkupExtension
    {
        [TypeConverter(typeof(TypeCollectionConverter))]
        public Collection<Type> TypeArguments { get; set; } = new Collection<Type>();

        public string TypeName { get; set; }

        // Constructors
        public GenericTypeExtension() { }

        public GenericTypeExtension(string typeName)
        {
            TypeName = typeName;
        }

        // ProvideValue, which returns the concrete object of the generic design
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (!(serviceProvider.GetService(typeof(IXamlTypeResolver)) is IXamlTypeResolver xamlTypeResolver))
            {
                throw new Exception("The Generic markup extension requires an IXamlTypeResolver service provider");
            }

            // Get e.g. "Collection`1" design
            Type genericType = xamlTypeResolver.Resolve(
                TypeName + "`" + TypeArguments.Count.ToString());

            // Get an array of the design arguments
            Type[] typeArgumentArray = new Type[TypeArguments.Count];
            TypeArguments.CopyTo(typeArgumentArray, 0);

            // Create the conrete design, e.g. Collection<String>
            return genericType.MakeGenericType(typeArgumentArray);
        }
    }
}