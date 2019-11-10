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

namespace Supremacy.Client.Views
{
    public class AssetsScreenPresentationModel : PresentationModelBase, INotifyPropertyChanged
    {
        [InjectionConstructor]
        public AssetsScreenPresentationModel([NotNull] IAppContext appContext)
            : base(appContext) { }

        public AssetsScreenPresentationModel()
            : base(DesignTimeAppContext.Instance)
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                throw new InvalidOperationException("This constructor should only be invoked at design time.");
           
            _colonies = DesignTimeAppContext.Instance.LocalPlayerEmpire.Colonies;
            _spiedOneColonies = DesignTimeAppContext.Instance.SpiedOneEmpire.Colonies;
            _spiedTwoColonies = DesignTimeAppContext.Instance.SpiedTwoEmpire.Colonies;
            _spiedThreeColonies = DesignTimeAppContext.Instance.SpiedThreeEmpire.Colonies;
            _spiedFourColonies = DesignTimeAppContext.Instance.SpiedFourEmpire.Colonies;
            _spiedFiveColonies = DesignTimeAppContext.Instance.SpiedFiveEmpire.Colonies;
            _spiedSixColonies = DesignTimeAppContext.Instance.SpiedSixEmpire.Colonies;
           // _infiltratedColonies = DesignTimeAppContext.Instance.LocalPlayerEmpire.InfiltratedColonies;
        }

        #region Colonies Property

        [field: NonSerialized]
        public event EventHandler ColoniesChanged;

        public event EventHandler SpiedOneColoniesChanged;

        public event EventHandler SpiedTwoColoniesChanged;

        public event EventHandler SpiedThreeColoniesChanged;

        public event EventHandler SpiedFourColoniesChanged;

        public event EventHandler SpiedFiveColoniesChanged;

        public event EventHandler SpiedSixColoniesChanged;

        public event EventHandler TotalPopulationChanged;

        public event EventHandler SpiedOneTotalPopulationChanged;

        public event EventHandler SpiedTwoTotalPopulationChanged;

        public event EventHandler SpiedThreeTotalPopulationChanged;

        public event EventHandler SpiedFourTotalPopulationChanged;

        public event EventHandler SpiedFiveTotalPopulationChanged;

        public event EventHandler SpiedSixTotalPopulationChanged;

        private IEnumerable<Colony> _colonies;

        private IEnumerable<Colony> _spiedOneColonies;

        private IEnumerable<Colony> _spiedTwoColonies;

        private IEnumerable<Colony> _spiedThreeColonies;

        private IEnumerable<Colony> _spiedFourColonies;

        private IEnumerable<Colony> _spiedFiveColonies;

        private IEnumerable<Colony> _spiedSixColonies;

        //private IPlayer _localPlayer;

        private IEnumerable<Colony> _infiltratedColonies;

        public IEnumerable<Colony> Colonies
        {
            get { return _colonies; }
            set
            {
                if (Equals(value, _colonies))
                    return;

                _colonies = value;

                OnColoniesChanged();

                OnTotalPopulationChanged();
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
 
        public IEnumerable<Colony> InfiltratedColonies
        {
            get { return _infiltratedColonies; }
            set
            {
                if (Equals(value, _infiltratedColonies))
                    return;

                _infiltratedColonies = value;

                OnColoniesChanged();

                OnTotalPopulationChanged();
            }
        }

        protected virtual void OnColoniesChanged()
        {
            ColoniesChanged.Raise(this);
            OnPropertyChanged("Colonies");    
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
        #endregion Colonies Property

        protected virtual void OnTotalPopulationChanged()
        {
            TotalPopulationChanged.Raise(this);
            OnPropertyChanged("TotalPopulation");
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
        #region TotalPopulations and Empire Names

        public Meter TotalPopulation
        {
            get
            {
                var civManager = GameContext.Current.CivilizationManagers[AppContext.LocalPlayerEmpire.Civilization];
                try  
                {
                    GameLog.Core.Intel.DebugFormat("TotalPopulation ={0}", civManager.TotalPopulation);
                    return civManager.TotalPopulation;
                }
                catch (Exception e)
                {
                    GameLog.Core.Stations.WarnFormat("Problem occured at TotalPopulation:");
                    GameLog.Core.General.Error(e);
                    return civManager.TotalPopulation;
                }
            }
        }
        public string LocalCivName
        {
            get
            {
                return GameContext.Current.CivilizationManagers[AppContext.LocalPlayerEmpire.Civilization].Civilization.Name;        
            }
        }
        public static Civilization Local
        {
            get
            {
                var localCiv = GameContext.Current.CivilizationManagers[DesignTimeObjects.GetCivLocalPlayer()];
                return localCiv.Civilization;
            }
        }

        public static Meter SpiedOneTotalPopulation
        {
            get
            {
                var civManager = DesignTimeObjects.GetSpiedCivilizationOne();
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
                    sp1Name = DesignTimeObjects.GetSpiedCivilizationOne().Civilization.Name;
                }
                catch
                {
                    //
                }
                return sp1Name;
            }
        }

        public static Civilization SpiedOneCiv
        {
            get
            { var SpiedCiv = DesignTimeObjects.GetSpiedCivilizationOne();
                return SpiedCiv.Civilization;
            }
        }

        public Meter SpiedTwoTotalPopulation
        {
            get
            {
                var civManager = DesignTimeObjects.GetSpiedCivilizationTwo();
                try
                {
                    GameLog.Core.Intel.DebugFormat("SpiedTwoTotalPopulation ={0}", civManager.TotalPopulation);
                    return civManager.TotalPopulation;
                }
                catch (Exception e)
                {
                    Meter zero = new Meter(0,0,0);
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
                    sp2Name = DesignTimeObjects.GetSpiedCivilizationTwo().Civilization.Name;
                }
                catch
                {
                    //
                }
                return sp2Name;
            }
        }
        public static Civilization SpiedTwoCiv
        {
            get
            {
                var SpiedCiv = DesignTimeObjects.GetSpiedCivilizationTwo();
                return SpiedCiv.Civilization;
            }
        }

        public Meter SpiedThreeTotalPopulation
        {
            get
            {
                var civManager = DesignTimeObjects.GetSpiedCivilizationThree();
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
                    sp3Name = DesignTimeObjects.GetSpiedCivilizationThree().Civilization.Name;
                }
                catch
                {

                }
                return sp3Name; 
            }
        }
        public static Civilization SpiedThreeCiv
        {
            get
            {
                var SpiedCiv = DesignTimeObjects.GetSpiedCivilizationThree();
                return SpiedCiv.Civilization;
            }
        }

        public Meter SpiedFourTotalPopulation
        {
            get
            {
                var civManager = DesignTimeObjects.GetSpiedCivilizationFour();
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
                    sp4Name = DesignTimeObjects.GetSpiedCivilizationFour().Civilization.Name;
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
                var SpiedCiv = DesignTimeObjects.GetSpiedCivilizationFour();
                return SpiedCiv.Civilization;
            }
        }

        public Meter SpiedFiveTotalPopulation
        {
            get
            {
                var civManager = DesignTimeObjects.GetSpiedCivilizationFive();
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
                    sp5Name = DesignTimeObjects.GetSpiedCivilizationFive().Civilization.Name;
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
                var SpiedCiv = DesignTimeObjects.GetSpiedCivilizationFive();
                return SpiedCiv.Civilization;
            }
        }

        public Meter SpiedSixTotalPopulation
        {
            get
            {
                var civManager = DesignTimeObjects.GetSpiedCivilizationSix();
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
                    sp6Name = DesignTimeObjects.GetSpiedCivilizationSix().Civilization.Name;
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
                var SpiedCiv = DesignTimeObjects.GetSpiedCivilizationSix();
                return SpiedCiv.Civilization;
            }
        }
        #endregion

        #region Credits Empire

        public Meter CreditsEmpire
        {
            get
            {
                var civManager = GameContext.Current.CivilizationManagers[AppContext.LocalPlayerEmpire.Civilization];
                return civManager.Credits;
            }
        }
        #endregion Credits Empire

        #region TotalIntelligence Empire

        public int TotalIntelligence
        {
            get
            {
                var civManager = GameContext.Current.CivilizationManagers[AppContext.LocalPlayerEmpire.Civilization];
                return civManager.TotalIntelligence;

            }
        }
        #endregion TotalIntelligence Empire

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
    }
}