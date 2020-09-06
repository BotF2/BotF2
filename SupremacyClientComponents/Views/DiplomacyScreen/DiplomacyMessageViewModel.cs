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
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.ServiceLocation;

using Supremacy.Annotations;
using Supremacy.Client.Context;
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
        private readonly Civilization _sender;
        private readonly Civilization _recipient;
        private readonly ObservableCollection<DiplomacyMessageElement> _elements;
        private readonly ObservableCollection<DiplomacyMessageElement> _offerElements;
        private readonly ObservableCollection<DiplomacyMessageElement> _requestElements;
        private readonly ObservableCollection<DiplomacyMessageElement> _statementElements;
        private readonly ObservableCollection<DiplomacyMessageElement> _treatyElements;
        //private readonly ObservableCollection<DiplomacyMessageElement> _acceptRejectElements;
        private readonly ReadOnlyObservableCollection<DiplomacyMessageElement> _elementsView;
        private readonly ReadOnlyObservableCollection<DiplomacyMessageElement> _offerElementsView;
        private readonly ReadOnlyObservableCollection<DiplomacyMessageElement> _requestElementsView;
        private readonly ReadOnlyObservableCollection<DiplomacyMessageElement> _statementElementsView;
        private readonly ReadOnlyObservableCollection<DiplomacyMessageElement> _treatyElementsView;
       // private readonly ReadOnlyObservableCollection<DiplomacyMessageElement> _acceptRejectElementsView; // no view of this???
        private readonly ObservableCollection<DiplomacyMessageAvailableElement> _availableElements;
        private readonly ReadOnlyObservableCollection<DiplomacyMessageAvailableElement> _availableElementsView;

        private readonly DelegateCommand<DiplomacyMessageElement> _removeElementCommand;

        private readonly ScriptExpression _treatyLeadInTextScript;
        private readonly ScriptExpression _offerLeadInTextScript;
        private readonly ScriptExpression _requestLeadInTextScript;

        private readonly ScriptParameters _leadInParameters;
        private readonly RuntimeScriptParameters _leadInRuntimeParameters;
        private readonly DelegateCommand<ICheckableCommandParameter> _setAcceptButton;
        private readonly DelegateCommand<ICheckableCommandParameter> _setRejectButton;
        private Dictionary<int, string> _acceptedRejected = new Dictionary<int, string> { { 999, "placeHolder" } };
        private Order _sendOrder;
        private string _response = "....";
        int _turnOfResponse;

        public DiplomacyMessageViewModel([NotNull] Civilization sender, [NotNull] Civilization recipient)
        {
            if (sender == null)
                throw new ArgumentNullException("sender");
            if (recipient == null)
                throw new ArgumentNullException("recipient");
            _setAcceptButton = new DelegateCommand<ICheckableCommandParameter>(ExecuteSetAcceptButton, CanExecuteSetAcceptButton);
            _setRejectButton = new DelegateCommand<ICheckableCommandParameter>(ExecuteSetRejectButton, CanExecuteSetRejectButton);
            _sender = sender;
            _recipient = recipient;
            _elements = new ObservableCollection<DiplomacyMessageElement>();
            _elementsView = new ReadOnlyObservableCollection<DiplomacyMessageElement>(_elements);
            _availableElements = new ObservableCollection<DiplomacyMessageAvailableElement>();
            _availableElementsView = new ReadOnlyObservableCollection<DiplomacyMessageAvailableElement>(_availableElements);
            _treatyElements = new ObservableCollection<DiplomacyMessageElement>();
            _treatyElementsView = new ReadOnlyObservableCollection<DiplomacyMessageElement>(_treatyElements);

            _offerElements = new ObservableCollection<DiplomacyMessageElement>();
            _offerElementsView = new ReadOnlyObservableCollection<DiplomacyMessageElement>(_offerElements);
            _requestElements = new ObservableCollection<DiplomacyMessageElement>();
            _requestElementsView = new ReadOnlyObservableCollection<DiplomacyMessageElement>(_requestElements);
            _statementElements = new ObservableCollection<DiplomacyMessageElement>();
            _statementElementsView = new ReadOnlyObservableCollection<DiplomacyMessageElement>(_statementElements);

            _removeElementCommand = new DelegateCommand<DiplomacyMessageElement>(
                ExecuteRemoveElementCommand,
                CanExecuteRemoveElementCommand);

            _leadInParameters = new ScriptParameters(
                new ScriptParameter("$sender", typeof(Civilization)),
                new ScriptParameter("$recipient", typeof(Civilization)));

            _leadInRuntimeParameters = new RuntimeScriptParameters
                                       {
                                           new RuntimeScriptParameter(_leadInParameters[0], _sender),
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

            CollectionViewSource.GetDefaultView(_availableElementsView).GroupDescriptions.Add(new PropertyGroupDescription("ActionDescription"));
        }

        private bool CanExecuteRemoveElementCommand(DiplomacyMessageElement element)
        {
            return IsEditing && element != null && _elements.Contains(element);
        }

        private void ExecuteRemoveElementCommand(DiplomacyMessageElement element)
        {
            if (!CanExecuteRemoveElementCommand(element))
                return;

            RemoveElement(element);
        }

        public ICommand SetAcceptButton
        {
            get { return _setAcceptButton; }
        }

        public ICommand SetRejectButton
        {
            get { return _setRejectButton; }
        }

        public string Response
        {
            get
            {
                int selectedID = 888;
                if (DiplomacyScreenViewModel.DesignInstance.SelectedForeignPower != null)
                {
                    selectedID = DiplomacyScreenViewModel.DesignInstance.SelectedForeignPower.Owner.CivID;
                }
                if (_acceptedRejected.ContainsKey(selectedID))
                    return _acceptedRejected[selectedID];
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
                        _acceptedRejected.Clear(); 
                        _turnOfResponse = turn;
                    }
                    int selectedID = DiplomacyScreenViewModel.DesignInstance.SelectedForeignPower.Owner.CivID;
                    if (_acceptedRejected.ContainsKey(selectedID))
                    {
                        _acceptedRejected.Remove(selectedID);
                        _acceptedRejected.Add(selectedID, value);
                    }
                    else _acceptedRejected.Add(selectedID, value);
                    _response = value;
                    OnPropertyChanged(true, "Response");
                }
            }
        }

        public Dictionary<int, string> AcceptedRejected
        {
            get { return _acceptedRejected; }
        }

        public Civilization Sender
        {
            get { return _sender; }
        }

        public Civilization Recipient
        {
            get { return _recipient; }
        }

        public ReadOnlyObservableCollection<DiplomacyMessageElement> Elements
        {
            get { return _elementsView; }
        }

        public ReadOnlyObservableCollection<DiplomacyMessageElement> TreatyElements
        {
            get { return _treatyElementsView; }
        }

        public ReadOnlyObservableCollection<DiplomacyMessageElement> RequestElements
        {
            get { return _requestElementsView; }
        }

        public ReadOnlyObservableCollection<DiplomacyMessageElement> OfferElements
        {
            get { return _offerElementsView; }
        }

        public ReadOnlyObservableCollection<DiplomacyMessageElement> StatementElements
        {
            get { return _statementElementsView; }
        }

        //public ReadOnlyObservableCollection<DiplomacyMessageElement> AcceptRejectElements  // we do not view this??? 
        //{
        //    get { return _acceptRejectElementsView; }
        //}

        public ReadOnlyObservableCollection<DiplomacyMessageAvailableElement> AvailableElements
        {
            get { return _availableElementsView; }
        }

        internal bool IsStatement
        {
            get { return _elements.All(o => o.ElementType <= DiplomacyMessageElementType.DenounceSabotageStatement); }
        }
        internal bool IsTreaty
        {
            get { return _elements.All(o => o.ElementType > DiplomacyMessageElementType.TreatyWarPact); }
        }
        public int HideAcceptRejectButtons
        {
            get
            {
                var sender = _sender;
                var recipient = _recipient;
                var diplomat = Diplomat.Get(recipient);
                var foreignPower = diplomat.GetForeignPower(sender);

                int decider = 1; // accept and reject buttons hidden if decider = 1, exposed if decider = 0.
                if (IsTreaty)
                    decider = 0;
                if (foreignPower.ResponseReceived != null)
                    decider = 1;
                if (decider == 0)
                    return 0;
                else return 1;
            }
        }

        internal IDiplomaticExchange CreateMessage()
            {
                return IsStatement ? (IDiplomaticExchange)CreateStatement() : CreateProposal();
            }

        public void Send()
        {
            var isStatement = _elements.All(o => o.ElementType <= DiplomacyMessageElementType.DenounceSabotageStatement);
            if (isStatement)
            {
                var statement = CreateStatement();
                if (statement == null)
                    return;

                _sendOrder = new SendStatementOrder(statement);

                ServiceLocator.Current.GetInstance<IPlayerOrderService>().AddOrder(_sendOrder);
            }
            else
            {
                var proposal = CreateProposal();
                if (proposal == null)
                    return;

                _sendOrder = new SendProposalOrder(proposal);

                ServiceLocator.Current.GetInstance<IPlayerOrderService>().AddOrder(_sendOrder);
            }

            IsEditing = false;

            _availableElements.Clear();
        }

        public void Edit()
        {
            if (_sendOrder != null)
                ServiceLocator.Current.GetInstance<IPlayerOrderService>().RemoveOrder(_sendOrder);
            
            _sendOrder = null;
            
            IsEditing = true;
            PopulateAvailableElements();
        }

        public void Cancel()
        {
            if (_sendOrder != null)
                ServiceLocator.Current.GetInstance<IPlayerOrderService>().RemoveOrder(_sendOrder);

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
            get { return _tone; }
            set
            {
                if (Equals(value, _tone))
                    return;

                _tone = value;
                _elements.ForEach(o => o.Tone = value);

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
            get { return _isEditing; }
            private set
            {
                if (Equals(value, _isEditing))
                    return;

                _isEditing = value;
                _elements.ForEach(o => o.IsEditing = value);

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
            get { return _offerLeadInText; }
            private set
            {
                if (Equals(value, _offerLeadInText))
                    return;

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

        public bool HasOfferLeadInText
        {
            get { return !string.IsNullOrWhiteSpace(OfferLeadInText); }
        }

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
            get { return _requestRequestLeadInText; }
            private set
            {
                if (Equals(value, _requestRequestLeadInText))
                    return;

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

        public bool HasRequestLeadInText
        {
            get { return !string.IsNullOrWhiteSpace(RequestLeadInText); }
        }

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
            get { return _treatyLeadInText; }
            private set
            {
                if (Equals(value, _treatyLeadInText))
                    return;

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

        public bool HasTreatyLeadInText
        {
            get { return !string.IsNullOrWhiteSpace(TreatyLeadInText); }
        }

        protected virtual void OnHasTreatyLeadInTextChanged()
        {
            HasTreatyLeadInTextChanged.Raise(this);
            OnPropertyChanged("HasTreatyLeadInText");
        }
        #endregion

        private void UpdateLeadInText()
        {
            var treatyLeadInId = DiplomacyStringID.None;
            var offerLeadInId = DiplomacyStringID.None;
            var requestLeadInId = DiplomacyStringID.None;

            if (_treatyElements.Count != 0)
            {
                var isWarPact = _treatyElements[0].ElementType == DiplomacyMessageElementType.TreatyWarPact; // bool  

                treatyLeadInId = isWarPact ? DiplomacyStringID.WarPactLeadIn : DiplomacyStringID.ProposalLeadIn;
                if (treatyLeadInId == DiplomacyStringID.ProposalLeadIn)
                    GameLog.Client.Diplomacy.DebugFormat("** Treaty Leadin text set, {0}", treatyLeadInId.ToString());

                if (_offerElements.Count != 0)
                    offerLeadInId = isWarPact ? DiplomacyStringID.WarPactOffersLeadIn : DiplomacyStringID.ProposalOffersLeadIn;
                if (_requestElements.Count != 0)
                    requestLeadInId = isWarPact ? DiplomacyStringID.WarPactDemandsLeadIn : DiplomacyStringID.ProposalDemandsLeadIn;
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
                _treatyLeadInTextScript.ScriptCode = QuoteString(LookupDiplomacyText(treatyLeadInId, _tone, _sender) ?? string.Empty);
                TreatyLeadInText = _treatyLeadInTextScript.Evaluate<string>(_leadInRuntimeParameters);
            }
            
            if (offerLeadInId == DiplomacyStringID.None)
            {
                OfferLeadInText = null;
            }
            else
            {
                _offerLeadInTextScript.ScriptCode = QuoteString(LookupDiplomacyText(offerLeadInId, _tone, _sender) ?? string.Empty);
                OfferLeadInText = _offerLeadInTextScript.Evaluate<string>(_leadInRuntimeParameters);
            }

            if (requestLeadInId == DiplomacyStringID.None)
            {
                RequestLeadInText = null;
            }
            else
            {
                _requestLeadInTextScript.ScriptCode = QuoteString(LookupDiplomacyText(requestLeadInId, _tone, _sender) ?? string.Empty);
                RequestLeadInText = _requestLeadInTextScript.Evaluate<string>(_leadInRuntimeParameters);
            }
        }

        internal static string QuoteString(string value)
        {
            if (value == null)
                return null;

            var sb = new StringBuilder(value.Length + 2);
            var bracketDepth = 0;

            sb.Append('"');

            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                var last = i == 0 ? '\0' : value[i - 1];
                if (c == '{' && last != '\\')
                    ++bracketDepth;
                else if (c == '}' && last != '\\')
                    --bracketDepth;
                else if (c == '"' && bracketDepth == 0)
                    sb.Append('\\');
                sb.Append(c);
            }
            
            sb.Append('"');

            return sb.ToString();
        }

        internal static string LookupDiplomacyText(DiplomacyStringID stringId, Tone tone, Civilization sender)
        {
            var civStringKey = new DiplomacyStringKey(sender != null ? sender.Key : null, stringId);

            LocalizedString localizedString;

            LocalizedTextGroup civTextGroup;
            LocalizedTextGroup defaultTextGroup;

            if (LocalizedTextDatabase.Instance.Groups.TryGetValue(civStringKey, out civTextGroup) &&
                civTextGroup.Entries.TryGetValue(tone, out localizedString))
            {
                return localizedString.LocalText;
            }

            var defaultStringKey = new DiplomacyStringKey(null, stringId);

            if (LocalizedTextDatabase.Instance.Groups.TryGetValue(defaultStringKey, out defaultTextGroup) &&
                defaultTextGroup.Entries.TryGetValue(tone, out localizedString))
            {
                return localizedString.LocalText;
            }

            if (civTextGroup != null && civTextGroup.DefaultEntry != null)
                return civTextGroup.DefaultEntry.LocalText;

            if (defaultTextGroup != null && defaultTextGroup.DefaultEntry != null)
                return defaultTextGroup.DefaultEntry.LocalText;

            return null;
        }

        internal void AddElement([NotNull] DiplomacyMessageAvailableElement availableElement)
        {
            if (availableElement == null)
                throw new ArgumentNullException("availableElement");

            var element = new DiplomacyMessageElement(_sender, _recipient, availableElement.ActionCategory, availableElement.ElementType, _removeElementCommand)
                          {
                              ParametersCallback = availableElement.ParametersCallback,
                              HasFixedParameter = availableElement.FixedParameter != null,
                              SelectedParameter = availableElement.FixedParameter,
                              IsEditing = IsEditing
                          };

            element.UpdateDescription();

            _elements.Add(element);

            string st = ""; // needed

            switch (availableElement.ActionCategory)
            {
                case DiplomacyMessageElementActionCategory.Offer:
                    _offerElements.Add(element);
                    if (element.Tone == Tone.Indignant)
                    {
                        st = ResourceManager.GetString("OFFER_DIALOG_HINT"); // need to update the embassy screen with a new window to get the send button activated without delay.
                        var result_Offer = MessageDialog.Show(st, MessageDialogButtons.Ok);
                        GameLog.Client.Diplomacy.DebugFormat("OFFER_DIALOG_HINT is outcommented");
                    }
                    break;
                case DiplomacyMessageElementActionCategory.Request:
                    _requestElements.Add(element);
                    if (element.Tone == Tone.Indignant)
                    {
                        st = ResourceManager.GetString("REQUEST_DIALOG_HINT"); // need to update the embassy screen with a new window to get the send button activated without delay.
                        var result_Request = MessageDialog.Show(st, MessageDialogButtons.Ok);
                        GameLog.Client.Diplomacy.DebugFormat("REQUEST_DIALOG_HINT is outcommented");
                    }
                    break;
                case DiplomacyMessageElementActionCategory.Propose:
                    _treatyElements.Add(element);
                    GameLog.Client.Diplomacy.DebugFormat("Proposal element added to _treatyElemetns, {0}", element.ToString());
                    st = ResourceManager.GetString("PROPOSE_DIALOG_HINT"); // need to update the embassy screen with a new window to get the send button activated without delay.
                    //var result_Propose = MessageDialog.Show(st, MessageDialogButtons.Ok);
                    GameLog.Client.Diplomacy.DebugFormat("PROPOSE_DIALOG_HINT is outcommented");
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
                    var result_DeclareWar = MessageDialog.Show(st, MessageDialogButtons.Ok);
                    GameLog.Client.Diplomacy.DebugFormat("DECLARE_WAR_DIALOG_HINT is outcommented");
                    _statementElements.Add(element);
                    break;
                //case DiplomacyMessageElementActionCategory.SendAcceptReject:
                //    _acceptRejectElements.Add(element);
                   // break;
            }

            PopulateAvailableElements();
            UpdateLeadInText();
        }

        private void RemoveElement(DiplomacyMessageElement element)
        {
            if (element == null)
                return;

            _elements.Remove(element);

            switch (element.ActionCategory)
            {
                case DiplomacyMessageElementActionCategory.Offer:
                    _offerElements.Remove(element);
                    break;
                case DiplomacyMessageElementActionCategory.Request:
                    _requestElements.Remove(element);
                    break;
                case DiplomacyMessageElementActionCategory.Propose:
                    _treatyElements.Remove(element);
                    break;
                case DiplomacyMessageElementActionCategory.Commend:
                case DiplomacyMessageElementActionCategory.Denounce:
                case DiplomacyMessageElementActionCategory.WarDeclaration:
                    _statementElements.Remove(element);
                    break;
            }

            PopulateAvailableElements();
            UpdateLeadInText();
        }

        private NewProposal CreateProposal(bool allowIncomplete = false)
        {
            if (_elements.Count == 0)
                return null;

            var clauses = new List<Clause>();

            foreach (var element in _elements)
            {
                var clauseType = DiplomacyScreenViewModel.ElementTypeToClauseType(element.ElementType);
               // GameLog.Client.Diplomacy.DebugFormat("((()))ElementTypeToClause out Clause ={0}", DiplomacyScreenViewModel.ElementTypeToClauseType(element.ElementType).ToString());
                if (clauseType == ClauseType.NoClause)
                    continue;

                if (element.HasParameter)
                {
                    var selectedParameter = element.SelectedParameter;
                    if (selectedParameter == null && !allowIncomplete)
                        continue;

                    var parameterInfo = selectedParameter as IClauseParameterInfo;
                    if (parameterInfo != null)
                    {
                        if (parameterInfo.IsParameterValid)
                            selectedParameter = parameterInfo.GetParameterData();
                        else if (!allowIncomplete)
                            continue;
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
                return null;
            foreach (var clause in clauses)
            {
                GameLog.Core.Diplomacy.DebugFormat("((()))Create Proposal sender {0}, Recipient = {1}: Tone = {2} clause type = {3} data = {4} duration = {5}",
                    _sender.ShortName, _recipient.ShortName, _tone, clause.ClauseType.ToString(), clause.Data, clause.Duration);
            }
            return new NewProposal(_sender, _recipient, clauses);
        }

        private Statement CreateStatement()
        {
            if (_elements.Count != 1)
                return null;

            var statementType = DiplomacyScreenViewModel.ElementTypeToStatementType(_elements[0].ElementType);
            if (statementType == StatementType.NoStatement)
                return null;
            //if(statementType != StatementType.NoStatement)
                //GameLog.Core.Diplomacy.DebugFormat("((()))Create Statement {0} *vs* Recipient = {1}: Tone = {2}  StatementType = {3} ",
                // _sender.ShortName, _recipient.ShortName, _tone, statementType.ToString());

            return new Statement(_sender, _recipient, statementType, _tone);
        }

        private void PopulateAvailableElements()
        {
            // ReSharper disable ImplicitlyCapturedClosure

            _availableElements.Clear();

            var diplomat = GameContext.Current.Diplomats[_sender];
            var currentProposal = CreateProposal(allowIncomplete: true);
            var currentStatement = CreateStatement();
            var recipientIsMember = DiplomacyHelper.IsMember(_recipient, _sender);

            /*
             * Statements must be the only element in a message.
             */
            if (_elements.Count == 0)
            {
                if (diplomat.CanCommendOrDenounceWar(_recipient, currentStatement))
                {
                    Func<IEnumerable<Civilization>> denouceWarParameters = () => diplomat.GetCommendOrDenounceWarParameters(_recipient, currentStatement).ToList();

                    Func<IEnumerable<Civilization>> commendWarParameters = () => denouceWarParameters().Where(
                        c =>
                        {
                            var status = GameContext.Current.DiplomacyData[_sender, c].Status;
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
                if (!DiplomacyHelper.AreAtWar(_sender, _recipient))
                {
                    _availableElements.Add(
                        new DiplomacyMessageAvailableElement
                        {
                            ActionCategory = DiplomacyMessageElementActionCategory.WarDeclaration,
                            ElementType = DiplomacyMessageElementType.WarDeclaration
                        });
                }
            }

            var anyActiveStatements = _elements.Any(o => o.ElementType <= DiplomacyMessageElementType.DenounceSabotageStatement);
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
                            FixedParameter = new CreditsDataViewModel(Diplomat.Get(_sender).OwnerTreasury)
                        });
                }

                var requestHonorMilitaryAgreementParameters = diplomat.GetRequestHonorMilitaryAgreementParameters(_recipient, currentProposal);
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
                            FixedParameter = new CreditsDataViewModel(Diplomat.Get(_sender).OwnerTreasury)
                        });
                }

                var offerHonorMilitaryAgreementParameters = diplomat.GetOfferHonorMilitaryAgreementParameters(_recipient, currentProposal);
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

            foreach (var availableElement in _availableElements)
            {
                var elementCopy = availableElement; // modified closure

                availableElement.AddCommand = new DelegateCommand(
                    () => AddElement(elementCopy),
                    () => IsEditing);
            }

            // ReSharper restore ImplicitlyCapturedClosure
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
                return;
            bool accepting = true;
            ProcessAcceptReject(accepting);

            InvalidateCommands();
        }

        private bool CanExecuteSetAcceptButton(ICheckableCommandParameter p)
        {
            if (p == null)
                return false;

            return true;
        }

        private void ExecuteSetRejectButton(ICheckableCommandParameter p)
        {
            if (!CanExecuteSetRejectButton(p))
                return;
            bool accepting = false;
            ProcessAcceptReject(accepting);

            InvalidateCommands();
        }

        private void ProcessAcceptReject(bool accepting)
        {
            int turn = GameContext.Current.TurnNumber;
            DiplomacyScreenViewModel diplomacyScreenViewModel = new DiplomacyScreenViewModel(ServiceLocator.Current.GetInstance<IAppContext>(), ServiceLocator.Current.GetInstance<IRegionManager>());
            diplomacyScreenViewModel.UpdateSelectedForeignPower();
            var selectedForeignPower = DiplomacyScreenViewModel.DesignInstance.SelectedForeignPower;
            var senderCiv = selectedForeignPower.Counterparty; // sender of proposal treaty
            var playerEmpire = DiplomacyScreenViewModel.DesignInstance.LocalPalyer; // local player reciever of proposal treaty
            var diplomat = Diplomat.Get(playerEmpire);
            var otherDiplomat = Diplomat.Get(senderCiv);
            var foreignPower = diplomat.GetForeignPower(senderCiv);
            var otherForeignPower = otherDiplomat.GetForeignPower(playerEmpire);
            bool localPlayerIsHosting = DiplomacyScreenViewModel.DesignInstance.localIsHost;
            string Accepted = "ACCEPTED";
            if (accepting == false)
                Accepted = "REJECTED";
            Response = Accepted;
            int selectedID = selectedForeignPower.Owner.CivID;

            if (_acceptedRejected.ContainsKey(selectedID))
            {
                if (_acceptedRejected[selectedID] != Accepted)
                {
                    _acceptedRejected.Remove(selectedID);
                    _acceptedRejected.Add(selectedID, Accepted);
                }
            }
            else _acceptedRejected.Add(selectedID, Accepted);

            //GameLog.Client.Diplomacy.DebugFormat("Local player IS Host....");
            DiplomacyHelper.AcceptRejectDictionary(foreignPower, accepting, turn); // creat entry for game host

            // creat entry for none host human player that clicked the accept - reject radio button         
            StatementType _statementType = DiplomacyHelper.GetStatementType(accepting, senderCiv, playerEmpire); // first is bool, 2nd sender ID(now the local player), last new receipient, in Dictinary Key                       
            GameLog.Client.Diplomacy.DebugFormat("Local player IS NOT Host, statementType = {0} accepting = {1} sender ={2} counterpartyID {3} local = {4} OwnerID ={5}"
                , Enum.GetName(typeof(StatementType), _statementType)
                , accepting
                , senderCiv.Key
                , foreignPower.CounterpartyID
                , playerEmpire.Key
                , foreignPower.OwnerID
                );
            if (_statementType != StatementType.NoStatement)
            {
                Statement statementToSend = new Statement(playerEmpire, senderCiv, _statementType, Tone.Receptive, turn);
                _sendOrder = new SendStatementOrder(statementToSend);
                ServiceLocator.Current.GetInstance<IPlayerOrderService>().AddOrder(_sendOrder);

                otherForeignPower.StatementSent = statementToSend; // load statement to send in foreignPower, statment type carries key for dictionary entery

                GameLog.Client.Diplomacy.DebugFormat("!! foreignPower.StatementSent *other*ForeignPower Recipient ={0} to Sender ={1}"
                    , statementToSend.Recipient.Key
                    , statementToSend.Sender.Key
                    );
            }
        }

        private bool CanExecuteSetRejectButton(ICheckableCommandParameter p)
        {
            if (p == null)
                return false;

            return true;
        }

        public static DiplomacyMessageViewModel FromReponse([NotNull] IResponse response)
        {
            GameLog.Core.Diplomacy.DebugFormat("$$ at FromResponse() with turnSent ={0} tone ={1}", response.TurnSent, response.Tone);
            if (response == null)
                throw new ArgumentNullException("response");

            DiplomacyStringID leadInId;

            switch (response.ResponseType)
            {
                case ResponseType.NoResponse:
                    return null;
                case ResponseType.Accept:
                    if (response.Proposal.IsGift())
                        leadInId = DiplomacyStringID.AcceptGiftLeadIn;
                    else if (response.Proposal.IsDemand())
                        leadInId = DiplomacyStringID.AcceptDemandLeadIn;
                    else if (!response.Proposal.HasTreaty())
                        leadInId = DiplomacyStringID.AcceptExchangeLeadIn;
                    else 
                        leadInId = DiplomacyStringID.AcceptProposalLeadIn;
                    break;
                case ResponseType.Reject:
                    if (response.Proposal.IsGift())
                        leadInId = DiplomacyStringID.RejectProposalLeadIn; // should not happen
                    else if (response.Proposal.IsDemand())
                        leadInId = DiplomacyStringID.RejectDemandLeadIn;
                    else if (!response.Proposal.HasTreaty())
                        leadInId = DiplomacyStringID.RejectExchangeLeadIn;
                    else
                        leadInId = DiplomacyStringID.RejectProposalLeadIn;
                    break;
                case ResponseType.Counter:
                    leadInId = DiplomacyStringID.CounterProposalLeadIn;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var message = new DiplomacyMessageViewModel(response.Sender, response.Recipient)
                          {
                              Tone = response.Proposal.Tone,
                          };

            message._treatyLeadInTextScript.ScriptCode = QuoteString(LookupDiplomacyText(leadInId, message._tone, message._sender) ?? string.Empty);
            message.TreatyLeadInText = message._treatyLeadInTextScript.Evaluate<string>(message._leadInRuntimeParameters);
            GameLog.Core.Diplomacy.DebugFormat("message ={0}", message);
            return message;
        }

        public static DiplomacyMessageViewModel FromProposal([NotNull] IProposal proposal)
        {
            GameLog.Core.Diplomacy.DebugFormat("$$ at FromProposal() with turnSent ={0} Recipient ={1} sender ={2}", proposal.TurnSent, proposal.Recipient, proposal.Sender);
            if (proposal == null)
                throw new ArgumentNullException("proposal");

            DiplomacyStringID leadInId;
             
            switch ((object)proposal.Clauses) // not all cases used below, ToDo
            {
                case ClauseType.TreatyOpenBorders:
                    leadInId = DiplomacyStringID.OpenBordersClause;
                    break;
                case ClauseType.TreatyAffiliation:
                    leadInId = DiplomacyStringID.AffiliationClause;
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
                    leadInId = DiplomacyStringID.OpenBordersClause;
                    break;
            }

            var message = new DiplomacyMessageViewModel(proposal.Sender, proposal.Recipient)
            {
                Tone = proposal.Tone,
            };

            message._treatyLeadInTextScript.ScriptCode = QuoteString(LookupDiplomacyText(leadInId, message._tone, message._sender) ?? string.Empty);
            message.TreatyLeadInText = message._treatyLeadInTextScript.Evaluate<string>(message._leadInRuntimeParameters);
            GameLog.Core.Diplomacy.DebugFormat("message ={0}", message);
            return message;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(bool placeHolder, string propertyName)
        {
            if(placeHolder)
                _propertyChanged.Raise(this, propertyName);
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
