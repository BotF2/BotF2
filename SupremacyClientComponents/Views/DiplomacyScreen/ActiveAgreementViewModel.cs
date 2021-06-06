//File:ActiveAgreementViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;

using Supremacy.Annotations;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Scripting;
using Supremacy.Utility;

namespace Supremacy.Client.Views
{
    public class ActiveAgreementViewModel : INotifyPropertyChanged
    {
        private readonly IAgreement _agreement;
        private readonly ObservableCollection<DiplomacyMessageElement> _elements;
        private readonly ReadOnlyObservableCollection<DiplomacyMessageElement> _elementsView;
        private readonly ScriptExpression _descriptionScript;
        private readonly ScriptParameters _descriptionParameters;
        private readonly RuntimeScriptParameters _descriptionRuntimeParameters;

        public ActiveAgreementViewModel([NotNull] IAgreement agreement)
        {
            if (agreement == null)
                throw new ArgumentNullException("agreement");

            _agreement = agreement;
            _elements = new ObservableCollection<DiplomacyMessageElement>();
            _elementsView = new ReadOnlyObservableCollection<DiplomacyMessageElement>(_elements);

            StartTurn = _agreement.StartTurn;
            EndTurn = _agreement.EndTurn;
            Category = ResolveCategory();

            PopulateElements();

            _descriptionParameters = new ScriptParameters(
                new ScriptParameter("$sender", typeof(Civilization)),
                new ScriptParameter("$recipient", typeof(Civilization)),
                new ScriptParameter("$startTurn", typeof(int)),
                new ScriptParameter("$endTurn", typeof(int)));

            _descriptionScript = new ScriptExpression(returnObservableResult: false)
                                 {
                                     Parameters = _descriptionParameters
                                 };

            _descriptionRuntimeParameters = new RuntimeScriptParameters
                                       {
                                           new RuntimeScriptParameter(_descriptionParameters[0], _agreement.Proposal.Sender),
                                           new RuntimeScriptParameter(_descriptionParameters[1], _agreement.Proposal.Recipient),
                                           new RuntimeScriptParameter(_descriptionParameters[2], (int)_agreement.StartTurn),
                                           new RuntimeScriptParameter(_descriptionParameters[3], (int)_agreement.EndTurn),
                                       };

            UpdateDescription();
        }

        public ReadOnlyObservableCollection<DiplomacyMessageElement> Elements => _elementsView;

        #region StartTurn Property

        [field: NonSerialized]
        public event EventHandler StartTurnChanged;

        private int _startTurn;

        public int StartTurn
        {
            get { return _startTurn; }
            private set
            {
                if (Equals(value, _startTurn))
                    return;

                _startTurn = value;

                OnStartTurnChanged();
            }
        }

        protected virtual void OnStartTurnChanged()
        {
            StartTurnChanged.Raise(this);
            OnPropertyChanged("StartTurn");
        }

        #endregion

        #region EndTurn Property

        [field: NonSerialized]
        public event EventHandler EndTurnChanged;

        private int _endTurn;

        public int EndTurn
        {
            get { return _endTurn; }
            private set
            {
                if (Equals(value, _endTurn))
                    return;

                _endTurn = value;

                OnEndTurnChanged();
            }
        }

        protected virtual void OnEndTurnChanged()
        {
            EndTurnChanged.Raise(this);
            OnPropertyChanged("EndTurn");
        }

        #endregion

        #region Category Property

        [field: NonSerialized]
        public event EventHandler CategoryChanged;

        private DiplomaticMessageCategory _category;

        public DiplomaticMessageCategory Category
        {
            get { return _category; }
            private set
            {
                if (Equals(value, _category))
                    return;

                _category = value;

                OnCategoryChanged();
            }
        }

        protected virtual void OnCategoryChanged()
        {
            CategoryChanged.Raise(this);
            OnPropertyChanged("Category");
        }

        #endregion

        #region Description Property

        [field: NonSerialized]
        public event EventHandler DescriptionChanged;

        private string _description;

        public string Description
        {
            get { return _description; }
            private set
            {
                if (Equals(value, _description))
                    return;

                _description = value;

                OnDescriptionChanged();
            }
        }

        protected virtual void OnDescriptionChanged()
        {
            DescriptionChanged.Raise(this);
            OnPropertyChanged("Description");
        }

        #endregion

        private DiplomaticMessageCategory ResolveCategory()
        {
            //GameLog.Client.Diplomacy.DebugFormat("Proposal ={0}", _agreement.Proposal.Clauses[0].Le);
            return ForeignPowerViewModel.ResolveMessageCategory(_agreement.Proposal);
        }

        private void PopulateElements()
        {
            var proposal = _agreement.Proposal;

            foreach (var clause in proposal.Clauses)
            {
                var element = new DiplomacyMessageElement(
                    proposal.Sender,
                    proposal.Recipient,
                    DiplomacyMessageElementActionCategory.Propose,
                    DiplomacyScreenViewModel.ElementTypeFromClauseType(clause.ClauseType),
                    null);
                //GameLog.Client.Diplomacy.DebugFormat("((()))DiplomacyMessageFromClauseType out ElementType ={0}, sender ={1}, recipient ={2}",
                //    DiplomacyScreenViewModel.ElementTypeFromClauseType(clause.ClauseType).ToString(),
                //    proposal.Sender.ShortName,
                //    proposal.Recipient.ShortName);
                if (clause.ClauseType == ClauseType.OfferGiveCredits ||
                    clause.ClauseType == ClauseType.RequestGiveCredits)
                {
                    var data = clause.GetData<CreditsClauseData>();
                    if (data != null)
                    {
                        element.HasFixedParameter = true;
                        element.SelectedParameter = new CreditsDataViewModel
                                                    {
                                                        ImmediateAmount = data.ImmediateAmount,
                                                        RecurringAmount = data.RecurringAmount
                                                    };
                    }
                }
                else if (clause.Data != null)
                {
                    element.SelectedParameter = clause.Data;
                }

                element.UpdateDescription();

                _elements.Add(element);
            }
        }

        private void UpdateDescription()
        {
            DiplomacyStringID leadInId;

            var hasDuration = _agreement.EndTurn != 0;
            var proposal = _agreement.Proposal;

            if (proposal.IsGift())
                leadInId = hasDuration ? DiplomacyStringID.ActiveAgreementDescriptionGift : DiplomacyStringID.ActiveAgreementDescriptionGiftNoDuration;
            else if (proposal.IsDemand())
                leadInId = hasDuration ? DiplomacyStringID.ActiveAgreementDescriptionDemand : DiplomacyStringID.ActiveAgreementDescriptionDemandNoDuration;
            else if (!proposal.HasTreaty())
                leadInId = hasDuration ? DiplomacyStringID.ActiveAgreementDescriptionExchange : DiplomacyStringID.ActiveAgreementDescriptionExchangeNoDuration;
            else
                leadInId = hasDuration ? DiplomacyStringID.ActiveAgreementDescriptionTreaty : DiplomacyStringID.ActiveAgreementDescriptionTreatyNoDuration;

            _descriptionScript.ScriptCode = DiplomacyMessageViewModel.QuoteString(
                DiplomacyMessageViewModel.LookupDiplomacyText(
                    leadInId,
                    proposal.Tone,
                    proposal.Sender) ?? string.Empty);

            Description = _descriptionScript.Evaluate<string>(_descriptionRuntimeParameters);
        }

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