// ScriptedEvent.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;

using Supremacy.Annotations;
using Supremacy.Game;
using Supremacy.Types;
using Supremacy.Utility;

namespace Supremacy.Scripting
{
    /// <summary>
    /// Base class for random events.
    /// </summary>
    [Serializable]
    public abstract class ScriptedEvent : SupportInitializeBase, IGameTurnListener, IDisposable
    {
        /// <summary>
        /// Value indicating that no limit should be placed on recurrences.
        /// </summary>
        public const int NoRecurrenceLimit = 0;

        /// <summary>
        /// Value indicating that no recurrences should be allowed.
        /// </summary>
        public const int NoRecurrences = -1;

        private int _lastExecution = 0;
        private int _minTurnsBetweenExecutions = NoRecurrenceLimit;
        private bool _isDisposed;

        public string EventID { get; private set; }

        protected int MinTurnsBetweenExecutions
        {
            get { return _minTurnsBetweenExecutions; }
            set
            {
                VerifyInitializing();
                _minTurnsBetweenExecutions = value;
            }
        }

        public int LastExecution
        {
            get { return _lastExecution; }
        }

        public virtual bool CanExecute
        {
            get
            {
                if (MinTurnsBetweenExecutions < 0)
                    return LastExecution != 0;

                return MinTurnsBetweenExecutions == 0 ||
                       LastExecution == 0 ||
                       GameContext.Current.TurnNumber - LastExecution > MinTurnsBetweenExecutions;
            }
        }

        protected virtual Random RandomProvider
        {
            get { return Supremacy.Utility.RandomProvider.Shared; }
        }

        protected void RecordExecution()
        {
            _lastExecution = GameContext.Current.TurnNumber;
        }

        public void Initialize([NotNull] string eventId, [NotNull] IDictionary<string, object> options)
        {
            if (eventId == null)
                throw new ArgumentNullException("eventId");
            if (options == null)
                throw new ArgumentNullException("options");

            BeginInit();

            try
            {
                EventID = eventId;
                InitializeCore(options);
            }
            finally
            {
                EndInit();
            }
        }

        internal virtual void InitializeCore(IDictionary<string, object> options)
        {
            object value;

            if (options.TryGetValue("MinTurnsBetweenExecutions", out value))
            {
                try
                {
                    MinTurnsBetweenExecutions = Convert.ToInt32(value);
                }
                catch
                {
                    GameLog.Client.GameData.ErrorFormat(
                        "Invalid MinTurnsBetweenExecutions value for event '{0}': {1}",
                        EventID,
                        value);

                    throw;
                }
            }

            InitializeOverride(options);
        }

        protected virtual void InitializeOverride(IDictionary<string, object> options) { }

        protected virtual void OnTurnStartedOverride(GameContext game) { }
        protected virtual void OnTurnPhaseStartedOverride(GameContext game, TurnPhase phase) { }
        protected virtual void OnTurnPhaseFinishedOverride(GameContext game, TurnPhase phase) { }
        protected virtual void OnTurnFinishedOverride(GameContext game) { }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            try { DisposeOverride(); }
            finally { _isDisposed = true; }
        }

        protected virtual void DisposeOverride() { }

        #region Implementation of IGameTurnListener
        public void OnTurnStarted(GameContext game)
        {
            OnTurnStartedOverride(game);
        }

        public void OnTurnPhaseStarted(GameContext game, TurnPhase phase)
        {
            OnTurnPhaseStartedOverride(game, phase);
        }

        public void OnTurnPhaseFinished(GameContext game, TurnPhase phase)
        {
            OnTurnPhaseFinishedOverride(game, phase);
        }

        public void OnTurnFinished(GameContext game)
        {
            OnTurnFinishedOverride(game);
        }
        #endregion
    }
}