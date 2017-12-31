using System;
using System.Windows;

using Supremacy.Game;

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
            if (source == null)
                throw new ArgumentNullException("source");
            return (bool)source.GetValue(ShowGreenItemsProperty);
        }

        public static void SetShowGreenItems(DependencyObject source, bool value)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            source.SetValue(ShowGreenItemsProperty, value);
        }

        #endregion

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
            if (source == null)
                throw new ArgumentNullException("source");
            return (bool)source.GetValue(ShowOrangeItemsProperty);
        }

        public static void SetShowOrangeItems(DependencyObject source, bool value)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            source.SetValue(ShowOrangeItemsProperty, value);
        }

        #endregion

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
            if (source == null)
                throw new ArgumentNullException("source");
            return (bool)source.GetValue(ShowRedItemsProperty);
        }

        public static void SetShowRedItems(DependencyObject source, bool value)
        {
            if (source == null)
                throw new ArgumentNullException("source");
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
            if (source == null)
                throw new ArgumentNullException("source");
            return (bool)source.GetValue(ShowBlueItemsProperty);
        }

        public static void SetShowBlueItems(DependencyObject source, bool value)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            source.SetValue(ShowBlueItemsProperty, value);
        }

        #endregion

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
                SitRepCategory.Military |
                SitRepCategory.Research |
                SitRepCategory.Resources |
                SitRepCategory.SpecialEvent |
                SitRepCategory.Personnel,
                FrameworkPropertyMetadataOptions.None));

        public static SitRepCategory GetVisibleCategories(DependencyObject source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return (SitRepCategory)source.GetValue(VisibleCategoriesProperty);
        }

        public static void SetVisibleCategories(DependencyObject source, SitRepCategory value)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            source.SetValue(VisibleCategoriesProperty, value);
        }

        #endregion
    }
}