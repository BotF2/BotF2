using System;

using Supremacy.Annotations;

namespace Supremacy.Personnel
{
    [Serializable]
    public class MissionEventArgs : EventArgs
    {
        private readonly Mission _mission;

        public MissionEventArgs([NotNull] Mission mission)
        {
            if (mission == null)
                throw new ArgumentNullException("mission");

            _mission = mission;
        }

        public Mission Mission
        {
            get { return _mission; }
        }
    }
}