using System;
using System.Collections.Generic;
using System.Linq;
using Supremacy.Annotations;
using Supremacy.Resources;
using Supremacy.Client.Context;
using Supremacy.Utility;

namespace Supremacy.Client.Audio
{
    [Flags]
    public enum PlaybackMode
    {
        None = 0,
        Sequential = 1,
        Random = 1 << 1,
        Loop = 1 << 2,
        Fade = 1 << 3
    }

    public interface IMusicPlayer : IDisposable
    {
        float Volume { get; set; }
        PlaybackMode PlayMode { get; set; }
        float FadeTime { get; set; }
        bool IsPlaying { get; }
        MusicEntry CurrentMusicEntry { get; }
        IAudioTrack CurrentAudioTrack { get; }

        void LoadMusic(MusicPack musicPack, string trackName = null);
        void SwitchMusic(string packName);
        void Play();
        bool Switch(string trackName);
        void Stop();
        void Next();
        void Prev();
        void Update();
    }

    public class MusicPlayer : IMusicPlayer
    {
        #region Fields
        private const int UpdateInterval = 40; // milli seconds
        private const float DefaultFadeTime = 2.0f; // seconds

        private bool _isDisposed = false;
        private readonly object _updateLock = new object();
        private IAppContext _appContext = null;
        private IAudioEngine _engine = null;
        private IAudioGrouping _channelGroup = null;
        private MusicPack _musicPack = null;
        private KeyValuePair<int, MusicEntry> _musicEntry;
        private IAudioTrack _audioTrack = null;
        private List<IAudioTrack> _endingTracks = new List<IAudioTrack>();
        private PlaybackMode _playMode = PlaybackMode.None;
        private bool _isPlaying = false;

        private float _fadeTime = DefaultFadeTime;
        private readonly IObservable<long> _updateTimer = null;
        private IDisposable _updateTimerSubscription = null;
        #endregion

        #region Properties
        public float Volume
        {
            get { return _channelGroup.Volume; }
            set { _channelGroup.Volume = value; }
        }

        public PlaybackMode PlayMode
        {
            get { return _playMode; }
            set { _playMode = value; }
        }

        public float FadeTime
        {
            get { return _fadeTime; }
            set { _fadeTime = value; }
        }

        public float FadeFactor
        {
            get { return UpdateInterval / (1000.0f * _fadeTime); }
            set { _fadeTime = UpdateInterval / (1000.0f * value); }
        }

        public bool IsPlaying => _isPlaying;
        public MusicEntry CurrentMusicEntry => _musicEntry.Value;
        public IAudioTrack CurrentAudioTrack => _audioTrack;
        #endregion

        #region Construction & Lifetime
        public MusicPlayer([NotNull] IAudioEngine engine, [NotNull] IAppContext appContext)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");
            if (appContext == null)
                throw new ArgumentNullException("appContext");

            _engine = engine;
            _appContext = appContext;
            _channelGroup = _engine.CreateGrouping("music");
            _updateTimer = Observable.Interval(TimeSpan.FromMilliseconds(UpdateInterval), _engine.Scheduler).Do(_ => Update());
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            lock (_updateLock)
            {
                if (_updateTimerSubscription != null)
                {
                    _updateTimerSubscription.Dispose();
                    _updateTimerSubscription = null;
                }

                if (_audioTrack != null)
                {
                    try
                    {
                        _audioTrack.Stop();
                        _audioTrack.Dispose();
                        _audioTrack = null;
                    }
                    catch (Exception e)
                    {
                        GameLog.Client.Audio.Error(e);
                    }
                }

                foreach (IAudioTrack track in _endingTracks)
                {
                    try
                    {
                        track.Stop();
                        track.Dispose();
                    }
                    catch (Exception e)
                    {
                        GameLog.Client.Audio.Error(e);
                    }
                }
                _endingTracks.Clear();

                if (_channelGroup != null)
                {
                    _channelGroup.Dispose();
                    _channelGroup = null;
                }
                _engine = null;
            }
        }
        #endregion

        #region Methods
        public void LoadMusic(MusicPack musicPack, string trackName = null)
        {
            try
            {
                lock (_updateLock)
                {
                    bool play = _isPlaying;
                    Stop();

                    _musicPack = musicPack;

                    if (trackName != null)
                        _musicEntry = _musicPack.FindByName(trackName);

                    if (trackName == null || _musicEntry.Value == null)
                    {
                        if (_playMode.HasFlag(PlaybackMode.Random))
                            _musicEntry = _musicPack.Random();
                        else if (_playMode.HasFlag(PlaybackMode.Sequential))
                            _musicEntry = _musicPack.Next();
                    }

                    if (play && _musicEntry.Value != null)
                    {
                        GameLog.Client.Audio.DebugFormat("called! Trackname: {0}, {1}, playMode={2}", _musicPack.Name, _musicEntry.Value.FileName, _playMode.ToString());
                        Play();
                    }
                }
            }
            catch (Exception e)
            {
                GameLog.Client.Audio.Error(e);
            }
        }

        public void SwitchMusic(string packName)
        {
            try
            {
                MusicPack pack;
                if (_appContext.ThemeMusicLibrary.MusicPacks.TryGetValue(packName, out pack) && pack.HasEntries()
                    || _appContext.DefaultMusicLibrary.MusicPacks.TryGetValue(packName, out pack) && pack.HasEntries())
                    LoadMusic(pack);
            }
            catch (Exception e)
            {
                GameLog.Client.Audio.Error(e);
            }
        }

        public void Play()
        {
            try
            {
                lock (_updateLock)
                {
                    Stop();
                    _isPlaying = true;

                    if (_musicEntry.Value != null)
                    {
                        _audioTrack = _engine.CreateTrack(
                            ResourceManager.GetResourcePath(_musicEntry.Value.FileName));

                        GameLog.Client.Audio.DebugFormat("called! _musicEntry.Value.FileName: {0}", _musicEntry.Value.FileName);

                        if (_audioTrack != null)
                        {
                            _audioTrack.Group = _channelGroup;
                            _audioTrack.Play(OnTrackEnd);

                            if (_updateTimerSubscription == null)
                                _updateTimerSubscription = _updateTimer.Subscribe();
                        }
                    }
                }

            }
            catch (Exception e)
            {
                GameLog.Client.Audio.Error(e);
            }
        }

        public bool Switch(string trackName)
        {
            try
            {
                lock (_updateLock)
                {
                    // TODO: restart track if already played?
                    if (_musicEntry.Value != null && _musicEntry.Value.TrackName.ToUpper().Equals(trackName.ToUpper()))
                    {
                        GameLog.Client.Audio.Debug("Switch = true (1)");
                        return true;
                    }

                    KeyValuePair<int, MusicEntry> newTrack = _musicPack.FindByName(trackName);
                    if (newTrack.Value == null)
                        return false;

                    bool play = _isPlaying;
                    Stop();
                    _musicEntry = newTrack;

                    if (play) Play();
                    {
                        GameLog.Client.Audio.DebugFormat("Switch = true (1), _musicPack={0}, _musicEntry={1}", _musicPack.Name, _musicEntry.Key);
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                GameLog.Client.Audio.Error(e);
                return false;
            }
        }

        public void Stop()
        {
            try
            {
                lock (_updateLock)
                {
                    _isPlaying = false;

                    if (_audioTrack == null)
                        return;

                    if (!_audioTrack.IsPlaying)
                    {
                        try
                        {
                            _audioTrack.Stop();
                            GameLog.Client.Audio.DebugFormat("Stop - Group={0}, Track={1}", _audioTrack.Group.ToString(), _audioTrack);
                            _audioTrack.Dispose();
                            _audioTrack = null;
                        }
                        catch (Exception e)
                        {
                            GameLog.Client.Audio.Error(e);
                        }
                    }
                    else
                    {
                        _endingTracks.Add(_audioTrack);
                        _audioTrack = null;
                    }
                }
            }
            catch (Exception e)
            {
                GameLog.Client.Audio.Error(e);
            }
        }

        public void Next()
        {
            try
            {
                lock (_updateLock)
                {
                    if (_musicPack == null)
                        return;

                    if (_playMode.HasFlag(PlaybackMode.Random))
                        _musicEntry = _musicPack.Random(_musicEntry.Key);
                    else if (_playMode.HasFlag(PlaybackMode.Sequential))
                        _musicEntry = _musicPack.Next(_musicEntry.Key);

                    GameLog.Client.Audio.DebugFormat("Next at _musicPack={0}, _musicEntry={1}", _musicPack.Name, _musicEntry.Value.FileName);

                    Play();
                }
            }
            catch (Exception e)
            {
                GameLog.Client.Audio.Error(e);
            }
        }

        public void Prev()
        {
            try
            {
                lock (_updateLock)
                {
                    if (_musicPack == null)
                        return;

                    if (_playMode.HasFlag(PlaybackMode.Random))
                        _musicEntry = _musicPack.Random(_musicEntry.Key);
                    else if (_playMode.HasFlag(PlaybackMode.Sequential))
                        _musicEntry = _musicPack.Prev(_musicEntry.Key);

                    GameLog.Client.Audio.DebugFormat("Prev at _musicPack={0}, _musicEntry={1}", _musicPack.Name, _musicEntry.Value.FileName);
                    Play();
                }
            }
            catch (Exception e)
            {
                GameLog.Client.Audio.Error(e);
            }
        }

        private void OnTrackEnd(IAudioTrack track)
        {
            try
            {
                lock (_updateLock)
                {
                    try
                    {
                        track.Dispose();
                    }
                    catch (Exception e)
                    {
                        GameLog.Client.Audio.Error(e);
                    }

                    if (track == _audioTrack)
                    {
                        _audioTrack = null;
                        Next();
                    }
                    else _endingTracks.Remove(track);

                    if (_audioTrack == null && _endingTracks.Count == 0)
                    {
                        if (_updateTimerSubscription != null)
                        {
                            _updateTimerSubscription.Dispose();
                            _updateTimerSubscription = null;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                GameLog.Client.Audio.Error(e);
            }
        }

        public void Update()
        {
            try
            {
                lock (_updateLock)
                {
                    if (_audioTrack != null && _audioTrack.IsPlaying && _audioTrack.Volume < 1.0f)
                        _audioTrack.FadeIn(FadeFactor / _fadeTime);

                    for (int i = _endingTracks.Count - 1; i >= 0; --i)
                    {
                        IAudioTrack track = _endingTracks[i];
                        track.FadeOut(FadeFactor / _fadeTime);
                        if (track.Volume <= 0.0f)
                        {
                            track.Dispose();
                            _endingTracks.RemoveAt(i);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                GameLog.Client.Audio.Error(e);
            }
        }
    }
    #endregion

}

