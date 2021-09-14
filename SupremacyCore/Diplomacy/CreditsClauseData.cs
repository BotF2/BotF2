using System;

using Supremacy.IO.Serialization;

namespace Supremacy.Diplomacy
{
    [Serializable]
    public class CreditsClauseData : IOwnedDataSerializableAndRecreatable
    {
        private int _immediateAmount;
        private int _recurringAmount;

        public CreditsClauseData(int immediateAmount, int recurringAmount)
        {
            _immediateAmount = immediateAmount;
            _recurringAmount = recurringAmount;
        }

        public int ImmediateAmount => _immediateAmount;

        public int RecurringAmount => _recurringAmount;

        #region Implementation of IOwnedDataSerializable

        public void DeserializeOwnedData(SerializationReader reader, object context)
        {
            _immediateAmount = reader.ReadInt32();
            _recurringAmount = reader.ReadInt32();
        }

        public void SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.Write(_immediateAmount);
            writer.Write(_recurringAmount);
        }

        #endregion
    }
}