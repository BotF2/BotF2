using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

using Supremacy.Resources;
using Supremacy.Universe;

namespace Supremacy.Client
{
    public class StarTypeImageConverter : IValueConverter
    {
        public static readonly StarTypeImageConverter Instance = new StarTypeImageConverter();

        public ImageSource Convert(StarType starType)
        {
            var resourcePath = string.Format("Resources/Images/Stars/Map/{0}.png", starType);
            var resourceFile = ResourceManager.VfsService.GetFile(resourcePath);
            if (resourceFile != null && resourceFile.Exists)
                return ImageCache.Current.Get(ResourceManager.GetResourceUri(resourcePath));

            return null;
        }

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var starType = value as StarType?;
            if (starType.HasValue)
                return Convert(starType.Value);
            return null;
        }

        #endregion

        #region Implementation of IValueConverter

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}