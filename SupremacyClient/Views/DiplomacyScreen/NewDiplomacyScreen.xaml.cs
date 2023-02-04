// File:NewDiplomacyScreen.xaml.cs
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Supremacy.Client.Context;
using Supremacy.Client.Controls;
using Supremacy.Diplomacy;
using Supremacy.Game;
using Supremacy.UI;
using Supremacy.Utility;

// TODO: Add legend to relationship graph

namespace Supremacy.Client.Views.DiplomacyScreen
{
    /// <summary>
    /// Interaction logic for NewDiplomacyScreen.xaml
    /// </summary>
    public partial class NewDiplomacyScreen : INewDiplomacyScreenView //System.ComponentModel.INotifyPropertyChanged
    {

        public NewDiplomacyScreen()
        {
            TextBlockExtensions.AddHyperlinkClickedHandler(this, OnMessageParameterLinkClick);
            InitializeComponent();
        }

        #region Implementation of IActiveAware

        private bool _isActive;

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (value == _isActive)
                {
                    return;
                }

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
            get => DataContext as DiplomacyScreenViewModel;
            set => DataContext = value;
        }

        public void OnCreated() { }

        public void OnDestroyed() { }

        #endregion

        private void OnMessageParameterLinkClick(object sender, HyperlinkClickedEventArgs e)
        {
            if (!(e.OriginalSource is Hyperlink hyperlink))
            {
                return;
            }

            if (!(hyperlink.DataContext is DiplomacyMessageElement element))
            {
                return;
            }

            object parameter = element.SelectedParameter;
            DataTemplate contentTemplate = parameter != null ? TryFindResource(parameter.GetType()) as DataTemplate : null;

            if (element.EditParameterCommand.CanExecute(contentTemplate))
            {
                element.EditParameterCommand.Execute(contentTemplate);
            }
        }

        private void SelecteForeignPower_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

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
                {
                    _instance = new DiplomacyGraphPenSelector();
                }

                return _instance;
            }
        }

        static DiplomacyGraphPenSelector()
        {
            RelationshipStatusBrushConverter converter = new RelationshipStatusBrushConverter();

            _pens = new Dictionary<ForeignPowerStatus, Pen>();
            _fallbackPen = new Pen(Brushes.Gainsboro, PenThickness);

            if (_fallbackPen.CanFreeze)
            {
                _fallbackPen.Freeze();
            }

            foreach (ForeignPowerStatus status in EnumHelper.GetValues<ForeignPowerStatus>())
            {
                if (!(converter.Convert(status, null, null, null) is Brush brush))
                {
                    continue;
                }

                if (brush.CanFreeze)
                {
                    brush.Freeze();
                }

                _pens[status] = new Pen(brush, PenThickness);
                _pens[status].TryFreeze();
            }
        }

        #region INodeGraphPenSelector Members
        public Pen GetPen(object parentNode, object childNode)
        {
            if (!(parentNode is DiplomacyGraphNode node1) || !(childNode is DiplomacyGraphNode node2))
            {
                return _fallbackPen;
            }


            if (GameContext.Current.DiplomacyData.TryGetValue(node1.Civilization, node2.Civilization, out IDiplomacyData data))
            {
                return _pens.TryGetValue(data.Status, out Pen pen) ? pen : _fallbackPen;
            }

            return _fallbackPen;
        }
        #endregion
    }

}
