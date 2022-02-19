// File:ShipInfoPanel.xaml.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

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


            if (!(GetTemplateChild("NameText") is TextBox nameText))
            {
                return;
            }

            nameText.LostFocus += NameText_OnLostFocus;
            nameText.GotFocus += NameText_OnGotFocus;
            nameText.TextChanged += NameText_OnTextChanged;


            if (!(GetTemplateChild("ClassText") is TextBox classText))
            {
                return;
            }

            classText.LostFocus += ClassText_OnLostFocus;
            classText.GotFocus += ClassText_OnGotFocus;
            classText.TextChanged += ClassText_OnTextChanged;

        }

        private static void NameText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(e.Source is TextBox nameText))
            {
                return;
            }

            System.Windows.Data.BindingExpression bindingExpression = nameText.GetBindingExpression(TextBox.TextProperty);
            if (bindingExpression == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(nameText.Text))
            {
                bindingExpression.UpdateSource();
            }
        }

        private static void ClassText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(e.Source is TextBox classText))
            {
                return;
            }

            System.Windows.Data.BindingExpression bindingExpression = classText.GetBindingExpression(TextBox.TextProperty);
            if (bindingExpression == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(classText.Text))
            {
                bindingExpression.UpdateSource();
            }
        }

        private void NameText_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (!(e.Source is TextBox nameText))
            {
                return;
            }

            _previousText = nameText.Text;
        }

        private void ClassText_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (!(e.Source is TextBox classText))
            {
                return;
            }

            _previousClassText = classText.Text;
        }

        private void NameText_OnLostFocus(object sender, RoutedEventArgs e)
        {
            string previousText = _previousText;
            _previousText = null;
            if ((!(e.Source is TextBox nameText)) || string.Equals(nameText.Text, previousText))
            {
                return;
            }

            Ship ship = DataContext as Ship;
            if (ship == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(nameText.Text.Trim()) || string.Equals(ship.Name, ship.ShipDesign.Name))
            {
                ship.Name = null;
            }

            ServiceLocator.Current.GetInstance<IPlayerOrderService>().AddOrder(new SetObjectNameOrder(ship, ship.Name));
        }

        private void ClassText_OnLostFocus(object sender, RoutedEventArgs e)
        {
            string previousText = _previousClassText;
            _previousClassText = null;
            if ((!(e.Source is TextBox classText)) || string.Equals(classText.Text, previousText))
            {
                return;
            }

            Ship ship = DataContext as Ship;
            if (ship == null)
            {
                return;
            }
            //if (String.IsNullOrEmpty(classText.Text.Trim()) || String.Equals(ship.ClassName, ship.ShipDesign.ClassName))
            //    ship.Class = null;
            ServiceLocator.Current.GetInstance<IPlayerOrderService>().AddOrder(new SetObjectNameOrder(ship, ship.ClassName));
        }

    }
}