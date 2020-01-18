using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using Microsoft.Practices.Unity;

using Supremacy.Annotations;
using Supremacy.Game;
using Supremacy.Universe;
using Supremacy.Utility;
using Supremacy.Client.Context;
using Supremacy.Types;
using Supremacy.Entities;
using Supremacy.Intelligence;

namespace Supremacy.Client.Views
{
    public class AssetsScreenPresentationModel : PresentationModelBase, INotifyPropertyChanged
    {
        protected int _totalIntelligenceProduction;
        protected int _totalIntelligenceDefenseAccumulated;
        protected int _totalIntelligenceAttackingAccumulated;
        public int TotalIntelligenceProduction
        {
            get
            {
                try
                {
                    //FillUpDefense();
                    return _totalIntelligenceProduction;
                    //var civManager = GameContext.Current.CivilizationManagers[AppContext.LocalPlayerEmpire.Civilization]; 
                    //return civManager.TotalIntelligenceProduction;

                }
                catch
                {
                    GameLog.Core.Intel.WarnFormat("Problem occured at TotalIntelligenceProduction get...");
                    return 0;
                }
            }
            set
            {
                try
                {
                    //FillUpDefense();
                    //NotifyPropertyChanged("TotalIntelligenceProduction");
                    FillUpDefense();
                    _totalIntelligenceProduction = value; // NotifyPropertyChanging("TotalIntelligence);
                    NotifyPropertyChanged("TotalIntelligenceProduction");
                }
                catch
                {
                    GameLog.Core.Intel.WarnFormat("Problem occured at TotalIntelligenceProduction set...");
                    //0 = value;
                }
            }
        }
        #region TotalIntelligence Empire

        public int TotalIntelligenceDefenseAccumulated
        {
            get
            {
                //try
                //{
                FillUpDefense();
                //var civManager = GameContext.Current.CivilizationManagers[AppContext.LocalPlayerEmpire.Civilization];
                //    return civManager.TotalIntelligenceDefenseAccumulated;
                //}
                //catch
                //{
                //    GameLog.Core.Intel.WarnFormat("Problem occured at TotalIntelligenceDefenseAccumulated...");
                //    return ;
                //}
                _totalIntelligenceDefenseAccumulated = IntelHelper.DefenseAccumulatedIntelInt;
                return _totalIntelligenceDefenseAccumulated;// IntelHelper.DefenseAccumulatedIntelInt;
            }
            set
            {
                FillUpDefense();
                _totalIntelligenceDefenseAccumulated = IntelHelper.DefenseAccumulatedIntelInt;
                _totalIntelligenceDefenseAccumulated = value;
                NotifyPropertyChanged("TotalIntelligenceDefenseAccumulated");
            }
        }

        public int TotalIntelligenceAttackingAccumulated
        {
            get
            {
                //try
                //{
                FillUpDefense();
                //var civManager = GameContext.Current.CivilizationManagers[AppContext.LocalPlayerEmpire.Civilization];
                //    return civManager.TotalIntelligenceAttackingAccumulated;
                //}
                //catch
                //{
                //    GameLog.Core.Intel.WarnFormat("Problem occured at TotalIntelligenceAttackingAccumulated...");
                //    return 0;
                //}
                _totalIntelligenceAttackingAccumulated = IntelHelper.AttackAccumulatedIntelInt;
                return _totalIntelligenceAttackingAccumulated;
            }
            set
            {
                FillUpDefense();
                _totalIntelligenceAttackingAccumulated = IntelHelper.AttackAccumulatedIntelInt;
                _totalIntelligenceAttackingAccumulated = value;
                NotifyPropertyChanged("TotalIntelligenceAttackingAccumulated");
            }
        }
        public Meter UpdateAttackingAccumulated(Civilization attackingCiv)
        {
            Meter attackMeter = GameContext.Current.CivilizationManagers[attackingCiv].TotalIntelligenceAttackingAccumulated;
            int newAttackIntelligence = 0;
            Int32.TryParse(attackMeter.CurrentValue.ToString(), out newAttackIntelligence);
            _totalIntelligenceAttackingAccumulated = newAttackIntelligence;
            return attackMeter;
        }
        protected virtual void FillUpDefense()
        {
            var civ = GameContext.Current.CivilizationManagers[DesignTimeObjects.LocalCivManager.Civilization];
            civ.TotalIntelligenceAttackingAccumulated.AdjustCurrent(civ.TotalIntelligenceAttackingAccumulated.CurrentValue * -1); // remove from Attacking
            civ.TotalIntelligenceAttackingAccumulated.UpdateAndReset();
            civ.TotalIntelligenceDefenseAccumulated.AdjustCurrent(civ.TotalIntelligenceDefenseAccumulated.CurrentValue); // add to Defense
            civ.TotalIntelligenceDefenseAccumulated.UpdateAndReset();
            //OnPropertyChanged("TotalIntelligenceAttackingAccumulated");
            //OnPropertyChanged("TotalIntelligenceDefenseAccumulated");
            //OnPropertyChanged("TotalIntelligenceProduction");

        }
        #endregion TotalIntelligence Empire

        [InjectionConstructor]
        public AssetsScreenPresentationModel([NotNull] IAppContext appContext)
            : base(appContext) { }

        public AssetsScreenPresentationModel()
            : base(DesignTimeAppContext.Instance)
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                throw new InvalidOperationException("This constructor should only be invoked at design time.");

            _colonies = DesignTimeObjects.LocalCivManager.Colonies;//DesignTimeAppContext.Instance.LocalPlayerEmpire.Colonies; //not this DesignTimeObjects.LocalCivManager.Colonies; 
            _spiedOneColonies = DesignTimeObjects.SpiedCivOne.Colonies;
            _spiedTwoColonies = DesignTimeObjects.SpiedCivTwo.Colonies;
            _spiedThreeColonies = DesignTimeObjects.SpiedCivThree.Colonies;
            _spiedFourColonies = DesignTimeObjects.SpiedCivFour.Colonies;
            _spiedFiveColonies = DesignTimeObjects.SpiedCivFive.Colonies;
            _spiedSixColonies = DesignTimeObjects.SpiedCivSix.Colonies;
            _totalIntelligenceProduction = GameContext.Current.CivilizationManagers[DesignTimeObjects.LocalCivManager.Civilization].TotalIntelligenceProduction;
            
            OnPropertyChanged("TotalIntelligenceAttackingAccumulated");
            OnPropertyChanged("TotalIntelligenceDefenseAccumulated");
            OnPropertyChanged("TotalIntelligenceProduction");
        }

        #region Colonies Property

        [field: NonSerialized]
        public event EventHandler ColoniesChanged;
        public event EventHandler TotalPopulationChanged;
        public event EventHandler TotalIntelligenceProductionChanged;
        public event EventHandler TotalIntelligenceAttackingAccumulatedChanged;
        public event EventHandler TotalIntelligenceDefenseAccumulatedChanged;

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

        private IEnumerable<Colony> _spiedOneColonies;

        private IEnumerable<Colony> _spiedTwoColonies;

        private IEnumerable<Colony> _spiedThreeColonies;

        private IEnumerable<Colony> _spiedFourColonies;

        private IEnumerable<Colony> _spiedFiveColonies;

        private IEnumerable<Colony> _spiedSixColonies;

        //private IEnumerable<Colony> _infiltratedColonies;

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
                OnTotalIntelligenceProductionChanged();
                OnTotalIntelligenceAttackingAccumulatedChanged();
                OnTotalIntelligenceDefenseAccumulatedChanged();
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
        //public IEnumerable<Colony> InfiltratedColonies
        //{
        //    get { return _infiltratedColonies; }
        //    set
        //    {
        //        if (Equals(value, _infiltratedColonies))
        //            return;

        //        _infiltratedColonies = value;

        //        OnColoniesChanged();
        //        OnTotalPopulationChanged();
        //    }
        //}
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
        protected virtual void OnSpiedOneColoniesChanged()
        {
            SpiedOneColoniesChanged.Raise(this);
            OnPropertyChanged("SpiedOneColonies");
        }
        protected virtual void OnSpiedOneTotalPopulationChanged()
        {
            SpiedOneTotalPopulationChanged.Raise(this);
            OnPropertyChanged("SpiedOneTotalPopulation");
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
        #endregion Colonies Property

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
        #region TotalPopulations and Empire Names
        public Meter TotalPopulation
        {
            get
            {
                var civManager = GameContext.Current.CivilizationManagers[DesignTimeObjects.LocalCivManager.Civilization]; // not this DesignTimeObjects.LocalCivManager.Civilization
                try
                {
                    //GameLog.Core.Intel.DebugFormat("TotalPopulation ={0}", civManager.TotalPopulation);
                    return civManager.TotalPopulation;
                }
                catch (Exception e)
                {
                    GameLog.Core.Stations.WarnFormat("Problem occured at TotalPopulation:");
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
                return GameContext.Current.CivilizationManagers[DesignTimeObjects.LocalCivManager.Civilization].Civilization.Name;  // keep this on AppContext
            }
        }
        public static Civilization Local
        {
            get
            {
                return GameContext.Current.CivilizationManagers[DesignTimeObjects.LocalCivManager].Civilization;
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
        public static string SpiedOneCivName
        {
            get
            {
                string sp1Name = "Empty";
                try
                {
                    sp1Name = DesignTimeObjects.SpiedCivOne.Civilization.Name;
                }
                catch
                {
                    // 
                    GameLog.Client.UI.ErrorFormat("##### Problem getting SpiedOneCivName");
                }
                return sp1Name;
            }
        }
        public static Civilization SpiedOneCiv
        {
            get
            {
                var SpiedCiv = DesignTimeObjects.SpiedCivOne;
                GameLog.Client.Intel.DebugFormat("##### trying to return SpiedCiv.Civilization = {0}", SpiedCiv.Civilization.Key);
                return SpiedCiv.Civilization;
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
        public static string SpiedTwoCivName
        {
            get
            {
                string sp2Name = "Empty";
                try
                {
                    sp2Name = DesignTimeObjects.SpiedCivTwo.Civilization.Name;
                }
                catch
                {
                    //
                    GameLog.Client.UI.ErrorFormat("##### Problem getting SpiedTwoCivName");
                }
                return sp2Name;
            }
        }
        public static Civilization SpiedTwoCiv
        {
            get
            {
                var SpiedCiv = DesignTimeObjects.SpiedCivTwo;
                return SpiedCiv.Civilization;
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

        public static string SpiedThreeCivName
        {
            get
            {
                string sp3Name = "Empty";
                try
                {
                    sp3Name = DesignTimeObjects.SpiedCivThree.Civilization.Name;
                }
                catch
                {
                    GameLog.Client.UI.ErrorFormat("##### Problem getting SpiedOneCivName");
                }
                return sp3Name;
            }
        }
        public static Civilization SpiedThreeCiv
        {
            get
            {
                var SpiedCiv = DesignTimeObjects.SpiedCivThree;
                return SpiedCiv.Civilization;
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
        public static string SpiedFourCivName
        {
            get
            {
                string sp4Name = "Empty";
                try
                {
                    sp4Name = DesignTimeObjects.SpiedCivFour.Civilization.Name;
                }
                catch
                {
                    //
                }
                return sp4Name;
            }
        }
        public static Civilization SpiedFourCiv
        {
            get
            {
                var SpiedCiv = DesignTimeObjects.SpiedCivFour;
                return SpiedCiv.Civilization;
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
        public static string SpiedFiveCivName
        {
            get
            {
                string sp5Name = "Empty";
                try
                {
                    sp5Name = DesignTimeObjects.SpiedCivFive.Civilization.Name;
                }
                catch
                {
                    //
                }
                return sp5Name;
            }
        }
        public static Civilization SpiedFiveCiv
        {
            get
            {
                var SpiedCiv = DesignTimeObjects.SpiedCivFive;
                return SpiedCiv.Civilization;
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
        public static string SpiedSixCivName
        {
            get
            {
                string sp6Name = "Empty";
                try
                {
                    sp6Name = DesignTimeObjects.SpiedCivSix.Civilization.Name;
                }
                catch
                {
                    //
                }
                return sp6Name;
            }
        }
        public static Civilization SpiedSixCiv
        {
            get
            {
                var SpiedCiv = DesignTimeObjects.SpiedCivSix;
                return SpiedCiv.Civilization;
            }
        }
        #endregion

        #region Credits Empire

        public Meter CreditsEmpire
        {
            get
            {
                try
                {
                    var civManager = GameContext.Current.CivilizationManagers[DesignTimeObjects.LocalCivManager.Civilization];
                    return civManager.Credits;
                }
                catch
                {
                    GameLog.Core.Intel.WarnFormat("Problem occured at CreditsEmpire:");
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
    }
}