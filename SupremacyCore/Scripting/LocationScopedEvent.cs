using System;
using System.Collections.Generic;

using Supremacy.Collections;
using Supremacy.Game;
using Supremacy.Universe;
using Supremacy.Utility;

namespace Supremacy.Scripting
{
    [Serializable]
    public abstract class LocationScopedEvent : CivilizationScopedEvent
    {
        private readonly KeyedCollectionBase<MapLocation, LocationTargetHistoryEntry> _locationTargetHistory;
        private int _locationRecurrencePeriod = NoRecurrenceLimit;

        protected LocationScopedEvent()
        {
            _locationTargetHistory = new KeyedCollectionBase<MapLocation, LocationTargetHistoryEntry>(entry => entry.Location);
        }

        /// <summary>
        /// The minimum number of turns to elapse between occurrences for a given location.
        /// Use <c>-1</c> to limit the event to one occurrence per location.
        /// </summary>
        protected int LocationRecurrencePeriod
        {
            get { return _locationRecurrencePeriod; }
            private set
            {
                VerifyInitializing();
                _locationRecurrencePeriod = value;
            }
        }

        protected IIndexedKeyedCollection<MapLocation, LocationTargetHistoryEntry> LocationTargetHistory
        {
            get { return _locationTargetHistory; }
        }

        internal sealed override void InitializeCore(IDictionary<string, object> options)
        {
            base.InitializeCore(options);

            object value;

            if (options.TryGetValue("LocationRecurrencePeriod", out value))
            {
                try
                {
                    LocationRecurrencePeriod = Convert.ToInt32(value);
                }
                catch
                {
                    GameLog.Client.GameData.ErrorFormat(
                        "Invalid LocationRecurrencePeriod value for event '{0}': {1}",
                        EventID,
                        value);

                    throw;
                }
            }
        }

        protected virtual bool CanTargetMapLocation(MapLocation location)
        {
            LocationTargetHistoryEntry entry;

            if (!_locationTargetHistory.TryGetValue(location, out entry))
                return true;

            if (LocationRecurrencePeriod < 0)
                return false;

            return (GameContext.Current.TurnNumber - entry.TurnNumber) > LocationRecurrencePeriod;
        }

        protected virtual void OnLocationTargeted(MapLocation location)
        {
            _locationTargetHistory.Remove(location);
            _locationTargetHistory.Add(new LocationTargetHistoryEntry(location, GameContext.Current.TurnNumber));

            RecordExecution();
        }

        protected override void OnTurnFinishedOverride(GameContext game)
        {
            base.OnTurnFinishedOverride(game);

            if (LocationRecurrencePeriod < 0)
                return;

            HashSet<MapLocation> removedItems = null;

            foreach (var entry in _locationTargetHistory)
            {
                if ((GameContext.Current.TurnNumber - entry.TurnNumber) > LocationRecurrencePeriod)
                {
                    if (removedItems == null)
                        removedItems = new HashSet<MapLocation>();
                    removedItems.Add(entry.Location);
                }
            }

            if (removedItems == null)
                return;

            foreach (var civId in removedItems)
                _locationTargetHistory.Remove(civId);
        }

        #region LocationTargetHistoryEntry Structure

        [Serializable]
        protected sealed class LocationTargetHistoryEntry
        {
            private readonly MapLocation _location;
            private readonly TurnNumber _turnNumber;

            public LocationTargetHistoryEntry(MapLocation location, TurnNumber turnNumber)
            {
                if (turnNumber.IsUndefined)
                    throw new ArgumentOutOfRangeException("turnNumber", "Turn number was undefined.");

                _location = location;
                _turnNumber = turnNumber;
            }

            public MapLocation Location
            {
                get { return _location; }
            }

            public TurnNumber TurnNumber
            {
                get { return _turnNumber; }
            }
        }

        #endregion
    }
}