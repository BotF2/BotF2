using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Supremacy.Client.Themes
{
    public static class ThemeProperties
    {
        #region Dependency Properties

        public static readonly DependencyProperty BackgroundHoverProperty = DependencyProperty.RegisterAttached(
            "BackgroundHover",
            typeof(Brush),
            typeof(ThemeProperties),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty BackgroundNormalProperty = DependencyProperty.RegisterAttached(
            "BackgroundNormal",
            typeof(Brush),
            typeof(ThemeProperties),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty BackgroundPressedProperty = DependencyProperty.RegisterAttached(
            "BackgroundPressed",
            typeof(Brush),
            typeof(ThemeProperties),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached(
            "CornerRadius",
            typeof(CornerRadius),
            typeof(ThemeProperties),
            new FrameworkPropertyMetadata(new CornerRadius(0)));

        public static readonly DependencyProperty DecorationDarkProperty = DependencyProperty.RegisterAttached(
            "DecorationDark",
            typeof(Brush),
            typeof(ThemeProperties),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty DecorationLightProperty = DependencyProperty.RegisterAttached(
            "DecorationLight",
            typeof(Brush),
            typeof(ThemeProperties),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty InnerBorderHoverProperty = DependencyProperty.RegisterAttached(
            "InnerBorderHover",
            typeof(Brush),
            typeof(ThemeProperties),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty InnerBorderNormalProperty = DependencyProperty.RegisterAttached(
            "InnerBorderNormal",
            typeof(Brush),
            typeof(ThemeProperties),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty InnerBorderPressedProperty = DependencyProperty.RegisterAttached(
            "InnerBorderPressed",
            typeof(Brush),
            typeof(ThemeProperties),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.RegisterAttached(
            "Orientation",
            typeof(Orientation),
            typeof(ThemeProperties),
            new FrameworkPropertyMetadata(Orientation.Horizontal));

        public static readonly DependencyProperty OuterBorderHoverProperty = DependencyProperty.RegisterAttached(
            "OuterBorderHover",
            typeof(Brush),
            typeof(ThemeProperties),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty OuterBorderNormalProperty = DependencyProperty.RegisterAttached(
            "OuterBorderNormal",
            typeof(Brush),
            typeof(ThemeProperties),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty OuterBorderPressedProperty = DependencyProperty.RegisterAttached(
            "OuterBorderPressed",
            typeof(Brush),
            typeof(ThemeProperties),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.RegisterAttached(
            "TextTrimming",
            typeof(TextTrimming),
            typeof(ThemeProperties),
            new FrameworkPropertyMetadata(TextBlock.TextTrimmingProperty.DefaultMetadata.DefaultValue));

        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.RegisterAttached(
            "TextWrapping",
            typeof(TextWrapping),
            typeof(ThemeProperties),
            new FrameworkPropertyMetadata(TextBlock.TextWrappingProperty.DefaultMetadata.DefaultValue));

        #endregion

        public static Brush GetBackgroundHover(DependencyObject target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            return (Brush)target.GetValue(BackgroundHoverProperty);
        }

        public static void SetBackgroundHover(DependencyObject target, Brush value)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            target.SetValue(BackgroundHoverProperty, value);
        }

        public static Brush GetBackgroundNormal(DependencyObject target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            return (Brush)target.GetValue(BackgroundNormalProperty);
        }

        public static void SetBackgroundNormal(DependencyObject target, Brush value)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            target.SetValue(BackgroundNormalProperty, value);
        }

        public static Brush GetBackgroundPressed(DependencyObject target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            return (Brush)target.GetValue(BackgroundPressedProperty);
        }

        public static void SetBackgroundPressed(DependencyObject target, Brush value)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            target.SetValue(BackgroundPressedProperty, value);
        }

        public static CornerRadius GetCornerRadius(DependencyObject target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            return (CornerRadius)target.GetValue(CornerRadiusProperty);
        }

        public static void SetCornerRadius(DependencyObject target, CornerRadius value)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            target.SetValue(CornerRadiusProperty, value);
        }

        public static Brush GetDecorationDark(DependencyObject target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            return (Brush)target.GetValue(DecorationDarkProperty);
        }

        public static void SetDecorationDark(DependencyObject target, Brush value)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            target.SetValue(DecorationDarkProperty, value);
        }

        public static Brush GetDecorationLight(DependencyObject target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            return (Brush)target.GetValue(DecorationLightProperty);
        }

        public static void SetDecorationLight(DependencyObject target, Brush value)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            target.SetValue(DecorationLightProperty, value);
        }

        public static Brush GetInnerBorderHover(DependencyObject target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            return (Brush)target.GetValue(InnerBorderHoverProperty);
        }

        public static void SetInnerBorderHover(DependencyObject target, Brush value)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            target.SetValue(InnerBorderHoverProperty, value);
        }

        public static Brush GetInnerBorderNormal(DependencyObject target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            return (Brush)target.GetValue(InnerBorderNormalProperty);
        }

        public static void SetInnerBorderNormal(DependencyObject target, Brush value)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            target.SetValue(InnerBorderNormalProperty, value);
        }

        public static Brush GetInnerBorderPressed(DependencyObject target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            return (Brush)target.GetValue(InnerBorderPressedProperty);
        }

        public static void SetInnerBorderPressed(DependencyObject target, Brush value)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            target.SetValue(InnerBorderPressedProperty, value);
        }

        public static Orientation GetOrientation(DependencyObject target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            return (Orientation)target.GetValue(OrientationProperty);
        }

        public static void SetOrientation(DependencyObject target, Orientation value)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            target.SetValue(OrientationProperty, value);
        }

        public static Brush GetOuterBorderHover(DependencyObject target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            return (Brush)target.GetValue(OuterBorderHoverProperty);
        }

        public static void SetOuterBorderHover(DependencyObject target, Brush value)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            target.SetValue(OuterBorderHoverProperty, value);
        }

        public static Brush GetOuterBorderNormal(DependencyObject target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            return (Brush)target.GetValue(OuterBorderNormalProperty);
        }

        public static void SetOuterBorderNormal(DependencyObject target, Brush value)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            target.SetValue(OuterBorderNormalProperty, value);
        }

        public static Brush GetOuterBorderPressed(DependencyObject target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            return (Brush)target.GetValue(OuterBorderPressedProperty);
        }

        public static void SetOuterBorderPressed(DependencyObject target, Brush value)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            target.SetValue(OuterBorderPressedProperty, value);
        }

        public static TextTrimming GetTextTrimming(DependencyObject target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            return (TextTrimming)target.GetValue(TextTrimmingProperty);
        }

        public static void SetTextTrimming(DependencyObject target, TextTrimming value)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            target.SetValue(TextTrimmingProperty, value);
        }

        public static TextWrapping GetTextWrapping(DependencyObject target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            return (TextWrapping)target.GetValue(TextWrappingProperty);
        }

        public static void SetTextWrapping(DependencyObject target, TextWrapping value)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            target.SetValue(TextWrappingProperty, value);
        }

    }
}