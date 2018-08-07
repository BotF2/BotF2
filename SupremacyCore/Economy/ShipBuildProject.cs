// ShipBuildProject.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Resources;
using Supremacy.Utility;

namespace Supremacy.Economy
{
    /// <summary>
    /// Represents a shipbuilding project in the game.
    /// </summary>
    [Serializable]
    public class ShipBuildProject : BuildProject
    {
        private int _shipyardId;

        private bool _shipBuildProjectTracing = true; // turn false to hide all the GameLogs

        /// <summary>
        /// Gets the description of the ship under construction.
        /// </summary>
        /// <value>The description.</value>
        public override string Description
        {
            get
            {
                if (_shipBuildProjectTracing)
                    GameLog.Print("ShipBuildProject= {0}", ResourceManager.GetString(BuildDesign.Name));
                return ResourceManager.GetString(BuildDesign.Name);
            }
        }

        /// <summary>
        /// Gets the dilithium needed.
        /// </summary>
        /// <value>The dilithium needed.</value>
        public int DilithiumNeeded
        {
            get
            {
                if (_shipBuildProjectTracing)
                    GameLog.Print("ResourcesRequired[ResourceType.Dilithium]= {0}", ResourcesRequired[ResourceType.Dilithium]);
                return ResourcesRequired[ResourceType.Dilithium];
            }
        }

        /// <summary>
        /// Gets the dilithium used.
        /// </summary>
        /// <value>The dilithium used.</value>
        public int DilithiumUsed
        {
            get
            {
                if (_shipBuildProjectTracing)
                    GameLog.Print("ResourcesRequired[ResourceType.Dilithium]= {0}", ResourcesRequired[ResourceType.Dilithium]);
                return ResourcesInvested[ResourceType.Dilithium];
            }
        }

        /// <summary>
        /// Gets the deuterium needed.
        /// </summary>
        /// <value>The deuterium needed.</value>
        public int DeuteriumNeeded
        {
            get
            {
                if (_shipBuildProjectTracing)
                    GameLog.Print("ResourcesRequired[ResourceType.Deuterium]= {0}", ResourcesRequired[ResourceType.Deuterium]);
                return ResourcesRequired[ResourceType.Deuterium];
            }
        }

        /// <summary>
        /// Gets the deuterium used.
        /// </summary>
        /// <value>The deuterium used.</value>
        public int DeuteriumUsed
        {
            get
            {
                if (_shipBuildProjectTracing)
                    GameLog.Print("ResourcesInvested[ResourceType.Deuterium]= {0}", ResourcesInvested[ResourceType.Deuterium]);
                return ResourcesInvested[ResourceType.Deuterium];
            }
        }

        /// <summary>
        /// Gets the raw materials needed.
        /// </summary>
        /// <value>The raw materials needed.</value>
        public int RawMaterialsNeeded
        {
            get
            {
                if (_shipBuildProjectTracing)
                    GameLog.Print("ResourcesRequired[ResourceType.RawMaterials]= {0}", ResourcesRequired[ResourceType.RawMaterials]);
                return ResourcesRequired[ResourceType.RawMaterials];
            }
        }

        /// <summary>
        /// Gets the raw materials used.
        /// </summary>
        /// <value>The raw materials used.</value>
        public int RawMaterialsUsed
        {
            get
            {
                if (_shipBuildProjectTracing)
                    GameLog.Print("ResourcesInvested[ResourceType.RawMaterials]= {0}", ResourcesInvested[ResourceType.RawMaterials]);
                return ResourcesInvested[ResourceType.RawMaterials];
            }
        }

        public bool HasRawMaterialsShortage
        {
            get
            {
                //if (_shipBuildProjectTracing == true)
                    GameLog.Print("GetFlag(BuildProjectFlags.RawMaterialsShortage)= {0}", GetFlag(BuildProjectFlags.RawMaterialsShortage));
                return GetFlag(BuildProjectFlags.RawMaterialsShortage);
            }
            protected set { SetFlag(BuildProjectFlags.RawMaterialsShortage, value); }
        }

        public bool HasDeuteriumShortage
        {
            get
            {
                if (_shipBuildProjectTracing)
                    GameLog.Print("GetFlag(BuildProjectFlags.DeuteriumShortage)= {0}", GetFlag(BuildProjectFlags.DeuteriumShortage));
                return GetFlag(BuildProjectFlags.DeuteriumShortage);
            }
            protected set
            {
                if (_shipBuildProjectTracing)
                    GameLog.Print("GetFlag(BuildProjectFlags.DeuteriumShortage)= {0}", GetFlag(BuildProjectFlags.DeuteriumShortage));
                SetFlag(BuildProjectFlags.DeuteriumShortage, value);
            }
        }

        public bool HasDilithiumShortage
        {
            get
            {
                if (_shipBuildProjectTracing)
                    GameLog.Print("GetFlag(BuildProjectFlags.DilithiumShortage)= {0}", GetFlag(BuildProjectFlags.DilithiumShortage));
                return GetFlag(BuildProjectFlags.DilithiumShortage);
            }
            protected set
            {
                if (_shipBuildProjectTracing)
                    GameLog.Print("GetFlag(BuildProjectFlags.DilithiumShortage)= {0}", GetFlag(BuildProjectFlags.DilithiumShortage));
                SetFlag(BuildProjectFlags.DilithiumShortage, value);
            }
        }

        /// <summary>
        /// Gets the shipyard at which this <see cref="ShipBuildProject"/> is under construction.
        /// </summary>
        /// <value>The shipyard.</value>
        public Shipyard Shipyard
        {
            get { return GameContext.Current.Universe.Objects[_shipyardId] as Shipyard; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShipBuildProject"/> class.
        /// </summary>
        /// <param name="shipyard">The shipyard.</param>
        /// <param name="design">The ship design.</param>
        public ShipBuildProject(Shipyard shipyard, ShipDesign design)
            : base(shipyard.Owner, shipyard, design)
        {
            //works
            //if (_shipBuildProjectTracing == true && shipyard.Owner.Name == "Borg")
            //    GameLog.Print("for Borg only: AVAILABLE ShipBuildProjects - shipyard.Name = {0}, {1}", shipyard.Name, design);

            _shipyardId = shipyard.ObjectID;
        }

        /// <summary>
        /// Gets the amount of industry available for investment during the current turn.
        /// </summary>
        /// <returns>The industry available.</returns>
        protected override int GetIndustryAvailable()
        {
            // works
            // if (_shipBuildProjectTracing == true && Shipyard.Owner.Name == "Borg")
            //    GameLog.Print("ShipBuildProject - GetIndustryAvailable - Shipyard.GetBuildOutput(0) = {0}", Shipyard.GetBuildOutput(0));
            return Shipyard.GetBuildOutput(0);
        }

        /// <summary>
        /// Creates an equivalent clone of this <see cref="ShipBuildProject"/>.
        /// </summary>
        /// <returns>The clone.</returns>
        public override BuildProject CloneEquivalent()
        {
            if (_shipBuildProjectTracing)
                GameLog.Print("ShipBuildProject - BuildProject CloneEquivalent = {0}, {1}", Shipyard.Name, BuildDesign.Name);
            return new ShipBuildProject(Shipyard, BuildDesign as ShipDesign);
        }
    }

    /// <summary>
    /// Represents a ship upgrade project in the game.
    /// </summary>
    [Serializable]
    public class ShipUpgradeProject : ShipBuildProject
    {
        private int _upgradeTargetId;

        private bool _shipUpgradeProjectTracing = true;

        /// <summary>
        /// Gets a value indicating whether this <see cref="ShipUpgradeProject"/> is an upgrade project.
        /// </summary>
        /// <value>
        /// <c>true</c>.
        /// </value>
        public override bool IsUpgrade
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the description of the ship under construction.
        /// </summary>
        /// <value>The description.</value>
        public override string Description
        {
            get
            {
                return String.Format(
                    ResourceManager.GetString("SHIP_UPGRADING_FORMAT"),
                    ResourceManager.GetString(BuildDesign.Name));
            }
        }

        /// <summary>
        /// Gets the ship being upgraded.
        /// </summary>
        /// <value>The ship being upgraded.</value>
        public Ship UpgradeTarget
        {
            get { return GameContext.Current.Universe.Objects[_upgradeTargetId] as Ship; }
        }

        /// <summary>
        /// Finishes this <see cref="ShipUpgradeProject"/> and creates the newly constructed item.
        /// </summary>
        public override void Finish()
        {
            if (_shipUpgradeProjectTracing)
                GameLog.Print("ShipBuildProject - Finish = {0}, {1}", UpgradeTarget.ShipDesign.Name, BuildDesign.Name);
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
            if (_shipUpgradeProjectTracing)
                GameLog.Print("ShipUpgradeProject - Finish = {0}, {1}, {2}",shipyard.Name, upgradeTarget.Name, design.Name);
            _upgradeTargetId = upgradeTarget.ObjectID;
        }

        /// <summary>
        /// Creates an equivalent clone of this <see cref="ShipBuildProject"/>.
        /// </summary>
        /// <returns>The clone.</returns>
        public override BuildProject CloneEquivalent()
        {
            if (_shipUpgradeProjectTracing)
                GameLog.Print("ShipBuildProject - CloneEquivalent 2");
            return null;
        }
    }

    /// <summary>
    /// Represents a ship repair project in the game.
    /// </summary>
    [Serializable]
    public class ShipRepairProject : ShipBuildProject
    {
        private int _repairTargetId;
        private int _laborRequired;
        private ResourceValueCollection _resourcesRequired;

        private bool _shipRepairProjectTracing = true;

        /// <summary>
        /// Gets the description of the ship being repaired.
        /// </summary>
        /// <value>The description.</value>
        public override string Description
        {
            get
            {
                if (_shipRepairProjectTracing)
                    GameLog.Print("ShipBuildProject - Repair = {0}, {1}", ResourceManager.GetString("SHIP_REPAIRING_FORMAT"), ResourceManager.GetString(BuildDesign.Name));
                return String.Format(
                    ResourceManager.GetString("SHIP_REPAIRING_FORMAT"),
                    ResourceManager.GetString(BuildDesign.Name));
            }
        }

        /// <summary>
        /// Gets the ship being repaired.
        /// </summary>
        /// <value>The ship being repaired.</value>
        public Ship RepairTarget
        {
            get { return GameContext.Current.Universe.Objects[_repairTargetId] as Ship; }
        }

        /// <summary>
        /// Gets the total industry required to complete this <see cref="ShipRepairProject"/>.
        /// </summary>
        /// <value>The industry required.</value>
        protected override int IndustryRequired
        {
            get { return _laborRequired; }
        }

        /// <summary>
        /// Gets the total resources required to complete this <see cref="ShipRepairProject"/>.
        /// </summary>
        /// <value>The resources required.</value>
        protected override ResourceValueCollection ResourcesRequired
        {
            get { return _resourcesRequired; }
        }

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
                    (repairTarget.FuelReserve.Maximum - repairTarget.FuelReserve.CurrentValue);
            }

            if (repairTarget.HullStrength.CurrentValue < repairTarget.ShipDesign.HullStrength)
            {
                float hullCurrent = repairTarget.HullStrength.CurrentValue;
                float hullTotal = repairTarget.ShipDesign.HullStrength;
                float pHullDamage = (1.0f - (hullCurrent / hullTotal));
                float rawMaterials = repairTarget.Design.BuildResourceCosts[ResourceType.RawMaterials];
                _resourcesRequired[ResourceType.RawMaterials] = (int)(pHullDamage * rawMaterials);
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
