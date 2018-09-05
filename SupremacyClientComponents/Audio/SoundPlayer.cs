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

        private bool _audioTraceLocally = false;

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
            if (_audioTraceLocally)
                GameLog.Print("called!");

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
            if (_audioTraceLocally)
                GameLog.Print("called!");

            if (_isDisposed)
                return;

            _isDisposed = true;

            lock(_updateLock)
            {
                foreach (var track in _audioTracks)
                {
                    try
                    {
                        track.Stop();
                        track.Dispose();
                    }
                    catch (Exception e) //ToDo: Just log or additional handling necessary?
                    {
                        GameLog.LogException(e);
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
            if(track == null) track = _appContext.DefaultMusicLibrary.LookupTrack(pack, sound);

            if (track != null)
            {
                if (_audioTraceLocally)
                    GameLog.Print("Soundplayer.cs: Play \"{0}\".", track.FileName);
                PlayFile(track.FileName);
            }
            else GameLog.Client.GameData.DebugFormat(
                "Soundplayer.cs: Could not locate track \"{0}\".", pack);
        }

        public void PlayAny(string pack)
        {
            if (_audioTraceLocally)
                GameLog.Print("called!");

            MusicPack musicPack = null;
            if(!_appContext.ThemeMusicLibrary.MusicPacks.TryGetValue(pack, out musicPack))
                _appContext.DefaultMusicLibrary.MusicPacks.TryGetValue(pack, out musicPack);

            if (musicPack != null)
            {
                var track = musicPack.Random();
                PlayFile(track.Value.FileName);
                if (_audioTraceLocally)
                    GameLog.Print("PlayAny musicPack={0}, Filename={1}", musicPack.Name, track.Value.FileName);
            }
            else GameLog.Client.GameData.DebugFormat(
                "Could not locate music pack \"{0}\".", pack);
        }

        public void PlayFile(string fileName)
        {
            if (_audioTraceLocally)
                GameLog.Print("called! {0}", fileName);

            if (fileName == null)
                throw new ArgumentNullException("fileName");

            var resourcePath = ResourceManager.GetResourcePath(fileName);

            if (!File.Exists(resourcePath))
            {
                GameLog.Client.GameData.DebugFormat($"Could not locate audio file \"{resourcePath}\".");
                return;
            }

            lock (_updateLock)
            {
                var audioTrack = _engine.CreateTrack(resourcePath);
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
            if (_audioTraceLocally)
                GameLog.Print("called!");

            lock (_updateLock)
            {
                try
                {
                    track.Dispose();
                    _audioTracks.Remove(track);
                }
                catch (Exception e) //ToDo: Just log or additional handling necessary?
                {
                    GameLog.Print("####### problem at OnTrackEnd");
                    GameLog.LogException(e);
                }
            }
        }
        #endregion
    }
}
