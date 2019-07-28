// SectorClaimGrid.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;

using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Universe;

using System.Linq;

using Supremacy.Collections;
using Supremacy.Utility;

namespace Supremacy.Game
{
    /// <summary>
    /// Contains all of the claims that civilizations have laid to various galactic sectors.
    /// </summary>
    [Serializable]
    public sealed class SectorClaimGrid
    {
        public const int MaxClaimValue = SectorClaim.MaxClaimValue;

        private readonly Dictionary<MapLocation, CollectionBase<SectorClaim>> _claims;
        private readonly Dictionary<int, CollectionBase<SectorClaim>> _claimsByOwner;

        public IIndexedCollection<SectorClaim> GetClaims(MapLocation location)
        {
            CollectionBase<SectorClaim> claims;

            if (_claims.TryGetValue(location, out claims))
                return claims;

            return ArrayWrapper<SectorClaim>.Empty;
        }

        public IIndexedCollection<SectorClaim> GetClaims(ICivIdentity owner)
        {
            CollectionBase<SectorClaim> claims;

            if (_claimsByOwner.TryGetValue(owner.CivID, out claims))
                return claims;

            return ArrayWrapper<SectorClaim>.Empty;
        }

        public IIndexedCollection<SectorClaim> GetClaims(int ownerId)
        {
            CollectionBase<SectorClaim> claims;

            if (_claimsByOwner.TryGetValue(ownerId, out claims))
                return claims;

            return ArrayWrapper<SectorClaim>.Empty;
        }

        /// <summary>
        /// Gets the owner.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        public Civilization GetOwner(MapLocation location)
        {
            CollectionBase<SectorClaim> claims;

            if (!_claims.TryGetValue(location, out claims))
                return null;

            if (claims.Count == 0)
                return null;

            if (claims.Count == 1)
                return claims[0].Owner;

            if (IsDisputed(location))
                return null;

            return claims.MaxElement(o => o.Weight).Owner;
        }

        /// <summary>
        /// Gets the perceived owner of the specified sector (as indicated on the galaxy grid) according to
        /// the specified civilization.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="asSeenBy">The civilization.</param>
        /// <returns></returns>
        public Civilization GetPerceivedOwner(MapLocation location, Civilization asSeenBy)
        {
            CollectionBase<SectorClaim> claims;

            if (!_claims.TryGetValue(location, out claims))
                return null;
            //GameLog.Core.Test.DebugFormat("location = {0}, asSeenBy = {1}", location.ToString(), asSeenBy.Key);
            var visibleClaimsEnumerator = claims
                .Select(o => new { o.Owner, o.Weight })
                .Where(o => Equals(o.Owner, asSeenBy) || DiplomacyHelper.IsContactMade(o.Owner, asSeenBy))
                .OrderByDescending(o => o.Weight)
                .GetEnumerator();
            //GameLog.Core.Test.DebugFormat("GetPerceivedOwner is partly done");

            if (!visibleClaimsEnumerator.MoveNext())
                return null;

            var winningClaim = visibleClaimsEnumerator.Current;

            if (visibleClaimsEnumerator.MoveNext())
            {
                /*
                 * If there is more than one visible claim to the sector, then the sector
                 * is disputed and has no legitimate owner.
                 */
                return null;
            }

            return winningClaim.Owner;
        }

        /// <summary>
        /// Determines whether the specified location is owned.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>
        /// <c>true</c> if the specified location is owned; otherwise, <c>false</c>.
        /// </returns>
        public bool IsOwned(MapLocation location)
        {
            CollectionBase<SectorClaim> claims;
            
            if (!_claims.TryGetValue(location, out claims))
                return false;

            return claims != null &&
                   claims.Count > 0;
        }

        /// <summary>
        /// Determines whether the specified location is claimed by a given civilization.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="civ">The civilization.</param>
        /// <param name="asSeenBy">Optionally, the civilization whose point of view is to be used.</param>
        /// <returns>
        /// <c>true</c> if the specified location is claimed by <paramref name="civ"/> and either
        /// <list type="bullet">
        /// <item><paramref name="asSeenBy"/> is <c>null</c>&#0160;<b>or</b></item>
        /// <item>contact has been made between <paramref name="civ"/> and <paramref name="asSeenBy"/>.</item>
        /// </list>
        /// Otherwise, <c>false</c> is returned.
        /// </returns>
        public bool IsClaimedByCiv(MapLocation location, Civilization civ, Civilization asSeenBy = null)
        {
            if (civ == null)
                throw new ArgumentNullException("civ");

            CollectionBase<SectorClaim> claims;

            if (!_claims.TryGetValue(location, out claims))
                return false;

            return claims.Any(
                claim =>
                {
                    if (claim.OwnerID != civ.CivID)
                        return false;

                    return asSeenBy == null ||
                           DiplomacyHelper.IsContactMade(civ, asSeenBy);
                });
        }

        /// <summary>
        /// Determines whether the specified location is disputed.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>
        /// <c>true</c> if the specified location is disputed; otherwise, <c>false</c>.
        /// </returns>
        public bool IsDisputed(MapLocation location)
        {
            CollectionBase<SectorClaim> claims;
            
            if (!_claims.TryGetValue(location, out claims))
                return false;

            return claims != null &&
                   claims.Count > 1;
        }

        /// <summary>
        /// Determines whether the specified location is disputed.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="accordingTo">The civilization with the point of view.</param>
        /// <returns>
        /// <c>true</c> if the specified location is disputed; otherwise, <c>false</c>.
        /// </returns>
        public bool IsDisputed(MapLocation location, Civilization accordingTo)
        {
            CollectionBase<SectorClaim> claims;
            
            if (!_claims.TryGetValue(location, out claims))
                return false;

            var knownClaims = (
                                  from owner in claims.Select(o => o.Owner)
                                  where Equals(owner, accordingTo) ||
                                        DiplomacyHelper.IsContactMade(owner, accordingTo)
                                  select owner
                              );

            return knownClaims.CountAtLeast(2);
        }

        /// <summary>
        /// Adds a claim to the specified sector on behalf of the given civilization.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="owner">The civilization.</param>
        /// <param name="weight">The weight of the claim.</param>
        public void AddClaim(MapLocation location, Civilization owner, int weight)
        {
            if (owner == null)
                return;

            if (weight < 0)
                return;

            if (weight > MaxClaimValue)
                weight = MaxClaimValue;

            CollectionBase<SectorClaim> claims;
            CollectionBase<SectorClaim> ownerClaims;

            var addedToClaimList = false;
            var addedToOwnerClaimList = false;

            if (!_claims.TryGetValue(location, out claims))
            {
                claims = new CollectionBase<SectorClaim> { new SectorClaim(owner.CivID, location, weight) };
                _claims.Add(location, claims);
                addedToClaimList = true;
            }

            if (!_claims.TryGetValue(location, out ownerClaims))
            {
                ownerClaims = new CollectionBase<SectorClaim> { new SectorClaim(owner.CivID, location, weight) };
                _claimsByOwner.Add(owner.CivID, ownerClaims);
                addedToOwnerClaimList = true;
            }

            if (!addedToClaimList)
            {
                for (var i = 0; i < claims.Count; i++)
                {
                    var claim = claims[i];
                    if (claim.OwnerID != owner.CivID)
                        continue;

                    claims[i] = new SectorClaim(claim.OwnerID, location, claim.Weight + weight);
                    addedToClaimList = true;
                    break;
                }

                if (!addedToClaimList)
                    claims.Add(new SectorClaim(owner.CivID, location, weight));
            }

            if (addedToOwnerClaimList)
                return;

            for (var i = 0; i < claims.Count; i++)
            {
                var claim = ownerClaims[i];
                if (claim.OwnerID != owner.CivID)
                    continue;

                ownerClaims[i] = new SectorClaim(claim.OwnerID, location, claim.Weight + weight);
                return;
            }

            ownerClaims.Add(new SectorClaim(owner.CivID, location, weight));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SectorClaimGrid"/> class.
        /// </summary>
        public SectorClaimGrid()
        {
            _claims = new Dictionary<MapLocation,CollectionBase<SectorClaim>>();
            _claimsByOwner = new Dictionary<int, CollectionBase<SectorClaim>>();
        }

        /// <summary>
        /// Clears the claims.
        /// </summary>
        public void ClearClaims()
        {
            _claims.Clear();
            _claimsByOwner.Clear();
        }
    }
}
