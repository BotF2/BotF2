// <!-- File:shift_8_Dialog.xaml.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Game;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Supremacy.Client.Views;
using Supremacy.Encyclopedia;
using Supremacy.Resources;
using Supremacy.Tech;
using Supremacy.Utility;
using Microsoft.Practices.ServiceLocation;
using Supremacy.Client.Context;
using System.Windows.Navigation;

namespace Supremacy.Client
{ 
    /// <summary>
    /// Interaction logic for shift_8_Dialog.xaml.cs
    /// </summary>
    public partial class CTRL_shift_8_Dialog
    {
        public CTRL_shift_8_Dialog()
        {
            //InitializeComponent();
            //LoadEncyclopediaEntries();
            //OnApplyTemplate();
            IAppContext appContext = ServiceLocator.Current.GetInstance<IAppContext>();

            //_ = InputBindings.Add(
            //    new KeyBinding(
            //        GenericCommands.CancelCommand,
            //        Key.Escape,
            //        ModifierKeys.None));

            //_ = InputBindings.Add(
            //    new KeyBinding(
            //        GenericCommands.AcceptCommand,
            //        Key.Enter,
            //        ModifierKeys.None));

            //_ = CommandBindings.Add(
            //    new CommandBinding(
            //        GenericCommands.CancelCommand,
            //        OnGenericCommandsCancelCommandExecuted));

            //_ = CommandBindings.Add(
            //    new CommandBinding(
            //        GenericCommands.AcceptCommand,
            //        OnGenericCommandsAcceptCommandExecuted));

            GameLog.Client.UIDetails.DebugFormat("shift_4-Dialog initialized");

        }


        private void DoLink(object sender, RequestNavigateEventArgs e)
        {
            //Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }


        private void OnGenericCommandsCancelCommandExecuted(object source, ExecutedRoutedEventArgs e)
        {
            ClientSettings.Current.Reload();
            //Close();
        }

        private void OnGenericCommandsAcceptCommandExecuted(object source, ExecutedRoutedEventArgs e)
        {
            SaveChangesAndHide();
        }

        private void SaveChangesAndHide()
        {
            ClientSettings.Current.Save();
            //Close();
        }

        private void OnGenericCommandsTracesSetAllwithoutDetailsCommandExecuted(object source, ExecutedRoutedEventArgs e)
        {
            ClientSettings.Current.TracesAudio = true;

            ClientSettings.Current.Save();
            ClientSettings.Current.Reload();
        }

        private void OnGenericCommandsTracesSetSomeCommandExecuted(object source, ExecutedRoutedEventArgs e)
        {
            ClientSettings.Current.TracesAudio = false;

            ClientSettings.Current.Save();
            ClientSettings.Current.Reload();
        }

        private void OnGenericCommandsTracesSetNoneCommandExecuted(object source, ExecutedRoutedEventArgs e)
        {
            //ClientSettings.Traces_ClearAllProperty();
            ClientSettings.Current.TracesAudio = false;

            ClientSettings.Current.Save();
            ClientSettings.Current.Reload();
        }
    }
}