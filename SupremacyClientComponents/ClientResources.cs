// ClientResources.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Windows;

using Supremacy.Annotations;

namespace Supremacy.Client
{
    public static class ClientResources
    {
        #region HeaderFontFamily Resource Key

        private static ComponentResourceKey _headerFontFamilyKey;

        public static ComponentResourceKey HeaderFontFamilyKey
        {
            get
            {
                EnsureResource("HeaderFontFamily", ref _headerFontFamilyKey);
                return _headerFontFamilyKey;
            }
        }

        #endregion

        #region DefaultFontFamily Resource Key

        private static ComponentResourceKey _defaultFontFamilyKey;

        public static ComponentResourceKey DefaultFontFamilyKey
        {
            get
            {
                EnsureResource("DefaultFontFamily", ref _defaultFontFamilyKey);
                return _defaultFontFamilyKey;
            }
        }

        #endregion

        #region InfoPaneFontFamily Resource Key

        private static ComponentResourceKey _infoPaneFontFamilyKey;

        public static ComponentResourceKey InfoPaneFontFamilyKey
        {
            get
            {
                EnsureResource("InfoPaneFontFamily", ref _infoPaneFontFamilyKey);
                return _infoPaneFontFamilyKey;
            }
        }

        #endregion

        #region HeaderFontSize Resource Key

        private static ComponentResourceKey _headerFontSizeKey;

        public static ComponentResourceKey HeaderFontSizeKey
        {
            get
            {
                EnsureResource("HeaderFontSize", ref _headerFontSizeKey);
                return _headerFontSizeKey;
            }
        }

        #endregion

        #region DefaultFontSize Resource Key

        private static ComponentResourceKey _defaultFontSizeKey;

        public static ComponentResourceKey DefaultFontSizeKey
        {
            get
            {
                EnsureResource("DefaultFontSize", ref _defaultFontSizeKey);
                return _defaultFontSizeKey;
            }
        }

        #endregion

        #region InfoPaneFontSize Resource Key

        private static ComponentResourceKey _infoPaneFontSizeKey;

        public static ComponentResourceKey InfoPaneFontSizeKey
        {
            get
            {
                EnsureResource("InfoPaneFontSize", ref _infoPaneFontSizeKey);
                return _infoPaneFontSizeKey;
            }
        }

        #endregion

        #region HeaderFontWeight Resource Key

        private static ComponentResourceKey _headerFontWeightKey;

        public static ComponentResourceKey HeaderFontWeightKey
        {
            get
            {
                EnsureResource("HeaderFontWeight", ref _headerFontWeightKey);
                return _headerFontWeightKey;
            }
        }

        #endregion

        #region DefaultFontWeight Resource Key

        private static ComponentResourceKey _defaultFontWeightKey;

        public static ComponentResourceKey DefaultFontWeightKey
        {
            get
            {
                EnsureResource("DefaultFontWeight", ref _defaultFontWeightKey);
                return _defaultFontWeightKey;
            }
        }

        #endregion

        #region InfoPaneFontWeight Resource Key

        private static ComponentResourceKey _infoPaneFontWeightKey;

        public static ComponentResourceKey InfoPaneFontWeightKey
        {
            get
            {
                EnsureResource("InfoPaneFontWeight", ref _infoPaneFontWeightKey);
                return _infoPaneFontWeightKey;
            }
        }

        #endregion

        #region ControlTextForegroundBrush Resource Key

        private static ComponentResourceKey _controlTextForegroundBrushKey;

        public static ComponentResourceKey ControlTextForegroundBrushKey
        {
            get
            {
                EnsureResource("ControlTextForegroundBrush", ref _controlTextForegroundBrushKey);
                return _controlTextForegroundBrushKey;
            }
        }

        #endregion

        #region HeaderTextForegroundBrush Resource Key

        private static ComponentResourceKey _headerTextForegroundBrushKey;

        public static ComponentResourceKey HeaderTextForegroundBrushKey
        {
            get
            {
                EnsureResource("HeaderTextForegroundBrush", ref _headerTextForegroundBrushKey);
                return _headerTextForegroundBrushKey;
            }
        }

        #endregion

        #region DefaultTextForegroundBrush Resource Key

        private static ComponentResourceKey _defaultTextForegroundBrushKey;

        public static ComponentResourceKey DefaultTextForegroundBrushKey
        {
            get
            {
                EnsureResource("DefaultTextForegroundBrush", ref _defaultTextForegroundBrushKey);
                return _defaultTextForegroundBrushKey;
            }
        }

        #endregion

        #region DisabledTextForegroundBrush Resource Key

        private static ComponentResourceKey _disabledTextForegroundBrushKey;

        public static ComponentResourceKey DisabledTextForegroundBrushKey
        {
            get
            {
                EnsureResource("DisabledTextForegroundBrush", ref _disabledTextForegroundBrushKey);
                return _disabledTextForegroundBrushKey;
            }
        }

        #endregion

        //#region ButtonBackgroundBrush Resource Key   // doesn't work

        //private static ComponentResourceKey _buttonBackgroundBrush;

        //public static ComponentResourceKey ButtonBackgroundBrush
        //{
        //    get
        //    {
        //        EnsureResource("ButtonBackgroundBrush", ref _buttonBackgroundBrush);
        //        return _buttonBackgroundBrush;
        //    }
        //}

        //#endregion ButtonBackgroundBrush Resource Key

        #region AlertTextForegroundBrush Resource Key

        private static ComponentResourceKey _alertTextForegroundBrushKey;

        public static ComponentResourceKey AlertTextForegroundBrushKey
        {
            get
            {
                EnsureResource("AlertTextForegroundBrush", ref _alertTextForegroundBrushKey);
                return _alertTextForegroundBrushKey;
            }
        }

        #endregion AlertTextForegroundBrush Resource Key

        #region HighlightBrush Resource Key

        private static ComponentResourceKey _highlightBrushKey;

        public static ComponentResourceKey HighlightBrushKey
        {
            get
            {
                EnsureResource("HighlightBrush", ref _highlightBrushKey);
                return _highlightBrushKey;
            }
        }

        #endregion

        #region InactiveHighlightBrush Resource Key

        private static ComponentResourceKey _inactiveHighlightBrushKey;

        public static ComponentResourceKey InactiveHighlightBrushKey
        {
            get
            {
                EnsureResource("InactiveHighlightBrush", ref _inactiveHighlightBrushKey);
                return _inactiveHighlightBrushKey;
            }
        }

        #endregion

        #region HorizontalSeparatorBackgroundBrush Resource Key

        private static ComponentResourceKey _Horizontal_Left_Right_Brush;

        public static ComponentResourceKey Horizontal_Left_Right_Brush
        {
            get
            {
                EnsureResource("HorizontalSeparatorBackgroundBrush", ref _Horizontal_Left_Right_Brush);
                return _Horizontal_Left_Right_Brush;
            }
        }

        #endregion

        #region HorizontalRightLeftSeparatorBackgroundBrush Resource Key

        private static ComponentResourceKey _Horizontal_Right_Left_Brush;

        public static ComponentResourceKey Horizontal_Right_Left_Brush
        {
            get
            {
                EnsureResource("HorizontalRightLeftSeparatorBackgroundBrush", ref _Horizontal_Right_Left_Brush);
                return _Horizontal_Right_Left_Brush;
            }
        }

        #endregion

        #region VerticalSeparatorBackgroundBrush Resource Key

        private static ComponentResourceKey _Vertical_Top_Bottom_Brush;

        public static ComponentResourceKey Vertical_Top_Bottom_Brush
        {
            get
            {
                EnsureResource("VerticalSeparatorBackgroundBrush", ref _Vertical_Top_Bottom_Brush);
                return _Vertical_Top_Bottom_Brush;
            }
        }

        #endregion

        #region VerticalBottomUpSeparatorBackgroundBrush Resource Key

        private static ComponentResourceKey _Vertical_Bottom_Top_Brush;

        public static ComponentResourceKey Vertical_Bottom_Top_Brush
        {
            get
            {
                EnsureResource("VerticalBottomUpSeparatorBackgroundBrush", ref _Vertical_Bottom_Top_Brush);
                return _Vertical_Bottom_Top_Brush;
            }
        }

        #endregion

        #region MessageDialogButtonStyle Resource Key

        private static ComponentResourceKey _messageDialogButtonStyleKey;

        public static ComponentResourceKey MessageDialogButtonStyleKey
        {
            get
            {
                EnsureResource("MessageDialogButtonStyle", ref _messageDialogButtonStyleKey);
                return _messageDialogButtonStyleKey;
            }
        }

        #endregion

        #region DialogContainerStyle Resource Key

        private static ComponentResourceKey _dialogContainerStyleKey;

        public static ComponentResourceKey DialogContainerStyleKey
        {
            get
            {
                EnsureResource("DialogContainerStyle", ref _dialogContainerStyleKey);
                return _dialogContainerStyleKey;
            }
        }

        #endregion

        #region ScrollableItemsControlStyle Resource Key

        private static ComponentResourceKey _scrollableItemsControlStyleKey;

        public static ComponentResourceKey ScrollableItemsControlStyleKey
        {
            get
            {
                EnsureResource("ScrollableItemsControlStyle", ref _scrollableItemsControlStyleKey);
                return _scrollableItemsControlStyleKey;
            }
        }

        #endregion

        #region ImageBorderBrush Resource Key

        private static ComponentResourceKey _imageBorderBrushKey;

        public static ComponentResourceKey ImageBorderBrushKey
        {
            get
            {
                EnsureResource("ImageBorderBrush", ref _imageBorderBrushKey);
                return _imageBorderBrushKey;
            }
        }

        #endregion

        #region ControlDisabledBorderBrush Resource Key

        private static ComponentResourceKey _controlDisabledBorderBrushKey;

        public static ComponentResourceKey ControlDisabledBorderBrushKey
        {
            get
            {
                EnsureResource("ControlDisabledBorderBrush", ref _controlDisabledBorderBrushKey);
                return _controlDisabledBorderBrushKey;
            }
        }

        #endregion

        private static void EnsureResource([NotNull] string resourceName, ref ComponentResourceKey resourceKey)
        {
            if (string.IsNullOrEmpty(resourceName))
                throw new ArgumentException("Argument cannot be null or empty.", "resourceName");
            if (resourceKey != null)
                return;
            resourceKey = new ComponentResourceKey(typeof(ClientResources), resourceName);
        }
    }
}