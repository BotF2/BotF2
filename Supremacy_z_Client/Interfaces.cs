// Interfaces.cs
//
// Copyright (c) 2008 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Microsoft.Practices.Composite;

using Supremacy.Game;

namespace Supremacy.Client
{
    public interface IGameScreen : IActiveAware
    {
        IGameContext Game { set; }
    }

    public interface IGameScreenController
    {
        void Run();
        void Terminate();
    }
}