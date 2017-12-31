using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;

using Supremacy.Annotations;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Personnel;
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

            UpdateIncomingMessage();
            UpdateActiveAgreements();
        }

        private void UpdateIncomingMessage()
        {
            if (_foreignPower.ResponseReceived == null)
                return;

            IncomingMessage = DiplomacyMessageViewModel.FromReponse(_foreignPower.ResponseReceived);
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
            get { return _foreignPower.Counterparty; }
        }

        public Civilization Owner
        {
            get { return _foreignPower.Owner; }
        }

        public Agent AssignedEnvoy
        {
            get { return _foreignPower.AssignedEnvoy; }
        }

        public Agent CounterpartyEnvoy
        {
            get { return _foreignPower.CounterpartyEnvoy; }
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

                OnIncomingMessageChanged();
            }
        }

        protected virtual void OnIncomingMessageChanged()
        {
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
                message = viewModel.CreateMessage();

            var proposal = message as IProposal;
            if (proposal != null)
            {
                if (proposal.IsDemand())
                    return DiplomaticMessageCategory.Demand;
                if (proposal.IsGift())
                    return DiplomaticMessageCategory.Gift;

                foreach (var clause in proposal.Clauses)
                {
                    if (!clause.ClauseType.IsTreatyClause())
                        continue;
                    if (clause.ClauseType == ClauseType.TreatyWarPact)
                        return DiplomaticMessageCategory.WarPact;
                    return DiplomaticMessageCategory.Treaty;
                }

                return DiplomaticMessageCategory.Exchange;
            }

            var response = message as IResponse;
            if (response != null)
                return DiplomaticMessageCategory.Response;

            var statement = message as Statement;
            if (statement != null)
            {
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
                        return DiplomaticMessageCategory.Statement;
                    
                    case StatementType.ThreatenDestroyColony:
                    case StatementType.ThreatenTradeEmbargo:
                    case StatementType.ThreatenDeclareWar:
                        return DiplomaticMessageCategory.Threat;
                }
            }

            return DiplomaticMessageCategory.None;
        }

        #region Implementation of ICivIdentity

        GameObjectID ICivIdentity.CivID
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

        public void RefreshEnvoys()
        {
            OnPropertyChanged("AssignedEnvoy");
            OnPropertyChanged("CounterpartyEnvoy");
        }
    }
}