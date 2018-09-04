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
                return;

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

        private bool _audioTraceLocally = false;

        private FMOD.System _system = null;
        private FMODGrouping _masterChannelGroup = null;

        private readonly List<FMODGrouping> _channelGroups = new List<FMODGrouping>();
        private readonly List<FMODAudioTrack> _tracks = new List<FMODAudioTrack>();

        private readonly object _updateLock = new object();
        private readonly IScheduler _scheduler = null;
        private readonly IObservable<long> _updateTimer = null;
        private IDisposable _updateTimerSubscription = null;
        #endregion

        #region Properties
        public static FMODAudioEngine Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new FMODAudioEngine();
                return _instance;
            }
        }

        public FMOD.System System
        {
            get { return _system; }
        }
        
        public IScheduler Scheduler
        {
            get { return _scheduler; }
        }

        public IAudioGrouping Master
        {
            get { return _masterChannelGroup; }
        }

        public float Volume
        {
            get { return _masterChannelGroup.Volume; }
            set { _masterChannelGroup.Volume = value; }
        }

        public object Lock
        {
            get { return _updateLock; }
        }
        #endregion

        #region Construction & Lifetime
        private FMODAudioEngine()
        {
            if (_audioTraceLocally)
                GameLog.Print("called!");

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

            _scheduler = new EventLoopScheduler("AudioEngine");
            _updateTimer = Observable.Interval(TimeSpan.FromMilliseconds(UpdateInterval), _scheduler).Do(_ => Update());
        }

        public void Dispose()
        {
            if (_audioTraceLocally)
                GameLog.Print("called!");

            if (_isDisposed)
                return;

            _isDisposed = true;

            lock (_updateLock)
            {
                try
                {
                    Stop();
                }
                catch (Exception e) //ToDo: Just log or additional handling necessary?
                {
                    GameLog.LogException(e);
                }

                // reverse iterate to allow remove by Dispose()
                for ( int i = _channelGroups.Count - 1; i >= 0; --i )
                {
                    try
                    {
                        _channelGroups[i].Dispose();
                    }
                    catch (Exception e) //ToDo: Just log or additional handling necessary?
                    {
                        GameLog.LogException(e);
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
                    catch (Exception e) //ToDo: Just log or additional handling necessary?
                    {
                        GameLog.LogException(e);
                    }
                }
                _tracks.Clear();

                try
                {
                    if (_system != null)
                        _system.release();
                    _system = null;
                }
                catch (Exception e) //ToDo: Just log or additional handling necessary?
                {
                    GameLog.LogException(e);
                }
            }
        }
        #endregion

        #region Methods
        public IAudioGrouping CreateGrouping(string name)
        {
            if (_audioTraceLocally)
                GameLog.Print("called!");

            FMODGrouping cg = null;
            lock (_updateLock)
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
                lock (_updateLock)
                {
                    track = new FMODAudioTrack(this, fileName);
                    _tracks.Add(track);
                    if (_audioTraceLocally)
                        GameLog.Print("called: audio track file \"{0}\" playing", fileName);
                }
            }
            else GameLog.Client.GameData.DebugFormat(
                "Could not locate audio track file \"{0}\".",
                fileName);

            return track;
        }

        public void Start()
        {
            try
            {
                lock (_updateLock)
                {
                    if (_updateTimerSubscription == null)
                        _updateTimerSubscription = _updateTimer.Subscribe();

                    if (_audioTraceLocally)
                        GameLog.Print("######### AudioEngine.Start - starting....");
                }
            }
            catch
            {
                GameLog.Print("######### problem at AudioEngine.Start");
            }
        }

        public void Stop()
        {
            lock (_updateLock)
            {
                if (_updateTimerSubscription != null)
                {
                    _updateTimerSubscription.Dispose();
                    _updateTimerSubscription = null;
                }

                foreach (var track in _tracks)
                {
                    try
                    {
                        track.Stop();
                        if (_audioTraceLocally)
                            GameLog.Print("######### AudioEngine.Stop - stopped track={0}", track.ToString());
                    }
                    catch (Exception e) //ToDo: Just log or additional handling necessary?
                    {
                        GameLog.Print("######### problem at AudioEngine.Stop");
                        GameLog.LogException(e);
                    }
                }
            }
        }

        public void Update()
        {
            lock (_updateLock)
            {
                try
                {
                    _system.update();
                }
                catch
                {
                    GameLog.Print("unimportant problem at AudioEngine");
                }
            }
        }
        
        internal void RemoveGrouping(FMODGrouping channelGroup)
        {
            if (_audioTraceLocally)
                GameLog.Print("called!");
            try
            {
                _channelGroups.Remove(channelGroup);
            }
            catch
            {
                GameLog.Print("######### problem at AudioEngine.RemoveGrouping");
            }
        }

        internal void RemoveTrack(FMODAudioTrack audioTrack)
        {
            if (_audioTraceLocally)
                GameLog.Print("called!");

            try
            {
                _tracks.Remove(audioTrack);
            }
            catch
            {
                GameLog.Print("######### problem at AudioEngine.RemoveTrack");
            }
        }
        #endregion
    }
}
