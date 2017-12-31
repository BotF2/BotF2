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

        private readonly IRegionManager _rootRegionManager;

        private IRegion _modalDialogsRegion;

        private ContentPresenter _activeDialogPresenter;

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
                return;

            _rootRegionManager = ServiceLocator.Current.GetInstance<IRegionManager>();
            //FocusManager.SetIsFocusScope(this, true);
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
            get { return GetValue(ActiveDialogProperty) as Dialog; }
            protected set { SetValue(ActiveDialogPropertyKey, value); }
        }

        internal ContentPresenter ActiveDialogPresenter
        {
            get { return _activeDialogPresenter; }
        }

        protected IRegionManager RootRegionManager
        {
            get { return _rootRegionManager; }
        }

        private IRegion ModalDialogsRegion
        {
            get
            {
                if ((_modalDialogsRegion == null) && 
                    (_rootRegionManager != null) &&
                    _rootRegionManager.Regions.ContainsRegionWithName(ClientRegions.ModalDialogs))
                {
                    _modalDialogsRegion = _rootRegionManager.Regions[ClientRegions.ModalDialogs];
                }
                return _modalDialogsRegion;
            }
        }

        public DialogOrderingMode OrderingMode
        {
            get { return (DialogOrderingMode)GetValue(OrderingModeProperty); }
            set { SetValue(OrderingModeProperty, value); }
        }

        private static void OnActiveDialogChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            var oldDialog = args.OldValue as Dialog;
            var newDialog = args.NewValue as Dialog;

            if (oldDialog != null)
                oldDialog.IsActive = false;

            if (newDialog != null)
                newDialog.IsActive = true;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _activeDialogPresenter = GetTemplateChild("PART_ActiveDialogPresenter") as ContentPresenter;
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
                return;

            if (HasItems && (SelectedIndex < 0))
                SelectedIndex = 0;

            UpdateActiveDialog();
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if ((e.Action != NotifyCollectionChangedAction.Remove) || (SelectedIndex != -1))
                return;

            var nextDialog = FindNextDialog();
            if (nextDialog != null)
                SelectedItem = nextDialog;
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            UpdateActiveDialog();

            var activeDialog = ActiveDialog;
            if (activeDialog != null)
                activeDialog.Loaded += OnActiveDialogLoaded;

            base.OnSelectionChanged(e);
        }

        private void OnActiveDialogLoaded(object sender, RoutedEventArgs args)
        {
            var sourceDialog = (Dialog)args.Source;
            var activeDialog = ActiveDialog;

            sourceDialog.Loaded -= OnActiveDialogLoaded;

            if (sourceDialog != activeDialog)
                return;

            var setFocus = IsKeyboardFocusWithin;
            if (!setFocus)
            {
                if (Equals(RegionManager.GetRegionName(this), ClientRegions.ModalDialogs))
                    setFocus = true;
                else if (Equals(RegionManager.GetRegionName(this), ClientRegions.ModelessDialogs))
                {
                    var modalDialogRegion = ModalDialogsRegion;
                    if ((modalDialogRegion != null) &&
                        !modalDialogRegion.ActiveViews.Any())
                    {
                        setFocus = true;
                    }
                }
            }

            if (!setFocus)
                return;

            activeDialog.SetFocus();
        }

        private Dialog GetActiveDialog()
        {
            var selectedItem = SelectedItem;
            if (selectedItem == null)
                return null;

            var activeDialog = selectedItem as Dialog;
            if (activeDialog == null)
                activeDialog = ItemContainerGenerator.ContainerFromIndex(SelectedIndex) as Dialog;

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

            var activeDialog = GetActiveDialog();
            if (activeDialog == null)
                return;

            var parent = VisualTreeHelper.GetParent(activeDialog) as FrameworkElement;
            if ((parent != null) && (TabOnceActiveElementPropertyDescriptor != null))
            {
                TabOnceActiveElementPropertyDescriptor.SetValue(parent, activeDialog);
                TabOnceActiveElementPropertyDescriptor.SetValue(this, parent);
            }

            ActiveDialog = activeDialog;

            FocusManager.SetFocusedElement(this, activeDialog);
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return (item is Dialog);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new Dialog();
        }

        private Dialog FindNextDialog()
        {
            var dialogs = Items.OfType<Dialog>();
            if (OrderingMode == DialogOrderingMode.Stack)
                dialogs = dialogs.Reverse();
            return dialogs.FirstOrDefault();
        }
    }
}