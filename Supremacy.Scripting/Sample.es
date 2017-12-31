/*
 * The expression language used for effects is
 * the LINQ-supported subset of C# expressions with
 * a couple useful additions.  One such addition is
 * support for locally-scoped expressions like
 * constants or helper lambdas.
 *
 * The following is essentially transformed into
 * the invocation expression e(locals), where
 * 'e' is a lambda with the primary expression as
 * its body (in this case, the 'from c in ...' query
 * expression) and the local expressions passed to
 * 'e' as parameters.
 */

local distance = (a, b) => MapLocation.GetDistance(
                               a.Location,
                               b.Location)
            
from c in $Universe.FindOwned<Colony>($Source.Owner)
where distance(c, $Source) <= #EffectRadius
select c