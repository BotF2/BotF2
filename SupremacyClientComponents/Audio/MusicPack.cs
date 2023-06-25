// File:MusicPack.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Resources;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Supremacy.Client.Audio
{
    [Serializable]
    public class MusicPack
    {
        #region Fields
        // be it a "SimpleMusicPack" or "DefaultMusicPack" it's all just a collection of MusicTracks isn't it?
        private const string PackDefName = "MusicPack";
        private const string TrackDefName = "Track";

        private readonly List<MusicEntry> _musicList = new List<MusicEntry>();
        private readonly Dictionary<string, MusicEntry> _musicDict = new Dictionary<string, MusicEntry>();

        [NonSerialized]
        private string _text;
        #endregion

        #region Properties
        public string Name { get; set; }
        public string Path { get; set; }
        public List<MusicEntry> Entries => _musicList;
        public Dictionary<string, MusicEntry> Dictionary => _musicDict;
        #endregion

        #region Methods
        public void Load(string packPath, string packName)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement xmlRoot;

            xmlDoc.Load(ResourceManager.GetResourcePath(packPath));
            xmlRoot = xmlDoc.DocumentElement;

            //List<MusicEntry> tracks = new List<MusicEntry>();
            foreach (XmlElement xmlPack in xmlRoot.GetElementsByTagName(PackDefName))
            {
                string musicPackName = xmlPack.GetAttribute("Name");
                if ((string.IsNullOrEmpty(packName) && !string.IsNullOrEmpty(musicPackName))
                    || (!string.IsNullOrEmpty(packName) && string.IsNullOrEmpty(musicPackName)))
                {
                    continue;
                }
                else if (!string.IsNullOrEmpty(packName) && !string.IsNullOrEmpty(musicPackName)
                    && !musicPackName.Trim().ToUpperInvariant().Equals(packName.Trim().ToUpperInvariant()))
                {
                    continue;
                }

                Load(xmlPack);
            }
        }

        public void Load(XmlElement xmlNode)
        {
            Name = xmlNode.GetAttribute("Name");
            Path = xmlNode.GetAttribute("Path");

            foreach (XmlElement track in xmlNode.GetElementsByTagName(TrackDefName))
            {
                string filename = track.InnerText.Trim();
                if (!string.IsNullOrEmpty(filename))
                {
                    // TODO: what sense does it have to have a different fading each track?
                    // shouldn't FadeTime better be part of the MusicPlayer settings?
                    //float fadeTime = 0f;
                    //if (track.HasAttribute("FadeTime"))
                    //{
                    //    var fadeTimeAttr = track.GetAttribute("FadeTime");
                    //    fadeTime = float.Parse(fadeTimeAttr, System.Globalization.CultureInfo.InvariantCulture);
                    //}

                    string trackName = track.GetAttribute("Name");
                    MusicEntry entry = new MusicEntry(trackName, System.IO.Path.Combine(Path, filename));
                    _musicList.Add(entry);
                    if (!string.IsNullOrEmpty(trackName))
                    {
                        _musicDict.Add(trackName, entry);
                        _text = "Step_0157: Track available > " + trackName;
                        //Console.WriteLine(_text);
                        GameLog.Client.AudioDetails.DebugFormat(_text);
                    }
                }
            }

            // TODO: load MusicPlayer.PlayMode from program settings? having different PlaybackModes for different MusicPacks (or races) doesn't make much sense.
            //if (xmlNode.HasAttribute("Selection"))
            //{
            //    string selMode = xmlNode.GetAttribute("Selection").ToUpperInvariant();
            //    if (selMode == "SEQUENTIAL")
            //        newPack.Selection = SelectionAlgo.Sequential;
            //}
        }

        public void Clear()
        {
            _musicList.Clear();
            _musicDict.Clear();
        }

        public bool HasEntries()
        {
            return _musicList.Count != 0;
        }

        public KeyValuePair<int, MusicEntry> FindByName(string trackName)
        {
            int pos = 0;
            foreach (MusicEntry track in _musicList)
            {
                if (track.TrackName.Equals(trackName, StringComparison.OrdinalIgnoreCase))
                {
                    return new KeyValuePair<int, MusicEntry>(pos, track);
                }

                ++pos;
            }

            return new KeyValuePair<int, MusicEntry>(-1, null);
        }

        public KeyValuePair<int, MusicEntry> Next(int prev = -1)
        {
            if (_musicList.Count == 0)
            {
                return new KeyValuePair<int, MusicEntry>();
            }

            int pos = prev != -1 && prev + 1 < _musicList.Count ? prev + 1 : 0;
            return new KeyValuePair<int, MusicEntry>(pos, _musicList[pos]);
        }

        public KeyValuePair<int, MusicEntry> Prev(int prev = -1)
        {
            if (_musicList.Count == 0)
            {
                return new KeyValuePair<int, MusicEntry>();
            }

            int pos = prev > 0 ? prev - 1 : _musicList.Count - 1;
            return new KeyValuePair<int, MusicEntry>(pos, _musicList[pos]);
        }

        public KeyValuePair<int, MusicEntry> Random(int prev = -1)
        {
            if (_musicList.Count == 0)
            {
                return new KeyValuePair<int, MusicEntry>();
            }

            int pos = 0;
            if (_musicList.Count > 1)
            {
                if (prev != -1)
                {
                    // do not repeat current entry
                    pos = RandomProvider.Shared.Next(_musicList.Count - 1);
                    if (pos >= prev)
                    {
                        ++pos;
                    }
                }
                else
                {
                    pos = RandomProvider.Shared.Next(_musicList.Count);
                }
            }

            return new KeyValuePair<int, MusicEntry>(pos, _musicList[pos]);
        }
        #endregion
    }
}
