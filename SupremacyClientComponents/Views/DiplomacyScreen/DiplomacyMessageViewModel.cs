//File:DiplomacyMessageViewModel
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Practices.ServiceLocation;

using Supremacy.Annotations;
using Supremacy.Client.Dialogs;
using Supremacy.Client.Input;
using Supremacy.Collections;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Resources;
using Supremacy.Scripting;
using Supremacy.Text;
using Supremacy.Utility;

namespace Supremacy.Client.Views
{
    public class DiplomacyMessageViewModel : INotifyPropertyChanged
    {
        private readonly Civilization _recipient;
        private readonly ObservableCollection<DiplomacyMessageElement> _elements;
        private readonly ObservableCollection<DiplomacyMessageElement> _offerElements;
        private readonly ObservableCollection<DiplomacyMessageElement> _requestElements;
        private readonly ObservableCollection<DiplomacyMessageElement> _statementElements;
        private readonly ObservableCollection<DiplomacyMessageElement> _treatyElements;
        private readonly ReadOnlyObservableCollection<DiplomacyMessageElement> _treatyElementsView;
        // private readonly ReadOnlyObservableCollection<DiplomacyMessageElement> _acceptRejectElementsView; // no view of this???
        private readonly ObservableCollection<DiplomacyMessageAvailableElement> _availableElements;
        private readonly DelegateCommand<DiplomacyMessageElement> _removeElementCommand;

        private readonly ScriptExpression _treatyLeadInTextScript;
        private readonly ScriptExpression _offerLeadInTextScript;
        private readonly ScriptExpression _requestLeadInTextScript;

        private readonly ScriptParameters _leadInParameters;
#pragma warning disable IDE0044 // Add readonly modifier
        private RuntimeScriptParameters _leadInRuntimeParameters;
#pragma warning restore IDE0044 // Add readonly modifier


        private readonly DelegateCommand<ICheckableCommandParameter> _setAcceptButton;
        private readonly DelegateCommand<ICheckableCommandParameter> _setRejectButton;
        private Order _sendOrder;
        private string _response = "....";
        int _turnOfResponse;

        public DiplomacyMessageViewModel([NotNull] Civilization sender, [NotNull] Civilization recipient)
        {
            _setAcceptButton = new DelegateCommand<ICheckableCommandParameter>(ExecuteSetAcceptButton, CanExecuteSetAcceptButton);
            _setRejectButton = new DelegateCommand<ICheckableCommandParameter>(ExecuteSetRejectButton, CanExecuteSetRejectButton);
            Sender = sender ?? throw new ArgumentNullException("sender");
            _recipient = recipient ?? throw new ArgumentNullException("recipient");
            _elements = new ObservableCollection<DiplomacyMessageElement>();
            Elements = new ReadOnlyObservableCollection<DiplomacyMessageElement>(_elements);
            _availableElements = new ObservableCollection<DiplomacyMessageAvailableElement>();
            AvailableElements = new ReadOnlyObservableCollection<DiplomacyMessageAvailableElement>(_availableElements);
            _treatyElements = new ObservableCollection<DiplomacyMessageElement>();
            _treatyElementsView = new ReadOnlyObservableCollection<DiplomacyMessageElement>(_treatyElements);

            _offerElements = new ObservableCollection<DiplomacyMessageElement>();
            OfferElements = new ReadOnlyObservableCollection<DiplomacyMessageElement>(_offerElements);
            _requestElements = new ObservableCollection<DiplomacyMessageElement>();
            RequestElements = new ReadOnlyObservableCollection<DiplomacyMessageElement>(_requestElements);
            _statementElements = new ObservableCollection<DiplomacyMessageElement>();
            StatementElements = new ReadOnlyObservableCollection<DiplomacyMessageElement>(_statementElements);

            _removeElementCommand = new DelegateCommand<DiplomacyMessageElement>(
                ExecuteRemoveElementCommand,
                CanExecuteRemoveElementCommand);

            _leadInParameters = new ScriptParameters(
                new ScriptParameter("$sender", typeof(Civilization)),
                new ScriptParameter("$recipient", typeof(Civilization)));

            _leadInRuntimeParameters = new RuntimeScriptParameters
                                       {
                                           new RuntimeScriptParameter(_leadInParameters[0], Sender),
                                           new RuntimeScriptParameter(_leadInParameters[1], _recipient)
                                       };

            _treatyLeadInTextScript = new ScriptExpression(returnObservableResult: false)
            {
                Parameters = _leadInParameters
            };

            _offerLeadInTextScript = new ScriptExpression(returnObservableResult: false)
            {
                Parameters = _leadInParameters
            };

            _requestLeadInTextScript = new ScriptExpression(returnObservableResult: false)
            {
                Parameters = _leadInParameters
            };

            CollectionViewSource.GetDefaultView(AvailableElements).GroupDescriptions.Add(new PropertyGroupDescription("ActionDescription"));
        }

        private bool CanExecuteRemoveElementCommand(DiplomacyMessageElement element)
        {
            return IsEditing && element != null && _elements.Contains(element);
        }

        private void ExecuteRemoveElementCommand(DiplomacyMessageElement element)
        {
            if (!CanExecuteRemoveElementCommand(element))
            {
                return;
            }

            RemoveElement(element);
        }

        public ICommand SetAcceptButton => _setAcceptButton;

        public ICommand SetRejectButton => _setRejectButton;

        public string Response
        {
            get
            {
                int selectedID = 888;
                if (DiplomacyScreenViewModel.DesignInstance.SelectedForeignPower != null)
                {
                    selectedID = DiplomacyScreenViewModel.DesignInstance.SelectedForeignPower.Owner.CivID;
                }
                if (AcceptedRejected.ContainsKey(selectedID))
                {
                    return AcceptedRejected[selectedID];
                }
                //}
                return "...";
            }
            set
            {
                if (_response != value)
                {
                    int turn = GameContext.Current.TurnNumber;
                    if (_turnOfResponse != turn)
                    {
                        AcceptedRejected.Clear();
                        _turnOfResponse = turn;
                    }
                    int selectedID = DiplomacyScreenViewModel.DesignInstance.SelectedForeignPower.Owner.CivID;
                    if (AcceptedRejected.ContainsKey(selectedID))
                    {
                        _ = AcceptedRejected.Remove(selectedID);
                        AcceptedRejected.Add(selectedID, value);
                    }
                    else
                    {
                        AcceptedRejected.Add(selectedID, value);
                    }

                    _response = value;
                    OnPropertyChanged(true, "Response");
                }
            }
        }

        public Dictionary<int, string> AcceptedRejected { get; } = new Dictionary<int, string> { { 999, "placeHolder" } };

        public Civilization Sender { get; }

        public Civilization Recipient => _recipient;

        public ReadOnlyObservableCollection<DiplomacyMessageElement> Elements { get; }

        public ReadOnlyObservableCollection<DiplomacyMessageElement> TreatyElements => _treatyElementsView;

        public ReadOnlyObservableCollection<DiplomacyMessageElement> RequestElements { get; }

        public ReadOnlyObservableCollection<DiplomacyMessageElement> OfferElements { get; }

        public ReadOnlyObservableCollection<DiplomacyMessageElement> StatementElements { get; }

        //public ReadOnlyObservableCollection<DiplomacyMessageElement> AcceptRejectElements  // we do not view this??? 
        //{
        //    get { return _acceptRejectElementsView; }
        //}

        public ReadOnlyObservableCollection<DiplomacyMessageAvailableElement> AvailableElements { get; }

        internal bool IsStatement => _elements.All(o => o.ElementType <= DiplomacyMessageElementType.DenounceSabotageStatement);
        internal bool IsTreaty => _elements.All(o => o.ElementType > DiplomacyMessageElementType.TreatyWarPact);
        public int HideAcceptRejectButtons
        {
            get
            {
                Civilization sender = Sender;
                Civilization recipient = _recipient;
                Diplomat diplomat = Diplomat.Get(recipient);
                ForeignPower foreignPower = diplomat.GetForeignPower(sender);

                int decider = 1; // accept and reject buttons hidden if decider = 1, exposed if decider = 0.
                if (IsTreaty)
                {
                    decider = 0;
                }

                if (foreignPower.ResponseReceived != null)
                {
                    decider = 1;
                }

                if (decider == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
        }

        internal IDiplomaticExchange CreateMessage()
        {
            return IsStatement ? (IDiplomaticExchange)CreateStatement() : CreateProposal();
        }

        public void Send()
        {
            bool isStatement = _elements.All(o => o.ElementType <= DiplomacyMessageElementType.DenounceSabotageStatement);
            if (isStatement)
            {
                Statement statement = CreateStatement();
                if (statement == null)
                {
                    return;
                }

                _sendOrder = new SendStatementOrder(statement);

                ServiceLocator.Current.GetInstance<IPlayerOrderService>().AddOrder(_sendOrder);
            }
            else
            {
                NewProposal proposal = CreateProposal();
                if (proposal == null)
                {
                    return;
                }

                _sendOrder = new SendProposalOrder(proposal);

                ServiceLocator.Current.GetInstance<IPlayerOrderService>().AddOrder(_sendOrder);
            }

            IsEditing = false;

            _availableElements.Clear();
        }

        public void Edit()
        {
            if (_sendOrder != null)
            {
                _ = ServiceLocator.Current.GetInstance<IPlayerOrderService>().RemoveOrder(_sendOrder);
            }

            _sendOrder = null;

            IsEditing = true;
            PopulateAvailableElements();
        }

        public void Cancel()
        {
            if (_sendOrder != null)
            {
                _ = ServiceLocator.Current.GetInstance<IPlayerOrderService>().RemoveOrder(_sendOrder);
            }

            _sendOrder = null;

            IsEditing = false;

            _availableElements.Clear();
            _elements.Clear();
        }

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
                _ = _elements.ForEach(o => o.Tone = value);

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
            private set
            {
                if (Equals(value, _isEditing))
                {
                    return;
                }

                _isEditing = value;
                _ = _elements.ForEach(o => o.IsEditing = value);

                OnIsEditingChanged();
            }
        }

        protected virtual void OnIsEditingChanged()
        {
            IsEditingChanged.Raise(this);
            OnPropertyChanged("IsEditing");
            InvalidateCommands();
        }

        #endregion

        #region OfferLeadInText Property

        [field: NonSerialized]
        public event EventHandler OfferLeadInTextChanged;

        private string _offerLeadInText;

        public string OfferLeadInText
        {
            get => _offerLeadInText;
            private set
            {
                if (Equals(value, _offerLeadInText))
                {
                    return;
                }

                _offerLeadInText = value;

                OnOfferLeadInTextChanged();
                OnHasOfferLeadInTextChanged();
            }
        }

        protected virtual void OnOfferLeadInTextChanged()
        {
            OfferLeadInTextChanged.Raise(this);
            OnPropertyChanged("OfferLeadInText");
        }

        #endregion

        #region HasOfferLeadInText Property

        [field: NonSerialized]
        public event EventHandler HasOfferLeadInTextChanged;

        public bool HasOfferLeadInText => !string.IsNullOrWhiteSpace(OfferLeadInText);

        protected virtual void OnHasOfferLeadInTextChanged()
        {
            HasOfferLeadInTextChanged.Raise(this);
            OnPropertyChanged("HasOfferLeadInText");
        }

        #endregion

        #region RequestLeadInText Property

        [field: NonSerialized]
        public event EventHandler RequestLeadInTextChanged;

        private string _requestRequestLeadInText;

        public string RequestLeadInText
        {
            get => _requestRequestLeadInText;
            private set
            {
                if (Equals(value, _requestRequestLeadInText))
                {
                    return;
                }

                _requestRequestLeadInText = value;

                OnRequestLeadInTextChanged();
                OnHasRequestLeadInTextChanged();
            }
        }

        protected virtual void OnRequestLeadInTextChanged()
        {
            RequestLeadInTextChanged.Raise(this);
            OnPropertyChanged("RequestLeadInText");
        }

        #endregion

        #region HasRequestLeadInText Property

        [field: NonSerialized]
        public event EventHandler HasRequestLeadInTextChanged;

        public bool HasRequestLeadInText => !string.IsNullOrWhiteSpace(RequestLeadInText);

        protected virtual void OnHasRequestLeadInTextChanged()
        {
            HasRequestLeadInTextChanged.Raise(this);
            OnPropertyChanged("HasRequestLeadInText");
        }

        #endregion

        #region TreatyLeadInText Property

        [field: NonSerialized]
        public event EventHandler TreatyLeadInTextChanged;

        private string _treatyLeadInText;

        public string TreatyLeadInText
        {
            get => _treatyLeadInText;
            private set
            {
                if (Equals(value, _treatyLeadInText))
                {
                    return;
                }

                _treatyLeadInText = value;

                OnTreatyLeadInTextChanged();
                OnHasTreatyLeadInTextChanged();
            }
        }

        protected virtual void OnTreatyLeadInTextChanged()
        {
            TreatyLeadInTextChanged.Raise(this);
            OnPropertyChanged("TreatyLeadInText");
        }

        #endregion

        #region HasTreatyLeadInText Property

        [field: NonSerialized]
        public event EventHandler HasTreatyLeadInTextChanged;

        public bool HasTreatyLeadInText => !string.IsNullOrWhiteSpace(TreatyLeadInText);

        protected virtual void OnHasTreatyLeadInTextChanged()
        {
            HasTreatyLeadInTextChanged.Raise(this);
            OnPropertyChanged("HasTreatyLeadInText");
        }
        #endregion

        public void UpdateLeadInText()
        {
            DiplomacyStringID treatyLeadInId = DiplomacyStringID.None;
            DiplomacyStringID offerLeadInId = DiplomacyStringID.None;
            DiplomacyStringID requestLeadInId = DiplomacyStringID.None;

            if (_treatyElements.Count != 0)
            {
                bool isWarPact = _treatyElements[0].ElementType == DiplomacyMessageElementType.TreatyWarPact;

                treatyLeadInId = isWarPact ? DiplomacyStringID.WarPactLeadIn : DiplomacyStringID.ProposalLeadIn;
                //if (treatyLeadInId == DiplomacyStringID.ProposalLeadIn)
                //{
                //    GameLog.Client.DiplomacyDetails.DebugFormat("** Treaty Leadin text set, {0}", treatyLeadInId.ToString());
                //}

                /* we do not currently use offer or demand with warpact */
                if (_offerElements.Count != 0)
                {
                    offerLeadInId = isWarPact ? DiplomacyStringID.WarPactOffersLeadIn : DiplomacyStringID.ProposalOffersLeadIn;
                }

                if (_requestElements.Count != 0)
                {
                    requestLeadInId = isWarPact ? DiplomacyStringID.WarPactDemandsLeadIn : DiplomacyStringID.ProposalDemandsLeadIn;
                }
            }
            else if (_requestElements.Count != 0)
            {
                if (_offerElements.Count != 0)
                {
                    offerLeadInId = DiplomacyStringID.ExchangeLeadIn;
                    requestLeadInId = DiplomacyStringID.ProposalDemandsLeadIn;
                }
                else
                {
                    requestLeadInId = DiplomacyStringID.DemandLeadIn;
                }
            }
            else if (_offerElements.Count != 0)
            {
                offerLeadInId = DiplomacyStringID.GiftLeadIn;
            }

            if (treatyLeadInId == DiplomacyStringID.None)
            {
                TreatyLeadInText = null;
            }
            else
            {
                _treatyLeadInTextScript.ScriptCode = QuoteString(LookupDiplomacyText(treatyLeadInId, _tone, Sender) ?? string.Empty);
                TreatyLeadInText = _treatyLeadInTextScript.Evaluate<string>(_leadInRuntimeParameters);
            }

            if (offerLeadInId == DiplomacyStringID.None)
            {
                OfferLeadInText = null;
            }
            else
            {
                _offerLeadInTextScript.ScriptCode = QuoteString(LookupDiplomacyText(offerLeadInId, _tone, Sender) ?? string.Empty);
                OfferLeadInText = _offerLeadInTextScript.Evaluate<string>(_leadInRuntimeParameters);
            }

            if (requestLeadInId == DiplomacyStringID.None)
            {
                RequestLeadInText = null;
            }
            else
            {
                _requestLeadInTextScript.ScriptCode = QuoteString(LookupDiplomacyText(requestLeadInId, _tone, Sender) ?? string.Empty);
                RequestLeadInText = _requestLeadInTextScript.Evaluate<string>(_leadInRuntimeParameters);
            }
        }

        internal static string QuoteString(string value)
        {
            if (value == null)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder(value.Length + 2);
            int bracketDepth = 0;

            _ = sb.Append('"');

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                char last = i == 0 ? '\0' : value[i - 1];
                if (c == '{' && last != '\\')
                {
                    ++bracketDepth;
                }
                else if (c == '}' && last != '\\')
                {
                    --bracketDepth;
                }
                else if (c == '"' && bracketDepth == 0)
                {
                    _ = sb.Append('\\');
                }

                _ = sb.Append(c);
            }

            _ = sb.Append('"');

            return sb.ToString();
        }

        internal static string LookupDiplomacyText(DiplomacyStringID stringId, Tone tone, Civilization sender)
        {
            DiplomacyStringKey civStringKey = new DiplomacyStringKey(sender?.Key, stringId);

            if (LocalizedTextDatabase.Instance.Groups.TryGetValue(civStringKey, out LocalizedTextGroup civTextGroup) &&
                civTextGroup.Entries.TryGetValue(tone, out LocalizedString localizedString))
            {
                return localizedString.LocalText;
            }

            DiplomacyStringKey defaultStringKey = new DiplomacyStringKey(null, stringId);

            if (LocalizedTextDatabase.Instance.Groups.TryGetValue(defaultStringKey, out LocalizedTextGroup defaultTextGroup) &&
                defaultTextGroup.Entries.TryGetValue(tone, out localizedString))
            {
                return localizedString.LocalText;
            }

            if (civTextGroup != null && civTextGroup.DefaultEntry != null)
            {
                return civTextGroup.DefaultEntry.LocalText;
            }

            if (defaultTextGroup != null && defaultTextGroup.DefaultEntry != null)
            {
                return defaultTextGroup.DefaultEntry.LocalText;
            }

            return null;
        }

        internal void AddElement([NotNull] DiplomacyMessageAvailableElement availableElement)
        {
            if (availableElement == null)
            {
                throw new ArgumentNullException("availableElement");
            }

            DiplomacyMessageElement element = new DiplomacyMessageElement(Sender, _recipient, availableElement.ActionCategory, availableElement.ElementType, _removeElementCommand)
            {
                ParametersCallback = availableElement.ParametersCallback,
                HasFixedParameter = availableElement.FixedParameter != null,
                SelectedParameter = availableElement.FixedParameter,
                IsEditing = IsEditing
            };

            element.UpdateDescription();

            _elements.Add(element);

            string st; // needed

            switch (availableElement.ActionCategory)
            {
                case DiplomacyMessageElementActionCategory.Offer:
                    _offerElements.Add(element);
                    if (element.Tone == Tone.Indignant)
                    {
                        st = ResourceManager.GetString("OFFER_DIALOG_HINT"); // need to update the embassy screen with a new window to get the send button activated without delay.
                        _ = MessageDialog.Show(st, MessageDialogButtons.Ok);
                        GameLog.Client.DiplomacyDetails.DebugFormat("OFFER_DIALOG_HINT is outcommented");
                    }
                    break;
                case DiplomacyMessageElementActionCategory.Request:
                    _requestElements.Add(element);
                    if (element.Tone == Tone.Indignant)
                    {
                        st = ResourceManager.GetString("REQUEST_DIALOG_HINT"); // need to update the embassy screen with a new window to get the send button activated without delay.
                        _ = MessageDialog.Show(st, MessageDialogButtons.Ok);
                        GameLog.Client.DiplomacyDetails.DebugFormat("REQUEST_DIALOG_HINT is outcommented");
                    }
                    break;
                case DiplomacyMessageElementActionCategory.Propose:
                    _treatyElements.Add(element);
                    //if (element != null && element.Description != null && element.SelectedParameter != null && element.ElementType != null)
                    if (element != null && element.Description != null && element.SelectedParameter != null /*&& element.ElementType != null*/)
                    {
                        GameLog.Client.DiplomacyDetails.DebugFormat("### Proposal element added to _treatyElemetns, {0}, {1}, {2}",
                            element.Description.ToString(),
                            element.SelectedParameter.ToString(),
                            element.ElementType.ToString());
                    }
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                    st = ResourceManager.GetString("PROPOSE_DIALOG_HINT"); // need to update the embassy screen with a new window to get the send button activated without delay.
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                    //var result_Propose = MessageDialog.Show(st, MessageDialogButtons.Ok);
                    GameLog.Client.DiplomacyDetails.DebugFormat("PROPOSE_DIALOG_HINT is outcommented");
                    break;
                case DiplomacyMessageElementActionCategory.Commend:
                    _statementElements.Add(element);
                    break;
                case DiplomacyMessageElementActionCategory.Denounce:
                    _statementElements.Add(element);
                    //if (element.Tone == Tone.Indignant) // and case = Denounce - both are the definition of StealCredits
                    //{
                    //    IntelHelper.ExecuteStealCredits(Sender, Recipient, "dip_Terrorists");
                    //}
                    break;
                case DiplomacyMessageElementActionCategory.WarDeclaration:
                    st = ResourceManager.GetString("DECLARE_WAR_DIALOG_HINT"); // need to update the embassy screen with a new window to get the send button activated without delay.
                    _ = MessageDialog.Show(st, MessageDialogButtons.Ok);
                    GameLog.Client.DiplomacyDetails.DebugFormat("DECLARE_WAR_DIALOG_HINT is outcommented");
                    _statementElements.Add(element);
                    break;
            }

            PopulateAvailableElements();
            UpdateLeadInText();
        }

        private void RemoveElement(DiplomacyMessageElement element)
        {
            if (element == null)
            {
                return;
            }

            _ = _elements.Remove(element);

            switch (element.ActionCategory)
            {
                case DiplomacyMessageElementActionCategory.Offer:
                    _ = _offerElements.Remove(element);
                    break;
                case DiplomacyMessageElementActionCategory.Request:
                    _ = _requestElements.Remove(element);
                    break;
                case DiplomacyMessageElementActionCategory.Propose:
                    _ = _treatyElements.Remove(element);
                    break;
                case DiplomacyMessageElementActionCategory.Commend:
                case DiplomacyMessageElementActionCategory.Denounce:
                case DiplomacyMessageElementActionCategory.WarDeclaration:
                    _ = _statementElements.Remove(element);
                    break;
            }

            PopulateAvailableElements();
            UpdateLeadInText();
        }

        private NewProposal CreateProposal(bool allowIncomplete = false)
        {
            if (_elements.Count == 0)
            {
                return null;
            }

            List<Clause> clauses = new List<Clause>();

            foreach (DiplomacyMessageElement element in _elements)
            {
                ClauseType clauseType = DiplomacyScreenViewModel.ElementTypeToClauseType(element.ElementType);
                // GameLog.Client.Diplomacy.DebugFormat("((()))ElementTypeToClause out Clause ={0}", DiplomacyScreenViewModel.ElementTypeToClauseType(element.ElementType).ToString());
                if (clauseType == ClauseType.NoClause)
                {
                    continue;
                }

                if (element.HasParameter)
                {
                    object selectedParameter = element.SelectedParameter;
                    if (selectedParameter == null && !allowIncomplete)
                    {
                        continue;
                    }

                    if (selectedParameter is IClauseParameterInfo parameterInfo)
                    {
                        if (parameterInfo.IsParameterValid)
                        {
                            selectedParameter = parameterInfo.GetParameterData();
                        }
                        else if (!allowIncomplete)
                        {
                            continue;
                        }
                    }

                    //
                    // It's possible for 'selectedParameter' to be null here.  We assume this is okay
                    // if IClauseParameterInfo.IsParameterValid returned 'true'.
                    //
                    clauses.Add(new Clause(clauseType, selectedParameter));
                }
                else
                {
                    clauses.Add(new Clause(clauseType));
                }
            }

            if (clauses.Count == 0)
            {
                return null;
            }

            foreach (Clause clause in clauses)
            {

                //GameLog.Core.Diplomacy.DebugFormat("((()))Create Proposal sender {0}, Recipient = {1}: Tone = {2} clause type = {3} data = {4} duration = {5}",
                _text =
                "Turn " + GameContext.Current.TurnNumber
                + ": Proposal created: Sender " + Sender.ShortName
                + " to > " + _recipient.ShortName
                + ": " + clause.ClauseType.ToString()
                + " ( " + _tone + "," + clause.Duration + clause.Data + " )"
                ;
                //Console.WriteLine(_text);
                GameLog.Core.Diplomacy.DebugFormat(_text);
                // if ClauseType == TreatyWarPact then clause.Data = string shortname of target civilization
            }
            return new NewProposal(Sender, _recipient, clauses);
        }

        private Statement CreateStatement()
        {
            if (_elements.Count != 1)
            {
                return null;
            }

            StatementType statementType = DiplomacyScreenViewModel.ElementTypeToStatementType(_elements[0].ElementType);
            if (statementType == StatementType.NoStatement)
            {
                return null;
            }
            //if(statementType != StatementType.NoStatement)
            //GameLog.Core.Diplomacy.DebugFormat("((()))Create Statement {0} *vs* Recipient = {1}: Tone = {2}  StatementType = {3} ",
            // _sender.ShortName, _recipient.ShortName, _tone, statementType.ToString());

            return new Statement(Sender, _recipient, statementType, _tone);
        }

        private void PopulateAvailableElements()
        {


            _availableElements.Clear();

            Diplomat diplomat = GameContext.Current.Diplomats[Sender];
            NewProposal currentProposal = CreateProposal(allowIncomplete: true);
            Statement currentStatement = CreateStatement();
            bool recipientIsMember = DiplomacyHelper.IsMember(_recipient, Sender);


            // Statements must be the only element in a message.


            if (_elements.Count == 0)
            {
                if (diplomat.CanCommendOrDenounceWar(_recipient, currentStatement))
                {
                    IEnumerable<Civilization> denouceWarParameters() => diplomat.GetCommendOrDenounceWarParameters(_recipient, currentStatement).ToList();

                    IEnumerable<Civilization> commendWarParameters() => denouceWarParameters().Where(
                        c =>
                        {
                            ForeignPowerStatus status = GameContext.Current.DiplomacyData[Sender, c].Status;
                            return status != ForeignPowerStatus.Affiliated &&
                                   status != ForeignPowerStatus.Allied &&
                                   status != ForeignPowerStatus.Friendly;
                        }).ToList();

                    /*
                     * No commending wars being fought against our friends...
                     */
                    if (commendWarParameters().Any())
                    {
                        _availableElements.Add(
                            new DiplomacyMessageAvailableElement
                            {
                                ActionCategory = DiplomacyMessageElementActionCategory.Commend,
                                ParametersCallback = commendWarParameters,
                                ElementType = DiplomacyMessageElementType.CommendWarStatement
                            });
                    }

                    _availableElements.Add(
                        new DiplomacyMessageAvailableElement
                        {
                            ActionCategory = DiplomacyMessageElementActionCategory.Denounce,
                            ParametersCallback = denouceWarParameters,
                            ElementType = DiplomacyMessageElementType.DenounceWarStatement
                        });
                }

                if (diplomat.CanCommendOrDenounceTreaty(_recipient, currentStatement))
                {
                    _availableElements.Add(
                        new DiplomacyMessageAvailableElement
                        {
                            ActionCategory = DiplomacyMessageElementActionCategory.Commend,
                            ParametersCallback = () => diplomat.GetCommendOrDenounceTreatyParameters(_recipient, currentStatement).ToList(),
                            ElementType = DiplomacyMessageElementType.CommendTreatyStatement
                        });

                    _availableElements.Add(
                        new DiplomacyMessageAvailableElement
                        {
                            ActionCategory = DiplomacyMessageElementActionCategory.Denounce,
                            ParametersCallback = () => diplomat.GetCommendOrDenounceTreatyParameters(_recipient, currentStatement).ToList(),
                            ElementType = DiplomacyMessageElementType.CommendTreatyStatement
                        });
                }

                if (diplomat.CanProposeWarPact(_recipient, currentProposal))
                {
                    _availableElements.Add(
                        new DiplomacyMessageAvailableElement
                        {
                            ActionCategory = DiplomacyMessageElementActionCategory.Propose,
                            ParametersCallback = () => diplomat.GetWarPactParameters(_recipient, currentProposal).ToList(),
                            ElementType = DiplomacyMessageElementType.TreatyWarPact
                        });
                }
                // add the war buttons: 1) for Declare War on lower left and 2) inside 'New Message' see declare war opption
                if (!DiplomacyHelper.AreAtWar(Sender, _recipient))
                {
                    _availableElements.Add(
                        new DiplomacyMessageAvailableElement
                        {
                            ActionCategory = DiplomacyMessageElementActionCategory.WarDeclaration,
                            ElementType = DiplomacyMessageElementType.WarDeclaration
                        });
                }
            }

            bool anyActiveStatements = _elements.Any(o => o.ElementType <= DiplomacyMessageElementType.DenounceSabotageStatement);
            if (!anyActiveStatements)
            {
                /*
                 * Request...
                 */
                if (!recipientIsMember &&
                    _elements.All(
                        o => o.ElementType != DiplomacyMessageElementType.RequestGiveCreditsClause &&
                             o.ElementType != DiplomacyMessageElementType.OfferGiveCreditsClause))
                {
                    _availableElements.Add(
                        new DiplomacyMessageAvailableElement
                        {
                            ActionCategory = DiplomacyMessageElementActionCategory.Request,
                            ElementType = DiplomacyMessageElementType.RequestGiveCreditsClause,
                            FixedParameter = new CreditsDataViewModel(Diplomat.Get(Sender).OwnerTreasury)
                        });
                }

                IEnumerable<Civilization> requestHonorMilitaryAgreementParameters = diplomat.GetRequestHonorMilitaryAgreementParameters(_recipient, currentProposal);
                if (requestHonorMilitaryAgreementParameters.Any())
                {
                    _availableElements.Add(
                        new DiplomacyMessageAvailableElement
                        {
                            ActionCategory = DiplomacyMessageElementActionCategory.Request,
                            ElementType = DiplomacyMessageElementType.RequestHonorMilitaryAgreementClause,
                            ParametersCallback = () => diplomat.GetRequestHonorMilitaryAgreementParameters(_recipient, currentProposal)
                        });
                }

                /* 
                 * Propose...
                 */
                if (diplomat.CanProposeCeaseFire(_recipient, currentProposal))
                {
                    _availableElements.Add(
                        new DiplomacyMessageAvailableElement
                        {
                            ActionCategory = DiplomacyMessageElementActionCategory.Propose,
                            ElementType = DiplomacyMessageElementType.TreatyCeaseFireClause
                        });
                }

                if (diplomat.CanProposeNonAggressionTreaty(_recipient, currentProposal))
                {
                    _availableElements.Add(
                        new DiplomacyMessageAvailableElement
                        {
                            ActionCategory = DiplomacyMessageElementActionCategory.Propose,
                            ElementType = DiplomacyMessageElementType.TreatyNonAggressionClause
                        });
                }

                if (diplomat.CanProposeOpenBordersTreaty(_recipient, currentProposal))
                {
                    _availableElements.Add(
                        new DiplomacyMessageAvailableElement
                        {
                            ActionCategory = DiplomacyMessageElementActionCategory.Propose,
                            ElementType = DiplomacyMessageElementType.TreatyOpenBordersClause
                        });
                }

                if (diplomat.CanProposeAffiliation(_recipient, currentProposal))
                {
                    _availableElements.Add(
                        new DiplomacyMessageAvailableElement
                        {
                            ActionCategory = DiplomacyMessageElementActionCategory.Propose,
                            ElementType = DiplomacyMessageElementType.TreatyAffiliationClause
                        });
                }

                if (diplomat.CanProposeDefensiveAlliance(_recipient, currentProposal))
                {
                    _availableElements.Add(
                        new DiplomacyMessageAvailableElement
                        {
                            ActionCategory = DiplomacyMessageElementActionCategory.Propose,
                            ElementType = DiplomacyMessageElementType.TreatyDefensiveAllianceClause
                        });
                }

                if (diplomat.CanProposeFullAlliance(_recipient, currentProposal))
                {
                    _availableElements.Add(
                        new DiplomacyMessageAvailableElement
                        {
                            ActionCategory = DiplomacyMessageElementActionCategory.Propose,
                            ElementType = DiplomacyMessageElementType.TreatyFullAllianceClause
                        });
                }

                if (diplomat.CanProposeMembership(_recipient, currentProposal))
                {
                    _availableElements.Add(
                        new DiplomacyMessageAvailableElement
                        {
                            ActionCategory = DiplomacyMessageElementActionCategory.Propose,
                            ElementType = DiplomacyMessageElementType.TreatyMembershipClause
                        });
                }

                /*
                 * Offer...
                 */
                if (!recipientIsMember &&
                    _elements.All(
                        o => o.ElementType != DiplomacyMessageElementType.RequestGiveCreditsClause &&
                             o.ElementType != DiplomacyMessageElementType.OfferGiveCreditsClause))
                {
                    _availableElements.Add(
                        new DiplomacyMessageAvailableElement
                        {
                            ActionCategory = DiplomacyMessageElementActionCategory.Offer,
                            ElementType = DiplomacyMessageElementType.OfferGiveCreditsClause,
                            FixedParameter = new CreditsDataViewModel(Diplomat.Get(Sender).OwnerTreasury)
                        });
                }

                IEnumerable<Civilization> offerHonorMilitaryAgreementParameters = diplomat.GetOfferHonorMilitaryAgreementParameters(_recipient, currentProposal);
                if (offerHonorMilitaryAgreementParameters.Any())
                {
                    _availableElements.Add(
                        new DiplomacyMessageAvailableElement
                        {
                            ActionCategory = DiplomacyMessageElementActionCategory.Offer,
                            ElementType = DiplomacyMessageElementType.OfferHonorMilitaryAgreementClause,
                            ParametersCallback = () => diplomat.GetOfferHonorMilitaryAgreementParameters(_recipient, currentProposal)
                        });
                }
            }

            foreach (DiplomacyMessageAvailableElement availableElement in _availableElements)
            {
                DiplomacyMessageAvailableElement elementCopy = availableElement; // modified closure

                availableElement.AddCommand = new DelegateCommand(
                    () => AddElement(elementCopy),
                    () => IsEditing);
            }


        }

        public void InvalidateCommands()
        {
            _setAcceptButton.RaiseCanExecuteChanged();
            _setRejectButton.RaiseCanExecuteChanged();
            _removeElementCommand.RaiseCanExecuteChanged();
        }

        private void ExecuteSetAcceptButton(ICheckableCommandParameter p)
        {
            if (!CanExecuteSetAcceptButton(p))
            {
                return;
            }

            bool accepting = true;
            ProcessAcceptReject(accepting);

            InvalidateCommands();
        }

        private bool CanExecuteSetAcceptButton(ICheckableCommandParameter p)
        {
            if (p == null)
            {
                return false;
            }

            return true;
        }

        private void ExecuteSetRejectButton(ICheckableCommandParameter p)
        {
            if (!CanExecuteSetRejectButton(p))
            {
                return;
            }

            bool accepting = false;
            ProcessAcceptReject(accepting);

            InvalidateCommands();
        }

        private void ProcessAcceptReject(bool accepting)
        {

            int turn = GameContext.Current.TurnNumber;

            Civilization playerEmpire = DiplomacyScreenViewModel.DesignInstance.LocalPalyer; // local player reciever of proposal treaty
            Diplomat diplomat = Diplomat.Get(playerEmpire);

            Civilization senderCiv = DiplomacyHelper.DiploScreenSelectedForeignPower;
            ForeignPower selectedForeignPower = diplomat.GetForeignPower(senderCiv);

            //bool localPlayerIsHosting = DiplomacyScreenViewModel.DesignInstance.localIsHost;
            string Accepted = "ACCEPTED";
            if (accepting == false)
            {
                Accepted = "REJECTED";
            }

            Response = Accepted;
            int selectedID = selectedForeignPower.Owner.CivID;

            if (AcceptedRejected.ContainsKey(selectedID))
            {
                if (AcceptedRejected[selectedID] != Accepted)
                {
                    _ = AcceptedRejected.Remove(selectedID);
                    AcceptedRejected.Add(selectedID, Accepted);
                }
            }
            else
            {
                AcceptedRejected.Add(selectedID, Accepted);
            }

            //GameLog.Client.Diplomacy.DebugFormat("Local player IS Host....");
            DiplomacyHelper.AcceptRejectDictionary(selectedForeignPower, accepting, turn); // creat entry for game host

            // creat entry for none host human player that clicked the accept - reject radio button         
            StatementType _statementType = DiplomacyHelper.GetStatementType(accepting, senderCiv, playerEmpire); // first is bool, 2nd sender ID(now the local player), last new receipient, in Dictinary Key                       
            GameLog.Client.Diplomacy.DebugFormat("Local player IS NOT Host, statementType = {0} accepting = {1} sender ={2} counterpartyID {3} local = {4} OwnerID ={5}"
                , Enum.GetName(typeof(StatementType), _statementType)
                , accepting
                , senderCiv.Key
                , selectedForeignPower.CounterpartyID
                , playerEmpire.Key
                , selectedForeignPower.OwnerID
                );
            if (_statementType != StatementType.NoStatement)
            {
                Statement statementToSend = new Statement(playerEmpire, senderCiv, _statementType, Tone.Receptive, turn);
                _sendOrder = new SendStatementOrder(statementToSend);
                ServiceLocator.Current.GetInstance<IPlayerOrderService>().AddOrder(_sendOrder);

                selectedForeignPower.StatementSent = statementToSend; // load statement to send in foreignPower, statment type carries key for dictionary entery

                GameLog.Client.Diplomacy.DebugFormat("!! foreignPower.StatementSent *other*ForeignPower Recipient ={0} to Sender ={1}"
                    , statementToSend.Recipient.Key
                    , statementToSend.Sender.Key
                    );
            }
        }

        private bool CanExecuteSetRejectButton(ICheckableCommandParameter p)
        {
            if (p == null)
            {
                return false;
            }

            return true;
        }

        public static DiplomacyMessageViewModel FromReponse([NotNull] IResponse response)
        {
            GameLog.Core.Diplomacy.DebugFormat("$$ at FromResponse() proposal turnSent ={0} tone ={1} recipient ={2} sender ={3} responce type = {4} proposal clause type ={5}"
                , response.Proposal.TurnSent
                , response.Tone
                , response.Recipient.ShortName
                , response.Sender.ShortName
                , response.ResponseType.ToString()
                , response.Proposal.Clauses[0].ClauseType.ToString());
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            DiplomacyStringID leadInId;

            switch (response.ResponseType)
            {
                case ResponseType.NoResponse:
                    return null;
                case ResponseType.Accept:
                    if (response.Proposal.IsGift())
                    {
                        leadInId = DiplomacyStringID.AcceptGiftLeadIn;
                    }
                    else
                    {
                        leadInId = response.Proposal.IsDemand()
                            ? DiplomacyStringID.AcceptDemandLeadIn
                            : !response.Proposal.HasTreaty() ? DiplomacyStringID.AcceptExchangeLeadIn : DiplomacyStringID.AcceptProposalLeadIn;
                    }

                    break;
                case ResponseType.Reject:
                    if (response.Proposal.IsGift())
                    {
                        leadInId = DiplomacyStringID.RejectProposalLeadIn; // should not happen
                    }
                    else
                    {
                        leadInId = response.Proposal.IsDemand()
                            ? DiplomacyStringID.RejectDemandLeadIn
                            : !response.Proposal.HasTreaty() ? DiplomacyStringID.RejectExchangeLeadIn : DiplomacyStringID.RejectProposalLeadIn;
                    }

                    break;
                case ResponseType.Counter:
                    leadInId = DiplomacyStringID.CounterProposalLeadIn;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            DiplomacyMessageViewModel message = new DiplomacyMessageViewModel(response.Sender, response.Recipient)
            {
                Tone = response.Proposal.Tone,
            };

            message._treatyLeadInTextScript.ScriptCode = QuoteString(LookupDiplomacyText(leadInId, message._tone, message.Sender) ?? string.Empty);
            message.TreatyLeadInText = message._treatyLeadInTextScript.Evaluate<string>(message._leadInRuntimeParameters);
            GameLog.Core.Diplomacy.DebugFormat("message ={0}", message);
            return message;
        }

        public static DiplomacyMessageViewModel FromProposal([NotNull] IProposal proposal)
        {
            GameLog.Core.Diplomacy.DebugFormat("$$ at FromProposal() with turnSent ={0} Recipient ={1} sender ={2}", proposal.TurnSent, proposal.Recipient, proposal.Sender);
            if (proposal == null)
            {
                throw new ArgumentNullException("proposal");
            }

            DiplomacyStringID leadInId;

            switch (proposal.Clauses[0].ClauseType) // not all cases used below, ToDo
            {
                case ClauseType.TreatyOpenBorders:
                    leadInId = DiplomacyStringID.OpenBordersClause;
                    break;
                case ClauseType.TreatyAffiliation:
                    leadInId = DiplomacyStringID.AffiliationClause;
                    break;
                case ClauseType.TreatyCeaseFire:
                    leadInId = DiplomacyStringID.CeaseFireClause;
                    break;
                case ClauseType.TreatyDefensiveAlliance:
                    leadInId = DiplomacyStringID.DefensiveAllianceClause;
                    break;
                case ClauseType.TreatyFullAlliance:
                    leadInId = DiplomacyStringID.FullAllianceClause;
                    break;
                case ClauseType.TreatyMembership:
                    leadInId = DiplomacyStringID.MembershipClause;
                    break;
                case ClauseType.TreatyNonAggression:
                    leadInId = DiplomacyStringID.NonAggressionPactClause;
                    break;
                case ClauseType.TreatyWarPact:
                    leadInId = DiplomacyStringID.WarPactClause;
                    break;
                default:
                    leadInId = DiplomacyStringID.None;
                    break;
            }

            DiplomacyMessageViewModel message = new DiplomacyMessageViewModel(proposal.Sender, proposal.Recipient)
            {
                Tone = proposal.Tone,
            };
            if (proposal.IsWarPact())
            {
                string target = proposal.Clauses[0].Data.ToString();
                message.TreatyLeadInText = string.Format(ResourceManager.GetString("WAR_PACT_LEADIN"), proposal.Recipient.ShortName, proposal.Sender.ShortName, target);

            }
            else
            {
                message._treatyLeadInTextScript.ScriptCode = QuoteString(LookupDiplomacyText(leadInId, message._tone, message.Sender) ?? string.Empty);
                message.TreatyLeadInText = message._treatyLeadInTextScript.Evaluate<string>(message._leadInRuntimeParameters);
            }
            //GameLog.Core.Diplomacy.DebugFormat("message ={0}", message.TreatyLeadInText);
            return message;
        }


#pragma warning disable CS0067 // The event 'DiplomacyMessageViewModel.PropertyChanged' is never used
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067 // The event 'DiplomacyMessageViewModel.PropertyChanged' is never used

        public void OnPropertyChanged(bool placeHolder, string propertyName)
        {
            if (placeHolder)
            {
                _propertyChanged.Raise(this, propertyName);
            }
        }

        #region Implementation of INotifyPropertyChanged

        [NonSerialized] private PropertyChangedEventHandler _propertyChanged;
        private string _text;

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
    }
}
