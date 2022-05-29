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
using Supremacy.Utility;
using System;
using System.Collections.Generic;
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
        //private readonly DelegateCommand<BuildProject> _addOneMoreToShipyardBuildQueueCommand;
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

            //_addOneMoreToShipyardBuildQueueCommand = new DelegateCommand<BuildProject>(
            //    ExecuteAddOneMoreToShipyardBuildQueueCommand,
            //    CanExecuteAddOneMoreToShipyardBuildQueueCommand);

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
                    StarSystem system = sector.System;
                    if (system == null)
                    {
                        return;
                    }

                    Colony colony = system.Colony;
                    if (colony == null || colony.OwnerID != AppContext.LocalPlayer.EmpireID)
                    {
                        return;
                    }

                    _newColonySelection = colony.ObjectID;
                });

            _previousColonyCommand = new DelegateCommand<object>(ExecutePreviousColonyCommand);
            _nextColonyCommand = new DelegateCommand<object>(ExecuteNextColonyCommand);
        }

        private void ExecutePreviousColonyCommand(object _)
        {
            List<Colony> colonies = Model.Colonies.ToList();
            Colony currentColony = Model.SelectedColony;

            int currentColonyIndex = colonies.IndexOf(currentColony);
            if (currentColonyIndex <= 0)
            {
                if (colonies.Count == 0)
                {
                    return;
                }

                Model.SelectedColony = colonies[colonies.Count - 1];
            }
            else
            {
                Model.SelectedColony = colonies[currentColonyIndex - 1];
            }
        }

        private void ExecuteNextColonyCommand(object _)
        {
            List<Colony> colonies = Model.Colonies.ToList();
            Colony currentColony = Model.SelectedColony;

            int currentColonyIndex = colonies.IndexOf(currentColony);
            Model.SelectedColony = (currentColonyIndex == (colonies.Count - 1)) || (currentColonyIndex < 0) ? colonies[0] : colonies[currentColonyIndex + 1];
        }

        protected override void OnViewActivating()
        {
            int newColonySelection = _newColonySelection;
            if (newColonySelection == -1)
            {
                Model.SelectedColony = AppContext.LocalPlayerEmpire.SeatOfGovernment;
                return;
            }
            Model.SelectedColony = AppContext.CurrentGame.Universe.Objects[newColonySelection] as Colony;
        }

        private bool CanExecuteToggleBuildingIsActiveCommand(Building building)
        {
            return Model.SelectedColony != null;
        }

        private void ExecuteToggleBuildingIsActiveCommand(Building building)
        {
            Colony colony = Model.SelectedColony;
            if (colony == null)
            {
                return;
            }

            _ = building.IsActive ? colony.DeactivateBuilding(building) : colony.ActivateBuilding(building);

            PlayerOrderService.AddOrder(new UpdateBuildingOrder(building));
        }

        private bool CanExecuteToggleShipyardBuildSlotCommand(ShipyardBuildSlot buildSlot)
        {
            if (buildSlot == null)
            {
                return false;
            }

            Colony colony = Model.SelectedColony;
            if (colony == null || colony.Shipyard != buildSlot.Shipyard)
            {
                return false;
            }

            return true;
        }

        private void ExecuteToggleShipyardBuildSlotCommand(ShipyardBuildSlot buildSlot)
        {
            if (buildSlot == null)
            {
                return;
            }

            Colony colony = Model.SelectedColony;
            if (colony == null || colony.Shipyard != buildSlot.Shipyard)
            {
                return;
            }

            _ = buildSlot.IsActive ? colony.DeactivateShipyardBuildSlot(buildSlot) : colony.ActivateShipyardBuildSlot(buildSlot);

            PlayerOrderService.AddOrder(new ToggleShipyardBuildSlotOrder(buildSlot));
        }

        private bool CanExecuteSelectShipBuildProjectCommand(ShipyardBuildSlot buildSlot)
        {
            if (buildSlot == null)
            {
                return false;
            }

            Colony colony = Model.SelectedColony;
            if (colony == null || colony.Shipyard != buildSlot.Shipyard)
            {
                return false;
            }

            return true; //buildSlot.IsActive; // && !buildSlot.HasProject;
        }

        private void ExecuteSelectShipBuildProjectCommand(ShipyardBuildSlot buildSlot)
        {
            if (buildSlot == null)
            {
                return;
            }

            Colony colony = Model.SelectedColony;
            if (colony == null || colony.Shipyard != buildSlot.Shipyard)
            {
                return;
            }

            //if (!buildSlot.IsActive || buildSlot.HasProject)
            //    return;

            OnceAgain:

            NewShipSelectionView view = new NewShipSelectionView(buildSlot);
            TechObjectDesignViewModel statsViewModel = new TechObjectDesignViewModel();

            _ = BindingOperations.SetBinding(
                statsViewModel,
                TechObjectDesignViewModel.DesignProperty,
                new Binding
                {
                    Source = view,
                    Path = new PropertyPath("SelectedBuildProject.BuildDesign")
                });

            view.AdditionalContent = statsViewModel;

            bool? result = view.ShowDialog();

            if (!result.HasValue || !result.Value)
            {
                return;
            }

            ShipBuildProject project = view.SelectedBuildProject;
            if (project == null)
            {
                return;
            }
            //var _buildQueueItem = new BuildQueueItem(project);
            AddProjectToBuildSlotQueue(project, colony.Shipyard);
            //AddProjectToBuildQueue(project, colony);
            //buildSlot.Shipyard.BuildQueue.Add(_buildQueueItem);
            //buildSlot.Shipyard.ProcessQueue();
            //buildSlot.Project = project;

            PlayerOrderService.AddOrder(new UpdateProductionOrder(buildSlot.Shipyard));
            goto OnceAgain;
        }

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        //private void ExecuteAddOneMoreShipBuildProjectCommand(ShipyardBuildSlot buildSlot, ShipBuildProject project)
        //{
        //    if (buildSlot == null)
        //    {
        //        return;
        //    }

        //    Colony colony = Model.SelectedColony;
        //    if (colony == null || colony.Shipyard != buildSlot.Shipyard)
        //    {
        //        return;
        //    }

        //    //if (!buildSlot.IsActive || buildSlot.HasProject)
        //    //    return;

        //    //NewShipSelectionView view = new NewShipSelectionView(buildSlot);
        //    //TechObjectDesignViewModel statsViewModel = new TechObjectDesignViewModel();

        //    //_ = BindingOperations.SetBinding(
        //    //    statsViewModel,
        //    //    TechObjectDesignViewModel.DesignProperty,
        //    //    new Binding
        //    //    {
        //    //        Source = view,
        //    //        Path = new PropertyPath("SelectedBuildProject.BuildDesign")
        //    //    });

        //    //view.AdditionalContent = statsViewModel;

        //    //bool? result = view.ShowDialog();

        //    //if (!result.HasValue || !result.Value)
        //    //{
        //    //    return;
        //    //}

        //    //ShipBuildProject project = view.SelectedBuildProject;
        //    if (project == null)
        //    {
        //        return;
        //    }
        //    //var _buildQueueItem = new BuildQueueItem(project);
        //    AddProjectToBuildSlotQueue(project, colony.Shipyard);
        //    //AddProjectToBuildQueue(project, colony);
        //    //buildSlot.Shipyard.BuildQueue.Add(_buildQueueItem);
        //    //buildSlot.Shipyard.ProcessQueue();
        //    //buildSlot.Project = project;

        //    PlayerOrderService.AddOrder(new UpdateProductionOrder(buildSlot.Shipyard));
        //}

        private bool CanExecuteToggleBuildingScrapCommand(object parameter)
        {
            if (parameter is ICheckableCommandParameter checkableParameter)
            {
                Building building = checkableParameter.InnerParameter as Building;
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
            return Model.SelectedColony != null;
        }

        private void ExecuteToggleBuildingScrapCommand(object parameter)
        {
            Building building = parameter as Building;
            if (building != null)
            {
                building.Scrap = !building.Scrap;
            }
            else
            {
                if (!(parameter is ICheckableCommandParameter checkableParameter))
                {
                    return;
                }

                building = checkableParameter.InnerParameter as Building;
                if (building == null)
                {
                    return;
                }

                checkableParameter.IsChecked = building.Scrap = !building.Scrap;
                checkableParameter.Handled = true;
            }

            PlayerOrderService.AddOrder(new UpdateBuildingOrder(building));
        }

        private bool CanExecuteUnscrapFacilityCommand(ProductionCategory category)
        {
            return Model.SelectedColony != null;
        }

        private void ExecuteUnscrapFacilityCommand(ProductionCategory category)
        {
            Colony colony = Model.SelectedColony;
            if (colony == null)
            {
                return;
            }

            int facilitiesToScrap = colony.GetScrappedFacilities(category);
            if (facilitiesToScrap == 0)
            {
                return;
            }

            colony.SetScrappedFacilities(category, --facilitiesToScrap);

            PlayerOrderService.AddOrder(new FacilityScrapOrder(colony));
        }

        private bool CanExecuteScrapFacilityCommand(ProductionCategory category)
        {
            return Model.SelectedColony != null;
        }

        private void ExecuteScrapFacilityCommand(ProductionCategory category)
        {
            Colony colony = Model.SelectedColony;
            if (colony == null)
            {
                return;
            }

            int facilitiesToScrap = colony.GetScrappedFacilities(category);
            if (facilitiesToScrap >= colony.GetTotalFacilities(category))
            {
                return;
            }

            colony.SetScrappedFacilities(category, ++facilitiesToScrap);

            PlayerOrderService.AddOrder(new FacilityScrapOrder(colony));
        }

        private bool CanExecuteDeactivateFacilityCommand(ProductionCategory category)
        {
            return Model.SelectedColony != null;
        }

        private void ExecuteDeactivateFacilityCommand(ProductionCategory category)
        {
            Colony colony = Model.SelectedColony;
            if (colony == null)
            {
                return;
            }

            _ = colony.DeactivateFacility(category);

            PlayerOrderService.AddOrder(new SetColonyProductionOrder(colony));
        }

        private bool CanExecuteActivateFacilityCommand(ProductionCategory category)
        {
            return Model.SelectedColony != null;
        }

        private void ExecuteActivateFacilityCommand(ProductionCategory category)
        {
            Colony colony = Model.SelectedColony;
            if (colony == null)
            {
                return;
            }

            _ = colony.ActivateFacility(category);

            PlayerOrderService.AddOrder(new SetColonyProductionOrder(colony));
        }

        protected override void RunOverride()
        {
            Model.Colonies = AppContext.LocalPlayerEmpire.Colonies;

            Colony selectedColony = Model.SelectedColony;
            if (selectedColony == null)
            {
                Model.SelectedColony = AppContext.LocalPlayerEmpire.SeatOfGovernment;
            }
        }

        protected override void TerminateOverride()
        {
            Colony selectedColony = Model.SelectedColony;
            if (selectedColony != null)
            {
                selectedColony.PropertyChanged -= OnSelectedColonyPropertyChanged;
            }

            Model.Colonies = null;
            Model.SelectedColony = null;
            Model.SelectedPlanetaryBuildProject = null;
            Model.SelectShipBuildProjectCommand = null;
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
                Colony colony = Model.SelectedColony;
                if (colony == null)
                {
                    return;
                }

                foreach (BuildQueueItem item in colony.BuildQueue)
                {
                    item.InvalidateTurnsRemaining();
                }
            }
        }

        private void OnSelectedColonyChanged(object sender, EventArgs args)
        {
            PropertyChangedRoutedEventArgs<Colony> e = (PropertyChangedRoutedEventArgs<Colony>)args;

            //GameLog.Core.UI.DebugFormat("OnSelectedColonyChanged -> Step 1");

            if (!IsRunning)
            {
                return;
            }

            //GameLog.Core.UI.DebugFormat("OnSelectedColonyChanged -> Step 2");

            if (this.Model.SelectedColony == null)
                this.Model.SelectedColony = this.AppContext.LocalPlayerEmpire.SeatOfGovernment;

            if (e.OldValue != null)
            {
                e.OldValue.PropertyChanged -= OnSelectedColonyPropertyChanged;
            }

            if (e.NewValue != null)
            {
                e.NewValue.PropertyChanged += OnSelectedColonyPropertyChanged;
            }

            UpdateBuildLists();

            Model.ActiveOrbitalBatteries = (e.NewValue != null) ? e.NewValue.ActiveOrbitalBatteries : 0;

            UpdateOrbitalBatteries();

            //GameLog.Core.UI.DebugFormat("OnSelectedColonyChanged -> Step 3");

            Colony selectedColony = Model.SelectedColony;
            GameLog.Core.UIDetails.DebugFormat("OnSelectedColonyChanged: selectedColony = {0}", selectedColony);  // Colony changes...
                                                                                                                  // ..."in the background", in F2 = System Screen (only own colonies), not in Galaxy View showing planets of foreign colonies
            if (selectedColony != null)
            {
                Microsoft.Practices.Composite.Regions.IRegionManager regionManager = CompositeRegionManager.GetRegionManager((DependencyObject)View);

                if (!regionManager.Regions.ContainsRegionWithName(CommonGameScreenRegions.PlanetsView))
                {
                    CompositeRegionManager.UpdateRegions();
                }

                if (regionManager.Regions.ContainsRegionWithName(CommonGameScreenRegions.PlanetsView))
                {
                    Microsoft.Practices.Composite.Regions.IRegion planetsViewRegion = regionManager.Regions[CommonGameScreenRegions.PlanetsView];
                    planetsViewRegion.Context = selectedColony.Sector;
                    //GameLog.Core.UIDetails.DebugFormat("OnSelectedColonyChanged: NEW value selectedColony.Sector = {0}", selectedColony.Sector);
                }
            }

            InvalidateCommands();
        }

        private void OnActiveOrbitalBatteriesChanged(object sender, EventArgs eventArgs)
        {
            UpdateOrbitalBatteries();
        }

        private bool _updatingOrbitalBatteries;
        private string _text;

        private void UpdateOrbitalBatteries()
        {
            if (_updatingOrbitalBatteries)
            {
                return;
            }

            _updatingOrbitalBatteries = true;

            try
            {
                Colony selectedColony = Model.SelectedColony;
                if (selectedColony == null || selectedColony.OrbitalBatteryDesign == null)
                {
                    Model.ActiveOrbitalBatteries = 0;
                    Model.MaxActiveOrbitalBatteries = 0;
                    return;
                }

                int activeCountDifference = Model.ActiveOrbitalBatteries - selectedColony.ActiveOrbitalBatteries;
                if (activeCountDifference != 0)
                {
                    do
                    {
                        if (activeCountDifference > 0)
                        {
                            if (selectedColony.ActivateOrbitalBattery())
                            {
                                --activeCountDifference;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (selectedColony.DeactivateOrbitalBattery())
                            {
                                ++activeCountDifference;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    while (activeCountDifference != 0);

                    PlayerOrderService.AddOrder(new UpdateOrbitalBatteriesOrder(selectedColony));
                }

                int maxActiveOrbitalBatteries = selectedColony.ActiveOrbitalBatteries;
                if (selectedColony.NetEnergy > 0)
                {
                    int possibleActivations = selectedColony.NetEnergy / selectedColony.OrbitalBatteryDesign.UnitEnergyCost;
                    if (possibleActivations > 0)
                    {
                        maxActiveOrbitalBatteries += possibleActivations;
                    }
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
            Colony selectedColony = Model.SelectedColony;
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
                {
                    Model.ShipyardBuildProjects = Enumerable.Empty<BuildProject>();
                }
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
            //_addOneMoreToShipyardBuildQueueCommand.RaiseCanExecuteChanged();
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
            //Model.AddOneMoreToShipyardBuildQueueCommand = _addOneMoreToShipyardBuildQueueCommand;
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

            _ = ClientEvents.TurnStarted.Subscribe(OnTurnStarted, ThreadOption.UIThread);
        }

        private void OnTurnStarted(GameContextEventArgs args)
        {
            Colony selectedColony = Model.SelectedColony;
            if (selectedColony == null)
            {
                Model.SelectedColony = AppContext.LocalPlayerEmpire.SeatOfGovernment;
            }

            Model.Colonies = AppContext.LocalPlayerEmpire.Colonies;
        }

        private bool CanExecuteCancelBuildProjectCommand(BuildProject project)
        {
            if (Model.SelectedColony == null)
            {
                return false;
            }

            if (project is ShipBuildProject)
            {
                return Model.SelectedColony.Shipyard != null;
            }

            return true;
        }

        private void ExecuteCancelBuildProjectCommand([NotNull] BuildProject project)
        {
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }

            IProductionCenter productionCenter = project.ProductionCenter;
            if (productionCenter == null)
            {
                return;
            }

            BuildSlot buildSlot = productionCenter.BuildSlots.FirstOrDefault(o => o.Project == project);
            if (buildSlot == null)
            {
                return;
            }

            if (project.IsPartiallyComplete || project.IsRushed)
            {
                MessageDialogResult confirmResult = MessageDialog.Show(
                    ResourceManager.GetString("CONFIRM_CANCEL_BUILD_HEADER"),
                    ResourceManager.GetString("CONFIRM_CANCEL_BUILD_MESSAGE"),
                    MessageDialogButtons.YesNo);

                if (confirmResult != MessageDialogResult.Yes)
                {
                    return;
                }
            }

            if (project.IsRushed)
            {
                CivilizationManager civMan = CivilizationManager.For(productionCenter.Owner);
                _ = civMan.Credits.AdjustCurrent(project.GetTotalCreditsCost());
            }

            project.Cancel();
            productionCenter.ProcessQueue();

            PlayerOrderService.AddOrder(new UpdateProductionOrder(productionCenter));

            UpdateBuildLists();
        }

        private bool CanExecuteBuyBuildProjectCommand(BuildProject project)
        {
            if (project == null)
            {
                return false;
            }

            if (project.IsCancelled)
            {
                //project.IsCancelled = false;
                //project.SetFlag(BuildProjectFlags.Cancelled);
                //
                // real  - var result = MessageDialog.Show("Unavailable for purchase - project has flag: IsCancelled", MessageDialogButtons.Ok);
                _ = MessageDialog.Show("Unavailable for purchase - sorry", MessageDialogButtons.Ok);
            }

            if (/*project.IsCancelled || */project.IsCompleted || project.IsRushed)
            {
                return false;
            }

            if (Model.SelectedColony == null)
            {
                return false;
            }

            CivilizationManager civMan = CivilizationManager.For(Model.SelectedColony.Owner);

            if (civMan.Credits.CurrentValue < project.GetTotalCreditsCost() * 5)  // 5 times expensive
            {
                int missingCredits = (5 * project.GetCurrentIndustryCost()) - civMan.Credits.CurrentValue;
                //int missingCredits = (5 * project.GetCurrentIndustryCost()) - project.IndustryInvested;
                string message = string.Format(ResourceManager.GetString("RUSH_BUILDING_INSUFFICIENT_CREDITS_MESSAGE"), missingCredits);

                //string message = string.Format(ResourceManager.GetString("RUSH_BUILDING_INSUFFICIENT_CREDITS_MESSAGE"));
                _ = MessageDialog.Show(ResourceManager.GetString("RUSH_BUILDING_INSUFFICIENT_CREDITS_HEADER"), message, MessageDialogButtons.Ok);
                _text = message
                    + " - project.GetCurrentIndustryCost() = " + project.GetCurrentIndustryCost()
                    + "; civMan.Credits.CurrentValue=" + civMan.Credits.CurrentValue
                    + " "
                    ;
                Console.WriteLine(_text);
                return false;
            }

            return true;
        }

        private void ExecuteBuyBuildProjectCommand([NotNull] BuildProject project)
        {
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }

            IProductionCenter productionCenter = project.ProductionCenter;
            if (productionCenter == null)
            {
                return;
            }

            BuildSlot buildSlot = productionCenter.BuildSlots.FirstOrDefault(o => o.Project == project);
            if (buildSlot == null)
            {
                return;
            }

            CivilizationManager civMan = CivilizationManager.For(Model.SelectedColony.Owner);

            string confirmationMessage = string.Format(ResourceManager.GetString("CONFIRM_RUSH_BUILDING_MESSAGE"),
                project.GetTotalCreditsCost(), civMan.Credits.CurrentValue);
            MessageDialogResult confirmResult = MessageDialog.Show(
                ResourceManager.GetString("CONFIRM_RUSH_BUILDING_HEADER"),
                confirmationMessage,
                MessageDialogButtons.YesNo);
            if (confirmResult != MessageDialogResult.Yes)
            {
                return;
            }

            // Temporarily update the resources so the player can immediately see the results of his spending, else we would get updated values only at the next turn.
            _ = civMan.Credits.AdjustCurrent(-project.GetTotalCreditsCost());
            //_ = civMan.BuyCostLastTurn.AdjustCurrent(project.GetTotalCreditsCost());
            civMan.BuyCostLastTurn += project.GetTotalCreditsCost();

            project.IsRushed = true;
            PlayerOrderService.AddOrder(new RushProductionOrder(productionCenter));
        }

        private bool CanExecuteRemoveFromPlanetaryBuildQueueCommand(BuildQueueItem item)
        {
            return Model.SelectedColony != null;
        }

        private bool CanExecuteRemoveFromShipyardBuildQueueCommand(BuildQueueItem item)
        {
            return Model.SelectedColony != null; // && (Model.SelectedColony.Shipyard != null));
        }

        //private bool CanExecuteClearBuildSlotQueueCommand(BuildProject item)
        //{
        //    return ((Model.SelectedColony != null) && (Model.SelectedColony.Shipyard != null));
        //}

        private void ExecuteRemoveFromPlanetaryBuildQueueCommand(BuildQueueItem item)
        {
            Colony colony = Model.SelectedColony;
            if (colony == null)
            {
                return;
            }

            RemoveItemFromBuildQueue(item, colony);
        }

        private void ExecuteRemoveFromShipyardBuildQueueCommand(BuildQueueItem item)
        {
            Colony colony = Model.SelectedColony;
            if (colony == null)
            {
                return;
            }

            //if (colony.Shipyard == null)
            //    return;

            RemoveItemFromShipyardBuildQueue(item, colony.Shipyard);
        }

        private bool CanExecuteAddToShipyardBuildQueueCommand(BuildProject project)
        {
            return (Model.SelectedColony != null) && (Model.SelectedColony.Shipyard != null);
        }

        //private bool CanExecuteAddToBuildSlotQueueCommand(BuildProject project)
        //{
        //    return ((Model.SelectedColony != null) && (Model.SelectedColony.Shipyard != null));
        //}
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]

        private void ExecuteAddToShipyardBuildQueueCommand(BuildProject project)
        {
            Colony colony = Model.SelectedColony;
            if (colony == null)
            {
                return;
            }

            if (colony.Shipyard == null)
            {
                return;
            }

            AddProjectToBuildQueue(project, colony.Shipyard);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void ExecuteAddOneMoreToShipyardBuildQueueCommand(BuildProject project)
        {
            Colony colony = Model.SelectedColony;
            if (colony == null)
            {
                return;
            }

            if (colony.Shipyard == null)
            {
                return;
            }

            AddProjectToBuildQueue(project, colony.Shipyard);
        }

        protected void RemoveItemFromBuildQueue([NotNull] BuildQueueItem item, [NotNull] IProductionCenter productionCenter)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (productionCenter == null)
            {
                throw new ArgumentNullException("productionCenter");
            }

            if ((item.Count <= 1) || !item.DecrementCount())
            {
                _ = productionCenter.BuildQueue.Remove(item);
            }

            PlayerOrderService.AddOrder(new UpdateProductionOrder(productionCenter));

            UpdateBuildLists();
        }

        protected void RemoveItemFromShipyardBuildQueue([NotNull] BuildQueueItem item, [NotNull] IProductionCenter productionCenter)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (productionCenter == null)
            {
                throw new ArgumentNullException("productionCenter");
            }

            if ((item.Count <= 1) || !item.DecrementCount())
            {
                _ = productionCenter.BuildQueue.Remove(item);
            }

            PlayerOrderService.AddOrder(new UpdateProductionOrder(productionCenter));

            UpdateBuildLists();
        }

        protected void AddProjectToBuildQueue([NotNull] BuildProject project, [NotNull] IProductionCenter productionCenter)
        {
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }

            if (productionCenter == null)
            {
                throw new ArgumentNullException("productionCenter");
            }

            bool newItemAdded = true;
            BuildQueueItem lastItemInQueue = productionCenter.BuildQueue.LastOrDefault();

            if ((lastItemInQueue != null) && project.IsEquivalent(lastItemInQueue.Project))
            {
                if (lastItemInQueue.IncrementCount())
                {
                    newItemAdded = false;
                }
            }

            if (newItemAdded)
            {
                productionCenter.BuildQueue.Add(new BuildQueueItem(project));
                productionCenter.ProcessQueue();
            }

            PlayerOrderService.AddOrder(new UpdateProductionOrder(productionCenter));

            if (productionCenter is Colony)
            {
                Model.SelectedPlanetaryBuildProject = null;
            }
            else if (productionCenter is Shipyard)
            {
                Model.SelectedShipyardBuildProject = null;
            }

            UpdateBuildLists();
        }
        protected void AddProjectToBuildSlotQueue([NotNull] BuildProject project, [NotNull] Shipyard shipyard)
        {
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }

            if (shipyard == null)
            {
                throw new ArgumentNullException("buildSlot");
            }

            bool newItemAdded = true;
            BuildQueueItem lastItemInQueue = shipyard.BuildQueue.LastOrDefault();

            if ((lastItemInQueue != null) && project.IsEquivalent(lastItemInQueue.Project))
            {
                if (lastItemInQueue.IncrementCount())
                {
                    newItemAdded = false;
                }
            }

            if (newItemAdded)
            {
                shipyard.BuildQueue.Add(new BuildQueueItem(project));
                shipyard.ProcessQueue();
            }

            PlayerOrderService.AddOrder(new UpdateProductionOrder(shipyard));

            //if (productionCenter is Colony)
            //    Model.SelectedPlanetaryBuildProject = null;
            //else if (productionCenter is Shipyard)
            Model.SelectedShipyardBuildProject = null;

            UpdateBuildLists();
        }
        protected override void UnregisterCommandAndEventHandlers()
        {
            base.UnregisterCommandAndEventHandlers();

            Model.AddToPlanetaryBuildQueueCommand = null;
            Model.AddToShipyardBuildQueueCommand = null;
            //Model.AddOneMoreToShipyardBuildQueueCommand = null;

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
            return Model.SelectedColony != null;
        }

        private void ExecuteAddToPlanetaryBuildQueueCommand([NotNull] BuildProject project)
        {
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }

            Colony colony = Model.SelectedColony;
            if (colony == null)
            {
                return;
            }

            AddProjectToBuildQueue(project, colony);
        }

        #endregion

        #region Overrides of GameScreenPresenterBase<ColonyScreenPresentationModel,IColonyScreenView>

        protected override string ViewName => StandardGameScreens.ColonyScreen;

        #endregion
    }
}