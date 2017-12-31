// AccessTextConverter.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Supremacy.Client
{
    [MarkupExtensionReturnType(typeof(AccessText))]
    public sealed class AccessTextExtension : MarkupExtension
    {
        public AccessTextExtension() {}

        public AccessTextExtension(object text)
        {
            Text = text;
        }

        [ConstructorArgument("text")]
        public object Text { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var accessText = new AccessText();

            var markupExtension = Text as MarkupExtension;
            if (markupExtension != null)
                accessText.SetValue(AccessText.TextProperty, markupExtension.ProvideValue(serviceProvider));
            else
                accessText.SetValue(AccessText.TextProperty, Text);

            return accessText;
        }
    }
}