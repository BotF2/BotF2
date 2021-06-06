using System;
using System.Collections.Generic;
using Supremacy.Annotations;
using Supremacy.Resources;
using System.IO;
using Supremacy.Utility;
using Supremacy.Client.Context;

namespace Supremacy.Client.Audio
{
    public interface ISoundPlayer : IDisposable
    {
        float Volume { get; set; }

        void Play(string pack, string sound);
        void PlayAny(string pack);
        void PlayFile(string fileName);
    }

    public class SoundPlayer : ISoundPlayer
    {
        #region Fields
        private bool _isDisposed = false;
        private readonly object _updateLock = new object();
        private IAudioEngine _engine = null;
        private IAppContext _appContext = null;
        private IAudioGrouping _channelGroup = null;
        private List<IAudioTrack> _audioTracks = new List<IAudioTrack>();
        #endregion

        #region Properties
        public float Volume
        {
            get { return _channelGroup.Volume; }
            set { _channelGroup.Volume = value; }
        }
        #endregion

        #region Construction & Lifetime
        public SoundPlayer([NotNull] IAudioEngine engine, [NotNull] IAppContext appContext)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");
            if (appContext == null)
                throw new ArgumentNullException("musicLibrary");

            _engine = engine;
            _appContext = appContext;
            _channelGroup = _engine.CreateGrouping("sound");
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            lock (_updateLock)
            {
                foreach (IAudioTrack track in _audioTracks)
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
                _audioTracks.Clear();

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
        public void Play(string pack, string sound)
        {
            MusicEntry track = _appContext.ThemeMusicLibrary.LookupTrack(pack, sound);
            if (track == null) track = _appContext.DefaultMusicLibrary.LookupTrack(pack, sound);

            if (track != null)
            {
                GameLog.Client.Audio.DebugFormat("Play \"{0}\".", track.FileName);
                PlayFile(track.FileName);
            }
            else
            {
                GameLog.Client.GameData.WarnFormat("Soundplayer.cs: Could not locate track \"{0}\".", pack);
            }
        }

        public void PlayAny(string pack)
        {
            MusicPack musicPack = null;
            if (!_appContext.ThemeMusicLibrary.MusicPacks.TryGetValue(pack, out musicPack))
                _appContext.DefaultMusicLibrary.MusicPacks.TryGetValue(pack, out musicPack);

            if (musicPack != null)
            {
                KeyValuePair<int, MusicEntry> track = musicPack.Random();
                PlayFile(track.Value.FileName);
                GameLog.Client.Audio.DebugFormat("PlayAny musicPack={0}, Filename={1}", musicPack.Name, track.Value.FileName);
            }
            else
            {
                GameLog.Client.GameData.WarnFormat("Could not locate music pack \"{0}\".", pack);
            }
        }

        public void PlayFile(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            string resourcePath = ResourceManager.GetResourcePath(fileName);

            if (!File.Exists(resourcePath))
            {
                GameLog.Client.Audio.WarnFormat($"Could not locate audio file \"{resourcePath}\".");
                return;
            }

            lock (_updateLock)
            {
                IAudioTrack audioTrack = _engine.CreateTrack(resourcePath);
                if (audioTrack != null)
                {
                    audioTrack.Group = _channelGroup;
                    audioTrack.Play(OnTrackEnd);

                    _audioTracks.Add(audioTrack);
                }
            }
        }

        private void OnTrackEnd(IAudioTrack track)
        {
            lock (_updateLock)
            {
                try
                {
                    track.Dispose();
                    _audioTracks.Remove(track);
                }
                catch (Exception e)
                {
                    GameLog.Client.Audio.Error(e);
                }
            }
        }
        #endregion
    }
}
