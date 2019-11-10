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
        // The collection of design arguments for the generic design
        private Collection<Type> _typeArguments = new Collection<Type>();
        // The generic design name (e.g. Dictionary, for the Dictionary<K,V> case)
        private string _typeName;

        public Collection<Type> TypeArguments
        {
            get { return _typeArguments; }
        }

        public string TypeName
        {
            get { return _typeName; }
            set { _typeName = value; }
        }

        // Constructors
        public GenericExtension() { }

        public GenericExtension(string typeName)
        {
            TypeName = typeName;
        }

        // ProvideValue, which returns an object instance of the constructed generic design
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            IXamlTypeResolver xamlTypeResolver = serviceProvider.GetService(typeof(IXamlTypeResolver)) as IXamlTypeResolver;

            if (xamlTypeResolver == null)
                throw new Exception("The Generic markup extension requires an IXamlTypeResolver service provider");

            // Get e.g. "Collection`1" design
            Type genericType = xamlTypeResolver.Resolve(
                _typeName + "`" + TypeArguments.Count.ToString());

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
        // Type arguments.  This is read/write so that it can be
        // set in Xaml attribute syntax with a design converter.
        private Collection<Type> _typeArguments = new Collection<Type>();

        // The generic design name (e.g. Dictionary, for the Dictionary<K,V> case)
        private string _typeName;

        [TypeConverter(typeof(TypeCollectionConverter))]
        public Collection<Type> TypeArguments
        {
            get { return _typeArguments; }
            set { _typeArguments = value; }
        }

        public string TypeName
        {
            get { return _typeName; }
            set { _typeName = value; }
        }

        // Constructors
        public GenericTypeExtension() { }

        public GenericTypeExtension(string typeName)
        {
            TypeName = typeName;
        }

        // ProvideValue, which returns the concrete object of the generic design
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            IXamlTypeResolver xamlTypeResolver = serviceProvider.GetService(typeof(IXamlTypeResolver)) as IXamlTypeResolver;
            if (xamlTypeResolver == null)
                throw new Exception("The Generic markup extension requires an IXamlTypeResolver service provider");

            // Get e.g. "Collection`1" design
            Type genericType = xamlTypeResolver.Resolve(
                _typeName + "`" + TypeArguments.Count.ToString());

            // Get an array of the design arguments
            Type[] typeArgumentArray = new Type[TypeArguments.Count];
            TypeArguments.CopyTo(typeArgumentArray, 0);

            // Create the conrete design, e.g. Collection<String>
            return genericType.MakeGenericType(typeArgumentArray);
        }
    }
}