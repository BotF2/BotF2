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
    // ReSharper disable RedundantExtendsListEntry
    public abstract class GameObject : Cloneable,
                                       IGameObject,
                                       IOwnedDataSerializableAndRecreatable,
                                       INotifyPropertyChanged
    // ReSharper restore RedundantExtendsListEntry
    {
        private int _objectId;
        
        [field:NonSerialized]
        public event EventHandler ObjectIDChanged;

        protected GameObject()
        {
            if (GameContext.Current != null)
                _objectId = GameContext.Current.GenerateID();
        }

        protected GameObject(int objectId)
        {
            if (objectId <= -1)
                throw new ArgumentException("Invalid object ID.");
            _objectId = objectId;
        }

        public override void CloneFrom(Cloneable source, ICloneContext context)
        {
            var typedSource = (GameObject)source;

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
            get { return _objectId; }
            protected internal set
            {
                _objectId = value;

                OnObjectIDChanged();
                OnPropertyChanged("ObjectID");
            }
        }

        private void OnObjectIDChanged()
        {
            var handler = ObjectIDChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected internal virtual void OnDeserialized() { }

#pragma warning disable 168
        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            Compact();
        }
#pragma warning restore 168

        /// <summary>
        /// Compacts this <see cref="GameObject"/> to reduce the serialization footprint.
        /// </summary>
        public virtual void Compact() { }

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

    	public virtual void SerializeOwnedData([NotNull] SerializationWriter writer, [CanBeNull] object context)
    	{
    	    if (writer == null)
    	        throw new ArgumentNullException("writer");

    	    writer.WriteOptimized(_objectId);
    	}

        public virtual void DeserializeOwnedData([NotNull] SerializationReader reader, [CanBeNull] object context)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            _objectId = reader.ReadOptimizedInt32();
        }
    }
}
