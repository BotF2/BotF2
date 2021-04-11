// <!-- File:f08_Dialog.xaml.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Microsoft.Practices.ServiceLocation;
using Supremacy.Client.Context;
using Supremacy.Game;
using Supremacy.Orbitals;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Supremacy.Client
{

    /// <summary>
    /// Interaction logic for f08_Dialog.xaml.cs
    /// </summary>
    public partial class F08_Dialog 
    {

        //private string _text = "text";
        //private string F08_Text_1 = "IsHitTestVisibleProperty";
        //private List<Fleet> _allFleets;
        //private Fleet _testFleet;
        private readonly string T2;
        //private IAppContext _appContext;

        //GameContext.Current.

        //_allFleets = GameContext.Current.Universe.Find<Fleet>().ToList();
        public F08_Dialog()
        {
            InitializeComponent();

            //_appContext = ServiceLocator.Current.GetInstance<IAppContext>();
            DataTemplate itemTemplate = TryFindResource("AssetsTreeItemTemplate") as DataTemplate;

            //string t1 = F08_Text_1;
            string t1 = F08_Text_1;
            //T1 = t1;

            string t2 = F08_Text_2;
            T2 = t2;
            t2 = T2;

            string t3 = F08_Text_3;
            //T3 = t2;

            //if (GameContext.Current != null)
            //    _allFleets = GameContext.Current.Universe.Find<Fleet>().ToList();

            //_testFleet = new Fleet();
            //_testFleet.TurnCreated = 1;
            //DataContext = _testFleet;

            //if (_allFleets != null)
            //    DataContext = _allFleets;

            //T1.Value = "hel";
            //string t3 = F08_Text_3;

            //string F08_Text_1 = "IsHitTestVisibleProperty";
            //string F08_Text_3 = "IsHitTestVisibleProperty";
            //string Empire = "IsHitTestVisibleProperty";
            //_allFleets = GameContext.Current.Universe.Find<Fleet>().ToList();

            InputBindings.Add(
                new KeyBinding(
                    GenericCommands.CancelCommand,
                    Key.Escape,
                    ModifierKeys.None));

            InputBindings.Add(
                new KeyBinding(
                    GenericCommands.AcceptCommand,
                    Key.Enter,
                    ModifierKeys.None));

            CommandBindings.Add(
                new CommandBinding(
                    GenericCommands.CancelCommand,
                    OnGenericCommandsCancelCommandExecuted));

            CommandBindings.Add(
                new CommandBinding(
                    GenericCommands.AcceptCommand,
                    OnGenericCommandsAcceptCommandExecuted));

            //CommandBindings.Add(
            //    new CommandBinding(
            //        GenericCommands.TracesSetAllwithoutDetailsCommand,
            //        OnGenericCommandsTracesSetAllwithoutDetailsCommandExecuted));

            //CommandBindings.Add(
            //    new CommandBinding(
            //        GenericCommands.TracesSetSomeCommand,
            //        OnGenericCommandsTracesSetSomeCommandExecuted));

            //CommandBindings.Add(
            //    new CommandBinding(
            //        GenericCommands.TracesSetNoneCommand,
            //        OnGenericCommandsTracesSetNoneCommandExecuted));


        }

        private class F08_Dialog_Class
        {
            //string _f08_Text_1 = "IsHitTestVisibleProperty";
            //public string F08_Text_1
            //{
            //    get { return "hello 8" /*_text1*/; }
            //}
            //    private class F08_Text_1 { return "hello 8" /*_text1*/; }

            //}


        }

        public string F08_Text_1
        {
            get { return "hello 8" /*_text1*/; }
        }

        public string F08_Text_2
        {
            get { return "hello 8" /*_text1*/; }
        }

        private string F08_Text_3
        {
            get { return "hello 8" /*_text1*/; }
        }

        private void OnGenericCommandsCancelCommandExecuted(object source, ExecutedRoutedEventArgs e)
        {
            ClientSettings.Current.Reload();
            Close();
        }

        private void OnGenericCommandsAcceptCommandExecuted(object source, ExecutedRoutedEventArgs e)
        {
            SaveChangesAndHide();
        }

        private void SaveChangesAndHide()
        {
            ClientSettings.Current.Save();
            Close();
        }

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