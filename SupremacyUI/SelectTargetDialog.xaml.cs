// SelectTargetDialog.xaml.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

using Supremacy.Resources;
using System.Collections;

namespace Supremacy.UI
{
    /// <summary>
    /// Interaction logic for SelectTargetDialog.xaml
    /// </summary>
    [TemplatePart(Name = "PART_TargetsList", Type = typeof(ListBox))]
    public partial class SelectTargetDialog : Window
    {
        public static readonly RoutedCommand SelectTargetCommand;

        private object result;
        private Selector targetsList;

        static SelectTargetDialog()
        {
            SelectTargetCommand = new RoutedCommand("SelectTarget", typeof(SelectTargetDialog));
        }

        public SelectTargetDialog()
        {
            InitializeComponent();

            base.Title = ResourceManager.GetString("ORDER_SELET_TARGET");

            CommandBindings.Add(
                new CommandBinding(SelectTargetCommand,
                                   SelectTargetCommand_Executed,
                                   SelectTargetCommand_CanExecute));
        }

        void SelectTargetCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (e.Parameter != null);
        }

        void SelectTargetCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            result = e.Parameter;
            if (result != null)
                DialogResult = true;
            else
                DialogResult = false;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            targetsList = GetTemplateChild("PART_TargetsList") as ListBox;
        }

        public static Object Show(ICollection targetList)
        {
            if ((targetList == null) || (targetList.Count == 0))
                return null;
            return Show(null, targetList);
        }

        public static Object Show(Window owner, ICollection targetList)
        {
            if ((targetList == null) || (targetList.Count == 0))
                return null;
            return Show(owner, targetList, null, null);
        }

        public static object Show(Window owner, ICollection targetList, string displayMember, string title)
        {
            var dialog = new SelectTargetDialog();
            dialog.ApplyTemplate();
            if (dialog.targetsList != null)
            {
                if (owner != null)
                    dialog.Owner = owner;
                dialog.targetsList.ItemsSource = targetList;
                if (displayMember != null)
                    dialog.targetsList.DisplayMemberPath = displayMember;
                if (title != null)
                    dialog.Title = title;
                dialog.ShowDialog();
                dialog.DataContext = null;
                if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
                    return dialog.result;
                dialog.Close();
            }
            return null;
        }
    }
}