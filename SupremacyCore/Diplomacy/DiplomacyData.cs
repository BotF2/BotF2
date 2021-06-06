// DiplomacyData.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Policy;
using System.Threading;
using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Data;
using Supremacy.Game;
using Supremacy.IO.Serialization;
using Supremacy.Types;
using Supremacy.Utility;

namespace Supremacy.Diplomacy
{
    public interface IDiplomacyData
    {
        int OwnerID { get; }
        int CounterpartyID { get; }
        Meter Regard { get; }
        Meter Trust { get; }
        RegardLevel EffectiveRegard { get; }
        int ContactTurn { get; }
        int ContactDuration { get; }
        ForeignPowerStatus Status { get; }
        int LastStatusChange { get; }
        int TurnsSinceLastStatusChange { get; }
    }

    public interface IDiplomacyDataExtended
    {
        int LastTotalWarAttack { get; }
        int LastColdWarAttack { get; }
        int LastIncursion { get; }
        IDiplomacyData BaseData { get; }
        IIndexedCollection<Motivation> Motivations { get; }
        Motivation CurrentMotivation { get; }
        //InfluenceMap ThreatMap { get; }

        void ClearMotivations();
        void ConsiderMotivation(Motivation motivation);
        void SortMotivations();
        void OnAttack();
        void OnIncursion();
        void SetContactTurn(int contactTurn = 0);
    }

    [Serializable]
    public class DiplomacyData : IDiplomacyData, INotifyPropertyChanged, IOwnedDataSerializableAndRecreatable
    {
        private int _ownerId;
        private int _counterpartyId;
        private Meter _regard;
        private Meter _trust;
        private int _contactTurn;
        private bool _firstDiplomaticAction;
        private int _lastStatusChange;
        private ForeignPowerStatus _diplomacyStatus;

        protected static TableMap DiplomacyTables => GameContext.Current.Tables.DiplomacyTables;

        public DiplomacyData(int ownerId, int counterpartyId)
        {
            _ownerId = ownerId;
            _counterpartyId = counterpartyId;
            _contactTurn = 0;
            _firstDiplomaticAction = false;
            _regard = new Meter(500, 0, 1000);
            _trust = new Meter(500, 0, 1000);
            _regard.CurrentValueChanged += OnRegardCurrentValueChanged;
            _trust.CurrentValueChanged += OnTrustCurrentValueChanged;
        }

        private void OnTrustCurrentValueChanged(object sender, MeterChangedEventArgs e)
        {
            OnPropertyChanged("EffectiveTrust");
        }

        private void OnRegardCurrentValueChanged(object sender, MeterChangedEventArgs e)
        {
            OnPropertyChanged("EffectiveRegard");
        }

        public int OwnerID => _ownerId;

        public int CounterpartyID => _counterpartyId;

        public Meter Regard => _regard;

        public Meter Trust => _trust;

        public RegardLevel EffectiveRegard => CalculateRegardLevel(_regard.CurrentValue);

        public int ContactTurn
        {
            get { return _contactTurn; }
            internal set { _contactTurn = value; }
        }

        public bool FirstDiplomaticAction
        {
            get { return _firstDiplomaticAction; }
            set { _firstDiplomaticAction = value; }
        }
        
        public int ContactDuration
        {
            get
            {
                if (_contactTurn == 0)
                    return -1;
                return GameContext.Current.TurnNumber - _contactTurn;
            }
        }

        public ForeignPowerStatus Status
        {
            get { return _diplomacyStatus; }
            internal set { _diplomacyStatus = value; }
        }

        public int LastStatusChange
        {
            get { return _lastStatusChange; }
            internal set { _lastStatusChange = value; }
        }

        public int TurnsSinceLastStatusChange
        {
            get
            {
                if (Status == ForeignPowerStatus.NoContact)
                    return 0;
                return GameContext.Current.TurnNumber - LastStatusChange;
            }
        }

        public bool AtWar => Status == ForeignPowerStatus.AtWar;

        public RegardLevel EffectiveTrust => CalculateRegardLevel(_trust.CurrentValue);

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _regard.CurrentValueChanged += OnRegardCurrentValueChanged;
            _trust.CurrentValueChanged += OnTrustCurrentValueChanged;
        }

        public static RegardLevel CalculateRegardLevel(int regard)
        {
            var regardLevel = RegardLevel.Detested;
            var regardLevelsTable = DiplomacyTables["RegardLevels"];

            foreach (var enumValue in EnumHelper.GetValues<RegardLevel>())
            {
                var lowerBound = (int?)regardLevelsTable.GetValue(enumValue.ToString(), 0);
                if (lowerBound == null)
                    continue;

                if (regard >= lowerBound && enumValue > regardLevel)
                    regardLevel = enumValue;
            }

            return regardLevel;
        }

        public void UpdateFrom([NotNull] IDiplomacyData diplomacyData)
        {
            if (diplomacyData == null)
                throw new ArgumentNullException("diplomacyData");

            _regard.SetValues(diplomacyData.Regard);
            _trust.SetValues(diplomacyData.Trust);
        }

        #region Implementation of INotifyPropertyChanged

        [NonSerialized] private PropertyChangedEventHandler _propertyChanged;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                while (true)
                {
                    var oldHandler = _propertyChanged;
                    var newHandler = (PropertyChangedEventHandler)Delegate.Combine(oldHandler, value);

                    if (Interlocked.CompareExchange(ref _propertyChanged, newHandler, oldHandler) == oldHandler)
                        return;
                }
            }
            remove
            {
                while (true)
                {
                    var oldHandler = _propertyChanged;
                    var newHandler = (PropertyChangedEventHandler)Delegate.Remove(oldHandler, value);

                    if (Interlocked.CompareExchange(ref _propertyChanged, newHandler, oldHandler) == oldHandler)
                        return;
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            _propertyChanged.Raise(this, propertyName);
        }

        #endregion

        void IOwnedDataSerializable.DeserializeOwnedData(SerializationReader reader, object context)
        {
            _ownerId = reader.ReadOptimizedInt32();
            _counterpartyId = reader.ReadOptimizedInt32();
            _regard = reader.Read<Meter>();
            _trust = reader.Read<Meter>();
            _contactTurn = reader.ReadOptimizedInt32();
            _firstDiplomaticAction = reader.ReadBoolean();
            _diplomacyStatus = (ForeignPowerStatus)reader.ReadOptimizedInt32();
            _lastStatusChange = reader.ReadOptimizedInt32();
        }

        void IOwnedDataSerializable.SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.WriteOptimized(_ownerId);
            writer.WriteOptimized(_counterpartyId);
            writer.WriteObject(_regard);
            writer.WriteObject(_trust);
            writer.WriteOptimized(_contactTurn);
            writer.WriteObject(_firstDiplomaticAction);
            writer.WriteOptimized((int)_diplomacyStatus);
            writer.WriteOptimized(_lastStatusChange);

            //later: do it for Saveload: GameLog.Core.SaveLoad.DebugFormat(""
            // works fine
            //GameLog.Core.SaveLoad.DebugFormat("_ownerId = {0}, _counterpartyId = {1}, _contactTurn = {4}, _lastStatusChange = {6}, (int)_diplomacyStatus = {5}, _regard = {2}, _trust = {3}"
            //    , _ownerId
            //    , _counterpartyId
            //    , _regard
            //    , _trust
            //    , _contactTurn
            //    , (int)_diplomacyStatus
            //    , _lastStatusChange
            //    );
        }
    }
}