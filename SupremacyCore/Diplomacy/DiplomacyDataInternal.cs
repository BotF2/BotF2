// File:DiplomacyDataInternal.cs
using System;
using Supremacy.Collections;
using Supremacy.Game;
using Supremacy.IO.Serialization;
using Supremacy.Types;
using Supremacy.Utility;

namespace Supremacy.Diplomacy
{
    [Serializable]
    public class DiplomacyDataInternal : IDiplomacyDataExtended, IDiplomacyData, IOwnedDataSerializableAndRecreatable
    {
        private DiplomacyData _baseData;
        private CollectionBase<Motivation> _motivations;
        //private InfluenceMap _threatMap;

        public int LastTotalWarAttack { get; protected set; }
        public int LastColdWarAttack { get; protected set; }
        public int LastIncursion { get; protected set; }

        private string _text;

        public DiplomacyDataInternal(int ownerId, int counterpartyid)
        {
            _baseData = new DiplomacyData(ownerId, counterpartyid);
            _motivations = new CollectionBase<Motivation>();
            //_threatMap = new InfluenceMap();
        }

        public DiplomacyDataInternal(DiplomacyData baseData)
        {
            _baseData = baseData ?? throw new ArgumentNullException("baseData");
        }

        #region IDiplomacyDataExtended Members

        public int OwnerID => _baseData.OwnerID;

        public int CounterpartyID => _baseData.CounterpartyID;

        public IDiplomacyData BaseData => _baseData;

        public IIndexedCollection<Motivation> Motivations => _motivations;

        public Motivation CurrentMotivation
        {
            get
            {
                int count = _motivations.Count;
                _text += "s" + _text;  // dummy - do not remove
                GameLog.Core.Diplomacy.DebugFormat("_motivations.Count={0}", count);

                if (count != 0)
                {
                    return _motivations[count - 1];
                }

                return Motivation.NoMotivation;
            }
        }

        //public InfluenceMap ThreatMap
        //{
        //    get { return _threatMap; }
        //}

        public void ClearMotivations()
        {
            GameLog.Core.Diplomacy.Debug("_motivations cleared...");
            _motivations.Clear();
        }

        public void ConsiderMotivation(Motivation motivation)
        {
            if (motivation == null)
            {
                throw new ArgumentNullException("motivation");
            }

            _motivations.AddSorted(motivation, m => m.Priority);
        }

        public void SortMotivations()
        {
            /*
             * Do nothing, as motivations are sorted on insertion.
             */
        }

        public void OnAttack()
        {
            if (EffectiveRegard >= RegardLevel.ColdWar)
            {
                LastColdWarAttack = GameContext.Current.TurnNumber;
                GameLog.Core.Diplomacy.DebugFormat("EffectiveRegard {0} equal/*UNDER* RegardLevel.ColdWar={1}, so LastColdWarAttack-Turnnumber={2}", EffectiveRegard, RegardLevel.ColdWar, LastColdWarAttack);
            }
            else if (EffectiveRegard <= RegardLevel.TotalWar)
            {
                LastTotalWarAttack = GameContext.Current.TurnNumber;
                GameLog.Core.Diplomacy.DebugFormat("EffectiveRegard {0} equal/*ABOVE* RegardLevel.TotalWar={1}, so LastTotalWarAttack-Turnnumber={2}", EffectiveRegard, RegardLevel.TotalWar, LastTotalWarAttack);
            }
        }

        public void OnIncursion()
        {
            LastIncursion = GameContext.Current.TurnNumber;
            GameLog.Core.Diplomacy.DebugFormat("this.LastIncursion-TurnNumber={0}", LastIncursion);
        }

        public void SetContactTurn(int contactTurn)
        {
            ContactTurn = contactTurn == 0 ? GameContext.Current.TurnNumber : contactTurn;
        }

        public void SetFirstDiplomaticAction(bool firstDiplomaticAction)
        {
            FirstDiplomaticAction = firstDiplomaticAction;
        }
        #endregion

        #region IDiplomacyData Members

        public Meter Regard => _baseData.Regard;

        public Meter Trust => _baseData.Trust;

        public RegardLevel EffectiveRegard => _baseData.EffectiveRegard;

        public int ContactTurn
        {
            get => _baseData.ContactTurn;
            protected set => _baseData.ContactTurn = value;
        }
        public bool FirstDiplomaticAction
        {
            get => _baseData.FirstDiplomaticAction;
            set => _baseData.FirstDiplomaticAction = value;
        }

        public int ContactDuration => _baseData.ContactDuration;

        public ForeignPowerStatus Status
        {
            get => _baseData.Status;
            internal set => _baseData.Status = value;
        }

        public int LastStatusChange
        {
            get => _baseData.LastStatusChange;
            internal set => _baseData.LastStatusChange = value;
        }

        public int TurnsSinceLastStatusChange => _baseData.TurnsSinceLastStatusChange;

        #endregion

        void IOwnedDataSerializable.DeserializeOwnedData(SerializationReader reader, object context)
        {
            _baseData = reader.Read<DiplomacyData>();
            _motivations = reader.Read<CollectionBase<Motivation>>();
            //_threatMap = reader.Read<InfluenceMap>();

            LastTotalWarAttack = reader.ReadOptimizedInt32();
            LastColdWarAttack = reader.ReadOptimizedInt32();
            LastIncursion = reader.ReadOptimizedInt32();

            //_text = "Step_0832: Deserialize"
            //    //+ reader.BytesRemaining
            //    + ", mot_Count= " + _motivations.Count
            //    + ", mot_Count= " + _motivations.Count
            //    + ", L_TW_Attack= " + LastTotalWarAttack
            //    + ", L_CW_Attack= " + LastColdWarAttack
            //    + ", L_Inc= " + LastIncursion
            //    ;
            //Console.WriteLine(_text);
            //GameLog.Core.SaveLoadDetails.DebugFormat(_text);
        }

        void IOwnedDataSerializable.SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.WriteObject(_baseData);
            writer.WriteObject(_motivations);
            //writer.WriteObject(_threatMap);

            writer.WriteOptimized(LastTotalWarAttack);
            writer.WriteOptimized(LastColdWarAttack);
            writer.WriteOptimized(LastIncursion);
        }
    }
}