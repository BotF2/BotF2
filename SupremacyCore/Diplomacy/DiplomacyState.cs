// DiplomacyState.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

using Supremacy.Utility;

namespace Supremacy.Diplomacy
{
    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://schemas.startreksupremacy.com/DiplomacyStates.xsd")]
    [XmlRoot(Namespace = "http://schemas.startreksupremacy.com/DiplomacyStates.xsd", IsNullable = false)]
    public class DiplomacyStates : INotifyPropertyChanged
    {
        #region Fields

        private DiplomacyState[] _diplomacyState;

        #endregion

        #region Properties and Indexers

        [XmlElement("DiplomacyState")]
        public DiplomacyState[] States
        {
            get { return _diplomacyState; }
            set
            {
                _diplomacyState = value;
                RaisePropertyChanged("States");
            }
        }

        #endregion

        #region Methods

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged.Raise(this, propertyName);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://schemas.startreksupremacy.com/DiplomacyStates.xsd")]
    public class DiplomacyState : INotifyPropertyChanged
    {
        #region Fields

        private readonly DiplomacyState _parentState;
        private int? _attackCiviliansRegardCost;
        private int? _attackCiviliansTrustCost;
        private RegardDecay _creditsRegardDecay;
        private RegardDecay _diplomacyRegardDecay;
        private int? _followThroughTrustBonus;
        private int? _hasPactTrustBonus;
        private int? _holdReceptionRegardBonus;
        private int? _incursionOfAllyRegardCost;
        private int? _incursionRegardCost;
        private int? _invadeColonyRegardCost;
        private int? _invaderMovementRegardCost;
        private string _key;
        private RegardDecay _knowledgeRegardDecay;
        private RegardDecay _militaryPowerRegardDecay;
        private RegardDecay _militarySafetyRegardDecay;
        private int? _noWarTrustBonus;
        private RegardDecay _productionRegardDecay;
        private ProposalClauseState[] _proposalClauseState;
        private int? _raidingRegardCost;
        private int? _threatProbability;
        private int? _tradeEmbargoRegardCost;
        private int? _unprovokedAttackRegardCost;
        private int? _unprovokedAttackTrustCost;
        private int? _wantMapTurns;

        #endregion

        #region Constructors

        public DiplomacyState() {}

        private DiplomacyState(DiplomacyState parentState)
        {
            _parentState = parentState;
        }

        #endregion

        #region Properties and Indexers

        public int? ThreatProbability
        {
            get
            {
                if (_threatProbability.HasValue)
                    return _threatProbability.Value;
                if (_parentState != null)
                    return _parentState._threatProbability;
                return null;
            }
            set
            {
                _threatProbability = value;
                RaisePropertyChanged("ThreatProbability");
            }
        }

        public RegardDecay MilitaryPowerRegardDecay
        {
            get { return _militaryPowerRegardDecay; }
            set
            {
                _militaryPowerRegardDecay = value;
                RaisePropertyChanged("MilitaryPowerRegardDecay");
            }
        }

        public RegardDecay MilitarySafetyRegardDecay
        {
            get { return _militarySafetyRegardDecay; }
            set
            {
                _militarySafetyRegardDecay = value;
                RaisePropertyChanged("MilitarySafetyRegardDecay");
            }
        }

        public RegardDecay DiplomacyRegardDecay
        {
            get { return _diplomacyRegardDecay; }
            set
            {
                _diplomacyRegardDecay = value;
                RaisePropertyChanged("DiplomacyRegardDecay");
            }
        }

        public RegardDecay CreditsRegardDecay
        {
            get { return _creditsRegardDecay; }
            set
            {
                _creditsRegardDecay = value;
                RaisePropertyChanged("CreditsRegardDecay");
            }
        }

        public RegardDecay KnowledgeRegardDecay
        {
            get { return _knowledgeRegardDecay; }
            set
            {
                _knowledgeRegardDecay = value;
                RaisePropertyChanged("KnowledgeRegardDecay");
            }
        }

        public RegardDecay ProductionRegardDecay
        {
            get { return _productionRegardDecay; }
            set
            {
                _productionRegardDecay = value;
                RaisePropertyChanged("ProductionRegardDecay");
            }
        }

        public int? IncursionRegardCost
        {
            get
            {
                if (_incursionRegardCost.HasValue)
                    return _incursionRegardCost.Value;
                if (_parentState != null)
                    return _parentState._incursionRegardCost;
                return null;
            }
            set
            {
                _incursionRegardCost = value;
                RaisePropertyChanged("IncursionRegardCost");
            }
        }

        public int? IncursionOfAllyRegardCost
        {
            get
            {
                if (_incursionOfAllyRegardCost.HasValue)
                    return _incursionOfAllyRegardCost.Value;
                if (_parentState != null)
                    return _parentState._incursionOfAllyRegardCost;
                return null;
            }
            set
            {
                _incursionOfAllyRegardCost = value;
                RaisePropertyChanged("IncursionOfAllyRegardCost");
            }
        }

        public int? InvaderMovementRegardCost
        {
            get
            {
                if (_invaderMovementRegardCost.HasValue)
                    return _invaderMovementRegardCost.Value;
                if (_parentState != null)
                    return _parentState._invaderMovementRegardCost;
                return null;
            }
            set
            {
                _invaderMovementRegardCost = value;
                RaisePropertyChanged("InvaderMovementRegardCost");
            }
        }

        public int? RaidingRegardCost
        {
            get
            {
                if (_raidingRegardCost.HasValue)
                    return _raidingRegardCost.Value;
                if (_parentState != null)
                    return _parentState._raidingRegardCost;
                return null;
            }
            set
            {
                _raidingRegardCost = value;
                RaisePropertyChanged("RaidingRegardCost");
            }
        }

        public int? AttackCiviliansRegardCost
        {
            get
            {
                if (_attackCiviliansRegardCost.HasValue)
                    return _attackCiviliansRegardCost.Value;
                if (_parentState != null)
                    return _parentState._attackCiviliansRegardCost;
                return null;
            }
            set
            {
                _attackCiviliansRegardCost = value;
                RaisePropertyChanged("AttackCiviliansRegardCost");
            }
        }

        public int? InvadeColonyRegardCost
        {
            get
            {
                if (_invadeColonyRegardCost.HasValue)
                    return _invadeColonyRegardCost.Value;
                if (_parentState != null)
                    return _parentState._invadeColonyRegardCost;
                return null;
            }
            set
            {
                _invadeColonyRegardCost = value;
                RaisePropertyChanged("InvadeColonyRegardCost");
            }
        }

        public int? HoldReceptionRegardBonus
        {
            get
            {
                if (_holdReceptionRegardBonus.HasValue)
                    return _holdReceptionRegardBonus.Value;
                if (_parentState != null)
                    return _parentState._holdReceptionRegardBonus;
                return null;
            }
            set
            {
                _holdReceptionRegardBonus = value;
                RaisePropertyChanged("HoldReceptionRegardBonus");
            }
        }

        public int? UnprovokedAttackRegardCost
        {
            get
            {
                if (_unprovokedAttackRegardCost.HasValue)
                    return _unprovokedAttackRegardCost.Value;
                if (_parentState != null)
                    return _parentState._unprovokedAttackRegardCost;
                return null;
            }
            set
            {
                _unprovokedAttackRegardCost = value;
                RaisePropertyChanged("UnprovokedAttackRegardCost");
            }
        }

        public int? TradeEmbargoRegardCost
        {
            get
            {
                if (_tradeEmbargoRegardCost.HasValue)
                    return _tradeEmbargoRegardCost.Value;
                if (_parentState != null)
                    return _parentState._tradeEmbargoRegardCost;
                return null;
            }
            set
            {
                _tradeEmbargoRegardCost = value;
                RaisePropertyChanged("TradeEmbargoRegardCost");
            }
        }

        public int? HasPactTrustBonus
        {
            get
            {
                if (_hasPactTrustBonus.HasValue)
                    return _hasPactTrustBonus.Value;
                if (_parentState != null)
                    return _parentState._hasPactTrustBonus;
                return null;
            }
            set
            {
                _hasPactTrustBonus = value;
                RaisePropertyChanged("HasPactTrustBonus");
            }
        }

        public int? NoWarTrustBonus
        {
            get
            {
                if (_noWarTrustBonus.HasValue)
                    return _noWarTrustBonus.Value;
                if (_parentState != null)
                    return _parentState._noWarTrustBonus;
                return null;
            }
            set
            {
                _noWarTrustBonus = value;
                RaisePropertyChanged("NoWarTrustBonus");
            }
        }

        public int? FollowThroughTrustBonus
        {
            get
            {
                if (_followThroughTrustBonus.HasValue)
                    return _followThroughTrustBonus.Value;
                if (_parentState != null)
                    return _parentState._followThroughTrustBonus;
                return null;
            }
            set
            {
                _followThroughTrustBonus = value;
                RaisePropertyChanged("FollowThroughTrustBonus");
            }
        }

        public int? AttackCiviliansTrustCost
        {
            get
            {
                if (_attackCiviliansTrustCost.HasValue)
                    return _attackCiviliansTrustCost.Value;
                if (_parentState != null)
                    return _parentState._attackCiviliansTrustCost;
                return null;
            }
            set
            {
                _attackCiviliansTrustCost = value;
                RaisePropertyChanged("AttackCiviliansTrustCost");
            }
        }

        public int? UnprovokedAttackTrustCost
        {
            get
            {
                if (_unprovokedAttackTrustCost.HasValue)
                    return _unprovokedAttackTrustCost.Value;
                if (_parentState != null)
                    return _parentState._unprovokedAttackTrustCost;
                return null;
            }
            set
            {
                _unprovokedAttackTrustCost = value;
                RaisePropertyChanged("UnprovokedAttackTrustCost");
            }
        }

        public int? WantMapTurns
        {
            get
            {
                if (_wantMapTurns.HasValue)
                    return _wantMapTurns.Value;
                if (_parentState != null)
                    return _parentState._wantMapTurns;
                return null;
            }
            set
            {
                _wantMapTurns = value;
                RaisePropertyChanged("WantMapTurns");
            }
        }

        [XmlElement("ProposalClauseState")]
        public ProposalClauseState[] ProposalClauseStates
        {
            get
            {
                if (_parentState == null)
                    return _proposalClauseState;
                return _proposalClauseState.Concat(
                    _parentState.ProposalClauseStates.Where(
                        o => !_proposalClauseState.Any(i => i.Clause == o.Clause)))
                    .ToArray();
            }
            set
            {
                _proposalClauseState = value;
                RaisePropertyChanged("ProposalClauseStates");
            }
        }

        [XmlAttribute(DataType = "NCName")]
        public string Key
        {
            get { return _key; }
            set
            {
                _key = value;
                RaisePropertyChanged("Key");
            }
        }

        #endregion

        #region Methods

        public static DiplomacyState Merge(DiplomacyState parentState)
        {
            if (parentState == null)
                throw new ArgumentNullException("parentState");
            return new DiplomacyState(parentState);
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged.Raise(this, propertyName);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    [Serializable]
    [XmlType(Namespace = "http://schemas.startreksupremacy.com/DiplomacyStates.xsd")]
    public class RegardDecay : INotifyPropertyChanged
    {
        #region Fields

        private double _negative;
        private double _positive;

        #endregion

        #region Properties and Indexers

        public double Positive
        {
            get { return _positive; }
            set
            {
                _positive = value;
                RaisePropertyChanged("Positive");
            }
        }

        public double Negative
        {
            get { return _negative; }
            set
            {
                _negative = value;
                RaisePropertyChanged("Negative");
            }
        }

        #endregion

        #region Methods

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged.Raise(this, propertyName);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    [Serializable]
    [XmlType(Namespace = "http://schemas.startreksupremacy.com/DiplomacyStates.xsd")]
    public class ProposalClauseState : INotifyPropertyChanged
    {
        #region Fields

        private int _acceptPriority;
        private ClauseType _clause;
        private int _recipientRegardResult;
        private int _rejectPriority;
        private int _senderRegardResult;
        private int _sendPriority;
        private int _violationRegardCost;
        private int _violationTrustCost;

        #endregion

        #region Properties and Indexers

        public ClauseType Clause
        {
            get { return _clause; }
            set
            {
                _clause = value;
                RaisePropertyChanged("Clause");
            }
        }

        public int SendPriority
        {
            get { return _sendPriority; }
            set
            {
                _sendPriority = value;
                RaisePropertyChanged("SendPriority");
            }
        }

        public int AcceptPriority
        {
            get { return _acceptPriority; }
            set
            {
                _acceptPriority = value;
                RaisePropertyChanged("AcceptPriority");
            }
        }

        public int RejectPriority
        {
            get { return _rejectPriority; }
            set
            {
                _rejectPriority = value;
                RaisePropertyChanged("RejectPriority");
            }
        }

        public int SenderRegardResult
        {
            get { return _senderRegardResult; }
            set
            {
                _senderRegardResult = value;
                RaisePropertyChanged("SenderRegardResult");
            }
        }

        public int RecipientRegardResult
        {
            get { return _recipientRegardResult; }
            set
            {
                _recipientRegardResult = value;
                RaisePropertyChanged("RecipientRegardResult");
            }
        }

        public int ViolationRegardCost
        {
            get { return _violationRegardCost; }
            set
            {
                _violationRegardCost = value;
                RaisePropertyChanged("ViolationRegardCost");
            }
        }

        public int ViolationTrustCost
        {
            get { return _violationTrustCost; }
            set
            {
                _violationTrustCost = value;
                RaisePropertyChanged("ViolationTrustCost");
            }
        }

        #endregion

        #region Methods

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged.Raise(this, propertyName);
        }

        public static ProposalClauseState Load(XContainer element)
        {
            return new ProposalClauseState
                   {
                       AcceptPriority = (int?)element.Element("AcceptPriority") ?? 0,
                       Clause = EnumHelper.ParseOrGetDefault<ClauseType>((string)element.Element("Clause")),
                       RecipientRegardResult = (int?)element.Element("RecipientRegardResult") ?? 0,
                       RejectPriority = (int?)element.Element("RejectPriority") ?? 0,
                       SenderRegardResult = (int?)element.Element("SenderRegardResult") ?? 0,
                       SendPriority = (int?)element.Element("SendPriority") ?? 0,
                       ViolationRegardCost = (int?)element.Element("ViolationRegardCost") ?? 0,
                       ViolationTrustCost = (int?)element.Element("ViolationTrustCost") ?? 0
                   };
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}