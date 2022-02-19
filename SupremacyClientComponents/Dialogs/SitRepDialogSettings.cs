using Supremacy.Game;
using System;
using System.Windows;

namespace Supremacy.Client.Dialogs
{
    public static class SitRepDialogSettings
    {
        #region ShowGreenItems Attached Property

        public static readonly DependencyProperty ShowGreenItemsProperty = DependencyProperty.RegisterAttached(
            "ShowGreenItems",
            typeof(bool),
            typeof(SitRepDialogSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public static bool GetShowGreenItems(DependencyObject source)
        {
            return source == null ? throw new ArgumentNullException("source") : (bool)source.GetValue(ShowGreenItemsProperty);
        }

        public static void SetShowGreenItems(DependencyObject source, bool value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            source.SetValue(ShowGreenItemsProperty, value);
        }

        #endregion ShowGreenItems Attached Property

        #region ShowOrangeItems Attached Property

        public static readonly DependencyProperty ShowOrangeItemsProperty = DependencyProperty.RegisterAttached(
            "ShowOrangeItems",
            typeof(bool),
            typeof(SitRepDialogSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public static bool GetShowOrangeItems(DependencyObject source)
        {
            return source == null ? throw new ArgumentNullException("source") : (bool)source.GetValue(ShowOrangeItemsProperty);
        }

        public static void SetShowOrangeItems(DependencyObject source, bool value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            source.SetValue(ShowOrangeItemsProperty, value);
        }

        #endregion ShowOrangeItems Attached Property

        #region ShowRedItems Attached Property

        public static readonly DependencyProperty ShowRedItemsProperty = DependencyProperty.RegisterAttached(
            "ShowRedItems",
            typeof(bool),
            typeof(SitRepDialogSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public static bool GetShowRedItems(DependencyObject source)
        {
            return source == null ? throw new ArgumentNullException("source") : (bool)source.GetValue(ShowRedItemsProperty);
        }

        public static void SetShowRedItems(DependencyObject source, bool value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            source.SetValue(ShowRedItemsProperty, value);
        }

        #endregion

        #region ShowBlueItems Attached Property

        public static readonly DependencyProperty ShowBlueItemsProperty = DependencyProperty.RegisterAttached(
            "ShowBlueItems",
            typeof(bool),
            typeof(SitRepDialogSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public static bool GetShowBlueItems(DependencyObject source)
        {
            return source == null ? throw new ArgumentNullException("source") : (bool)source.GetValue(ShowBlueItemsProperty);
        }

        public static void SetShowBlueItems(DependencyObject source, bool value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            source.SetValue(ShowBlueItemsProperty, value);
        }

        #endregion ShowBlueItems Attached Property

        #region ShowGrayItems Attached Property

        public static readonly DependencyProperty ShowGrayItemsProperty = DependencyProperty.RegisterAttached(
            "ShowGrayItems",
            typeof(bool),
            typeof(SitRepDialogSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public static bool GetShowGrayItems(DependencyObject source)
        {
            return source == null ? throw new ArgumentNullException("source") : (bool)source.GetValue(ShowGrayItemsProperty);
        }

        public static void SetShowGrayItems(DependencyObject source, bool value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            source.SetValue(ShowGrayItemsProperty, value);
        }

        #endregion ShowGrayItems Attached Property

        #region ShowPurpleItems Attached Property

        public static readonly DependencyProperty ShowPurpleItemsProperty = DependencyProperty.RegisterAttached(
            "ShowPurpleItems",
            typeof(bool),
            typeof(SitRepDialogSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public static bool GetShowPurpleItems(DependencyObject source)
        {
            return source == null ? throw new ArgumentNullException("source") : (bool)source.GetValue(ShowPurpleItemsProperty);
        }

        public static void SetShowPurpleItems(DependencyObject source, bool value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            source.SetValue(ShowPurpleItemsProperty, value);
        }

        #endregion ShowPurpleItems Attached Property

        #region ShowYellowItems Attached Property

        public static readonly DependencyProperty ShowYellowItemsProperty = DependencyProperty.RegisterAttached(
            "ShowYellowItems",
            typeof(bool),
            typeof(SitRepDialogSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public static bool GetShowYellowItems(DependencyObject source)
        {
            return source == null ? throw new ArgumentNullException("source") : (bool)source.GetValue(ShowYellowItemsProperty);
        }

        public static void SetShowYellowItems(DependencyObject source, bool value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            source.SetValue(ShowYellowItemsProperty, value);
        }

        #endregion ShowYellowItems Attached Property

        #region ShowCrimsonItems Attached Property

        public static readonly DependencyProperty ShowCrimsonItemsProperty = DependencyProperty.RegisterAttached(
            "ShowCrimsonItems",
            typeof(bool),
            typeof(SitRepDialogSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public static bool GetShowCrimsonItems(DependencyObject source)
        {
            return source == null ? throw new ArgumentNullException("source") : (bool)source.GetValue(ShowCrimsonItemsProperty);
        }

        public static void SetShowCrimsonItems(DependencyObject source, bool value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            source.SetValue(ShowCrimsonItemsProperty, value);
        }

        #endregion ShowCrimsonItems Attached Property

        #region ShowPinkItems Attached Property

        public static readonly DependencyProperty ShowPinkItemsProperty = DependencyProperty.RegisterAttached(
            "ShowPinkItems",
            typeof(bool),
            typeof(SitRepDialogSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public static bool GetShowPinkItems(DependencyObject source)
        {
            return source == null ? throw new ArgumentNullException("source") : (bool)source.GetValue(ShowPinkItemsProperty);
        }

        public static void SetShowPinkItems(DependencyObject source, bool value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            source.SetValue(ShowPinkItemsProperty, value);
        }

        #endregion ShowPinkItems Attached Property

        #region ShowBrownItems Attached Property

        public static readonly DependencyProperty ShowBrownItemsProperty = DependencyProperty.RegisterAttached(
            "ShowBrownItems",
            typeof(bool),
            typeof(SitRepDialogSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public static bool GetShowBrownItems(DependencyObject source)
        {
            return source == null ? throw new ArgumentNullException("source") : (bool)source.GetValue(ShowBrownItemsProperty);
        }

        public static void SetShowBrownItems(DependencyObject source, bool value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            source.SetValue(ShowBrownItemsProperty, value);
        }

        #endregion ShowBrownItems Attached Property

        #region ShowYellowRedItems Attached Property

        public static readonly DependencyProperty ShowYellowRedItemsProperty = DependencyProperty.RegisterAttached(
            "ShowYellowRedItems",
            typeof(bool),
            typeof(SitRepDialogSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public static bool GetShowYellowRedItems(DependencyObject source)
        {
            return source == null ? throw new ArgumentNullException("source") : (bool)source.GetValue(ShowYellowRedItemsProperty);
        }

        public static void SetShowYellowRedItems(DependencyObject source, bool value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            source.SetValue(ShowYellowRedItemsProperty, value);
        }

        #endregion ShowYellowRedItems Attached Property

        #region ShowBlueDarkItems Attached Property

        public static readonly DependencyProperty ShowBlueDarkItemsProperty = DependencyProperty.RegisterAttached(
            "ShowBlueDarkItems",
            typeof(bool),
            typeof(SitRepDialogSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public static bool GetShowBlueDarkItems(DependencyObject source)
        {
            return source == null ? throw new ArgumentNullException("source") : (bool)source.GetValue(ShowBlueDarkItemsProperty);
        }

        public static void SetShowBlueDarkItems(DependencyObject source, bool value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            source.SetValue(ShowBlueDarkItemsProperty, value);
        }

        #endregion ShowBlueDarkItems Attached Property

        #region ShowAquaItems Attached Property

        public static readonly DependencyProperty ShowAquaItemsProperty = DependencyProperty.RegisterAttached(
            "ShowAquaItems",
            typeof(bool),
            typeof(SitRepDialogSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public static bool GetShowAquaItems(DependencyObject source)
        {
            return source == null ? throw new ArgumentNullException("source") : (bool)source.GetValue(ShowAquaItemsProperty);
        }

        public static void SetShowAquaItems(DependencyObject source, bool value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            source.SetValue(ShowAquaItemsProperty, value);
        }

        #endregion ShowAquaItems Attached Property

        #region VisibleCategories Attached Property

        public static readonly DependencyProperty VisibleCategoriesProperty = DependencyProperty.RegisterAttached(
            "VisibleCategories",
            typeof(SitRepCategory),
            typeof(SitRepDialogSettings),
            new FrameworkPropertyMetadata(
                SitRepCategory.NewColony |
                SitRepCategory.ColonyStatus |
                SitRepCategory.Construction |
                SitRepCategory.Diplomacy |
                SitRepCategory.FirstContact |
                SitRepCategory.General |
                SitRepCategory.Intelligence |
                SitRepCategory.NewInfiltrate |
                SitRepCategory.Military |
                SitRepCategory.Research |
                SitRepCategory.Resources |
                SitRepCategory.SpecialEvent,
                FrameworkPropertyMetadataOptions.None));

        public static SitRepCategory GetVisibleCategories(DependencyObject source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return (SitRepCategory)source.GetValue(VisibleCategoriesProperty);
        }

        public static void SetVisibleCategories(DependencyObject source, SitRepCategory value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            source.SetValue(VisibleCategoriesProperty, value);
        }

        #endregion
    }
}