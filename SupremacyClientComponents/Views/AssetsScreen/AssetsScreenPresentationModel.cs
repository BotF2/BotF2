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
using System.Linq;



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
                    _text = "Step_5454:; Get TotalIntelProduction=; " + _totalIntelligenceProduction
                              ;
                    Console.WriteLine(_text);
                    //GameLog.Client.Intel.DebugFormat("Get TotalIntelProduction ={0}", _totalIntelligenceProduction);
                    return _totalIntelligenceProduction;
                }
                catch (Exception e)
                {
                    _text = "Step_5456:; Problem occured at TotalIntelligenceProduction get, exception "
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
                    //FillUpDefense();
                    _totalIntelligenceProduction = value;
                    _text = "Step_5458:; Set TotalIntelProduction=; " + _totalIntelligenceProduction;

                    Console.WriteLine(_text);
                    //GameLog.Client.Intel.DebugFormat(_text);
                    NotifyPropertyChanged("TotalIntelligenceProduction");
                }
                catch (Exception e)
                {
                    _text = "Step_5457:; Problem occured at TotalIntelligenceProduction set;" + e.Message + e.StackTrace;
          
                    Console.WriteLine(_text);
                    GameLog.Client.Intel.DebugFormat("Problem occured at TotalIntelligenceProduction set, Exception {0} {1}", e.Message, e.StackTrace);
                }
            }
        }


        public int TotalIntelligenceDefenseAccumulated
        {
            get
            {
                //FillUpDefense();
                _totalIntelligenceDefenseAccumulated = MyLocalCivManager.TotalIntelligenceDefenseAccumulated.CurrentValue;
                //works
                _text = "Step_5450:; Get TotalIntelDefenseAccumulated=; " + _totalIntelligenceDefenseAccumulated;
          
                Console.WriteLine(_text);
                //GameLog.Client.Intel.DebugFormat("Get TotalIntelDefenseAccumulated ={0}", _totalIntelligenceDefenseAccumulated);
                return _totalIntelligenceDefenseAccumulated;
            }
            set
            {
                //FillUpDefense();
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
                //FillUpDefense();
                _totalIntelligenceAttackingAccumulated = MyLocalCivManager.TotalIntelligenceAttackingAccumulated.CurrentValue;
                //works
                _text = "Step_5452:; Get TotalIntelDefenseAccumulated=; " + _totalIntelligenceProduction;
          
                Console.WriteLine(_text);
                //GameLog.Client.Intel.DebugFormat("Get TotalIntelDefenseAccumulated ={0}", _totalIntelligenceAttackingAccumulated);
                return _totalIntelligenceAttackingAccumulated;
            }
            set
            {
                //FillUpDefense();
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
            //GameLog.Client.Intel.DebugFormat("Before update attackMeter ={0} for attakcing civ ={1}", attackMeter, attackingCiv);
            _ = int.TryParse(attackMeter.CurrentValue.ToString(), out int newAttackIntelligence);
            _totalIntelligenceAttackingAccumulated = newAttackIntelligence;
            //works
            _text = "Step_5444:; After update attackMeter =;" + attackMeter + "; for attacking civ =;" + attackingCiv;
            Console.WriteLine(_text);
            //GameLog.Client.Intel.DebugFormat(" After update attackMeter ={0} for attacking civ ={1}", attackMeter, attackingCiv);
            return attackMeter;
        }
        //protected virtual void FillUpDefense()
        //{
        //    CivilizationManager civ = GameContext.Current.CivilizationManagers[MyLocalCivManager.Civilization];
        //    _ = civ.TotalIntelligenceAttackingAccumulated.AdjustCurrent(civ.TotalIntelligenceAttackingAccumulated.CurrentValue * -1); // remove from Attacking
        //    civ.TotalIntelligenceAttackingAccumulated.UpdateAndReset();
        //    _ = civ.TotalIntelligenceDefenseAccumulated.AdjustCurrent(civ.TotalIntelligenceDefenseAccumulated.CurrentValue); // add to Defense
        //    civ.TotalIntelligenceDefenseAccumulated.UpdateAndReset();
        //    //OnPropertyChanged("TotalIntelligenceAttackingAccumulated");
        //    //OnPropertyChanged("TotalIntelligenceDefenseAccumulated");
        //    //OnPropertyChanged("TotalIntelligenceProduction");

        //}
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
            //_spied_0_Colonies = DesignTimeObjects.SpiedCiv_0.Colonies;
            _spied_0_Colonies = (IEnumerable<Colony>)DesignTimeObjects.SpiedCiv_0.SeatOfGovernment.System.Colony;
            _spied_1_Colonies = (IEnumerable<Colony>)DesignTimeObjects.SpiedCiv_1.SeatOfGovernment.System.Colony;
            _spied_2_Colonies = (IEnumerable<Colony>)DesignTimeObjects.SpiedCiv_2.SeatOfGovernment.System.Colony;
            _spied_3_Colonies = (IEnumerable<Colony>)DesignTimeObjects.SpiedCiv_3.SeatOfGovernment.System.Colony;
            _spied_4_Colonies = (IEnumerable<Colony>)DesignTimeObjects.SpiedCiv_4.SeatOfGovernment.System.Colony;
            _spied_5_Colonies = (IEnumerable<Colony>)DesignTimeObjects.SpiedCiv_5.SeatOfGovernment.System.Colony;
            _spied_6_Colonies = (IEnumerable<Colony>)DesignTimeObjects.SpiedCiv_6.SeatOfGovernment.System.Colony;
            //_spied_2_Colonies = DesignTimeObjects.SpiedCiv_2.Colonies;
            //_spied_3_Colonies = DesignTimeObjects.SpiedCiv_3.Colonies;
            //_spied_4_Colonies = DesignTimeObjects.SpiedCiv_4.Colonies;
            //_spied_5_Colonies = DesignTimeObjects.SpiedCiv_5.Colonies;
            //_spied_6_Colonies = DesignTimeObjects.SpiedCiv_6.Colonies;
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

        public event EventHandler Spied_0_ColoniesChanged;
        public event EventHandler Spied_0_TotalPopulationChanged;

        public event EventHandler Spied_1_ColoniesChanged;
        public event EventHandler Spied_1_TotalPopulationChanged;

        public event EventHandler Spied_2_ColoniesChanged;
        public event EventHandler Spied_2_TotalPopulationChanged;

        public event EventHandler Spied_3_ColoniesChanged;
        public event EventHandler Spied_3_TotalPopulationChanged;

        public event EventHandler Spied_4_ColoniesChanged;
        public event EventHandler Spied_4_TotalPopulationChanged;

        public event EventHandler Spied_5_ColoniesChanged;
        public event EventHandler Spied_5_TotalPopulationChanged;

        public event EventHandler Spied_6_ColoniesChanged;
        public event EventHandler Spied_6_TotalPopulationChanged;

        private IEnumerable<Colony> _colonies;

        private IEnumerable<Colony> _spied_0_Colonies;

        private IEnumerable<Colony> _spied_1_Colonies;

        private IEnumerable<Colony> _spied_2_Colonies;

        private IEnumerable<Colony> _spied_3_Colonies;

        private IEnumerable<Colony> _spied_4_Colonies;

        private IEnumerable<Colony> _spied_5_Colonies;

        private IEnumerable<Colony> _spied_6_Colonies;

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

                //FillUpDefense();
                OnColoniesChanged();
                OnTotalPopulationChanged();
                OnTotalResearchChanged();
                OnInstallingSpyNetworkChanged();
                OnTotalIntelligenceProductionChanged();
                OnTotalIntelligenceAttackingAccumulatedChanged();
                OnTotalIntelligenceDefenseAccumulatedChanged();
            }
        }
        public IEnumerable<Colony> Spied_0_Colonies
        {
            get => _spied_0_Colonies;
            set
            {
                if (Equals(value, _spied_0_Colonies))
                {
                    return;
                }

                _spied_0_Colonies = value;

                OnSpied_0_ColoniesChanged();
                OnSpied_0_TotalPopulationChanged();
            }
        }
        public IEnumerable<Colony> Spied_1_Colonies
        {
            get => _spied_1_Colonies;
            set
            {
                if (Equals(value, _spied_1_Colonies))
                {
                    return;
                }

                _spied_1_Colonies = value;

                OnSpied_1_ColoniesChanged();
                OnSpied_1_TotalPopulationChanged();
            }
        }
        public IEnumerable<Colony> Spied_2_Colonies
        {
            get => _spied_2_Colonies;
            set
            {
                if (Equals(value, _spied_2_Colonies))
                {
                    return;
                }

                _spied_2_Colonies = value;

                OnSpied_2_ColoniesChanged();
                OnSpied_2_TotalPopulationChanged();
            }
        }
        public IEnumerable<Colony> Spied_3_Colonies
        {
            get => _spied_3_Colonies;
            set
            {
                if (Equals(value, _spied_3_Colonies))
                {
                    return;
                }

                _spied_3_Colonies = value;

                OnSpied_3_ColoniesChanged();
                OnSpied_3_TotalPopulationChanged();
            }
        }
        public IEnumerable<Colony> Spied_4_Colonies
        {
            get => _spied_4_Colonies;
            set
            {
                if (Equals(value, _spied_4_Colonies))
                {
                    return;
                }

                _spied_4_Colonies = value;

                OnSpied_4_ColoniesChanged();
                OnSpied_4_TotalPopulationChanged();
            }
        }
        public IEnumerable<Colony> Spied_5_Colonies
        {
            get => _spied_5_Colonies;
            set
            {
                if (Equals(value, _spied_5_Colonies))
                {
                    return;
                }

                _spied_5_Colonies = value;

                OnSpied_5_ColoniesChanged();
                OnSpied_5_TotalPopulationChanged();
            }
        }
        public IEnumerable<Colony> Spied_6_Colonies
        {
            get => _spied_6_Colonies;
            set
            {
                if (Equals(value, _spied_6_Colonies))
                {
                    return;
                }

                _spied_6_Colonies = value;

                OnSpied_6_ColoniesChanged();
                OnSpied_6_TotalPopulationChanged();
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
        protected virtual void OnSpied_0_ColoniesChanged()
        {
            Spied_0_ColoniesChanged.Raise(this);
            OnPropertyChanged("Spied_0_Colonies");
        }
        protected virtual void OnSpied_1_ColoniesChanged()
        {
            Spied_1_ColoniesChanged.Raise(this);
            OnPropertyChanged("Spied_1_Colonies");
        }
        protected virtual void OnSpied_2_ColoniesChanged()
        {
            Spied_2_ColoniesChanged.Raise(this);
            OnPropertyChanged("Spied_2_Colonies");
        }
        protected virtual void OnSpied_3_ColoniesChanged()
        {
            Spied_3_ColoniesChanged.Raise(this);
            OnPropertyChanged("Spied_3_Colonies");
        }
        protected virtual void OnSpied_4_ColoniesChanged()
        {
            Spied_4_ColoniesChanged.Raise(this);
            OnPropertyChanged("Spied_4_Colonies");
        }
        protected virtual void OnSpied_5_ColoniesChanged()
        {
            Spied_5_ColoniesChanged.Raise(this);
            OnPropertyChanged("Spied_5_Colonies");
        }
        protected virtual void OnSpied_6_ColoniesChanged()
        {
            Spied_6_ColoniesChanged.Raise(this);
            OnPropertyChanged("Spied_6_Colonies");
        }
        protected virtual void OnSpied_0_TotalPopulationChanged()
        {
            Spied_0_TotalPopulationChanged.Raise(this);
            OnPropertyChanged("Spied_0_TotalPopulation");
        }
        protected virtual void OnSpied_1_TotalPopulationChanged()
        {
            Spied_1_TotalPopulationChanged.Raise(this);
            OnPropertyChanged("Spied_1_TotalPopulation");
        }
        protected virtual void OnSpied_2_TotalPopulationChanged()
        {
            Spied_2_TotalPopulationChanged.Raise(this);
            OnPropertyChanged("Spied_2_TotalPopulation");
        }
        protected virtual void OnSpied_3_TotalPopulationChanged()
        {
            Spied_3_TotalPopulationChanged.Raise(this);
            OnPropertyChanged("Spied_3_TotalPopulation");
        }
        protected virtual void OnSpied_4_TotalPopulationChanged()
        {
            Spied_4_TotalPopulationChanged.Raise(this);
            OnPropertyChanged("Spied_4_TotalPopulation");
        }
        protected virtual void OnSpied_5_TotalPopulationChanged()
        {
            Spied_5_TotalPopulationChanged.Raise(this);
            OnPropertyChanged("Spied_5_TotalPopulation");
        }
        protected virtual void OnSpied_6_TotalPopulationChanged()
        {
            Spied_6_TotalPopulationChanged.Raise(this);
            OnPropertyChanged("Spied_6_TotalPopulation");
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
        public static Civilization Spied_0_Civ
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_0;
                // GameLog.Client.Test.DebugFormat("##### trying to return SpiedCiv.Civilization = {0}", SpiedCiv.Civilization.Key);
                return SpiedCiv.Civilization;
            }
        }
        public static Colony Spied_0_SeatOfGovernment
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_0;
                Colony SeatOfGovernment = GameContext.Current.CivilizationManagers[SpiedCiv].SeatOfGovernment;
                // GameLog.Client.Test.DebugFormat("##### trying to return SpiedCiv_0 SeatOfGovernment = {0}", SeatOfGovernment);
                return SeatOfGovernment;
            }
        }
        public static Meter Spied_0_TotalPopulation
        {
            get
            {
                CivilizationManager civManager = DesignTimeObjects.SpiedCiv_0;
                try
                {
                    GameLog.Core.Test.DebugFormat("Spied_0_TotalPopulation ={0}", civManager.TotalPopulation);
                    return civManager.TotalPopulation;
                }
                catch (Exception e)
                {
                    Meter zero = new Meter(0, 0, 0);
                    GameLog.Core.Intel.WarnFormat("Problem occured at Spied_0_TotalPopulation:");
                    GameLog.Core.General.Error(e);
                    return zero;
                }
            }
        }



        //## Terran ##
        public static string SpiedTerranName => "Terran Empire";
        public static Civilization Spied_1_Civ
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_1;
                _text = "Step_5402:; trying to return Spied_1_Civ.Civilization =;" + SpiedCiv.Civilization.Key;
                Console.WriteLine(_text);
                //GameLog.Client.Intel.DebugFormat("Step_5432:; trying to return Spied_1_Civ.Civilization = {0}", SpiedCiv.Civilization.Key);
                return SpiedCiv.Civilization;
            }
        }
        public static Colony Spied_1_SeatOfGovernment
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_1;
                Colony SeatOfGovernment = GameContext.Current.CivilizationManagers[SpiedCiv].SeatOfGovernment;
                _text = "Step_5404:; trying to return Spied_1_Civ.Civilization =;" + SpiedCiv.Civilization.Key;
                Console.WriteLine(_text);
                GameLog.Client.Test.DebugFormat("##### trying to return SpiedCiv_1 SeatOfGovernment = {0}", SeatOfGovernment);
                return SeatOfGovernment;
            }
        }
        public static Meter Spied_1_TotalPopulation
        {
            get
            {
                CivilizationManager civManager = DesignTimeObjects.SpiedCiv_1;
                try
                {
                    _text = "Step_5412:; Spied_1_TotalPopulation =;" + civManager.TotalPopulation;
                    Console.WriteLine(_text);
                    GameLog.Core.Intel.DebugFormat(_text);
                    return civManager.TotalPopulation;
                }
                catch (Exception e)
                {
                    Meter zero = new Meter(0, 0, 0);
                    _text = "Step_5414:; Problem occured at Spied_1_TotalPopulation:";
                    Console.WriteLine(_text);

                    GameLog.Core.Intel.WarnFormat(_text);
                    GameLog.Core.General.Error(e);
                    return zero;
                }
            }
        }



        //## Romulan ##
        public static string SpiedRomName => "Romulans";
        public static Civilization Spied_2_Civ
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_2;
                _text = "Step_5422:; trying to return Spied_2_Civ.Civilization =;" + SpiedCiv.Civilization.Key;
                Console.WriteLine(_text);
                //GameLog.Client.Intel.DebugFormat(_text);

                return SpiedCiv.Civilization;
            }
        }
        public static Colony Spied_2_SeatOfGovernment
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_2;
                Colony SeatOfGovernment = GameContext.Current.CivilizationManagers[SpiedCiv].SeatOfGovernment;
                _text = "Step_5432:; trying to return Spied_1_Civ.Civilization =;" + SpiedCiv.Civilization.Key;
                Console.WriteLine(_text);
                GameLog.Client.Test.DebugFormat(_text);
                return SeatOfGovernment;
            }
        }
        public Meter Spied_2_TotalPopulation
        {
            get
            {
                CivilizationManager civManager = DesignTimeObjects.SpiedCiv_2;
                try
                {
                    _text = "Step_5422:; trying to return Spied_2_Civ.Civilization =;" + civManager.Civilization.Key;
                    Console.WriteLine(_text);
                    //GameLog.Core.Intel.DebugFormat(_text);
                    return civManager.TotalPopulation;
                }
                catch (Exception e)
                {
                    Meter zero = new Meter(0, 0, 0);
                    _text = "Step_5424:; trying to return Spied_2_Civ.Civilization =;" + civManager.Civilization.Key;
                    Console.WriteLine(_text);
                    GameLog.Core.Intel.WarnFormat(_text);
                    GameLog.Core.General.Error(e);
                    return zero;
                }
            }
        }

        // ## Klingons ##
        public static string SpiedKlingName => "Klingons";
        public static Civilization Spied_3_Civ
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_3;
                _text = "Step_5432:; trying to return Spied_3_Civ.Civilization =;" + SpiedCiv.Civilization.Key;
                Console.WriteLine(_text);
                //GameLog.Client.Intel.DebugFormat(_text);

                return SpiedCiv.Civilization;
            }
        }
        public static Colony Spied_3_SeatOfGovernment
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_3;
                Colony SeatOfGovernment = GameContext.Current.CivilizationManagers[SpiedCiv].SeatOfGovernment;
                _text = "Step_5436:; trying to return SpiedCiv_3 SeatOfGovernment =;" + SeatOfGovernment;
                Console.WriteLine(_text);
                GameLog.Client.Intel.DebugFormat(_text);
                return SeatOfGovernment;
            }
        }
        public Meter Spied_3_TotalPopulation
        {
            get
            {
                CivilizationManager civManager = DesignTimeObjects.SpiedCiv_3;
                try
                {
                    _text = "Step_5432:; trying to return Spied_3_Civ.Civilization =;" + civManager.Civilization.Key;
                    Console.WriteLine(_text);
                    //GameLog.Core.Intel.DebugFormat(_text);
                    return civManager.TotalPopulation;
                }
                catch (Exception e)
                {
                    Meter zero = new Meter(0, 0, 0);
                    _text = "Step_5432:; trying to return Spied_3_Civ.Civilization =;" + civManager.Civilization.Key;
                    Console.WriteLine(_text);
                    GameLog.Core.Intel.WarnFormat("Problem occured at Spied_3_TotalPopulation:");
                    GameLog.Core.General.Error(e);
                    return zero;
                }
            }
        }

        //## Cardassians ##
        public static string SpiedCardName => "Cardassians";
        public static Civilization Spied_4_Civ
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_4;
                _text = "Step_5442:; trying to return Spied_4_Civ.Civilization =;" + SpiedCiv.Civilization.Key;
                Console.WriteLine(_text);
                //GameLog.Client.Intel.DebugFormat(_text);

                return SpiedCiv.Civilization;
            }
        }
        public static Colony Spied_4_SeatOfGovernment
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
        public Meter Spied_4_TotalPopulation
        {
            get
            {
                CivilizationManager civManager = DesignTimeObjects.SpiedCiv_4;
                try
                {
                    _text = "Step_5446:; trying to return Spied_4_Civ.Civilization =;" + civManager.Civilization.Key;
                    Console.WriteLine(_text);
                    GameLog.Core.Intel.DebugFormat(_text);
                    return civManager.TotalPopulation;
                }
                catch (Exception e)
                {
                    Meter zero = new Meter(0, 0, 0);
                    _text = "Step_5432:; trying to return Spied_4_Civ.Civilization =;" + civManager.Civilization.Key;
                    Console.WriteLine(_text);
                    GameLog.Core.Intel.WarnFormat("Problem occured at Spied_4_TotalPopulation:");
                    GameLog.Core.General.Error(e);
                    return zero;
                }
            }
        }

        //## Dominion ##
        public static string SpiedDomName => "Dominion";
        public static Civilization Spied_5_Civ
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_5;
                _text = "Step_5452:; trying to return Spied_5_Civ.Civilization =;" + SpiedCiv.Civilization.Key;
                Console.WriteLine(_text);
                //GameLog.Client.Intel.DebugFormat(_text);

                return SpiedCiv.Civilization;
            }
        }
        public static Colony Spied_5_SeatOfGovernment
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_5;
                Colony SeatOfGovernment = GameContext.Current.CivilizationManagers[SpiedCiv].SeatOfGovernment;
                _text = "Step_5454:; trying to return Spied_5_Civ.Civilization =;" + SpiedCiv.Civilization.Key;
                Console.WriteLine(_text);
                //GameLog.Client.Intel.DebugFormat(_text);
                return SeatOfGovernment;
            }
        }
        public Meter Spied_5_TotalPopulation
        {
            get
            {
                CivilizationManager civManager = DesignTimeObjects.SpiedCiv_5;
                try
                {
                    _text = "Step_5456:; trying to return Spied_5_Civ.Civilization =;" + civManager.Civilization.Key;
                    Console.WriteLine(_text);
                    //GameLog.Core.Intel.DebugFormat(_text);
                    return civManager.TotalPopulation;
                }
                catch (Exception e)
                {
                    Meter zero = new Meter(0, 0, 0);
                    _text = "Step_5458; trying to return Spied_5_Civ.Civilization =;" + civManager.Civilization.Key;
                    Console.WriteLine(_text);
                    GameLog.Core.Intel.WarnFormat("Problem occured at Spied_5_TotalPopulation:");
                    GameLog.Core.General.Error(e);
                    return zero;
                }
            }
        }

        // ## Borg ##
        public static string SpiedBorgName => "Borg";
        public static Civilization Spied_6_Civ
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_6;
                _text = "Step_5462:; trying to return Spied_6_Civ.Civilization =;" + SpiedCiv.Civilization.Key;
                Console.WriteLine(_text);
                //GameLog.Client.Intel.DebugFormat(_text);

                return SpiedCiv.Civilization;
            }
        }
        public static Colony Spied_6_SeatOfGovernment
        {
            get
            {
                CivilizationManager SpiedCiv = DesignTimeObjects.SpiedCiv_6;
                Colony SeatOfGovernment = GameContext.Current.CivilizationManagers[SpiedCiv].SeatOfGovernment;
                _text = "Step_5464:; trying to return Spied_6_Civ.Civilization =;" + SpiedCiv.Civilization.Key;
                Console.WriteLine(_text);
                //GameLog.Client.Intel.DebugFormat(_text);
                return SeatOfGovernment;
            }
        }
        public Meter Spied_6_TotalPopulation
        {
            get
            {
                CivilizationManager civManager = DesignTimeObjects.SpiedCiv_6;
                try
                {
                    _text = "Step_5466:; trying to return Spied_6_Civ.Civilization =;" + civManager.Civilization.Key;
                    Console.WriteLine(_text);
                    //GameLog.Core.Intel.DebugFormat("Spied_6_TotalPopulation ={0}", civManager.TotalPopulation);
                    return civManager.TotalPopulation;
                }
                catch (Exception e)
                {
                    Meter zero = new Meter(0, 0, 0);
                    _text = "Step_5468:; trying to return Spied_1_Civ.Civilization =;" + civManager.Civilization.Key;
                    Console.WriteLine(_text);
                    GameLog.Core.Intel.WarnFormat("Problem occured at Spied_6_TotalPopulation:");
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

                    //_text = "Step_5472:; trying to return civManager.Credits =;" + civManager.Credits + " for " + civManager.Civilization.Key;
                    //Console.WriteLine(_text);
                    //GameLog.Client.Intel.DebugFormat("Step_5432:; trying to return Spied_1_Civ.Civilization = {0}", SpiedCiv.Civilization.Key);

                    return civManager.Credits;
                }
                catch (Exception e)
                {
                    _text = "Step_5476: Problem occured at CreditsEmpire: " + newline + e.Message + newline + e.StackTrace;
                    Console.WriteLine(_text);
                    GameLog.Core.Intel.WarnFormat(_text);
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