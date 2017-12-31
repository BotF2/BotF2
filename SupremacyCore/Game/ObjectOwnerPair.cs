using System;

using Supremacy.IO.Serialization;

namespace Supremacy.Game
{
    [Serializable]
    public struct ObjectOwnerPair : IOwnedDataSerializableAndRecreatable, IEquatable<ObjectOwnerPair>
    {
        private GameObjectID _objectId;
        private GameObjectID _ownerId;

        public ObjectOwnerPair(GameObjectID objectId, GameObjectID ownerId)
        {
            _objectId = objectId;
            _ownerId = ownerId;
        }

        public GameObjectID ObjectID
        {
            get { return _objectId; }
        }

        public GameObjectID OwnerID
        {
            get { return _ownerId; }
        }

        public void DeserializeOwnedData(SerializationReader reader, object context)
        {
            _objectId = reader.ReadInt32();
            _ownerId = reader.ReadInt32();
        }

        public void SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.Write(_objectId);
            writer.Write(_ownerId);
        }

        public bool Equals(ObjectOwnerPair other)
        {
            return other._objectId == _objectId &&
                other._ownerId == _ownerId;
        }

        public override bool Equals(object obj)
        {
            if (obj is ObjectOwnerPair)
                return Equals((ObjectOwnerPair)obj);
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_objectId.GetHashCode() * 397) ^ _ownerId.GetHashCode();
            }
        }

        public static bool operator ==(ObjectOwnerPair left, ObjectOwnerPair right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ObjectOwnerPair left, ObjectOwnerPair right)
        {
            return !left.Equals(right);
        }
    }
}