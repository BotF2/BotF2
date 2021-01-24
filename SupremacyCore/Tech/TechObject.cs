// TechObject.cs
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
using Supremacy.Utility;

namespace Supremacy.Tech
{
    /// <summary>
    /// The base class representing any tech object.
    /// </summary>
    [Serializable]
    public class TechObject : UniverseObject
    {
        private int _designId;
        private bool _scrap;

        /// <summary>
        /// Gets the type of the UniverseObject.
        /// </summary>
        /// <value>The type of the UniverseObject.</value>
        public override UniverseObjectType ObjectType
        {
            get { return UniverseObjectType.TechObject; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="TechObject"/> will be scrapped.
        /// </summary>
        /// <value><c>true</c> this <see cref="TechObject"/> will be scrapped; otherwise, <c>false</c>.</value>
        public bool Scrap
        {
            get { return _scrap; }
            set
            {
                _scrap = value;
                OnPropertyChanged("Scrap");
            }
        }

        /// <summary>
        /// Gets or sets the name of this <see cref="TechObject"/>.
        /// </summary>
        /// <value>The name.</value>
        public override string Name
        {
            get { return base.Name ?? ((Design != null) ? Design.Name : null); }
            set { base.Name = value; }
        }

        /// <summary>
        /// Gets or sets the design of this <see cref="TechObject"/>.
        /// </summary>
        /// <value>The design.</value>
        public TechObjectDesign Design
        {
            get {
                try
                    {

                    //GameLog.Core.General.DebugFormat("working on design ID {0}"
                    //    , _designId
                    //    );
                    if (GameContext.Current != null && GameContext.Current.TechDatabase != null)
                        return GameContext.Current.TechDatabase[_designId];
                    else
                    {
                        //GameLog.Core.General.ErrorFormat("### Problem on Design name {0} design ID {1}"
                        //    , Design.Name                     
                        //    , _designId
                        //    );
                        return null;
                    }
                        
                    }
                catch (Exception e)
                    {
                        GameLog.Core.General.Error(string.Format("### Problem on Design name {0} design ID {1}"
                            , Design.Name
                            , _designId
                            , e));
                        return GameContext.Current.TechDatabase[_designId];
                    }
                }
            set { _designId = (value != null) ? value.DesignID : TechObjectDesign.InvalidDesignID; }
        }

        public TechObject() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TechObject"/> class using the specified design.
        /// </summary>
        /// <param name="design">The design.</param>
        public TechObject(TechObjectDesign design)
            : this()
        {
            if (design == null)
                throw new ArgumentNullException("design");
            _designId = design.DesignID;
            _scrap = false;
        }

		public override void SerializeOwnedData(SerializationWriter writer, object context)
		{
			base.SerializeOwnedData(writer, context);
			writer.WriteOptimized(_designId);
			writer.Write(_scrap);
		}

		public override void DeserializeOwnedData(SerializationReader reader, object context)
		{
			base.DeserializeOwnedData(reader, context);
			_designId = reader.ReadOptimizedInt32();
			_scrap = reader.ReadBoolean();
		}

        public override void CloneFrom(Cloneable source, ICloneContext context)
        {
            var typedSource = (TechObject)source;

            base.CloneFrom(typedSource, context);

            Design = typedSource.Design;
            Scrap = typedSource.Scrap;
        }
    }

    /// <summary>
    /// Defines the types of tech objects used in the game.
    /// </summary>
    public enum TechObjectType
    {
        /// <summary>
        /// Orbital batteries
        /// </summary>
        Batteries,
        /// <summary>
        /// Planetary buildings
        /// </summary>
        Buildings,
        /// <summary>
        /// Production facilities
        /// </summary>
        Facilities,
        /// <summary>
        /// Ships
        /// </summary>
        Ships,
        /// <summary>
        /// Shipyards
        /// </summary>
        Shipyards,
        /// <summary>
        /// Space stations
        /// </summary>
        Stations
    }
}
