using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.Unity;
using Supremacy.Annotations;
using Supremacy.Buildings;
using Supremacy.Client.Commands;
using Supremacy.Client.Dialogs;
using Supremacy.Client.Events;
using Supremacy.Client.Input;
using Supremacy.Economy;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Tech;
using Supremacy.Universe;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

using CompositeRegionManager = Microsoft.Practices.Composite.Presentation.Regions.RegionManager;

namespace Supremacy.Client.Views
{
    public class ColonyScreenPresenter : GameScreenPresenterBase<ColonyScreenPresentationModel, IColonyScreenView>,
                                         IColonyScreenPresenter
    {
        private readonly DelegateCommand<BuildProject> _addToPlanetaryBuildQueueCommand;
        private readonly DelegateCommand<BuildProject> _addToShipyardBuildQueueCommand;
        private readonly DelegateCommand<BuildQueueItem> _removeFromPlanetaryBuildQueueCommand;
        private readonly DelegateCommand<BuildQueueItem> _removeFromShipyardBuildQueueCommand;
        private readonly DelegateCommand<BuildProject> _cancelBuildProjectCommand;
        private readonly DelegateCommand<BuildProject> _buyBuildProjectCommand;
        private readonly DelegateCommand<ProductionCategory> _activateFacilityCommand;
        private readonly DelegateCommand<ProductionCategory> _deactivateFacilityCommand;
        private readonly DelegateCommand<ProductionCategory> _scrapFacilityCommand;
        private readonly DelegateCommand<ProductionCategory> _unscrapFacilityCommand;
        private readonly DelegateCommand<object> _toggleBuildingScrapCommand;
        private readonly DelegateCommand<Building> _toggleBuildingIsActiveCommand;
        private readonly DelegateCommand<ShipyardBuildSlot> _toggleShipyardBuildSlotCommand;
        private readonly DelegateCommand<ShipyardBuildSlot> _selectShipBuildProjectCommand;
        private readonly DelegateCommand<Sector> _selectSectorCommand;
        private readonly DelegateCommand<object> _previousColonyCommand;
        private readonly DelegateCommand<object> _nextColonyCommand;

        private int _newColonySelection;

        #region Constructors and Finalizers
        public ColonyScreenPresenter(
            [NotNull] IUnityContainer container,
            [NotNull] ColonyScreenPresentationModel model,
            [NotNull] IColonyScreenView view) : base(container, model, view)
        {
            _addToPlanetaryBuildQueueCommand = new DelegateCommand<BuildProject>(
                ExecuteAddToPlanetaryBuildQueueCommand,
                CanExecuteAddToPlanetaryBuildQueueCommand);

            _addToShipyardBuildQueueCommand = new DelegateCommand<BuildProject>(
                ExecuteAddToShipyardBuildQueueCommand,
                CanExecuteAddToShipyardBuildQueueCommand);

            _removeFromPlanetaryBuildQueueCommand = new DelegateCommand<BuildQueueItem>(
                ExecuteRemoveFromPlanetaryBuildQueueCommand,
                CanExecuteRemoveFromPlanetaryBuildQueueCommand);

            _removeFromShipyardBuildQueueCommand = new DelegateCommand<BuildQueueItem>(
                ExecuteRemoveFromShipyardBuildQueueCommand,
                CanExecuteRemoveFromShipyardBuildQueueCommand);

            _cancelBuildProjectCommand = new DelegateCommand<BuildProject>(
                ExecuteCancelBuildProjectCommand,
                CanExecuteCancelBuildProjectCommand);

            _buyBuildProjectCommand = new DelegateCommand<BuildProject>(
                ExecuteBuyBuildProjectCommand,
                CanExecuteBuyBuildProjectCommand);

            _activateFacilityCommand = new DelegateCommand<ProductionCategory>(
                ExecuteActivateFacilityCommand,
                CanExecuteActivateFacilityCommand);

            _deactivateFacilityCommand = new DelegateCommand<ProductionCategory>(
                ExecuteDeactivateFacilityCommand,
                CanExecuteDeactivateFacilityCommand);

            _scrapFacilityCommand = new DelegateCommand<ProductionCategory>(
                ExecuteScrapFacilityCommand,
                CanExecuteScrapFacilityCommand);

            _unscrapFacilityCommand = new DelegateCommand<ProductionCategory>(
                ExecuteUnscrapFacilityCommand,
                CanExecuteUnscrapFacilityCommand);

            _toggleBuildingScrapCommand = new DelegateCommand<object>(
                ExecuteToggleBuildingScrapCommand,
                CanExecuteToggleBuildingScrapCommand);

            _toggleBuildingIsActiveCommand = new DelegateCommand<Building>(
                ExecuteToggleBuildingIsActiveCommand,
                CanExecuteToggleBuildingIsActiveCommand);
            
            _toggleShipyardBuildSlotCommand = new DelegateCommand<ShipyardBuildSlot>(
                ExecuteToggleShipyardBuildSlotCommand,
                CanExecuteToggleShipyardBuildSlotCommand);

            _selectShipBuildProjectCommand = new DelegateCommand<ShipyardBuildSlot>(
                ExecuteSelectShipBuildProjectCommand,
                CanExecuteSelectShipBuildProjectCommand);

            _selectSectorCommand = new DelegateCommand<Sector>(
                sector =>
                {
                    var system = sector.System;
                    if (system == null)
                        return;

                    var colony = system.Colony;
                    if (colony == null || colony.OwnerID != AppContext.LocalPlayer.EmpireID)
                        return;

                    _newColonySelection = colony.ObjectID;
                });

            _previousColonyCommand = new DelegateCommand<object>(ExecutePreviousColonyCommand);
            _nextColonyCommand = new DelegateCommand<object>(ExecuteNextColonyCommand);
        }

        private void ExecutePreviousColonyCommand(object _)
        {
            var colonies = Model.Colonies.ToList();
            var currentColony = Model.SelectedColony;

            var currentColonyIndex = colonies.IndexOf(currentColony);
            if (currentColonyIndex <= 0)
            {
                if (colonies.Count == 0)
                    return;

                Model.SelectedColony = colonies[colonies.Count - 1];
            }
            else
            {
                Model.SelectedColony = colonies[currentColonyIndex - 1];
            }
        }

        private void ExecuteNextColonyCommand(object _)
        {
            var colonies = Model.Colonies.ToList();
            var currentColony = Model.SelectedColony;

            var currentColonyIndex = colonies.IndexOf(currentColony);
            if ((currentColonyIndex == (colonies.Count - 1)) || (currentColonyIndex < 0))
                Model.SelectedColony = colonies[0];
            else
                Model.SelectedColony = colonies[currentColonyIndex + 1];
        }

        protected override void OnViewActivating()
        {
            var newColonySelection = _newColonySelection;
            if (newColonySelection == -1)
            {
                Model.SelectedColony = AppContext.LocalPlayerEmpire.SeatOfGovernment;
                return;
            }
            Model.SelectedColony = AppContext.CurrentGame.Universe.Objects[newColonySelection] as Colony;
        }

        private bool CanExecuteToggleBuildingIsActiveCommand(Building building)
        {
            return (Model.SelectedColony != null);
        }

        private void ExecuteToggleBuildingIsActiveCommand(Building building)
        {
            var colony = Model.SelectedColony;
            if (colony == null)
                return;

            if (building.IsActive)
                colony.DeactivateBuilding(building);
            else
                colony.ActivateBuilding(building);

            PlayerOrderService.AddOrder(new UpdateBuildingOrder(building));
        }

        private bool CanExecuteToggleShipyardBuildSlotCommand(ShipyardBuildSlot buildSlot)
        {
            if (buildSlot == null)
                return false;

            var colony = Model.SelectedColony;
            if (colony == null || colony.Shipyard != buildSlot.Shipyard)
                return false;

            return true;
        }

        private void ExecuteToggleShipyardBuildSlotCommand(ShipyardBuildSlot buildSlot)
        {
            if (buildSlot == null)
                return;

            var colony = Model.SelectedColony;
            if (colony == null || colony.Shipyard != buildSlot.Shipyard)
                return;

            if (buildSlot.IsActive)
                colony.DeactivateShipyardBuildSlot(buildSlot);
            else
                colony.ActivateShipyardBuildSlot(buildSlot);

            PlayerOrderService.AddOrder(new ToggleShipyardBuildSlotOrder(buildSlot));
        }
        
        private bool CanExecuteSelectShipBuildProjectCommand(ShipyardBuildSlot buildSlot)
        {
            if (buildSlot == null)
                return false;

            var colony = Model.SelectedColony;
            if (colony == null || colony.Shipyard != buildSlot.Shipyard)
                return false;

            return buildSlot.IsActive && !buildSlot.HasProject;
        }

        private void ExecuteSelectShipBuildProjectCommand(ShipyardBuildSlot buildSlot)
        {
            if (buildSlot == null)
                return;

            var colony = Model.SelectedColony;
            if (colony == null || colony.Shipyard != buildSlot.Shipyard)
                return;

            if (!buildSlot.IsActive || buildSlot.HasProject)
                return;

            var view = new NewShipSelectionView(buildSlot);
            var statsViewModel = new TechObjectDesignViewModel();

            BindingOperations.SetBinding(
                statsViewModel,
                TechObjectDesignViewModel.DesignProperty,
                new Binding
                {
                    Source = view,
                    Path = new PropertyPath("SelectedBuildProject.BuildDesign")
                });

            view.AdditionalContent = statsViewModel;

            var result = view.ShowDialog();

            if (!result.HasValue || !result.Value)
                return;

            var project = view.SelectedBuildProject;
            if (project == null)
                return;

            buildSlot.Project = project;
            
            PlayerOrderService.AddOrder(new UpdateProductionOrder(buildSlot.Shipyard));
        }

        private bool CanExecuteToggleBuildingScrapCommand(object parameter)
        {
            var checkableParameter = parameter as ICheckableCommandParameter;
            if (checkableParameter != null)
            {
                var building = checkableParameter.InnerParameter as Building;
                if (building == null)
                {
                    checkableParameter.IsChecked = false;
                    return false;
                }
                checkableParameter.IsChecked = building.Scrap;
                checkableParameter.Handled = true;
            }
            else if (!(parameter is Building))
            {
                return false;
            }
            return (Model.SelectedColony != null);
        }

        private void ExecuteToggleBuildingScrapCommand(object parameter)
        {
            var building = parameter as Building;
            if (building != null)
            {
                building.Scrap = !building.Scrap;
            }
            else
            {
                var checkableParameter = parameter as ICheckableCommandParameter;
                if (checkableParameter == null)
                    return;
                
                building = checkableParameter.InnerParameter as Building;
                if (building == null)
                    return;

                checkableParameter.IsChecked = (building.Scrap = !building.Scrap);
                checkableParameter.Handled = true;
            }

            PlayerOrderService.AddOrder(new UpdateBuildingOrder(building));
        }

        private bool CanExecuteUnscrapFacilityCommand(ProductionCategory category)
        {
            return (Model.SelectedColony != null);
        }

        private void ExecuteUnscrapFacilityCommand(ProductionCategory category)
        {
            var colony = Model.SelectedColony;
            if (colony == null)
                return;

            var facilitiesToScrap = colony.GetScrappedFacilities(category);
            if (facilitiesToScrap == 0)
                return;

            colony.SetScrappedFacilities(category, --facilitiesToScrap);

            PlayerOrderService.AddOrder(new FacilityScrapOrder(colony));
        }

        private bool CanExecuteScrapFacilityCommand(ProductionCategory category)
        {
            return (Model.SelectedColony != null);
        }

        private void ExecuteScrapFacilityCommand(ProductionCategory category)
        {
            var colony = Model.SelectedColony;
            if (colony == null)
                return;

            var facilitiesToScrap = colony.GetScrappedFacilities(category);
            if (facilitiesToScrap >= colony.GetTotalFacilities(category))
                return;

            colony.SetScrappedFacilities(category, ++facilitiesToScrap);

            PlayerOrderService.AddOrder(new FacilityScrapOrder(colony));
        }

        private bool CanExecuteDeactivateFacilityCommand(ProductionCategory category)
        {
            return (Model.SelectedColony != null);
        }

        private void ExecuteDeactivateFacilityCommand(ProductionCategory category)
        {
            var colony = Model.SelectedColony;
            if (colony == null)
                return;

            colony.DeactivateFacility(category);

            PlayerOrderService.AddOrder(new SetColonyProductionOrder(colony));
        }

        private bool CanExecuteActivateFacilityCommand(ProductionCategory category)
        {
            return (Model.SelectedColony != null);
        }

        private void ExecuteActivateFacilityCommand(ProductionCategory category)
        {
            var colony = Model.SelectedColony;
            if (colony == null)
                return;

            colony.ActivateFacility(category);

            PlayerOrderService.AddOrder(new SetColonyProductionOrder(colony));
        }

        protected override void RunOverride()
        {
            Model.Colonies = AppContext.LocalPlayerEmpire.Colonies;

            var selectedColony = Model.SelectedColony;
            if (selectedColony == null)
                Model.SelectedColony = AppContext.LocalPlayerEmpire.SeatOfGovernment;
        }

        protected override void TerminateOverride()
        {
            var selectedColony = Model.SelectedColony;
            if (selectedColony != null)
                selectedColony.PropertyChanged -= OnSelectedColonyPropertyChanged;

            Model.Colonies = null;
            Model.SelectedColony = null;
            Model.SelectedPlanetaryBuildProject = null;
            Model.SelectShipBuildProjectCommand = null;
            //this.Model.SelectBuildIntelProjectCommand = null;
        }

        private void OnSelectedColonyPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "NetEnergy" || e.PropertyName == "ActiveOrbitalBatteries")
            {
                UpdateOrbitalBatteries();
            }
            else if (e.PropertyName == "NetIndustry")
            {
                UpdateBuildLists();

                // Update buildItems in colony queue
                var colony = Model.SelectedColony;
                if (colony == null)
                    return;

                foreach (BuildQueueItem item in colony.BuildQueue)
                    item.InvalidateTurnsRemaining();
            }
        }

        private void OnSelectedColonyChanged(object sender, EventArgs args)
        {
            var e = (PropertyChangedRoutedEventArgs<Colony>)args;

            if (!IsRunning)
                return;

            //if (this.Model.Colonies == null)
            //    this.Model.Colonies = this.AppContext.LocalPlayerEmpire.Colonies;

            if (e.OldValue != null)
                e.OldValue.PropertyChanged -= OnSelectedColonyPropertyChanged;

            if (e.NewValue != null)
                e.NewValue.PropertyChanged += OnSelectedColonyPropertyChanged;

            UpdateBuildLists();

            Model.ActiveOrbitalBatteries = (e.NewValue != null) ? e.NewValue.ActiveOrbitalBatteries : 0;

            UpdateOrbitalBatteries();

            var selectedColony = Model.SelectedColony;
            if (selectedColony != null)
            {
                var regionManager = CompositeRegionManager.GetRegionManager((DependencyObject)View);

                if (!regionManager.Regions.ContainsRegionWithName(CommonGameScreenRegions.PlanetsView))
                    CompositeRegionManager.UpdateRegions();

                if (regionManager.Regions.ContainsRegionWithName(CommonGameScreenRegions.PlanetsView))
                {
                    var planetsViewRegion = regionManager.Regions[CommonGameScreenRegions.PlanetsView];
                    planetsViewRegion.Context = selectedColony.Sector;
                }
            }

            InvalidateCommands();
        }

        private void OnActiveOrbitalBatteriesChanged(object sender, EventArgs eventArgs)
        {
            UpdateOrbitalBatteries();
        }

        private bool _updatingOrbitalBatteries;

        private void UpdateOrbitalBatteries()
        {
            if (_updatingOrbitalBatteries)
                return;

            _updatingOrbitalBatteries = true;

            try
            {
                var selectedColony = Model.SelectedColony;
                if (selectedColony == null || selectedColony.OrbitalBatteryDesign == null)
                {
                    Model.ActiveOrbitalBatteries = 0;
                    Model.MaxActiveOrbitalBatteries = 0;
                    return;
                }

                var activeCountDifference = Model.ActiveOrbitalBatteries - selectedColony.ActiveOrbitalBatteries;
                if (activeCountDifference != 0)
                {
                    do
                    {
                        if (activeCountDifference > 0)
                        {
                            if (selectedColony.ActivateOrbitalBattery())
                                --activeCountDifference;
                            else
                                break;
                        }
                        else
                        {
                            if (selectedColony.DeactivateOrbitalBattery())
                                ++activeCountDifference;
                            else
                                break;
                        }
                    }
                    while (activeCountDifference != 0);

                    PlayerOrderService.AddOrder(new UpdateOrbitalBatteriesOrder(selectedColony));
                }

                var maxActiveOrbitalBatteries = selectedColony.ActiveOrbitalBatteries;
                if (selectedColony.NetEnergy > 0)
                {
                    var possibleActivations = selectedColony.NetEnergy / selectedColony.OrbitalBatteryDesign.UnitEnergyCost;
                    if (possibleActivations > 0)
                        maxActiveOrbitalBatteries += possibleActivations;
                }

                Model.MaxActiveOrbitalBatteries = maxActiveOrbitalBatteries;
                Model.ActiveOrbitalBatteries = selectedColony.ActiveOrbitalBatteries;
            }
            finally
            {
                _updatingOrbitalBatteries = false;
            }
        }

        private void UpdateBuildLists()
        {
            var selectedColony = Model.SelectedColony;
            if (selectedColony != null)
            {
                Model.PlanetaryBuildProjects = TechTreeHelper.GetBuildProjects(Model.SelectedColony);
                if (selectedColony.Shipyard != null)
                {
                    BuildProject[] shipList = TechTreeHelper.GetShipyardBuildProjects(selectedColony.Shipyard)
                                                .OrderByDescending(s => s.BuildDesign.BuildCost)
                                                .ToArray();

                    Model.ShipyardBuildProjects = shipList;

                }
                else
                    Model.ShipyardBuildProjects = Enumerable.Empty<BuildProject>();
            }
            else
            {
                Model.PlanetaryBuildProjects = Enumerable.Empty<BuildProject>();
            }
        }

        protected override void InvalidateCommands()
        {
            base.InvalidateCommands();

            _addToPlanetaryBuildQueueCommand.RaiseCanExecuteChanged();
            _addToShipyardBuildQueueCommand.RaiseCanExecuteChanged();
            _removeFromPlanetaryBuildQueueCommand.RaiseCanExecuteChanged();
            _removeFromShipyardBuildQueueCommand.RaiseCanExecuteChanged();
            _cancelBuildProjectCommand.RaiseCanExecuteChanged();
            _buyBuildProjectCommand.RaiseCanExecuteChanged();
            _activateFacilityCommand.RaiseCanExecuteChanged();
            _deactivateFacilityCommand.RaiseCanExecuteChanged();
            _scrapFacilityCommand.RaiseCanExecuteChanged();
            _unscrapFacilityCommand.RaiseCanExecuteChanged();
            _toggleBuildingScrapCommand.RaiseCanExecuteChanged();
            _toggleBuildingIsActiveCommand.RaiseCanExecuteChanged();
            _toggleShipyardBuildSlotCommand.RaiseCanExecuteChanged();
            _selectShipBuildProjectCommand.RaiseCanExecuteChanged();
        }

        protected override void RegisterCommandAndEventHandlers()
        {
            base.RegisterCommandAndEventHandlers();

            Model.AddToPlanetaryBuildQueueCommand = _addToPlanetaryBuildQueueCommand;
            Model.AddToShipyardBuildQueueCommand = _addToShipyardBuildQueueCommand;
            Model.RemoveFromPlanetaryBuildQueueCommand = _removeFromPlanetaryBuildQueueCommand;
            Model.RemoveFromShipyardBuildQueueCommand = _removeFromShipyardBuildQueueCommand;
            Model.CancelBuildProjectCommand = _cancelBuildProjectCommand;
            Model.BuyBuildProjectCommand = _buyBuildProjectCommand;
            Model.ScrapFacilityCommand = _scrapFacilityCommand;
            Model.UnscrapFacilityCommand = _unscrapFacilityCommand;
            Model.ActivateFacilityCommand = _activateFacilityCommand;
            Model.DeactivateFacilityCommand = _deactivateFacilityCommand;
            Model.ToggleBuildingIsActiveCommand = _toggleBuildingIsActiveCommand;
            Model.ToggleBuildingScrapCommand = _toggleBuildingScrapCommand;
            Model.ToggleShipyardBuildSlotCommand = _toggleShipyardBuildSlotCommand;
            Model.SelectShipBuildProjectCommand = _selectShipBuildProjectCommand;

            Model.SelectedColonyChanged += OnSelectedColonyChanged;
            Model.ActiveOrbitalBatteriesChanged += OnActiveOrbitalBatteriesChanged;

            ColonyScreenCommands.ToggleBuildingScrapCommand.RegisterCommand(_toggleBuildingScrapCommand);
            ColonyScreenCommands.PreviousColonyCommand.RegisterCommand(_previousColonyCommand);
            ColonyScreenCommands.NextColonyCommand.RegisterCommand(_nextColonyCommand);

            GalaxyScreenCommands.SelectSector.RegisterCommand(_selectSectorCommand);

            ClientEvents.TurnStarted.Subscribe(OnTurnStarted, ThreadOption.UIThread);
        }

        private void OnTurnStarted(GameContextEventArgs args)
        {
            var selectedColony = Model.SelectedColony;
            if (selectedColony == null)
                Model.SelectedColony = AppContext.LocalPlayerEmpire.SeatOfGovernment;

            Model.Colonies = AppContext.LocalPlayerEmpire.Colonies;
        }

        private bool CanExecuteCancelBuildProjectCommand(BuildProject project)
        {
            if (Model.SelectedColony == null)
                return false;

            if (project is ShipBuildProject)
                return (Model.SelectedColony.Shipyard != null);

            return true;
        }

        private void ExecuteCancelBuildProjectCommand([NotNull] BuildProject project)
        {
            if (project == null)
                throw new ArgumentNullException("project");

            var productionCenter = project.ProductionCenter;
            if (productionCenter == null)
                return;

            var buildSlot = productionCenter.BuildSlots.FirstOrDefault(o => o.Project == project);
            if (buildSlot == null)
                return;

            if (project.IsPartiallyComplete || project.IsRushed)
            {
                var confirmResult = MessageDialog.Show(
                    ResourceManager.GetString("CONFIRM_CANCEL_BUILD_HEADER"),
                    ResourceManager.GetString("CONFIRM_CANCEL_BUILD_MESSAGE"),
                    MessageDialogButtons.YesNo);

                if (confirmResult != MessageDialogResult.Yes)
                    return;
            }

            if (project.IsRushed)
            {
                var civMan = CivilizationManager.For(productionCenter.Owner);
                civMan.Credits.AdjustCurrent(project.GetTotalCreditsCost());
            }

            project.Cancel();
            productionCenter.ProcessQueue();

            PlayerOrderService.AddOrder(new UpdateProductionOrder(productionCenter));

            UpdateBuildLists();
        }

        private bool CanExecuteBuyBuildProjectCommand(BuildProject project)
        {
            if (project == null)
                return false;

            if (project.IsCancelled || project.IsCompleted || project.IsRushed)
                return false;
            
            if (Model.SelectedColony == null)
                return false;

            var civMan = CivilizationManager.For(Model.SelectedColony.Owner);

            if (civMan.Credits.CurrentValue < project.GetTotalCreditsCost())
            {
                int missingCredits = project.GetCurrentIndustryCost() - civMan.Credits.CurrentValue;
                string message = string.Format(ResourceManager.GetString("RUSH_BUILDING_INSUFFICIENT_CREDITS_MESSAGE"), missingCredits);
                var result = MessageDialog.Show(ResourceManager.GetString("RUSH_BUILDING_INSUFFICIENT_CREDITS_HEADER"), message, MessageDialogButtons.Ok);
                return false;
            }

            return true;
        }

        private void ExecuteBuyBuildProjectCommand([NotNull] BuildProject project)
        {
            if (project == null)
                throw new ArgumentNullException("project");

            var productionCenter = project.ProductionCenter;
            if (productionCenter == null)
                return;

            var buildSlot = productionCenter.BuildSlots.FirstOrDefault(o => o.Project == project);
            if (buildSlot == null)
                return;

            var civMan = CivilizationManager.For(Model.SelectedColony.Owner);

            string confirmationMessage = string.Format(ResourceManager.GetString("CONFIRM_RUSH_BUILDING_MESSAGE"),
                project.GetTotalCreditsCost(), civMan.Credits.CurrentValue);
            var confirmResult = MessageDialog.Show(
                ResourceManager.GetString("CONFIRM_RUSH_BUILDING_HEADER"),
                confirmationMessage,
                MessageDialogButtons.YesNo);
            if (confirmResult != MessageDialogResult.Yes)
                return;

            // Temporarily update the resources so the player can immediately see the results of his spending, else we would get updated values only at the next turn.
            civMan.Credits.AdjustCurrent(-project.GetTotalCreditsCost());

            project.IsRushed = true;
            PlayerOrderService.AddOrder(new RushProductionOrder(productionCenter));
        }

        private bool CanExecuteRemoveFromShipyardBuildQueueCommand(BuildQueueItem item)
        {
            return ((Model.SelectedColony != null) && (Model.SelectedColony.Shipyard != null));
        }

        private void ExecuteRemoveFromShipyardBuildQueueCommand(BuildQueueItem item)
        {
            var colony = Model.SelectedColony;
            if (colony == null)
                return;

            if (colony.Shipyard == null)
                return;

            RemoveItemFromBuildQueue(item, colony.Shipyard);
        }

        private bool CanExecuteRemoveFromPlanetaryBuildQueueCommand(BuildQueueItem item)
        {
            return (Model.SelectedColony != null);
        }

        private void ExecuteRemoveFromPlanetaryBuildQueueCommand(BuildQueueItem item)
        {
            var colony = Model.SelectedColony;
            if (colony == null)
                return;

            RemoveItemFromBuildQueue(item, colony);
        }

        private bool CanExecuteAddToShipyardBuildQueueCommand(BuildProject project)
        {
            return ((Model.SelectedColony != null) && (Model.SelectedColony.Shipyard != null));
        }

        private void ExecuteAddToShipyardBuildQueueCommand(BuildProject project)
        {
            var colony = Model.SelectedColony;
            if (colony == null)
                return;

            if (colony.Shipyard == null)
                return;

            AddProjectToBuildQueue(project, colony.Shipyard);
        }

        protected void RemoveItemFromBuildQueue([NotNull] BuildQueueItem item, [NotNull] IProductionCenter productionCenter)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            if (productionCenter == null)
                throw new ArgumentNullException("productionCenter");

            if ((item.Count <= 1) || !item.DecrementCount())
                productionCenter.BuildQueue.Remove(item);

            PlayerOrderService.AddOrder(new UpdateProductionOrder(productionCenter));

            UpdateBuildLists();
        }

        protected void AddProjectToBuildQueue([NotNull] BuildProject project, [NotNull] IProductionCenter productionCenter)
        {
            if (project == null)
                throw new ArgumentNullException("project");
            if (productionCenter == null)
                throw new ArgumentNullException("productionCenter");

            var newItemAdded = true;
            var lastItemInQueue = productionCenter.BuildQueue.LastOrDefault();

            if ((lastItemInQueue != null) && project.IsEquivalent(lastItemInQueue.Project))
            {
                if (lastItemInQueue.IncrementCount())
                    newItemAdded = false;
            }

            if (newItemAdded)
            {
                productionCenter.BuildQueue.Add(new BuildQueueItem(project));
                productionCenter.ProcessQueue();
            }

            PlayerOrderService.AddOrder(new UpdateProductionOrder(productionCenter));

            if (productionCenter is Colony)
                Model.SelectedPlanetaryBuildProject = null;
            else if (productionCenter is Shipyard)
                Model.SelectedShipyardBuildProject = null;

            UpdateBuildLists();
        }

        protected override void UnregisterCommandAndEventHandlers()
        {
            base.UnregisterCommandAndEventHandlers();

            Model.AddToPlanetaryBuildQueueCommand = null;
            Model.AddToShipyardBuildQueueCommand = null;
            Model.RemoveFromPlanetaryBuildQueueCommand = null;
            Model.RemoveFromShipyardBuildQueueCommand = null;
            Model.CancelBuildProjectCommand = null;
            Model.BuyBuildProjectCommand = null;
            Model.ScrapFacilityCommand = null;
            Model.UnscrapFacilityCommand = null;
            Model.ActivateFacilityCommand = null;
            Model.DeactivateFacilityCommand = null;
            Model.ToggleBuildingIsActiveCommand = null;
            Model.ToggleBuildingScrapCommand = null;

            Model.SelectedColonyChanged -= OnSelectedColonyChanged;
            Model.ActiveOrbitalBatteriesChanged -= OnActiveOrbitalBatteriesChanged;

            ColonyScreenCommands.ToggleBuildingScrapCommand.UnregisterCommand(_toggleBuildingScrapCommand);
            ColonyScreenCommands.PreviousColonyCommand.UnregisterCommand(_previousColonyCommand);
            ColonyScreenCommands.NextColonyCommand.UnregisterCommand(_nextColonyCommand);

            GalaxyScreenCommands.SelectSector.UnregisterCommand(_selectSectorCommand);

            ClientEvents.TurnStarted.Unsubscribe(OnTurnStarted);
        }

        private bool CanExecuteAddToPlanetaryBuildQueueCommand(BuildProject arg)
        {
            return (Model.SelectedColony != null);
        }

        private void ExecuteAddToPlanetaryBuildQueueCommand([NotNull] BuildProject project)
        {
            if (project == null)
                throw new ArgumentNullException("project");

            var colony = Model.SelectedColony;
            if (colony == null)
                return;

            AddProjectToBuildQueue(project, colony);
        }

        #endregion

        #region Overrides of GameScreenPresenterBase<ColonyScreenPresentationModel,IColonyScreenView>

        protected override string ViewName
        {
            get { return StandardGameScreens.ColonyScreen; }
        }

        #endregion
    }
}