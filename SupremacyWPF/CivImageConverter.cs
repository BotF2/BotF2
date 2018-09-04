// CivImageConverter.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

using Supremacy.Entities;
using Supremacy.Resources;

namespace Supremacy.Client
{
    [ValueConversion(typeof(Civilization), typeof(ImageSource))]
    public class CivImageConverter : IValueConverter
    {
        private static readonly string ImageMissingPath;
        private static readonly string[] Extensions;
        private static readonly string[] Folders;
        private static readonly string BasePath;

        static CivImageConverter()
        {
            ImageMissingPath = @"Resources\Images\TechObjects\__image_missing.png";
            Extensions = new string[] { "png", "jpg" };
            Folders = new string[] { "Civilizations", "Races" };
            BasePath = @"Resources\Images\{1}\{2}.{3}";
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            var civ = value as Civilization;
            if (civ != null)
                return Convert(civ.Key);
            return Convert(value.ToString()) ?? value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        private static string KeyToFilename(string key)
        {
            StringBuilder filename = new StringBuilder();
            for (int i = 0; i < key.Length; i++)
            {
                if (char.IsLetterOrDigit(key[i]))
                    filename.Append(char.ToLowerInvariant(key[i]));
            }
            return filename.ToString();
        }

        public ImageSource Convert(string civKey)
        {
            if (string.IsNullOrEmpty(civKey))
                return null;
            string baseDir = Environment.CurrentDirectory;
            foreach (string folder in Folders)
            {
                foreach (string extension in Extensions)
                {
                    string filename = string.Format(
                        BasePath,
                        baseDir,
                        folder,
                        KeyToFilename(civKey),
                        extension);
                    if (File.Exists(ResourceManager.GetResourcePath(filename)))
                    {
                        return ImageCache.Current.Get(ResourceManager.GetResourceUri(filename));
                    }
                }
            }
            if (File.Exists(ResourceManager.GetResourcePath(ImageMissingPath)))
            {
                return ImageCache.Current.Get(ResourceManager.GetResourceUri(ImageMissingPath));
            }
            return null;
        }
    }
}
