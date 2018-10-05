// StructureBuildProject.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Buildings;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Resources;
using Supremacy.Tech;
using Supremacy.Universe;
using System;

namespace Supremacy.Economy
{
    /// <summary>
    /// Represents a planetary building construction project in the game.
    /// </summary>
    [Serializable]
    public class StructureBuildProject : BuildProject
    {
        private readonly int _colonyId;

        /// <summary>
        /// Gets the colony at which construction is taking place.
        /// </summary>
        /// <value>The colony.</value>
        public Colony Colony
        {
            get { return GameContext.Current.Universe.Objects[_colonyId] as Colony; }
        }

        /// <summary>
        /// Gets the amount of industry available for investment during the current turn.
        /// </summary>
        /// <returns>The industry available.</returns>
        protected override int GetIndustryAvailable()
        {
            return Colony.GetProductionOutput(ProductionCategory.Industry);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StructureBuildProject"/> class.
        /// </summary>
        /// <param name="colony">The source colony.</param>
        /// <param name="design">The building design.</param>
        public StructureBuildProject(Colony colony, PlanetaryTechObjectDesign design)
            : base(colony.Owner, colony, design)
        {
            _colonyId = colony.ObjectID;
        }

        /// <summary>
        /// Creates an equivalent clone of this <see cref="StructureBuildProject"/>.
        /// </summary>
        /// <returns>The clone.</returns>
        public override BuildProject CloneEquivalent()
        {
            return new StructureBuildProject(Colony, BuildDesign as PlanetaryTechObjectDesign);
        }
    }

    /// <summary>
    /// Represents a planetary building upgrade project in the game.
    /// </summary>
    [Serializable]
    public class StructureUpgradeProject : StructureBuildProject
    {
        private readonly int _upgradeTargetId;

        /// <summary>
        /// Gets a value indicating whether this <see cref="StructureUpgradeProject"/>
        /// is an upgrade project.
        /// </summary>
        /// <value>
        /// <c>true</c>
        /// </value>
        public override bool IsUpgrade
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the description of the target building design
        /// </summary>
        /// <value>The description.</value>
        public override string Description
        {
            get
            {
                return string.Format(ResourceManager.GetString("UPGRADE_FORMAT_STRING"),
                    ResourceManager.GetString(BuildDesign.Name));
            }
        }

        /// <summary>
        /// Gets the target upgrade design.
        /// </summary>
        /// <value>The target upgrade design.</value>
        public Building UpgradeTarget
        {
            get { return GameContext.Current.Universe.Objects[_upgradeTargetId] as Building; }
        }

        /// <summary>
        /// Finishes this <see cref="StructureUpgradeProject"/> and creates the newly constructed item.
        /// </summary>
        public override void Finish()
        {
            GameContext.Current.Universe.Destroy(UpgradeTarget);
            base.Finish();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StructureUpgradeProject"/> class.
        /// </summary>
        /// <param name="upgradeTarget">The building to be upgraded.</param>
        /// <param name="design">The target upgrade design.</param>
        public StructureUpgradeProject(Building upgradeTarget, BuildingDesign design)
            : base(upgradeTarget.Sector.System.Colony, design)
        {
            _upgradeTargetId = upgradeTarget.ObjectID;
        }

        /// <summary>
        /// Creates an equivalent clone of this <see cref="StructureBuildProject"/>.
        /// </summary>
        /// <returns>The clone.</returns>
        public override BuildProject CloneEquivalent()
        {
            return null;
        }
    }

    /// <summary>
    /// Represents a planetary building upgrade project in the game.
    /// </summary>
    [Serializable]
    public class ShipyardUpgradeProject : StructureBuildProject
    {
        private readonly int _upgradeTargetId;

        /// <summary>
        /// Gets a value indicating whether this <see cref="StructureUpgradeProject"/>
        /// is an upgrade project.
        /// </summary>
        /// <value>
        /// <c>true</c>
        /// </value>
        public override bool IsUpgrade
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the description of the target building design
        /// </summary>
        /// <value>The description.</value>
        public override string Description
        {
            get
            {
                return string.Format(ResourceManager.GetString("UPGRADE_FORMAT_STRING"),
                    ResourceManager.GetString(BuildDesign.Name));
            }
        }

        /// <summary>
        /// Gets the target upgrade design.
        /// </summary>
        /// <value>The target upgrade design.</value>
        public Shipyard UpgradeTarget
        {
            get
            {
                var shipyard = Colony.Shipyard;
                if (shipyard == null || shipyard.ObjectID != _upgradeTargetId)
                    return null;
                return shipyard;
            }
        }

        /// <summary>
        /// Finishes this <see cref="StructureUpgradeProject"/> and creates the newly constructed item.
        /// </summary>
        public override void Finish()
        {
            for (var i = 0; i < UpgradeTarget.BuildSlots.Count; i++)
            {
                var slot = UpgradeTarget.BuildSlots[i];

                if (slot.HasProject && slot.Project.IsPartiallyComplete)
                {
                    slot.Project.Finish();
                    slot.Project = null;
                }
            }

            // next is to scrap Upgrade target which might not be available any more
            var _scrapped = UpgradeTarget.Name;            
            GameContext.Current.Universe.Destroy(UpgradeTarget);
            base.Finish();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StructureUpgradeProject"/> class.
        /// </summary>
        /// <param name="upgradeTarget">The building to be upgraded.</param>
        /// <param name="design">The target upgrade design.</param>
        public ShipyardUpgradeProject(Shipyard upgradeTarget, ShipyardDesign design)
            : base(upgradeTarget.Sector.System.Colony, design)
        {
            _upgradeTargetId = upgradeTarget.ObjectID;
        }

        /// <summary>
        /// Creates an equivalent clone of this <see cref="StructureBuildProject"/>.
        /// </summary>
        /// <returns>The clone.</returns>
        public override BuildProject CloneEquivalent()
        {
            return null;
        }
    }

}
