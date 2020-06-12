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
using Supremacy.SpyOperations;
using Supremacy.Types;
using Supremacy.Universe;
using Supremacy.Utility;
using Supremacy.Client.Context;



namespace Supremacy.Client.Views
{
    public class AssetsScreenPresentationModel : PresentationModelBase, INotifyPropertyChanged
    {
        protected int _totalIntelligenceProduction;
        protected int _totalIntelligenceDefenseAccumulated;
        protected int _totalIntelligenceAttackingAccumulated;
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

        public CivilizationManager MyLocalCivManager
        {
            get { return IntelHelper.LocalCivManager; }
        }

        public List<Civilization> LocalSpyingCivList
        {
            get
            {
                if (MyLocalCivManager.Civilization.CivID == 0)
                {
                    _localSpyingCivList = IntelHelper._spyingCiv_0_List;

                }
                if (MyLocalCivManager.Civilization.CivID == 1) _localSpyingCivList = IntelHelper._spyingCiv_1_List;
                if (MyLocalCivManager.Civilization.CivID == 2) _localSpyingCivList = IntelHelper._spyingCiv_2_List;
                if (MyLocalCivManager.Civilization.CivID == 3) _localSpyingCivList = IntelHelper._spyingCiv_3_List;
                if (MyLocalCivManager.Civilization.CivID == 4) _localSpyingCivList = IntelHelper._spyingCiv_4_List;
                if (MyLocalCivManager.Civilization.CivID == 5) _localSpyingCivList = IntelHelper._spyingCiv_5_List;
                if (MyLocalCivManager.Civilization.CivID == 6) _localSpyingCivList = IntelHelper._spyingCiv_6_List;

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
         
                    GameLog.Core.UI.DebugFormat("Get TotalIntelProcudtion ={0}", _totalIntelligenceProduction);
                    return _totalIntelligenceProduction;
                }
                catch (Exception e)
                {
                    GameLog.Core.UI.DebugFormat("Problem occured at TotalIntelligenceProduction get, exception {0} {1}", e.Message, e.TargetSite);
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
                    GameLog.Core.UI.DebugFormat("Set TotalIntelProcudtion ={0}", _totalIntelligenceProduction);
                    NotifyPropertyChanged("TotalIntelligenceProduction");
                }
                catch (Exception e)
                {
                    GameLog.Core.UI.DebugFormat("Problem occured at TotalIntelligenceProduction set, Exception {0} {1}", e.Message, e.StackTrace);
                }
            }
        }
        

        public int TotalIntelligenceDefenseAccumulated
        {
            get
            {
                FillUpDefense();          
                _totalIntelligenceDefenseAccumulated = MyLocalCivManager.TotalIntelligenceDefenseAccumulated.CurrentValue;
                GameLog.Core.UI.DebugFormat("Get TotalIntelDefenseAccumulated ={0}", _totalIntelligenceDefenseAccumulated);
                return _totalIntelligenceDefenseAccumulated;
            }
            set
            {
                FillUpDefense();
               //_totalIntelligenceDefenseAccumulated = IntelHelper.DefenseAccumulatedInteInt;
                _totalIntelligenceDefenseAccumulated = MyLocalCivManager.TotalIntelligenceDefenseAccumulated.CurrentValue;
                _totalIntelligenceDefenseAccumulated = value;
                GameLog.Core.UI.DebugFormat("Set TotalIntelDefenseAccumulated ={0}", _totalIntelligenceDefenseAccumulated);
                NotifyPropertyChanged("TotalIntelligenceDefenseAccumulated");
            }
        }

        public int TotalIntelligenceAttackingAccumulated
        {
            get
            {
                FillUpDefense();
                _totalIntelligenceAttackingAccumulated = MyLocalCivManager.TotalIntelligenceAttackingAccumulated.CurrentValue;
                GameLog.Core.UI.DebugFormat("Get TotalIntelDefenseAccumulated ={0}", _totalIntelligenceAttackingAccumulated);
                return _totalIntelligenceAttackingAccumulated;
            }
            set
            {
                FillUpDefense();
               // _totalIntelligenceAttackingAccumulated = IntelHelper.AttackingAccumulatedInteInt;
                _totalIntelligenceAttackingAccumulated = MyLocalCivManager.TotalIntelligenceAttackingAccumulated.CurrentValue;
                _totalIntelligenceAttackingAccumulated = value;
                GameLog.Core.UI.DebugFormat("Set TotalIntelDefenseAccumulated ={0}", _totalIntelligenceAttackingAccumulated);
                NotifyPropertyChanged("TotalIntelligenceAttackingAccumulated");
            }
        }
        public Meter UpdateAttackingAccumulated(Civilization attackingCiv)
        {
            Meter attackMeter = GameContext.Current.CivilizationManagers[attackingCiv].TotalIntelligenceAttackingAccumulated;
            GameLog.Core.UI.DebugFormat("Before update attackMeter ={0} for attakcing civ ={1}", attackMeter, attackingCiv);
            int newAttackIntelligence = 0;
            Int32.TryParse(attackMeter.CurrentValue.ToString(), out newAttackIntelligence);
            _totalIntelligenceAttackingAccumulated = newAttackIntelligence;
            GameLog.Core.UI.DebugFormat(" After update attackMeter ={0} for attacking civ ={1}", attackMeter, attackingCiv);
            return attackMeter;
        }
        protected virtual void FillUpDefense()
        {
            var civ = GameContext.Current.CivilizationManagers[MyLocalCivManager.Civilization];
            civ.TotalIntelligenceAttackingAccumulated.AdjustCurrent(civ.TotalIntelligenceAttackingAccumulated.CurrentValue * -1); // remove from Attacking
            civ.TotalIntelligenceAttackingAccumulated.UpdateAndReset();
            civ.TotalIntelligenceDefenseAccumulated.AdjustCurrent(civ.TotalIntelligenceDefenseAccumulated.CurrentValue); // add to Defense
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
                throw new InvalidOperationException("This constructor should only be invoked at design time.");

            _colonies = MyLocalCivManager.Colonies; //not the host on a remote machine, DesignTimeObjects.LocalCivManager.Colonies;
            _spiedZeroColonies = DesignTimeObjects.SpiedCivZero.Colonies;
            _spiedOneColonies = DesignTimeObjects.SpiedCivOne.Colonies;
            _spiedTwoColonies = DesignTimeObjects.SpiedCivTwo.Colonies;
            _spiedThreeColonies = DesignTimeObjects.SpiedCivThree.Colonies;
            _spiedFourColonies = DesignTimeObjects.SpiedCivFour.Colonies;
            _spiedFiveColonies = DesignTimeObjects.SpiedCivFive.Colonies;
            _spiedSixColonies = DesignTimeObjects.SpiedCivSix.Colonies;
            _totalIntelligenceProduction = GameContext.Current.CivilizationManagers[MyLocalCivManager.Civilization].TotalIntelligenceProduction;
            _totalIntelligenceAttackingAccumulated = GameContext.Current.CivilizationManagers[MyLocalCivManager.Civilization].TotalIntelligenceAttackingAccumulated.CurrentValue;
            _totalIntelligenceDefenseAccumulated = GameContext.Current.CivilizationManagers[MyLocalCivManager.Civilization].TotalIntelligenceDefenseAccumulated.CurrentValue;

            OnPropertyChanged("InstallingSpyNetwork");
            OnPropertyChanged("TotalIntelligenceAttackingAccumulated");
            OnPropertyChanged("TotalIntelligenceDefenseAccumulated");
            OnPropertyChanged("TotalIntelligenceProduction");
        }

        #region Colonies Property

        [field: NonSerialized]
        public event EventHandler ColoniesChanged;
        public event EventHandler TotalPopulationChanged;
        public event EventHandler InstallingSpyNetworkChanged;
        public event EventHandler TotalIntelligenceProductionChanged;
        public event EventHandler TotalIntelligenceAttackingAccumulatedChanged;
        public event EventHandler TotalIntelligenceDefenseAccumulatedChanged;

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
            get { return _colonies; }
            set
            {
                if (Equals(value, _colonies))
                    return;

                _colonies = value;

                FillUpDefense();
                OnColoniesChanged();
                OnTotalPopulationChanged();
                OnInstallingSpyNetworkChanged();
                OnTotalIntelligenceProductionChanged();
                OnTotalIntelligenceAttackingAccumulatedChanged();
                OnTotalIntelligenceDefenseAccumulatedChanged();
            }
        }
        public IEnumerable<Colony> SpiedZeroColonies
        {
            get { return _spiedZeroColonies; }
            set
            {
                if (Equals(value, _spiedZeroColonies))
                    return;

                _spiedZeroColonies = value;

                OnSpiedZeroColoniesChanged();
                OnSpiedZeroTotalPopulationChanged();
            }
        }
        public IEnumerable<Colony> SpiedOneColonies
        {
            get { return _spiedOneColonies; }
            set
            {
                if (Equals(value, _spiedOneColonies))
                    return;

                _spiedOneColonies = value;
               
                OnSpiedOneColoniesChanged();
                OnSpiedOneTotalPopulationChanged();
            }
        }
        public IEnumerable<Colony> SpiedTwoColonies
        {
            get { return _spiedTwoColonies; }
            set
            {
                if (Equals(value, _spiedTwoColonies))
                    return;

                _spiedTwoColonies = value;

                OnSpiedTwoColoniesChanged();
                OnSpiedTwoTotalPopulationChanged();
            }
        }
        public IEnumerable<Colony> SpiedThreeColonies
        {
            get { return _spiedThreeColonies; }
            set
            {
                if (Equals(value, _spiedThreeColonies))
                    return;

                _spiedThreeColonies = value;

                OnSpiedThreeColoniesChanged();
                OnSpiedThreeTotalPopulationChanged();
            }
        }
        public IEnumerable<Colony> SpiedFourColonies
        {
            get { return _spiedFourColonies; }
            set
            {
                if (Equals(value, _spiedFourColonies))
                    return;

                _spiedFourColonies = value;

                OnSpiedFourColoniesChanged();
                OnSpiedFourTotalPopulationChanged();
            }
        }
        public IEnumerable<Colony> SpiedFiveColonies
        {
            get { return _spiedFiveColonies; }
            set
            {
                if (Equals(value, _spiedFiveColonies))
                    return;

                _spiedFiveColonies = value;

                OnSpiedFiveColoniesChanged();
                OnSpiedFiveTotalPopulationChanged();
            }
        }
        public IEnumerable<Colony> SpiedSixColonies
        {
            get { return _spiedSixColonies; }
            set
            {
                if (Equals(value, _spiedSixColonies))
                    return;

                _spiedSixColonies = value;

                OnSpiedSixColoniesChanged();
                OnSpiedSixTotalPopulationChanged();
            }
        }
        protected virtual void OnColoniesChanged()
        {
            //GameLog.Core.UI.DebugFormat("AssetsScreenPresenterModel OnColoniesChange at line 228");
            ColoniesChanged.Raise(this);
            OnPropertyChanged("Colonies");
        }
        protected virtual void OnTotalPopulationChanged()
        {
            TotalPopulationChanged.Raise(this);
            OnPropertyChanged("TotalPopulation");
        }
        protected virtual void OnInstallingSpyNetworkChanged()
        {
            InstallingSpyNetworkChanged.Raise(this);
            OnPropertyChanged("InstallingSpyNetwork");
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
                var civManager = MyLocalCivManager; // not this DesignTimeObjects.LocalCivManager.Civilization
                try
                {
                    //GameLog.Core.Intel.DebugFormat("TotalPopulation ={0}", civManager.TotalPopulation);
                    return civManager.TotalPopulation;
                }
                catch (Exception e)
                {
                    GameLog.Core.Stations.WarnFormat("Problem occured at TotalPopulation: {0} {1}", e.Message, e.StackTrace);
                    GameLog.Core.General.Error(e);
                    Meter zero = new Meter(0, 0, 0);
                    return zero; //civManager.TotalPopulation;

                }
            }
        }

        public string LocalCivName
        {
            get
            {
                return MyLocalCivManager.Civilization.Name;  // keep this on AppContext
            }
        }
        public static Civilization LocalCiv
        {
            get
            {
                return IntelHelper.LocalCivManager.Civilization;
            }
        }
        // ### Federation ####
        public static Civilization SpiedZeroCiv
        {
            get
            {
                var SpiedCiv = DesignTimeObjects.SpiedCivZero;
                GameLog.Client.Test.DebugFormat("##### trying to return SpiedCiv.Civilization = {0}", SpiedCiv.Civilization.Key);
                return SpiedCiv.Civilization;
            }
        }
        public static Colony SpiedZeroSeatOfGovernment
        {
            get
            {
                var SpiedCiv = DesignTimeObjects.SpiedCivZero;
                var SeatOfGovernment = GameContext.Current.CivilizationManagers[SpiedCiv].SeatOfGovernment;
                GameLog.Client.Test.DebugFormat("##### trying to return SpiedCivZero SeatOfGovernment = {0}", SeatOfGovernment);
                return SeatOfGovernment;
            }
        }
        public static Meter SpiedZeroTotalPopulation
        {
            get
            {
                var civManager = DesignTimeObjects.SpiedCivZero;
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
        public static string SpiedFedName
        {
            get
            {
                return "Federation";
            }
        }
        //## Terran ##
        public static Civilization SpiedOneCiv
        {
            get
            {
                var SpiedCiv = DesignTimeObjects.SpiedCivOne;
                GameLog.Client.Intel.DebugFormat("##### trying to return SpiedOneCiv.Civilization = {0}", SpiedCiv.Civilization.Key);
                return SpiedCiv.Civilization;
            }
        }
        public static Colony SpiedOneSeatOfGovernment
        {
            get
            {
                var SpiedCiv = DesignTimeObjects.SpiedCivOne;
                var SeatOfGovernment = GameContext.Current.CivilizationManagers[SpiedCiv].SeatOfGovernment;
                GameLog.Client.Test.DebugFormat("##### trying to return SpiedCivOne SeatOfGovernment = {0}", SeatOfGovernment);
                return SeatOfGovernment;
            }
        }
        public static Meter SpiedOneTotalPopulation
        {
            get
            {
                var civManager = DesignTimeObjects.SpiedCivOne;
                try
                {
                    GameLog.Core.Intel.DebugFormat("SpiedOneTotalPopulation ={0}", civManager.TotalPopulation);
                    return civManager.TotalPopulation;
                }
                catch (Exception e)
                {
                    Meter zero = new Meter(0, 0, 0);
                    GameLog.Core.Intel.WarnFormat("Problem occured at SpiedOneTotalPopulation:");
                    GameLog.Core.General.Error(e);
                    return zero;
                }
            }
        }  
        public static string SpiedTerranName
        {
            get
            {
                return "Terran Empire"; 
            }
        }
        //## Romulan ##
        public static Civilization SpiedTwoCiv
        {
            get
            {
                var SpiedCiv = DesignTimeObjects.SpiedCivTwo;
                return SpiedCiv.Civilization;
            }
        }
        public static Colony SpiedTwoSeatOfGovernment
        {
            get
            {
                var SpiedCiv = DesignTimeObjects.SpiedCivTwo;
                var SeatOfGovernment = GameContext.Current.CivilizationManagers[SpiedCiv].SeatOfGovernment;
                GameLog.Client.Test.DebugFormat("##### trying to return SpiedCivTwo SeatOfGovernment = {0}", SeatOfGovernment);
                return SeatOfGovernment;
            }
        }
        public Meter SpiedTwoTotalPopulation
        {
            get
            {
                var civManager = DesignTimeObjects.SpiedCivTwo;
                try
                {
                    GameLog.Core.Intel.DebugFormat("SpiedTwoTotalPopulation ={0}", civManager.TotalPopulation);
                    return civManager.TotalPopulation;
                }
                catch (Exception e)
                {
                    Meter zero = new Meter(0, 0, 0);
                    GameLog.Core.Intel.WarnFormat("Problem occured at SpiedTwoTotalPopulation:");
                    GameLog.Core.General.Error(e);
                    return zero;
                }
            }
        }
        public static string SpiedRomName
        {
            get
            {
                return "Romulans"; 
            }
        }
        // ## Klingons ##
        public static Civilization SpiedThreeCiv
        {
            get
            {
                var SpiedCiv = DesignTimeObjects.SpiedCivThree;
                return SpiedCiv.Civilization;
            }
        }
        public static Colony SpiedThreeSeatOfGovernment
        {
            get
            {
                var SpiedCiv = DesignTimeObjects.SpiedCivThree;
                var SeatOfGovernment = GameContext.Current.CivilizationManagers[SpiedCiv].SeatOfGovernment;
                GameLog.Client.Intel.DebugFormat("##### trying to return SpiedCivThree SeatOfGovernment = {0}", SeatOfGovernment);
                return SeatOfGovernment;
            }
        }
        public Meter SpiedThreeTotalPopulation
        {
            get
            {
                var civManager = DesignTimeObjects.SpiedCivThree;
                try
                {
                    GameLog.Core.Intel.DebugFormat("SpiedThreeTotalPopulation ={0}", civManager.TotalPopulation);
                    return civManager.TotalPopulation;
                }
                catch (Exception e)
                {
                    Meter zero = new Meter(0, 0, 0);
                    GameLog.Core.Intel.WarnFormat("Problem occured at SpiedThreeTotalPopulation:");
                    GameLog.Core.General.Error(e);
                    return zero;
                }
            }
        }
        public static string SpiedKlingName
        {
            get
            {
                return "Klingons";
            }
        }
        //## Cardassians ##
        public static Civilization SpiedFourCiv
        {
            get
            {
                var SpiedCiv = DesignTimeObjects.SpiedCivFour;
                return SpiedCiv.Civilization;
            }
        }
        public static Colony SpiedFourSeatOfGovernment
        {
            get
            {
                var SpiedCiv = DesignTimeObjects.SpiedCivFour;
                var SeatOfGovernment = GameContext.Current.CivilizationManagers[SpiedCiv].SeatOfGovernment;
                GameLog.Client.Intel.DebugFormat("##### trying to return SpiedCivFour SeatOfGovernment = {0}", SeatOfGovernment);
                return SeatOfGovernment;
            }
        }
        public Meter SpiedFourTotalPopulation
        {
            get
            {
                var civManager = DesignTimeObjects.SpiedCivFour;
                try
                {
                    GameLog.Core.Intel.DebugFormat("SpiedFourTotalPopulation ={0}", civManager.TotalPopulation);
                    return civManager.TotalPopulation;
                }
                catch (Exception e)
                {
                    Meter zero = new Meter(0, 0, 0);
                    GameLog.Core.Intel.WarnFormat("Problem occured at SpiedFourTotalPopulation:");
                    GameLog.Core.General.Error(e);
                    return zero;
                }
            }
        }
        public static string SpiedCardName
        {
            get
            {
                return "Cardassians";
            }
        }
        //## Dominion ##
        public static Civilization SpiedFiveCiv
        {
            get
            {
                var SpiedCiv = DesignTimeObjects.SpiedCivFive;
                return SpiedCiv.Civilization;
            }
        }
        public static Colony SpiedFiveSeatOfGovernment
        {
            get
            {
                var SpiedCiv = DesignTimeObjects.SpiedCivFive;
                var SeatOfGovernment = GameContext.Current.CivilizationManagers[SpiedCiv].SeatOfGovernment;
                GameLog.Client.Intel.DebugFormat("##### trying to return SpiedCivFive SeatOfGovernment = {0}", SeatOfGovernment);
                return SeatOfGovernment;
            }
        }
        public Meter SpiedFiveTotalPopulation
        {
            get
            {
                var civManager = DesignTimeObjects.SpiedCivFive;
                try
                {
                    GameLog.Core.Intel.DebugFormat("SpiedFiveTotalPopulation ={0}", civManager.TotalPopulation);
                    return civManager.TotalPopulation;
                }
                catch (Exception e)
                {
                    Meter zero = new Meter(0, 0, 0);
                    GameLog.Core.Intel.WarnFormat("Problem occured at SpiedFiveTotalPopulation:");
                    GameLog.Core.General.Error(e);
                    return zero;
                }
            }
        }
        public static string SpiedDomName
        {
            get
            {
                return "Dominion";
            }
        }
        // ## Borg ##
        public static Civilization SpiedSixCiv
        {
            get
            {
                var SpiedCiv = DesignTimeObjects.SpiedCivSix;
                return SpiedCiv.Civilization;
            }
        }
        public static Colony SpiedSixSeatOfGovernment
        {
            get
            {
                var SpiedCiv = DesignTimeObjects.SpiedCivSix;
                var SeatOfGovernment = GameContext.Current.CivilizationManagers[SpiedCiv].SeatOfGovernment;
                GameLog.Client.Intel.DebugFormat("##### trying to return SpiedCivSix SeatOfGovernment = {0}", SeatOfGovernment);
                return SeatOfGovernment;
            }
        }
        public Meter SpiedSixTotalPopulation
        {
            get
            {
                var civManager = DesignTimeObjects.SpiedCivSix;
                try
                {
                    GameLog.Core.Intel.DebugFormat("SpiedSixTotalPopulation ={0}", civManager.TotalPopulation);
                    return civManager.TotalPopulation;
                }
                catch (Exception e)
                {
                    Meter zero = new Meter(0, 0, 0);
                    GameLog.Core.Intel.WarnFormat("Problem occured at SpiedSixTotalPopulation:");
                    GameLog.Core.General.Error(e);
                    return zero;
                }
            }
        }
        public static string SpiedBorgName
        {
            get
            {
                return "Borg";
            }
        }

        #endregion

        #region Credits Empire

        public Meter CreditsEmpire // do we need this??? Local player only
        {
            get
            {
                try
                {
                    var civManager = GameContext.Current.CivilizationManagers[DesignTimeObjects.CivilizationManager.Civilization];
                    return civManager.Credits;
                }
                catch (Exception e)
                {
                    GameLog.Core.Intel.WarnFormat("Problem occured at CreditsEmpire: {0} {1}", e.Message, e.StackTrace);
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
            if (PropertyChanged!= null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region Implementation of INotifyPropertyChanged
        [NonSerialized]
        private PropertyChangedEventHandler _propertyChanged;
        

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                while (true)
                {
                    var oldHandler = _propertyChanged;
                    var newHandler = (PropertyChangedEventHandler)Delegate.Combine(oldHandler, value);

                    if (Interlocked.CompareExchange(ref _propertyChanged, newHandler, oldHandler) == oldHandler)
                        return;
                }
            }
            remove
            {
                while (true)
                {
                    var oldHandler = _propertyChanged;
                    var newHandler = (PropertyChangedEventHandler)Delegate.Remove(oldHandler, value);

                    if (Interlocked.CompareExchange(ref _propertyChanged, newHandler, oldHandler) == oldHandler)
                        return;
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            _propertyChanged.Raise(this, propertyName);
        }

        #endregion

        //protected void Reset() // do we need this??
        //{
        //    TotalIntelligenceAttackingAccumulated = 1;
        //    TotalIntelligenceDefenseAccumulated = 1;
        //}


        //public static /*DiplomaticMessageCategory*/ SpyOrderCategory ResolveSpyOrderCategory(object message) // DiplomaticMessageCategory is enum of 1 to 9 message types
        //{
        //    //GameLog.Client.Diplomacy.DebugFormat("ResolveMessageCategory beginning...");

        //    var viewModel = message as DiplomacyMessageViewModel;

        //    //var proposal = message as IProposal;

        //    //if (viewModel != null)
        //    //{
        //    //    // worksGameLog.Client.Diplomacy.DebugFormat("Message: Sender ={1} *vs* Recipient = {0}", viewModel.Recipient, viewModel.Sender);
        //    //    //GameLog.Client.Diplomacy.DebugFormat("Message: Sender ={1} *vs* Recipient = {0} - Category {2}", viewModel.Recipient, viewModel.Sender, proposal.ToString());
        //    //    message = viewModel.CreateMessage(); // create statment vs create proposal
        //    //}

        //    //var proposal = message as IProposal;

        //    //GameLog.Client.Diplomacy.DebugFormat("proposal ={0}", proposal);
        //    //if (proposal != null)
        //    //{
        //    //    //GameLog.Client.Diplomacy.DebugFormat("Proposal: Sender ={1} *vs* Recipient = {0} ", proposal.Recipient, proposal.Sender);
        //    //    //GameLog.Client.Diplomacy.DebugFormat("Proposal: Sender ={1} *vs* Recipient = {0} - Category {2}", proposal.Recipient, proposal.Sender, proposal.ToString());
        //    //    if (proposal.IsDemand())
        //    //    {
        //    //        GameLog.Client.Diplomacy.DebugFormat("Message Category Demand");
        //    //        return DiplomaticMessageCategory.Demand;
        //    //    }
        //    //    if (proposal.IsGift())
        //    //    {
        //    //        GameLog.Client.Diplomacy.DebugFormat("Message Category Gift");
        //    //        return DiplomaticMessageCategory.Gift;
        //    //    }
        //    //    foreach (var clause in proposal.Clauses)
        //    //    {
        //    //        if (!clause.ClauseType.IsTreatyClause())
        //    //            continue;
        //    //        if (clause.ClauseType == ClauseType.TreatyWarPact)
        //    //        {
        //    //            GameLog.Client.Diplomacy.DebugFormat("Message Category 1 War Pact");
        //    //            return DiplomaticMessageCategory.WarPact;
        //    //        }
        //    //        GameLog.Client.Diplomacy.DebugFormat("Message Category 2 Treaty");
        //    //        return DiplomaticMessageCategory.Treaty;
        //    //    }
        //    //    GameLog.Client.Diplomacy.DebugFormat("Message Exchange");
        //    //    return DiplomaticMessageCategory.Exchange;
        //    //}

        //    //var response = message as IResponse;

        //    //if (response != null)
        //    //{
        //    //    GameLog.Client.Diplomacy.DebugFormat("Response Recipient ={0} Sender ={1}", response.Recipient, response.Sender);
        //    //    return DiplomaticMessageCategory.Response;
        //    //}

        //    //var statement = message as Statement;

        //    //if (statement != null)
        //    //{
        //    //    GameLog.Client.Diplomacy.DebugFormat("Statement Recipient ={0} Sender ={1}", statement.Recipient, statement.Sender);
        //    //    switch (statement.StatementType)
        //    //    {
        //    //        case StatementType.CommendRelationship:
        //    //        case StatementType.CommendAssault:
        //    //        case StatementType.CommendInvasion:
        //    //        case StatementType.CommendSabotage:
        //    //        case StatementType.DenounceWar:
        //    //        case StatementType.DenounceRelationship:
        //    //        case StatementType.DenounceAssault:
        //    //        case StatementType.DenounceInvasion:
        //    //        case StatementType.DenounceSabotage:    // listing was changed
        //    //            GameLog.Client.Diplomacy.DebugFormat("Message Statement");
        //    //            return DiplomaticMessageCategory.Statement;

        //    //        case StatementType.ThreatenDestroyColony:
        //    //        case StatementType.ThreatenTradeEmbargo:
        //    //        case StatementType.ThreatenDeclareWar:
        //    //            GameLog.Client.Diplomacy.DebugFormat("Message Threat");
        //    //            return DiplomaticMessageCategory.Threat;
        //    //    }
        //    //}
        //    // a lot of times hitted without giving an info ... GameLog.Client.Diplomacy.DebugFormat("Message Category None");
        //    return SpyOrderCategory.None;
        //}


        //#region OutgoingSpyOrder Property

        //[field: NonSerialized]
        //public event EventHandler OutgoingSpyOrderChanged;

        //private /*DiplomacyMessageViewModel*/ NewSpyOrder _outgoingSpyOrder;

        //public /*DiplomacyMessageViewModel*/ NewSpyOrder OutgoingSpyOrder
        //{
        //    get
        //    {
        //        // Gamelog for GET _outgoingSpyOrder mostly not needed
        //        //if (_outgoingSpyOrder != null && _outgoingSpyOrder.Elements.Count() > 0)
        //        GameLog.Client.Diplomacy.DebugFormat("OutgoingSpyOrder GET = {0} >> {1}",
        //                _outgoingSpyOrder.Sender.Name, _outgoingSpyOrder.Recipient.Name);
        //                    //, _outgoingSpyOrder.Elements.Count().ToString());
        //        return _outgoingSpyOrder;
        //    }
        //    set
        //    {
        //        if (Equals(value, _outgoingSpyOrder))
        //            return;

        //        _outgoingSpyOrder = value;

        //        OnOutgoingSpyOrderChanged();

        //        //if (_outgoingSpyOrder != null && _outgoingSpyOrder.Elements.Count() > 0)
        //        GameLog.Client.Diplomacy.DebugFormat("OutgoingSpyOrder SET = {0} >> {1}",
        //            _outgoingSpyOrder.Sender.Name, _outgoingSpyOrder.Recipient.Name);
        //           //, _outgoingSpyOrder.Elements.Count().ToString());
        //    }
        //}

        //protected virtual void OnOutgoingSpyOrderChanged()
        //{
        //    //GameLog.Client.Diplomacy.DebugFormat("Now at OnOutgoingSpyOrderChanged() - call OnPropertyChanged");
        //    OutgoingSpyOrderChanged.Raise(this);
        //    OnPropertyChanged("OutgoingSpyOrder");
        //    OnOutgoingSpyOrderCategoryChanged();
        //}

        //#endregion OutgoingSpyOrder Property

        //#region OutgoingSpyOrderCategory Property

        //[field: NonSerialized]
        //public event EventHandler OutgoingSpyOrderCategoryChanged;

        //public /*DiplomaticMessageCategory*/ SpyOrderCategory OutgoingSpyOrderCategory
        //{
        //    get
        //    {
        //        if (ResolveSpyOrderCategory(OutgoingSpyOrder).ToString() != "None")
        //            GameLog.Client.Diplomacy.DebugFormat("OutgoingSpyOrderCategory = {0}", ResolveSpyOrderCategory(OutgoingSpyOrder));
        //        return ResolveSpyOrderCategory(OutgoingSpyOrder);
        //    }
        //}

        //protected internal virtual void OnOutgoingSpyOrderCategoryChanged()
        //{
        //    GameLog.Client.Diplomacy.DebugFormat("SpyOrder Category Changed");
        //    OutgoingSpyOrderCategoryChanged.Raise(this);
        //    OnPropertyChanged("OutgoingSpyOrderCategory");
        //}


        //#endregion OutgoingSpyOrderCategory Property


    }
}