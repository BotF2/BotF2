// StringResource.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

using Supremacy.Resources;

namespace Supremacy.Client
{

    public enum StringCaseEnum
    {
        Original = 0,
        Lower = 1,
        Upper = 2
    }

    [ContentProperty("Key")]
    [MarkupExtensionReturnType(typeof(object))]
    public sealed class StringResource : MarkupExtension
    {
        private string _key;

        [ConstructorArgument("key")]
        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        public StringCaseEnum Case { get; set; }

        public StringResource()
        {
            _key = String.Empty;
            Case = StringCaseEnum.Original;
        }

        public StringResource(string key)
        {
            _key = key;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var text = ResourceManager.GetString(_key);
            switch (Case)
            {
                case StringCaseEnum.Lower:
                    text = text.ToLower();
                    break;
                case StringCaseEnum.Upper:
                    text = text.ToUpper();
                    break;
            }
            if (serviceProvider != null)
            {
                var provideValueTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
                if (provideValueTarget != null)
                {
                    var property = provideValueTarget.TargetProperty;
                    if (!(property is DependencyProperty) && 
                        !(property is PropertyInfo) && 
                        !(property is PropertyDescriptor))
                    {
                        return this;
                    }
                    if (Equals(property, ContentControl.ContentProperty))
                        return new AccessText { Text = text };
                }
            }
            return text;
        }
    }
}
