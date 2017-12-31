using System.Windows;
using System.Windows.Controls;

using Supremacy.Economy;

namespace Supremacy.Client
{
    public class ShipyardDockView : Control
    {
        static ShipyardDockView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ShipyardDockView),
                new FrameworkPropertyMetadata(typeof(ShipyardDockView)));

            FocusableProperty.OverrideMetadata(
                typeof(ShipyardDockView),
                new FrameworkPropertyMetadata(false));

            IsTabStopProperty.OverrideMetadata(
                typeof(ShipyardDockView),
                new FrameworkPropertyMetadata(false));
        }

        #region BuildSlot Property
        public static readonly DependencyProperty BuildSlotProperty = DependencyProperty.Register(
            "BuildSlot",
            typeof(ShipyardBuildSlot),
            typeof(ShipyardDockView),
            new PropertyMetadata());

        public ShipyardBuildSlot BuildSlot
        {
            get { return (ShipyardBuildSlot)GetValue(BuildSlotProperty); }
            set { SetValue(BuildSlotProperty, value); }
        }
        #endregion
    }
}
