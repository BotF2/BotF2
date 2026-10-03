// ShipBuildProject.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Resources;
using Supremacy.Utility;
using System;

namespace Supremacy.Economy
{
    /// <summary>
    /// Represents a shipbuilding project in the game.
    /// </summary>
    [Serializable]
    public class ShipBuildProject : BuildProject
    {
        private readonly int _shipyardId;

        /// <summary>
        /// Gets the description of the ship under construction.
        /// </summary>
        /// <value>The description.</value>
        public override string Description => ResourceManager.GetString(BuildDesign.Name);

        /// <summary>
        /// Gets the dilithium needed.
        /// </summary>
        /// <value>The dilithium needed.</value>
        public int DilithiumNeeded => ResourcesRequired[ResourceType.Dilithium];

        /// <summary>
        /// Gets the dilithium used.
        /// </summary>
        /// <value>The dilithium used.</value>
        public int DilithiumUsed => ResourcesInvested[ResourceType.Dilithium];

        /// <summary>
        /// Gets the deuterium needed.
        /// </summary>
        /// <value>The deuterium needed.</value>
        public int DeuteriumNeeded => ResourcesRequired[ResourceType.Deuterium];

        /// <summary>
        /// Gets the deuterium used.
        /// </summary>
        /// <value>The deuterium used.</value>
        public int DeuteriumUsed => ResourcesInvested[ResourceType.Deuterium];

        /// <summary>
        /// Gets the DURANIUM needed.
        /// </summary>
        /// <value>The DURANIUM needed.</value>
        public int DuraniumNeeded => ResourcesRequired[ResourceType.Duranium];

        /// <summary>
        /// Gets the DURANIUM used.
        /// </summary>
        /// <value>The DURANIUM used.</value>
        public int DuraniumUsed => ResourcesInvested[ResourceType.Duranium];

        public int IndustryCapacity => Shipyard.GetBuildOutput(0);

        public bool HasDuraniumShortage
        {
            get => GetFlag(BuildProjectFlags.DuraniumShortage);
            protected set => SetFlag(BuildProjectFlags.DuraniumShortage, value);
        }

        public bool HasDeuteriumShortage
        {
            get => GetFlag(BuildProjectFlags.DeuteriumShortage);
            protected set => SetFlag(BuildProjectFlags.DeuteriumShortage, value);
        }

        public bool HasDilithiumShortage
        {
            get => GetFlag(BuildProjectFlags.DilithiumShortage);
            protected set => SetFlag(BuildProjectFlags.DilithiumShortage, value);
        }

        /// <summary>
        /// Gets the shipyard at which this <see cref="ShipBuildProject"/> is under construction.
        /// </summary>
        /// <value>The shipyard.</value>
        public Shipyard Shipyard => GameContext.Current.Universe.Objects[_shipyardId] as Shipyard;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShipBuildProject"/> class.
        /// </summary>
        /// <param name="shipyard">The shipyard.</param>
        /// <param name="design">The ship design.</param>
        public ShipBuildProject(Shipyard shipyard, ShipDesign design)
            : base(shipyard.Owner, shipyard, design)
        {
            _shipyardId = shipyard.ObjectID;
        }

        /// <summary>
        /// Gets the amount of industry available for investment during the current turn.
        /// </summary>
        /// <returns>The industry available.</returns>
        protected override int GetIndustryAvailable()
        {
            return Shipyard.GetBuildOutput(0);
        }

        /// <summary>
        /// Creates an equivalent clone of this <see cref="ShipBuildProject"/>.
        /// </summary>
        /// <returns>The clone.</returns>
        public override BuildProject CloneEquivalent()
        {
            return new ShipBuildProject(Shipyard, BuildDesign as ShipDesign);
        }
    }

    /// <summary>
    /// Represents a ship upgrade project in the game.
    /// </summary>
    [Serializable]
    public class ShipUpgradeProject : ShipBuildProject
    {
        private readonly int _upgradeTargetId;

        //private bool _shipUpgradeProjectTracing = true;

        /// <summary>
        /// Gets a value indicating whether this <see cref="ShipUpgradeProject"/> is an upgrade project.
        /// </summary>
        /// <value>
        /// <c>true</c>.
        /// </value>
        public override bool IsUpgrade => true;

        /// <summary>
        /// Gets the description of the ship under construction.
        /// </summary>
        /// <value>The description.</value>
        public override string Description => string.Format(
                    ResourceManager.GetString("SHIP_UPGRADING_FORMAT"),
                    ResourceManager.GetString(BuildDesign.Name));

        /// <summary>
        /// Gets the ship being upgraded.
        /// </summary>
        /// <value>The ship being upgraded.</value>
        public Ship UpgradeTarget => GameContext.Current.Universe.Objects[_upgradeTargetId] as Ship;

        /// <summary>
        /// Finishes this <see cref="ShipUpgradeProject"/> and creates the newly constructed item.
        /// </summary>
        public override void Finish()
        {
            GameLog.Core.ShipProduction.DebugFormat("ShipBuildProject - Finish = {0}, {1}", UpgradeTarget.ShipDesign.Name, BuildDesign.Name);
            base.Finish();
            UpgradeTarget.ShipDesign = BuildDesign as ShipDesign;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShipUpgradeProject"/> class.
        /// </summary>
        /// <param name="shipyard">The shipyard.</param>
        /// <param name="upgradeTarget">The ship being upgraded.</param>
        /// <param name="design">The target upgrade design.</param>
        public ShipUpgradeProject(Shipyard shipyard, Ship upgradeTarget, ShipDesign design)
            : base(shipyard, design)
        {
            _upgradeTargetId = upgradeTarget.ObjectID;
        }

        /// <summary>
        /// Creates an equivalent clone of this <see cref="ShipBuildProject"/>.
        /// </summary>
        /// <returns>The clone.</returns>
        public override BuildProject CloneEquivalent()
        {
            return null;
        }
    }

    /// <summary>
    /// Represents a ship repair project in the game.
    /// </summary>
    [Serializable]
    public class ShipRepairProject : ShipBuildProject
    {
//#pragma warning disable IDE0044 // Add readonly modifier
        private int _repairTargetId;
        private int _laborRequired;
        private ResourceValueCollection _resourcesRequired;
//#pragma warning restore IDE0044 // Add readonly modifier

        /// <summary>
        /// Gets the description of the ship being repaired.
        /// </summary>
        /// <value>The description.</value>
        public override string Description => string.Format(ResourceManager.GetString("SHIP_REPAIRING_FORMAT"),
                    ResourceManager.GetString(BuildDesign.Name));

        /// <summary>
        /// Gets the ship being repaired.
        /// </summary>
        /// <value>The ship being repaired.</value>
        public Ship RepairTarget => GameContext.Current.Universe.Objects[_repairTargetId] as Ship;

        /// <summary>
        /// Gets the total industry required to complete this <see cref="ShipRepairProject"/>.
        /// </summary>
        /// <value>The industry required.</value>
        protected override int IndustryRequired => _laborRequired;

        /// <summary>
        /// Gets the total resources required to complete this <see cref="ShipRepairProject"/>.
        /// </summary>
        /// <value>The resources required.</value>
        protected override ResourceValueCollection ResourcesRequired => _resourcesRequired;

        /// <summary>
        /// Finishes this <see cref="ShipRepairProject"/>.
        /// </summary>
        public override void Finish()
        {
            RepairTarget.HullStrength.ReplenishAndReset();
            RepairTarget.FuelReserve.ReplenishAndReset();
            RepairTarget.Crew.Reset(RepairTarget.ShipDesign.CrewSize);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShipRepairProject"/> class.
        /// </summary>
        /// <param name="shipyard">The shipyard.</param>
        /// <param name="repairTarget">The ship to be repaired.</param>
        public ShipRepairProject(Shipyard shipyard, Ship repairTarget)
            : base(shipyard, repairTarget.ShipDesign)
        {
            _repairTargetId = repairTarget.ObjectID;
            _laborRequired = (repairTarget.HullStrength.Maximum
                - repairTarget.HullStrength.CurrentValue) * 2;
            _resourcesRequired = new ResourceValueCollection();
            if (repairTarget.FuelReserve.CurrentValue < repairTarget.FuelReserve.Maximum)
            {
                _resourcesRequired[ResourceType.Deuterium] =
                    repairTarget.FuelReserve.Maximum - repairTarget.FuelReserve.CurrentValue;
            }

            if (repairTarget.HullStrength.CurrentValue < repairTarget.ShipDesign.HullStrength)
            {
                float hullCurrent = repairTarget.HullStrength.CurrentValue;
                float hullTotal = repairTarget.ShipDesign.HullStrength;
                float pHullDamage = 1.0f - (hullCurrent / hullTotal);
                float duranium = repairTarget.Design.BuildResourceCosts[ResourceType.Duranium];
                _resourcesRequired[ResourceType.Duranium] = (int)(pHullDamage * duranium);
            }
        }

        /// <summary>
        /// Creates an equivalent clone of this <see cref="ShipBuildProject"/>.
        /// </summary>
        /// <returns>The clone.</returns>
        public override BuildProject CloneEquivalent()
        {
            return null;
        }
    }
}
