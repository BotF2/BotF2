// File:SystemAssaultScreenViewModel
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows.Data;
using System.Windows.Input;

using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.Composite.Regions;

using Supremacy.Annotations;
using Supremacy.Buildings;
using Supremacy.Client.Commands;
using Supremacy.Client.Dialogs;
using Supremacy.Client.Events;
using Supremacy.Client.Input;
using Supremacy.Combat;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Resources;
using Supremacy.Tech;
using Supremacy.Types;
using Supremacy.Universe;
using Supremacy.Utility;

using System.Linq;

using Supremacy.Collections;
using Supremacy.VFS;
using Supremacy.Xaml;
using Supremacy.Client.Audio;

using NavigationCommands = Supremacy.Client.Commands.NavigationCommands;
using Scheduler = System.Concurrency.Scheduler;
using Microsoft.Practices.ServiceLocation;
using Supremacy.Client.Context;

namespace Supremacy.Client.Views
{
    [TypeConverter(typeof(StateConverter<SystemAssaultScreenState>))]
    public sealed class SystemAssaultScreenState : State
    {
        public static readonly SystemAssaultScreenState AwaitingPlayerOrders = new SystemAssaultScreenState(0);
        public static readonly SystemAssaultScreenState WaitingForUpdate = new SystemAssaultScreenState(1);
        public static readonly SystemAssaultScreenState ReplayingResults = new SystemAssaultScreenState(2);
        public static readonly SystemAssaultScreenState Finished = new SystemAssaultScreenState(3);

        private SystemAssaultScreenState(int value)
            : base(value) { }
    }

    public class SystemAssaultScreenViewModel : ViewModelBase<ISystemAssaultScreenView, SystemAssaultScreenViewModel>
    {
        private static SystemAssaultScreenViewModel _designInstance;

        public static SystemAssaultScreenViewModel DesignInstance
        {
            get
            {
                if (_designInstance == null)
                    _designInstance = new SystemAssaultScreenViewModel(DesignTimeAppContext.Instance, null,
                        ServiceLocator.Current.GetInstance<ISoundPlayer>());
                return _designInstance;
            }
        }
        //internal override void RaisePropertyChangedEvent(string v)
        //{
        //    // do nothing here
        //}
        private readonly StateManager<SystemAssaultScreenState> _stateManager;

        private readonly DelegateCommand<InvasionUnit> _standbyOrderCommand;
        private readonly DelegateCommand<InvasionUnit> _attackOrderCommand;
        private readonly DelegateCommand<InvasionUnit> _landTroopsOrderCommand;
        private readonly DelegateCommand<ICheckableCommandParameter> _setActionCommand;
        private readonly DelegateCommand<ICheckableCommandParameter> _setTargetingStrategyCommand;
        private readonly DelegateCommand<object> _commitOrdersCommand;
        private readonly DelegateCommand<object> _doneCommand;

        private readonly Meter _defenderPopulation;
        private readonly Meter _colonyShieldStrength;
        private readonly Meter _defenderCombatStrength;

        private readonly ObservableCollection<AssaultUnitViewModel> _invadingUnits;
        private readonly ObservableCollection<AssaultUnitViewModel> _troopTransports;
        private readonly ObservableCollection<AssaultUnitViewModel> _destroyedInvadingUnits;
        private readonly ObservableCollection<AssaultUnitViewModel> _defendingUnits;
        private readonly ObservableCollection<AssaultUnitViewModel> _destroyedDefendingUnits;

        private ISoundPlayer _soundPlayer = null;
        private InvasionArena _currentUpdate;

        public SystemAssaultScreenViewModel([NotNull] IAppContext appContext, IRegionManager regionManager, [NotNull] ISoundPlayer soundPlayer)
            : base(appContext, regionManager)
        {
            _standbyOrderCommand = new DelegateCommand<InvasionUnit>(ExecuteStandbyOrderCommand, CanExecuteStandbyOrderCommand);
            _attackOrderCommand = new DelegateCommand<InvasionUnit>(ExecuteAttackOrderCommand, CanExecuteAttackOrderCommand);
            _landTroopsOrderCommand = new DelegateCommand<InvasionUnit>(ExecuteLandTroopsOrderCommand, CanExecuteLandTroopsOrderCommand);
            _commitOrdersCommand = new DelegateCommand<object>(ExecuteCommitOrdersCommand, CanExecuteCommitOrdersCommand);
            _setActionCommand = new DelegateCommand<ICheckableCommandParameter>(ExecuteSetActionCommand, CanExecuteSetActionCommand);
            _setTargetingStrategyCommand = new DelegateCommand<ICheckableCommandParameter>(ExecuteSetTargetingStrategyCommand, CanExecuteSetTargetingStrategyCommand);
            _doneCommand = new DelegateCommand<object>(ExecuteDoneCommand, CanExecuteDoneCommand);

            _defenderPopulation = new Meter(0, 0, 0);
            _defenderCombatStrength = new Meter(0, 0, 0);
            _colonyShieldStrength = new Meter(0, 0, 0);

            _defenderCombatStrength.CurrentValueChanged += (o, eventArgs) => OnSelectedTransportsNetCombatStrengthChanged();

            _invadingUnits = new ObservableCollection<AssaultUnitViewModel>();
            _destroyedInvadingUnits = new ObservableCollection<AssaultUnitViewModel>();
            _defendingUnits = new ObservableCollection<AssaultUnitViewModel>();
            _destroyedDefendingUnits = new ObservableCollection<AssaultUnitViewModel>();
            _troopTransports = new ObservableCollection<AssaultUnitViewModel>();

            CollectionViewSource.GetDefaultView(_invadingUnits).GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            CollectionViewSource.GetDefaultView(_destroyedInvadingUnits).GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            CollectionViewSource.GetDefaultView(_defendingUnits).GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            CollectionViewSource.GetDefaultView(_destroyedDefendingUnits).GroupDescriptions.Add(new PropertyGroupDescription("Category"));

            _stateManager = new StateManager<SystemAssaultScreenState>(
                SystemAssaultScreenState.Finished,
                new StateTransition<SystemAssaultScreenState>(
                    SystemAssaultScreenState.AwaitingPlayerOrders,
                    SystemAssaultScreenState.WaitingForUpdate,
                    StateChangeDisposition.MustHappen),
                new StateTransition<SystemAssaultScreenState>(
                    SystemAssaultScreenState.WaitingForUpdate,
                    SystemAssaultScreenState.ReplayingResults,
                    StateChangeDisposition.MustHappen),
                new StateTransition<SystemAssaultScreenState>(
                    SystemAssaultScreenState.ReplayingResults,
                    SystemAssaultScreenState.AwaitingPlayerOrders,
                    StateChangeDisposition.MustHappen),
                new StateTransition<SystemAssaultScreenState>(
                    SystemAssaultScreenState.ReplayingResults,
                    SystemAssaultScreenState.Finished,
                    StateChangeDisposition.MustHappen),
                new StateTransition<SystemAssaultScreenState>(
                    SystemAssaultScreenState.Finished,
                    SystemAssaultScreenState.AwaitingPlayerOrders,
                    StateChangeDisposition.MustHappen),
                new StateTransition<SystemAssaultScreenState>(
                    SystemAssaultScreenState.AwaitingPlayerOrders,
                    SystemAssaultScreenState.Finished,
                    StateChangeDisposition.Optional),
                new StateTransition<SystemAssaultScreenState>(
                    SystemAssaultScreenState.WaitingForUpdate,
                    SystemAssaultScreenState.Finished,
                    StateChangeDisposition.Optional));

            _stateManager.StateChanged += (sender, args) => OnStateChanged();

            if (soundPlayer == null)
                throw new ArgumentNullException("soundPlayer");
            _soundPlayer = soundPlayer;

            RoundNumber = 1;
        }

        public override string ViewName
        {
            get { return StandardGameScreens.SystemAssaultScreen; }
        }

        #region Commands

        public ICommand SetActionCommand
        {
            get { return _setActionCommand; }
        }

        public ICommand SetTargetingStrategyCommand
        {
            get { return _setTargetingStrategyCommand; }
        }

        public ICommand CommitOrdersCommand
        {
            get { return _commitOrdersCommand; }
        }

        public ICommand StandbyOrderCommand
        {
            get { return _standbyOrderCommand; }
        }

        public ICommand AttackOrderCommand
        {
            get { return _attackOrderCommand; }
        }

        public ICommand LandTroopsOrderCommand
        {
            get { return _landTroopsOrderCommand; }
        }

        public ICommand DoneCommand
        {
            get { return _doneCommand; }
        }

        public Meter DefenderPopulation
        {
            get { return _defenderPopulation; }
        }

        public Meter DefenderCombatStrength
        {
            get { return _defenderCombatStrength; }
        }

        public Meter ColonyShieldStrength
        {
            get { return _colonyShieldStrength; }
        }

        #endregion

        public IEnumerable<AssaultUnitViewModel> InvadingUnits
        {
            get { return _invadingUnits; }
        }

        public IEnumerable<AssaultUnitViewModel> TroopTransports
        {
            get { return _troopTransports; }
        }

        public IEnumerable<AssaultUnitViewModel> DefendingUnits
        {
            get { return _defendingUnits; }
        }

        public IEnumerable<AssaultUnitViewModel> DestroyedInvadingUnits
        {
            get { return _destroyedInvadingUnits; }
        }

        public IEnumerable<AssaultUnitViewModel> DestroyedDefendingUnits
        {
            get { return _destroyedDefendingUnits; }
        }

        #region Invader Property

        [field: NonSerialized]
        public event EventHandler InvaderChanged;

        private Civilization _invader;

        public Civilization Invader
        {
            get { return _invader; }
            set
            {
                if (Equals(value, _invader))
                    return;

                _invader = value;

                OnInvaderChanged();
            }
        }

        protected virtual void OnInvaderChanged()
        {
            InvaderChanged.Raise(this);
            OnPropertyChanged("Invader");
        }

        #endregion

        #region Defender Property

        [field: NonSerialized]
        public event EventHandler DefenderChanged;

        private Civilization _defender;

        public Civilization Defender
        {
            get { return _defender; }
            set
            {
                if (Equals(value, _defender))
                    return;

                _defender = value;

                OnDefenderChanged();
            }
        }

        protected virtual void OnDefenderChanged()
        {
            DefenderChanged.Raise(this);
            OnPropertyChanged("Defender");
        }

        #endregion

        #region InvasionStatus Property

        [field: NonSerialized]
        public event EventHandler InvasionStatusChanged;

        public InvasionStatus InvasionStatus
        {
            get { return _currentUpdate == null ? InvasionStatus.InProgress : _currentUpdate.Status; }
        }

        protected virtual void OnInvasionStatusChanged()
        {
            InvasionStatusChanged.Raise(this);
            OnPropertyChanged("InvasionStatus");
        }

        #endregion

        #region StarSystem Property

        [field: NonSerialized]
        public event EventHandler StarSystemChanged;

        private StarSystem _starSystem;

        public StarSystem StarSystem
        {
            get { return _starSystem; }
            set
            {
                if (Equals(value, _starSystem))
                    return;

                _starSystem = value;

                OnStarSystemChanged();
            }
        }

        protected virtual void OnStarSystemChanged()
        {
            StarSystemChanged.Raise(this);
            OnPropertyChanged("StarSystem");
        }

        #endregion

        #region RoundNumber Property

        [field: NonSerialized]
        public event EventHandler RoundNumberChanged;

        private int _roundNumber;

        public int RoundNumber
        {
            get { return _roundNumber; }
            private set
            {
                if (Equals(value, _roundNumber))
                    return;

                _roundNumber = value;

                OnRoundNumberChanged();
            }
        }

        protected virtual void OnRoundNumberChanged()
        {
            RoundNumberChanged.Raise(this);
            OnPropertyChanged("RoundNumber");
        }

        #endregion

        #region SelectedAction Property

        [field: NonSerialized]
        public event EventHandler SelectedActionChanged;

        private InvasionAction? _selectedAction;

        public InvasionAction? SelectedAction
        {
            get { return _selectedAction; }
            set
            {
                //if (Equals(value, _selectedAction))
                //    return;

                _selectedAction = value;

                OnSelectedActionChanged();
            }
        }

        protected virtual void OnSelectedActionChanged()
        {
            SelectedActionChanged.Raise(this);
            OnPropertyChanged("SelectedAction");
        }

        #endregion

        #region SelectedTargetingStrategy Property

        [field: NonSerialized]
        public event EventHandler SelectedTargetingStrategyChanged;

        private InvasionTargetingStrategy _selectedTargetingStrategy = InvasionTargetingStrategy.Balanced;

        public InvasionTargetingStrategy SelectedTargetingStrategy
        {
            get { return _selectedTargetingStrategy; }
            set
            {
                if (Equals(value, _selectedTargetingStrategy))
                    return;

                if (EnumHelper.IsDefined(value))
                    _selectedTargetingStrategy = value;
                else
                    _selectedTargetingStrategy = InvasionTargetingStrategy.Balanced;

                OnSelectedTargetingStrategyChanged();
            }
        }

        protected virtual void OnSelectedTargetingStrategyChanged()
        {
            SelectedTargetingStrategyChanged.Raise(this);
            OnPropertyChanged("SelectedTargetingStrategy");
        }

        #endregion

        #region State Property

        [field: NonSerialized]
        public event EventHandler StateChanged;

        public SystemAssaultScreenState State
        {
            get { return _stateManager.CurrentState; }
        }

        protected virtual void OnStateChanged()
        {
            StateChanged.Raise(this);
            OnPropertyChanged("State");
            OnExplosionIntervalChanged();
            InvalidateCommands();
        }

        #endregion

        #region ExplosionInterval Property

        [field: NonSerialized]
        public event EventHandler ExplosionIntervalChanged;

        private TimeSpan? _explosionInterval;

        public TimeSpan? ExplosionInterval
        {
            get { return _explosionInterval; }
            set
            {
                if (Equals(value, _explosionInterval))
                    return;

                _explosionInterval = value;

                OnExplosionIntervalChanged();
            }
        }

        protected virtual void OnExplosionIntervalChanged()
        {
            ExplosionIntervalChanged.Raise(this);
            OnPropertyChanged("ExplosionInterval");
        }

        #endregion

        #region PrimaryPlanet Property

        [field: NonSerialized]
        public event EventHandler PrimaryPlanetChanged;

        private Planet _primaryPlanet;

        public Planet PrimaryPlanet
        {
            get { return _primaryPlanet; }
            set
            {
                if (Equals(value, _primaryPlanet))
                    return;

                _primaryPlanet = value;

                OnPrimaryPlanetChanged();
            }
        }

        protected virtual void OnPrimaryPlanetChanged()
        {
            PrimaryPlanetChanged.Raise(this);
            OnPropertyChanged("PrimaryPlanet");
        }

        #endregion

        #region SelectedTransportsCombatStrength Property

        [field: NonSerialized]
        public event EventHandler SelectedTransportsCombatStrengthChanged;

        private int _selectedTransportsCombatStrength;

        public int SelectedTransportsCombatStrength
        {
            get { return _selectedTransportsCombatStrength; }
            private set
            {
                if (Equals(value, _selectedTransportsCombatStrength))
                    return;

                _selectedTransportsCombatStrength = value;

                OnSelectedTransportsCombatStrengthChanged();
                OnGroundCombatOddsChanged();
            }
        }

        protected virtual void OnSelectedTransportsCombatStrengthChanged()
        {
            SelectedTransportsCombatStrengthChanged.Raise(this);
            OnPropertyChanged("SelectedTransportsCombatStrength");
            OnSelectedTransportsNetCombatStrengthChanged();
        }

        protected void UpdateSelectedTransportsCombatStrength()
        {
            SelectedTransportsCombatStrength = TroopTransports
                .Where(o => o.IsSelected && !o.IsDestroyed)
                .Select(o => o.Unit.Source)
                .OfType<Ship>()
                .Select(o => o.ShipDesign.WorkCapacity)
                .Sum(pop => CombatHelper.ComputeGroundCombatStrength(Invader, StarSystem.Location, pop));
        }

        #endregion

        #region SelectedTransportsNetCombatStrength Property

        [field: NonSerialized]
        public event EventHandler SelectedTransportsNetCombatStrengthChanged;



        public int SelectedTransportsNetCombatStrength
        {
            get { return SelectedTransportsCombatStrength - DefenderCombatStrength.CurrentValue; }
        }

        protected virtual void OnSelectedTransportsNetCombatStrengthChanged()
        {
            SelectedTransportsNetCombatStrengthChanged.Raise(this);
            GroundCombatOddsChanged.Raise(this);
            OnPropertyChanged("SelectedTransportsNetCombatStrength");
        }

        #endregion


        #region GroundCombatOdds Property

        [field: NonSerialized]
        public event EventHandler GroundCombatOddsChanged;



        public int GroundCombatOdds
        {
            get
            {
                int GroundCombatOddsValue = 100; 
                try
                {
                    int attack = SelectedTransportsCombatStrength ;   // minus ? - 5
                    int defend = _defenderCombatStrength.CurrentValue ;  // plus + 5

                    GroundCombatOddsValue = 100 + attack - defend;

                    GameLog.Client.General.DebugFormat("SelectedTransportsCombatStrength = {0}, _defenderCombatStrength.CurrentValue = {1}, GroundCombatOdds = {2}",
                        SelectedTransportsCombatStrength, _defenderCombatStrength.CurrentValue, GroundCombatOddsValue);

                    // 
                }
                catch (Exception e)
                {
                    GameLog.Client.SystemAssault.DebugFormat("Exception {0} {1}", e.Message, e.StackTrace);
                }

                //GroundCombatOddsValue = GroundCombatOddsValue / 100;

                if (GroundCombatOddsValue > 100)
                    GroundCombatOddsValue = 100;

                if (GroundCombatOddsValue < 0)
                    GroundCombatOddsValue = 0;

                //GroundCombatOddsValue = 99;
                return GroundCombatOddsValue;
            }
        }

        protected virtual void OnGroundCombatOddsChanged()
        {
            GroundCombatOddsChanged.Raise(this);
            OnPropertyChanged("SelectedTransportsNetCombatStrength");
            OnPropertyChanged("GroundCombatOdds");
        }

        #endregion

        #region Command Handlers

        private bool CanExecuteSetActionCommand(ICheckableCommandParameter p)
        {
            if (p == null || State != SystemAssaultScreenState.AwaitingPlayerOrders)
                return false;

            var action = p.InnerParameter as InvasionAction?;
            if (action == null)
                return true;

            var canExecute = true;

            switch (action.Value)
            {
                case InvasionAction.AttackOrbitalDefenses:
                    canExecute = _currentUpdate.HasOrbitalDefenses;
                    break;
                case InvasionAction.BombardPlanet:
                case InvasionAction.UnloadAllOrdinance:
                    canExecute = _currentUpdate.HasAttackingUnits;
                    break;
                case InvasionAction.LandTroops:
                    canExecute = _currentUpdate.CanLandTroops;
                    break;
            }

            p.IsChecked = (SelectedAction == action) && canExecute;

            return canExecute;
        }

        private void ExecuteSetActionCommand(ICheckableCommandParameter p)
        {
            if (!CanExecuteSetActionCommand(p))
                return;

            SelectedAction = (InvasionAction?)p.InnerParameter;
            InvalidateCommands();
        }

        private bool CanExecuteSetTargetingStrategyCommand(ICheckableCommandParameter p)
        {
            if (p == null || State != SystemAssaultScreenState.AwaitingPlayerOrders)
                return false;

            var targetingStrategy = p.InnerParameter as InvasionTargetingStrategy?;
            if (targetingStrategy == null)
                return false;

            p.IsChecked = (SelectedTargetingStrategy == targetingStrategy);

            return SelectedAction.HasValue &&
                   SelectedAction == InvasionAction.BombardPlanet;
        }

        private void ExecuteSetTargetingStrategyCommand(ICheckableCommandParameter p)
        {
            if (!CanExecuteSetTargetingStrategyCommand(p))
                return;

            SelectedTargetingStrategy = (InvasionTargetingStrategy)p.InnerParameter;
            InvalidateCommands();
        }

        private bool CanExecuteCommitOrdersCommand(object _)
        {
            if (State != SystemAssaultScreenState.AwaitingPlayerOrders || !SelectedAction.HasValue)
                return false;

            if (SelectedAction == InvasionAction.LandTroops && !TroopTransports.Any(o => o.IsSelected))
                return false;

            return true;
        }

        private bool CanExecuteLandTroopsOrderCommand(InvasionUnit arg)
        {
            return false;
        }

        private bool CanExecuteAttackOrderCommand(InvasionUnit arg)
        {
            return false;
        }

        private bool CanExecuteStandbyOrderCommand(InvasionUnit arg)
        {
            return false;
        }

        private void ExecuteCommitOrdersCommand(object _)
        {
            if (!CanExecuteCommitOrdersCommand(null))
                return;

            // ReSharper disable PossibleInvalidOperationException
            ClientCommands.SendInvasionOrders.Execute(
                new InvasionOrders(
                    _currentUpdate.InvasionID,
                    SelectedAction.Value,
                    SelectedTargetingStrategy,
                    TroopTransports.Where(o => o.IsSelected).Select(o => o.Unit)));
            // ReSharper restore PossibleInvalidOperationException

            _stateManager.TryChange(SystemAssaultScreenState.WaitingForUpdate);
        }

        private void ExecuteLandTroopsOrderCommand(InvasionUnit unit) { }

        private void ExecuteAttackOrderCommand(InvasionUnit unit) { }

        private void ExecuteStandbyOrderCommand(InvasionUnit unit) { }

        private void ExecuteDoneCommand(object _)
        {
            ClientCommands.EndInvasion.Execute(null);
            NavigationCommands.ActivateScreen.Execute(StandardGameScreens.GalaxyScreen);
            Terminate();
        }

        private bool CanExecuteDoneCommand(object _)
        {
            return State == SystemAssaultScreenState.Finished;
        }

        #endregion

        protected override void RunOverride()
        {
            var dialog = View as IDialog;
            if (dialog != null && !dialog.IsOpen)
                dialog.Show();
        }

        protected override void RegisterCommandAndEventHandlers()
        {
            ClientEvents.InvasionUpdateReceived.Subscribe(OnInvasionUpdateReceived, ThreadOption.UIThread);
        }

        protected override void UnregisterCommandAndEventHandlers()
        {
            ClientEvents.InvasionUpdateReceived.Unsubscribe(OnInvasionUpdateReceived);
        }

        protected override void InvalidateCommands()
        {
            base.InvalidateCommands();

            _standbyOrderCommand.RaiseCanExecuteChanged();
            _attackOrderCommand.RaiseCanExecuteChanged();
            _landTroopsOrderCommand.RaiseCanExecuteChanged();
            _commitOrdersCommand.RaiseCanExecuteChanged();
            _setActionCommand.RaiseCanExecuteChanged();
            _setTargetingStrategyCommand.RaiseCanExecuteChanged();
            _doneCommand.RaiseCanExecuteChanged();
        }

        protected override void TerminateOverride()
        {
            OnClose();

            if (State != SystemAssaultScreenState.Finished)
                _stateManager.TryChange(SystemAssaultScreenState.Finished);
        }

        private void OnClose()
        {
            var dialog = View as IDialog;
            if (dialog != null)
                dialog.Close();

            _currentUpdate = null;
            _invadingUnits.Clear();
            _troopTransports.ForEach(o => o.IsSelectedChanged -= OnTroopTransportIsSelectedChanged);
            _troopTransports.Clear();
            _destroyedInvadingUnits.Clear();
            _defendingUnits.Clear();
            _destroyedDefendingUnits.Clear();

            StarSystem = null;
            PrimaryPlanet = null;

            RoundNumber = 1;
        }

        private void OnTroopTransportIsSelectedChanged(object sender, EventArgs eventArgs)
        {
            UpdateSelectedTransportsCombatStrength();
        }

        private void OnInvasionUpdateReceived(ClientDataEventArgs<InvasionArena> e)
        {
            var newUpdate = e.Value;

            if (_currentUpdate != null &&
                _currentUpdate.InvasionID != newUpdate.InvasionID)
            {
                if (_currentUpdate.IsFinished)
                    _currentUpdate = null;
                else
                    throw new InvalidOperationException("Combat update received while another combat was in progress.");
            }

            ProcessUpdate(newUpdate);

            //ServiceLocator.Current.GetInstance<INavigationService>().ActivateScreen(this.ViewName);
        }

        private void PlaybackResults([NotNull] InvasionArena update)
        {
            var settings = AssaultScreenSettings.Instance;

            _stateManager.TryChange(SystemAssaultScreenState.ReplayingResults);
            
            if (SelectedAction == InvasionAction.UnloadAllOrdinance)
            {
                ExplosionInterval = settings.ExplosionIntervalUnloadAllOrdinance;
            }
            else if (SelectedAction == InvasionAction.BombardPlanet)
            {
                switch (SelectedTargetingStrategy)
                {
                    case InvasionTargetingStrategy.MaximumPrecision:
                        ExplosionInterval = settings.ExplosionIntervalMaxPrecision;
                        break;
                    case InvasionTargetingStrategy.MaximumDamage:
                        ExplosionInterval = settings.ExplosionIntervalMaxDamage;
                        break;
                    default:
                    case InvasionTargetingStrategy.Balanced:
                        ExplosionInterval = settings.ExplosionIntervalBalanced;
                        break;
                }
            }

            string soundEffect = null;
            var playbackDuration = TimeSpan.Zero;

            switch (SelectedAction)
            {
                case InvasionAction.AttackOrbitalDefenses:
                    playbackDuration = settings.AttackOrbitalDefensesReplayDuration;
                    soundEffect= "AttackOrbitalDefenses";
                    break;
                case InvasionAction.BombardPlanet:
                    playbackDuration = settings.BombardmentReplayDuration;
                    soundEffect= "Bombardment_SM";
                    break;
                case InvasionAction.UnloadAllOrdinance:
                    playbackDuration = settings.UnloadAllOrdinanceReplayDuration;
                    soundEffect = "Bombardment_LG";
                    break;
                case InvasionAction.LandTroops:
                    playbackDuration = settings.LandTroopsReplayDuration;
                    soundEffect = "CombatLaser";
                    break;
            }

            if (soundEffect != null)
                _soundPlayer.Play("GroundCombat", soundEffect);

            if (ClientSettings.Current.EnableCombatScreen == false)
            {
                //GameLog.Print("SystemAssault - Quick Running");
                playbackDuration = TimeSpan.FromSeconds(1);
                GameLog.Client.General.DebugFormat("SystemAssault - Quick Running, PlaybackDuration={0}, ClientSettings.Current.EnableCombatScreen={1}", playbackDuration.ToString(), ClientSettings.Current.EnableCombatScreen);
                soundEffect = "CombatAll";
            }

            Observable
                .Timer(playbackDuration, Scheduler.ThreadPool)
                .ObserveOnDispatcher()
                .Subscribe(
                    _ =>
                    {
                        ExplosionInterval = null;
                        ProcessUpdateCallback(false, update);
                    });
        }

        public void ProcessUpdate([NotNull] InvasionArena update)
        {
            if (update == null)
                throw new ArgumentNullException("update");

            var newInvasion = (_currentUpdate == null || _currentUpdate.InvasionID != update.InvasionID);
            if (newInvasion ||
                SelectedAction == InvasionAction.StandDown)
            {
                ProcessUpdateCallback(newInvasion, update);
                return;
            }

            PlaybackResults(update);
        }

        private void ProcessUpdateCallback(bool newInvasion, InvasionArena update)
        {
            _currentUpdate = update;

            var colony = update.Colony;

            RoundNumber = update.RoundNumber;
            OnInvasionStatusChanged();

            if (newInvasion)
            {
                StarSystem = GameContext.Current.Universe.Map[colony.Location].System;
                GameLog.Client.General.DebugFormat("New Invasion on {0} at {1}", GameContext.Current.Universe.Map[colony.Location].System, GameContext.Current.Universe.Map[colony.Location].Location);

                PrimaryPlanet = StarSystem.Planets
                    .OrderByDescending(p => p.GetGrowthRate(colony.Inhabitants))
                    .ThenByDescending(p => p.GetMaxPopulation(colony.Inhabitants))
                    .FirstOrDefault();

                Invader = update.Invader;
                Defender = StarSystem.Owner;

                _invadingUnits.Clear();
                _troopTransports.ForEach(o => o.IsSelectedChanged -= OnTroopTransportIsSelectedChanged);
                _troopTransports.Clear();
                _destroyedInvadingUnits.Clear();
                _defendingUnits.Clear();
                _destroyedDefendingUnits.Clear();

                _invadingUnits.AddRange(update.InvadingUnits.Select(o => new AssaultUnitViewModel(o)));
                _defendingUnits.AddRange(update.DefendingUnits.Select(o => new AssaultUnitViewModel(o)));

                _invadingUnits
                    .Where(o => o.Category == AssaultUnitCategory.TroopTransport && !o.IsDestroyed)
                    .ForEach(
                        o =>
                        {
                            o.IsSelected = true;
                            o.IsSelectedChanged += OnTroopTransportIsSelectedChanged;
                            _troopTransports.Add(o);
                        });

                UpdateSelectedTransportsCombatStrength();

                _defenderPopulation.SetValues(update.Population);
                _colonyShieldStrength.SetValues(update.ColonyShieldStrength);
                _defenderCombatStrength.Maximum = update.DefenderCombatStrength;
                _defenderCombatStrength.Reset(update.DefenderCombatStrength);
                // GameLog.Print("New Invasion (Round 1) on {0} at {1}, _defenderPopulation={2}, Population={3}", GameContext.Current.Universe.Map[colony.Location].System, GameContext.Current.Universe.Map[colony.Location], 
                //                                            _defenderPopulation, GameContext.Current.Universe.Map[colony.Location].System.Colony.Population);
            }
            else
            {
                //works   GameLog.Print("Proceeding Invasion on {0} at {1}, Round={2}", GameContext.Current.Universe.Map[colony.Location].System, GameContext.Current.Universe.Map[colony.Location], RoundNumber);
                foreach (var invadingUnit in update.InvadingUnits)
                {
                    var model = _invadingUnits.FirstOrDefault(o => Equals(o.Unit, invadingUnit));
                    if (model == null)
                        continue;

                    model.UpdateUnit(invadingUnit);

                    if (invadingUnit.IsDestroyed)
                    {
                        _invadingUnits.Remove(model);
                        _troopTransports.Remove(model);
                        model.IsSelectedChanged -= OnTroopTransportIsSelectedChanged;
                        _destroyedInvadingUnits.Add(model);
                    }
                }

                foreach (var defendingUnit in update.DefendingUnits)
                {
                    var model = _defendingUnits.FirstOrDefault(o => Equals(o.Unit, defendingUnit));
                    if (model == null)
                        continue;

                    model.UpdateUnit(defendingUnit);

                    if (defendingUnit.IsDestroyed)
                    {
                        _defendingUnits.Remove(model);
                        _destroyedDefendingUnits.Add(model);
                    }
                }
            }

            _defenderPopulation.SetValues(update.Population);
            _colonyShieldStrength.SetValues(update.ColonyShieldStrength);
            _defenderCombatStrength.CurrentValue = update.DefenderCombatStrength;



            GameLog.Client.General.DebugFormat("Proceeding Invasion on {0} {1} - Round {4}, Population: Last={2}, _current={3}", GameContext.Current.Universe.Map[colony.Location].System, GameContext.Current.Universe.Map[colony.Location].Location,
                                            GameContext.Current.Universe.Map[colony.Location].System.Colony.Population, _defenderPopulation, RoundNumber);

            if (update.IsFinished)
                SelectedAction = null;

            if (newInvasion)
            {
                _stateManager.TryChange(SystemAssaultScreenState.AwaitingPlayerOrders);
            }
            else
            {
                var nextState = update.IsFinished ? SystemAssaultScreenState.Finished : SystemAssaultScreenState.AwaitingPlayerOrders;
                _stateManager.TryChange(nextState);
            }
        }
    }

    public enum AssaultUnitCategory
    {
        CombatantShip,
        TroopTransport,
        OrbitalBattery,
        ProductionFacility,
        MilitaryStructure,
        CivilianStructure
    }

    public class AssaultUnitViewModel : INotifyPropertyChanged
    {
        private readonly AssaultUnitCategory _category;
        private readonly int _troopCount;
        private InvasionUnit _unit;

        public AssaultUnitViewModel([NotNull] InvasionUnit unit)
        {
            if (unit == null)
                throw new ArgumentNullException("unit");

            UpdateUnit(unit);

            _category = ResolveCategory(_unit);

            if (_category == AssaultUnitCategory.TroopTransport)
                _troopCount = CombatHelper.ComputeGroundCombatStrength(unit.Source.Owner, unit.Source.Location, ((Ship)(unit.Source)).ShipDesign.WorkCapacity);
        }

        public InvasionUnit Unit
        {
            get { return _unit; }
        }

        public int TroopCount
        {
            get { return _troopCount; }
        }

        private static AssaultUnitCategory ResolveCategory(InvasionUnit unit)
        {
            if (unit is InvasionFacility)
                return AssaultUnitCategory.ProductionFacility;

            var structure = unit as InvasionStructure;
            if (structure != null)
            {
                var design = (BuildingDesign)structure.Design;
                var militaryBonuses = design.GetBonuses(
                    BonusType.PercentGroundCombat,
                    BonusType.PercentGroundDefense,
                    BonusType.PlanetaryShielding,
                    BonusType.PlanetaryShielding,
                    BonusType.PercentPlanetaryShielding);

                if (militaryBonuses.Any())
                    return AssaultUnitCategory.MilitaryStructure;

                return AssaultUnitCategory.CivilianStructure;
            }

            if (unit.Source is OrbitalBattery)
                return AssaultUnitCategory.OrbitalBattery;

            var orbital = unit as InvasionOrbital;
            if (orbital != null &&
                ((Ship)orbital.Source).ShipType == ShipType.Transport)
            {
                return AssaultUnitCategory.TroopTransport;
            }

            return AssaultUnitCategory.CombatantShip;
        }

        public void UpdateUnit([NotNull] InvasionUnit unit)
        {
            if (unit == null)
                throw new ArgumentNullException("unit");

            _unit = unit;

            OnHasShieldsChanged();
            OnShieldStrengthChanged();
            OnHitPointsChanged();
            OnIsDestroyedChanged();
        }

        #region ShieldStrength Property

        [field: NonSerialized]
        public event EventHandler ShieldStrengthChanged;

        public Meter ShieldStrength
        {
            get
            {
                var orbital = _unit as InvasionOrbital;
                if (orbital == null)
                    return null;
                return orbital.ShieldStrength;
            }
        }

        protected virtual void OnShieldStrengthChanged()
        {
            ShieldStrengthChanged.Raise(this);
            OnPropertyChanged("ShieldStrength");
        }

        #endregion

        #region HitPoints Property

        [field: NonSerialized]
        public event EventHandler HitPointsChanged;

        public Meter HitPoints
        {
            get
            {
                var orbital = _unit as InvasionOrbital;
                if (orbital == null)
                    return _unit.Health;
                return orbital.HullStrength;
            }
        }

        protected virtual void OnHitPointsChanged()
        {
            HitPointsChanged.Raise(this);
            OnPropertyChanged("HitPoints");
        }

        #endregion

        #region HasShields Property

        [field: NonSerialized]
        public event EventHandler HasShieldsChanged;

        public bool HasShields
        {
            get
            {
                var orbital = _unit as InvasionOrbital;

                return orbital != null &&
                       orbital.Source.ShieldStrength.Maximum > 0;
            }
        }

        protected virtual void OnHasShieldsChanged()
        {
            HasShieldsChanged.Raise(this);
            OnPropertyChanged("HasShields");
        }

        #endregion

        #region IsDestroyed Property

        [field: NonSerialized]
        public event EventHandler IsDestroyedChanged;

        public bool IsDestroyed
        {
            get { return _unit.IsDestroyed; }
        }

        protected virtual void OnIsDestroyedChanged()
        {
            IsDestroyedChanged.Raise(this);
            OnPropertyChanged("IsDestroyed");
        }

        #endregion

        #region IsSelected Property

        [field: NonSerialized]
        public event EventHandler IsSelectedChanged;

        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (Equals(value, _isSelected))
                    return;

                _isSelected = value;

                OnIsSelectedChanged();
            }
        }

        protected virtual void OnIsSelectedChanged()
        {
            IsSelectedChanged.Raise(this);
            OnPropertyChanged("IsSelected");
        }

        #endregion

        #region Design Property

        public TechObjectDesign Design
        {
            get { return _unit.Design; }
        }

        #endregion

        #region Name Property

        public string Name
        {
            get { return _unit.Name; }
        }

        #endregion

        #region Category Property

        public AssaultUnitCategory Category
        {
            get { return _category; }
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        [NonSerialized] private PropertyChangedEventHandler _propertyChanged;

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

    public class AssaultScreenSettings : SupportInitializeBase, INotifyPropertyChanged
    {
        private TimeSpan _explosionIntervalMaxPrecision = TimeSpan.FromSeconds(0.75);
        private TimeSpan _explosionIntervalBalanced = TimeSpan.FromSeconds(0.5);
        private TimeSpan _explosionIntervalMaxDamage = TimeSpan.FromSeconds(0.25);
        private TimeSpan _explosionIntervalUnloadAllOrdinance = TimeSpan.FromSeconds(0.05);
        private TimeSpan _attackOrbitalDefensesReplayDuration = TimeSpan.FromSeconds(3); // Don´t want to wait 11 secounds... for animation/sound
        private TimeSpan _bombardmentReplayDuration = TimeSpan.FromSeconds(2); // 2 is better then 8
        private TimeSpan _unloadAllOrdinanceReplayDuration = TimeSpan.FromSeconds(3); //AHA! Don´t want to wait 11 secounds... for animation/sound
        private TimeSpan _landTroopsReplayDuration = TimeSpan.FromSeconds(2); // from 16 to2

        public TimeSpan ExplosionIntervalMaxPrecision
        {
            get { return _explosionIntervalMaxPrecision; }
            set
            {
                VerifyInitializing();
                _explosionIntervalMaxPrecision = value;
                OnPropertyChanged("ExplosionIntervalMaxPrecision");
            }
        }

        public TimeSpan ExplosionIntervalBalanced
        {
            get { return _explosionIntervalBalanced; }
            set
            {
                VerifyInitializing();
                _explosionIntervalBalanced = value;
                OnPropertyChanged("ExplosionIntervalBalanced");
            }
        }

        public TimeSpan ExplosionIntervalMaxDamage
        {
            get { return _explosionIntervalMaxDamage; }
            set
            {
                VerifyInitializing();
                _explosionIntervalMaxDamage = value;
                OnPropertyChanged("ExplosionIntervalMaxDamage");
            }
        }

        public TimeSpan ExplosionIntervalUnloadAllOrdinance
        {
            get { return _explosionIntervalUnloadAllOrdinance; }
            set
            {
                VerifyInitializing();
                _explosionIntervalUnloadAllOrdinance = value;
                OnPropertyChanged("ExplosionIntervalUnloadAllOrdinance");
            }
        }

        public TimeSpan AttackOrbitalDefensesReplayDuration
        {
            get { return _attackOrbitalDefensesReplayDuration; }
            set
            {
                VerifyInitializing();
                _attackOrbitalDefensesReplayDuration = value;
                OnPropertyChanged("AttackOrbitalDefensesReplayDuration");
            }
        }

        public TimeSpan BombardmentReplayDuration
        {
            get { return _bombardmentReplayDuration; }
            set
            {
                VerifyInitializing();
                _bombardmentReplayDuration = value;
                OnPropertyChanged("BombardmentReplayDuration");
            }
        }

        public TimeSpan UnloadAllOrdinanceReplayDuration
        {
            get { return _unloadAllOrdinanceReplayDuration; }
            set
            {
                VerifyInitializing();
                _unloadAllOrdinanceReplayDuration = value;
                OnPropertyChanged("UnloadAllOrdinanceReplayDuration");
            }
        }

        public TimeSpan LandTroopsReplayDuration
        {
            get { return _landTroopsReplayDuration; }
            set
            {
                VerifyInitializing();
                _landTroopsReplayDuration = value;
                OnPropertyChanged("LandTroopsReplayDuration");
            }
        }

        #region Shared Instance

        private static AssaultScreenSettings _instance;

        public static AssaultScreenSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AssaultScreenSettings();
                    _instance.Refresh();
                }
                return _instance;
            }
        }

        #endregion

        #region I/O Operations

        private const string DataFileUri = "vfs:///Resources/Data/AssaultScreenSettings.xaml";

        public void Refresh()
        {
            try
            {
                IVirtualFileInfo dataFile;

                if (!ResourceManager.VfsService.TryGetFileInfo(new Uri(DataFileUri), out dataFile) ||
                    !dataFile.Exists)
                {
                    // nobody knows structure or content of this file
                    //GameLog.Client.GameData.WarnFormat(
                    //    "Could not locate data file \"{0}\".  Using default values instead.",
                    //    DataFileUri);

                    return;
                }

                using (var stream = dataFile.OpenRead())
                {
                    XamlHelper.LoadInto(this, stream);
                }
            }
            catch (Exception e)
            {
                GameLog.Client.GameData.Error(
                    string.Format(
                        "An error occurred while loading data file \"{0}\".  " +
                        "Check the error log for exception details.",
                        DataFileUri),
                    e);
            }
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        [NonSerialized] private PropertyChangedEventHandler _propertyChanged;

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
