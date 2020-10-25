// File:ShipInfoPanel.xaml.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Practices.ServiceLocation;
using Supremacy.Game;
using Supremacy.Orbitals;

namespace Supremacy.Client
{
    /// <summary>
    /// Interaction logic for ShipInfoPanel.xaml
    /// </summary>

    public partial class ShipInfoPanel
    {
        private string _previousText;
        private string _previousClassText;

        public ShipInfoPanel()
        {
            InitializeComponent();
            DataContextChanged += delegate
                                       {
                                           _previousText = null;
                                           _previousClassText = null;
                                       };
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var nameText = GetTemplateChild("NameText") as TextBox;

            if (nameText == null)
                return;
            nameText.LostFocus += NameText_OnLostFocus;
            nameText.GotFocus += NameText_OnGotFocus;
            nameText.TextChanged += NameText_OnTextChanged;

            var classText = GetTemplateChild("ClassText") as TextBox;

            if (classText == null)
                return;
            classText.LostFocus += ClassText_OnLostFocus;
            classText.GotFocus += ClassText_OnGotFocus;
            classText.TextChanged += ClassText_OnTextChanged;

        }

        private static void NameText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var nameText = e.Source as TextBox;
            if (nameText == null)
                return;
            var bindingExpression = nameText.GetBindingExpression(TextBox.TextProperty);
            if (bindingExpression == null)
                return;
            if (!String.IsNullOrEmpty(nameText.Text))
                bindingExpression.UpdateSource();
        }

        private static void ClassText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var classText = e.Source as TextBox;
            if (classText == null)
                return;
            var bindingExpression = classText.GetBindingExpression(TextBox.TextProperty);
            if (bindingExpression == null)
                return;
            if (!String.IsNullOrEmpty(classText.Text))
                bindingExpression.UpdateSource();
        }

        private void NameText_OnGotFocus(object sender, RoutedEventArgs e)
        {
            var nameText = e.Source as TextBox;
            if (nameText == null)
                return;
            _previousText = nameText.Text;
        }

        private void ClassText_OnGotFocus(object sender, RoutedEventArgs e)
        {
            var classText= e.Source as TextBox;
            if (classText== null)
                return;
            _previousClassText = classText.Text;
        }

        private void NameText_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var nameText = e.Source as TextBox;
            var previousText = _previousText;
            _previousText = null;
            if ((nameText == null) || String.Equals(nameText.Text, previousText))
                return;
            var ship = DataContext as Ship;
            if (ship == null)
                return;
            if (String.IsNullOrEmpty(nameText.Text.Trim()) || String.Equals(ship.Name, ship.ShipDesign.Name))
                ship.Name = null;
            ServiceLocator.Current.GetInstance<IPlayerOrderService>().AddOrder(new SetObjectNameOrder(ship, ship.Name));
        }

        private void ClassText_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var classText = e.Source as TextBox;
            var previousText = _previousClassText;
            _previousClassText = null;
            if ((classText == null) || String.Equals(classText.Text, previousText))
                return;
            var ship = DataContext as Ship;
            if (ship == null)
                return;
            //if (String.IsNullOrEmpty(classText.Text.Trim()) || String.Equals(ship.ClassName, ship.ShipDesign.ClassName))
            //    ship.Class = null;
                ServiceLocator.Current.GetInstance<IPlayerOrderService>().AddOrder(new SetObjectNameOrder(ship, ship.ClassName));
        }

    }
}