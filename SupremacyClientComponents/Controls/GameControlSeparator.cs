using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace Supremacy.Client.Controls
{
    public class GameControlSeparator : GameControlBase
    {
        #region Style Keys

        #region MenuItemSeparatorSmallStyleKey Resource Key
        private static ComponentResourceKey _menuItemSeparatorSmallStyleKey;

        public static ResourceKey MenuItemSeparatorSmallStyleKey
        {
            get
            {
                if (_menuItemSeparatorSmallStyleKey == null)
                {
                    _menuItemSeparatorSmallStyleKey = new ComponentResourceKey(
                        typeof(GameControlSeparator),
                        "MenuItemSeparatorSmallStyleKey");
                }
                return _menuItemSeparatorSmallStyleKey;
            }
        }
        #endregion

        #region VerticalGroupItemSeparatorSmallStyleKey Resource Key
        private static ComponentResourceKey _verticalGroupItemSeparatorSmallStyleKey;

        public static ResourceKey VerticalGroupItemSeparatorSmallStyleKey
        {
            get
            {
                if (_verticalGroupItemSeparatorSmallStyleKey == null)
                {
                    _verticalGroupItemSeparatorSmallStyleKey = new ComponentResourceKey(
                        typeof(GameControlSeparator),
                        "VerticalGroupItemSeparatorSmallStyleKey");
                }
                return _verticalGroupItemSeparatorSmallStyleKey;
            }
        }
        #endregion

        #region HorizontalGroupItemSeparatorSmallStyleKey Resource Key
        private static ComponentResourceKey _horizontalGroupItemSeparatorSmallStyleKey;

        public static ResourceKey HorizontalGroupItemSeparatorSmallStyleKey
        {
            get
            {
                if (_horizontalGroupItemSeparatorSmallStyleKey == null)
                {
                    _horizontalGroupItemSeparatorSmallStyleKey = new ComponentResourceKey(
                        typeof(GameControlSeparator),
                        "HorizontalGroupItemSeparatorSmallStyleKey");
                }
                return _horizontalGroupItemSeparatorSmallStyleKey;
            }
        }
        #endregion

        #region MenuItemSeparatorLargeStyleKey Resource Key
        private static ComponentResourceKey _menuItemSeparatorLargeStyleKey;

        public static ResourceKey MenuItemSeparatorLargeStyleKey
        {
            get
            {
                if (_menuItemSeparatorLargeStyleKey == null)
                {
                    _menuItemSeparatorLargeStyleKey = new ComponentResourceKey(
                        typeof(GameControlSeparator),
                        "MenuItemSeparatorLargeStyleKey");
                }
                return _menuItemSeparatorLargeStyleKey;
            }
        }
        #endregion

        #region VerticalGroupItemSeparatorLargeStyleKey Resource Key
        private static ComponentResourceKey _verticalGroupItemSeparatorLargeStyleKey;

        public static ResourceKey VerticalGroupItemSeparatorLargeStyleKey
        {
            get
            {
                if (_verticalGroupItemSeparatorLargeStyleKey == null)
                {
                    _verticalGroupItemSeparatorLargeStyleKey = new ComponentResourceKey(
                        typeof(GameControlSeparator),
                        "VerticalGroupItemSeparatorLargeStyleKey");
                }
                return _verticalGroupItemSeparatorLargeStyleKey;
            }
        }
        #endregion

        #region HorizontalGroupItemSeparatorLargeStyleKey Resource Key
        private static ComponentResourceKey _horizontalGroupItemSeparatorLargeStyleKey;

        public static ResourceKey HorizontalGroupItemSeparatorLargeStyleKey
        {
            get
            {
                if (_horizontalGroupItemSeparatorLargeStyleKey == null)
                {
                    _horizontalGroupItemSeparatorLargeStyleKey = new ComponentResourceKey(
                        typeof(GameControlSeparator),
                        "HorizontalGroupItemSeparatorLargeStyleKey");
                }
                return _horizontalGroupItemSeparatorLargeStyleKey;
            }
        }
        #endregion

        #endregion

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static GameControlSeparator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(GameControlSeparator),
                new FrameworkPropertyMetadata(typeof(GameControlSeparator)));

            IsTabStopProperty.OverrideMetadata(
                typeof(GameControlSeparator),
                new FrameworkPropertyMetadata(false));

            FocusableProperty.OverrideMetadata(
                typeof(GameControlSeparator),
                new FrameworkPropertyMetadata(false));
        }

        public GameControlSeparator() { }

        public GameControlSeparator(string label)
            : this()
        {
            Label = label;
        }

        public bool IsFirstItem => GameItemsControl.GetIsFirstItem(this);

        public bool IsLastItem => GameItemsControl.GetIsLastItem(this);
    }
}