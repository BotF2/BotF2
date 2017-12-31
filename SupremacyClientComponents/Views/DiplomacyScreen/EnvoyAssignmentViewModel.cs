// EnvoyAssignmentViewModel.cs
// 
// Copyright (c) 2011 Mike Strobel
// 
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
// 
// All other rights reserved.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Data;
using System.Windows.Input;

using Microsoft.Practices.ServiceLocation;

using Supremacy.Client.Input;
using Supremacy.Diplomacy;
using Supremacy.Game;
using Supremacy.Personnel;
using Supremacy.Utility;
using Supremacy.Client.Context;

namespace Supremacy.Client.Views
{
    public class EnvoyAssignmentViewModel : INotifyPropertyChanged
    {
        private readonly DeferrableObservableCollection<Agent> _agents;
        private readonly ReadOnlyObservableCollection<Agent> _agentsView;
        private readonly DelegateCommand<Agent> _assignAgentCommand;

        public EnvoyAssignmentViewModel()
        {
            _agents = new DeferrableObservableCollection<Agent>();

            _appContext = Designer.IsInDesignMode ? DesignTimeAppContext.Instance : ServiceLocator.Current.GetInstance<IAppContext>();

            if (_appContext == null || !_appContext.IsGameInPlay)
                return;

            _agents.AddRange(_appContext.LocalPlayerEmpire.AgentPool.CurrentAgents);
            _agentsView = new ReadOnlyObservableCollection<Agent>(_agents);

            _assignAgentCommand = new DelegateCommand<Agent>(ExecuteAssignAgentCommand, CanExecuteAssignAgentCommand);

            _agentsCollectionView = CollectionViewSource.GetDefaultView(_agentsView);
            _agentsCollectionView.SortDescriptions.Add(new SortDescription("Status", ListSortDirection.Ascending));
            _agentsCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("Status"));

            SelectFirstAvailableAgent();
        }

        public void Refresh()
        {
            _agents.BeginUpdate();
            _agents.Clear();
            _agents.AddRange(_appContext.LocalPlayerEmpire.AgentPool.CurrentAgents);
            _agents.EndUpdate();

            SelectFirstAvailableAgent();
        }

        private void SelectFirstAvailableAgent()
        {
            SelectedAgent = _agentsCollectionView.Cast<Agent>().FirstOrDefault(o => o.IsAvailableForMission);
        }

        private void ExecuteAssignAgentCommand(Agent agent)
        {
            if (!CanExecuteAssignAgentCommand(agent))
                return;

            if (!PlayerOperations.AssignDiplomaticEnvoy(agent, Diplomat.Get(agent.OwnerID).GetForeignPower(_foreignPower)))
                return;

            ForeignPower.RefreshEnvoys();

            _agentsCollectionView.Refresh();

            SelectFirstAvailableAgent();
            RequeryCommands();
        }

        private bool CanExecuteAssignAgentCommand(Agent agent)
        {
            return agent != null &&
                   ForeignPower != null &&
                   ForeignPower.AssignedEnvoy == null &&
                   agent.IsAvailableForMission;
        }

        public ReadOnlyObservableCollection<Agent> Agents
        {
            get { return _agentsView; }
        }

        public ICommand AssignAgentCommand
        {
            get { return _assignAgentCommand; }
        }

        #region ForeignPower Property

        [field: NonSerialized]
        public event EventHandler ForeignPowerChanged;

        private ForeignPowerViewModel _foreignPower;

        public ForeignPowerViewModel ForeignPower
        {
            get { return _foreignPower; }
            set
            {
                if (Equals(value, _foreignPower))
                    return;

                _foreignPower = value;

                OnForeignPowerChanged();
            }
        }

        protected virtual void OnForeignPowerChanged()
        {
            ForeignPowerChanged.Raise(this);
            OnPropertyChanged("ForeignPower");
            RequeryCommands();
        }

        #endregion

        #region SelectedAgent Property

        [field: NonSerialized]
        public event EventHandler SelectedAgentChanged;

        private Agent _selectedAgent;

        public Agent SelectedAgent
        {
            get { return _selectedAgent; }
            set
            {
                if (Equals(value, _selectedAgent))
                    return;

                _selectedAgent = value;
                _agentsCollectionView.MoveCurrentTo(value);

                OnSelectedAgentChanged();
            }
        }

        protected virtual void OnSelectedAgentChanged()
        {
            SelectedAgentChanged.Raise(this);
            OnPropertyChanged("SelectedAgent");
            RequeryCommands();
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        [NonSerialized] private PropertyChangedEventHandler _propertyChanged;
        private readonly IAppContext _appContext;
        private readonly ICollectionView _agentsCollectionView;

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

        private void RequeryCommands()
        {
            _assignAgentCommand.RaiseCanExecuteChanged();
        }
    }
}