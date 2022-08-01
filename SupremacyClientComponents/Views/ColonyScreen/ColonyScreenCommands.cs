// ColonyScreenCommands.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Microsoft.Practices.Composite.Presentation.Commands;

namespace Supremacy.Client.Views
{
    public static class ColonyScreenCommands
    {
        public static CompositeCommand ToggleBuildingScrapCommand = new CompositeCommand();
        public static CompositeCommand NextColonyCommand = new CompositeCommand();
        //public static CompositeCommand NextTABinsideColonyCommand = new CompositeCommand();
        public static CompositeCommand ShowColonyManagementCommand = new CompositeCommand();
        public static CompositeCommand ShowColonyBuildListCommand = new CompositeCommand();
        public static CompositeCommand ShowShipyardCommand = new CompositeCommand();
        public static CompositeCommand PreviousColonyCommand = new CompositeCommand();
    }
}