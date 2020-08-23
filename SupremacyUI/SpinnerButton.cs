// SpinnerButton.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Supremacy.UI
{
    public class SpinnerButton : ListBox
    {
        public static RoutedCommand NextItemCommand;

        static SpinnerButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(SpinnerButton),
                new FrameworkPropertyMetadata(typeof(SpinnerButton)));
            NextItemCommand = new RoutedCommand("NextItem", typeof(SpinnerButton));
        }

        public SpinnerButton()
        {
            _ = CommandBindings.Add(
                new CommandBinding(NextItemCommand,
                                   NextItemExecuted));
        }

        private void NextItemExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (Items.Count == 0)
            {
                return;
            }
            if (SelectedIndex < (Items.Count - 1))
            {
                SelectedIndex++;
            }
            else
            {
                SelectedIndex = 0;
            }
        }
    }
}
