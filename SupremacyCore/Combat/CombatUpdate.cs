// CombatUpdate.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;

using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Universe;
using Supremacy.Utility;

namespace Supremacy.Combat
{
    [Serializable]
    public class CombatUpdate
    {
        private int _combatId;
        private int _roundNumber;
        private int _ownerId;
        protected Dictionary<string, int> _combatPartyStrengths; // string in key of civ and int is total fire power of civ
        private bool _standoff;
        private MapLocation _location;
        private IList<CombatAssets> _friendlyAssets;
        private IList<CombatAssets> _hostileAssets;

        private int _friendlyEmpireStrength;
        private int _hostileEmpireStrength;

        public CombatUpdate(int combatId, int roundNumber, bool standoff, Civilization owner, MapLocation location, IList<CombatAssets> friendlyAssets, IList<CombatAssets> hostileAssets)
        {
            if (owner == null)
                throw new ArgumentNullException("owner"); 
            if (friendlyAssets == null)
                throw new ArgumentNullException("friendlyAssets");
            if (hostileAssets == null)
                throw new ArgumentNullException("hostileAssets");
            _combatId = combatId;
            _roundNumber = roundNumber;
            _standoff = standoff;
            _ownerId = owner.CivID;
            _location = location;
            _friendlyAssets = friendlyAssets;
            _hostileAssets = hostileAssets;
        }

        public int FriendlyEmpireStrength
        {
            get
            {
                //_friendlyEmpireStrength = _combatPartyStrengths[Owner.Key.ToString()];
                //GameLog.Core.General.DebugFormat("_friendlyEmpireStrength = {0}", _friendlyEmpireStrength);
                //_friendlyEmpireStrength = _combatPartyStrengths[Owner.Key];
                //GameLog.Core.General.DebugFormat("_friendlyEmpireStrength = {0}", _friendlyEmpireStrength);
                return _friendlyEmpireStrength + 1;
                //return _friendlyEmpireStrength;
            }
            set
            {
                _friendlyEmpireStrength = 123;
                //_friendlyEmpireStrength = value;
            }
        }

        public int HostileEmpireStrength
        {
            get
            {
                GameLog.Core.General.DebugFormat("_hostileEmpireStrength = {0}", _hostileEmpireStrength);
                return _hostileEmpireStrength + 2;
                //return _hostileEmpireStrength;
            }
            set
            {
                _hostileEmpireStrength = 123;
                //_hostileEmpireStrength = value;
            }
        }

        public int CombatID
        {
            get { return _combatId; }
        }

        public int RoundNumber
        {
            get { return _roundNumber; }
        }

        public int OwnerID
        {
            get { return _ownerId; }
        }

        public MapLocation Location
        {
            get { return _location; }
        }

        public Sector Sector
        {
            get { return GameContext.Current.Universe.Map[Location]; }
        }

        public Civilization Owner
        {
            get { return GameContext.Current.Civilizations[_ownerId]; }
        }

        public Dictionary<string, int> CombatPartyStrengths
        {
            get
            {
                //foreach (var empire in _combatPartyStrengths)
                //{
                //    GameLog.Core.General.DebugFormat("EmpireStrength {1} for {0}", empire.Key, empire.Value);
                //}

                return _combatPartyStrengths;
            }
        }

        public IList<CombatAssets> FriendlyAssets
        {
            get { return _friendlyAssets; }
        }

        public IList<CombatAssets> HostileAssets
        {
            get { return _hostileAssets; }
        }

        public bool IsStandoff
        {
            get { return _standoff; }
        }

        public bool IsCombatOver
        {
            get
            {
                if (_standoff)
                    return true;

                int friendlyAssets = 0;
                int hostileAssets = 0;
                
                foreach (CombatAssets assets in FriendlyAssets)
                {
                    if (assets.HasSurvivingAssets)
                    {
                        //GameLog.Core.Combat.DebugFormat("Combat: friendlyAssets(assets.CombatShips.Count)={0}", assets.CombatShips.Count);
                        friendlyAssets++;
                    }
                }

                //if (_combatUpdateTraceLocally == true)
                //GameLog.Print("Combat: friendlyAssets(Amount)={0}", friendlyAssets);

                if (friendlyAssets == 0)
                {
                    GameLog.Core.Combat.DebugFormat("Combat: friendlyAssets (number of involved entities)={0}", friendlyAssets);
                    return true;
                }

                foreach (CombatAssets assets in HostileAssets)
                {
                    if (assets.HasSurvivingAssets)
                    {
                        //GameLog.Core.Combat.DebugFormat("Combat: hostileAssets(assets.CombatShips.Count)={0}", assets.CombatShips.Count);
                        hostileAssets++;
                    }
                }

                if (hostileAssets == 0)
                {
                    //GameLog.Core.Combat.DebugFormat("Combat: hostileAssets (number of involved entities)={0}", hostileAssets);
                    return true;
                }

                return (hostileAssets == 0);
            }
        }
    }
}
