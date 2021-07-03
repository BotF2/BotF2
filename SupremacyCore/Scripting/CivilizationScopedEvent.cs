// 
// CivilizationScopedEvent.cs
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
using Supremacy.IO.Serialization;
using Supremacy.Utility;

namespace Supremacy.Scripting
{
    [Serializable]
    public abstract class CivilizationScopedEvent : ScriptedEvent
    {
        private readonly CivilizationKeyedMap<CivTargetHistoryEntry> _civilizationTargetHistory;
        private int _civilizationRecurrencePeriod = NoRecurrenceLimit;

        protected CivilizationScopedEvent()
        {
            _civilizationTargetHistory = new CivilizationKeyedMap<CivTargetHistoryEntry>(entry => entry.CivID);
        }

        /// <summary>
        /// The minimum number of turns to elapse between occurrences for a given civilization.
        /// Use <c>-1</c> to limit the event to one occurrence per civilization.
        /// </summary>
        protected int CivilizationRecurrencePeriod
        {
            get => _civilizationRecurrencePeriod;
            private set
            {
                VerifyInitializing();
                _civilizationRecurrencePeriod = value;
            }
        }

        protected IKeyedCollection<int, CivTargetHistoryEntry> CivilizationTargetHistory => _civilizationTargetHistory;

        protected virtual bool CanTargetCivilization([NotNull] Civilization civ)
        {
            if (civ == null)
            {
                throw new ArgumentNullException("civ");
            }


            if (!_civilizationTargetHistory.TryGetValue(civ.CivID, out CivTargetHistoryEntry entry))
            {
                return true;
            }

            if (CivilizationRecurrencePeriod < 0)
            {
                return false;
            }

            return (GameContext.Current.TurnNumber - entry.TurnNumber) > CivilizationRecurrencePeriod;
        }

        protected virtual void OnCivilizationTargeted([NotNull] Civilization civ)
        {
            if (civ == null)
            {
                throw new ArgumentNullException("civ");
            }

            if (CivilizationRecurrencePeriod == 0)
            {
                return;
            }

            _ = _civilizationTargetHistory.Remove(civ.CivID);
            _civilizationTargetHistory.Add(new CivTargetHistoryEntry(civ, GameContext.Current.TurnNumber));

            RecordExecution();
        }

        internal override void InitializeCore(IDictionary<string, object> options)
        {
            base.InitializeCore(options);


            if (options.TryGetValue("CivilizationRecurrencePeriod", out object value))
            {
                try
                {
                    CivilizationRecurrencePeriod = Convert.ToInt32(value);
                }
                catch
                {
                    GameLog.Client.GameData.ErrorFormat(
                        "Invalid CivilizationRecurrencePeriod value for event '{0}': {1}",
                        EventID,
                        value);

                    throw;
                }
            }
        }

        protected override void OnTurnFinishedOverride(GameContext game)
        {
            if (CivilizationRecurrencePeriod < 0)
            {
                return;
            }

            HashSet<int> removedItems = null;

            foreach (CivTargetHistoryEntry entry in _civilizationTargetHistory)
            {
                if ((GameContext.Current.TurnNumber - entry.TurnNumber) > CivilizationRecurrencePeriod)
                {
                    if (removedItems == null)
                    {
                        removedItems = new HashSet<int>();
                    }

                    _ = removedItems.Add(entry.CivID);
                }
            }

            if (removedItems == null)
            {
                return;
            }

            foreach (int civId in removedItems)
            {
                _ = _civilizationTargetHistory.Remove(civId);
            }
        }

        #region CivTargetHistoryEntry Structure

        [Serializable]
        protected sealed class CivTargetHistoryEntry : IOwnedDataSerializableAndRecreatable
        {
            private int _civId;
            private int _turnNumber;

            public CivTargetHistoryEntry([NotNull] Civilization civ, int turnNumber)
            {
                if (civ == null)
                {
                    throw new ArgumentNullException("civ");
                }

                if (turnNumber == 0)
                {
                    throw new ArgumentOutOfRangeException("turnNumber", "Turn number was undefined.");
                }

                _civId = civ.CivID;
                _turnNumber = turnNumber;
            }

            public int CivID => _civId;

            public Civilization Civilization => GameContext.Current.Civilizations[_civId];

            public int TurnNumber => _turnNumber;

            void IOwnedDataSerializable.DeserializeOwnedData(SerializationReader reader, object context)
            {
                _civId = reader.ReadOptimizedInt32();
                _turnNumber = reader.ReadOptimizedInt32();
            }

            void IOwnedDataSerializable.SerializeOwnedData(SerializationWriter writer, object context)
            {
                writer.WriteOptimized(_civId);
                writer.WriteOptimized(_turnNumber);
            }
        }

        #endregion
    }
}
