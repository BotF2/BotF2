using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using Microsoft.Practices.Unity;

using Supremacy.Annotations;
using Supremacy.Game;
using Supremacy.Universe;
using Supremacy.Orbitals;
using Supremacy.Utility;
using Supremacy.Client.Context;
using Supremacy.Types;

namespace Supremacy.Client.Views
{
    public class AssetsScreenPresentationModel : PresentationModelBase, INotifyPropertyChanged
    {
        [InjectionConstructor]
        public AssetsScreenPresentationModel([NotNull] IAppContext appContext)
            : base(appContext) {}

        public AssetsScreenPresentationModel()
            : base(DesignTimeAppContext.Instance)
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                throw new InvalidOperationException("This constructor should only be invoked at design time.");

            Colonies = DesignTimeAppContext.Instance.LocalPlayerEmpire.Colonies;
            var AllColonies = GameContext.Current.Universe.Find<Colony>(UniverseObjectType.Colony);

            // need a list of colonies infiltrated by local player, add colony to list on being infiltrated.

            //InfiltratedColonies = DesignTimeAppContext.Instance.LocalPalyerEmpire.InfiltratedColonies;
        }

        #region Colonies Property

        [field: NonSerialized]
        public event EventHandler ColoniesChanged;

        public event EventHandler TotalPopulationChanged;

        private IEnumerable<Colony> _colonies;

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

        #endregion Colonies Property

        #region ShipsList Property

        [field: NonSerialized]
        public event EventHandler ShipsListChanged;

        //public event EventHandler TotalPopulationChanged;

        private IEnumerable<Ship> _shipsList;

        //private IEnumerable<Colony> _infiltratedColonies;

        public IEnumerable<Ship> ShipsList
        {
            get { return _shipsList; }
            set
            {
                if (Equals(value, _shipsList))
                    return;

                _shipsList = value;

                OnShipsListChanged();

                //OnTotalPopulationChanged();
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

        protected virtual void OnShipsListChanged()
        {
            ShipsListChanged.Raise(this);
            OnPropertyChanged("ShipsList");
        }

        #endregion ShipsList Property

        protected virtual void OnTotalPopulationChanged()
        {
            TotalPopulationChanged.Raise(this);
            OnPropertyChanged("TotalPopulation");
        }

        #region TotalPopulation Empire

        public Meter TotalPopulation
        {
            get
            {
                try    // maybe slows down the game very much
                {
                    
                    var civManager = GameContext.Current.CivilizationManagers[AppContext.LocalPlayerEmpire.Civilization];
                    GameLog.Core.Stations.DebugFormat("TotalPopulation ={0}", civManager.TotalPopulation);
                    return civManager.TotalPopulation;
                }
                catch (Exception e)
                {
                    GameLog.Core.Stations.WarnFormat("Problem occured at TotalPopulation:");
                    GameLog.Core.General.Error(e);
                    return GameContext.Current.CivilizationManagers[AppContext.LocalPlayerEmpire.Civilization].TotalPopulation;
                }
            }
        }
        #endregion

        #region Credits Empire

        public Meter CreditsEmpire
        {
            get
            {
                //try    // maybe slows down the game very much
                //{
                var civManager = GameContext.Current.CivilizationManagers[AppContext.LocalPlayerEmpire.Civilization];
                return civManager.Credits;

                
                //}
                //catch (Exception e)
                //{
                //    GameLog.Print("Problem occured at TotalPopulation");
                //    return GameContext.Current.CivilizationManagers[AppContext.LocalPlayerEmpire.Civilization].TotalPopulation;
                //}
            }
        }
        #endregion Credits Empire

        #region TotalIntelligence Empire

        public int TotalIntelligence
        {
            get
            {
                //try    // maybe slows down the game very much
                //{
                var civManager = GameContext.Current.CivilizationManagers[AppContext.LocalPlayerEmpire.Civilization];
                return civManager.TotalIntelligence;


                //}
                //catch (Exception e)
                //{
                //    GameLog.Print("Problem occured at TotalPopulation");
                //    return GameContext.Current.CivilizationManagers[AppContext.LocalPlayerEmpire.Civilization].TotalPopulation;
                //}
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