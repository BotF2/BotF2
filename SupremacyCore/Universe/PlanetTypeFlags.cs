using System;
using System.Collections.Generic;

using Supremacy.Annotations;
using Supremacy.Utility;

using System.Linq;

namespace Supremacy.Universe
{
    [Serializable]
    public struct PlanetTypeFlags : IEquatable<PlanetTypeFlags>
    {
        public static readonly PlanetTypeFlags Empty;
        public static readonly PlanetTypeFlags RogueOnly;
        public static readonly PlanetTypeFlags StandardHabitablePlanets;

        static PlanetTypeFlags()
        {
            Empty = new PlanetTypeFlags();

            RogueOnly = new PlanetTypeFlags(PlanetType.Rogue);

            StandardHabitablePlanets = new PlanetTypeFlags(
                EnumHelper.GetValues<PlanetType>()
                          .Where(o => !o.MatchAttribute(UninhabitableAttribute.Default))
                          .ToArray());
        }

        private readonly int _flags;

        public PlanetTypeFlags([NotNull] IEnumerable<PlanetType> planetTypes)
        {
            if (planetTypes == null)
                throw new ArgumentNullException("planetTypes");

            _flags = 0;

            foreach (PlanetType planetType in planetTypes)
                _flags |= 1 << (int)planetType;
        }

        public PlanetTypeFlags(params PlanetType[] planetTypes)
        {
            _flags = 0;

            foreach (PlanetType planetType in planetTypes)
                _flags |= 1 << (int)planetType;
        }

        public bool this[PlanetType planetType]
        {
            get
            {
                int mask = 1 << (int)planetType;
                return (_flags & mask) == mask;
            }
        }

        public bool Equals(PlanetTypeFlags other)
        {
            return _flags == other._flags;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            PlanetTypeFlags? other = obj as PlanetTypeFlags?;
            return other.HasValue && Equals(other.GetValueOrDefault());
        }

        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyFieldInGetHashCode
            return _flags;
            // ReSharper restore NonReadonlyFieldInGetHashCode
        }

        public static bool operator ==(PlanetTypeFlags left, PlanetTypeFlags right)
        {
            return left._flags == right._flags;
        }

        public static bool operator !=(PlanetTypeFlags left, PlanetTypeFlags right)
        {
            return left._flags != right._flags;
        }
    }
}