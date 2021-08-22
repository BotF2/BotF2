// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Effects;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.IO.Serialization;
using Supremacy.Resources;
using Supremacy.Types;
using Supremacy.Utility;
using System;
using System.Diagnostics;

namespace Supremacy.Universe
{
    /// <summary>
    /// Base class for all objects that exist physically in the game universe.
    /// Includes information about name, ownership, and location.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{ToString() + \" (ID=\" + ObjectID + \")\"}")]
    public abstract class UniverseObject :
        DynamicObject,
        IUniverseObject,
        IEffectTarget,
        IEffectTargetInternal
    {
        private Lazy<EffectBindingCollection> _effectBindings = new Lazy<EffectBindingCollection>();
        private MapLocation _location;
        private short _ownerId = (short)Civilization.InvalidID;
        private string _name;
        private int _turnCreated;
        private int _lastOwnershipChange;
        [field: NonSerialized]
        public event EventHandler LocationChanged;
        [field: NonSerialized]
        public event EventHandler OwnerIDChanged;

        public override void CloneFrom(Cloneable source, ICloneContext context)
        {
            UniverseObject typedSource = (UniverseObject)source;

            base.CloneFrom(source, context);

            _location = typedSource._location;
            _ownerId = typedSource._ownerId;
            _name = typedSource._name;
            _turnCreated = typedSource._turnCreated;
        }

        /// <summary>
        /// Gets or sets the name of this <see cref="UniverseObject"/>.
        /// </summary>
        /// <value>The name.</value>
        public virtual string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Gets or sets the owner ID of this <see cref="UniverseObject"/>.  This should be the
        /// CivID property of the owner Civilization.
        /// </summary>
        /// <value>The owner ID.</value>
        [Indexable]
        public virtual int OwnerID
        {
            get => _ownerId;
            set
            {
                if (value < 0)
                {
                    value = Civilization.InvalidID;
                }

                _ownerId = (short)value;
                OnOwnerIDChanged();
                OnPropertyChanged("OwnerID");
            }
        }

        /// <summary>
        /// Gets whether or not this <see cref="UniverseObject"/> is owned.
        /// </summary>
        public bool IsOwned => _ownerId != Civilization.InvalidID;

        /// <summary>
        /// Gets or sets the owner of this <see cref="UniverseObject"/>.
        /// </summary>
        /// <value>The owner.</value>
        public Civilization Owner
        {
            get
            {
                if (OwnerID == Civilization.InvalidID)
                {
                    return null;
                }

                return GameContext.Current.Civilizations[OwnerID];
            }
            set
            {
                if (value == Owner)
                {
                    return;
                }

                OwnerID = (value != null)
                    ? value.CivID
                    : Civilization.InvalidID;

                _lastOwnershipChange = GameContext.Current.TurnNumber;

                OnPropertyChanged("Owner");
            }
        }

        /// <summary>
        /// Gets the turn number on which this <see cref="UniverseObject"/> was created.
        /// </summary>
        /// <value>The turn created.</value>
        public int TurnCreated => _turnCreated;

        /// <summary>
        /// Gets the turn number on which ownership of this <see cref="Colony"/> last changed.
        /// </summary>
        /// <value>The turn created.</value>
        public int LastOwnershipChange => _lastOwnershipChange;

        public int Age => GameContext.Current.TurnNumber - _turnCreated;

        /// <summary>
        /// Gets the galactic sector in which this <see cref="UniverseObject"/> resides.
        /// </summary>
        /// <value>The sector.</value>
        public Sector Sector => GameContext.Current.Universe.Map[Location];

        /// <summary>
        /// Gets the galactic Quadrant in which this <see cref="UniverseObject"/> resides.
        /// </summary>
        public Quadrant Quadrant
        {
            get
            {
                SectorMap map = GameContext.Current.Universe.Map;
                if (Location.X < map.Width / 2)
                {
                    if (Location.Y < map.Height / 2)
                    {
                        return Quadrant.Gamma;
                    }

                    return Quadrant.Alpha;
                }
                if (Location.Y < map.Height / 2)
                {
                    return Quadrant.Delta;
                }

                return Quadrant.Beta;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="UniverseObject"/> can move.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="UniverseObject"/> can move; otherwise, <c>false</c>.
        /// </value>
        public virtual bool CanMove => false;

        /// <summary>
        /// Gets the type of the UniverseObject.
        /// </summary>
        /// <value>The type of the UniverseObject.</value>
        [Indexable]
        public virtual UniverseObjectType ObjectType => UniverseObjectType.Unknown;

        /// <summary>
        /// Initializes a new instance of the <see cref="UniverseObject"/> class.
        /// </summary>
        protected UniverseObject()
        {
            _turnCreated = GameContext.Current.TurnNumber;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UniverseObject"/> class
        /// </summary>
        /// <param name="objectId">Unique ID within scope of GameContext</param>
        protected UniverseObject(int objectId) : base(objectId)
        {
            //GameLog.Core.General.DebugFormat("TurnNumber = _turnCreated ={0}, objectID ={1}",TurnCreated, objectId);
            _turnCreated = GameContext.Current.TurnNumber;
        }

        /// <summary>
        /// Resets this <see cref="UniverseObject"/> at the end of each game turn.
        /// 
        /// If there are any fields or properties of this <see cref="UniverseObject"/>
        /// that should be reset or modified at the end of each turn, perform
        /// those operations here.
        /// </summary>
        protected internal virtual void Reset() { }

        /// <summary>
        /// Called when the location of this <see cref="UniverseObject"/> changes.
        /// </summary>
        protected virtual void OnLocationChanged()
        {
            LocationChanged?.Invoke(this, EventArgs.Empty);
        }

        protected void OnOwnerIDChanged()
        {
            OwnerIDChanged?.Invoke(this, EventArgs.Empty);
        }

        [Indexable]
        public MapLocation Location
        {
            get => _location;
            set
            {
                _location = value;
                OnLocationChanged();
                OnPropertyChanged("Location");
            }
        }

        public int DistanceTo([NotNull] UniverseObject other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            return MapLocation.GetDistance(Location, other.Location);
        }

        public void Destroy()
        {
            _ = GameContext.Current.Universe.Destroy(this);
        }

        #region Overridden Members
        public override void DeserializeOwnedData(SerializationReader reader, object context)
        {
            base.DeserializeOwnedData(reader, context);

            _location = new MapLocation(reader.ReadByte(), reader.ReadByte());
            _name = reader.ReadOptimizedString();
            _ownerId = (short)(reader.ReadOptimizedInt16() - 1);
            _turnCreated = reader.ReadOptimizedUInt16();

            DeserializeEffectData(reader, context);
        }

        public override void SerializeOwnedData(SerializationWriter writer, object context)
        {
            base.SerializeOwnedData(writer, context);

            writer.Write((byte)_location.X);
            writer.Write((byte)_location.Y);
            writer.WriteOptimized(_name);
            writer.WriteOptimized(_ownerId + 1);
            writer.WriteOptimized((ushort)_turnCreated);

            SerializeEffectData(writer, context);
        }

        protected internal override void OnDeserialized()
        {
            if (_effectBindings == null)
            {
                _effectBindings = new Lazy<EffectBindingCollection>();
            }

            base.OnDeserialized();
        }

        private void SerializeEffectData([NotNull] SerializationWriter writer, object context)
        {
            bool hasEffectBindings = _effectBindings != null &&
                                    _effectBindings.IsValueCreated;

            writer.Write(hasEffectBindings);

            if (hasEffectBindings)
            {
                _effectBindings.Value.SerializeOwnedData(writer, context);
            }
        }

        private void DeserializeEffectData([NotNull] SerializationReader reader, object context)
        {
            bool hasEffectBindings = reader.ReadBoolean();

            if (!hasEffectBindings)
            {
                return;
            }

            if (_effectBindings == null)
            {
                _effectBindings = new Lazy<EffectBindingCollection>();
            }

            _effectBindings.Value.DeserializeOwnedData(reader, context);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="UniverseObject"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="UniverseObject"/>.
        /// </returns>
        public override string ToString()
        {
            if (Name != null)
            {
                return ResourceManager.GetString(Name);
            }

            return base.ToString();
        }

        /// <summary>
        /// Serves as a hash function for a particular design.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="UniverseObject"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return ObjectID;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/>
        /// is equal to the current <see cref="UniverseObject"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare
        /// with the current <see cref="UniverseObject"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal
        /// to the current <see cref="UniverseObject"/>; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return Equals(obj as UniverseObject);
        }

        /// <summary>
        /// Determines whether the specified <see cref="UniverseObject"/>
        /// is equal to the current <see cref="UniverseObject"/>.
        /// </summary>
        /// <param name="obj">The <see cref="UniverseObject"/> to compare
        /// with the current <see cref="UniverseObject"/>.</param>
        /// <returns>
        /// true if the specified <see cref="UniverseObject"/> is equal
        /// to the current <see cref="UniverseObject"/>; otherwise, false.
        /// </returns>
        public virtual bool Equals(UniverseObject obj)
        {
            if (obj == null)
            {
                return false;
            }

            return Equals(obj.ObjectID, ObjectID);
        }

        /// <summary>
        /// Equality operator for two <see cref="T:Supremacy.Universe.UniverseObject"/>s
        /// </summary>
        /// <param name="a">First <see cref="T:Supremacy.Universe.UniverseObject"/></param>
        /// <param name="b">Second <see cref="T:Supremacy.Universe.UniverseObject"/></param>
        /// <returns>
        /// <c>true</c> if <see cref="GameObject.ObjectID"/> properties match; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(UniverseObject a, UniverseObject b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.ObjectID == b.ObjectID;
        }

        /// <summary>
        /// Inequality operator for two <see cref="T:Supremacy.Universe.UniverseObject"/>s
        /// </summary>
        /// <param name="a">First <see cref="T:Supremacy.Universe.UniverseObject"/></param>
        /// <param name="b">Second <see cref="T:Supremacy.Universe.UniverseObject"/></param>
        /// <returns>
        /// <c>true</c> if <see cref="GameObject.ObjectID"/> properties do not match; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(UniverseObject a, UniverseObject b)
        {
            return !(a == b);
        }
        #endregion

        #region Implementation of IEffectTarget

        public IEffectBindingCollection EffectBindings
        {
            get
            {
                try
                {
                    return _effectBindings.Value;
                }
                catch (Exception e)
                {
                    // doesn't work - only crashes when using Live Visual Tree directly in Visual Studio - then just click on Continue - use F5 to continue
                    GameLog.Core.General.Error(e);
                    return _effectBindings.Value;
                }
            }
        }

        EffectBindingCollection IEffectTargetInternal.EffectBindingsInternal => _effectBindings.Value;

        #endregion
    }

    public enum UniverseObjectType
    {
        Unknown = 0,
        StarSystem,
        Colony,
        TechObject,
        Building,
        Shipyard,
        Orbital,
        Ship,
        Station,
        DefensePlatform,
        Fleet,
    }

}
