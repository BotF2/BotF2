// StructureBuildProject.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Buildings;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Resources;
using Supremacy.Tech;
using Supremacy.Universe;
using Supremacy.Utility;

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

        bool _tracingStructureUpgradeProject = false;   // turn true if you want

        //if (_tracingStructureUpgradeProject == true)
        //    GameLog.Print("_tracingStructureUpgradeProject is turned to true");

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
                return String.Format(
                    ResourceManager.GetString("UPGRADE_FORMAT_STRING"),
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
            if (_tracingStructureUpgradeProject)
                GameLog.Print("Finish is Destroy STRUCTURE UpgradeTarget = {0}", UpgradeTarget.Name);
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
            if (_tracingStructureUpgradeProject)
                GameLog.Print("STRUCTURE upgradeTarget.ObjectID = {0}", upgradeTarget.ObjectID);
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

        bool _tracingShipyardUpgradeProject = false;   // turn true if you want

        // if (_tracingShipyardUpgradeProject == true)
        //     GameLog.Print("_tracingShipyardUpgradeProject is turned to true");

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
                return String.Format(
                    ResourceManager.GetString("UPGRADE_FORMAT_STRING"),
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
                if (_tracingShipyardUpgradeProject)
                    GameLog.Print("UpgradeTarget shipyard name = {0}", shipyard.Name );
                return shipyard;
            }
        }

        /// <summary>
        /// Finishes this <see cref="StructureUpgradeProject"/> and creates the newly constructed item.
        /// </summary>
        public override void Finish()
        {
            if (_tracingShipyardUpgradeProject)
                GameLog.Print("_tracingShipyardUpgradeProject is turned to true");

            if (_tracingShipyardUpgradeProject)
                GameLog.Print("trying to complete current build project for {0}, UpgradeTarget.IsBuilding(UpgradeTarget.Design) = {1}", 
                UpgradeTarget.Name, UpgradeTarget.IsBuilding(UpgradeTarget.Design));

            //if (UpgradeTarget.BuildQueue.Count > 0)
            //if (UpgradeTarget.IsBuilding(UpgradeTarget.Design))
            //if (UpgradeTarget.Sector.System.Colony.Shipyard.BuildSlots.)





            //var _shipUnderConstruction = this.Colony.BuildQueue.Contains;

            //GameLog.Print("UpgradeTarget shipyard name = {0}", this.Colony.BuildQueue.Contains.);
            //(TechObjectDesign "BORG_SCOUT_I");

            //if (this.Colony.Shipyard.BuildSlots.)

            for (var i = 0; i < UpgradeTarget.BuildSlots.Count; i++)
            {
                var slot = UpgradeTarget.BuildSlots[i];

                if (slot.HasProject && slot.Project.IsPartiallyComplete)
                {
                    if (_tracingShipyardUpgradeProject)
                        GameLog.Print("slot.HasProject && slot.Project.IsPartiallyComplete ...for {0} in SlotID={1}", slot.Project.BuildDesign,  slot.SlotID);
                    //, colony.Name, civ.Name);      Slot {2} is finished at {3} for {4}
                    slot.Project.Finish();

                    if (_tracingShipyardUpgradeProject)
                        GameLog.Print("slot.Project.Finish is done for SlotID = {0}", slot.SlotID);

                    slot.Project = null;

                    //if (
                    //    //projectFinished && 
                    //    (slot.Project == null))
                    //    { 
                    //    //&&
                    //    //!UpgradeTarget.BuildSlots.(o => o.Project != null))

                    //    GameLog.Print("slot.Project == null ...for {0} in SlotID={1}", slot.Project.BuildDesign, slot.SlotID);
                    //    //civManager.SitRepEntries.Add(new BuildQueueEmptySitRepEntry(civ, colony, true));
                    //}
                    if (_tracingShipyardUpgradeProject)
                        GameLog.Print("checking next slotID={0} +1", slot.Project.BuildDesign, slot.SlotID);

                }
            }
            
            // wrong info.....in some unknown kind even a 2-Turn-Ship is immediately builded while Shpiyard is upgraded
            // MessageBox.Show("Upgrading SHIPYARD is waiting until BuildQueue is empty", "INFO", MessageBoxButton.OK);
            //  GameLog.Print("Upgrading is waiting until BuildQueue is empty");    
            
            //  no MessageDialog available, might due to SupremacyCore      var result = MessageDialog.Show("Upgrading is waiting until BuildQueue is empty", MessageDialogButtons.ok);

            
            if (_tracingShipyardUpgradeProject)
                GameLog.Print("trying to complete current build project for {0} - BEFORE Scrapping Upgrade Target", UpgradeTarget.Name);

            // next is to scrap Upgrade target which might not be available any more
            var _scrapped = UpgradeTarget.Name;

            //if (UpgradeTarget.ObjectType = Shipyard)
            //////////////////////////////////////    new: GameContext.Current.Universe.Scrap(UpgradeTarget);
            //if (UpgradeTarget.Scrap)

            if (_tracingShipyardUpgradeProject)
                GameLog.Print("Finish is Destroy SHIPYARD UpgradeTarget = {0}", _scrapped);
            // atm no destroying       
            GameContext.Current.Universe.Destroy(UpgradeTarget);

            if (_tracingShipyardUpgradeProject)
                GameLog.Print("Finish is Destroy SHIPYARD UpgradeTarget = {0}", _scrapped);

            if (_tracingShipyardUpgradeProject)
                GameLog.Print("trying to complete current build project for {0} - BEFORE base.Finish", _scrapped);

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
            if (_tracingShipyardUpgradeProject)
                GameLog.Print("upgradeTarget.ObjectID = {0}", upgradeTarget.ObjectID);
            _upgradeTargetId = upgradeTarget.ObjectID;
        }



        /// <summary>
        /// Creates an equivalent clone of this <see cref="StructureBuildProject"/>.
        /// </summary>
        /// <returns>The clone.</returns>
        public override BuildProject CloneEquivalent()
        {
            if (_tracingShipyardUpgradeProject)
                GameLog.Print("BuildProject CloneEquivalent() ");
            return null;
        }
    }

    //public class IntelyardUpgradeProject : StructureBuildProject
    //{
    //    private readonly int _upgradeTargetId;

    //    /// <summary>
    //    /// Gets a value indicating whether this <see cref="StructureUpgradeProject"/>
    //    /// is an upgrade project.
    //    /// </summary>
    //    /// <value>
    //    /// <c>true</c>
    //    /// </value>
    //    public override bool IsUpgrade
    //    {
    //        get { return true; }
    //    }

    //    /// <summary>
    //    /// Gets the description of the target building design
    //    /// </summary>
    //    /// <value>The description.</value>
    //    public override string Description
    //    {
    //        get
    //        {
    //            return String.Format(
    //                ResourceManager.GetString("UPGRADE_FORMAT_STRING"),
    //                ResourceManager.GetString(BuildDesign.Name));
    //        }
    //    }

    //    /// <summary>
    //    /// Gets the target upgrade design.
    //    /// </summary>
    //    /// <value>The target upgrade design.</value>
    //    //public Intelyard UpgradeTarget
    //    //{
    //    //    get
    //    //    {
    //    //        var intelyard = this.Colony.Intelyard;
    //    //        if (intelyard == null || intelyard.ObjectID != _upgradeTargetId)
    //    //            return null;
    //    //        return intelyard;
    //    //    }
    //    //}

    //    /// <summary>
    //    /// Finishes this <see cref="StructureUpgradeProject"/> and creates the newly constructed item.
    //    /// </summary>
    //    public override void Finish()
    //    {
    //        GameContext.Current.Universe.Destroy(UpgradeTarget);
    //        base.Finish();
    //    }

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="StructureUpgradeProject"/> class.
    //    /// </summary>
    //    /// <param name="upgradeTarget">The building to be upgraded.</param>
    //    /// <param name="design">The target upgrade design.</param>
    //    //public IntelyardUpgradeProject(Intelyard upgradeTarget, IntelyardDesign design)
    //    //    : base(upgradeTarget.Sector.System.Colony, design)
    //    //{
    //    //    _upgradeTargetId = upgradeTarget.ObjectID;
    //    //}



    //    /// <summary>
    //    /// Creates an equivalent clone of this <see cref="StructureBuildProject"/>.
    //    /// </summary>
    //    /// <returns>The clone.</returns>
    //    public override BuildProject CloneEquivalent()
    //    {
    //        return null;
    //    }
    //}

}
