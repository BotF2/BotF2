// AudioEngine.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using FMOD;
using Supremacy.Annotations;
using System.Threading;
using Supremacy.Utility;


namespace Supremacy.Client.Audio
{
    public delegate void PlaybackEnd_Callback(IAudioTrack track);

    public interface IAudioTrack : IDisposable
    {
        bool IsPlaying { get; }
        bool IsPaused { get; set; }
        float Volume { get; set; }
        bool Looping { get; set; }
        IAudioGrouping Group { get; set; }

        void Play(PlaybackEnd_Callback callback = null);
        void Stop();
        void FadeIn(float fadeStep);
        void FadeOut(float fadeStep);
    }

    public sealed class FMODAudioTrack : IAudioTrack
    {
        #region Fields
        private bool _isDisposed = false;
        private FMODAudioEngine _engine = null;
        private readonly Sound _sound = null;
        private Channel _channel = null;
        private float _volume = 1.0f;
        private PlaybackEnd_Callback _endCallback = null;
        private FMODGrouping _group = null;
        private CHANNEL_CALLBACK _channelCallbackDelegate;
        #endregion

        #region Properties
        public bool IsPlaying
        {
            get
            {
                if (_channel == null)
                    return false;

                bool playing = false;
                FMODErr.Check(_channel.isPlaying(ref playing));
                return playing;
            }
        }

        public bool IsPaused
        {
            get
            {
                bool paused = false;
                FMODErr.Check(_channel.getPaused(ref paused));
                return paused;
            }
            set
            {
                lock (_engine.Lock)
                {
                    FMODErr.Check(_channel.setPaused(value));
                }
            }
        }

        public float Volume
        {
            get
            {
                if (_channel != null)
                    FMODErr.Check(_channel.getVolume(ref _volume));
                return _volume;
            }
            set
            {
                lock (_engine.Lock)
                {
                    if (_channel != null)
                        FMODErr.Check(_channel.setVolume(value));
                    _volume = value;
                }
            }
        }

        public bool Looping
        {
            get
            {
                lock (_engine.Lock)
                {
                    MODE mode = 0;
                    FMODErr.Check(_sound.getMode(ref mode));
                    return mode.HasFlag(MODE.LOOP_NORMAL);
                }
            }
            set
            {
                lock (_engine.Lock)
                {
                    MODE mode = MODE.HARDWARE | MODE.CREATESTREAM;
                    mode |= value ? MODE.LOOP_NORMAL : MODE.LOOP_OFF;
                    FMODErr.Check(_sound.setMode(mode));
                }
            }
        }

        public IAudioGrouping Group
        {
            get
            {
                return _group;
            }
            set
            {
                lock (_engine.Lock)
                {
                    _group = value as FMODGrouping;
                    if (_channel != null)
                        FMODErr.Check(_channel.setChannelGroup(_group != null ? _group.ChannelGroup : null));
                }
            }
        }
        #endregion

        #region Construction & Lifetime
        /// <summary>
        /// Initializes a new instance of the <see cref="FMODAudioTrack"/> class.
        /// Loads the file and creates sound and channel
        /// </summary>
        internal FMODAudioTrack([NotNull] FMODAudioEngine engine, string filePath)
        {
            _engine = engine;

            MODE creationMode = MODE.HARDWARE | MODE.CREATESTREAM | MODE.LOOP_OFF;
            FMODErr.Check(_engine.System.createSound(filePath, creationMode, ref _sound));

            // keep a reference on the callback delegate for passing to unmanaged code
            _channelCallbackDelegate = new CHANNEL_CALLBACK(OnPlaybackEnd);
        }

        public void Dispose()
        {
            lock (_engine.Lock)
            {
                if (_isDisposed)
                    return;

                _isDisposed = true;

                if (_channel != null)
                {
                    try
                    {
                        Stop();
                        GameLog.Client.Audio.Debug("Stopped");
                    }
                    catch (Exception e)
                    {
                        GameLog.Client.Audio.Error(e);
                    }
                }

                _engine.RemoveTrack(this);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Plays the sound on a free channel
        /// </summary>
        /// <param name="system">The system.</param>
        public void Play(PlaybackEnd_Callback callback = null)
        {
            try
            {
                lock (_engine.Lock)
                {
                    _endCallback = callback;

                    FMODErr.Check(
                        _engine.System.playSound(
                            CHANNELINDEX.FREE,
                            _sound,
                            true,
                            ref _channel));
                    if (_group != null)
                        FMODErr.Check(_channel.setChannelGroup(_group.ChannelGroup));

                    Volume = _volume;
                    _channel.setCallback(CHANNEL_CALLBACKTYPE.END, _channelCallbackDelegate, 0);

                    IsPaused = false;
                }
            }
            catch (Exception e)
            {
                GameLog.Client.Audio.Error(e);
            }
           
        }

        /// <summary>
        /// Stops the playing of the sound on this channel.
        /// </summary>
        public void Stop()
        {
            try
            {
                lock (_engine.Lock)
                {
                    if (_channel != null)
                    {
                        FMODErr.Check(_channel.stop());
                        _channel = null;
                        GameLog.Client.Audio.DebugFormat("No problem at AudioTrack.Stop");
                    }
                }
            }
            catch (Exception e)
            {
                GameLog.Client.Audio.Error(e);
            }
        }

        /// <summary>
        /// Fadings the in track.
        /// Since this value will be added to the current volume, 1 thus represent an instantaneous fade.
        /// </summary>
        /// <param name="fadeStep">The fade step.</param>
        public void FadeIn(float fadeStep)
        {
            try
            {
                lock (_engine.Lock)
                {
                    Volume = Math.Min(Volume + fadeStep, 1.0f);
                }

            }
            catch (Exception e)
            {
                GameLog.Client.Audio.Error(e);
            }
        }

        /// <summary>
        /// Fadings the out track.
        /// </summary>
        /// <param name="fadeStep">The fade step</param>
        /// <returns>the volume was null</returns>
        public void FadeOut(float fadeStep)
        {
            try
            {
                lock (_engine.Lock)
                {
                    Volume = Math.Max(Volume - fadeStep, 0f);
                }
            }
            catch (Exception e)
            {
                GameLog.Client.Audio.Error(e);
            }
        }

        private RESULT OnPlaybackEnd(IntPtr channelraw, CHANNEL_CALLBACKTYPE type, int command, uint commanddata1, uint commanddata2)
        {
            _channel = null;

            try
            {
                if (_endCallback != null)
                {
                    // unlock to avoid deadlocks
                    Monitor.Exit(_engine.Lock);
                    _endCallback(this);
                    Monitor.Enter(_engine.Lock);
                }

                return RESULT.OK;
            }
            catch (Exception e)
            {
                GameLog.Client.Audio.Error(e);
                return RESULT.ERR_DSP_NOTFOUND;
            }
        }
        #endregion
    }
}
