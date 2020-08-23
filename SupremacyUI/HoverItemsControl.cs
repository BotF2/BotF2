// HoverItemsControl.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Supremacy.UI
{
    public class HoverItemsControl : ItemsControl
    {
        public delegate void ItemClickedEventHandler(object sender, object clickedItem);

        public static readonly DependencyProperty HoveredItemProperty;

        static HoverItemsControl()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(HoverItemsControl),
                new FrameworkPropertyMetadata(typeof(HoverItemsControl)));

            FocusVisualStyleProperty.OverrideMetadata(
                typeof(HoverItemsControl),
                new FrameworkPropertyMetadata(
                    null,
                    (DependencyObject d, object baseValue) => null));

            HoveredItemProperty = DependencyProperty.Register(
                "HoveredItem",
                typeof(object),
                typeof(HoverItemsControl),
                new PropertyMetadata(HoverItemsControl_HoveredItemChanged));
        }

        private static void HoverItemsControl_HoveredItemChanged(
            DependencyObject source,
            DependencyPropertyChangedEventArgs e)
        {
            if ((source is HoverItemsControl sourceControl) && (sourceControl.HoveredItemChanged != null))
            {
                sourceControl.HoveredItemChanged(sourceControl, e);
            }
        }
        
        public event ItemClickedEventHandler ItemClicked;
        public event DependencyPropertyChangedEventHandler HoveredItemChanged;

        public HoverItemsControl()
        {
            ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
            PreviewMouseLeftButtonDown += new MouseButtonEventHandler(HoverItemsControl_PreviewMouseLeftButtonDown);
        }

        private void HoverItemsControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                foreach (object item in Items)
                {
                    if (ItemContainerGenerator.ContainerFromItem(item) is FrameworkElement itemContainer)
                    {
                        itemContainer.MouseEnter += ItemContainer_MouseEnter;
                        itemContainer.MouseLeave += ItemContainer_MouseLeave;
                        itemContainer.MouseLeftButtonDown += ItemContainer_MouseLeftButtonDown;
                    }
                }
                ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;
            }
        }

        public object HoveredItem
        {
            get => GetValue(HoveredItemProperty);
            set => SetValue(HoveredItemProperty, value);
        }

        protected void OnItemClicked(object clickedItem)
        {
            ItemClicked?.Invoke(this, clickedItem);
        }
        
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (object item in Items)
                {
                    if (ItemContainerGenerator.ContainerFromItem(item) is FrameworkElement itemContainer)
                    {
                        itemContainer.MouseEnter += ItemContainer_MouseEnter;
                        itemContainer.MouseLeave += ItemContainer_MouseLeave;
                        itemContainer.MouseLeftButtonDown += ItemContainer_MouseLeftButtonDown;
                    }
                }
            }
            else
            {
                if (e.OldItems != null)
                {
                    foreach (object oldItem in e.OldItems)
                    {
                        if (ItemContainerGenerator.ContainerFromItem(oldItem) is FrameworkElement itemContainer)
                        {
                            itemContainer.MouseEnter -= ItemContainer_MouseEnter;
                            itemContainer.MouseLeave -= ItemContainer_MouseLeave;
                            itemContainer.MouseLeftButtonDown -= ItemContainer_MouseLeftButtonDown;
                        }
                    }
                }
                if (e.NewItems != null)
                {
                    foreach (object newItem in e.NewItems)
                    {
                        if (ItemContainerGenerator.ContainerFromItem(newItem) is FrameworkElement itemContainer)
                        {
                            itemContainer.MouseEnter += ItemContainer_MouseEnter;
                            itemContainer.MouseLeave += ItemContainer_MouseLeave;
                            itemContainer.MouseLeftButtonDown += ItemContainer_MouseLeftButtonDown;
                        }
                    }
                }
            }
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
            if (oldValue != null)
            {
                foreach (object oldItem in oldValue)
                {
                    if (ItemContainerGenerator.ContainerFromItem(oldItem) is FrameworkElement itemContainer)
                    {
                        itemContainer.MouseEnter -= ItemContainer_MouseEnter;
                        itemContainer.MouseLeave -= ItemContainer_MouseLeave;
                        itemContainer.MouseLeftButtonDown -= ItemContainer_MouseLeftButtonDown;
                    }
                }
            }
            if (newValue != null)
            {
                foreach (object newItem in newValue)
                {
                    if (ItemContainerGenerator.ContainerFromItem(newItem) is FrameworkElement itemContainer)
                    {
                        itemContainer.MouseEnter += ItemContainer_MouseEnter;
                        itemContainer.MouseLeave += ItemContainer_MouseLeave;
                        itemContainer.MouseLeftButtonDown += ItemContainer_MouseLeftButtonDown;
                    }
                }
            }
        }

        private void ItemContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            object senderItem = ItemContainerGenerator.ItemFromContainer(sender as DependencyObject);
            if (senderItem != null)
            {
                e.Handled = true;
                OnItemClicked(senderItem);
            }
        }

        //Commented out as Visual Studio says it is not used

        //void itemContainer_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    object senderItem = ItemContainerGenerator.ItemFromContainer(sender as DependencyObject);
        //    if ((senderItem != null) && (senderItem == HoveredItem))
        //    {
        //        if (!((bool)e.NewValue))
        //            HoveredItem = null;
        //    }
        //    else if ((bool)e.NewValue)
        //    {
        //        HoveredItem = senderItem;
        //    }
        //}

        private void ItemContainer_MouseLeave(object sender, MouseEventArgs e)
        {
            object senderItem = ItemContainerGenerator.ItemFromContainer(sender as DependencyObject);
            if (senderItem == HoveredItem)
            {
                HoveredItem = null;
            }
        }

        private void ItemContainer_MouseEnter(object sender, MouseEventArgs e)
        {
            HoveredItem = ItemContainerGenerator.ItemFromContainer(sender as DependencyObject);
        }
    }
}
