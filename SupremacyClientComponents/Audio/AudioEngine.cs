// AudioEngine.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Concurrency;
using System.IO;
using System.Linq;

using FMOD;
using Supremacy.Utility;

namespace Supremacy.Client.Audio
{
    public class FMODErr
    {
        public static void Check(RESULT result)
        {
            if (result == RESULT.OK)
            {
                return;
            }

            throw new ApplicationException("FMOD error! " + result + " - " + Error.String(result));
        }
    }

    public interface IAudioEngine : IDisposable
    {
        IScheduler Scheduler { get; }
        IAudioGrouping Master { get; }
        float Volume { get; set; }

        IAudioGrouping CreateGrouping(string name);
        IAudioTrack CreateTrack(string fileName);
        void Start();
        void Stop();
        void Update();
    }

    public class FMODAudioEngine : IAudioEngine
    {
        #region Fields
        private const int UpdateInterval = 100; // milli seconds

        private static FMODAudioEngine _instance = null;
        private bool _isDisposed = false;

        private FMOD.System _system = null;
        private FMODGrouping _masterChannelGroup = null;

        private readonly List<FMODGrouping> _channelGroups = new List<FMODGrouping>();
        private readonly List<FMODAudioTrack> _tracks = new List<FMODAudioTrack>();
        private readonly IObservable<long> _updateTimer = null;
        private IDisposable _updateTimerSubscription = null;
        #endregion

        #region Properties
        public static FMODAudioEngine Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FMODAudioEngine();
                }

                return _instance;
            }
        }

        public FMOD.System System => _system;

        public IScheduler Scheduler { get; } = null;

        public IAudioGrouping Master => _masterChannelGroup;

        public float Volume
        {
            get => _masterChannelGroup.Volume;
            set => _masterChannelGroup.Volume = value;
        }

        public object Lock { get; } = new object();
        #endregion

        #region Construction & Lifetime
        private FMODAudioEngine()
        {
            FMODErr.Check(Factory.System_Create(ref _system));

            uint version = 0;
            FMODErr.Check(_system.getVersion(ref version));
            if (version < VERSION.number)
            {
                throw new ApplicationException(
                    "Error! You are using an old version of FMOD "
                    + version.ToString("X")
                    + ". This program requires "
                    + VERSION.number.ToString("X") + ".");
            }

            FMODErr.Check(_system.init(16, INITFLAG.NORMAL, IntPtr.Zero));
            ChannelGroup channelGroup = null;
            FMODErr.Check(_system.getMasterChannelGroup(ref channelGroup));
            _masterChannelGroup = new FMODGrouping(this, channelGroup);

            Scheduler = new EventLoopScheduler("AudioEngine");
            _updateTimer = Observable.Interval(TimeSpan.FromMilliseconds(UpdateInterval), Scheduler).Do(_ => Update());
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            lock (Lock)
            {
                try
                {
                    Stop();
                }
                catch (Exception e)
                {
                    GameLog.Client.Audio.Error(e);
                }

                // reverse iterate to allow remove by Dispose()
                for (int i = _channelGroups.Count - 1; i >= 0; --i)
                {
                    try
                    {
                        _channelGroups[i].Dispose();
                    }
                    catch (Exception e)
                    {
                        GameLog.Client.Audio.Error(e);
                    }
                }
                _channelGroups.Clear();

                // reverse iterate to allow remove by Dispose()
                for (int i = _tracks.Count - 1; i >= 0; --i)
                {
                    try
                    {
                        _tracks[i].Dispose();
                    }
                    catch (Exception e)
                    {
                        GameLog.Client.Audio.Error(e);
                    }
                }
                _tracks.Clear();

                try
                {
                    if (_system != null)
                    {
                        _ = _system.release();
                    }

                    _system = null;
                }
                catch (Exception e)
                {
                    GameLog.Client.Audio.Error(e);
                }
            }
        }
        #endregion

        #region Methods
        public IAudioGrouping CreateGrouping(string name)
        {
            FMODGrouping cg = null;
            lock (Lock)
            {
                cg = new FMODGrouping(this, name);
                cg.Parent = Master;
                _channelGroups.Add(cg);
            }
            return cg;
        }

        public IAudioTrack CreateTrack(string fileName)
        {
            FMODAudioTrack track = null;
            if (File.Exists(fileName))
            {
                lock (Lock)
                {
                    track = new FMODAudioTrack(this, fileName);
                    _tracks.Add(track);
                    GameLog.Client.Audio.DebugFormat("Audio track file \"{0}\" playing", fileName);
                }
            }
            else
            {
                GameLog.Client.Audio.WarnFormat("Could not locate audio track file \"{0}\".",
                    fileName);
            }

            return track;
        }

        public void Start()
        {
            try
            {
                lock (Lock)
                {
                    if (_updateTimerSubscription == null)
                    {
                        _updateTimerSubscription = _updateTimer.Subscribe();
                    }

                    GameLog.Client.Audio.Debug("Starting....");
                }
            }
            catch (Exception e)
            {
                GameLog.Client.Audio.Error(e);
            }
        }

        public void Stop()
        {
            lock (Lock)
            {
                if (_updateTimerSubscription != null)
                {
                    _updateTimerSubscription.Dispose();
                    _updateTimerSubscription = null;
                }

                foreach (FMODAudioTrack track in _tracks)
                {
                    try
                    {
                        track.Stop();
                        GameLog.Client.Audio.DebugFormat("Stopped track {0}", track.ToString());
                    }
                    catch (Exception e) //ToDo: Just log or additional handling necessary?
                    {
                        GameLog.Client.Audio.Error(e);
                    }
                }
            }
        }

        public void Update()
        {
            lock (Lock)
            {
                try
                {
                    _ = _system.update();
                }
                catch
                {
                    //We can ignore a problem here
                }
            }
        }

        internal void RemoveGrouping(FMODGrouping channelGroup)
        {
            try
            {
                _ = _channelGroups.Remove(channelGroup);
            }
            catch (Exception e)
            {
                GameLog.Client.Audio.Error(e);
            }
        }

        internal void RemoveTrack(FMODAudioTrack audioTrack)
        {
            try
            {
                _ = _tracks.Remove(audioTrack);
            }
            catch (Exception e)
            {
                GameLog.Client.Audio.Error(e);
            }
        }
        #endregion
    }
}
