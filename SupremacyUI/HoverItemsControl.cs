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

        static void HoverItemsControl_HoveredItemChanged(
            DependencyObject source,
            DependencyPropertyChangedEventArgs e)
        {
            HoverItemsControl sourceControl = source as HoverItemsControl;
            if ((sourceControl != null) && (sourceControl.HoveredItemChanged != null))
                sourceControl.HoveredItemChanged(sourceControl, e);
        }
        
        public event ItemClickedEventHandler ItemClicked;
        public event DependencyPropertyChangedEventHandler HoveredItemChanged;

        public HoverItemsControl()
        {
            ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
            PreviewMouseLeftButtonDown += new MouseButtonEventHandler(HoverItemsControl_PreviewMouseLeftButtonDown);
        }

        void HoverItemsControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Console.Out.WriteLine();
        }

        void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                foreach (object item in Items)
                {
                    FrameworkElement itemContainer = ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                    if (itemContainer != null)
                    {
                        itemContainer.MouseEnter += itemContainer_MouseEnter;
                        itemContainer.MouseLeave += itemContainer_MouseLeave;
                        itemContainer.MouseLeftButtonDown += itemContainer_MouseLeftButtonDown;
                    }
                }
                ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;
            }
        }

        public object HoveredItem
        {
            get { return GetValue(HoveredItemProperty); }
            set { SetValue(HoveredItemProperty, value); }
        }

        protected void OnItemClicked(object clickedItem)
        {
            if (ItemClicked != null)
                ItemClicked(this, clickedItem);
        }
        
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (object item in Items)
                {
                    FrameworkElement itemContainer = ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                    if (itemContainer != null)
                    {
                        itemContainer.MouseEnter += itemContainer_MouseEnter;
                        itemContainer.MouseLeave += itemContainer_MouseLeave;
                        itemContainer.MouseLeftButtonDown += itemContainer_MouseLeftButtonDown;
                    }
                }
            }
            else
            {
                if (e.OldItems != null)
                {
                    foreach (object oldItem in e.OldItems)
                    {
                        FrameworkElement itemContainer = ItemContainerGenerator.ContainerFromItem(oldItem) as FrameworkElement;
                        if (itemContainer != null)
                        {
                            itemContainer.MouseEnter -= itemContainer_MouseEnter;
                            itemContainer.MouseLeave -= itemContainer_MouseLeave;
                            itemContainer.MouseLeftButtonDown -= itemContainer_MouseLeftButtonDown;
                        }
                    }
                }
                if (e.NewItems != null)
                {
                    foreach (object newItem in e.NewItems)
                    {
                        FrameworkElement itemContainer = ItemContainerGenerator.ContainerFromItem(newItem) as FrameworkElement;
                        if (itemContainer != null)
                        {
                            itemContainer.MouseEnter += itemContainer_MouseEnter;
                            itemContainer.MouseLeave += itemContainer_MouseLeave;
                            itemContainer.MouseLeftButtonDown += itemContainer_MouseLeftButtonDown;
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
                    FrameworkElement itemContainer = ItemContainerGenerator.ContainerFromItem(oldItem) as FrameworkElement;
                    if (itemContainer != null)
                    {
                        itemContainer.MouseEnter -= itemContainer_MouseEnter;
                        itemContainer.MouseLeave -= itemContainer_MouseLeave;
                        itemContainer.MouseLeftButtonDown -= itemContainer_MouseLeftButtonDown;
                    }
                }
            }
            if (newValue != null)
            {
                foreach (object newItem in newValue)
                {
                    FrameworkElement itemContainer = ItemContainerGenerator.ContainerFromItem(newItem) as FrameworkElement;
                    if (itemContainer != null)
                    {
                        itemContainer.MouseEnter += itemContainer_MouseEnter;
                        itemContainer.MouseLeave += itemContainer_MouseLeave;
                        itemContainer.MouseLeftButtonDown += itemContainer_MouseLeftButtonDown;
                    }
                }
            }
        }

        void itemContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            object senderItem = ItemContainerGenerator.ItemFromContainer(sender as DependencyObject);
            if (senderItem != null)
            {
                e.Handled = true;
                OnItemClicked(senderItem);
            }
        }

        void itemContainer_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            object senderItem = ItemContainerGenerator.ItemFromContainer(sender as DependencyObject);
            if ((senderItem != null) && (senderItem == HoveredItem))
            {
                if (!((bool)e.NewValue))
                    HoveredItem = null;
            }
            else if ((bool)e.NewValue)
            {
                HoveredItem = senderItem;
            }
        }

        void itemContainer_MouseLeave(object sender, MouseEventArgs e)
        {
            object senderItem = ItemContainerGenerator.ItemFromContainer(sender as DependencyObject);
            if (senderItem == HoveredItem)
                HoveredItem = null;
        }

        void itemContainer_MouseEnter(object sender, MouseEventArgs e)
        {
            HoveredItem = ItemContainerGenerator.ItemFromContainer(sender as DependencyObject);
        }
    }
}
