// <!-- File:f09_Dialog.xaml.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Utility;
using System.Windows.Input;

namespace Supremacy.Client
{
    /// <summary>
    /// Interaction logic for f09_Dialog.xaml.cs
    /// </summary>
    public partial class F09_Dialog
    {
        public F09_Dialog()
        {
            InitializeComponent();

            _ = InputBindings.Add(
                new KeyBinding(
                    GenericCommands.CancelCommand,
                    Key.Escape,
                    ModifierKeys.None));

            _ = InputBindings.Add(
                new KeyBinding(
                    GenericCommands.AcceptCommand,
                    Key.Enter,
                    ModifierKeys.None));

            _ = CommandBindings.Add(
                new CommandBinding(
                    GenericCommands.CancelCommand,
                    OnGenericCommandsCancelCommandExecuted));

            _ = CommandBindings.Add(
                new CommandBinding(
                    GenericCommands.AcceptCommand,
                    OnGenericCommandsAcceptCommandExecuted));

            //_ = CommandBindings.Add(
            //    new CommandBinding(
            //        GenericCommands.CancelCommand,
            //        OnGenericCommandsCancelCommandExecuted));

            //_ = CommandBindings.Add(
            //    new CommandBinding(
            //        GenericCommands.AcceptCommand,
            //        OnGenericCommandsAcceptCommandExecuted));

            //_ = CommandBindings.Add(
            //    new CommandBinding(
            //        GenericCommands.TracesSetAllwithoutDetailsCommand,
            //        OnGenericCommandsTracesSetAllwithoutDetailsCommandExecuted));

            //_ = CommandBindings.Add(
            //    new CommandBinding(
            //        GenericCommands.TracesSetSomeCommand,
            //        OnGenericCommandsTracesSetSomeCommandExecuted));

            //_ = CommandBindings.Add(
            //    new CommandBinding(
            //        GenericCommands.TracesSetNoneCommand,
            //        OnGenericCommandsTracesSetNoneCommandExecuted));

            GameLog.Client.UIDetails.DebugFormat("F09-Dialog initialized");
        }

        private void OnGenericCommandsCancelCommandExecuted(object source, ExecutedRoutedEventArgs e)
        {
            //ClientSettings.Current.Reload();
            Close();
        }

        private void OnGenericCommandsAcceptCommandExecuted(object source, ExecutedRoutedEventArgs e)
        {
            //SaveChangesAndHide();
            Close();
        }

        //private void SaveChangesAndHide()
        //{
        //    ClientSettings.Current.Save();
        //    Close();
        //}

        //private void OnGenericCommandsTracesSetAllwithoutDetailsCommandExecuted(object source, ExecutedRoutedEventArgs e)
        //{
        //    ClientSettings.Current.TracesAudio = true;

        //    ClientSettings.Current.Save();
        //    ClientSettings.Current.Reload();
        //}

        //private void OnGenericCommandsTracesSetSomeCommandExecuted(object source, ExecutedRoutedEventArgs e)
        //{
        //    ClientSettings.Current.TracesAudio = false;

        //    ClientSettings.Current.Save();
        //    ClientSettings.Current.Reload();
        //}

        //private void OnGenericCommandsTracesSetNoneCommandExecuted(object source, ExecutedRoutedEventArgs e)
        //{
        //    //ClientSettings.Traces_ClearAllProperty();
        //    ClientSettings.Current.TracesAudio = false;

        //    ClientSettings.Current.Save();
        //    ClientSettings.Current.Reload();
        //}
    }
}