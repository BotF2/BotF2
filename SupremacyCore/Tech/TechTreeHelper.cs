// TechTreeHelper.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Microsoft.Practices.ServiceLocation;
using Supremacy.Annotations;
using Supremacy.Buildings;
using Supremacy.Client;
using Supremacy.Collections;
using Supremacy.Diplomacy;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Resources;
using Supremacy.Scripting;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable PossibleMultipleEnumeration

namespace Supremacy.Tech
{
    /// <summary>
    /// Helper class for performing operations and analysis on tech trees.
    /// </summary>
    public static class TechTreeHelper
    {
        private static IClientContext _clientContext;
        private static readonly BuildRestriction[] _planetTypeRestrictions;
        private static readonly Dictionary<BuildRestriction, PlanetType> _planetRestrictionTypeMap;

        static TechTreeHelper()
        {
            _planetTypeRestrictions = new[]
                                       {
                                           BuildRestriction.ArcticPlanet, BuildRestriction.BarrenPlanet,
                                           BuildRestriction.CrystallinePlanet, BuildRestriction.DemonPlanet,
                                           BuildRestriction.DesertPlanet, BuildRestriction.JunglePlanet,
                                           BuildRestriction.OceanicPlanet, BuildRestriction.RoguePlanet,
                                           BuildRestriction.TerranPlanet, BuildRestriction.VolcanicPlanet,
                                           BuildRestriction.GasGiant
                                       };

            _planetRestrictionTypeMap = new Dictionary<BuildRestriction, PlanetType>
                                         {
                                             { BuildRestriction.ArcticPlanet, PlanetType.Arctic },
                                             { BuildRestriction.BarrenPlanet, PlanetType.Barren },
                                             { BuildRestriction.CrystallinePlanet, PlanetType.Crystalline },
                                             { BuildRestriction.DemonPlanet, PlanetType.Demon },
                                             { BuildRestriction.DesertPlanet, PlanetType.Desert },
                                             { BuildRestriction.GasGiant, PlanetType.GasGiant },
                                             { BuildRestriction.JunglePlanet, PlanetType.Jungle },
                                             { BuildRestriction.OceanicPlanet, PlanetType.Oceanic },
                                             { BuildRestriction.RoguePlanet, PlanetType.Rogue },
                                             { BuildRestriction.TerranPlanet, PlanetType.Terran },
                                             { BuildRestriction.VolcanicPlanet, PlanetType.Volcanic }
                                         };
        }

        private static IClientContext ClientContext
        {
            get
            {
                if (_clientContext == null)
                    _clientContext = ServiceLocator.Current.GetInstance<IClientContext>();
                return _clientContext;
            }
        }

        /// <summary>
        /// Gets all of the tech designs of the specified design from the tech tree
        /// of the specified <see cref="CivilizationManager"/>.
        /// </summary>
        /// <param name="civManager">The civilization's manager.</param>
        /// <param name="type">The type of designs to get.</param>
        /// <param name="researchedOnly">Whether to include only designs that have been researched.</param>
        /// <returns>The tech designs.</returns>
        public static ICollection<TechObjectDesign> GetTechDatabaseDesigns(
            CivilizationManager civManager,
            TechObjectType type,
            bool researchedOnly)
        {
            if (civManager == null)
                civManager = ClientContext.LocalPlayerEmpire;

            var techTree = civManager.TechTree;
            var results = new List<TechObjectDesign>();
            
            switch (type)
            {
                case TechObjectType.Batteries:
                    results.AddRange(
                        techTree.OrbitalBatteryDesigns
                            .Where(o => !researchedOnly || MeetsTechLevels(civManager, o)));
                    break;

                case TechObjectType.Buildings:
                    results.AddRange(
                        techTree.BuildingDesigns
                            .Where(o => !researchedOnly || MeetsTechLevels(civManager, o)));
                    break;

                case TechObjectType.Facilities:
                    results.AddRange(
                        techTree.ProductionFacilityDesigns
                            .Where(o => !researchedOnly || MeetsTechLevels(civManager, o)));
                    break;

                case TechObjectType.Ships:
                    results.AddRange(
                        techTree.ShipDesigns
                            .Where(o => !researchedOnly || MeetsTechLevels(civManager, o)));
                    break;

                case TechObjectType.Shipyards:
                    results.AddRange(
                        techTree.ShipyardDesigns
                            .Where(o => !researchedOnly || MeetsTechLevels(civManager, o)));
                    break;

                case TechObjectType.Stations:
                    results.AddRange(
                        techTree.StationDesigns
                            .Where(o => !researchedOnly || MeetsTechLevels(civManager, o)));
                    break;
            }

            return results;
        }

        /// <summary>
        /// Gets the tech designs in a civilization's tech tree that are available at the 
        /// civilization's current tech levels.
        /// </summary>
        /// <param name="civilization">The civilization.</param>
        /// <returns>The tech designs.</returns>
        public static ICollection<TechObjectDesign> GetDesignsForCurrentTechLevels(Civilization civilization)
        {
            if (civilization == null)
                throw new ArgumentNullException("civilization");

            var results = new HashSet<TechObjectDesign>();
            var civManager = GameContext.Current.CivilizationManagers[civilization];

            if (civManager != null)
                results.UnionWith(civManager.TechTree.Where(o => MeetsTechLevels(civManager, o)));

            return results;
        }

        /// <summary>
        /// Gets the potential shipbuilding projects for the specified shipyard.
        /// </summary>
        /// <param name="shipyard">The shipyard.</param>
        /// <returns>The shipbuilding projects.</returns>
        public static IList<BuildProject> GetShipyardBuildProjects(Shipyard shipyard)
        {
            var civManager = GameContext.Current.CivilizationManagers[shipyard.OwnerID];
            var shipDesigns = new List<ShipDesign>();
            var shipyardDesign = shipyard.ShipyardDesign;
            var unavailableShipDesigns = new List<ShipDesign>();

            /* 
             * Find all ship designs whose tech requirements have been met.
             */
            shipDesigns.AddRange(
                civManager.TechTree.ShipDesigns.Where(
                    o => MeetsTechLevels(civManager, o)));

            /*
             * Mark all obsolete designs for removal.
             */
            unavailableShipDesigns.AddRange(
                shipDesigns.SelectMany(
                    o => o.ObsoletedDesigns.OfType<ShipDesign>()));

            /* 
             * Check for any designs that cannot be built because their tech levels exceed
             * the capabilities of the local shipyard and mark them for removal.  This check
             * was deferred to ensure that any other designs obsoleted by these designs were
             * properly marked for removal.
             */
            unavailableShipDesigns.AddRange(
                shipDesigns.Where(
                    o => !IsShipDesignWithinShipyardCapabilities(o, shipyardDesign)));

            /*
             * Construct the list of ShipBuildProjects from the remaining designs.
             */
            var colony = shipyard.Sector.System.Colony;
            return shipDesigns
                .Except(unavailableShipDesigns)
                .Where(o => MeetsPrerequisites(colony, o))
                .Select(o => new ShipBuildProject(shipyard, o))
                .Cast<BuildProject>()
                .ToList();
        }

        /// <summary>
        /// Gets the potential planetary build projects for the specified colony.
        /// </summary>
        /// <param name="colony">The colony.</param>
        /// <returns>The planetary build projects.</returns>
        public static IList<BuildProject> GetBuildProjects(Colony colony)
        {
            bool _tracingTechTreeHelper = false;   // turn true if you need

            var colonyOwner = colony.Owner;
            var civManager = GameContext.Current.CivilizationManagers[colonyOwner];
            var results = new List<BuildProject>();

            foreach (var productionCategory in EnumHelper.GetValues<ProductionCategory>())
            {
                var localFacilityType = colony.GetFacilityType(productionCategory);

                if (colony.GetTotalFacilities(productionCategory) > 0)
                {
                    var keepLooking = true;

                    var design = colony.GetFacilityType(productionCategory);
                    var baseDesign = new ProductionFacilityBuildProject(colony, design);

                    results.Add(baseDesign);

                    var facilityBuildProject = colony.BuildSlots[0].Project as ProductionFacilityBuildProject;

                    if (facilityBuildProject != null &&
                        facilityBuildProject.FacilityDesign.Category == productionCategory)
                    {
                        if (facilityBuildProject.FacilityDesign != baseDesign.FacilityDesign)
                            results.Remove(baseDesign);

                        keepLooking = false;
                    }

                    if (!keepLooking)
                        goto NextCategory;

                    var facilityCategory = productionCategory;

                    var designsInBuildQueue = colony.BuildQueue
                        .Select(o => o.Project)
                        .OfType<ProductionFacilityBuildProject>()
                        .Where(o => o.FacilityDesign.Category == facilityCategory);

                    if (designsInBuildQueue.Any(o => o.FacilityDesign == baseDesign.FacilityDesign))
                        goto NextCategory;

                    var conflictingDesignsInBuildQueue = designsInBuildQueue
                        .Where(o => o.FacilityDesign != baseDesign.FacilityDesign)
                        .Cast<BuildProject>()
                        .ToHashSet();

                    if (conflictingDesignsInBuildQueue.Any())
                    {
                        results.Remove(baseDesign);
                        goto NextCategory;
                    }

                    var upgrades = design.UpgradableDesigns
                        .OfType<ProductionFacilityDesign>()
                        .Intersect(GameContext.Current.TechTrees[colonyOwner].ProductionFacilityDesigns)
                        .Where(o => !colony.IsBuilding(o))
                        .ToList();

                    var bestUpgrade = GetBestFacilityDesign(
                        colony, 
                        design.Category,
                        upgrades);

                    if (bestUpgrade != null)
                        results.Add(new ProductionFacilityUpgradeProject(colony, bestUpgrade));

                NextCategory:
                    continue;
                }

                if (localFacilityType != null &&
                    civManager.TechTree.Contains(localFacilityType))
                {
                    results.Add(
                        new ProductionFacilityBuildProject(
                            colony,
                            localFacilityType));
                }
                else
                {
                    var design = GetBestFacilityDesign(colony, productionCategory);
                    if (design != null)
                        results.Add(new ProductionFacilityBuildProject(colony, design));
                }
            }

            // w00t
            if (colony.TotalOrbitalBatteries > 0)
            {
                var design = colony.OrbitalBatteryDesign;
                var baseDesign = new OrbitalBatteryBuildProject(colony, design);
                var keepGoing = true;

                results.Add(baseDesign);

                var batteryBuildProject = colony.BuildSlots[0].Project as OrbitalBatteryBuildProject;
                if (batteryBuildProject != null)
                {
                    if (batteryBuildProject.OrbitalBatteryDesign != baseDesign.OrbitalBatteryDesign)
                        results.Remove(baseDesign);

                    keepGoing = false;
                }

                if (!keepGoing)
                    goto DoneWithBatteries;

                var designsInBuildQueue = colony.BuildQueue
                    .Select(o => o.Project)
                    .OfType<OrbitalBatteryBuildProject>();

                if (designsInBuildQueue.Any())
                    goto DoneWithBatteries;

                var conflictingDesignsInBuildQueue = designsInBuildQueue
                    .Where(o => o.OrbitalBatteryDesign != baseDesign.OrbitalBatteryDesign)
                    .Cast<BuildProject>()
                    .ToHashSet();

                if (conflictingDesignsInBuildQueue.Any())
                {
                    results.Remove(baseDesign);
                    goto DoneWithBatteries;
                }

                var upgrades = design.UpgradableDesigns
                    .OfType<OrbitalBatteryDesign>()
                    .Intersect(GameContext.Current.TechTrees[colonyOwner].OrbitalBatteryDesigns)
                    .Where(o => !colony.IsBuilding(o) && MeetsTechLevels(civManager, o))
                    .ToList();

                results.AddRange(
                    upgrades.Select(
                        upgrade => new OrbitalBatteryUpgradeProject(colony, upgrade)));

            DoneWithBatteries:
                ;
            }
            else
            {
                var currentBuild = civManager.TechTree.OrbitalBatteryDesigns
                    .Where(colony.IsBuilding)
                    .FirstOrDefault();

                if (currentBuild != null)
                {
                    if (!IsOrbitalBatteryObsolete(colony, currentBuild) &&
                        MeetsTechLevels(civManager, currentBuild) &&
                        MeetsPrerequisites(colony, currentBuild))
                    {
                        results.Add(new OrbitalBatteryBuildProject(colony, currentBuild));
                    }
                }
                else
                {
                    var buildableBatteries =
                        (from design in civManager.TechTree.OrbitalBatteryDesigns
                         where !IsOrbitalBatteryObsolete(colony, design) &&
                               MeetsTechLevels(civManager, design) &&
                               MeetsPrerequisites(colony, design)
                         select design)
                            .ToHashSet();

                    buildableBatteries.ExceptWith(results.Select(o => o.BuildDesign).OfType<OrbitalBatteryDesign>());

                    results.AddRange(buildableBatteries.Select(o => new OrbitalBatteryBuildProject(colony, o)));
                }
            }

            results.AddRange(
                colony.Buildings
                    .SelectMany(
                        b => b.BuildingDesign.UpgradableDesigns
                                 .Where(ud => civManager.TechTree.Contains(ud))
                                 .OfType<BuildingDesign>()
                                 .Select(
                                     ud => new
                                           {
                                               BaseDesign = b,
                                               UpgradeDesign = ud
                                           }))
                    .Where(
                        o => !colony.HasBuilding(o.UpgradeDesign) &&
                             !colony.IsBuilding(o.UpgradeDesign) &&
                             MeetsTechLevels(civManager, o.UpgradeDesign) &&
                             MeetsRestrictions(colony, o.UpgradeDesign) &&
                             MeetsPrerequisites(colony, o.UpgradeDesign))
                    .Select(
                        o => new StructureUpgradeProject(
                                 o.BaseDesign,
                                 o.UpgradeDesign)));

            var originalOwner = colony.OriginalOwner;
            var buildingDesigns = civManager.TechTree.BuildingDesigns.AsEnumerable();

            if (originalOwner != colonyOwner &&
                DiplomacyHelper.IsMember(originalOwner, colonyOwner))
            {
                var minorRaceTechTree = GameContext.Current.TechTrees[originalOwner];
                if (minorRaceTechTree != null)
                    buildingDesigns = buildingDesigns.Union(minorRaceTechTree.BuildingDesigns);
            }

            var buildableStructures =
                (from design in buildingDesigns
                 where MeetsTechLevels(civManager, design) &&
                       MeetsRestrictions(colony, design) &&
                       !IsBuildingObsolete(colony, design) &&
                       MeetsPrerequisites(colony, design)
                 select design).ToHashSet();
            //GameLog.Client.GameData.DebugFormat("TechHelpercs: colony, buildingDesigns: {0} - {1}", colony, buildingDesigns.Union(GameContext.Current.TechTrees[originalOwner].BuildingDesigns));

            buildableStructures.ExceptWith(results.Select(o => o.BuildDesign).OfType<BuildingDesign>());

            results.AddRange(buildableStructures.Select(o => new StructureBuildProject(colony, o)));

            if (CanBuildShipyard(colony))
            {
                var buildableShipyards =
                    (from design in civManager.TechTree.ShipyardDesigns
                     where CanBuildShipyard(colony) &&
                           MeetsTechLevels(civManager, design) &&
                           MeetsRestrictions(colony, design) &&
                           !IsShipyardObsolete(colony, design) &&
                           MeetsPrerequisites(colony, design)
                     select design).ToHashSet();

                buildableShipyards.ExceptWith(results.Select(o => o.BuildDesign).OfType<ShipyardDesign>());

                results.AddRange(buildableShipyards.Select(o => new StructureBuildProject(colony, o)));
            }

            if (colony.Shipyard != null)
            {
                if (_tracingTechTreeHelper)
                    GameLog.Core.General.DebugFormat("shipyard = {0}, first UpgradableDesigns = {1}, Buildqueue has {2} orders", colony.Shipyard.Name, colony.Shipyard.ShipyardDesign.UpgradableDesigns.First().Name, colony.Shipyard.BuildQueue.Count);

                results.AddRange(
                    colony.Shipyard.ShipyardDesign.UpgradableDesigns
                                     .Where(ud => civManager.TechTree.Contains(ud))
                                     .OfType<ShipyardDesign>()
                                     .Select(
                                         ud => new
                                               {
                                                   UpgradeTarget = colony.Shipyard,
                                                   UpgradeDesign = ud
                                               })
                        .Where(
                            o => !colony.HasShipyard(o.UpgradeDesign) &&
                                 !colony.IsBuilding(o.UpgradeDesign) &&
                                 MeetsTechLevels(civManager, o.UpgradeDesign) &&
                                 MeetsRestrictions(colony, o.UpgradeDesign) &&
                                 MeetsPrerequisites(colony, o.UpgradeDesign))
                        .Select(
                            o => new ShipyardUpgradeProject(
                                     o.UpgradeTarget,
                                     o.UpgradeDesign)));
            }

            results.SortInPlace(CompareBuildProjects);

            return results;
        }

        /// <summary>
        /// Determines whether a new shipyard can be constructed at the specified colony.
        /// </summary>
        /// <param name="colony">The colony.</param>
        /// <returns>
        /// <c>true</c> if a new shipyard can be constructed; otherwise, <c>false</c>.
        /// </returns>
        private static bool CanBuildShipyard([NotNull] Colony colony)
        {
            if (colony == null)
                throw new ArgumentNullException("colony");

            if (colony.Shipyard != null)
                return false;

            return !colony.BuildSlots.Any(o => o.HasProject && o.Project.BuildDesign is ShipyardDesign) &&
                   !colony.BuildQueue.Any(o => o.Project.BuildDesign is ShipyardDesign);
        }

        /// <summary>
        /// Determines whether the specified building design is obsolete at a given colony.
        /// </summary>
        /// <param name="colony">The colony.</param>
        /// <param name="design">The building design.</param>
        /// <returns>
        /// <c>true</c> if obsolete; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsBuildingObsolete(
            Colony colony,
            /* ReSharper disable SuggestBaseTypeForParameter */
            BuildingDesign design
            /* ReSharper restore SuggestBaseTypeForParameter */)
        {
            var civManager =  GameContext.Current.CivilizationManagers[colony.OwnerID];

            return civManager.TechTree.BuildingDesigns.Any(
                otherDesign => colony.HasBuilding(otherDesign) &&
                               MeetsTechLevels(civManager, otherDesign) &&
                               GetObsoletedTree(civManager, otherDesign).Contains(design));
        }

        /// <summary>
        /// Determines whether the specified shipyard design is obsolete at a given colony.
        /// </summary>
        /// <param name="colony">The colony.</param>
        /// <param name="design">The shipyard design.</param>
        /// <returns>
        /// <c>true</c> if obsolete; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsShipyardObsolete(
            Colony colony,
            /* ReSharper disable SuggestBaseTypeForParameter */
            ShipyardDesign design
            /* ReSharper restore SuggestBaseTypeForParameter */)
        {
            var civManager =  GameContext.Current.CivilizationManagers[colony.OwnerID];

            return civManager.TechTree.ShipyardDesigns.Any(
                otherDesign => colony.HasShipyard(otherDesign) &&
                               MeetsTechLevels(civManager, otherDesign) &&
                               GetObsoletedTree(civManager, otherDesign).Contains(design));
        }

        /// <summary>
        /// Determines whether the specified orbital battery design is obsolete at a given colony.
        /// </summary>
        /// <param name="colony">The colony.</param>
        /// <param name="design">The building design.</param>
        /// <returns>
        /// <c>true</c> if obsolete; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsOrbitalBatteryObsolete(
            Colony colony,
            /* ReSharper disable SuggestBaseTypeForParameter */
            OrbitalBatteryDesign design
            /* ReSharper restore SuggestBaseTypeForParameter */)
        {
            var civManager = GameContext.Current.CivilizationManagers[colony.OwnerID];

            if (colony.OrbitalBatteryDesign == null)
                return false;

            if (GetObsoletedTree(civManager, colony.OrbitalBatteryDesign).Contains(design))
                return true;

            return false;
        }

        /// <summary>
        /// Determines whether the specified facility design is obsolete at a given colony.
        /// </summary>
        /// <param name="colony">The colony.</param>
        /// <param name="design">The facility design.</param>
        /// <returns>
        /// <c>true</c> if obsolete; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsFacilityObsolete(Colony colony, ProductionFacilityDesign design)
        {
            var civManager = GameContext.Current.CivilizationManagers[colony.OwnerID];

            var localDesign = colony.GetFacilityType(design.Category);
            if (localDesign == null)
                return false;

            if (GetObsoletedTree(civManager, localDesign).Contains(design))
                return true;

            return false;
        }

        /// <summary>
        /// Gets all of the tech designs in the tech tree of the given civilization
        /// manager that are rendered obsolete by the specified design.
        /// </summary>
        /// <param name="civManager">The civilization manager.</param>
        /// <param name="design">The design.</param>
        /// <returns>The obsoleted designs.</returns>
        private static ICollection<TechObjectDesign> GetObsoletedTree(
            CivilizationManager civManager,
            TechObjectDesign design)
        {
            var results = new HashSet<TechObjectDesign>();
            var techTree = civManager.TechTree;

            foreach (var obsoleteDesign in design.ObsoletedDesigns.Where(techTree.Contains))
            {
                results.Add(obsoleteDesign);
                results.UnionWith(GetObsoletedTree(civManager, obsoleteDesign));
            }

            return results;
        }

        /// <summary>
        /// A <see cref="Comparison&lt;BuildProject&gt;"/> designed to present
        /// <see cref="BuildProject"/>s in the desired order in the client UI.
        /// </summary>
        /// <param name="a">First operand.</param>
        /// <param name="b">Second operand.</param>
        /// <returns>The result of the comparison.</returns>
        private static int CompareBuildProjects(BuildProject a, BuildProject b)
        {
            if (a == null)
            {
                if (b == null)
                    return 0;
                return -1;
            }
            if (b == null)
            {
                return 1;
            }

            bool isFacilityA = (a is ProductionFacilityBuildProject);
            bool isFacilityB = (b is ProductionFacilityBuildProject);

            bool isUpgradeA = a.IsUpgrade;
            bool isUpgradeB = b.IsUpgrade;

            if (isUpgradeA)
            {
                if (isUpgradeB)
                {
                    if (isFacilityA)
                    {
                        if (isFacilityB)
                        {
                            return ((int)((ProductionFacilityBuildProject)a).FacilityDesign.Category).CompareTo(
                                    (int)((ProductionFacilityBuildProject)b).FacilityDesign.Category);
                        }
                        return -1;
                    }

                    if (isFacilityB)
                        return 1;

                    return StringComparer.CurrentCulture.Compare(
                        ResourceManager.GetString(a.BuildDesign.Name),
                        ResourceManager.GetString(b.BuildDesign.Name));
                }
                return -1;
            }

            if (isUpgradeB)
                return 1;

            if (isFacilityA)
            {
                if (isFacilityB)
                {
                    return ((int)((ProductionFacilityBuildProject)a).FacilityDesign.Category).CompareTo(
                            (int)((ProductionFacilityBuildProject)b).FacilityDesign.Category);
                }
                return -1;
            }

            if (isFacilityB)
                return 1;

            return string.Compare(a.BuildDesign.Name, b.BuildDesign.Name, StringComparison.Ordinal);
        }

        /// <summary>
        /// Determines whether the tech levels of the given civilization manager are sufficient
        /// to unlock the given design.
        /// </summary>
        /// <param name="civManager">The civilization manager.</param>
        /// <param name="design">The design.</param>
        /// <returns><c>true</c> if tech levels are sufficient; otherwise, <c>false</c>.</returns>
        internal static bool MeetsTechLevels(
            [NotNull] CivilizationManager civManager,
            [NotNull] TechObjectDesign design)
        {
            if (civManager == null)
                throw new ArgumentNullException("civManager");
            if (design == null)
                throw new ArgumentNullException("design");

            return EnumHelper.GetValues<TechCategory>().All(
                techCategory => design.TechRequirements[techCategory] <=
                                civManager.Research.GetTechLevel(techCategory));
        }

        /// <summary>
        /// Determines whether the specified ship design can be built at a given shipyard.
        /// </summary>
        /// <param name="shipDesign">The ship design.</param>
        /// <param name="shipyardDesign">The design of the shipyard.</param>
        /// <returns>
        /// <c>true</c> if the ship can be built at the shipyard; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsShipDesignWithinShipyardCapabilities(
            // ReSharper disable SuggestBaseTypeForParameter
            [NotNull] ShipDesign shipDesign,
            // ReSharper restore SuggestBaseTypeForParameter
            [NotNull] ShipyardDesign shipyardDesign)
        {
            if (shipDesign == null)
                throw new ArgumentNullException("shipDesign");
            if (shipyardDesign == null)
                throw new ArgumentNullException("shipyardDesign");

            return EnumHelper.GetValues<TechCategory>().All(
                techCategory => shipDesign.TechRequirements[techCategory] <=
                                shipyardDesign.MaxBuildTechLevel);
        }

        /// <summary>
        /// Gets the best facility design in the specified category that can be built at the given colony.
        /// </summary>
        /// <param name="colony">The colony.</param>
        /// <param name="category">The production category.</param>
        /// <returns>The facility design.</returns>
        public static ProductionFacilityDesign GetBestFacilityDesign(
            [NotNull] Colony colony, 
            ProductionCategory category)
        {
            if (colony == null)
                throw new ArgumentNullException("colony");

            return GetBestFacilityDesign(
                colony, 
                category,
                GameContext.Current.TechTrees[colony.OwnerID].ProductionFacilityDesigns);
        }

        /// <summary>
        /// Gets the best facility design in the specified category that can be built at the given colony.
        /// </summary>
        /// <param name="colony">The colony.</param>
        /// <param name="category">The production category.</param>
        /// <param name="availableDesigns">The search space.</param>
        /// <returns>The facility design.</returns>
        public static ProductionFacilityDesign GetBestFacilityDesign(
            [NotNull] Colony colony,
            ProductionCategory category,
            [NotNull] IEnumerable<ProductionFacilityDesign> availableDesigns)
        {
            if (colony == null)
                throw new ArgumentNullException("colony");

            if (availableDesigns == null)
                throw new ArgumentNullException("availableDesigns");

            if (!availableDesigns.Any())
                return null;

            var civManager = GetCivManager(colony);
            var removedDesigns = new HashSet<TechObjectDesign>();

            var designs = availableDesigns
                .Where(
                    design => design != null &&
                              design.Category == category)
                .Where(
                    design => MeetsTechLevels(civManager, design) &&
                              MeetsPrerequisites(colony, design) &&
                              !IsFacilityObsolete(colony, design)).ToList();

            foreach (var design in designs)
                removedDesigns.UnionWith(GetObsoletedTree(civManager, design));

            designs.RemoveAll(removedDesigns.Contains);

            if (designs.Count == 0)
                return null;

            return designs.MaxBy(d => d.UnitOutput);
        }

        /// <summary>
        /// Determines if the prerequisites of a given tech design are met at the specified colony.
        /// </summary>
        /// <param name="colony">The colony.</param>
        /// <param name="design">The design.</param>
        /// <returns><c>true</c> if the prerequisites are met; otherwise, <c>false</c>.</returns>
        internal static bool MeetsPrerequisites([NotNull] Colony colony, [NotNull] TechObjectDesign design)
        {
            if (colony == null)
                throw new ArgumentNullException("colony");

            if (design == null)
                throw new ArgumentNullException("design");

            return (
                       from prerequisiteGroup in design.Prerequisites
                       where prerequisiteGroup.Count != 0
                       select design is ShipDesign || design is ProductionFacilityDesign
                                  ? prerequisiteGroup.Where(o => o is BuildingDesign)
                                  : prerequisiteGroup
                       into prerequisites
                       where prerequisites.Any()
                       select prerequisites.Any(
                           prerequisite => TechObjectExists(
                               colony: colony,
                               design: prerequisite,
                               isActive: design is ShipDesign))
                   )
                .All(prerequisiteGroupSatisfied => prerequisiteGroupSatisfied);
        }

        /// <summary>
        /// Determines if the build restrictions of a given building design are met at the specified colony.
        /// </summary>
        /// <param name="colony">The colony.</param>
        /// <param name="design">The design.</param>
        /// <returns><c>true</c> if the build restrictions are met; otherwise, <c>false</c>.</returns>
        private static bool MeetsRestrictions([NotNull] Colony colony, [NotNull] PlanetaryTechObjectDesign design)
        {
            if (colony == null)
                throw new ArgumentNullException("colony");
            if (design == null)
                throw new ArgumentNullException("design");

            if (IsBuildLimitReached(colony, design, includeConstruction: true))
                return false;

            var buildCondition = design.BuildCondition;
            if (buildCondition != null)
            {
                //TODO
                return true;
            }

            var system = colony.System;

            //
            // SYSTEM RESOURCE BONUS RESTRICTIONS
            //

            if ((design.Restriction & BuildRestriction.DilithiumBonus) == BuildRestriction.DilithiumBonus)
            {
                if (!colony.System.HasDilithiumBonus)
                    return false;
            }
            if ((design.Restriction & BuildRestriction.RawMaterialsBonus) == BuildRestriction.RawMaterialsBonus)
            {
                if (!colony.System.HasRawMaterialsBonus)
                    return false;
            }

            if ((design.Restriction & BuildRestriction.Asteroids) == BuildRestriction.Asteroids)
            {
                if (!system.ContainsPlanetType(PlanetType.Asteroids))
                    return false;
            }

            //
            // PLANET TYPE RESTRICTIONS
            //

            if (!MeetsPlanetRestrictions(colony, design))
                return false;

            //
            // STAR TYPE RESTRICTIONS
            //

            if ((design.Restriction & BuildRestriction.BlueStar) == BuildRestriction.BlueStar)
            {
                if (system.StarType != StarType.Blue)
                    return false;
            }
            //if ((design.Restriction & BuildRestriction.GreenStar) == BuildRestriction.GreenStar)
            //{
            //    if (system.StarType != StarType.Green)
            //        return false;
            //}
            if ((design.Restriction & BuildRestriction.RedStar) == BuildRestriction.RedStar)
            {
                if (system.StarType != StarType.Red)
                    return false;
            }
            if ((design.Restriction & BuildRestriction.WhiteStar) == BuildRestriction.WhiteStar)
            {
                if (system.StarType != StarType.White)
                    return false;
            }
            if ((design.Restriction & BuildRestriction.YellowStar) == BuildRestriction.YellowStar)
            {
                if (system.StarType != StarType.Yellow)
                    return false;
            }
            if ((design.Restriction & BuildRestriction.Nebula) == BuildRestriction.Nebula)
            {
                if (system.StarType != StarType.Nebula)
                    return false;
            }
			
            //
            // POPULATION RESTRICTIONS
            //
            
            if ((design.Restriction & BuildRestriction.ConqueredSystem) == BuildRestriction.ConqueredSystem)
            {
                if (colony.OriginalOwner == colony.Owner || DiplomacyHelper.IsMember(colony.OriginalOwner, colony.Owner))
                    return false;
            }

            if ((design.Restriction & BuildRestriction.HomeSystem) == BuildRestriction.HomeSystem &&
                !Equals(colony.System, GameContext.Current.CivilizationManagers[colony.OwnerID].HomeSystem))
            {
                var colonyOwner = colony.Owner;
                var originalOwner = colony.OriginalOwner;

                if (colonyOwner == originalOwner ||
                    !DiplomacyHelper.IsMember(originalOwner, colonyOwner) ||
                    colony != GameContext.Current.Universe.HomeColonyLookup[originalOwner])
                {
                    return false;
                }
                
                var memberTechTree = GameContext.Current.TechTrees[originalOwner];
                if (!memberTechTree.BuildingDesigns.Contains(design))
                    return false;
            }
            
            if ((design.Restriction & BuildRestriction.NativeSystem) == BuildRestriction.NativeSystem)
            {
                if (colony.OriginalOwner != colony.Owner || colony.Inhabitants != colony.Owner.Race)
                    return false;
            }
            
            if ((design.Restriction & BuildRestriction.NonNativeSystem) == BuildRestriction.NonNativeSystem)
            {
                //
                // TODO: Figure out exactly how this should work, particularly regarding civilizations whose primary
                // race and colonizing race are not the same (e.g. Founders and Jem'hadar for The Dominion).
                //

                if (colony.Inhabitants == colony.Owner.Race)
                    return false;

                var colonyOwner = colony.Owner;
                var originalOwner = colony.OriginalOwner;

                if (colonyOwner == originalOwner)
                    return false;
            }

            if ((design.Restriction & BuildRestriction.MemberSystem) == BuildRestriction.MemberSystem)
            {
                var colonyOwner = colony.Owner;
                var originalOwner = colony.OriginalOwner;

                if (!DiplomacyHelper.IsMember(originalOwner, colonyOwner))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the build limit has been met for a specific building design.
        /// </summary>
        /// <param name="colony">The colony where the design would be constructed.</param>
        /// <param name="design">The design.</param>
        /// <returns>
        /// <c>true</c> if the build limit has been met; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsBuildLimitReached(Colony colony, TechObjectDesign design, bool includeConstruction = false)
        {
            if (colony == null)
                throw new ArgumentNullException("colony");

            var p = design as PlanetaryTechObjectDesign;
            if (p == null)
                return false;

            return IsBuildLimitReachedCore(colony, design, p.BuildLimit, p.BuildLimitScope, includeConstruction) ||
                   (p.HasRestriction(BuildRestriction.OnePerSystem) &&
                    IsBuildLimitReachedCore(colony, design, 1, BuildLimitScope.System, includeConstruction)) ||
                   (p.HasRestriction(BuildRestriction.OnePerEmpire) &&
                    IsBuildLimitReachedCore(colony, design, 1, BuildLimitScope.Civilization, includeConstruction)) ||
                   (p.HasRestriction(BuildRestriction.OnePer100MaxPopUnits) &&
                    IsBuildLimitReachedCore(colony, design, colony.Population.Maximum / 100, BuildLimitScope.System, includeConstruction));
        }

        private static bool IsBuildLimitReachedCore(Colony colony, TechObjectDesign design, int buildLimit, BuildLimitScope buildLimitScope, bool includeConstruction = false)
        {
            var gameContext = GameContext.Current;

            switch (buildLimitScope)
            {
                case BuildLimitScope.Civilization:
                    return CivilizationManager.For(colony.OwnerID).Colonies
                                              .Sum(o => GetTechObjectCount(o, design, includeConstruction)) >= buildLimit;

                case BuildLimitScope.Galaxy:
                    return gameContext.Universe
                                      .Find<Colony>(UniverseObjectType.Colony)
                                      .Sum(o => GetTechObjectCount(o, design, includeConstruction)) >= buildLimit;

                case BuildLimitScope.System:
                    return GetTechObjectCount(colony, design, includeConstruction) >= buildLimit;

                default:
                    return false;
            }
        }

        public static int GetTechObjectCount(Colony colony, TechObjectDesign design, bool includeConstruction = false, bool includeInactive = true)
        {
            var count = 0;

            if (includeConstruction)
            {
                if (design is ShipDesign)
                {
                    var shipyard = colony.Shipyard;
                    if (shipyard != null)
                    {
                        count += shipyard.BuildSlots.Count(o => o.HasProject && o.Project.BuildDesign == design);
                        count += shipyard.BuildQueue.Where(o => o.Project.BuildDesign == design).Sum(o => o.Count);
                    }
                }
                else
                {
                    count += colony.BuildSlots.Count(o => o.HasProject && o.Project.BuildDesign == design);
                    count += colony.BuildQueue.Where(o => o.Project.BuildDesign == design).Sum(o => o.Count);
                }
            }

            var buildingDesign = design as BuildingDesign;
            if (buildingDesign != null)
            {
                if (includeInactive)
                    count += colony.BuildingsInternal.Count(o => o.BuildingDesign == buildingDesign);
                else
                    count += colony.BuildingsInternal.Count(o => o.IsActive && o.BuildingDesign == buildingDesign);

                return count;
            }

            var shipyardDesign = design as ShipyardDesign;
            if (shipyardDesign != null && colony.HasShipyard(shipyardDesign))
                return ++count;

            var batteryDesign = design as OrbitalBatteryDesign;
            if (batteryDesign != null && colony.OrbitalBatteryDesign == batteryDesign)
            {
                if (includeInactive)
                    count += colony.TotalOrbitalBatteries;
                else
                    count += colony.ActiveOrbitalBatteries;

                return count;
            }

            var facilityDesign = design as ProductionFacilityDesign;
            if (facilityDesign != null && colony.GetFacilityType(facilityDesign.Category) == facilityDesign)
            {
                if (includeInactive)
                    count += colony.GetTotalFacilities(facilityDesign.Category);
                else
                    count += colony.GetActiveFacilities(facilityDesign.Category);

                return count;
            }

            var shipDesign = design as ShipDesign;
            if (shipDesign != null)
            {
                var ships = GameContext.Current.Universe.FindAt<Ship>(
                    colony.Location,
                    ship => ship.OwnerID == colony.OwnerID &&
                            ship.Design == design);

                return count + ships.Count;
            }

            return count;
        }

        private static bool MeetsPlanetRestrictions([NotNull] Colony colony, [NotNull] PlanetaryTechObjectDesign design)
        {
            if (colony == null)
                throw new ArgumentNullException("colony");
            if (design == null)
                throw new ArgumentNullException("design");

            if ((design.Restriction & BuildRestriction.Moons) == BuildRestriction.Moons && !colony.System.Planets.Any(o => o.Moons.Length > 0))
                return false;

            var required = _planetTypeRestrictions.Where(r => ((design.Restriction & r) == r)).ToList();
            if (required.Count == 0)
                return true;

            return required.Select(r => _planetRestrictionTypeMap[r]).Any(p => colony.System.ContainsPlanetType(p));
        }

        /// <summary>
        /// Determines whether an object of the specified design exists at a given colony, or,
        /// if the design has a 'One per Empire' restriction, whether it exists somewhere in
        /// the owner civilization's territory.
        /// </summary>
        /// <param name="colony">The colony.</param>
        /// <param name="design">The design.</param>
        /// <param name="isActive">Whether or not the object must be active.</param>
        /// <returns><c>true</c> if an object exists; otherwise, <c>false</c>.</returns>
        private static bool TechObjectExists([NotNull] Colony colony, [NotNull] TechObjectDesign design, bool isActive)
        {
            if (colony == null)
                throw new ArgumentNullException("colony");
            if (design == null)
                throw new ArgumentNullException("design");

            var facilityDesign = design as ProductionFacilityDesign;
            if (facilityDesign != null)
                return colony.GetFacilityType(facilityDesign.Category) == facilityDesign;

            var buildingDesign = design as BuildingDesign;
            if (buildingDesign != null)
            {
                if ((buildingDesign.Restriction & BuildRestriction.OnePerEmpire) == BuildRestriction.OnePerEmpire)
                {
                    var civManager = GameContext.Current.CivilizationManagers[colony.OwnerID];
                    if (civManager.Colonies.Any(c => c.HasBuilding(buildingDesign, isActive)))
                        return true;
                }
                return colony.HasBuilding(buildingDesign, isActive);
            }

            var shipyardDesign = design as ShipyardDesign;
            if (shipyardDesign != null)
            {
                if ((shipyardDesign.Restriction & BuildRestriction.OnePerEmpire) == BuildRestriction.OnePerEmpire)
                {
                    var civManager = GameContext.Current.CivilizationManagers[colony.OwnerID];
                    if (civManager.Colonies.Any(c => c.HasShipyard(shipyardDesign)))
                        return true;
                }
                return colony.HasShipyard(shipyardDesign);
            }

            return false;
        }

        /// <summary>
        /// Gets the <see cref="CivilizationManager"/> for the owner of a colony.
        /// </summary>
        /// <param name="ownedObject">The colony.</param>
        /// <returns>The <see cref="CivilizationManager"/>.</returns>
        private static CivilizationManager GetCivManager([NotNull] UniverseObject ownedObject)
        {
            if (ownedObject == null)
                throw new ArgumentNullException("ownedObject");
            return GameContext.Current.CivilizationManagers[ownedObject.OwnerID];
        }
    }
}

// ReSharper restore PossibleMultipleEnumeration

