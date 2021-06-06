// FleetOrders.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.AI;
using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Diplomacy;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Client;
using Supremacy.Pathfinding;
using Supremacy.Resources;
using Supremacy.Tech;
using Supremacy.Text;
using Supremacy.Types;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Supremacy.Intelligence;

namespace Supremacy.Orbitals
{
    [Serializable]
    public static class FleetOrders
    {
        public static readonly EngageOrder EngageOrder;
        public static readonly AvoidOrder AvoidOrder;
        public static readonly ColonizeOrder ColonizeOrder;
       // public static readonly RaidOrder RaidOrder;
        public static readonly SabotageOrder SabotageOrder;
		public static readonly InfluenceOrder InfluenceOrder;
        public static readonly MedicalOrder MedicalOrder;
        public static readonly SpyOnOrder SpyOnOrder; // install spy network
        public static readonly TowOrder TowOrder;
        public static readonly WormholeOrder WormholeOrder;
        public static readonly CollectDeuteriumOrder CollectDeuteriumOrder;
        //public static readonly EscortOrder EscortOrder;
        public static readonly BuildStationOrder BuildStationOrder;
        public static readonly ExploreOrder ExploreOrder;
        public static readonly AssaultSystemOrder AssaultSystemOrder;

        private static readonly List<FleetOrder> _orders;


        static FleetOrders()
        {
            EngageOrder = new EngageOrder();
            AvoidOrder = new AvoidOrder();
            ColonizeOrder = new ColonizeOrder();
            // RaidOrder = new RaidOrder();
            SabotageOrder = new SabotageOrder();
            InfluenceOrder = new InfluenceOrder();
            MedicalOrder = new MedicalOrder();
            SpyOnOrder = new SpyOnOrder();
            TowOrder = new TowOrder();
            WormholeOrder = new WormholeOrder();
            CollectDeuteriumOrder = new CollectDeuteriumOrder();
            //EscortOrder = new EscortOrder();
            BuildStationOrder = new BuildStationOrder();
            ExploreOrder = new ExploreOrder();
            AssaultSystemOrder = new AssaultSystemOrder();

            _orders = new List<FleetOrder>
                      {
                          EngageOrder,
                          AvoidOrder,
                          ColonizeOrder,
                          //RaidOrder,
                          SabotageOrder,
                          InfluenceOrder,
                          MedicalOrder,
                          SpyOnOrder, // install spy network
                          //TowOrder,
                          WormholeOrder,
                          CollectDeuteriumOrder,
                          // EscortOrder, // this is done in UnitAI by adding escort to fleet as non combat ships (fleet) get order to leave home system
                          BuildStationOrder,
                          ExploreOrder,
                          AssaultSystemOrder,
                      };

        }

        public static ICollection<FleetOrder> GetAvailableOrders(Fleet fleet)
        {
            return _orders.Where(o => o.CanAssignOrder(fleet)).Select(o => o.Create()).ToList();
        }
    }

    #region Engage Order
    [Serializable]
    public sealed class EngageOrder : FleetOrder
    {
        public override string OrderName => ResourceManager.GetString("FLEET_ORDER_ENGAGE");

        public override string Status => ResourceManager.GetString("FLEET_ORDER_ENGAGE");

        public override bool IsValidOrder(Fleet fleet)
        {
            return (base.IsValidOrder(fleet) && fleet.IsCombatant);
        }

        public override FleetOrder Create()
        {
            return new EngageOrder();
        }
    }
    #endregion

    #region Assault System Order
    [Serializable]
    public sealed class AssaultSystemOrder : FleetOrder
    {
        public override string OrderName => LocalizedTextDatabase.Instance.GetString(typeof(AssaultSystemOrder), "Description");
        //             get { return ResourceManager.GetString("SYSTEM_ASSAULT_DESCRIPTION"); }
        //             get { return LocalizedTextDatabase.Instance.GetString(typeof(AssaultSystemOrder), "Description"); }

        public override string Status
        {
            get
            {
                var statusFormat = LocalizedTextDatabase.Instance.GetString(typeof(AssaultSystemOrder), "StatusFormat");

                //GameLog.Core.Combat.DebugFormat("getting Status of Assault System");

                //var statusFormat = ResourceManager.GetString("SYSTEM_ASSAULT_STATUS_FORMAT");
                if (statusFormat == null)
                    return OrderName;

                var fleet = Fleet;
                var sector = fleet?.Sector.Name;  // checking for Sector existing and Name

                //GameLog.Core.Combat.DebugFormat("getting Status of Assault System...returning {0}", string.Format(statusFormat, sector));
                return string.Format(statusFormat, sector);
            }
        }

        public override bool IsValidOrder(Fleet fleet)
        {
            //GameLog.Core.Combat.DebugFormat("Is AssaultSystem a valid order - beginning to check..."); 
            if (!base.IsValidOrder(fleet))
                return false;

            if (!fleet.HasTroopTransports)
                return false;

            var system = GameContext.Current.Universe.Map[fleet.Location].System;
            if (system == null || !system.IsInhabited)
                return false;
            //GameLog.Core.Combat.DebugFormat("Is AssaultSystem a valid order - check mostly done...");

            //GameLog.Core.Combat.DebugFormat("Is AssaultSystem a valid order - returning {0}", DiplomacyHelper.AreAtWar(system.Colony.Owner, fleet.Owner));
            return DiplomacyHelper.AreAtWar(system.Colony.Owner, fleet.Owner);
        }

        public override FleetOrder Create()
        {
            return new AssaultSystemOrder();
        }
    }
    #endregion

    #region Avoid Order

    [Serializable]
    public sealed class AvoidOrder : FleetOrder
    {
        public override string OrderName => ResourceManager.GetString("FLEET_ORDER_AVOID");

        public override string Status => ResourceManager.GetString("FLEET_ORDER_AVOID");

        public override bool WillEngageHostiles => false;

        public override FleetOrder Create()
        {
            return new AvoidOrder();
        }
    }

    #endregion

    #region ColonizeOrder

    [Serializable]
    public sealed class ColonizeOrder : FleetOrder
    {
        private readonly bool _isComplete;

        public override string OrderName => ResourceManager.GetString("FLEET_ORDER_COLONIZE");

        public override string Status => ResourceManager.GetString("FLEET_ORDER_COLONIZE");

        public override FleetOrder Create()
        {
            return new ColonizeOrder();
        }

        public override bool IsComplete => _isComplete;

        public override bool IsCancelledOnRouteChange => true;

        public override bool IsRouteCancelledOnAssign => true;

        public override bool WillEngageHostiles => false;

        public ColonizeOrder()
        {
            _isComplete = false;
        }

        private Ship FindBestColonyShip()
        {
            Ship bestShip = null;
            foreach (Ship ship in Fleet.Ships)
            {
                if (ship.ShipType == ShipType.Colony)
                {
                    if ((bestShip == null)
                        || (ship.ShipDesign.WorkCapacity > bestShip.ShipDesign.WorkCapacity))
                    {
                        bestShip = ship;
                    }
                }
            }
            return bestShip;
        }

        public override bool IsValidOrder(Fleet fleet)
        {
            if (!base.IsValidOrder(fleet))
                return false;
            if (fleet.Sector.System == null)
                return false;
            if (fleet.Sector.System.HasColony)
                return false;
            if (fleet.Sector.IsOwned && (fleet.Sector.Owner != fleet.Owner))
                return false;
            if (!fleet.Sector.System.IsHabitable(fleet.Owner.Race))
                return false;
            if (!fleet.Ships.Any(s => s.ShipType == ShipType.Colony))
                return false;
            
            return true;
        }

        protected internal override void OnTurnBeginning()
        {
            base.OnTurnBeginning();
            if (_isComplete)
                return;
            var colonyShip = FindBestColonyShip();
            if (colonyShip == null)
                return;
            
            var colony = new Colony(Fleet.Sector.System, Fleet.Owner.Race);
            var civManager = GameContext.Current.CivilizationManagers[Fleet.Owner];

            colony.ObjectID = GameContext.Current.GenerateID();
            colony.Population.BaseValue = colonyShip.ShipDesign.WorkCapacity;
            colony.Population.Reset();
            colony.Name = Fleet.Sector.System.Name;
            colony.Owner = Fleet.Owner;

            Fleet.Sector.System.Owner = Fleet.Owner;
            Fleet.Sector.System.Colony = colony;

            GameContext.Current.Universe.Objects.Add(colony);
            civManager.Colonies.Add(colony);
            colony.Morale.BaseValue = civManager.Civilization.BaseMoraleLevel;

            colony.Morale.Reset();

            ColonyBuilder.Build(colony);

            civManager.MapData.SetScanned(colony.Location, true, 1);
            civManager.ApplyMoraleEvent(MoraleEvent.ColonizeSystem, Fleet.Sector.System.Location);
            civManager.SitRepEntries.Add(new NewColonySitRepEntry(Fleet.Owner, colony));

            GameContext.Current.Universe.Destroy(colonyShip);
        }

        protected internal override void OnOrderAssigned()
        {
            base.OnOrderAssigned();
            if (!Fleet.Route.IsEmpty)
                Fleet.Route = TravelRoute.Empty;
        }
    }

    #endregion

    #region MedicalOrder
    [Serializable]
    public sealed class MedicalOrder : FleetOrder
    {
        public override string OrderName => ResourceManager.GetString("FLEET_ORDER_MEDICAL");

        public override string Status => ResourceManager.GetString("FLEET_ORDER_MEDICAL");

        public override FleetOrder Create()
        {
            return new MedicalOrder();
        }

        public override bool IsCancelledOnMove => true;

        public override bool IsCancelledOnRouteChange => true;

        public override bool IsRouteCancelledOnAssign => true;

        public override bool WillEngageHostiles => false;

        public override bool IsValidOrder(Fleet fleet)
        {
            if (!base.IsValidOrder(fleet))
            {
                return false;
            }
            if (fleet.Sector.System == null)
            {
                return false;
            }
            if (fleet.Sector.System.Colony == null)
            {
                return false;
            }
            if (fleet.Sector.System.Colony.Health.CurrentValue == 100)
            {
                return false;
            }
            return fleet.Ships.Any(s => s.ShipType == ShipType.Medical);
        }

        protected internal override void OnTurnEnding()
        {
            string blank = " ";

            //Medicate the colony --- // PopulationHealth is a percent value !!  // healthAdjustment is also a percent valuee.g. 80% * 1,3= 104% 
            //PopHealth = 0.16 (not 16)
            //int helpByShip = Fleet.Ships.Where(s => s.ShipType == ShipType.Medical).Sum(s => s.ShipDesign.PopulationHealth);
            Colony colony = Fleet.Sector.System.Colony;
            int oldHealth = colony.Health.CurrentValue;
            var healthAdjustment = 1.01f + (Fleet.Ships.Where(s => s.ShipType == ShipType.Medical).Sum(s => s.ShipDesign.PopulationHealth) / 2);
            //healthAdjustment = helpByShip / 10;
            if (healthAdjustment > 1.24f) healthAdjustment = 1.24f;
            if (Fleet.Sector.System.Colony is null)
                { /*do nothing*/ }
            else if(Fleet.Ships.Any(s => s.ShipType == ShipType.Medical))
                {
                Fleet.Sector.System.Colony.Health.AdjustCurrent(healthAdjustment);
                Fleet.Sector.System.Colony.Health.UpdateAndReset();


                string _report = Fleet.ObjectID 
                    + blank + Fleet.Name + " (" + Fleet.ClassName + ") doing Medical help at"
                    + blank + Fleet.Sector.System.Colony.Name
                    //+ blank + Fleet.Sector.System.Colony.ObjectID 
                    + blank + Fleet.Sector.System.Colony.Location + ": value adjusted ="
                    + blank + healthAdjustment + "%, new ="
                    + blank + Fleet.Sector.System.Colony.Health.CurrentValue
                    + blank + "(old=" + oldHealth + ")";

                Console.WriteLine(_report);
                GameLog.Core.ColoniesDetails.DebugFormat(_report);
                //GameLog.Core.Colonies.DebugFormat("{0} (# {1} {2}) doing Medical help at {3} ({4} at {5}): value adjusted = {6}%, new = {7}"
                //    , Fleet.Name, Fleet.ObjectID, Fleet.Ships.FirstOrDefault().ShipDesign.Name
                //    , Fleet.Sector.System.Colony.Name, Fleet.Sector.System.Colony.ObjectID, Fleet.Sector.System.Colony.Location
                //    , healthAdjustment, Fleet.Sector.System.Colony.Health.CurrentValue);

                _report = healthAdjustment + " up to " + Fleet.Sector.System.Colony.Health.CurrentValue + " (old: " + oldHealth + ") by " + Fleet.Name + " (Medical Ship)";
                GameContext.Current.CivilizationManagers[Fleet.OwnerID].SitRepEntries.Add(new ShipMedicalHelpProvidedSitRepEntry(Fleet.Owner, Fleet.Location, _report));

                _report = healthAdjustment + " up to " + Fleet.Sector.System.Colony.Health.CurrentValue + " (old: " + oldHealth + ") by " + Fleet.Name + " (" + Fleet.Owner.ShortName + " Medical Ship)";
                GameContext.Current.CivilizationManagers[Fleet.Sector.System.OwnerID].SitRepEntries.Add(new ShipMedicalHelpProvidedSitRepEntry(Fleet.Owner, Fleet.Location, _report));
            }

            //If the colony is not ours, just doing small medical help + increase regard + trust etc
            if (Fleet.Sector.System.Colony is null) // currentx
            {
                //do nothing
            }
            else if(Fleet.Sector.System.Owner != null && Fleet.Sector.System.Colony.Owner != null && Fleet.Sector.System.Owner != Fleet.Owner )
            {
                var foreignPower = Diplomat.Get(Fleet.Sector.System.Owner).GetForeignPower(Fleet.Owner);
                healthAdjustment = ((healthAdjustment - 1) / 3) + 1;

                // only small medical help = +1
                Fleet.Sector.System.Colony.Health.AdjustCurrent(healthAdjustment);  // 10%
                Fleet.Sector.System.Colony.Health.UpdateAndReset();
                //ToDo: SitRep

                // send a medical ship to other civilization's colony and get trust
                if (Fleet.Sector.System.Colony.Owner != Fleet.Owner && Fleet.Ships.Any(s => s.ShipType == ShipType.Medical))
                {
                    DiplomacyHelper.ApplyTrustChange(Fleet.Sector.System.Owner, Fleet.Owner, 50);
                    DiplomacyHelper.ApplyRegardChange(Fleet.Sector.System.Owner, Fleet.Owner, 55);
                    Diplomat.Get(Fleet.Owner).GetForeignPower(Fleet.Sector.System.Owner).UpdateRegardAndTrustMeters();
                }
                // Nonaggression treaty - you promissed not to go into the other empires space - go there and trust is lost, aggrement canceled
                else if (GameContext.Current.AgreementMatrix.IsAgreementActive(Fleet.Owner, Fleet.Sector.System.Colony.Owner, ClauseType.TreatyNonAggression))
                {
                    DiplomacyHelper.ApplyTrustChange(Fleet.Sector.System.Owner, Fleet.Owner, -55);
                    DiplomacyHelper.ApplyRegardChange(Fleet.Sector.System.Owner, Fleet.Owner, -65);
                    Diplomat.Get(Fleet.Owner).GetForeignPower(Fleet.Sector.System.Owner).UpdateRegardAndTrustMeters();
                    foreignPower.CancelTreaty();
                    //firstManager.SitRepEntries.Add(new WarDeclaredSitRepEntry(secondCiv, firstCiv));
                    //secondManager.SitRepEntries.Add(new WarDeclaredSitRepEntry(secondCiv, firstCiv));
                    ////var soundPlayer = new SoundPlayer("Resources/SoundFX/GroundCombat/Bombardment_SM.ogg"); ToDo - not working yet
                }

                string _report = Fleet.ObjectID
                    + blank + Fleet.Name + " doing Medical help at "
                    + blank + Fleet.Sector.System.Colony.Name
                    //+ blank + Fleet.Sector.System.Colony.ObjectID 
                    + blank + Fleet.Sector.System.Colony.Location + ": value adjusted = "
                    + blank + healthAdjustment + "%, new = "
                    + blank + Fleet.Sector.System.Colony.Health.CurrentValue;

                Console.WriteLine(_report);
                GameLog.Core.ColoniesDetails.DebugFormat(_report);
            }
        }

        public override bool IsComplete => Fleet.Sector.System.Colony.Health.CurrentValue >= 100;
    }
    #endregion

    #region SpyOnOrder

    [Serializable]
    public sealed class SpyOnOrder : FleetOrder
    {
        private readonly bool _isComplete;

        public override string OrderName => ResourceManager.GetString("FLEET_ORDER_SPY_ON");

        public override string Status => ResourceManager.GetString("FLEET_ORDER_SPY_ON");

        public override FleetOrder Create()
        {
            return new SpyOnOrder();
        }

        public override bool IsComplete => _isComplete;

        public override bool IsCancelledOnRouteChange => true;

        public override bool IsRouteCancelledOnAssign => true;

        public override bool WillEngageHostiles => false;

        public SpyOnOrder()
        {
            _isComplete = false;
        }

        private Ship FindBestSpyOnShip()
        {
            Ship bestShip = null;
            foreach (Ship ship in Fleet.Ships)
            {
                if (ship.ShipType == ShipType.Spy)
                {
                    if ((bestShip == null)
                        || (ship.ShipDesign.WorkCapacity > bestShip.ShipDesign.WorkCapacity))
                    {
                        bestShip = ship;
                    }
                }
            }
            return bestShip;
        }

        public override bool IsValidOrder(Fleet fleet)
        {
            //var civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
            if (!base.IsValidOrder(fleet))
                return false;
           // if (civManager.SpiedCivList.Where(S => S.CivID == fleet.Sector.System.Colony.OwnerID).Any()) // only install spy network once per empire
                   // return false;
            if (fleet.Sector.System == null)
                return false;
            if (fleet.Sector.System.Colony == null)
                return false;
            if (fleet.Sector.IsOwned && (fleet.Sector.Owner == fleet.Owner))
                return false;
            if (fleet.Sector.Owner.CivID == 6) // borg systems us sabotage order
                return false;
            if (!fleet.Sector.Owner.IsEmpire)  // if it is NOT an empire, return false
                return false;
            if (fleet.Sector.System.Colony.Name != fleet.Sector.Owner.HomeSystemName)
                return false;
            foreach (var ship in fleet.Ships)
            {
                if (ship.ShipType == ShipType.Spy)
                {
                    return true;
                }
                    
            }
            return false;
        }

        protected internal override void OnTurnBeginning()
        {
            base.OnTurnBeginning();
            if (!IsAssigned)
                return;
            if (_isComplete)
                return;
            var spyOnShip = FindBestSpyOnShip();
            if (spyOnShip == null)
                return;
            CreateSpyOn(
                Fleet.Owner,
                Fleet.Sector.System);
            //GameContext.Current.Universe.Destroy(SpyOnShip);
        }

        protected internal override void OnOrderAssigned()
        {
            base.OnOrderAssigned();
            if (!Fleet.Route.IsEmpty)
                Fleet.Route = TravelRoute.Empty;
        }

        private static void CreateSpyOn(Civilization civ, StarSystem system)
        {
            var colonies =  GameContext.Current.CivilizationManagers[system.Owner].Colonies; //IntelHelper.NewSpiedColonies; ???????
            //var civManager = GameContext.Current.CivilizationManagers[civ];

            //int defenseIntelligence = GameContext.Current.CivilizationManagers[system.Owner].TotalIntelligence + 1;  // TotalIntelligence of attacked civ
            //if (defenseIntelligence - 1 < 0.1)
            //    defenseIntelligence = 2;

            //int attackingIntelligence = GameContext.Current.CivilizationManagers[civ].TotalIntelligence + 1;  // TotalIntelligence of attacked civ
            //if (attackingIntelligence - 1 < 0.1)
            //    attackingIntelligence = 1;

            //int ratio = attackingIntelligence / defenseIntelligence;
            ////max ratio for no exceeding gaining points
            //if (ratio > 10)
            //    ratio = 10;

            IntelHelper.SendXSpiedY(civ, system.Owner, colonies);          
            GameLog.Client.Test.DebugFormat("CreateSpyOn calls IntelHelper SendTargetOne for system ={0} owner ={1}", system, system.Owner);
        }
    }

    #endregion

    //#region RaidOrder

    //[Serializable]
    //public sealed class RaidOrder : FleetOrder
    //{
    //    private readonly bool _isComplete;

    //    public override string OrderName
    //    {
    //        get { return ResourceManager.GetString("FLEET_ORDER_RAID"); }
    //    }

    //    public override string Status
    //    {
    //        get { return ResourceManager.GetString("FLEET_ORDER_RAID"); }
    //    }

    //    public override FleetOrder Create()
    //    {
    //        return new RaidOrder();
    //    }

    //    public override bool IsComplete
    //    {
    //        get { return _isComplete; }
    //    }

    //    public override bool IsCancelledOnRouteChange
    //    {
    //        get { return true; }
    //    }

    //    public override bool IsRouteCancelledOnAssign
    //    {
    //        get { return true; }
    //    }

    //    public override bool WillEngageHostiles
    //    {
    //        get { return false; }
    //    }

    //    public RaidOrder()
    //    {
    //        _isComplete = false;
    //    }

    //    private Ship FindBestRaidShip()
    //    {
    //        Ship bestShip = null;
    //        foreach (Ship ship in Fleet.Ships)
    //        {
    //            if (ship.ShipType == ShipType.FastAttack)
    //            {
    //                if ((bestShip == null)
    //                    || (ship.ShipDesign.WorkCapacity > bestShip.ShipDesign.WorkCapacity))
    //                {
    //                    bestShip = ship;
    //                }
    //            }
    //        }
    //        return bestShip;
    //    }

    //    public override bool IsValidOrder(Fleet fleet)
    //    {
    //        return false; // replaced by AssetsScreen spy missions
    //        //if (!base.IsValidOrder(fleet))
    //        //    return false;
    //        //if (fleet.Sector.System == null)
    //        //    return false;
    //        //if (!fleet.Sector.System.HasColony)
    //        //    return false;
    //        //if (fleet.Sector.System.Owner == fleet.Owner)
    //        //    return false;
    //        //if (!fleet.Ships.Any(s => s.ShipType == ShipType.FastAttack))
    //        //    return false;

    //        //return true;
    //    }

    //    protected internal override void OnTurnBeginning()
    //    {
    //        base.OnTurnBeginning();
    //        if (_isComplete)
    //            return;
    //        var raidShip = FindBestRaidShip();
    //        if (raidShip == null)
    //            return;

    //        var raidedCiv = GameContext.Current.CivilizationManagers[Fleet.Sector.System.Owner];
    //        var raiderCiv = GameContext.Current.CivilizationManagers[Fleet.Owner];

    //        int defenseIntelligence = raidedCiv.TotalIntelligenceProduction + 1;
    //        if (defenseIntelligence - 1 < 0.1)
    //            defenseIntelligence = 2;

    //        int attackingIntelligence = raiderCiv.TotalIntelligenceProduction + 1;
    //        if (attackingIntelligence - 1 < 0.1)
    //            attackingIntelligence = 1;

    //        int ratio = attackingIntelligence / defenseIntelligence;
    //        if (ratio > 10)
    //            ratio = 10;

    //        //TODO: Actually do something with the ratio

    //        GameLog.Core.Intel.DebugFormat("{0} is raiding {1} at {2} (AttackIntel={3}, DefenseIntel={4}, Ratio={5})",
    //            raiderCiv, raidedCiv, Fleet.Sector.System, attackingIntelligence, defenseIntelligence, ratio);

    //        int gainedCredits = Fleet.Sector.System.Colony.TaxCredits;

    //        if (gainedCredits > 10)
    //            gainedCredits = gainedCredits * ratio / 10;

    //        GameLog.Core.Intel.DebugFormat("{0} gained {1} by raiding the {2} colony at {3}",
    //            raiderCiv, gainedCredits, raidedCiv, Fleet.Sector.System);
    //        GameLog.Core.Intel.DebugFormat("{0} credits - Before={1}, After={2}",
    //            raiderCiv.Credits.CurrentValue, raiderCiv.Credits.CurrentValue + gainedCredits);
    //        GameLog.Core.Intel.DebugFormat("{0} credits - Before={1}, After={2}",
    //            raidedCiv.Credits.CurrentValue, raidedCiv.Credits.CurrentValue - gainedCredits);

    //        raiderCiv.SitRepEntries.Add(new NewRaidSitRepEntry(raidedCiv.Civilization, Fleet.Sector.System.Colony, gainedCredits, raidedCiv.Credits.CurrentValue));

    //        raiderCiv.Credits.AdjustCurrent(gainedCredits);
    //        raidedCiv.Credits.AdjustCurrent(gainedCredits * -1);
    //    }

    //    protected internal override void OnOrderAssigned()
    //    {
    //        base.OnOrderAssigned();
    //        if (!Fleet.Route.IsEmpty)
    //            Fleet.Route = TravelRoute.Empty;
    //    }
    //}
    //#endregion

    #region SabotageOrder

    [Serializable]
    public sealed class SabotageOrder : FleetOrder
    {
        private readonly bool _isComplete;

        public override string OrderName => ResourceManager.GetString("FLEET_ORDER_SABOTAGE");

        public override string Status => ResourceManager.GetString("FLEET_ORDER_SABOTAGE");

        public override FleetOrder Create()
        {
            return new SabotageOrder();
        }

        public override bool IsComplete => _isComplete;

        public override bool IsCancelledOnRouteChange => true;

        public override bool IsRouteCancelledOnAssign => true;

        public override bool WillEngageHostiles => false;

        public SabotageOrder()
        {
            _isComplete = false;
        }

        private Ship FindBestSabotageShip()
        {
            Ship bestShip = null;
            foreach (Ship ship in Fleet.Ships)
            {
                if (ship.ShipType == ShipType.Spy)
                {
                    if ((bestShip == null)
                        || (ship.ShipDesign.WorkCapacity > bestShip.ShipDesign.WorkCapacity))
                    {
                        bestShip = ship;
                    }
                }
            }
            return bestShip;
        }

        public override bool IsValidOrder(Fleet fleet)
        {
            // Borg systems only in place of AssetsScreen spy missions for the rest
            if (!base.IsValidOrder(fleet))
                return false;
            if (fleet.Sector.System == null)
                return false;
            //try // no owner for wormholes....
            //{
            //    if (fleet.Sector.Owner.Key == "BORG" || fleet.Sector.Owner.Key == null)
            //        return false;
            //}
            //catch { }
            //if (fleet.Sector.IsOwned && (fleet.Sector.Owner == fleet.Owner))
            //    return false;
            try
            {
                if (fleet.Sector.Owner != null && fleet.Sector.Owner.Key == "BORG")
                {
                    foreach (var ship in fleet.Ships)
                    {
                        if (ship.ShipType == ShipType.Spy)
                            return true;
                    }
                }
            }
            catch { }
            return false;
        }

        protected internal override void OnTurnBeginning()
        {
            base.OnTurnBeginning();
            if (_isComplete)
                return;
            var sabotageShip = FindBestSabotageShip();
            if (sabotageShip == null)
                return;
            CreateSabotage(
                Fleet.Owner,
                Fleet.Sector.System);
            GameContext.Current.Universe.Destroy(sabotageShip);
        }

        protected internal override void OnOrderAssigned()
        {
            base.OnOrderAssigned();
            if (!Fleet.Route.IsEmpty)
                Fleet.Route = TravelRoute.Empty;
        }

        private static void CreateSabotage(Civilization civ, StarSystem system)
        {
            //var sabotagedCiv = GameContext.Current.CivilizationManagers[system.Owner].Colonies;
            var civManager = GameContext.Current.CivilizationManagers[civ.Key];
            int ratioLevel = 1;

            int defenseIntelligence = GameContext.Current.CivilizationManagers[system.Owner].TotalIntelligenceProduction + 1;  // TotalIntelligence of attacked civ
            if (defenseIntelligence - 1 < 0.1)
                defenseIntelligence = 2;

            int attackingIntelligence = GameContext.Current.CivilizationManagers[civ].TotalIntelligenceProduction + 1;  // TotalIntelligence of attacked civ
            if (attackingIntelligence - 1 < 0.1)
                attackingIntelligence = 1;

            int ratio = attackingIntelligence / defenseIntelligence;
            //max ratio for no exceeding gaining points
            if (ratio > 10)
                ratio = 10;

            GameLog.Core.Intel.DebugFormat("owner= {0}, system= {1} is SABOTAGED by civ= {2} (Intelligence: defense={3}, attack={4}, ratio={5})",
                system.Owner, system.Name, civ.Name, defenseIntelligence, attackingIntelligence, ratio);

            GameLog.Core.Intel.DebugFormat("Owner= {0}, system= {1} at {2} (sabotaged): Energy={3} out of facilities={4}, in total={5}",
                system.Owner, system.Name, system.Location,
                system.Colony.GetEnergyUsage(),
                system.Colony.GetActiveFacilities(ProductionCategory.Energy),
                system.Colony.GetTotalFacilities(ProductionCategory.Energy));
            GameLog.Core.Intel.DebugFormat("{0}: TotalEnergyFacilities before={1}",
                system.Name, system.Colony.GetTotalFacilities(ProductionCategory.Energy));

            //Effect of sabatoge
            int removeEnergyFacilities = 0;
            if (system.Colony.GetTotalFacilities(ProductionCategory.Energy) > 1 && ratio > 1)// Energy: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeEnergyFacilities = 1;
                system.Colony.RemoveFacilities(ProductionCategory.Energy, 1);
                ratioLevel = 1;
            }

            // if ratio > 2 than remove one more  EnergyFacility
            if (system.Colony.GetTotalFacilities(ProductionCategory.Energy) > 2 && ratio > 2)// Energy: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeEnergyFacilities = 3;  //  2 and one from before
                system.Colony.RemoveFacilities(ProductionCategory.Energy, 2);
                ratioLevel = 2;
            }

            // if ratio > 3 than remove one more  EnergyFacility
            if (system.Colony.GetTotalFacilities(ProductionCategory.Energy) > 3 && ratio > 3)// Energy: remaining everything down to 1, for ratio: first value > 1 is 2, so ratio must be 2 or more
            {
                removeEnergyFacilities = 5;  //   3 and 3 from before = 6 in total , max 6 should be enough for one sabotage ship
                system.Colony.RemoveFacilities(ProductionCategory.Energy, 3);
                ratioLevel = 3;
            }

            if (system.Colony.GetTotalFacilities(ProductionCategory.Energy) > 0)
            {
                var attackedCivManager = GameContext.Current.CivilizationManagers[system.Owner];
                attackedCivManager.SitRepEntries.Add(new NewSabotagedSitRepEntry(
                       system.Owner, civ, system.Colony, ProductionCategory.Energy.ToString(), removeEnergyFacilities, system.Colony.GetTotalFacilities(ProductionCategory.Energy), civ.ShortName, ratioLevel));

                civManager.SitRepEntries.Add(new NewSabotagingSitRepEntry(
                        civ, system.Owner, system.Colony, ProductionCategory.Energy.ToString(), removeEnergyFacilities, system.Colony.GetTotalFacilities(ProductionCategory.Energy), civ.ShortName, ratioLevel));
            }
            //GameLog.Core.Intel.DebugFormat("{0}: TotalEnergyFacilities after={1}", system.Name, system.Colony.GetTotalFacilities(ProductionCategory.Energy));
            //civManager.SitRepEntries.Add(new NewSabotageFromShipSitRepEntry(civ, system.Colony, removeEnergyFacilities, system.Colony.GetTotalFacilities(ProductionCategory.Energy)));

        }
    }

    #endregion

    #region InfluenceOrder

    [Serializable]
    // Diplomatic mission ... by sending a diplomatic ship, treaties are easier to make in DiplomaticScreen
    // positive: ...increasing Regard + Trust
    // negative: ...exit membership from foreign empire
    // positive to your systems, colonies: increasing morale earth first
    public sealed class InfluenceOrder : FleetOrder  
    {
        private readonly bool _isComplete;

        public override string OrderName => ResourceManager.GetString("FLEET_ORDER_INFLUENCE");

        public override string Status => ResourceManager.GetString("FLEET_ORDER_INFLUENCE");

        public override FleetOrder Create()
        {
            return new InfluenceOrder();
        }

        public override bool IsComplete => _isComplete;

        public override bool IsCancelledOnRouteChange => true;

        public override bool IsRouteCancelledOnAssign => true;

        public override bool WillEngageHostiles => false;

        public InfluenceOrder()
        {
            _isComplete = false;
        }

        private Ship FindBestInfluenceShip()
        {
            Ship bestShip = null;
            foreach (Ship ship in Fleet.Ships)
            {
                if (ship.ShipType == ShipType.Diplomatic)  
                {
                    if ((bestShip == null)
                        || (ship.ShipDesign.WorkCapacity > bestShip.ShipDesign.WorkCapacity))
                    {
                        bestShip = ship;
                    }
                }
            }
            return bestShip;
        }

        public override bool IsValidOrder(Fleet fleet)
        {
            if (!base.IsValidOrder(fleet))
                return false;
            if (fleet.Sector.System == null)
                return false;
            if (!fleet.Sector.System.HasColony)
                return false;
            if (!fleet.Ships.Any(s => s.ShipType == ShipType.Diplomatic))
                return false;
            if (fleet.Sector.System.Owner.Key == "BORG")
                return false;
            return true;
        }

        protected internal override void OnTurnBeginning()
        {
            base.OnTurnBeginning();
            if (_isComplete)
                return;
            var _influenceShip = FindBestInfluenceShip();
            if (_influenceShip == null)
                return;

            var influencedCiv = GameContext.Current.CivilizationManagers[Fleet.Sector.System.Owner];
            var influencerCiv = GameContext.Current.CivilizationManagers[Fleet.Owner];

            // plan is: 
            // - maxValue for Trust = 1000 .... increasing a little bit quicker than Regard
            // - maxValue for Regard= 1000 .... from Regard treaties are affected (see \Resources\Data\DiplomacyTables.txt Line 1 RegardLevels

            // part 1: increase morale at own colony  // not above 95 so it's just for bad morale (population in bad mood)
            if (Fleet.Sector.System.Owner == Fleet.Owner)
            {
                GameLog.Core.Diplomacy.DebugFormat("{0} is influencing their colony at {1}",
                    Fleet.Owner, Fleet.Sector.System.Name);
                if (Fleet.Sector.System.Colony.Morale.CurrentValue < 95)
                {
                    Fleet.Sector.System.Colony.Morale.AdjustCurrent(+3);
                    Fleet.Sector.System.Colony.Morale.UpdateAndReset();
                    GameLog.Core.Diplomacy.DebugFormat("{0} successfully increased the morale at {1}",
                        influencerCiv, Fleet.Sector.System.Name);
                }
                return;
            }
            // part 2: to AI race
            if (!Fleet.Sector.System.Owner.IsHuman) 
            {
                var diplomat = Diplomat.Get(Fleet.Sector.System.Owner);
                var foreignPower = diplomat.GetForeignPower(Fleet.Owner);
                DiplomacyHelper.ApplyRegardChange(influencerCiv.Civilization, influencedCiv.Civilization, +55);
                //foreignPower.AddRegardEvent(new RegardEvent(30, RegardEventType.DiplomaticShip, +50));
                DiplomacyHelper.ApplyTrustChange(influencerCiv.Civilization, influencedCiv.Civilization, +50);
                GameLog.Core.Diplomacy.DebugFormat("{0} is attempting to influence the {1} at {2} regard ={3} trust ={4}",
                       influencerCiv, influencedCiv, Fleet.Sector.System,
                       foreignPower.DiplomacyData.Regard.CurrentValue, foreignPower.DiplomacyData.Trust.CurrentValue);
            }
        }

        protected internal override void OnOrderAssigned()
        {
            base.OnOrderAssigned();
            if (!Fleet.Route.IsEmpty)
                Fleet.Route = TravelRoute.Empty;
        }

    }

    #endregion

    #region TowOrder

    [Serializable]
    public sealed class TowOrder : FleetOrder
    {
        private int _targetFleetId = -1;
        private bool _shipsLocked;
        private bool _orderLocked;
        private FleetOrder _lastOrder;

        public override object Target
        {
            get { return TargetFleet; }
            set
            {
                if (value == null)
                    TargetFleet = null;
                if (value is Fleet)
                    TargetFleet = value as Fleet;
                else
                    throw new ArgumentException("Target must be of type Supremacy.Orbitals.Fleet");
                OnPropertyChanged("Target");
            }
        }

        public Fleet TargetFleet
        {
            get { return GameContext.Current.Universe.Objects[_targetFleetId] as Fleet; }
            private set
            {
                var currentTarget = TargetFleet;
                if (currentTarget != null)
                    EndTow();
                if (value == null)
                    _targetFleetId = -1;
                else
                    _targetFleetId = value.ObjectID;
                OnPropertyChanged("TargetFleet");
            }
        }

        public override string OrderName => ResourceManager.GetString("FLEET_ORDER_TOW");

        public override string Status => String.Format(
                    ResourceManager.GetString("FLEET_ORDER_STATUS_TOW"),
                    TargetFleet);

        public override string DisplayText
        {
            get
            {
                if (!Fleet.Route.IsEmpty)
                {
                    int turns = Fleet.Route.Length / Fleet.Speed;
                    string formatString;
                    if ((Fleet.Route.Length % Fleet.Speed) != 0)
                        turns++;
                    if (turns == 1)
                        formatString = ResourceManager.GetString("ORDER_ETA_TURN_MULTILINE");
                    else
                        formatString = ResourceManager.GetString("ORDER_ETA_TURNS_MULTILINE");
                    return String.Format(formatString, Status, turns);
                }
                return Status;
            }
        }

        public override bool WillEngageHostiles => false;

        public override bool IsComplete
        {
            get
            {
                var targetFleet = TargetFleet;
                return (targetFleet != null) && targetFleet.IsInTow && !targetFleet.IsStranded && Fleet.Route.IsEmpty;
            }
        }

        public override FleetOrder Create()
        {
            return new TowOrder();
        }

        public override bool IsTargetRequired(Fleet fleet)
        {
            return true;
        }

        private void BeginTow()
        {
            if (TargetFleet.IsInTow)
                return;

            TargetFleet.IsInTow = true;

            if (!TargetFleet.Order.IsCancelledOnRouteChange)
            {
                _lastOrder = TargetFleet.Order;
                _orderLocked = TargetFleet.IsOrderLocked;
            }

            _shipsLocked = TargetFleet.AreShipsLocked;

            TargetFleet.LockShips();

            if (_orderLocked)
                TargetFleet.UnlockOrder();

            TargetFleet.SetOrder(FleetOrders.AvoidOrder.Create());
            TargetFleet.LockOrder();

            if (TargetFleet.IsRouteLocked)
                TargetFleet.UnlockRoute();

            TargetFleet.SetRoute(TravelRoute.Empty);
            TargetFleet.LockRoute();

            Fleet.LockShips();
        }

        private void EndTow()
        {
            if (!TargetFleet.IsInTow)
                return;

            TargetFleet.UnlockOrder();
            TargetFleet.UnlockRoute();

            if (_lastOrder != null)
                TargetFleet.SetOrder(_lastOrder);
            else
                TargetFleet.SetOrder(TargetFleet.GetDefaultOrder());

            if (_orderLocked)
                TargetFleet.LockOrder();
            if (!_shipsLocked)
                TargetFleet.UnlockShips();

            TargetFleet.IsInTow = false;

            Fleet.UnlockShips();
        }

        protected internal override void OnOrderAssigned()
        {
            base.OnOrderAssigned();
            if (TargetFleet != null)
                BeginTow();
        }

        protected internal override void OnOrderCancelled()
        {
            if (TargetFleet != null)
                EndTow();
            base.OnOrderCancelled();
        }

        protected internal override void OnOrderCompleted()
        {
            if (TargetFleet != null)
                EndTow();
            base.OnOrderCompleted();
        }

        protected internal override void OnTurnBeginning()
        {
            base.OnTurnBeginning();

            var targetFleet = TargetFleet;
            if ((targetFleet != null) && targetFleet.IsInTow)
                TargetFleet.SetRoute(TravelRoute.Empty);
        }

        protected internal override void OnTurnEnding()
        {
            base.OnTurnEnding();

            var targetFleet = TargetFleet;
            var civManager = GameContext.Current.CivilizationManagers[Fleet.OwnerID];

            if (targetFleet != null)
            {
                var ship = targetFleet.Ships.SingleOrDefault();
                if ((ship != null) && (!FleetHelper.IsFleetInFuelRange(targetFleet)))
                {
                    int fuelNeeded = ship.FuelReserve.Maximum - ship.FuelReserve.CurrentValue;
                    ship.FuelReserve.AdjustCurrent(civManager.Resources[ResourceType.Deuterium].AdjustCurrent(-fuelNeeded));
                }
            }

            if (IsComplete)
                Fleet.SetOrder(Fleet.GetDefaultOrder());
        }

        public override void OnFleetMoved()
        {
            base.OnFleetMoved();
            if (TargetFleet != null)
                TargetFleet.Location = Fleet.Location;
        }

        public override bool IsValidOrder(Fleet fleet)
        {
            if (!base.IsValidOrder(fleet))
                return false;
            if (fleet.Ships.Count != 1)
                return false;
            if (fleet == Fleet && fleet.IsStranded)
                return false;
            return true;
        }

        public override IEnumerable<object> FindTargets(Fleet source)
        {
            var targets = new List<Object>();
            foreach (var targetFleet in GameContext.Current.Universe.FindAt<Fleet>(source.Location))
            {
                if ((targetFleet != source)
                    && (targetFleet.Owner == source.Owner)
                    && (targetFleet.Ships.Count == 1)
                    && targetFleet.IsStranded)
                {
                    targets.Add(targetFleet);
                }
            }
            return targets;
        }
    }

    #endregion

    #region Wormhole Order


    [Serializable]
    public sealed class WormholeOrder : FleetOrder
    {
        private MapLocation _startingLocation;

        public override string OrderName => ResourceManager.GetString("FLEET_ORDER_ENTER_WORMHOLE");

        public override string Status => String.Format(
                    ResourceManager.GetString("FLEET_ORDER_ENTER_WORMHOLE"),
                    Fleet);

        public override string DisplayText => String.Format(
                    ResourceManager.GetString("ORDER_ENTER_WORMHOLE"),
                    Status);

        public override bool WillEngageHostiles => false;

        public override bool IsComplete => Fleet.Location != _startingLocation;

        public override FleetOrder Create()
        {
            return new WormholeOrder();
        }

        public override bool IsTargetRequired(Fleet fleet)
        {
            return false;
        }

        protected internal override void OnOrderAssigned()
        {
            base.OnOrderAssigned();
            if (Fleet != null)
                _startingLocation = Fleet.Location;
        }


        protected internal override void OnTurnEnding()
        {

            if (Fleet != null)
            {
                //Wormhole leads nowhere so destroy the fleet
                if (Fleet.Sector.System.WormholeDestination == null)
                {
                    var civManager = GameContext.Current.CivilizationManagers[Fleet.OwnerID];
                    GameLog.Core.General.DebugFormat("Fleet {0} destroyed by wormhole at {1}", Fleet.ObjectID, Fleet.Location);
                    civManager.SitRepEntries.Add(new ShipDestroyedInWormholeSitRepEntry(Fleet.Owner, Fleet.Location));
                    Fleet.Destroy();
                }
                else
                {
                    Fleet.Location = (MapLocation)Fleet.Sector.System.WormholeDestination;
                    GameLog.Core.General.DebugFormat("Fleet {0} entered wormhole at {1} and was moved to {2}", Fleet.ObjectID, _startingLocation, Fleet.Location);

                    if (IsComplete)
                        Fleet.SetOrder(Fleet.GetDefaultOrder());
                }
            }
        }

        public override bool IsValidOrder(Fleet fleet)
        {

            if (fleet.Sector.System != null && fleet.Sector.System.StarType == StarType.Wormhole)
                return true;

            return false;
        }

        public override bool IsCancelledOnRouteChange => true;

        public override bool IsRouteCancelledOnAssign => true;
    }
    #endregion

    #region Collect Deuterium Order

    [Serializable]
    public sealed class CollectDeuteriumOrder : FleetOrder
    {
        private int _turnsCollecting;

        public override string OrderName => ResourceManager.GetString("FLEET_ORDER_COLLECT_DEUTERIUM");

        public override string Status => ResourceManager.GetString("FLEET_ORDER_COLLECT_DEUTERIUM");

        public override bool IsCancelledOnRouteChange => true;

        public override bool IsRouteCancelledOnAssign => true;

        public override bool WillEngageHostiles => false;

        public override FleetOrder Create()
        {
            return new CollectDeuteriumOrder();
        }

        public override bool IsValidOrder(Fleet fleet)
        {
            if (!base.IsValidOrder(fleet))
                return false;
            if (!FleetHelper.IsFleetInFuelRange(fleet))
            {
                bool needsFuel = false;
                foreach (var ship in fleet.Ships)
                {
                    if (ship.FuelReserve.IsMaximized)
                        continue;
                    needsFuel = true;
                    break;
                }
                if (needsFuel)
                {
                    var system = fleet.Sector.System;
                    if (system != null)
                        return ((system.StarType == StarType.Nebula) || system.ContainsPlanetType(PlanetType.GasGiant));
                }
            }
            return false;
        }

        protected internal override void OnTurnBeginning()
        {
            base.OnTurnBeginning();

            if ((++_turnsCollecting % 2) != 0)
                return;

            foreach (var ship in Fleet.Ships)
                ship.FuelReserve.AdjustCurrent(1);
        }
    }

    #endregion

    #region Build Station Order

    [Serializable]
    public sealed class BuildStationOrder : FleetOrder
    {
        private bool _finished;
        private StationBuildProject _buildProject;

        public StationDesign StationDesign => BuildProject.BuildDesign as StationDesign;

        public override string OrderName => ResourceManager.GetString("FLEET_ORDER_BUILD_STATION");

        public override string Status => String.Format(
                    ResourceManager.GetString("FLEET_ORDER_STATUS_BUILD_STATION"),
                    ResourceManager.GetString(_buildProject.StationDesign.Name));

        public override string TargetDisplayMember => "BuildDesign.LocalizedName";

        public override object Target
        {
            get { return BuildProject; }
            set { BuildProject = value as StationBuildProject; }
        }

        public StationBuildProject BuildProject
        {
            get
            {
                return _buildProject;
            }
            set
            {
                _buildProject = value;
            }
        }

        public override Percentage? PercentComplete
        {
            get
            {
                if (BuildProject != null)
                    return BuildProject.PercentComplete;
                return null;
            }
        }

        public override bool IsCancelledOnRouteChange => true;

        public override bool IsRouteCancelledOnAssign => true;

        public override bool IsCancelledOnMove => true;

        public override bool IsComplete => (BuildProject != null) && BuildProject.IsCompleted;

        public override FleetOrder Create()
        {
            return new BuildStationOrder();
        }

        public override IEnumerable<object> FindTargets([NotNull] Fleet source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var designs = new List<StationDesign>();
            var targets = new List<object>();
            var civManager = GameContext.Current.CivilizationManagers[source.Owner];

            if (civManager == null)
            {
                GameLog.Core.General.WarnFormat(
                    "Failed to load CivilizationManager for fleet owner (fleet ID = {0}, owner ID = {1})",
                    source.ObjectID,
                    (source.Owner != null) ? source.Owner.ShortName : source.OwnerID.ToString());
                return targets;
            }

            foreach (var stationDesign in civManager.TechTree.StationDesigns)
            {
                if (TechTreeHelper.MeetsTechLevels(civManager, stationDesign))
                    designs.Add(stationDesign);
            }

            for (int i = 0; i < designs.Count; i++)
            {
                for (int j = 0; j < designs.Count; j++)
                {
                    if (i == j)
                        continue;
                    foreach (var obsoleteDesign in designs[i].ObsoletedDesigns)
                    {
                        if (obsoleteDesign != designs[j])
                            continue;
                        designs.RemoveAt(j);
                        if (i > j)
                            i--;
                        j--;
                    }
                }
            }

            foreach (var design in designs)
            {
                targets.Add(new StationBuildProject(new FleetProductionCenter(source), design));
                //GameLog.Core.Stations.DebugFormat("{0} {1} at {2} is building a {3}", source.ObjectID, source.Name, source.Location, design);
            }

            return targets;
        }

        public override bool IsValidOrder(Fleet fleet)
        {
            if (fleet.Sector.Station != null)
                return false;
            if (fleet.Sector.IsOwned && (fleet.Sector.Owner != fleet.Owner))
                return false;
            foreach (var ship in fleet.Ships)
            {
                if (ship.ShipType == ShipType.Construction)
                {
                    return true;
                }

            }
            return false;
        }

        public override bool CanAssignOrder(Fleet fleet)
        {
            if (!IsValidOrder(fleet))
                return false;

            // if build order already set, can't assign it again
            if (fleet.Order is BuildStationOrder)
                return false;

            // can't start building if any other ship is already building an outpost
            foreach (var otherFleet in GameContext.Current.Universe.FindAt<Fleet>(fleet.Location))
            {
                if ((otherFleet != fleet) && (otherFleet.Order is BuildStationOrder))
                    return false;
            }

            // needs to be a construction ship
            foreach (var ship in fleet.Ships)
            {
                if (ship.ShipType == ShipType.Construction)
                    return true;
            }

            return false;
        }

        public override bool IsTargetRequired(Fleet fleet)
        {
            return true;
        }

        protected internal override void OnTurnBeginning()
        {
            base.OnTurnBeginning();

            if (!IsAssigned)
                return;

            var project = _buildProject;
            GameLog.Core.Production.DebugFormat("project: Builder = {2}, BuildDesign = {1}, Description = {0} ", project.Description, project.BuildDesign, project.Builder);
            if ((project == null) || (project.ProductionCenter == null) || project.IsCompleted)
                return;

            var civManager = GameContext.Current.CivilizationManagers[project.Builder];
            if (civManager == null)
            {
                var owner = project.ProductionCenter.Owner;
                GameLog.Core.General.WarnFormat(
                    "Failed to load CivilizationManager for build project owner (build project ID = {0}, owner ID = {1})",
                    project.ProductionCenter.ObjectID,
                    (owner != null) ? owner.ShortName : "null");
                return;
            }

            var buildOutput = project.ProductionCenter.GetBuildOutput(0);
            var resources = new ResourceValueCollection
            {
                [ResourceType.RawMaterials] = civManager.Resources[ResourceType.RawMaterials].CurrentValue
            };

            var usedResources = resources.Clone();

            project.Advance(ref buildOutput, usedResources);

            //RawMaterialsBefore = usedResources[ResourceType.RawMaterials] - resources[ResourceType.RawMaterials];

            GameLog.Core.Production.DebugFormat("project: Builder = {0}, BuildDesign = {1}, Duranium before {2}, AdjustValue = {3}", project.Builder
                , project.BuildDesign, civManager.Resources[ResourceType.RawMaterials].CurrentValue
                , usedResources[ResourceType.RawMaterials] - resources[ResourceType.RawMaterials]);
            
            civManager.Resources[ResourceType.RawMaterials].AdjustCurrent(
                usedResources[ResourceType.RawMaterials] - resources[ResourceType.RawMaterials]);

        }

        protected internal override void OnOrderCompleted()
        {
            base.OnOrderCompleted();

            if (!_finished && (BuildProject != null))
            {
                BuildProject.Finish();
                _finished = true;
            }
           ;
            var destroyedShip = Fleet.Ships.FirstOrDefault(o => o.ShipType == ShipType.Construction);
            if (destroyedShip != null)
                GameContext.Current.Universe.Destroy(destroyedShip);
            GameLog.Core.Stations.DebugFormat("Destroyed = {0}", destroyedShip);
        }

        public override void OnFleetMoved()
        {
            base.OnFleetMoved();
            if (BuildProject != null)
                BuildProject.Cancel();
        }

        #region FleetProductionCenter Class

        internal class FleetProductionCenter : IProductionCenter
        {
            private readonly int _fleetId;
            private readonly BuildSlot _buildSlot;

            // ReSharper disable SuggestBaseTypeForParameter
            public FleetProductionCenter(Fleet fleet)
            {
                if (fleet == null)
                    throw new ArgumentNullException("fleet");
                _fleetId = fleet.ObjectID;
                _buildSlot = new BuildSlot();
            }

            // ReSharper restore SuggestBaseTypeForParameter

            public Fleet Fleet => GameContext.Current.Universe.Objects[_fleetId] as Fleet;

            #region IProductionCenter Members

            public IIndexedEnumerable<BuildSlot> BuildSlots => IndexedEnumerable.Single(_buildSlot);

            public int GetBuildOutput(int slot)
            {
                return Fleet.Ships.Where(o => o.ShipType == ShipType.Construction).Sum(o => o.ShipDesign.WorkCapacity);
            }

            public IList<BuildQueueItem> BuildQueue => new ReadOnlyCollection<BuildQueueItem>(new List<BuildQueueItem>());

            public void ProcessQueue() { }

            #endregion

            #region IUniverseObject Members

            public int ObjectID => Fleet.ObjectID;

            public MapLocation Location => Fleet.Location;

            public int OwnerID => Fleet.OwnerID;

            public Civilization Owner => Fleet.Owner;

            #endregion
        }

        #endregion
    }

    #endregion

    #region Explore Order

    [Serializable]
    public sealed class ExploreOrder : FleetOrder
    {
        public override string OrderName => ResourceManager.GetString("FLEET_ORDER_EXPLORE");

        public override string Status => ResourceManager.GetString("FLEET_ORDER_STATUS_EXPLORE");

        public override bool WillEngageHostiles => false;

        public override bool IsCancelledOnRouteChange => true;

        public override FleetOrder Create()
        {
            return new ExploreOrder();
        }

        protected internal override void OnTurnBeginning()
        {
            base.OnTurnBeginning();
            if (!IsAssigned)
                return;
            if (Fleet.Route.IsEmpty && (Fleet.UnitAIType != UnitAIType.SystemAttack || Fleet.UnitAIType != UnitAIType.Reserve))
            {
                if (UnitAI.GetBestSectorToExplore(Fleet, out Sector bestSector))
                {
                    Fleet.SetRouteInternal(AStar.FindPath(Fleet, PathOptions.SafeTerritory, null, new List<Sector> { bestSector }));
                    Fleet.UnitAIType = UnitAIType.Explorer;
                    Fleet.Activity = UnitActivity.Mission;
                }
            }
        }
    }

    #endregion
}
