// File:DiplomacyScreenViewModel.cs
using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.ServiceLocation;
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
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Supremacy.Client.Views
{

    public class DiplomacyScreenViewModel : ViewModelBase<INewDiplomacyScreenView, DiplomacyScreenViewModel>
    {

        private bool _isMembershipButtonVisible;
        private bool _isFullAllianceButtonVisible;
        private readonly Dictionary<int, int> _cancelationRegardDictionary = new Dictionary<int, int> { { 999, 0 } };
        private readonly Dictionary<int, int> _cancelationTrustDictionary = new Dictionary<int, int> { { 999, 0 } };

        #region Design-Time Instance

        private static DiplomacyScreenViewModel _designInstance;

        public static DiplomacyScreenViewModel DesignInstance
        {
            get
            {

                if (_designInstance != null)
                {
                    return _designInstance;
                }

                _designInstance = new DiplomacyScreenViewModel(DesignTimeAppContext.Instance, null);

                if (_designInstance.ForeignPowers != null) // && _designInstance.ForeignPowers.Count() > 0)
                {
                    _designInstance.SelectedForeignPower = _designInstance.ForeignPowers.First();
                }

                _designInstance.DisplayMode = DiplomacyScreenDisplayMode.Outbox;

                //_designInstance.MakeProposalCommand.Execute(null);
                if (_designInstance.SelectedForeignPower != null)
                {
                    try
                    {
                        _designInstance.SelectedForeignPower.OutgoingMessage.AvailableElements.First(o => o.ActionCategory == DiplomacyMessageElementActionCategory.Propose).AddCommand.Execute(null);
                        _designInstance.SelectedForeignPower.OutgoingMessage.AvailableElements.First(o => o.ActionCategory == DiplomacyMessageElementActionCategory.Offer).AddCommand.Execute(null);
                    }
                    catch
                    {
                        GameLog.Client.Diplomacy.DebugFormat("DiplomacyScreenViewModel DesignInstance null");
                    }
                }
                return _designInstance;
            }
        }

        #endregion Design-Time Instance

        public Civilization LocalPalyer => GameContext.Current.CivilizationManagers[ServiceLocator.Current.GetInstance<IAppContext>().LocalPlayer.CivID].Civilization;
        public bool localIsHost => ServiceLocator.Current.GetInstance<IAppContext>().IsGameHost;

        private readonly ObservableCollection<ForeignPowerViewModel> _foreignPowers;

        /*
          DISPLAY MODE
         */
        private readonly DelegateCommand<ICheckableCommandParameter> _setDisplayModeCommand;

        private readonly DelegateCommand _commendCommand;
        private readonly DelegateCommand _denounceCommand;
        private readonly DelegateCommand _threatenCommand;

        private readonly DelegateCommand _declareWarCommand;
        private readonly DelegateCommand _endWarCommand;  // other naming in the code: CeaseFire
        private readonly DelegateCommand _openBordersCommand;

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
            ForeignPowers = new ReadOnlyObservableCollection<ForeignPowerViewModel>(_foreignPowers);
            // DISPLAY MODE
            _setDisplayModeCommand = new DelegateCommand<ICheckableCommandParameter>(ExecuteSetDisplayModeComand, CanExecuteSetDisplayModeComand);
            

            _commendCommand = new DelegateCommand(ExecuteCommendCommand, CanExecuteCommendCommand);
            _denounceCommand = new DelegateCommand(ExecuteDenounceCommand, CanExecuteDenounceCommand);
            _threatenCommand = new DelegateCommand(ExecuteThreatenCommand, CanExecuteThreatenCommand);

            _declareWarCommand = new DelegateCommand(ExecuteDeclareWarCommand, CanExecuteDeclareWarCommand);
            _endWarCommand = new DelegateCommand(ExecuteEndWarCommand, CanExecuteEndWarCommand);
            _openBordersCommand = new DelegateCommand(ExecuteOpenBordersCommand, CanExecuteOpenBordersCommand);

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
            {
                SelectedGraphNode = node;
            }
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
            {
                return false;
            }

            return true;
        }

        private bool CanExecuteNewProposalCommandCore(out ForeignPowerViewModel selectedForeignPower)
        {
            selectedForeignPower = SelectedForeignPower;

            if (selectedForeignPower == null)
            {
                return false;
            }

            if (!selectedForeignPower.IsDiplomatAvailable)
            {
                return false;
            }

            if (selectedForeignPower.OutgoingMessage != null)
            {
                return false;
            }

            return true;
        }

        private void ExecuteMakeProposalCommand()
        {

            if (!CanExecuteNewProposalCommandCore(out ForeignPowerViewModel foreignPower))
            {
                return;
            }

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

            if (!CanExecuteDeclareWarCommandCore(out ForeignPowerViewModel foreignPower))
            {
                return;
            }

            DisplayMode = DiplomacyScreenDisplayMode.Outbox; // new

            AreOutgoingMessageCommandsVisibleChanged.Raise(this);
            OnPropertyChanged("AreOutgoingMessageCommandsVisible");

            DiplomacyMessageViewModel message = new DiplomacyMessageViewModel(_playerCivilization, _selectedForeignPower.Counterparty);

            message.Edit();

            DiplomacyMessageAvailableElement declareWarElement = message.AvailableElements.FirstOrDefault(o => o.ElementType == DiplomacyMessageElementType.WarDeclaration);
            if (declareWarElement == null || !declareWarElement.AddCommand.CanExecute(null))
            {
                return;
            }

            declareWarElement.AddCommand.Execute(null);

            foreignPower.OutgoingMessage = message;

            InvalidateCommands();
            OnCommandVisibilityChanged();
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

            if (!CanExecuteEndWarCommandCore(out ForeignPowerViewModel foreignPower))
            {
                return;
            }

            DisplayMode = DiplomacyScreenDisplayMode.Outbox; // new

            AreOutgoingMessageCommandsVisibleChanged.Raise(this);
            OnPropertyChanged("AreOutgoingMessageCommandsVisible");

            DiplomacyMessageViewModel message = new DiplomacyMessageViewModel(_playerCivilization, _selectedForeignPower.Counterparty);

            message.Edit();

            DiplomacyMessageAvailableElement endWarElement = message.AvailableElements.FirstOrDefault(o => o.ElementType == DiplomacyMessageElementType.TreatyCeaseFireClause); // CeaseFire = endWar
            if (endWarElement == null || !endWarElement.AddCommand.CanExecute(null))
            {
                return;
            }

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

            if (!CanExecuteOpenBordersCommandCore(out ForeignPowerViewModel foreignPower))
            {
                return;
            }

            DisplayMode = DiplomacyScreenDisplayMode.Outbox; // new

            AreOutgoingMessageCommandsVisibleChanged.Raise(this);
            OnPropertyChanged("AreOutgoingMessageCommandsVisible");

            DiplomacyMessageViewModel message = new DiplomacyMessageViewModel(_playerCivilization, _selectedForeignPower.Counterparty);

            message.Edit();

            DiplomacyMessageAvailableElement openBordersElement = message.AvailableElements.FirstOrDefault(o => o.ElementType == DiplomacyMessageElementType.TreatyOpenBordersClause);
            if (openBordersElement == null || !openBordersElement.AddCommand.CanExecute(null))
            {
                return;
            }

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

            if (!CanExecuteNonAgressionCommandCore(out ForeignPowerViewModel foreignPower))
            {
                return;
            }

            DisplayMode = DiplomacyScreenDisplayMode.Outbox; // new

            AreOutgoingMessageCommandsVisibleChanged.Raise(this);
            OnPropertyChanged("AreOutgoingMessageCommandsVisible");

            DiplomacyMessageViewModel message = new DiplomacyMessageViewModel(_playerCivilization, _selectedForeignPower.Counterparty);

            message.Edit();

            DiplomacyMessageAvailableElement nonAgressionElement = message.AvailableElements.FirstOrDefault(o => o.ElementType == DiplomacyMessageElementType.TreatyNonAggressionClause);
            if (nonAgressionElement == null || !nonAgressionElement.AddCommand.CanExecute(null))
            {
                return;
            }

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

            if (!CanExecuteAffiliationCommandCore(out ForeignPowerViewModel foreignPower))
            {
                return;
            }

            DisplayMode = DiplomacyScreenDisplayMode.Outbox; // new

            AreOutgoingMessageCommandsVisibleChanged.Raise(this);
            OnPropertyChanged("AreOutgoingMessageCommandsVisible");

            DiplomacyMessageViewModel message = new DiplomacyMessageViewModel(_playerCivilization, _selectedForeignPower.Counterparty);

            message.Edit();

            DiplomacyMessageAvailableElement affiliationElement = message.AvailableElements.FirstOrDefault(o => o.ElementType == DiplomacyMessageElementType.TreatyOpenBordersClause);
            if (affiliationElement == null || !affiliationElement.AddCommand.CanExecute(null))
            {
                return;
            }

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

            if (!CanExecuteDefenceAllianceCommandCore(out ForeignPowerViewModel foreignPower))
            {
                return;
            }

            DisplayMode = DiplomacyScreenDisplayMode.Outbox; // new = DiplomacyScreenDisplayMode.Outbox; // new

            AreOutgoingMessageCommandsVisibleChanged.Raise(this);
            OnPropertyChanged("AreOutgoingMessageCommandsVisible");

            DiplomacyMessageViewModel message = new DiplomacyMessageViewModel(_playerCivilization, _selectedForeignPower.Counterparty);

            message.Edit();

            DiplomacyMessageAvailableElement defenceAllianceElement = message.AvailableElements.FirstOrDefault(o => o.ElementType == DiplomacyMessageElementType.TreatyDefensiveAllianceClause);
            if (defenceAllianceElement == null || !defenceAllianceElement.AddCommand.CanExecute(null))
            {
                return;
            }

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

            if (!CanExecuteFullAllianceCommandCore(out ForeignPowerViewModel foreignPower))
            {
                return;
            }

            DisplayMode = DiplomacyScreenDisplayMode.Outbox; // new = DiplomacyScreenDisplayMode.Outbox; // new

            AreOutgoingMessageCommandsVisibleChanged.Raise(this);
            OnPropertyChanged("AreOutgoingMessageCommandsVisible");

            DiplomacyMessageViewModel message = new DiplomacyMessageViewModel(_playerCivilization, _selectedForeignPower.Counterparty);

            message.Edit();

            DiplomacyMessageAvailableElement fullAllianceElement = message.AvailableElements.FirstOrDefault(o => o.ElementType == DiplomacyMessageElementType.TreatyFullAllianceClause);
            if (fullAllianceElement == null || !fullAllianceElement.AddCommand.CanExecute(null))
            {
                return;
            }

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

            if (!CanExecuteMembershipCommandCore(out ForeignPowerViewModel foreignPower))
            {
                return;
            }

            DisplayMode = DiplomacyScreenDisplayMode.Outbox; // new = DiplomacyScreenDisplayMode.Outbox; // new

            AreOutgoingMessageCommandsVisibleChanged.Raise(this);
            OnPropertyChanged("AreOutgoingMessageCommandsVisible");

            DiplomacyMessageViewModel message = new DiplomacyMessageViewModel(_playerCivilization, _selectedForeignPower.Counterparty);

            message.Edit();

            DiplomacyMessageAvailableElement membershipElement = message.AvailableElements.FirstOrDefault(o => o.ElementType == DiplomacyMessageElementType.TreatyMembershipClause);
            if (membershipElement == null || !membershipElement.AddCommand.CanExecute(null))
            {
                return;
            }

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
            {
                return;
            }

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
            {
                return;
            }

            SelectedForeignPower.OutgoingMessage.Send();
            GameLog.Client.DiplomacyDetails.DebugFormat("Diplo Message: SEND button pressed...");
            if (SelectedForeignPower != null && SelectedForeignPower.OutgoingMessage != null)
            {
                int _selectedID = SelectedForeignPower.Counterparty.CivID;

                foreach (DiplomacyMessageElement element in SelectedForeignPower.OutgoingMessage.StatementElements)
                {
                    //GameLog.Client.Diplomacy.DebugFormat("!@^% element = {0} ", element.Description);
                    if (element.ElementType == DiplomacyMessageElementType.WarDeclaration)
                    {
                        _ = int.TryParse(SelectedForeignPower.CounterpartyRegard.ToString(), out int regard);
                        _ = int.TryParse(SelectedForeignPower.CounterpartyTrust.ToString(), out int trust);
                        if (_cancelationRegardDictionary.ContainsKey(_selectedID))
                        {
                            _ = _cancelationRegardDictionary.Remove(_selectedID);
                            _cancelationRegardDictionary.Add(_selectedID, regard);
                        }
                        else { _cancelationRegardDictionary.Add(_selectedID, regard); }
                        if (_cancelationTrustDictionary.ContainsKey(_selectedID))
                        {
                            _ = _cancelationTrustDictionary.Remove(_selectedID);
                            _cancelationTrustDictionary.Add(_selectedID, trust);
                        }
                        else { _cancelationTrustDictionary.Add(_selectedID, trust); }

                        DiplomacyHelper.ApplyTrustChange(SelectedForeignPower.Owner, SelectedForeignPower.Counterparty, regard * -1);
                        DiplomacyHelper.ApplyRegardChange(SelectedForeignPower.Owner, SelectedForeignPower.Counterparty, trust * -1);
                    }

                }
            }

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
            {
                return;
            }

            int _selectedID = SelectedForeignPower.Counterparty.CivID;

            foreach (DiplomacyMessageElement element in SelectedForeignPower.OutgoingMessage.StatementElements)
            {
                //GameLog.Client.Diplomacy.DebugFormat("!@^% element = {0} ", element.Description);
                if (element.ElementType == DiplomacyMessageElementType.WarDeclaration)
                {
                    int? regard = _cancelationRegardDictionary[_selectedID];
                    int? trust = _cancelationTrustDictionary[_selectedID];
                    if (regard != null)
                    {
                        DiplomacyHelper.ApplyTrustChange(SelectedForeignPower.Owner, SelectedForeignPower.Counterparty, (int)regard);
                    }

                    if (trust != null)
                    {
                        DiplomacyHelper.ApplyRegardChange(SelectedForeignPower.Owner, SelectedForeignPower.Counterparty, (int)trust);
                    }
                }
            }

            SelectedForeignPower.OutgoingMessage.Cancel();
            SelectedForeignPower.OutgoingMessage = null;

            OnCommandVisibilityChanged();
            OnIsMessageEditInProgressChanged();
            Refresh();
        }

        private void ExecuteSetDisplayModeComand(ICheckableCommandParameter p)
        {
            if (!CanExecuteSetDisplayModeComand(p))
            {
                return;
            }

            DisplayMode = (DiplomacyScreenDisplayMode)p.InnerParameter;

            InvalidateCommands();
        }

        private bool CanExecuteSetDisplayModeComand(ICheckableCommandParameter p)
        {
            if (p == null)
            {
                return false;
            }

            DiplomacyScreenDisplayMode? displayMode = p.InnerParameter as DiplomacyScreenDisplayMode?;
            if (displayMode == null)
            {
                p.IsChecked = false;
                return false;
            }

            p.IsChecked = displayMode == DisplayMode;
            return true;
        }

        #endregion


        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override void UnregisterCommandAndEventHandlers()
        {
            base.UnregisterCommandAndEventHandlers();

            ClientEvents.TurnStarted.Unsubscribe(OnTurnStarted);
        }

        protected override void RegisterCommandAndEventHandlers()
        {
            base.RegisterCommandAndEventHandlers();

            _ = ClientEvents.TurnStarted.Subscribe(OnTurnStarted, ThreadOption.UIThread);
        }

        protected override void InvalidateCommands()
        {
            base.InvalidateCommands();

            _setDisplayModeCommand.RaiseCanExecuteChanged();

            _commendCommand.RaiseCanExecuteChanged();
            _denounceCommand.RaiseCanExecuteChanged();
            _threatenCommand.RaiseCanExecuteChanged();

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

            _selectedForeignPower?.InvalidateCommands();
        }

        private void OnTurnStarted(GameContextEventArgs args)
        {
            Refresh();
        }

        private void Refresh()
        {
            PlayerCivilization = ServiceLocator.Current.GetInstance<IAppContext>().LocalPlayer.Empire;

            RefreshForeignPowers();
            RefreshRelationshipGraph();
        }

        #region Overrides of ViewModelBase<INewDiplomacyScreenView,DiplomacyScreenViewModel>

        public override string ViewName => StandardGameScreens.DiplomacyScreen;

        protected internal override void RegisterViewWithRegion()
        {
            _ = RegionManager.Regions[ClientRegions.GameScreens].Add(View, ViewName, true);
        }

        protected internal override void UnregisterViewWithRegion()
        {
            RegionManager.Regions[ClientRegions.GameScreens].Remove(View);
        }

        #endregion

        public ReadOnlyObservableCollection<ForeignPowerViewModel> ForeignPowers { get; }
        // DiplayMode
        public ICommand SetDisplayModeCommand => _setDisplayModeCommand;

        public ICommand CommendCommand => _commendCommand;

        public ICommand DenounceCommand => _denounceCommand;

        public ICommand ThreatenCommand => _threatenCommand;

        //public ICommand MakeProposalCommand
        //{
        //    get { return _makeProposalCommand; }
        //}

        public ICommand DeclareWarCommand => _declareWarCommand;
        public ICommand EndWarCommand => _endWarCommand;
        public ICommand OpenBordersCommand => _openBordersCommand;
        public ICommand NonAgressionCommand => _nonAgressionCommand;
        public ICommand AffiliationCommand => _affiliationCommand;
        public ICommand DefenceAllianceCommand => _defenceAllianceCommand;
        public ICommand FullAllianceCommand => _fullAllianceCommand;
        public ICommand MembershipCommand => _membershipCommand;

        public ICommand EditMessageCommand => _editMessageCommand;

        public ICommand SendMessageCommand => _sendMessageCommand;

        public ICommand CancelMessageCommand => _cancelMessageCommand;

        public ICommand ResetGraphCommand => _resetGraphCommand;

        public ICommand SetSelectedGraphNodeCommand => _setSelectedGraphNodeCommand;

        #region PlayerCivilization Property

        [field: NonSerialized]
        public event EventHandler PlayerCivilizationChanged;

        private Civilization _playerCivilization;

        public Civilization PlayerCivilization
        {
            get => _playerCivilization;
            set
            {
                if (Equals(value, _playerCivilization))
                {
                    return;
                }

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
            get => _selectedGraphNode;
            set
            {
                if (Equals(value, _selectedGraphNode))
                {
                    return;
                }

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
            get => _localPlayerGraphNode;
            set
            {
                if (Equals(value, _localPlayerGraphNode))
                {
                    return;
                }

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
            get => _selectedForeignPower;
            set
            {
                if (Equals(value, _selectedForeignPower))
                {
                    return;
                }

                _selectedForeignPower = value;
                OnSelectedForeignPowerChanged();
            }
        }

        public ForeignPowerViewModel GetSelectedForeignPower()
        {
            return _selectedForeignPower;
        }

        public DiplomacyMessageViewModel OutgoingMessage() // new 2023-09-30
        {
            return null;
        }
        public void UpdateSelectedForeignPower()
        {
            OnSelectedForeignPowerChanged();
        }
        protected virtual void OnSelectedForeignPowerChanged()
        {
            SelectedForeignPowerChanged.Raise(this);
            OnPropertyChanged("SelectedForeignPower");
            if (_selectedForeignPower != null)
            {
                ExecuteMakeProposalCommand();
            }

            OnAreOutgoingMessageCommandsVisibleChanged();
            OnAreIncomingMessageCommandsVisibleChanged();
            if (AreNewMessageCommandsVisible) { }
            OnAreNewMessageCommandsVisibleChanged();
            if (IsMembershipButtonVisible) { }
            OnMembershipButtonVisibleChanged();
            if (IsFullAllianceButtonVisible) { }
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

                ForeignPowerViewModel selectedForeignPower = SelectedForeignPower;  // if one is selected in the screen

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

                ForeignPowerViewModel selectedForeignPower = SelectedForeignPower;

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
                ForeignPowerViewModel selectedForeignPower = SelectedForeignPower;

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
                ForeignPowerViewModel selectedForeignPower = SelectedForeignPower;
                _isMembershipButtonVisible = selectedForeignPower != null && !selectedForeignPower.Counterparty.IsEmpire;
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
                ForeignPowerViewModel selectedForeignPower = SelectedForeignPower;
                _isFullAllianceButtonVisible = selectedForeignPower != null && selectedForeignPower.Counterparty.IsEmpire;
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
            get =>
                //ForeignPowerShortName.
                _displayMode;
            set
            {
                if (Equals(value, _displayMode))
                {
                    return;
                }

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

        public bool IsMessageEditInProgress => DisplayMode == DiplomacyScreenDisplayMode.Outbox &&
                       SelectedForeignPower != null &&
                       SelectedForeignPower.OutgoingMessage != null &&
                       SelectedForeignPower.OutgoingMessage.IsEditing;

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
            Civilization selectedForeignPower = SelectedForeignPower?.Counterparty;

            SelectedForeignPower = null;

            _foreignPowers.Clear();


            int playerEmpireId = ServiceLocator.Current.GetInstance<IAppContext>().LocalPlayer.EmpireID; // local player
            Diplomat playerDiplomat = Diplomat.Get(playerEmpireId);

            foreach (Civilization civ in GameContext.Current.Civilizations)
            {

                if (civ.CivID == playerEmpireId || !DiplomacyHelper.IsContactMade(playerEmpireId, civ.CivID) || DiplomacyHelper.GetForeignPowerStatus(civ, playerDiplomat.Owner) == ForeignPowerStatus.OwnerIsSubjugated)
                {
                    continue;
                }
                //
                //Console.WriteLine("Step_9333:; RefreshForeignPowers... " + civ.Name);

                ForeignPower foreignPower = playerDiplomat.GetForeignPower(civ);
                ForeignPowerViewModel foreignPowerViewModel = new ForeignPowerViewModel(foreignPower);

                _foreignPowers.Add(foreignPowerViewModel);
                // GameLog.Client.Diplomacy.DebugFormat("!!! View of local player {1} for {0}: {2} ({3}/{4})", civ.ShortName, AppContext.LocalPlayer.Empire.Name
                //, foreignPowerViewModel.Status
                //, foreignPowerViewModel.CounterpartyRegard
                //, foreignPowerViewModel.CounterpartyTrust
                //);
            }

            if (selectedForeignPower != null)
            {
                SelectedForeignPower = _foreignPowers.FirstOrDefault(o => o.Counterparty.CivID == selectedForeignPower.CivID);
            }
        }

        private void RefreshRelationshipGraph()
        {
            int count = GameContext.Current.Civilizations.Count;
            List<DiplomacyGraphNode> nodes = new List<DiplomacyGraphNode>(count);
            Civilization localPlayerEmpire = ServiceLocator.Current.GetInstance<IAppContext>().LocalPlayer.Empire;

            DiplomacyGraphNode localPlayerNode = null;

            foreach (Civilization civ in GameContext.Current.Civilizations)
            {
                DiplomacyGraphNode node = new DiplomacyGraphNode(civ, _setSelectedGraphNodeCommand);

                nodes.Add(node);

                if (civ == localPlayerEmpire)
                {
                    localPlayerNode = node;
                }
            }

            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count; j++)
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