using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Supremacy.Client.Controls
{
    public class GameItemsControl : ItemsControl,
                                    ILogicalParent,
                                    IVariantControl
    {
        #region Dependency Properties
        public static readonly DependencyProperty ContextProperty;
        public static readonly DependencyProperty VariantSizeProperty;

        #region IsFirstItem Property
        private static readonly DependencyPropertyKey IsFirstItemPropertyKey = DependencyProperty.RegisterReadOnly(
            "IsFirstItem",
            typeof(bool),
            typeof(GameItemsControl),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None,
                null,
                CoerceIsFirstItem));

        public static readonly DependencyProperty IsFirstItemProperty = IsFirstItemPropertyKey.DependencyProperty;

        public bool IsFirstItem => (bool)GetValue(IsFirstItemProperty);

        private static object CoerceIsFirstItem(DependencyObject d, object baseValue)
        {
            GameItemsControl itemsControl = ItemsControlFromItemContainer(d) as GameItemsControl;
            if (itemsControl == null)
                return false;

            int itemsCount = itemsControl.Items.Count;
            if (itemsCount == 0)
                return false;

            object item = itemsControl.ItemContainerGenerator.ItemFromContainer(d);
            if (item == null)
                return false;

            return Equals(item, itemsControl.Items[0]);
        }

        public static bool GetIsFirstItem(DependencyObject d)
        {
            return (bool)d.GetValue(IsFirstItemProperty);
        }
        #endregion

        #region IsLastItem Property
        private static readonly DependencyPropertyKey IsLastItemPropertyKey = DependencyProperty.RegisterReadOnly(
            "IsLastItem",
            typeof(bool),
            typeof(GameItemsControl),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.None,
                null,
                CoerceIsLastItem));

        public static readonly DependencyProperty IsLastItemProperty = IsLastItemPropertyKey.DependencyProperty;

        public bool IsLastItem => (bool)GetValue(IsLastItemProperty);

        private static object CoerceIsLastItem(DependencyObject d, object baseValue)
        {
            GameItemsControl itemsControl = ItemsControlFromItemContainer(d) as GameItemsControl;
            if (itemsControl == null)
                return false;

            int itemsCount = itemsControl.Items.Count;
            if (itemsCount == 0)
                return false;

            object item = itemsControl.ItemContainerGenerator.ItemFromContainer(d);
            if (item == null)
                return false;

            return Equals(item, itemsControl.Items[itemsCount - 1]);
        }

        public static bool GetIsLastItem(DependencyObject d)
        {
            return (bool)d.GetValue(IsLastItemProperty);
        }
        #endregion

        #endregion

        #region Constructors and Finalizers
        static GameItemsControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(GameItemsControl),
                new FrameworkPropertyMetadata(typeof(GameItemsControl)));

            ContextProperty = GameControlService.ContextProperty.AddOwner(
                typeof(GameItemsControl),
                new FrameworkPropertyMetadata(
                    GameControlContext.None,
                    OnContextPropertyValueChanged));

            VariantSizeProperty = GameControlService.VariantSizeProperty.AddOwner(
                typeof(GameItemsControl),
                new FrameworkPropertyMetadata(
                    VariantSize.Medium,
                    OnVariantSizePropertyValueChanged));
        }
        #endregion

        #region IVariantControl Implementation
        public VariantSize VariantSize
        {
            get { return (VariantSize)GetValue(VariantSizeProperty); }
            set { SetValue(VariantSizeProperty, value); }
        }

        public GameControlContext Context
        {
            get { return (GameControlContext)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }
        #endregion

        #region ILogicalParent Implementation
        void ILogicalParent.AddLogicalChild(object child)
        {
            AddLogicalChild(child);
        }

        void ILogicalParent.RemoveLogicalChild(object child)
        {
            RemoveLogicalChild(child);
        }
        #endregion

        #region Methods
        private static void OnContextPropertyValueChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            GameItemsControl control = (GameItemsControl)d;
            GameControlContext oldContext = (GameControlContext)e.OldValue;
            GameControlContext newContext = (GameControlContext)e.NewValue;

            control.OnContextChanged(oldContext, newContext);
        }

        private static void OnVariantSizePropertyValueChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            GameItemsControl control = (GameItemsControl)d;
            VariantSize oldVariantSize = (VariantSize)e.OldValue;
            VariantSize newVariantSize = (VariantSize)e.NewValue;

            control.OnVariantSizeChanged(oldVariantSize, newVariantSize);
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            element.CoerceValue(IsFirstItemProperty);
            element.CoerceValue(IsLastItemProperty);

            IVariantControl variantControl = element as IVariantControl;
            if (variantControl != null)
                variantControl.Context = Context;
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);

            element.ClearValue(IsFirstItemPropertyKey);
            element.ClearValue(IsLastItemPropertyKey);
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
                OnValidateItems(Items);
            else if ((e.NewItems != null) && (e.NewItems.Count > 0))
                OnValidateItems(e.NewItems);

            base.OnItemsChanged(e);
        }

        protected virtual void OnValidateItems(IList items) { }

        protected virtual void OnVariantSizeChanged(VariantSize oldVariantSize, VariantSize newVariantSize)
        {
            foreach (object item in Items)
            {
                if (IsItemItsOwnContainerOverride(item))
                {
                    IVariantControl variantControl = item as IVariantControl;
                    if (variantControl != null)
                        variantControl.VariantSize = newVariantSize;
                }
                else
                {
                    IVariantControl itemContainer = ItemContainerGenerator.ContainerFromItem(item) as IVariantControl;
                    if (itemContainer != null)
                        itemContainer.VariantSize = newVariantSize;
                }
            }
        }

        protected virtual void OnContextChanged(GameControlContext oldContext, GameControlContext newContext)
        {
            foreach (object item in Items)
            {
                if (IsItemItsOwnContainerOverride(item))
                {
                    IVariantControl variantControl = item as IVariantControl;
                    if (variantControl != null)
                        variantControl.Context = newContext;
                }
                else
                {
                    IVariantControl itemContainer = ItemContainerGenerator.ContainerFromItem(item) as IVariantControl;
                    if (itemContainer != null)
                        itemContainer.Context = newContext;
                }
            }
        }
        #endregion
    }
}