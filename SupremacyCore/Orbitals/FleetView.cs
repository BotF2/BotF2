// FleetView.cs
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

using Supremacy.Collections;
using Supremacy.Entities;
using Supremacy.Game;
using System.Windows.Media.Imaging;
using Supremacy.Diplomacy;
using Supremacy.Utility;
using Supremacy.Resources;

namespace Supremacy.Orbitals
{
    public class FleetViewWrapper
    {
        public FleetViewWrapper(FleetView fleet)
        {
            View = fleet;
        }

        public FleetView View { get; private set; }

        public BitmapImage InsigniaImage { get; set; }

        public bool IsUnknown { get; set; }

        public bool IsUnScannable { get; set; }

        public string Name
        {
            get
            {
                if (IsUnknown)
                    //return "unknown";
                    return string.Format(ResourceManager.GetString("UNKNOWN"));
                if (IsUnScannable)
                    //return "scan blocked";
                    return string.Format(ResourceManager.GetString("SCAN_BLOCKED"));
                else
                    return View.Name;
            }
        }

        public string ClassName
        {
            get
            {
                if (IsUnknown)
                    // return "unknown";
                return string.Format(ResourceManager.GetString("UNKNOWN"));
                if (IsUnScannable)
                    return string.Format(ResourceManager.GetString("SCAN_BLOCKED"));
                    //return "scan blocked";
                else
                    return View.ClassName;
            }
        }
    }

    [Serializable]
    public sealed class FleetView : IEquatable<FleetView>
    {
        #region Fields
        private bool _isOwned;
        private bool _isOwnerKnown;
        private bool _isPresenceKnown;
        private ArrayWrapper<ShipView> _ships;
        private int _sourceId;
        #endregion

        #region Constructors and Finalizers
        private FleetView() { }
        #endregion

        #region Properties and Indexers
        public bool IsDesignOfShipsKnown => _ships.All(ship => ship.IsDesignKnown);

        public bool IsNumberOfShipsKnown => _ships.All(ship => ship.IsPresenceKnown);

        public bool IsOwned => _isOwned;

        public bool IsOwnerKnown => _isOwnerKnown;

        public bool IsPresenceKnown => _isPresenceKnown;

        public string Name
        {
            get
            {
                if (IsOwned || IsDesignOfShipsKnown)
                    return Source.Name;
                if (IsNumberOfShipsKnown)
                    return _ships.Count + " in Nebula, Cloaked or Camouflaged";
                if (IsOwnerKnown)
                {
                    string ownerName = Source.Owner.ShortName;
                    if (ownerName.EndsWith("s"))
                        ownerName = ownerName.Substring(0, ownerName.Length - 1);
                    return ownerName + " Fleet";
                }
                return string.Format(ResourceManager.GetString("UNKNOWN_FLEET")); //"Unknown Fleet";
            }
        }

        public string ClassName
        {
            get
            {
                if (IsOwned || IsDesignOfShipsKnown)
                {
                    try
                    {
                        return Source.ClassName;
                    }
                    catch (Exception e)
                    {
                        GameLog.Core.General.Error(string.Format("Problem with null ClassName ObjectID = {0} Name = {1}",
                            Source.ObjectID, Source.Name),
                            e);
                    }
                }
                return "Class Name Unknown";
            }
        }

        public IIndexedCollection<ShipView> Ships => _ships;

        public Fleet Source => GameContext.Current.Universe.Get<Fleet>(_sourceId);
        #endregion

        #region Public and Protected Methods
        public static FleetView Create(Civilization owner, Fleet fleet)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");
            if (fleet == null)
                throw new ArgumentNullException("fleet");

            List<ShipView> ships = new List<ShipView>();
            FleetView fleetView = new FleetView
                {
                    _sourceId = fleet.ObjectID,
                    _isOwned = (fleet.OwnerID == owner.CivID)
                };

            CivilizationManager civManager = GameContext.Current.CivilizationManagers[owner];

            foreach (Ship ship in fleet.Ships)
            {
                //A player always know their own ships
                if (fleet.OwnerID == owner.CivID)
                {
                    // works    GameLog.Print("add a ship to it's fleetview ID = {1} ship.Name = {0}", ship.Name, ship.ObjectID);
                    ships.Add(new ShipView(
                        ship,
                        true,
                        true,
                        fleetView._isOwned));
                    continue;
                    //break;    // break leads to only one ship (shown) inside the fleet
                }

                //If we've got this far, it's not the players ship
                int scanStrength = civManager.MapData.GetScanStrength(fleet.Location);
                bool isPresenceKnown = false;
                bool isDesignKnown = false;
                int netScanStrength = 0;

                //Work out the net scan strenth for this ship
                if (ship.IsCloaked)
                {
                    netScanStrength = scanStrength - ship.CloakStrength;
                }
                else if (ship.IsCamouflaged)
                {
                    netScanStrength = scanStrength - ship.OrbitalDesign.CamouflagedStrength; // ship.CamouflagedStrength;
                }
                else
                {
                    netScanStrength = scanStrength;
                }

                if (netScanStrength >= 0)
                {
                    isPresenceKnown = true;
                }
                if (netScanStrength >= 1)
                {
                    isDesignKnown = (DiplomacyHelper.IsContactMade(owner, fleet.Owner) || DiplomacyHelper.IsScanBlocked(owner, fleet.Sector));
                    //if(DiplomacyHelper.IsScanBlocked(owner, fleet.Sector) == true)
                    //GameLog.Client.Intel.DebugFormat("scanblocking = {0}, Contact ={1} isDesignKnown ={2}",
                    //    DiplomacyHelper.IsScanBlocked(owner, fleet.Sector), (DiplomacyHelper.IsContactMade(owner, fleet.Owner) || DiplomacyHelper.IsScanBlocked(owner, fleet.Sector)));
                }

                ships.Add(new ShipView(
                    ship,
                    isPresenceKnown,
                    isDesignKnown,
                    fleetView._isOwned));

            }

            fleetView._isPresenceKnown = ships.All(ship => ship.IsPresenceKnown);
            fleetView._isOwnerKnown = ships.All(ship => ship.IsDesignKnown);

            fleetView._ships = new ArrayWrapper<ShipView>(ships.ToArray());

            return fleetView;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as FleetView);
        }

        public override int GetHashCode()
        {
            return _sourceId;
        }
        #endregion

        #region Implementation of IEquatable<FleetView>
        public bool Equals(FleetView other)
        {
            if (ReferenceEquals(other, null))
                return false;
            return (other._sourceId == _sourceId);
        }
        #endregion
    }

    [Serializable]
    public sealed class ShipView : IEquatable<ShipView>
    {
        #region Fields
        private readonly bool _isDesignKnown;
        private readonly bool _isOwned;
        private readonly bool _isPresenceKnown;
        private readonly int _sourceId;
        #endregion

        #region Constructors and Finalizers
        public ShipView(
            // ReSharper disable SuggestBaseTypeForParameter
            Ship source,
            // ReSharper restore SuggestBaseTypeForParameter
            bool isPresenceKnown,
            bool isDesignKnown,
            bool isOwned)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            _sourceId = source.ObjectID;
            _isPresenceKnown = isPresenceKnown;
            _isDesignKnown = isDesignKnown;
            _isOwned = isOwned;
        }
        #endregion

        #region Properties and Indexers
        public bool IsDesignKnown => _isDesignKnown;

        public bool IsOwned => _isOwned;

        public bool IsPresenceKnown => _isPresenceKnown;

        public Ship Source => GameContext.Current.Universe.Get<Ship>(_sourceId);
        #endregion

        #region Public and Protected Methods
        public override bool Equals(object obj)
        {
            return Equals(obj as ShipView);
        }

        public override int GetHashCode()
        {
            return _sourceId;
        }
        #endregion

        #region Implementation of IEquatable<ShipView>
        public bool Equals(ShipView other)
        {
            if (ReferenceEquals(other, null))
                return false;
            return (other._sourceId == _sourceId);
        }
        #endregion
    }
}