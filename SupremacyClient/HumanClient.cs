// HumanClient.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Combat;

namespace Supremacy.Client
{
    internal delegate void SendChatMessageDelegate(string message, int recipientId);

    internal delegate void SendCombatOrdersDelegate(CombatOrders orders);

    internal delegate void SendCombatTarget1Delegate(CombatTargetPrimaries target1);

    internal delegate void SendCombatTarget2Delegate(CombatTargetSecondaries target2);

    internal delegate void FinishTurnDelegate();

}