// ImageCache.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Windows.Media.Imaging;
using TvdP.Collections;

namespace Supremacy.Client
{
    public sealed class ImageCache
    {
        private readonly Cache<string, BitmapImage> _cache;

        static ImageCache()
        {
            Current = new ImageCache();
        }

        public ImageCache()
        {
            _cache = new Cache<string, BitmapImage>(StringComparer.OrdinalIgnoreCase);
        }

        public static ImageCache Current { get; private set; }

        public bool ForceCaching { get; set; }

        private BitmapImage Load(string uri)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            if (ForceCaching)
            {
                image.CacheOption = BitmapCacheOption.OnLoad;
            }

            image.UriSource = new Uri(uri, UriKind.RelativeOrAbsolute);
            image.EndInit();
            image.Freeze();
            return image;
        }

        public BitmapImage Get(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            return Get(uri.OriginalString);
        }

        public BitmapImage Get(string uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }


            if (_cache.TryGetItem(uri, out BitmapImage image))
            {
                return image;
            }

            image = Load(uri);

            if (image == null)
            {
                return null;
            }

            return _cache.GetOldest(uri, image);
        }
    }
}
