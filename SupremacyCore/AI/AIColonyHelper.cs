// File:AIColonyHelper.cs
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Economy;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Universe;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Supremacy.AI
{
    public static class AIColonyHelper
    {
        public static bool IsInDanger(Colony colony)
        {
            if (colony == null)
            {
                throw new ArgumentNullException(nameof(colony));
            }

            return PlayerAI.GetSectorDanger(colony.Owner, colony.Sector, 2) > 0;
        }

        public static BuildProject GetBuildProject(Colony colony)
        {
            if (colony == null)
            {
                throw new ArgumentNullException(nameof(colony));
            }

            return colony.BuildSlots[0].Project;
        }

        public static ICollection<Orbital> GetDefenders(Colony colony)
        {
            if (colony == null)
            {
                throw new ArgumentNullException(nameof(colony));
            }

            return GameContext.Current.Universe.FindAt<Orbital>(colony.Location)
                .Where(o => o.Owner == colony.Owner).ToList();

        }

        public static int GetProjectedPopulation(Colony colony, int numberOfTurns)
        {
            Types.Meter populationCopy = colony.Population.Clone();

            populationCopy.UpdateAndReset();

            if (populationCopy.IsMaximized)
            {
                return populationCopy.CurrentValue;
            }

            for (int i = 0; (i < numberOfTurns) && !populationCopy.IsMaximized; i++)
            {
                _ = populationCopy.AdjustCurrent(colony.GrowthRate);
            }

            return populationCopy.CurrentValue;
        }
    }
}
