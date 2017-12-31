// AgentExperienceCategory.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

namespace Supremacy.Personnel
{
    /*
                            Spy     Dip    Off
                            ===     ===    ===
        Leadership           -       S      P
        Charisma             -       P      S
        Deception            P       S      -
        Stealth              P       -      S
        Combat               S       -      P
        Empathy              S       P      -
    */

    /// <summary>
    /// Describes the various categories of experience ratings for Agents.
    /// </summary>
    public enum AgentSkill : byte
    {
        Leadership = 0,
        Charisma = 1,
        Deception = 2,
        Stealth = 3,
        Combat = 4,
        Empathy = 5
    }
}