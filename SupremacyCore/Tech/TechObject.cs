// TechObject.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Diagnostics;
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
        //private string _text;

        /// <summary>
        /// Gets the type of the UniverseObject.
        /// </summary>
        /// <value>The type of the UniverseObject.</value>
        public override UniverseObjectType ObjectType => UniverseObjectType.TechObject;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="TechObject"/> will be scrapped.
        /// </summary>
        /// <value><c>true</c> this <see cref="TechObject"/> will be scrapped; otherwise, <c>false</c>.</value>
        public bool Scrap
        {
            get => _scrap;
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
            get
            {
                string v = base.Name ?? (Design?.Name);
                if (v == null) 
                { 
                    v = "MissingName";
                }
                ;
                return v;
            }

            set => base.Name = value;
        }

        /// <summary>
        /// Gets or sets the design of this <see cref="TechObject"/>.
        /// </summary>
        /// <value>The design.</value>
        public TechObjectDesign Design
        {
            get
            {
                try
                {
                    //_text = "Step_0256:; working on _designId= " + _designId;
                    //Console.WriteLine(_text);
                    //GameLog.Core.General.DebugFormat(_text);

                    if (GameContext.Current != null && GameContext.Current.TechDatabase != null)
                    {
                        return GameContext.Current.TechDatabase[_designId];
                    }
                    else
                    {
                        //GameLog.Core.General.ErrorFormat("### Problem on Design name {0} design ID {1}"
                        //    , Design.Name                     
                        //    , _designId
                        //    );

                        // is this causing the stack overflow ? > should not

                        // at start TechDatabase is simply not loaded yet
                        
                        _text = "Step_2051:; "
                            + "### Problem on Design name" //+ Design.Name
                            + " designId= " + _designId + " ( > mostly Shipyards due to missing TechDatabase ?)"
                            ;
                        //Console.WriteLine(_text);

                        //Debugger.Break();
                        
                        //GameLog.Client.GameData.ErrorFormat(_text);
                        return null;
                    }

                }
                catch (Exception e)
                {
                    Debugger.Break();
                    _text = "Step_2053:; "
                        + "### Problem on Design name" + Design.Name
                        + " designId" + _designId
                        + " > " + e.ToString()
                        ;
                    Console.WriteLine(_text);
                    GameLog.Core.General.Error(string.Format("### Problem on Design name {0} design ID {1}"
                        , Design.Name
                        , _designId
                        , e));
                    return GameContext.Current.TechDatabase[_designId];
                }
            }
            set => _designId = (value != null) ? value.DesignID : TechObjectDesign.InvalidDesignID;
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
            {
                throw new ArgumentNullException("design");
            }

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
            TechObject typedSource = (TechObject)source;

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
