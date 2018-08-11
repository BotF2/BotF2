// PlayerSlot.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.ComponentModel;

namespace Supremacy.Game
{
    [Serializable]
    public class PlayerSlot : INotifyPropertyChanged
    {
        #region Fields
        private SlotClaim _claim;
        private int _empireID;

        private string _empireName;

        private Player _player;
        private int _slotID;
        private SlotStatus _status;
        #endregion

        #region Properties and Indexers
        public SlotClaim Claim
        {
            get { return _claim; }
            set
            {
                _claim = value;
                OnPropertyChanged("Claim");
                OnPropertyChanged("IsVacant");
                OnPropertyChanged("IsFrozen");
            }
        }

        public int EmpireID
        {
            get { return _empireID; }
            set
            {
                _empireID = value;
                OnPropertyChanged("EmpireID");
            }
        }

        public string EmpireName
        {
            get { return _empireName; }
            set
            {
                _empireName = value;
                OnPropertyChanged("EmpireName");
            }
        }

        public bool IsClosed
        {
            get { return (Status == SlotStatus.Closed); }
            set
            {
                if (!value && IsFrozen)
                    return;
                Status = value ? SlotStatus.Open : SlotStatus.Closed;
                Player = null;
            }
        }

        public bool IsFrozen
        {
            get { return false; }
        }

        public bool IsVacant
        {
            get
            {
                if (Claim == SlotClaim.Assigned)
                    return false;
                return ((Status == SlotStatus.Open) || (Status == SlotStatus.Computer));
            }
        }

        public Player Player
        {
            get { return _player; }
            set
            {
                _player = value;
                OnPropertyChanged("Player");
            }
        }

        public int SlotID
        {
            get { return _slotID; }
            set
            {
                _slotID = value;
                OnPropertyChanged("SlotID");
            }
        }

        public SlotStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged("Status");
                OnPropertyChanged("IsClosed");
                OnPropertyChanged("IsVacant");
                OnPropertyChanged("IsFrozen");
            }
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        [field : NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Public and Protected Methods
        public void Clear()
        {
            if (IsFrozen)
                return;
            Status = SlotStatus.Open;
            Claim = SlotClaim.Unassigned;
            if ((Player != null) && (Player.EmpireID == EmpireID))
                Player.EmpireID = GameObjectID.InvalidID;
            Player = null;
        }

        public void Close()
        {
            if (IsFrozen)
                return;
            Status = SlotStatus.Closed;
            Claim = SlotClaim.Unassigned;
            if (Player != null)
                Player.EmpireID = GameObjectID.InvalidID;
            Player = null;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}