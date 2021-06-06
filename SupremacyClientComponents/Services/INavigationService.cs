using Microsoft.Practices.Composite.Regions;
using Supremacy.Annotations;
using Supremacy.Client.Commands;
using Supremacy.Client.Context;
using Supremacy.Client.Dialogs;
using Supremacy.Client.Events;
using Supremacy.Client.Input;
using Supremacy.Economy;
using Supremacy.Game;
using Supremacy.Resources;
using Supremacy.Universe;
using Supremacy.Utility;
using System;

namespace Supremacy.Client.Services
{
    public interface INavigationService
    {
        bool ActivateScreen(string screenName); // jumps to a different screen
        void NavigateToColony(Colony colony);   // jumps to the galaxy screen, selecting and centering the colony
        void RushColonyProduction(Colony colony);    // not the good place for this but don't know where to put it. Will ask the user if he really wants to rush the production
    }

    public class NavigationService : INavigationService
    {
        private readonly IDispatcherService _dispatcherService;
        private readonly IRegionManager _regionManager;
        private readonly INavigationCommandsProxy _navigationCommands;
        private readonly IAppContext _appContext;

        public NavigationService(
            [NotNull] IDispatcherService dispatcherService,
            [NotNull] IRegionManager regionManager,
            [NotNull] INavigationCommandsProxy navigationCommands,
            [NotNull] IAppContext appContext)
        {
            if (dispatcherService == null)
                throw new ArgumentNullException("dispatcherService");
            if (regionManager == null)
                throw new ArgumentNullException("regionManager");
            if (navigationCommands == null)
                throw new ArgumentNullException("navigationCommands");
            if (appContext == null)
                throw new ArgumentNullException("appContext");

            _dispatcherService = dispatcherService;
            _regionManager = regionManager;
            _navigationCommands = navigationCommands;
            _appContext = appContext;

            _navigationCommands.ActivateScreen.RegisterCommand(new DelegateCommand<string>(s => _dispatcherService.Invoke((Func<string, bool>)ActivateScreen, s)));
            _navigationCommands.NavigateToColony.RegisterCommand(new DelegateCommand<Colony>(NavigateToColony));
            _navigationCommands.RushColonyProduction.RegisterCommand(new DelegateCommand<Colony>(RushColonyProduction));
        }

        #region Implementation of INavigationService

        public bool ActivateScreen(string screenName)
        {
            object view = _regionManager.Regions[ClientRegions.GameScreens].GetView(screenName);
            if (view == null)
                return false;

            ViewActivatingEventArgs activatingArgs = new ViewActivatingEventArgs(view);

            ClientEvents.ViewActivating.Publish(activatingArgs);

            if (activatingArgs.Cancel)
                return false;

            GameLog.Client.UI.DebugFormat("[INavigationService] Activating Screen: {0}", screenName);

            _regionManager.Regions[ClientRegions.GameScreens].Activate(view);

            GameLog.Client.UI.DebugFormat("[INavigationService] Screen Activated: {0}", screenName);

            return true;
        }

        public void NavigateToColony(Colony colony)
        {
            if (!_appContext.IsGameInPlay)
                return;

            CivilizationManager playerEmpire = _appContext.LocalPlayerEmpire;
            if (playerEmpire == null)
                return;

            if (colony == null)
                return;

            GameLog.Client.UI.DebugFormat("[INavigationService] Navigating to Colony: {0}", colony.Name);

            bool ownedByPlayer = (colony.OwnerID == playerEmpire.CivilizationID);

            ActivateScreen(StandardGameScreens.GalaxyScreen);

            GalaxyScreenCommands.SelectSector.Execute(colony.Sector);
            GalaxyScreenCommands.CenterOnSector.Execute(colony.Sector);

            if (!ownedByPlayer)
                return;
        }

        public void RushColonyProduction(Colony colony)
        {
            CivilizationManager playerEmpire = _appContext.LocalPlayerEmpire;
            if (playerEmpire == null)
                return;

            if (colony == null)
                return;

            bool ownedByPlayer = (colony.OwnerID == playerEmpire.CivilizationID);
            if (!ownedByPlayer)
                return;

            BuildProject project = colony.BuildSlots[0].Project;
            if (project == null)
                return;

            if (project.IsCancelled || project.IsCompleted || project.IsRushed)
                return;

            int creditsNeeded = project.GetTotalCreditsCost();

            if (playerEmpire.Credits.CurrentValue < creditsNeeded)
                return;

            string confirmationMessage = string.Format(ResourceManager.GetString("CONFIRM_RUSH_BUILDING_MESSAGE"),
                creditsNeeded, playerEmpire.Credits.CurrentValue);

            MessageDialogResult confirmResult = MessageDialog.Show(
                ResourceManager.GetString("CONFIRM_RUSH_BUILDING_HEADER"),
                confirmationMessage,
                MessageDialogButtons.YesNo);

            if (confirmResult != MessageDialogResult.Yes)
                return;

            GameLog.Client.Colonies.DebugFormat("Rushing production for Colony: {0}", colony.Name);

            project.IsRushed = true;
            PlayerOrderService.Instance.AddOrder(new RushProductionOrder(project.ProductionCenter));
        }

        #endregion
    }
}