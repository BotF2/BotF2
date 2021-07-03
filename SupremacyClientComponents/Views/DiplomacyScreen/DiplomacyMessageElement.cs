using System;
using System.Collections;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Supremacy.Annotations;
using Supremacy.Client.Data;
using Supremacy.Client.Dialogs;
using Supremacy.Client.Input;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Scripting;
using Supremacy.Utility;

using System.Linq;

namespace Supremacy.Client.Views
{
    public class DiplomacyMessageElement : INotifyPropertyChanged, ILinkCommandSite
    {
        private readonly Civilization _sender;
        private readonly Civilization _recipient;
        private readonly ScriptExpression _scriptExpression;
        private readonly DelegateCommand<DataTemplate> _editParameterCommand;

        public DiplomacyMessageElement(
            [NotNull] Civilization sender,
            [NotNull] Civilization recipient,
            DiplomacyMessageElementActionCategory actionCategory,
            DiplomacyMessageElementType elementType,
            ICommand removeCommand)
        {
            _sender = sender ?? throw new ArgumentNullException("sender");
            _recipient = recipient ?? throw new ArgumentNullException("recipient");
            ActionCategory = actionCategory;
            ElementType = elementType; // includes TreatyWarPact
            RemoveCommand = removeCommand;

            _editParameterCommand = new DelegateCommand<DataTemplate>(
                ExecuteEditParameterCommand,
                CanExecuteEditParameterCommand);

            Type parameterType = GetViewModelParameterTypeForElementType(elementType);

            ScriptParameters scriptParameters = new ScriptParameters(
                new ScriptParameter("$sender", typeof(Civilization)),
                new ScriptParameter("$recipient", typeof(Civilization)));
            //new ScriptParameter("$target", typeof(Civilization)));

            if (parameterType != null) // for target of war pact, who do both sender and recipient declare war on
            {
                scriptParameters = scriptParameters.Merge(
                    new ScriptParameter(
                        "$parameter",
                        GetViewModelParameterTypeForElementType(elementType)));
            }

            _scriptExpression = new ScriptExpression(returnObservableResult: false)
            {
                Parameters = scriptParameters
            };

        }

        private void ExecuteEditParameterCommand(DataTemplate contentTemplate)
        {
            if (!CanExecuteEditParameterCommand(contentTemplate))
            {
                return;
            }

            Type parameterType = GetViewModelParameterTypeForElementType(ElementType);
            string displayMemberPath = (string)null;

            if (HasFixedParameter)
            {
                _ = MessageDialog.Show(
                    new ContentPresenter
                    {
                        Content = SelectedParameter,
                        ContentTemplate = contentTemplate
                    },
                    MessageDialogButtons.Ok);
            }
            else
            {
                if (parameterType == typeof(Civilization))
                {
                    displayMemberPath = "ShortName";
                }

                object parameter = TargetSelectionDialog.Show(
                    _parametersCallback().Cast<object>(),
                    displayMemberPath,
                    "SELECT PARAMETER");

                if (parameter != null)
                {
                    SelectedParameter = parameter;
                }
            }

            UpdateDescription();
        }

        private bool CanExecuteEditParameterCommand(DataTemplate contentTemplate)
        {
            if (!IsEditing || !HasParameter)
            {
                return false;
            }

            if (HasFixedParameter)
            {
                return SelectedParameter != null;
            }

            return HasParameter;
        }

        public DiplomacyMessageElementType ElementType { get; }

        public DiplomacyMessageElementActionCategory ActionCategory { get; }

        public ICommand EditParameterCommand => _editParameterCommand;

        public ICommand RemoveCommand { get; }

        #region Description Property

        [field: NonSerialized]
        public event EventHandler DescriptionChanged;

        private string _description;

        public string Description
        {
            get => _description;
            private set
            {
                if (Equals(value, _description))
                {
                    return;
                }

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

        #region Tone Property

        [field: NonSerialized]
        public event EventHandler ToneChanged;

        private Tone _tone;

        public Tone Tone
        {
            get => _tone;
            set
            {
                if (Equals(value, _tone))
                {
                    return;
                }

                _tone = value;

                OnToneChanged();
            }
        }

        protected virtual void OnToneChanged()
        {
            ToneChanged.Raise(this);
            OnPropertyChanged("Tone");
        }

        #endregion

        #region IsEditing Property

        [field: NonSerialized]
        public event EventHandler IsEditingChanged;

        private bool _isEditing;

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (Equals(value, _isEditing))
                {
                    return;
                }

                _isEditing = value;

                OnIsEditingChanged();
            }
        }

        protected virtual void OnIsEditingChanged()
        {
            IsEditingChanged.Raise(this);
            OnPropertyChanged("IsEditing");
        }

        #endregion

        #region HasParameter Property

        [field: NonSerialized]
        public event EventHandler HasParameterChanged;

        public bool HasParameter => HasFixedParameter || _parametersCallback != null;

        protected virtual void OnHasParameterChanged()
        {
            HasParameterChanged.Raise(this);
            OnPropertyChanged("HasParameter");
        }

        #endregion

        #region HasFixedParameter Property

        [field: NonSerialized]
        public event EventHandler HasFixedParameterChanged;

        private bool _hasFixedParameter;

        public bool HasFixedParameter
        {
            get => _hasFixedParameter;
            set
            {
                if (Equals(value, _hasFixedParameter))
                {
                    return;
                }

                _hasFixedParameter = value;

                OnHasFixedParameterChanged();
            }
        }

        protected virtual void OnHasFixedParameterChanged()
        {
            HasFixedParameterChanged.Raise(this);
            OnPropertyChanged("HasFixedParameter");
        }

        #endregion

        #region IsParameterSelected Property

        [field: NonSerialized]
        public event EventHandler IsParameterSelectedChanged;

        public bool IsParameterSelected => _selectedParameter != null;

        protected virtual void OnIsParameterSelectedChanged()
        {
            IsParameterSelectedChanged.Raise(this);
            OnPropertyChanged("IsParameterSelected");
        }

        #endregion

        #region SelectedParameter Property

        [field: NonSerialized]
        public event EventHandler SelectedParameterChanged;

        private object _selectedParameter;

        public object SelectedParameter
        {
            get => _selectedParameter;
            set
            {
                if (Equals(value, _selectedParameter))
                {
                    return;
                }

                _selectedParameter = value;

                OnSelectedParameterChanged();
                OnIsParameterSelectedChanged();
            }
        }

        protected virtual void OnSelectedParameterChanged()
        {
            SelectedParameterChanged.Raise(this);
            OnPropertyChanged("SelectedParameter");
        }

        #endregion

        #region AvailableParameters Property

        [field: NonSerialized]
        public event EventHandler AvailableParametersChanged;

        private Func<IEnumerable> _parametersCallback;

        public Func<IEnumerable> ParametersCallback
        {
            get => _parametersCallback;
            set
            {
                if (Equals(value, _parametersCallback))
                {
                    return;
                }

                _parametersCallback = value;

                OnAvailableParametersChanged();
                OnHasParameterChanged();
            }
        }

        protected virtual void OnAvailableParametersChanged()
        {
            AvailableParametersChanged.Raise(this);
            OnPropertyChanged("AvailableParameters");
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
                    PropertyChangedEventHandler oldHandler = _propertyChanged;
                    PropertyChangedEventHandler newHandler = (PropertyChangedEventHandler)Delegate.Combine(oldHandler, value);

                    if (Interlocked.CompareExchange(ref _propertyChanged, newHandler, oldHandler) == oldHandler)
                    {
                        return;
                    }
                }
            }
            remove
            {
                while (true)
                {
                    PropertyChangedEventHandler oldHandler = _propertyChanged;
                    PropertyChangedEventHandler newHandler = (PropertyChangedEventHandler)Delegate.Remove(oldHandler, value);

                    if (Interlocked.CompareExchange(ref _propertyChanged, newHandler, oldHandler) == oldHandler)
                    {
                        return;
                    }
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            _propertyChanged.Raise(this, propertyName);
        }

        #endregion

        public void UpdateDescription()
        {
            Description = GetDescription();
        }

        private static Type GetViewModelParameterTypeForElementType(DiplomacyMessageElementType elementType)
        {
            switch (elementType)
            {
                case DiplomacyMessageElementType.CommendWarStatement:
                case DiplomacyMessageElementType.CommendTreatyStatement:
                case DiplomacyMessageElementType.CommendAssaultStatement:
                case DiplomacyMessageElementType.CommendInvasionStatement:
                case DiplomacyMessageElementType.CommendSabotageStatement:
                case DiplomacyMessageElementType.DenounceWarStatement:
                case DiplomacyMessageElementType.DenounceTreatyStatement:
                case DiplomacyMessageElementType.DenounceAssaultStatement:
                case DiplomacyMessageElementType.DenounceInvasionStatement:
                case DiplomacyMessageElementType.DenounceSabotageStatement:
                case DiplomacyMessageElementType.OfferWithdrawTroopsClause:
                    return typeof(Civilization);

                case DiplomacyMessageElementType.RequestWithdrawTroopsClause:
                    break;
                case DiplomacyMessageElementType.OfferStopPiracyClause:
                    break;
                case DiplomacyMessageElementType.RequestStopPiracyClause:
                    break;
                case DiplomacyMessageElementType.OfferBreakAgreementClause:
                    break;
                case DiplomacyMessageElementType.RequestBreakAgreementClause:
                    break;
                case DiplomacyMessageElementType.OfferGiveCreditsClause:
                case DiplomacyMessageElementType.RequestGiveCreditsClause:
                    return typeof(CreditsDataViewModel);
                case DiplomacyMessageElementType.OfferGiveResourcesClause:
                    break;
                case DiplomacyMessageElementType.RequestGiveResourcesClause:
                    break;
                case DiplomacyMessageElementType.OfferMapDataClause:
                    break;
                case DiplomacyMessageElementType.RequestMapDataClause:
                    break;
                case DiplomacyMessageElementType.OfferHonorMilitaryAgreementClause:
                    break;
                case DiplomacyMessageElementType.RequestHonorMilitaryAgreementClause:
                    break;
                case DiplomacyMessageElementType.OfferEndEmbargoClause:
                    break;
                case DiplomacyMessageElementType.RequestEndEmbargoClause:
                    break;
                case DiplomacyMessageElementType.WarDeclaration:
                    break;
                case DiplomacyMessageElementType.TreatyWarPact:
                    return typeof(Civilization);
                case DiplomacyMessageElementType.TreatyCeaseFireClause:
                    break;
                case DiplomacyMessageElementType.TreatyNonAggressionClause:
                    break;
                case DiplomacyMessageElementType.TreatyOpenBordersClause:
                    break;
                case DiplomacyMessageElementType.TreatyTradePactClause:
                    break;
                case DiplomacyMessageElementType.TreatyResearchPactClause:
                    break;
                case DiplomacyMessageElementType.TreatyAffiliationClause:
                    break;
                case DiplomacyMessageElementType.TreatyDefensiveAllianceClause:
                    break;
                case DiplomacyMessageElementType.TreatyFullAllianceClause:
                    break;
                case DiplomacyMessageElementType.TreatyMembershipClause:
                    break;
            }
            return null;
        }

        private string GetDescription()
        {
            string text = DiplomacyMessageViewModel.LookupDiplomacyText(ResolveStringID(), _tone, _sender) ?? string.Empty;

            _scriptExpression.ScriptCode = DiplomacyMessageViewModel.QuoteString(text);

            RuntimeScriptParameters parameters = new RuntimeScriptParameters
                             {
                                 new RuntimeScriptParameter(_scriptExpression.Parameters[0], _sender),
                                 new RuntimeScriptParameter(_scriptExpression.Parameters[1], _recipient)
                             };

            if (_scriptExpression.Parameters.Count > 2)
            {
                parameters.Add(new RuntimeScriptParameter(_scriptExpression.Parameters[2], _selectedParameter));
            }

            return _scriptExpression.Evaluate<string>(parameters);
        }

        private DiplomacyStringID ResolveStringID()
        {
            switch (ElementType)
            {
                case DiplomacyMessageElementType.CommendWarStatement:
                    break;
                case DiplomacyMessageElementType.CommendTreatyStatement:
                    break;
                case DiplomacyMessageElementType.CommendAssaultStatement:
                    break;
                case DiplomacyMessageElementType.CommendInvasionStatement:
                    break;
                case DiplomacyMessageElementType.CommendSabotageStatement:
                    break;
                case DiplomacyMessageElementType.DenounceWarStatement:
                    break;
                case DiplomacyMessageElementType.DenounceTreatyStatement:
                    break;
                case DiplomacyMessageElementType.DenounceAssaultStatement:
                    break;
                case DiplomacyMessageElementType.DenounceInvasionStatement:
                    break;
                case DiplomacyMessageElementType.DenounceSabotageStatement:
                    break;
                case DiplomacyMessageElementType.OfferWithdrawTroopsClause:
                    break;
                case DiplomacyMessageElementType.RequestWithdrawTroopsClause:
                    break;
                case DiplomacyMessageElementType.OfferStopPiracyClause:
                    break;
                case DiplomacyMessageElementType.RequestStopPiracyClause:
                    break;
                case DiplomacyMessageElementType.OfferBreakAgreementClause:
                    break;
                case DiplomacyMessageElementType.RequestBreakAgreementClause:
                    break;
                case DiplomacyMessageElementType.OfferGiveCreditsClause:
                    if (SelectedParameter is CreditsDataViewModel offerCreditsData)
                    {
                        if (offerCreditsData.RecurringAmount > 0 && offerCreditsData.ImmediateAmount > 0)
                        {
                            return DiplomacyStringID.CreditsOfferImmediateAndRecurring;
                        }

                        if (offerCreditsData.RecurringAmount > 0)
                        {
                            return DiplomacyStringID.CreditsOfferRecurring;
                        }
                    }
                    return DiplomacyStringID.CreditsOfferImmediate;

                case DiplomacyMessageElementType.RequestGiveCreditsClause:
                    if (SelectedParameter is CreditsDataViewModel requestCreditsData)
                    {
                        if (requestCreditsData.RecurringAmount > 0 && requestCreditsData.ImmediateAmount > 0)
                        {
                            return DiplomacyStringID.CreditsDemandImmediateAndRecurring;
                        }

                        if (requestCreditsData.RecurringAmount > 0)
                        {
                            return DiplomacyStringID.CreditsDemandRecurring;
                        }
                    }
                    return DiplomacyStringID.CreditsDemandImmediate;

                case DiplomacyMessageElementType.OfferGiveResourcesClause:
                    break;
                case DiplomacyMessageElementType.RequestGiveResourcesClause:
                    break;
                case DiplomacyMessageElementType.OfferMapDataClause:
                    break;
                case DiplomacyMessageElementType.RequestMapDataClause:
                    break;
                case DiplomacyMessageElementType.OfferHonorMilitaryAgreementClause:
                    break;
                case DiplomacyMessageElementType.RequestHonorMilitaryAgreementClause:
                    break;
                case DiplomacyMessageElementType.OfferEndEmbargoClause:
                    break;
                case DiplomacyMessageElementType.RequestEndEmbargoClause:
                    break;

                case DiplomacyMessageElementType.WarDeclaration:
                    return DiplomacyStringID.WarDeclaration;

                case DiplomacyMessageElementType.TreatyWarPact:
                    return DiplomacyStringID.WarPactClause;

                case DiplomacyMessageElementType.TreatyCeaseFireClause:
                    return DiplomacyStringID.CeaseFireClause;

                case DiplomacyMessageElementType.TreatyNonAggressionClause:
                    return DiplomacyStringID.NonAggressionPactClause;

                case DiplomacyMessageElementType.TreatyOpenBordersClause:
                    return DiplomacyStringID.OpenBordersClause;

                case DiplomacyMessageElementType.TreatyTradePactClause:
                    break;

                case DiplomacyMessageElementType.TreatyResearchPactClause:
                    break;

                case DiplomacyMessageElementType.TreatyAffiliationClause:
                    return DiplomacyStringID.AffiliationClause;

                case DiplomacyMessageElementType.TreatyDefensiveAllianceClause:
                    return DiplomacyStringID.DefensiveAllianceClause;

                case DiplomacyMessageElementType.TreatyFullAllianceClause:
                    return DiplomacyStringID.FullAllianceClause;

                case DiplomacyMessageElementType.TreatyMembershipClause:
                    return DiplomacyStringID.MembershipClause;
            }

            return DiplomacyStringID.None;
        }

        #region Implementation of ILinkCommandSite

        ICommand ILinkCommandSite.LinkCommand => new DelegateCommand(() => { });

        object ILinkCommandSite.LinkCommandParameter => this;

        #endregion
    }
}