using System;
using System.ComponentModel;
using System.Windows.Markup;

using Supremacy.Collections;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Utility;

namespace Supremacy.Personnel
{
    [Serializable]
    [DictionaryKeyProperty("Owner")]
    public class AgentProfileCollection : KeyedCollectionBase<string, AgentProfile>, ISupportInitializeNotification
    {
        private string _ownerId;
        
        [NonSerialized]
        private Lazy<Civilization> _owner;

        public AgentProfileCollection()
            : base(o => o.Name)
        {
            _owner = new Lazy<Civilization>(() => GameContext.Current.Civilizations[_ownerId]);
            //GameLog.Client.GameData.DebugFormat("AgentProfileCollection.cs: _owner={0}, _ownerId={1}, Civ[_ownerId]={1},_ownerName={1},", 
            //                                        _owner, _ownerId); 
                                            
        }

        [TypeConverter(typeof(CivilizationConverter))]
        public Civilization Owner
        {
            get
            {
                if (_ownerId == null)
                    return null;
                //GameLog causes a crash
                //GameLog.Client.GameData.DebugFormat("AgentProfileCollection.cs: GET _owner={0}, _ownerId={1}, Civ[_ownerId]={1},_ownerName={1},",
                //                                    _owner.Value, _owner.ToString());

                return _owner.Value;
            }
            set
            {
                VerifyInitializing();
                _ownerId = (value == null) ? null : value.Key;
                //GameLog MAYBE causes a crash
                GameLog.Client.GameData.DebugFormat("AgentProfileCollection.cs: SET _owner={0}",
                                    _ownerId.ToString());
            }
        }

        private bool _isInitialized;
        private bool _isInitializing;

        protected void VerifyInitializing()
        {
            if (!_isInitialized)
                return;

            throw new InvalidOperationException(SR.InvalidOperationException_AlreadyInitialized);
        }

        protected override void OnKeyCollision(string key, AgentProfile item)
        {
            var owner = Owner;

            GameLog.Client.GameData.WarnFormat(
                "Skipping agent '{0}' because an agent with that name " +
                "is already defined for Civilization '{1}'.",
                item.Name,
                (owner != null) ? owner.ShortName : (_ownerId ?? "<Unknown>"));
        }

        public override void DeserializeOwnedData(IO.Serialization.SerializationReader reader, object context)
        {
            base.DeserializeOwnedData(reader, context);

            _isInitialized = reader.ReadBoolean();
            _ownerId = reader.ReadString();

            if (_owner == null)
                _owner = new Lazy<Civilization>(() => GameContext.Current.Civilizations[_ownerId]);
        }

        public override void SerializeOwnedData(IO.Serialization.SerializationWriter writer, object context)
        {
            base.SerializeOwnedData(writer, context);

            writer.Write(_isInitialized);
            writer.Write(_ownerId);
        }

        #region Implementation of ISupportInitialize
        public void BeginInit()
        {
            lock (SyncRoot)
            {
                _isInitialized = false;
                _isInitializing = true;
            }
        }

        public void EndInit()
        {
            lock (SyncRoot)
            {
                if (_isInitialized)
                    return;

                _isInitialized = true;
                _isInitializing = false;

                OnInitialized();
            }
        }
        #endregion

        #region Implementation of ISupportInitializeNotification
        public bool IsInitialized
        {
            get
            {
                lock (SyncRoot)
                    return _isInitialized;
            }
        }

        public bool IsInitializing
        {
            get
            {
                lock (SyncRoot)
                    return _isInitializing;
            }
        }

        [field: NonSerialized]
        public event EventHandler Initialized;

        private void OnInitialized()
        {
            var handler = Initialized;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        #endregion
    }
}