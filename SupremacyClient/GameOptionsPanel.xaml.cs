// GameOptionsPanel.xaml.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Windows.Media.Imaging;

using Microsoft.Practices.ServiceLocation;

using Supremacy.Game;
using Supremacy.Types;
using Supremacy.Utility;
using System.Windows.Controls;
using Supremacy.IO;

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

            //PlayerNameSPInput.Text = "choice your name";
            lstGalaxySize.ItemsSource = EnumHelper.GetValues<GalaxySize>();
            lstGalaxyShape.ItemsSource = EnumHelper.GetValues<GalaxyShape>();
            lstPlanetDensity.ItemsSource = EnumHelper.GetValues<PlanetDensity>();
            lstStarDensity.ItemsSource = EnumHelper.GetValues<StarDensity>();
            lstMinorRaces.ItemsSource = EnumHelper.GetValues<MinorRaceFrequency>();
            lstTechLevel.ItemsSource = EnumHelper.GetValues<StartingTechLevel>();
            //lstIntroPlayable.ItemsSource = EnumHelper.GetValues<IntroPlayable>();
            lstFederationPlayable.ItemsSource = EnumHelper.GetValues<EmpirePlayable>();
            lstRomulanPlayable.ItemsSource = EnumHelper.GetValues<EmpirePlayable>();
            lstKlingonPlayable.ItemsSource = EnumHelper.GetValues<EmpirePlayable>();
            lstCardassianPlayable.ItemsSource = EnumHelper.GetValues<EmpirePlayable>();
            lstDominionPlayable.ItemsSource = EnumHelper.GetValues<EmpirePlayable>();
            lstBorgPlayable.ItemsSource = EnumHelper.GetValues<EmpirePlayable>();
            lstTerranEmpirePlayable.ItemsSource = EnumHelper.GetValues<EmpirePlayable>();

            //PlayerNameSPInput.SelectionChanged += (sender, args) => { OnOptionsChanged(); TrySetLastPlayerName(); };
            lstGalaxySize.SelectionChanged += (sender,args) => OnOptionsChanged();
            lstGalaxyShape.SelectionChanged += (sender, args) => { OnOptionsChanged(); UpdateGalaxyImage(); };
            lstPlanetDensity.SelectionChanged += (sender, args) => OnOptionsChanged();
            lstStarDensity.SelectionChanged += (sender, args) => OnOptionsChanged();
            lstMinorRaces.SelectionChanged += (sender, args) => OnOptionsChanged();
            lstTechLevel.SelectionChanged += (sender, args) => OnOptionsChanged();
            //lstIntroPlayable.SelectionChanged += (sender, args) => { OnOptionsChanged(); UpdateIntroPlayable(); };
            lstFederationPlayable.SelectionChanged += (sender, args) => { OnOptionsChanged(); UpdateFederationPlayable(); };
            lstRomulanPlayable.SelectionChanged += (sender, args) => { OnOptionsChanged(); UpdateRomulanPlayable(); };
            lstKlingonPlayable.SelectionChanged += (sender, args) => { OnOptionsChanged(); UpdateKlingonPlayable(); };
            lstCardassianPlayable.SelectionChanged += (sender, args) => { OnOptionsChanged(); UpdateCardassianPlayable(); };
            lstDominionPlayable.SelectionChanged += (sender, args) => { OnOptionsChanged(); UpdateDominionPlayable(); };
            lstBorgPlayable.SelectionChanged += (sender, args) => { OnOptionsChanged(); UpdateBorgPlayable(); };
            lstTerranEmpirePlayable.SelectionChanged += (sender, args) => { OnOptionsChanged(); UpdateTerranEmpirePlayable(); };

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
                    _options = ServiceLocator.Current.GetInstance<GameOptions>();
                return _options;
            }
            set
            {
                _options = value;
                DataContext = _options;
                if (IsLoaded)
                    UpdateGalaxyImage();
            }
        }
        #endregion

        #region Methods
        private void OnOptionsChanged()
        {
            if (OptionsChanged != null)
                OptionsChanged();
        }

        private void UpdateGalaxyImage()
        {
            try
            {
                var imageSource = new BitmapImage(
                    new Uri(
                        "vfs:///Resources/Images/Galaxies/" + lstGalaxyShape.SelectedItem + ".png",
                        UriKind.Absolute));
                GalaxyImage.Source = imageSource;
            }
            catch (Exception e) //ToDo: how to handle this exception? Set to default "missing image"?
            {
                GameLog.LogException(e);
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
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
            }

        }

        void TrySetLastPlayerName()
        {
            try
            {
                string playerName = PlayerNameSPInput.Text;
                if (playerName.Length > 0)
                {
                    StorageManager.WriteSetting("LastPlayerName", playerName);
                }
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
            }
        }


        private void UpdateIntroPlayable()
        {
            try
            {

            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
            }
        }

        private void UpdatePlayerNameSP()
        {
            try
            {
                TrySetLastPlayerName();
            }
            catch (Exception e) //ToDo: how to handle this exception? Set to default "missing image"?
            {
                GameLog.LogException(e);
            }
        }

        private void UpdateFederationPlayable()
        {
            try
            {

                // needed, before turned back to YES/NO instead of Federations/Terrans/No

                //GameLog.Client.GameData.DebugFormat("GameOptionsPanel.xaml.cs: in general...  lstFederationPlayable={0}; lstTerranEmpirePlayable={0}",
                //                                        lstFederationPlayable.SelectedValue, lstTerranEmpirePlayable.SelectedValue);

                //if (lstFederationPlayable.SelectedIndex == 0)   //ToString() == "Federation")
                //{
                //    //GameLog.Client.GameData.DebugFormat("GameOptionsPanel.xaml.cs: lstFederationPlayable={0}; lstTerranEmpirePlayable={1}",
                //    //                                                       lstFederationPlayable.SelectedValue, lstTerranEmpirePlayable.SelectedValue);
                //    lstTerranEmpirePlayable.SelectedIndex = 0;   // 0 = No Terran,   1 = Terran YES
                //}

                //if (lstFederationPlayable.SelectedIndex == 1)    // ToString() == "Terrans")
                //{
                //    //GameLog.Client.GameData.DebugFormat("GameOptionsPanel.xaml.cs: lstFederationPlayable={0}; lstTerranEmpirePlayable={1}",
                //    //                                                       lstFederationPlayable.SelectedValue, lstTerranEmpirePlayable.SelectedValue);
                //    lstTerranEmpirePlayable.SelectedIndex = 1;   // 0 = No Terran,   1 = Terran YES
                //}

                //if (lstFederationPlayable.SelectedIndex == 2)    //   ToString() == "No")
                //{ 
                //    //GameLog.Client.GameData.DebugFormat("GameOptionsPanel.xaml.cs: lstFederationPlayable={0}; lstTerranEmpirePlayable={1}",
                //    //                                                        lstFederationPlayable.SelectedValue, lstTerranEmpirePlayable.SelectedValue);
                //}
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
            }
        }

        private void UpdateTerranEmpirePlayable()
        {
            try
            {
                //lst
                // needed, before turned back to YES/NO instead of Federations/Terrans/No


                //GameLog.Client.GameData.DebugFormat("GameOptionsPanel.xaml.cs: in general...  lstTerranEmpirePlayable={0}; lstFederationPlayable={0}",
                //                                        lstTerranEmpirePlayable.SelectedValue, lstFederationPlayable.SelectedValue);

                //if (lstTerranEmpirePlayable.SelectedIndex == 0)   //   "No")
                //{
                //    //GameLog.Client.GameData.DebugFormat("GameOptionsPanel.xaml.cs: lstTerranEmpirePlayable={0}; lstFederationPlayable={1}",
                //    //lstTerranEmpirePlayable.SelectedValue, lstFederationPlayable.SelectedValue);
                //    lstFederationPlayable.SelectedIndex = 0;   // 0 = Federation
                //}

                //if (lstTerranEmpirePlayable.SelectedIndex == 1)    //  "Yes")
                //{
                //    //GameLog.Client.GameData.DebugFormat("GameOptionsPanel.xaml.cs: lstTerranEmpirePlayable={0}; lstFederationPlayable={1}",
                //    //lstTerranEmpirePlayable.SelectedValue, lstFederationPlayable.SelectedValue);
                //    lstFederationPlayable.SelectedIndex = 1;   // 1 = Terrans 
                //}

                //// Terrans just have YES or NO
                ////if (lstTerranEmpirePlayable.SelectedIndex == 2)    //   ToString() == "No")
                ////{
                ////    //GameLog.Client.GameData.DebugFormat("GameOptionsPanel.xaml.cs: lstTerranEmpirePlayable={0}; lstFederationPlayable={1}",
                ////    lstTerranEmpirePlayable.SelectedValue, lstFederationPlayable.SelectedValue);
                ////}
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
            }
        }

        private void UpdateRomulanPlayable()
        {
            try
            {
                // at least if "no" on the right side then grey out the left side
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
            }
        }

        private void UpdateKlingonPlayable()
        {
            try
            {
                // at least if "no" on the right side then grey out the left side
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
            }
        }

        private void UpdateCardassianPlayable()
        {
            try
            {
                // at least if "no" on the right side then grey out the left side
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
            }
        }

        private void UpdateDominionPlayable()
        {
            try
            {
                // at least if "no" on the right side then grey out the left side
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
            }
        }

        private void UpdateBorgPlayable()
        {
            try
            {
                // at the moment no idea
                
                //var imageSource = new BitmapImage(
                //    new Uri(
                //        "vfs:///Resources/Images/Galaxies/" + this.lstGalaxyShape.SelectedItem + ".png",
                //        UriKind.Absolute));
                //GalaxyImage.Source = imageSource;
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
            }
        }


        
        #endregion
    }
}