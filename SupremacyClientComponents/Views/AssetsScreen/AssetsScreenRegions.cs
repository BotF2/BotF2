// GalaxyScreenRegions.cs
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
    public static class AssetsScreenRegions
    {
        public const string SpyList = "SpyList";
        public const string TaskForceList = "TaskForceList";
        public const string TradeRouteList = "TradeRouteList";
        public const string EmpireOverview = "EmpireOverview";
        public const string EmpireResources = "EmpireResources";
        public const string AssignedShipList = "AssignedShipList";
        public const string AvailableShipList = "AvailableShipList";
        public const string ShipStats = "ShipStats";
        public const string ShipClassStats = "ShipClassStats";
        //public const string GalaxyGrid = "GalaxyGrid";

        public static IEnumerable<string> GetRegionNames()
        {
            return new[]
                   {
                       SpyList, TaskForceList, TradeRouteList,
                       EmpireOverview, EmpireResources, AssignedShipList,
                       AvailableShipList, ShipStats, ShipClassStats
                   };
        }
    }
}