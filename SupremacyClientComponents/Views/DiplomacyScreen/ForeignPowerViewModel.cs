//File:ForeignPowerViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;

using Supremacy.Annotations;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Types;
using Supremacy.Utility;

using System.Linq;

namespace Supremacy.Client.Views
{
    public class ForeignPowerViewModel : INotifyPropertyChanged, ICivIdentity
    {
        private readonly ForeignPower _foreignPower;
        private readonly ObservableCollection<ActiveAgreementViewModel> _activeAgreements;
        private readonly ReadOnlyObservableCollection<ActiveAgreementViewModel> _activeAgreementsView;
        public ForeignPowerViewModel([NotNull] ForeignPower foreignPower)
         {
            if (foreignPower == null)
            {
                return;
                throw new ArgumentNullException("foreignPower");
            }

            _foreignPower = foreignPower;
            _activeAgreements = new ObservableCollection<ActiveAgreementViewModel>();
            _activeAgreementsView = new ReadOnlyObservableCollection<ActiveAgreementViewModel>(_activeAgreements);

            //GameLog.Client.Diplomacy.DebugFormat("foreignPower Owner = {0}", foreignPower.Owner.ShortName);
            
            UpdateIncomingMessage();
            UpdateActiveAgreements();
        }

        private void UpdateIncomingMessage()
        {
            //GameLog.Client.Diplomacy.DebugFormat("Checking for IncomingMessage...");
            if (_foreignPower.ResponseReceived == null && _foreignPower.ProposalReceived == null)
            {
                // GameLog.Client.Diplomacy.DebugFormat("$$ _foreignPower Response and Proposal = null, no incoming message yet");
                return;
            }
            if (_foreignPower.ResponseReceived != null)
            {
                IncomingMessage = DiplomacyMessageViewModel.FromReponse(_foreignPower.ResponseReceived);
                GameLog.Client.Diplomacy.DebugFormat("$$ Incoming Response Owner ={0} CounterParty ={1} Message Treaty Leadin text ={2}"
                    , _foreignPower.Owner.Key
                    , _foreignPower.Counterparty.Key
                    , IncomingMessage.TreatyLeadInText);
            }

            else if (_foreignPower.ProposalReceived.IncludesTreaty() == true)
            {
                IncomingMessage = DiplomacyMessageViewModel.FromProposal(_foreignPower.ProposalReceived);
                GameLog.Client.Diplomacy.DebugFormat("$$ Incoming Proposal Owner ={0} CounterParty ={1} message Treaty Leadin text ={2}"
                    , _foreignPower.Owner.Key
                    , _foreignPower.Counterparty.Key
                    , IncomingMessage.TreatyLeadInText);
                DiplomacyHelper.DiploScreenSelectedForeignPower = GameContext.Current.CivilizationManagers[_foreignPower.Counterparty.CivID].Civilization;
            }
        }



        private void UpdateActiveAgreements()
        {
            var agreements = GameContext.Current.AgreementMatrix[_foreignPower.OwnerID, _foreignPower.CounterpartyID];
            if (agreements == null)
                return;

            foreach (var agreement in agreements.OrderByDescending(o => (int)o.StartTurn))
            {
                _activeAgreements.Add(new ActiveAgreementViewModel(agreement));
                GameLog.Client.Diplomacy.DebugFormat(Environment.NewLine + "                                                      Turn {3};added;sender=; {1};recipient=; {2};agreement=;{0}"
                    , agreement.Proposal.Clauses[0].ClauseType.ToString()
                    , agreement.Sender
                    , agreement.Recipient
                    , GameContext.Current.TurnNumber);
            }
        }

        public Civilization Counterparty =>
                //GameLog.Client.Diplomacy.DebugFormat("_foreignPower.Counterparty = {0}", _foreignPower.Counterparty);
                _foreignPower.Counterparty;

        public string CounterpartyDiplomacyReport =>
                //works, but too long    GameLog.Client.Diplomacy.DebugFormat("_foreignPower.Counterparty.DiplomacyReport = {0}", _foreignPower.Counterparty.DiplomacyReport);
                _foreignPower.Counterparty.DiplomacyReport;

        public Civilization Owner => _foreignPower.Owner;

        public Meter CounterpartyRegard =>
                //GameLog.Client.Diplomacy.DebugFormat("coutnerpartyRegard = {0}", _foreignPower.CounterpartyDiplomacyData.Regard);
                _foreignPower.CounterpartyDiplomacyData.Regard;

        public Meter CounterpartyTrust =>
                //GameLog.Client.Diplomacy.DebugFormat("coutnerpartyTrust = {0}", _foreignPower.CounterpartyDiplomacyData.Trust);
                _foreignPower.CounterpartyDiplomacyData.Trust;

        public RegardLevel EffectiveRegard =>
                // this is stuff for just shown in view
                //GameLog.Client.Diplomacy.DebugFormat("coutnerpartyEffectiveRegard ={0}", _foreignPower.CounterpartyDiplomacyData.EffectiveRegard);
                _foreignPower.CounterpartyDiplomacyData.EffectiveRegard;

        public ForeignPowerStatus Status =>
                // this is stuff for just shown in view
                // GameLog.Client.Diplomacy.DebugFormat("coutnerparty status ={0}", _foreignPower.CounterpartyDiplomacyData.Status);
                _foreignPower.CounterpartyDiplomacyData.Status;

        public int TurnsSinceLastStatusChange => GameContext.Current.TurnNumber - _foreignPower.LastStatusChange;

        public bool IsEmbargoInPlace => _foreignPower.IsEmbargoInPlace;

        public bool IsDiplomatAvailable =>
                //GameLog.Client.Diplomacy.DebugFormat("Is Diplomat Available ={0}, false if AtWar", _foreignPower.IsDiplomatAvailable);
                _foreignPower.IsDiplomatAvailable;

        public ReadOnlyObservableCollection<ActiveAgreementViewModel> ActiveAgreements => _activeAgreementsView;

        #region IncomingMessage Property

        [field: NonSerialized]
        public event EventHandler IncomingMessageChanged;

        private DiplomacyMessageViewModel _incomingMessage;

        public DiplomacyMessageViewModel IncomingMessage
        {
            get
            {
                if (_incomingMessage != null && _incomingMessage.Elements.Count() > 0)
                    GameLog.Client.Diplomacy.DebugFormat("get IncomingMessage = {0}, Count = {1}", _incomingMessage.TreatyLeadInText, _incomingMessage.Elements.Count());

                return _incomingMessage;
            }
            set
            {
                if (Equals(value, _incomingMessage))
                    return;

                _incomingMessage = value;
                GameLog.Client.Diplomacy.DebugFormat("set _incomingMessage = {0}", _incomingMessage.TreatyLeadInText);

                OnIncomingMessageChanged();
            }
        }

        protected virtual void OnIncomingMessageChanged()
        {
            //GameLog.Client.Diplomacy.DebugFormat("OnIncomingMessageChanged = TRUE");
            IncomingMessageChanged.Raise(this);
            OnPropertyChanged("IncomingMessage");
            OnIncomingMessageCategoryChanged();
        }

        #endregion

        #region OutgoingMessage Property

        [field: NonSerialized]
        public event EventHandler OutgoingMessageChanged;

        private DiplomacyMessageViewModel _outgoingMessage;

        public DiplomacyMessageViewModel OutgoingMessage
        {
            get
            {
                // Gamelog for GET _outgoingMessage mostly not needed
                //if (_outgoingMessage != null && _outgoingMessage.Elements.Count() > 0)
                //    GameLog.Client.Diplomacy.DebugFormat("OutgoingMessage GET = {0} >> {1}, CountElem.={2}",
                //            _outgoingMessage.Sender.Name, _outgoingMessage.Recipient.Name, _outgoingMessage.Elements.Count().ToString());
                return _outgoingMessage;
            }
            set
            {
                if (Equals(value, _outgoingMessage))
                    return;

                _outgoingMessage = value;

                OnOutgoingMessageChanged();

                string _gamelogPart2 = "";

                if (_outgoingMessage != null && _outgoingMessage.Elements.Count() > 0)
                {
                    for (int i = 0; i < _outgoingMessage.Elements.Count(); i++)
                    {
                        GameLog.Client.Diplomacy.DebugFormat(
                            "OutgoingMessage SET = {0} to {1}, count{2}, {3} = {4} {5}",
                            _outgoingMessage.Sender.Name, _outgoingMessage.Recipient.Name
                            , _outgoingMessage.Elements.Count().ToString()
                            , _outgoingMessage.Elements[i].ActionCategory.ToString()
                            , _outgoingMessage.Elements[i].Description.ToString()
                            , _gamelogPart2
                        );
                    }
                }
            }
        }

        protected virtual void OnOutgoingMessageChanged()
        {
            //GameLog.Client.Diplomacy.DebugFormat("Now at OnOutgoingMessageChanged() - call OnPropertyChanged");
            OutgoingMessageChanged.Raise(this);
            OnPropertyChanged("OutgoingMessage");
            OnOutgoingMessageCategoryChanged();
        }

        #endregion

        public int MemberMessage
        {
            get
            {
                if (Status == ForeignPowerStatus.OwnerIsMember)
                    return 1;
                else return 0;
            }
        }
        public int AtWarMessage
        {
            get
            {
                if (Status == ForeignPowerStatus.AtWar)
                    return 1;
                else return 0;
            }
        }

        public int AlliesMessage
        {
            get
            {
                if (Status == ForeignPowerStatus.Allied)
                    return 1;
                else return 0;
            }
        }
        public int AffiliatedMessage
        {
            get
            {
                if (Status == ForeignPowerStatus.Affiliated)
                    return 1;
                else return 0;
            }
        }

        public int SelectForeignPowerMessage
        {
            get
            {
                if (_foreignPower == null)
                    return 1;
                else return 0;
            }
        }

        public int NoOutgoingMessage
        {
            get
            {
                if (OutgoingMessage == null)
                    return 1;
                else return 0;
            }
        }

        #region IncomingMessageCategory Property

        [field: NonSerialized]
        public event EventHandler IncomingMessageCategoryChanged;

        public DiplomaticMessageCategory IncomingMessageCategory =>
                //if (_foreignPower.ProposalReceived != null || _foreignPower.ResponseReceived != null || _foreignPower.StatementReceived != null)

                //GameLog.Client.Diplomacy.DebugFormat("$$ Proposal received ? ={0}, Response received = {1}, Statement Received ={2}, Proposal Received ={3}"
                //    , _foreignPower.ProposalReceived.Clauses[0].ClauseType.ToString()
                //    ,_foreignPower.ResponseReceived.ResponseType.ToString()
                //    , _foreignPower.StatementReceived.StatementType.ToString()
                //    , _foreignPower.ProposalReceived);

                ResolveMessageCategory(_foreignPower.ProposalReceived ?? (object)_foreignPower.ResponseReceived ?? _foreignPower.StatementReceived ?? (object)_foreignPower.ProposalReceived);

        protected virtual void OnIncomingMessageCategoryChanged()
        {
            //GameLog.Client.Diplomacy.DebugFormat("IncomingMessage category changed");
            IncomingMessageCategoryChanged.Raise(this);
            OnPropertyChanged("IncomingMessageCategory");
        }
        #endregion

        #region OutgoingMessageCategory Property

        [field: NonSerialized]
        public event EventHandler OutgoingMessageCategoryChanged;

        public DiplomaticMessageCategory OutgoingMessageCategory
        {
            get
            {
                try
                {
                    if (OutgoingMessage != null)
                    {
                       GameLog.Client.Diplomacy.DebugFormat("##### ........ OutgoingMessageCategory = {0} Sender ={1} first statement element ={2} latst ={3}", ResolveMessageCategory(OutgoingMessage).ToString(),
                         OutgoingMessage.Sender.ShortName, OutgoingMessage.StatementElements.FirstOrDefault(), OutgoingMessage.StatementElements.LastOrDefault());
                    }
                }
                catch { GameLog.Client.Diplomacy.DebugFormat("Unable to get outgoing message to reslove catagory"); }
                if (ResolveMessageCategory(OutgoingMessage).ToString() != "None")
                    GameLog.Client.Diplomacy.DebugFormat("OutgoingMessageCategory = {0}", ResolveMessageCategory(OutgoingMessage));
                return ResolveMessageCategory(OutgoingMessage);
            }
        }

        protected internal virtual void OnOutgoingMessageCategoryChanged()
        {
            //GameLog.Client.Diplomacy.DebugFormat("Message Category Changed");
            OutgoingMessageCategoryChanged.Raise(this);
            OnPropertyChanged("OutgoingMessageCategory");
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
            //GameLog.Client.Diplomacy.DebugFormat("propertyName ={0}", propertyName);
            _propertyChanged.Raise(this, propertyName);
        }

        #endregion

        internal static DiplomaticMessageCategory ResolveMessageCategory(object message) // DiplomaticMessageCategory is enum of 1 to 9 message types
        {
            //GameLog.Client.Diplomacy.DebugFormat("ResolveMessageCategory beginning...");

            var viewModel = message as DiplomacyMessageViewModel;

            var proposal = message as IProposal;

            if (viewModel != null)
            {
                // works
                //GameLog.Client.Diplomacy.DebugFormat("Message: Sender ={1} *vs* Recipient = {0}", viewModel.Recipient, viewModel.Sender);
                //GameLog.Client.Diplomacy.DebugFormat("Message: Sender ={1} *vs* Recipient = {0} - Category {2}", viewModel.Recipient, viewModel.Sender, proposal.ToString());
                message = viewModel.CreateMessage(); // create statment vs create proposal
            }

            //GameLog.Client.Diplomacy.DebugFormat("proposal ={0}", proposal);
            if (proposal != null)
            {
                //GameLog.Client.Diplomacy.DebugFormat("Proposal: Sender ={1} *vs* Recipient = {0} ", proposal.Recipient, proposal.Sender);
                //GameLog.Client.Diplomacy.DebugFormat("Proposal: Sender ={1} *vs* Recipient = {0} - Category {2}", proposal.Recipient, proposal.Sender, proposal.ToString());
                if (proposal.IsDemand())
                {
                    //GameLog.Client.Diplomacy.DebugFormat("Message Category Demand");
                    return DiplomaticMessageCategory.Demand;
                }
                if (proposal.IsGift())
                {
                    //GameLog.Client.Diplomacy.DebugFormat("Message Category Gift");
                    return DiplomaticMessageCategory.Gift;
                }
                foreach (var clause in proposal.Clauses)
                {
                    if (!clause.ClauseType.IsTreatyClause())
                        continue;
                    if (clause.ClauseType == ClauseType.TreatyWarPact)
                    {
                        //GameLog.Client.Diplomacy.DebugFormat("Message Category 1 War Pact");
                        return DiplomaticMessageCategory.WarPact;
                    }
                    //GameLog.Client.Diplomacy.DebugFormat("Message Category 2 Treaty");
                    return DiplomaticMessageCategory.Treaty;
                }
                //GameLog.Client.Diplomacy.DebugFormat("Message Exchange");
                return DiplomaticMessageCategory.Exchange;
            }

            var response = message as IResponse;
            
            if (response != null)
            {
                //GameLog.Client.Diplomacy.DebugFormat("Response Recipient ={0} Sender ={1}", response.Recipient, response.Sender);
                return DiplomaticMessageCategory.Response;
            }

            Statement statement = message as Statement;
            
            if (statement != null)
            {
                //GameLog.Client.Diplomacy.DebugFormat("Statement Recipient ={0} Sender ={1}", statement.Recipient, statement.Sender);
                switch (statement.StatementType)
                {
                    //case StatementType.CommendRelationship:
                    //case StatementType.CommendAssault:
                    //case StatementType.CommendInvasion:
                    //case StatementType.CommendSabotage:
                    case StatementType.DenounceWar:
                    //case StatementType.DenounceRelationship:
                    //case StatementType.DenounceAssault:
                    //case StatementType.DenounceInvasion:
                    //case StatementType.DenounceSabotage:
                    case StatementType.SabotageFood:
                    case StatementType.SabotageIndustry:
                    case StatementType.SabotageEnergy:
                    case StatementType.StealCredits:
                    case StatementType.StealResearch:
                    case StatementType.F01:
                    case StatementType.F02:
                    case StatementType.F03:
                    case StatementType.F04:
                    case StatementType.F05:
                    case StatementType.F10:
                    case StatementType.F12:
                    case StatementType.F13:
                    case StatementType.F14:
                    case StatementType.F15:
                    case StatementType.F20:
                    case StatementType.F21:
                    case StatementType.F23:
                    case StatementType.F24:
                    case StatementType.F25:
                    case StatementType.F30:
                    case StatementType.F31:
                    case StatementType.F32:
                    case StatementType.F34:
                    case StatementType.F35:
                    case StatementType.F40:
                    case StatementType.F41:
                    case StatementType.F42:
                    case StatementType.F43:
                    case StatementType.F45:
                    case StatementType.F50:
                    case StatementType.F51:
                    case StatementType.F52:
                    case StatementType.F53:
                    case StatementType.F54:
                    case StatementType.T01:
                    case StatementType.T02:
                    case StatementType.T03:
                    case StatementType.T04:
                    case StatementType.T05:
                    case StatementType.T10:
                    case StatementType.T12:
                    case StatementType.T13:
                    case StatementType.T14:
                    case StatementType.T15:
                    case StatementType.T20:
                    case StatementType.T21:
                    case StatementType.T23:
                    case StatementType.T24:
                    case StatementType.T25:
                    case StatementType.T30:
                    case StatementType.T31:
                    case StatementType.T32:
                    case StatementType.T34:
                    case StatementType.T35:
                    case StatementType.T40:
                    case StatementType.T41:
                    case StatementType.T42:
                    case StatementType.T43:
                    case StatementType.T45:
                    case StatementType.T50:
                    case StatementType.T51:
                    case StatementType.T52:
                    case StatementType.T53:
                    case StatementType.T54:
                        //GameLog.Client.Diplomacy.DebugFormat("Message Statement");
                        return DiplomaticMessageCategory.Statement;
                    
                    //case StatementType.ThreatenDestroyColony:
                    //case StatementType.ThreatenTradeEmbargo:
                    //case StatementType.ThreatenDeclareWar:
                    //    GameLog.Client.Diplomacy.DebugFormat("Message Threat");
                        //return DiplomaticMessageCategory.Threat;
                }
            }
            // a lot of times hitted without giving an info ... GameLog.Client.Diplomacy.DebugFormat("Message Category None");
            return DiplomaticMessageCategory.None;
        }

        #region Implementation of ICivIdentity

        int ICivIdentity.CivID => _foreignPower.CounterpartyID;

        #endregion

        public void InvalidateCommands()
        {
            if (_outgoingMessage != null)
                _outgoingMessage.InvalidateCommands();

            if (_incomingMessage != null)
                _incomingMessage.InvalidateCommands();
        }
    }
}