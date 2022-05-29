// File:DebugCommands.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Microsoft.Practices.Composite.Presentation.Commands;

namespace Supremacy.Client.Commands
{
    public static class DebugCommands
    {
        public static readonly CompositeCommand RevealMap = new CompositeCommand();  // ALT + F
        public static readonly CompositeCommand OutputMap = new CompositeCommand();  // ALT + M

        public static readonly CompositeCommand ShowBuildings = new CompositeCommand();  // ALT + 1
        public static readonly CompositeCommand ShowBuildList = new CompositeCommand();  // ALT + 2
        public static readonly CompositeCommand ShowShipyard = new CompositeCommand();  // ALT + 3

        public static readonly CompositeCommand CheatMenu = new CompositeCommand();  // ALT + C
        public static readonly CompositeCommand F12_Screen = new CompositeCommand(); // F12
        public static readonly CompositeCommand F11_Screen = new CompositeCommand(); // F11
        public static readonly CompositeCommand F10_Screen = new CompositeCommand(); // F10
        public static readonly CompositeCommand F09_Screen = new CompositeCommand(); // F9
        public static readonly CompositeCommand F08_Screen = new CompositeCommand(); // F8
        public static readonly CompositeCommand F07_Screen = new CompositeCommand();  // F7
        public static readonly CompositeCommand F06_Screen = new CompositeCommand(); // F6
    }
}