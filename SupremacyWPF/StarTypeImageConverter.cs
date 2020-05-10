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
            string resourcePath = string.Format("Resources/Images/Stars/Map/{0}.png", starType);
            VFS.IVirtualFileInfo resourceFile = ResourceManager.VfsService.GetFile(resourcePath);
            return resourceFile != null && resourceFile.Exists ? ImageCache.Current.Get(ResourceManager.GetResourceUri(resourcePath)) : null;
        }

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            StarType? starType = value as StarType?;
            return starType.HasValue ? Convert(starType.Value) : null;
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