// ObjectVisibilityManager.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Entities;
using Supremacy.Universe;
using Supremacy.Utility;

using System.Linq;

namespace Supremacy.Game
{
    [Serializable]
    public sealed class ObjectVisibilityManager
    {
        private readonly Dictionary<GameObjectVisibilityKey, CompositeVisibility> _entries;

        [NonSerialized]
        private ReaderWriterSpinLock _spinLock;

        public ObjectVisibilityManager()
        {
            _spinLock = new ReaderWriterSpinLock();
            _entries = new Dictionary<GameObjectVisibilityKey, CompositeVisibility>();
        }

        public void AddPermanentVisibility(
            GameObjectID civId,
            GameObjectID targetId,
            UniverseObjectVisibility visibility)
        {
            if (!civId.IsValid)
                throw new ArgumentException("Value must be a valid GameObjectID", "civId");
            if (!targetId.IsValid)
                throw new ArgumentException("Value must be a valid GameObjectID", "targetId");

            AddVisibility(civId, targetId, visibility, CompositeVisibility.PermanentDuration);
        }

        public void AddPermanentVisibility(
            Civilization civ,
            UniverseObject target,
            UniverseObjectVisibility visibility)
        {
            if (civ == null)
                throw new ArgumentNullException("civ");
            if (target == null)
                throw new ArgumentNullException("target");

            AddVisibility(civ, target, visibility, CompositeVisibility.PermanentDuration);
        }

        public void AddVisibility(
            Civilization civ,
            UniverseObject target,
            UniverseObjectVisibility visibility,
            int duration)
        {
            if (civ == null)
                throw new ArgumentNullException("civ");
            if (target == null)
                throw new ArgumentNullException("target");

            AddVisibility(civ.CivID, target.ObjectID, visibility, duration);
        }

        public void AddVisibility(
            GameObjectID civId,
            GameObjectID targetId,
            UniverseObjectVisibility visibility,
            int duration)
        {
            if (!civId.IsValid)
                throw new ArgumentException("Value must be a valid GameObjectID", "civId");
            if (!targetId.IsValid)
                throw new ArgumentException("Value must be a valid GameObjectID", "targetId");

            var key = new GameObjectVisibilityKey(civId, targetId);

            _spinLock.EnterWriteLock();

            try
            {
                if (!_entries.TryGetValue(key, out CompositeVisibility compositeVisibility))
                    compositeVisibility = new CompositeVisibility();
                
                compositeVisibility.AddVisibility(visibility, duration);

                _entries[key] = compositeVisibility;
            }
            finally
            {
                _spinLock.ExitWriteLock();
            }
        }

        public UniverseObjectVisibility GetVisibility(Civilization civ, UniverseObject target)
        {
            if (civ == null)
                throw new ArgumentNullException("civ");
            if (target == null)
                throw new ArgumentNullException("target");

            _spinLock.EnterReadLock();

            try
            {
                return GetVisibility(civ.CivID, target.ObjectID);
            }
            finally
            {
                _spinLock.ExitReadLock();
            }
        }

        public UniverseObjectVisibility GetVisibility(GameObjectID civId, GameObjectID targetId)
        {
            if (!civId.IsValid)
                throw new ArgumentException("Value must be a valid GameObjectID", "civId");
            if (!targetId.IsValid)
                throw new ArgumentException("Value must be a valid GameObjectID", "targetId");

            CompositeVisibility compositeVisibility;
            var key = new GameObjectVisibilityKey(civId, targetId);

            _spinLock.EnterReadLock();

            try
            {
                if (!_entries.TryGetValue(key, out compositeVisibility))
                    return UniverseObjectVisibility.None;
            }
            finally
            {
                _spinLock.ExitReadLock();
            }

            return compositeVisibility.Visibility;
        }

        public void ClearForCivilization([NotNull] Civilization civ)
        {
            if (civ == null)
                throw new ArgumentNullException("civ");

            var civId = civ.CivID;

            _spinLock.EnterReadLock();

            try
            {
                var removedEntries = _entries.Keys.Where(o => o.CivID == civId).ToList();

                _spinLock.EnterWriteLock();

                try
                {
                    _entries.RemoveRange(removedEntries);
                }
                finally
                {
                    _spinLock.ExitWriteLock();
                }
            }
            finally
            {
                _spinLock.ExitReadLock();
            }
        }

        public void ClearForTarget([NotNull] UniverseObject target)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            var targetId = target.ObjectID;

            _spinLock.EnterReadLock();

            try
            {
                var removedEntries = _entries.Keys.Where(o => o.TargetID == targetId).ToList();

                _spinLock.EnterWriteLock();

                try
                {
                    _entries.RemoveRange(removedEntries);
                }
                finally
                {
                    _spinLock.ExitWriteLock();
                }
            }
            finally
            {
                _spinLock.ExitReadLock();
            }
        }

        public void OnTurn()
        {
            _spinLock.EnterReadLock();

            try
            {
                var removedEntries = _entries
                    .ForEach(o => o.Value.OnTurn())
                    .Where(o => o.Value.Visibility == UniverseObjectVisibility.None)
                    .ToList();

                _spinLock.EnterWriteLock();

                try
                {
                    _entries.RemoveRange(removedEntries);
                }
                finally
                {
                    _spinLock.ExitWriteLock();
                }
            }
            finally
            {
                _spinLock.ExitReadLock();
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            Interlocked.CompareExchange(ref _spinLock, new ReaderWriterSpinLock(), null);
        }

        #region GameObjectVisibilityKey Struct
        private struct GameObjectVisibilityKey : IEquatable<GameObjectVisibilityKey>
        {
            private readonly GameObjectID _civId;
            private readonly GameObjectID _targetId;

            public GameObjectID CivID
            {
                get { return _civId; }
            }

            public GameObjectID TargetID
            {
                get { return _targetId; }
            }

            public GameObjectVisibilityKey([NotNull] GameObjectID civId, [NotNull] GameObjectID targetId)
            {
                if (!civId.IsValid)
                    throw new ArgumentException("Value must be a valid GameObjectID", "civId");
                if (!targetId.IsValid)
                    throw new ArgumentException("Value must be a valid GameObjectID", "targetId");

                _civId = civId;
                _targetId = targetId;
            }

            public bool Equals(GameObjectVisibilityKey other)
            {
                return other._civId.Equals(_civId) && other._targetId.Equals(_targetId);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;
                if (obj.GetType() != typeof(GameObjectVisibilityKey))
                    return false;
                return Equals((GameObjectVisibilityKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_civId.GetHashCode() * 397) ^ _targetId.GetHashCode();
                }
            }

            public static bool operator ==(GameObjectVisibilityKey left, GameObjectVisibilityKey right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(GameObjectVisibilityKey left, GameObjectVisibilityKey right)
            {
                return !left.Equals(right);
            }
        }
        #endregion
    }

    [Serializable]
    internal struct CompositeVisibility
    {
        public const int PermanentDuration = Byte.MaxValue;

        private byte[] _durations;

        public void AddVisibility(UniverseObjectVisibility visibility, int duration)
        {
            AddVisibilityCore(visibility, duration);
        }

        public void AddPermanentVisibility(UniverseObjectVisibility visibility)
        {
            AddVisibilityCore(visibility, PermanentDuration);
        }

        private void AddVisibilityCore(UniverseObjectVisibility visibility, int duration)
        {
            var index = 0;

            EnsureData();

            foreach (var value in EnumHelper.GetValues<UniverseObjectVisibility>().Skip(1))
            {
                if (((ushort)visibility & (ushort)value) == (ushort)value)
                {
                    if (duration == PermanentDuration)
                        _durations[index] = PermanentDuration;
                    else
                        _durations[index] = (byte)Math.Max(_durations[index], duration);
                }
                index++;
            }
        }

        public UniverseObjectVisibility Visibility
        {
            get
            {
                var index = 0;
                var result = UniverseObjectVisibility.None;

                EnsureData();

                foreach (var value in EnumHelper.GetValues<UniverseObjectVisibility>().Skip(1))
                {
                    if (_durations[index] != 0)
                        result |= value;
                    ++index;
                }
                return result;
            }
        }

        private void EnsureData()
        {
            if (_durations == null)
                _durations = new byte[EnumHelper.GetValues<UniverseObjectVisibility>().Length - 1];
        }

        public void OnTurn()
        {
            EnsureData();

            for (var index = 0; index < _durations.Length; index++)
            {
                if ((_durations[index] > 0) & (_durations[index] != PermanentDuration))
                    --_durations[index];
            }
        }
    }
}
