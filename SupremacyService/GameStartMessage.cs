// File:GameStartMessage.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Runtime.Serialization;

using Supremacy.Game;
using Supremacy.IO;
using Supremacy.Types;

namespace Supremacy.WCF
{
    [DataContract]
    [Serializable]
    public class GameStartMessage
    {
        [DataMember]
        private readonly byte[] _buffer;

        public GameStartData Data => StreamUtility.Read<GameStartData>(_buffer);

        public GameStartMessage(GameStartData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            try
            {
                _buffer = StreamUtility.Write(data);
            }
            catch (Exception e)
            {
                throw new SupremacyException(
                    "An error occurred while preparing a game start message.",
                    e,
                    SupremacyExceptionAction.Disconnect);
            }
        }
    }

    [DataContract]
    [Serializable]
    public class GameUpdateMessage
    {
        [DataMember]
        private readonly byte[] _buffer;

        [NonSerialized]
        private GameUpdateData _data;

        public GameUpdateData Data
        {
            get
            {
                if (_data == null)
                {
                    _data = StreamUtility.Read<GameUpdateData>(_buffer);
                }

                return _data;
            }
        }

        public GameUpdateMessage(GameUpdateData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            _buffer = StreamUtility.Write(data);
        }
    }
}
