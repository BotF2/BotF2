// NavigationCommandsProxy.cs
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
    public class NavigationCommandsProxy : INavigationCommandsProxy
    {
        #region INavigationCommandsProxy Implementation
        public CompositeCommand ActivateScreen
        {
            get { return NavigationCommands.ActivateScreen; }
        }

        public CompositeCommand NavigateToColony
        {
            get { return NavigationCommands.NavigateToColony; }
        }

        public CompositeCommand RushColonyProduction
        {
            get { return NavigationCommands.RushColonyProduction; }
        }
        #endregion
    }
}