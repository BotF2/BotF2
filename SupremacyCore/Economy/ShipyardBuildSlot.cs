using System;
using Supremacy.IO.Serialization;
using Supremacy.Orbitals;

namespace Supremacy.Economy
{
    [Serializable]
    public class ShipyardBuildSlot : BuildSlot
    {
        [NonSerialized]
        private Shipyard _shipyard;
        [NonSerialized]
        private int _slotId;
        private bool _isActive;

        public Shipyard Shipyard
        {
            get => _shipyard;
            set
            {
                _shipyard = value;
                OnPropertyChanged("Shipyard");
            }
        }

        public int SlotID
        {
            get => _slotId;
            set
            {
                _slotId = value;
                OnPropertyChanged("SlotID");
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;

                OnPropertyChanged("IsActive");
                OnPropertyChanged("OnHold");
            }
        }

        public override void DeserializeOwnedData(SerializationReader reader, object context)
        {
            base.DeserializeOwnedData(reader, context);

            _isActive = reader.ReadBoolean();
        }

        public override bool OnHold => HasProject && (Project.IsPaused || !IsActive);

        public override void SerializeOwnedData(SerializationWriter writer, object context)
        {
            base.SerializeOwnedData(writer, context);

            writer.Write(_isActive);
        }
    }
}