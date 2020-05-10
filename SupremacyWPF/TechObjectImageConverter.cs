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
            {
                return null;
            }

            string result = ResourceManager.GetResourcePath(path);
            if (File.Exists(result))
            {
                return result;
            }

            result = ResourceManager.GetResourcePath(ImageMissingPath);
            return File.Exists(result) ? result : null;
        }

        public static Uri GetImageUri(string path)
        {
            if (path == null)
            {
                return null;
            }

            return Uri.TryCreate(path, UriKind.Absolute, out Uri uri) ? uri : ResourceManager.GetResourceUri(GetImagePath(path));
        }

        public static BitmapImage GetImage(string path)
        {
            if (path == null)
            {
                return null;
            }

            try
            {
                Uri imageUri = GetImageUri(path);
                return ImageCache.Current.Get(imageUri);
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
            }

            return null;
        }
    }

    [ValueConversion(typeof(Sector), typeof(string))]
    public class SectorNameConverter : AppContextAwareValueConverter
    {
        #region IValueConverter Members
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Sector sector = value as Sector;
            return sector == null ? null : (object)sector.Location;
        }
        #endregion
    }
    
    [ValueConversion(typeof(Sector), typeof(int))]
    public class SectorScanStrengthConverter : AppContextAwareValueConverter
    {
        #region IValueConverter Members
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Sector sector = value as Sector;
            if (sector == null)
            {
                return null;
            }

            CivilizationMapData mapData = AppContext.LocalPlayerEmpire.MapData;
            if (mapData == null)
            {
                return null;
            }

            return !mapData.IsScanned(sector.Location) ? 0 : (object)mapData.GetScanStrength(sector.Location);
        }
        #endregion
    }

    [ValueConversion(typeof(Sector), typeof(Brush))]
    public class SectorScanStrengthBrushConverter : AppContextAwareValueConverter
    {
        #region IValueConverter Members
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Sector sector = value as Sector;
            if (sector == null)
            {
                return null;
            }

            CivilizationMapData mapData = AppContext.LocalPlayerEmpire.MapData;
            if (mapData == null)
            {
                return null;
            }

            if (mapData.IsScanned(sector.Location))
            {
                int scanStrength = mapData.GetScanStrength(sector.Location);

                if (scanStrength > 0)
                {
                    return Brushes.Lime;
                }

                if (scanStrength < 0)
                {
                    return Brushes.Crimson;
                }
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
            if (value is BuildQueueItem item)
            {
                string description = ResourceManager.GetString(item.Project.Description);
                if ("UpperCase" == (parameter as string))
                {
                    description = description.ToUpperInvariant();
                }

                return item.Count > 1 ? string.Format("{0}x {1}", item.Count, description) : description;
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
            string objectKey = "";

            if (value != null)
            {
                if (value is string)
                {
                    objectKey = (string)value;
                }
                else if (value is int)
                {
                    objectKey = GameContext.Current.Civilizations[(int)value].Key;
                }
                else if (value is ICivIdentity)
                {
                    objectKey = GameContext.Current.Civilizations[((ICivIdentity)value).CivID].Key;
                }
                else if (value is UniverseObject)
                {
                    objectKey = ((UniverseObject)value).Owner.Key.ToLowerInvariant();
                }
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
            string fileName = objectKey.ToLowerInvariant();

            imageUri = File.Exists(ResourceManager.GetResourcePath(@"Resources\Images\Insignias\" + fileName + ".png"))
                ? ResourceManager.GetResourceUri(@"Resources\Images\Insignias\" + fileName + ".png")
                : File.Exists(ResourceManager.GetResourcePath(@"Resources\Images\Insignias\" + fileName + ".jpg"))
                ? ResourceManager.GetResourceUri(@"Resources\Images\Insignias\" + fileName + ".jpg")
                : ResourceManager.GetResourceUri(@"Resources\Images\Insignias\__default.png");

            return ImageCache.Current.Get(imageUri);
        }
    }

    [ValueConversion(typeof(object), typeof(Brush))]
    public class CivBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

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
            {
                return null;
            }

            SolidColorBrush brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(civ.Color));
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
            return value is string imageUri ? ImageCache.Current.Get(ResourceManager.GetResourceUri(imageUri)) : value;
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
                    {
                        imagePath += ".png";
                    }
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
            // works - Text is coming out of en.txt and maybe other files from \Data-folder
            //GameLog.Core.General.DebugFormat("StringResourceConverter: value = {0}, targetType = {1}, parameter = {2}, culture = {3}", 
            //    value, targetType, parameter, culture);
            if (value == null)
            {
                return null;
            }

            if (parameter != null)
            {
                return ResourceManager.GetString(value.ToString()).ToUpperInvariant();
            }
            //GameLog.Core.General.DebugFormat("StringResourceConverter: returning = {0}",
            //    ResourceManager.GetString(value.ToString()));
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
            return value is ResearchField
                ? Convert(
                    ResourceManager.GetString(((ResearchField)value).Name) + ".png")
                : null;
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
            ResearchField field = value as ResearchField;

            return field == null
                ? null
                : new Binding
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
