// IProductionCenter.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

using Supremacy.Collections;
using Supremacy.Universe;
using Wintellect.PowerCollections;

namespace Supremacy.Economy
{
    /// <summary>
    /// An interface for all production centers in the game.
    /// </summary>
    public interface IProductionCenter : IUniverseObject
    {
        /// <summary>
        /// Gets the build slots at this <see cref="IProductionCenter"/>.
        /// </summary>
        /// <value>The build slots.</value>
        IIndexedEnumerable<BuildSlot> BuildSlots { get; }

        /// <summary>
        /// Gets the build output for the specified build slot number.
        /// </summary>
        /// <param name="slot">The build slot number.</param>
        /// <returns>The build output.</returns>
        int GetBuildOutput(int slot);

        /// <summary>
        /// Gets the build queue at this <see cref="IProductionCenter"/>.
        /// </summary>
        /// <value>The build queue.</value>
        IList<BuildQueueItem> BuildQueue { get; }

        /// <summary>
        /// Remove any completed projects from the build slots and dequeue new projects
        /// as slots become available.
        /// </summary>
        void ProcessQueue();
    }

    public static class ProductionCenterExtensions
    {
        public static void InvalidateBuildTimes(this IProductionCenter source)
        {
            foreach (var buildSlot in source.BuildSlots)
            {
                var project = buildSlot.Project;
                if (project != null)
                    project.InvalidateTurnsRemaining();
            }
            foreach (var project in source.BuildQueue.Where(o => o.Project != null).Select(o => o.Project))
            {
                project.InvalidateTurnsRemaining();
            }
        }

        public static void ClearBuildPrioritiesAndConsolidate(this IProductionCenter source)
        {
            for (int i = 0; i < source.BuildQueue.Count; i++)
            {
                source.BuildQueue[i].Project.Priority = BuildProject.MinPriority;
                while (i < (source.BuildQueue.Count - 1))
                {
                    if (source.BuildQueue[i].Project.IsEquivalent(source.BuildQueue[i + 1].Project))
                    {
                        source.BuildQueue[i].IncrementCount();
                        source.BuildQueue.RemoveAt(i + 1);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        public static void PrioritizeProduction(
            this IProductionCenter source,
            Func<BuildProject, int> priorityFunc,
            bool suspendActiveProjects)
        {
            if (source.BuildQueue.Count == 0)
                return;

            if (suspendActiveProjects)
            {
                foreach (var slot in source.BuildSlots.Where(slot => slot.HasProject))
                {
                    source.BuildQueue.Add(new BuildQueueItem(slot.Project));
                    slot.Project = null;
                }
            }

            // Sort BuildQueueItems in *descending* order of priority.
            Algorithms.SortInPlace(
                source.BuildQueue,
                (a, b) => priorityFunc(b.Project).CompareTo(priorityFunc(a.Project)));

            for (var i = 0; (i < source.BuildSlots.Count) && (source.BuildQueue.Count > 0); i++)
            {
                if (source.BuildSlots[i].HasProject)
                    continue;

                source.BuildSlots[i].Project = source.BuildQueue[0].Project;
                source.BuildQueue[0].DecrementCount();

                if (source.BuildQueue[0].Count == 0)
                    source.BuildQueue.RemoveAt(0);
            }
        }
    }
}
