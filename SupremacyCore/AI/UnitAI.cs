// UnitAI.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

//using Obtics.Collections;
using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Diplomacy;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Intelligence;
using Supremacy.Orbitals;
using Supremacy.Pathfinding;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Supremacy.AI
{
    public static class UnitAI
    {
        //#pragma warning disable IDE0044 // Add readonly modifier
        private readonly static IEnumerable<Sector> _deathStars = GameContext.Current.Universe.FindStarType<Sector>(StarType.BlackHole).ToList()
            .Concat(GameContext.Current.Universe.FindStarType<Sector>(StarType.NeutronStar).ToList());
        //#pragma warning restore IDE0044 // Add readonly modifier

        [NonSerialized]
        private static string _text;
        private static string _designText;
        private static string _fleet_Text;
        private static string _fleetText = "";
        private static bool writeDirectly = true;
        private static bool _checkFleetOrders;
        private static bool _checkOnlyPlayersUnits = true;
        private static bool checkShips_Colony;
        private static bool checkShips_Construction;
        private static bool checkShips_Medical;
        private static bool checkShips_Transport;
        private static bool checkShips_Spy;
        private static bool checkShips_Diplomatic;
        private static bool checkShips_Science;
        private static bool checkShips_Scout;
        private static bool checkShips_Battle;


        //private static string _shipText;
        private static readonly string blank = " ";
        private static readonly string newline = Environment.NewLine;

        public static void DoTurn([NotNull] Civilization civ)
        {
            StarSystem homeSystem = GameContext.Current.CivilizationManagers[civ].HomeSystem;
            //MapLocation rendezvousSystem = GameContext.Current.CivilizationManagers[civ].RendezvousLocation;
            Sector rendezvousSector = homeSystem.Sector;
            //rendezvousSector.Location = GameContext.Current.CivilizationManagers[civ].RendezvousLocation.
            _fleetText = "" + _fleetText;
            try
            {

                _text = "Step_1102:; ##################### UnitAI.DoTurn begins...for "
                    + civ.Key + " ##################### ";
                if (writeDirectly) Console.WriteLine(_text);
                _text = _designText;// dummy - please keep

                if (civ == null) { throw new ArgumentNullException(nameof(civ)); }

                if (civ.TargetCivilization != null) // Just for info
                {
                    _text = "Step_6260:; UnitAI-DoTurn; "
                        + "TargetCiv (not null !!) > " + civ.TargetCivilization + " "
                        ;
                    if (writeDirectly) Console.WriteLine(_text);
                }


                if (civ.CivID < 999) // unit AI only for empires >> 999 is 'for all', below '7' is just Empires
                {
                    //_text = "Step_3102:; ##########################################   UnitAI for Empires or as well for minors...";
                    //if (writeDirectly) Console.WriteLine(_text);


                    List<Fleet> _all_Fleets_of_Civ = GameContext.Current.Universe.FindOwned<Fleet>(civ).ToList();
                    // finds also fleets with ID -1 ... maybe Colonizer which disappered and just deleted in next round?

                    // in case of ..
                    //List<Ship> allAttackWarShips = new List<Ship>();
                    //if (true)
                    //{
                    //foreach (Fleet civFleet in _all_Fleets_of_Civ)
                    //{
                        //foreach (Ship ship in civFleet.Ships.Where(s => s.ShipType >= ShipType.Scout || s.ShipType == ShipType.Transport).ToList())
                        //{
                        //    if (civFleet.OwnerID != -1)
                        //    {
                        //        allAttackWarShips.Add(ship);
                        //        CreateShipText(ship, out string _shipText);
                        //        if (writeDirectly) Console.WriteLine("Step_3103:; in case of .. added to allAttackWarShips; " + _shipText);
                        //        // GameLog.Client.AIDetails.DebugFormat("A ship all attack ships {0} location ={1}", ship.Name, ship.Location );
                        //    }
                        //}




                        //_text = "--------------------";
                        //if (writeDirectly) Console.WriteLine(_text);

                        // **** The UnitAI fleet by fleet looping 
                        foreach (Fleet fleet in _all_Fleets_of_Civ) // each fleet of the current civ
                        {
                            _fleet_Text = ""; // collects all Console_Textes 
                            _fleetText = ""; // dummy - please keep

                            if (fleet.ObjectID == -1)
                                continue;

                            bool _rendezvousAble = true;
                            if (writeDirectly) Console.WriteLine("Step_7888:; " + CreateFleetText(fleet, out _fleetText) + " > Order= " + fleet.Order);

                            switch (fleet.Order.OrderName)
                            {

                                //case FleetOrders.IdleOrder.OrderName:
                                case "On Idle Status":
                                case "Engage":
                                    _rendezvousAble = true;
                                    break;
                                case "On The Way":
                                default:
                                    _rendezvousAble = false;
                                    break;
                            }

                            //switch

                            if (fleet.IsColonizer) _rendezvousAble = false;
                            if (fleet.IsScout) _rendezvousAble = false;
                            if (fleet.IsConstructor) _rendezvousAble = false;
                            if (fleet.IsMedical) _rendezvousAble = false;
                            if (fleet.IsSpy) _rendezvousAble = false;
                            if (fleet.IsDiplomatic) _rendezvousAble = false;

                            if (fleet.IsScience) _rendezvousAble = false;
                            //{
                            //    _rendezvousAble = false;
                            //}

                            try { _designText = fleet.Ships[0].Design.ToString(); }
                            catch
                            {
                                string _Text = fleet.Location + " > Fleet: " + fleet.ObjectID + blank + fleet.Name
                                    + blank + fleet.Ships[0].Design.ToString() + blank + fleet.Owner + blank + ", UnitAIType=> ** " + fleet.UnitAIType /*+ ", TargetCiv=NULL" + fleet.Owner.TargetCivilization*/
                                    + ", Order= " + fleet.Order.ToString();
                                if (writeDirectly) Console.WriteLine("Step_6149:; " + _Text);
                                _fleet_Text += newline + _text;
                            }

                            string _fleetOrderText = "NoOrder";
                            if (fleet.Order != null)
                                _fleetOrderText = fleet.Order.ToString();


                            CreateFleetText(fleet, out _fleetText);
                            //_fleetText = fleet.Location
                            //    + " > " + fleet.Ships[0].Design.ToString()
                            //    + " > " + fleet.ObjectID + ":  " + fleet.Name

                            //    //+ blank + fleet.Owner
                            //    + blank + ", UnitAIType=> ** " + fleet.UnitAIType /*+ ", TargetCiv=NULL" + fleet.Owner.TargetCivilization*/
                            //    + blank + ", Activity=> ** " + fleet.Activity /*+ ", TargetCiv=NULL" + fleet.Owner.TargetCivilization*/
                            //    + ", Order= ** " + _fleetOrderText;
                            if (writeDirectly) Console.WriteLine("Step_6151:; " + _fleetText);
                            //_fleet_Text += newline + _text;
                            // as well go to CTRL+F and 'checking fleets'

                            CloakAll(fleet);


                            // ****  a) Main stuff - we don't have a target civ > for b) some orders are changed
                            //if (civ.TargetCivilization == null)
                            //{
                                _text = "Step_6153:; " // Turn " + GameContext.Current.TurnNumber
                                    + "" + _fleetText
                                    + " ... ( no CIV to target > just usual business )"
                                    + ", _rendezvousAble= " + _rendezvousAble
                                    ;
                                if (writeDirectly) Console.WriteLine(_text);
                                _fleet_Text += /*newline + */_text;
                                // >Check Fleets

                                // just for testing
                                _checkOnlyPlayersUnits = false;
                                if (fleet.Owner.IsHuman)
                                {
                                    //continue;
                                    _checkFleetOrders = true;
                                    _checkOnlyPlayersUnits = true;
                                }

                            DoRendezvous(fleet);





                                // no more actions here if owner is human player
                                if (fleet.Owner.IsHuman)
                                {
                                    //continue;
                                }

                                //SELECT
                                checkShips_Construction = true;
                                if (fleet.IsConstructor) DoConstruction(fleet); // Constructor get the first "GetEscort"
                                checkShips_Colony = true;
                                if (fleet.IsColonizer) DoColony(fleet);
                                checkShips_Scout = true;
                                if (fleet.IsScout) DoScout(fleet);
                                checkShips_Medical = true;
                                if (fleet.IsMedical) DoMedical(fleet);
                                checkShips_Spy = true;
                                if (fleet.IsSpy) DoSpy(fleet);
                                checkShips_Diplomatic = true;
                                if (fleet.IsDiplomatic) DoDiplomatic(fleet);
                                checkShips_Science = true;
                                if (fleet.IsScience) DoScience(fleet);

                                checkShips_Battle = true;
                                if (fleet.IsCombatant 
                                    && !fleet.IsMedical
                                    && !fleet.IsDiplomatic
                                    && !fleet.IsColonizer
                                    && !fleet.IsConstructor
                                    ) 
                                        DoBattleShips(fleet); // inclusive TransportShips
                                                                             //checkShips_Transport = true;
                                                                             //DoTransportShips(fleet);

                        goto End_Ships;


                        // if Type=SystemAttack and (before) no TargetCiv => return to HomeSystem
                        if (fleet.UnitAIType == UnitAIType.SystemAttack) // call off attack
                                {
                                    _text = /*"UnitAI-DoTurn; "*/
                                        /*+ */"UnitAIType == NoUnitAI: " + _fleetText
                                        ;
                                    if (writeDirectly) Console.WriteLine(_text);
                                    _fleet_Text += newline + _text;

                                    List<Ship> shipList = fleet.Ships.ToList();
                                    if (shipList.Count() > 0)  //&& fleet.UnitAIType != UnitAIType.SystemDefense )
                                    {

                                        foreach (Ship ship in shipList)
                                        {
                                            if (ship.ShipType >= ShipType.Scout)
                                            {
                                                Fleet tempFleet = new Fleet(); // keep making a new 'tempFleet'
                                                fleet.RemoveShip(ship);
                                                tempFleet.AddShip(ship);
                                                tempFleet.Owner = fleet.Owner;
                                                tempFleet.UnitAIType = UnitAIType.NoUnitAI;
                                                tempFleet.Activity = UnitActivity.NoActivity;
                                                if (fleet.Location != homeSystem.Location)
                                                {
                                                    tempFleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { homeSystem.Sector }));
                                                }
                                                else
                                                {
                                                    tempFleet.Route.Clear();
                                                }
                                            }
                                        }
                                    }
                                } // end of if Type=SystemAttack and (before) no TargetCiv => return to HomeSystem
                                  // >Check Fleets

                                //else // if nothing special


                                //else 
                                if (GameContext.Current.TurnNumber > 0 &&    // before > 4 = why just from turn 5 on ?
                                 fleet.Ships.Count > 0 && // 2024-03-30
                                    !fleet.Ships.Any(o =>
                                    o.ShipType == ShipType.Colony
                                 || o.ShipType == ShipType.Construction
                                 || o.ShipType == ShipType.Medical
                                 || o.ShipType == ShipType.Science
                                 || o.ShipType == ShipType.Scout
                                 || o.ShipType == ShipType.Diplomatic
                                 || o.ShipType == ShipType.Spy)) // do not mess with esorted fleets or these ship types 
                                                                 // so any combat ship without the types above
                                {
                                    // if not in home sector AND fleet is busy in Reserve or Escort
                                    // than > Go to OLD: HomeSystem, NEW: RendezvousLocation

                                    if (_checkFleetOrders == true && _checkOnlyPlayersUnits)
                                        _text += ""; // dummy

                                    // so combatant ships
                                    if (/*fleet.Sector != ourHomeSystem.Sector &&*/
                                        (fleet.UnitAIType != UnitAIType.Reserve || fleet.Route.IsEmpty)) // escort left over after colonizing, construction...
                                    {
                                        fleet.Owner = civ;
                                        fleet.OwnerID = civ.CivID;
                                        fleet.UnitAIType = UnitAIType.Attack;
                                        fleet.Activity = UnitActivity.NoActivity;
                                        fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { rendezvousSector }));
                                        fleet.SetOrder(new ExploreOrder()); // 2024-03-30
                                        foreach (var ship in fleet.Ships)
                                        {
                                            CreateShipText(ship, out string _shipText);
                                            _text = "Step_6230:; " + _fleetText + " > Explore";
                                            if (writeDirectly) Console.WriteLine(_text);
                                            _fleet_Text += newline + _text;
                                        }

                                    }
                                    //GameLog.Core.AIDetails.DebugFormat("## NOT Total War, fleet = {0}, Ships ={1}, {2}, {3}, {4}, {5}", fleet.Ships[0].DesignName, fleet.Ships.Count(), fleet.Owner, fleet.Sector.Name, fleet.UnitAIType, fleet.Activity, fleet.Location);
                                    //if (fleet.Ships.Count() > 1)
                                    //{
                                    //    GameLog.Core.AIDetails.DebugFormat("## NOT Total War, first Ship = {0} {1}", fleet.Ships[0].Name, fleet.Ships[0].DesignName);
                                    //    GameLog.Core.AIDetails.DebugFormat("## NOT Total War, second Ship = {0} {1}", fleet.Ships[1].Name, fleet.Ships[1].DesignName);
                                    //}
                                }
                                //else 

                                // no RendezvousSector for Minor races > so they are not staying at home all the time
                                if (fleet.Owner.IsEmpire && _rendezvousAble && rendezvousSector != null && rendezvousSector.Location.ToString() != "(0, 0)")
                                {
                                    fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { rendezvousSector }));
                                    foreach (var ship in fleet.Ships)
                                    {
                                        CreateShipText(ship, out string _shipText);
                                        _text = "Step_6230:; " + _fleetText + " > Heading up to RendezvousSector= " + rendezvousSector.Location;
                                        if (writeDirectly) Console.WriteLine(_text);
                                        _fleet_Text += newline + _text;
                                    }
                                }
                                else
                                {
                                    foreach (var ship in fleet.Ships)
                                    {
                                        CreateShipText(ship, out string _shipText);
                                        _text = "Step_6234:; " + _fleetText + " > GOING NOWHERE ... e.g. RendezvousSector " + rendezvousSector.Location + " is not set";
                                        //if (writeDirectly) Console.WriteLine(_text);
                                        _fleet_Text += newline + _text;
                                    }
                                }
                        //else
                        //{
                        //    if (fleet.Ships.Where(o => o.ShipType == ShipType.Construction).Any())
                        //    {
                        //        GameLog.Core.AIDetails.DebugFormat("Target null Constructor fleet = {0}, Ships ={1}, {2}, {3}, {4}, {5}", fleet.Ships[0].DesignName, fleet.Ships.Count(), fleet.Owner, fleet.Sector.Name, fleet.UnitAIType, fleet.Activity, fleet.Location);
                        //        if (fleet.Ships.Count() > 1)
                        //        {
                        //            GameLog.Core.AIDetails.DebugFormat("Top-Target null Construct,first Ship {0} {1} route {2} Activity {3} UnitAI {4}", fleet.Ships[0].Name, fleet.Ships[0].DesignName, fleet.Route.Length, fleet.Activity, fleet.UnitAIType);
                        //            GameLog.Core.AIDetails.DebugFormat("Top-Target null Construct,second Ship {0} {1} route {2} Activity {3} UnitAI {4}", fleet.Ships[1].Name, fleet.Ships[1].DesignName, fleet.Route.Length, fleet.Activity, fleet.UnitAIType);
                        //        }
                        //    }
                        //}

                        //} // no a) civ.TargetCivilization == null


                        goto End_Ships;
                        // **** b) We have a target civilization
                        if (civ.TargetCivilization != null) // see Step_6260
                            {
                                Fleet attackFleet = new Fleet();

                                StarSystem othersHomeSystem = homeSystem; // same as home until there is a target civ
                                                                          // just report to Console + define othersHomeSystem
                                if (civ.TargetCivilization != null)
                                {
                                    othersHomeSystem = GameContext.Current.CivilizationManagers[civ.TargetCivilization].HomeSystem;

                                    _text = "Step_6150:; UnitAI = Fleets > "
                                        + homeSystem.Location
                                        + " " + civ.Name
                                        + " has TargetCivilization > " + civ.TargetCivilization.Name
                                        + " - HomeSystem at " + othersHomeSystem.Location;
                                    if (writeDirectly) Console.WriteLine(_text);
                                    _fleet_Text += newline + _text;
                                }

                                //fleet.UnitAIType = UnitAIType.SystemAttack; //2024-06-09

                                foreach (var ship in fleet.Ships)
                                {
                                    CreateShipText(ship, out string _shipText);
                                    _text = "Step_3279:; " + othersHomeSystem.Location 
                                        + " Ship in Fleet= "
                                                    //+ othersHomeSystem.Name + "; Owner= " + othersHomeSystem.Owner
                                                    //+ "; Ship= " + ship.ObjectID + "; Owner= " + ship.Name
                                                    //+ "; Ship= " + ship.Design + "; Owner= " + ship.Name
                                                    + _shipText
                                                    ;
                                    if (writeDirectly) Console.WriteLine(_text);
                                    _fleet_Text += newline + _text;
                                }


                            goto End_Ships;

                                // To remove
                            if (writeDirectly) Console.WriteLine(_text);
                            else if (fleet.Ships.Any(o => o.ShipType >= ShipType.Scout || o.ShipType == ShipType.Transport))// No systemattack fleet so make one
                                {
                                    //GameLog.Core.AIDetails.DebugFormat("Combat Ships ={0} with target ={1}", civ.Name, civ.TargetCivilization.Name);
                                    // non combat ships in a fleet with escort
                                    if (fleet.Ships.Count > 1 && fleet.Ships.Any(o => o.ShipType < ShipType.Transport))
                                    {
                                        // Break up escorted fleets to send combat ship home
                                        if (fleet.UnitAIType == UnitAIType.Colonizer)
                                        {
                                            RemoveEscortShips(fleet, ShipType.Colony);
                                            continue;
                                        }
                                        if (fleet.UnitAIType == UnitAIType.Constructor)
                                        {
                                            RemoveEscortShips(fleet, ShipType.Construction);
                                            continue;
                                        }

                                        CreateShipText(fleet.Ships[0], out string _shipText);
                                        // all other ships > UnitAIType.SystemAttack
                                        if (fleet.IsCombatant)
                                        {
                                            fleet.UnitAIType = UnitAIType.SystemAttack;
                                            _text = "Step_3265:; " + othersHomeSystem.Location + " Set to SystemAttack > "
                                                    + homeSystem.Name + ", Owner= " + homeSystem.Owner
                                                    + _shipText
                                                    //+ " > ordered one more facility for > Industry"
                                                    ;
                                            if (writeDirectly) Console.WriteLine(_text);
                                            _fleet_Text += newline + _text;
                                        }

                                    }
                                    else
                                    {
                                        CreateShipText(fleet.Ships[0], out string _shipText);
                                        // all other ships > UnitAIType.SystemAttack
                                        if (fleet.IsCombatant)
                                        {
                                            fleet.UnitAIType = UnitAIType.SystemAttack;
                                            _text = "Step_3266:; " + othersHomeSystem.Location + " Set to SystemAttack > "
                                                    + homeSystem.Name + ", Owner= " + homeSystem.Owner
                                                    + _shipText
                                                    //+ " > ordered one more facility for > Industry"
                                                    ;
                                            if (writeDirectly) Console.WriteLine(_text);
                                            _fleet_Text += newline + _text;
                                        }
                                    }
                                }


                                if (fleet.Location != homeSystem.Location && !fleet.Route.Waypoints.Contains(homeSystem.Location))
                                {
                                    if (fleet.IsScout)
                                    {
                                        // send scouts home
                                        fleet.Route.Clear(); // stop exloring
                                        fleet.SetOrder(new AvoidOrder());
                                        BuildAndSendFleet(fleet, civ, UnitActivity.Mission, UnitAIType.Reserve, homeSystem.Sector);
                                        continue;
                                        // GameLog.Core.AIDetails.DebugFormat("Ordering Scout & FastAttack {0} to explore from {1}", fleet.ClassName, fleet.Location);
                                    }
                                    else if (fleet.Ships.Count() > 1)
                                    {
                                        int numberofships = fleet.Ships.Count();
                                        //Fleet anotherFleet = new Fleet();
                                        _text = "Step_8789:; " + _fleetText + " > Fleet is to separate into single ships > check the fleet for crashes here "
                                                //+ _fleetText
                                                ;
                                        if (writeDirectly) Console.WriteLine(_text);
                                        _fleet_Text += newline + _text;

                                        //try
                                        //{
                                        foreach (Ship currentship in fleet.Ships)
                                        {
                                            //Ship currentship = fleet.Ships[i];
                                            //Fleet anotherFleet = new Fleet();
                                            if (currentship != null && currentship.IsCombatant)
                                            {
                                                Fleet anotherFleet = new Fleet();
                                                fleet.RemoveShip(currentship);
                                                anotherFleet.AddShip(currentship);
                                                BuildAndSendFleet(anotherFleet, civ, UnitActivity.Mission, UnitAIType.Reserve, homeSystem.Sector);
                                            }
                                        }
                                        //}
                                        //catch 
                                        //{
                                        //    _text = "Step_8789:; " + _fleetText + " > Fleet is to separate into single ships > check the fleet for crashes here "
                                        //            //+ _fleetText
                                        //            ;
                                        //    if (writeDirectly) Console.WriteLine(_text);
                                        //}
                                        //foreach (Ship ship in fleet.Ships) // is Colonizer still in fleet or if not > Crash
                                        //{
                                        //    if (ship.ShipType != ShipType.Colony || ship.ShipType != ShipType.Construction || ship.ShipType != ShipType.Medical)
                                        //    {
                                        //        fleet.RemoveShip(ship);
                                        //    }

                                        //    anotherFleet.AddShip(ship);
                                        //    BuildAndSendFleet(anotherFleet, civ, UnitActivity.Mission, UnitAIType.Reserve, ourHomeSystem.Sector);
                                        //    continue;
                                        //}
                                    }
                                }
                                else if (fleet.Sector == homeSystem.Sector)
                                {
                                    List<Ship> listOfShips = fleet.Ships.ToList();
                                    if (listOfShips.Count() > 0)
                                    {
                                        foreach (Ship ship in listOfShips)
                                        {
                                            if (ship.ShipType >= ShipType.Scout || ship.ShipType == ShipType.Transport)
                                            {
                                                if (homeSystem.Sector.GetOwnedFleets(civ).FirstOrDefault(o => o.UnitAIType == UnitAIType.SystemAttack) == null)
                                                //&& o.UnitAIType == UnitAIType.SystemAttack) == null)
                                                {
                                                    fleet.UnitAIType = UnitAIType.SystemAttack;
                                                    attackFleet = fleet;
                                                }
                                                else
                                                {
                                                    fleet.RemoveShip(ship);
                                                    attackFleet.AddShip(ship);
                                                    //GameLog.Core.AIDetails.DebugFormat("The {0} Attack Fleet adding ship ={1} attack fleet count ={2}"
                                                    //       , civ.Name, ship.Name, attackFleet.Ships.Count());
                                                    //foreach (Ship nextShip in attackFleet.Ships)
                                                    //{
                                                    //    GameLog.Core.AIDetails.DebugFormat("Added The ship ={0} {1}"
                                                    //        , nextShip.Name, nextShip.OrbitalDesign.ToString());
                                                    //}
                                                    //GameLog.Client.AIDetails.DebugFormat("All civ {0} ship count{1}", civ.Key, _all_Fleets_of_Civ.ToList().Count());
                                                    //foreach (var anotherFleet in _all_Fleets_of_Civ)
                                                    //{
                                                    //    foreach (Ship anotherShip in anotherFleet.Ships)
                                                    //    {
                                                    //        GameLog.Core.AIDetails.DebugFormat("All civ {0} ship ={1} {2}"
                                                    //            , civ.Name, anotherShip.Name, anotherShip.OrbitalDesign.ToString());
                                                    //    }
                                                    //}
                                                }
                                                attackFleet.Location = homeSystem.Location;
                                                attackFleet.UnitAIType = UnitAIType.SystemAttack;
                                                attackFleet.Activity = UnitActivity.Hold;
                                                attackFleet.Owner = civ;
                                                attackFleet.OwnerID = civ.CivID;
                                                attackFleet.SetOrder(new EngageOrder());
                                                continue;
                                                //GameLog.Core.AIDetails.DebugFormat("## Attackfleet Ship Count={0}, {1}, {2}, {3}, {4},"
                                                //    , attackFleet.Ships.Count, attackFleet.Name, attackFleet.Owner, attackFleet.UnitAIType.ToString(), attackFleet.Location);
                                            }
                                        }
                                    }
                                }



                            }


                            // **** The non-combat ships, targetciv null or not null
                            if (fleet.Ships.Count() > 0)
                            {
                                //_text = "Step_6240:; " + _fleetText + " > #### fleet.IsColonizer ";
                                if (writeDirectly) Console.WriteLine(_text);
                                //if (fleet.IsColonizer)// || fleet.UnitAIType == UnitAIType.Colonizer)
                                //{
                                //    _text = "Step_6301:; " + _fleetText + " > fleet.IsColonizer ";
                                //    if (writeDirectly) Console.WriteLine(_text);
                                //    _fleet_Text += newline + _text;

                            }
                    End_Ships:;
                    } // End of foreach ship = continues with the next

                    //}
                    // ** end of UnitAI ship by ship loop for civ
                    if (writeDirectly) Console.WriteLine(newline + "Fleet_Text: " + _fleet_Text);

                    if (_checkFleetOrders == true && _checkOnlyPlayersUnits)
                        _text += ""; // dummy

                    //End_Ships:;
                }
            }

            catch (Exception e)
            {
                _text = "Step_9778:; #### problem at UnitAI.cs" + newline + e.ToString();
                if (writeDirectly) Console.WriteLine(_text);
                _fleet_Text += newline + _text;
                if (writeDirectly) Console.WriteLine(_fleet_Text);
                GameLog.Core.General.Error(e);
            }
        }

        private static void DoRendezvous(Fleet fleet)
        {
            CivilizationManager civM = GameContext.Current.CivilizationManagers[fleet.Owner.CivID];
            Sector rendezvousSector = civM.RendezvousSector;

            if (rendezvousSector != null && rendezvousSector.Location.ToString() != "(0, 0)")
            {
                // Set_Order
                if (fleet.Order == FleetOrders.RendezvousLocation_Set_Here_Order)
                {
                    _text = "Step_6154:; Turn " + GameContext.Current.TurnNumber
                            + " > " + _fleetText
                            + " ... ( no CIV to target > just usual business )"
                            ;
                    if (writeDirectly) Console.WriteLine(_text);
                    _fleet_Text += newline + _text;

                    if (_checkFleetOrders == true && _checkOnlyPlayersUnits)
                        _text += ""; // dummy
                }


                // RendezvousLocation_Go_There_Order

                if (fleet.Order == FleetOrders.RendezvousLocation_Go_There_Order)
                //fleet.Order != FleetOrders.AssaultSystemOrder &&
                //fleet.Order != FleetOrders.BuildStationOrder &&
                //fleet.Order != FleetOrders.CollectDeuteriumOrder &&
                //fleet.Order != FleetOrders.ColonizeOrder &&
                //fleet.Order != FleetOrders. &&
                //)
                {
                    if (rendezvousSector != null && rendezvousSector.Location.ToString() != "(0, 0)")
                    {

                        fleet.UnlockRoute();
                        fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { rendezvousSector }));
                        fleet.SetOrder(new RendezvousLocation_Go_There_Order());
                        fleet.Activity = UnitActivity.NoActivity;

                        foreach (var ship in fleet.Ships)
                        {
                            CreateShipText(ship, out string shipText);
                            _text = "Step_6235:; " + _fleetText
                                + " > going to RendezvousSector= " + rendezvousSector.Location
                                + " , Home= " + civM.HomeSystem.Location /*+ " )"*/;
                            if (writeDirectly) Console.WriteLine(_text);
                            _fleet_Text += newline + _text;
                        }
                    }
                }
            }
        }

        private static void DoBattleShips(Fleet fleet) // ToDo
        {
            CreateFleetText(fleet, out string _fleetText);
            Civilization civ = fleet.Owner;
            //checkBattleShips = true;
            if (_checkFleetOrders == true && _checkOnlyPlayersUnits && checkShips_Battle)
                _text += ""; // dummy

            checkShips_Transport = true;
            DoTransportShips(fleet);

            if (civ.TargetCivilization == null)
            {

            }

            if (civ.TargetCivilization != null) // see Step_6260
            {
                Fleet attackFleet = new Fleet();
                List<Ship> allAttackWarShips = new List<Ship>();

                StarSystem ourHomeSystem = GameContext.Current.CivilizationManagers[civ].HomeSystem;
                StarSystem othersHomeSystem = GameContext.Current.CivilizationManagers[civ].HomeSystem;  // same as home until there is a target civ
                

                
                // just report to Console + define othersHomeSystem
                if (civ.TargetCivilization != null)
                {
                    othersHomeSystem = GameContext.Current.CivilizationManagers[civ.TargetCivilization].HomeSystem;

                    _text = "Step_6150:; UnitAI = Fleets > "
                        //+ ourHomeSystem.Location
                        + " " + civ.Name
                        + " has TargetCivilization > " + civ.TargetCivilization.Name
                        + " - HomeSystem at " + othersHomeSystem.Location;
                    if (writeDirectly) Console.WriteLine(_text);
                    _fleet_Text += newline + _text;
                }

                //fleet.UnitAIType = UnitAIType.SystemAttack; //2024-06-09

                foreach (var ship in fleet.Ships)
                {
                    CreateShipText(ship, out string _shipText);
                    _text = "Step_3279:; " + othersHomeSystem.Location
                        + " Ship in Fleet= "
                                    //+ othersHomeSystem.Name + "; Owner= " + othersHomeSystem.Owner
                                    //+ "; Ship= " + ship.ObjectID + "; Owner= " + ship.Name
                                    //+ "; Ship= " + ship.Design + "; Owner= " + ship.Name
                                    + _shipText
                                    ;
                    if (writeDirectly) Console.WriteLine(_text);
                    _fleet_Text += newline + _text;
                }

                // Is there a systemattack fleet, YES
                if (fleet.UnitAIType == UnitAIType.SystemAttack && ourHomeSystem != othersHomeSystem)
                {
                    if (fleet.Location == ourHomeSystem.Location)  // fleet is at home
                    {
                        attackFleet = fleet;
                        List<Colony> colonyTargetes = GameContext.Current.Universe.FindOwned<Colony>(civ.TargetCivilization).ToList();
                        if (attackFleet.Ships.Count() >= allAttackWarShips.Count())
                        {
                            // send fleet to other home system
                            int civFirePower = CalculateFirePower(civ);
                            int targetFirePower = CalculateFirePower(civ.TargetCivilization, othersHomeSystem);
                            if (targetFirePower * 1.1 < civFirePower)
                            {
                                //fleet.Owner = civ; 
                                //fleet.Location = ourHomeSystem.Location;

                                fleet.SetOrder(new EngageOrder());
                                if (fleet.Location != othersHomeSystem.Location)
                                {
                                    fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { othersHomeSystem.Sector }));
                                }
                            }
                            else if (colonyTargetes.Count() > 1)
                            {
                                double lastRange = 999;
                                _ = colonyTargetes.Remove(othersHomeSystem.Colony);
                                foreach (Colony colonyTarget in colonyTargetes)
                                {
                                    MapLocation target = colonyTarget.Location;
                                    MapLocation ai = ourHomeSystem.Location;
                                    double curretRange = Math.Sqrt(Math.Pow(target.X - ai.X, 2) + Math.Pow(target.Y - ai.Y, 2));
                                    if (curretRange < lastRange)
                                    {
                                        lastRange = curretRange;
                                        fleet.Route.Clear();
                                        fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { colonyTarget.Sector }));
                                    }
                                }
                            }
                            else
                            {
                                civ.TargetCivilization = null;
                            }

                            //GameLog.Core.AIDetails.DebugFormat("Civ {0} now in SystemAttack UnitAIType target ={1}, attack fleet location ={2} Count() ={3}, Route length {4} "
                            //    , civ.Name, civ.TargetCivilization.Name, attackFleet.Location, attackFleet.Ships.Count, attackFleet.Route.Length);
                        }
                    }
                    else if ((fleet.Location == othersHomeSystem.Location // fleet is at target
                    || GameContext.Current.Universe.FindOwned<Colony>(civ.TargetCivilization).Any(o => o.Location == fleet.Location))
                    && fleet.Ships.Any(o => o.ShipType == ShipType.Transport) && fleet.Ships.Any(o => o.IsCombatant))
                    {
                        // ************* ToDo invasion 
                        SystemAssault(fleet);
                        _text = "Step_3235:; " + othersHomeSystem.Location + " Do Invasion at Target system = "
                                + othersHomeSystem.Name + ", Owner= " + othersHomeSystem.Owner
                                //+ " > ordered one more facility for > Industry"
                                ;
                        if (writeDirectly) Console.WriteLine(_text);
                        _fleet_Text += newline + _text;

                        //GameLog.Core.AIDetails.DebugFormat("## Do Invasion at Target system, civ ={0}, targetCivilization ={1}", civ.Name, civ.TargetCivilization.Name);
                        // send home and re-set UnitAIType / can we already set ships to reserve here for return to home?
                    }
                    else if ((fleet.Location == othersHomeSystem.Location
                    || GameContext.Current.Universe.FindOwned<Colony>(civ.TargetCivilization).Any(o => o.Location == fleet.Location))
                    && (!fleet.Ships.Any(n => n.ShipType == ShipType.Transport) || !fleet.Ships.Any(n => n.IsCombatant))) // No Combat and no transport
                    {
                        fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { ourHomeSystem.Sector }));
                        fleet.UnitAIType = UnitAIType.NoUnitAI;
                        fleet.Activity = UnitActivity.NoActivity;
                        _text = "Step_3235:; " + othersHomeSystem.Location + " Set Route to Target system = "
                                + othersHomeSystem.Name + ", Owner= " + othersHomeSystem.Owner
                                //+ " > ordered one more facility for > Industry"
                                ;
                        if (writeDirectly) Console.WriteLine(_text);
                        _fleet_Text += newline + _text;
                    }
                    else
                    // Target system not longer existing ?
                    if (fleet.UnitAIType == UnitAIType.SystemAttack
                        && fleet.Location != ourHomeSystem.Location
                        //         && fleet.Location != othersHomeSystem.Location
                        && (fleet.Route.IsEmpty || !fleet.Route.Waypoints.Contains(othersHomeSystem.Location)))
                    {
                        fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { ourHomeSystem.Sector }));
                        fleet.UnitAIType = UnitAIType.NoUnitAI;
                        fleet.Activity = UnitActivity.NoActivity;
                        _text = "Step_3235:; " + othersHomeSystem.Location + " Set Route to our own HomeSystem = "
                                + ourHomeSystem.Name + ", Owner= " + ourHomeSystem.Owner
                                //+ " > ordered one more facility for > Industry"
                                ;
                        if (writeDirectly) Console.WriteLine(_text);
                        _fleet_Text += newline + _text;
                        civ.TargetCivilization = null;  // 2024-06-09 why was this ?? // Target system not longer existing ?
                    }
                    //else if (fleet.Activity == UnitActivity.NoActivity
                    //    && fleet.UnitAIType == UnitAIType.Reserve
                    //    && fleet.Location != ourHomeSystem.Location
                    //    && (fleet.Route.IsEmpty || !fleet.Route.Waypoints.Contains(othersHomeSystem.Location)))
                    //{
                    //    fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { ourHomeSystem.Sector }));
                    //    fleet.UnitAIType = UnitAIType.NoUnitAI;
                    //    fleet.Activity = UnitActivity.NoActivity;
                    //}                                
                }




                checkShips_Transport = true;
                DoTransportShips(fleet);
            }
        }

        private static void CloakAll(Fleet fleet)
        {
            if (fleet.Ships.Count > 0)
            {
                //Make sure all fleets are cloaked
                foreach (Ship ship in fleet.Ships.Where(ship => ship.CanCloak && !ship.IsCloaked))
                {
                    //GameLog.Core.AIDetails.DebugFormat("Turn {0}: Cloaking {1} {2} {3}"
                    //, GameContext.Current.TurnNumber.ToString(), ship.ObjectID, ship.Name, ship.ClassName);
                    ship.IsCloaked = true;
                }
                //_text = "UnitAI-DoTurn for; " 
                //    + fleet.ObjectID + "; "
                //    + fleet.Name + "; "
                //    + fleet.Owner + "; "
                //    + fleet.Location + "; "
                //    + "TargetCiv=" + civ.TargetCivilization
                //    ;
                //if (writeDirectly) Console.WriteLine(_text);
            }
        }


        private static void DoScience(Fleet fleet)
        {
            CreateFleetText(fleet, out string _fleetText);
            //_text = "Step_6290:; " + _fleetText + " > next > fleet.IsScience";
            //if (writeDirectly) Console.WriteLine(_text);
            //if (fleet.IsScience)
            //{
            _text = "Step_6910:; " 
                + _fleetText + " > #### fleet.IsScience"
                //+ " " + fleet.ObjectID
                //+ " " + fleet.Name
                ////+ " to go to " + bestSystemToColonize.Name
                //+ " " + fleet.Location
                ;
            if (writeDirectly) Console.WriteLine(_text);
            _fleet_Text += newline + _text;

            if (fleet.Order == FleetOrders.IdleOrder) fleet.Activity = UnitActivity.NoActivity;

            //checkScienceShips = true;
            if (_checkFleetOrders == true && _checkOnlyPlayersUnits && checkShips_Science)
                _text += ""; // dummy

            //Science Ship
            if (fleet.Activity == UnitActivity.NoActivity || fleet.Route.IsEmpty /*|| fleet.Order.IsComplete*/)
            {
                if (GetBestSystemForScience(fleet, out StarSystem bestSystemForScience))
                {
                    if (bestSystemForScience != null)
                    {
                        bool hasOurSpyNetwork = CheckForSpyNetwork(bestSystemForScience.Owner, fleet.Owner); // false for e.g. Nebula
                        if (!hasOurSpyNetwork)
                        {
                            if (bestSystemForScience.Sector == fleet.Sector)
                            {
                                fleet.SetOrder(new MissionOrder());
                                fleet.UnitAIType = UnitAIType.Science;
                                fleet.Activity = UnitActivity.Mission;
                                // GameLog.Core.AIDetails.DebugFormat("Science fleet {0} at Research location {1}", fleet.ObjectID, fleet.Location);
                            }
                            else
                            {
                                //GetFleetOwner(fleet);
                                fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { bestSystemForScience.Sector }));
                                fleet.UnitAIType = UnitAIType.Science;
                                fleet.Activity = UnitActivity.Mission;

                                _text = "Step_6913:; " + _fleetText + " > SET Route to " + bestSystemForScience.Location;
                                if (writeDirectly) Console.WriteLine(_text);
                                _fleet_Text += newline + _text;
                                // GameLog.Core.AIDetails.DebugFormat("Ordering science fleet {0} to {1}", fleet.ObjectID, bestSystemForScience);
                            }
                        }
                    }
                }
                else
                {
                    //not found a bestSystemForScience > go to Explore
                    fleet.SetOrder(new ExploreOrder());
                    _text = "Step_6911:; " + _fleetText + " > #### not found a bestSystemForScience > go to Explore"
                            //+ " " + fleet.ObjectID
                            //+ " " + fleet.Name
                            ////+ " to go to " + bestSystemToColonize.Name
                            //+ " " + fleet.Location
                            ;
                    if (writeDirectly) Console.WriteLine(_text);
                    _fleet_Text += newline + _text;
                    //GameLog.Core.AIDetails.DebugFormat("Nothing to do for science fleet {0}", fleet.ObjectID);
                }
                //}
                //}
                //}
            }
        }

        private static void DoDiplomatic(Fleet fleet)
        {
            CreateFleetText(fleet, out string _fleetText);
            //_text = "Step_6270:; next > fleet.IsDiplomatic   " + _fleetText;
            //if (writeDirectly) Console.WriteLine(_text);

            if (fleet.IsDiplomatic)
            {
                _text = "Step_6711:; " + _fleetText + " > fleet.IsDiplomatic"
                    ;
                if (writeDirectly) Console.WriteLine(_text);
                _fleet_Text += newline + _text;

                fleet.UnitAIType = UnitAIType.Diplomatic;

                //checkDiplomaticShips = true;
                if (_checkFleetOrders == true && _checkOnlyPlayersUnits && checkShips_Diplomatic)
                    _text += ""; // dummy

                // Send diplomatic ship and influence order, but what do we do with race traits and diplomacy?
                //if (fleet.Owner.Traits.Contains(CivTraits.Peaceful.ToString()) || fleet.Owner.Traits.Contains(CivTraits.Kindness.ToString()))
                //{
                //    // ToDo;
                //}
                if (fleet.Activity == UnitActivity.NoActivity || fleet.Route.IsEmpty /*|| fleet.Order.IsComplete*/)
                {
                    if (GetBestColonyForDiplomacy(fleet, out Colony bestSystemForDiplomacy))
                    {
                        //if (bestSystemForDiplomacy.OwnerID < 6)
                        if (bestSystemForDiplomacy.OwnerID < 999)
                        {
                            if (bestSystemForDiplomacy.Sector == fleet.Sector)
                            {
                                fleet.SetOrder(new InfluenceOrder());
                                fleet.UnitAIType = UnitAIType.Diplomatic;
                                fleet.Activity = UnitActivity.Mission;
                                // GameLog.Core.AIDetails.DebugFormat("Ordering diplomacy fleet {0} in {1} to influence", fleet.ObjectID, fleet.Location);
                            }
                            else
                            {
                                //GetFleetOwner(fleet);
                                Civilization civ = GameContext.Current.CivilizationManagers[fleet.Owner.CivID].Civilization;
                                fleet.Owner = civ;
                                fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { bestSystemForDiplomacy.Sector }));
                                fleet.UnitAIType = UnitAIType.Diplomatic;
                                fleet.Activity = UnitActivity.Mission;
                                _text = "Step_6712:; " + _fleetText
                                        + " > ordered to Influence of " + bestSystemForDiplomacy.Location
                                        ;
                                if (writeDirectly) Console.WriteLine(_text);
                                _fleet_Text += newline + _text;
                                //  GameLog.Core.AIDetails.DebugFormat("Ordering diplomacy fleet {0} to {1}", fleet.ObjectID, bestSystemForDiplomacy);
                            }
                        }
                    }
                    //else
                    //{
                    //  GameLog.Core.AIDetails.DebugFormat("Nothing to do for diplomacy fleet {0}", fleet.ObjectID);
                    //}
                }
            }
        }

        private static void DoSpy(Fleet fleet)
        {
            fleet.UnitAIType = UnitAIType.Spy;
            //_text = "Step_6280:; next > fleet.IsSpy   " + _fleetText;
            //if (writeDirectly) Console.WriteLine(_text);
            //if (fleet.IsSpy) // install spy network
            //{
            _text = "Step_6810:; " + CreateFleetText(fleet, out _fleetText) + " > #### fleet.IsSpy"
                ;
            if (writeDirectly) Console.WriteLine(_text);
            _fleet_Text += newline + _text;

            //checkSpyShips = true;
            if (_checkFleetOrders == true && _checkOnlyPlayersUnits && checkShips_Spy)
                _text += ""; // dummy

            if (fleet.Order == FleetOrders.IdleOrder)
            {
                fleet.Activity = UnitActivity.NoActivity;
            }
            else
            {
                fleet.Activity = UnitActivity.Mission;
            }

            if (_checkFleetOrders == true  && _checkOnlyPlayersUnits)
                _text += ""; // dummy

            //Spy
            if (!fleet.Route.IsEmpty && fleet.Activity != UnitActivity.Mission/*fleet.Activity == UnitActivity.NoActivity || */ /*|| fleet.Order.IsComplete*/)
            {
                if (GetBestColonyForSpying(fleet, out Colony bestSystemForSpying))
                {
                    if (bestSystemForSpying != null && bestSystemForSpying.OwnerID < 7)
                    {
                        bool hasOurSpyNetwork = CheckForSpyNetwork(bestSystemForSpying.Owner, fleet.Owner);
                        if (!hasOurSpyNetwork)
                        {
                            if (bestSystemForSpying.Sector == fleet.Sector)
                            {
                                fleet.SetOrder(new SpyOnOrder()); // install spy network
                                                                  //fleet.UnitAIType = UnitAIType.NoUnitAI;
                                fleet.Activity = UnitActivity.Mission;
                                // GameLog.Core.AIDetails.DebugFormat("Ordering spy fleet {0} in {1} to install spy network", fleet.ObjectID, fleet.Location);
                            }
                            else
                            {
                                //GetFleetOwner(fleet);
                                Civilization civ = GameContext.Current.CivilizationManagers[fleet.Owner.CivID].Civilization;
                                fleet.Owner = civ;
                                fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { bestSystemForSpying.Sector }));
                                fleet.UnitAIType = UnitAIType.Spy;
                                fleet.Activity = UnitActivity.Mission;
                                // GameLog.Core.AIDetails.DebugFormat("Ordering spy fleet {0} to {1}", fleet.ObjectID, bestSystemForSpying);
                            }
                        }
                    }
                }
                else
                {
                    fleet.SetOrder(new ExploreOrder());
                    _text = "Step_6813:; " + CreateFleetText(fleet, out string _fleetText) + " > Explore due to > no bestSystemForSpying for Spy Ship "
                            //+ "; Activity= " + fleet.Activity.ToString()

                            //+ "; Route empty= " + fleet.Route.IsEmpty
                            //+ "; to go > " + bestSectorForStation.Location
                            //+ " " + bestSectorForStation.Name
                            ;
                    if (writeDirectly) Console.WriteLine(_text);
                    _fleet_Text += newline + _text;
                    //GameLog.Core.AIDetails.DebugFormat("Nothing to do for spy fleet {0}", fleet.ObjectID);

                    if (_checkFleetOrders == true && _checkOnlyPlayersUnits)
                        _text += ""; // dummy
                }
            }
            //}
        }

        private static void DoMedical(Fleet fleet)
        {
            CreateFleetText(fleet, out string _fleetText);
            //_text = "Step_6269:; next > fleet.IsMedical   " + _fleetText;
            //if (writeDirectly) Console.WriteLine(_text);
            //if (fleet.IsMedical)
            //{
            _text = "Step_6610:; " + _fleetText + " > fleet.IsMedical: "
                ;
            if (writeDirectly) Console.WriteLine(_text);
            _fleet_Text += newline + _text;

            //checkMedicalShips = true;
            if (_checkFleetOrders == true && _checkOnlyPlayersUnits && checkShips_Medical)
                _text += ""; // dummy

            //Medical
            if (fleet.Route.IsEmpty 
                && fleet.Sector.System != null && fleet.Sector.System.Colony != null 
                && fleet.Sector.Owner == fleet.Owner /*|| !fleet.Sector.Owner.IsEmpire*/
                && !fleet.Sector.System.Colony.Health.IsMaximized)
            {
                fleet.SetOrder(new MedicalOrder());
                fleet.Order = FleetOrders.MedicalOrder;
                fleet.Activity = UnitActivity.NoActivity;
                fleet.UnitAIType = UnitAIType.Medical;
                fleet.UnlockRoute();
            }
            else
            {
                fleet.Activity = UnitActivity.NoActivity;
            }


            if (fleet.Activity == UnitActivity.NoActivity)// || fleet.Route.IsEmpty/* || fleet.Order.IsComplete*/)
            {
                if (GetBestColonyForMedical(fleet, out Colony bestSystemForMedical))
                {
                    if (bestSystemForMedical != null && bestSystemForMedical.Sector == fleet.Sector)
                    {
                        //Colony medical treatment
                        fleet.SetOrder(new MedicalOrder());
                        fleet.UnitAIType = UnitAIType.Medical;
                        fleet.Activity = UnitActivity.Mission;
                        _text += " > Order= " + fleet.Order;
                        // GameLog.Core.AIDetails.DebugFormat("Ordering medical fleet {0} in {1} to treat the population", fleet.ObjectID, fleet.Location);
                    }
                    else if (bestSystemForMedical != null)
                    {
                        //GetFleetOwner(fleet);
                        Civilization civ = GameContext.Current.CivilizationManagers[fleet.Owner.CivID].Civilization;
                        fleet.Owner = civ;
                        fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { bestSystemForMedical.Sector }));
                        fleet.UnitAIType = UnitAIType.Medical;
                        fleet.Activity = UnitActivity.Mission;
                        _text += " > Heading to " + bestSystemForMedical.Location;
                        // GameLog.Core.AIDetails.DebugFormat("Ordering medical fleet {0} to {1}", fleet.ObjectID, bestSystemForMedical);
                    }
                    //_text = "Step_6610:; " + _fleetText + " > fleet.IsMedical: "
                    //    + " " + fleet.ObjectID
                    //    + " " + fleet.Name
                    //    + " at " + bestSystemForMedical.Name
                    //    + " " + fleet.Location
                    //    ;
                    if (writeDirectly) Console.WriteLine(_text);
                    _fleet_Text += newline + _text;
                }
                //else
                //{
                //GameLog.Core.AIDetails.DebugFormat("Nothing to do for medical fleet {0}", fleet.ObjectID);
                //}

                //}
            }
        }

        private static void DoConstruction(Fleet fleet)
        {
            CreateFleetText(fleet, out string _fleetText);
            fleet.UnitAIType = UnitAIType.Constructor;

            Civilization civ = GameContext.Current.CivilizationManagers[fleet.Owner.CivID].Civilization;
            //_text = "Step_6250:; next > fleet.IsConstructor   " + _fleetText;
            //if (writeDirectly) Console.WriteLine(_text);
            //if (fleet.IsConstructor) // || fleet.UnitAIType == UnitAIType.Constructor || (fleet.Ships.Any(a => a.ShipType == ShipType.Construction) && fleet.Ships.Count() == 2))
            //{
            _text = "Step_6510:; " + _fleetText + " > fleet.IsConstructor:"
                    //+ " " + fleet.ObjectID
                    //+ " " + fleet.Name
                    ////+ " to go to " + bestSystemToColonize.Name
                    //+ " " + fleet.Location
                    ;
            //if (writeDirectly) Console.WriteLine(_text);
            _fleet_Text += newline + _text;

            //checkConstructionShips = true;
            if (_checkFleetOrders == true && _checkOnlyPlayersUnits && checkShips_Construction)
                _text += ""; // dummy

            //GameLog.Client.AIDetails.DebugFormat("*////*Top of Constuctor, Owner ={0}, shiptype ={1}, #Ships ={2}, Activity ={3} duration ={4} Route empty ={5}",
            //         fleet.Owner.Key, fleet.Ships[0].ShipType, fleet.Ships.Count(), fleet.Activity, fleet.ActivityDuration, fleet.Route.IsEmpty);
            if (fleet.Ships.Any(x => x.ShipType == ShipType.Construction)) // a fleet that really has a constuctor
            {
                List<Fleet> allCivFleets = GameContext.Current.Universe.FindOwned<Fleet>(civ).ToList();
                List<Fleet> allConstuctFleetsHere = allCivFleets
                    .Where(a => a.Sector == fleet.Sector)
                    .Where(a => a.IsConstructor || (a.MultiFleetHasAConstructor /*&& a.Ships.Count == 2*/)).ToList();
                bool bestSector = GetBestSectorForStation(fleet, allConstuctFleetsHere, out Sector bestSectorForStation);

                _text = "Step_6521:; " + _fleetText + " > Found Constructor"
                    + "; Activity= " + fleet.Activity.ToString()

                    + "; Route empty= " + fleet.Route.IsEmpty
                    + "; to go > " + bestSectorForStation.Location
                    + " " + bestSectorForStation.Name
                    ;
                if (writeDirectly) Console.WriteLine(_text);
                _fleet_Text += newline + _text;
                //GameLog.Client.AIDetails.DebugFormat("Found Constructor, fleet location ={0}, Owner ={1}, #Ships ={2}, Activity ={3} duration ={4} Route empty ={5}",
                //        fleet.Sector.Name, fleet.Owner.Key, fleet.Ships.Count(), fleet.Activity, fleet.ActivityDuration, fleet.Route.IsEmpty);

                //ShipType.Construction
                if (fleet.Activity == UnitActivity.BuildStation)
                {
                    _text += " - Construction ongoing...";
                    if (writeDirectly) Console.WriteLine(_text);
                    _fleet_Text += newline + _text;
                    //GameLog.Client.AIDetails.DebugFormat("Start Construction, fleet order ={0}, Owner ={1}, Sector ={2},{3}, Activity ={4} duration ={5} start ={6}",
                    //fleet.Order.OrderName, fleet.Owner.Key, fleet.Sector.Name, fleet.Location, fleet.Activity, fleet.ActivityDuration, fleet.ActivityStart);
                }
                //ShipType.Construction
                if (fleet.IsStranded || !fleet.CanMove) // && fleet.UnitAIType != UnitAIType.Building) // && !systemOfEmpire(fleet)
                {
                    if (fleet.UnitAIType != UnitAIType.Building && fleet.Sector.Station != null)
                    {
                        _text = "Step_6527:; " + _fleetText + " > Constructor can't move"
                                + "; Activity= " + fleet.Activity.ToString()

                                + "; Route empty= " + fleet.Route.IsEmpty
                                + "; to go > " + bestSectorForStation.Location
                                + " " + bestSectorForStation.Name
                                ;
                        if (writeDirectly) Console.WriteLine(_text);
                        _fleet_Text += newline + _text;

                        GameLog.Client.AIDetails.DebugFormat("fleet stranded ={0}, Owner ={1}, Sector ={2},{3}, Activity ={4} duration ={5} start ={6} order ={7}",
                        fleet.IsStranded, fleet.Owner.Key, fleet.Sector.Name, fleet.Location, fleet.Activity, fleet.ActivityDuration, fleet.ActivityStart, fleet.Order.OrderName);

                        BuildStation(fleet, allConstuctFleetsHere);
                    }
                }
                //ShipType.Construction
                if (bestSector)
                {
                    if (bestSectorForStation.Location == fleet.Sector.Location)
                    {
                        if (fleet.Activity == UnitActivity.Mission && fleet.Activity != UnitActivity.BuildStation)
                        {
                            _text += " - Order to build a station...";
                            if (writeDirectly) Console.WriteLine(_text);
                            _fleet_Text += newline + _text;

                            BuildStation(fleet, allConstuctFleetsHere);
                        }
                    }
                    else if (bestSectorForStation != fleet.Sector) // best sector can change over time
                    {
                        //if (fleet.UnitAIType != UnitAIType.Constructor 
                        if (fleet.Activity != UnitActivity.Hold
                        && fleet.Activity != UnitActivity.BuildStation) // have a place to go and not already trying to move or build then go
                        {
                            //GetFleetOwner(fleet);
                            if (!fleet.Ships.Any(s => s.ShipType >= ShipType.FastAttack) && civ.TargetCivilization == null)
                            {
                                //_text += "Step_6888:; LATER > bring back > GetFleetEscort";
                                //if (writeDirectly) Console.WriteLine(_text);
                                GetFleetEscort(fleet, bestSectorForStation); // escorts added to fleet at home conlony 
                            }


                            fleet.Owner = civ;
                            fleet.OwnerID = civ.CivID;
                            //fleet.Route.Clear();
                            fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { bestSectorForStation }));
                            fleet.UnitAIType = UnitAIType.Constructor;
                            fleet.Activity = UnitActivity.Mission;
                            //fleet.Order = FleetOrders.AvoidOrder;

                            GameLog.Core.AIDetails.DebugFormat("Ordering a constructor fleet {0} to {1}, {2}", fleet.Owner.Name, bestSectorForStation.Location, bestSectorForStation.Name);
                            GameLog.Core.AIDetails.DebugFormat("Ordering a constructor first ship Name {0} Design {1}", fleet.Ships[0].Name, fleet.Ships[0].DesignName);
                            if (fleet.Ships.Count() > 1)
                            {
                                GameLog.Core.AIDetails.DebugFormat("Ordering a constructor second ship Name {0} Design {1}", fleet.Ships[1].Name, fleet.Ships[1].DesignName);
                            }
                        }
                        else if (fleet.Route.IsEmpty && fleet.Activity != UnitActivity.BuildStation && fleet.Activity != UnitActivity.Hold)
                        {
                            GameLog.Core.AIDetails.DebugFormat("Empty Route constructor fleet ship 1 Name {0} Design {1}", fleet.Ships[0].Name, fleet.Ships[0].DesignName);
                            fleet.Owner = civ;
                            fleet.OwnerID = civ.CivID;
                            //fleet.Route.Clear();
                            fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { bestSectorForStation }));
                            fleet.UnitAIType = UnitAIType.Constructor;
                            fleet.Activity = UnitActivity.Mission;
                        }
                    }


                    // if you were on hold helping build but now there is a station then look to move
                    if (fleet.Activity == UnitActivity.Hold && fleet.Sector.Station != null)
                    {
                     //GetFleetOwner(fleet);
                        fleet.Owner = civ;
                        //fleet.Route.Clear();
                        fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { bestSectorForStation }));
                        fleet.UnitAIType = UnitAIType.Constructor;
                        fleet.Activity = UnitActivity.Mission;
                    }
                }
            }
            else if (fleet.UnitAIType == UnitAIType.Constructor && fleet.Ships.Count() == 1)  // combat ships set to constuctor AI but lost their constructor ship 
            {
                RemoveEscortShips(fleet, ShipType.Construction);
                //continue;
            }
            //}
            //End of ShipType.Construction
        }

        private static void DoScout(Fleet fleet)
        {
            fleet.UnitAIType = UnitAIType.Explorer;
            fleet.Activity = UnitActivity.Mission;
            fleet.SetOrder(new ExploreOrder());// explore is really set in FleetOrders OnTurnBegining()

            CreateFleetText(fleet, out string _fleetText);
            // Scout
            //if (fleet.IsScout) //(fleet.Ships.Where(o => o.ShipType == ShipType.Scout).Any())
            //{
            _text = "Step_6220:; " + _fleetText + " > fleet.IsScout > EXPLORE  ";
            //if (writeDirectly) Console.WriteLine(_text);
            _fleet_Text += newline + _text;

            //checkScoutShips = true;
            if (_checkFleetOrders == true && _checkOnlyPlayersUnits && checkShips_Scout)
                _text += ""; // dummy

            int _c = 0;
            foreach (var ship in fleet.Ships)
            {
                _c += 1;
                _text = "Step_6225:; " + CreateShipText(ship, shipText: out string shiptext) + " > fleet.IsScout > EXPLORE > " + _c + " of " + fleet.Ships.Count;
                if (writeDirectly)
                    Console.WriteLine(_text);
                _fleet_Text += newline + _text;
            }
            //}
            if (writeDirectly) Console.WriteLine(_text);

            //checkScoutShips = true;
            if (_checkFleetOrders == true && _checkOnlyPlayersUnits && checkShips_Scout)
                _text += ""; // dummy
        } // End of DoScout(Fleet fleet)


        private static void DoTransportShips(Fleet fleet)
        {
            CreateFleetText(fleet, out string _fleetText);
            // Scout
            //if (fleet.IsScout) //(fleet.Ships.Where(o => o.ShipType == ShipType.Scout).Any())
            //{
            _text = "Step_6228:; " + _fleetText + " > fleet.IsTransport >  ";
            //if (writeDirectly) Console.WriteLine(_text);
            _fleet_Text += newline + _text;

            //checkTransportShips = true;
            if (_checkFleetOrders == true && _checkOnlyPlayersUnits && checkShips_Transport)
                _text += ""; // dummy

            // explore is really set in FleetOrders OnTurnBegining()
            fleet.SetOrder(new ExploreOrder());

            fleet.UnitAIType = UnitAIType.Transport;
            fleet.Activity = UnitActivity.NoActivity;

            int _c = 0;
            foreach (var ship in fleet.Ships)
            {
                _c += 1;
                _text = "Step_6225:; " + CreateShipText(ship, shipText: out string shiptext) + " > fleet.IsTransport > " + _c + " of " + fleet.Ships.Count;
                if (writeDirectly)
                    Console.WriteLine(_text);
                _fleet_Text += newline + _text;
            }
            //}
            if (writeDirectly) Console.WriteLine(_text);

            //checkScoutShips = true;
            if (_checkFleetOrders == true && _checkOnlyPlayersUnits && checkShips_Transport)
                _text += ""; // dummy
        }

        private static void DoColony(Fleet fleet)
        {
            CreateFleetText(fleet, out string _fleetText);
            //if (fleet.IsColonizer)// || fleet.UnitAIType == UnitAIType.Colonizer)
            //{
            fleet.UnitAIType = UnitAIType.Colonizer;

            _text = "Step_6301:; " + _fleetText + " > fleet.IsColonizer ";
            if (writeDirectly) Console.WriteLine(_text);
            _fleet_Text += newline + _text;

            //checkColonyShips = true;
            if (_checkFleetOrders == true && _checkOnlyPlayersUnits && checkShips_Colony)
                _text += ""; // dummy

            if (!fleet.Route.IsEmpty && fleet.Sector.System != null)
            {
                if (fleet.Sector.System.IsInhabited)
                {
                    fleet.Activity = UnitActivity.NoActivity;
                }
                else  // not tested
                {
                    fleet.Activity = UnitActivity.AimReached;
                }
            }


            //Colonizer
            if (/*fleet.Sector == ourHomeSystem.Sector && */(fleet.Activity == UnitActivity.NoActivity || fleet.Route.IsEmpty /*|| fleet.Order.IsComplete*/)) // || fleet.Activity == UnitActivity.Mission
            {
                if (GetBestSystemToColonize(fleet, out StarSystem bestSystemToColonize))
                {
                    //Head to the system  
                    //fleet.Owner = civ;
                    fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { bestSystemToColonize.Sector }));
                    if (bestSystemToColonize == null)
                    {
                        _text = "Step_6320:; " + _fleetText
                                + " > found no bestSystemToColonize "
                               ;
                        if (writeDirectly) Console.WriteLine(_text);
                        _fleet_Text += newline + _text;
                    }
                    else
                    {
                        fleet.UnitAIType = UnitAIType.Colonizer;
                        fleet.Activity = UnitActivity.Mission;
                        if (!fleet.Ships.Any(s => s.ShipType >= ShipType.FastAttack) /*&& civ.TargetCivilization == null*/)
                        {
                            //_text += "Step_6889:; LATER > bring back > GetFleetEscort";
                            //if (writeDirectly) Console.WriteLine(_text);
                            GetFleetEscort(fleet, bestSystemToColonize.Sector);
                        }
                        _text = "Step_6321:; " + _fleetText
                            + " > Ordered for Colonizing > "

                            + " to go to " + bestSystemToColonize.Name
                            + " " + bestSystemToColonize.Location

                            ;
                        if (writeDirectly) Console.WriteLine(_text);
                        _fleet_Text += newline + _text;
                        // GameLog.Core.AIDetails.DebugFormat("Ordering {0} colonizer {1} to go to {2} {3}", fleet.Owner, fleet.Name, bestSystemToColonize.Name, bestSystemToColonize.Location);
                    }
                }
                //else if (fleet.Ships.Where(s => s.ShipType >= ShipType.FastAttack).Any())
                //{
                //    RemoveEscortShips(fleet, ShipType.Colony);
                //}

            }

            //if (fleet.Sector != ourHomeSystem.Sector) // only colonize when not at homesystem 
            //{
            //_text = "Step_6410:; " + _fleetText + ": Colonizer on the road:";
            //if (writeDirectly) Console.WriteLine(_text);

            //if (fleet.Sector != bes)

            _text = "Step_6420:; " + _fleetText + ": WeAreAtSystemToColonize= " + WeAreAtSystemToColonize(fleet);
            //if (writeDirectly) Console.WriteLine(_text);
            _fleet_Text += newline + _text;

            _text = "Step_6430:; " + _fleetText + ": SystemIsAlreadyTaken= " + SystemIsAlreadyTaken(fleet);
            //if (writeDirectly) Console.WriteLine(_text);
            _fleet_Text += newline + _text;

            if (WeAreAtSystemToColonize(fleet) && !SystemIsAlreadyTaken(fleet))
            {
                if (fleet.Ships.Any(a => a.ShipType == ShipType.Colony))
                {
                    _text = "Step_6440:; " + _fleetText + ": AnyColonyShip > ok = has Colony ship";
                    if (writeDirectly) Console.WriteLine(_text);
                    _fleet_Text += newline + _text;

                    //fleet.Route.Clear();
                    fleet.SetOrder(new ColonizeOrder());
                    _text = "Step_6450:; " + _fleetText + " > ColonizeOrder !";
                    if (writeDirectly) Console.WriteLine(_text);
                    _fleet_Text += newline + _text;
                    if (!fleet.Ships.Any(x => x.ShipType == ShipType.Colony))
                    {
                        RemoveEscortShips(fleet, ShipType.Colony);
                    }

                    //continue;
                }
                else
                {
                    RemoveEscortShips(fleet, ShipType.Colony);
                    //continue;
                }

            }
            else if ((WeAreAtSystemToColonize(fleet) && SystemIsAlreadyTaken(fleet))
                && fleet.Route.IsEmpty || fleet.Route == null)
            {
                _text = "Step_6460:; Colonizing aim reached: " + _fleetText;
                if (writeDirectly) Console.WriteLine(_text);
                _fleet_Text += newline + _text;

                if (GetBestSystemToColonize(fleet, out StarSystem bestSystemToColonize))
                {
                    //GetFleetOwner(fleet);
                    //fleet.Owner = civ;
                    fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { bestSystemToColonize.Sector }));
                    fleet.UnitAIType = UnitAIType.Colonizer;
                    fleet.Activity = UnitActivity.Mission;
                }
                else
                {
                    if (fleet.Ships.Count > 1)
                    {
                        RemoveEscortShips(fleet, ShipType.Colony);
                    }

                    CivilizationManager civM = GameContext.Current.CivilizationManagers[fleet.Owner.CivID];
                    //CivilizationManager civM = fleet.Owner.RendezvousSector;
                    Sector _rendezvous_Sector = civM.RendezvousSector;
                    //GetCivM(fleet).RendezvousSector;
                    //civ = fleet.GetCiv(fleet);

                    fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { _rendezvous_Sector }));
                    fleet.Activity = UnitActivity.NoActivity;
                    fleet.UnitAIType = UnitAIType.NoUnitAI;
                    //continue;
                }
            }
            //}
            //}
        }

        private static string CreateShipText(Ship ship, out string shipText)
        {
            // call via > CreateShipText(ship, out string _shipText);

            if (ship == null || ship.Fleet == null)
            {
                shipText = " - no ship or no fleet";
                return shipText;
            }


            shipText = //"Step_3103:; "+ 
                ship.Location

                + " ; " + ship.ObjectID + " ;SHIP "

                + " ; " + ship.Design
                + " ; " + ship.Name
                + " ;UnitAIType= " + ship.Fleet.UnitAIType
                + " ; " + ship.Fleet.Activity
                + " ; " + ship.Fleet.Order

                ;
            return shipText;
        }

        private static string CreateFleetText(Fleet fleet, out string _fleetText)
        {
            // call via > CreateFleetText(fleet, out string _fleetText);

            //if (ship == null)
            //    return " - no ship";

            _fleetText = //"Step_3103:; "+ 
                fleet.Location

                + " ; " + fleet.ObjectID + " ;Fleet"
                + " ; " + fleet.Ships[0].Design
                + " ; " + fleet.Name


                + " ;UnitAIType= " + fleet.UnitAIType
                + " ; " + fleet.Activity
                + " ; " + fleet.Order



                ;
            return _fleetText;
        }

        // Methods
        private static int CalculateFirePower(Civilization civ)
        {
            int firePower = 0;
            foreach (Fleet civFleet in GameContext.Current.Universe.FindOwned<Fleet>(civ).ToList())
            {
                foreach (Ship ship in civFleet.Ships.Where(s => s.ShipType >= ShipType.Scout || s.ShipType == ShipType.Transport).ToList())
                {
                    firePower += ship.Firepower();
                    // GameLog.Client.AIDetails.DebugFormat("A ship all attack ships {0} location ={1}", ship.Name, ship.Location );
                }
            }
            return firePower;
        }
        private static int CalculateFirePower(Civilization civ, StarSystem otherHomeSystem)
        {
            int firePower = 0;
            foreach (Fleet civFleet in GameContext.Current.Universe.FindOwned<Fleet>(civ).ToList())
            {
                foreach (Ship ship in civFleet.Ships.Where(s => s.ShipType >= ShipType.Scout || s.ShipType == ShipType.Transport).ToList())
                {
                    firePower += ship.Firepower();
                    // GameLog.Client.AIDetails.DebugFormat("A ship all attack ships {0} location ={1}", ship.Name, ship.Location );
                }
            }
            if (otherHomeSystem.Sector.Station != null)
            {
                firePower += otherHomeSystem.Sector.Station.Firepower();
            }
            return firePower;
        }

        public static void BuildAndSendFleet(Fleet fleet, Civilization theCiv, UnitActivity activity, UnitAIType unitAIType, Sector destination)
        {
            fleet.SetOrder(new AvoidOrder());
            fleet.Activity = activity;
            fleet.UnitAIType = unitAIType;
            fleet.Owner = theCiv;
            fleet.OwnerID = theCiv.CivID;
            fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { destination }));
        }

        public static void SystemAssault(Fleet fleet)
        {
            GetFleetOwner(fleet);
            if (fleet.Order != FleetOrders.AssaultSystemOrder)
            {
                fleet.SetOrder(new AssaultSystemOrder());
            }
        }

        public static bool CanAllShipsGetThere(Civilization attacker, Civilization attacked)
        {
            IEnumerable<Fleet> attackWarShips = GameContext.Current.Universe.FindOwned<Fleet>(attacker).Where(s => s.IsBattleFleet
                    || s.IsFastAttack || s.IsScout).ToList();
            StarSystem othersHomeSystem = GameContext.Current.CivilizationManagers[attacked].HomeSystem;
            {
                foreach (Fleet testRangeFleet in attackWarShips)
                {
                    if (!FleetHelper.IsSectorWithinFuelRange(othersHomeSystem.Sector, testRangeFleet))
                    {
                        return false;
                    }
                }
            }
            if (!attacked.IsEmpire)
            {
                GameLog.Client.AIDetails.DebugFormat("The {0} found minor {1} to Invation", attacker.Name, attacked.Name);
            }
            else
            {
                GameLog.Client.AIDetails.DebugFormat("The {0} found Empire {1} for Invation", attacker.Name, attacked.Name);
            }

            return true;
        }

        /*
         * Colonization
         */

        /// <summary>
        /// Get the best <see cref="StarSystem"/> for the given <see cref="Fleet"/>
        /// to colonize
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool GetBestSystemToColonize(Fleet fleet, out StarSystem result)
        {

            GetFleetOwner(fleet);
            if (fleet == null)
            {
                throw new ArgumentNullException(nameof(fleet));
            }


            List<Fleet> colonizerFleets = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner)
                .Where(o => o.IsColonizer || o.MultiFleetHasAColonizer).ToList();
            List<Fleet> otherFleets = colonizerFleets.Where(o => o != fleet).ToList(); // other colony ships

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
            CivilizationMapData mapData = civManager.MapData;

            //Get a list of all systems that we can colonise
            List<StarSystem> systems = GameContext.Current.Universe.Find<StarSystem>()
            //We need to know about it (no cheating)

            // AI: numbers of IsScanned && IsExplored might be low
            // test - .Where(r => mapData.IsScanned(r.Location)) //&& mapData.IsExplored(r.Location))
            .Where(s => !s.IsOwned || s.Owner == fleet.Owner)
            .Where(t => !t.IsInhabited && !t.HasColony)
            .Where(u => u.IsHabitable(fleet.Owner.Race))
            .Where(v => v.StarType != StarType.RadioPulsar && v.StarType != StarType.NeutronStar)
            .Where(w => FleetHelper.IsSectorWithinFuelRange(w.Sector, fleet))
            .Where(x => !otherFleets.Any(f => f.Route.Waypoints.LastOrDefault() == x.Location || x.Location == f.Location && f.Order is ColonizeOrder)
            //.Where(y => GameContext.Current.Universe.FindAt<Orbital>(y.Location).Any(o => DiplomacyHelper.ArePotentialEnemies(fleet.Owner, y.Owner))
            )
            //That isn't owned by another civilization
            //That isn't currently inhabited or have a current colony
            //That's actually inhabitable
            //That are in fuel range
            //That doesn't have potential enemies in it
            //Where a ship isn't heading there already
            //Where a ship isn't there and colonizing
            .ToList();

            // find out the reasons for low count of > available systems



            List<StarSystem> systemsToControlList = GameContext.Current.Universe.Find<StarSystem>()
                .Where(r => mapData.IsScanned(r.Location) && mapData.IsExplored(r.Location))
                .Where(w => FleetHelper.IsSectorWithinFuelRange(w.Sector, fleet))
                .ToList();
            _text = "Step_6160:; GetBestSystemToColonize:; Available systems with FuelRange for " + fleet
                + " > " + systemsToControlList.Count
                ;
            if (writeDirectly) Console.WriteLine(_text);

            foreach (var item in systemsToControlList)
            {
                _text = "Step_6170:; GetBestSystemToColonize:; "
                    + " for " + fleet.Location
                    + " " + fleet.Name
                    + " > " + GameEngine.LocationString(item.Location.ToString())
                    + " " + item.Name
                    + " - Dist: " + GetDistanceTo(fleet.Location, item.Location)
                    + ", Inhabited=" + item.IsInhabited
                    ;
                if (writeDirectly) Console.WriteLine(_text);
            }

            // systems is the original result
            if (systems.Count == 0)
            {
                _text = "Step_6190:; Found nothing to colonize > systems.Count = 0 for"
                    + ": " + fleet.Location + blank
                    + fleet.Name
                    + " > Going to Order EXPLORE "
                    ;
                if (writeDirectly) Console.WriteLine(_text);

                // 2023-07-15: new
                //fleet.Order = FleetOrders.ExploreOrder;

                result = null;
                return false;
            }

            List<StarSystem> enemySystems = new List<StarSystem>() { systems.FirstOrDefault() };
            //StarSystem placeholder = enemySystems.FirstOrDefault();
            foreach (StarSystem system in systems)
            {
                //works
                //_text = "Step_6196:; Colonizing-Aim of " + systems.Count
                //    + " > IsInhabited= " + system.IsInhabited
                //    + ": " + system.Location
                //    + blank + system.Name


                //    ;
                //if (writeDirectly) Console.WriteLine(_text);

                if (system.Owner != null && GameContext.Current.Universe.FindAt<Orbital>(system.Location).Any(o => DiplomacyHelper.ArePotentialEnemies(fleet.Owner, system.Owner)))
                {
                    enemySystems.Add(system);
                }
            }

            foreach (StarSystem removeSystem in enemySystems)
            {
                _text = "Step_6197:; Colonizing-Aim of " + systems.Count
                    + " (REMOVED): " + removeSystem.Location + blank
                    + removeSystem.Name
                    ;
                if (writeDirectly) Console.WriteLine(_text);

                //_ = systems.Remove(removeSystem);  // enemySystems is first or default that is ONE, why remove - caused crashes as well
            }
            //foreach (var system in systems)
            //{
            //    GameLog.Client.AIDetails.DebugFormat("System ={0}, {1} Colony? ={2} owner ={3}, Habitable? ={4} starType {5} for {6}"
            //        , system.Name, system.Location, system.HasColony, system.Owner, system.IsHabitable(fleet.Owner.Race), system.StarType, fleet.Owner);
            //}
            if (systems.Count == 0)
            {
                result = null;
                return false;
            }

            IOrderedEnumerable<StarSystem> sortResults = from system in systems
                                                         orderby GetColonizeValue(system, fleet)
                                                         select system;

            result = sortResults.Last();
            _text = "Step_7679:; "
                    + "GetBestColonyForColonize= >>> " + result.Location
                    + " " + result.Name
                    + ", Owner= " + result.Owner
                    ;
            if (writeDirectly) Console.WriteLine(_text);
            //GameLog.Client.AIDetails.DebugFormat("Best System for {0}, star ={1}, {2} {3}, value ={4}", fleet.Owner, result.Name, result.StarType, result.Location, GetColonizeValue(result, fleet.Owner));
            return true;  // returns a true for success and the found system
        }

        public static bool WeAreAtSystemToColonize(Fleet fleet)
        {
            GetFleetOwner(fleet);
            if (fleet == null)
            {
                throw new ArgumentNullException(nameof(fleet));
            }

            if (fleet.Sector != null && fleet.Sector.System != null && fleet.Sector.System.IsInhabited)
                return false;

            List<Fleet> colonizerFleets = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner)
                .Where(o => o.IsColonizer || o.MultiFleetHasAColonizer).ToList();

            // is next working correctly and not disturbing each other
            //List<Fleet> otherFleets = colonizerFleets.Where(o => o != fleet).ToList(); // other colony ships
            //_ = otherFleets.Remove(fleet);
            //if (otherFleets.Count != 0)
            //{
            //    bool anotherColonyShipGoing = GameContext.Current.Universe.Objects
            //    .Where(x => !otherFleets.Any(f => f.Route.Waypoints.LastOrDefault() == x.Location || x.Location == f.Location && f.Order is ColonizeOrder))
            //    .Any();
            //    if (anotherColonyShipGoing)
            //    {
            //        return false;
            //    }
            //}

            bool systemAtLocation = GameContext.Current.Universe.Objects
                .Where(a => a.Sector == fleet.Sector)
                .Where(b => b.ObjectType == UniverseObjectType.StarSystem)
                .Where(c => c.Sector.System.StarType != StarType.RadioPulsar && c.Sector.System.StarType != StarType.NeutronStar)
                .Any(c => c.Sector.System.IsHabitable(fleet.Owner.Race));

            return systemAtLocation;
        }

        /// <summary>
        /// Determines how valuable colonizing a particular <see cref="StarSystem"/>
        /// will be for a <see cref="Civilization"/>
        /// </summary>
        /// <param name="system"></param>
        /// <param name="civ"></param>
        /// <returns></returns>
        public static float GetColonizeValue(StarSystem system, Fleet fleet)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            if (fleet == null)
            {
                throw new ArgumentNullException(nameof(fleet));
            }


            //Alter this to alter priority
            const int DilithiumBonusValue = 20;
            const int DuraniumBonusValue = 20;

            float value = 0;
            value += 2000 - (100 * GetDistanceTo(fleet.Location, system.Location));

            if (system.HasDilithiumBonus)
            {
                value += DilithiumBonusValue;
            }

            if (system.HasDuraniumBonus)
            {
                value += DuraniumBonusValue;
            }

            value += system.GetMaxPopulation(fleet.Owner.Race) * system.GetGrowthRate(fleet.Owner.Race);

            //works
            //_text = "Step_1234:; "
            //         + " civ= " + fleet.Owner
            //         + ", Colonize-Value " + value
            //         + ", " + system.Location + " " + system.Name



            //        ;
            //if (writeDirectly) Console.WriteLine(_text);
            //GameLog.Core.AIDetails.DebugFormat("Colonize value for {0} is {1} for {2}", system, value, civ.Name);
            return value;
        }

        /// <summary>
        /// Get combat ship <see cref="Fleet"/> to protect colonyship
        /// </summary>
        /// <param name="colonyFleet"></param>
        /// <param name="colonySector"></param>
        /// <returns></returns>
        public static void GetFleetEscort(Fleet fleetToFollow, Sector finalSector)
        {
            GetFleetOwner(fleetToFollow);
            if (finalSector == null)
            {
                return;
            }
            List<Fleet> escortFleets = GameContext.Current.Universe.HomeColonyLookup[fleetToFollow.Owner].Sector.GetOwnedFleets(fleetToFollow.Owner)
                .Where(b => b.Sector == GameContext.Current.Universe.HomeColonyLookup[fleetToFollow.Owner].Sector).ToList();

            foreach (Fleet aFeet in escortFleets)
            {
                if (aFeet.Ships.Any(o => o.ShipType == ShipType.Cruiser || o.ShipType == ShipType.HeavyCruiser || o.ShipType == ShipType.FastAttack
                    && aFeet.Ships.Count() > 0
                    && aFeet.Owner == fleetToFollow.Owner
                    && aFeet.CanMove && aFeet.ClassName != "UNKNOWN"
                    && aFeet.Ships[0].ObjectID > 1))
                {
                    _ = aFeet.Ships.Sort((x, y) => y.ShipType.CompareTo(x.ShipType));

                    if (aFeet.Ships.Count() >= 1)
                    {
                        Ship ship = aFeet.Ships.Last();
                        MapLocation location = ship.Location;
                        aFeet.RemoveShip(ship);
                        fleetToFollow.AddShip(ship);
                        fleetToFollow.Location = location;
                        break;
                    }
                    _text = "Step_4367:; GetFleetEscort"
                        + "ESCORT= " + aFeet.ObjectID
                        + " " + aFeet.Name
                        + " " + aFeet.ClassName
                        + " " + aFeet.UnitAIType

                        + ", Act= " + aFeet.Activity
                        + ", UnitAIType= " + aFeet.UnitAIType
                        + ", Order= " + aFeet.Order

                        + " > going to= " + finalSector.Location
                        + " " + finalSector.Name
                        + ", steps= " + fleetToFollow.Route.Steps.Count

                        + " escorting " + fleetToFollow.ObjectID
                        + " " + fleetToFollow.Name

                            ;
                    if (writeDirectly) Console.WriteLine(_text);
                    //GameLog.Core.AIDetails.DebugFormat("ESCORT ={0} {1} unitAIType {2} activity {3} sector ={4} for ship ={5} {6} step count ={7}"
                    //, aFeet.Owner, aFeet.ClassName, aFeet.UnitAIType, aFeet.Activity, finalSector.Name, fleetToFollow.Owner, fleetToFollow.Name, fleetToFollow.Route.Steps.Count);
                    return;
                }
            }
        }

        private static void RemoveEscortShips(Fleet fleet, ShipType type)
        {
            //int shipCount = fleet.Ships.Count();
            GetFleetOwner(fleet);
            //List<Ship> listOfShips = new List<Ship>();
            Fleet newFleet = new Fleet();
            //foreach (Ship ship in fleet.Ships)
            //{
            //    listOfShips.Add(ship);
            //}
            List<Ship> listOfShips = fleet.Ships.ToList();
            MapLocation location;
            foreach (Ship ship in listOfShips)
            {
                if (ship.ShipType != type)
                {
                    location = ship.Location;
                    fleet.RemoveShip(ship);
                    newFleet.AddShip(ship);
                    newFleet.Location = location;
                    //if (location != GameContext.Current.CivilizationManagers[fleet.OwnerID].HomeSystem.Location)
                    //    newFleet.SetRoute(AStar.FindPath(newFleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { GameContext.Current.CivilizationManagers[fleet.Owner].HomeSystem.Sector }));
                    GameLog.Core.AIDetails.DebugFormat("RemoveEscortShips ship ={0} at {1}", ship.Name, ship.Location);

                }
            }
            newFleet.SetOrder(new IdleOrder());
            newFleet.Owner = fleet.Owner;
            newFleet.OwnerID = fleet.OwnerID;
            newFleet.UnitAIType = UnitAIType.NoUnitAI;  // no specific one
            newFleet.Activity = UnitActivity.NoActivity;

            var _rendezvousSector = GameContext.Current.CivilizationManagers[fleet.Owner].RendezvousSector;

            if (_rendezvousSector != null && newFleet.Location != _rendezvousSector.Location && _rendezvousSector.Location.ToString() != "(0, 0)")
            {
                newFleet.SetRoute(AStar.FindPath(newFleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { _rendezvousSector }));
            }
            else
            {
                newFleet.SetRoute(AStar.FindPath(newFleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { GameContext.Current.CivilizationManagers[fleet.Owner].HomeSystem.Sector }));
            }

            //GameLog.Core.AIDetails.DebugFormat("New Fleet route length {0}, Activity {1} UnitAIType {2}", fleet.Route.Length, fleet.Activity, fleet.UnitAIType);
        }

        /*
         * Exploration value
         */

        /// <summary>
        /// Determines how valuable it will be for the given <see cref="Fleet"/>
        /// to explore the given <see cref="Sector"/>
        /// </summary>
        /// <param name="sector"></param>
        /// <param name="fleet"></param>
        /// <returns></returns>
        public static int GetExploreValue(Sector sector, Civilization civ)
        {
            if (sector == null)
            {
                throw new ArgumentNullException(nameof(sector));
            }

            if (civ == null)
            {
                throw new ArgumentNullException(nameof(civ));
            }

            //These values are the priority of each item
            //const int UnscannedSectorValue = -100;
            const int UnexploredSectorValue = 200;
            const int HasStarSystemValue = 300;
            const int InitiatesFirstContactValue = 400;

            int value = 0;

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[civ];
            CivilizationMapData mapData = civManager.MapData;

            //Unexplored
            if (!mapData.IsExplored(sector.Location))
            {
                value += UnexploredSectorValue;
                //Unexplored star system
                if (sector.System != null)
                {
                    value += HasStarSystemValue;
                }
            }

            //First contact
            if (sector.System?.HasColony == true && (sector.System.Colony.Owner != civ) && !DiplomacyHelper.IsContactMade(sector.Owner, civ))
            {
                value += InitiatesFirstContactValue;
            }

            //GameLog.Core.AIDetails.DebugFormat("Explore priority for {0} is {1}", sector, value);
            return value;
        }

        /*
       * Explor best sector
       */

        /// <summary>
        /// Gets the best <see cref="Sector"/> to explore for the given <see cref="Fleet"/>
        /// </summary>
        /// <param name="fleet"></param>
        /// <returns></returns>
        public static bool GetBestSectorToExplore(Fleet fleet, out Sector sector)
        {
            GetFleetOwner(fleet);
            if (fleet == null)
            {
                throw new ArgumentNullException(nameof(fleet));
            }
            if (fleet.Ships.Count() == 0)
            {
                sector = null;
                return false;
            }
            List<Fleet> ownFleets = new List<Fleet>();
            try
            {
                ownFleets = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner).Where(f => f.CanMove && f != fleet && !f.Route.IsEmpty).ToList();
            }
            catch (Exception e)
            {
                if (fleet != null)
                {
                    GameLog.Client.General.ErrorFormat("fleet.ObjectId ={0} {1} {2} error ={3}", fleet.ObjectID, fleet.Name, fleet.ClassName, e);
                }
                else { GameLog.Client.General.Error(e); }
            }

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
            CivilizationMapData mapData = civManager.MapData;
            List<StarSystem> starsToExplore = new List<StarSystem>();
            List<Sector> sectorsToExplore = new List<Sector>();
            GetFleetOwner(fleet);



            if (fleet.Owner != null)
            {
                starsToExplore = GameContext.Current.Universe.Find<StarSystem>()
                    //We need to know about it (no cheating)
                    .Where(s => mapData.IsScanned(s.Location) && s.Owner != fleet.Owner
                    //&& (!s.IsOwned || (s.Owner != fleet.Owner))
                    && s.StarType != StarType.BlackHole
                        && s.StarType != StarType.XRayPulsar
                        && s.StarType != StarType.NeutronStar
                        && s.StarType != StarType.Quasar
                        && s.StarType != StarType.RadioPulsar
                        && s.StarType != StarType.XRayPulsar
                        && s.StarType != StarType.Wormhole
                    && DiplomacyHelper.IsTravelAllowed(fleet.Owner, s.Sector)
                    && FleetHelper.IsSectorWithinFuelRange(s.Sector, fleet)
                    && !ownFleets.Any(f => f.Route.Waypoints.Any(wp => s.Location == wp)))
                    //No point exploring our own space
                    //Where we can enter the sector
                    //Where is in fuel range of the ship
                    //Where no fleets are already heading there or through there
                    .ToList();
            }

            if (starsToExplore.Count > 0)
            {
                starsToExplore.Sort((a, b) =>
                    (GetExploreValue(a.Sector, fleet.Owner) - HomeSystemDistanceModifier(fleet, a.Sector))
                    .CompareTo(GetExploreValue(b.Sector, fleet.Owner) - HomeSystemDistanceModifier(fleet, b.Sector)));
                sector = starsToExplore[starsToExplore.Count() - 1].Sector;
                return true;
            }
            else
            {
                sector = civManager.RendezvousSector;
                return true;
            }

            //if (fleet.Owner != null)(s)
            //{
            //    sectorsToExplore = GameContext.Current.SectorClaims.wh
            //    //sectorsToExplore = GameContext.Current.Universe.Map.Find<_sectors>()
            //        //We need to know about it (no cheating)
            //        .Where(s => s.Owner == fleet.Owner
            //        && DiplomacyHelper.IsTravelAllowed(fleet.Owner, s)
            //        && FleetHelper.IsSectorWithinFuelRange(s, fleet)
            //        && !ownFleets.Any(f => f.Route.Waypoints.Any(wp => s.Location == wp)))
            //        //No point exploring our own space
            //        //Where we can enter the sector
            //        //Where is in fuel range of the ship
            //        //Where no fleets are already heading there or through there
            //        .ToList();
            //}

            //sectorsToExplore.Sort((a, b) =>
            //    (GetExploreValue(a, fleet.Owner) - HomeSystemDistanceModifier(fleet, a))
            //    .CompareTo(GetExploreValue(b, fleet.Owner) - HomeSystemDistanceModifier(fleet, b)));
            //sector = starsToExplore[starsToExplore.Count() - 1].Sector;
            //return true;

            // End of GetBestSectorToExplor
        }

        /*
         * NoOneElseTakingSystem
         */
        public static bool SystemIsAlreadyTaken(Fleet fleet)
        {
            GetFleetOwner(fleet);
            //_text = "Check for SystemNotAlreadyTaken....";
            if (writeDirectly) Console.WriteLine(_text);
            bool otherColony = GameContext.Current.Universe.Objects.Where(o => o.Location == fleet.Location && o.ObjectType == UniverseObjectType.Colony && o.Owner != fleet.Owner).Any();
            //_text = "Searching for Crash: SystemNotAlreadyTaken-2=Station";
            if (writeDirectly) Console.WriteLine(_text);
            bool otherStation = GameContext.Current.Universe.Objects.Where(o => o.Location == fleet.Location && o.ObjectType == UniverseObjectType.Station).Any();
            if (otherColony || otherStation)
            {
                return true;
            }

            return false;
        }

        /*
        * Station value
        */

        /// <summary>
        /// Determines how valuable a <see cref="Station"/> would be
        /// in a given <see cref="Sector"/>
        /// </summary>
        /// <param name="sector"></param>
        /// <param name="fleet"></param>
        /// <returns></returns>
        public static int GetStationValue(Sector sector, Fleet fleet, List<UniverseObject> universeObjects)
        {
            //GameLog.Client.AIDetails.DebugFormat("GetStationValue");
            GetFleetOwner(fleet);
            //GameLog.Client.AIDetails.DebugFormat("GetFleetOwner");
            if (sector == null)
            {
                //GameLog.Client.AIDetails.DebugFormat("null");
                throw new ArgumentNullException(nameof(sector));
            }
            if (sector.Station != null
                || sector.GetFleets().Where(o => o.UnitAIType == UnitAIType.Constructor) != null && sector.GetFleets().Any(o => o.Activity == UnitActivity.BuildStation) 
                || sector.System != null 
                    && (sector.System.StarType == StarType.BlackHole 
                    || sector.System.StarType == StarType.NeutronStar
                    || sector.System.StarType == StarType.RadioPulsar 
                    || sector.System.StarType == StarType.XRayPulsar)
                || (sector.Owner != null && sector.Owner.CivID <= 6))
            //|| sector.GetNeighbors().Where(o => o.Owner != null && o.Owner.CivID <= 6).Any())   
            {
                return -4000;
                //GameLog.Client.AIDetails.DebugFormat("A");
            }

            int value = 1;
            //_text = "Searching for Crash: someObject";
            //if (writeDirectly) Console.WriteLine(_text);

            IEnumerable<UniverseObject> someObject = universeObjects.Where(o => o.Sector == sector);
            if (someObject != null)
            {
                const int SystemSectorValue = 2500;
                const int StrandedShipSectorValue = 5000;
                const int PastFuelRange = 30000;
                const int DistanceFactor = -1500; // was 100

                if (!FleetHelper.IsSectorWithinFuelRange(sector, fleet))
                {
                    value += PastFuelRange;
                    // GameLog.Client.AIDetails.DebugFormat("B");
                }

                if ((sector.System != null)
                    && (sector.System.Owner == null || sector.System.OwnerID > 6))
                {
                    value += SystemSectorValue;
                    if (sector.System.StarType == StarType.Blue
                        || sector.System.StarType == StarType.Orange
                        || sector.System.StarType == StarType.Red
                        || sector.System.StarType == StarType.White
                        || sector.System.StarType == StarType.Yellow
                        || sector.System.StarType == StarType.Wormhole)
                    {
                        value += SystemSectorValue;
                        //GameLog.Client.AIDetails.DebugFormat("C");
                    }
                }
                Sector homeSector = GameContext.Current.CivilizationManagers[fleet.OwnerID].HomeSystem.Sector;
                //Sector homeSector = GameContext.Current.Universe.HomeColonyLookup[fleet.Owner].Sector; // crashes e.g. for Borg

                try
                {
                    int distance = MapLocation.GetDistance(sector.Location, homeSector.Location);

                    value += DistanceFactor * distance;
                }
                catch
                {
                    _text = "Step_6912:; " + _fleetText + " > ### unable to get furthest object from home world for station value"
                            //+ " " + fleet.ObjectID
                            //+ " " + fleet.Name
                            ////+ " to go to " + bestSystemToColonize.Name
                            //+ " " + fleet.Location
                            ;
                    if (writeDirectly) Console.WriteLine(_text);
                    _fleet_Text += newline + _text;
                    //GameLog.Client.AIDetails.DebugFormat("unable to get furthest object from home world for station value");
                }

                List<Sector> strandedShipSectors = FindStrandedShipSectors(fleet.Owner); //altering collection while sorting it!!!!!!!!!!!!!!
                if (strandedShipSectors.Count > 0)
                {
                    if (strandedShipSectors.Contains(sector))
                    {
                        value += StrandedShipSectorValue;
                        //GameLog.Client.AIDetails.DebugFormat("D");
                    }
                }
            }
            //GameLog.Core.AIDetails.DebugFormat("Station at {0} has value {1}", sector.Location, (value + randomInt));

            //_text = "Step_6914:; " + _fleetText + " > "
            //        //+ " " + fleet.ObjectID
            //        //+ " " + fleet.Name
            //        ////+ " to go to " + bestSystemToColonize.Name
            //        //+ " " + fleet.Location
            //        ;
            //if (writeDirectly) Console.WriteLine(_text);
            //_fleet_Text += newline + _text;

            return value; // + randomInt;


        }

        public static void BuildStation(Fleet fleet)
        {
            GetFleetOwner(fleet);
            // GameLog.Core.AIDetails.DebugFormat("Constructor fleet {0} build station at {1}, {2} UnitActivity = {3}", fleet.Owner.Key, fleet.Sector.Name, fleet.Location, fleet.Activity.ToString());
            BuildStationOrder order = new BuildStationOrder();
            order.BuildProject = order.FindTargets(fleet).Cast<StationBuildProject>().Last(); // OrDefault(o => o.StationDesign.IsCombatant);
            if (order.BuildProject != null && order.CanAssignOrder(fleet) && fleet.Activity != UnitActivity.BuildStation)
            {
                fleet.SetOrder(order);
                fleet.UnitAIType = UnitAIType.Building;
                fleet.Activity = UnitActivity.BuildStation;

                _text = "Step_3700:; "
                    + fleet.Location
                    + " > Constructor fleet " + blank + fleet.ObjectID
                    + blank + fleet.Name
                    + blank + fleet.ClassName
                    + " has order= " + fleet.Order.OrderName
                    + ", activity= " + fleet.Activity.ToString()
                    ;
                if (writeDirectly) Console.WriteLine(_text);
                //GameLog.Core.AIDetails.DebugFormat("Constructor fleet {0} order {1} at {2} UnitActivity = {3}", fleet.Owner.Key, fleet.Order.OrderName, fleet.Sector.Name, fleet.Activity.ToString());
            }

        }
        private static void BuildStation(Fleet fleet, List<Fleet> allFleets)
        {
            fleet.Route.Clear();
            if (allFleets.Count() > 1) // for any other constructor fleets here?
            {
                for (int i = 0; i < allFleets.Count; i++)
                {
                    if (i == 0)
                    {
                        BuildStation(fleet);
                    }
                    else
                    {  // stay and help build
                        allFleets[i].UnitAIType = UnitAIType.Building;
                        allFleets[i].Activity = UnitActivity.Hold;
                    }
                }
            }
            else
            {
                BuildStation(fleet);
            }
        }

        /*
        * Station best sector
        */

        /// <summary>
        /// Returns the best possible <see cref="Sector"/> for a given <see cref="Fleet"/>
        /// to build a <see cref="Station"/> in
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool GetBestSectorForStation(Fleet fleet, List<Fleet> constructionFleets, out Sector bestSector)
        {
            _text = "Step_6520:; " + fleet.Location + blank + fleet.Name + " GetBestSectorForStation.... ";
            if (writeDirectly) Console.WriteLine(_text);
            //GameLog.Client.AIDetails.DebugFormat(_text);
            GetFleetOwner(fleet);
            if (fleet == null)
            {
                throw new ArgumentNullException(nameof(fleet));
            }
            if (fleet.Ships.Count == 0)
            {
                bestSector = null;
                return false;
            }

            bool _write = writeDirectly;
            //_write = true;
            _write = false;

            _text = "GetBestSectorForStation: ";
            int halfMapWidthX = GameContext.Current.Universe.Map.Width / 2;
            _text += newline + "halfMapWidthX= " + halfMapWidthX.ToString();
            if (_write) Console.WriteLine(halfMapWidthX);

            int halfMapHeightY = GameContext.Current.Universe.Map.Height / 2;
            _text += newline + "halfMapHeightY= " + halfMapHeightY.ToString();
            if (_write) Console.WriteLine(halfMapHeightY);

            int thirdMapWidthX = GameContext.Current.Universe.Map.Width / 3;
            _text += newline + "thirdMapWidthX= " + thirdMapWidthX.ToString();
            if (_write) Console.WriteLine(thirdMapWidthX);

            int thirdMapHeightY = GameContext.Current.Universe.Map.Height / 3;
            _text += newline + "thirdMapHeightY= " + thirdMapHeightY.ToString();
            if (_write) Console.WriteLine(thirdMapHeightY);

            int quarterMapWidthX = GameContext.Current.Universe.Map.Width / 4;
            _text += newline + "quarterMapWidthX= " + quarterMapWidthX.ToString();
            if (_write) Console.WriteLine(quarterMapWidthX);

            int quarterMapHeightY = GameContext.Current.Universe.Map.Height / 4;
            _text += newline + "quarterMapHeightY= " + quarterMapHeightY.ToString();
            if (_write) Console.WriteLine(quarterMapHeightY);

            int lengthQuarterMap = (int)Math.Sqrt((int)Math.Pow(quarterMapWidthX, 2) + (int)Math.Pow(quarterMapHeightY, 2));
            _text += newline + "lengthQuarterMap= " + lengthQuarterMap.ToString();
            if (_write) Console.WriteLine(lengthQuarterMap);

            int lengthThirdMap = (int)Math.Sqrt((int)Math.Pow(thirdMapWidthX, 2) + (int)Math.Pow(thirdMapHeightY, 2));
            _text += newline + "lengthThirdMap= " + lengthThirdMap.ToString();
            if (_write) Console.WriteLine(lengthThirdMap);

            if (_write) Console.WriteLine(_text);



            switch (fleet.Owner.Key)
            {
                case "BORG":
                //{
                //    _text = "Step_6560: GetBestSectorForStation for Borg";
                //    if (writeDirectly) Console.WriteLine(_text);
                //    //GameLog.Client;.AIDetails.DebugFormat(_text);
                //    int borgX = GameContext.Current.Universe.HomeColonyLookup[fleet.Owner].Location.X;
                //    int borgXDelta = Math.Abs(GameContext.Current.Universe.HomeColonyLookup[fleet.Owner].Location.X - halfMapWidthX) / 4;
                //    int borgY = GameContext.Current.Universe.HomeColonyLookup[fleet.Owner].Location.Y;
                //    int borgYDelta = Math.Abs(halfMapHeightY - GameContext.Current.Universe.HomeColonyLookup[fleet.Owner].Location.Y) / 4;

                //    MapLocation borgHomeLocation = GameContext.Current.Universe.HomeColonyLookup[fleet.Owner].Location;
                //    List<UniverseObject> objectsAlongCenterAxis = GameContext.Current.Universe.Objects
                //        .Where(s => s.Location != null
                //        && s.Sector.Station == null
                //        && s.Location.X >= halfMapWidthX + borgXDelta && s.Location.X <= borgX
                //        && s.Location.Y <= Math.Abs(halfMapHeightY - borgYDelta) && s.Location.Y >= borgY + borgYDelta)
                //        //&& s.Location == borgHomeLocation)                         
                //        .ToList();

                //    if (objectsAlongCenterAxis.Count == 0)
                //    {
                //        bestSector = null;
                //        return false;
                //    }
                //    // GameLog.Core.AIDetails.DebugFormat("{0} Universe Objects for {1} station search", objectsAlongCenterAxis.Count(), fleet.Owner.Key);
                //    try
                //    {
                //        objectsAlongCenterAxis.Sort((a, b) =>
                //            GetStationValue(a.Sector, fleet, objectsAlongCenterAxis)
                //            .CompareTo(GetStationValue(b.Sector, fleet, objectsAlongCenterAxis)));
                //    }
                //    catch
                //    {
                //        _text = "unable to sort objects for Borg station location";
                //        if (writeDirectly) Console.WriteLine(_text);
                //        GameLog.Client.AIDetails.DebugFormat(_text);
                //        bestSector = null;
                //        return false;
                //    }
                //    List<Fleet> otherConstructors = GameContext.Current.Universe.Find<Fleet>()
                //            .Where(o => o.Owner != fleet.Owner && o.IsConstructor || o.MultiFleetHasAConstructor).ToList();
                //    List<Sector> _sectors = new List<Sector>();
                //    foreach (UniverseObject anObject in objectsAlongCenterAxis)
                //    {
                //        _sectors.Add(anObject.Sector);
                //    }
                //    foreach (Fleet aFleet in otherConstructors)
                //    {
                //        if (_sectors.Any(o => o == aFleet.Sector))
                //        {
                //            _ = _sectors.Remove(aFleet.Sector);
                //        }
                //    }

                //    bestSector = _sectors.Last(); //objectsAlongCenterAxis[objectsAlongCenterAxis.Count - 1].Sector;

                //    if (bestSector == null)
                //    {
                //        return false;
                //    }

                //    _text = "Borg station selected sector  at " + bestSector.Location + " " + bestSector.Name;
                //    if (writeDirectly) Console.WriteLine(_text);
                //    // GameLog.Core.AIDetails.DebugFormat(_text);
                //    return true;
                //}

                case "DOMINION":
                //{
                //    _text = "Step_6580: GetBestSectorForStation for DOMINION";
                //    if (writeDirectly) Console.WriteLine(_text);
                //    //GameLog.Client.AIDetails.DebugFormat("Dominion");
                //    int domX = GameContext.Current.Universe.HomeColonyLookup[fleet.Owner].Location.X;
                //    if (writeDirectly) Console.WriteLine(domX);
                //    int domXDelta = Math.Abs(halfMapWidthX - GameContext.Current.Universe.HomeColonyLookup[fleet.Owner].Location.X) / 4;
                //    if (writeDirectly) Console.WriteLine(domXDelta);
                //    int domY = GameContext.Current.Universe.HomeColonyLookup[fleet.Owner].Location.Y;
                //    if (writeDirectly) Console.WriteLine(domY);
                //    int domYDelta = Math.Abs(halfMapHeightY - GameContext.Current.Universe.HomeColonyLookup[fleet.Owner].Location.Y) / 4;
                //    if (writeDirectly) Console.WriteLine(domYDelta);

                //    List<UniverseObject> objectsAlongCenterAxis = GameContext.Current.Universe.Objects
                //        // .Where(c => !FleetHelper.IsSectorWithinFuelRange(c.Sector, fleet))
                //        .Where(s => s.Location != null
                //        && s.Sector.Station == null
                //        && s.Location.X <= Math.Abs(halfMapWidthX - domXDelta) && s.Location.X > domX
                //        && s.Location.Y <= halfMapHeightY - domYDelta && s.Location.Y > domY)
                //        // find a list of objects in some sector around Dom side of galactic center
                //        .ToList();
                //    //foreach (var item in objectsAlongCenterAxis)
                //    //{
                //    //    _text = item.Location
                //    //        + " " + item.Name
                //    //        + " " + item.ObjectID

                //    //        ;
                //    //    if (writeDirectly) Console.WriteLine(_text);

                //    //    //if (item.ObjectType != )
                //    //}


                //    if (objectsAlongCenterAxis.Count == 0)
                //    {
                //        bestSector = null;
                //        return false;
                //    }
                //    //GameLog.Core.AIDetails.DebugFormat("{0} Universe Objects for {1} station search", objectsAlongCenterAxis.Count(), fleet.Owner.Key);

                //    try
                //    {
                //        _text = "try objectsAlongCenterAxis... for fleet" + fleet.ObjectID + " " + fleet.Name + " " + fleet.ClassName;
                //        if (writeDirectly) Console.WriteLine(_text);

                //        objectsAlongCenterAxis.Sort((a, b) =>
                //       GetStationValue(a.Sector, fleet, objectsAlongCenterAxis)
                //       .CompareTo(GetStationValue(b.Sector, fleet, objectsAlongCenterAxis)));

                //    }
                //    catch
                //    {
                //        _text = "unable to sort objects for Dominion station location";
                //        if (writeDirectly) Console.WriteLine(_text);
                //        GameLog.Client.AIDetails.DebugFormat(_text);
                //        bestSector = null;
                //        return false;
                //    }
                //    bestSector = objectsAlongCenterAxis.Last().Sector; //[objectsAlongCenterAxis.Count - 1].Sector;
                //    if (bestSector == null)
                //    {
                //        return false;
                //    }

                //    _text = "Dominion station selected sector  at " + bestSector.Location + " " + bestSector.Name;
                //    if (writeDirectly) Console.WriteLine(_text);
                //    // GameLog.Core.AIDetails.DebugFormat(_text);
                //    return true;
                //}
                case "KLINGONS":
                case "TERRANEMPIRE":
                case "FEDERATION":
                case "ROMULANS":
                case "CARDASSIANS":
                    {
                        //GameLog.Client.AIDetails.DebugFormat("Klingons to Cardassians");
                        //var furthestObject = GameContext.Current.Universe.FindFurthestObject<UniverseObject>(homeSector.Location, fleet.Owner);
                        Sector homeSector = GameContext.Current.Universe.HomeColonyLookup[fleet.Owner].Sector;

                        int xloc = GameContext.Current.Universe.Map.Width / 2;
                        int yloc = GameContext.Current.Universe.Map.Height / 2;

                        MapLocation _center = new MapLocation(xloc, yloc);


                        List <UniverseObject> objectsAroundHome = GameContext.Current.Universe.Objects
                            .Where(s => s.Location != null
                            && s.ObjectType != UniverseObjectType.Ship
                            && s.ObjectType != UniverseObjectType.Fleet
                            //&& s.ObjectType != UniverseObjectType.StarSystem.bla
                            && s.Sector.Station == null
                            && s.Sector.Owner == null
                            && GetDistanceTo(_center, s.Location) < xloc -3)  // do not build at the edge of the map
                            //&& GetDistanceTo(_center, s.Location) < lengthThirdMap)
                            .ToList();

                        if (_checkFleetOrders == true && _checkOnlyPlayersUnits)
                            _text += ""; // dummy

                        if (objectsAroundHome.Count == 0)
                        {
                            bestSector = null;
                            return false;
                        }

                        // GameLog.Core.AIDetails.DebugFormat("{0} Universe Objects for {1} station search", objectsAroundHome.Count(), fleet.Owner.Key);
                        try
                        {
                            objectsAroundHome.Sort((a, b) =>
                                          GetStationValue(a.Sector, fleet, objectsAroundHome)
                                          .CompareTo(GetStationValue(b.Sector, fleet, objectsAroundHome)));
                        }
                        catch { bestSector = null; return false; }

                        try
                        {
                            //string _obj = "";
                            foreach (var item in objectsAroundHome)
                            {
                                int distance = MapLocation.GetDistance(item.Location, homeSector.Location);
                                _text = "Step_6915:; " 
                                     + "" + _fleetText
                                     + ">  DistfromHome > " + GameEngine.Do_2_Digit(distance.ToString())
                                     + ", Value for Station= " + GetStationValue(item.Sector, fleet, objectsAroundHome)
                                     + " for " + item.Location
                                     + " " + item.Name

                                    ;
                                //if (writeDirectly) Console.WriteLine(_text);
                                _fleet_Text += newline + _text;

                                //_obj += newline + _text;
                            }
                            //if (writeDirectly) Console.WriteLine(_obj);

                            if (_checkFleetOrders == true && _checkOnlyPlayersUnits)
                                _text += ""; // dummy

                            

                            bestSector = objectsAroundHome.Last().Sector; //[objectsAroundHome.Count - 1].Sector;
                            _text = "Step_6550:; " + bestSector.Location + " Name = " + bestSector.Name + ": sector selected for station build for " + fleet.Owner.Key;
                            if (writeDirectly) Console.WriteLine(_text);
                            _fleetText += newline + _text;

                            if (_checkFleetOrders == true && _checkOnlyPlayersUnits)
                                _text += ""; // dummy

                            // GameLog.Core.AIDetails.DebugFormat(_text);
                        }
                        catch { bestSector = null; return false; }

                        return bestSector != null;
                    }
                default:
                    bestSector = null;
                    _text = fleet.Owner.Key + " no sector for station";
                    if (writeDirectly) Console.WriteLine(_text);
                    //GameLog.Core.AIDetails.DebugFormat(_text);

                    if (_checkFleetOrders == true && _checkOnlyPlayersUnits)
                        _text += ""; // dummy

                    return false; // could not find sector for station
            }
        }

        /// <summary>
        /// Determines the value of a <see cref="Civilization"/> providing
        /// medical services to a <see cref="Colony"/>
        /// </summary>
        /// <param name="colony"></param>
        /// <param name="civ"></param>
        /// <returns></returns>
        public static int GetMedicalValue(Colony colony, Civilization civ)
        {
            if (colony == null)
            {
                throw new ArgumentNullException(nameof(colony));
            }

            if (civ == null)
            {
                throw new ArgumentNullException(nameof(civ));
            }

            //Tweak these to set priorities
            const int OwnColonyPriority = 100;
            const int AlliedColonyPriority = 15;
            const int FriendlyColonyPriority = 10;
            const int NeutralColonyPriority = 5;

            int value = 0;

            if (colony.Owner == civ)
            {
                value += OwnColonyPriority;
            }
            else if (DiplomacyHelper.AreAllied(colony.Owner, civ))
            {
                value += AlliedColonyPriority;
            }
            else if (DiplomacyHelper.AreFriendly(colony.Owner, civ))
            {
                value += FriendlyColonyPriority;
            }
            else if (DiplomacyHelper.AreNeutral(colony.Owner, civ))
            {
                value += NeutralColonyPriority;
            }

            value += 100 - colony.Health.CurrentValue;
            // GameLog.Core.AIDetails.DebugFormat("Medical value for {0} is {1})", colony, value);
            return value;
        }

        /*
        * Medical best colony
        */

        /// <summary>
        /// Determines the best <see cref="Colony"/> for a <see cref="Fleet"/>
        /// to provide medical services to
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool GetBestColonyForMedical(Fleet fleet, out Colony result)
        {
            GetFleetOwner(fleet);
            if (fleet == null)
            {
                throw new ArgumentNullException(nameof(fleet));
            }

            List<Fleet> medFleets = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner)
                .Where(o => o.IsMedical).ToList();
            List<Fleet> otherFleets = medFleets.Where(o => o != fleet).ToList();

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
            CivilizationMapData mapData = civManager.MapData;
            IEnumerable<Fleet> medicalShips = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner).Where(s => s.IsMedical);
            List<Colony> possibleColonies = new List<Colony>();
            List<Colony> _col_available = new List<Colony>();
            GetFleetOwner(fleet);
            if (fleet.Owner != null)
            {
                _col_available = GameContext.Current.Universe.Find<Colony>()
                    .Where(h => h.Health.CurrentValue < 90)
                    .Where(s => mapData.IsScanned(s.Location) && mapData.IsExplored(s.Location))
                    .Where(d => d.Owner.Key == fleet.Owner.Key)
                    .ToList();

                _text = "Step_7678:; " + CreateFleetText(fleet, out _fleetText) + " > GetBestColonyForMedical...found= "  + _col_available.Count;

                foreach (var _col in _col_available)
                {
                    //_text += " > " + _col.Location + " "
                }
                if (writeDirectly) Console.WriteLine(_text);


                possibleColonies = GameContext.Current.Universe.Find<Colony>()
                    // where health < 90
                    .Where(h => h.Health.CurrentValue < 90)
                //We need to know about it (no cheating)
                .Where(s => mapData.IsScanned(s.Location) && mapData.IsExplored(s.Location))
                // Borg do not play well with others
                .Where(d => d.Owner.Key == fleet.Owner.Key /*|| !d.Owner.IsEmpire*/)  // doesn't work
                //.Where(d => d.Owner.Key != "BORG" && fleet.Owner.Key != "BORG" && ) // old
                //.Where(d => d.Owner.Key == "BORG" && fleet.Owner.Key == "BORG")
                //In fuel range
                .Where(c => FleetHelper.IsSectorWithinFuelRange(c.Sector, fleet)
                //Where we can enter the sector
                //Where there aren't any hostiles
                //Where they aren't at war
                && c.Owner.Key == fleet.Owner.Key
                && DiplomacyHelper.IsTravelAllowed(fleet.Owner, c.Sector)
                && GameContext.Current.Universe.FindAt<Orbital>(c.Location).Any(o => DiplomacyHelper.ArePotentialEnemies(fleet.Owner, o.Owner))
                && !DiplomacyHelper.AreAtWar(c.Owner, fleet.Owner))
                //Where other med ship is not already going
                .Where(d => !otherFleets.Any(f => f.Route.Waypoints.LastOrDefault() == d.Location || d.Location == f.Location && f.Order is MedicalOrder))
                .ToList();
            }

            if (possibleColonies.Count == 0 )
            {
                result = null;
                _text = "Step_7674:; " + CreateFleetText(fleet, out _fleetText) + " > GetBestColonyForMedical > no colony found ";
                if (writeDirectly) Console.WriteLine(_text);

                return false;
            }

            possibleColonies.Sort((a, b) =>
                (GetMedicalValue(a, fleet.Owner) * HomeSystemDistanceModifier(fleet, a.Sector))
                .CompareTo(GetMedicalValue(b, fleet.Owner) * HomeSystemDistanceModifier(fleet, b.Sector)));

            result = possibleColonies[possibleColonies.Count - 1];
            _text = "Step_7677:; " + CreateFleetText(fleet, out _fleetText)
                    + "GetBestColonyForMedical= " + result.Location
                    + " > " + result.Name
                    + ", Owner= " + result.Sector.Owner
                    ;
            if (writeDirectly) Console.WriteLine(_text);
            return true;
        }

        /*
        * Diplomacy value
        */

        /// <summary>
        /// Determines the value diplomacy <see cref="Colony"/>
        /// to a given <see cref="Civilization"/>
        /// </summary>
        /// <param name="colony"></param>
        /// <param name="civ"></param>
        /// <returns></returns>
        public static int GetDiplomaticValue(Colony colony, Civilization civ)
        {
            if (colony == null)
            {
                throw new ArgumentNullException(nameof(colony));
            }

            if (civ == null)
            {
                throw new ArgumentNullException(nameof(civ));
            }

            Civilization otherCiv = colony.Owner;
            if (colony.System.Name != otherCiv.HomeSystemName)
            {
                return 0;
            }

            Diplomat diplomat = Diplomat.Get(civ);

            if (otherCiv.CivID == civ.CivID)
            {
                return 0;
            }

            if (!DiplomacyHelper.IsContactMade(civ.CivID, otherCiv.CivID))
            {
                return 0;
            }

            ForeignPower foreignPower = diplomat.GetForeignPower(otherCiv);
            Diplomat otherdiplomat = Diplomat.Get(otherCiv);
            ForeignPower otherForeignPower = otherdiplomat.GetForeignPower(civ);
            if (foreignPower.DiplomacyData.Status == ForeignPowerStatus.OwnerIsMember || otherForeignPower.DiplomacyData.Status == ForeignPowerStatus.OwnerIsMember)
            {
                return 0;
            }

            #region Foreign Traits List

            string traitsOfForeignCiv = otherCiv.Traits;
            string[] foreignTraits = traitsOfForeignCiv.Split(',');

            #endregion Foreign Traits List

            #region The Civ's Traits List

            string traitsOfCiv = civ.Traits;
            string[] theCivTraits = traitsOfCiv.Split(',');

            #endregion The Civ's Traits List

            // traits in common relative to the number of triats a civilization has
            IEnumerable<string> commonTraitItems = foreignTraits.Intersect(theCivTraits);

            int countCommon = 0;
            foreach (string aString in commonTraitItems)
            {
                countCommon++;
            }

            int[] countArray = new int[] { foreignTraits.Length, theCivTraits.Length };
            int fewestTotalTraits = countArray.Min();

            int similarTraits = countCommon * 10 / fewestTotalTraits;

            const int EnemyColonyPriority = 0;
            const int NeutralColonyPriority = 10;
            const int FriendlyColonyPriority = 5;

            //int value = similarTraits;  // just for Gamelog below

            if (DiplomacyHelper.AreAllied(otherCiv, civ) || DiplomacyHelper.AreFriendly(otherCiv, civ))
            {
                similarTraits += FriendlyColonyPriority;
            }
            else if (DiplomacyHelper.AreNeutral(otherCiv, civ))
            {
                similarTraits += NeutralColonyPriority;
            }
            else if (DiplomacyHelper.AreAtWar(otherCiv, civ))
            {
                similarTraits += EnemyColonyPriority;
            }

            //GameLog.Core.AIDetails.DebugFormat("diplomacy value for {0} belonging to {1} is {2} to the {3}", colony.Name, otherCiv.Key, value, civ.Key);
            return similarTraits;
        }

        /*
        * Diplomacy best colony
        */

        /// <summary>
        /// Determines the best <see cref="Colony"/> for a <see cref="Fleet"/>
        /// to provide medical services to
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool GetBestColonyForDiplomacy(Fleet fleet, out Colony result)
        {
            GetFleetOwner(fleet);
            if (fleet == null)
            {
                throw new ArgumentNullException(nameof(fleet));
            }

            List<Fleet> diplomacyFleets = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner)
                .Where(o => o.IsDiplomatic).ToList();
            List<Fleet> otherFleets = diplomacyFleets.Where(o => o != fleet).ToList();

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
            CivilizationMapData mapData = civManager.MapData;
            IEnumerable<Fleet> diplomaticShips = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner).Where(s => s.IsDiplomatic);
            List<Colony> possibleColonies = new List<Colony>();
            GetFleetOwner(fleet);
            if (fleet.Owner != null)
            {
                possibleColonies = GameContext.Current.Universe.Find<Colony>()
                //We need to know about it (no cheating)
                .Where(s => mapData.IsScanned(s.Location)
                && mapData.IsExplored(s.Location)
                && s.Owner != fleet.Owner)
                //In fuel range
                .Where(c => FleetHelper.IsSectorWithinFuelRange(c.Sector, fleet)
                //Where we can enter the sector
                //Where there aren't any hostiles
                //Where they aren't at war
                && DiplomacyHelper.IsTravelAllowed(fleet.Owner, c.Sector)
                && GameContext.Current.Universe.FindAt<Orbital>(c.Location).Any(o => DiplomacyHelper.ArePotentialEnemies(fleet.Owner, o.Owner))
                && !DiplomacyHelper.AreAtWar(c.Owner, fleet.Owner))
                //Where other diploatic is not already going
                .Where(d => !otherFleets.Any(f => f.Route.Waypoints.LastOrDefault() == d.Location || d.Location == f.Location && f.Order is SpyOnOrder))
                .ToList();
            }

            if (possibleColonies.Count == 0)
            {
                result = null;
                return false;
            }

            possibleColonies.Sort((a, b) =>
                (GetDiplomaticValue(a, fleet.Owner) * HomeSystemDistanceModifier(fleet, a.Sector))
                .CompareTo(GetDiplomaticValue(b, fleet.Owner) * HomeSystemDistanceModifier(fleet, b.Sector)));
            result = possibleColonies[possibleColonies.Count - 1];



            _text = "Step_7676:; " + CreateFleetText(fleet, out string _fleetText)
                + "GetBestColonyForDiplomacy= " + result.Location
                + " > " + result.Name
                + ", Owner= " + result.Owner
                //+ ", for " 
                ;
            if (writeDirectly) Console.WriteLine(_text);
            if (_checkFleetOrders == true && _checkOnlyPlayersUnits)
                _text += ""; // dummy
            return true;
        }

        /*
         * Spying value
         */

        /// <summary>
        /// Determines the value of spying on a <see cref="Colony"/>
        /// to a given <see cref="Civilization"/>
        /// </summary>
        /// <param name="colony"></param>
        /// <param name="civ"></param>
        /// <returns></returns>
        public static int GetSpyingValue(Colony colony, Civilization civ)
        {
            if (colony == null)
            {
                throw new ArgumentNullException(nameof(colony));
            }

            if (civ == null)
            {
                throw new ArgumentNullException(nameof(civ));
            }
            Civilization otherCiv = colony.Owner;
            if (colony.System.Name == otherCiv.HomeSystemName)
            {
                return 1000;
            }

            if (otherCiv.CivID == civ.CivID)
            {
                return 0;
            }

            if (!DiplomacyHelper.IsContactMade(civ.CivID, otherCiv.CivID))
            {
                return 0;
            }

            const int EnemyColonyPriority = 50;
            const int NeutralColonyPriority = 25;
            const int FriendlyColonyPriority = 10;

            int value = 0;

            if (DiplomacyHelper.AreAllied(colony.Owner, civ) || DiplomacyHelper.AreFriendly(colony.Owner, civ))
            {
                value += FriendlyColonyPriority;
            }
            else if (DiplomacyHelper.AreNeutral(colony.Owner, civ))
            {
                value += NeutralColonyPriority;
            }
            else if (DiplomacyHelper.AreAtWar(colony.Owner, civ))
            {
                value += EnemyColonyPriority;
            }

            // GameLog.Core.AIDetails.DebugFormat("Spying value for {0} is {1}", colony, value);
            return value;

        }

        /*
        * Science value
        */

        /// <summary>
        /// Determines the value of science on a <see cref="StarSystem"/>
        /// to a given <see cref="Civilization"/>
        /// </summary>
        /// <param name="system"></param>
        /// <param name="civ"></param>
        /// <returns></returns>
        public static int GetScienceValue(StarSystem system, Civilization civ) // civ is fleet.Owner
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            if (civ == null)
            {
                throw new ArgumentNullException(nameof(civ));
            }
            if (system.Owner != null && system.Owner != civ)
            {
                //Civilization otherCiv = system.Owner;
                if (DiplomacyHelper.AreAtWar(system.Owner, civ))
                {
                    return 0;
                }
            }
            const int StarTypeNebula = 5;
            const int StarTypeColor = 10;
            const int StarTypeMoreFun = 15;
            const int StarTypeBlackHoleNeutronStar = -20;
            const int StarTypeWormhole = 30;
            int value = 0;

            if (system.StarType == StarType.Nebula)
            {
                value += StarTypeNebula;
            }
            else if (system.StarType == StarType.Blue ||
                system.StarType == StarType.Orange ||
                system.StarType == StarType.Red ||
                system.StarType == StarType.White ||
                system.StarType == StarType.Yellow)
            {
                value += StarTypeColor;
            }
            else if (system.StarType == StarType.XRayPulsar ||
                system.StarType == StarType.RadioPulsar ||
                system.StarType == StarType.Quasar)
            {
                value += StarTypeMoreFun;
            }
            else if (system.StarType == StarType.NeutronStar || system.StarType == StarType.BlackHole)
            {
                value += StarTypeBlackHoleNeutronStar;
            }
            else if (system.StarType == StarType.Wormhole)
            {
                value += StarTypeWormhole;
            }

            // GameLog.Core.AIDetails.DebugFormat("Spying value for {0} is {1}", system, value);
            return value;
        }

        /*
        / Spy best colony
        */

        /// <summary>
        /// Gets the best <see cref="Colony"/> for a <see cref="Fleet"/> to spy on
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        /// 
        public static bool GetBestColonyForSpying(Fleet fleet, out Colony result)
        {
            GetFleetOwner(fleet);
            if (fleet == null)
            {
                throw new ArgumentNullException(nameof(fleet));
            }


            List<Fleet> spyFleets = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner)
                .Where(o => o.IsSpy).ToList();
            List<Fleet> otherFleets = spyFleets.Where(o => o != fleet).ToList();

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
            CivilizationMapData mapData = civManager.MapData;
            IEnumerable<Fleet> spyShips = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner).Where(s => s.IsSpy);
            List<Colony> possibleColonies = new List<Colony>();
            GetFleetOwner(fleet);
            if (fleet.Owner != null)
            {
                possibleColonies = GameContext.Current.Universe.Find<Colony>()
                //That isn't owned by us but is scanned and is empire
                .Where(c => c.Owner != fleet.Owner && mapData.IsScanned(c.Location) && c.Owner.IsEmpire
                //That is explored and within range
                && mapData.IsExplored(c.Location) && FleetHelper.IsSectorWithinFuelRange(c.Sector, fleet)
                //
                && CheckForSpyNetwork(c.Owner, fleet.Owner) == false
                && DiplomacyHelper.IsTravelAllowed(fleet.Owner, c.Sector))
                //Where other spy is not already going
                .Where(d => !otherFleets.Any(f => f.Route.Waypoints.LastOrDefault() == d.Location || d.Location == f.Location && f.Order is SpyOnOrder))
                .ToList();
            }

            if (possibleColonies.Count == 0)
            {
                // GameLog.Client.AIDetails.DebugFormat("Damn, no Home System of Empire found, possible colonies = {0}", possibleColonies.Count());
                result = null;
                return false;
            }

            possibleColonies.Sort((a, b) =>
                (GetSpyingValue(a, fleet.Owner) * HomeSystemDistanceModifier(fleet, a.Sector))
                .CompareTo(GetSpyingValue(b, fleet.Owner) * HomeSystemDistanceModifier(fleet, b.Sector)));
            result = possibleColonies[possibleColonies.Count - 1];
            _text = "Step_7678:; "+ CreateFleetText(fleet, out string _fleetText)
                    + "GetBestColonyForSpying= " + result.Location
                    + " > " + result.Name
                    + ", Owner= " + result.Owner
                    //+ " for >> " 
                    ;
            if (writeDirectly) Console.WriteLine(_text);
            // GameLog.Client.AIDetails.DebugFormat("Yippy, System of Empire found!, possible spied colony = {0}", possibleColonies.FirstOrDefault().Name);
            return true;
        }

        /*
        / Science best system
        */

        /// <summary>
        /// Gets the best <see cref="StarSystem"/> for a <see cref="Fleet"/> to spy on
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool GetBestSystemForScience(Fleet fleet, out StarSystem result)
        {
            GetFleetOwner(fleet);
            if (fleet == null)
            {
                throw new ArgumentNullException(nameof(fleet));
            }

            List<Fleet> scienceFleets = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner)
                .Where(o => o.IsScience).ToList();
            List<Fleet> otherFleets = scienceFleets.Where(o => o != fleet).ToList();

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
            CivilizationMapData mapData = civManager.MapData;
            IEnumerable<Fleet> scienceShips = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner).Where(s => s.IsScience);
            List<StarSystem> possibleSystems = new List<StarSystem>();
            GetFleetOwner(fleet);
            if (fleet.Owner != null)
            {
                possibleSystems = GameContext.Current.Universe.Find<StarSystem>()
                //That isn't owned by us
                .Where(s => s.Sector != null 
                //&& mapData.IsScanned(s.Location)
                //&& mapData.IsExplored(s.Location) 
                && FleetHelper.IsSectorWithinFuelRange(s.Sector, fleet)
                && DiplomacyHelper.IsTravelAllowed(fleet.Owner, s.Sector)
                        //&& s.StarType == StarType.BlackHole
                        && s.StarType == StarType.XRayPulsar
                        || s.StarType == StarType.NeutronStar
                        || s.StarType == StarType.Quasar
                        || s.StarType == StarType.RadioPulsar
                        || s.StarType == StarType.XRayPulsar
                        || s.StarType == StarType.Wormhole

                )
                //Where other science ship is not already going
                .Where(d => !otherFleets.Any(f => f.Route.Waypoints.LastOrDefault() == d.Location || d.Location == f.Location))
                .ToList();

            }
            if (possibleSystems.Contains(civManager.HomeSystem))
            {
                _ = possibleSystems.Remove(civManager.HomeSystem);
            }

            if (possibleSystems.Count == 0)
            {
                //  GameLog.Client.AIDetails.DebugFormat("Damn, no Science System of Empire found, possible colonies = {0}", possibleSystems.Count());
                result = null;
                return false;
            }

            possibleSystems.Sort((a, b) =>
                (GetScienceValue(a, fleet.Owner) * HomeSystemDistanceModifier(fleet, a.Sector))
                .CompareTo(GetScienceValue(b, fleet.Owner) * HomeSystemDistanceModifier(fleet, b.Sector)));
            result = possibleSystems[possibleSystems.Count - 1];
            //  GameLog.Client.AIDetails.DebugFormat("Yippy, Science System found!, possible  = {0}", possibleSystems.FirstOrDefault().Name);
            return true;
        }

        /// <summary>
        /// Provides a modifier to prioritise targets that are closer the home system
        /// of the <see cref="Civilization"/> that owns the <see cref="Fleet"/>
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="targetSector"></param>
        /// <returns></returns>
        public static float HomeSystemDistanceModifier(Fleet fleet, Sector targetSector)
        {
            GetFleetOwner(fleet);
            if (fleet == null)
            {
                throw new ArgumentNullException(nameof(fleet));
            }

            if (targetSector == null)
            {
                throw new ArgumentNullException(nameof(targetSector));
            }

            int distance = MapLocation.GetDistance(targetSector.Location, GameContext.Current.CivilizationManagers[fleet.Owner].HomeSystem.Location);
            return 1 / (distance + 1);
        }
        public static bool CheckForSpyNetwork(Civilization civSpied, Civilization civSpying)
        {
            if (civSpied == null)
            {
                return false;
            }
            List<Civilization> spiedCivs = new List<Civilization>();
            switch (civSpied.CivID)
            {
                case 0:
                    spiedCivs = IntelHelper.SpyingCiv_0_List;
                    break;
                case 1:
                    spiedCivs = IntelHelper.SpyingCiv_1_List;
                    break;
                case 2:
                    spiedCivs = IntelHelper.SpyingCiv_2_List;
                    break;
                case 3:
                    spiedCivs = IntelHelper.SpyingCiv_3_List;
                    break;
                case 4:
                    spiedCivs = IntelHelper.SpyingCiv_4_List;
                    break;
                case 5:
                    spiedCivs = IntelHelper.SpyingCiv_5_List;
                    break;
                    //case 6:
                    //default:
                    //    return true;
            }
            if (spiedCivs != null && spiedCivs.Contains(civSpying))
            {
                return true;
            }

            return false;

        }
        private static List<Sector> FindStrandedShipSectors(Civilization civ)
        {
            List<Fleet> strandedFleets = GameContext.Current.Universe.FindOwned<Fleet>(civ).Where(o => o.IsStranded).ToList();

            List<Sector> sectorList = new List<Sector>();

            foreach (Fleet strandedFleet in strandedFleets)
            {
                sectorList.Add(strandedFleet.Sector);
            }
            return sectorList;
        }

        private static int GetDistanceTo(MapLocation startLocation, MapLocation endLocation)
        {
            int distance = (int)Math.Sqrt((int)Math.Pow(startLocation.X - endLocation.X, 2) + (int)Math.Pow(startLocation.Y - endLocation.Y, 2));
            return distance;
        }
        private static void GetFleetOwner(Fleet fleet)
        {
            foreach (Ship ship in fleet.Ships)
            {
                if (fleet.Owner != null)
                {
                    break;
                }

                if (ship.Owner != null)
                {
                    fleet.Owner = ship.Owner;
                    fleet.OwnerID = ship.OwnerID;
                    break;
                }
            }
        }

    }


}
