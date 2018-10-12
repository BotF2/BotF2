// Ship.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Game;
using Supremacy.IO.Serialization;
using Supremacy.Types;
using Supremacy.Universe;

namespace Supremacy.Orbitals
{
    /// <summary>
    /// Represents a ship in the game.
    /// </summary>
    [Serializable]
    public class Ship : Orbital
    {
        #region Fields
        private int _fleetId;
        private byte _speed;
        private byte _range;
        private byte _cloakStrength;
        private bool _isCloaked;
        private byte _camouflagedStrength;
        private bool _isCamouflaged;
        private bool _isAssimilated;
        private byte _scanStrength;
        private Meter _fuelReserve;
        private ShipType _shipType;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the type of the UniverseObject.
        /// </summary>
        /// <value>The type of the UniverseObject.</value>
        public sealed override UniverseObjectType ObjectType
        {
            get { return UniverseObjectType.Ship; }
        }

        /// <summary>
        /// Gets or sets the name of this <see cref="Ship"/>.
        /// </summary>
        /// <value>The name.</value>
        public override string Name
        {
            get { return base.Name; }
            set { base.Name = value; }
        }

        /// <summary>
        /// Returns the name of the design
        /// eg "FED_SCOUT_III"
        /// </summary>
        public string DesignName
        {
            get { return ShipDesign.Name; }
        }

        /// <summary>
        /// Returns the name of the design
        /// eg "FED_SCOUT_III"
        /// </summary>
        public string ClassName
        {
            get { return ShipDesign.ClassName + " Class"; }
        }

        /// <summary>
        /// Returns the class and level  of the design
        /// eg "Cruiser II"
        /// </summary>
        //public string ClassLevel 
        //{
        //    get { return ShipDesign.ClassLevel; }
        //}

        /// <summary>
        /// Gets the fuel reserve.
        /// </summary>
        /// <value>The fuel reserve.</value>
        public Meter FuelReserve
        {
            get { return _fuelReserve; }
        }

        public override bool IsMobile
        {
            get { return (_speed > 0); }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Ship"/> is stranded.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="Ship"/> is stranded; otherwise, <c>false</c>.
        /// </value>
        public bool IsStranded
        {
            get { return _fuelReserve.IsMinimized; }
        }

        /// <summary>
        /// Gets or sets the speed.
        /// </summary>
        /// <value>The speed.</value>
        public int Speed
        {
            get
            {
                if ((Fleet != null) && (Fleet.IsInTow || (Fleet.Order is TowOrder)))
                    return 1;
                return _speed;
            }
            set
            {
                _speed = (byte)value;
                OnPropertyChanged("Speed");
            }
        }

        /// <summary>
        /// Gets or sets the range.
        /// </summary>
        /// <value>The range.</value>
        public int Range
        {
            get { return _range; }
            set
            {
                _range = (byte)value;
                OnPropertyChanged("Speed");
            }
        }

        /// <summary>
        /// Gets the type of the ship.
        /// </summary>
        /// <value>The type of the ship.</value>
        public ShipType ShipType
        {
            get { return _shipType; }
        }

        /// <summary>
        /// Gets or sets the ship design.
        /// </summary>
        /// <value>The ship design.</value>
        public ShipDesign ShipDesign
        {
            get { return Design as ShipDesign; }
            set
            {
                Design = value;
                OnPropertyChanged("ShipDesign");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Ship"/> is cloaked.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="Ship"/> is cloaked; otherwise, <c>false</c>.
        /// </value>
        public bool IsCloaked
        {
            get { return _isCloaked; }
            set
            {
                if (CanCloak)
                {
                    _isCloaked = value;
                    OnPropertyChanged("IsCloaked");
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Ship"/> can cloak.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this <see cref="Ship"/> can cloak; otherwise, <c>false</c>.
        /// </value>
        public bool CanCloak
        {
            get { return (ShipDesign.CloakStrength > 0); }
        }

        /// <summary>
        /// Gets or sets the cloak strength.
        /// </summary>
        /// <value>The cloak strength.</value>
        public int CloakStrength
        {
            get { return _cloakStrength; }
            set
            {
                _cloakStrength = (byte)value;
                OnPropertyChanged("CloakStrength");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Ship"/> is camouflaged.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="Ship"/> is camouflaged; otherwise, <c>false</c>.
        /// </value>
        public bool IsCamouflaged
        {
            get { return _isCamouflaged; }
            set
            {
                if (CanCamouflage)
                {
                    _isCamouflaged = value;
                    OnPropertyChanged("IsCamouflaged");
                }
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Ship"/> is assimilated.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="Ship"/> is assimilated otherwise, <c>false</c>.
        /// </value>
        public bool IsAssimilated
        {
            get { return _isAssimilated; }
            set
            {
                _isAssimilated = value;
                OnPropertyChanged("IsAssimilated");
            }
        }
        /// <summary>
        /// Gets a value indicating whether this <see cref="Ship"/> can camouflage.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this <see cref="Ship"/> can camouflage; otherwise, <c>false</c>.
        /// </value>
        public bool CanCamouflage
        {
            get { return (ShipDesign.CamouflagedStrength > 0); }
        }

        /// <summary>
        /// Gets or sets the camouflage strength.
        /// </summary>
        /// <value>The camouflage strength.</value>
        public int CamouflagedStrength
        {
            get { return _camouflagedStrength; }
            set
            {
                _camouflagedStrength = (byte)value;
                OnPropertyChanged("CamouflagedStrength");
            }
        }
        /// <summary>
        /// Gets or sets the fleet to which this <see cref="Ship"/> is attached.
        /// </summary>
        /// <value>The fleet to which this <see cref="Ship"/> is attached.</value>
        public Fleet Fleet
        {
            get { return GameContext.Current.Universe.Objects[_fleetId] as Fleet; }
            set
            {
                var oldFleet = Fleet;
                if ((oldFleet != null) && oldFleet.AreShipsLocked)
                {
                    return;
                }
                if (value != null)
                {
                    if (value.AreShipsLocked)
                    {
                        return;
                    }
                    if (value.ObjectID != _fleetId)
                    {
                        _fleetId = value.ObjectID;
                        value.AddShip(this);
                        OnPropertyChanged("Fleet");
                    }
                }
                else
                {
                    _fleetId = GameObjectID.InvalidID;
                    OnPropertyChanged("Fleet");
                }
                if ((oldFleet != null) && (value != oldFleet))
                {
                    if (oldFleet.Ships.Contains(this))
                        oldFleet.RemoveShip(this);
                    if (oldFleet.Ships.Count == 0)
                        GameContext.Current.Universe.Destroy(oldFleet);
                }
            }
        }
        #endregion

        public Ship() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="Ship"/> class.
        /// </summary>
        /// <param name="design">The design.</param>
        public Ship(ShipDesign design)
            : base(design)
        {
            _shipType = design.ShipType;
            _fuelReserve = new Meter(design.FuelCapacity, 0, design.FuelCapacity);
            _fuelReserve.CurrentValueChanged += FuelReserve_CurrentValueChanged;
            //_cloakStrength = design.CloakStrength;
            //_camouflagedStrength = design.CamouflagedStrength;
        }

        /// <summary>
        /// Handles the CurrentValueChanged event of the FuelReserve control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MeterChangedEventArgs"/> instance containing the event data.</param>
        private void FuelReserve_CurrentValueChanged(object sender, MeterChangedEventArgs e)
        {
            if (!e.Cancel)
            {
                if (_fuelReserve.IsMinimized)
                {
                    OnPropertyChanged("IsStranded");
                    //IsCloaked = false;
                }
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Creates a new <see cref="Fleet"/> containing only this <see cref="Ship"/>.
        /// </summary>
        /// <returns>The new <see cref="Fleet"/>.</returns>
        public Fleet CreateFleet()
        {
            return CreateFleet(GameContext.Current.GenerateID());
        }

        /// <summary>
        /// Creates a new <see cref="Fleet"/> containing only this <see cref="Ship"/>.
        /// </summary>
        /// <param name="fleetId">The object ID of the new <see cref="Fleet"/>.</param>
        /// <returns>The new <see cref="Fleet"/>.</returns>
        public Fleet CreateFleet(int fleetId)
        {
            if ((Fleet == null) || !Fleet.AreShipsLocked)
            {
                var newFleet = new Fleet(fleetId)
                               {
                                   OwnerID = OwnerID,
                                   Location = Location
                               };
                GameContext.Current.Universe.Objects.Add(newFleet);
                Fleet = newFleet;
                //newFleet.Order = newFleet.GetDefaultOrder();
                return newFleet;
            }
            return null;
        }

        /// <summary>
        /// Resets this <see cref="Ship"/> at the end of each game turn.
        /// If there are any fields or properties of this <see cref="Ship"/>
        /// that should be reset or modified at the end of each turn, perform
        /// those operations here.
        /// </summary>
        protected internal override void Reset()
        {
            base.Reset();
            var design = ShipDesign;
            if (design != null)
            {
                _speed = (byte)design.Speed;
                _range = (byte)design.Range;
                _cloakStrength = (byte)design.CloakStrength;
                _camouflagedStrength = (byte)design.CamouflagedStrength;
                _scanStrength = (byte)design.ScanStrength;
            }
            _fuelReserve.UpdateAndReset();
        }

		public override void SerializeOwnedData(SerializationWriter writer, object context)
		{
			base.SerializeOwnedData(writer, context);
			//writer.Write(_cloakStrength);
            //writer.Write(_camouflagedStrength);
            writer.Write(_fleetId);
			writer.WriteObject(_fuelReserve);
			writer.Write(_isCloaked);
            writer.Write(_isCamouflaged);
            writer.Write(_isAssimilated);
            writer.Write(_range);
			writer.Write(_speed);
            writer.Write((byte)_shipType);
		}

		public override void DeserializeOwnedData(SerializationReader reader, object context)
		{
			base.DeserializeOwnedData(reader, context);
			//_cloakStrength = reader.ReadByte();
            //_camouflagedStrength = reader.ReadByte();
            _fleetId = reader.ReadInt32();
			_fuelReserve = (Meter)reader.ReadObject();
			_isCloaked = reader.ReadBoolean();
            _isCamouflaged = reader.ReadBoolean();
            _isAssimilated = reader.ReadBoolean();
            _range = reader.ReadByte();
			_speed = reader.ReadByte();
            _shipType = (ShipType)reader.ReadByte();
		}
    }
}
