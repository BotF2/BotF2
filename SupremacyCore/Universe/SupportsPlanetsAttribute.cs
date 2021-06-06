// SupportsPlanetsAttribute.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Utility;
using System;

namespace Supremacy.Universe
{
    public class SupportsPlanetsAttribute : Attribute
    {
        #region Fields
        public static readonly SupportsPlanetsAttribute Default = new SupportsPlanetsAttribute();

        private PlanetSize[] _allowedSizes;
        private PlanetType[] _allowedTypes;
        private int _maxNumberOfPlanets = StarSystem.MaxPlanetsPerSystem;
        #endregion


        #region Properties
        public int MaxNumberOfPlanets
        {
            get { return _maxNumberOfPlanets; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "value must be a positive integer");
                if (value > StarSystem.MaxPlanetsPerSystem)
                    throw new ArgumentOutOfRangeException("value", "value must be less than " + StarSystem.MaxPlanetsPerSystem);
                _maxNumberOfPlanets = value;
            }
        }

        public bool IsAllowedTypesDefined => ((_allowedTypes != null) && (_allowedTypes.Length > 0));

        public bool IsAllowedSizesDefined => ((_allowedSizes != null) && (_allowedSizes.Length > 0));

        public PlanetType[] AllowedTypes
        {
            get { return _allowedTypes; }
            set { _allowedTypes = value; }
        }

        public PlanetSize[] AllowedSizes
        {
            get { return _allowedSizes; }
            set { _allowedSizes = value; }
        }
        #endregion

        #region Methods
        public override bool Match(object obj)
        {
            GameLog.Core.UI.DebugFormat("Matching Attribut: incoming obj = {0}, matched with {1}", obj, obj is SupportsPlanetsAttribute);
            return (obj is SupportsPlanetsAttribute);
        }

        public override bool IsDefaultAttribute()
        {
            return true;
        }
        #endregion
    }
}