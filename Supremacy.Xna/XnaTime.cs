using System;

namespace Supremacy.Xna
{
    public class XnaTime
    {
        public XnaTime() { }

        public XnaTime(TimeSpan totalRealTime, TimeSpan elapsedRealTime, TimeSpan totalGameTime, TimeSpan elapsedGameTime)
            : this(totalRealTime, elapsedRealTime, totalGameTime, elapsedGameTime, false) { }

        public XnaTime(TimeSpan totalRealTime, TimeSpan elapsedRealTime, TimeSpan totalGameTime, TimeSpan elapsedGameTime, bool isRunningSlowly)
        {
            TotalRealTime = totalRealTime;
            ElapsedRealTime = elapsedRealTime;
            TotalGameTime = totalGameTime;
            ElapsedGameTime = elapsedGameTime;
            IsRunningSlowly = isRunningSlowly;
        }

        public TimeSpan ElapsedGameTime { get; internal set; }
        public TimeSpan ElapsedRealTime { get; internal set; }
        public bool IsRunningSlowly { get; internal set; }
        public TimeSpan TotalGameTime { get; internal set; }
        public TimeSpan TotalRealTime { get; internal set; }
    }
}