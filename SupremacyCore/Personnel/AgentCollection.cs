using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

using Supremacy.Collections;
using Supremacy.Game;
using Supremacy.IO.Serialization;
using Supremacy.Utility;

namespace Supremacy.Personnel
{
    [Serializable]
    public sealed class AgentCollection : IOwnedDataSerializableAndRecreatable, IIndexedKeyedCollection<GameObjectID, Agent>
    {
        private KeyedCollectionBase<GameObjectID, Agent> _agents;

        public AgentCollection()
        {
            _agents = CreateAgentsCollection();
        }

        private KeyedCollectionBase<GameObjectID, Agent> CreateAgentsCollection()
        {
            var result = new KeyedCollectionBase<GameObjectID, Agent>(o => o.ObjectID);
            result.CollectionChanged += (sender, args) => CollectionChanged.Raise(this, args);
            return result;
        }

        internal void Add(Agent agent)
        {
            _agents.Add(agent);
        }

        internal bool Remove(Agent agent)
        {
            return _agents.Remove(agent);
        }

        #region Implementation of IOwnedDataSerializable

        void IOwnedDataSerializable.DeserializeOwnedData(SerializationReader reader, object context)
        {
            if (_agents == null)
                _agents = CreateAgentsCollection();
            else if (_agents.Count != 0)
                _agents.Clear();

            _agents.AddRange(reader.ReadArray<Agent>());
        }

        void IOwnedDataSerializable.SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.WriteArray(_agents.ToArray());
        }

        #endregion

        #region Implementation of IEnumerable

        IEnumerator<Agent> IEnumerable<Agent>.GetEnumerator()
        {
            return _agents.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _agents.GetEnumerator();
        }

        #endregion

        #region Implementation of IKeyedLookup<in GameObjectID,out Agent>

        public Agent this[GameObjectID key]
        {
            get { return _agents[key]; }
        }

        public bool Contains(GameObjectID key)
        {
            return _agents.Contains(key);
        }

        IEqualityComparer<GameObjectID> IKeyedLookup<GameObjectID, Agent>.KeyComparer
        {
            get { return EqualityComparer<GameObjectID>.Default; }
        }

        #endregion

        #region Implementation of IKeyedCollection<GameObjectID,Agent>

        public IEnumerable<GameObjectID> Keys
        {
            get { return _agents.Keys; }
        }

        public bool TryGetValue(GameObjectID key, out Agent value)
        {
            return _agents.TryGetValue(key, out value);
        }

        #endregion

        #region Implementation of INotifyCollectionChanged

        [field: NonSerialized]
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region Implementation of IIndexedEnumerable<out Agent>

        public int Count
        {
            get { return _agents.Count; }
        }

        Agent IIndexedEnumerable<Agent>.this[int index]
        {
            get { return ((IIndexedCollection<Agent>)_agents)[index]; }
        }

        #endregion

        #region Implementation of IIndexedCollection<Agent>

        public bool Contains(Agent agent)
        {
            return _agents.Contains(agent);
        }

        public int IndexOf(Agent agent)
        {
            return _agents.IndexOf(agent);
        }

        #endregion

        public void AddRange(IEnumerable<Agent> agents)
        {
            _agents.AddRange(agents);
        }
    }
}