using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Supremacy.Resources;

namespace Supremacy.Client.Dialogs
{
    public sealed partial class TargetSelectionDialog
    {
        public static readonly RoutedCommand SelectTargetCommand;

        #region TargetDisplayMember (Dependency Property)
        private static readonly DependencyPropertyKey TargetDisplayMemberPathPropertyKey;

        public static readonly DependencyProperty TargetDisplayMemberPathProperty;

        public string TargetDisplayMember
        {
            get { return (string)GetValue(TargetDisplayMemberPathProperty); }
            private set { SetValue(TargetDisplayMemberPathPropertyKey, value); }
        }
        #endregion

        #region Targets (Dependency Property)
        private static readonly DependencyPropertyKey TargetsPropertyKey;
        public static readonly DependencyProperty TargetsProperty;

        public IEnumerable Targets
        {
            get { return (IEnumerable)GetValue(TargetsProperty); }
            private set { SetValue(TargetsPropertyKey, value); }
        }
        #endregion

        #region Targets (Dependency Property)
        public static readonly DependencyProperty SelectedTargetProperty;

        public object SelectedTarget
        {
            get { return GetValue(SelectedTargetProperty); }
            set { SetValue(SelectedTargetProperty, value); }
        }
        #endregion

        static TargetSelectionDialog()
        {
            TargetsPropertyKey = DependencyProperty.RegisterReadOnly(
                "Targets",
                typeof(IEnumerable),
                typeof(TargetSelectionDialog),
                new FrameworkPropertyMetadata(Enumerable.Empty<object>()));

            TargetsProperty = TargetsPropertyKey.DependencyProperty;

            TargetDisplayMemberPathPropertyKey = DependencyProperty.RegisterReadOnly(
                "TargetDisplayMember",
                typeof(string),
                typeof(TargetSelectionDialog),
                new FrameworkPropertyMetadata(string.Empty));

            TargetDisplayMemberPathProperty = TargetDisplayMemberPathPropertyKey.DependencyProperty;

            SelectedTargetProperty = DependencyProperty.Register(
                "SelectedTarget",
                typeof(object),
                typeof(TargetSelectionDialog),
                new FrameworkPropertyMetadata(
                    null,
                    null,
                    CoerceSelectedTarget));

            SelectTargetCommand = new RoutedCommand(
                "SelectTarget",
                typeof(TargetSelectionDialog));
        }

        private static object CoerceSelectedTarget(DependencyObject o, object value)
        {
            IEnumerable targets = o.GetValue(TargetsProperty) as IEnumerable;

            if ((targets != null) && targets.OfType<object>().Contains(value))
                return value;

            return DependencyProperty.UnsetValue;
        }

        private TargetSelectionDialog()
        {
            InitializeComponent();

            Header = ResourceManager.GetString("ORDER_SELET_TARGET");

            CommandBindings.Add(
                new CommandBinding(
                    SelectTargetCommand,
                    (s, e) => DialogResult = (SelectedTarget != null),
                    (s, e) => e.CanExecute = (e.Parameter != null)));
        }

        public static TTarget Show<TTarget>(IEnumerable<TTarget> targets) where TTarget : class
        {
            if ((targets == null) || !targets.Any())
                return null;
            return Show(targets, ".", String.Empty);
        }

        public static TTarget Show<TTarget>(IEnumerable<TTarget> targets, string displayMember, string title) where TTarget : class
        {
            TargetSelectionDialog dialog = new TargetSelectionDialog
                         {
                             Targets = targets,
                             TargetDisplayMember = displayMember,
                             Header = title
                         };

            bool? dialogResult = dialog.ShowDialog();
            if (!dialogResult.HasValue || !dialogResult.Value)
                return null;

            return dialog.SelectedTarget as TTarget;
        }

        private void OnTargetsListMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DependencyObject source = e.OriginalSource as DependencyObject;
            if (source == null)
                return;

            ListBoxItem contanier = source.FindVisualAncestorByType<ListBoxItem>();
            if (contanier == null)
                return;

            DialogResult = true;
        }
    }
}