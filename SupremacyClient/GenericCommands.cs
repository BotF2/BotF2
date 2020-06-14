// GenericCommands.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.Windows.Input;

namespace Supremacy.Client
{
    public static class GenericCommands
    {
        public static readonly RoutedCommand AcceptCommand = new RoutedCommand("Accept", typeof(GenericCommands));
        public static readonly RoutedCommand CancelCommand = new RoutedCommand("Cancel", typeof(GenericCommands));
        public static readonly RoutedCommand TracesSetAllCommand = new RoutedCommand("TracesSetAll", typeof(GenericCommands));
        public static readonly RoutedCommand TracesSetSomeCommand = new RoutedCommand("TracesSetSome", typeof(GenericCommands));
        public static readonly RoutedCommand TracesSetNoneCommand = new RoutedCommand("TracesSetNone", typeof(GenericCommands));
    }
}
