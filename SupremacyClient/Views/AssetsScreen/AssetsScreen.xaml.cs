using Supremacy.Client.Context;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Intelligence;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;

using System.Windows.Media.Imaging;

namespace Supremacy.Client.Views
{
    /// <summary>
    /// Interaction logic for AssetsScreen.xaml
    /// </summary>
    public partial class AssetsScreen : IAssetsScreenView
    {
        Civilization civLocalPlayer = DesignTimeObjects.CivilizationManager.Civilization;
        Civilization SpiedZeroCiv = DesignTimeObjects.SpiedCivZero.Civilization;
        Civilization SpiedOneCiv = DesignTimeObjects.SpiedCivOne.Civilization;
        Civilization SpiedTwoCiv = DesignTimeObjects.SpiedCivTwo.Civilization;
        Civilization SpiedThreeCiv = DesignTimeObjects.SpiedCivThree.Civilization;
        Civilization SpiedFourCiv = DesignTimeObjects.SpiedCivFour.Civilization;
        Civilization SpiedFiveCiv = DesignTimeObjects.SpiedCivFive.Civilization;
        Civilization SpiedSixCiv = DesignTimeObjects.SpiedCivSix.Civilization;
        public AssetsScreen()
        {
            InitializeComponent();
            IsVisibleChanged += OnIsVisibleChanged;
            BlameNoOneFed.Visibility = Visibility.Visible;
            BlameNoOneFed.IsChecked = true;
            BlameTerroristsFed.Visibility = Visibility.Visible;

            BlameNoOneTerran.Visibility = Visibility.Visible;
            BlameNoOneTerran.IsChecked = true;
            BlameTerroristsTerran.Visibility = Visibility.Visible;

            BlameNoOneRom.Visibility = Visibility.Visible;
            BlameNoOneRom.IsChecked = true;
            BlameTerroristsRom.Visibility = Visibility.Visible;

            BlameNoOneKling.Visibility = Visibility.Visible;
            BlameNoOneKling.IsChecked = true;
            BlameTerroristsKling.Visibility = Visibility.Visible;

            BlameNoOneCard.Visibility = Visibility.Visible;
            BlameNoOneCard.IsChecked = true;
            BlameTerroristsCard.Visibility = Visibility.Visible;

            BlameNoOneDom.Visibility = Visibility.Visible;
            BlameNoOneDom.IsChecked = true;
            BlameTerroristsDom.Visibility = Visibility.Visible;

            BlameNoOneBorg.Visibility = Visibility.Visible;
            BlameNoOneBorg.IsChecked = true;
            BlameTerroristsBorg.Visibility = Visibility.Visible;

            //BlameFederation.Visibility = Visibility.Collapsed;
            BlameTerranEmpireFed.Visibility = Visibility.Collapsed;
            BlameRomulansFed.Visibility = Visibility.Collapsed;
            BlameKlingonsFed.Visibility = Visibility.Collapsed;
            BlameCardassiansFed.Visibility = Visibility.Collapsed;
            BlameDominionFed.Visibility = Visibility.Collapsed;
            BlameBorgFed.Visibility = Visibility.Collapsed;

            BlameFederationTerran.Visibility = Visibility.Collapsed;
            //BlameTerranEmpireFed.Visibility = Visibility.Collapsed;
            BlameRomulansTerran.Visibility = Visibility.Collapsed;
            BlameKlingonsTerran.Visibility = Visibility.Collapsed;
            BlameCardassiansTerran.Visibility = Visibility.Collapsed;
            BlameDominionTerran.Visibility = Visibility.Collapsed;
            BlameBorgTerran.Visibility = Visibility.Collapsed;

            BlameFederationRom.Visibility = Visibility.Collapsed;
            BlameTerranEmpireRom.Visibility = Visibility.Collapsed;
            //BlameRomulansRom.Visibility = Visibility.Collapsed;
            BlameKlingonsRom.Visibility = Visibility.Collapsed;
            BlameCardassiansRom.Visibility = Visibility.Collapsed;
            BlameDominionRom.Visibility = Visibility.Collapsed;
            BlameBorgRom.Visibility = Visibility.Collapsed;

            BlameFederationKling.Visibility = Visibility.Collapsed;
            BlameTerranEmpireKling.Visibility = Visibility.Collapsed;
            BlameRomulansKling.Visibility = Visibility.Collapsed;
            //BlameKlingonsKling.Visibility = Visibility.Collapsed;
            BlameCardassiansKling.Visibility = Visibility.Collapsed;
            BlameDominionKling.Visibility = Visibility.Collapsed;
            BlameBorgKling.Visibility = Visibility.Collapsed;

            BlameFederationCard.Visibility = Visibility.Collapsed;
            BlameTerranEmpireCard.Visibility = Visibility.Collapsed;
            BlameRomulansCard.Visibility = Visibility.Collapsed;
            BlameKlingonsCard.Visibility = Visibility.Collapsed;
            //BlameCardassiansCard.Visibility = Visibility.Collapsed;
            BlameDominionCard.Visibility = Visibility.Collapsed;
            BlameBorgCard.Visibility = Visibility.Collapsed;

            BlameFederationDom.Visibility = Visibility.Collapsed;
            BlameTerranEmpireDom.Visibility = Visibility.Collapsed;
            BlameRomulansDom.Visibility = Visibility.Collapsed;
            BlameKlingonsDom.Visibility = Visibility.Collapsed;
            BlameCardassiansDom.Visibility = Visibility.Collapsed;
            //BlameDominionDom.Visibility = Visibility.Collapsed;
            BlameBorgDom.Visibility = Visibility.Collapsed;

            BlameFederationBorg.Visibility = Visibility.Collapsed;
            BlameTerranEmpireBorg.Visibility = Visibility.Collapsed;
            BlameRomulansBorg.Visibility = Visibility.Collapsed;
            BlameKlingonsBorg.Visibility = Visibility.Collapsed;
            BlameCardassiansBorg.Visibility = Visibility.Collapsed;
            BlameDominionBorg.Visibility = Visibility.Collapsed;
            //BlameBorgBorg.Visibility = Visibility.Collapsed;

            LoadInsignia();
        }
        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                ResumeAnimations();
                GameLog.Client.UI.DebugFormat("begin of checking visible");
                if (SpiedZeroCiv == civLocalPlayer || !AssetsHelper.IsSpiedSix(SpiedZeroCiv))
                {
                    GameLog.Client.UI.DebugFormat("SpiedSixCiv checking visible .... ");

                    EmpireExpanderFed.Visibility = Visibility.Collapsed;
                }
                if (SpiedOneCiv == civLocalPlayer||!AssetsHelper.IsSpiedOne(SpiedOneCiv))
                {
                    GameLog.Client.UI.DebugFormat("SpiedOneCiv checking visible .... ");
                    EmpireExpanderTerran.Visibility = Visibility.Collapsed;
                }
                //else
                //{
                //    if (SpiedOneCiv != civLocalPlayer)
                //    {
                //        EmpireExpanderFed.Visibility = Visibility.Collapsed;

                //        //SabotageEnergyOne.Visibility = Visibility.Visible;
                //        //SabotageFoodOne.Visibility = Visibility.Visible;
                //        //SabotageIndustryOne.Visibility = Visibility.Visible;
                //        //StealResearchOne.Visibility = Visibility.Visible;
                //    }
                //}

                if (SpiedTwoCiv == civLocalPlayer || !AssetsHelper.IsSpiedTwo(SpiedTwoCiv))
                {
                    GameLog.Client.UI.DebugFormat("SpiedTwoCiv checking visible .... ");
                    EmpireExpanderRom.Visibility = Visibility.Collapsed;
                }
                //else
                //{
                //    if (SpiedTwoCiv != civLocalPlayer)
                //    {
                //        EmpireExpanderTwo.Visibility = Visibility.Visible;

                //        //SabotageEnergyTwo.Visibility = Visibility.Visible;
                //        //SabotageFoodTwo.Visibility = Visibility.Visible;
                //        //SabotageIndustryTwo.Visibility = Visibility.Visible;
                //        //StealResearchTwo.Visibility = Visibility.Visible;
                //    }
                //}

                if (SpiedThreeCiv == civLocalPlayer || !AssetsHelper.IsSpiedThree(SpiedThreeCiv))
                {
                    GameLog.Client.UI.DebugFormat("SpiedThreeCiv checking visible .... ");
                    EmpireExpanderKling.Visibility = Visibility.Collapsed;
                }
                //else
                //{
                //    if (SpiedThreeCiv != civLocalPlayer)
                //    {
                //        EmpireExpanderThree.Visibility = Visibility.Visible;

                //        //SabotageEnergyThree.Visibility = Visibility.Visible;
                //        //SabotageFoodThree.Visibility = Visibility.Visible;
                //        //SabotageIndustryThree.Visibility = Visibility.Visible;
                //        //StealResearchThree.Visibility = Visibility.Visible;
                //    }
                //}

                if (SpiedFourCiv == civLocalPlayer || !AssetsHelper.IsSpiedFour(SpiedFourCiv))
                {
                    GameLog.Client.UI.DebugFormat("SpiedFourCiv checking visible .... ");
                    EmpireExpanderCard.Visibility = Visibility.Collapsed;
                }
                //else
                //{
                //    if (SpiedFourCiv != civLocalPlayer)
                //    {
                //        EmpireExpanderFour.Visibility = Visibility.Visible;

                //        //SabotageEnergyFour.Visibility = Visibility.Visible;
                //        //SabotageFoodFour.Visibility = Visibility.Visible;
                //        //SabotageIndustryFour.Visibility = Visibility.Visible;
                //        //StealResearchFour.Visibility = Visibility.Visible;
                //    }
                //}

                if (SpiedFiveCiv == civLocalPlayer || !AssetsHelper.IsSpiedFive(SpiedFiveCiv))
                {
                    GameLog.Client.UI.DebugFormat("SpiedFiveCiv checking visible .... ");
                    EmpireExpanderDom.Visibility = Visibility.Collapsed;
                }
                //else
                //{
                //    if (SpiedFiveCiv != civLocalPlayer)
                //    {
                //        EmpireExpanderFive.Visibility = Visibility.Visible;

                //        //SabotageEnergyFive.Visibility = Visibility.Visible;
                //        //SabotageFoodFive.Visibility = Visibility.Visible;
                //        //SabotageIndustryFive.Visibility = Visibility.Visible;
                //        //StealResearchFive.Visibility = Visibility.Visible;
                //    }
                //}
                if (SpiedSixCiv == civLocalPlayer || !AssetsHelper.IsSpiedSix(SpiedSixCiv))
                {
                    GameLog.Client.UI.DebugFormat("SpiedSixCiv checking visible .... ");

                    EmpireExpanderBorg.Visibility = Visibility.Collapsed;
                }
                //else
                //{
                //    if (SpiedSixCiv != civLocalPlayer)

                //    {
                //        EmpireExpanderSix.Visibility = Visibility.Visible;

                //        //SabotageEnergySix.Visibility = Visibility.Visible;
                //        //SabotageFoodSix.Visibility = Visibility.Visible;
                //        //SabotageIndustrySix.Visibility = Visibility.Visible;
                //        //StealResearchSix.Visibility = Visibility.Visible;
                //    }
                //}

                //GameLog.Client.UI.DebugFormat("in the middle of checking visible .... ");

                GameLog.Client.UI.DebugFormat("end  of checking visible");

                Dictionary<int, Civilization> sevenCivs = new Dictionary<int, Civilization>();
                int index = 0;
                foreach (var civInDictionary in GameContext.Current.Civilizations) 
                {
                    if (civInDictionary.IsEmpire)
                    {
                        sevenCivs.Add(civInDictionary.CivID, civInDictionary);
                        index++;
                    }
                    if (index >= 7)
                        break;
                }

                GameLog.Client.UI.DebugFormat("FED: begin of checking BLAME visible");
                if (sevenCivs.Keys.Contains(0) && AppContext.LocalPlayer.Empire.Key != "FEDERATION" &&
                    GameContext.Current.DiplomacyData[AssetsScreenPresentationModel.Local, sevenCivs[0]].IsContactMade())
                {
                  //  BlameFederationTerran.Visibility = Visibility.Visible;
                    BlameFederationRom.Visibility = Visibility.Visible;
                  //  BlameFederationKling.Visibility = Visibility.Visible;
                    BlameFederationCard.Visibility = Visibility.Visible;
                  //  BlameFederationDom.Visibility = Visibility.Visible;
                   // BlameFederationBorg.Visibility = Visibility.Visible;
                }
                GameLog.Client.UI.DebugFormat("FED: end   of checking BLAME visible");
                if (sevenCivs.Keys.Contains(1) && AppContext.LocalPlayer.Empire.Key != "TERRANEMPIRE" &&
                    GameContext.Current.DiplomacyData[AssetsScreenPresentationModel.Local, sevenCivs[1]].IsContactMade())
                {
                 //   BlameTerranEmpireFed.Visibility = Visibility.Visible;
                    BlameTerranEmpireRom.Visibility = Visibility.Visible;
                 //   BlameTerranEmpireKling.Visibility = Visibility.Visible;
                    BlameTerranEmpireCard.Visibility = Visibility.Visible;
                 //   BlameTerranEmpireDom.Visibility = Visibility.Visible;
                 //   BlameTerranEmpireBorg.Visibility = Visibility.Visible;
                }
                if (sevenCivs.Keys.Contains(2) && AppContext.LocalPlayer.Empire.Key != "ROMULANS" &&
                    GameContext.Current.DiplomacyData[AssetsScreenPresentationModel.Local, sevenCivs[2]].IsContactMade())
                {
                  //  BlameRomulansFed.Visibility = Visibility.Visible;
                 //   BlameRomulansTerran.Visibility = Visibility.Visible;
                  //  BlameRomulansKling.Visibility = Visibility.Visible;
                    BlameRomulansCard.Visibility = Visibility.Visible;
                  //  BlameRomulansDom.Visibility = Visibility.Visible;
                  //  BlameRomulansBorg.Visibility = Visibility.Visible;
                }
                if (sevenCivs.Keys.Contains(3) && AppContext.LocalPlayer.Empire.Key != "KLINGONS" &&
                    GameContext.Current.DiplomacyData[AssetsScreenPresentationModel.Local, sevenCivs[3]].IsContactMade())
                {
                 //   BlameKlingonsFed.Visibility = Visibility.Visible;
                  //  BlameKlingonsTerran.Visibility = Visibility.Visible;
                    BlameKlingonsRom.Visibility = Visibility.Visible;
                    BlameKlingonsCard.Visibility = Visibility.Visible;
                  //  BlameKlingonsDom.Visibility = Visibility.Visible;
                   // BlameKlingonsBorg.Visibility = Visibility.Visible;
                }
                GameLog.Client.UI.DebugFormat("CARD: begin of checking BLAME visible");
                if (sevenCivs.Keys.Contains(4) && AppContext.LocalPlayer.Empire.Key != "CARDASSIANS" &&
                    GameContext.Current.DiplomacyData[AssetsScreenPresentationModel.Local, sevenCivs[4]].IsContactMade()) // && sevenCivs[4].Key != "CARDASSIANS")
                {
                 //   BlameCardassiansFed.Visibility = Visibility.Visible;
                  //  BlameCardassiansTerran.Visibility = Visibility.Visible;
                    BlameCardassiansRom.Visibility = Visibility.Visible;
                  //  BlameCardassiansKling.Visibility = Visibility.Visible;
                  //  BlameCardassiansCard.Visibility = Visibility.Visible;
                  //  BlameCardassiansDom.Visibility = Visibility.Visible;
                    //BlameCardassiansBorg.Visibility = Visibility.Visible;
                
                }
                GameLog.Client.UI.DebugFormat("CARD: end   of checking BLAME visible");
                if (sevenCivs.Keys.Contains(5) && AppContext.LocalPlayer.Empire.Key != "DOMINION" &&
                    GameContext.Current.DiplomacyData[AssetsScreenPresentationModel.Local, sevenCivs[5]].IsContactMade())
                {
                   // BlameDominionFed.Visibility = Visibility.Visible;
                   // BlameDominionTerran.Visibility = Visibility.Visible;
                    BlameDominionRom.Visibility = Visibility.Visible;
                   // BlameDominionKling.Visibility = Visibility.Visible;
                    BlameDominionCard.Visibility = Visibility.Visible;
                   // BlameDominionBorg.Visibility = Visibility.Visible;
                }

                if (sevenCivs.Keys.Contains(6) && AppContext.LocalPlayer.Empire.Key != "BORG" &&
                    GameContext.Current.DiplomacyData[AssetsScreenPresentationModel.Local, sevenCivs[6]].IsContactMade())
                {
                    //GameLog.Client.Intel.DebugFormat("**************fed ={0} borg={1} fed borg is contact made {2}", AssetsScreenPresentationModel.Local.Key, sevenCivs[6].Key, GameContext.Current.DiplomacyData[AssetsScreenPresentationModel.Local, sevenCivs[6]].IsContactMade());
                   // BlameBorgFed.Visibility = Visibility.Visible;
                    //BlameBorgTerran.Visibility = Visibility.Visible;
                    BlameBorgRom.Visibility = Visibility.Visible;
                    //BlameBorgKling.Visibility = Visibility.Visible;
                    BlameBorgCard.Visibility = Visibility.Visible;
                }
            }
            else
                PauseAnimations();
        }
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return null;
        }

        protected void PauseAnimations()
        {
            foreach (var animationsHost in this.FindVisualDescendantsByType<DependencyObject>().OfType<IAnimationsHost>())
            {
                try
                {
                    animationsHost.PauseAnimations();
                }
                catch (Exception e)
                {
                    GameLog.Client.General.Error(e);
                }
            }
        }

        protected void ResumeAnimations()
        {
            foreach (var animationsHost in this.FindVisualDescendantsByType<DependencyObject>().OfType<IAnimationsHost>())
            {
                try
                {
                    animationsHost.ResumeAnimations();
                }
                catch (Exception e)
                {
                    GameLog.Client.General.Error(e);
                }
            }
        }

        protected void StopAnimations()
        {
            foreach (var animationsHost in this.FindVisualDescendantsByType<DependencyObject>().OfType<IAnimationsHost>())
            {
                try
                {
                    animationsHost.StopAnimations();
                }
                catch (Exception e)
                {
                    GameLog.Client.General.Error(e);
                }
            }
        }

        #region Implementation of IActiveAware

        private bool _isActive;

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (value == _isActive)
                    return;

                _isActive = value;

                IsActiveChanged.Raise(this);
            }
        }

        public event EventHandler IsActiveChanged;

        #endregion

        #region Implementation of IGameScreenView<AssetsScreenPresentationModel>

        public IAppContext AppContext { get; set; }

        public AssetsScreenPresentationModel Model
        {
            get { return DataContext as AssetsScreenPresentationModel; }
            set { DataContext = value; }
        }

        public void OnCreated() { }

        public void OnDestroyed()
        {
            StopAnimations();
        }
        #endregion
        #region OnButtonClicks
        private void OnEnergyOneClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedOneCiv);
        }
        private void OnEnergyTwoClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedTwoCiv);
        }
        private void OnEnergyThreeClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedThreeCiv);
        }
        private void OnEnergyFourClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnEnergyFiveClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFiveCiv);
        }
        private void OnEnergySixClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageEnergy(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedSixCiv);
        }
        private void OnFoodOneClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedOneCiv);
        }
        private void OnFoodTwoClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedTwoCiv);
        }
        private void OnFoodThreeClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedThreeCiv);
        }
        private void OnFoodFourClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnFoodFiveClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFiveCiv);
        }
        private void OnFoodSixClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageFood(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedSixCiv);
        }
        private void OnResearchOneClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnResearchTwoClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnResearchThreeClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnResearchFourClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnResearchFiveClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnResearchSixClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnIndustryOneClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnIndustryTwoClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnIndustryThreeClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnIndustryFourClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnIndustryFiveClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnIndustrySixClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        #endregion
        private void LoadInsignia()
        {
            GameLog.Client.UI.DebugFormat("Loading Insignias/FEDERATION.png and more");
            BitmapImage insigniaFed = new BitmapImage();
            var uriFed = new Uri("vfs:///Resources/Images/Insignias/FEDERATION.png");
            insigniaFed.BeginInit();
            insigniaFed.UriSource = uriFed;
            insigniaFed.EndInit();

            BitmapImage insigniaTerran = new BitmapImage();
            var uriTerran = new Uri("vfs:///Resources/Images/Insignias/TERRANEMPIRE.png");
            insigniaTerran.BeginInit();
            insigniaTerran.UriSource = uriTerran;
            insigniaTerran.EndInit();

            BitmapImage insigniaRom = new BitmapImage();
            var uriRom = new Uri("vfs:///Resources/Images/Insignias/ROMULANS.png");
            insigniaRom.BeginInit();
            insigniaRom.UriSource = uriRom;
            insigniaRom.EndInit();

            BitmapImage insigniaKling = new BitmapImage();
            var uriKling = new Uri("vfs:///Resources/Images/Insignias/KLINGONS.png");
            insigniaKling.BeginInit();
            insigniaKling.UriSource = uriKling;
            insigniaKling.EndInit();

            BitmapImage insigniaCard = new BitmapImage();
            var uriCard = new Uri("vfs:///Resources/Images/Insignias/CARDASSIANS.png");
            insigniaCard.BeginInit();
            insigniaCard.UriSource = uriCard;
            insigniaCard.EndInit();

            BitmapImage insigniaDom = new BitmapImage();
            var uriDom = new Uri("vfs:///Resources/Images/Insignias/DOMINION.png");
            insigniaDom.BeginInit();
            insigniaDom.UriSource = uriDom;
            insigniaDom.EndInit();

            BitmapImage insigniaBorg = new BitmapImage();
            var uriBorg = new Uri("vfs:///Resources/Images/Insignias/BORG.png");
            insigniaBorg.BeginInit();
            insigniaBorg.UriSource = uriBorg;
            insigniaBorg.EndInit();
            GameLog.Client.UI.DebugFormat("Loading Insignias is finished");

            List<int> CivIDs = new List<int>();
            if (AssetsScreenPresentationModel.SpiedOneCiv != null)
                CivIDs.Add(AssetsScreenPresentationModel.SpiedOneCiv.CivID);
            if (AssetsScreenPresentationModel.SpiedTwoCiv != null)
                CivIDs.Add(AssetsScreenPresentationModel.SpiedTwoCiv.CivID);
            if (AssetsScreenPresentationModel.SpiedThreeCiv != null)
                CivIDs.Add(AssetsScreenPresentationModel.SpiedThreeCiv.CivID);
            if (AssetsScreenPresentationModel.SpiedFourCiv != null)
                CivIDs.Add(AssetsScreenPresentationModel.SpiedFourCiv.CivID);
            if (AssetsScreenPresentationModel.SpiedFiveCiv != null)
                CivIDs.Add(AssetsScreenPresentationModel.SpiedFiveCiv.CivID);
            if (AssetsScreenPresentationModel.SpiedSixCiv != null)
                CivIDs.Add(AssetsScreenPresentationModel.SpiedSixCiv.CivID);
            GameLog.Client.UI.DebugFormat("Adding SpiedCiv is finished");

            if (CivIDs.Count >= 1)
            {
                switch (CivIDs[0])
                {
                    case 0:
                        InsigniaOne.Source = insigniaFed;
                        break;
                    case 1:
                        InsigniaOne.Source = insigniaTerran;
                        break;
                    case 2:
                        InsigniaOne.Source = insigniaRom;
                        break;
                    case 3:
                        InsigniaOne.Source = insigniaKling;
                        break;
                    case 4:
                        InsigniaOne.Source = insigniaCard;
                        break;
                    case 5:
                        InsigniaOne.Source = insigniaDom;
                        break;
                    case 6:
                        InsigniaOne.Source = insigniaBorg;
                        break;
                    default:
                        break;
                }
            }
            if (CivIDs.Count >= 2)
            {
                switch (CivIDs[1])
                {
                    case 0:
                        InsigniaTwo.Source = insigniaFed;
                        break;
                    case 1:
                        InsigniaTwo.Source = insigniaTerran;
                        break;
                    case 2:
                        InsigniaTwo.Source = insigniaRom;
                        break;
                    case 3:
                        InsigniaTwo.Source = insigniaKling;
                        break;
                    case 4:
                        InsigniaTwo.Source = insigniaCard;
                        break;
                    case 5:
                        InsigniaTwo.Source = insigniaDom;
                        break;
                    case 6:
                        InsigniaTwo.Source = insigniaBorg;
                        break;
                    default:
                        break;
                }
            }
            if (CivIDs.Count >= 3)
            {
                switch (CivIDs[2])
                {
                    case 0:
                        InsigniaThree.Source = insigniaFed;
                        break;
                    case 1:
                        InsigniaThree.Source = insigniaTerran;
                        break;
                    case 2:
                        InsigniaThree.Source = insigniaRom;
                        break;
                    case 3:
                        InsigniaThree.Source = insigniaKling;
                        break;
                    case 4:
                        InsigniaThree.Source = insigniaCard;
                        break;
                    case 5:
                        InsigniaThree.Source = insigniaDom;
                        break;
                    case 6:
                        InsigniaThree.Source = insigniaBorg;
                        break;
                    default:
                        break;
                }
            }
            if (CivIDs.Count >= 4)
            {
                switch (CivIDs[3])
                {
                    case 0:
                        InsigniaFour.Source = insigniaFed;
                        break;
                    case 1:
                        InsigniaFour.Source = insigniaTerran;
                        break;
                    case 2:
                        InsigniaFour.Source = insigniaRom;
                        break;
                    case 3:
                        InsigniaFour.Source = insigniaKling;
                        break;
                    case 4:
                        InsigniaFour.Source = insigniaCard;
                        break;
                    case 5:
                        InsigniaFour.Source = insigniaDom;
                        break;
                    case 6:
                        InsigniaFour.Source = insigniaBorg;
                        break;
                    default:
                        break;
                }
            }
            if (CivIDs.Count >= 5)
            {
                switch (CivIDs[4])
                {
                    case 0:
                        InsigniaFive.Source = insigniaFed;
                        break;
                    case 1:
                        InsigniaFive.Source = insigniaTerran;
                        break;
                    case 2:
                        InsigniaFive.Source = insigniaRom;
                        break;
                    case 3:
                        InsigniaFive.Source = insigniaKling;
                        break;
                    case 4:
                        InsigniaFive.Source = insigniaCard;
                        break;
                    case 5:
                        InsigniaFive.Source = insigniaDom;
                        break;
                    case 6:
                        InsigniaFive.Source = insigniaBorg;
                        break;
                    default:
                        break;
                }
            }
            if (CivIDs.Count >= 6)
            {
                switch (CivIDs[5])
                {
                    case 0:
                        InsigniaSix.Source = insigniaFed;
                        break;
                    case 1:
                        InsigniaSix.Source = insigniaTerran;
                        break;
                    case 2:
                        InsigniaSix.Source = insigniaRom;
                        break;
                    case 3:
                        InsigniaSix.Source = insigniaKling;
                        break;
                    case 4:
                        InsigniaSix.Source = insigniaCard;
                        break;
                    case 5:
                        InsigniaSix.Source = insigniaDom;
                        break;
                    case 6:
                        InsigniaSix.Source = insigniaBorg;
                        break;
                    default:
                        break;
                }
            }
            GameLog.Client.UI.DebugFormat("Insignia is finished");
        }      
    }
}