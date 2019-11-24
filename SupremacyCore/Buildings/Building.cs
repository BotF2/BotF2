// Building.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Effects;
using Supremacy.IO.Serialization;
using Supremacy.Tech;
using Supremacy.Types;
using Supremacy.Universe;

using System.Linq;

namespace Supremacy.Buildings
{
    /// <summary>
    /// Represents a planetary building.
    /// </summary>
    [Serializable]
    public class Building : TechObject, IEffectSource
    {
        private bool _isActive;

        /// <summary>
        /// Gets a value indicating whether this <see cref="Building"/> is active.
        /// </summary>
        /// <value><c>true</c> if this <see cref="Building"/> is active; otherwise, <c>false</c>.</value>
        public bool IsActive
        {
            get { return _isActive || BuildingDesign.AlwaysOnline; }
            set
            {
                _isActive = value;
                OnPropertyChanged("IsActive");
            }
        }

        /// <summary>
        /// Gets the type of the UniverseObject.
        /// </summary>
        /// <value>The type of the UniverseObject.</value>
        public override UniverseObjectType ObjectType
        {
            get { return UniverseObjectType.Building; }
        }

        /// <summary>
        /// Gets the building design.
        /// </summary>
        /// <value>The building design.</value>
        public BuildingDesign BuildingDesign
        {
            get { return Design as BuildingDesign; }
        }

        public Building() { }

        /// <summary>
        /// Initializes a new <see cref="Building"/> of the specified design.
        /// </summary>
        /// <param name="design">The design.</param>
        public Building(BuildingDesign design)
            : base(design)
        {
            if (design.AlwaysOnline)
                _isActive = true;

            _effectGroupBindings = new EffectGroupBindingCollection();
            _effectGroupBindings.AddRange(design.Effects.Select(o => o.Bind(this)));
        }

        public override void SerializeOwnedData(SerializationWriter writer, object context)
        {
            base.SerializeOwnedData(writer, context);
            writer.Write(_isActive);
        }

        public override void DeserializeOwnedData(SerializationReader reader, object context)
        {
            base.DeserializeOwnedData(reader, context);
            _isActive = reader.ReadBoolean();
        }

        public override void CloneFrom(Cloneable source, ICloneContext context)
        {
            var typedSource = (Building)source;

            base.CloneFrom(typedSource, context);

            IsActive = typedSource.IsActive;
        }

        #region Implementation of IEffectSource

        private EffectGroupBindingCollection _effectGroupBindings;
        public IEffectGroupBindingCollection EffectGroupBindings
        {
            get { return _effectGroupBindings; }
        }

        #endregion
    }
}
