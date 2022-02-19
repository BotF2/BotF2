using System.Collections.Generic;

using Supremacy.Collections;
using Supremacy.Game;

using System.Linq;

namespace Supremacy.WCF
{
    internal sealed class ServerPlayerInfoCollection : KeyedCollectionBase<Player, ServerPlayerInfo>
    {
        private readonly Dictionary<string, ServerPlayerInfo> _sessionIdLookup;
        private readonly Dictionary<int, ServerPlayerInfo> _playerIdLookup;

        internal ServerPlayerInfoCollection()
            : base(o => o.Player)
        {
            _sessionIdLookup = new Dictionary<string, ServerPlayerInfo>();
            _playerIdLookup = new Dictionary<int, ServerPlayerInfo>();
        }

        internal ServerPlayerInfo FromPlayerId(int playerId)
        {
            lock (SyncRoot)
            {

                if (_playerIdLookup.TryGetValue(playerId, out ServerPlayerInfo playerInfo))
                {
                    return playerInfo;
                }

                return null;
            }
        }

        internal ServerPlayerInfo FromEmpireId(int empireId)
        {
            lock (SyncRoot)
            {
                return Items.FirstOrDefault(o => o.Player.EmpireID == empireId);
            }
        }


        internal ServerPlayerInfo FromSessionId(string sessionId)
        {
            lock (SyncRoot)
            {

                if (_sessionIdLookup.TryGetValue(sessionId, out ServerPlayerInfo playerInfo))
                {
                    return playerInfo;
                }

                return null;
            }
        }

        protected override void InsertItem(int index, ServerPlayerInfo item)
        {
            lock (SyncRoot)
            {
                _sessionIdLookup[item.Session.SessionId] = item;
                _playerIdLookup[item.Player.PlayerID] = item;

                base.InsertItem(index, item);
            }
        }

        protected override void RemoveItem(int index)
        {
            lock (SyncRoot)
            {
                ServerPlayerInfo item = this[index];

                _ = _sessionIdLookup.Remove(item.Session.SessionId);
                _ = _playerIdLookup.Remove(item.Player.PlayerID);

                base.RemoveItem(index);
            }
        }

        protected override void ClearItems()
        {
            lock (SyncRoot)
            {
                _sessionIdLookup.Clear();
                _playerIdLookup.Clear();

                base.ClearItems();
            }
        }

        public ServerPlayerInfo[] ToArray()
        {
            ServerPlayerInfo[] array;

            lock (SyncRoot)
            {
                array = new ServerPlayerInfo[Count];
                CopyTo(array, 0);
            }

            return array;
        }

        public Player[] ToPlayerArray()
        {
            Player[] array;

            lock (SyncRoot)
            {
                array = new Player[Count];

                for (int i = 0; i < Count; i++)
                {
                    array[i] = Items[i].Player;
                }
            }

            return array;
        }
    }
}