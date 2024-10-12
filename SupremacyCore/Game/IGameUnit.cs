// File:IGameUnit.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Universe;

namespace Supremacy.Game
{
    public interface IGameUnit : IUniverseObject
    {
        UnitAIType UnitAIType { get; set; }
        UnitActivity Activity { get; set; }
        int ActivityStart { get; }
        int ActivityDuration { get; }
    }
}
