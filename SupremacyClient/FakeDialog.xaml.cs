// File:FakeDialog.xaml.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.Windows.Input;

namespace Supremacy.Client
{
    /// <summary>
    /// Interaction logic for FakeDialog.xaml
    /// </summary>
    public partial class FakeDialog
    {
        public FakeDialog()
        {
            InitializeComponent();

            //    InputBindings.Add(
            //        new KeyBinding(
            //            GenericCommands.CancelCommand,
            //            Key.Escape,
            //            ModifierKeys.None));


        }

        //private void OnGenericCommandsCancelCommandExecuted(object source, ExecutedRoutedEventArgs e)
        //{
        //    ClientSettings.Current.Reload();
        //    Close();
        //}

        private void OnGenericCommandsAcceptCommandExecuted(object source, ExecutedRoutedEventArgs e)
        {
            //    SaveChangesAndHide();
        }

        //private void SaveChangesAndHide()
        //{
        //    ClientSettings.Current.Save();
        //    Close();
        //}


    }
}