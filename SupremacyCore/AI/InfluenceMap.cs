// InfluenceMap.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Game;
using Supremacy.IO.Serialization;
using Supremacy.Universe;

namespace Supremacy.AI
{
    [Serializable]
    public class InfluenceMap : IOwnedDataSerializableAndRecreatable
    {
        protected const uint MaxValue = 255;

        //protected const uint AlliedMask = 0xFF000000;
        //protected const byte AlliedOffset = 24;
        //protected const uint FriendlyMask = 0x00FF0000;
        //protected const byte FriendlyOffset = 16;
        //protected const uint NeutralMask = 0x0000FF00;
        //protected const byte NeutralOffset = 8;
        //protected const uint EnemyMask = 0x000000FF;
        //protected const byte EnemyOffset = 0;

        protected const ulong AlliedMask = 0x00000000FFFFFFFFL;
        protected const byte AlliedOffset = 0;
        protected const ulong EnemyMask = 0xFFFFFFFF00000000L;
        protected const byte EnemyOffset = 32;

        protected ulong[,] _values;

        public InfluenceMap() : this(GameContext.Current) { }

        public InfluenceMap(GameContext game)
        {
            if (game == null)
                throw new ArgumentNullException("game");
            _values = new ulong[game.Universe.Map.Width, game.Universe.Map.Height];
        }

        public int GetAllied(MapLocation location)
        {
            return (int)((_values[location.X, location.X] & AlliedMask) >> AlliedOffset);
        }

        //public int GetFriendly(MapLocation location)
        //{
        //    return (int)((_values[location.X, location.X] & FriendlyMask) >> FriendlyOffset);
        //}

        //public int GetNeutral(MapLocation location)
        //{
        //    return (int)((_values[location.X, location.X] & NeutralMask) >> NeutralOffset);
        //}

        public int GetEnemy(MapLocation location)
        {
            return (int)((_values[location.X, location.X] & EnemyMask) >> EnemyOffset);
        }

        public int GetInfluence(MapLocation location)
        {
            return GetAllied(location)
                   //+ GetFriendly(location) / 2
                   //- GetNeutral(location) / 2
                   - GetEnemy(location);
        }

        protected void SetValue(MapLocation location, ulong mask, byte offset, int value)
        {
            _values[location.X, location.Y] =
                (_values[location.X, location.Y] & ~mask)
                | ((Math.Min((uint)value, MaxValue) << offset) & mask);
        }

        public void AddAllied(MapLocation location, int amount)
        {
            SetValue(location, AlliedMask, AlliedOffset, GetAllied(location) + amount);
        }

        //public void AddFriendly(MapLocation location, int amount)
        //{
        //    SetValue(location, FriendlyMask, FriendlyOffset, GetFriendly(location) + amount);
        //}

        //public void AddNeutral(MapLocation location, int amount)
        //{
        //    SetValue(location, NeutralMask, NeutralOffset, GetNeutral(location) + amount);
        //}

        public void AddEnemy(MapLocation location, int amount)
        {
            SetValue(location, EnemyMask, EnemyOffset, GetEnemy(location) + amount);
        }

        void IOwnedDataSerializable.DeserializeOwnedData(SerializationReader reader, object context)
        {
            var width = reader.ReadOptimizedInt32();
            var height = reader.ReadOptimizedInt32();

            var values = new ulong[width, height];

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                    values[x, y] = reader.ReadUInt64();
            }

            _values = values;
        }

        void IOwnedDataSerializable.SerializeOwnedData(SerializationWriter writer, object context)
        {
            var values = _values;
            var width = values.GetLength(0);
            var height = values.GetLength(1);

            writer.WriteOptimized(width);
            writer.WriteOptimized(height);

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                    writer.Write(values[x, y]);
            }
        }
    }
}
