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
        private readonly DelegateCommand<object> _f12_ScreenCommand;
        private readonly DelegateCommand<object> _f11_ScreenCommand;
        private readonly DelegateCommand<object> _f10_ScreenCommand;
        private readonly DelegateCommand<object> _f09_ScreenCommand;
        private readonly DelegateCommand<object> _f08_ScreenCommand;
        private readonly DelegateCommand<object> _f07_ScreenCommand;
        private readonly DelegateCommand<object> _f06_ScreenCommand;

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
            _f12_ScreenCommand = new DelegateCommand<object>(Execute_f12_ScreenCommand);
            _f11_ScreenCommand = new DelegateCommand<object>(Execute_f11_ScreenCommand);
            _f10_ScreenCommand = new DelegateCommand<object>(Execute_f10_ScreenCommand);
            _f09_ScreenCommand = new DelegateCommand<object>(Execute_f09_ScreenCommand);
            _f08_ScreenCommand = new DelegateCommand<object>(Execute_f08_ScreenCommand);
            _f07_ScreenCommand = new DelegateCommand<object>(Execute_f07_ScreenCommand);
            _f06_ScreenCommand = new DelegateCommand<object>(Execute_f06_ScreenCommand);

            DebugCommands.RevealMap.RegisterCommand(_revealMapCommand);
            DebugCommands.CheatMenu.RegisterCommand(_cheatMenuCommand);
            DebugCommands.F12_Screen.RegisterCommand(_f12_ScreenCommand);
            DebugCommands.F11_Screen.RegisterCommand(_f11_ScreenCommand);
            DebugCommands.F10_Screen.RegisterCommand(_f10_ScreenCommand);
            DebugCommands.F09_Screen.RegisterCommand(_f09_ScreenCommand);
            DebugCommands.F08_Screen.RegisterCommand(_f08_ScreenCommand);
            DebugCommands.F07_Screen.RegisterCommand(_f07_ScreenCommand);
            DebugCommands.F06_Screen.RegisterCommand(_f06_ScreenCommand);
        }

        private void OnUnloaded(object sender, RoutedEventArgs args)
        {
            GalaxyGrid.SectorDoubleClicked -= OnSectorDoubleClicked;
            Content = null;
            GalaxyGrid = null;
            DebugCommands.RevealMap.UnregisterCommand(_revealMapCommand);
            DebugCommands.CheatMenu.UnregisterCommand(_cheatMenuCommand);
            DebugCommands.F12_Screen.UnregisterCommand(_f12_ScreenCommand);
            DebugCommands.F11_Screen.UnregisterCommand(_f11_ScreenCommand);
            DebugCommands.F10_Screen.UnregisterCommand(_f10_ScreenCommand);
            DebugCommands.F09_Screen.UnregisterCommand(_f09_ScreenCommand);
            DebugCommands.F08_Screen.UnregisterCommand(_f08_ScreenCommand);
            DebugCommands.F07_Screen.UnregisterCommand(_f07_ScreenCommand);
            DebugCommands.F06_Screen.UnregisterCommand(_f06_ScreenCommand);
        }

        private void OnLocalPlayerEmpireChanged()
        {
            if (!_appContext.IsGameInPlay || _appContext.IsGameEnding)
                return;

            CivilizationManager localPlayerEmpire = _appContext.LocalPlayerEmpire;
            if (localPlayerEmpire == null)
                return;
        }

        private void OnSectorDoubleClicked(Sector sector)
        {
            if ((sector == null) || (sector.System == null))
                return;

            Colony colony = sector.System.Colony;
            if (colony == null)
                return;

            _navigationCommands.ActivateScreen.Execute(StandardGameScreens.ColonyScreen);
        }

        private void ExecuteRevealMapCommand(object t)
        {
            if (!_appContext.IsSinglePlayerGame)
                return;

            SectorMap map = _appContext.CurrentGame.Universe.Map;
            Entities.Civilization playerCiv = _appContext.LocalPlayer.Empire;
            CivilizationMapData mapData = _appContext.LocalPlayerEmpire.MapData;

            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    MapLocation loc = new MapLocation(x, y);
                    mapData.SetExplored(loc, true);
                    mapData.SetScanStrength(loc, 99); 
                }
            }

            Diplomat diplomat = Diplomat.Get(playerCiv);

            foreach (Entities.Civilization civ in GameContext.Current.Civilizations)
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

            CheatMenu cheatMenu = new CheatMenu(_appContext);
            cheatMenu.ShowDialog();
        }

        private void Execute_f12_ScreenCommand(object t)
        {
            //if (!_appContext.IsSinglePlayerGame)
            //    return;

            GameInfoScreen _f12_Screen = new GameInfoScreen(_appContext);
            _f12_Screen.ShowDialog();
        }
        private void Execute_f11_ScreenCommand(object t)
        {
            //if (!_appContext.IsSinglePlayerGame)
            //    return;

            GameInfoScreen _f11_Screen = new GameInfoScreen(_appContext);
            _f11_Screen.ShowDialog();
        }
        private void Execute_f10_ScreenCommand(object t)
        {
            //if (!_appContext.IsSinglePlayerGame)
            //    return;

            GameInfoScreen _f10_Screen = new GameInfoScreen(_appContext);
            _f10_Screen.ShowDialog();
        }

        private void Execute_f09_ScreenCommand(object t)
        {
            //if (!_appContext.IsSinglePlayerGame)
            //    return;

            //var _f09_Screen = new GameInfoScreen(_appContext);
            //_f09_Screen.ShowDialog();
            GameInfoScreen GameInfoScreen = new GameInfoScreen(_appContext);
            GameInfoScreen.ShowDialog();
        }

        private void Execute_f08_ScreenCommand(object t)
        {
            //if (!_appContext.IsSinglePlayerGame)
            //    return;

            ColonyInfoScreen _f08_Screen = new ColonyInfoScreen(_appContext);
            _f08_Screen.ShowDialog();
        }

        private void Execute_f07_ScreenCommand(object t)
        {
            //if (!_appContext.IsSinglePlayerGame)
            //    return;

            GameInfoScreen _f07_Screen = new GameInfoScreen(_appContext);
            _f07_Screen.ShowDialog();
        }

        private void Execute_f06_ScreenCommand(object t)
        {
            //if (!_appContext.IsSinglePlayerGame)
            //    return;

            ColorInfoScreen _f06_Screen = new ColorInfoScreen(_appContext);
            _f06_Screen.ShowDialog();
        }
        #endregion

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            IAppContext appContext = sender as IAppContext;
            
            if (appContext == null)
                return false;

            PropertyChangedEventArgs propertyChangedEventArgs = e as PropertyChangedEventArgs;
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