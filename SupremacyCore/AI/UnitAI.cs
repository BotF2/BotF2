// UnitAI.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

using Supremacy.Annotations;
using Supremacy.Diplomacy;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Pathfinding;
using Supremacy.Universe;

using Supremacy.Utility;

namespace Supremacy.AI
{
    public static class UnitAI
    {

        public static bool unitAITrace = false;

        public static void DoTurn([NotNull] Civilization civ)
        {
            if (civ == null)
                throw new ArgumentNullException("civ");

            //if (PlayerContext.Current.IsHumanPlayer(civ))
            //    return;

            foreach (var fleet in GameContext.Current.Universe.FindOwned<Fleet>(civ))
            {
                foreach (var ship in fleet.Ships.Where(ship => ship.CanCloak && !ship.IsCloaked))
                {
                    if (unitAITrace)
                        GameLog.Print("UnitAI: ## ship will be cloaked ## fleet={0}, {1}, {2}, {3}, {4},", fleet.ObjectID, fleet.Name, fleet.Owner, fleet.Order, fleet.Location);

                    ship.IsCloaked = true;
                }

                // works
                if (unitAITrace)
                    GameLog.Print("UnitAI: fleet={0}, {2}, {3}, {4}, {1}", fleet.ObjectID, fleet.Name, fleet.Owner, fleet.Order, fleet.Location);

                if (!fleet.CanMove)
                    continue;

                if (fleet.IsScout)
                {
                    if (fleet.Activity == UnitActivity.NoActivity)
                    {
                        fleet.SetOrder(new ExploreOrder());
                        fleet.UnitAIType = UnitAIType.Explorer;
                        fleet.Activity = UnitActivity.Mission;
                        if (unitAITrace)
                            GameLog.Print("UnitAI: ## IsScout ##  fleet={0}, {1}, {2}, {3}, {4},", fleet.ObjectID, fleet.Name, fleet.Owner, fleet.Order, fleet.Location);
                    }
                }
                if (fleet.IsColonizer)
                {
                    if (fleet.Activity == UnitActivity.NoActivity || fleet.Route.IsEmpty || fleet.Order.IsComplete)
                    {
                        if (CanColonize(civ, fleet.Sector))
                        {
                            fleet.SetOrder(new ColonizeOrder());
                            fleet.UnitAIType = UnitAIType.Colonizer;
                            fleet.Activity = UnitActivity.Mission;
                            if (unitAITrace)
                                GameLog.Print("UnitAI: ## IsColonizer-Colonizing ##  fleet={0}, {1}, {2}, {3}, {4},", fleet.ObjectID, fleet.Name, fleet.Owner, fleet.Order, fleet.Location);
                        }
                        else
                        {
                            StarSystem result;
                            if (GetBestSystemToColonize(civ, fleet.Sector.Location, -1, GetCurrentColonizationTargets(civ).ToList(), fleet, out result))
                            {
                                if (fleet.Sector.System == result)
                                {
                                    fleet.SetOrder(new ColonizeOrder());
                                    fleet.UnitAIType = UnitAIType.Colonizer;
                                    fleet.Activity = UnitActivity.Mission;
                                    if (unitAITrace)
                                        GameLog.Print("UnitAI: ## IsColonizer - AIM found ##  fleet={0}, {1}, {2}, {3}, {4},", fleet.ObjectID, fleet.Name, fleet.Owner, fleet.Order, fleet.Location);
                                }
                                else if (CanEnterSector(result.Location, civ))
                                {
                                    fleet.SetRoute(AStar.FindPath(fleet, PathOptions.SafeTerritory, null, new List<Sector> { result.Sector }));
                                    fleet.UnitAIType = UnitAIType.Colonizer;
                                    fleet.Activity = UnitActivity.Mission;
                                    if (unitAITrace)
                                        GameLog.Print("UnitAI: ## IsColonizer - Move to AIM because the sector there can be entered ##  fleet={0}, {1}, {2}, {3}, {4},", fleet.ObjectID, fleet.Name, fleet.Owner, fleet.Order, fleet.Location);
                                }
                            }
                        }
                    }
                }

                if (fleet.IsConstructor)
                {
                    fleet.UnitAIType = UnitAIType.Constructor;
                    if (FleetOrders.BuildStationOrder.IsValidOrder(fleet) &&
                        fleet.Sector.System.Colony == GameContext.Current.Universe.HomeColonyLookup[civ])
                    {
                        var order = new BuildStationOrder();
                        order.BuildProject = order.FindTargets(fleet).Cast<StationBuildProject>().LastOrDefault(o => o.StationDesign.IsCombatant);
                        if (order.BuildProject != null && order.CanAssignOrder(fleet))
                        {
                            fleet.SetOrder(order);
                            fleet.Activity = UnitActivity.Mission;
                        }
                    }
                    if (unitAITrace)
                        GameLog.Print("UnitAI: ## IsConstructor ##  fleet={0}, {1}, {2}, {3}, {4},", fleet.ObjectID, fleet.Name, fleet.Owner, fleet.Order, fleet.Location);
                }

                if (fleet.IsBattleFleet)
                {
                    if (unitAITrace)
                        GameLog.Print("UnitAI: ## IsBattleFleet ##  fleet={0}, {1}, {2}, {3}, {4},", fleet.ObjectID, fleet.Name, fleet.Owner, fleet.Order, fleet.Location);

                    var defenseFleet = GameContext.Current.Universe.HomeColonyLookup[civ].Sector.GetOwnedFleets(civ).FirstOrDefault(o => o.UnitAIType == UnitAIType.SystemDefense);
                    if (fleet.Activity == UnitActivity.NoActivity && defenseFleet == null)
                    {
                        fleet.UnitAIType = UnitAIType.SystemDefense;
                        fleet.Activity = UnitActivity.Hold;
                        if (unitAITrace)
                            GameLog.Print("UnitAI: ## IsBattleFleet - on SystemDefence ##  fleet={0}, {1}, {2}, {3}, {4},", fleet.ObjectID, fleet.Name, fleet.Owner, fleet.Order, fleet.Location);
                    }
                    else if (fleet.Activity == UnitActivity.NoActivity && fleet.Ships.Count == 1 && fleet.Sector == defenseFleet.Sector)
                    {
                        var ship = fleet.Ships[0];
                        if (unitAITrace)
                            GameLog.Print("UnitAI: ## IsBattleFleet - on SystemDefence - Ship added ##  fleet={0}, {1}, {2}, {3}, {4},", fleet.ObjectID, fleet.Name, fleet.Owner, fleet.Order, fleet.Location);

                        fleet.RemoveShip(ship);
                        defenseFleet.AddShip(ship);
                        //    if (_unitAITrace == true)
                        //        GameLog.Print("UnitAI: ## IsBattleFleet - on SystemDefence - Ship added ##  fleet={0}, {1}, {2}, {3}, {4},", fleet.ObjectID, fleet.Name, fleet.Owner, fleet.Order, fleet.Location);
                    }
                }
            }
        }

        public static int GetAttackOdds(Fleet fleet, Sector sector, bool potentialEnemy)
        {
            int ourStrength;
            int theirStrength = 0;
            int ourFirepower;
            int theirFirepower = 0;
            int baseOdds;
            int strengthFactor;
            int damageToUs;
            int damageToThem;
            int neededRoundsUs;
            int neededRoundsThem;
            int neededRoundsDiff;
            int finalOdds;
            IList<Orbital> defenders = GetDefenders(null, fleet, !potentialEnemy, potentialEnemy, false);

            if (defenders.Count == 0)
                return 100;

            ourStrength = GetCombatStrength(fleet);
            ourFirepower = GetFirepower(fleet);

            if (ourStrength == 0)
                return 1;

            foreach (Orbital defender in defenders)
            {
                theirStrength += GetCombatStrength(defender);
                theirFirepower += GetFirepower(defender);
            }

            baseOdds = (100 * ourStrength) / (ourStrength + theirStrength);

            if (baseOdds == 0)
                return 1;

            strengthFactor = ((ourFirepower + theirFirepower + 1) / 2);

            damageToUs = Math.Max(1, (theirFirepower + strengthFactor) / (ourFirepower + strengthFactor));
            damageToThem = Math.Max(1, (ourFirepower + strengthFactor) / (theirFirepower + strengthFactor));

            neededRoundsUs = (GetTotalHitPoints(defenders) + damageToThem - 1) / damageToThem;
            neededRoundsThem = (GetTotalHitPoints(fleet) + damageToUs - 1) / damageToUs;

            neededRoundsDiff = (neededRoundsUs - neededRoundsThem);
            if (neededRoundsDiff > 0)
            {
                theirStrength *= (1 + neededRoundsDiff);
            }
            else
            {
                ourStrength *= (1 - neededRoundsDiff);
            }

            finalOdds = ((ourStrength * 100) / (ourStrength + theirStrength));
            finalOdds += (((100 - finalOdds) * GetWithdrawalProbability(fleet)) / 100);
            //finalOdds += getAttackOddsChange for player

            return Math.Max(1, Math.Min(finalOdds, 99));
        }

        private static int GetWithdrawalProbability(Fleet fleet)
        {
            if (fleet == null)
                throw new ArgumentNullException("fleet");

            int nonCombatantCount = fleet.Ships.Count(ship => !ship.IsCombatant);
            return (100 * (int)(nonCombatantCount / (double)fleet.Ships.Count));
        }

        public static IList<Orbital> GetDefenders(Civilization owner, Fleet attacker, bool testAtWar, bool testPotentialEnemy, bool testCanMove)
        {
            var defenders = new List<Orbital>();
            foreach (Orbital defender in GameContext.Current.Universe.FindAt<Orbital>(attacker.Location))
            {
                if ((owner == null || defender.Owner == owner) &&
                    IsVisible(defender, owner) &&
                    (!testPotentialEnemy || IsPotentialEnemy(attacker.Owner, defender.Owner)) &&
                    (!testAtWar || IsAtWar(attacker.Owner, defender.Owner)) &&
                    (!testCanMove || defender.CanMove))
                {
                    defenders.Add(defender);
                }
            }
            return defenders;
        }

        public static bool IsAtWar(Civilization source, Civilization target)
        {
            return (DiplomacyHelper.GetForeignPowerStatus(source, target) == ForeignPowerStatus.AtWar);
        }

        public static bool IsPotentialEnemy(Civilization source, Civilization target)
        {
            switch (DiplomacyHelper.GetForeignPowerStatus(source, target))
            {
                case ForeignPowerStatus.AtWar:
                case ForeignPowerStatus.Neutral:
                case ForeignPowerStatus.NoContact:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsVisible(UniverseObject unit, Civilization civ)
        {
            if (unit == null)
                throw new ArgumentNullException("unit");

            if (unit is Fleet)
                return FleetView.Create(civ, (Fleet)unit).IsPresenceKnown;

            try
            {
                CivilizationManager civManager = GameContext.Current.CivilizationManagers[civ];
                if ((civManager != null) && (civManager.MapData != null))
                    return civManager.MapData.IsExplored(unit.Location);
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
            }

            return false;
        }

        public static int GetEffectiveCombatStrength(Fleet fleet)
        {
            return fleet.Ships.Sum(ship => GetEffectiveCombatStrength(ship));
        }

        public static int GetEffectiveCombatStrength(Orbital orbital)
        {
            int effectiveStrength = GetCombatStrength(orbital);
            effectiveStrength *= (orbital.ShieldStrength.Maximum
                + orbital.ShieldStrength.CurrentValue
                + orbital.HullStrength.Maximum
                + orbital.HullStrength.CurrentValue);
            effectiveStrength *= ((2 * orbital.ShieldStrength.Maximum)
                                  + (2 * orbital.HullStrength.Maximum));
            return effectiveStrength;
        }

        public static int GetCombatStrength(Fleet fleet)
        {
            if (fleet == null)
                throw new ArgumentNullException("fleet");

            return fleet.Ships.Sum(ship => GetCombatStrength(ship));
        }

        public static int GetCombatStrength(Orbital orbital)
        {
            if (orbital == null)
                throw new ArgumentNullException("orbital");
            int strength = 0;
            if (orbital.OrbitalDesign.PrimaryWeapon != null)
            {
                strength += (orbital.OrbitalDesign.PrimaryWeapon.Damage
                             * orbital.OrbitalDesign.PrimaryWeapon.Count);
            }
            if (orbital.OrbitalDesign.SecondaryWeapon != null)
            {
                strength += (orbital.OrbitalDesign.SecondaryWeapon.Damage
                             * orbital.OrbitalDesign.SecondaryWeapon.Count);
            }
            strength *= (orbital.ShieldStrength.CurrentValue + orbital.HullStrength.CurrentValue);
            strength /= (orbital.ShieldStrength.Maximum + orbital.HullStrength.Maximum);
            return strength;
        }

        public static int GetFirepower(Fleet fleet)
        {
            if (fleet == null)
                throw new ArgumentNullException("fleet");
            return fleet.Ships.Sum(ship => GetFirepower(ship));
        }

        public static int GetFirepower(Orbital orbital)
        {
            if (orbital == null)
                throw new ArgumentNullException("orbital");
            int firepower = 0;
            if (orbital.OrbitalDesign.PrimaryWeapon != null)
            {
                firepower += (orbital.OrbitalDesign.PrimaryWeapon.Damage
                              * orbital.OrbitalDesign.PrimaryWeapon.Count);
            }
            if (orbital.OrbitalDesign.SecondaryWeapon != null)
            {
                firepower += (orbital.OrbitalDesign.SecondaryWeapon.Damage
                              * orbital.OrbitalDesign.SecondaryWeapon.Count);
            }
            return firepower;
        }

        public static int GetTotalHitPoints(Orbital orbital)
        {
            if (orbital == null)
                throw new ArgumentNullException("orbital");
            return (orbital.HullStrength.CurrentValue + orbital.ShieldStrength.CurrentValue);
        }

        public static int GetTotalHitPoints(IEnumerable<Orbital> orbitals)
        {
            if (orbitals == null)
                throw new ArgumentNullException("orbitals");
            return orbitals.Sum(orbital => GetTotalHitPoints(orbital));
        }

        public static int GetTotalHitPoints(Fleet fleet)
        {
            if (fleet == null)
                throw new ArgumentNullException("fleet");
            return fleet.Ships.Sum(ship => GetTotalHitPoints(ship));
        }

        public static bool GetBestSystemToColonize(Civilization owner, MapLocation origin, int radius, IList<MapLocation> except, Fleet fleet, out StarSystem result)
        {
            var systems = new List<StarSystem>(GameContext.Current.Universe.Find<StarSystem>(UniverseObjectType.StarSystem));
            var rect = radius == -1
                ? new MapRectangle(0, 0, GameContext.Current.Universe.Map.Width, GameContext.Current.Universe.Map.Height)
                : new MapRectangle(origin.X - radius, origin.Y - radius, origin.X + radius, origin.Y + radius);
            for (int i = 0; i < systems.Count; i++)
            {
                if (!systems[i].Location.Intersects(rect) || except.Contains(systems[i].Location) ||
                    !FleetHelper.IsSectorWithinFuelRange(systems[i].Sector, fleet) ||
                    systems[i].IsInhabited || (systems[i].IsOwned && systems[i].Owner != owner))
                {
                    systems.RemoveAt(i--);
                }
            }
            systems.Sort(
                delegate(StarSystem a, StarSystem b)
                {
                    //To do a better solution, at the moment we just did to AVOID a crash for (a == null)
                    if (a == null)
                        return (int)((b.GetMaxPopulation(owner.Race) * b.GetGrowthRate(owner.Race))
                                      - (b.GetMaxPopulation(owner.Race) * b.GetGrowthRate(owner.Race)));
                    foreach (Orbital orbital in GameContext.Current.Universe.FindAt<Orbital>(a.Location))
                    {
                        if (IsPotentialEnemy(owner, orbital.Owner))
                            return -1;
                    }
                    return (int)((a.GetMaxPopulation(owner.Race) * a.GetGrowthRate(owner.Race))
                                  - (b.GetMaxPopulation(owner.Race) * b.GetGrowthRate(owner.Race)));
                });
            if (systems.Count > 0)
            {
                result = systems[systems.Count - 1];
                return true;
            }
            result = null;
            return false;
        }

        public static IEnumerable<MapLocation> GetCurrentColonizationTargets(Civilization civ)
        {
            var fleets = GameContext.Current.Universe.FindOwned<Fleet>(civ).Where(o => o.IsColonizer);
            return fleets.Select(o => o.Route.Waypoints.LastOrDefault());
        }

        public static bool CanColonize(Civilization civ, Sector sector)
        {
            if (sector.System == null)
                return false;
            // sector.Owner was just for avoid crashes
            //if (sector.Owner != null)
            //    return false;
            if (sector.System.HasColony)
                return false;
            if (sector.IsOwned && sector.Owner != civ)
                return false;
            return sector.System.IsHabitable(civ.Race);
        }

        public static bool CanEnterSector(MapLocation location, Civilization civ)
        {
            return CanEnterSector(GameContext.Current.Universe.Map[location], civ);
        }

        public static bool CanEnterSector(Sector sector, Civilization civ)
        {
            if (sector == null)
                throw new ArgumentNullException("sector");
            if (civ == null)
                throw new ArgumentNullException("civ");
            if (!sector.IsOwned)
                return true;
            if (sector.Owner == civ)
                return true;
            return DiplomacyHelper.IsTravelAllowed(civ, sector);
        }

        public static bool IsEnemyUnitVisible(Sector sector, Civilization civ)
        {
            if (sector == null)
                throw new ArgumentNullException("sector");
            return IsEnemyUnitVisible(sector.Location, civ);
        }

        public static bool IsEnemyUnitVisible(MapLocation location, Civilization civ)
        {
            if (civ == null)
                throw new ArgumentNullException("civ");
            return GameContext.Current.Universe.FindAt<Orbital>(location).Any(orbital => IsPotentialEnemy(civ, orbital.Owner));
        }

        public static int GetExploreValue(MapLocation location, Fleet fleet)
        {
            return GetExploreValue(GameContext.Current.Universe.Map[location], fleet);
        }

        public static IEnumerable<Sector> GetSectorsVisibleToFleet(Fleet fleet, Sector sector)
        {
            if (sector == null)
                throw new ArgumentNullException("sector");
            return GetSectorsVisibleToFleet(fleet, sector.Location);
        }

        public static IEnumerable<Sector> GetSectorsVisibleToFleet(Fleet fleet, MapLocation location)
        {
            var sectors = new HashSet<Sector>();
            SectorMap map = GameContext.Current.Universe.Map;
            int startX = Math.Max(0, location.X - fleet.SensorRange);
            int startY = Math.Max(0, location.Y - fleet.SensorRange);
            int endX = Math.Min(map.Width - 1, location.X + fleet.SensorRange);
            int endY = Math.Min(map.Height - 1, location.Y + fleet.SensorRange);
            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    sectors.Add(map[x, y]);
                }
            }
            return sectors;
        }

        public static int GetExploreValue(Sector sector, Fleet fleet)
        {
            if (sector == null)
                throw new ArgumentNullException("sector");
            if (fleet == null)
                throw new ArgumentNullException("fleet");

            if (!fleet.IsOwned)
                return 0;

            int value = 0;
            int extraValue = 0;

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[fleet.Owner];
            CivilizationMapData mapData = civManager.MapData;

            if (!CanEnterSector(sector, fleet.Owner))
                return 0;

            if (!FleetHelper.IsSectorWithinFuelRange(sector, fleet))
                return 0;

            if (!mapData.IsScanned(sector.Location))
                value += 5000;

            if (!mapData.IsExplored(sector.Location))
            {
                if (sector.IsOwned && (sector.Owner != fleet.Owner))
                    value += 2000;
                if (sector.System != null)
                    extraValue += 50000;
            }

            var homeColony = civManager.ControlsHomeSystem && !civManager.IsHomeColonyDestroyed
                                 ? civManager.HomeColony
                                 : civManager.SeatOfGovernment;

            foreach (Sector otherSector in GetSectorsVisibleToFleet(fleet, sector))
            {
                if (!mapData.IsExplored(otherSector.Location))
                {
                    if (!mapData.IsScanned(otherSector.Location))
                    {
                        value += 1000;
                        if (otherSector.IsOwned && (otherSector.Owner != fleet.Owner))
                            value += 2000;
                    }
                    if ((otherSector.System != null) && (otherSector.System.Owner != fleet.Owner))
                    {
                        extraValue += 10000;
                    }
                }
            }

            if (value > 0)
            {
                value += extraValue;
            }

            // Explore the area around the home system first.
            if (civManager.Colonies.Count == 1 &&
                homeColony != null &&
                homeColony.Owner == fleet.Owner)
            {
                var distance = MapLocation.GetDistance(sector.Location, homeColony.Location);
                if (distance > 8)
                    value /= (distance > 16) ? 5 : 3;
            }

            return value;
        }

        public static int GetExploreTurnValue(Fleet fleet)
        {
            if (fleet == null)
                throw new ArgumentNullException("fleet");

            int value = 0;
            int sectorCount = 0;
            CivilizationMapData mapData = GameContext.Current.CivilizationManagers[fleet.Owner].MapData;

            foreach (Sector sector in MapHelper.GetSectorsWithinRadius(fleet.Sector, fleet.SensorRange))
            {
                if (!mapData.IsExplored(sector.Location))
                {
                    value += GetExploreValue(sector, fleet);
                    sectorCount++;
                }
            }

            if (sectorCount > 0)
            {
                value /= ((sectorCount + 2) / 3);
            }

            return value;
        }


        public static TravelRoute GetBestExploreRoute(Fleet fleet)
        {
            CivilizationManager manager = GameContext.Current.CivilizationManagers[fleet.Owner];
            List<Fleet> ownFleets = GameContext.Current.Universe.FindOwned<Fleet>(fleet.Owner).Where(f => f.CanMove && f != fleet && !f.Route.IsEmpty).ToList();
            var possibleRoutes = new List<TravelRoute>();

            //First priority is what stars there are left to explore
            //Get all stars that are in range of the fleet,
            //that aren't 
            var starsToExplore = GameContext.Current.Universe
                .Find<StarSystem>()
                //Where the star is in range of the fleet
                .Where(s => fleet.Range >= manager.MapData.GetFuelRange(s.Location))
                //Where the star isn't explored
                .Where(s => !manager.MapData.IsExplored(s.Location))
                //Where the star has been scanned
                .Where(s => manager.MapData.IsScanned(s.Location))
                //Where no fleets are already heading there
                .Where(s => ownFleets.All(f => f.Route.Waypoints.All(wp => s.Location != wp)))
                //Where we can enter the sector
                .Where(s => CanEnterSector(s.Sector, manager.Civilization))
                .ToList();

            //Calculate all of the possible routes to each star to explore
            //for this fleet
            foreach (var star in starsToExplore)
            {
                var route = AStar.FindPath(fleet, star.Sector);
                if (route.Waypoints.Count > 0)
                {
                    possibleRoutes.Add(route);
                }
            }

            //If we have found possible routes, narrow it down
            if (possibleRoutes.Count > 0)
            {
                //Order by the length of the route
                possibleRoutes = possibleRoutes.OrderBy(r => r.Length).ToList();
                //Return the shortest
                if (unitAITrace)
                    GameLog.Print("Fleet {0} ordered to explore star in sector {1}", fleet.Name, possibleRoutes[0].Waypoints.Last().ToString());
                return possibleRoutes[0];
            }

            if (unitAITrace)
                GameLog.Print("No stars to explore for fleet {0}. Checking for unscanned sectors...", fleet.Name);

            //Second priority is where hasn't been scanned
            var allNotScannedLocations = Enumerable
                .Range(0, GameContext.Current.Universe.Map.Height)
                .SelectMany(y => Enumerable.Range(0, GameContext.Current.Universe.Map.Width)
                .Select(x => new MapLocation(x, y)))
                //Where is within range of the fleet
                .Where(l => fleet.Range >= manager.MapData.GetFuelRange(l))
                //Where we can enter the sector
                .Where(l => CanEnterSector(l, manager.Civilization))
                //That hasn't been scanned
                .Where(l => !manager.MapData.IsScanned(l))
                //Where no fleets are already heading there
                .Where(l => ownFleets.All(f => f.Route.Waypoints.All(wp => l != wp)))
                //Where there are no fleets that will scan it as they go somewhere
                .Where(l => ownFleets.All(f => MapLocation.GetDistance(f.Route.Waypoints.Last(), l) > f.SensorRange))
                .ToList();

            //Find a route for this fleet to each of the target locations
            foreach (var location in allNotScannedLocations) {
                var route = AStar.FindPath(fleet, new Sector(location));
                if (route.Length > 0)
                {
                    possibleRoutes.Add(route);
                }
            }

            //If we have a possible route
            if (possibleRoutes.Count > 0)
            {
                //Order by the length
                possibleRoutes = possibleRoutes.OrderBy(r => r.Length).ToList();
                //Return the shortest
                if (unitAITrace)
                    GameLog.Print("Fleet {0} ordered to explore unscanned sector {1}", fleet.Name, possibleRoutes[0].Waypoints.Last().ToString());
                return possibleRoutes[0];
            }

            //No route can be found for this ship to explore.
            if (unitAITrace)
                GameLog.Print("No unscanned sectors found for fleet {0} to explore. Nothing left to explore", fleet.Name);
            return TravelRoute.Empty;
        }
    }
}
