using System;

using Supremacy.Annotations;

namespace Supremacy.Personnel
{
    [Serializable]
    public class MissionPhaseChangedEventArgs : MissionEventArgs
    {
        private readonly MissionPhase _oldPhase;
        private readonly MissionPhase _newPhase;

        public MissionPhaseChangedEventArgs([NotNull] MissionPhase oldPhase, [NotNull] MissionPhase newPhase)
            : base(oldPhase.Mission)
        {
            if (oldPhase == null)
                throw new ArgumentNullException("oldPhase");
            if (newPhase == null)
                throw new ArgumentNullException("newPhase");

            _oldPhase = oldPhase;
            _newPhase = newPhase;
        }

        [NotNull]
        public MissionPhase OldPhase
        {
            get { return _oldPhase; }
        }

        [NotNull]
        public MissionPhase NewPhase
        {
            get { return _newPhase; }
        }
    }
}