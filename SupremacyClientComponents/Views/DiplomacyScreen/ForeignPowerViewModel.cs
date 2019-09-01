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
                throw new ArgumentNullException("foreignPower");

            _foreignPower = foreignPower;
            _activeAgreements = new ObservableCollection<ActiveAgreementViewModel>();
            _activeAgreementsView = new ReadOnlyObservableCollection<ActiveAgreementViewModel>(_activeAgreements);

            GameLog.Core.Diplomacy.DebugFormat("foreignPower = {0}", foreignPower.Owner);

            UpdateIncomingMessage();
            UpdateActiveAgreements();
        }

        private void UpdateIncomingMessage()
        {
            GameLog.Core.Diplomacy.DebugFormat("IncomingMessage ...beginning");
            if (_foreignPower.ResponseReceived == null) // && _foreignPower.ProposalReceived == null)
            {          
                GameLog.Core.Diplomacy.DebugFormat("_foreignPower.ResponseReceived or proposal = null");
                return;
            }
            //if (_foreignPower.ResponseReceived == null)
            //{
            //    GameLog.Core.Diplomacy.DebugFormat("IncomingMessage proposal to={0} from={1}", _foreignPower.ProposalReceived.Recipient, _foreignPower.ProposalReceived.Sender);
            //    return;
            //}   
                //GameLog.Core.Diplomacy.DebugFormat("IncomingMessage Response to={0} from={1}", _foreignPower.ResponseReceived.Recipient, _foreignPower.ResponseReceived.Sender);
                IncomingMessage = DiplomacyMessageViewModel.FromReponse(_foreignPower.ResponseReceived);
                GameLog.Core.Diplomacy.DebugFormat("IncomingMessage ={0}", IncomingMessage.Elements.ToString());
        }

        private void UpdateActiveAgreements()
        {
            var agreements = GameContext.Current.AgreementMatrix[_foreignPower.OwnerID, _foreignPower.CounterpartyID];
            if (agreements == null)
                return;

            foreach (var agreement in agreements.OrderByDescending(o => (int)o.StartTurn))
                _activeAgreements.Add(new ActiveAgreementViewModel(agreement));
        }

        public Civilization Counterparty
        {
            get
            {
                GameLog.Core.Diplomacy.DebugFormat("_foreignPower.Counterparty = {0}", _foreignPower.Counterparty);
                return _foreignPower.Counterparty;
            }
        }

        public string CounterpartyDiplomacyReport
        {
            get
            {
                // just "We are the Borg"     GameLog.Core.Diplomacy.DebugFormat("_foreignPower.Counterparty.DiplomacyReport = {0}", _foreignPower.Counterparty.DiplomacyReport);
                return _foreignPower.Counterparty.DiplomacyReport;
            }
        }

        public Civilization Owner
        {
            get { return _foreignPower.Owner; }
        }

        public Meter CounterpartyRegard
        {
            get { return _foreignPower.CounterpartyDiplomacyData.Regard; }
        }

        public Meter CounterpartyTrust
        {
            get { return _foreignPower.CounterpartyDiplomacyData.Trust; }
        }

        public RegardLevel EffectiveRegard
        {
            get { return _foreignPower.CounterpartyDiplomacyData.EffectiveRegard; }
        }

        public ForeignPowerStatus Status
        {
            get { return _foreignPower.CounterpartyDiplomacyData.Status; }
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
            get { return _foreignPower.IsDiplomatAvailable; }
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
            get { return _incomingMessage; }
            set
            {
                if (Equals(value, _incomingMessage))
                    return;

                _incomingMessage = value;
                GameLog.Core.Diplomacy.DebugFormat("_incomingMessage = {0}", _incomingMessage);

                OnIncomingMessageChanged();
            }
        }

        protected virtual void OnIncomingMessageChanged()
        {
            GameLog.Core.Diplomacy.DebugFormat("OnIncomingMessageChanged = TRUE");
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
            get { return _outgoingMessage; }
            set
            {
                if (Equals(value, _outgoingMessage))
                    return;

                _outgoingMessage = value;

                OnOutgoingMessageChanged();
            }
        }

        protected virtual void OnOutgoingMessageChanged()
        {
            OutgoingMessageChanged.Raise(this);
            OnPropertyChanged("OutgoingMessage");
            OnOutgoingMessageCategoryChanged();
        }

        #endregion

        #region IncomingMessageCategory Property

        [field: NonSerialized]
        public event EventHandler IncomingMessageCategoryChanged;

        public DiplomaticMessageCategory IncomingMessageCategory
        {
            get { return ResolveMessageCategory(_foreignPower.ProposalReceived ?? (object)_foreignPower.ResponseReceived ?? _foreignPower.StatementReceived); }
        }

        protected virtual void OnIncomingMessageCategoryChanged()
        {
            GameLog.Core.Diplomacy.DebugFormat("IncomingMessage ={0}", IncomingMessage.Elements.ToString());
            IncomingMessageCategoryChanged.Raise(this);
            OnPropertyChanged("IncomingMessageCategory");
        }

        #endregion

        #region OutgoingMessageCategory Property

        [field: NonSerialized]
        public event EventHandler OutgoingMessageCategoryChanged;

        public DiplomaticMessageCategory OutgoingMessageCategory
        {
            get { return ResolveMessageCategory(OutgoingMessage); }
        }

        protected internal virtual void OnOutgoingMessageCategoryChanged()
        {
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
            _propertyChanged.Raise(this, propertyName);
        }

        #endregion

        internal static DiplomaticMessageCategory ResolveMessageCategory(object message)
        {
            var viewModel = message as DiplomacyMessageViewModel;
            if (viewModel != null)
            {
                message = viewModel.CreateMessage();
                GameLog.Core.Diplomacy.DebugFormat("Message Recipient ={0} Sender ={1}", viewModel.Recipient, viewModel.Sender);
            }

            var proposal = message as IProposal;
            if (proposal != null)
            {
                GameLog.Core.Diplomacy.DebugFormat("Proposal Recipient ={0} Sender ={1}", proposal.Recipient, proposal.Sender);
                if (proposal.IsDemand())
                {
                    GameLog.Core.Diplomacy.DebugFormat("Message Categroy Demand");
                    return DiplomaticMessageCategory.Demand;
                }
                if (proposal.IsGift())
                {
                    GameLog.Core.Diplomacy.DebugFormat("Message Categroy Gift");
                    return DiplomaticMessageCategory.Gift;
                }
                foreach (var clause in proposal.Clauses)
                {
                    if (!clause.ClauseType.IsTreatyClause())
                        continue;
                    if (clause.ClauseType == ClauseType.TreatyWarPact)
                    {
                        GameLog.Core.Diplomacy.DebugFormat("Message Categroy 1 War Pact");
                        return DiplomaticMessageCategory.WarPact;
                    }
                    GameLog.Core.Diplomacy.DebugFormat("Message Categroy 2 Treaty");
                    return DiplomaticMessageCategory.Treaty;
                }
                GameLog.Core.Diplomacy.DebugFormat("Message Exchange");
                return DiplomaticMessageCategory.Exchange;
            }

            var response = message as IResponse;
            
            if (response != null)
            {
                GameLog.Core.Diplomacy.DebugFormat("Response Recipient ={0} Sender ={1}", response.Recipient, response.Sender);
                return DiplomaticMessageCategory.Response;
            }

            var statement = message as Statement;
            
            if (statement != null)
            {
                GameLog.Core.Diplomacy.DebugFormat("Statement Recipient ={0} Sender ={1}", statement.Recipient, statement.Sender);
                switch (statement.StatementType)
                {
                    case StatementType.CommendRelationship:
                    case StatementType.CommendAssault:
                    case StatementType.CommendInvasion:
                    case StatementType.CommendSabotage:
                    case StatementType.DenounceWar:
                    case StatementType.DenounceRelationship:
                    case StatementType.DenounceAssault:
                    case StatementType.DenounceInvasion:
                    case StatementType.DenounceSabotage:
                        GameLog.Core.Diplomacy.DebugFormat("Message Statement");
                        return DiplomaticMessageCategory.Statement;
                    
                    case StatementType.ThreatenDestroyColony:
                    case StatementType.ThreatenTradeEmbargo:
                    case StatementType.ThreatenDeclareWar:
                        GameLog.Core.Diplomacy.DebugFormat("Message Threat");
                        return DiplomaticMessageCategory.Threat;
                }
            }
            GameLog.Core.Diplomacy.DebugFormat("Message None");
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