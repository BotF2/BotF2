// UniverseObjectView.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Entities;
using Supremacy.Game;

namespace Supremacy.Universe
{
    public enum UniverseObjectVisibility : ushort
    {
        None = 0x0000,
        ExistenceKnown = 0x0001,
        OwnerKnown = 0x0003,
        TypeKnown = 0x0007,
        ChildrenKnown = 0x000F,
        ChildTypesKnown = 0x001F,
        DetailsKnown = 0x003F
    }

    [Serializable]
    public abstract class UniverseObjectView
    {
        public const int UnknownOwnerID = -2;

        private readonly int _targetId;
        private int _ownerId = UnknownOwnerID;
        private string _name;

        protected UniverseObjectView(UniverseObject target)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            _targetId = target.ObjectID;
            _name = target.Name;
            Location = target.Location;
        }

        public int TargetID
        {
            get { return _targetId; }
        }

        public int OwnerID
        {
            get { return _ownerId; }
            set { _ownerId = value; }
        }

        public bool OwnerKnown
        {
            get { return (_ownerId != UnknownOwnerID); }
        }

        public UniverseObject Target
        {
            get { return GameContext.Current.Universe.Objects[_targetId]; }
        }

        public Civilization Owner
        {
            get
            {
                if (!OwnerKnown || (OwnerID == Civilization.InvalidID))
                    return null;
                return GameContext.Current.Civilizations[_ownerId];
            }
        }

        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public MapLocation Location { get; set; }

        public Sector Sector
        {
            get { return GameContext.Current.Universe.Map[Location]; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                Location = value.Location;
            }
        }

        public override string ToString()
        {
            return Name ?? "Unknown Object " + _targetId;
        }
    }
}
