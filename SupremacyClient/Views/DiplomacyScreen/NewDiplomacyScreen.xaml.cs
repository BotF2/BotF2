using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Practices.Composite.Regions;
using Obtics.Collections;
using Supremacy.AI;
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
                    bool accepting = false;
                    if (response == "ACCEPT")
                            accepting = true;

                    var senderCiv = Model.SelectedForeignPower.Counterparty;
                    var playerEmpire = AppContext.LocalPlayerEmpire.Civilization; // local player
                    var diplomat = Diplomat.Get(playerEmpire);
                    var foreignPower = diplomat.GetForeignPower(senderCiv);
                    bool localPlayerIsHosting = AppContext.IsGameHost;
                    int turn = GameContext.Current.TurnNumber;
                    if (localPlayerIsHosting)
                    {
                        GameLog.Client.Diplomacy.DebugFormat("Local player IS Host....");
                        DiplomacyHelper.AcceptRejectDictionary(foreignPower, accepting, turn); // creat entry for game host
                    }
                    else
                    {      // creat entry for none host human player that clicked the accept - reject radio button         
                        var _statementType = DiplomacyHelper.GetStatementType(accepting, (Civilization)senderCiv, playerEmpire); // first is bool, then sender ID(now the local player), last new receipient, in Dictinary Key                       
                        GameLog.Client.Diplomacy.DebugFormat("Local player IS NOT Host, statementType = {0} accepting? {1} sender ={2}",
                            Enum.GetName(typeof(StatementType), _statementType),
                            senderCiv.Key);
                        if (_statementType != StatementType.NoStatement)
                        {
                            
                            Statement statementToSend = new Statement( senderCiv, playerEmpire, _statementType, Tone.Receptive, turn); //DiplomacyExtensions.GetStatementSent(diplomat, senderCiv);
                            foreignPower.StatementSent = statementToSend; // load statement to send in foreignPower, statment type carries key for dictionary entery
                            GameLog.Client.Diplomacy.DebugFormat("!! Statement sender ={0} to recipient ={1}", statementToSend.Sender.Key, statementToSend.Recipient.Key);
                        }
                    }

                    GameLog.Client.Diplomacy.DebugFormat("Turn {0}: Button response = {4}, Player = {1}, Sender = {2} vs counterParty {3} local player is host ={5}"
                        , GameContext.Current.TurnNumber
                        , playerEmpire.Key
                        , foreignPower.Owner
                        , foreignPower.Counterparty.Key
                        , response
                        , localPlayerIsHosting
                        );
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
