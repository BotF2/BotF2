// CivList.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Supremacy.Game;
using Supremacy.Collections;

namespace Supremacy.Entities
{
    [Serializable]
    public class CivList : IList<Civilization>
    {
        private readonly ArrayList<GameObjectID> _civIds;

        public CivList() : this(Enumerable.Empty<Civilization>())
        {
            //if (this.ElementAt<2>)
            //    this.Remove
        }

        public CivList(IEnumerable<Civilization> civs)
        {
            if (civs == null)
                throw new ArgumentNullException("civs");
            _civIds = new ArrayList<GameObjectID>(civs.Select(o => o.CivID));
        }

        #region IList<Civilization> Members
        public int IndexOf(Civilization civ)
        {
            if (civ == null)
                return -1;
            return _civIds.IndexOf(civ.CivID);
        }

        public void Insert(int index, Civilization civ)
        {
            if (civ == null)
                throw new ArgumentNullException("civ");
            _civIds.Insert(index, civ.CivID);
        }

        public void RemoveAt(int index)
        {
            _civIds.RemoveAt(index);
        }

        public Civilization this[int index]
        {
            get { return GameContext.Current.Civilizations[_civIds[index]]; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value"); 
                _civIds[index] = value.CivID;
            }
        }
        #endregion

        #region ICollection<Civilization> Members
        public void Add(Civilization civ)
        {
            if (civ == null)
                throw new ArgumentNullException("civ");
            _civIds.Add(civ.CivID);
        }

        public void Clear()
        {
            _civIds.Clear();
        }

        public bool Contains(Civilization civ)
        {
            if (civ == null)
                return false;
            return _civIds.Contains(civ.CivID);
        }

        public void CopyTo(Civilization[] array, int arrayIndex)
        {
            this.ToArray().CopyTo(array, arrayIndex);
        }

        public bool Remove(Civilization civ)
        {
            if (civ == null)
                return false;
            return _civIds.Remove(civ.CivID);
        }

        public int Count
        {
            get { return _civIds.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
        #endregion

        #region IEnumerable<Civilization> Members
        IEnumerator<Civilization> IEnumerable<Civilization>.GetEnumerator()
        {
            return _civIds.Select(o => GameContext.Current.Civilizations[o]).GetEnumerator();
        }
        #endregion

        #region IEnumerable Members
        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable<Civilization>)this).GetEnumerator();
        }
        #endregion
    }
}