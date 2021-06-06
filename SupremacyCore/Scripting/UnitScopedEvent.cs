// 
// UnitScopedEvent.cs
// 
// Copyright (c) 2013-2013 Mike Strobel
// 
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
// 
// All other rights reserved.
// 

using System;
using System.Collections.Generic;

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Universe;
using Supremacy.Utility;

namespace Supremacy.Scripting
{
    [Serializable]
    public abstract class UnitScopedEvent<TUnit> : CivilizationScopedEvent
        where TUnit : UniverseObject
    {
        private readonly KeyedCollectionBase<int, UnitTargetHistoryEntry> _unitTargetHistory;
        private int _unitRecurrencePeriod = NoRecurrenceLimit;

        protected UnitScopedEvent()
        {
            _unitTargetHistory = new KeyedCollectionBase<int, UnitTargetHistoryEntry>(entry => entry.TargetID);
        }

        /// <summary>
        /// The minimum number of turns to elapse between occurrences for a given unit.
        /// Use <c>-1</c> to limit the event to one occurrence per unit.
        /// </summary>
        protected int UnitRecurrencePeriod
        {
            get { return _unitRecurrencePeriod; }
            private set
            {
                VerifyInitializing();
                _unitRecurrencePeriod = value;
            }
        }

        protected IKeyedCollection<int, UnitTargetHistoryEntry> UnitTargetHistory => _unitTargetHistory;

        internal sealed override void InitializeCore(IDictionary<string, object> options)
        {
            base.InitializeCore(options);

            object value;

            if (options.TryGetValue("UnitRecurrencePeriod", out value))
            {
                try
                {
                    UnitRecurrencePeriod = Convert.ToInt32(value);
                }
                catch
                {
                    GameLog.Client.GameData.ErrorFormat(
                        "Invalid UnitRecurrencePeriod value for event '{0}': {1}",
                        EventID,
                        value);

                    throw;
                }
            }
        }

        protected virtual bool CanTargetUnit([NotNull] TUnit unit)
        {
            if (unit == null)
                throw new ArgumentNullException("unit");

            var owner = unit.Owner;
            if (owner != null && !CanTargetCivilization(owner))
                return false;

            UnitTargetHistoryEntry entry;

            if (!_unitTargetHistory.TryGetValue(unit.ObjectID, out entry))
                return true;

            if (UnitRecurrencePeriod < 0)
                return false;

            return (GameContext.Current.TurnNumber - entry.TurnNumber) > UnitRecurrencePeriod;
        }

        protected virtual void OnUnitTargeted([NotNull] TUnit unit)
        {
            if (unit == null)
                throw new ArgumentNullException("unit");

            var owner = unit.Owner;
            if (owner != null)
                OnCivilizationTargeted(owner);

            _unitTargetHistory.Remove(unit.ObjectID);
            _unitTargetHistory.Add(new UnitTargetHistoryEntry(unit, GameContext.Current.TurnNumber));
        }

        protected override void OnTurnFinishedOverride(GameContext game)
        {
            base.OnTurnFinishedOverride(game);

            if (UnitRecurrencePeriod < 0)
                return;

            HashSet<int> removedItems = null;

            foreach (var entry in _unitTargetHistory)
            {
                if ((GameContext.Current.TurnNumber - entry.TurnNumber) > UnitRecurrencePeriod)
                {
                    if (removedItems == null)
                        removedItems = new HashSet<int>();
                    removedItems.Add(entry.TargetID);
                }
            }

            if (removedItems == null)
                return;

            foreach (var civId in removedItems)
                _unitTargetHistory.Remove(civId);
        }

        #region UnitTargetHistoryEntry Structure

        [Serializable]
        protected sealed class UnitTargetHistoryEntry
        {
            private readonly int _targetId;
            private readonly int _ownerId;
            private readonly int _turnNumber;

            public UnitTargetHistoryEntry([NotNull] TUnit target, int turnNumber)
            {
                if (target == null)
                    throw new ArgumentNullException("target");
                if (turnNumber == 0)
                    throw new ArgumentOutOfRangeException("turnNumber", "Turn number was undefined.");

                _targetId = target.ObjectID;
                _ownerId = target.OwnerID;
                _turnNumber = turnNumber;
            }

            public TUnit Target => GameContext.Current.Universe.Objects[_targetId] as TUnit;

            public Civilization Owner => GameContext.Current.Civilizations[_ownerId];

            public int TargetID => _targetId;

            public int OwnerID => _ownerId;

            public int TurnNumber => _turnNumber;
        }

        #endregion
    }
}
