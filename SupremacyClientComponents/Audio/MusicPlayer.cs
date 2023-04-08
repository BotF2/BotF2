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
        private readonly IAppContext _appContext = null;
        private IAudioEngine _engine = null;
        private IAudioGrouping _channelGroup = null;
        private MusicPack _musicPack = null;
        private KeyValuePair<int, MusicEntry> _musicEntry;
        private readonly List<IAudioTrack> _endingTracks = new List<IAudioTrack>();
        private readonly IObservable<long> _updateTimer = null;
        private IDisposable _updateTimerSubscription = null;
        private string _text;
        #endregion

        #region Properties
        public float Volume
        {
            get => _channelGroup.Volume;
            set => _channelGroup.Volume = value;
        }

        public PlaybackMode PlayMode { get; set; } = PlaybackMode.None;

        public float FadeTime { get; set; } = DefaultFadeTime;

        public float FadeFactor
        {
            get => UpdateInterval / (1000.0f * FadeTime);
            set => FadeTime = UpdateInterval / (1000.0f * value);
        }

        public bool IsPlaying { get; private set; } = false;
        public MusicEntry CurrentMusicEntry => _musicEntry.Value;
        public IAudioTrack CurrentAudioTrack { get; private set; } = null;
        #endregion

        #region Construction & Lifetime
        public MusicPlayer([NotNull] IAudioEngine engine, [NotNull] IAppContext appContext)
        {
            _engine = engine ?? throw new ArgumentNullException("engine");
            _appContext = appContext ?? throw new ArgumentNullException("appContext");
            _channelGroup = _engine.CreateGrouping("music");
            _updateTimer = Observable.Interval(TimeSpan.FromMilliseconds(UpdateInterval), _engine.Scheduler).Do(_ => Update());
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            lock (_updateLock)
            {
                if (_updateTimerSubscription != null)
                {
                    _updateTimerSubscription.Dispose();
                    _updateTimerSubscription = null;
                }

                if (CurrentAudioTrack != null)
                {
                    try
                    {
                        CurrentAudioTrack.Stop();
                        CurrentAudioTrack.Dispose();
                        CurrentAudioTrack = null;
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
                    bool play = IsPlaying;
                    Stop();

                    _musicPack = musicPack;

                    for (int i = 0; i < musicPack.Entries.Count; i++)
                    {
                        Console.WriteLine(i+1 + " > " + musicPack.Entries[i].FileName);
                    } 

                    if (trackName != null)
                    {
                        _musicEntry = _musicPack.FindByName(trackName);
                    }

                    if (trackName == null || _musicEntry.Value == null)
                    {
                        if (PlayMode.HasFlag(PlaybackMode.Random))
                        {
                            _musicEntry = _musicPack.Random();
                        }
                        else if (PlayMode.HasFlag(PlaybackMode.Sequential))
                        {
                            _musicEntry = _musicPack.Next();
                        }
                    }

                    if (play && _musicEntry.Value != null)
                    {
                        GameLog.Client.Audio.DebugFormat("called! Trackname: {0}, {1}, playMode={2}", _musicPack.Name, _musicEntry.Value.FileName, PlayMode.ToString());
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
                if (_appContext.ThemeMusicLibrary.MusicPacks.TryGetValue(packName, out MusicPack pack) && pack.HasEntries()
                    || _appContext.DefaultMusicLibrary.MusicPacks.TryGetValue(packName, out pack) && pack.HasEntries())
                {
                    LoadMusic(pack);
                }
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
                    IsPlaying = true;

                    if (_musicEntry.Value != null)
                    {
                        CurrentAudioTrack = _engine.CreateTrack(
                            ResourceManager.GetResourcePath(_musicEntry.Value.FileName));

                        GameLog.Client.Audio.DebugFormat("called! _musicEntry.Value.FileName: {0}", _musicEntry.Value.FileName);

                        if (CurrentAudioTrack != null)
                        {
                            CurrentAudioTrack.Group = _channelGroup;
                            CurrentAudioTrack.Play(OnTrackEnd);

                            if (_updateTimerSubscription == null)
                            {
                                _updateTimerSubscription = _updateTimer.Subscribe();
                            }
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
                    {
                        return false;
                    }

                    bool play = IsPlaying;
                    Stop();
                    _musicEntry = newTrack;

                    if (play)
                    {
                        Play();
                    }

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
                    IsPlaying = false;

                    if (CurrentAudioTrack == null)
                    {
                        return;
                    }

                    if (!CurrentAudioTrack.IsPlaying)
                    {
                        try
                        {
                            CurrentAudioTrack.Stop();
                            GameLog.Client.Audio.DebugFormat("Stop - Group={0}, Track={1}", CurrentAudioTrack.Group.ToString(), CurrentAudioTrack);
                            CurrentAudioTrack.Dispose();
                            CurrentAudioTrack = null;
                        }
                        catch (Exception e)
                        {
                            GameLog.Client.Audio.Error(e);
                        }
                    }
                    else
                    {
                        _endingTracks.Add(CurrentAudioTrack);
                        CurrentAudioTrack = null;
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
                    {
                        return;
                    }

                    if (PlayMode.HasFlag(PlaybackMode.Random))
                    {
                        _musicEntry = _musicPack.Random(_musicEntry.Key);
                    }
                    else if (PlayMode.HasFlag(PlaybackMode.Sequential))
                    {
                        _musicEntry = _musicPack.Next(_musicEntry.Key);
                    }

                    //GameLog.Client.Audio.DebugFormat("Next at _musicPack={0}, _musicEntry={1}", _musicPack.Name, _musicEntry.Value.FileName);

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
                    {
                        return;
                    }

                    if (PlayMode.HasFlag(PlaybackMode.Random))
                    {
                        _musicEntry = _musicPack.Random(_musicEntry.Key);
                    }
                    else if (PlayMode.HasFlag(PlaybackMode.Sequential))
                    {
                        _musicEntry = _musicPack.Prev(_musicEntry.Key);
                    }

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

                    if (track == CurrentAudioTrack)
                    {
                        CurrentAudioTrack = null;
                        Next();
                    }
                    else
                    {
                        _ = _endingTracks.Remove(track);
                    }

                    if (CurrentAudioTrack == null && _endingTracks.Count == 0)
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
                _text = "ERROR on OnTrackEnd...";
                Console.WriteLine(_text);
                GameLog.Client.Audio.Error(_text + e);
            }
        }

        public void Update()
        {
            try
            {
                lock (_updateLock)
                {
                    if (CurrentAudioTrack != null && CurrentAudioTrack.IsPlaying && CurrentAudioTrack.Volume < 1.0f)
                    {
                        CurrentAudioTrack.FadeIn(FadeFactor / FadeTime);
                    }

                    for (int i = _endingTracks.Count - 1; i >= 0; --i)
                    {
                        IAudioTrack track = _endingTracks[i];
                        track.FadeOut(FadeFactor / FadeTime);
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

