// MusicEntry.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using Supremacy.Annotations;

namespace Supremacy.Client.Audio
{
    [Serializable]
    public class MusicEntry
    {
        #region Fields
        private readonly string _trackName;
        private readonly string _fileName;
        //private readonly float _fadeTime;
        #endregion

        #region Properties
        public string FileName => _fileName;

        public string TrackName => _trackName;

        // TODO: what sense does it have to have a different fading each track?
        // shouldn't FadeTime better be part of the MusicPlayer settings?
        //public float FadeTime
        //{
        //    get { return _fadeTime; }
        //}
        #endregion

        #region Constructor
        public MusicEntry([NotNull] string trackName, [NotNull] string fileName) //, float fadeTime)
        {
            _trackName = trackName ?? throw new ArgumentNullException("trackName");
            _fileName = fileName ?? throw new ArgumentNullException("fileName");
            //_fadeTime = fadeTime;
        }
        #endregion
    }
}
