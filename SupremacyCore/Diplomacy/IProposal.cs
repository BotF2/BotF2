// File:IProposal.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Diplomacy.Visitors;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Scripting;
using Supremacy.Utility;
//using Supremacy.;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
//using Supremacy.views

namespace Supremacy.Diplomacy
{
    public interface IProposal : IDiplomaticExchange
    {
        int TurnSent { get; }
        IIndexedCollection<IClause> Clauses { get; }
        Tone Tone { get; }
    }

    [Serializable]
    public class NewProposal : IProposal
    {
        private readonly int _turnSent;
        private readonly int _sender;
        private readonly int _recipient;
        private readonly CollectionBase<IClause> _clauses;

        public NewProposal(Civilization sender, Civilization recipient, params IClause[] clauses)
            : this(GameContext.Current.TurnNumber, sender, recipient, (IEnumerable<IClause>)clauses) { }

        public NewProposal(Civilization sender, Civilization recipient, IEnumerable<IClause> clauses)
            : this(GameContext.Current.TurnNumber, sender, recipient, clauses) { }

        public NewProposal(int turnSent, [NotNull] ICivIdentity sender, [NotNull] ICivIdentity recipient, [NotNull] params IClause[] clauses)
            : this(turnSent, sender, recipient, (IEnumerable<IClause>)clauses) { }

        public NewProposal(int turnSent, [NotNull] ICivIdentity sender, [NotNull] ICivIdentity recipient, [NotNull] IEnumerable<IClause> clauses)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }

            if (recipient == null)
            {
                throw new ArgumentNullException("recipient");
            }

            if (clauses == null)
            {
                throw new ArgumentNullException("clauses");
            }

            _turnSent = turnSent;
            _sender = sender.CivID;
            _recipient = recipient.CivID;
            _clauses = new CollectionBase<IClause>();

            _clauses.AddRange(clauses);
        }

        #region Implementation of IProposal

        public int TurnSent => _turnSent;

        public Civilization Sender => GameContext.Current.Civilizations[_sender];

        public Civilization Recipient => GameContext.Current.Civilizations[_recipient];

        public IIndexedCollection<IClause> Clauses => _clauses;

        public Tone Tone => Tone.Calm;

        #endregion
        //}

        public static NewProposal CreateProposal(Civilization _civ1, Civilization _civ2, bool allowIncomplete = false)
        {
            //private readonly 
            ObservableCollection<DiplomacyMessageElement> _elements = null;
            //private readonly 
            //Civilization _recipient;
            //private readonly ObservableCollection<DiplomacyMessageElement> _elements;
            //ObservableCollection<DiplomacyMessageElement> _offerElements;
            //ObservableCollection<DiplomacyMessageElement> _requestElements;
            //ObservableCollection<DiplomacyMessageElement> _statementElements;
            //ObservableCollection<DiplomacyMessageElement> _treatyElements;
            //ReadOnlyObservableCollection<DiplomacyMessageElement> _treatyElementsView;
            // ReadOnlyObservableCollection<DiplomacyMessageElement> _acceptRejectElementsView; // no view of this???
            //ObservableCollection<DiplomacyMessageAvailableElement> _availableElements;
            //DelegateCommand<DiplomacyMessageElement> _removeElementCommand;


            if (_elements.Count == 0)
            {
                return null;
            }
            string _text;

            List<Clause> clauses = new List<Clause>();

            //foreach (DiplomacyMessageElement element in _elements)
            //{
            //    ClauseType clauseType = DiplomacyScreenViewModel.ElementTypeToClauseType(element.ElementType);
            //    // GameLog.Client.Diplomacy.DebugFormat("((()))ElementTypeToClause out Clause ={0}", DiplomacyScreenViewModel.ElementTypeToClauseType(element.ElementType).ToString());
            //    if (clauseType == ClauseType.NoClause)
            //    {
            //        continue;
            //    }

            //    if (element.HasParameter)
            //    {
            //        object selectedParameter = element.SelectedParameter;
            //        if (selectedParameter == null && !allowIncomplete)
            //        {
            //            continue;
            //        }

            //        if (selectedParameter is IClauseParameterInfo parameterInfo)
            //        {
            //            if (parameterInfo.IsParameterValid)
            //            {
            //                selectedParameter = parameterInfo.GetParameterData();
            //            }
            //            else if (!allowIncomplete)
            //            {
            //                continue;
            //            }
            //        }

            //        //
            //        // It's possible for 'selectedParameter' to be null here.  We assume this is okay
            //        // if IClauseParameterInfo.IsParameterValid returned 'true'.
            //        //
            //        clauses.Add(new Clause(clauseType, selectedParameter));
            //    }
            //    else
            //    {
            //        clauses.Add(new Clause(clauseType));
            //    }
            //}

            if (clauses.Count == 0)
            {
                return null;
            }

            foreach (Clause clause in clauses)
            {

                //GameLog.Core.Diplomacy.DebugFormat("((()))Create Proposal sender {0}, Recipient = {1}: Tone = {2} clause type = {3} data = {4} duration = {5}",
                _text =
                "Turn " + GameContext.Current.TurnNumber
                + ": Proposal created: Sender " + _civ1.ShortName
                + " to > " + _civ2.ShortName
                + ": " + clause.ClauseType.ToString()
                //+ " ( " + _tone 
                + "," + clause.Duration + clause.Data + " )"
                ;
                //Console.WriteLine(_text);
                GameLog.Core.Diplomacy.DebugFormat(_text);
                // if ClauseType == TreatyWarPact then clause.Data = string shortname of target civilization
            }
            return new NewProposal(_civ1, _civ2, clauses);
        }

        public interface IClauseParameterInfo
        {
            bool IsParameterValid { get; }
            object GetParameterData();
        }

        private class DiplomacyMessageElement
        {
            private readonly Civilization _sender;
            private readonly Civilization _recipient;
            private readonly ScriptExpression _scriptExpression;
            //private readonly DelegateCommand<DataTemplate> _editParameterCommand;
            public DiplomacyMessageElement(
    [NotNull] Civilization sender,
    [NotNull] Civilization recipient,
    //DiplomacyMessageElementActionCategory actionCategory,
    //DiplomacyMessageElementType elementType,
    ICommand removeCommand)
            {
                _sender = sender ?? throw new ArgumentNullException("sender");
                _recipient = recipient ?? throw new ArgumentNullException("recipient");
                //ActionCategory = actionCategory;
                //ElementType = elementType; // includes TreatyWarPact
                //RemoveCommand = removeCommand;

                //_editParameterCommand = new DelegateCommand<DataTemplate>(
                //    ExecuteEditParameterCommand,
                //    CanExecuteEditParameterCommand);

                //Type parameterType = GetViewModelParameterTypeForElementType(elementType);

                ScriptParameters scriptParameters = new ScriptParameters(
                    new ScriptParameter("$sender", typeof(Civilization)),
                    new ScriptParameter("$recipient", typeof(Civilization)));
                //new ScriptParameter("$target", typeof(Civilization)));

                //if (parameterType != null) // for target of war pact, who do both sender and recipient declare war on
                //{
                //    scriptParameters = scriptParameters.Merge(
                //        new ScriptParameter(
                //            "$parameter",
                //            GetViewModelParameterTypeForElementType(elementType)));
                //}

                _scriptExpression = new ScriptExpression(returnObservableResult: false)
                {
                    Parameters = scriptParameters
                };

            }
        }
    }

    public static class ProposalExtensions
    {
        public static IProposal Clone([NotNull] this IProposal proposal)
        {
            if (proposal == null)
            {
                throw new ArgumentNullException("proposal");
            }

            return new NewProposal(
                proposal.TurnSent,
                proposal.Sender,
                proposal.Recipient,
                proposal.Clauses.Select(o => o.Clone()));
        }

        public static bool IsGift(this IProposal proposal)
        {
            if (proposal == null)
            {
                return false;
            }

            bool isGift = false;
            foreach (IClause clause in proposal.Clauses)
            {
                switch (clause.ClauseType)
                {
                    //case ClauseType.RequestBreakAgreement:
                    //case ClauseType.RequestEndEmbargo:
                    case ClauseType.RequestCredits:
                    //case ClauseType.RequestGiveResources:
                    //case ClauseType.RequestHonorMilitaryAgreement:
                    //case ClauseType.RequestMapData:
                    //case ClauseType.RequestStopPiracy:
                    //case ClauseType.RequestWithdrawTroops:
                    case ClauseType.TreatyDefensiveAlliance:
                    case ClauseType.TreatyFullAlliance:
                    case ClauseType.TreatyCeaseFire:
                    case ClauseType.TreatyWarPact:
                    case ClauseType.TreatyAffiliation:
                    case ClauseType.TreatyNonAggression:
                    case ClauseType.TreatyOpenBorders:
                    //case ClauseType.TreatyResearchPact:
                    //case ClauseType.TreatyTradePact:
                    case ClauseType.TreatyMembership:
                        return false;
                    default:
                        isGift = true;
                        break;
                }
            }
            return isGift;
        }

        public static bool IsDemand(this IProposal proposal)
        {
            if (proposal == null)
            {
                return false;
            }

            bool isDemand = false;
            foreach (IClause clause in proposal.Clauses)
            {
                switch (clause.ClauseType)
                {
                    //case ClauseType.OfferBreakAgreement:
                    //case ClauseType.OfferEndEmbargo:
                    case ClauseType.OfferCredits:
                    //case ClauseType.OfferGiveResources:
                    //case ClauseType.OfferHonorMilitaryAgreement:
                    //case ClauseType.OfferMapData:
                    //case ClauseType.OfferStopPiracy:
                    //case ClauseType.OfferWithdrawTroops:
                    case ClauseType.TreatyDefensiveAlliance:
                    case ClauseType.TreatyFullAlliance:
                    case ClauseType.TreatyCeaseFire:
                    case ClauseType.TreatyWarPact:
                    case ClauseType.TreatyAffiliation:
                    case ClauseType.TreatyNonAggression:
                    case ClauseType.TreatyOpenBorders:
                    //case ClauseType.TreatyResearchPact:
                    //case ClauseType.TreatyTradePact:
                    case ClauseType.TreatyMembership:
                        return false;
                    default:
                        isDemand = true;
                        break;
                }
            }
            return isDemand;
        }

        public static bool HasTreaty(this IProposal proposal)
        {
            if (proposal == null)
            {
                return false;
            }

            // GameLog.Core.Diplomacy.DebugFormat("hasTreaty: null or clause = {0}", proposal.Clauses.ToString()/*, proposal.ToString()*/);

            foreach (IClause clause in proposal.Clauses)
            {
                switch (clause.ClauseType)
                {
                    case ClauseType.TreatyDefensiveAlliance:
                    case ClauseType.TreatyFullAlliance:
                    case ClauseType.TreatyCeaseFire:
                    case ClauseType.TreatyWarPact:
                    case ClauseType.TreatyAffiliation:
                    case ClauseType.TreatyNonAggression:
                    case ClauseType.TreatyOpenBorders:
                    //case ClauseType.TreatyResearchPact:
                    //case ClauseType.TreatyTradePact:
                    case ClauseType.TreatyMembership:
                        // GameLog doesn't work
                        //GameLog.Core.Diplomacy.DebugFormat("hasTreaty: ClauseType = {0}", clause.ClauseType.ToString()/*, proposal.ToString()*/);
                        return true;
                }
            }
            return false;
        }

        public static bool HasClause(this IProposal proposal, ClauseType clause)
        {
            // GameLog.Core.Diplomacy.DebugFormat("hasClause: null or clause = {0}", clause.ToString()/*, proposal.ToString()*/);

            if (proposal == null)
            {
                return false;
            }

            //GameLog.Core.Diplomacy.DebugFormat("Visitor = {0}: Accepting proposal = {1}", visitor.ToString(), proposal.ToString());

            return proposal.Clauses.Any(c => c.ClauseType == clause);
        }

        public static void Accept([NotNull] this IProposal proposal, [NotNull] IProposalVisitor visitor)
        {
            if (proposal == null)
            {
                throw new ArgumentNullException("proposal");
            }

            if (visitor == null)
            {
                throw new ArgumentNullException("visitor");
            }

            GameLog.Core.DiplomacyDetails.DebugFormat("Turn {3}: Sender ={2}, proposal clause type = {0}, Recipient ={1} "
                , proposal.Clauses[0].ClauseType.ToString()
                , proposal.Recipient.ShortName
                , proposal.Sender.ShortName
                , GameContext.Current.TurnNumber);

            if (proposal.IsGift())
            {
                visitor.VisitGift(proposal);
                return;
            }

            if (proposal.IsDemand())
            {
                visitor.VisitDemand(proposal);
                return;
            }

            if (proposal.IsWarPact())
            {
                visitor.VisitWarPact(proposal);
                return;
            }

            if (proposal.HasTreaty())
            {
                visitor.VisitTreatyProposal(proposal);
                return;
            }

            visitor.VisitExchange(proposal);
        }
    }
}