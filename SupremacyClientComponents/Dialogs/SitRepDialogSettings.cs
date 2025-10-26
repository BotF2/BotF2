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


        #region ShowGreenDarkItems Attached Property

        public static readonly DependencyProperty ShowGreenDarkItemsProperty = DependencyProperty.RegisterAttached(
            "ShowGreenDarkItems",
            typeof(bool),
            typeof(SitRepDialogSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public static bool GetShowGreenDarkItems(DependencyObject source)
        {
            return source == null ? throw new ArgumentNullException("source") : (bool)source.GetValue(ShowGreenDarkItemsProperty);
        }

        public static void SetShowGreenDarkItems(DependencyObject source, bool value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            source.SetValue(ShowGreenDarkItemsProperty, value);
        }

        #endregion ShowGreenDarkItems Attached Property


        #region ShowGreenDark2Items Attached Property

        public static readonly DependencyProperty ShowGreenDark2ItemsProperty = DependencyProperty.RegisterAttached(
            "ShowGreenDark2Items",
            typeof(bool),
            typeof(SitRepDialogSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public static bool GetShowGreenDark2Items(DependencyObject source)
        {
            return source == null ? throw new ArgumentNullException("source") : (bool)source.GetValue(ShowGreenDark2ItemsProperty);
        }

        public static void SetShowGreenDark2Items(DependencyObject source, bool value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            source.SetValue(ShowGreenDark2ItemsProperty, value);
        }

        #endregion ShowGreenDark2Items Attached Property

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

            source.SetValue(ShowGrayDarkItemsProperty, value);
        }

        #endregion ShowGrayItems Attached Property


        #region ShowGrayDarkItems Attached Property

        public static readonly DependencyProperty ShowGrayDarkItemsProperty = DependencyProperty.RegisterAttached(
            "ShowGrayDarkItems",
            typeof(bool),
            typeof(SitRepDialogSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public static bool GetShowGrayDarkItems(DependencyObject source)
        {
            return source == null ? throw new ArgumentNullException("source") : (bool)source.GetValue(ShowGrayDarkItemsProperty);
        }

        public static void SetShowGrayDarkItems(DependencyObject source, bool value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            source.SetValue(ShowGrayDarkItemsProperty, value);
        }

        #endregion ShowGrayDarkItems Attached Property

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

        #region ShowBlue2Items Attached Property

        public static readonly DependencyProperty ShowBlue2ItemsProperty = DependencyProperty.RegisterAttached(
            "ShowBlue2Items",
            typeof(bool),
            typeof(SitRepDialogSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public static bool GetShowBlue2Items(DependencyObject source)
        {
            return source == null ? throw new ArgumentNullException("source") : (bool)source.GetValue(ShowBlue2ItemsProperty);
        }

        public static void SetShowBlue2Items(DependencyObject source, bool value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            source.SetValue(ShowBlue2ItemsProperty, value);
        }

        #endregion ShowBlue2Items Attached Property


        #region ShowDilithiumItems Attached Property

        public static readonly DependencyProperty ShowDilithiumItemsProperty = DependencyProperty.RegisterAttached(
            "ShowDilithiumItems",
            typeof(bool),
            typeof(SitRepDialogSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public static bool GetShowDilithiumItems(DependencyObject source)
        {
            return source == null ? throw new ArgumentNullException("source") : (bool)source.GetValue(ShowDilithiumItemsProperty);
        }

        public static void SetShowDilithiumItems(DependencyObject source, bool value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            source.SetValue(ShowDilithiumItemsProperty, value);
        }

        #endregion ShowDilithiumItems Attached Property


        #region ShowDeuteriumItems Attached Property

        public static readonly DependencyProperty ShowDeuteriumItemsProperty = DependencyProperty.RegisterAttached(
            "ShowDeuteriumItems",
            typeof(bool),
            typeof(SitRepDialogSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public static bool GetShowDeuteriumItems(DependencyObject source)
        {
            return source == null ? throw new ArgumentNullException("source") : (bool)source.GetValue(ShowDeuteriumItemsProperty);
        }

        public static void SetShowDeuteriumItems(DependencyObject source, bool value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            source.SetValue(ShowDeuteriumItemsProperty, value);
        }

        #endregion ShowDeuteriumItems Attached Property

        #region ShowDuraniumItems Attached Property

        public static readonly DependencyProperty ShowDuraniumItemsProperty = DependencyProperty.RegisterAttached(
            "ShowDuraniumItems",
            typeof(bool),
            typeof(SitRepDialogSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public static bool GetShowDuraniumItems(DependencyObject source)
        {
            return source == null ? throw new ArgumentNullException("source") : (bool)source.GetValue(ShowDuraniumItemsProperty);
        }

        public static void SetShowDuraniumItems(DependencyObject source, bool value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            source.SetValue(ShowDuraniumItemsProperty, value);
        }

        #endregion ShowDilithiumItems Attached Property


        #region ShowCreditsItems Attached Property

        public static readonly DependencyProperty ShowCreditsItemsProperty = DependencyProperty.RegisterAttached(
            "ShowCreditsItems",
            typeof(bool),
            typeof(SitRepDialogSettings),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));

        public static bool GetShowCreditsItems(DependencyObject source)
        {
            return source == null ? throw new ArgumentNullException("source") : (bool)source.GetValue(ShowCreditsItemsProperty);
        }

        public static void SetShowCreditsItems(DependencyObject source, bool value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            source.SetValue(ShowCreditsItemsProperty, value);
        }

        #endregion ShowCreditsItems Attached Property

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