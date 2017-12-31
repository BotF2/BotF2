using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using Microsoft.Practices.Unity;

using Supremacy.Annotations;
using Supremacy.Game;
using Supremacy.Personnel;
using Supremacy.Universe;
using Supremacy.Utility;
using Supremacy.Client.Context;
using Supremacy.Types;

namespace Supremacy.Client.Views
{
    public class AgentPresenter
    {
        private Agent _agent;

        public AgentPresenter(Agent agent)
        {
            if (agent == null)
                throw new InvalidOperationException("Agent shouldn't be null! BAD BAD BAD!");

            _agent = agent;
        }

        public Agent Agent
        {
            get { return _agent; }
        }
    }

    public class AssetsScreenPresentationModel : PresentationModelBase, INotifyPropertyChanged
    {
        private AgentCollection _agentData;
        private List<AgentPresenter> _agentPresenters;
        [InjectionConstructor]
        public AssetsScreenPresentationModel([NotNull] IAppContext appContext)
            : base(appContext) {}

        public AssetsScreenPresentationModel()
            : base(DesignTimeAppContext.Instance)
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                throw new InvalidOperationException("This constructor should only be invoked at design time.");

            _agentData = DesignTimeAppContext.Instance.LocalPlayerEmpire.AgentPool.CurrentAgents;
            Colonies = DesignTimeAppContext.Instance.LocalPlayerEmpire.Colonies;
        }

        #region Agents Property
        [field: NonSerialized]
        public event EventHandler AgentsChanged;

        public AgentCollection AgentData
        {
            get { return _agentData; }
            set
            {
                if (Equals(value, _agentData))
                    return;

                _agentData = value;

                List<AgentPresenter>  tmpAgentPresenters = new List<AgentPresenter>();
                if (_agentData != null)
                {
                    foreach (Agent agent in _agentData)
                    {
                        tmpAgentPresenters.Add(new AgentPresenter(agent));
                    }
                }
                AgentPresenters = tmpAgentPresenters;
            }
        }

        public List<AgentPresenter> AgentPresenters
        {
            get { return _agentPresenters; }
            set
            {
                if (Equals(value, _agentPresenters))
                    return;

                _agentPresenters = value;

                OnAgentsChanged();
            }
        }

        protected virtual void OnAgentsChanged()
        {
            AgentsChanged.Raise(this);
            OnPropertyChanged("AgentPresenters");
        }

        #endregion

        #region Colonies Property

        [field: NonSerialized]
        public event EventHandler ColoniesChanged;

        public event EventHandler TotalPopulationChanged;

        private IEnumerable<Colony> _colonies;

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

        protected virtual void OnColoniesChanged()
        {
            ColoniesChanged.Raise(this);
            OnPropertyChanged("Colonies");
        }

        #endregion

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
                //try    // maybe slows down the game very much
                //{
                    var civManager = GameContext.Current.CivilizationManagers[AppContext.LocalPlayerEmpire.Civilization];
                    return civManager.TotalPopulation;
                //}
                //catch (Exception e)
                //{
                //    GameLog.Print("Problem occured at TotalPopulation");
                //    return GameContext.Current.CivilizationManagers[AppContext.LocalPlayerEmpire.Civilization].TotalPopulation;
                //}
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
    }
}