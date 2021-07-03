using System;
using System.Diagnostics;

namespace Supremacy.Xna
{
    internal class XnaClock
    {
        private long _baseRealTime;
        private TimeSpan _currentTimeBase;
        private TimeSpan _currentTimeOffset;
        private TimeSpan _elapsedAdjustedTime;
        private TimeSpan _elapsedTime;
        private long _lastRealTime;
        private bool _lastRealTimeValid;
        private int _suspendCount;
        private long _suspendStartTime;
        private long _timeLostToSuspension;

        public XnaClock()
        {
            Reset();
        }

        private static TimeSpan CounterToTimeSpan(long delta)
        {
            long ticks = delta * 0x989680L / Frequency;
            return TimeSpan.FromTicks(ticks);
        }

        internal void Reset()
        {
            _currentTimeBase = TimeSpan.Zero;
            _currentTimeOffset = TimeSpan.Zero;
            _baseRealTime = Counter;
            _lastRealTimeValid = false;
        }

        internal void Resume()
        {
            _suspendCount--;

            if (_suspendCount > 0)
            {
                return;
            }

            long counter = Counter;

            _timeLostToSuspension += counter - _suspendStartTime;
            _suspendStartTime = 0L;
        }

        internal void Step()
        {
            long counter = Counter;

            if (!_lastRealTimeValid)
            {
                _lastRealTime = counter;
                _lastRealTimeValid = true;
            }

            try
            {
                _currentTimeOffset = CounterToTimeSpan(counter - _baseRealTime);
            }
            catch (OverflowException)
            {
                _currentTimeBase += _currentTimeOffset;
                _baseRealTime = _lastRealTime;

                try
                {
                    _currentTimeOffset = CounterToTimeSpan(counter - _baseRealTime);
                }
                catch (OverflowException)
                {
                    _baseRealTime = counter;
                    _currentTimeOffset = TimeSpan.Zero;
                }
            }

            try
            {
                _elapsedTime = CounterToTimeSpan(counter - _lastRealTime);
            }
            catch (OverflowException)
            {
                _elapsedTime = TimeSpan.Zero;
            }

            try
            {
                long adjustedTime = _lastRealTime + _timeLostToSuspension;
                _elapsedAdjustedTime = CounterToTimeSpan(counter - adjustedTime);
                _timeLostToSuspension = 0L;
            }
            catch (OverflowException)
            {
                _elapsedAdjustedTime = TimeSpan.Zero;
            }

            _lastRealTime = counter;
        }

        internal void Suspend()
        {
            _suspendCount++;

            if (_suspendCount == 1)
            {
                _suspendStartTime = Counter;
            }
        }

        internal static long Counter => Stopwatch.GetTimestamp();

        internal TimeSpan CurrentTime => _currentTimeBase + _currentTimeOffset;

        internal TimeSpan ElapsedAdjustedTime => _elapsedAdjustedTime;

        internal TimeSpan ElapsedTime => _elapsedTime;

        internal static long Frequency => Stopwatch.Frequency;
    }
}