using Supremacy.Annotations;
using Supremacy.Client.Audio;
using Supremacy.Economy;
using Supremacy.Game;
using Supremacy.Tech;
using Supremacy.Utility;
using System;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;


namespace Supremacy.Client.Views
{

    public partial class NewShipSelectionView
    {
        private readonly IMusicPlayer _musicPlayer = null;
        private readonly ISoundPlayer _soundPlayer = null;
        //[NotNull] new MusicPlayer musicPlayer;
        private string _text;

        public NewShipSelectionView(ShipyardBuildSlot buildSlot, [NotNull] IMusicPlayer musicPlayer)
        {
            InitializeComponent();

            //doesn't work
            if (_musicPlayer != null)
            {
                _musicPlayer.SwitchMusic("ShipBuildingScreenMusic");
            }

            if (_soundPlayer != null)
            {
                _soundPlayer.PlayFile("Resources/SoundFX/ScreenMusic/Summary.ogg");
            }


            BuildProject[] shipList = TechTreeHelper.GetShipyardBuildProjects(buildSlot.Shipyard)
                                        .OrderBy(s => s.BuildDesign.Key)
                                        .ToArray();

            BuildProjectList.ItemsSource = shipList;

            _ = SetBinding(
                SelectedBuildProjectProperty,
                new Binding
                {
                    Source = BuildProjectList,
                    Path = new PropertyPath(Selector.SelectedItemProperty),
                    Mode = BindingMode.OneWay
                });

            if (BuildProjectList.Items.Count > 0)
            {
                BuildProjectList.SelectedIndex = 0;  // to display SHIP_INFO_TEXT just at screen opening
            }
        }

        #region SelectedBuildProject Property
        public static readonly DependencyProperty SelectedBuildProjectProperty = DependencyProperty.Register(
            "SelectedBuildProject",
            typeof(ShipBuildProject),
            typeof(NewShipSelectionView),
            new PropertyMetadata());

        public ShipBuildProject SelectedBuildProject // Change in this is seen at ColonyScreenPresenter inside of ExecuteSelectShipBuildProjectCommand(ShipyardBuildSlot buildSlot)
        {
            get => (ShipBuildProject)GetValue(SelectedBuildProjectProperty);
            set => SetValue(SelectedBuildProjectProperty, value);
        }
        #endregion

        #region AdditionalContent Property
        public static readonly DependencyProperty AdditionalContentProperty = DependencyProperty.Register(
            "AdditionalContent",
            typeof(object),
            typeof(NewShipSelectionView),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.None));
        //private string _text;

        public object AdditionalContent
        {
            get => GetValue(AdditionalContentProperty);
            set => SetValue(AdditionalContentProperty, value);
        }
        #endregion

        public string ShipFunctionPath => "vfs:///Resources/Specific_Empires_UI/" + Context.DesignTimeAppContext.Instance.LocalPlayerEmpire.Civilization.Key + "/ColonyScreen/Ship_Functions.png";

        public int SpecialWidth1 => Context.DesignTimeAppContext.Instance.ASpecialWidth1;// ActualWidthProperty;  // used in view
        public int SpecialHeight1 => Context.DesignTimeAppContext.Instance.ASpecialHeight1;

        private void CanExecuteAcceptCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedBuildProject != null;
        }

        private void ExecuteAcceptCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (SelectedBuildProject == null)
            {
                return;
            }

            DialogResult = true;
        }

        private void OnBuildProjectListMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(e.OriginalSource is DependencyObject source))
            {
                return;
            }

            ListBoxItem contanier = source.FindVisualAncestorByType<ListBoxItem>();
            if (contanier == null)
            {
                return;
            }

            //string _soundfile = "Resources\\SoundFX\\sound001.wav"; // player just plays wav
            //if (File.Exists(_soundfile))
            //{
            //    System.Media.SoundPlayer player = new System.Media.SoundPlayer(_soundfile);
            //    //GameLog.Client.General.Debug("Playing sound001.wav");
            //    //var soundPlayer = new SoundPlayer("Resources/SoundFX/sound001.wav");
            //    player.Play();
            //}
            //else
            //{
            //    _text = "Resources/SoundFX/sound001.wav not found...";
            //    Console.WriteLine("Step_1248:; " + _text);
            //    GameLog.Client.Audio.InfoFormat(_text);
            //}
            _text = "dummy" + _text; // please keep


            DialogResult = true;   // just trying to solve the HowMany Ships to build by keeping screen open
        }

        //private void OnBuildProjectHowManyChanged(object sender, MouseButtonEventArgs e)
        //{
        //    if (!(e.OriginalSource is DependencyObject source))
        //    {
        //        return;
        //    }

        //    ListBoxItem contanier = source.FindVisualAncestorByType<ListBoxItem>();
        //    if (contanier == null)
        //    {
        //        return;
        //    }

        //    DialogResult = true;
        //}
    }
}
