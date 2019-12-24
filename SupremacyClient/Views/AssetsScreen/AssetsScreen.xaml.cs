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
            BlameNoOne1.Visibility = Visibility.Visible;
            BlameNoOne1.IsChecked = true;
            BlameTerrorists1.Visibility = Visibility.Visible;
            //BlameFederation1.Visibility = Visibility.Collapsed;
            //BlameTerranEmpire1.Visibility = Visibility.Collapsed;
            //BlameRomulans1.Visibility = Visibility.Collapsed;
            //BlameKlingons1.Visibility = Visibility.Collapsed;
            //BlameCardassians1.Visibility = Visibility.Collapsed;
            //BlameDominion1.Visibility = Visibility.Collapsed;
            //BlameBorg1.Visibility = Visibility.Collapsed;

            BlameNoOne2.Visibility = Visibility.Visible;
            BlameNoOne2.IsChecked = true;
            BlameTerrorists2.Visibility = Visibility.Visible;
            //BlameFederation2.Visibility = Visibility.Collapsed;
            //BlameTerranEmpire2.Visibility = Visibility.Collapsed;
            //BlameRomulans2.Visibility = Visibility.Collapsed;
            //BlameKlingons2.Visibility = Visibility.Collapsed;
            //BlameCardassians2.Visibility = Visibility.Collapsed;
            //BlameDominion2.Visibility = Visibility.Collapsed;
            //BlameBorg2.Visibility = Visibility.Collapsed;

            BlameNoOne3.Visibility = Visibility.Visible;
            BlameNoOne3.IsChecked = true;
            BlameTerrorists3.Visibility = Visibility.Visible;
            //BlameFederation3.Visibility = Visibility.Collapsed;
            //BlameTerranEmpire3.Visibility = Visibility.Collapsed;
            //BlameRomulans3.Visibility = Visibility.Collapsed;
            //BlameKlingons3.Visibility = Visibility.Collapsed;
            //BlameCardassians3.Visibility = Visibility.Collapsed;
            //BlameDominion3.Visibility = Visibility.Collapsed;
            //BlameBorg3.Visibility = Visibility.Collapsed;

            BlameNoOne4.Visibility = Visibility.Visible;
            BlameNoOne4.IsChecked = true;
            BlameTerrorists4.Visibility = Visibility.Visible;
            //BlameFederation4.Visibility = Visibility.Collapsed;
            //BlameTerranEmpire4.Visibility = Visibility.Collapsed;
            //BlameRomulans4.Visibility = Visibility.Collapsed;
            //BlameKlingons4.Visibility = Visibility.Collapsed;
            //BlameCardassians4.Visibility = Visibility.Collapsed;
            //BlameDominion4.Visibility = Visibility.Collapsed;
            //BlameBorg4.Visibility = Visibility.Collapsed;

            BlameNoOne5.Visibility = Visibility.Visible;
            BlameNoOne5.IsChecked = true;
            BlameTerrorists5.Visibility = Visibility.Visible;
            //BlameFederation5.Visibility = Visibility.Collapsed;
            //BlameTerranEmpire5.Visibility = Visibility.Collapsed;
            //BlameRomulans5.Visibility = Visibility.Collapsed;
            //BlameKlingons5.Visibility = Visibility.Collapsed;
            //BlameCardassians5.Visibility = Visibility.Collapsed;
            //BlameDominion5.Visibility = Visibility.Collapsed;
            //BlameBorg5.Visibility = Visibility.Collapsed;

            BlameNoOne6.Visibility = Visibility.Visible;
            BlameNoOne6.IsChecked = true;
            BlameTerrorists6.Visibility = Visibility.Visible;
            //BlameFederation6.Visibility = Visibility.Collapsed;
            //BlameTerranEmpire6.Visibility = Visibility.Collapsed;
            //BlameRomulans6.Visibility = Visibility.Collapsed;
            //BlameKlingons6.Visibility = Visibility.Collapsed;
            //BlameCardassians6.Visibility = Visibility.Collapsed;
            //BlameDominion6.Visibility = Visibility.Collapsed;
            //BlameBorg6.Visibility = Visibility.Collapsed;

            LoadInsignia();
        }
        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                ResumeAnimations();
                GameLog.Client.UI.DebugFormat("begin of checking visible");

                if (!AssetsHelper.IsSpiedOne(SpiedOneCiv))
                {
                    GameLog.Client.UI.DebugFormat("SpiedOneCiv checking visible .... ");
                    EmpireExpanderOne.Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (SpiedOneCiv != civLocalPlayer)
                    {
                        EmpireExpanderOne.Visibility = Visibility.Collapsed;

                        //SabotageEnergyOne.Visibility = Visibility.Visible;
                        //SabotageFoodOne.Visibility = Visibility.Visible;
                        //SabotageIndustryOne.Visibility = Visibility.Visible;
                        //StealResearchOne.Visibility = Visibility.Visible;
                    }
                }

                if (!AssetsHelper.IsSpiedTwo(SpiedTwoCiv))
                {
                    GameLog.Client.UI.DebugFormat("SpiedTwoCiv checking visible .... ");
                    EmpireExpanderTwo.Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (SpiedTwoCiv != civLocalPlayer)
                    {
                        EmpireExpanderTwo.Visibility = Visibility.Visible;

                        //SabotageEnergyTwo.Visibility = Visibility.Visible;
                        //SabotageFoodTwo.Visibility = Visibility.Visible;
                        //SabotageIndustryTwo.Visibility = Visibility.Visible;
                        //StealResearchTwo.Visibility = Visibility.Visible;
                    }
                }

                if (!AssetsHelper.IsSpiedThree(SpiedThreeCiv))
                {
                    GameLog.Client.UI.DebugFormat("SpiedThreeCiv checking visible .... ");
                    EmpireExpanderThree.Visibility = Visibility.Collapsed;

                }
                else
                {
                    if (SpiedThreeCiv != civLocalPlayer)
                    {
                        EmpireExpanderThree.Visibility = Visibility.Visible;

                        //SabotageEnergyThree.Visibility = Visibility.Visible;
                        //SabotageFoodThree.Visibility = Visibility.Visible;
                        //SabotageIndustryThree.Visibility = Visibility.Visible;
                        //StealResearchThree.Visibility = Visibility.Visible;
                    }
                }

                if (!AssetsHelper.IsSpiedFour(SpiedFourCiv))
                {
                    GameLog.Client.UI.DebugFormat("SpiedFourCiv checking visible .... ");
                    EmpireExpanderFour.Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (SpiedFourCiv != civLocalPlayer)
                    {
                        EmpireExpanderFour.Visibility = Visibility.Visible;

                        //SabotageEnergyFour.Visibility = Visibility.Visible;
                        //SabotageFoodFour.Visibility = Visibility.Visible;
                        //SabotageIndustryFour.Visibility = Visibility.Visible;
                        //StealResearchFour.Visibility = Visibility.Visible;
                    }
                }

                if (!AssetsHelper.IsSpiedFive(SpiedFiveCiv))
                {
                    GameLog.Client.UI.DebugFormat("SpiedFiveCiv checking visible .... ");
                    EmpireExpanderFive.Visibility = Visibility.Collapsed;
                }

                else
                {
                    if (SpiedFiveCiv != civLocalPlayer)
                    {
                        EmpireExpanderFive.Visibility = Visibility.Visible;

                        //SabotageEnergyFive.Visibility = Visibility.Visible;
                        //SabotageFoodFive.Visibility = Visibility.Visible;
                        //SabotageIndustryFive.Visibility = Visibility.Visible;
                        //StealResearchFive.Visibility = Visibility.Visible;
                    }
                }
                if (!AssetsHelper.IsSpiedSix(SpiedSixCiv))
                {
                    GameLog.Client.UI.DebugFormat("SpiedSixCiv checking visible .... ");

                    EmpireExpanderSix.Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (SpiedSixCiv != civLocalPlayer)

                    {
                        EmpireExpanderSix.Visibility = Visibility.Visible;

                        //SabotageEnergySix.Visibility = Visibility.Visible;
                        //SabotageFoodSix.Visibility = Visibility.Visible;
                        //SabotageIndustrySix.Visibility = Visibility.Visible;
                        //StealResearchSix.Visibility = Visibility.Visible;
                    }
                }

                //GameLog.Client.UI.DebugFormat("in the middle of checking visible .... ");

                GameLog.Client.UI.DebugFormat("end  of checking visible");
                List<Civilization> sevenCivsList = new List<Civilization>();
                Dictionary<int, Civilization> sevenCivsDictionary = new Dictionary<int, Civilization>();
                int index = 0;
                foreach (var civIn in GameContext.Current.Civilizations) 
                {
                    if (civIn.IsEmpire)
                    {
                        sevenCivsDictionary.Add(civIn.CivID, civIn);
                        sevenCivsList.Add(civIn);
                        index++;
                    }
                    if (index >= 7)
                        break;
                }

                GameLog.Client.UI.DebugFormat("FED: begin of checking BLAME visible");
                if (sevenCivsDictionary.Keys.Contains(0) && AppContext.LocalPlayer.Empire.Key != "FEDERATION" &&
                    GameContext.Current.DiplomacyData[AssetsScreenPresentationModel.Local, sevenCivsDictionary[0]].IsContactMade())
                {
                    BlameFederation1.Visibility = Visibility.Collapsed;
                }
                GameLog.Client.UI.DebugFormat("FED: end   of checking BLAME visible");
                if (sevenCivsDictionary.Keys.Contains(1) && AppContext.LocalPlayer.Empire.Key != "TERRANEMPIRE" &&
                    GameContext.Current.DiplomacyData[AssetsScreenPresentationModel.Local, sevenCivsDictionary[1]].IsContactMade())
                {
                    if (sevenCivsDictionary[1] == sevenCivsList[0])
                    {
                        BlameTerranEmpire1.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        BlameTerranEmpire2.Visibility = Visibility.Collapsed;
                    }
                }
                if (sevenCivsDictionary.Keys.Contains(2) && AppContext.LocalPlayer.Empire.Key != "ROMULANS" &&
                    GameContext.Current.DiplomacyData[AssetsScreenPresentationModel.Local, sevenCivsDictionary[2]].IsContactMade())
                {
                    if (sevenCivsDictionary[2] == sevenCivsList[0])
                    {
                        BlameRomulans1.Visibility = Visibility.Collapsed;
                    }
                    if (sevenCivsDictionary[2] == sevenCivsList[1])
                    {
                        BlameRomulans2.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        BlameRomulans3.Visibility = Visibility.Collapsed;
                    }
                }
                if (sevenCivsDictionary.Keys.Contains(3) && AppContext.LocalPlayer.Empire.Key != "KLINGONS" &&
                    GameContext.Current.DiplomacyData[AssetsScreenPresentationModel.Local, sevenCivsDictionary[3]].IsContactMade())
                {
                    if (sevenCivsDictionary[3] == sevenCivsList[0])
                    {
                        BlameKlingons1.Visibility = Visibility.Collapsed;
                    }
                    if (sevenCivsDictionary[3] == sevenCivsList[1])
                    {
                        BlameKlingons2.Visibility = Visibility.Collapsed;
                    }
                    if (sevenCivsDictionary[3] == sevenCivsList[2])
                    {
                        BlameKlingons3.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        BlameKlingons4.Visibility = Visibility.Collapsed;
                    }
                }
                GameLog.Client.UI.DebugFormat("CARD: begin of checking BLAME visible");
                if (sevenCivsDictionary.Keys.Contains(4) && AppContext.LocalPlayer.Empire.Key != "CARDASSIANS" &&
                    GameContext.Current.DiplomacyData[AssetsScreenPresentationModel.Local, sevenCivsDictionary[4]].IsContactMade()) // && sevenCivs[4].Key != "CARDASSIANS")
                {
                    if (sevenCivsDictionary[4] == sevenCivsList[0])
                    {
                        BlameCardassians1.Visibility = Visibility.Collapsed;
                    }
                    if (sevenCivsDictionary[4] == sevenCivsList[1])
                    {
                        BlameCardassians2.Visibility = Visibility.Collapsed;
                    }
                    if (sevenCivsDictionary[4] == sevenCivsList[2])
                    {
                        BlameCardassians3.Visibility = Visibility.Collapsed;
                    }
                    if (sevenCivsDictionary[4] == sevenCivsList[3])
                    {
                        BlameCardassians4.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        BlameCardassians5.Visibility = Visibility.Collapsed;
                    }
                }
                GameLog.Client.UI.DebugFormat("CARD: end   of checking BLAME visible");
                if (sevenCivsDictionary.Keys.Contains(5) && AppContext.LocalPlayer.Empire.Key != "DOMINION" &&
                    GameContext.Current.DiplomacyData[AssetsScreenPresentationModel.Local, sevenCivsDictionary[5]].IsContactMade())
                {
                    if (sevenCivsDictionary[5] == sevenCivsList[0])
                    {
                        BlameDominion1.Visibility = Visibility.Collapsed;
                    }
                    if (sevenCivsDictionary[5] == sevenCivsList[1])
                    {
                        BlameDominion2.Visibility = Visibility.Collapsed;
                    }
                    if (sevenCivsDictionary[5] == sevenCivsList[2])
                    {
                        BlameDominion3.Visibility = Visibility.Collapsed;
                    }
                    if (sevenCivsDictionary[5] == sevenCivsList[3])
                    {
                        BlameDominion4.Visibility = Visibility.Collapsed;
                    }
                    if (sevenCivsDictionary[5] == sevenCivsList[4])
                    {
                        BlameDominion5.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        BlameDominion6.Visibility = Visibility.Collapsed;
                    }
                }

                if (sevenCivsDictionary.Keys.Contains(6) && AppContext.LocalPlayer.Empire.Key != "BORG" &&
                    GameContext.Current.DiplomacyData[AssetsScreenPresentationModel.Local, sevenCivsDictionary[6]].IsContactMade())
                {
                    if (sevenCivsDictionary[6] == sevenCivsList[0])
                    {
                        BlameBorg1.Visibility = Visibility.Collapsed;
                    }
                    if (sevenCivsDictionary[6] == sevenCivsList[1])
                    {
                        BlameBorg2.Visibility = Visibility.Collapsed;
                    }
                    if (sevenCivsDictionary[6] == sevenCivsList[2])
                    {
                        BlameBorg3.Visibility = Visibility.Collapsed;
                    }
                    if (sevenCivsDictionary[6] == sevenCivsList[3])
                    {
                        BlameBorg4.Visibility = Visibility.Collapsed;
                    }
                    if (sevenCivsDictionary[6] == sevenCivsList[4])
                    {
                        BlameBorg5.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        BlameBorg6.Visibility = Visibility.Collapsed;
                    }
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
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedOneCiv);
        }
        private void OnResearchTwoClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedTwoCiv);
        }
        private void OnResearchThreeClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedThreeCiv);
        }
        private void OnResearchFourClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnResearchFiveClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFiveCiv);
        }
        private void OnResearchSixClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.StealResearch(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedSixCiv);
        }
        private void OnIndustryOneClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedOneCiv);
        }
        private void OnIndustryTwoClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedTwoCiv);
        }
        private void OnIndustryThreeClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedThreeCiv);
        }
        private void OnIndustryFourClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFourCiv);
        }
        private void OnIndustryFiveClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedFiveCiv);
        }
        private void OnIndustrySixClick(object sender, RoutedEventArgs e)
        {
            IntelHelper.SabotageIndustry(IntelHelper.NewSpiedColonies.FirstOrDefault(), AssetsScreenPresentationModel.SpiedSixCiv);
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