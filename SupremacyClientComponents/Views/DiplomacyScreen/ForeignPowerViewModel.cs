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

            GameLog.Client.Diplomacy.DebugFormat("foreignPower Owner = {0}", foreignPower.Owner.ShortName);

            UpdateIncomingMessage();
            UpdateActiveAgreements();
        }

        private void UpdateIncomingMessage()
        {
            GameLog.Client.Diplomacy.DebugFormat("Checking for IncomingMessage...");
            if (_foreignPower.ResponseReceived == null && _foreignPower.ProposalReceived == null)
            {
               // GameLog.Client.Diplomacy.DebugFormat("$$ _foreignPower Response and Proposal = null, no incoming message yet");
                return;
            }
            if (_foreignPower.ResponseReceived != null)
            { 
            IncomingMessage = DiplomacyMessageViewModel.FromReponse(_foreignPower.ResponseReceived);
            GameLog.Client.Diplomacy.DebugFormat("$$ Incoming Response Owner {0} CounterParty {1} Message {2}", _foreignPower.Owner.Key, _foreignPower.Counterparty.Key, IncomingMessage.ToString());
            }

            else if (_foreignPower.ProposalReceived.IncludesTreaty() == true)
            {
                IncomingMessage = DiplomacyMessageViewModel.FromProposal(_foreignPower.ProposalReceived);
                GameLog.Client.Diplomacy.DebugFormat("$$ Incoming Proposal Owner {0} CounterParty {1} Message {2}", _foreignPower.Owner.Key, _foreignPower.Counterparty.Key, IncomingMessage.ToString());
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
                GameLog.Client.Diplomacy.DebugFormat("added for sender = {1} to recipient = {2}: agrement = {0}", agreement, agreement.Sender, agreement.Recipient);
            }
        }

        public Civilization Counterparty
        {
            get
            {
                //GameLog.Client.Diplomacy.DebugFormat("_foreignPower.Counterparty = {0}", _foreignPower.Counterparty);
                return _foreignPower.Counterparty;
            }
        }

        public string CounterpartyDiplomacyReport
        {
            get
            {
                //works, but too long    GameLog.Client.Diplomacy.DebugFormat("_foreignPower.Counterparty.DiplomacyReport = {0}", _foreignPower.Counterparty.DiplomacyReport);
                return _foreignPower.Counterparty.DiplomacyReport;
            }
        }

        public Civilization Owner
        {
            get { return _foreignPower.Owner; }
        }

        public Meter CounterpartyRegard
        {
            get
            {
                //GameLog.Client.Diplomacy.DebugFormat("coutnerpartyRegard = {0}", _foreignPower.CounterpartyDiplomacyData.Regard);
                return _foreignPower.CounterpartyDiplomacyData.Regard;
            }
        }

        public Meter CounterpartyTrust
        {
            get
            {
                //GameLog.Client.Diplomacy.DebugFormat("coutnerpartyTrust = {0}", _foreignPower.CounterpartyDiplomacyData.Trust);
                return _foreignPower.CounterpartyDiplomacyData.Trust;
            }
        }

        public RegardLevel EffectiveRegard
        {
            get
            {
                // this is stuff for just shown in view
                //GameLog.Client.Diplomacy.DebugFormat("coutnerpartyEffectiveRegard ={0}", _foreignPower.CounterpartyDiplomacyData.EffectiveRegard);
                return _foreignPower.CounterpartyDiplomacyData.EffectiveRegard;
            }
        }

        public ForeignPowerStatus Status
        {
            get
            {
                // this is stuff for just shown in view
                // GameLog.Client.Diplomacy.DebugFormat("coutnerparty status ={0}", _foreignPower.CounterpartyDiplomacyData.Status);
                return _foreignPower.CounterpartyDiplomacyData.Status;
            }
        }

        public int TurnsSinceLastStatusChange
        {
            get { return GameContext.Current.TurnNumber - _foreignPower.LastStatusChange; }
        }

        public bool IsEmbargoInPlace
        {
            get { return _foreignPower.IsEmbargoInPlace; }
        }

        public bool IsDiplomatAvailable
        {
            get
            {
                GameLog.Client.Diplomacy.DebugFormat("Is Diplomat Available ={0}, false if AtWar", _foreignPower.IsDiplomatAvailable);
                return _foreignPower.IsDiplomatAvailable;
            }
        }

        public ReadOnlyObservableCollection<ActiveAgreementViewModel> ActiveAgreements
        {
            get { return _activeAgreementsView; }
        }

        #region IncomingMessage Property

        [field: NonSerialized]
        public event EventHandler IncomingMessageChanged;

        private DiplomacyMessageViewModel _incomingMessage;

        public DiplomacyMessageViewModel IncomingMessage
        {
            get
            {
                if (_incomingMessage != null && _incomingMessage.Elements.Count() > 0)
                    GameLog.Client.Diplomacy.DebugFormat("get IncomingMessage = {0}, Count = {1}", _incomingMessage, _incomingMessage.Elements.Count());

                return _incomingMessage;
            }
            set
            {
                if (Equals(value, _incomingMessage))
                    return;

                _incomingMessage = value;
                GameLog.Client.Diplomacy.DebugFormat("set _incomingMessage = {0}", _incomingMessage);

                OnIncomingMessageChanged();
            }
        }

        protected virtual void OnIncomingMessageChanged()
        {
            GameLog.Client.Diplomacy.DebugFormat("OnIncomingMessageChanged = TRUE");
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

        public DiplomaticMessageCategory IncomingMessageCategory
        {
            get
            {
                if (_foreignPower.ProposalReceived != null || _foreignPower.ResponseReceived != null || _foreignPower.StatementReceived != null|| _foreignPower.ProposalReceived != null)
                GameLog.Client.Diplomacy.DebugFormat("$$ Proposal received ? ={0}, Response received = {1}, Statement Received ={2}, Proposal Received ={3}", _foreignPower.ProposalReceived, _foreignPower.ResponseReceived, _foreignPower.StatementReceived, _foreignPower.ProposalReceived);

                return ResolveMessageCategory(_foreignPower.ProposalReceived ?? (object)_foreignPower.ResponseReceived ?? _foreignPower.StatementReceived ?? (object)_foreignPower.ProposalReceived);
            }
        }

        protected virtual void OnIncomingMessageCategoryChanged()
        {
            GameLog.Client.Diplomacy.DebugFormat("IncomingMessage category changed");
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
                GameLog.Client.Diplomacy.DebugFormat("Message: Sender ={1} *vs* Recipient = {0}", viewModel.Recipient, viewModel.Sender);
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
                    GameLog.Client.Diplomacy.DebugFormat("Message Category Demand");
                    return DiplomaticMessageCategory.Demand;
                }
                if (proposal.IsGift())
                {
                    GameLog.Client.Diplomacy.DebugFormat("Message Category Gift");
                    return DiplomaticMessageCategory.Gift;
                }
                foreach (var clause in proposal.Clauses)
                {
                    if (!clause.ClauseType.IsTreatyClause())
                        continue;
                    if (clause.ClauseType == ClauseType.TreatyWarPact)
                    {
                        GameLog.Client.Diplomacy.DebugFormat("Message Category 1 War Pact");
                        return DiplomaticMessageCategory.WarPact;
                    }
                    GameLog.Client.Diplomacy.DebugFormat("Message Category 2 Treaty");
                    return DiplomaticMessageCategory.Treaty;
                }
                GameLog.Client.Diplomacy.DebugFormat("Message Exchange");
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

        int ICivIdentity.CivID
        {
            get { return _foreignPower.CounterpartyID; }
        }

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