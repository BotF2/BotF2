// DialogManager.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

using Microsoft.Practices.Composite.Presentation.Regions;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.ServiceLocation;

namespace Supremacy.Client.Dialogs
{
    public enum DialogOrderingMode
    {
        Stack,
        Queue
    }

    [TemplatePart(Name = "PART_ActiveDialogPresenter", Type = typeof(ContentPresenter))]
    public class DialogManager : Selector
    {
        public static readonly DependencyProperty ActiveDialogProperty;
        public static readonly DependencyProperty OrderingModeProperty;

        protected static readonly DependencyPropertyKey ActiveDialogPropertyKey;
        protected static readonly DependencyPropertyDescriptor TabOnceActiveElementPropertyDescriptor;
        private IRegion _modalDialogsRegion;

        static DialogManager()
        {
            ActiveDialogPropertyKey = DependencyProperty.RegisterReadOnly(
                "ActiveDialog",
                typeof(Dialog),
                typeof(DialogManager),
                new PropertyMetadata(OnActiveDialogChanged));

            OrderingModeProperty = DependencyProperty.Register(
                "OrderingMode",
                typeof(DialogOrderingMode),
                typeof(DialogManager),
                new PropertyMetadata(DialogOrderingMode.Stack));

            FocusManager.IsFocusScopeProperty.OverrideMetadata(
                typeof(DialogManager),
                new PropertyMetadata(true));

            ActiveDialogProperty = ActiveDialogPropertyKey.DependencyProperty;

            TabOnceActiveElementPropertyDescriptor = GetTabOnceActiveElementPropertyDescriptor();

            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DialogManager),
                new FrameworkPropertyMetadata(typeof(DialogManager)));
        }

        public DialogManager()
        {
            if (Designer.IsInDesignMode)
            {
                return;
            }

            RootRegionManager = ServiceLocator.Current.GetInstance<IRegionManager>();
        }

        private static DependencyPropertyDescriptor GetTabOnceActiveElementPropertyDescriptor()
        {
            try
            {
                return DependencyPropertyDescriptor.FromName(
                    "TabOnceActiveElement",
                    typeof(KeyboardNavigation),
                    typeof(FrameworkElement));
            }
            catch
            {
                return null;
            }
        }

        public Dialog ActiveDialog
        {
            get => GetValue(ActiveDialogProperty) as Dialog;
            protected set => SetValue(ActiveDialogPropertyKey, value);
        }

        internal ContentPresenter ActiveDialogPresenter { get; private set; }

        protected IRegionManager RootRegionManager { get; }

        private IRegion ModalDialogsRegion
        {
            get
            {
                if ((_modalDialogsRegion == null) &&
                    (RootRegionManager != null) &&
                    RootRegionManager.Regions.ContainsRegionWithName(ClientRegions.ModalDialogs))
                {
                    _modalDialogsRegion = RootRegionManager.Regions[ClientRegions.ModalDialogs];
                }
                return _modalDialogsRegion;
            }
        }

        public DialogOrderingMode OrderingMode
        {
            get => (DialogOrderingMode)GetValue(OrderingModeProperty);
            set => SetValue(OrderingModeProperty, value);
        }

        private static void OnActiveDialogChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            if (args.OldValue is Dialog oldDialog)
            {
                oldDialog.IsActive = false;
            }

            if (args.NewValue is Dialog newDialog)
            {
                newDialog.IsActive = true;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ActiveDialogPresenter = GetTemplateChild("PART_ActiveDialogPresenter") as ContentPresenter;
            UpdateActiveDialog();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ItemContainerGenerator.StatusChanged += OnGeneratorStatusChanged;
        }

        private void OnGeneratorStatusChanged(object sender, EventArgs e)
        {
            if (ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
            {
                return;
            }

            if (HasItems && (SelectedIndex < 0))
            {
                SelectedIndex = 0;
            }

            UpdateActiveDialog();
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if ((e.Action != NotifyCollectionChangedAction.Remove) || (SelectedIndex != -1))
            {
                return;
            }

            Dialog nextDialog = FindNextDialog();
            if (nextDialog != null)
            {
                SelectedItem = nextDialog;
            }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            UpdateActiveDialog();

            Dialog activeDialog = ActiveDialog;
            if (activeDialog != null)
            {
                activeDialog.Loaded += OnActiveDialogLoaded;
            }

            base.OnSelectionChanged(e);
        }

        private void OnActiveDialogLoaded(object sender, RoutedEventArgs args)
        {
            Dialog sourceDialog = (Dialog)args.Source;
            Dialog activeDialog = ActiveDialog;

            sourceDialog.Loaded -= OnActiveDialogLoaded;

            if (sourceDialog != activeDialog)
            {
                return;
            }

            bool setFocus = IsKeyboardFocusWithin;
            if (!setFocus)
            {
                if (Equals(RegionManager.GetRegionName(this), ClientRegions.ModalDialogs))
                {
                    setFocus = true;
                }
                else if (Equals(RegionManager.GetRegionName(this), ClientRegions.ModelessDialogs))
                {
                    IRegion modalDialogRegion = ModalDialogsRegion;
                    if ((modalDialogRegion != null) &&
                        !modalDialogRegion.ActiveViews.Any())
                    {
                        setFocus = true;
                    }
                }
            }

            if (!setFocus)
            {
                return;
            }

            _ = activeDialog.SetFocus();
        }

        private Dialog GetActiveDialog()
        {
            object selectedItem = SelectedItem;
            if (selectedItem == null)
            {
                return null;
            }

            if (!(selectedItem is Dialog activeDialog))
            {
                activeDialog = ItemContainerGenerator.ContainerFromIndex(SelectedIndex) as Dialog;
            }

            return activeDialog;
        }

        private void UpdateActiveDialog()
        {
            if (SelectedIndex < 0)
            {
                ActiveDialog = null;
                FocusManager.SetFocusedElement(this, null);
                return;
            }

            Dialog activeDialog = GetActiveDialog();
            if (activeDialog == null)
            {
                return;
            }

            if ((VisualTreeHelper.GetParent(activeDialog) is FrameworkElement parent) && (TabOnceActiveElementPropertyDescriptor != null))
            {
                TabOnceActiveElementPropertyDescriptor.SetValue(parent, activeDialog);
                TabOnceActiveElementPropertyDescriptor.SetValue(this, parent);
            }

            ActiveDialog = activeDialog;

            FocusManager.SetFocusedElement(this, activeDialog);
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is Dialog;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new Dialog();
        }

        private Dialog FindNextDialog()
        {
            System.Collections.Generic.IEnumerable<Dialog> dialogs = Items.OfType<Dialog>();
            if (OrderingMode == DialogOrderingMode.Stack)
            {
                dialogs = dialogs.Reverse();
            }

            return dialogs.FirstOrDefault();
        }
    }
}