using System;
using System.ComponentModel;

using Supremacy.Entities;
using Supremacy.Universe;
using Supremacy.Utility;

namespace Supremacy.Game
{
    [Serializable]
    [ImmutableObject(true)]
    public struct SectorClaim : IEquatable<SectorClaim>, IFormattable
    {
        public const int MaxClaimValue = 255;

        private const uint OwnerIdMask = 0xFF000000;
        private const byte OwnerIdOffset = 24;
        private const uint LocationXMask = 0x00FF0000;
        private const byte LocationXOffset = 16;
        private const uint LocationYMask = 0x0000FF00;
        private const byte LocationYOffset = 8;
        private const uint WeightMask = 0x000000FF;

        private readonly uint _data;

        private SectorClaim(uint data)
        {
            _data = data;
        }

        public SectorClaim(ICivIdentity owner, MapLocation location, int weight)
            : this(Guard.ArgumentNotNull(owner, "owner").CivID, location, weight) { }

        public SectorClaim(int ownerId, MapLocation location, int weight)
        {
            if (ownerId == -1)
                throw new ArgumentException("Invalid Civilization ID.", "ownerId");

            _data = GetClaim(ownerId, location, weight);
        }

        public int OwnerID => ExtractOwnerID(_data);

        public Civilization Owner => ExtractOwner(_data);

        public MapLocation Location => ExtractLocation(_data);

        public int Weight => ExtractWeight(_data);

        #region Helper Methods

        private static uint GetClaim(int civId, MapLocation location, int weight)
        {
            return (uint)((civId << OwnerIdOffset) & OwnerIdMask |
                          (location.X << LocationXOffset) & LocationXMask |
                          (location.Y << LocationYOffset) & LocationYMask |
                          weight & WeightMask);
        }

        private static int ExtractOwnerID(uint claim)
        {
            return (int)((claim & OwnerIdMask) >> OwnerIdOffset);
        }

        private static MapLocation ExtractLocation(uint claim)
        {
            return new MapLocation(
                (int)((claim & LocationXMask) >> LocationXOffset),
                (int)((claim & LocationYMask) >> LocationYOffset));
        }

        private static Civilization ExtractOwner(uint claim)
        {
            int ownerId = ExtractOwnerID(claim);

            if (ownerId == -1)
                return null;

            Civilization owner;
            
            if (GameContext.Current.Civilizations.TryGetValue(ownerId, out owner))
                return owner;

            return null;
        }

        private static int ExtractWeight(uint claim)
        {
            return (int)(claim & WeightMask);
        }

        #endregion

        #region Conversion Operators

        public static implicit operator uint(SectorClaim claim)
        {
            return claim._data;
        }

        public static implicit operator SectorClaim(uint claimData)
        {
            return new SectorClaim(claimData);
        }

        #endregion

        #region Equality Members

        public bool Equals(SectorClaim other)
        {
            return other._data == _data;
        }

        public override bool Equals(object obj)
        {
            SectorClaim? other = obj as SectorClaim?;
            return other.HasValue && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)_data;
        }

        public static bool operator ==(SectorClaim left, SectorClaim right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SectorClaim left, SectorClaim right)
        {
            return !left.Equals(right);
        }

        #endregion

        #region Implementation of IFormattable

        string IFormattable.ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(
                formatProvider,
                format,
                _data);
        }

        #endregion
    }
}