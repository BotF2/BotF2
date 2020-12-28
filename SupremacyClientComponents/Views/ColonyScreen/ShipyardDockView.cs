using System.Collections.Generic;
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

        #region ShipyardBuildSlot Property
        public static readonly DependencyProperty ShipyardBuildSlotProperty = DependencyProperty.Register(
            "ShipyardBuildSlot",
            typeof(ShipyardBuildSlot),
            typeof(ShipyardDockView),
            new PropertyMetadata());

        public  ShipyardBuildSlot ShipyardBuildSlot
        {
            get { return (ShipyardBuildSlot)GetValue(ShipyardBuildSlotProperty); }
            set { SetValue(ShipyardBuildSlotProperty, value); }
        }
        #endregion

        #region BuildSlotQueueProject Property
        public static readonly DependencyProperty BuildSlotQueueProjectProperty = DependencyProperty.Register(
            "BuildSlotQueueProject",
            typeof(ShipyardBuildSlot),
            typeof(ShipyardDockView),
            new PropertyMetadata());

        public ShipyardBuildSlot BuildSlotQueueProject
        {
            get { return (ShipyardBuildSlot)GetValue(BuildSlotQueueProjectProperty); }
            set { SetValue(BuildSlotQueueProjectProperty, value); }
        }
        #endregion
    }
}
