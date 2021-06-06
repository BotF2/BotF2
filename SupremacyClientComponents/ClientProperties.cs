using System;
using System.Windows;

namespace Supremacy.Client
{
    public class ClientProperties
    {
        #region ScaleFactor Attached Property

        public static readonly DependencyProperty ScaleFactorProperty = DependencyProperty.RegisterAttached(
            "ScaleFactor",
            typeof(double),
            typeof(ClientProperties),
            new FrameworkPropertyMetadata(
                1.0,
                FrameworkPropertyMetadataOptions.Inherits));

        public static double GetScaleFactor(UIElement source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return (double)source.GetValue(ScaleFactorProperty);
        }

        public static void SetScaleFactor(UIElement source, double value)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            source.SetValue(ScaleFactorProperty, value);
        }

        #endregion
    }
}