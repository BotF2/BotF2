using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.Composite.Regions;
using Supremacy.Annotations;
using Supremacy.Client.Context;
using Supremacy.Client.Events;
using Supremacy.Client.Input;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Supremacy.Client.Views
{
    public class DiplomacyScreenViewModel : ViewModelBase<INewDiplomacyScreenView, DiplomacyScreenViewModel>
    {

        #region Design-Time Instance

        private static DiplomacyScreenViewModel _designInstance;

        public static DiplomacyScreenViewModel DesignInstance
        {
            get
            {
                if (_designInstance != null)
                    return _designInstance;

                // ReSharper disable AssignNullToNotNullAttribute
                _designInstance = new DiplomacyScreenViewModel(DesignTimeAppContext.Instance, null);
                // ReSharper restore AssignNullToNotNullAttribute

                _designInstance.SelectedForeignPower = _designInstance.ForeignPowers.First();
                _designInstance.DisplayMode = DiplomacyScreenDisplayMode.Outbox;

                _designInstance.MakeProposalCommand.Execute(null);
                _designInstance.SelectedForeignPower.OutgoingMessage.AvailableElements.First(o => o.ActionCategory == DiplomacyMessageElementActionCategory.Propose).AddCommand.Execute(null);
                _designInstance.SelectedForeignPower.OutgoingMessage.AvailableElements.First(o => o.ActionCategory == DiplomacyMessageElementActionCategory.Offer).AddCommand.Execute(null);

                return _designInstance;
            }
        }

        #endregion

        private readonly ObservableCollection<ForeignPowerViewModel> _foreignPowers;
        private readonly ReadOnlyObservableCollection<ForeignPowerViewModel> _foreignPowersView;
        
        private readonly DelegateCommand<ICheckableCommandParameter> _setDisplayModeCommand;

        private readonly DelegateCommand _commendCommand;
        private readonly DelegateCommand _denounceCommand;
        private readonly DelegateCommand _threatenCommand;
        private readonly DelegateCommand _makeProposalCommand;
        private readonly DelegateCommand _declareWarCommand;
        private readonly DelegateCommand _editMessageCommand;
        private readonly DelegateCommand _sendMessageCommand;
        private readonly DelegateCommand _cancelMessageCommand;
        private readonly DelegateCommand _resetGraphCommand;
        private readonly DelegateCommand<DiplomacyGraphNode> _setSelectedGraphNodeCommand;
 
        public DiplomacyScreenViewModel([NotNull] IAppContext appContext, [NotNull] IRegionManager regionManager)
            : base(appContext, regionManager)
        {
            _foreignPowers = new ObservableCollection<ForeignPowerViewModel>();
            _foreignPowersView = new ReadOnlyObservableCollection<ForeignPowerViewModel>(_foreignPowers);

            _setDisplayModeCommand = new DelegateCommand<ICheckableCommandParameter>(ExecuteSetDisplayModeComand, CanExecuteSetDisplayModeComand);

            _commendCommand = new DelegateCommand(ExecuteCommendCommand, CanExecuteCommendCommand);
            _denounceCommand = new DelegateCommand(ExecuteDenounceCommand, CanExecuteDenounceCommand);
            _threatenCommand = new DelegateCommand(ExecuteThreatenCommand, CanExecuteThreatenCommand);
            _makeProposalCommand = new DelegateCommand(ExecuteMakeProposalCommand, CanExecuteMakeProposalCommand);
            _declareWarCommand = new DelegateCommand(ExecuteDeclareWarCommand, CanExecuteDeclareWarCommand);
            _editMessageCommand = new DelegateCommand(ExecuteEditMessageCommand, CanExecuteEditMessageCommand);
            _sendMessageCommand = new DelegateCommand(ExecuteSendMessageCommand, CanExecuteSendMessageCommand);
            _cancelMessageCommand = new DelegateCommand(ExecuteCancelMessageCommand, CanExecuteCancelMessageCommand);
            _resetGraphCommand = new DelegateCommand(ExecuteResetGraphCommand);
            _setSelectedGraphNodeCommand = new DelegateCommand<DiplomacyGraphNode>(ExecuteSetSelectedGraphNodeCommand);

            Refresh();
        }

        private bool CanExecuteSetSelectedGraphNodeCommand(DiplomacyGraphNode node)
        {
            return node != null;
        }

        private void ExecuteSetSelectedGraphNodeCommand(DiplomacyGraphNode node)
        {
            if (CanExecuteSetSelectedGraphNodeCommand(node))
                SelectedGraphNode = node;
        }

        private void ExecuteResetGraphCommand()
        {
            SelectedGraphNode = LocalPlayerGraphNode;
        }

        #region Command Handlers

        private bool CanExecuteCommendCommand()
        {
            return false;
        }

        private void ExecuteCommendCommand()
        {
            OnCommandVisibilityChanged();
        }

        private bool CanExecuteDenounceCommand()
        {
            return false;
        }

        private void ExecuteDenounceCommand()
        {
            OnCommandVisibilityChanged();
        }

        private bool CanExecuteThreatenCommand()
        {
            return false;
        }

        private void ExecuteThreatenCommand()
        {
            OnCommandVisibilityChanged();
        }

        private bool CanExecuteMakeProposalCommand()
        {
            if (!CanExecuteNewProposalCommandCore(out ForeignPowerViewModel selectedForeignPower))
                return false;

            return true;
        }

        private bool CanExecuteNewProposalCommandCore(out ForeignPowerViewModel selectedForeignPower)
        {
            selectedForeignPower = SelectedForeignPower;

            if (selectedForeignPower == null)
                return false;

            if (!selectedForeignPower.IsDiplomatAvailable)
                return false;

            if (selectedForeignPower.OutgoingMessage != null)
                return false;

            return true;
        }

        private void ExecuteMakeProposalCommand()
        {
            ForeignPowerViewModel foreignPower;

            if (!CanExecuteNewProposalCommandCore(out foreignPower))
                return;

            foreignPower.OutgoingMessage = new DiplomacyMessageViewModel(_playerCivilization, _selectedForeignPower.Counterparty);
            foreignPower.OutgoingMessage.Edit();

            GameLog.Core.Diplomacy.DebugFormat("OutgoingMessage from {0}", foreignPower.Owner);

            OnCommandVisibilityChanged();
            OnIsMessageEditInProgressChanged();
            InvalidateCommands();
        }

        private bool CanExecuteDeclareWarCommand()
        {
            return CanExecuteDeclareWarCommandCore(out ForeignPowerViewModel foreignPower);
        }

        private bool CanExecuteDeclareWarCommandCore(out ForeignPowerViewModel selectedForeignPower)
        {
            selectedForeignPower = SelectedForeignPower;

            return selectedForeignPower != null &&
                   selectedForeignPower.OutgoingMessage == null &&
                   selectedForeignPower.Status != ForeignPowerStatus.AtWar;
        }

        private void ExecuteDeclareWarCommand()
        {
            ForeignPowerViewModel foreignPower;

            if (!CanExecuteDeclareWarCommandCore(out foreignPower))
                return;

            var message = new DiplomacyMessageViewModel(_playerCivilization, _selectedForeignPower.Counterparty);

            message.Edit();

            var declareWarElement = message.AvailableElements.FirstOrDefault(o => o.ElementType == DiplomacyMessageElementType.WarDeclaration);
            if (declareWarElement == null || !declareWarElement.AddCommand.CanExecute(null))
                return;

            declareWarElement.AddCommand.Execute(null);

            foreignPower.OutgoingMessage = message;

            InvalidateCommands();
            OnCommandVisibilityChanged();
        }

        private bool CanExecuteEditMessageCommand()
        {
            return DisplayMode == DiplomacyScreenDisplayMode.Outbox &&
                   SelectedForeignPower != null &&
                   SelectedForeignPower.OutgoingMessage != null &&
                   !SelectedForeignPower.OutgoingMessage.IsEditing;
        }

        private void ExecuteEditMessageCommand()
        {
            if (!CanExecuteEditMessageCommand())
                return;

            SelectedForeignPower.OutgoingMessage.Edit();

            OnCommandVisibilityChanged();
            OnIsMessageEditInProgressChanged();
        }

        private bool CanExecuteSendMessageCommand()
        {
            return DisplayMode == DiplomacyScreenDisplayMode.Outbox &&
                   SelectedForeignPower != null &&
                   SelectedForeignPower.OutgoingMessage != null &&
                   SelectedForeignPower.OutgoingMessage.IsEditing &&
                   SelectedForeignPower.OutgoingMessage.Elements.Count != 0;
        }

        private void ExecuteSendMessageCommand()
        {
            if (!CanExecuteSendMessageCommand())
                return;

            SelectedForeignPower.OutgoingMessage.Send();
            SelectedForeignPower.OnOutgoingMessageCategoryChanged();

            OnCommandVisibilityChanged();
            OnIsMessageEditInProgressChanged();
        }

        private bool CanExecuteCancelMessageCommand()
        {
            return DisplayMode == DiplomacyScreenDisplayMode.Outbox &&
                   SelectedForeignPower != null &&
                   SelectedForeignPower.OutgoingMessage != null;
        }

        private void ExecuteCancelMessageCommand()
        {
            if (!CanExecuteCancelMessageCommand())
                return;

            SelectedForeignPower.OutgoingMessage.Cancel();
            SelectedForeignPower.OutgoingMessage = null;

            OnCommandVisibilityChanged();
            OnIsMessageEditInProgressChanged();
        }

        private void ExecuteSetDisplayModeComand(ICheckableCommandParameter p)
        {
            if (!CanExecuteSetDisplayModeComand(p))
                return;

            DisplayMode = (DiplomacyScreenDisplayMode)p.InnerParameter;

            InvalidateCommands();
        }

        private bool CanExecuteSetDisplayModeComand(ICheckableCommandParameter p)
        {
            if (p == null)
                return false;

            var displayMode = p.InnerParameter as DiplomacyScreenDisplayMode?;
            if (displayMode == null)
            {
                p.IsChecked = false;
                return false;
            }

            p.IsChecked = (displayMode == DisplayMode);
            return true;
        }

        #endregion

        protected override void UnregisterCommandAndEventHandlers()
        {
            base.UnregisterCommandAndEventHandlers();

            ClientEvents.TurnStarted.Unsubscribe(OnTurnStarted);
        }

        protected override void RegisterCommandAndEventHandlers()
        {
            base.RegisterCommandAndEventHandlers();

            ClientEvents.TurnStarted.Subscribe(OnTurnStarted, ThreadOption.UIThread);
        }

        protected override void InvalidateCommands()
        {
            base.InvalidateCommands();

            _setDisplayModeCommand.RaiseCanExecuteChanged();

            _commendCommand.RaiseCanExecuteChanged();
            _denounceCommand.RaiseCanExecuteChanged();
            _threatenCommand.RaiseCanExecuteChanged();
            _makeProposalCommand.RaiseCanExecuteChanged();
            _declareWarCommand.RaiseCanExecuteChanged();
            _editMessageCommand.RaiseCanExecuteChanged();
            _sendMessageCommand.RaiseCanExecuteChanged();
            _cancelMessageCommand.RaiseCanExecuteChanged();

            if (_selectedForeignPower != null)
                _selectedForeignPower.InvalidateCommands();
        }

        private void OnTurnStarted(GameContextEventArgs args)
        {
            Refresh();
        }

        private void Refresh()
        {
            PlayerCivilization = AppContext.LocalPlayer.Empire;

            RefreshForeignPowers();
            RefreshRelationshipGraph();
        }

        #region Overrides of ViewModelBase<INewDiplomacyScreenView,DiplomacyScreenViewModel>

        public override string ViewName
        {
            get { return StandardGameScreens.DiplomacyScreen; }
        }

        protected internal override void RegisterViewWithRegion()
        {
            RegionManager.Regions[ClientRegions.GameScreens].Add(View, ViewName, true);
        }

        protected internal override void UnregisterViewWithRegion()
        {
            RegionManager.Regions[ClientRegions.GameScreens].Remove(View);
        }

        #endregion

        public ReadOnlyObservableCollection<ForeignPowerViewModel> ForeignPowers
        {
            get { return _foreignPowersView; }
        }

        public ICommand SetDisplayModeCommand
        {
            get { return _setDisplayModeCommand; }
        }

        public ICommand CommendCommand
        {
            get { return _commendCommand; }
        }

        public ICommand DenounceCommand
        {
            get { return _denounceCommand; }
        }

        public ICommand ThreatenCommand
        {
            get { return _threatenCommand; }
        }

        public ICommand MakeProposalCommand
        {
            get { return _makeProposalCommand; }
        }

        public ICommand DeclareWarCommand
        {
            get { return _declareWarCommand; }
        }

        public ICommand EditMessageCommand
        {
            get { return _editMessageCommand; }
        }

        public ICommand SendMessageCommand
        {
            get { return _sendMessageCommand; }
        }

        public ICommand CancelMessageCommand
        {
            get { return _cancelMessageCommand; }
        }

        public ICommand ResetGraphCommand
        {
            get { return _resetGraphCommand; }
        }

        public ICommand SetSelectedGraphNodeCommand
        {
            get { return _setSelectedGraphNodeCommand; }
        }

        #region PlayerCivilization Property

        [field: NonSerialized]
        public event EventHandler PlayerCivilizationChanged;

        private Civilization _playerCivilization;

        public Civilization PlayerCivilization
        {
            get { return _playerCivilization; }
            set
            {
                if (Equals(value, _playerCivilization))
                    return;

                _playerCivilization = value;

                OnPlayerCivilizationChanged();
            }
        }

        protected virtual void OnPlayerCivilizationChanged()
        {
            PlayerCivilizationChanged.Raise(this);
            OnPropertyChanged("PlayerCivilization");
        }

        #endregion

        #region SelectedGraphNode Property

        [field: NonSerialized]
        public event EventHandler SelectedGraphNodeChanged;

        private DiplomacyGraphNode _selectedGraphNode;

        public DiplomacyGraphNode SelectedGraphNode
        {
            get { return _selectedGraphNode; }
            set
            {
                if (Equals(value, _selectedGraphNode))
                    return;

                _selectedGraphNode = value;

                OnSelectedGraphNodeChanged();
            }
        }

        protected virtual void OnSelectedGraphNodeChanged()
        {
            SelectedGraphNodeChanged.Raise(this);
            OnPropertyChanged("SelectedGraphNode");
        }

        #endregion

        #region LocalPlayerGraphNode Property

        [field: NonSerialized]
        public event EventHandler LocalPlayerGraphNodeChanged;

        private DiplomacyGraphNode _localPlayerGraphNode;

        public DiplomacyGraphNode LocalPlayerGraphNode
        {
            get { return _localPlayerGraphNode; }
            set
            {
                if (Equals(value, _localPlayerGraphNode))
                    return;

                _localPlayerGraphNode = value;

                OnLocalPlayerGraphNodeChanged();
            }
        }

        protected virtual void OnLocalPlayerGraphNodeChanged()
        {
            LocalPlayerGraphNodeChanged.Raise(this);
            OnPropertyChanged("LocalPlayerGraphNode");
        }

        #endregion

        #region SelectedForeignPower Property

        [field: NonSerialized]
        public event EventHandler SelectedForeignPowerChanged;

        private ForeignPowerViewModel _selectedForeignPower;

        public ForeignPowerViewModel SelectedForeignPower
        {
            get
            {
                //if (_selectedForeignPower.Owner != null)
                //    GameLog.Client.Diplomacy.DebugFormat("_selectedForeignPower GET = {0}", _selectedForeignPower.Owner);
                //else
                //{
                //    GameLog.Client.Diplomacy.DebugFormat("_selectedForeignPower is NULL");
                //}
                return _selectedForeignPower;
            }
            set
            {
                if (Equals(value, _selectedForeignPower))
                    return;

                _selectedForeignPower = value;

                //if (_selectedForeignPower.Owner != null)
                //    GameLog.Client.Diplomacy.DebugFormat("_selectedForeignPower SET = {0}", _selectedForeignPower.Owner);
                //else
                //{
                //    GameLog.Client.Diplomacy.DebugFormat("_selectedForeignPower is NULL");
                //}

                OnSelectedForeignPowerChanged();
            }
        }

        protected virtual void OnSelectedForeignPowerChanged()
        {
            SelectedForeignPowerChanged.Raise(this);
            OnPropertyChanged("SelectedForeignPower");
            OnAreOutgoingMessageCommandsVisibleChanged();
            OnAreIncomingMessageCommandsVisibleChanged();
            OnAreNewMessageCommandsVisibleChanged();
        }

        #endregion

        #region AreOutgoingMessageCommandsVisible Property

        [field: NonSerialized]
        public event EventHandler AreOutgoingMessageCommandsVisibleChanged;

        public bool AreOutgoingMessageCommandsVisible
        {
            get
            {
                if (DisplayMode != DiplomacyScreenDisplayMode.Outbox)
                    return false;

                var selectedForeignPower = SelectedForeignPower;  // if one is selected in the screen

                if (selectedForeignPower != null)
                    GameLog.Client.Diplomacy.DebugFormat("DisplayMode is Outbox, SelectedForeignPower ={0}", selectedForeignPower.Counterparty.Key);

                return selectedForeignPower != null &&
                       selectedForeignPower.OutgoingMessage != null;
            }
        }

        protected virtual void OnAreOutgoingMessageCommandsVisibleChanged()
        {
            AreOutgoingMessageCommandsVisibleChanged.Raise(this);
            OnPropertyChanged("AreOutgoingMessageCommandsVisible");
        }

        #endregion

        #region AreIncomingMessageCommandsVisible Property

        [field: NonSerialized]
        public event EventHandler AreIncomingMessageCommandsVisibleChanged;

        public bool AreIncomingMessageCommandsVisible
        {
            get
            {
                if (DisplayMode != DiplomacyScreenDisplayMode.Inbox)
                {
                    //GameLog.Core.Diplomacy.DebugFormat("DisplayMode not DiplomacyScreenDispalyMode.Inbox" );
                    return false;
                }
                else
                {
                    GameLog.Core.Diplomacy.DebugFormat("DisplayMode = INBOX selected");
                }

                var selectedForeignPower = SelectedForeignPower;

                GameLog.Core.Diplomacy.DebugFormat("DisplayMode is Inbox, SelectedForeignPower ={0}", selectedForeignPower.Counterparty.Key);
                return selectedForeignPower != null && selectedForeignPower.IncomingMessage != null && 
                       !selectedForeignPower.IncomingMessage.IsStatement;
            }
        }

        protected virtual void OnAreIncomingMessageCommandsVisibleChanged()
        {
            AreIncomingMessageCommandsVisibleChanged.Raise(this);
            OnPropertyChanged("AreIncomingMessageCommandsVisible");
        }

        #endregion

        #region AreNewMessageCommandsVisible Property

        [field: NonSerialized]
        public event EventHandler AreNewMessageCommandsVisibleChanged;

        public bool AreNewMessageCommandsVisible
        {
            get
            {
                if (DisplayMode != DiplomacyScreenDisplayMode.Outbox)
                    return false;

                var selectedForeignPower = SelectedForeignPower;

                return selectedForeignPower != null &&
                       selectedForeignPower.OutgoingMessage == null;
            }
        }

        protected virtual void OnAreNewMessageCommandsVisibleChanged()
        {
            AreNewMessageCommandsVisibleChanged.Raise(this);
            OnPropertyChanged("AreNewMessageCommandsVisible");
        }

        #endregion

        #region DisplayMode Property

        [field: NonSerialized]
        public event EventHandler DisplayModeChanged;

        private DiplomacyScreenDisplayMode _displayMode;

        public DiplomacyScreenDisplayMode DisplayMode
        {
            get { return _displayMode; }
            set
            {
                if (Equals(value, _displayMode))
                    return;

                _displayMode = value;

                OnDisplayModeChanged();
            }
        }

        protected virtual void OnDisplayModeChanged()
        {
            DisplayModeChanged.Raise(this);
            OnPropertyChanged("DisplayMode");
            InvalidateCommands();
            OnCommandVisibilityChanged();
        }

        #endregion

        #region IsMessageEditInProgress Property

        [field: NonSerialized]
        public event EventHandler IsMessageEditInProgressChanged;

        public bool IsMessageEditInProgress
        {
            get
            {
                return DisplayMode == DiplomacyScreenDisplayMode.Outbox &&
                       SelectedForeignPower != null &&
                       SelectedForeignPower.OutgoingMessage != null &&
                       SelectedForeignPower.OutgoingMessage.IsEditing;
            }
        }

        protected virtual void OnIsMessageEditInProgressChanged()
        {
            IsMessageEditInProgressChanged.Raise(this);
            OnPropertyChanged("IsMessageEditInProgress");
        }

        #endregion

        private void OnCommandVisibilityChanged()
        {
            OnAreIncomingMessageCommandsVisibleChanged();
            OnAreOutgoingMessageCommandsVisibleChanged();
            OnAreNewMessageCommandsVisibleChanged();
        }

        private void RefreshForeignPowers()
        {
            var selectedForeignPower = (SelectedForeignPower != null) ? SelectedForeignPower.Counterparty : null;

            SelectedForeignPower = null;

            _foreignPowers.Clear();

            var playerEmpireId = AppContext.LocalPlayer.EmpireID;
            var playerDiplomat = Diplomat.Get(playerEmpireId);

            foreach (var civ in GameContext.Current.Civilizations)
            {
                if (civ.CivID == playerEmpireId || !DiplomacyHelper.IsContactMade(playerEmpireId, civ.CivID))
                    continue;

                var foreignPower = playerDiplomat.GetForeignPower(civ);
                var foreignPowerViewModel = new ForeignPowerViewModel(foreignPower);

                _foreignPowers.Add(foreignPowerViewModel);
                GameLog.Client.Diplomacy.DebugFormat("Added ForeignPowerViewModel: civ {0} vs {2} (playerEmpireID {1})", civ.ShortName, playerEmpireId, AppContext.LocalPlayer.Empire.Name);
            }

            if (selectedForeignPower != null)
                SelectedForeignPower = _foreignPowers.FirstOrDefault(o => o.Counterparty.CivID == selectedForeignPower.CivID);
           
        }

        private void RefreshRelationshipGraph()
        {
            var count = GameContext.Current.Civilizations.Count;
            var nodes = new List<DiplomacyGraphNode>(count);
            var localPlayerEmpire = AppContext.LocalPlayer.Empire;

            DiplomacyGraphNode localPlayerNode = null;

            foreach (var civ in GameContext.Current.Civilizations)
            {
                var node = new DiplomacyGraphNode(civ, _setSelectedGraphNodeCommand);
                
                nodes.Add(node);

                if (civ == localPlayerEmpire)
                    localPlayerNode = node;
            }
            
            for (var i = 0; i < count; i++)
            {
                for (var j = 0; j < count; j++)
                {
                    if (i != j &&
                        DiplomacyHelper.IsContactMade(nodes[i].Civilization, nodes[j].Civilization) &&
                        (nodes[i].Civilization == localPlayerEmpire || DiplomacyHelper.IsContactMade(localPlayerEmpire, nodes[i].Civilization)) &&
                        (nodes[j].Civilization == localPlayerEmpire || DiplomacyHelper.IsContactMade(localPlayerEmpire, nodes[j].Civilization)))
                    {
                        nodes[i].Children.Add(nodes[j]);
                    }
                }
            }

            LocalPlayerGraphNode = localPlayerNode;
            SelectedGraphNode = localPlayerNode;
        }

        internal static DiplomacyMessageElementType ElementTypeFromClauseType(ClauseType clauseType)
        {
            switch (clauseType)
            {
                case ClauseType.OfferWithdrawTroops:
                    return DiplomacyMessageElementType.OfferWithdrawTroopsClause;
                case ClauseType.RequestWithdrawTroops:
                    return DiplomacyMessageElementType.RequestWithdrawTroopsClause;
                case ClauseType.OfferStopPiracy:
                    return DiplomacyMessageElementType.OfferStopPiracyClause;
                case ClauseType.RequestStopPiracy:
                    return DiplomacyMessageElementType.RequestStopPiracyClause;
                case ClauseType.OfferBreakAgreement:
                    return DiplomacyMessageElementType.OfferBreakAgreementClause;
                case ClauseType.RequestBreakAgreement:
                    return DiplomacyMessageElementType.RequestBreakAgreementClause;
                case ClauseType.OfferGiveCredits:
                    return DiplomacyMessageElementType.OfferGiveCreditsClause;
                case ClauseType.RequestGiveCredits:
                    return DiplomacyMessageElementType.RequestGiveCreditsClause;
                case ClauseType.OfferGiveResources:
                    return DiplomacyMessageElementType.OfferGiveResourcesClause;
                case ClauseType.RequestGiveResources:
                    return DiplomacyMessageElementType.RequestGiveResourcesClause;
                case ClauseType.OfferMapData:
                    return DiplomacyMessageElementType.OfferMapDataClause;
                case ClauseType.RequestMapData:
                    return DiplomacyMessageElementType.RequestMapDataClause;
                case ClauseType.OfferHonorMilitaryAgreement:
                    return DiplomacyMessageElementType.OfferHonorMilitaryAgreementClause;
                case ClauseType.RequestHonorMilitaryAgreement:
                    return DiplomacyMessageElementType.RequestHonorMilitaryAgreementClause;
                case ClauseType.OfferEndEmbargo:
                    return DiplomacyMessageElementType.OfferEndEmbargoClause;
                case ClauseType.RequestEndEmbargo:
                    return DiplomacyMessageElementType.RequestEndEmbargoClause;
                case ClauseType.TreatyWarPact:
                    return DiplomacyMessageElementType.TreatyWarPact;
                case ClauseType.TreatyCeaseFire:
                    return DiplomacyMessageElementType.TreatyCeaseFireClause;
                case ClauseType.TreatyNonAggression:
                    return DiplomacyMessageElementType.TreatyNonAggressionClause;
                case ClauseType.TreatyOpenBorders:
                    return DiplomacyMessageElementType.TreatyOpenBordersClause;
                case ClauseType.TreatyTradePact:
                    return DiplomacyMessageElementType.TreatyTradePactClause;
                case ClauseType.TreatyResearchPact:
                    return DiplomacyMessageElementType.TreatyResearchPactClause;
                case ClauseType.TreatyAffiliation:
                    return DiplomacyMessageElementType.TreatyAffiliationClause;
                case ClauseType.TreatyDefensiveAlliance:
                    return DiplomacyMessageElementType.TreatyDefensiveAllianceClause;
                case ClauseType.TreatyFullAlliance:
                    return DiplomacyMessageElementType.TreatyFullAllianceClause;
                case ClauseType.TreatyMembership:
                    return DiplomacyMessageElementType.TreatyMembershipClause;
                default:
                    throw new ArgumentOutOfRangeException("clauseType", "Unknown clause type: " + clauseType);
            }
        }

        internal static ClauseType ElementTypeToClauseType(DiplomacyMessageElementType elementType)
        {
            switch (elementType)
            {
                case DiplomacyMessageElementType.OfferWithdrawTroopsClause:
                    return ClauseType.OfferWithdrawTroops;
                case DiplomacyMessageElementType.RequestWithdrawTroopsClause:
                    return ClauseType.RequestWithdrawTroops;
                case DiplomacyMessageElementType.OfferStopPiracyClause:
                    return ClauseType.OfferStopPiracy;
                case DiplomacyMessageElementType.RequestStopPiracyClause:
                    return ClauseType.RequestStopPiracy;
                case DiplomacyMessageElementType.OfferBreakAgreementClause:
                    return ClauseType.OfferBreakAgreement;
                case DiplomacyMessageElementType.RequestBreakAgreementClause:
                    return ClauseType.RequestBreakAgreement;
                case DiplomacyMessageElementType.OfferGiveCreditsClause:
                    return ClauseType.OfferGiveCredits;
                case DiplomacyMessageElementType.RequestGiveCreditsClause:
                    return ClauseType.RequestGiveCredits;
                case DiplomacyMessageElementType.OfferGiveResourcesClause:
                    return ClauseType.OfferGiveResources;
                case DiplomacyMessageElementType.RequestGiveResourcesClause:
                    return ClauseType.RequestGiveResources;
                case DiplomacyMessageElementType.OfferMapDataClause:
                    return ClauseType.OfferMapData;
                case DiplomacyMessageElementType.RequestMapDataClause:
                    return ClauseType.RequestMapData;
                case DiplomacyMessageElementType.OfferHonorMilitaryAgreementClause:
                    return ClauseType.OfferHonorMilitaryAgreement;
                case DiplomacyMessageElementType.RequestHonorMilitaryAgreementClause:
                    return ClauseType.RequestHonorMilitaryAgreement;
                case DiplomacyMessageElementType.OfferEndEmbargoClause:
                    return ClauseType.OfferEndEmbargo;
                case DiplomacyMessageElementType.RequestEndEmbargoClause:
                    return ClauseType.RequestEndEmbargo;
                case DiplomacyMessageElementType.TreatyWarPact:
                    return ClauseType.TreatyWarPact;
                case DiplomacyMessageElementType.TreatyCeaseFireClause:
                    return ClauseType.TreatyCeaseFire;
                case DiplomacyMessageElementType.TreatyNonAggressionClause:
                    return ClauseType.TreatyNonAggression;
                case DiplomacyMessageElementType.TreatyOpenBordersClause:
                    return ClauseType.TreatyOpenBorders;
                case DiplomacyMessageElementType.TreatyTradePactClause:
                    return ClauseType.TreatyTradePact;
                case DiplomacyMessageElementType.TreatyResearchPactClause:
                    return ClauseType.TreatyResearchPact;
                case DiplomacyMessageElementType.TreatyAffiliationClause:
                    return ClauseType.TreatyAffiliation;
                case DiplomacyMessageElementType.TreatyDefensiveAllianceClause:
                    return ClauseType.TreatyDefensiveAlliance;
                case DiplomacyMessageElementType.TreatyFullAllianceClause:
                    return ClauseType.TreatyFullAlliance;
                case DiplomacyMessageElementType.TreatyMembershipClause:
                    return ClauseType.TreatyMembership;
                default:
                    return ClauseType.NoClause;
            }
        }

        internal static StatementType ElementTypeToStatementType(DiplomacyMessageElementType elementType)
        {
            switch (elementType)
            {
                case DiplomacyMessageElementType.WarDeclaration:
                    return StatementType.WarDeclaration;
                case DiplomacyMessageElementType.CommendWarStatement:
                    return StatementType.CommendWar;
                case DiplomacyMessageElementType.CommendTreatyStatement:
                    return StatementType.CommendRelationship;
                case DiplomacyMessageElementType.CommendAssaultStatement:
                    return StatementType.CommendAssault;
                case DiplomacyMessageElementType.CommendInvasionStatement:
                    return StatementType.CommendInvasion;
                case DiplomacyMessageElementType.CommendSabotageStatement:
                    return StatementType.CommendSabotage;
                case DiplomacyMessageElementType.DenounceWarStatement:
                    return StatementType.DenounceWar;
                case DiplomacyMessageElementType.DenounceTreatyStatement:
                    return StatementType.DenounceRelationship;
                case DiplomacyMessageElementType.DenounceAssaultStatement:
                    return StatementType.DenounceAssault;
                case DiplomacyMessageElementType.DenounceInvasionStatement:
                    return StatementType.DenounceInvasion;
                case DiplomacyMessageElementType.DenounceSabotageStatement:
                    return StatementType.DenounceSabotage;
                default:
                    return StatementType.NoStatement;
            }
        }
    }
}