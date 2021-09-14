// File:GameOptionsPanel.xaml.cs
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Microsoft.Practices.ServiceLocation;
using Supremacy.Game;
using Supremacy.Types;
using Supremacy.Utility;
using System;
using System.Windows.Media.Imaging;

namespace Supremacy.Client
{
    /// <summary>
    /// Interaction logic for GameOptionsPanel.xaml
    /// </summary>
    public partial class GameOptionsPanel
    {
        #region Fields
        private GameOptions _options;
        #endregion

        #region Events
        public event DefaultEventHandler OptionsChanged;
        #endregion

        #region Constructors
        public GameOptionsPanel()
        {
            InitializeComponent();

            //PlayerNameSPInput.Text = "Choose your name";
            lstGalaxySize.ItemsSource = EnumHelper.GetValues<GalaxySize>();
            lstGalaxyShape.ItemsSource = EnumHelper.GetValues<GalaxyShape>();
            lstPlanetDensity.ItemsSource = EnumHelper.GetValues<PlanetDensity>();
            lstStarDensity.ItemsSource = EnumHelper.GetValues<StarDensity>();
            lstMinorRaces.ItemsSource = EnumHelper.GetValues<MinorRaceFrequency>();
            lstGalaxyCanon.ItemsSource = EnumHelper.GetValues<GalaxyCanon>();
            lstTechLevel.ItemsSource = EnumHelper.GetValues<StartingTechLevel>();

            lstFederationPlayable.ItemsSource = EnumHelper.GetValues<EmpirePlayable>();
            lstRomulanPlayable.ItemsSource = EnumHelper.GetValues<EmpirePlayable>();
            lstKlingonPlayable.ItemsSource = EnumHelper.GetValues<EmpirePlayable>();
            lstCardassianPlayable.ItemsSource = EnumHelper.GetValues<EmpirePlayable>();
            lstDominionPlayable.ItemsSource = EnumHelper.GetValues<EmpirePlayable>();
            lstBorgPlayable.ItemsSource = EnumHelper.GetValues<EmpirePlayable>();
            lstTerranEmpirePlayable.ItemsSource = EnumHelper.GetValues<EmpirePlayable>();

            //lstModifierRecurringBalancing.ItemsSource = EnumHelper.GetValues<EmpireModifierRecurringBalancing>();
            //lstGamePace.ItemsSource = EnumHelper.GetValues<GamePace>();
            //lstTurnTimer.ItemsSource = EnumHelper.GetValues<TurnTimerEnum>();

            //lstFederationModifier.ItemsSource = EnumHelper.GetValues<EmpireModifier>();
            //lstRomulanModifier.ItemsSource = EnumHelper.GetValues<EmpireModifier>();
            //lstKlingonModifier.ItemsSource = EnumHelper.GetValues<EmpireModifier>();
            //lstCardassianModifier.ItemsSource = EnumHelper.GetValues<EmpireModifier>();
            //lstDominionModifier.ItemsSource = EnumHelper.GetValues<EmpireModifier>();
            //lstBorgModifier.ItemsSource = EnumHelper.GetValues<EmpireModifier>();
            //lstTerranEmpireModifier.ItemsSource = EnumHelper.GetValues<EmpireModifier>();

            //PlayerNameSPInput.SelectionChanged += (sender, args) => { OnOptionsChanged(); TrySetLastPlayerName(); };
            lstGalaxySize.SelectionChanged += (sender, args) => OnOptionsChanged();
            lstGalaxyShape.SelectionChanged += (sender, args) => { OnOptionsChanged(); UpdateGalaxyImage(); };
            lstPlanetDensity.SelectionChanged += (sender, args) => OnOptionsChanged();
            lstStarDensity.SelectionChanged += (sender, args) => OnOptionsChanged();
            lstMinorRaces.SelectionChanged += (sender, args) => OnOptionsChanged();
            lstGalaxyCanon.SelectionChanged += (sender, args) => OnOptionsChanged();
            lstTechLevel.SelectionChanged += (sender, args) => OnOptionsChanged();
            lstFederationPlayable.SelectionChanged += (sender, args) => { OnOptionsChanged(); };
            lstRomulanPlayable.SelectionChanged += (sender, args) => { OnOptionsChanged(); };
            lstKlingonPlayable.SelectionChanged += (sender, args) => { OnOptionsChanged(); };
            lstCardassianPlayable.SelectionChanged += (sender, args) => { OnOptionsChanged(); };
            lstDominionPlayable.SelectionChanged += (sender, args) => { OnOptionsChanged(); };
            lstBorgPlayable.SelectionChanged += (sender, args) => { OnOptionsChanged(); };
            lstTerranEmpirePlayable.SelectionChanged += (sender, args) => { OnOptionsChanged(); };

            //lstModifierRecurringBalancing.SelectionChanged += (sender, args) => { OnOptionsChanged(); };
            //lstGamePace.SelectionChanged += (sender, args) => { OnOptionsChanged(); };
            //lstTurnTimer.SelectionChanged += (sender, args) => { OnOptionsChanged(); };

            //lstFederationModifier.SelectionChanged += (sender, args) => { OnOptionsChanged(); };
            //lstRomulanModifier.SelectionChanged += (sender, args) => { OnOptionsChanged(); };
            //lstKlingonModifier.SelectionChanged += (sender, args) => { OnOptionsChanged(); };
            //lstCardassianModifier.SelectionChanged += (sender, args) => { OnOptionsChanged(); };
            //lstDominionModifier.SelectionChanged += (sender, args) => { OnOptionsChanged(); };
            //lstBorgModifier.SelectionChanged += (sender, args) => { OnOptionsChanged(); };
            //lstTerranEmpireModifier.SelectionChanged += (sender, args) => { OnOptionsChanged(); };

            //try
            //{
            //    PlayerNameSPInput.Text = StorageManager.ReadSetting<string, string>("LastPlayerName");
            //}
            //catch { }

            Loaded += (sender, args) => UpdateGalaxyImage();

            Options = ServiceLocator.Current.GetInstance<GameOptions>();
        }
        #endregion

        #region Properties
        public GameOptions Options
        {
            get
            {
                if (_options == null)
                {
                    _options = ServiceLocator.Current.GetInstance<GameOptions>();
                }

                return _options;
            }
            set
            {
                _options = value;
                DataContext = _options;
                if (IsLoaded)
                {
                    UpdateGalaxyImage();
                }
            }
        }
        #endregion

        #region Methods
        private void OnOptionsChanged()
        {
            OptionsChanged?.Invoke();
        }

        private void UpdateGalaxyImage()
        {
            try
            {
                BitmapImage imageSource = new BitmapImage(
                    new Uri(
                        "vfs:///Resources/Images/UI/Galaxies/" + lstGalaxyShape.SelectedItem + ".png",
                        UriKind.Absolute));
                GalaxyImage.Source = imageSource;
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
            }
        }

        void TryGetLastPlayerName(string PlayerNameSP)
        {
            try
            {
                //PlayerNameSPInput.Text = "R1D3";
                //PlayerNameSPInput.Text = StorageManager.ReadSetting<string, string>("LastPlayerName");
                //PlayerNameSP.CaretIndex = PlayerNameSP.Text.Length;
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
            }

        }

        void TrySetLastPlayerName()
        {
            try
            {
                // not finished yet

                //string playerName = PlayerNameSPInput.Text;
                //if (playerName.Length > 0)
                //{
                //    StorageManager.WriteSetting("LastPlayerName", playerName);
                //}
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
            }
        }

        private void UpdatePlayerNameSP()
        {
            try
            {
                TrySetLastPlayerName();
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
            }
        }

        #endregion
    }
}