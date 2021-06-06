// Motivation.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

namespace Supremacy.Diplomacy
{
    [Serializable]
    public class Motivation : IEquatable<Motivation>
    {
        public static readonly Motivation NoMotivation;

        static Motivation()
        {
            NoMotivation = new Motivation
            {
                Type = MotivationType.NoMotivation,
                Priority = 0
            };
        }

        public MotivationType Type { get; set; }
        public int Priority { get; set; }

        public bool IsValid => (Type != MotivationType.NoMotivation);

        public override bool Equals(object obj)
        {
            return Equals(obj as Motivation);
        }

        public override int GetHashCode()
        {
            return (((int)Type << 16) | Priority);
        }

        #region IEquatable<Motivation> Members
        public bool Equals(Motivation other)
        {
            if (other == null)
                return false;
            return ((other.Type == Type) && (other.Priority == Priority));
        }
        #endregion
    }
}