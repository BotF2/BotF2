// PlayerOperations.cs
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

using Microsoft.Practices.ServiceLocation;

using Supremacy.Annotations;
using Supremacy.Diplomacy;
using Supremacy.Economy;
using Supremacy.Orbitals;
using Supremacy.Client;
using Supremacy.Tech;
using Supremacy.Universe;

namespace Supremacy.Game
{
    /// <summary>
    /// Provides a set of common player operations, which includes automatically
    /// submitting any requisite Orders.
    /// </summary>
    public static class PlayerOperations
    {
        private static IClientContext _clientContext;
        private static IPlayerOrderService _playerOrderService;
        private static IGameObjectIDService _gameObjectIDService;

        private static IGameObjectIDService GameObjectIDService
        {
            get
            {
                if (_gameObjectIDService == null)
                    _gameObjectIDService = ServiceLocator.Current.GetInstance<IGameObjectIDService>();
                return _gameObjectIDService;
            }
        }

        // ReSharper disable UnusedMember.Local
        private static IClientContext ClientContext
        {
            get
            {
                if (_clientContext == null)
                    _clientContext = ServiceLocator.Current.GetInstance<IClientContext>();
                return _clientContext;
            }
        }
        // ReSharper restore UnusedMember.Local

        private static IPlayerOrderService PlayerOrderService
        {
            get
            {
                if (_playerOrderService == null)
                    _playerOrderService = ServiceLocator.Current.GetInstance<IPlayerOrderService>();
                return _playerOrderService;
            }
        }

        public static void ActivateShipyardBuildSlot([NotNull] ShipyardBuildSlot buildSlot)
        {
            if (buildSlot == null)
                throw new ArgumentNullException("buildSlot");
            if (buildSlot.IsActive)
                return;
            if (!buildSlot.Shipyard.Sector.System.Colony.ActivateShipyardBuildSlot(buildSlot))
                return;
            PlayerOrderService.AddOrder(new ToggleShipyardBuildSlotOrder(buildSlot));
        }

        public static void DeactivateShipyardBuildSlot([NotNull] ShipyardBuildSlot buildSlot)
        {
            if (buildSlot == null)
                throw new ArgumentNullException("buildSlot");
            if (!buildSlot.IsActive)
                return;
            if (!buildSlot.Shipyard.Sector.System.Colony.DeactivateShipyardBuildSlot(buildSlot))
                return;
            PlayerOrderService.AddOrder(new ToggleShipyardBuildSlotOrder(buildSlot));
        }

        public static void SetFleetRoute(Fleet fleet, TravelRoute route)
        {
            if (fleet==null)
                throw new ArgumentNullException("fleet");
            if (route == null)
                route = TravelRoute.Empty;
            fleet.SetRoute(route);
            PlayerOrderService.AddOrder(new SetFleetRouteOrder(fleet));
        }

        public static void SetFleetOrder(Fleet fleet, FleetOrder order)
        {
            if (fleet == null)
                throw new ArgumentNullException("fleet");
            if (order == null)
                order = fleet.GetDefaultOrder();
            fleet.SetOrder(order);
            PlayerOrderService.AddOrder(new SetFleetOrderOrder(fleet));
        }

        public static void MergeFleets(Fleet sourceFleet, Fleet destinationFleet)
        {
            if (sourceFleet == null)
                throw new ArgumentNullException("sourceFleet");
            if (destinationFleet == null)
                return;
            foreach (Ship ship in new List<Ship>(sourceFleet.Ships))
            {
                destinationFleet.AddShip(ship);
                PlayerOrderService.AddOrder(new RedeployShipOrder(ship, destinationFleet));
            }
        }

        public static void RedeployShip(Ship ship)
        {
            RedeployShipInternal(ship, null);
        }

        public static void RedeployShip(Ship ship, Fleet destinationFleet)
        {
            RedeployShipInternal(ship, destinationFleet);
        }

        private static void RedeployShipInternal(Ship ship, Fleet destinationFleet)
        {
            if (ship == null)
                throw new ArgumentNullException("ship");

            if (destinationFleet != null)
            {
                destinationFleet.AddShip(ship);
            }
            else
            {
                GameObjectID? newFleetId;
                try
                {
                    newFleetId = GameObjectIDService.GetNewObjectID();
                    if (!newFleetId.HasValue)
                        return;
                }
                catch
                {
                    return;
                }
                destinationFleet = ship.CreateFleet(newFleetId.Value);
                PlayerOrderService.AddOrder(new CreateFleetOrder(ship.Owner, newFleetId.Value, ship.Location));
            }
            PlayerOrderService.AddOrder(new RedeployShipOrder(ship, destinationFleet));
        }

        public static void Scrap(bool scrap, params TechObject[] items)
        {
            Scrap(scrap, (IEnumerable<TechObject>)items);
        }

        public static void Scrap(bool scrap, [NotNull] IEnumerable<TechObject> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            foreach (var item in items)
            {
                PlayerOrderService.AddOrder(new ScrapOrder(scrap, item));
                item.Scrap = scrap;
            }
        }
    }
}
