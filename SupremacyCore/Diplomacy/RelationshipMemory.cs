// RelationshipMemory.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Game;

namespace Supremacy.Diplomacy
{
    public interface IAttitudeAdjuster
    {
        string Description { get; }
        bool IsNegative { get; }
        int Value { get; }
        int TurnCreated { get; }
        int TurnLastUpdated { get; }
    }

    [Serializable]
    public class RelationshipMemory : IAttitudeAdjuster
    {
        public const int RecentThreshold = 10;

        private readonly int _turnCreated;
        private readonly object _parameter;
        private readonly MemoryType _memoryType;
        private int _value;

        public MemoryType MemoryType => _memoryType;

        public int TurnCreated => _turnCreated;

        public int TurnLastUpdated => _turnCreated;

        public int Value
        {
            get => _value;
            internal set => _value = value;
        }

        public object Parameter => _parameter;

        public RelationshipMemory(MemoryType memoryType, int value)
            : this(memoryType, value, null) { }

        public RelationshipMemory(MemoryType memoryType, int value, object parameter)
            : this(memoryType, GameContext.Current.TurnNumber, value, parameter) { }

        public RelationshipMemory(MemoryType memoryType, int turnCreated, int value, object parameter)
        {
            if (turnCreated < 0)
            {
                throw new ArgumentOutOfRangeException("turnCreated", "must be >= 0");
            }

            _memoryType = memoryType;
            _turnCreated = turnCreated;
            _value = value;
            _parameter = parameter;
        }

        #region IAttitudeAdjuster Members
        public string Description => _memoryType.ToString();

        public bool IsNegative => _value < 0;
        #endregion

        public override string ToString()
        {
            return string.Format("{0} ({1})", _memoryType, _value);
        }
    }
}