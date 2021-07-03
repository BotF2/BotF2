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
    public enum StringCase
    {
        Original = 0,
        Lower = 1,
        Upper = 2
    }

    [ContentProperty("Key")]
    [MarkupExtensionReturnType(typeof(object))]
    public sealed class StringResourceExtension : MarkupExtension
    {
        [ConstructorArgument("key")]
        public string Key { get; set; }

        public StringCase Case { get; set; }

        public StringResourceExtension()
        {
            Key = string.Empty;
            Case = StringCase.Original;
        }

        public StringResourceExtension(string key)
        {
            Key = key;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            string text = ResourceManager.GetString(Key);

            switch (Case)
            {
                case StringCase.Lower:
                    text = text.ToLower();
                    break;
                case StringCase.Upper:
                    text = text.ToUpper();
                    break;
            }


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

            return text;
        }
    }
}
