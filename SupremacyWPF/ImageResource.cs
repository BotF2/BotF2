// ImageResource.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.IO;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

using Supremacy.Resources;

namespace Supremacy.Client
{
    [MarkupExtensionReturnType(typeof(BitmapImage))]
    public sealed class ImageResource : MarkupExtension
    {
        private string filename;

        public ImageResource(string filename)
        {
            this.filename = filename;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (filename == null)
                return null;
            if (!File.Exists(filename))
                return null;
            return ImageCache.Current.Get(ResourceManager.GetResourceUri(filename));
        }
    }
}
