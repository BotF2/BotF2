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

using Supremacy.Collections;

using TvdP.Collections;

namespace Supremacy.Client
{
    public abstract class Cache<T> where T : class
    {
        private readonly CacheEntryCollection<T> _cache;

        protected Cache()
        {
            _cache = new CacheEntryCollection<T>();
        }

        public T this[string key]
        {
            get
            {
                if (_cache.Contains(key))
                {
                    if (_cache[key].Reference.IsAlive)
                    {
                        return _cache[key].Reference.Target as T;
                    }
                    _cache.Remove(key);
                    return Get(key);
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    if (_cache.Contains(key))
                    {
                        var reference = _cache[key].Reference;
                        if (!reference.IsAlive || (reference.Target != value))
                            _cache.Add(new CacheEntry(key, value));
                    }
                    else
                    {
                        _cache.Add(new CacheEntry(key, value));
                    }
                }
                else if (_cache.Contains(key))
                {
                    _cache.Remove(key);
                }
            }
        }

        public T Get(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            return Get(uri.OriginalString);
        }

        public T Get(string uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            if (_cache.Contains(uri))
            {
                if (_cache[uri].Reference.IsAlive)
                {
                    return _cache[uri].Reference.Target as T;
                }
                _cache.Remove(uri);
            }
            try
            {
                var item = Load(uri);
                if (item != null)
                {
                    this[uri] = item;
                }
                return item;
            }
            catch
            {
                return null;
            }
        }

        protected abstract T Load(string uri);

        public struct CacheEntry
        {
            private readonly string _key;
            private readonly WeakReference _reference;

            public string Key
            {
                get { return _key; }
            }

            public WeakReference Reference
            {
                get { return _reference; }
            }

            public CacheEntry(string key, T target)
            {
                _key = key;
                _reference = new WeakReference(target);
            }
        }
    }

    public sealed class CacheEntryCollection<T> : KeyedCollectionBase<string, Cache<T>.CacheEntry> where T : class
    {
        public CacheEntryCollection() : base(o => o.Key) {}
    }

    public sealed class ImageCache
    {
        private static readonly ImageCache s_current;
        private readonly Cache<string, BitmapImage> _cache;

        static ImageCache()
        {
            s_current = new ImageCache();
        }

        public ImageCache()
        {
            _cache = new Cache<string, BitmapImage>(StringComparer.OrdinalIgnoreCase);
        }

        public static ImageCache Current
        {
            get { return s_current; }
        }

        public bool ForceCaching { get; set; }

        private BitmapImage Load(string uri)
        {
            var image = new BitmapImage();
            image.BeginInit();
            if (ForceCaching)
                image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(uri, UriKind.RelativeOrAbsolute);
            image.EndInit();
            image.Freeze();
            return image;
        }

        public BitmapImage Get(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            return Get(uri.OriginalString);
        }

        public BitmapImage Get(string uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            if (uri == null)
                throw new ArgumentNullException("uri");

            BitmapImage image;

            if (_cache.TryGetItem(uri, out image))
                return image;

            image = Load(uri);

            if (image == null)
                return null;

            return _cache.GetOldest(uri, image);
        }
    }
}
