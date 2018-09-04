// TechObjectImageConverter.cs
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
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Resources;
using Supremacy.Tech;
using Supremacy.Universe;
using Supremacy.Utility;

namespace Supremacy.Client
{
    internal static class ImageHelper
    {
        public const string ImageMissingPath = @"Resources\Images\__image_missing.png";

        public static string GetImagePath(string path)
        {
            if (path == null)
                return null;
            string result = ResourceManager.GetResourcePath(path);
            if (File.Exists(result))
                return result;
            result = ResourceManager.GetResourcePath(ImageMissingPath);
            if (File.Exists(result))
                return result;
            return null;
        }

        public static Uri GetImageUri(string path)
        {
            if (path == null)
                return null;
            Uri uri;
            if (Uri.TryCreate(path, UriKind.Absolute, out uri))
                return uri;
            return ResourceManager.GetResourceUri(GetImagePath(path));
        }

        public static BitmapImage GetImage(string path)
        {
            if (path == null)
                return null;
            try
            {
                var imageUri = GetImageUri(path);
                return ImageCache.Current.Get(imageUri);
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
            }

            return null;
        }
    }

    [ValueConversion(typeof(Sector), typeof(String))]
    public class SectorNameConverter : AppContextAwareValueConverter
    {
        #region IValueConverter Members
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sector = value as Sector;
            if (sector == null)
                return null;

            return sector.Location;
        }
        #endregion
    }
    
    [ValueConversion(typeof(Sector), typeof(int))]
    public class SectorScanStrengthConverter : AppContextAwareValueConverter
    {
        #region IValueConverter Members
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sector = value as Sector;
            if (sector == null)
                return null;

            var mapData = AppContext.LocalPlayerEmpire.MapData;
            if (mapData == null)
                return null;

            if (!mapData.IsScanned(sector.Location))
                return 0;

            return mapData.GetScanStrength(sector.Location);
        }
        #endregion
    }

    [ValueConversion(typeof(Sector), typeof(Brush))]
    public class SectorScanStrengthBrushConverter : AppContextAwareValueConverter
    {
        #region IValueConverter Members
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sector = value as Sector;
            if (sector == null)
                return null;

            var mapData = AppContext.LocalPlayerEmpire.MapData;
            if (mapData == null)
                return null;

            if (mapData.IsScanned(sector.Location))
            {
                var scanStrength = mapData.GetScanStrength(sector.Location);

                if (scanStrength > 0)
                    return Brushes.Lime;

                if (scanStrength < 0)
                    return Brushes.Crimson;
            }

            return Brushes.Yellow;
        }
        #endregion
    }

    [ValueConversion(typeof(BuildQueueItem), typeof(string))]
    public class BuildQueueItemDescriptionConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BuildQueueItem item = value as BuildQueueItem;
            if (item != null)
            {
                string description = ResourceManager.GetString(item.Project.Description);
                if ("UpperCase" == (parameter as string))
                    description = description.ToUpperInvariant();
                if (item.Count > 1)
                    return String.Format("{0}x {1}", item.Count, description);
                return description;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        #endregion
    }

    [ValueConversion(typeof(object), typeof(ImageSource))]
    public class CivInsigniaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var objectKey = "" ;

            if (value != null)
            {
                if (value is string)
                    objectKey = (string)value;
                else if (value is int || value is GameObjectID)
                    objectKey = GameContext.Current.Civilizations[(int)value].Key;
                else if (value is ICivIdentity)
                    objectKey = GameContext.Current.Civilizations[((ICivIdentity)value).CivID].Key;
                else if (value is UniverseObject)
                    objectKey = ((UniverseObject)value).Owner.Key.ToLowerInvariant();
            }

            return Convert(objectKey);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public static BitmapImage Convert(string objectKey)
        {
            Uri imageUri;
            var fileName = objectKey.ToLowerInvariant();

            if (File.Exists(ResourceManager.GetResourcePath(@"Resources\Images\Insignias\" + fileName + ".png")))
                imageUri = ResourceManager.GetResourceUri(@"Resources\Images\Insignias\" + fileName + ".png");
            else if (File.Exists(ResourceManager.GetResourcePath(@"Resources\Images\Insignias\" + fileName + ".jpg")))
                imageUri = ResourceManager.GetResourceUri(@"Resources\Images\Insignias\" + fileName + ".jpg");
            else
                imageUri = ResourceManager.GetResourceUri(@"Resources\Images\Insignias\__default.png");

            return ImageCache.Current.Get(imageUri);
        }
    }

    [ValueConversion(typeof(object), typeof(Brush))]
    public class CivBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            Civilization civ = null;

            if (value is string)
            {
                civ = GameContext.Current.Civilizations[(string)value];
            }
            else if (value is int)
            {
                civ = GameContext.Current.Civilizations[(int)value];
            }
            else if (value is Civilization)
            {
                civ = (Civilization)value;
            }
            else if (value is UniverseObject)
            {
                civ = ((UniverseObject)value).Owner;
            }

            if (civ == null)
                return null;

            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(civ.Color));
            brush.Freeze();

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(string), typeof(ImageSource))]
    public class ImageUriConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string imageUri = value as string;
            if (imageUri != null)
            {
                return ImageCache.Current.Get(ResourceManager.GetResourceUri(imageUri));
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
        #endregion
    }

    [ValueConversion(typeof(object), typeof(BitmapImage))]
    public class TechObjectImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                string imagePath;
                if (value is TechObjectDesign)
                {
                    imagePath = ((TechObjectDesign)value).Image;
                }
                else
                {
                    imagePath = value.ToString();
                    if (string.IsNullOrEmpty(Path.GetExtension(imagePath)))
                        imagePath += ".png";
                }
                return Convert(imagePath);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public static BitmapImage Convert(string imagePath)
        {
            return ImageHelper.GetImage(imagePath);
        }
    }

    [ValueConversion(typeof(string), typeof(string))]
    public class StringResourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            if (parameter != null)
                return ResourceManager.GetString(value.ToString()).ToUpperInvariant();
            return ResourceManager.GetString(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(ResearchField), typeof(BitmapImage))]
    public class ResearchFieldImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ResearchField)
            {
                return Convert(
                    ResourceManager.GetString(((ResearchField)value).Name) + ".png");
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        protected static BitmapImage Convert(string objectKey)
        {
            return ImageHelper.GetImage(@"Resources\Images\Research\Fields\" + objectKey.ToLowerInvariant());
        }
    }

    [ValueConversion(typeof(ResearchField), typeof(BindingBase))]
    public class ResearchFieldDistributionBindingConverter : AppContextAwareValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var field = value as ResearchField;
            
            if (field == null)
                return null;

            return new Binding
                   {
                       Source = AppContext.LocalPlayerEmpire.Research.Distributions[field.FieldID],
                       Path = new PropertyPath("Value"),
                       Mode = BindingMode.TwoWay
                   };
        }
    }

    [ValueConversion(typeof(string), typeof(BitmapImage))]
    public class EncyclopediaImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ImageHelper.GetImage(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
