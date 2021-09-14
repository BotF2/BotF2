using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

using Supremacy.Resources;

namespace Supremacy.Client
{
    [ContentProperty("Key")]
    [MarkupExtensionReturnType(typeof(string))]
    public sealed class EnumStringResourceExtension : MarkupExtension
    {
        [ConstructorArgument("key")]
        public string Key { get; set; }

        public StringCase Case { get; set; }

        public EnumStringResourceExtension()
        {
            Key = string.Empty;
            Case = StringCase.Original;
        }

        public EnumStringResourceExtension(string key)
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