using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Practices.Composite.Regions;
using Obtics.Collections;
using Supremacy.Client.Context;
using Supremacy.Client.Controls;
using Supremacy.Diplomacy;
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
                    var player = (ICivIdentity)Model.PlayerCivilization;
                    var coutner = (ICivIdentity)Model.SelectedForeignPower.Owner;
                    ForeignPower power = new ForeignPower(player, coutner);
                    if (response == "ACCEPT")
                    {
                        power.PendingAction = PendingDiplomacyAction.AcceptProposal;
                    }
                    else if (response == "COUNTER")
                    {
                        power.PendingAction = PendingDiplomacyAction.None;
                    }
                    else if (response == "REJECT")
                    {
                        power.PendingAction = PendingDiplomacyAction.RejectProposal;
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
