// File:DiplomacyScreenViewModel.cs
using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.ServiceLocation;
using Supremacy.Annotations;
using Supremacy.Client.Context;
using Supremacy.Client.Controls;
using Supremacy.Client.Events;
using Supremacy.Client.Input;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Intelligence;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Supremacy.Client.Views
{

    public class DiplomacyScreenViewModel : ViewModelBase<INewDiplomacyScreenView, DiplomacyScreenViewModel>
    {
        private bool _isMembershipButtonVisible;
        private bool _isFullAllianceButtonVisible;
       
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
                if (_designInstance.ForeignPowers != null)
                _designInstance.SelectedForeignPower = _designInstance.ForeignPowers.First();
                _designInstance.DisplayMode = DiplomacyScreenDisplayMode.Outbox;
                
                //_designInstance.MakeProposalCommand.Execute(null);
                _designInstance.SelectedForeignPower.OutgoingMessage.AvailableElements.First(o => o.ActionCategory == DiplomacyMessageElementActionCategory.Propose).AddCommand.Execute(null);
                _designInstance.SelectedForeignPower.OutgoingMessage.AvailableElements.First(o => o.ActionCategory == DiplomacyMessageElementActionCategory.Offer).AddCommand.Execute(null);
               
                return _designInstance;
            }
        }

        #endregion Design-Time Instance

        public Civilization LocalPalyer => (Civilization)GameContext.Current.CivilizationManagers[AppContext.LocalPlayer.CivID].Civilization;
        public bool localIsHost => AppContext.IsGameHost;

        private readonly ObservableCollection<ForeignPowerViewModel> _foreignPowers;
        private readonly ReadOnlyObservableCollection<ForeignPowerViewModel> _foreignPowersView;
        /*
          DISPLAY MODE
         */
        private readonly DelegateCommand<ICheckableCommandParameter> _setDisplayModeCommand; 

        private readonly DelegateCommand _commendCommand;
        private readonly DelegateCommand _denounceCommand;
        private readonly DelegateCommand _threatenCommand;
        //private readonly DelegateCommand _makeProposalCommand;
        private readonly DelegateCommand _declareWarCommand;
        private readonly DelegateCommand _endWarCommand;  // other naming in the code: CeaseFire
        private readonly DelegateCommand _openBordersCommand;
        //private readonly DelegateCommand _acceptRejectCommand;
        private readonly DelegateCommand _nonAgressionCommand;
        private readonly DelegateCommand _affiliationCommand;
        private readonly DelegateCommand _defenceAllianceCommand;
        private readonly DelegateCommand _fullAllianceCommand;
        private readonly DelegateCommand _membershipCommand;
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
            // DISPLAY MODE
            _setDisplayModeCommand = new DelegateCommand<ICheckableCommandParameter>(ExecuteSetDisplayModeComand, CanExecuteSetDisplayModeComand);

            _commendCommand = new DelegateCommand(ExecuteCommendCommand, CanExecuteCommendCommand);
            _denounceCommand = new DelegateCommand(ExecuteDenounceCommand, CanExecuteDenounceCommand);
            _threatenCommand = new DelegateCommand(ExecuteThreatenCommand, CanExecuteThreatenCommand);

           // _makeProposalCommand = new DelegateCommand(ExecuteMakeProposalCommand, CanExecuteMakeProposalCommand);

            _declareWarCommand = new DelegateCommand(ExecuteDeclareWarCommand, CanExecuteDeclareWarCommand);
            _endWarCommand = new DelegateCommand(ExecuteEndWarCommand, CanExecuteEndWarCommand);
            _openBordersCommand = new DelegateCommand(ExecuteOpenBordersCommand, CanExecuteOpenBordersCommand);
            //_acceptRejectCommand = new DelegateCommand(ExecuteAcceptRejectDictionaryCommand, CanExecuteAcceptRejectDictionaryCommand);
            _nonAgressionCommand = new DelegateCommand(ExecuteNonAgressionCommand, CanExecuteNonAgressionCommand);
            _affiliationCommand = new DelegateCommand(ExecuteAffiliationCommand, CanExecuteAffiliationCommand);
            _defenceAllianceCommand = new DelegateCommand(ExecuteDefenceAllianceCommand, CanExecuteDefenceAllianceCommand);
            _fullAllianceCommand = new DelegateCommand(ExecuteFullAllianceCommand, CanExecuteFullAllianceCommand);
            _membershipCommand = new DelegateCommand(ExecuteMembershipCommand, CanExecuteMembershipCommand);
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

            //GameLog.Core.Diplomacy.DebugFormat("OutgoingMessage from {0}", foreignPower.Owner);

            OnCommandVisibilityChanged();
            OnIsMessageEditInProgressChanged();
            InvalidateCommands();
            //Refresh(); crashes
        }

        #region DeclareWarCommandButton
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

            DisplayMode = DiplomacyScreenDisplayMode.Outbox; // new
     
            AreOutgoingMessageCommandsVisibleChanged.Raise(this);
            OnPropertyChanged("AreOutgoingMessageCommandsVisible");

            var message = new DiplomacyMessageViewModel(_playerCivilization, _selectedForeignPower.Counterparty);

            message.Edit();

            var declareWarElement = message.AvailableElements.FirstOrDefault(o => o.ElementType == DiplomacyMessageElementType.WarDeclaration);
            if (declareWarElement == null || !declareWarElement.AddCommand.CanExecute(null))
                return;

            declareWarElement.AddCommand.Execute(null);

            foreignPower.OutgoingMessage = message;

            InvalidateCommands();
            OnCommandVisibilityChanged();
            //Refresh();
        }
        #endregion DeclareWarCommandButton


        #region EndWarCommandButton
        private bool CanExecuteEndWarCommand()
        {
            return CanExecuteEndWarCommandCore(out ForeignPowerViewModel foreignPower);
        }

        private bool CanExecuteEndWarCommandCore(out ForeignPowerViewModel selectedForeignPower)
        {
            selectedForeignPower = SelectedForeignPower;

            return selectedForeignPower != null &&
                   selectedForeignPower.Status == ForeignPowerStatus.AtWar;
        }

        private void ExecuteEndWarCommand()
        {
            ForeignPowerViewModel foreignPower;

            if (!CanExecuteEndWarCommandCore(out foreignPower))
                return;

            DisplayMode = DiplomacyScreenDisplayMode.Outbox; // new
       
            AreOutgoingMessageCommandsVisibleChanged.Raise(this);
            OnPropertyChanged("AreOutgoingMessageCommandsVisible");

            var message = new DiplomacyMessageViewModel(_playerCivilization, _selectedForeignPower.Counterparty);

            message.Edit();

            var endWarElement = message.AvailableElements.FirstOrDefault(o => o.ElementType == DiplomacyMessageElementType.TreatyCeaseFireClause); // CeaseFire = endWar
            if (endWarElement == null || !endWarElement.AddCommand.CanExecute(null))
                return;

            endWarElement.AddCommand.Execute(null);

            foreignPower.OutgoingMessage = message;

            InvalidateCommands();
            OnCommandVisibilityChanged();
           // Refresh();
        }
        #endregion EndWarCommandButton


        #region OpenBordersCommandButton
        private bool CanExecuteOpenBordersCommand()
        {
            //Refresh();
            return CanExecuteOpenBordersCommandCore(out ForeignPowerViewModel foreignPower);
        }

        private bool CanExecuteOpenBordersCommandCore(out ForeignPowerViewModel selectedForeignPower)
        {
            selectedForeignPower = SelectedForeignPower;

            return selectedForeignPower != null &&
                   selectedForeignPower.Status != ForeignPowerStatus.AtWar;
       //// conditions for TradeRoute
       ////agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyOpenBorders) ||
       ////agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyTradePact) ||
       ////agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyAffiliation) ||
       ////agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyDefensiveAlliance) ||
       ////agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyFullAlliance);
        }

        private void ExecuteOpenBordersCommand()
        {
            ForeignPowerViewModel foreignPower;

            if (!CanExecuteOpenBordersCommandCore(out foreignPower))
                return;

            DisplayMode = DiplomacyScreenDisplayMode.Outbox; // new
  
            AreOutgoingMessageCommandsVisibleChanged.Raise(this);
            OnPropertyChanged("AreOutgoingMessageCommandsVisible");

            var message = new DiplomacyMessageViewModel(_playerCivilization, _selectedForeignPower.Counterparty);

            message.Edit();

            var openBordersElement = message.AvailableElements.FirstOrDefault(o => o.ElementType == DiplomacyMessageElementType.TreatyOpenBordersClause);
            if (openBordersElement == null || !openBordersElement.AddCommand.CanExecute(null))
                return;

            openBordersElement.AddCommand.Execute(null);

            foreignPower.OutgoingMessage = message;

            InvalidateCommands();
            OnCommandVisibilityChanged();
            //Refresh();
        }
        #endregion OpenBordersCommandButton

        #region SendAcceptRejectDictionary

        private bool CanExecuteAcceptRejectDictionaryCommand()
        {
            //Refresh();
            return CanExecuteAcceptRejectDictionaryCommandCore(out ForeignPowerViewModel foreignPower);
        }
        private bool CanExecuteAcceptRejectDictionaryCommandCore(out ForeignPowerViewModel selectedForeignPower)
        {
            selectedForeignPower = SelectedForeignPower;

            return selectedForeignPower != null; // &&
                   //selectedForeignPower.OutgoingMessage == null &&
                   //selectedForeignPower.Status != ForeignPowerStatus.AtWar;
            //// conditions for TradeRoute
            ////agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyOpenBorders) ||
            ////agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyTradePact) ||
            ////agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyAffiliation) ||
            ////agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyDefensiveAlliance) ||
            ////agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyFullAlliance);
        }

        #endregion SendAcceptRejectDictionary

        #region NonAgressionCommandButton
        private bool CanExecuteNonAgressionCommand()
        {
           //Refresh();
            return CanExecuteNonAgressionCommandCore(out ForeignPowerViewModel foreignPower);
        }

        private bool CanExecuteNonAgressionCommandCore(out ForeignPowerViewModel selectedForeignPower)
        {
            selectedForeignPower = SelectedForeignPower;

            return selectedForeignPower != null &&
                   //selectedForeignPower.OutgoingMessage == null &&
                   selectedForeignPower.Status != ForeignPowerStatus.AtWar;
            //// conditions for TradeRoute
            ////agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyOpenBorders) ||
            ////agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyTradePact) ||
            ////agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyAffiliation) ||
            ////agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyDefensiveAlliance) ||
            ////agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyFullAlliance);
        }

        private void ExecuteNonAgressionCommand()
        {
            ForeignPowerViewModel foreignPower;

            if (!CanExecuteNonAgressionCommandCore(out foreignPower))
                return;

            DisplayMode = DiplomacyScreenDisplayMode.Outbox; // new
     
            AreOutgoingMessageCommandsVisibleChanged.Raise(this);
            OnPropertyChanged("AreOutgoingMessageCommandsVisible");

            var message = new DiplomacyMessageViewModel(_playerCivilization, _selectedForeignPower.Counterparty);

            message.Edit();

            var nonAgressionElement = message.AvailableElements.FirstOrDefault(o => o.ElementType == DiplomacyMessageElementType.TreatyNonAggressionClause);
            if (nonAgressionElement == null || !nonAgressionElement.AddCommand.CanExecute(null))
                return;

            nonAgressionElement.AddCommand.Execute(null);

            foreignPower.OutgoingMessage = message;

            InvalidateCommands();
            OnCommandVisibilityChanged();
            //Refresh();
        }
        #endregion NonAgressionCommandButton


        #region AffiliationCommandButton
        private bool CanExecuteAffiliationCommand()
        {
            //Refresh();
            return CanExecuteAffiliationCommandCore(out ForeignPowerViewModel foreignPower);
        }

        private bool CanExecuteAffiliationCommandCore(out ForeignPowerViewModel selectedForeignPower)
        {
            selectedForeignPower = SelectedForeignPower;

            return selectedForeignPower != null &&
                   selectedForeignPower.Status != ForeignPowerStatus.AtWar;
            //// conditions for TradeRoute
            ////agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyOpenBorders) ||
            ////agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyTradePact) ||
            ////agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyAffiliation) ||
            ////agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyDefensiveAlliance) ||
            ////agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyFullAlliance);
        }

        private void ExecuteAffiliationCommand()
        {
            ForeignPowerViewModel foreignPower;

            if (!CanExecuteAffiliationCommandCore(out foreignPower))
                return;

            DisplayMode = DiplomacyScreenDisplayMode.Outbox; // new
      
            AreOutgoingMessageCommandsVisibleChanged.Raise(this);
            OnPropertyChanged("AreOutgoingMessageCommandsVisible");

            var message = new DiplomacyMessageViewModel(_playerCivilization, _selectedForeignPower.Counterparty);

            message.Edit();

            var affiliationElement = message.AvailableElements.FirstOrDefault(o => o.ElementType == DiplomacyMessageElementType.TreatyOpenBordersClause);
            if (affiliationElement == null || !affiliationElement.AddCommand.CanExecute(null))
                return;

            affiliationElement.AddCommand.Execute(null);

            foreignPower.OutgoingMessage = message;

            InvalidateCommands();
            OnCommandVisibilityChanged();
           // Refresh();
        }
        #endregion AffiliationCommandButton


        #region DefenceAllianceCommandButton
        private bool CanExecuteDefenceAllianceCommand()
        {
            //Refresh();
            return CanExecuteDefenceAllianceCommandCore(out ForeignPowerViewModel foreignPower);
        }

        private bool CanExecuteDefenceAllianceCommandCore(out ForeignPowerViewModel selectedForeignPower)
        {
            selectedForeignPower = SelectedForeignPower;

            return selectedForeignPower != null &&
                   //selectedForeignPower.OutgoingMessage == null &&
                   selectedForeignPower.Status != ForeignPowerStatus.AtWar;
            //// conditions for TradeRoute
            ////agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyOpenBorders) ||
            ////agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyTradePact) ||
            ////agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyAffiliation) ||
            ////agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyDefensiveAlliance) ||
            ////agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyFullAlliance);
        }

        private void ExecuteDefenceAllianceCommand()
        {
            ForeignPowerViewModel foreignPower;

            if (!CanExecuteDefenceAllianceCommandCore(out foreignPower))
                return;

            DisplayMode = DiplomacyScreenDisplayMode.Outbox; // new = DiplomacyScreenDisplayMode.Outbox; // new
     
            AreOutgoingMessageCommandsVisibleChanged.Raise(this);
            OnPropertyChanged("AreOutgoingMessageCommandsVisible");

            var message = new DiplomacyMessageViewModel(_playerCivilization, _selectedForeignPower.Counterparty);

            message.Edit();

            var defenceAllianceElement = message.AvailableElements.FirstOrDefault(o => o.ElementType == DiplomacyMessageElementType.TreatyDefensiveAllianceClause);
            if (defenceAllianceElement == null || !defenceAllianceElement.AddCommand.CanExecute(null))
                return;

            defenceAllianceElement.AddCommand.Execute(null);

            foreignPower.OutgoingMessage = message;

            InvalidateCommands();
            OnCommandVisibilityChanged();
           // Refresh();
        }
        #endregion DefenceAllianceCommandButton

        #region FullAllianceCommandButton
        private bool CanExecuteFullAllianceCommand()
        {
            //Refresh();
            return CanExecuteFullAllianceCommandCore(out ForeignPowerViewModel foreignPower);
        }

        private bool CanExecuteFullAllianceCommandCore(out ForeignPowerViewModel selectedForeignPower)
        {
            selectedForeignPower = SelectedForeignPower;

            return selectedForeignPower != null &&
                   selectedForeignPower.Status != ForeignPowerStatus.AtWar;
        }

        private void ExecuteFullAllianceCommand()
        {
            ForeignPowerViewModel foreignPower;

            if (!CanExecuteFullAllianceCommandCore(out foreignPower))
                return;

            DisplayMode = DiplomacyScreenDisplayMode.Outbox; // new = DiplomacyScreenDisplayMode.Outbox; // new
      
            AreOutgoingMessageCommandsVisibleChanged.Raise(this);
            OnPropertyChanged("AreOutgoingMessageCommandsVisible");

            var message = new DiplomacyMessageViewModel(_playerCivilization, _selectedForeignPower.Counterparty);

            message.Edit();

            var fullAllianceElement = message.AvailableElements.FirstOrDefault(o => o.ElementType == DiplomacyMessageElementType.TreatyFullAllianceClause);
            if (fullAllianceElement == null || !fullAllianceElement.AddCommand.CanExecute(null))
                return;

            fullAllianceElement.AddCommand.Execute(null);

            foreignPower.OutgoingMessage = message;

            InvalidateCommands();
            OnCommandVisibilityChanged();
           // Refresh();
        }
        #endregion FullAllianceCommandButton


        #region MembershipCommandButton
        private bool CanExecuteMembershipCommand()
        {
           // Refresh();
            return CanExecuteMembershipCommandCore(out ForeignPowerViewModel foreignPower);
        }

        private bool CanExecuteMembershipCommandCore(out ForeignPowerViewModel selectedForeignPower)
        {
            selectedForeignPower = SelectedForeignPower;

            return selectedForeignPower != null &&
                   selectedForeignPower.Status != ForeignPowerStatus.AtWar;
        }

        private void ExecuteMembershipCommand()
        {
            ForeignPowerViewModel foreignPower;

            if (!CanExecuteMembershipCommandCore(out foreignPower))
                return;

            DisplayMode = DiplomacyScreenDisplayMode.Outbox; // new = DiplomacyScreenDisplayMode.Outbox; // new

            AreOutgoingMessageCommandsVisibleChanged.Raise(this);
            OnPropertyChanged("AreOutgoingMessageCommandsVisible");

            var message = new DiplomacyMessageViewModel(_playerCivilization, _selectedForeignPower.Counterparty);

            message.Edit();

            var membershipElement = message.AvailableElements.FirstOrDefault(o => o.ElementType == DiplomacyMessageElementType.TreatyMembershipClause);
            if (membershipElement == null || !membershipElement.AddCommand.CanExecute(null))
                return;

            membershipElement.AddCommand.Execute(null);

            foreignPower.OutgoingMessage = message;

            InvalidateCommands();
            OnCommandVisibilityChanged();
            //Refresh();
        }
        #endregion MembershipCommandButton

        private bool CanExecuteEditMessageCommand()
        {
            return DisplayMode == DiplomacyScreenDisplayMode.Outbox; //&&
                   //SelectedForeignPower != null &&
                   //SelectedForeignPower.OutgoingMessage != null &&
                   //!SelectedForeignPower.OutgoingMessage.IsEditing;
        }

        private void ExecuteEditMessageCommand()
        {
            if (!CanExecuteEditMessageCommand())
                return;
            if (SelectedForeignPower != null &&
                SelectedForeignPower.OutgoingMessage != null &&
                !SelectedForeignPower.OutgoingMessage.IsEditing)
            {
                SelectedForeignPower.OutgoingMessage.Edit();
                OnCommandVisibilityChanged();
                OnIsMessageEditInProgressChanged();
            }
        }

        private bool CanExecuteSendMessageCommand()
        {
            return DisplayMode == DiplomacyScreenDisplayMode.Outbox &&
                   SelectedForeignPower != null &&
                   SelectedForeignPower.OutgoingMessage != null &&
                   SelectedForeignPower.OutgoingMessage.IsEditing; //&&
                   //SelectedForeignPower.OutgoingMessage.Elements.Count != 0;
        }

        private void ExecuteSendMessageCommand()
        {
            if (!CanExecuteSendMessageCommand())
                return;

            SelectedForeignPower.OutgoingMessage.Send();
            GameLog.Client.Diplomacy.DebugFormat("Diplo Message: SEND button pressed...");
            SelectedForeignPower.OnOutgoingMessageCategoryChanged();

            OnCommandVisibilityChanged();
            OnIsMessageEditInProgressChanged();
            //Refresh();
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
            Refresh();
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


        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

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
            //_makeProposalCommand.RaiseCanExecuteChanged();
            _declareWarCommand.RaiseCanExecuteChanged();
            _endWarCommand.RaiseCanExecuteChanged();
            _openBordersCommand.RaiseCanExecuteChanged();
            _nonAgressionCommand.RaiseCanExecuteChanged();
            _affiliationCommand.RaiseCanExecuteChanged();
            _defenceAllianceCommand.RaiseCanExecuteChanged();
            _fullAllianceCommand.RaiseCanExecuteChanged();
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
        // DiplayMode
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

        //public ICommand MakeProposalCommand
        //{
        //    get { return _makeProposalCommand; }
        //}

        public ICommand DeclareWarCommand
        {
            get { return _declareWarCommand; }
        }
        public ICommand EndWarCommand
        {
            get { return _endWarCommand; }
        }
        public ICommand OpenBordersCommand
        {
            get { return _openBordersCommand; }
        }
        public ICommand NonAgressionCommand
        {
            get { return _nonAgressionCommand; }
        }
        public ICommand AffiliationCommand
        {
            get { return _affiliationCommand; }
        }
        public ICommand DefenceAllianceCommand
        {
            get { return _defenceAllianceCommand; }
        }
        public ICommand FullAllianceCommand
        {
            get { return _fullAllianceCommand; }
        }
        public ICommand MembershipCommand
        {
            get { return _membershipCommand; }
        }

        public ICommand EditMessageCommand
        {
            get { return _editMessageCommand; }
        }

        public ICommand SendMessageCommand
        {
            get
            {
                return _sendMessageCommand;
            }
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
                return _selectedForeignPower;
            }
            set
            {
                if (Equals(value, _selectedForeignPower))
                    return;

                _selectedForeignPower = value;
                OnSelectedForeignPowerChanged();
            }
        }

        protected virtual void OnSelectedForeignPowerChanged()
        {
            SelectedForeignPowerChanged.Raise(this);
            OnPropertyChanged("SelectedForeignPower");
            if (_selectedForeignPower != null)
                ExecuteMakeProposalCommand();
            OnAreOutgoingMessageCommandsVisibleChanged();
            OnAreIncomingMessageCommandsVisibleChanged();
            if (AreNewMessageCommandsVisible){}
            OnAreNewMessageCommandsVisibleChanged();
            if (IsMembershipButtonVisible){}
            OnMembershipButtonVisibleChanged();
            if (IsFullAllianceButtonVisible) {}
            OnFullAllianceButtonVisibleChanged();
            
        }

        #endregion

        #region AreOutgoingMessageCommandsVisible Property

        [field: NonSerialized]
        public event EventHandler AreOutgoingMessageCommandsVisibleChanged;

        public bool AreOutgoingMessageCommandsVisible
        {
            get
            {
                //if (DisplayMode != DiplomacyScreenDisplayMode.Outbox)
                //    return false;

                var selectedForeignPower = SelectedForeignPower;  // if one is selected in the screen

                // works, mostly not needed
                //if (selectedForeignPower != null)
                //    GameLog.Client.Diplomacy.DebugFormat("DisplayMode is Outbox, SelectedForeignPower = {0}", selectedForeignPower.Counterparty.Key);

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
                if (DisplayMode != DiplomacyScreenDisplayMode.Outbox)
                {
                    //GameLog.Core.Diplomacy.DebugFormat("DisplayMode not DiplomacyScreenDispalyMode.Inbox" );
                    return false;
                }

                var selectedForeignPower = SelectedForeignPower;

                //works, mostly not needed
                //if (selectedForeignPower != null)
                //GameLog.Core.Diplomacy.DebugFormat("DisplayMode is Inbox, SelectedForeignPower ={0}", selectedForeignPower.Counterparty.Key);

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
         
        #region IsMembershipButtonVisible Property

        [field: NonSerialized]
        public event EventHandler IsMembershipButtonVisibleChanged;
        public bool IsMembershipButtonVisible
        {
            get
            {
                var selectedForeignPower = SelectedForeignPower;
                _isMembershipButtonVisible = (selectedForeignPower != null && !selectedForeignPower.Counterparty.IsEmpire);
                return _isMembershipButtonVisible;
            }
        }
        protected virtual void OnMembershipButtonVisibleChanged()
        {
            IsMembershipButtonVisibleChanged.Raise(this);
            OnPropertyChanged("IsMembershipButtonVisible");
        }
        #endregion

        #region IsFullAllianceButtonVisible Property

        [field: NonSerialized]
        public event EventHandler IsFullAllianceButtonVisibleChanged;
        public bool IsFullAllianceButtonVisible
        {
            get
            {
                var selectedForeignPower = SelectedForeignPower;
                _isFullAllianceButtonVisible = (selectedForeignPower != null && selectedForeignPower.Counterparty.IsEmpire);
                return _isFullAllianceButtonVisible;
            }
        }
        protected virtual void OnFullAllianceButtonVisibleChanged()
        {
            IsFullAllianceButtonVisibleChanged.Raise(this);
            OnPropertyChanged("IsFullAllianceButtonVisible");
        }
        #endregion

        #region DisplayMode Property

        [field: NonSerialized]
        public event EventHandler DisplayModeChanged;

        private DiplomacyScreenDisplayMode _displayMode;

        public DiplomacyScreenDisplayMode DisplayMode
        {
            get
            {                
                //ForeignPowerShortName.
                return _displayMode;
            }
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

        //public bool IsFullAllianceButtonVisible { get; private set; }

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

            var playerEmpireId = AppContext.LocalPlayer.EmpireID; // local player
            var playerDiplomat = Diplomat.Get(playerEmpireId);

            foreach (var civ in GameContext.Current.Civilizations)
            {
                if (civ.CivID == playerEmpireId || !DiplomacyHelper.IsContactMade(playerEmpireId, civ.CivID))
                    continue;

                var foreignPower = playerDiplomat.GetForeignPower(civ);
                var foreignPowerViewModel = new ForeignPowerViewModel(foreignPower);

                _foreignPowers.Add(foreignPowerViewModel);
               // GameLog.Client.Diplomacy.DebugFormat("!!! View of local player {1} for {0}: {2} ({3}/{4})", civ.ShortName, AppContext.LocalPlayer.Empire.Name
                    //, foreignPowerViewModel.Status
                    //, foreignPowerViewModel.CounterpartyRegard
                    //, foreignPowerViewModel.CounterpartyTrust
                    //);
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
            //GameLog.Client.Diplomacy.DebugFormat("((()))DiplomacyMessageFromClauseType ClauseType param ={0}", clauseType );
            switch (clauseType)
            {
                //case ClauseType.OfferWithdrawTroops:
                //    return DiplomacyMessageElementType.OfferWithdrawTroopsClause;
                //case ClauseType.RequestWithdrawTroops:
                //    return DiplomacyMessageElementType.RequestWithdrawTroopsClause;
                //case ClauseType.OfferStopPiracy:
                //    return DiplomacyMessageElementType.OfferStopPiracyClause;
                //case ClauseType.RequestStopPiracy:
                //    return DiplomacyMessageElementType.RequestStopPiracyClause;
                //case ClauseType.OfferBreakAgreement:
                //    return DiplomacyMessageElementType.OfferBreakAgreementClause;
                //case ClauseType.RequestBreakAgreement:
                //    return DiplomacyMessageElementType.RequestBreakAgreementClause;
                case ClauseType.OfferGiveCredits:
                    return DiplomacyMessageElementType.OfferGiveCreditsClause;
                case ClauseType.RequestGiveCredits:
                    return DiplomacyMessageElementType.RequestGiveCreditsClause;
                //case ClauseType.OfferGiveResources:
                //    return DiplomacyMessageElementType.OfferGiveResourcesClause;
                //case ClauseType.RequestGiveResources:
                    //return DiplomacyMessageElementType.RequestGiveResourcesClause;
                //case ClauseType.OfferMapData:
                //    return DiplomacyMessageElementType.OfferMapDataClause;
                //case ClauseType.RequestMapData:
                //    return DiplomacyMessageElementType.RequestMapDataClause;
                //case ClauseType.OfferHonorMilitaryAgreement:
                //    return DiplomacyMessageElementType.OfferHonorMilitaryAgreementClause;
                //case ClauseType.RequestHonorMilitaryAgreement:
                //    return DiplomacyMessageElementType.RequestHonorMilitaryAgreementClause;
                //case ClauseType.OfferEndEmbargo:
                //    return DiplomacyMessageElementType.OfferEndEmbargoClause;
                //case ClauseType.RequestEndEmbargo:
                //    return DiplomacyMessageElementType.RequestEndEmbargoClause;
                case ClauseType.TreatyWarPact:
                    return DiplomacyMessageElementType.TreatyWarPact;
                case ClauseType.TreatyCeaseFire:
                    return DiplomacyMessageElementType.TreatyCeaseFireClause;
                case ClauseType.TreatyNonAggression:
                    return DiplomacyMessageElementType.TreatyNonAggressionClause;
                case ClauseType.TreatyOpenBorders:
                    return DiplomacyMessageElementType.TreatyOpenBordersClause;
                //case ClauseType.TreatyTradePact:
                //    return DiplomacyMessageElementType.TreatyTradePactClause;
                //case ClauseType.TreatyResearchPact:
                //    return DiplomacyMessageElementType.TreatyResearchPactClause;
                case ClauseType.TreatyAffiliation:
                    return DiplomacyMessageElementType.TreatyAffiliationClause;
                case ClauseType.TreatyDefensiveAlliance:
                    return DiplomacyMessageElementType.TreatyDefensiveAllianceClause;
                case ClauseType.TreatyFullAlliance:
                    return DiplomacyMessageElementType.TreatyFullAllianceClause;
                case ClauseType.TreatyMembership:
                    return DiplomacyMessageElementType.TreatyMembershipClause;
                //case ClauseType.TreatyAcceptRejectDictionary:  
                //    return DiplomacyMessageElementType.UpdateAcceptRejectDictionaryStatement;
                default:
                    throw new ArgumentOutOfRangeException("clauseType", "Unknown clause type: " + clauseType);
            }
        }

        internal static ClauseType ElementTypeToClauseType(DiplomacyMessageElementType elementType) // clicking send in Diplomatic Screen action
        {
            //GameLog.Client.Diplomacy.DebugFormat("((()))ElementToClause DiploMessageElement param ={0}", elementType);
            switch (elementType)
            {
                //case DiplomacyMessageElementType.OfferWithdrawTroopsClause:
                //    return ClauseType.OfferWithdrawTroops;
                //case DiplomacyMessageElementType.RequestWithdrawTroopsClause:
                //    return ClauseType.RequestWithdrawTroops;
                //case DiplomacyMessageElementType.OfferStopPiracyClause:
                //    return ClauseType.OfferStopPiracy;
                //case DiplomacyMessageElementType.RequestStopPiracyClause:
                //    return ClauseType.RequestStopPiracy;
                //case DiplomacyMessageElementType.OfferBreakAgreementClause:
                //    return ClauseType.OfferBreakAgreement;
                //case DiplomacyMessageElementType.RequestBreakAgreementClause:
                //    return ClauseType.RequestBreakAgreement;
                case DiplomacyMessageElementType.OfferGiveCreditsClause:
                    return ClauseType.OfferGiveCredits;
                case DiplomacyMessageElementType.RequestGiveCreditsClause:
                    return ClauseType.RequestGiveCredits;
                //case DiplomacyMessageElementType.OfferGiveResourcesClause:
                //    return ClauseType.OfferGiveResources;
                //case DiplomacyMessageElementType.RequestGiveResourcesClause:
                //    return ClauseType.RequestGiveResources;
                //case DiplomacyMessageElementType.OfferMapDataClause:
                //    return ClauseType.OfferMapData;
                //case DiplomacyMessageElementType.RequestMapDataClause:
                //    return ClauseType.RequestMapData;
                //case DiplomacyMessageElementType.OfferHonorMilitaryAgreementClause:
                //    return ClauseType.OfferHonorMilitaryAgreement;
                //case DiplomacyMessageElementType.RequestHonorMilitaryAgreementClause:
                //    return ClauseType.RequestHonorMilitaryAgreement;
                //case DiplomacyMessageElementType.OfferEndEmbargoClause:
                //    return ClauseType.OfferEndEmbargo;
                //case DiplomacyMessageElementType.RequestEndEmbargoClause:
                //    return ClauseType.RequestEndEmbargo;
                case DiplomacyMessageElementType.TreatyWarPact:
                    return ClauseType.TreatyWarPact;
                case DiplomacyMessageElementType.TreatyCeaseFireClause:
                    return ClauseType.TreatyCeaseFire;
                case DiplomacyMessageElementType.TreatyNonAggressionClause:
                    return ClauseType.TreatyNonAggression;
                case DiplomacyMessageElementType.TreatyOpenBordersClause:
                    return ClauseType.TreatyOpenBorders;
                //case DiplomacyMessageElementType.TreatyTradePactClause:
                //    return ClauseType.TreatyTradePact;
                //case DiplomacyMessageElementType.TreatyResearchPactClause:
                //    return ClauseType.TreatyResearchPact;
                case DiplomacyMessageElementType.TreatyAffiliationClause:
                    return ClauseType.TreatyAffiliation;
                case DiplomacyMessageElementType.TreatyDefensiveAllianceClause:
                    return ClauseType.TreatyDefensiveAlliance;
                case DiplomacyMessageElementType.TreatyFullAllianceClause:
                    return ClauseType.TreatyFullAlliance;
                case DiplomacyMessageElementType.TreatyMembershipClause:
                    return ClauseType.TreatyMembership;
                //case DiplomacyMessageElementType.UpdateAcceptRejectDictionaryClause:
                default:
                    return ClauseType.NoClause;
            }
        }

        internal static StatementType ElementTypeToStatementType(DiplomacyMessageElementType elementType) // see your action element as statement on Diplomatic Screen
        {
            //GameLog.Client.Diplomacy.DebugFormat("((()))ElementToStatement DiploMessageElement param ={0}", elementType);
            switch (elementType)
            {
                case DiplomacyMessageElementType.WarDeclaration:
                    return StatementType.WarDeclaration;
                case DiplomacyMessageElementType.CommendWarStatement:
                    return StatementType.CommendWar;
                //case DiplomacyMessageElementType.CommendTreatyStatement:
                //    return StatementType.CommendRelationship;
                //case DiplomacyMessageElementType.CommendAssaultStatement:
                //    return StatementType.CommendAssault;
                //case DiplomacyMessageElementType.CommendInvasionStatement:
                //    return StatementType.CommendInvasion;
                //case DiplomacyMessageElementType.CommendSabotageStatement:
                //    return StatementType.CommendSabotage;
                case DiplomacyMessageElementType.DenounceWarStatement:
                    return StatementType.DenounceWar;
                //case DiplomacyMessageElementType.DenounceTreatyStatement:
                //    return StatementType.DenounceRelationship;
                //case DiplomacyMessageElementType.DenounceAssaultStatement:
                //    return StatementType.DenounceAssault;
                //case DiplomacyMessageElementType.DenounceInvasionStatement:
                //    return StatementType.DenounceInvasion;
                //case DiplomacyMessageElementType.DenounceSabotageStatement:
                //    return StatementType.DenounceSabotage;
                default:
                    return StatementType.NoStatement;
            }
        }
    }
}