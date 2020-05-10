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
        [ConstructorArgument("key")]
        public string Key { get; set; }

        public StringCaseEnum Case { get; set; }

        public StringResource()
        {
            Key = string.Empty;
            Case = StringCaseEnum.Original;
        }

        public StringResource(string key)
        {
            Key = key;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            string text = ResourceManager.GetString(Key);
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
                if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget provideValueTarget)
                {
                    object property = provideValueTarget.TargetProperty;
                    if (!(property is DependencyProperty) &&
                        !(property is PropertyInfo) &&
                        !(property is PropertyDescriptor))
                    {
                        return this;
                    }
                    if (Equals(property, ContentControl.ContentProperty))
                    {
                        return new AccessText { Text = text };
                    }
                }
            }
            return text;
        }
    }
}
