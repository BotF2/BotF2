using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Practices.Composite.Regions;
using Obtics.Collections;
using Supremacy.Client.Context;
using Supremacy.Client.Controls;
using Supremacy.Diplomacy;
using Supremacy.Diplomacy.Visitors;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.UI;
using Supremacy.Utility;

// TODO: Add legend to relationship graph

namespace Supremacy.Client.Views.DiplomacyScreen
{
    /// <summary>
    /// Interaction logic for NewDiplomacyScreen.xaml
    /// </summary>
    public partial class NewDiplomacyScreen : INewDiplomacyScreenView
    {
        public NewDiplomacyScreen()
        {
            TextBlockExtensions.AddHyperlinkClickedHandler(this, OnMessageParameterLinkClick);
            InitializeComponent();
        }

        private void clickAcceptCounterReject(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton != null)
            {
                var response = (string)radioButton.Content;
                if (Model.SelectedForeignPower != null)
                {
                    // NEED TO UPDATE? foreignPower.ProposalReceived - it is does not get here from somewhere, look in DiplomacyAI
                    var selectedCiv = Model.SelectedForeignPower.Counterparty;

                    var playerEmpire = AppContext.LocalPlayer; // local player
                    Diplomat diplomat = new Diplomat(selectedCiv);
                    
                    var foreignPower = diplomat.GetForeignPower(playerEmpire);    //  Model.SelectedForeignPower.Counterparty);
                    //int turn = GameContext.Current.TurnNumber;
                    //Diplomat otherDiplomate = new Diplomat(playerEmpire);
                    //var otherForeignPower = otherDiplomate.GetForeignPower(selectedCiv);
                    //if(power.ResponseReceived != null && power.ResponseReceived.Proposal != null)
                    //    GameLog.Client.Diplomacy.DebugFormat("$$ ResponseReceived Proposal clauses ={0}", power.ResponseReceived.Proposal.Clauses.ToString());
                    //if (power.ResponseSent != null && power.ResponseSent.Proposal != null)
                    //    GameLog.Client.Diplomacy.DebugFormat("$$ ResponseSent Proposal clauses ={0}", power.ResponseSent.Proposal.Clauses.ToString());
                    //if (power.PendingAction != null)
                    //    GameLog.Client.Diplomacy.DebugFormat("$$ PendingAction ={0}", power.PendingAction.ToString());
                    //if (power.ProposalReceived != null)
                    //    GameLog.Client.Diplomacy.DebugFormat("$$ ProposalReceived Clauses ={0}", power.ProposalReceived.Clauses.ToString());
                    //if (power.ProposalSent != null)
                    //    GameLog.Client.Diplomacy.DebugFormat("$$ ProposalSent clauses ={0}", power.ProposalSent.Clauses.ToString());

                    if  (response == "ACCEPT" || response == "COUNTER" || response == "REJECT") //&& foreignPower.ProposalReceived != null) 
                    {
                        //NewProposal proposal = new NewProposal(foreignPower.Owner, foreignPower.Counterparty, foreignPower.ProposalReceived.Clauses);
                 
                        if (response == "ACCEPT")
                        {
    
                            foreignPower.PendingAction = PendingDiplomacyAction.AcceptProposal;
                        }
                        else if (response == "COUNTER")
                        {
                            foreignPower.PendingAction = PendingDiplomacyAction.None;
                        }
                        else if (response == "REJECT")
                        {
                           foreignPower.PendingAction = PendingDiplomacyAction.RejectProposal;
                        }
                        foreignPower.LastProposalReceived = foreignPower.ProposalReceived;
                        foreignPower.ProposalReceived = null;
                    }
                }
            }
        }

        #region Implementation of IActiveAware

        private bool _isActive;

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (value == _isActive)
                    return;

                _isActive = value;
                
                IsActiveChanged.Raise(this);
            }
        }

        public event EventHandler IsActiveChanged;

        #endregion

        #region Implementation of IGameScreenView<DiplomacyScreenViewModel>

        public IAppContext AppContext { get; set; }

        public DiplomacyScreenViewModel Model
        {
            get { return DataContext as DiplomacyScreenViewModel; }
            set { DataContext = value; }
        }

        public void OnCreated() { }

        public void OnDestroyed() { }

        #endregion

        private void OnMessageParameterLinkClick(object sender, HyperlinkClickedEventArgs e)
        {
            var hyperlink = e.OriginalSource as Hyperlink;
            if (hyperlink == null)
                return;

            var element = hyperlink.DataContext as DiplomacyMessageElement;
            if (element == null)
                return;

            var parameter = element.SelectedParameter;
            var contentTemplate = parameter != null ? TryFindResource(parameter.GetType()) as DataTemplate : null;

            if (element.EditParameterCommand.CanExecute(contentTemplate))
                element.EditParameterCommand.Execute(contentTemplate);
        }
    }

    internal class DiplomacyGraphPenSelector : INodeGraphPenSelector
    {
        private const double PenThickness = 2.0;

        private static readonly Pen _fallbackPen;
        private static readonly Dictionary<ForeignPowerStatus, Pen> _pens;

        private static DiplomacyGraphPenSelector _instance;

        public static DiplomacyGraphPenSelector Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DiplomacyGraphPenSelector();
                return _instance;
            }
        }

        static DiplomacyGraphPenSelector()
        {
            var converter = new RelationshipStatusBrushConverter();

            _pens = new Dictionary<ForeignPowerStatus, Pen>();
            _fallbackPen = new Pen(Brushes.Gainsboro, PenThickness);

            if (_fallbackPen.CanFreeze)
                _fallbackPen.Freeze();

            foreach (var status in EnumHelper.GetValues<ForeignPowerStatus>())
            {
                var brush = converter.Convert(status, null, null, null) as Brush;
                if (brush == null)
                    continue;

                if (brush.CanFreeze)
                    brush.Freeze();

                _pens[status] = new Pen(brush, PenThickness);
                _pens[status].TryFreeze();
            }
        }

        #region INodeGraphPenSelector Members
        public Pen GetPen(object parentNode, object childNode)
        {
            var node1 = parentNode as DiplomacyGraphNode;
            var node2 = childNode as DiplomacyGraphNode;

            if (node1 == null || node2 == null)
                return _fallbackPen;

            Pen pen;
            IDiplomacyData data;

            if (GameContext.Current.DiplomacyData.TryGetValue(node1.Civilization, node2.Civilization, out data))
                return _pens.TryGetValue(data.Status, out pen) ? pen : _fallbackPen;

            return _fallbackPen;
        }
        #endregion
    }

}
