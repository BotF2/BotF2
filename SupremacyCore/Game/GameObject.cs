// GameObject.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Runtime.Serialization;

using Supremacy.Annotations;
using Supremacy.Collections;
using System.ComponentModel;

using Supremacy.IO.Serialization;
using Supremacy.Types;

namespace Supremacy.Game
{
    public interface IGameObject
    {
        int ObjectID { get; }
    }

    [Serializable]

    public abstract class GameObject : Cloneable,
                                       IGameObject,
                                       IOwnedDataSerializableAndRecreatable,
                                       INotifyPropertyChanged

    {
        private int _objectId;

        [field: NonSerialized]
        public event EventHandler ObjectIDChanged;
        public string _turnnumber = GameContext.Current.TurnNumber.ToString();
        public string blank = " ";
        public string newline = Environment.NewLine;

        protected GameObject()
        {
            if (GameContext.Current != null)
            {
                _objectId = GameContext.Current.GenerateID();
            }
        }

        protected GameObject(int objectId)
        {
            if (objectId <= -1)
            {
                throw new ArgumentException("Invalid object ID.");
            }

            _objectId = objectId;
        }

        public override void CloneFrom(Cloneable source, ICloneContext context)
        {
            GameObject typedSource = (GameObject)source;

            base.CloneFrom(typedSource, context);

            _objectId = typedSource._objectId;
        }

        /// <summary>
        /// Gets the unique identifier of this <see cref="GameObject"/> within the scope
        /// of its parent GameContext.
        /// </summary>
        /// <value>Object ID</value>
        [Indexable]
        public int ObjectID
        {
            get => _objectId;
            protected internal set
            {
                _objectId = value;

                OnObjectIDChanged();
                OnPropertyChanged("ObjectID");
            }
        }

        private void OnObjectIDChanged()
        {
            ObjectIDChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected internal virtual void OnDeserialized() { }


        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            Compact();
        }


        /// <summary>
        /// Compacts this <see cref="GameObject"/> to reduce the serialization footprint.
        /// </summary>
        public virtual void Compact() { }

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public virtual void SerializeOwnedData([NotNull] SerializationWriter writer, [CanBeNull] object context)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            writer.WriteOptimized(_objectId);
        }

        public virtual void DeserializeOwnedData([NotNull] SerializationReader reader, [CanBeNull] object context)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            _objectId = reader.ReadOptimizedInt32();
        }
    }
}
