// SinglePlayerStartScreen.xaml.cs
//
// Copyright (c) 2007-2008 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Annotations;
using Supremacy.Client.Audio;
using Supremacy.Client.Dialogs;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;


namespace Supremacy.Client
{
    /// <summary>
    /// Interaction logic for SinglePlayerStartScreen.xaml
    /// </summary>
    public partial class SinglePlayerStartScreen
    {
        #region Field
        ISoundPlayer _soundPlayer = null;
        
        Boolean _startAudio = false;
        #endregion

        #region Constructors and Finalizers
        public SinglePlayerStartScreen([NotNull] ISoundPlayer soundPlayer)
        {
            if (soundPlayer == null)
                throw new ArgumentNullException("soundPlayer");
            _soundPlayer = soundPlayer;

            InitializeComponent();

            GameContext.PushThreadContext(new GameContext());

            try
            {
                DataContext = CivDatabase.Load()
                    .Where(civ => civ.CivilizationType == CivilizationType.Empire)
                    .ToList();
                CivSelector.SelectionChanged += CivSelector_SelectionChanged;

                //Select Federation to begin with
                List<Civilization> tmp = (List<Civilization>)DataContext;
                if (tmp[0].ShortName == "Intro")
                {
                    CivSelector.SelectedIndex = 1;
                }
                else
                {
                    CivSelector.SelectedIndex = 0;
                }
            }
            finally
            {
                GameContext.PopThreadContext();
            }
        }

        void CivSelector_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!_startAudio)
            {
                _startAudio = true;
                return;
            }
            if (CivSelector.SelectedIndex >= 0)
            {
                switch(CivSelector.SelectedValue.ToString())
                {
                    case "Federation":
                        _soundPlayer.Play("Menu", "FedSelection");
                        break;
                    case "Terran Empire":
                        _soundPlayer.Play("Menu", "TerranSelection");
                        break;
                    case "Romulans":
                        _soundPlayer.Play("Menu", "RomSelection");
                        break;
                    case "Klingons":
                        _soundPlayer.Play("Menu", "KlingSelection");
                        break;
                    case "Cardassians":
                        _soundPlayer.Play("Menu", "CardSelection");
                        break;
                    case "Dominion":
                        _soundPlayer.Play("Menu", "DomSelection");
                        break;
                    case "Borg":
                        _soundPlayer.Play("Menu", "BorgSelection");
                        break;
                }
            }
        }
        #endregion

        #region Properties and Indexers
        public int EmpireID
        {
            get { return CivSelector.SelectedIndex; }
        }

        public GameOptions Options
        {
            get { return OptionsPanel.Options; }
        }
        #endregion

        #region Private Methods

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // first check for races available

            // aim is to have this variable, also for translation: 
            // Just usage of civ and stuff in en.txt must fit manually, in en.txt Terran Empire can be translated into french "Empire"
            // only INSIDE CODE we use names like "RomulanPlayable" instead of "Civ_3_Playable"

            if (CivSelector.SelectedIndex >= 0)
            {
                switch (CivSelector.SelectedValue.ToString())
                {
                    case "Intro":
                        if (Options.IntroPlayable == EmpirePlayable.No)
                        {
                            MessageDialog.Show(Environment.NewLine +
                                ResourceManager.GetString("CIV_0_NOT_IN GAME"), MessageDialogButtons.Ok);
                            return;
                        }
                        break;
                    case "Federation":
                        if (Options.FederationPlayable == EmpirePlayable.No)
                        {
                            MessageDialog.Show(Environment.NewLine +
                                ResourceManager.GetString("CIV_1_NOT_IN GAME"), MessageDialogButtons.Ok);
                            return;
                        }
                        break;
                    case "Terran Empire":
                        if (Options.TerranEmpirePlayable == EmpirePlayable.No)
                        {
                            MessageDialog.Show(Environment.NewLine +
                                ResourceManager.GetString("CIV_2_NOT_IN GAME"), MessageDialogButtons.Ok);
                            return;
                        }
                        break;
                    case "Romulans":
                        if (Options.RomulanPlayable == EmpirePlayable.No)
                        {
                            MessageDialog.Show(Environment.NewLine +
                                ResourceManager.GetString("CIV_3_NOT_IN GAME"), MessageDialogButtons.Ok);
                            return;
                        }
                        break;
                    case "Klingons":
                        if (Options.KlingonPlayable == EmpirePlayable.No)
                        {
                            MessageDialog.Show(Environment.NewLine +
                                ResourceManager.GetString("CIV_4_NOT_IN GAME"), MessageDialogButtons.Ok);
                            return;
                        }
                        break;
                    case "Cardassians":
                        if (Options.CardassianPlayable == EmpirePlayable.No)
                        {
                            MessageDialog.Show(Environment.NewLine +
                                ResourceManager.GetString("CIV_5_NOT_IN GAME"), MessageDialogButtons.Ok);
                            return;
                        }
                        break;
                    case "Dominion":
                        if (Options.DominionPlayable == EmpirePlayable.No)
                        {
                            MessageDialog.Show(Environment.NewLine +
                                ResourceManager.GetString("CIV_6_NOT_IN GAME"), MessageDialogButtons.Ok);
                            return;
                        }
                        break;
                    case "Borg":
                        if (Options.BorgPlayable == EmpirePlayable.No)
                        {
                            MessageDialog.Show(Environment.NewLine +
                                ResourceManager.GetString("CIV_7_NOT_IN GAME"), MessageDialogButtons.Ok);
                            return;
                        }
                        break;
                }
            }

            _soundPlayer.Play("Menu", "LoadingGame");
            GameOptionsManager.SaveDefaults(OptionsPanel.Options);
            DialogResult = true;
        }

        #endregion
    }

    public static class BindableExtender
    {
        #region Constants
        public static readonly DependencyProperty BindableTextProperty =
            DependencyProperty.RegisterAttached(
                "BindableText",
                typeof(string),
                typeof(BindableExtender),
                new UIPropertyMetadata(
                    null,
                    BindableTextProperty_PropertyChanged));
        #endregion

        #region Public and Protected Methods
        public static string GetBindableText(DependencyObject obj)
        {
            return (string)obj.GetValue(BindableTextProperty);
        }

        public static void SetBindableText(
            DependencyObject obj,
            string value)
        {
            obj.SetValue(BindableTextProperty, value);
        }
        #endregion

        #region Private Methods
        private static void BindableTextProperty_PropertyChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is Run)
            {
                ((Run)dependencyObject).Text = (string)e.NewValue;
            }
        }
        #endregion
    }
}