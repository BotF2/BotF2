using System;
using System.ComponentModel;
using System.Windows;

using Microsoft.Practices.Unity;

using Supremacy.Annotations;
using Supremacy.Client.Commands;
using Supremacy.Client.Input;
using Supremacy.Diplomacy;
using Supremacy.Game;
using Supremacy.Universe;
using Supremacy.Client.Views.GalaxyScreen;
using Supremacy.Client.Context;
using Supremacy.Client.Dialogs;

namespace Supremacy.Client.Views
{
    public partial class GalaxyGridView : IWeakEventListener
    {
        private readonly IUnityContainer _container;
        private readonly IAppContext _appContext;
        private readonly INavigationCommandsProxy _navigationCommands;
        private readonly DelegateCommand<object> _revealMapCommand;
        private readonly DelegateCommand<object> _cheatMenuCommand;
        private readonly DelegateCommand<object> _gameInfoScreenCommand;

        #region Constructors and Finalizers
        public GalaxyGridView([NotNull] IUnityContainer container)
        {
            if (container == null)
                throw new ArgumentNullException("container");

            _container = container;
            _appContext = _container.Resolve<IAppContext>();
            _navigationCommands = _container.Resolve<INavigationCommandsProxy>();

            InitializeComponent();

            Loaded += delegate
                           {
                               GalaxyGrid.Update();
                               GalaxyGrid.SelectedSector = _appContext.LocalPlayerEmpire.SeatOfGovernment.Sector;
                               GalaxyGrid.CenterOnSelectedSector();
                           };
            Unloaded += OnUnloaded;

            GalaxyGrid.SectorDoubleClicked += OnSectorDoubleClicked;

            PropertyChangedEventManager.AddListener(_appContext, this, "LocalPlayerEmpire");
            
            _revealMapCommand = new DelegateCommand<object>(ExecuteRevealMapCommand);
            _cheatMenuCommand = new DelegateCommand<object>(ExecuteCheatMenuCommand);
            _gameInfoScreenCommand = new DelegateCommand<object>(ExecuteGameInfoScreenCommand);

            DebugCommands.RevealMap.RegisterCommand(_revealMapCommand);
            DebugCommands.CheatMenu.RegisterCommand(_cheatMenuCommand);
            DebugCommands.GameInfoScreen.RegisterCommand(_gameInfoScreenCommand);
        }

        private void OnUnloaded(object sender, RoutedEventArgs args)
        {
            GalaxyGrid.SectorDoubleClicked -= OnSectorDoubleClicked;
            Content = null;
            GalaxyGrid = null;
            DebugCommands.RevealMap.UnregisterCommand(_revealMapCommand);
            DebugCommands.CheatMenu.UnregisterCommand(_cheatMenuCommand);
            DebugCommands.GameInfoScreen.UnregisterCommand(_gameInfoScreenCommand);
        }

        private void OnLocalPlayerEmpireChanged()
        {
            if (!_appContext.IsGameInPlay || _appContext.IsGameEnding)
                return;

            var localPlayerEmpire = _appContext.LocalPlayerEmpire;
            if (localPlayerEmpire == null)
                return;
        }

        private void OnSectorDoubleClicked(Sector sector)
        {
            if ((sector == null) || (sector.System == null))
                return;

            var colony = sector.System.Colony;
            if (colony == null)
                return;

            _navigationCommands.ActivateScreen.Execute(StandardGameScreens.ColonyScreen);
        }

        private void ExecuteRevealMapCommand(object t)
        {
            if (!_appContext.IsSinglePlayerGame)
                return;

            var map = _appContext.CurrentGame.Universe.Map;
            var playerCiv = _appContext.LocalPlayer.Empire;
            var mapData = _appContext.LocalPlayerEmpire.MapData;

            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    MapLocation loc = new MapLocation(x, y);
                    mapData.SetExplored(loc, true);
                    mapData.SetScanStrength(loc, 99); 
                }
            }

            var diplomat = Diplomat.Get(playerCiv);

            foreach (var civ in GameContext.Current.Civilizations)
            {
                if (civ == playerCiv)
                    continue;
                if (diplomat.GetForeignPower(civ).DiplomacyData.Status == ForeignPowerStatus.NoContact)
                    diplomat.GetForeignPower(civ).DiplomacyData.Status = ForeignPowerStatus.Neutral;
            }
            GalaxyGrid.Update();
        }

        private void ExecuteCheatMenuCommand(object t)
        {

            // to do: just check whether IsHumanPlayer more than one (whenever SP is started by MP-Screen)
            //if (PlayerContext.Current.Players.Count)
            //    if (PlayerContext.Current.Players.Contains)
                    if (!_appContext.IsSinglePlayerGame)
                    {
                MessageDialog.Show("Cheat Menu is not available in MultiPlayer", "INFO", MessageDialogButtons.Ok);
                return;
            }

            var cheatMenu = new CheatMenu(_appContext);
            cheatMenu.ShowDialog();
        }

        private void ExecuteGameInfoScreenCommand(object t)
        {
            //if (!_appContext.IsSinglePlayerGame)
            //    return;

            var gameInfoScreen = new GameInfoScreen(_appContext);
            gameInfoScreen.ShowDialog();
        }

        private void ExecuteColonyInfoScreenCommand(object t)
        {
            //if (!_appContext.IsSinglePlayerGame)
            //    return;

            var colonyInfoScreen = new ColonyInfoScreen(_appContext);
            colonyInfoScreen.ShowDialog();
        }
        #endregion

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            var appContext = sender as IAppContext;
            
            if (appContext == null)
                return false;

            var propertyChangedEventArgs = e as PropertyChangedEventArgs;
            if (propertyChangedEventArgs == null)
                return false;

            switch (propertyChangedEventArgs.PropertyName)
            {
                case "LocalPlayerEmpire":
                    OnLocalPlayerEmpireChanged();
                    break;
            }

            return true;
        }
    }
}