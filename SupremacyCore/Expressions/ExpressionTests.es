/*
 * distanceFromSource: Lambda Function
 *
 * Accepts a UniverseObject and returns its distance
 * (in sectors) from the effect source.
 */
local distanceFromSource = 
      o => MapLocation.GetDistance(
               o.Location,
               $Source.Location)

/*
 * isOwnedOrAllied: Lambda Function
 *
 * Accepts a UniverseObject and returns 'true' if the
 * object is owned by the same civilization as the effect
 * source, or if the two owners are allied.  Otherwise,
 * it returns 'false'.
 */               
local isOwnedOrAllied =
      o => o.OwnerID == $Source.OwnerID ||
           DiplomacyHelper.AreAllied(o.Owner, $Source.Owner)

/*
 * Target Selection
 *
 * Selects all colonies meeting the following conditions:
 *
 *   (1) It is owned by the same civilization that owns the
 *       effect source (or the owner is an ally);
 *
 *   (2) The colony is within 5 sectors of the effect source;
 *
 *   (3) And the morale level at the target colony is equal to
 *       or greater than the value of the MinMorale parameter.
 */
from o in Universe
where o is Colony && 
      isOwnedOrAllied(o) &&
      o.Morale.CurrentValue > #MinMorale &&
      distanceFromSource(o) <= 5
select o