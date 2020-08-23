// ColonyScreenRegions.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.Collections.Generic;

namespace Supremacy.Client.Views
{
    public class ColonyScreenRegions
    {
        public const string ColonyInfo = "ColonyInfo";
        public const string ProductionManagement = "ProductionManagement";
        public const string PlanetaryBuildQueue = "PlanetaryBuildQueue";
        public const string ShipyardBuildQueue = "ShipyardBuildQueue";
        public const string PlanetaryBuildList = "PlanetaryBuildList";
        public const string ShipyardBuildList = "ShipyardBuildList";
        public const string SelectedPlanetaryBuildProjectInfo = "SelectedPlanetaryBuildProjectInfo";
        public const string SelectedShipyardBuildProjectInfo = "SelectedShipyardBuildProjectInfo";
        public const string StructureList = "StructureList";
        public const string HandlingList = "HandlingList";

        public static IEnumerable<string> GetRegionNames()
        {
            return new[]
                   {
                       ColonyInfo, ProductionManagement, PlanetaryBuildQueue, ShipyardBuildQueue,
                       PlanetaryBuildList, ShipyardBuildList, SelectedPlanetaryBuildProjectInfo,
                       SelectedShipyardBuildProjectInfo, StructureList,HandlingList
                   };
        }
    }
}