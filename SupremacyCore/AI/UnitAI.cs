// File:UnitAI.cs
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
using System.Diagnostics;
using System.Linq;

namespace Supremacy.AI
{
    public static class UnitAI
    {
        //        //#pragma warning disable IDE0044 // Add readonly modifier
        //        private readonly static IEnumerable<Sector> _deathStars = GameContext.Current.Universe.FindStarType<Sector>(StarType.NeutronStar).ToList()
        //            .Concat(GameContext.Current.Universe.FindStarType<Sector>(StarType.BlackHole).ToList());

        //#pragma warning disable IDE0052 // Remove unread private members

        //        // just for testing
        //        private readonly static IEnumerable<Sector> _NeutronStars = GameContext.Current.Universe.FindStarType<Sector>(StarType.NeutronStar).ToList()
        //    //.Concat(GameContext.Current.Universe.FindStarType<Sector>(StarType.BlackHole).ToList())
        //            ;

        //        private readonly static IEnumerable<Sector> _BlackHoles = 
        //    //        GameContext.Current.Universe.FindStarType<Sector>(StarType.NeutronStar).ToList()
        //    //.Concat(
        //                GameContext.Current.Universe.FindStarType<Sector>(StarType.BlackHole).ToList()
        //                //)
        //            ;
        //#pragma warning restore IDE0052 // Remove unread private members

        //#pragma warning restore IDE0044 // Add readonly modifier

        [NonSerialized]
        //private static string _text;
        //private static string _designText;
        private static string _fleet_Text;
        private static string _fleets_Summary = "";
        private static bool _writeDirectly_Fleets = false;
        //private static double _defenseSectorIntValue;


        //private static bool _checkFleetOrders;
        //private static bool _checkOnlyPlayersUnits = true;
        //private static bool _checkShips_Colony = false;
        //private static bool _checkShips_Construction = false;
        //private static bool _checkShips_Medical = false;
        //private static bool _checkShips_Transport = false;
        //private static bool _checkShips_Spy = false;
        //private static bool _checkShips_Diplomatic = false;
        //private static bool _checkShips_Science = false;
        //private static bool _checkShips_Scout = false;
        //private static bool _checkShips_Battle = false;
        //private static string _loc_otherHomeSystem;
        //private static bool _prepareSystemAssault;


        //private static string _shipText;
        //private static readonly string _newline = Environment.NewLine;
        //private static readonly object _soundPlayer;

        public static IEnumerable<Sector> DeathStars
        {
            get
            {
                IEnumerable<Sector> _NeutronStars = GameContext.Current.Universe.FindStarType<Sector>(StarType.NeutronStar).ToList();
                IEnumerable<Sector> _BlackHoles = GameContext.Current.Universe.FindStarType<Sector>(StarType.BlackHole).ToList();


                IEnumerable<Sector> _deathStars = _NeutronStars.Concat(_BlackHoles);



                //private readonly static 
                //IEnumerable <Sector> _deathStars = GameContext.Current.Universe.FindStarType<Sector>(StarType.NeutronStar).ToList()
                //    .Concat(GameContext.Current.Universe.FindStarType<Sector>(StarType.BlackHole).ToList());

                if (GameContext.Current.Universe.FindStarType<Sector>(StarType.NeutronStar).Count == 0 &&
                    GameContext.Current.Universe.FindStarType<Sector>(StarType.BlackHole).Count == 0)
                    _deathStars = new List<Sector> { };

                return _deathStars;
            }
        }

        public static void DoTurn([NotNull] Civilization _civ)
        {
            //_soundPlayer = soundPlayer ?? throw new ArgumentNullException("soundPlayer");
            CivilizationManager _civM = GameContext.Current.CivilizationManagers[_civ.CivID];
            StarSystem _ourHomeSystem = _civM.HomeSystem;
            //MapLocation accumulateSystem = GameContext.Current.CivilizationManagers[_civM].AccumulateLocation;
            Sector _accumulateSector = _ourHomeSystem.Sector;
            //_accumulateSector.Location = GameContext.Current.CivilizationManagers[_civM].AccumulateLocation.
            Sector _strandedShips_Sector;
            _fleets_Summary = "" + _fleets_Summary;
            string _newline = Environment.NewLine;

            //_civM.Assault_AttackValue = _civM.Assault_AttackValue * -1; // it's a += so this sets to zero




            string _text = "Step_1103:; ##################### UnitAI.DoTurn begins...for "
                + _civ.Key + " #####################   " + DateTime.Now;
            if (_writeDirectly_Fleets) Console.WriteLine(_text);

            if (_civ.Key == "DOMINION")
            {
                //Debugger.Break();

            }

            //_text = _designText;// dummy - please keep

            if (_civ == null)
            {
                throw new ArgumentNullException(nameof(_civ));
            }

            //if (_civM.Assault_TargetCiv != null) // Just for info
            //{
            //    //_text = "Step_6260:; UnitAI-DoTurn; "
            //    //    + "Assault_TargetCiv (not null !!) > " + _civM.Assault_TargetCiv + " "
            //    //    ;
            //    //if (_writeDirectly_Fleets) Console.WriteLine(_text);

            //    // just report to Console + define _targetSystem

            StarSystem othersHomeSystem = GameContext.Current.CivilizationManagers[_civ].HomeSystem; // dummy

            //    _text = "Step_6140:;" // UnitAI = Fleets > "
            //                          //+ _ourHomeSystem.Location
            //            + " " + _civM.Name
            //            + " at " + _ourHomeSystem.Location
            //            + " has Assault_TargetCiv > " + _civM.Assault_TargetCiv.Name
            //            + " - HomeSystem at " + _targetSystem.Location
            //            ;
            //    if (_writeDirectly_Fleets) Console.WriteLine(_text);
            //    _fleet_Text += _newline + _text;

            //    //// just report to Console + define _targetSystem
            //    //if (_civM.Assault_TargetCiv != null)
            //    //{
            //othersHomeSystem = GameContext.Current.CivilizationManagers[_civ].HomeSystem;
            //    //_prepareSystemAssault = true;

            //    //_text = "Step_6150:; BattleFleets > "
            //    //    //+ _ourHomeSystem.Location
            //    //    + " " + _civM.Name
            //    //    + " at " + _ourHomeSystem.Location
            //    //    + " has Assault_TargetCiv > " + _civM.Assault_TargetCiv.Name
            //    //    + " - HomeSystem at " + _targetSystem.Location
            //    //    ;
            //    //if (_writeDirectly_Fleets) Console.WriteLine(_text);
            //    //_fleet_Text += _newline + _text;

            //    //List<Colony> _colonyTargets = GameContext.Current.Universe.FindOwned<Colony>(_civM.Assault_TargetCiv).ToList();

            //    //foreach (var item in _colonyTargets)
            //    //{
            //    //    _text = "Step_3441:; " + GameEngine.LocationString(item.Location.ToString())
            //    //        + " > possible target for _colonyTargets"
            //    //        + " > for " + _civM.Name
            //    //        + " " + GameEngine.LocationString(_ourHomeSystem.Location.ToString())
            //    //                    //+ _targetSystem.Name + "; Owner= " + _targetSystem.Owner
            //    //                    //+ "; Ship= " + _ship.ObjectID + "; Owner= " + _ship.Name
            //    //                    //+ "; Ship= " + _ship.Design + "; Owner= " + _ship.Name
            //    //                    //+ _shipText
            //    //                    ;
            //    //    if (_writeDirectly_Fleets) Console.WriteLine(_text);
            //    //    _fleet_Text += _newline + _text;
            //    //}


            //    if (targetFirePower * 1.1 < civFirePower)
            //    {
            //        //_fleet.Owner = _civM; 
            //        //_fleet.Location = _ourHomeSystem.Location;

            //        //_fleet.SetOrder(new EngageOrder());
            //        //if (_fleet.Location != _targetSystem.Location)
            //        //{
            //        //    _fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { _targetSystem.Sector }));
            //        //}
            //    }
            //    else if (_colonyTargets.Count() > 1)
            //    {
            //        //double lastRange = 999;
            //        _ = _colonyTargets.Remove(_targetSystem.Colony);
            //        foreach (Colony colonyTarget in _colonyTargets)
            //        {
            //            MapLocation target = colonyTarget.Location;
            //            MapLocation ai = _ourHomeSystem.Location;
            //            double curretRange = Math.Sqrt(Math.Pow(target.X - ai.X, 2) + Math.Pow(target.Y - ai.Y, 2));
            //            //if (curretRange < lastRange)
            //            //{
            //            //    lastRange = curretRange;
            //            //    _fleet.Route.Clear();
            //            //    _fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { colonyTarget.Sector }));
            //            //}
            //        }
            //    }
            //    else
            //    {
            //        _civM.Assault_TargetCiv = null;
            //    }
            //}
            //else
            //{
            //    _text = "Step_6153:; UnitAI = Fleets > "
            //            //+ _ourHomeSystem.Location
            //            + " " + _civM.Name
            //            + " has * no * Assault_TargetCiv > " //+ _civM.Assault_TargetCiv.Name
            //                                                  //+ " - HomeSystem at " + _targetSystem.Location
            //            ;

            //    if (_writeDirectly_Fleets) Console.WriteLine(_text);
            //    _fleet_Text += _newline + _text;
            //}




            if (_civ.CivID < 999) // unit AI only for empires >> 999 is 'for all', below '7' is just Empires
            {
                //_text = "Step_3102:; ##########################################   UnitAI for Empires or as well for minors...";
                //if (_writeDirectly_Colony) Console.WriteLine(_text);


                List<Fleet> _all_Fleets_of_Civ = GameContext.Current.Universe.FindOwned<Fleet>(_civ).ToList();
                // finds also _fleets with ID -1 ... maybe Colonizer which disappered and just deleted in next round?

                // in case of ..
                //List<Ship> _allAttackWarShips = new List<Ship>();
                //if (true)
                //{
                //foreach (Fleet civFleet in _all_Fleets_of_Civ)
                //{
                //foreach (Ship _ship in civFleet.Ships.Where(s => s.ShipType >= ShipType.Scout || s.ShipType == ShipType.Transport).ToList())
                //{
                //    if (civFleet.OwnerID != -1)
                //    {
                //        _allAttackWarShips.Add(_ship);
                //        CreateShipText(_ship, out string _shipText);
                //        if (_writeDirectly_Colony) Console.WriteLine("Step_3103:; in case of .. added to _allAttackWarShips; " + _shipText);
                //        // GameLog.Client.AI.DebugFormat("A _ship all attack ships {0} location ={1}", _ship.Name, _ship.Location );
                //    }
                //}




                //_text = "--------------------";
                //if (_writeDirectly_Colony) Console.WriteLine(_text);

                // **** The UnitAI _fleet by _fleet looping 
                foreach (Fleet _fleet in _all_Fleets_of_Civ) // each _fleet of the current _civM
                {
                    _fleet_Text = ""; // collects all Console_Textes 
                    _fleets_Summary = ""; // dummy - please keep

                    if (_fleet.ObjectID == -1)
                    {
                        _fleet.Destroy();
                        continue;
                    }


                    bool _accumulateAble = true;
                    //if (_writeDirectly_Colony) Console.WriteLine("Step_7888:; " + CreateUpdateFleetText(_fleet, out _fleets_Summary) + " > Order= " + _fleet.Order);

                    switch (_fleet.Order.OrderName)
                    {

                        //case FleetOrders.IdleOrder.OrderName:
                        case "On Idle Status":
                        case "Engage":
                            _accumulateAble = true;
                            break;
                        case "On The Way":
                        default:
                            _accumulateAble = false;
                            break;
                    }

                    //AdaptShips_UNUSED(_fleet);

                    //switch

                    if (_fleet.IsStranded)
                    {
                        _strandedShips_Sector = _fleet.Sector;
                    }

                    if (_fleet.IsColonizer) _accumulateAble = false;
                    if (_fleet.IsScout) _accumulateAble = false;
                    if (_fleet.IsConstructor) _accumulateAble = false;
                    if (_fleet.IsMedical) _accumulateAble = false;
                    if (_fleet.IsSpy) _accumulateAble = false;
                    if (_fleet.IsDiplomatic) _accumulateAble = false;
                    if (_fleet.IsScience) _accumulateAble = false;


                    //try
                    //{
                    //    //_designText = _fleet.Ships[0].Design.ToString(); 
                    //}
                    //catch
                    //{
                    //    _text = "Step_6147:; " + CreateUpdateFleetText(_fleet, out _fleets_Summary);
                    //    if (_writeDirectly_Fleets) Console.WriteLine("Step_6149:; " + _text);
                    //    _fleet_Text += _newline + _text;
                    //}

                    ////string _fleetOrderText = "NoOrder";
                    //if (_fleet.Order != null)
                    //    _fleetOrderText = _fleet.Order.ToString();


                    CreateUpdateFleetText(_fleet, out _fleets_Summary);
                    //Console.WriteLine("Step_6152:; " + _fleets_Summary);

                    _fleet_Text += _newline + _text;

                    if (_fleet.Ships.Count > 1) { Print_Ships_of_Fleet(_fleet); }

                    //_fleets_Summary = _fleet.Location
                    //    + " > " + _fleet.Ships[0].Design.ToString()
                    //    + " > " + _fleet.ObjectID + ":  " + _fleet.Name

                    //    //+ " " + _fleet.Owner
                    //    + " " + ", AITypeUnit=> ** " + _fleet.AITypeUnit /*+ ", Assault_TargetCiv=NULL" + _fleet.Owner.Assault_TargetCiv*/
                    //    + " " + ", Activity=> ** " + _fleet.Activity /*+ ", Assault_TargetCiv=NULL" + _fleet.Owner.Assault_TargetCiv*/
                    //    + ", Order= ** " + _fleetOrderText;

                    // double
                    //if (_writeDirectly_Colony)
                    //


                    //_fleet_Text += _newline + _text;
                    // as well go to CTRL+F and 'checking _fleets'

                    CloakAll(_fleet);

                    if (_fleet.IsStranded)  // is stranded
                    {
                        goto All_Done_UnitAI;
                    }

                    if (_fleet.LowestFuelLevel < 3        // running out of fuel
                        && _fleet.IsConstructor == false) // not for Constructors on "Rescue Mission"
                    {
                        _fleet.Route.Clear();
                        _fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, DeathStars, new List<Sector> { _ourHomeSystem.Sector }));

                        if (_fleet.Owner.IsHuman)
                        {
                            Debugger.Break();
                        }
                    }

                    //if (_fleet.LowestFuelLevel < 3        // running out of fuel
                    //        && _fleet.IsConstructor == false) // not for Constructors on "Rescue Mission"
                    //{
                    //    _fleet.Route.Clear();
                    //    _fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, DeathStars, new List<Sector> { _ourHomeSystem.Sector }));
                    //}




                    // ****  a) Main stuff - we don't have a target _civM > for b) some orders are changed
                    //if (_civM.Assault_TargetCiv == null)
                    //{


                    //_text = "Step_6153:; " // Turn " + GameContext.Current.TurnNumber
                    //    + "" + _fleets_Summary
                    //    + " ... ( no CIV to target > just usual business )"
                    //    + ", _accumulateAble= " + _accumulateAble
                    //    ;
                    //if (_writeDirectly_Colony) Console.WriteLine(_text);
                    //_fleet_Text += /*_newline + */_text;



                    // >Check Fleets

                    // just for testing

                    //bool _checkFleetOrders = false;
                    //bool _checkOnlyPlayersUnits = false;
                    //if (_fleet.Owner.IsHuman)
                    //{
                    //    //continue;
                    //    _checkFleetOrders = true;
                    //    _checkOnlyPlayersUnits = true;

                    //Debugger.Break();

                    //}



                    DoAccumulate(_fleet);

                    // no more actions here if owner is human player
                    if (_fleet.Owner.IsHuman)
                    {
                        //continue;
                    }

                    //SELECT
                    //_checkShips_Construction = true;
                    if (_fleet.IsConstructor) DoConstructionShip(_fleet); // Constructor get the first "GetEscort"
                    //_checkShips_Colony = true;
                    if (_fleet.IsColonizer) DoColonyShip(_fleet);
                    //_checkShips_Scout = true;
                    if (_fleet.IsScout) DoScout(_fleet);
                    //_checkShips_Medical = true;
                    if (_fleet.IsMedical) DoMedical(_fleet);
                    //_checkShips_Spy = true;
                    if (_fleet.IsSpy) DoSpy(_fleet);
                    //_checkShips_Diplomatic = true;
                    if (_fleet.IsDiplomatic) DoDiplomatic(_fleet);
                    //_checkShips_Science = true;
                    if (_fleet.IsScience) DoScienceShip(_fleet);

                    //_checkShips_Transport = true;
                    if (_fleet.IsTransport) DoTransportShip(_fleet);
                        
                    // let transport ships explore instead of doing nothing at the beginning
                    if (_fleet.IsTransport && _civM.Assault_TargetCiv != null)
                    {
                        DoScout(_fleet); // ..using Scout-Exlore for ... let transport ships explore instead of doing nothing at the beginning
                    }

                    //bool _checkShips_Battle = true;

                    //if (_fleet.Owner.IsHuman) { Debugger.Break(); }

                    if (_fleet.IsCombatant
                        && !_fleet.IsMedical
                        && !_fleet.IsDiplomatic
                        && !_fleet.IsColonizer
                        && !_fleet.IsConstructor
                        && !_fleet.IsSpy
                        && !_fleet.IsScout
                        && !_fleet.IsScience
                        )
                    {
                        if (_fleet.Owner.IsHuman)
                        {
                            //Debugger.Break();
                        }

                        //DoBattleShips(_fleet); // inclusive TransportShips
                        //                       //_checkShips_Transport = true;
                        //                       //DoTransportShip(_fleet);
                        //                       //}
                        //                       //else
                        //                       //{
                        //                       //    //goto End_Ships;
                        //                       //}

                        if (_fleet.Owner.IsHuman)
                        {
                            //Debugger.Break();
                        }

                        // if Type=SystemAttack and (before) no Assault_TargetCiv => return to HomeSystem
                        if (_fleet.AITypeUnit == UnitAIType.SystemAttack && _civM.Assault_TargetCiv == null) // call off attack
                        {
                            _text = /*"UnitAI-DoTurn; "*/
                                /*+ */"Step_3657:; " + _fleets_Summary
                                ;
                            if (_writeDirectly_Fleets) Console.WriteLine(_text);
                            _fleet_Text += _newline + _text;

                            List<Ship> shipList = _fleet.Ships.ToList();
                            if (shipList.Count() > 0)  //&& _fleet.AITypeUnit != AITypeUnit.SystemDefense )
                            {

                                foreach (Ship ship in shipList)
                                {
                                    if (ship.ShipType >= ShipType.Scout)
                                    {
                                        Fleet tempFleet = new Fleet(); // keep making a new 'tempFleet'
                                        _fleet.RemoveShip(ship);
                                        tempFleet.AddShip(ship);
                                        tempFleet.Owner = _fleet.Owner;
                                        tempFleet.AITypeUnit = UnitAIType.NoUnitAI;
                                        tempFleet.Activity = UnitActivity.NoActivity;
                                        if (_fleet.Location != _ourHomeSystem.Location)
                                        {
                                            tempFleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, DeathStars, new List<Sector> { _ourHomeSystem.Sector }));
                                        }
                                        else
                                        {
                                            tempFleet.Route.Clear();
                                        }
                                    }
                                }
                            }
                        } // end of if Type=SystemAttack and (before) no Assault_TargetCiv => return to HomeSystem
                          // >Check Fleets

                        //else // if nothing special


                        //else 
                        if (GameContext.Current.TurnNumber > 0    // before > 4 = why just from turn 5 on ?
                         && _fleet.Ships.Count > 0  // 2024-03-30
                         && _civM.Assault_TargetCiv == null
                         && !_fleet.Ships.Any(o =>
                            o.ShipType == ShipType.Colony
                         || o.ShipType == ShipType.Construction
                         || o.ShipType == ShipType.Medical
                         || o.ShipType == ShipType.Science
                         || o.ShipType == ShipType.Scout
                         || o.ShipType == ShipType.Diplomatic
                         || o.ShipType == ShipType.Spy)) // do not mess with esorted _fleets or these _ship types 
                                                         // so any combat _ship without the types above
                        {
                            // if not in home _location AND _fleet is busy in Reserve or Escort
                            // than > Go to OLD: HomeSystem, NEW: AccumulateLocation

                            //if (_fleet.Owner.IsHuman) { Debugger.Break(); }

                            // so combatant ships
                            if (/*_fleet.Sector != _ourHomeSystem.Sector &&*/
                                (_fleet.AITypeUnit != UnitAIType.Reserve && _fleet.Route.IsEmpty)) // escort left over after colonizing, construction...
                            {
                                _fleet.Owner = _civ;
                                _fleet.OwnerID = _civ.CivID;
                                _fleet.AITypeUnit = UnitAIType.Attack;
                                _fleet.Activity = UnitActivity.NoActivity;
                                _fleet.SetOrder(new ExploreOrder()); // 2024-03-30
                                _fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, DeathStars, new List<Sector> { _accumulateSector }));

                                foreach (var ship in _fleet.Ships)
                                {
                                    CreateShipText(ship, out string _shipText);
                                    _text = "Step_6231:; " + _fleets_Summary + " > Explore";
                                    //if (_writeDirectly_Fleets) Console.WriteLine(_text);
                                    _fleet_Text += _newline + _text;
                                }

                            }
                            //GameLog.Core.AI.DebugFormat("## NOT Total War, _fleet = {0}, Ships ={1}, {2}, {3}, {4}, {5}", _fleet.Ships[0].DesignName, _fleet.Ships.Count(), _fleet.Owner, _fleet.Sector.Name, _fleet.AITypeUnit, _fleet.Activity, _fleet.Location);
                            //if (_fleet.Ships.Count() > 1)
                            //{
                            //    GameLog.Core.AI.DebugFormat("## NOT Total War, first Ship = {0} {1}", _fleet.Ships[0].Name, _fleet.Ships[0].DesignName);
                            //    GameLog.Core.AI.DebugFormat("## NOT Total War, second Ship = {0} {1}", _fleet.Ships[1].Name, _fleet.Ships[1].DesignName);
                            //}
                        }
                        //else 

                        // no AccumulateSector for Minor races > so they are not staying at home all the time
                        if (_fleet.Owner.IsEmpire
                            && _accumulateAble
                            && _accumulateSector != null
                            && _accumulateSector.Location.ToString() != "(0, 0)")
                        {
                            if (_fleet.AITypeUnit == UnitAIType.SystemAttack)
                            {
                                goto NoAccumulateForSystemAttacks;
                            }

                            _fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, DeathStars, new List<Sector> { _accumulateSector }));
                            foreach (var ship in _fleet.Ships)
                            {
                                CreateShipText(ship, out string _shipText);
                                _text = "Step_6230:; " + _shipText
                                    //+ " > Heading up to AccumulateSector= " + _accumulateSector.Location
                                    ;
                                //if (_writeDirectly_Fleets) Console.WriteLine(_text);
                                _fleet_Text += _newline + _text;
                            }
                        NoAccumulateForSystemAttacks:;
                        }
                        else
                        {
                            foreach (var ship in _fleet.Ships)
                            {
                                CreateShipText(ship, out string _shipText);
                                _text = "Step_6234:; " + _shipText + " > GOING NOWHERE ... e.g. AccumulateSector " + _accumulateSector.Location + " is not set";
                                //if (_writeDirectly_Colony) Console.WriteLine(_text);
                                _fleet_Text += _newline + _text;
                            }
                        }


                    }

                All_Done_UnitAI:;
                    //else
                    //{
                    //    if (_fleet.Ships.Where(o => o.ShipType == ShipType.Construction).Any())
                    //    {
                    //        GameLog.Core.AI.DebugFormat("Target null Constructor _fleet = {0}, Ships ={1}, {2}, {3}, {4}, {5}", _fleet.Ships[0].DesignName, _fleet.Ships.Count(), _fleet.Owner, _fleet.Sector.Name, _fleet.AITypeUnit, _fleet.Activity, _fleet.Location);
                    //        if (_fleet.Ships.Count() > 1)
                    //        {
                    //            GameLog.Core.AI.DebugFormat("Top-Target null Construct,first Ship {0} {1} route {2} Activity {3} UnitAI {4}", _fleet.Ships[0].Name, _fleet.Ships[0].DesignName, _fleet.Route.Length, _fleet.Activity, _fleet.AITypeUnit);
                    //            GameLog.Core.AI.DebugFormat("Top-Target null Construct,second Ship {0} {1} route {2} Activity {3} UnitAI {4}", _fleet.Ships[1].Name, _fleet.Ships[1].DesignName, _fleet.Route.Length, _fleet.Activity, _fleet.AITypeUnit);
                    //        }
                    //    }
                    //}

                    //} // no a) _civM.Assault_TargetCiv == null


                    //goto End_Ships;
                    // **** b) We have a target civilization
                    //            if (_civM.Assault_TargetCiv != null) // see Step_6260
                    //            {
                    //                Fleet _attackFleet = new Fleet();

                    //                StarSystem _targetSystem = homeSystem; // same as home until there is a target _civM
                    //                                                          // just report to Console + define _targetSystem
                    //                if (_civM.Assault_TargetCiv != null)
                    //                {
                    //                    _targetSystem = GameContext.Current.CivilizationManagers[_civM.Assault_TargetCiv].HomeSystem;

                    //                    _text = "Step_6150:; UnitAI = Fleets > "
                    //                        + homeSystem.Location
                    //                        + " " + _civM.Name
                    //                        + " has Assault_TargetCiv > " + _civM.Assault_TargetCiv.Name
                    //                        + " - HomeSystem at " + _targetSystem.Location;
                    //                    if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    //                    _fleet_Text += _newline + _text;
                    //                }

                    //                //_fleet.AITypeUnit = AITypeUnit.SystemAttack; //2024-06-09
                    //                Print_Ships_of_Fleet(_fleet);

                    //                //foreach (var _ship in _fleet.Ships)
                    //                //{
                    //                //    CreateShipText(_ship, out string _shipText);
                    //                //    _text = "Step_3279:; " + GameEngine.LocationString(_targetSystem.Location.ToString())
                    //                //        + " Ship in Fleet= "
                    //                //                    //+ _targetSystem.Name + "; Owner= " + _targetSystem.Owner
                    //                //                    //+ "; Ship= " + _ship.ObjectID + "; Owner= " + _ship.Name
                    //                //                    //+ "; Ship= " + _ship.Design + "; Owner= " + _ship.Name
                    //                //                    + _shipText
                    //                //                    ;
                    //                //    if (_writeDirectly_Colony) Console.WriteLine(_text);
                    //                //    _fleet_Text += _newline + _text;
                    //                //}


                    //                //goto End_Ships;

                    //                // To remove
                    //                //if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    //                //else 
                    //                if (_fleet.Ships.Any(o => o.ShipType >= ShipType.Scout || o.ShipType == ShipType.Transport))// No systemattack _fleet so make one
                    //                {
                    //                    //GameLog.Core.AI.DebugFormat("Combat Ships ={0} with target ={1}", _civM.Name, _civM.Assault_TargetCiv.Name);
                    //                    // non combat ships in a _fleet with escort
                    //                    if (_fleet.Ships.Count > 1 && _fleet.Ships.Any(o => o.ShipType < ShipType.Transport))
                    //                    {
                    //                        // Break up escorted _fleets to send combat _ship home
                    //                        if (_fleet.AITypeUnit == AITypeUnit.Colonizer)
                    //                        {
                    //                            RemoveEscortShips(_fleet, ShipType.Colony);
                    //                            continue;
                    //                        }
                    //                        if (_fleet.AITypeUnit == AITypeUnit.Constructor)
                    //                        {
                    //                            RemoveEscortShips(_fleet, ShipType.Construction);
                    //                            continue;
                    //                        }

                    //                        CreateShipText(_fleet.Ships[0], out string _shipText);
                    //                        // all other ships > AITypeUnit.SystemAttack
                    //                        if (_fleet.IsCombatant)
                    //                        {
                    //                            _fleet.AITypeUnit = AITypeUnit.SystemAttack;
                    //                            _text = "Step_3265:; " + _targetSystem.Location + " Set to SystemAttack > "
                    //                                    + homeSystem.Name + ", Owner= " + homeSystem.Owner
                    //                                    + _shipText
                    //                                    //+ " > ordered one more facility for > Industry"
                    //                                    ;
                    //                            if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    //                            _fleet_Text += _newline + _text;
                    //                        }

                    //                    }
                    //                    else
                    //                    {
                    //                        CreateShipText(_fleet.Ships[0], out string _shipText);
                    //                        // all other ships > AITypeUnit.SystemAttack
                    //                        if (_fleet.IsCombatant)
                    //                        {
                    //                            _fleet.AITypeUnit = AITypeUnit.SystemAttack;
                    //                            _text = "Step_3266:; " + _targetSystem.Location + " Set to SystemAttack > "
                    //                                    + homeSystem.Name + ", Owner= " + homeSystem.Owner
                    //                                    + _shipText
                    //                                    //+ " > ordered one more facility for > Industry"
                    //                                    ;
                    //                            if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    //                            _fleet_Text += _newline + _text;
                    //                        }
                    //                    }
                    //                }


                    //                if (_fleet.Location != homeSystem.Location && !_fleet.Route.Waypoints.Contains(homeSystem.Location))
                    //                {
                    //                    if (_fleet.IsScout)
                    //                    {
                    //                        // send scouts home
                    //                        _fleet.Route.Clear(); // stop exloring
                    //                        _fleet.SetOrder(new AvoidOrder());
                    //                        BuildAndSendFleet(_fleet, _civM, UnitActivity.Mission, AITypeUnit.Reserve, homeSystem.Sector);
                    //                        continue;
                    //                        // GameLog.Core.AI.DebugFormat("Ordering Scout & FastAttack {0} to explore from {1}", _fleet.ClassName, _fleet.Location);
                    //                    }
                    //                    else if (_fleet.Ships.Count() > 1)
                    //                    {
                    //                        int numberofships = _fleet.Ships.Count();
                    //                        //Fleet anotherFleet = new Fleet();
                    //                        _text = "Step_8789:; " + _fleets_Summary + " > Fleet is to separate into single ships > check the _fleet for crashes here "
                    //                                //+ _fleets_Summary
                    //                                ;
                    //                        if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    //                        _fleet_Text += _newline + _text;

                    //                        //try
                    //                        //{
                    //                        foreach (Ship currentship in _fleet.Ships)
                    //                        {
                    //                            //Ship currentship = _fleet.Ships[i];
                    //                            //Fleet anotherFleet = new Fleet();
                    //                            if (currentship != null && currentship.IsCombatant)
                    //                            {
                    //                                Fleet anotherFleet = new Fleet();
                    //                                _fleet.RemoveShip(currentship);
                    //                                anotherFleet.AddShip(currentship);
                    //                                BuildAndSendFleet(anotherFleet, _civM, UnitActivity.Mission, AITypeUnit.Reserve, homeSystem.Sector);
                    //                            }
                    //                        }
                    //                        //}
                    //                        //catch 
                    //                        //{
                    //                        //    _text = "Step_8789:; " + _fleets_Summary + " > Fleet is to separate into single ships > check the _fleet for crashes here "
                    //                        //            //+ _fleets_Summary
                    //                        //            ;
                    //                        //    if (_writeDirectly_Colony) Console.WriteLine(_text);
                    //                        //}
                    //                        //foreach (Ship _ship in _fleet.Ships) // is Colonizer still in _fleet or if not > Crash
                    //                        //{
                    //                        //    if (_ship.ShipType != ShipType.Colony || _ship.ShipType != ShipType.Construction || _ship.ShipType != ShipType.Medical)
                    //                        //    {
                    //                        //        _fleet.RemoveShip(_ship);
                    //                        //    }

                    //                        //    anotherFleet.AddShip(_ship);
                    //                        //    BuildAndSendFleet(anotherFleet, _civM, UnitActivity.Mission, AITypeUnit.Reserve, _ourHomeSystem.Sector);
                    //                        //    continue;
                    //                        //}
                    //                    }
                    //                }
                    //                else if (_fleet.Sector == homeSystem.Sector)
                    //                {
                    //                    List<Ship> listOfShips = _fleet.Ships.ToList();
                    //                    if (listOfShips.Count() > 0)
                    //                    {
                    //                        foreach (Ship _ship in listOfShips)
                    //                        {
                    //                            if (_ship.ShipType >= ShipType.Scout || _ship.ShipType == ShipType.Transport)
                    //                            {
                    //                                if (homeSystem.Sector.GetOwnedFleets(_civM).FirstOrDefault(o => o.AITypeUnit == AITypeUnit.SystemAttack) == null)
                    //                                //&& o.AITypeUnit == AITypeUnit.SystemAttack) == null)
                    //                                {
                    //                                    _fleet.AITypeUnit = AITypeUnit.SystemAttack;
                    //                                    _attackFleet = _fleet;
                    //                                }
                    //                                else
                    //                                {
                    //                                    _fleet.RemoveShip(_ship);
                    //                                    _attackFleet.AddShip(_ship);
                    //                                    //GameLog.Core.AI.DebugFormat("The {0} Attack Fleet adding _ship ={1} attack _fleet count ={2}"
                    //                                    //       , _civM.Name, _ship.Name, _attackFleet.Ships.Count());
                    //                                    //foreach (Ship nextShip in _attackFleet.Ships)
                    //                                    //{
                    //                                    //    GameLog.Core.AI.DebugFormat("Added The _ship ={0} {1}"
                    //                                    //        , nextShip.Name, nextShip.OrbitalDesign.ToString());
                    //                                    //}
                    //                                    //GameLog.Client.AI.DebugFormat("All _civM {0} _ship count{1}", _civM.Key, _all_Fleets_of_Civ.ToList().Count());
                    //                                    //foreach (var anotherFleet in _all_Fleets_of_Civ)
                    //                                    //{
                    //                                    //    foreach (Ship anotherShip in anotherFleet.Ships)
                    //                                    //    {
                    //                                    //        GameLog.Core.AI.DebugFormat("All _civM {0} _ship ={1} {2}"
                    //                                    //            , _civM.Name, anotherShip.Name, anotherShip.OrbitalDesign.ToString());
                    //                                    //    }
                    //                                    //}
                    //                                }
                    //                                _attackFleet.Location = homeSystem.Location;
                    //                                _attackFleet.AITypeUnit = AITypeUnit.SystemAttack;
                    //                                _attackFleet.Activity = UnitActivity.Hold;
                    //                                _attackFleet.Owner = _civM;
                    //                                _attackFleet.OwnerID = _civM.CivID;
                    //                                _attackFleet.SetOrder(new EngageOrder());
                    //                                continue;
                    //                                //GameLog.Core.AI.DebugFormat("## Attackfleet Ship Count={0}, {1}, {2}, {3}, {4},"
                    //                                //    , _attackFleet.Ships.Count, _attackFleet.Name, _attackFleet.Owner, _attackFleet.AITypeUnit.ToString(), _attackFleet.Location);
                    //                            }
                    //                        }
                    //                    }
                    //                }



                    //            }


                    //            // **** The non-combat ships, targetciv null or not null
                    //            if (_fleet.Ships.Count() > 0)
                    //            {
                    //                //_text = "Step_6240:; " + _fleets_Summary + " > #### _fleet.IsColonizer ";
                    //                if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    //                //if (_fleet.IsColonizer)// || _fleet.AITypeUnit == AITypeUnit.Colonizer)
                    //                //{
                    //                //    _text = "Step_6301:; " + _fleets_Summary + " > _fleet.IsColonizer ";
                    //                //    if (_writeDirectly_Colony) Console.WriteLine(_text);
                    //                //    _fleet_Text += _newline + _text;

                    //            }
                    //        End_Ships:;
                    //        } // End of foreach _ship = continues with the next

                    //        //}
                    //        // ** end of UnitAI _ship by _ship loop for _civM
                    //        if (_writeDirectly_Fleets) Console.WriteLine(_newline + "Fleet_Text: " + _fleet_Text);

                    //        if (_checkFleetOrders == true && _checkOnlyPlayersUnits)
                    //            _text += ""; // dummy

                    //        //End_Ships:;
                    //    }
                    //}
                }

                if (_civM.SystemAssault_Accumulate_Location_1 != null && _civM.SystemAssault_Accumulate_Location_1.ToString() != "(0, 0)")
                {

                    _civM.Assault_AttackValue = 0; // _civM.Assault_AttackValue * -1; // it's a += so this sets to zero
                    //_civM.Assault_AttackValue = 0; // no, it's always a +=

                    List<Fleet> _fleetsAtLocation_All = new List<Fleet>(GameContext.Current.Universe.Find<Fleet>(UniverseObjectType.Fleet))
                .Where(f => /*f.Location.ToString() == _civM.SystemAssault_Accumulate_Location_1.ToString() &&*/ f.OwnerID == _civM.CivilizationID).ToList();

                    List<Fleet> _fleetsAtLocation = new List<Fleet>(GameContext.Current.Universe.Find<Fleet>(UniverseObjectType.Fleet))
                .Where(f => f.Location.ToString() == _civM.SystemAssault_Accumulate_Location_1.ToString() && f.OwnerID == _civM.CivilizationID).ToList();

                    foreach (var _f in _fleetsAtLocation)
                    {
                        if (_f.IsCombatant) // && _fleetAtLocation.Ships.Any(o => o.ShipType == ShipType.Transport))
                        {
                            _civM.Assault_AttackValue += _f.Firepower(); // 
                        }

                        if (_f.IsTransport)
                        {
                            _civM.Assault_AttackValue += (int)_f.Ships[0].ShipDesign.WorkCapacity;
                        }
                    }

                    if (_civM.Assault_TargetCiv != null)
                    {


                        MapLocation _targetLocation = GameContext.Current.CivilizationManagers[_civM.Assault_TargetCiv.CivID].HomeSystem.Location;

                        if (_civ.IsHuman)
                        {
                            //Debugger.Break();
                        }


                        //Checkfor_Power(_civ, _civM.SystemAssault_Accumulate_Location_1, _targetLocation);

                        //int _FirePower_Accumulated = 0;
                        int _defenseSectorIntValue = 10; // 100 > 10 for testing
                                                 //StarSystem _targetSystem = 
                        GameContext.Current.Universe.Find<StarSystem>().TryFindFirstItem(s => s.Location.ToString() == _targetLocation.ToString(), out StarSystem _targetSystem);
                        _defenseSectorIntValue += _targetSystem.Colony.Population.CurrentValue;  // 
                        if (_targetSystem.Colony.OrbitalBatteries.Count > 0)
                        {
                            _defenseSectorIntValue += (_targetSystem.Colony.OrbitalBatteries.Count
                                             * (_targetSystem.Colony.OrbitalBatteries[0].Design.PrimaryWeapon.Count
                                             * _targetSystem.Colony.OrbitalBatteries[0].Design.PrimaryWeapon.Damage)
                                             + (_targetSystem.Colony.OrbitalBatteries[0].Design.SecondaryWeapon.Count
                                             * _targetSystem.Colony.OrbitalBatteries[0].Design.SecondaryWeapon.Damage))
                                             /10 // 

                                             ;  // 


                        }

                        if (_targetSystem.Sector.Station != null)
                        {
                            _defenseSectorIntValue += _targetSystem.Sector.Station.Firepower() / 200;  // station only half
                        }

                        _text = "Step_3255:; "  /*+ _loc_otherHomeSystem + " > "*/
                                //+ CreateUpdateFleetText(_fleet, out _fleetText) + Environment.NewLine + " > Set Route to Target system = "
                                + _targetSystem.Name
                                + " at " + _targetSystem.Location
                                + ", Owner= " + _targetSystem.Owner
                                //+ " > ordered one more facility for > Industry"
                                + " > _FirePower_Total= " + _civM.Assault_AttackValue
                                + " vs _defense= " + _defenseSectorIntValue
                                ;
                        if (_writeDirectly_Fleets) Console.WriteLine(_text);
                        _fleet_Text += Environment.NewLine + _text;

                        if (_civM.Assault_AttackValue > 0 && _civM.Assault_AttackValue > _defenseSectorIntValue)
                        {
                            foreach (var _fleet2 in _fleetsAtLocation)
                            {
                                _fleet2.AITypeUnit = UnitAIType.SystemAttack;
                                _fleet2.Activity = UnitActivity.Mission;
                                _fleet2.SetRoute(AStar.FindPath(_fleet2, PathOptions.SafeTerritory, DeathStars, new List<Sector> { _targetSystem.Sector }));

                            }

                                if (_civ.IsHuman)
                            {
                                Debugger.Break();
                            }
                        }

                        if (_civ.IsHuman)
                        {
                            //Debugger.Break();  // here_DoBattle
                        }

                        List<Colony> _colonyTargets = GameContext.Current.Universe.FindOwned<Colony>(_civM.Assault_TargetCiv).ToList();

                        string _possTargets = "";
                        foreach (var item in _colonyTargets)
                        {
                            _text = "Step_3441:; " + GameEngine.LocationString(item.Location.ToString())
                                + " > possible target for SystemAttack"
                                + " > for " + _civM.Civilization.Key
                                + " " + GameEngine.LocationString(_ourHomeSystem.Location.ToString())
                                            //+ _targetSystem.Name + "; Owner= " + _targetSystem.Owner
                                            //+ "; Ship= " + _ship.ObjectID + "; Owner= " + _ship.Name
                                            //+ "; Ship= " + _ship.Design + "; Owner= " + _ship.Name
                                            //+ _shipText
                                            ;
                            //if (_writeDirectly_Fleets) Console.WriteLine(_text);
                            _possTargets += Environment.NewLine + _text;
                            _fleet_Text += Environment.NewLine + _text;

                            //if (_civ.IsHuman)
                            //{
                            //    Debugger.Break();
                            //}
                        }
                        if (_writeDirectly_Fleets) 
                            Console.WriteLine("Step_3442:; _possTargets = " /*+ Environment.NewLine*/
                            + _possTargets);
                    }
                }
            }
            //}


            //catch (Exception e)
            //{
            //    _text = "Step_9778:; #### problem at UnitAI.cs" + _newline + e.ToString();
            //    if (_writeDirectly_Fleets) Console.WriteLine(_text);
            //    _fleet_Text += _newline + _text;
            //    if (_writeDirectly_Fleets) Console.WriteLine(_fleet_Text);
            //    GameLog.Core.General.Error(e);
            //}

            //SoundPlayer soundPlayer = new SoundPlayer("Resources/SoundFX/sound001.wav");
        }

        private static void Checkfor_Power(Civilization _civ, MapLocation _systemAssault_Accumulate_Location_1, MapLocation _targetLocation)
        {
            string _text;
            string _newline = Environment.NewLine;
            string _fleetText;// = Environment.NewLine;

            Fleet _attackFleet = new Fleet();
            //Fleet _attackTransporters = new Fleet();

            List<Fleet> _fleetsAtLocation = new List<Fleet>(GameContext.Current.Universe.Find<Fleet>(UniverseObjectType.Fleet))
            .Where(f => f.Location == _systemAssault_Accumulate_Location_1 
            && f.OwnerID == _civ.CivID).ToList();

            CivilizationManager _civM = GameContext.Current.CivilizationManagers[_civ];

            _civM.Assault_AttackValue = 0;

            if (_civ.IsHuman)
            {
                Debugger.Break();
            }

            Debugger.Break(); // always

            foreach (var _fleet in _fleetsAtLocation)
            {
                if (_fleet.IsCombatant) // && _fleetAtLocation.Ships.Any(o => o.ShipType == ShipType.Transport))
                {
                    _civM.Assault_AttackValue += _fleet.Firepower(); // 
                    foreach (var item in _fleet.Ships)
                    {
                        _attackFleet.AddShip(item);
                    }
                }

                if (_fleet.IsTransport)
                {
                    _civM.Assault_AttackValue += (int)_fleet.Ships[0].ShipDesign.WorkCapacity;
                    foreach (var item in _fleet.Ships)
                    {
                        _attackFleet.AddShip(item);
                    }
                }
            }

            int _defenseSector = 10; // 100 > 10 for testing
            //StarSystem _targetSystem = 
                GameContext.Current.Universe.Find<StarSystem>().TryFindFirstItem(s => s.Location.ToString() == _targetLocation.ToString(),out StarSystem _targetSystem);
            if (_targetSystem.Sector.Station != null)
            {
                _defenseSector += _targetSystem.Sector.Station.Firepower() / 200;  // station only half
                _defenseSector += _targetSystem.Colony.Population.CurrentValue;  // 
                if (_targetSystem.Colony.OrbitalBatteries.Count > 0)
                {
                    _defenseSector += _targetSystem.Colony.OrbitalBatteries.Count
                                     * (_targetSystem.Colony.OrbitalBatteries[0].Design.PrimaryWeapon.Count
                                     * _targetSystem.Colony.OrbitalBatteries[0].Design.PrimaryWeapon.Damage)
                                     + (_targetSystem.Colony.OrbitalBatteries[0].Design.SecondaryWeapon.Count
                                     * _targetSystem.Colony.OrbitalBatteries[0].Design.SecondaryWeapon.Damage)

                                     ;  // 


                }
            }

            if (_civM.Assault_AttackValue > _defenseSector * 1.1) // 10% more attack power than defense
                {
                    _text = "Step_3234:; "
                        + GameEngine.LocationString(_targetLocation.ToString())
                         + " > AttackPower= " + _civM.Assault_AttackValue
                        + " vs " + _defenseSector + " DefenseSector "
                        ;
                    if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    _fleet_Text += Environment.NewLine + _text;


                    //_fleetInvades = true;
                    _attackFleet.Route.Clear();
                    _attackFleet.AITypeUnit = UnitAIType.SystemAttack;
                    _attackFleet.Order = FleetOrders.AssaultSystemOrder;
                    // ************* ToDo invasion 
                    SystemAssault(_attackFleet);
                    _text = "Step_3235:; " + CreateUpdateFleetText(_attackFleet, out _fleetText) + " Do Invasion at Target system = "
                            + _targetLocation 
                            //+ ", Owner= " + _targetSystem.Owner
                            //+ " > ordered one more facility for > Industry"
                            ;
                    if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    _fleet_Text += Environment.NewLine + _text;
                    if (_attackFleet.Owner.IsHuman)
                    {
                        //Debugger.Break();
                    }
                    //GameLog.Core.AI.DebugFormat("## Do Invasion at Target system, _civM ={0}, targetCivilization ={1}", _

                    //GameLog.Core.AI.DebugFormat("## Do Invasion at Target system, _civM ={0}, targetCivilization ={1}", _civM.Name, _civM.Assault_TargetCiv.Name);
                    // send home and re-set AITypeUnit / can we already set ships to reserve here for return to home?
                }
            }
        

        //private static void AdaptShips_UNUSED(Fleet _fleet)
        //{
        //    AITypeUnit _unitAIType = _fleet.AITypeUnit;
        //    UnitActivity _unitActivity = _fleet.Activity;
        //    var _order = _fleet.Order.ToString();

        //    foreach (var _ship in _fleet.Ships)
        //    {
        //        if (_ship.ShipType.ToString() == _fleet.AITypeUnit.ToString())
        //        {
        //            //_ship.a
        //        }
        //    }
        //}

        private static void Print_Ships_of_Fleet(Fleet _fleet)
        {
            int _count = 0;
            foreach (var _ship in _fleet.Ships)
            {
                _count++;

                CreateShipText(_ship, out string _shipText);
                string _text = "Step_3279:; " + GameEngine.LocationString(_fleet.Location.ToString())

                                //+ _targetSystem.Name + "; Owner= " + _targetSystem.Owner
                                //+ "; Ship= " + _ship.ObjectID + "; Owner= " + _ship.Name
                                //+ "; Ship= " + _ship.Design + "; Owner= " + _ship.Name
                                + " > " + _fleet.ObjectID

                                + " --- Ship " + _count + " of " + _fleet.Ships.Count

                                //+ " " + _fleet.Name
                                + " >> " + _shipText

                                ;
                if (_writeDirectly_Fleets) Console.WriteLine(_text);
                _fleet_Text += Environment.NewLine + _text;
            }
        } // End of Print_Ships_of_Fleet

        private static void DoAccumulate(Fleet _fleet)
        {
            CivilizationManager _civM = GameContext.Current.CivilizationManagers[_fleet.Owner.CivID];
            Sector _accumulateSector = _civM.AccumulateSector;
            string _text = _civM.AccumulateLocation.ToString();
            //string _newline = Environment.NewLine;

            _text = "Step_6232:; " + CreateUpdateFleetText(_fleet, out _fleets_Summary)
            + " > AccumulateSector= " + _civM.AccumulateLocation.ToString()
                        //+ " , Home= " + _civM.HomeSystem.Location /*+ " )"*/
                        ;
            if (_writeDirectly_Fleets) 
                Console.WriteLine(_text);
            _fleet_Text += Environment.NewLine + _text;

            if (_fleet.IsConstructor
                || _fleet.IsStranded // Stranded
                                     //|| _fleet.IsSpy
                                     //|| _fleet.IsColonizer
                )
            {
                goto No_Accumulate;
            }
            //else
            //{
            //    _text = "Step_6231:; " + CreateUpdateFleetText(_fleet, out _fleets_Summary)
            //    + " > AccumulateSector= " + _accumulateSector.Location
            //                //+ " , Home= " + _civM.HomeSystem.Location /*+ " )"*/
            //                ;
            //    if (_writeDirectly_Fleets) Console.WriteLine(_text);
            //    _fleet_Text += Environment.NewLine + _text;
            //}

            if (_fleet.Owner.IsHuman)
            {
                //Debugger.Break();
            }


            if (_accumulateSector != null && _accumulateSector.Location.ToString() != "(0, 0)")
            {
                //always Fleet.Order > .ToString() because Fleet.Order always contain a special _fleet and more

                // Set_Order
                if (_fleet.Order.ToString() == FleetOrders.AccumulateLocation_Set_Here_Order.ToString())
                {
                    _text = "Step_6154:; Turn " + GameContext.Current.TurnNumber
                            + " > " + _fleets_Summary
                            + " ... ( no CIV to target > just usual business )"
                            ;
                    if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    _fleet_Text += Environment.NewLine + _text;

                    //if (_checkFleetOrders == true && _checkOnlyPlayersUnits)
                    if (_fleet.Owner.IsHuman) Debugger.Break();
                }


                // AccumulateLocation_Go_There_Order

                if (_fleet.AITypeUnit != UnitAIType.Explorer
                    && _fleet.Order.ToString() == FleetOrders.AccumulateLocation_Go_There_Order.ToString()
                    || _fleet.Order.ToString() == FleetOrders.IdleOrder.ToString())
                //_fleet.Order != FleetOrders.AssaultSystemOrder &&
                //_fleet.Order != FleetOrders.BuildStationOrder &&
                //_fleet.Order != FleetOrders.CollectDeuteriumOrder &&
                //_fleet.Order != FleetOrders.ColonizeOrder &&
                //_fleet.Order != FleetOrders. &&
                //)
                {
                    //if (_accumulateSector != null && _accumulateSector.Location.ToString() != "(0, 0)")
                    //{


                        _fleet.SetOrder(new AccumulateLocation_Go_There_Order());
                        _fleet.Activity = UnitActivity.GoToAccumulateSector;
                        _fleet.UnlockRoute();
                        _fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, DeathStars, new List<Sector> { _accumulateSector }));
                        foreach (var ship in _fleet.Ships)
                        {
                            //CreateShipText(_ship, out string shipText);
                            _text = "Step_6237:; " + CreateShipText(ship, out string shipText)
                                + ", Home= " + _civM.HomeSystem.Location /*+ " )"*/
                                + " > going to AccumulateSector= " + _accumulateSector.Location

                                ;
                            //if (_writeDirectly_Fleets) Console.WriteLine(_text);
                            //_fleet_Text += Environment.NewLine + _text;
                        }
                    }

                    if (_fleet.IsScout) 
                    DoScout(_fleet);

                    if (_fleet.IsColonizer) 
                    DoColonyShip(_fleet);
                //}
            }
        No_Accumulate:;

            //if (_fleet.Ships.Count > 1 && _fleet.Sector == _civM.AccumulateSector)
            //{
            //    _fleet.Order = FleetOrders.IdleOrder;
            //    SplitIntoSingleShips(_fleet, _fleet.Order); // = at DoAccumulate
            //}
        }

        private static void DoAccumulateAt(Fleet _fleet, MapLocation _location)
        {
            // _location might be Accumulate or SystemAssault 1 or 2
            //CivilizationManager _civM = GameContext.Current.CivilizationManagers[_fleet.Owner.CivID];
            _fleet.GetCivM(_fleet, out CivilizationManager _civM);
            Sector _sector = new Sector(_location);
            Sector _accumulateSector = _sector;
            string _text = _civM.AccumulateLocation.ToString();
            //string _newline = Environment.NewLine;



            _text = "Step_6233:; " + CreateUpdateFleetText(_fleet, out _fleets_Summary)
            + " > AccumulateSector= " + _accumulateSector.Location
                        //+ " , Home= " + _civM.HomeSystem.Location /*+ " )"*/
                        ;
            if (_writeDirectly_Fleets) Console.WriteLine(_text);
            _fleet_Text += Environment.NewLine + _text;

            if (_fleet.IsConstructor
                || _fleet.IsSpy
                || _fleet.IsColonizer)
            {
                goto No_Accumulate;
            }

            if (_fleet.Owner.IsHuman)
            {
                //Debugger.Break();
            }

            if (_accumulateSector != null && _accumulateSector.Location.ToString() != "(0, 0)")
            {
                //always Fleet.Order > .ToString() because Fleet.Order always contain a special _fleet and more

                // Set_Order
                if (_fleet.Order.ToString() == FleetOrders.AccumulateLocation_Set_Here_Order.ToString())
                {
                    _text = "Step_6154:; Turn " + GameContext.Current.TurnNumber
                            + " > " + _fleets_Summary
                            + " ... ( no CIV to target > just usual business )"
                            ;
                    if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    _fleet_Text += Environment.NewLine + _text;

                    //if (_checkFleetOrders == true && _checkOnlyPlayersUnits)
                    if (_fleet.Owner.IsHuman) Debugger.Break();
                }


                // AccumulateLocation_Go_There_Order

                if (_fleet.Order.ToString() == FleetOrders.AccumulateLocation_Go_There_Order.ToString()
                    || _fleet.Order.ToString() == FleetOrders.IdleOrder.ToString())
                //_fleet.Order != FleetOrders.AssaultSystemOrder &&
                //_fleet.Order != FleetOrders.BuildStationOrder &&
                //_fleet.Order != FleetOrders.CollectDeuteriumOrder &&
                //_fleet.Order != FleetOrders.ColonizeOrder &&
                //_fleet.Order != FleetOrders. &&
                //)
                {
                    if (_accumulateSector != null && _accumulateSector.Location.ToString() != "(0, 0)")
                    {
                        _fleet.UnlockRoute();
                        _fleet.SetOrder(new AccumulateLocation_Go_There_Order());
                        _fleet.Activity = UnitActivity.NoActivity;

                        _fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, DeathStars, new List<Sector> { _accumulateSector }));
                        foreach (var ship in _fleet.Ships)
                        {
                            //CreateShipText(_ship, out string shipText);
                            _text = "Step_6235:; " + CreateShipText(ship, out string shipText)
                                + " ; Home= " + _civM.HomeSystem.Location /*+ " )"*/
                                + " > going to AccumulateSector= " + _accumulateSector.Location

                                ;
                            if (_writeDirectly_Fleets) Console.WriteLine(_text);
                            _fleet_Text += Environment.NewLine + _text;
                        }
                    }
                    //DoScout(_fleet);
                    //DoColonyShip(_fleet);
                }
            }
        No_Accumulate:;
        }

        private static void DoBattleShips(Fleet _fleet) // ToDo
        {
            CreateUpdateFleetText(_fleet, out string _fleetText);
            Console.WriteLine("Step_3434:; " + _fleetText + " > DoBattleShips");

            if (_fleet.Owner.IsHuman)
            {
                //Debugger.Break();  
            }


            Civilization _civ = _fleet.Owner;
            CivilizationManager _civM = GameContext.Current.CivilizationManagers[_civ];
            //_fleet.GetCivM(_fleet, out CivilizationManager _civM);
            //string Environment.NewLine = Environment.NewLine;
            //int _attackValue;
            //int _defenseSectorIntValue = 10;

            _fleet.AITypeUnit = UnitAIType.Attack;

            // nothing if one of these is part of a _fleet
            if (_fleet.HasConstructionShip) goto No_DoBattleShips;
            if (_fleet.IsColonizer || _fleet.AITypeUnit == UnitAIType.Colonizer) goto No_DoBattleShips;
            if (_fleet.AITypeUnit == UnitAIType.Medical) goto No_DoBattleShips; // medical ships on medical mission
            if (_fleet.IsDiplomatic) goto No_DoBattleShips;
            if (_fleet.IsSpy) goto No_DoBattleShips;
            if (_fleet.IsEscort) goto No_DoBattleShips;

            if (_fleet.IsCombatant)
            {
                _fleet.AITypeUnit = UnitAIType.Attack;
            }

            if (_fleet.IsTransport)
            {
                _fleet.AITypeUnit = UnitAIType.Transport;
            }

            //checkBattleShips = true;
            //if (_checkFleetOrders == true && _checkOnlyPlayersUnits && _checkShips_Battle)
            if (_fleet.Owner.IsHuman)
            {
                //Debugger.Break();  // see next - DoBattleShips
            }

            //_checkShips_Transport = true;
            //DoTransportShip(_fleet); // once again at the end

            if (_civM.Assault_TargetCiv == null)
            {
                // do not Accumulate if one of these is part of a _fleet && Assault_TargetCiv == null
                if (_fleet.IsScout) goto No_Accumulate;
                if (_fleet.IsScience) goto No_Accumulate;

                DoAccumulate(_fleet); // Accumulate regulary
                                      //DoTransportShip();

            No_Accumulate:;
            }

            //No_Accumulate:;


            //if (_civM.Assault_TargetCiv != null) // see Step_6260
            //{
            //    Sector _targetSector = _civM.HomeSystem.Sector;
            //    _targetSector = GetBestSystemFor_AccumulateFor_SystemAttack(
            //        GameContext.Current.CivilizationManagers[_civM.Assault_TargetCiv.CivID].HomeSystem.Sector
            //        , _civM.HomeSystem.Sector
            //        , out Sector _sectorToAccumulate);
            //    _civM.SystemAssault_Accumulate_Sector_1 = _targetSector;
            //}

            if (_fleet.Owner.IsHuman)
            {
                Debugger.Break();  // see next - DoBattleShips
            }


            // there is a SystemAssault_Accumulate_Sector_1 + Target_Civ
            if (_civM.Assault_TargetCiv != null && _civM.SystemAssault_Accumulate_Sector_1 != _civM.HomeSystem.Sector) // see Step_6260
            {
                StarSystem _targetSystem = GameContext.Current.CivilizationManagers[_civM.Assault_TargetCiv.CivID].HomeSystem;  // same as home until there is a target _civM

                if (_targetSystem == null)
                {
                    Debugger.Break();
                }

                StarSystem _ourHomeSystem = _civM.HomeSystem;
                if (_targetSystem == null && _targetSystem.Owner == _ourHomeSystem.Owner)
                {
                    _civM.Assault_TargetCiv = null;
                    _civM.SystemAssault_Accumulate_Sector_1 = null;
                    _civM.SystemAssault_Accumulate_Location_1 = new MapLocation();
                    return;
                }


                Fleet _attackFleet = new Fleet();
                //List<Ship> _allAttackWarShips = new List<Ship>();


                string _loc_otherHomeSystem = GameEngine.LocationString(_targetSystem.Location.ToString());
                bool _prepareSystemAssault = true;

                string _text = "Step_6150:; "
                    + CreateUpdateFleetText(_fleet, out _fleetText)
                    //+ " " + _civ.Name
                    //+ " at " + _ourHomeSystem.Location
                    /*+ " has "*/ + " > TARGET-Civilization > " + _civM.Assault_TargetCiv.Name
                    + " - HomeSystem at " + _loc_otherHomeSystem
                    ;
                if (_writeDirectly_Fleets) Console.WriteLine(_text);
                //_fleet_Text += Environment.NewLine + _text;

                if (_fleet.Owner.IsHuman)
                {
                    //Debugger.Break();
                }



                //}
                //else
                //{
                //    _text = "Step_6153:; UnitAI = Fleets > "
                //            //+ _ourHomeSystem.Location
                //            + " " + _civM.Name
                //            + " has * no * Assault_TargetCiv > " + _civM.Assault_TargetCiv.Name
                //            //+ " - HomeSystem at " + _targetSystem.Location
                //            ;

                //    if (_writeDirectly_Fleets) Console.WriteLine(_text);
                //    _fleet_Text += Environment.NewLine + _text;
                //}

                //_fleet.AITypeUnit = AITypeUnit.SystemAttack; //2024-06-09

                //Print_Ships_of_Fleet(_fleet);

                //foreach (var _ship in _fleet.Ships)
                //{
                //    CreateShipText(_ship, out string _shipText);

                //    _text = "Step_3279:; " + GameEngine.LocationString(_targetSystem.Location.ToString())
                //        + " Ship in Fleet= "
                //                    //+ _targetSystem.Name + "; Owner= " + _targetSystem.Owner
                //                    //+ "; Ship= " + _ship.ObjectID + "; Owner= " + _ship.Name
                //                    //+ "; Ship= " + _ship.Design + "; Owner= " + _ship.Name
                //                    + _shipText
                //                    ;
                //    if (_writeDirectly_Colony) Console.WriteLine(_text);
                //    _fleet_Text += Environment.NewLine + _text;
                //}

                //No_DoBattleShips:;
                //Debugger.Break();



                //List<Ship> _allShips = new List<Ship>();
                bool _fleetInvades = false;
                //int _attackValue;

                List<Fleet> _fleetsAtLocation = new List<Fleet>(GameContext.Current.Universe.Find<Fleet>(UniverseObjectType.Fleet))
                            .Where(f => f.Location == _fleet.Location && f.OwnerID == _civ.CivID).ToList();

                //_attacks


                if (_fleet.Owner.IsHuman)
                {
                    //Debugger.Break();  // see next - DoBattleShips
                }

                // Invasion ??
                if ((_fleet.Location == _targetSystem.Location // _fleet is at target
                || GameContext.Current.Universe.FindOwned<Colony>(_civM.Assault_TargetCiv).Any(o => o.Location == _fleet.Location))
                && _fleet.Ships.Any(o => o.ShipType == ShipType.Transport)
                && _fleet.Ships.Any(o => o.IsCombatant)
                )
                {
                    //for 
                    //_allShips.Add(_fleet.Ships[0]);
                    //_allShips.Add(GameContext.Current.Universe.FindOwned<Fleet>(_civ)
                    //    .Any(o => o.Location == _fleet.Location));




                    //List<Fleet> _fleetsAtLocation = new List<Fleet>(GameContext.Current.Universe.Find<Fleet>(UniverseObjectType.Fleet))
                    //        .Where(f => f.Location == _civM.AccumulateLocation && f.OwnerID == _civ.CivID).ToList();

                    //_civM.Assault_AttackValue = 0;

                    //foreach (var _f in _fleetsAtLocation)
                    //{
                    //    if (_f.IsCombatant) // && _fleetAtLocation.Ships.Any(o => o.ShipType == ShipType.Transport))
                    //    {
                    //        _civM.Assault_AttackValue += _f.Firepower(); // 
                    //    }

                    //    if (_f.IsTransport)
                    //    {
                    //        _civM.Assault_AttackValue += (int)_f.Ships[0].ShipDesign.WorkCapacity;
                    //    }
                    //}

                    //string _otherSystemText = GameEngine.LocationString(_targetSystem.Location.ToString())
                    //    + " " + _targetSystem.Name
                    //    //+ " > "

                    //    ;
                    //int _defenseSectorIntValue = 10; // 100 > 10 for testing
                    //if (_targetSystem.Sector.Station != null)
                    //{
                    //    _defenseSectorIntValue += _targetSystem.Sector.Station.Firepower() / 200;  // station only half
                    //    _defenseSectorIntValue += _targetSystem.Colony.Population.CurrentValue;  // 
                    //    if (_targetSystem.Colony.OrbitalBatteries.Count > 0)
                    //    {
                    //        _defenseSectorIntValue += _targetSystem.Colony.OrbitalBatteries.Count
                    //                         * (_targetSystem.Colony.OrbitalBatteries[0].Design.PrimaryWeapon.Count
                    //                         * _targetSystem.Colony.OrbitalBatteries[0].Design.PrimaryWeapon.Damage)
                    //                         + (_targetSystem.Colony.OrbitalBatteries[0].Design.SecondaryWeapon.Count
                    //                         * _targetSystem.Colony.OrbitalBatteries[0].Design.SecondaryWeapon.Damage)

                    //                         ;  // 


                    //    }
                    //}


                    //if (_civM.Assault_AttackValue > _defenseSectorIntValue * 1.1) // 10% more attack power than defense
                    //{
                    //    _text = "Step_3234:; "
                    //        + _otherSystemText
                    //         + " > AttackPower= " + _civM.Assault_AttackValue
                    //        + " vs " + _defenseSectorIntValue + " DefenseSector "
                    //        ;
                    //    if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    //    _fleet_Text += Environment.NewLine + _text;


                    //    _fleetInvades = true;
                    //    _fleet.Route.Clear();
                    //    _fleet.AITypeUnit = UnitAIType.SystemAttack;
                    //    _fleet.Order = FleetOrders.AssaultSystemOrder;
                    //    // ************* ToDo invasion 
                    //    SystemAssault(_fleet);
                    //    _text = "Step_3235:; " + CreateUpdateFleetText(_fleet, out _fleetText) + " Do Invasion at Target system = "
                    //            + _targetSystem.Name + ", Owner= " + _targetSystem.Owner
                    //            //+ " > ordered one more facility for > Industry"
                    //            ;
                    //    if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    //    _fleet_Text += Environment.NewLine + _text;
                    //    if (_fleet.Owner.IsHuman)
                    //    {
                    //        //Debugger.Break();
                    //    }
                    //    //GameLog.Core.AI.DebugFormat("## Do Invasion at Target system, _civM ={0}, targetCivilization ={1}", _

                    //    //GameLog.Core.AI.DebugFormat("## Do Invasion at Target system, _civM ={0}, targetCivilization ={1}", _civM.Name, _civM.Assault_TargetCiv.Name);
                    //    // send home and re-set AITypeUnit / can we already set ships to reserve here for return to home?
                    //}
                }

                if (_fleet.Owner.IsHuman)
                {
                    //Debugger.Break();  
                }

                //  no invasion due to not at the system location
                if (!_fleetInvades && _prepareSystemAssault && _civM.SystemAssault_Accumulate_Sector_1 != null)
                {
                    _text = "Step_3443:; "
                        + "" + CreateUpdateFleetText(_fleet, out _fleetText)
                    //+"" + _civ.Name
                    + " > Sector for SystemAttack="

                    + " " + GameEngine.LocationString(_civM.SystemAssault_Accumulate_Sector_1.Location.ToString())
                    ;
                    if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    //_fleet_Text += Environment.NewLine + _text;


                    if (_fleet.Owner.IsHuman)
                    {
                        Debugger.Break();
                    }

                    //if (_fleet.Location == _ourHomeSystem.Location)  // _fleet is at home
                    //{
                    _fleet.AITypeUnit = UnitAIType.SystemAttack;
                    _fleet.Order = FleetOrders.AssaultSystemOrder;

                    if (_fleet.Sector.Location != _targetSystem.Location || _fleet.Sector.Location == _civM.SystemAssault_Accumulate_Location_1)
                    {
                        _fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, DeathStars, new List<Sector> { _civM.SystemAssault_Accumulate_Sector_1 }));
                    }
                    else
                    {
                        _fleet.Route.Clear();
                        _fleet.SetRoute(TravelRoute.Empty);

                    }

                    _text = "Step_3444:; "
                                + "" + CreateUpdateFleetText(_fleet, out _fleetText)
                            //+"" + _civ.Name
                            + " > Sector for Accumulate_for_SystemAttack= "
                            + " " + GameEngine.LocationString(_targetSystem.Location.ToString())
                            ;
                    if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    _fleet_Text += Environment.NewLine + _text;

                    if (_fleet.Owner.IsHuman)
                    {
                        //Debugger.Break();
                    }


                    //_attackFleet.add = _fleet;
                    //List<Colony> _colonyTargets = GameContext.Current.Universe.FindOwned<Colony>(_civM.Assault_TargetCiv).ToList();

                    //foreach (var item in _colonyTargets)
                    //{
                    //    _text = "Step_3441:; " + GameEngine.LocationString(item.Location.ToString())
                    //        + " > possible target for SystemAttack"
                    //        + " > for " + _civM.Civilization.Key
                    //        + " " + GameEngine.LocationString(_ourHomeSystem.Location.ToString())
                    //                    //+ _targetSystem.Name + "; Owner= " + _targetSystem.Owner
                    //                    //+ "; Ship= " + _ship.ObjectID + "; Owner= " + _ship.Name
                    //                    //+ "; Ship= " + _ship.Design + "; Owner= " + _ship.Name
                    //                    //+ _shipText
                    //                    ;
                    //    if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    //    _fleet_Text += Environment.NewLine + _text;
                    //}

                    //if (_attackFleet.Ships.Count() >= _allAttackWarShips.Count())
                    //if (_prepareSystemAssault)
                    //    {
                    // send _fleet to other home system
                    //int civFirePower = CalculateFirePower(_civM);
                    //int targetFirePower = CalculateFirePower(_civM.Assault_TargetCiv, _targetSystem);
                    //if (targetFirePower * 1.1 < civFirePower)
                    //{
                    //    //_fleet.Owner = _civM; 
                    //    //_fleet.Location = _ourHomeSystem.Location;

                    //    _fleet.SetOrder(new EngageOrder());
                    //    if (_fleet.Location != _targetSystem.Location)
                    //    {
                    //        _fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { _targetSystem.Sector }));
                    //    }
                    //}
                    //else if (_colonyTargets.Count() > 1)
                    //{
                    //    double lastRange = 999;
                    //    _ = _colonyTargets.Remove(_targetSystem.Colony);
                    //    foreach (Colony colonyTarget in _colonyTargets)
                    //    {
                    //        MapLocation target = colonyTarget.Location;
                    //        MapLocation ai = _ourHomeSystem.Location;
                    //        double curretRange = Math.Sqrt(Math.Pow(target.X - ai.X, 2) + Math.Pow(target.Y - ai.Y, 2));
                    //        if (curretRange < lastRange)
                    //        {
                    //            lastRange = curretRange;
                    //            _fleet.Route.Clear();
                    //            _fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { colonyTarget.Sector }));
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    _civM.Assault_TargetCiv = null;
                    //}

                    //GameLog.Core.AI.DebugFormat("Civ {0} now in SystemAttack AITypeUnit target ={1}, attack _fleet location ={2} Count() ={3}, Route length {4} "
                    //    , _civM.Name, _civM.Assault_TargetCiv.Name, _attackFleet.Location, _attackFleet.Ships.Count, _attackFleet.Route.Length);
                    //}
                    //}


                    //Fleet _attackFleet = new Fleet();
                    //List<Ship> _allShips = new List<Ship>();


                    //int _FirePower_Accumulated = 0;

                    // AccumulateLocation
                    //if (_fleet.Location == _civM.SystemAssault_Accumulate_Location_1
                    ////|| GameContext.Current.Universe.FindOwned<Colony>(_civM.Assault_TargetCiv).Any(o => o.Location == _fleet.Location))
                    ////&& (!_fleet.Ships.Any(n => n.ShipType == ShipType.Transport)
                    ////|| !_fleet.Ships.Any(n => n.IsCombatant))
                    //) // No Combat and no transport
                    //{
                    //    if (_fleet.Owner.IsHuman)
                    //    {
                    //        //Debugger.Break();  // see next
                    //    }
                    //    //int _FirePower_Accumulated = 0;

                    //    //List<Ship> _allShipsAtAccumulate = new List<Ship>(GameContext.Current.Universe.Find<Ship>(UniverseObjectType.Ship)).ToList();
                    //    //List<Fleet> _fleetsAtLocation = new List<Fleet>(GameContext.Current.Universe.Find<Fleet>(UniverseObjectType.Fleet))
                    //    //    .Where(f => f.Location == _civM.AccumulateLocation && f.OwnerID == _civ.CivID).ToList();

                    //    //foreach (var item in _fleetsAtLocation)
                    //    //{
                    //    //    if (_fleet.Ships.Count > 1)
                    //    //    {
                    //    //        SplitIntoSingleShips(_fleet, _fleet.Order); // DoBattleShips
                    //    //    }
                    //    //}

                    //    // Update
                    //    //_fleetsAtLocation = GameContext.Current.Universe.Find<Fleet>(UniverseObjectType.Fleet)
                    //    //        .Where(f => f.Location == _fleet.Location && f.OwnerID == _civ.CivID).ToList();

                    //    //foreach (var item in _fleetsAtLocation)
                    //    //{
                    //    //    _FirePower_Accumulated += item.Firepower();
                    //    //    _fleet.Route.Clear();
                    //    //}

                    //    /*int _defenseSectorIntValue = 10;*/ // 100 > 10 for testing
                    //    //if (_targetSystem.Sector.Station != null)
                    //    //{
                    //    //    _defenseSectorIntValue += _targetSystem.Sector.Station.Firepower() / 200;  // station only half
                    //    //    _defenseSectorIntValue += _targetSystem.Colony.Population.CurrentValue;  // 
                    //    //    if (_targetSystem.Colony.OrbitalBatteries.Count > 0)
                    //    //    {
                    //    //        _defenseSectorIntValue += _targetSystem.Colony.OrbitalBatteries.Count
                    //    //                         * (_targetSystem.Colony.OrbitalBatteries[0].Design.PrimaryWeapon.Count
                    //    //                         * _targetSystem.Colony.OrbitalBatteries[0].Design.PrimaryWeapon.Damage)
                    //    //                         + (_targetSystem.Colony.OrbitalBatteries[0].Design.SecondaryWeapon.Count
                    //    //                         * _targetSystem.Colony.OrbitalBatteries[0].Design.SecondaryWeapon.Damage)

                    //    //                         ;  // 


                    //    //    }
                    //    //}

                    //    //if (_fleet.Owner.IsHuman)
                    //    //{
                    //    //    //Debugger.Break();
                    //    //}


                    //    //if (_FirePower_Accumulated > 0 && _FirePower_Accumulated > _defenseSectorIntValue)
                    //    //{
                    //    //    if (_fleet.Owner.IsHuman)
                    //    //    {
                    //    //        Debugger.Break();
                    //    //    }

                    //    //    _fleet.AITypeUnit = UnitAIType.SystemAttack;
                    //    //    _fleet.Activity = UnitActivity.Mission;
                    //    //    _fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, DeathStars, new List<Sector> { _targetSystem.Sector }));
                    //    //    _text = "Step_3255:; "  /*+ _loc_otherHomeSystem + " > "*/
                    //    //            + CreateUpdateFleetText(_fleet, out _fleetText) + Environment.NewLine + " > Set Route to Target system = "
                    //    //            + _targetSystem.Name
                    //    //            + " at " + _targetSystem.Location
                    //    //            + ", Owner= " + _targetSystem.Owner
                    //    //            //+ " > ordered one more facility for > Industry"
                    //    //            + " > _FirePower_Total= " + _FirePower_Accumulated
                    //    //            + " vs _defense= " + _defenseSectorIntValue
                    //    //            ;
                    //    //    if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    //    //    _fleet_Text += Environment.NewLine + _text;
                    //    //}

                    //    //if (_fleet.Owner.IsHuman)
                    //    //{
                    //    //    //Debugger.Break();  // here_DoBattle
                    //    //}
                    //}


                    //else
                    // Target system not longer existing ?
                    // Invasion ??
                    //if (_fleet.AITypeUnit == AITypeUnit.SystemAttack
                    //    && _fleet.Location == _targetSystem.Location
                    //    //         && _fleet.Location != _targetSystem.Location
                    //    && (_fleet.Route.IsEmpty)
                    //    /* || !_fleet.Route.Waypoints.Contains(_targetSystem.Location))*/)
                    //{
                    //    Debugger.Break();

                    //    _fleet.AITypeUnit = AITypeUnit.NoUnitAI;
                    //    _fleet.Activity = UnitActivity.NoActivity;
                    //    _fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, DeathStars, new List<Sector> { _ourHomeSystem.Sector }));
                    //    _text = "Step_3233:; " + _fleetText + _loc_otherHomeSystem + " Set Route to our own HomeSystem = "
                    //            + _ourHomeSystem.Name + ", Owner= " + _ourHomeSystem.Owner
                    //            //+ " > ordered one more facility for > Industry"
                    //            ;
                    //    if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    //    _fleet_Text += Environment.NewLine + _text;
                    //    _civM.Assault_TargetCiv = null;  // 2024-06-09 why was this ?? // Target system not longer existing ?
                    //}
                    //else if (_fleet.Activity == UnitActivity.NoActivity
                    //    && _fleet.AITypeUnit == AITypeUnit.Reserve
                    //    && _fleet.Location != _ourHomeSystem.Location
                    //    && (_fleet.Route.IsEmpty || !_fleet.Route.Waypoints.Contains(_targetSystem.Location)))
                    //{
                    //    _fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { _ourHomeSystem.Sector }));
                    //    _fleet.AITypeUnit = AITypeUnit.NoUnitAI;
                    //    _fleet.Activity = UnitActivity.NoActivity;
                    //}
                    //
                    //No_DoBattleShips:;
                    //    Debugger.Break();

                    //_fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, DeathStars
                    //    , new List<Sector> { _civM.SystemAssault_Accumulate_Sector_1 }));
                    _text = "Step_3258:; "  /*+ _loc_otherHomeSystem + " > "*/
                            + CreateUpdateFleetText(_fleet, out _fleetText)
                            + " > Set Route to SystemAssault_Accumulate_Sector_1= " + _civM.SystemAssault_Accumulate_Sector_1
                            //+ " > _FirePower_Accumulated=" + _FirePower_Accumulated
                            ;
                    //if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    _fleet_Text += Environment.NewLine + _text;
                }
                //No_DoBattleShips:




                //_checkShips_Transport = true;
                //DoTransportShip(_fleet);

            }

        No_DoBattleShips:;

        }

        private static void SplitIntoSingleShips(Fleet fleet, FleetOrder order)
        {
            //_fleet.RemoveShip(_ship);
            foreach (var ship in fleet.Ships)
            {
                Fleet newfleet = new Fleet();
                ship.CreateFleet();
                newfleet.Owner = fleet.Owner;
                newfleet.Route.Clear();
                newfleet.SetOrder(order);
                if (newfleet.Order == null)
                {
                    newfleet.SetOrder(order);
                }
            }
        }

        private static void CloakAll(Fleet fleet)
        {
            if (fleet.Ships.Count > 0)
            {
                //Make sure all _fleets are cloaked
                foreach (Ship ship in fleet.Ships.Where(ship => ship.CanCloak && !ship.IsCloaked))
                {
                    //GameLog.Core.AI.DebugFormat("Turn {0}: Cloaking {1} {2} {3}"
                    //, GameContext.Current.TurnNumber.ToString(), _ship.ObjectID, _ship.Name, _ship.ClassName);
                    ship.IsCloaked = true;
                }
                //_text = "UnitAI-DoTurn for; " 
                //    + _fleet.ObjectID + "; "
                //    + _fleet.Name + "; "
                //    + _fleet.Owner + "; "
                //    + _fleet.Location + "; "
                //    + "Assault_TargetCiv=" + _civM.Assault_TargetCiv
                //    ;
                //if (_writeDirectly_Colony) Console.WriteLine(_text);
            }
        }


        private static void DoScienceShip(Fleet _fleet)
        {
            _fleet.AITypeUnit = UnitAIType.Science;
            //string _newline = Environment.NewLine;

            //CivilizationManager _civM = GameContext.Current.CivilizationManagers[_fleet.OwnerID];
            _fleet.GetCivM(_fleet, out CivilizationManager _civM);

            //if (_checkFleetOrders == true && _checkOnlyPlayersUnits && _checkShips_Science)



            //CreateUpdateFleetText(_fleet, out string _fleetText);
            //_text = "Step_6290:; " + _fleets_Summary + " > next > _fleet.IsScience";
            //if (_writeDirectly_Colony) Console.WriteLine(_text);
            //if (_fleet.IsScience)
            //{

            string _text = "Step_6910:; "
                + CreateUpdateFleetText(_fleet, out string _fleetText) + " > #### _fleet.IsScience"
                //+ " " + _fleet.ObjectID
                //+ " " + _fleet.Name
                ////+ " to go to " + bestSystemToColonize.Name
                //+ " " + _fleet.Location
                ;
            if (_writeDirectly_Fleets) Console.WriteLine(_text);
            _fleet_Text += Environment.NewLine + _text;

            //if (_fleet.Owner.IsHuman)
            //{
            //    Debugger.Break();
            //}

            if (_fleet.Order == FleetOrders.IdleOrder)
                _fleet.Activity = UnitActivity.NoActivity;
            if (_fleet.Order.ToString() == FleetOrders.AccumulateLocation_Go_There_Order.ToString())
                _fleet.Activity = UnitActivity.NoActivity;

            //checkScienceShips = true;

            if (_fleet.Owner.IsHuman)
            {
                //Debugger.Break();
            }

            if (_fleet.Activity == UnitActivity.GoToAccumulateSector
                && _civM.AccumulateLocation != null
                && _fleet.Location == _civM.AccumulateLocation
                )
            {
                _fleet.Activity = UnitActivity.NoActivity;
                _fleet.Route.Clear();
            }

            if (_fleet.Route != null && _fleet.Route.Waypoints.Count > 0
                && _fleet.Route.Waypoints.Last() == _civM.HomeSystem.Location)
            {
                _fleet.Activity = UnitActivity.NoActivity;
                _fleet.Route.Clear();
            }

            //Science Ship
            if (_fleet.Activity == UnitActivity.NoActivity /*|| _fleet.Route.IsEmpty || _fleet.Order.IsComplete*/)
            {
                if (GetBestSystemFor_Science(_fleet, out StarSystem bestSystemForScience))
                {
                    if (bestSystemForScience != null)
                    {
                        //bool hasOurSpyNetwork = CheckForSpyNetwork(bestSystemForScience.Owner, _fleet.Owner); // false for e.g. Nebula
                        //if (!hasOurSpyNetwork)
                        //{
                        if (bestSystemForScience.Sector == _fleet.Sector)
                        {
                            _fleet.Route.Clear();

                            //_fleet.SetOrder(new MissionOrder());  // Does this send the ship back to home system ?
                            //_fleet.Order = FleetOrder.
                            _fleet.AITypeUnit = UnitAIType.Science;
                            _fleet.Activity = UnitActivity.Mission;
                            _text = "Step_6916:; " + _fleetText + " is at research location " + bestSystemForScience.Location;
                            if (_writeDirectly_Fleets) Console.WriteLine(_text);
                            _fleet_Text += Environment.NewLine + _text;
                            // GameLog.Core.AI.DebugFormat("Science _fleet {0} at Research location {1}", _fleet.ObjectID, _fleet.Location);
                        }
                        else
                        {
                            //GetFleetOwner(_fleet);
                            _fleet.AITypeUnit = UnitAIType.Science;
                            _fleet.Activity = UnitActivity.Mission;
                            _fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, DeathStars, new List<Sector> { bestSystemForScience.Sector }));


                            _text = "Step_6917:; " + _fleetText + " > SET Route to " + bestSystemForScience.Location;
                            if (_writeDirectly_Fleets) Console.WriteLine(_text);
                            _fleet_Text += Environment.NewLine + _text;
                            // GameLog.Core.AI.DebugFormat("Ordering science _fleet {0} to {1}", _fleet.ObjectID, bestSystemForScience);
                        }
                        //}
                    }
                }
                else
                {
                    //not found a bestSystemForScience > go to Explore
                    _fleet.SetOrder(new ExploreOrder());
                    _text = "Step_6911:; " + _fleetText + " > #### not found a bestSystemForScience > go to Explore"
                            //+ " " + _fleet.ObjectID
                            //+ " " + _fleet.Name
                            ////+ " to go to " + bestSystemToColonize.Name
                            //+ " " + _fleet.Location
                            ;
                    if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    _fleet_Text += Environment.NewLine + _text;
                    //GameLog.Core.AI.DebugFormat("Nothing to do for science _fleet {0}", _fleet.ObjectID);
                }
                //}
                //}
                //}
            }
        }

        private static void DoDiplomatic(Fleet _fleet)
        {
            string _fleetText;
            //string _newline = Environment.NewLine;
            _fleet.GetCivM(_fleet, out CivilizationManager _civM);
            //_text = "Step_6270:; next > _fleet.IsDiplomatic   " + _fleets_Summary;
            //if (_writeDirectly_Colony) Console.WriteLine(_text);

            //if (_fleet.IsDiplomatic)
            //{
            _fleet.AITypeUnit = UnitAIType.Diplomatic;

            string _text = "Step_6711:; " + CreateUpdateFleetText(_fleet, out _fleetText) //+ " > _fleet.IsDiplomatic"
                ;
            if (_writeDirectly_Fleets) Console.WriteLine(_text);
            _fleet_Text += Environment.NewLine + _text;

            //checkDiplomaticShips = true;
            //if (_checkFleetOrders == true && _checkOnlyPlayersUnits && _checkShips_Diplomatic)

            if (_fleet.Owner.IsHuman)
            {
                //Debugger.Break();            
            }


            // Send diplomatic _ship and influence _order, but what do we do with race traits and diplomacy?
            //if (_fleet.Owner.Traits.Contains(CivTraits.Peaceful.ToString()) || _fleet.Owner.Traits.Contains(CivTraits.Kindness.ToString()))
            //{
            //    // ToDo;
            //}

            //Always search for a better option
            if (/*_fleet.Activity == UnitActivity.NoActivity || */_fleet.Route.IsEmpty /*|| _fleet.Order.IsComplete*/)
            {
                if (GetBestColonyFor_Diplomacy(_fleet, out Colony _bestSystemForDiplomacy))
                {
                    //if (_bestSystemForDiplomacy.OwnerID < 6)
                    if (_bestSystemForDiplomacy.OwnerID < 999)
                    {
                        if (_bestSystemForDiplomacy.Sector == _fleet.Sector)
                        {
                            _fleet.SetOrder(new InfluenceOrder());
                            _fleet.AITypeUnit = UnitAIType.Diplomatic;
                            _fleet.Activity = UnitActivity.Mission;
                            // GameLog.Core.AI.DebugFormat("Ordering diplomacy _fleet {0} in {1} to influence", _fleet.ObjectID, _fleet.Location);
                        }
                        else
                        {
                            //GetFleetOwner(_fleet);
                            Civilization civ = GameContext.Current.CivilizationManagers[_fleet.Owner.CivID].Civilization;
                            _fleet.Owner = civ;
                            _fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, DeathStars, new List<Sector> { _bestSystemForDiplomacy.Sector }));
                            _fleet.AITypeUnit = UnitAIType.Diplomatic;
                            _fleet.Activity = UnitActivity.Mission;
                            _text = "Step_6712:; " + CreateUpdateFleetText(_fleet, out _fleetText)
                                    + " > ordered to Influence of " + _bestSystemForDiplomacy.Location
                                    + " " + _bestSystemForDiplomacy.Name
                                    + ", Owner= " + _bestSystemForDiplomacy.Owner
                                    ;
                            if (_writeDirectly_Fleets) Console.WriteLine(_text);
                            _fleet_Text += Environment.NewLine + _text;
                            //  GameLog.Core.AI.DebugFormat("Ordering diplomacy _fleet {0} to {1}", _fleet.ObjectID, _bestSystemForDiplomacy);
                        }
                    }
                }
                //else
                //{
                //  GameLog.Core.AI.DebugFormat("Nothing to do for diplomacy _fleet {0}", _fleet.ObjectID);
                //}
                //}
            }

            if (_fleet.Sector == _civM.HomeSystem.Sector && _civM.Z_Ship_Diplomatic_Available > 1) // and nothing to 
            {
                DestroyFleet(_fleet);
            }
        }

        private static void DoSpy(Fleet _fleet)
        {
            _fleet.AITypeUnit = UnitAIType.Spy;
            //_text = "Step_6280:; next > _fleet.IsSpy   " + _fleet_Text;
            //if (_writeDirectly_Colony) Console.WriteLine(_text);
            //if (_fleet.IsSpy) // install spy network
            //{
            //string _newline = Environment.NewLine;
            _fleet.GetCivM(_fleet, out CivilizationManager _civM);

            string _text = "Step_6810:; " + CreateUpdateFleetText(_fleet, out _fleet_Text)
                + " > #### _fleet.IsSpy"

                ;


            //if (_writeDirectly_Fleets) Console.WriteLine(_text);
            _fleet_Text += Environment.NewLine + _text;

            var _bestSystem = GetBestColonyFor_Spying(_fleet, out Colony bestSystemFor_Spying);

            //checkSpyShips = true;
            //if (_checkFleetOrders == true && _checkOnlyPlayersUnits && _checkShips_Spy)
            if (_fleet.Owner.IsHuman)
            {
                //Debugger.Break();
            }


            // Set Spy Order if at _bestSystem
            if (_fleet.Sector.System != null && _fleet.Sector.System.ToString() == _bestSystem.ToString())
            {
                _fleet.SetOrder(new SpyOnOrder());

            }
            else if (_fleet.Order.ToString() == FleetOrders.IdleOrder.ToString())
            {
                // ff Idle set NoActivity
                _fleet.Activity = UnitActivity.NoActivity;
            }



            //Spy
            if (_fleet.Route.IsEmpty && _fleet.Activity != UnitActivity.Mission/*_fleet.Activity == UnitActivity.NoActivity || */ /*|| _fleet.Order.IsComplete*/)
            {
                if (GetBestColonyFor_Spying(_fleet, out bestSystemFor_Spying))
                {
                    if (bestSystemFor_Spying != null && bestSystemFor_Spying.OwnerID < 7)
                    {
                        bool hasOurSpyNetwork = CheckForSpyNetwork(bestSystemFor_Spying.Owner, _fleet.Owner);
                        if (!hasOurSpyNetwork)
                        {
                            if (bestSystemFor_Spying.Sector == _fleet.Sector)
                            {
                                _fleet.SetOrder(new SpyOnOrder()); // install spy network
                                                                   //_fleet.AITypeUnit = AITypeUnit.NoUnitAI;
                                _fleet.Activity = UnitActivity.Mission;
                                _text = "Step_6822:; " + CreateUpdateFleetText(_fleet, out _fleet_Text)
                                        + " > ORDER to install a spy network"
                                        ;

                                //if (_writeDirectly_Fleets) Console.WriteLine(_text);
                                _fleet_Text += Environment.NewLine + _text;
                                // GameLog.Core.AI.DebugFormat("Ordering spy _fleet {0} in {1} to install spy network", _fleet.ObjectID, _fleet.Location);
                            }
                            else
                            {
                                //GetFleetOwner(_fleet);
                                Civilization civ = GameContext.Current.CivilizationManagers[_fleet.Owner.CivID].Civilization;
                                _fleet.Owner = civ;

                                _fleet.AITypeUnit = UnitAIType.Spy;
                                _fleet.Activity = UnitActivity.Mission;
                                _fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, DeathStars, new List<Sector> { bestSystemFor_Spying.Sector }));
                                _text = "Step_6822:; " + CreateUpdateFleetText(_fleet, out _fleet_Text)
                                        + " > ORDER to install a spy network"
                                        ;

                                if (_writeDirectly_Fleets) Console.WriteLine(_text);
                                //_fleet_Text += _newline + _text;
                                // GameLog.Core.AI.DebugFormat("Ordering spy _fleet {0} in {1} to install spy network", _fleet.ObjectID, _fleet.Location);
                                // GameLog.Core.AI.DebugFormat("Ordering spy _fleet {0} to {1}", _fleet.ObjectID, bestSystemFor_Spying);
                            }
                        }
                    }
                }
                else // if idle = no route, no mission > EXPLORE
                {
                    _fleet.SetOrder(new ExploreOrder());
                    _fleet.Activity = UnitActivity.Explore;
                    _text = "Step_6813:; " + CreateUpdateFleetText(_fleet, out string _fleetText) + " > no bestSystemFor_Spying for Spy Ship > now EXPLORE "
                            //+ "; Activity= " + _fleet.Activity.ToString()

                            //+ "; Route empty= " + _fleet.Route.IsEmpty
                            //+ "; to go > " + _bestSectorForStation.Location
                            //+ " " + _bestSectorForStation.Name
                            ;
                    //if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    _fleet_Text += Environment.NewLine + _text;
                    //GameLog.Core.AI.DebugFormat("Nothing to do for spy _fleet {0}", _fleet.ObjectID);

                }
            }

            //if (_checkFleetOrders == true && _checkOnlyPlayersUnits)
            //{
            //    //Debugger.Break();  // Do Spy
            //}

            if (_fleet.Sector == _civM.HomeSystem.Sector && _civM.Z_Ship_Spy_Available > 2) // and nothing to 
            {
                DestroyFleet(_fleet);
            }

        }

        private static void DoMedical(Fleet _fleet)
        {
            CreateUpdateFleetText(_fleet, out string _fleetText);
            //string _newline = Environment.NewLine;
            _fleet.GetCivM(_fleet, out CivilizationManager _civM);
            //_text = "Step_6269:; next > _fleet.IsMedical   " + _fleets_Summary;
            //if (_writeDirectly_Colony) Console.WriteLine(_text);
            //if (_fleet.IsMedical)
            //{
            string _text = "Step_6610:; " + _fleetText + " > _fleet.IsMedical: "
                ;
            if (_writeDirectly_Fleets) Console.WriteLine(_text);
            _fleet_Text += Environment.NewLine + _text;

            //checkMedicalShips = true;
            //if (_checkFleetOrders == true && _checkOnlyPlayersUnits && _checkShips_Medical)
            if (_fleet.Owner.IsHuman)
            {
                //Debugger.Break();
            }

            //Medical
            if (_fleet.Route.IsEmpty
                && _fleet.Sector.System != null && _fleet.Sector.System.Colony != null
                //&& _fleet.Sector.Owner == _fleet.Owner /*|| !_fleet.Sector.Owner.IsEmpire*/
                && !_fleet.Sector.System.Colony.Health.IsMaximized) // not maximized !
            {
                _fleet.SetOrder(new MedicalOrder());
                _fleet.Order = FleetOrders.MedicalOrder;
                _fleet.Activity = UnitActivity.Mission;
                _fleet.AITypeUnit = UnitAIType.Medical;
                _fleet.UnlockRoute();
            }
            else
            {
                _fleet.Activity = UnitActivity.NoActivity;
            }


            if (_fleet.Activity == UnitActivity.NoActivity)// || _fleet.Route.IsEmpty/* || _fleet.Order.IsComplete*/)
            {
                if (GetBestColonyFor_Medical(_fleet, out Colony bestSystemForMedical))
                {
                    _text = "Step_7684:; " + _fleetText;
                    if (bestSystemForMedical != null && bestSystemForMedical.Sector == _fleet.Sector)
                    {
                        //Colony medical treatment
                        _fleet.SetOrder(new MedicalOrder());
                        _fleet.AITypeUnit = UnitAIType.Medical;
                        _fleet.Activity = UnitActivity.Mission;
                        _text += " > Order= " + _fleet.Order;
                        // GameLog.Core.AI.DebugFormat("Ordering medical _fleet {0} in {1} to treat the population", _fleet.ObjectID, _fleet.Location);
                    }
                    else if (bestSystemForMedical != null)
                    {
                        //GetFleetOwner(_fleet);
                        Civilization civ = GameContext.Current.CivilizationManagers[_fleet.Owner.CivID].Civilization;
                        _fleet.Owner = civ;
                        _fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, DeathStars, new List<Sector> { bestSystemForMedical.Sector }));
                        _fleet.AITypeUnit = UnitAIType.Medical;
                        _fleet.Activity = UnitActivity.Mission;
                        _text += " > Heading to " + bestSystemForMedical.Location;
                        // GameLog.Core.AI.DebugFormat("Ordering medical _fleet {0} to {1}", _fleet.ObjectID, bestSystemForMedical);
                    }
                    //_text = "Step_6610:; " + _fleets_Summary + " > _fleet.IsMedical: "
                    //    + " " + _fleet.ObjectID
                    //    + " " + _fleet.Name
                    //    + " at " + bestSystemForMedical.Name
                    //    + " " + _fleet.Location
                    //    ;
                    if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    _fleet_Text += Environment.NewLine + _text;
                }
                //else
                //{
                //GameLog.Core.AI.DebugFormat("Nothing to do for medical _fleet {0}", _fleet.ObjectID);
                //}

                //}
            }


            if (_fleet.Sector == _civM.HomeSystem.Sector && _civM.Z_Ship_Medical_Available > 3) // and nothing to 
            {
                DestroyFleet(_fleet);
            }
        }

        private static void DoConstructionShip(Fleet _fleet)
        {

            _fleet.AITypeUnit = UnitAIType.Constructor;
            string _shipText = "";
            bool bool_bestSector = false;
            Sector _bestSectorForStation = null;
            string _newline = Environment.NewLine;

            Civilization _civ = GameContext.Current.CivilizationManagers[_fleet.Owner.CivID].Civilization;
            //CivilizationManager _civM = GameContext.Current.CivilizationManagers[_fleet.Owner.CivID];
            _fleet.GetCivM(_fleet, out CivilizationManager _civM);
            //_text = "Step_6250:; next > _fleet.IsConstructor   " + _fleets_Summary;
            //if (_writeDirectly_Colony) Console.WriteLine(_text);
            //if (_fleet.IsConstructor) // || _fleet.AITypeUnit == AITypeUnit.Constructor || (_fleet.Ships.Any(a => a.ShipType == ShipType.Construction) && _fleet.Ships.Count() == 2))
            //{
            string _text = "Step_6510:; " + CreateUpdateFleetText(_fleet, out string _fleetText) + " > _fleet.IsConstructor:"
                    //+ " " + _fleet.ObjectID
                    //+ " " + _fleet.Name
                    ////+ " to go to " + bestSystemToColonize.Name
                    //+ " " + _fleet.Location
                    ;
            if (_writeDirectly_Fleets) Console.WriteLine(_text);
            _fleet_Text += _newline + _text;

            if (_fleet.Owner.IsHuman)
            {
                //Debugger.Break();    //checkconstruction // for search + finding this place
            }

            //List<Fleet> allCivFleets = GameContext.Current.Universe.FindOwned<Fleet>(_civ).ToList();

            List<Fleet> _allConstructFleetsHere = GameContext.Current.Universe.FindOwned<Fleet>(_civ).ToList()
                .Where(a => a.Sector == _fleet.Sector)
                .Where(a => a.IsConstructor).ToList();// || (a.MultiFleetHasAConstructor && a.Ships.Count == 2)).ToList();


            //bool _checkShips_Construction = true;
            //if (_checkFleetOrders == true && _checkOnlyPlayersUnits && _checkShips_Construction)


            // Borg + System in Sector + No station + No _location owner ( Borg can not colonize )
            if (_fleet.Owner.Key == "BORG"
                && !_fleet.Sector.IsOwned
                && _fleet.Sector.Station == null
                && _fleet.Sector.System != null
                && _fleet.Sector.System.StarType != StarType.NeutronStar
                && _fleet.Sector.System.StarType != StarType.BlackHole
                //&& _fleet.Sector.System.StarType != StarType.RadioPulsar 


                )  // "occupy" system for not falling into other hands - Borg can't colonize !
            {
                _fleet.Activity = UnitActivity.BuildStation;
                //_fleet.Order = FleetOrders.BuildStationOrder;

                _text += " - Order to build a station...";
                if (_writeDirectly_Fleets) Console.WriteLine(_text);
                _fleet_Text += _newline + _text;

                //if (_fleet.build == null || _fleet.Order.PercentComplete < 1)
                //{
                BuildStation(_fleet, _allConstructFleetsHere);
                //_fleet.Activity = UnitActivity.Hold;
                //_fleet.Route.Clear();
                goto EndofConstructionShip;
                //}
            }


            if (_fleet.Owner.IsHuman)
            {
                ////Debugger.Break();    //checkconstruction // for search + finding this place
            }

            // normally: Build a station if Activity is "build"
            if (_fleet.Activity == UnitActivity.BuildStation && _fleet.Sector.Station == null && !_fleet.Sector.IsOwned)
            {
                _fleet.Activity = UnitActivity.BuildStation;
                //_fleet.Order = FleetOrders.BuildStationOrder;
                if (_fleet.Activity == UnitActivity.BuildStation/* && _fleet.Order == UnitActivity.BuildStation*/)
                {


                    if (_fleet.Order.PercentComplete == null || _fleet.Order.PercentComplete * 100 < 1)
                    {
                        BuildStation(_fleet, _allConstructFleetsHere);
                        _fleet.Route.Clear();

                        _text += " - Order to build a station...";
                        if (_writeDirectly_Fleets) Console.WriteLine(_text);
                        _fleet_Text += _newline + _text;

                        goto EndofConstructionShip;
                    }
                }
            }
            else
            {
                //_fleet.Activity = UnitActivity.NoActivity;
                //bool_bestSector = GetBestSectorFor_BuildStation(_fleet, _allConstructFleetsHere, out _bestSectorForStation);
            }

            if (!_fleet.Ships.Any(x => x.ShipType == ShipType.Construction))
            {
                Debugger.Break();// a _fleet that really has a constuctor
            }

            //_checkShips_Construction = true;
            //if (_checkFleetOrders == true && _checkOnlyPlayersUnits && _checkShips_Construction)
            if (_fleet.Owner.IsHuman)
            {
                //Debugger.Break();    //checkconstruction // for search + finding this place
            }


            //GameLog.Client.AI.DebugFormat("*////*Top of Constuctor, Owner ={0}, shiptype ={1}, #Ships ={2}, Activity ={3} duration ={4} Route empty ={5}",
            //         _fleet.Owner.Key, _fleet.Ships[0].ShipType, _fleet.Ships.Count(), _fleet.Activity, _fleet.ActivityDuration, _fleet.Route.IsEmpty);

            bool_bestSector = GetBestSectorFor_BuildStation(_fleet, _allConstructFleetsHere, out _bestSectorForStation);

            if (_fleet.Route.IsEmpty || _fleet.Route.Waypoints.Count < 2)
            {
                // is it necassary ?? > yes due to one of the > if (bool_bestSector)

                //bool_bestSector = GetBestSectorFor_BuildStation(_fleet, _allConstructFleetsHere, out _bestSectorForStation);

                if (_civM.StrandedShipsSector != null && _civM.StrandedShipsSector.Location != _civM.HomeSystem.Location)
                {
                    _bestSectorForStation = _civM.StrandedShipsSector;
                }

                if (_bestSectorForStation == null)
                {
                    bool_bestSector = false;
                    _bestSectorForStation = _civM.HomeSystem.Sector; // go home
                    if (_civM.StrandedShipsSector != null)
                    {
                        _bestSectorForStation = _civM.StrandedShipsSector;
                    }
                    if (_civM.AccumulateSector != null)
                    {
                        _bestSectorForStation = _civM.AccumulateSector;
                    }

                    BuildStation(_fleet, _allConstructFleetsHere);
                }


                //else
                //{
                //    //Travel_to_Sector(_fleet, _bestSectorForStation);
                //    _bestSectorForStation = _civM.HomeSystem.Sector; // go home
                //    _fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, DeathStars, new List<Sector> { _bestSectorForStation }));

                //    _text = "Step_6521:; " + _fleetText + " > Found Constructor" // Values > "Step_6913"
                //        + "; Activity= " + _fleet.Activity.ToString()

                //        //+ "; Route empty= " + _fleet.Route.IsEmpty
                //        + "; to go > " + _bestSectorForStation.Location
                //        //+ " " + _bestSectorForStation.Name
                //        ;
                //    if (_writeDirectly_Fleets) Console.WriteLine(_text);
                //    _fleet_Text += _newline + _text;
                //}

                //if (_checkFleetOrders == true && _checkOnlyPlayersUnits && _checkShips_Construction)
                if (_fleet.Owner.IsHuman)
                {
                    //Debugger.Break();    //checkconstruction // for search + finding this place
                }

                if (bool_bestSector == false && _fleet.Sector.Station == null && _fleet.IsStranded)
                {
                    bool_bestSector = true;
                    _bestSectorForStation = _fleet.Sector;
                }

                if (bool_bestSector)
                {
                    if (_bestSectorForStation.Location == _fleet.Sector.Location || GetDistanceTo(_fleet.Sector.Location, _bestSectorForStation.Location) < 2)
                    {
                        _fleet.Activity = UnitActivity.BuildStation;
                        //_fleet.Order = FleetOrders.BuildStationOrder;
                        if (_fleet.Activity == UnitActivity.BuildStation/* && _fleet.Order == UnitActivity.BuildStation*/)
                        {
                            _text += " - Order to build a station...";
                            //if (_writeDirectly_Fleets) Console.WriteLine(_text);
                            _fleet_Text += _newline + _text;

                            if (_fleet.Order.PercentComplete == null || _fleet.Order.PercentComplete * 100 < 1)
                            {
                                BuildStation(_fleet, _allConstructFleetsHere);
                                //_fleet.Route.Clear();
                            }
                            goto EndofConstructionShip;
                        }
                    }

                }
            }


            //GameLog.Client.AI.DebugFormat("Found Constructor, _fleet location ={0}, Owner ={1}, #Ships ={2}, Activity ={3} duration ={4} Route empty ={5}",
            //        _fleet.Sector.Name, _fleet.Owner.Key, _fleet.Ships.Count(), _fleet.Activity, _fleet.ActivityDuration, _fleet.Route.IsEmpty);

            //ShipType.Construction
            //if (_fleet.Activity == UnitActivity.BuildStation && )
            //{
            //    _text += " - Construction ongoing...";
            //    if (_writeDirectly_Fleets) Console.WriteLine(_text);
            //    _fleet_Text += _newline + _text;
            //    //GameLog.Client.AI.DebugFormat("Start Construction, _fleet _order ={0}, Owner ={1}, Sector ={2},{3}, Activity ={4} duration ={5} start ={6}",
            //    //_fleet.Order.OrderName, _fleet.Owner.Key, _fleet.Sector.Name, _fleet.Location, _fleet.Activity, _fleet.ActivityDuration, _fleet.ActivityStart);
            //}


            //ShipType.Construction
            if (_fleet.IsStranded || !_fleet.CanMove) // && _fleet.AITypeUnit != AITypeUnit.Building) // && !systemOfEmpire(_fleet)
            {
                if (_fleet.Ships.Count > 1)
                {
                    SplitIntoSingleShips(_fleet, _fleet.Order); // = at DoConstructionShip
                }


                if (_fleet.AITypeUnit != UnitAIType.Building && _fleet.Sector.Station == null)
                {
                    _text = "Step_6527:; " + _fleetText + " > Constructor can't move"
                            + "; Activity= " + _fleet.Activity.ToString()

                            + "; Route empty= " + _fleet.Route.IsEmpty
                            + "; to go > " + _bestSectorForStation.Location
                            + " " + _bestSectorForStation.Name
                            ;
                    if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    _fleet_Text += _newline + _text;

                    _text = "Step_6527:; " + _fleetText
                            + " > stranded= " + _fleet.IsStranded
                            + "; Activity= " + _fleet.Activity.ToString()

                            + "; Route empty= " + _fleet.Route.IsEmpty
                            + "; to go > " + _bestSectorForStation.Location
                            + " " + _bestSectorForStation.Name
                            ;
                    if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    _fleet_Text += _newline + _text;
                    //GameLog.Client.AI.DebugFormat("_fleet stranded ={0}, Owner ={1}, Sector ={2},{3}, Activity ={4} duration ={5} start ={6} _order ={7}",
                    //_fleet.IsStranded, _fleet.Owner.Key, _fleet.Sector.Name, _fleet.Location, _fleet.Activity, _fleet.ActivityDuration, _fleet.ActivityStart, _fleet.Order.OrderName);

                    if (_fleet.Order.PercentComplete == null || _fleet.Order.PercentComplete < 1)
                    {
                        BuildStation(_fleet, _allConstructFleetsHere);
                        _fleet.Route.Clear();
                    }
                }
            }

            //ShipType.Construction
            if (bool_bestSector)
            {
                if (_bestSectorForStation.Location == _fleet.Sector.Location)
                {
                    _fleet.Activity = UnitActivity.BuildStation;
                    if (_fleet.Activity == UnitActivity.BuildStation/* && _fleet.Activity == UnitActivity.BuildStation*/)
                    {
                        _text += " - Order to build a station...";
                        if (_writeDirectly_Fleets) Console.WriteLine(_text);
                        _fleet_Text += _newline + _text;

                        if (_fleet.Order.PercentComplete == null || _fleet.Order.PercentComplete < 1)
                        {
                            BuildStation(_fleet, _allConstructFleetsHere);
                        }

                        goto EndofConstructionShip;
                    }
                }

                if (_bestSectorForStation != _fleet.Sector && _fleet.Order != FleetOrders.BuildStationOrder) // best _location can change over time
                {
                    _fleet.Activity = UnitActivity.Mission;
                    //if (_fleet.AITypeUnit != AITypeUnit.Constructor 
                    //if (_fleet.Activity != UnitActivity.Hold
                    //&& _fleet.Activity != UnitActivity.BuildStation) // have a place to go and not already trying to move or build then go
                    //{
                    //GetFleetOwner(_fleet);

                    // no more escorts !!
                    //if (!_fleet.Ships.Any(s => s.ShipType >= ShipType.FastAttack) && _civM.Assault_TargetCiv == null)
                    //{
                    //    //_text += "Step_6888:; LATER > bring back > GetFleetEscort";
                    //    //if (_writeDirectly_Colony) Console.WriteLine(_text);
                    //    GetFleetEscort(_fleet, _bestSectorForStation); // escorts added to _fleet at home conlony 
                    //}


                    _fleet.Owner = _civ;
                    _fleet.OwnerID = _civ.CivID;
                    //_fleet.Route.Clear();

                    _fleet.AITypeUnit = UnitAIType.Constructor;
                    //_fleet.Activity = UnitActivity.Mission;  // double
                    _fleet.Order = FleetOrders.TravelOrder;
                    _fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, DeathStars, new List<Sector> { _bestSectorForStation }));

                    _text = "Step_7666:; " + CreateUpdateFleetText(_fleet, out _fleetText)
                        + " > ordering to bool_bestSector at " + GameEngine.LocationString(_bestSectorForStation.Location.ToString())
                        + " named " + _bestSectorForStation.Name
                        ;
                    //if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    _fleet_Text += _newline + _text;

                    //_text = "Step_7667:; " + CreateUpdateFleetText(_fleet, out _fleets_Summary)
                    //    + " > ordering to bool_bestSector > First _ship=  " + CreateShipText(_fleet.Ships[0], out _shipText)
                    //    + " named " + _bestSectorForStation.Name
                    //    ;
                    //if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    //_fleet_Text += _newline + _text;

                    //GameLog.Core.AI.DebugFormat("Ordering a constructor _fleet {0} to {1}, {2}", _fleet.Owner.Name, _bestSectorForStation.Location, _bestSectorForStation.Name);
                    //GameLog.Core.AI.DebugFormat("Ordering a constructor first _ship Name {0} Design {1}", _fleet.Ships[0].Name, _fleet.Ships[0].DesignName);

                    int i = 0;
                    foreach (var item in _fleet.Ships)
                    {
                        _text = "Step_7668:; " + CreateShipText(item, out _shipText)
                                //+ CreateUpdateFleetText(_fleet, out _fleets_Summary)
                                + " > ordering to bool_bestSector > Ship " + i
                                //+ " named " + _bestSectorForStation.Name
                                ;
                        //if (_writeDirectly_Fleets) Console.WriteLine(_text);
                        //_fleet_Text += _newline + _text;

                        i++;
                    }
                    //GameLog.Core.AI.DebugFormat("Ordering a constructor second _ship Name {0} Design {1}", _fleet.Ships[1].Name, _fleet.Ships[1].DesignName);
                    //}
                }

                if (_fleet.Route.IsEmpty && _fleet.Activity != UnitActivity.BuildStation && _fleet.Activity != UnitActivity.Hold)
                {
                    GameLog.Core.AI.DebugFormat("Empty Route constructor _fleet _ship 1 Name {0} Design {1}", _fleet.Ships[0].Name, _fleet.Ships[0].DesignName);
                    _fleet.Owner = _civ;
                    _fleet.OwnerID = _civ.CivID;
                    //_fleet.Route.Clear();

                    _fleet.AITypeUnit = UnitAIType.Constructor;
                    _fleet.Activity = UnitActivity.Mission;
                    _fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, DeathStars, new List<Sector> { _bestSectorForStation }));

                }
                //}


                // if you were on hold helping build but now there is a station then look to move
                if (_fleet.Activity == UnitActivity.Hold && _fleet.Sector.Station != null)
                {
                    //GetFleetOwner(_fleet);
                    _fleet.Owner = _civ;
                    //_fleet.Route.Clear();
                    _fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, DeathStars, new List<Sector> { _bestSectorForStation }));
                    _fleet.AITypeUnit = UnitAIType.Constructor;
                    _fleet.Activity = UnitActivity.Mission;
                }
            }


            if (_fleet.AITypeUnit == UnitAIType.Constructor && _fleet.Ships.Count() == 1)  // combat ships set to constuctor AI but lost their constructor _ship 
            {
                RemoveEscortShips(_fleet, ShipType.Construction);
                //continue;
            }

            if (_fleet.Sector == _civM.HomeSystem.Sector && _civM.Z_Ship_Construction_Available > 4) // and nothing to 
            {
                DestroyFleet(_fleet);
            }

        EndofConstructionShip:;
            //}
            //End of ShipType.Construction
        }

        //private static void Travel_to_Sector(Fleet _fleet, Sector _bestSectorForStation)
        //{
        //    //_fleet.Order = FleetOrders.TravelOrder;
        //    _fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, DeathStars, new List<Sector> { _bestSectorForStation }));

        //    string _text = "Step_7669:; " + CreateUpdateFleetText(_fleet, out string _fleetText)
        //        + " > ordering to bool_bestSector at " + GameEngine.LocationString(_bestSectorForStation.Location.ToString())
        //        + " named " + _bestSectorForStation.Name
        //        ;
        //    Console.WriteLine(_text);
        //}

        private static void DoScout(Fleet _fleet)
        {
            _fleet.AITypeUnit = UnitAIType.Explorer;
            _fleet.Activity = UnitActivity.Mission;
            _fleet.SetOrder(new ExploreOrder());// explore is really set in FleetOrders OnTurnBegining()

            _fleet.GetCivM(_fleet, out CivilizationManager _civM);
            //string _newline = Environment.NewLine;

            //CreateUpdateFleetText(_fleet, out string _fleetText);
            // Scout
            //if (_fleet.IsScout) //(_fleet.Ships.Where(o => o.ShipType == ShipType.Scout).Any())
            //{
            string _text = "Step_6220:; " + CreateUpdateFleetText(_fleet, out string _fleetText) + " > _fleet.IsScout > EXPLORE  ";
            if (_writeDirectly_Fleets) Console.WriteLine(_text);
            _fleet_Text += Environment.NewLine + _text;

            //checkScoutShips;
            //_checkShips_Scout = true;
            //if (_checkFleetOrders == true && _checkOnlyPlayersUnits && _checkShips_Scout)
            if (_fleet.Owner.IsHuman)
            {
                // ExploreOrder is done in FleetOrders.cs > Explore > OnTurnBegin

                //Debugger.Break();


                // But: 
                // > GetBestSectorTo_Explore is done is this file

            }

            StarSystem _fleetSystem = GameContext.Current.CivilizationManagers[_fleet.OwnerID].HomeSystem;
            StarSystem homeSystem = GameContext.Current.CivilizationManagers[_fleet.OwnerID].HomeSystem;
            StarSystem bestSystem;

            if (UnitAI.GetBestSectorTo_Explore(_fleet, out Sector bestSector))
            {
                _fleet.SetRouteInternal(AStar.FindPath(_fleet, PathOptions.SafeTerritory, null, new List<Sector> { bestSector }));
                _fleet.AITypeUnit = UnitAIType.Explorer;
                _fleet.Activity = UnitActivity.Mission;
            }


            //if (UnitAI.GetBestSystemFor_Science(_fleet, out bestSystem))
            //{
            //    if (bestSystem.Location != _fleetSystem.Location)
            //    {
            //        _fleet.SetRouteInternal(AStar.FindPath(_fleet, PathOptions.SafeTerritory, null, new List<Sector> { bestSystem.Sector }));
            //        _fleet.AITypeUnit = UnitAIType.Explorer;
            //        _fleet.Activity = UnitActivity.Mission;
            //    }
            //}

            //this crashes on OnTurnBeginning

            //if (!_fleet.Route.IsEmpty && _fleet.Route.Waypoints.LastOrDefault().X == homeSystem.Location.X)
            //{
            //    if (UnitAI.GetBestSectorTo_Explore(_fleet, out Sector bestSector))
            //    {
            //        _fleet.SetRouteInternal(AStar.FindPath(_fleet, PathOptions.SafeTerritory, null, new List<Sector> { bestSector }));
            //        _fleet.AITypeUnit = UnitAIType.Explorer;
            //        _fleet.Activity = UnitActivity.Mission;
            //    }
            //}

            if (_fleet.Route.IsEmpty && (_fleet.AITypeUnit != UnitAIType.SystemAttack || _fleet.AITypeUnit != UnitAIType.Reserve))
            {
                if (UnitAI.GetBestSectorTo_Explore(_fleet, out bestSector))
                {
                    _fleet.SetRouteInternal(AStar.FindPath(_fleet, PathOptions.SafeTerritory, null, new List<Sector> { bestSector }));
                    _fleet.AITypeUnit = UnitAIType.Explorer;
                    _fleet.Activity = UnitActivity.Mission;
                }
                else
                {
                    //StarSystem homeSystem = GameContext.Current.CivilizationManagers[_fleet.OwnerID].HomeSystem;                    
                    //StarSystem _fleetSystem = GameContext.Current.CivilizationManagers[_fleet.OwnerID].HomeSystem;  


                    // fly somewhere
                    if (_fleet.Sector.System != null)
                    {
                        _fleetSystem = _fleet.Sector.System;
                    }

                    if (UnitAI.GetBestSystemFor_Science(_fleet, out bestSystem))
                    {
                        if (bestSystem.Location != _fleetSystem.Location)
                        {
                            //Sector _bestSystemSector = new Sector();
                            _fleet.SetRouteInternal(AStar.FindPath(_fleet, PathOptions.SafeTerritory, null, new List<Sector> { bestSystem.Sector }));
                            _fleet.AITypeUnit = UnitAIType.Explorer;
                            _fleet.Activity = UnitActivity.Mission;
                        }
                        else
                        {
                            // 'vacation at home' 
                            _fleet.SetRouteInternal(AStar.FindPath(_fleet, PathOptions.SafeTerritory, null, new List<Sector> { homeSystem.Sector }));
                            _fleet.AITypeUnit = UnitAIType.Explorer;
                            _fleet.Activity = UnitActivity.Mission;
                        }
                    }
                }
            }
            _text = "Step_5554:; " + UnitAI.CreateUpdateFleetText(_fleet, out _fleetText);
            Console.WriteLine(_text);


            //Print_Ships_of_Fleet(_fleet);

            //foreach (var _ship in _fleet.Ships)
            //{
            //    _c += 1;
            //            //_ship.ShipType. = ShipType.Spy;
            //    _text = "Step_6229:; " + CreateShipText(_ship, _shipText: out string shiptext) + " > _fleet.IsScout > EXPLORE > " + _c + " of " + _fleet.Ships.Count;
            //    if (_writeDirectly_Fleets)
            //        Console.WriteLine(_text);
            //    _fleet_Text += _newline + _text;
            //}
            //}
            //if (_writeDirectly_Fleets) Console.WriteLine(_text);

            //checkScoutShips = true;  // after 
            //if (_checkFleetOrders == true && _checkOnlyPlayersUnits && _checkShips_Scout)
            //{
            //    Debugger.Break();
            //}

            if (_fleet.Sector == _civM.HomeSystem.Sector && _civM.Z_Ship_Scout_Available > 3) // and nothing to 
            {
                DestroyFleet(_fleet);
            }


        } // End of DoScout(Fleet _fleet)


        private static void DoTransportShip(Fleet _fleet)
        {
            _fleet.AITypeUnit = UnitAIType.Transport;
            string _newline = Environment.NewLine;
            CreateUpdateFleetText(_fleet, out string _fleetText);
            // Scout
            //if (_fleet.IsScout) //(_fleet.Ships.Where(o => o.ShipType == ShipType.Scout).Any())
            //{
            string _text = "Step_6228:; " + _fleetText + " > _fleet.IsTransport >  ";
            if (_writeDirectly_Fleets) Console.WriteLine(_text);
            _fleet_Text += _newline + _text;



            //CivilizationManager _civM = GameContext.Current.CivilizationManagers[_fleet.Owner.CivID];
            _fleet.GetCivM(_fleet, out CivilizationManager _civM);
            Civilization _targetCiv = _civM.Assault_TargetCiv;

            //checkTransportShips = true;
            //if (_checkFleetOrders == true && _checkOnlyPlayersUnits && _checkShips_Transport)
            if (_fleet.Owner.IsHuman)
            {
                //Debugger.Break();
                //_text += ""; // dummy
            }

            if (_targetCiv == null)
            {
                DoAccumulate(_fleet);
                // explore is really set in FleetOrders OnTurnBegining()



                //_fleet.SetOrder(new ExploreOrder());
            }
            else
            {
                //if (_civM.SystemAssault_Accumulate_Sector_2 != null && _civM.SystemAssault_Accumulate_Sector_2.Location.ToString() != "(0, 0)")
                //{
                //    DoAccumulateAt(_fleet, _civM.SystemAssault_Accumulate_Sector_2.Location);
                //}
                //else
                //{
                    //_text = "Step_6229:; " + _fleetText + " > _fleet.IsTransport >  ";
                    //if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    //_fleet_Text += _newline + _text;
                    if (_civM.SystemAssault_Accumulate_Sector_1 != null && _civM.SystemAssault_Accumulate_Sector_1.Location.ToString() != "(0, 0)")
                    {
                        DoAccumulateAt(_fleet, _civM.SystemAssault_Accumulate_Sector_1.Location);

                    }

                //}

                //if (_civM.SystemAssault_Accumulate_Sector_1 == null || _civM.SystemAssaultSector_2.Location.ToString() != "(0, 0)"
                //    && _civM.SystemAssaultSector_2 == null || _civM.SystemAssaultSector_2.Location.ToString() != "(0, 0)")
                //    DoAccumulate(_fleet);

                _text = "Step_6229:; " + _fleetText + " > new _location set to go ?";
                if (_writeDirectly_Fleets) Console.WriteLine(_text);
                _fleet_Text += _newline + _text;

            }


            // explore is really set in FleetOrders OnTurnBegining()

            //_fleet.SetOrder(new ExploreOrder());


            //_fleet.Activity = UnitActivity.NoActivity;


            //Print_Ships_of_Fleet(_fleet);

            //foreach (var _ship in _fleet.Ships)
            //{
            //    _c += 1;
            //    _text = "Step_6225:; " + CreateShipText(_ship, _shipText: out string shiptext) + " > _fleet.IsTransport > " + _c + " of " + _fleet.Ships.Count;
            //    if (_writeDirectly_Fleets)
            //        Console.WriteLine(_text);
            //    _fleet_Text += _newline + _text;
            //}
            ////}
            //if (_writeDirectly_Fleets) Console.WriteLine(_text);

            //_checkShips_Transport = true;
            //if (_checkFleetOrders == true && _checkOnlyPlayersUnits && _checkShips_Transport)
            if (_fleet.Owner.IsHuman)
            {
                //Debugger.Break();
            }


        }

        private static void DoColonyShip(Fleet _fleet)
        {
            _fleet.AITypeUnit = UnitAIType.Colonizer;

            string _newline = Environment.NewLine;
            //CreateUpdateFleetText(_fleet, out string _fleetText);
            //if (_fleet.IsColonizer)// || _fleet.AITypeUnit == AITypeUnit.Colonizer)
            //{


            //_text = "Step_6301:; " + _fleetText + " > _fleet.IsColonizer ";
            //if (_writeDirectly_Fleets) Console.WriteLine(_text);
            //_fleet_Text += _newline + _text;

            bool _colonizeable = SystemIsColonizeable(_fleet);

            string _text = /*_newline + */"Step_6420:; " + CreateUpdateFleetText(_fleet, out string _fleetText) + " > _fleet.IsColonizer "
                + " > SystemIsColonizeable= " + _colonizeable
            //+ ", SystemIsAlreadyTaken= " + SystemIsAlreadyTaken(_fleet)
            ;
            //if (_writeDirectly_Fleets) Console.WriteLine(_text);
            _fleet_Text += _newline + _text;

            //checkColonyShips = true;
            //_checkShips_Colony = true;
            //if (_checkFleetOrders == true && _checkOnlyPlayersUnits && _checkShips_Colony)
            if (_fleet.Owner.IsHuman)
            {
                Debugger.Break();
            }


            if (_colonizeable)// && !SystemIsAlreadyTaken(_fleet))
            {
                if (_fleet.Ships.Any(a => a.ShipType == ShipType.Colony))
                {
                    _text = "Step_6440:; " + _fleetText + " > AnyColonyShip > ok = has Colony _ship";
                    if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    //_fleet_Text += _newline + _text;

                    //_fleet.Route.Clear();
                    _fleet.SetOrder(new ColonizeOrder());

                    _text = "Step_6452:; " + _fleetText + " > ColonizeOrder !";
                    if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    _fleet_Text += _newline + _text;

                    if (!_fleet.Ships.Any(x => x.ShipType == ShipType.Colony))
                    {
                        RemoveEscortShips(_fleet, ShipType.Colony);
                    }

                    goto Colonizer_is_done;
                }
                else
                {
                    RemoveEscortShips(_fleet, ShipType.Colony);

                    _text = "Step_6454:; " + _fleetText + " > RemoveEscortColonize !";
                    if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    _fleet_Text += _newline + _text;

                    Debugger.Break();
                }

            }

            //if (_fleet.Route.IsEmpty && _fleet.Sector.System != null)
            //{
            //    if (_fleet.Sector.System.IsInhabited)
            //    {
            //        _fleet.Activity = UnitActivity.NoActivity;
            //    }
            //    //else  // not tested
            //    //{
            //    //    _fleet.Activity = UnitActivity.AimReached; // does not help, I assume !
            //    //}
            //}



            //if ((SystemIsColonizeable(_fleet) && SystemIsAlreadyTaken(_fleet))
            //if (_fleet.Route.IsEmpty || _fleet.Route == null)
            //{
            //    _text = "Step_6460:; " + _fleets_Summary+ " > no colonizing possible here..." ;
            //    if (_writeDirectly_Fleets) Console.WriteLine(_text);
            //    _fleet_Text += _newline + _text;

            if (GetBestSystemTo_Colonize(_fleet, out StarSystem bestSystemToColonize) && bestSystemToColonize != null)
            {
                //GetFleetOwner(_fleet);
                //_fleet.Owner = _civM;

                // 2025-01-12: PathOptions.IgnoreDanger instead SafeTerritory
                _fleet.AITypeUnit = UnitAIType.Colonizer;
                _fleet.SetOrder(new ColonizeOrder());
                _fleet.Order = FleetOrders.MissionOrder;
                _fleet.Activity = UnitActivity.Mission;
                _fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.IgnoreDanger, DeathStars, new List<Sector> { bestSystemToColonize.Sector }));
                //    _text = "Step_6427:; " + CreateUpdateFleetText(_fleet, out _fleetText) + " > is new ordered "
                //;
                //    if (_writeDirectly_Fleets) Console.WriteLine(_text);
                //    _fleet_Text += _newline + _text;
            }
            else // goto HomeSystem
            {
                //if (_fleet.Ships.Count > 1)
                //{
                //    RemoveEscortShips(_fleet, ShipType.Colony);
                //}

                //CivilizationManager _civM = GameContext.Current.CivilizationManagers[_fleet.Owner.CivID];
                _fleet.GetCivM(_fleet, out CivilizationManager _civM);

                if (_fleet.Route.IsEmpty || _fleet.Route == null)
                {



                    //CivilizationManager _civM = _fleet.Owner.AccumulateSector;
                    //Sector _accumulate_Sector = _civM.HomeSystem.Sector;
                    //GetCivM(_fleet).AccumulateSector;
                    //_civM = _fleet.GetCiv(_fleet);
                    _fleet.SetOrder(new AccumulateLocation_Go_There_Order());
                    _fleet.Activity = UnitActivity.GoToAccumulateSector;
                    _fleet.Order = FleetOrders.TravelOrder;
                    _fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, DeathStars, new List<Sector> { _civM.HomeSystem.Sector }));

                    //_fleet.AITypeUnit = AITypeUnit.NoUnitAI;
                    //continue;
                    _text = "Step_6428:; " + CreateUpdateFleetText(_fleet, out _fleetText) + " > is new ordered "
                                ;
                    if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    _fleet_Text += _newline + _text;
                }

                if (_fleet.Sector == _civM.HomeSystem.Sector && _civM.Z_Ship_Colony_Available > 1) // and nothing to 
                {
                    DestroyFleet(_fleet);
                }
                //        _text = "Step_6429:; " + CreateUpdateFleetText(_fleet, out _fleetText) + " > will be destroyed "
                //;
                //        if (_writeDirectly_Fleets) Console.WriteLine(_text);
                //        _fleet_Text += _newline + _text;


                //        _text = GameEngine.LocationString(_fleet.Location.ToString())
                //            + " > Fleet " + _fleet.ObjectID
                //            + ": * " + _fleet.Name
                //            + " * ( " + _fleet.ClassName
                //            + " ) > was destroyed to reduce abundance"
                //            ;
                //        _ = GameContext.Current.Universe.Destroy(_fleet.Ships.LastOrDefault());
                //        _civM.SitRepEntries.Add(new ReportEntry_CoS(_civM.Civilization, _civM.HomeSystem.Location, _text, _text, "", SitRepPriority.Red));


            }
        //}


        ////Colonizer - No Activity and Route is empty > search a new BestSystemToColonize
        //if (/*_fleet.Sector == _ourHomeSystem.Sector && */(_fleet.Activity == UnitActivity.NoActivity || _fleet.Route.IsEmpty/*|| _fleet.Order.IsComplete*/)) // || _fleet.Activity == UnitActivity.Mission
        //{
        //    if (GetBestSystemTo_Colonize(_fleet, out StarSystem bestSystemToColonize))
        //    {
        //        //Head to the system  
        //        //_fleet.Owner = _civM;
        //        //_fleet.SetRoute(AStar.FindPath(_fleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { bestSystemToColonize.Sector }));
        //        if (bestSystemToColonize == null)
        //        {
        //            _text = "Step_6320:; " + _fleets_Summary
        //                    + " > found no bestSystemToColonize "
        //                   ;
        //            if (_writeDirectly_Fleets) Console.WriteLine(_text);
        //            _fleet_Text += _newline + _text;
        //        }
        //        else
        //        {
        //            _fleet.AITypeUnit = AITypeUnit.Colonizer;
        //            _fleet.Activity = UnitActivity.Mission;
        //            if (!_fleet.Ships.Any(s => s.ShipType >= ShipType.FastAttack) /*&& _civM.Assault_TargetCiv == null*/)
        //            {
        //                //_text += "Step_6889:; LATER > bring back > GetFleetEscort";
        //                //if (_writeDirectly_Colony) Console.WriteLine(_text);
        //                GetFleetEscort(_fleet, bestSystemToColonize.Sector);
        //            }
        //            _text = "Step_6321:; " + _fleets_Summary
        //                + " > Ordered for Colonizing > "

        //                + " to go to " + bestSystemToColonize.Name
        //                + " " + bestSystemToColonize.Location

        //                ;
        //            if (_writeDirectly_Fleets) Console.WriteLine(_text);
        //            _fleet_Text += _newline + _text;
        //            // GameLog.Core.AI.DebugFormat("Ordering {0} colonizer {1} to go to {2} {3}", _fleet.Owner, _fleet.Name, bestSystemToColonize.Name, bestSystemToColonize.Location);
        //        }
        //    }
        //    //else if (_fleet.Ships.Where(s => s.ShipType >= ShipType.FastAttack).Any())
        //    //{
        //    //    RemoveEscortShips(_fleet, ShipType.Colony);
        //    //}

        //}

        //if (_fleet.Sector != _ourHomeSystem.Sector) // only colonize when not at homesystem 
        //{
        //_text = "Step_6410:; " + _fleets_Summary + ": Colonizer on the road:";
        //if (_writeDirectly_Colony) Console.WriteLine(_text);

        //if (_fleet.Sector != bes)

        Colonizer_is_done:;
            //}
            //}
        } // End of Colonizer

        private static void DestroyFleet(Fleet _fleet)
        {
            string _text = "Step_6429:; " + CreateUpdateFleetText(_fleet, out string _fleetText) + " > will be destroyed "
;
            if (_writeDirectly_Fleets) Console.WriteLine(_text);
            //_fleet_Text += _newline + _text;


            _text = GameEngine.LocationString(_fleet.Location.ToString())
                + " > Fleet " + _fleet.ObjectID
                + ": * " + _fleet.Name
                + " * ( " + _fleet.ClassName
                + " ) > was destroyed to reduce abundance"
                ;

            _ = GameContext.Current.Universe.Destroy(_fleet.Ships.LastOrDefault());

            _fleet.GetCivM(_fleet, out CivilizationManager _civM);

            _civM.SitRepEntries.Add(new ReportEntry_CoS(_civM.Civilization, _civM.HomeSystem.Location, _text, _text, "", SitRepPriority.Red));
        }

        public static string CreateShipText(Ship ship, out string _shipText)
        {
            // call via > CreateShipText(_ship, out string _shipText);

            if (ship == null || ship.Fleet == null)
            {
                _shipText = " - no _ship or no _fleet";
                return _shipText;
            }


            _shipText = //"Step_3103:; "+ 
                GameEngine.LocationString(ship.Location.ToString())

                + " ; " + GameEngine.Do_5_Digit(ship.ObjectID.ToString()) + " ; SHIP"

                + " ; " + ship.Design
                + " ; " + ship.Name
                + " ;AITypeUnit= " + ship.Fleet.AITypeUnit

                + " ; " + ship.Fleet.Activity
                + " ; " + ship.Fleet.Order
                //+ " ;AITypeUnit= " + _ship.Fleet.AITypeUnit
                //+ " ; " + _ship.Fleet.Activity
                //+ " ; " + _ship.Fleet.Order

                ;
            return _shipText;
        }

        public static string CreateUpdateFleetText(Fleet fleet, out string _fleetText)
        {
            // call via > CreateUpdateFleetText(_fleet, out string _fleets_Summary);

            if (fleet == null)
            {
                _fleetText = " - no _fleet";
                return _fleetText;
            }
            //    

            string _heading_to = "no movement";

            if (fleet.Route != null && fleet.Route.Waypoints.Count < 1)
            {

            }
            else
            {
                _heading_to = "atm heading to " + fleet.Route.Waypoints.Last().ToString();
            }

            _fleetText = //"Step_3103:; "+ 
                GameEngine.LocationString(fleet.Location.ToString())

                + " ; " + GameEngine.Do_5_Digit(fleet.ObjectID.ToString()) + " ;Fleet"
                + " ; " + fleet.Ships[0].Design
                + " ; " + fleet.Name

                + " ; AITypeUnit= " + fleet.AITypeUnit // without " " infront of it
                + " ; " + fleet.Activity
                + " ; " + fleet.Order
                + " ; RouteSteps= " + fleet.Route.Length
                + " ; " + _heading_to

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
                    // GameLog.Client.AI.DebugFormat("A _ship all attack ships {0} location ={1}", _ship.Name, _ship.Location );
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
                    // GameLog.Client.AI.DebugFormat("A _ship all attack ships {0} location ={1}", _ship.Name, _ship.Location );
                }
            }
            if (otherHomeSystem.Sector.Station != null)
            {
                firePower += otherHomeSystem.Sector.Station.Firepower();
            }
            return firePower;
        }

        public static void BuildAndSendFleet(Fleet fleet, Civilization _civ, UnitActivity activity, UnitAIType unitAIType, Sector destination)
        {
            fleet.SetOrder(new AccumulateLocation_Go_There_Order());
            fleet.Activity = activity;
            fleet.AITypeUnit = unitAIType;
            fleet.Owner = _civ;
            fleet.OwnerID = _civ.CivID;
            fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, DeathStars, new List<Sector> { destination }));
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
                GameLog.Client.AI.DebugFormat("The {0} found minor {1} to Invation", attacker.Name, attacked.Name);
            }
            else
            {
                GameLog.Client.AI.DebugFormat("The {0} found Empire {1} for Invation", attacker.Name, attacked.Name);
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
        public static bool GetBestSystemTo_Colonize(Fleet fleet, out StarSystem result)
        {

            //GetFleetOwner(_fleet);
            if (fleet == null)
            {
                Debugger.Break();
                throw new ArgumentNullException(nameof(fleet));
            }
            string _text = "";


            List<Fleet> colonizerFleets = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner)
                .Where(o => o.IsColonizer || o.MultiFleetHasAColonizer).ToList();
            List<Fleet> otherFleets = colonizerFleets.Where(o => o != fleet).ToList(); // other _colony ships

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
            CivilizationMapData mapData = civManager.MapData;

            //Get a list of all systems that we can colonise
            List<StarSystem> systems = GameContext.Current.Universe.Find<StarSystem>()
            //We need to know about it (no cheating)

            // AI: numbers of IsScanned && IsExplored might be low
            // test - .Where(r => mapData.IsScanned(r.Location)) //&& mapData.IsExplored(r.Location))
            .Where(s => !s.IsOwned /*|| s.Owner == _fleet.Owner*/)
            .Where(t => !t.IsInhabited && !t.HasColony)
            .Where(u => u.IsHabitable(fleet.Owner.Race))
            .Where(v => v.StarType != StarType.RadioPulsar && v.StarType != StarType.NeutronStar)
            .Where(w => FleetHelper.IsSectorWithinFuelRange(w.Sector, fleet))

            .Where(x => !otherFleets.Any(f => x.Location == f.Location && f.Order is ColonizeOrder /*|| x.Location == f.Location && f.Order is ColonizeOrder*/))
            .Where(x => !otherFleets.Any(f => f.Route.Waypoints.LastOrDefault() == x.Location || x.Location == f.Location && f.Order is ColonizeOrder)
            //.Where(y => GameContext.Current.Universe.FindAt<Orbital>(y.Location).Any(o => DiplomacyHelper.ArePotentialEnemies(_fleet.Owner, y.Owner))
            )
            //That isn't owned by another civilization
            //That isn't currently inhabited or have a current _colony
            //That's actually inhabitable
            //That are in fuel range
            //That doesn't have potential enemies in it
            //Where a _ship isn't heading there already
            //Where a _ship isn't there and colonizing
            .ToList();

            // find out the reasons for low count of > available systems



            List<StarSystem> systemsToControlList = GameContext.Current.Universe.Find<StarSystem>()
                .Where(r => mapData.IsScanned(r.Location) && mapData.IsExplored(r.Location))
                .Where(c => !c.IsInhabited)
                .Where(w => FleetHelper.IsSectorWithinFuelRange(w.Sector, fleet))
                .ToList();

            int _systemsInhabited = 0;
            string _uninhabitedText = "";
            string _inhabitedText = "";
            foreach (var item in systemsToControlList)
            {
                _systemsInhabited += 1;
                //_text = "Step_6170:; Controll-List of GetBestSystemTo_Colonize:; "
                //    + " for " + _fleet.Location
                //    + " " + _fleet.Name
                //    + " > " + item.IsInhabited + " for Inhabited"
                //    + " - Dist: " + GetDistanceTo(_fleet.Location, item.Location)
                //    + " > " + GameEngine.LocationString(item.Location.ToString())
                //    + " " + item.Name
                //    ;

                //if (_writeDirectly_Fleets) Console.WriteLine(_text);
            }

            _text = "Step_6160:; GetBestSystemTo_Colonize:; Available systems with FuelRange for " + fleet
                + " > " + systemsToControlList.Count
                + ", unhabited= " + " " + _uninhabitedText
                + ", already inhabited= " + _systemsInhabited + " " + _inhabitedText
                ;
            //if (_writeDirectly_Fleets) Console.WriteLine(_text);

            //if (_fleet.Owner.IsHuman) Debugger.Break();


            // systems is the original result
            if (systems.Count == 0)
            {
                _text = "Step_6190:; " + UnitAI.CreateUpdateFleetText(fleet, out _fleet_Text)
                    //+ GameEngine.LocationString(_fleet.Location.ToString())
                    //+ " " + _fleet.Name
                    + " > Found nothing to colonize > systems.Count = 0 "
                    //+ ": " + _fleet.Location + " "

                    //+ " > Going to Order EXPLORE "
                    ;
                if (_writeDirectly_Fleets) Console.WriteLine(_text);

                // 2023-07-15: new >> NOT HERE
                //_fleet.SetOrder(new ExploreOrder());
                //_fleet.Order = FleetOrders.ExploreOrder;

                result = null;
                return false;
            }

            List<StarSystem> enemySystems = new List<StarSystem>() { civManager.HomeSystem };
            //StarSystem placeholder = enemySystems.FirstOrDefault();
            foreach (StarSystem system in systems)
            {
                //works
                _text = "Step_6196:; Colonizing-Aim of " + systems.Count
                    + " > IsInhabited= " + system.IsInhabited
                    + ": " + system.Location
                    + " " + system.Name
                    ;
                //if (_writeDirectly_Fleets) 

                //Console.WriteLine(_text);

                if (system.Owner != null && GameContext.Current.Universe.FindAt<Orbital>(system.Location).Any(o => DiplomacyHelper.ArePotentialEnemies(fleet.Owner, system.Owner)))
                {
                    enemySystems.Add(system);
                }
            }

            foreach (StarSystem removeSystem in enemySystems)
            {
                _text = "Step_6197:; Colonizing-Aim of " + systems.Count
                    + " (REMOVED): " + removeSystem.Location + " "
                    + removeSystem.Name
                    ;
                //if (_writeDirectly_Fleets) Console.WriteLine(_text);

                //_ = systems.Remove(removeSystem);  // enemySystems is first or default that is ONE, why remove - caused crashes as well
            }
            //foreach (var system in systems)
            //{
            //    GameLog.Client.AI.DebugFormat("System ={0}, {1} Colony? ={2} owner ={3}, Habitable? ={4} starType {5} for {6}"
            //        , system.Name, system.Location, system.HasColony, system.Owner, system.IsHabitable(_fleet.Owner.Race), system.StarType, _fleet.Owner);
            //}
            if (systems.Count == 0)
            {
                result = null;
                return false;
            }

            IOrderedEnumerable<StarSystem> sortResults = from system in systems
                                                         orderby GetValue_Colonize(system, fleet)
                                                         select system;

            result = sortResults.Last();
            _text = "Step_6198:; " + CreateUpdateFleetText(fleet, out string _fleetText)
                    + " > GetBestColonyForColonize= >>> " + result.Location
                    + " " + result.Name
                    + ", Owner= " + result.Owner
                    ;
            //if (_writeDirectly_Fleets) Console.WriteLine(_text);
            //GameLog.Client.AI.DebugFormat("Best System for {0}, star ={1}, {2} {3}, value ={4}", _fleet.Owner, result.Name, result.StarType, result.Location, GetValue_Colonize(result, _fleet.Owner));

            //if (_fleet.Owner.IsHuman) Debugger.Break();

            return true;  // returns a true for success and the found system
        }

        public static bool SystemIsColonizeable(Fleet fleet)
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


            //GetFleetOwner(_fleet);
            //_text = "Check for SystemNotAlreadyTaken....";
            //if (_writeDirectly_Fleets) Console.WriteLine(_text);

            bool otherColony = GameContext.Current.Universe.Objects.Where(o => o.Location == fleet.Location && o.ObjectType == UniverseObjectType.Colony && o.Owner != fleet.Owner).Any();
            //_text = "Searching for Crash: SystemNotAlreadyTaken-2=Station";
            //if (_writeDirectly_Fleets) Console.WriteLine(_text);

            bool otherStation = GameContext.Current.Universe.Objects.Where(o => o.Location == fleet.Location && o.ObjectType == UniverseObjectType.Station).Any();
            if (otherColony || otherStation)
            {
                return true;
            }
            //return false;



            // is next working correctly and not disturbing each other
            //List<Fleet> otherFleets = colonizerFleets.Where(o => o != _fleet).ToList(); // other _colony ships
            //_ = otherFleets.Remove(_fleet);
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
        public static float GetValue_Colonize(StarSystem system, Fleet fleet)
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

            value += (int)system.GetMaxPopulation(fleet.Owner.Race) * system.GetGrowthRate(fleet.Owner.Race);

            //works
            string _text = "Step_1238:;"
                     + " _civM= " + fleet.Owner
                     + ", Colonize-Value " + (int)value
                     + " for " + system.Location + " " + system.Name

                    ;
            //if (_writeDirectly_Fleets) Console.WriteLine(_text);
            //GameLog.Core.AI.DebugFormat("Colonize value for {0} is {1} for {2}", system, value, _civM.Name);
            return value;
        }

        /// <summary>
        /// Get combat _ship <see cref="Fleet"/> to protect colonyship
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
                    string _text = "Step_4367:; GetFleetEscort"
                        + "ESCORT= " + aFeet.ObjectID
                        + " " + aFeet.Name
                        + " " + aFeet.ClassName
                        + " " + aFeet.AITypeUnit

                        + ", Act= " + aFeet.Activity
                        + ", AITypeUnit= " + aFeet.AITypeUnit
                        + ", Order= " + aFeet.Order

                        + " > going to= " + finalSector.Location
                        + " " + finalSector.Name
                        + ", steps= " + fleetToFollow.Route.Steps.Count

                        + " escorting " + fleetToFollow.ObjectID
                        + " " + fleetToFollow.Name

                            ;
                    if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    //GameLog.Core.AI.DebugFormat("ESCORT ={0} {1} unitAIType {2} activity {3} _location ={4} for _ship ={5} {6} step count ={7}"
                    //, aFeet.Owner, aFeet.ClassName, aFeet.AITypeUnit, aFeet.Activity, finalSector.Name, fleetToFollow.Owner, fleetToFollow.Name, fleetToFollow.Route.Steps.Count);
                    return;
                }
            }
        }

        private static void RemoveEscortShips(Fleet fleet, ShipType type)
        {
            //int shipCount = _fleet.Ships.Count();
            GetFleetOwner(fleet);
            //List<Ship> listOfShips = new List<Ship>();
            Fleet newFleet = new Fleet();
            //foreach (Ship _ship in _fleet.Ships)
            //{
            //    listOfShips.Add(_ship);
            //}
            //List<Ship> listOfShips = _fleet.Ships.ToList();
            MapLocation location;
            foreach (Ship ship in fleet.Ships)
            {
                if (ship.ShipType.ToString() != type.ToString()) // without ToString it might doesn't work
                {
                    location = ship.Location;
                    fleet.RemoveShip(ship);
                    newFleet.AddShip(ship); // remove escort
                    newFleet.Location = location;
                    //if (location != GameContext.Current.CivilizationManagers[_fleet.OwnerID].HomeSystem.Location)
                    //    newFleet.SetRoute(AStar.FindPath(newFleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { GameContext.Current.CivilizationManagers[_fleet.Owner].HomeSystem.Sector }));

                    string _text = "Step_6422:; " + UnitAI.CreateShipText(ship, out string _shipText) + " > RemoveEscortShips "
            ;
                    //if (_writeDirectly_Fleets) 
                    Console.WriteLine(_text);
                    //_fleet_Text += _newline + _text;
                    //GameLog.Core.AI.DebugFormat("RemoveEscortShips _ship ={0} at {1}", _ship.Name, _ship.Location);

                }
            }

            if (newFleet.Ships.Count > 0)
            {
                newFleet.SetOrder(new IdleOrder());
                newFleet.Owner = fleet.Owner;
                newFleet.OwnerID = fleet.OwnerID;
                newFleet.AITypeUnit = UnitAIType.NoUnitAI;  // no specific one
                newFleet.Activity = UnitActivity.NoActivity;

                var _accumulateSector = GameContext.Current.CivilizationManagers[fleet.Owner].AccumulateSector;

                IEnumerable<Sector> _deathStars = UnitAI.DeathStars;

                if (_accumulateSector != null && newFleet.Location != _accumulateSector.Location && _accumulateSector.Location.ToString() != "(0, 0)")
                {
                    newFleet.SetRoute(AStar.FindPath(newFleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { _accumulateSector }));
                }
                else
                {
                    //old _fleet might be null
                    var _homeSector = GameContext.Current.CivilizationManagers[newFleet.Owner.CivID].HomeSystem.Sector;
                    newFleet.SetRoute(AStar.FindPath(newFleet, PathOptions.SafeTerritory, _deathStars, new List<Sector> { _homeSector }));
                }
            }

            //GameLog.Core.AI.DebugFormat("New Fleet route length {0}, Activity {1} AITypeUnit {2}", _fleet.Route.Length, _fleet.Activity, _fleet.AITypeUnit);
        }


        /// <summary>
        /// Determines how valuable it will be for the given <see cref="Fleet"/>
        /// to explore the given <see cref="Sector"/>
        /// </summary>
        /// <param name="sector"></param>
        /// <param name="fleet"></param>
        /// <returns></returns>
        public static int GetValue_Explore(Sector sector, Civilization civ)
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

            //GameLog.Core.AI.DebugFormat("Explore priority for {0} is {1}", _location, value);
            return value;
        }

        /*
        * Explor best _location
        */

        /// <summary>
        /// Gets the best <see cref="Sector"/> to explore for the given <see cref="Fleet"/>
        /// </summary>
        /// <param name="fleet"></param>
        /// <returns></returns>
        public static bool GetBestSectorTo_Explore(Fleet fleet, out Sector sector)
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
                    GameLog.Client.General.ErrorFormat("_fleet.ObjectId ={0} {1} {2} error ={3}", fleet.ObjectID, fleet.Name, fleet.ClassName, e);
                }
                else { GameLog.Client.General.Error(e); }
            }

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
            CivilizationMapData mapData = civManager.MapData;
            List<StarSystem> starsToExplore = new List<StarSystem>();
            List<Sector> sectorsToExplore = new List<Sector>();
            GetFleetOwner(fleet);

            if (fleet.Owner.IsHuman)
            {
                //Debugger.Break();
            }

            if (fleet.Owner != null)
            {
                starsToExplore = GameContext.Current.Universe.Find<StarSystem>()
                    //We need to know about it (no cheating)
                    .Where(s => mapData.IsScanned(s.Location) && s.Owner != fleet.Owner
                    //&& (!s.IsOwned || (s.Owner != _fleet.Owner))
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
                    //Where we can enter the _location
                    //Where is in fuel range of the _ship
                    //Where no _fleets are already heading there or through there
                    .ToList();
            }

            if (starsToExplore.Count > 0)
            {

                Random rnd = new Random();
                int _randomNumber = rnd.Next(0, starsToExplore.Count - 1);
                starsToExplore.Sort((a, b) =>
                    (GetValue_Explore(a.Sector, fleet.Owner) - HomeSystemDistanceModifier(fleet, a.Sector))
                    .CompareTo(GetValue_Explore(b.Sector, fleet.Owner) - HomeSystemDistanceModifier(fleet, b.Sector)));
                //sector = starsToExplore[starsToExplore.Count() - 1].Sector;  // with this the scout is stuck is the 'best' result
                sector = starsToExplore[_randomNumber].Sector;
                return true;
            }
            else
            {
                sector = civManager.AccumulateSector;
                return true;
            }

            //if (_fleet.Owner != null)(s)
            //{
            //    sectorsToExplore = GameContext.Current.SectorClaims.wh
            //    //sectorsToExplore = GameContext.Current.Universe.Map.Find<_sectors>()
            //        //We need to know about it (no cheating)
            //        .Where(s => s.Owner == _fleet.Owner
            //        && DiplomacyHelper.IsTravelAllowed(_fleet.Owner, s)
            //        && FleetHelper.IsSectorWithinFuelRange(s, _fleet)
            //        && !ownFleets.Any(f => f.Route.Waypoints.Any(wp => s.Location == wp)))
            //        //No point exploring our own space
            //        //Where we can enter the _location
            //        //Where is in fuel range of the _ship
            //        //Where no _fleets are already heading there or through there
            //        .ToList();
            //}

            //sectorsToExplore.Sort((a, b) =>
            //    (GetValue_Explore(a, _fleet.Owner) - HomeSystemDistanceModifier(_fleet, a))
            //    .CompareTo(GetValue_Explore(b, _fleet.Owner) - HomeSystemDistanceModifier(_fleet, b)));
            //_location = starsToExplore[starsToExplore.Count() - 1].Sector;
            //return true;

            // End of GetBestSectorToExplore
        }

        public static Sector GetBestSystemFor_AccumulateFor_SystemAttack(Sector _targetSector, Sector ourHomeSystem, out Sector _sector)
        {
            //2025-01-04

            //GetFleetOwner(_fleet);
            //if (_fleet == null)
            //{
            //    throw new ArgumentNullException(nameof(_fleet));
            //}
            //if (_fleet.Ships.Count() == 0)
            //{
            //    _location = null;
            //    return false;
            //}
            //List<Fleet> ownFleets = new List<Fleet>();
            //try
            //{
            //    ownFleets = GameContext.Current.Universe.FindOwned<Fleet>(_fleet.Owner).Where(f => f.CanMove && f != _fleet && !f.Route.IsEmpty).ToList();
            //}
            //catch (Exception e)
            //{
            //    if (_fleet != null)
            //    {
            //        GameLog.Client.General.ErrorFormat("_fleet.ObjectId ={0} {1} {2} error ={3}", _fleet.ObjectID, _fleet.Name, _fleet.ClassName, e);
            //    }
            //    else { GameLog.Client.General.Error(e); }
            //}

            CivilizationManager civM = GameContext.Current.CivilizationManagers[ourHomeSystem.Owner.CivID];
            //CivilizationMapData mapData = _civM.MapData;
            //List<StarSystem> starsToExplore = new List<StarSystem>();
            //List<Sector> sectorsToExplore = new List<Sector>();
            //GetFleetOwner(_fleet);
            List<Sector> _availableSectors = new List<Sector>();
            List<Sector> _maybeSectors = new List<Sector>();
            string _newline = Environment.NewLine;
            bool _write = true;
            _sector = null;
            string _text;

            int _distance = 2;

        _SearchAvailableSectorsOnceAgain:
            _availableSectors = MapHelper.GetSectorsWithinRadius(_targetSector, _distance).ToList();

            //if (_checkFleetOrders == true && _checkOnlyPlayersUnits)
            //{
            //Debugger.Break();
            //}

            foreach (var _sec in _availableSectors)
            {

                int _distance2 = MapLocation.GetDistance(_sec.Location, ourHomeSystem.Location);
                _text = "Step_6923:; "
                     + "" + _fleets_Summary
                     + " >  DistfromHome > " + GameEngine.Do_x2_Digit_String(_distance2.ToString())
                     + ", Value for Sector= " + GetValue_Sector(_sec.Location, ourHomeSystem.Owner, -1500)
                     + " for " + _sec.Location
                     + " " + _sec.Name

                    ;
                //if (_write) Console.WriteLine(_text);
                //_fleet_Text += _newline + _text;

                if (_sec.System != null)
                {
                    if (_sec.System.StarType == StarType.BlackHole
                        || _sec.System.StarType == StarType.NeutronStar
                        || _sec.System.StarType == StarType.XRayPulsar
                        || _sec.System.StarType == StarType.RadioPulsar
                        || _sec.System.StarType == StarType.Quasar
                        || _sec.Station != null
                        )
                        _text += ""; // dummy
                }
                else
                {
                    _maybeSectors.Add(_sec);
                }

            }



            //_availableSectors = MapHelper.GetSectorsWithinRadius(_fleet.Sector, _distance).ToList();

            if (_maybeSectors.Count < 1)
            {
                _distance += 1;
                goto _SearchAvailableSectorsOnceAgain;
            }


            try
            {
                //Sector s1 = GetSectorsWithinRadius
                _maybeSectors.Sort((a, b) =>
                              GetValue_Sector(a.Location, ourHomeSystem.Owner, -1500)
                              .CompareTo(GetValue_Sector(b.Location, ourHomeSystem.Owner, -1500)));
                _sector = _maybeSectors.LastOrDefault();
            }
            catch { _sector = null; return null; }

            //if ()
            civM.SystemAssault_Accumulate_Sector_1 = _sector;
            civM.SystemAssault_Accumulate_Location_1 = _sector.Location;

            _text = "Step_6926:; "
                     + "" + civM.Civilization
                     + " > Accumulate*Location*= " + civM.SystemAssault_Accumulate_Location_1
                     + " + AccumulateSector > " + civM.SystemAssault_Accumulate_Sector_1

                    //+ " for " + _sec.Location
                    //+ " " + _sec.Name

                    ;
            if (_write) Console.WriteLine(_text);
            _fleet_Text += _newline + _text;


            return _sector;

        }// End of GetBestSystemFor_AccumulateFor_SystemAttack


        public static int GetValue_Sector(MapLocation _loc, Civilization civ, int DistanceFactor) // Fleet _fleet, List<UniverseObject> universeObjects)
        {
            string _newline = Environment.NewLine;
            string _text = "";
            //GameLog.Client.AI.DebugFormat("GetValue_Station");
            //GetFleetOwner(_fleet);
            //GameLog.Client.AI.DebugFormat("GetFleetOwner");
            if (_loc == null)
            {
                return -9999;
                //GameLog.Client.AI.DebugFormat("null");
                //throw new ArgumentNullException(nameof(_location));
            }
            Sector sector = new Sector(_loc);

            if (sector.Station != null && sector.Station.Owner != civ)
            {
                return -5000;
            }

            if (sector.System != null
                    && (sector.System.StarType == StarType.BlackHole
                    || sector.System.StarType == StarType.NeutronStar
                    || sector.System.StarType == StarType.RadioPulsar
                    || sector.System.StarType == StarType.XRayPulsar)
                || (sector.Owner != null && sector.Owner.CivID <= 6))
            //|| _location.GetNeighbors().Where(o => o.Owner != null && o.Owner.CivID <= 6).Any())   
            {
                return -4000;
                //GameLog.Client.AI.DebugFormat("A");
            }

            int value = 1;
            //_text = "Searching for Crash: someObject";
            //if (_writeDirectly_Colony) Console.WriteLine(_text);

            //IEnumerable<UniverseObject> someObject = universeObjects.Where(o => o.Sector == _location);
            if (sector != null)
            {
                const int SystemSectorValue = 0;
                //const int StrandedShipSectorValue = 50000;
                //const int PastFuelRange = 30000;
                //const int DistanceFactor = 1500; // was 100

                //if (!FleetHelper.IsSectorWithinFuelRange(_location, _fleet))
                //{
                //    value += PastFuelRange;
                //    // GameLog.Client.AI.DebugFormat("B");
                //}

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
                        //GameLog.Client.AI.DebugFormat("C");
                    }
                }
                //Sector homeSector = GameContext.Current.CivilizationManagers[_fleet.OwnerID].HomeSystem.Sector;
                //Sector homeSector = GameContext.Current.Universe.HomeColonyLookup[_fleet.Owner].Sector; // crashes e.g. for Borg

                try
                {
                    int distance = MapLocation.GetDistance(sector.Location, GameContext.Current.CivilizationManagers[civ.CivID].HomeSystem.Location);

                    value += DistanceFactor * distance;
                }
                catch
                {
                    _text = "Step_6922:; " + _fleets_Summary + " > ### unable to get furthest object from home world for station value"
                            //+ " " + _fleet.ObjectID
                            //+ " " + _fleet.Name
                            ////+ " to go to " + bestSystemToColonize.Name
                            //+ " " + _fleet.Location
                            ;
                    if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    _fleet_Text += _newline + _text;
                    return -9998;
                    //GameLog.Client.AI.DebugFormat("unable to get furthest object from home world for station value");
                }

                //List<Sector> strandedShipSectors = FindStrandedShipSectors(_fleet.Owner); //altering collection while sorting it!!!!!!!!!!!!!!
                //if (strandedShipSectors.Count > 0)
                //{
                //    if (strandedShipSectors.Contains(_location))
                //    {
                //        value += StrandedShipSectorValue;
                //        //GameLog.Client.AI.DebugFormat("D");
                //    }
                //}
            }
            //GameLog.Core.AI.DebugFormat("Station at {0} has value {1}", _location.Location, (value + randomInt));

            //_text = "Step_6914:; " + _fleets_Summary + " > "
            //        //+ " " + _fleet.ObjectID
            //        //+ " " + _fleet.Name
            //        ////+ " to go to " + bestSystemToColonize.Name
            //        //+ " " + _fleet.Location
            //        ;
            //if (_writeDirectly_Colony) Console.WriteLine(_text);
            //_fleet_Text += _newline + _text;

            return value; // + randomInt;
        }

        /// <summary>
        /// Determines how valuable a <see cref="Station"/> would be
        /// in a given <see cref="Sector"/>
        /// </summary>
        /// <param name="_sector"></param>
        /// <param name="_fleet"></param>
        /// <returns></returns>
        public static int GetValue_Station(Sector _sector, Fleet _fleet, List<UniverseObject> universeObjects)
        {
            //GameLog.Client.AI.DebugFormat("GetValue_Station");
            GetFleetOwner(_fleet);
            string _newline = Environment.NewLine;
            string _text = "";
            //GameLog.Client.AI.DebugFormat("GetFleetOwner");
            if (_sector == null)
            {
                //GameLog.Client.AI.DebugFormat("null");
                throw new ArgumentNullException(nameof(_sector));
            }
            if (_sector.Station != null
                || _sector.GetFleets().Where(o => o.AITypeUnit == UnitAIType.Constructor) != null && _sector.GetFleets().Any(o => o.Activity == UnitActivity.BuildStation)
                || _sector.System != null
                    && (_sector.System.StarType == StarType.BlackHole
                    || _sector.System.StarType == StarType.NeutronStar
                    || _sector.System.StarType == StarType.RadioPulsar
                    || _sector.System.StarType == StarType.XRayPulsar)
                || (_sector.Owner != null && _sector.Owner.CivID <= 6))
            //|| _location.GetNeighbors().Where(o => o.Owner != null && o.Owner.CivID <= 6).Any())   
            {
                return -4000;
                //GameLog.Client.AI.DebugFormat("A");
            }

            int value = 1;
            //_text = "Searching for Crash: someObject";
            //if (_writeDirectly_Colony) Console.WriteLine(_text);

            IEnumerable<UniverseObject> someObject = universeObjects.Where(o => o.Sector == _sector);
            if (someObject != null)
            {
                const int SystemSectorValue = 2500;
                const int StrandedShipSectorValue = 50000;
                const int PastFuelRange = 30000;
                const int DistanceFactor = 3000; // was 100

                if (FleetHelper.IsSectorWithinFuelRange(_sector, _fleet))
                {
                    value += PastFuelRange;
                    // GameLog.Client.AI.DebugFormat("B");
                }

                if ((_sector.System != null)
                    && (_sector.System.Owner == null || _sector.System.OwnerID > 6))
                {
                    value += SystemSectorValue;
                    if (_sector.System.StarType == StarType.Blue
                        || _sector.System.StarType == StarType.Orange
                        || _sector.System.StarType == StarType.Red
                        || _sector.System.StarType == StarType.White
                        || _sector.System.StarType == StarType.Yellow
                        || _sector.System.StarType == StarType.Wormhole)
                    {
                        value += SystemSectorValue;
                        //GameLog.Client.AI.DebugFormat("C");
                    }
                }
                Sector homeSector = GameContext.Current.CivilizationManagers[_fleet.OwnerID].HomeSystem.Sector;
                //Sector homeSector = GameContext.Current.Universe.HomeColonyLookup[_fleet.Owner].Sector; // crashes e.g. for Borg

                try
                {
                    int distance = MapLocation.GetDistance(_sector.Location, homeSector.Location);

                    if (GameContext.Current.TurnNumber < 9)
                        distance = 5; // not to far at the beginning

                    value += DistanceFactor * distance;
                }
                catch
                {
                    _text = "Step_6912:; " + _fleets_Summary + " > ### unable to get furthest object from home world for station value"
                            //+ " " + _fleet.ObjectID
                            //+ " " + _fleet.Name
                            ////+ " to go to " + bestSystemToColonize.Name
                            //+ " " + _fleet.Location
                            ;
                    if (_writeDirectly_Fleets) Console.WriteLine(_text);
                    _fleet_Text += _newline + _text;
                    //GameLog.Client.AI.DebugFormat("unable to get furthest object from home world for station value");
                }

                List<Sector> strandedShipSectors = FindStrandedShipSectors(_fleet.Owner); //altering collection while sorting it!!!!!!!!!!!!!!
                if (strandedShipSectors.Count > 0)
                {
                    if (strandedShipSectors.Contains(_sector))
                    {
                        value += StrandedShipSectorValue;
                        //GameLog.Client.AI.DebugFormat("D");
                    }
                }
            }
            //GameLog.Core.AI.DebugFormat("Station at {0} has value {1}", _location.Location, (value + randomInt));

            //_text = "Step_6914:; " + _fleets_Summary + " > "
            //        //+ " " + _fleet.ObjectID
            //        //+ " " + _fleet.Name
            //        ////+ " to go to " + bestSystemToColonize.Name
            //        //+ " " + _fleet.Location
            //        ;
            //if (_writeDirectly_Colony) Console.WriteLine(_text);
            //_fleet_Text += _newline + _text;

            return value; // + randomInt;
        }

        public static void BuildStation(Fleet _fleet)
        {
            GetFleetOwner(_fleet);
            string _fleetText = "";
            string _text = "";
            //string _newline = Environment.NewLine;
            // GameLog.Core.AI.DebugFormat("Constructor _fleet {0} build station at {1}, {2} UnitActivity = {3}", _fleet.Owner.Key, _fleet.Sector.Name, _fleet.Location, _fleet.Activity.ToString());
            BuildStationOrder _order = new BuildStationOrder();
            _order.BuildProject = _order.FindTargets(_fleet).Cast<StationBuildProject>().LastOrDefault(o => o.StationDesign.IsCombatant);

            if (_fleet.Owner.IsHuman)
            {
                //Debugger.Break();
            }

            bool _canBeAssigned = _order.CanAssignOrder(_fleet); // e.g. _location has no other owner

            if (_fleet.IsStranded)
            {
                _canBeAssigned = true;
            }

            if (_order.BuildProject != null/* && _canBeAssigned && _fleet.Activity != UnitActivity.BuildStation*/)
            {
                _fleet.SetOrder(_order);
                _fleet.AITypeUnit = UnitAIType.Constructor;
                _fleet.Activity = UnitActivity.BuildStation;

                _text = "Step_3703:; "
                    + CreateUpdateFleetText(_fleet, out _fleetText)
                    //+ GameEngine.LocationString(_fleet.Location.ToString())
                    //+ " > Constructor _fleet " + " " + _fleet.ObjectID
                    //+ " " + _fleet.Name
                    //+ " " + _fleet.ClassName
                    + " > has _order= " + _fleet.Order.OrderName
                    + " " + _order.BuildProject
                    //+ ", activity= " + _fleet.Activity.ToString()
                    ;
                //if (_writeDirectly_Fleets) Console.WriteLine(_text);
                _fleet_Text += Environment.NewLine + _text;

                if (_fleet.Owner.IsHuman)
                {
                    //Debugger.Break(); 
                }
                //GameLog.Core.AI.DebugFormat("Constructor _fleet {0} _order {1} at {2} UnitActivity = {3}", _fleet.Owner.Key, _fleet.Order.OrderName, _fleet.Sector.Name, _fleet.Activity.ToString());
            }
            else
            {
                _text = "Step_3709:; "
                        + GameEngine.LocationString(_fleet.Location.ToString())
                        + " > Constructor _fleet " + " " + _fleet.ObjectID
                        + " " + _fleet.Name
                        + " " + _fleet.ClassName
                        + " has _order= " + _fleet.Order.OrderName
                        + ", activity= " + _fleet.Activity.ToString()
                        + " >> but can not build !?!?!"
                        ;
                if (_writeDirectly_Fleets) Console.WriteLine(_text);
                _fleet_Text += Environment.NewLine + _text;
            }

        }
        private static void BuildStation(Fleet fleet, List<Fleet> allFleets)
        {
            fleet.Route.Clear();
            if (allFleets != null)
            {
                if (allFleets.Count > 1) // for any other constructor _fleets here?
                {
                    for (int i = 0; i < allFleets.Count; i++)
                    {
                        if (i == 0)
                        {
                            BuildStation(fleet);
                            fleet.AITypeUnit = UnitAIType.Building;
                        }
                        else
                        {  // stay and help build
                            allFleets[i].AITypeUnit = UnitAIType.Building;
                            allFleets[i].Activity = UnitActivity.Hold;
                        }
                    }
                }
                else
                {
                    BuildStation(fleet);
                    fleet.AITypeUnit = UnitAIType.Constructor;
                }
            }

        }

        /*
        * Station best _location
        */
        /// <summary>
        /// Returns the best possible <see cref="Sector"/> for a given <see cref="Fleet"/>
        /// to build a <see cref="Station"/> in
        /// </summary>
        /// <param name="_fleet"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool GetBestSectorFor_BuildStation(Fleet _fleet, List<Fleet> constructionFleets, out Sector _bestSector)
        {
            if (_fleet == null || _fleet.ObjectID < 0)
            {
                _bestSector = null;
                return false;
            }

            _bestSector = null;
            string _bestSectorName = "NO BEST SECTOR";
            string _newline = Environment.NewLine;

            string _text = "Step_6520:; " + CreateUpdateFleetText(_fleet, out _fleet_Text)
                + " > GetBestSectorFor_BuildStation.... ";
            if (_writeDirectly_Fleets) Console.WriteLine(_text);
            //GameLog.Client.AI.DebugFormat(_text);



            _fleet.GetCivM(_fleet, out CivilizationManager _civM);


            GetFleetOwner(_fleet);

            if (_fleet == null)
            {
                throw new ArgumentNullException(nameof(_fleet));
            }

            if (_fleet.Ships.Count == 0)
            {
                _bestSector = null;
                return false;
            }

            bool _write = _writeDirectly_Fleets;
            //_write = true;
            _write = false;

            //_fleet.GetCivM(_fleet, out CivilizationManager _civM);

            //if (_civM.StrandedShipsSector != null)
            //{

            //    _bestSector = _civM.StrandedShipsSector;
            //    return true;
            //}


            //_text = "GetBestSectorFor_BuildStation: ";
            //int halfMapWidthX = GameContext.Current.Universe.Map.Width / 2;
            //_text += _newline + "halfMapWidthX= " + halfMapWidthX.ToString();
            //if (_write) Console.WriteLine(halfMapWidthX);

            //int halfMapHeightY = GameContext.Current.Universe.Map.Height / 2;
            //_text += _newline + "halfMapHeightY= " + halfMapHeightY.ToString();
            //if (_write) Console.WriteLine(halfMapHeightY);

            //int thirdMapWidthX = GameContext.Current.Universe.Map.Width / 3;
            //_text += _newline + "thirdMapWidthX= " + thirdMapWidthX.ToString();
            //if (_write) Console.WriteLine(thirdMapWidthX);

            //int thirdMapHeightY = GameContext.Current.Universe.Map.Height / 3;
            //_text += _newline + "thirdMapHeightY= " + thirdMapHeightY.ToString();
            //if (_write) Console.WriteLine(thirdMapHeightY);

            //int quarterMapWidthX = GameContext.Current.Universe.Map.Width / 4;
            //_text += _newline + "quarterMapWidthX= " + quarterMapWidthX.ToString();
            //if (_write) Console.WriteLine(quarterMapWidthX);

            //int quarterMapHeightY = GameContext.Current.Universe.Map.Height / 4;
            //_text += _newline + "quarterMapHeightY= " + quarterMapHeightY.ToString();
            //if (_write) Console.WriteLine(quarterMapHeightY);

            //int lengthQuarterMap = (int)Math.Sqrt((int)Math.Pow(quarterMapWidthX, 2) + (int)Math.Pow(quarterMapHeightY, 2));
            //_text += _newline + "lengthQuarterMap= " + lengthQuarterMap.ToString();
            //if (_write) Console.WriteLine(lengthQuarterMap);

            //int lengthThirdMap = (int)Math.Sqrt((int)Math.Pow(thirdMapWidthX, 2) + (int)Math.Pow(thirdMapHeightY, 2));
            //_text += _newline + "lengthThirdMap= " + lengthThirdMap.ToString();
            //if (_write) Console.WriteLine(lengthThirdMap);

            //if (_write) Console.WriteLine(_text);


            //_write = true;
            ////_write = false;
            //switch (_fleet.Owner.Key)
            //{
            //    case "BORG":
            //    //{
            //    //    _text = "Step_6560: GetBestSectorFor_BuildStation for Borg";
            //    //    if (_writeDirectly_Colony) Console.WriteLine(_text);
            //    //    //GameLog.Client;.AI.DebugFormat(_text);
            //    //    int borgX = GameContext.Current.Universe.HomeColonyLookup[_fleet.Owner].Location.X;
            //    //    int borgXDelta = Math.Abs(GameContext.Current.Universe.HomeColonyLookup[_fleet.Owner].Location.X - halfMapWidthX) / 4;
            //    //    int borgY = GameContext.Current.Universe.HomeColonyLookup[_fleet.Owner].Location.Y;
            //    //    int borgYDelta = Math.Abs(halfMapHeightY - GameContext.Current.Universe.HomeColonyLookup[_fleet.Owner].Location.Y) / 4;

            //    //    MapLocation borgHomeLocation = GameContext.Current.Universe.HomeColonyLookup[_fleet.Owner].Location;
            //    //    List<UniverseObject> objectsAlongCenterAxis = GameContext.Current.Universe.Objects
            //    //        .Where(s => s.Location != null
            //    //        && s.Sector.Station == null
            //    //        && s.Location.X >= halfMapWidthX + borgXDelta && s.Location.X <= borgX
            //    //        && s.Location.Y <= Math.Abs(halfMapHeightY - borgYDelta) && s.Location.Y >= borgY + borgYDelta)
            //    //        //&& s.Location == borgHomeLocation)                         
            //    //        .ToList();

            //    //    if (objectsAlongCenterAxis.Count == 0)
            //    //    {
            //    //        bool_bestSector = null;
            //    //        return false;
            //    //    }
            //    //    // GameLog.Core.AI.DebugFormat("{0} Universe Objects for {1} station search", objectsAlongCenterAxis.Count(), _fleet.Owner.Key);
            //    //    try
            //    //    {
            //    //        objectsAlongCenterAxis.Sort((a, b) =>
            //    //            GetValue_Station(a.Sector, _fleet, objectsAlongCenterAxis)
            //    //            .CompareTo(GetValue_Station(b.Sector, _fleet, objectsAlongCenterAxis)));
            //    //    }
            //    //    catch
            //    //    {
            //    //        _text = "unable to sort objects for Borg station location";
            //    //        if (_writeDirectly_Colony) Console.WriteLine(_text);
            //    //        GameLog.Client.AI.DebugFormat(_text);
            //    //        bool_bestSector = null;
            //    //        return false;
            //    //    }
            //    //    List<Fleet> otherConstructors = GameContext.Current.Universe.Find<Fleet>()
            //    //            .Where(o => o.Owner != _fleet.Owner && o.IsConstructor || o.MultiFleetHasAConstructor).ToList();
            //    //    List<Sector> _sectors = new List<Sector>();
            //    //    foreach (UniverseObject anObject in objectsAlongCenterAxis)
            //    //    {
            //    //        _sectors.Add(anObject.Sector);
            //    //    }
            //    //    foreach (Fleet aFleet in otherConstructors)
            //    //    {
            //    //        if (_sectors.Any(o => o == aFleet.Sector))
            //    //        {
            //    //            _ = _sectors.Remove(aFleet.Sector);
            //    //        }
            //    //    }

            //    //    bool_bestSector = _sectors.Last(); //objectsAlongCenterAxis[objectsAlongCenterAxis.Count - 1].Sector;

            //    //    if (bool_bestSector == null)
            //    //    {
            //    //        return false;
            //    //    }

            //    //    _text = "Borg station selected _location  at " + bool_bestSector.Location + " " + bool_bestSector.Name;
            //    //    if (_writeDirectly_Colony) Console.WriteLine(_text);
            //    //    // GameLog.Core.AI.DebugFormat(_text);
            //    //    return true;
            //    //}

            //    case "DOMINION":
            //    //{
            //    //    _text = "Step_6580: GetBestSectorFor_BuildStation for DOMINION";
            //    //    if (_writeDirectly_Colony) Console.WriteLine(_text);
            //    //    //GameLog.Client.AI.DebugFormat("Dominion");
            //    //    int domX = GameContext.Current.Universe.HomeColonyLookup[_fleet.Owner].Location.X;
            //    //    if (_writeDirectly_Colony) Console.WriteLine(domX);
            //    //    int domXDelta = Math.Abs(halfMapWidthX - GameContext.Current.Universe.HomeColonyLookup[_fleet.Owner].Location.X) / 4;
            //    //    if (_writeDirectly_Colony) Console.WriteLine(domXDelta);
            //    //    int domY = GameContext.Current.Universe.HomeColonyLookup[_fleet.Owner].Location.Y;
            //    //    if (_writeDirectly_Colony) Console.WriteLine(domY);
            //    //    int domYDelta = Math.Abs(halfMapHeightY - GameContext.Current.Universe.HomeColonyLookup[_fleet.Owner].Location.Y) / 4;
            //    //    if (_writeDirectly_Colony) Console.WriteLine(domYDelta);

            //    //    List<UniverseObject> objectsAlongCenterAxis = GameContext.Current.Universe.Objects
            //    //        // .Where(c => !FleetHelper.IsSectorWithinFuelRange(c.Sector, _fleet))
            //    //        .Where(s => s.Location != null
            //    //        && s.Sector.Station == null
            //    //        && s.Location.X <= Math.Abs(halfMapWidthX - domXDelta) && s.Location.X > domX
            //    //        && s.Location.Y <= halfMapHeightY - domYDelta && s.Location.Y > domY)
            //    //        // find a list of objects in some _location around Dom side of galactic center
            //    //        .ToList();
            //    //    //foreach (var item in objectsAlongCenterAxis)
            //    //    //{
            //    //    //    _text = item.Location
            //    //    //        + " " + item.Name
            //    //    //        + " " + item.ObjectID

            //    //    //        ;
            //    //    //    if (_writeDirectly_Colony) Console.WriteLine(_text);

            //    //    //    //if (item.ObjectType != )
            //    //    //}


            //    //    if (objectsAlongCenterAxis.Count == 0)
            //    //    {
            //    //        bool_bestSector = null;
            //    //        return false;
            //    //    }
            //    //    //GameLog.Core.AI.DebugFormat("{0} Universe Objects for {1} station search", objectsAlongCenterAxis.Count(), _fleet.Owner.Key);

            //    //    try
            //    //    {
            //    //        _text = "try objectsAlongCenterAxis... for _fleet" + _fleet.ObjectID + " " + _fleet.Name + " " + _fleet.ClassName;
            //    //        if (_writeDirectly_Colony) Console.WriteLine(_text);

            //    //        objectsAlongCenterAxis.Sort((a, b) =>
            //    //       GetValue_Station(a.Sector, _fleet, objectsAlongCenterAxis)
            //    //       .CompareTo(GetValue_Station(b.Sector, _fleet, objectsAlongCenterAxis)));

            //    //    }
            //    //    catch
            //    //    {
            //    //        _text = "unable to sort objects for Dominion station location";
            //    //        if (_writeDirectly_Colony) Console.WriteLine(_text);
            //    //        GameLog.Client.AI.DebugFormat(_text);
            //    //        bool_bestSector = null;
            //    //        return false;
            //    //    }
            //    //    bool_bestSector = objectsAlongCenterAxis.Last().Sector; //[objectsAlongCenterAxis.Count - 1].Sector;
            //    //    if (bool_bestSector == null)
            //    //    {
            //    //        return false;
            //    //    }

            //    //    _text = "Dominion station selected _location  at " + bool_bestSector.Location + " " + bool_bestSector.Name;
            //    //    if (_writeDirectly_Colony) Console.WriteLine(_text);
            //    //    // GameLog.Core.AI.DebugFormat(_text);
            //    //    return true;
            //    //}
            //    case "KLINGONS":
            //    case "TERRANEMPIRE":
            //    case "FEDERATION":
            //    case "ROMULANS":
            //    case "CARDASSIANS":
            //        {
            //var furthestObject = GameContext.Current.Universe.FindFurthestObject<UniverseObject>(homeSector.Location, _fleet.Owner);

            if (_fleet.Owner.IsHuman)
            {
                //Debugger.Break();
            }

            Sector homeSector = GameContext.Current.Universe.HomeColonyLookup[_fleet.Owner].Sector;

            int xloc = GameContext.Current.Universe.Map.Width / 2;
            int yloc = GameContext.Current.Universe.Map.Height / 2;

            MapLocation _center = new MapLocation(xloc, yloc);


            List<UniverseObject> objectsAroundHome = GameContext.Current.Universe.Objects
                .Where(s => s.Location != null
                && s.ObjectType != UniverseObjectType.Ship
                && s.ObjectType != UniverseObjectType.Fleet
                //&& s.ObjectType != UniverseObjectType.StarSystem.bla
                && s.Sector.Station == null
                && s.Sector.Owner == null
                && GetDistanceTo(_center, s.Location) < xloc - 3 // but if there is a system + BORG don't colonize... > ToDo 2025-03-16
                )  // do not build at the edge of the map
                   //&& GetDistanceTo(_center, s.Location) < lengthThirdMap)
                .ToList();

            List<Sector> _availableSectors = new List<Sector>();
            List<Sector> _maybeSectors = new List<Sector>();

            int _distance = 4;

        _SearchAvailableSectorsOnceAgain:
            _availableSectors = MapHelper.GetSectorsWithinRadius(_fleet.Sector, _distance).ToList();

            //List<UniverseObject> _allCivFleets = GameContext.Current.Universe.Objects
            //    .Where(o => o.ObjectType == UniverseObjectType.Ship
            //    && o.OwnerID == _fleet.OwnerID
            //    && o.CanMove
            //    ).ToList();

            //foreach (var item in _allCivFleets)
            //{
            //    //if (item.ObjectType.)
            //}

            //if (_checkFleetOrders == true && _checkOnlyPlayersUnits)
            if (_fleet.Owner.IsHuman)
            {
                //Debugger.Break();
            }

            string _availableSectorsText = "";

            foreach (var _sector in _availableSectors)
            {
                int _distance2 = MapLocation.GetDistance(_sector.Location, homeSector.Location);

                if (_distance2 != _distance - 1)  // not all sectors > too much > only the far away
                {
                    continue;
                }

                _text = "Step_6913:; "
                     + "" + _fleets_Summary
                     + " >  DistfromHome > " + GameEngine.Do_x2_Digit_String(_distance2.ToString())
                     + ", Value for Station= " + GetValue_Station(_sector, _fleet, objectsAroundHome)
                     + " for " + _sector.Location
                     + " " + _sector.Name
                    ;

                //if (_write) 
                //Console.WriteLine(_text);
                _availableSectorsText += _text;
                //_fleet_Text += _newline + _text;

                if (_sector.System != null)
                {
                    if (_sector.System.StarType == StarType.BlackHole
                        || _sector.System.StarType == StarType.NeutronStar
                        || _sector.System.StarType == StarType.XRayPulsar
                        || _sector.System.StarType == StarType.RadioPulsar
                        || _sector.System.StarType == StarType.Quasar

                        )
                        _text += "";


                }
                else
                {
                    if (_fleet.Owner.IsHuman)
                    {
                        //Debugger.Break();
                    }

                    if (GetDistanceTo(_center, _sector.Location) > xloc - 2 || GetDistanceTo(_center, _sector.Location) > yloc - 2) // do not build at the edge of the map
                    {
                        // only add if BORG (they don't colonize but need to occupy sectors with planets
                        if (_fleet.Owner.Key == "BORG" && !_sector.IsOwned && _sector.Station == null)
                        {
                            _maybeSectors.Add(_sector);
                        }
                        else
                        {
                            if (!_sector.IsOwned && _sector.Station == null) // owned _location = no build _order
                            {
                                if (FleetHelper.IsSectorWithinFuelRange(_sector, _fleet))
                                {
                                    _maybeSectors.Add(_sector);
                                }

                            }

                        }
                    }

                }

            }

            if (_fleet.Owner.IsHuman)
            {
                //Debugger.Break();
            }

            //_availableSectors = MapHelper.GetSectorsWithinRadius(_fleet.Sector, _distance).ToList();

            if (_maybeSectors.Count < 1)
            {
                _distance += 1;
                goto _SearchAvailableSectorsOnceAgain;
            }

            //_fleet.GetCivM(_fleet, out CivilizationManager _civM);

            try
            {
                _maybeSectors.Sort((a, b) =>
                              GetValue_Station(a, _fleet, objectsAroundHome)
                              .CompareTo(GetValue_Station(b, _fleet, objectsAroundHome)));

                _bestSector = _maybeSectors.Last();
                _bestSectorName = _bestSector.Name;

                if (_bestSector != null)
                {
                    return true;
                }


                //if (_maybeSectors.Count > _civM.Z_Ship_Construction_Available + _civM.Z_Ship_Construction_Ordered - 1)
                //{
                //    _civM.Z_Ship_Construction_Needed += 1;
                //}
            }
            catch { _bestSector = null; return false; }

            //try
            //{
            //    objectsAroundHome.Sort((a, b) =>
            //                  GetValue_Station(a.Sector, _fleet, objectsAroundHome)
            //                  .CompareTo(GetValue_Station(b.Sector, _fleet, objectsAroundHome)));
            //}
            //catch { bool_bestSector = null; return false; }



            //if (objectsAroundHome.Count == 0)
            //{
            //    bool_bestSector = null;
            //    return false;
            //}

            // GameLog.Core.AI.DebugFormat("{0} Universe Objects for {1} station search", objectsAroundHome.Count(), _fleet.Owner.Key);

            // old stuff - only objects were checked, but not all sectors
            //try
            //{
            //    objectsAroundHome.Sort((a, b) =>
            //                  GetValue_Station(a.Sector, _fleet, objectsAroundHome)
            //                  .CompareTo(GetValue_Station(b.Sector, _fleet, objectsAroundHome)));
            //}
            //catch { bool_bestSector = null; return false; }

            try
            {
                //string _obj = "";
                //foreach (var item in objectsAroundHome)
                //{
                //    int distance = MapLocation.GetDistance(item.Location, homeSector.Location);
                //    _text = "Step_6915:; "
                //         + "" + _fleets_Summary
                //         + " >  DistfromHome > " + GameEngine.Do_x2_Digit_String(distance.ToString())
                //         + ", Value for Station= " + GetValue_Station(item.Sector, _fleet, objectsAroundHome)
                //         + " for (object) " + item.Location
                //         + " " + item.Name + " "

                //        ;
                //    if (_write) Console.WriteLine(_text);
                //    _fleet_Text += _newline + _text;

                //    //_obj += _newline + _text;
                //}
                //if (_writeDirectly_Colony) Console.WriteLine(_obj);

                //if (_checkFleetOrders == true && _checkOnlyPlayersUnits)
                if (_fleet.Owner.IsHuman)
                {
                    //Debugger.Break();
                }



                //bool_bestSector = objectsAroundHome.Last().Sector; //[objectsAroundHome.Count - 1].Sector;

                //2024-12-28
                //_bestSector = _maybeSectors.Last();

                _text = "Step_6550:; "
                    + "" + CreateUpdateFleetText(_fleet, out _fleet_Text)
                    + _newline + ": _location selected for station build for "
                    + " > " + GameEngine.LocationString(_bestSector.Location.ToString())
                    + " Name = " + _bestSectorName

                     ;
                //if (_writeDirectly_Fleets) Console.WriteLine(_text);
                //_fleets_Summary += _newline + _text;

                //if (_checkFleetOrders == true && _checkOnlyPlayersUnits)
                if (_fleet.Owner.IsHuman)
                {
                    //Debugger.Break();
                }

                // GameLog.Core.AI.DebugFormat(_text);
            }
            catch { _bestSector = null; return false; }


            //return _bestSector != null;
            //}

            //default: // non of the Empires
            //_bestSector = null;
            _text = "Step_7759:; " + CreateUpdateFleetText(_fleet, out _fleet_Text) + " > no _location for station";
            if (_writeDirectly_Fleets) Console.WriteLine(_text);
            //GameLog.Core.AI.DebugFormat(_text);

            //if (_checkFleetOrders == true && _checkOnlyPlayersUnits)
            if (_fleet.Owner.IsHuman)
            {
                Debugger.Break();
            }

            return false; // could not find _location for station
            //}
        }

        /// <summary>
        /// Determines the value of a <see cref="Civilization"/> providing
        /// medical services to a <see cref="Colony"/>
        /// </summary>
        /// <param name="colony"></param>
        /// <param name="civ"></param>
        /// <returns></returns>
        public static int GetValue_Medical(Colony colony, Civilization civ)
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
            // GameLog.Core.AI.DebugFormat("Medical value for {0} is {1})", _colony, value);
            return value;
        }

        /*
        * Medical best _colony
        */

        /// <summary>
        /// Determines the best <see cref="Colony"/> for a <see cref="Fleet"/>
        /// to provide medical services to
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool GetBestColonyFor_Medical(Fleet fleet, out Colony result)
        {
            GetFleetOwner(fleet);
            if (fleet == null)
            {
                throw new ArgumentNullException(nameof(fleet));
            }
            string _text = "";

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

                _text = "Step_7678:; " + CreateUpdateFleetText(fleet, out _fleet_Text) + " > GetBestColonyFor_Medical...available= " + _col_available.Count;

                foreach (var _col in _col_available)
                {
                    _text += " > " + _col.Location;
                }
                if (_writeDirectly_Fleets) Console.WriteLine(_text);


                possibleColonies = GameContext.Current.Universe.Find<Colony>()
                    // where health < 90
                    .Where(h => h.Health.CurrentValue < 90)
                //We need to know about it (no cheating)
                .Where(s => mapData.IsScanned(s.Location) && mapData.IsExplored(s.Location))
                // Borg do not play well with others
                .Where(d => d.Owner.Key == fleet.Owner.Key /*|| !d.Owner.IsEmpire*/)  // first own systems
                                                                                      //.Where(d => d.Owner.Key != "BORG" && _fleet.Owner.Key != "BORG" && ) // old
                                                                                      //.Where(d => d.Owner.Key == "BORG" && _fleet.Owner.Key == "BORG")
                                                                                      //In fuel range
                .Where(c => FleetHelper.IsSectorWithinFuelRange(c.Sector, fleet)
                //Where we can enter the _location
                //Where there aren't any hostiles
                //Where they aren't at war
                && c.Owner.Key == fleet.Owner.Key
                && DiplomacyHelper.IsTravelAllowed(fleet.Owner, c.Sector)
                && GameContext.Current.Universe.FindAt<Orbital>(c.Location).Any(o => DiplomacyHelper.ArePotentialEnemies(fleet.Owner, o.Owner))
                && !DiplomacyHelper.AreAtWar(c.Owner, fleet.Owner))
                //Where other med _ship is not already going
                .Where(d => !otherFleets.Any(f => f.Route.Waypoints.LastOrDefault() == d.Location || d.Location == f.Location && f.Order is MedicalOrder))
                .ToList();
            }

            if (fleet != null && possibleColonies.Count == 0)
            {
                possibleColonies = GameContext.Current.Universe.Find<Colony>()
                            //where health < 90
                            .Where(h => h.Health.CurrentValue < 90)
                        //We need to know about it (no cheating)
                        .Where(s => mapData.IsScanned(s.Location) && mapData.IsExplored(s.Location))
                        //Borg do not play well with others
                        .Where(d => !d.Owner.IsEmpire)  // including Minors
                                                        //.Where(d => d.Owner.Key != "BORG" && _fleet.Owner.Key != "BORG" && ) // old
                                                        //.Where(d => d.Owner.Key == "BORG" && _fleet.Owner.Key == "BORG")
                                                        //In fuel range
                        .Where(c => FleetHelper.IsSectorWithinFuelRange(c.Sector, fleet)
                        //Where we can enter the _location
                        //Where there aren't any hostiles
                        //Where they aren't at war
                        //&& c.Owner.Key == _fleet.Owner.Key
                        && DiplomacyHelper.IsTravelAllowed(fleet.Owner, c.Sector)
                        && GameContext.Current.Universe.FindAt<Orbital>(c.Location).Any(o => DiplomacyHelper.ArePotentialEnemies(fleet.Owner, o.Owner))
                        && !DiplomacyHelper.AreAtWar(c.Owner, fleet.Owner))
                        //Where other med _ship is not already going
                        .Where(d => !otherFleets.Any(f => f.Route.Waypoints.LastOrDefault() == d.Location || d.Location == f.Location && f.Order is MedicalOrder))
                        .ToList();

                result = null;

                _text = "Step_7676:; " + CreateUpdateFleetText(fleet, out _fleets_Summary) + " > GetBestColonyFor_Medical...possible NON-Empire= " + possibleColonies.Count;

                foreach (var _col in _col_available)
                {
                    _text += " > " + _col.Health.CurrentValue + " at " + _col.Location
                        //+ ", Health= " + _col.Health.CurrentValue
                        ;
                }
                if (_writeDirectly_Fleets) Console.WriteLine(_text);


                _text = "Step_7674:; " + CreateUpdateFleetText(fleet, out _fleets_Summary) + " > GetBestColonyFor_Medical > actually no _colony found ";
                if (_writeDirectly_Fleets) Console.WriteLine(_text);

                return false;
            }

            possibleColonies.Sort((a, b) =>
                (GetValue_Medical(a, fleet.Owner) * HomeSystemDistanceModifier(fleet, a.Sector))
                .CompareTo(GetValue_Medical(b, fleet.Owner) * HomeSystemDistanceModifier(fleet, b.Sector)));

            result = possibleColonies[possibleColonies.Count - 1];

            if (result == null && _col_available.Count > 0)
            {
                result = _col_available.First();
            }

            _text = "Step_7677:; " + CreateUpdateFleetText(fleet, out _fleets_Summary)
                    + " > GetBestColonyFor_Medical= " + result.Location
                    + " > " + result.Name
                    + ", Owner= " + result.Sector.Owner
                    ;
            if (_writeDirectly_Fleets) Console.WriteLine(_text);
            return true;
        } // end of GetBestColonyFor > Medical

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
        public static int GetValue_Diplomatic(Colony colony, Civilization civ)
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
            if (foreignPower.DiplomacyData.Status == ForeignPowerStatus.OwnerIsMember
                || otherForeignPower.DiplomacyData.Status == ForeignPowerStatus.OwnerIsMember)
            {
                return 0;
            }

            if (foreignPower.DiplomacyData.Trust.CurrentValue > 900
                || foreignPower.DiplomacyData.Regard.CurrentValue > 900)
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

            //GameLog.Core.AI.DebugFormat("diplomacy value for {0} belonging to {1} is {2} to the {3}", _colony.Name, _otherCiv.Key, value, _civM.Key);
            return similarTraits;
        }

        /*
        * Diplomacy best _colony
        */

        /// <summary>
        /// Determines the best <see cref="Colony"/> for a <see cref="Fleet"/>
        /// to provide medical services to
        /// </summary>
        /// <param name="_fleet"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool GetBestColonyFor_Diplomacy(Fleet _fleet, out Colony result)
        {
            GetFleetOwner(_fleet);
            if (_fleet == null)
            {
                throw new ArgumentNullException(nameof(_fleet));
            }
            string _text = "";

            List<Fleet> diplomacyFleets = GameContext.Current.Universe.FindOwned<Fleet>(_fleet.Owner)
                .Where(o => o.IsDiplomatic).ToList();
            List<Fleet> otherFleets = diplomacyFleets.Where(o => o != _fleet).ToList();

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[_fleet.Owner];
            CivilizationMapData mapData = civManager.MapData;
            IEnumerable<Fleet> diplomaticShips = GameContext.Current.Universe.FindOwned<Fleet>(_fleet.Owner).Where(s => s.IsDiplomatic);
            List<Colony> possibleColonies = new List<Colony>();
            GetFleetOwner(_fleet);
            if (_fleet.Owner != null)
            {
                possibleColonies = GameContext.Current.Universe.Find<Colony>()
                //We need to know about it (no cheating)
                .Where(s => mapData.IsScanned(s.Location)
                && mapData.IsExplored(s.Location)
                && s.Owner != _fleet.Owner)
                //In fuel range
                .Where(c => FleetHelper.IsSectorWithinFuelRange(c.Sector, _fleet)
                //Where we can enter the _location
                //Where there aren't any hostiles
                //Where they aren't at war
                && DiplomacyHelper.IsTravelAllowed(_fleet.Owner, c.Sector)
                && GameContext.Current.Universe.FindAt<Orbital>(c.Location).Any(o => DiplomacyHelper.ArePotentialEnemies(_fleet.Owner, o.Owner))
                && !DiplomacyHelper.AreAtWar(c.Owner, _fleet.Owner))
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
                (GetValue_Diplomatic(a, _fleet.Owner) * HomeSystemDistanceModifier(_fleet, a.Sector))
                .CompareTo(GetValue_Diplomatic(b, _fleet.Owner) * HomeSystemDistanceModifier(_fleet, b.Sector)));
            result = possibleColonies[possibleColonies.Count - 1];



            _text = "Step_7676:; " + CreateUpdateFleetText(_fleet, out string _fleetText)
                + " > GetBestColonyFor_Diplomacy= " + result.Location
                + " > " + result.Name
                + ", Owner= " + result.Owner
                //+ ", for " 
                ;
            if (_writeDirectly_Fleets) Console.WriteLine(_text);
            //if (_checkFleetOrders == true && _checkOnlyPlayersUnits)
            if (_fleet.Owner.IsHuman)
            {
                //Debugger.Break();
            }
            return true;
        }

        /*
         * Spying value
         */

        /// <summary>
        /// Determines the value of spying on a <see cref="Colony"/>
        /// to a given <see cref="Civilization"/>
        /// </summary>
        /// <param name="_colony"></param>
        /// <param name="_civ"></param>
        /// <returns></returns>
        public static int GetValue_Spying(Colony _colony, Civilization _civ)
        {
            if (_colony == null)
            {
                throw new ArgumentNullException(nameof(_colony));
            }

            if (_civ == null)
            {
                throw new ArgumentNullException(nameof(_civ));
            }
            Civilization _otherCiv = _colony.Owner;
            if (_colony.System.Name == _otherCiv.HomeSystemName)
            {
                return 1000;
            }

            if (_otherCiv.CivID == _civ.CivID)
            {
                return 0;
            }

            if (!DiplomacyHelper.IsContactMade(_civ.CivID, _otherCiv.CivID))
            {
                return 0;
            }

            const int EnemyColonyPriority = 50;
            const int NeutralColonyPriority = 25;
            const int FriendlyColonyPriority = 10;

            int value = 0;

            if (DiplomacyHelper.AreAllied(_colony.Owner, _civ) || DiplomacyHelper.AreFriendly(_colony.Owner, _civ))
            {
                value += FriendlyColonyPriority;
            }
            else if (DiplomacyHelper.AreNeutral(_colony.Owner, _civ))
            {
                value += NeutralColonyPriority;
            }
            else if (DiplomacyHelper.AreAtWar(_colony.Owner, _civ))
            {
                value += EnemyColonyPriority;
            }

            // GameLog.Core.AI.DebugFormat("Spying value for {0} is {1}", _colony, value);
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
        public static int GetValue_Science(StarSystem system, Civilization civ) // _civM is _fleet.Owner
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
                //Civilization _otherCiv = system.Owner;
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

            // GameLog.Core.AI.DebugFormat("Spying value for {0} is {1}", system, value);
            return value;
        }


        /// <summary>
        /// Gets the best <see cref="Colony"/> for a <see cref="Fleet"/> to spy on
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        /// 
        public static bool GetBestColonyFor_Spying(Fleet fleet, out Colony result)
        {
            GetFleetOwner(fleet);
            if (fleet == null)
            {
                throw new ArgumentNullException(nameof(fleet));
            }
            string _text = "";


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
                // GameLog.Client.AI.DebugFormat("Damn, no Home System of Empire found, possible colonies = {0}", possibleColonies.Count());
                result = null;
                return false;
            }

            possibleColonies.Sort((a, b) =>
                (GetValue_Spying(a, fleet.Owner) * HomeSystemDistanceModifier(fleet, a.Sector))
                .CompareTo(GetValue_Spying(b, fleet.Owner) * HomeSystemDistanceModifier(fleet, b.Sector)));
            result = possibleColonies[possibleColonies.Count - 1];
            _text = "Step_7677:; " + CreateUpdateFleetText(fleet, out string _fleetText)
                    + " > GetBestColonyFor_Spying= " + result.Location
                    + " > " + result.Name
                    + ", Owner= " + result.Owner
                    //+ " for >> " 
                    ;
            if (_writeDirectly_Fleets) Console.WriteLine(_text);
            // GameLog.Client.AI.DebugFormat("Yippy, System of Empire found!, possible spied _colony = {0}", possibleColonies.FirstOrDefault().Name);
            return true;
        }

        /*
        / Science best system
        */

        /// <summary>
        /// Gets the best <see cref="StarSystem"/> for a <see cref="Fleet"/> to spy on
        /// </summary>
        /// <param name="_fleet"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool GetBestSystemFor_Science(Fleet _fleet, out StarSystem result)
        {
            GetFleetOwner(_fleet);
            if (_fleet == null)
            {
                throw new ArgumentNullException(nameof(_fleet));
            }

            List<Fleet> scienceFleets = GameContext.Current.Universe.FindOwned<Fleet>(_fleet.Owner)
                .Where(o => o.IsScience).ToList();
            List<Fleet> otherFleets = scienceFleets.Where(o => o != _fleet).ToList();

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[_fleet.Owner];
            CivilizationMapData mapData = civManager.MapData;
            IEnumerable<Fleet> scienceShips = GameContext.Current.Universe.FindOwned<Fleet>(_fleet.Owner).Where(s => s.IsScience);
            List<StarSystem> possibleSystems = new List<StarSystem>();
            GetFleetOwner(_fleet);
            if (_fleet.Owner != null)
            {
                possibleSystems = GameContext.Current.Universe.Find<StarSystem>()
                //That isn't owned by us
                .Where(s => s.Sector != null
                //&& mapData.IsScanned(s.Location)
                //&& mapData.IsExplored(s.Location) 
                && FleetHelper.IsSectorWithinFuelRange(s.Sector, _fleet)
                && DiplomacyHelper.IsTravelAllowed(_fleet.Owner, s.Sector)
                        //&& s.StarType == StarType.BlackHole
                        && s.StarType == StarType.XRayPulsar
                        || s.StarType == StarType.NeutronStar
                        || s.StarType == StarType.Quasar
                        || s.StarType == StarType.RadioPulsar
                        || s.StarType == StarType.XRayPulsar
                        || s.StarType == StarType.Wormhole

                )
                //Where other science _ship is not already going
                .Where(d => !otherFleets.Any(f => f.Route.Waypoints.LastOrDefault() == d.Location || d.Location == f.Location))
                .ToList();

            }
            if (possibleSystems.Contains(civManager.HomeSystem))
            {
                _ = possibleSystems.Remove(civManager.HomeSystem);
            }

            if (possibleSystems.Count == 0)
            {
                //  GameLog.Client.AI.DebugFormat("Damn, no Science System of Empire found, possible colonies = {0}", possibleSystems.Count());
                result = null;
                return false;
            }

            possibleSystems.Sort((a, b) =>
                (GetValue_Science(a, _fleet.Owner) * HomeSystemDistanceModifier(_fleet, a.Sector))
                .CompareTo(GetValue_Science(b, _fleet.Owner) * HomeSystemDistanceModifier(_fleet, b.Sector)));
            result = possibleSystems[possibleSystems.Count - 1];
            //  GameLog.Client.AI.DebugFormat("Yippy, Science System found!, possible  = {0}", possibleSystems.FirstOrDefault().Name);
            return true;
        }

        /// <summary>
        /// Provides a modifier to prioritise targets that are closer the home system
        /// of the <see cref="Civilization"/> that owns the <see cref="Fleet"/>
        /// </summary>
        /// <param name="_fleet"></param>
        /// <param name="_targetSector"></param>
        /// <returns></returns>
        public static float HomeSystemDistanceModifier(Fleet _fleet, Sector _targetSector)
        {
            GetFleetOwner(_fleet);
            if (_fleet == null)
            {
                throw new ArgumentNullException(nameof(_fleet));
            }

            if (_targetSector == null)
            {
                throw new ArgumentNullException(nameof(_targetSector));
            }

            int distance = MapLocation.GetDistance(_targetSector.Location, GameContext.Current.CivilizationManagers[_fleet.Owner].HomeSystem.Location);
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

            //after loading spiedCivs == null due to not loaded yet ( ToDo )
            if (spiedCivs != null && spiedCivs.Contains(civSpying))
            {
                return true;
            }

            return false;

        }

        public static List<Sector> FindStrandedShipSectors(Civilization civ)
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
        private static void GetFleetOwner(Fleet _fleet)
        {
            foreach (Ship _ship in _fleet.Ships)
            {
                if (_fleet.Owner != null)
                {
                    break;
                }

                if (_ship.Owner != null)
                {
                    _fleet.Owner = _ship.Owner;
                    _fleet.OwnerID = _ship.OwnerID;
                    break;
                }
            }
        }

    }


}
