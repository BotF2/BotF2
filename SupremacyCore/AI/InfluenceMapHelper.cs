// File:InfluenceMapHelper.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using Supremacy.Collections;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Universe;
using System.Linq;

namespace Supremacy.AI
{
    public static class InfluenceMapHelper
    {
        public static InfluenceMap BuildInfluenceMap(GameContext game, Civilization owner)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            InfluenceMap map = new InfluenceMap(game);

            // avoid problems with 'ValueTuple'

            //_ = game.Civilizations
            //    .AsParallel()
            //    .Where(c => (owner != c) && DiplomacyHelper.AreAllied(owner, c))
            //    .SelectMany(c => game.Universe.FindOwned<Fleet>(c))
            //    .Where(f => f.IsCombatant)
            //    .SelectMany(f => GetFleetInfluence(game, f))
            //    .ForEach(o => map.AddAllied(o.Item1, o.Item2));

            //_ = game.Civilizations
            //    .AsParallel()
            //    .Where(c => (owner != c) && DiplomacyHelper.AreAtWar(owner, c))
            //    .SelectMany(c => game.Universe.FindOwned<Fleet>(c))
            //    .Where(f => f.IsCombatant)
            //    .SelectMany(f => GetFleetInfluence(game, f))
            //    .ForEach(o => map.AddEnemy(o.Item1, o.Item2));

            return map;
        }

        //private static IEnumerable<ValueTuple<MapLocation, int>> GetFleetInfluence(GameContext game, Fleet fleet)
        //{
        //    int startX = Math.Max(
        //        Math.Min(fleet.Location.X - fleet.Speed,
        //                 fleet.Location.X - fleet.Range),
        //        0);
        //    int startY = Math.Max(
        //        Math.Min(fleet.Location.Y - fleet.Speed,
        //                 fleet.Location.Y - fleet.Range),
        //        0);
        //    int endX = Math.Min(
        //        Math.Min(fleet.Location.X + fleet.Speed,
        //                 fleet.Location.X + fleet.Range),
        //        game.Universe.Map.Width - 1);
        //    int endY = Math.Min(
        //        Math.Min(fleet.Location.X + fleet.Speed,
        //                 fleet.Location.X + fleet.Range),
        //        game.Universe.Map.Height - 1);

        //    for (int x = startX; x <= endX; x++)
        //    {
        //        for (int y = startY; x <= endY; y++)
        //        {
        //            yield return new ValueTuple<MapLocation, int>(
        //                new MapLocation(x, y),
        //                fleet.EffectiveCombatStrength());
        //        }
        //    }

        //    yield break;
        //}
    }
}
