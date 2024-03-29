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
            get => _claim;
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
            get => _empireID;
            set
            {
                _empireID = value;
                OnPropertyChanged("EmpireID");
            }
        }

        public string EmpireName
        {
            get => _empireName;
            set
            {
                _empireName = value;
                OnPropertyChanged("EmpireName");
            }
        }

        public bool IsClosed
        {
            get => Status == SlotStatus.Closed;
            set
            {
                if (!value && IsFrozen)
                {
                    return;
                }

                Status = value ? SlotStatus.Open : SlotStatus.Closed;
                Player = null;
            }
        }

        public bool IsFrozen => false;

        public bool IsVacant
        {
            get
            {
                if (Claim == SlotClaim.Assigned)
                {
                    return false;
                }

                return (Status == SlotStatus.Open) || (Status == SlotStatus.Computer);
            }
        }

        public Player Player
        {
            get => _player;
            set
            {
                _player = value;
                OnPropertyChanged("Player");
            }
        }

        public int SlotID
        {
            get => _slotID;
            set
            {
                _slotID = value;
                OnPropertyChanged("SlotID");
            }
        }

        public SlotStatus Status
        {
            get => _status;
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
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Public and Protected Methods
        public void Clear()
        {
            if (IsFrozen)
            {
                return;
            }

            Status = SlotStatus.Open;
            Claim = SlotClaim.Unassigned;
            if ((Player != null) && (Player.EmpireID == EmpireID))
            {
                Player.EmpireID = -1;
            }

            Player = null;
        }

        public void Close()
        {
            if (IsFrozen)
            {
                return;
            }

            Status = SlotStatus.Closed;
            Claim = SlotClaim.Unassigned;
            if (Player != null)
            {
                Player.EmpireID = -1;
            }

            Player = null;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}