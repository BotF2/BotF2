//File: AssetsScreenPresentationModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using Microsoft.Practices.Unity;
using Supremacy.Annotations;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Intelligence;
using Supremacy.Types;
using Supremacy.Universe;
using Supremacy.Utility;
using Supremacy.Client.Context;



namespace Supremacy.Client.Views
{
    public class AssetsScreenPresentationModel : PresentationModelBase, INotifyPropertyChanged
    {
        protected Meter _totalResearch;
        protected int _totalIntelligenceProduction;
        protected int _totalIntelligenceDefenseAccumulated;
        protected int _totalIntelligenceAttackingAccumulated;
        protected int _valuesFromTurn;
        private List<Civilization> _localSpyingCivList;

        #region designInstance stuff
        //private static AssetsScreenPresentationModel _designInstance;

        //public static AssetsScreenPresentationModel DesignInstance
        //{
        //    get
        //    {
        //        if (_designInstance == null)
        //        {
        //            _designInstance = new AssetsScreenPresentationModel(DesignTimeAppContext.Instance)
        //            {
        //                SelectedColony = DesignTimeObjects.Colony
        //            };
        //        }

        //        return _designInstance;
        //    }
        //}
        ////public AssetsScreenPresentationModel(IAppContext appContext)
        ////: base(appContext) { }

        //public event EventHandler SelectedColonyChanged;

        //private Colony _selectedColony;
        //public Colony SelectedColony
        //{
        //    get { return _selectedColony; }
        //    set
        //    {
        //        var oldValue = _selectedColony;
        //        _selectedColony = value;
        //        OnSelectedColonyChanged(oldValue, value);
        //    }
        //}
        //private void OnSelectedColonyChanged(Colony oldValue, Colony newValue)
        //{
        //    var handler = SelectedColonyChanged;
        //    if (handler != null)
        //        handler(this, new PropertyChangedRoutedEventArgs<Colony>(oldValue, newValue));
        //    OnPropertyChanged("SelectedColony");
        //}
        #endregion

        #region Properties for AssestsScreen

        public CivilizationManager MyLocalCivManager => IntelHelper.LocalCivManager;

        public List<Civilization> LocalSpyingCivList
        {
            get
            {
                if (MyLocalCivManager.Civilization.CivID == 0)
                {
                    _localSpyingCivList = IntelHelper._spyingCiv_0_List;

                }
                if (MyLocalCivManager.Civilization.CivID == 1)
                {
                    _localSpyingCivList = IntelHelper._spyingCiv_1_List;
                }

                if (MyLocalCivManager.Civilization.CivID == 2)
                {
                    _localSpyingCivList = IntelHelper._spyingCiv_2_List;
                }

                if (MyLocalCivManager.Civilization.CivID == 3)
                {
                    _localSpyingCivList = IntelHelper._spyingCiv_3_List;
                }

                if (MyLocalCivManager.Civilization.CivID == 4)
                {
                    _localSpyingCivList = IntelHelper._spyingCiv_4_List;
                }

                if (MyLocalCivManager.Civilization.CivID == 5)
                {
                    _localSpyingCivList = IntelHelper._spyingCiv_5_List;
                }

                if (MyLocalCivManager.Civilization.CivID == 6)
                {
                    _localSpyingCivList = IntelHelper._spyingCiv_6_List;
                }

                return _localSpyingCivList;
            }
        }

        public int TotalIntelligenceProduction
        {
            get
            {
                try
                {
                    _totalIntelligenceProduction = MyLocalCivManager.TotalIntelligenceProduction;
                    _text = "Step_5450:; "
                              ;
                    Console.WriteLine(_text);
                    //GameLog.Client.Intel.DebugFormat("Get TotalIntelProduction ={0}", _totalIntelligenceProduction);
                    return _totalIntelligenceProduction;
                }
                catch (Exception e)
                {
                    _text = "Step_5450:; Problem occured at TotalIntelligenceProduction get, exception "
                        + e.Message
                        + newline + e.TargetSite
          ;
                    Console.WriteLine(_text);
                    //GameLog.Client.Intel.DebugFormat(_text);
                    return 0;
                }
            }
            set
            {
                try
                {
                    _totalIntelligenceProduction = MyLocalCivManager.TotalIntelligenceProduction;
                    FillUpDefense();
                    _totalIntelligenceProduction = value;
                    GameLog.Client.Intel.DebugFormat("Set TotalIntelProduction ={0}", _totalIntelligenceProduction);
                    NotifyPropertyChanged("TotalIntelligenceProduction");
                }
                catch (Exception e)
                {
                    GameLog.Client.Intel.DebugFormat("Problem occured at TotalIntelligenceProduction set, Exception {0} {1}", e.Message, e.StackTrace);
                }
            }
        }


        public int TotalIntelligenceDefenseAccumulated
        {
            get
            {
                FillUpDefense();
                _totalIntelligenceDefenseAccumulated = MyLocalCivManager.TotalIntelligenceDefenseAccumulated.CurrentValue;
                //works   GameLog.Client.Intel.DebugFormat("Get TotalIntelDefenseAccumulated ={0}", _totalIntelligenceDefenseAccumulated);
                return _totalIntelligenceDefenseAccumulated;
            }
            set
            {
                FillUpDefense();
                //_totalIntelligenceDefenseAccumulated = IntelHelper.DefenseAccumulatedInteInt;
                _totalIntelligenceDefenseAccumulated = MyLocalCivManager.TotalIntelligenceDefenseAccumulated.CurrentValue;
                _totalIntelligenceDefenseAccumulated = value;
                //works   GameLog.Client.Intel.DebugFormat("Set TotalIntelDefenseAccumulated ={0}", _totalIntelligenceDefenseAccumulated);
                NotifyPropertyChanged("TotalIntelligenceDefenseAccumulated");
            }
        }

        public int TotalIntelligenceAttackingAccumulated
        {
            get
            {
                FillUpDefense();
                _totalIntelligenceAttackingAccumulated = MyLocalCivManager.TotalIntelligenceAttackingAccumulated.CurrentValue;
                //works   GameLog.Client.Intel.DebugFormat("Get TotalIntelDefenseAccumulated ={0}", _totalIntelligenceAttackingAccumulated);
                return _totalIntelligenceAttackingAccumulated;
            }
            set
            {
                FillUpDefense();
                // _totalIntelligenceAttackingAccumulated = IntelHelper.AttackingAccumulatedInteInt;
                _totalIntelligenceAttackingAccumulated = MyLocalCivManager.TotalIntelligenceAttackingAccumulated.CurrentValue;
                _totalIntelligenceAttackingAccumulated = value;
                //works   GameLog.Client.Intel.DebugFormat("Set TotalIntelDefenseAccumulated ={0}", _totalIntelligenceAttackingAccumulated);
                NotifyPropertyChanged("TotalIntelligenceAttackingAccumulated");
            }
        }
        public Meter UpdateAttackingAccumulated(Civilization attackingCiv)
        {
            Meter attackMeter = GameContext.Current.CivilizationManagers[attackingCiv].TotalIntelligenceAttackingAccumulated;
            //works
            _text = "Step_5442:; Before update attackMeter =;" + attackMeter + "; for attacking civ =;" + attackingCiv;
            Console.WriteLine(_text);
            //GameLog.Client.Intel.DebugFormat("Step_5432:; trying to return SpiedOneCiv.Civilization = {0}", SpiedCiv.Civilization.Key);
            //GameLog.Client.Intel.DebugFormat("Before update attackMeter ={0} for attakcing civ ={1}", attackMeter, attackingCiv);
            _ = int.TryParse(attackMeter.CurrentValue.ToString(), out int newAttackIntelligence);
            _totalIntelligenceAttackingAccumulated = newAttackIntelligence;
            //works
            _text = "Step_5444:; After update attackMeter =;" + attackMeter + "; for attacking civ =;" + attackingCiv;
            Console.WriteLine(_text);
            //GameLog.Client.Intel.DebugFormat("Step_5432:; trying to return SpiedOneCiv.Civilization = {0}", SpiedCiv.Civilization.Key);
            //GameLog.Client.Intel.DebugFormat(" After update attackMeter ={0} for attacking civ ={1}", attackMeter, attackingCiv);
            return attackMeter;
        }
        protected virtual void FillUpDefense()
        {
            CivilizationManager civ = GameContext.Current.CivilizationManagers[MyLocalCivManager.Civilization];
            _ = civ.TotalIntelligenceAttackingAccumulated.AdjustCurrent(civ.TotalIntelligenceAttackingAccumulated.CurrentValue * -1); // remove from Attacking
            civ.TotalIntelligenceAttackingAccumulated.UpdateAndReset();
            _ = civ.TotalIntelligenceDefenseAccumulated.AdjustCurrent(civ.TotalIntelligenceDefenseAccumulated.CurrentValue); // add to Defense
            civ.TotalIntelligenceDefenseAccumulated.UpdateAndReset();
            //OnPropertyChanged("TotalIntelligenceAttackingAccumulated");
            //OnPropertyChanged("TotalIntelligenceDefenseAccumulated");
            //OnPropertyChanged("TotalIntelligenceProduction");

        }
        #endregion 

        [InjectionConstructor]
        public AssetsScreenPresentationModel([NotNull] IAppContext appContext)
            : base(appContext) { }

        public AssetsScreenPresentationModel()
            : base(DesignTimeAppContext.Instance)
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                throw new InvalidOperationException("This constructor should only be invoked at design time.");
            }

            _colonies = MyLocalCivManager.Colonies; //not the host on a remote machine, DesignTimeObjects.LocalCivManager.Colonies;
            _spiedZeroColonies = DesignTimeObjects.SpiedCiv_0.Colonies;
            _spiedOneColonies = DesignTimeObjects.SpiedCiv_1.Colonies;
            _spiedTwoColonies = DesignTimeObjects.SpiedCiv_2.Colonies;
            _spiedThreeColonies = DesignTimeObjects.SpiedCiv_3.Colonies;
            _spiedFourColonies = DesignTimeObjects.SpiedCiv_4.Colonies;
            _spiedFiveColonies = DesignTimeObjects.SpiedCiv_5.Colonies;
            _spiedSixColonies = DesignTimeObjects.SpiedCiv_6.Colonies;
            _totalResearch = GameContext.Current.CivilizationManagers[MyLocalCivManager.Civilization].Research.CumulativePoints;
            _totalIntelligenceProduction = GameContext.Current.CivilizationManagers[MyLocalCivManager.Civilization].TotalIntelligenceProduction;
            _totalIntelligenceAttackingAccumulated = GameContext.Current.CivilizationManagers[MyLocalCivManager.Civilization].TotalIntelligenceAttackingAccumulated.CurrentValue;
            _totalIntelligenceDefenseAccumulated = GameContext.Current.CivilizationManagers[MyLocalCivManager.Civilization].TotalIntelligenceDefenseAccumulated.CurrentValue;
            _valuesFromTurn = GameContext.Current.TurnNumber;



            OnPropertyChanged("InstallingSpyNetwork");
            OnPropertyChanged("TotalIntelligenceAttackingAccumulated");
            OnPropertyChanged("TotalIntelligenceDefenseAccumulated");
            OnPropertyChanged("TotalIntelligenceProduction");
        }

        #region Colonies Property

        [field: NonSerialized]
        public event EventHandler ValuesFromTurnChanged;
        public event EventHandler ColoniesChanged;
        public event EventHandler TotalPopulationChanged;
        public event EventHandler CreditsEmpireChanged;

        public event EventHandler TotalResearchChanged;
        public event EventHandler InstallingSpyNetworkChanged;
        public event EventHandler TotalIntelligenceProductionChanged;
        public event EventHandler TotalIntelligenceAttackingAccumulatedChanged;
        public event EventHandler TotalIntelligenceDefenseAccumulatedChanged;

        public event EventHandler TotalDilithiumChanged;
        public event EventHandler TotalDeuteriumChanged;
        public event EventHandler TotalDuraniumChanged;

        public event EventHandler SpiedZeroColoniesChanged;
        public event EventHandler SpiedZeroTotalPopulationChanged;

        public event EventHandler SpiedOneColoniesChanged;
        public event EventHandler SpiedOneTotalPopulationChanged;

        public event EventHandler SpiedTwoColoniesChanged;
        public event EventHandler SpiedTwoTotalPopulationChanged;

        public event EventHandler SpiedThreeColoniesChanged;
        public event EventHandler SpiedThreeTotalPopulationChanged;

        public event EventHandler SpiedFourColoniesChanged;
        public event EventHandler SpiedFourTotalPopulationChanged;

        public event EventHandler SpiedFiveColoniesChanged;
        public event EventHandler SpiedFiveTotalPopulationChanged;

        public event EventHandler SpiedSixColoniesChanged;
        public event EventHandler SpiedSixTotalPopulationChanged;

        private IEnumerable<Colony> _colonies;

        private IEnumerable<Colony> _spiedZeroColonies;

        private IEnumerable<Colony> _spiedOneColonies;

        private IEnumerable<Colony> _spiedTwoColonies;

        private IEnumerable<Colony> _spiedThreeColonies;

        private IEnumerable<Colony> _spiedFourColonies;

        private IEnumerable<Colony> _spiedFiveColonies;

        private IEnumerable<Colony> _spiedSixColonies;

        public IEnumerable<Colony> Colonies
        {
            get =>
                //if (_colonies != null)
                //    OnColoniesChanged();
                _colonies;
            set
            {
                if (Equals(value, _colonies))
                {
                    return;
                }

                _colonies = value;

                FillUpDefense();
                OnColoniesChanged();
                OnTotalPopulationChanged();
                OnTotalResearchChanged();
                OnInstallingSpyNetworkChanged();
                OnTotalIntelligenceProductionChanged();
                OnTotalIntelligenceAttackingAccumulatedChanged();
                OnTotalIntelligenceDefenseAccumulatedChanged();
            }
        }
        public IEnumerable<Colony> SpiedZeroColonies
        {
            get => _spiedZeroColonies;
            set
            {
                if (Equals(value, _spiedZeroColonies))
                {
                    return;
                }

                _spiedZeroColonies = value;

                OnSpiedZeroColoniesChanged();
                OnSpiedZeroTotalPopulationChanged();
            }
        }
        public IEnumerable<Colony> SpiedOneColonies
        {
            get => _spiedOneColonies;
            set
            {
                if (Equals(value, _spiedOneColonies))
                {
                    return;
                }

                _spiedOneColonies = value;

                OnSpiedOneColoniesChanged();
                OnSpiedOneTotalPopulationChanged();
            }
        }
        public IEnumerable<Colony> SpiedTwoColonies
        {
            get => _spiedTwoColonies;
            set
            {
                if (Equals(value, _spiedTwoColonies))
                {
                    return;
                }

                _spiedTwoColonies = value;

                OnSpiedTwoColoniesChanged();
                OnSpiedTwoTotalPopulationChanged();
            }
        }
        public IEnumerable<Colony> SpiedThreeColonies
        {
            get => _spiedThreeColonies;
            set
            {
                if (Equals(value, _spiedThreeColonies))
                {
                    return;
                }

                _spiedThreeColonies = value;

                OnSpiedThreeColoniesChanged();
                OnSpiedThreeTotalPopulationChanged();
            }
        }
        public IEnumerable<Colony> SpiedFourColonies
        {
            get => _spiedFourColonies;
            set
            {
                if (Equals(value, _spiedFourColonies))
                {
                    return;
                }

                _spiedFourColonies = value;

                OnSpiedFourColoniesChanged();
                OnSpiedFourTotalPopulationChanged();
            }
        }
        public IEnumerable<Colony> SpiedFiveColonies
        {
            get => _spiedFiveColonies;
            set
            {
                if (Equals(value, _spiedFiveColonies))
                {
                    return;
                }

                _spiedFiveColonies = value;

                OnSpiedFiveColoniesChanged();
                OnSpiedFiveTotalPopulationChanged();
            }
        }
        public IEnumerable<Colony> SpiedSixColonies
        {
            get => _spiedSixColonies;
            set
            {
                if (Equals(value, _spiedSixColonies))
                {
                    return;
                }

                _spiedSixColonies = value;

                OnSpiedSixColoniesChanged();
                OnSpiedSixTotalPopulationChanged();
            }
        }
        protected virtual void OnInstallingSpyNetworkChanged()
        {
            InstallingSpyNetworkChanged.Raise(this);
            OnPropertyChanged("InstallingSpyNetwork");
        }

        protected virtual void OnValuesFromTurnChanged()
        {
            ValuesFromTurnChanged.Raise(this);
            OnPropertyChanged("ValuesFromTurn");
        }
        protected virtual void OnColoniesChanged()
        {
            //GameLog.Client.Intel.DebugFormat("AssetsScreenPresenterModel OnColoniesChange at line 228");
            ColoniesChanged.Raise(this);
            OnPropertyChanged("Colonies");
        }
        protected virtual void OnTotalPopulationChanged()
        {
            TotalPopulationChanged.Raise(this);
            OnPropertyChanged("TotalPopulation");
        }
        protected virtual void OnTotalResearchChanged()
        {
            TotalResearchChanged.Raise(this);
            OnPropertyChanged("TotalResearch");
        }
        protected virtual void OnCreditsEmpireChanged()
        {
            CreditsEmpireChanged.Raise(this);
            OnPropertyChanged("CreditsEmpire");
        }
        protected virtual void OnTotalDilithiumChanged()
        {
            TotalDilithiumChanged.Raise(this);
            OnPropertyChanged("TotalDilithium");
        }
        protected virtual void OnTotalDeuteriumChanged()
        {
            TotalDeuteriumChanged.Raise(this);
            OnPropertyChanged("TotalDeuterium");
        }
        protected virtual void OnTotalDuraniumChanged()
        {
            TotalDuraniumChanged.Raise(this);
            OnPropertyChanged("TotalDuranium");
        }


        protected virtual void OnTotalIntelligenceProductionChanged()
        {
            TotalIntelligenceProductionChanged.Raise(this);
            OnPropertyChanged("TotalIntelligenceProduction");
        }
        protected virtual void OnTotalIntelligenceAttackingAccumulatedChanged()
        {
            TotalIntelligenceAttackingAccumulatedChanged.Raise(this);
            OnPropertyChanged("TotalIntelligenceAttackingAccumulated");
        }
        protected virtual void OnTotalIntelligenceDefenseAccumulatedChanged()
        {
            TotalIntelligenceDefenseAccumulatedChanged.Raise(this);
            OnPropertyChanged("TotalIntelligenceDefenseAccumulated");
        }
        protected virtual void OnSpiedZeroColoniesChanged()
        {
            SpiedZeroColoniesChanged.Raise(this);
            OnPropertyChanged("SpiedZeroColonies");
        }
        protected virtual void OnSpiedOneColoniesChanged()
        {
            SpiedOneColoniesChanged.Raise(this);
            OnPropertyChanged("SpiedOneColonies");
        }
        protected virtual void OnSpiedTwoColoniesChanged()
        {
            SpiedTwoColoniesChanged.Raise(this);
            OnPropertyChanged("SpiedTwoColonies");
        }
        protected virtual void OnSpiedThreeColoniesChanged()
        {
            SpiedThreeColoniesChanged.Raise(this);
            OnPropertyChanged("SpiedThreeColonies");
        }
        protected virtual void OnSpiedFourColoniesChanged()
        {
            SpiedFourColoniesChanged.Raise(this);
            OnPropertyChanged("SpiedFourColonies");
        }
        protected virtual void OnSpiedFiveColoniesChanged()
        {
            SpiedFiveColoniesChanged.Raise(this);
            OnPropertyChanged("SpiedFiveColonies");
        }
        protected virtual void OnSpiedSixColoniesChanged()
        {
            SpiedSixColoniesChanged.Raise(this);
            OnPropertyChanged("SpiedSixColonies");
        }
        protected virtual void OnSpiedZeroTotalPopulationChanged()
        {
            SpiedZeroTotalPopulationChanged.Raise(this);
            OnPropertyChanged("SpiedZeroTotalPopulation");
        }
        protected virtual void OnSpiedOneTotalPopulationChanged()
        {
            SpiedOneTotalPopulationChanged.Raise(this);
            OnPropertyChanged("SpiedOneTotalPopulation");
        }
        protected virtual void OnSpiedTwoTotalPopulationChanged()
        {
            SpiedTwoTotalPopulationChanged.Raise(this);
            OnPropertyChanged("SpiedTwoTotalPopulation");
        }
        protected virtual void OnSpiedThreeTotalPopulationChanged()
        {
            SpiedThreeTotalPopulationChanged.Raise(this);
            OnPropertyChanged("SpiedThreeTotalPopulation");
        }
        protected virtual void OnSpiedFourTotalPopulationChanged()
        {
            SpiedFourTotalPopulationChanged.Raise(this);
            OnPropertyChanged("SpiedFourTotalPopulation");
        }
        protected virtual void OnSpiedFiveTotalPopulationChanged()
        {
            SpiedFiveTotalPopulationChanged.Raise(this);
            OnPropertyChanged("SpiedFiveTotalPopulation");
        }
        protected virtual void OnSpiedSixTotalPopulationChanged()
        {
            SpiedSixTotalPopulationChanged.Raise(this);
            OnPropertyChanged("SpiedSixTotalPopulation");
        }
        #endregion

        #region TotalPopulations and Empire Names
        public Meter TotalPopulation
        {
            get
            {
                CivilizationManager civManager = MyLocalCivManager; // not this DesignTimeObjects.LocalCivManager.Civilization
                try
                {
                    //GameLog.Core.Intel.DebugFormat("TotalPopulation ={0}", civManager.TotalPopulation);
                    return civManager.TotalPopulation;
                }
                catch (Exception e)
                {
                    GameLog.Core.GameData.WarnFormat("Problem occured at TotalPopulation: {0} {1}", e.Message, e.StackTrace);
                    GameLog.Core.General.Error(e);
                    Meter zero = new Meter(0, 0, 0);
                    return zero; //civManager.TotalPopulation;

                }
            }
        }

        public int TotalResearch
        {
            get
            {
                CivilizationManager civManager = MyLocalCivManager; // not this DesignTimeObjects.LocalCivManager.Civilization
                try
                {
                    //GameLog.Core.Intel.DebugFormat("TotalPopulation ={0}", civManager.TotalPopulation);
                    return civManager.Research.CumulativePoints.CurrentValue;
                }
                catch (Exception e)
                {
                    GameLog.Core.GameData.WarnFormat("Problem occured at TotalResearch: {0} {1}", e.Message, e.StackTrace);
                    GameLog.Core.General.Error(e);
                    //Meter zero = new Meter(0, 0, 0);
                    return 0; //civManager.TotalPopulation;

                }
            }
        }

        public int ValuesFromTurn => GameContext.Current.TurnNumber;

        public int TotalDilithium
        {
            get
            {
                CivilizationManager civManager = MyLocalCivManager; // not this DesignTimeObjects.LocalCivManager.Civilization
                try
                {
                    //GameLog.Core.Intel.DebugFormat("TotalPopulation ={0}", civManager.TotalPopulation);
                    return civManager.Resources.Dilithium.CurrentValue;
                }
                catch (Exception e)
                {
                    GameLog.Core.GameData.WarnFormat("Problem occured at TotalDilithium: {0} {1}", e.Message, e.StackTrace);
                    GameLog.Core.General.Error(e);
                    //Meter zero = new Meter(0, 0, 0);
                    return 0;

                }
            }
        }
        public int TotalDeuterium
        {
            get
            {
                CivilizationManager civManager = MyLocalCivManager; // not this DesignTimeObjects.LocalCivManager.Civilization
                try
                {
                    //GameLog.Core.Intel.DebugFormat("TotalPopulation ={0}", civManager.TotalPopulation);
                    return civManager.Resources.Deuterium.CurrentValue;
                }
                catch (Exception e)
                {
                    GameLog.Core.GameData.WarnFormat("Problem occured at TotalDeuterium: {0} {1}", e.Message, e.StackTrace);
                    GameLog.Core.General.Error(e);
                    //Meter zero = new Meter(0, 0, 0);
                    return 0;
                }
            }
        }

        public int TotalDuranium
        {
            get
            {
                CivilizationManager civManager = MyLocalCivManager; // not this DesignTimeObjects.LocalCivManager.Civilization
                try
                {
                    //GameLog.Core.Intel.DebugFormat("TotalPopulation ={0}", civManager.TotalPopulation);
                    return civManager.Resources.Duranium.CurrentValue;
                }
                catch (Exception e)
                {
                    GameLog.Core.GameData.WarnFormat("Problem occured at TotalDuranium: {0} {1}", e.Message, e.StackTrace);
                    GameLog.Core.General.Error(e);
                    //Meter zero = new Meter(0, 0, 0);
                    return 0;
                }
            }
        }


        public string LocalCivName => MyLocalCivManager.Civilization.Name;  // keep this on AppContext
        public static Civilization LocalCiv => IntelHelper.LocalCivManager.Civilization;
        // ### Federation ####
        public static string SpiedFedName => "Federation";
        public static Civilization SpiedZeroCiv
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_0;
                // GameLog.Client.Test.DebugFormat("##### trying to return SpiedCiv.Civilization = {0}", SpiedCiv.Civilization.Key);
                return SpiedCiv.Civilization;
            }
        }
        public static Colony SpiedZeroSeatOfGovernment
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_0;
                Colony SeatOfGovernment = GameContext.Current.CivilizationManagers[SpiedCiv].SeatOfGovernment;
                // GameLog.Client.Test.DebugFormat("##### trying to return SpiedCiv_0 SeatOfGovernment = {0}", SeatOfGovernment);
                return SeatOfGovernment;
            }
        }
        public static Meter SpiedZeroTotalPopulation
        {
            get
            {
                CivilizationManager civManager = DesignTimeObjects.SpiedCiv_0;
                try
                {
                    GameLog.Core.Test.DebugFormat("SpiedZeroTotalPopulation ={0}", civManager.TotalPopulation);
                    return civManager.TotalPopulation;
                }
                catch (Exception e)
                {
                    Meter zero = new Meter(0, 0, 0);
                    GameLog.Core.Intel.WarnFormat("Problem occured at SpiedZeroTotalPopulation:");
                    GameLog.Core.General.Error(e);
                    return zero;
                }
            }
        }



        //## Terran ##
        public static string SpiedTerranName => "Terran Empire";
        public static Civilization SpiedOneCiv
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_1;
                _text = "Step_5402:; trying to return SpiedOneCiv.Civilization =;" + SpiedCiv.Civilization.Key;
                Console.WriteLine(_text);
                //GameLog.Client.Intel.DebugFormat("Step_5432:; trying to return SpiedOneCiv.Civilization = {0}", SpiedCiv.Civilization.Key);
                return SpiedCiv.Civilization;
            }
        }
        public static Colony SpiedOneSeatOfGovernment
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_1;
                Colony SeatOfGovernment = GameContext.Current.CivilizationManagers[SpiedCiv].SeatOfGovernment;
                _text = "Step_5404:; trying to return SpiedOneCiv.Civilization =;" + SpiedCiv.Civilization.Key;
                Console.WriteLine(_text);
                //GameLog.Client.Intel.DebugFormat("Step_5432:; trying to return SpiedOneCiv.Civilization = {0}", SpiedCiv.Civilization.Key);
                GameLog.Client.Test.DebugFormat("##### trying to return SpiedCiv_1 SeatOfGovernment = {0}", SeatOfGovernment);
                return SeatOfGovernment;
            }
        }
        public static Meter SpiedOneTotalPopulation
        {
            get
            {
                CivilizationManager civManager = DesignTimeObjects.SpiedCiv_1;
                try
                {
                    _text = "Step_5412:; SpiedOneTotalPopulation =;" + civManager.TotalPopulation;
                    Console.WriteLine(_text);
                    GameLog.Core.Intel.DebugFormat(_text);
                    return civManager.TotalPopulation;
                }
                catch (Exception e)
                {
                    Meter zero = new Meter(0, 0, 0);
                    _text = "Step_5414:; Problem occured at SpiedOneTotalPopulation:";
                    Console.WriteLine(_text);

                    GameLog.Core.Intel.WarnFormat(_text);
                    GameLog.Core.General.Error(e);
                    return zero;
                }
            }
        }



        //## Romulan ##
        public static string SpiedRomName => "Romulans";
        public static Civilization SpiedTwoCiv
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_2;
                _text = "Step_5422:; trying to return SpiedTwoCiv.Civilization =;" + SpiedCiv.Civilization.Key;
                Console.WriteLine(_text);
                //GameLog.Client.Intel.DebugFormat("Step_5432:; trying to return SpiedOneCiv.Civilization = {0}", SpiedCiv.Civilization.Key);

                return SpiedCiv.Civilization;
            }
        }
        public static Colony SpiedTwoSeatOfGovernment
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_2;
                Colony SeatOfGovernment = GameContext.Current.CivilizationManagers[SpiedCiv].SeatOfGovernment;
                _text = "Step_5432:; trying to return SpiedOneCiv.Civilization =;" + SpiedCiv.Civilization.Key;
                Console.WriteLine(_text);
                //GameLog.Client.Intel.DebugFormat("Step_5432:; trying to return SpiedOneCiv.Civilization = {0}", SpiedCiv.Civilization.Key);
                GameLog.Client.Test.DebugFormat("##### trying to return SpiedCiv_2 SeatOfGovernment = {0}", SeatOfGovernment);
                return SeatOfGovernment;
            }
        }
        public Meter SpiedTwoTotalPopulation
        {
            get
            {
                CivilizationManager civManager = DesignTimeObjects.SpiedCiv_2;
                try
                {
                    _text = "Step_5422:; trying to return SpiedTwoCiv.Civilization =;" + civManager.Civilization.Key;
                    Console.WriteLine(_text);
                    //GameLog.Core.Intel.DebugFormat(_text);
                    return civManager.TotalPopulation;
                }
                catch (Exception e)
                {
                    Meter zero = new Meter(0, 0, 0);
                    _text = "Step_5424:; trying to return SpiedTwoCiv.Civilization =;" + civManager.Civilization.Key;
                    Console.WriteLine(_text);
                    //GameLog.Client.Intel.DebugFormat("Step_5432:; trying to return SpiedOneCiv.Civilization = {0}", SpiedCiv.Civilization.Key);
                    GameLog.Core.Intel.WarnFormat("Problem occured at SpiedTwoTotalPopulation:");
                    GameLog.Core.General.Error(e);
                    return zero;
                }
            }
        }

        // ## Klingons ##
        public static string SpiedKlingName => "Klingons";
        public static Civilization SpiedThreeCiv
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_3;
                _text = "Step_5432:; trying to return SpiedOneCiv.Civilization =;" + SpiedCiv.Civilization.Key;
                Console.WriteLine(_text);
                //GameLog.Client.Intel.DebugFormat("Step_5432:; trying to return SpiedOneCiv.Civilization = {0}", SpiedCiv.Civilization.Key);

                return SpiedCiv.Civilization;
            }
        }
        public static Colony SpiedThreeSeatOfGovernment
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_3;
                Colony SeatOfGovernment = GameContext.Current.CivilizationManagers[SpiedCiv].SeatOfGovernment;
                _text = "Step_5436:; trying to return SpiedCiv_3 SeatOfGovernment =;" + SeatOfGovernment;
                Console.WriteLine(_text);
                GameLog.Client.Intel.DebugFormat("##### trying to return SpiedCiv_3 SeatOfGovernment = {0}", SeatOfGovernment);
                return SeatOfGovernment;
            }
        }
        public Meter SpiedThreeTotalPopulation
        {
            get
            {
                CivilizationManager civManager = DesignTimeObjects.SpiedCiv_3;
                try
                {
                    _text = "Step_5432:; trying to return SpiedOneCiv.Civilization =;" + civManager.Civilization.Key;
                    Console.WriteLine(_text);
                    GameLog.Core.Intel.DebugFormat("SpiedThreeTotalPopulation ={0}", civManager.TotalPopulation);
                    return civManager.TotalPopulation;
                }
                catch (Exception e)
                {
                    Meter zero = new Meter(0, 0, 0);
                    _text = "Step_5432:; trying to return SpiedOneCiv.Civilization =;" + civManager.Civilization.Key;
                    Console.WriteLine(_text);
                    GameLog.Core.Intel.WarnFormat("Problem occured at SpiedThreeTotalPopulation:");
                    GameLog.Core.General.Error(e);
                    return zero;
                }
            }
        }

        //## Cardassians ##
        public static string SpiedCardName => "Cardassians";
        public static Civilization SpiedFourCiv
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_4;
                _text = "Step_5442:; trying to return SpiedOneCiv.Civilization =;" + SpiedCiv.Civilization.Key;
                Console.WriteLine(_text);
                //GameLog.Client.Intel.DebugFormat(_text);

                return SpiedCiv.Civilization;
            }
        }
        public static Colony SpiedFourSeatOfGovernment
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_4;
                Colony SeatOfGovernment = GameContext.Current.CivilizationManagers[SpiedCiv].SeatOfGovernment;
                _text = "Step_5444:; trying to return SpiedCiv_4 SeatOfGovernment = ;" + SeatOfGovernment;
                Console.WriteLine(_text);
                //GameLog.Client.Intel.DebugFormat(_text);
                return SeatOfGovernment;
            }
        }
        public Meter SpiedFourTotalPopulation
        {
            get
            {
                CivilizationManager civManager = DesignTimeObjects.SpiedCiv_4;
                try
                {
                    _text = "Step_5446:; trying to return SpiedOneCiv.Civilization =;" + civManager.Civilization.Key;
                    Console.WriteLine(_text);
                    //GameLog.Client.Intel.DebugFormat("Step_5432:; trying to return SpiedOneCiv.Civilization = {0}", SpiedCiv.Civilization.Key);
                    GameLog.Core.Intel.DebugFormat("SpiedFourTotalPopulation ={0}", civManager.TotalPopulation);
                    return civManager.TotalPopulation;
                }
                catch (Exception e)
                {
                    Meter zero = new Meter(0, 0, 0);
                    _text = "Step_5432:; trying to return SpiedOneCiv.Civilization =;" + civManager.Civilization.Key;
                    Console.WriteLine(_text);
                    //GameLog.Client.Intel.DebugFormat("Step_5432:; trying to return SpiedOneCiv.Civilization = {0}", SpiedCiv.Civilization.Key);
                    GameLog.Core.Intel.WarnFormat("Problem occured at SpiedFourTotalPopulation:");
                    GameLog.Core.General.Error(e);
                    return zero;
                }
            }
        }

        //## Dominion ##
        public static string SpiedDomName => "Dominion";
        public static Civilization SpiedFiveCiv
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_5;
                _text = "Step_5452:; trying to return SpiedOneCiv.Civilization =;" + SpiedCiv.Civilization.Key;
                Console.WriteLine(_text);
                //GameLog.Client.Intel.DebugFormat("Step_5432:; trying to return SpiedOneCiv.Civilization = {0}", SpiedCiv.Civilization.Key);

                return SpiedCiv.Civilization;
            }
        }
        public static Colony SpiedFiveSeatOfGovernment
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_5;
                Colony SeatOfGovernment = GameContext.Current.CivilizationManagers[SpiedCiv].SeatOfGovernment;
                _text = "Step_5454:; trying to return SpiedOneCiv.Civilization =;" + SpiedCiv.Civilization.Key;
                Console.WriteLine(_text);
                GameLog.Client.Intel.DebugFormat(_text);
                return SeatOfGovernment;
            }
        }
        public Meter SpiedFiveTotalPopulation
        {
            get
            {
                CivilizationManager civManager = DesignTimeObjects.SpiedCiv_5;
                try
                {
                    _text = "Step_5456:; trying to return SpiedOneCiv.Civilization =;" + civManager.Civilization.Key;
                    Console.WriteLine(_text);
                    //GameLog.Client.Intel.DebugFormat("Step_5432:; trying to return SpiedOneCiv.Civilization = {0}", SpiedCiv.Civilization.Key);
                    GameLog.Core.Intel.DebugFormat("SpiedFiveTotalPopulation ={0}", civManager.TotalPopulation);
                    return civManager.TotalPopulation;
                }
                catch (Exception e)
                {
                    Meter zero = new Meter(0, 0, 0);
                    _text = "Step_5458; trying to return SpiedOneCiv.Civilization =;" + civManager.Civilization.Key;
                    Console.WriteLine(_text);
                    //GameLog.Client.Intel.DebugFormat("Step_5432:; trying to return SpiedOneCiv.Civilization = {0}", SpiedCiv.Civilization.Key);
                    GameLog.Core.Intel.WarnFormat("Problem occured at SpiedFiveTotalPopulation:");
                    GameLog.Core.General.Error(e);
                    return zero;
                }
            }
        }

        // ## Borg ##
        public static string SpiedBorgName => "Borg";
        public static Civilization SpiedSixCiv
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_6;
                _text = "Step_5462:; trying to return SpiedOneCiv.Civilization =;" + SpiedCiv.Civilization.Key;
                Console.WriteLine(_text);
                //GameLog.Client.Intel.DebugFormat("Step_5432:; trying to return SpiedOneCiv.Civilization = {0}", SpiedCiv.Civilization.Key);

                return SpiedCiv.Civilization;
            }
        }
        public static Colony SpiedSixSeatOfGovernment
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_6;
                Colony SeatOfGovernment = GameContext.Current.CivilizationManagers[SpiedCiv].SeatOfGovernment;
                _text = "Step_5464:; trying to return SpiedOneCiv.Civilization =;" + SpiedCiv.Civilization.Key;
                Console.WriteLine(_text);
                //GameLog.Client.Intel.DebugFormat("Step_5432:; trying to return SpiedOneCiv.Civilization = {0}", SpiedCiv.Civilization.Key);
                GameLog.Client.Intel.DebugFormat("##### trying to return SpiedCiv_6 SeatOfGovernment = {0}", SeatOfGovernment);
                return SeatOfGovernment;
            }
        }
        public Meter SpiedSixTotalPopulation
        {
            get
            {
                CivilizationManager civManager = DesignTimeObjects.SpiedCiv_6;
                try
                {
                    _text = "Step_5466:; trying to return SpiedOneCiv.Civilization =;" + civManager.Civilization.Key;
                    Console.WriteLine(_text);
                    //GameLog.Client.Intel.DebugFormat("Step_5432:; trying to return SpiedOneCiv.Civilization = {0}", SpiedCiv.Civilization.Key);
                    GameLog.Core.Intel.DebugFormat("SpiedSixTotalPopulation ={0}", civManager.TotalPopulation);
                    return civManager.TotalPopulation;
                }
                catch (Exception e)
                {
                    Meter zero = new Meter(0, 0, 0);
                    _text = "Step_5468:; trying to return SpiedOneCiv.Civilization =;" + civManager.Civilization.Key;
                    Console.WriteLine(_text);
                    //GameLog.Client.Intel.DebugFormat("Step_5432:; trying to return SpiedOneCiv.Civilization = {0}", SpiedCiv.Civilization.Key);
                    GameLog.Core.Intel.WarnFormat("Problem occured at SpiedSixTotalPopulation:");
                    GameLog.Core.General.Error(e);
                    return zero;
                }
            }
        }


        #endregion

        #region Credits Empire

        public Meter CreditsEmpire // do we need this??? Local player only
        {
            get
            {
                    CivilizationManager civManager = GameContext.Current.CivilizationManagers[DesignTimeObjects.CivilizationManager.Civilization];
                try
                {

                    _text = "Step_5472:; trying to return SpiedOneCiv.Civilization =;" + civManager.Civilization.Key;
                    Console.WriteLine(_text);
                    //GameLog.Client.Intel.DebugFormat("Step_5432:; trying to return SpiedOneCiv.Civilization = {0}", SpiedCiv.Civilization.Key);

                    return civManager.Credits;
                }
                catch (Exception e)
                {
                    _text = "Step_5474:; trying to return SpiedOneCiv.Civilization =;" + civManager.Civilization.Key;
                    Console.WriteLine(_text);
                    GameLog.Core.Intel.WarnFormat("Step_5476: Problem occured at CreditsEmpire: {0} {1}", e.Message, e.StackTrace);
                    Meter zero = new Meter(0, 0, 0);
                    return zero;
                }
            }
        }

        #endregion Credits Empire

        #region Implementation of NotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Implementation of INotifyPropertyChanged
        [NonSerialized]
        private PropertyChangedEventHandler _propertyChanged;
        public static string _text;
        public string newline = Environment.NewLine;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                while (true)
                {
                    PropertyChangedEventHandler oldHandler = _propertyChanged;
                    PropertyChangedEventHandler newHandler = (PropertyChangedEventHandler)Delegate.Combine(oldHandler, value);

                    if (Interlocked.CompareExchange(ref _propertyChanged, newHandler, oldHandler) == oldHandler)
                    {
                        return;
                    }
                }
            }
            remove
            {
                while (true)
                {
                    PropertyChangedEventHandler oldHandler = _propertyChanged;
                    PropertyChangedEventHandler newHandler = (PropertyChangedEventHandler)Delegate.Remove(oldHandler, value);

                    if (Interlocked.CompareExchange(ref _propertyChanged, newHandler, oldHandler) == oldHandler)
                    {
                        return;
                    }
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            _propertyChanged.Raise(this, propertyName);
        }

        #endregion

    }
}