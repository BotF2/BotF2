// NavigationCommands.cs
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
    public static class NavigationCommands
    {
        #region Constants
        public static readonly CompositeCommand ActivateScreen = new CompositeCommand();
        public static readonly CompositeCommand NavigateToColony = new CompositeCommand();
        public static readonly CompositeCommand RushColonyProduction = new CompositeCommand();
        #endregion
    }
}