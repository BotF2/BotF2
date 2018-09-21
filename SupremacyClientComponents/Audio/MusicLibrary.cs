using System;
using System.Collections.Generic;
using System.Xml;
using Supremacy.Resources;
using System.Diagnostics;
using Supremacy.Utility;
using System.Windows;

namespace Supremacy.Client.Audio
{
    public interface IMusicLibrary
    {
        Dictionary<string, MusicPack> MusicPacks { get; }

        void Load(String libraryPath);
        void Load(XmlElement xmlNode);
        void Clear();

        MusicEntry LookupTrack(string packName, string trackName);
    }

    [Serializable]
    public class MusicLibrary : IMusicLibrary
    {
        #region Fields
        private const string PackDefName = "MusicPack";
        private Dictionary<string, MusicPack> _musicPacks = new Dictionary<string, MusicPack>(StringComparer.OrdinalIgnoreCase);
        #endregion

        #region Properties
        public Dictionary<string, MusicPack> MusicPacks
        {
            get { return _musicPacks; }
        }
        #endregion

        #region Methods
        public void Load(string libraryPath)
        {
            Debug.Assert(!string.IsNullOrEmpty(libraryPath));

            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.Load(ResourceManager.GetResourcePath(libraryPath));
                XmlElement xmlRoot = xmlDoc.DocumentElement;

                Load(xmlRoot);
            }
            catch
            {
                
                GameLog.Client.GameData.DebugFormat("MusicLibrary.cs: MusicPacks.xml is missing ({0})", libraryPath);
                MessageBox.Show("MusicPacks.xml is missing for played empire", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Load(XmlElement xmlNode)
        {
            _musicPacks.Clear();
            foreach (XmlElement xmlPack in xmlNode.GetElementsByTagName(PackDefName))
            {
                var musicPack = new MusicPack();
                musicPack.Load(xmlPack);
                _musicPacks.Add(musicPack.Name, musicPack);

                GameLog.Client.General.DebugFormat("adding: musicPack.Name={0}", musicPack.Name);
            }
        }

        public void Clear()
        {
            _musicPacks.Clear();
        }

        public MusicEntry LookupTrack(string packName, string trackName)
        {
            MusicPack pack;
            MusicPacks.TryGetValue(packName, out pack);

            if (pack != null)
            {
                MusicEntry track = null;
                pack.Dictionary.TryGetValue(trackName, out track);

                GameLog.Client.General.DebugFormat("trackName={0}, track.FileName={1}", trackName, track.FileName);

                return track;
            }
            else return null;
        }
        #endregion
    }
}
