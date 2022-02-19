// Treasury.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.ComponentModel;

namespace Supremacy.Economy
{
    /// <summary>
    /// Provides a snapshot of what a civilization's treasury looked like at an earlier point
    /// in the game.  The idea is that we can remember the values of the last 20 or so turns
    /// and use that to determine the financial situation, e.g. if a civ is in danger of going
    /// bankrupt in the near future.
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public struct TreasurySnapshot
    {
        private readonly int _turnNumber;
        private readonly int _initialCreditReserves;
        private readonly int _totalIncome;
        private readonly int _totalExpenses;

        public TreasurySnapshot(int turnNumber, int initialCreditReserves, int totalIncome, int totalExpenses)
        {
            _turnNumber = turnNumber;
            _initialCreditReserves = initialCreditReserves;
            _totalIncome = totalIncome;
            _totalExpenses = totalExpenses;
        }

        public int TurnNumber => _turnNumber;

        public int InitialCreditReserves => _initialCreditReserves;

        public int TotalIncome => _totalIncome;

        public int TotalExpenses => _totalExpenses;

        public int FinalCreditReserves => _initialCreditReserves + _totalIncome - _totalExpenses;

        public int NetChange => FinalCreditReserves - _initialCreditReserves;
    }

    [Serializable]
    public class Treasury : ICloneable
    {
        public const int HistoryLength = 20;

        private int _currentLevel;
        private int _grossIncome;
        private int _maintenance;
        private int _previousLevel;
        private int _previousChange;

        public Treasury() { }

        public Treasury(Treasury initialTreasury)
        {
            if (initialTreasury == null)
            {
                throw new ArgumentNullException("initialTreasury");
            }

            CopyFrom(initialTreasury);
        }

        public Treasury(int initialLevel)
        {
            if (initialLevel < 0)
            {
                throw new ArgumentOutOfRangeException("initialLevel", "amount must be non-negative");
            }

            CurrentLevel = initialLevel;
        }

        protected void CopyFrom(Treasury treasury)
        {
            if (treasury == null)
            {
                throw new ArgumentNullException("treasury");
            }

            _currentLevel = treasury._currentLevel;
            _grossIncome = treasury._grossIncome;
            _maintenance = treasury._maintenance;
            _previousChange = treasury._previousChange;
            _previousLevel = treasury._previousLevel;
        }

        public int CurrentLevel
        {
            get => _currentLevel;
            protected set => _currentLevel = value;
        }

        public int PreviousLevel
        {
            get => _previousLevel;
            protected set => _previousLevel = value;
        }

        public int Maintenance
        {
            get => _maintenance;
            protected set => _maintenance = value;
        }

        public int PreviousChange
        {
            get => _previousChange;
            protected set => _previousChange = value;
        }

        public int CurrentChange => CurrentLevel - PreviousLevel;

        public int GrossIncome
        {
            get => _grossIncome;
            protected set
            {
                _grossIncome = value;
                System.Diagnostics.Debug.Assert(_grossIncome >= 0);
            }
        }

        public bool IsBankruptcyImminent
        {
            get
            {
                if (CurrentChange >= 0)
                {
                    return false;
                }

                return CurrentChange >= (PreviousChange * 3);
            }
        }

        public int NetIncome => GrossIncome - Maintenance;

        public bool TryGiveAmount(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException("amount", "amount must be non-negative");
            }

            if (amount > CurrentLevel)
            {
                return false;
            }

            CurrentLevel -= amount;
            return true;
        }

        public void Add(Treasury treasury)
        {
            if (treasury == null)
            {
                throw new ArgumentNullException("treasury");
            }

            Add(treasury.CurrentLevel);
        }

        public void Subtract(Treasury treasury)
        {
            if (treasury == null)
            {
                throw new ArgumentNullException("treasury");
            }

            Subtract(treasury.CurrentLevel);
        }

        public void Add(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException("amount", "amount must be non-negative");
            }

            CurrentLevel += amount;
        }

        public void Subtract(int amount)
        {
            if (amount < 0)
            {
                amount *= -1;
            }

            CurrentLevel -= amount;
        }

        public void AddIncome(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException("amount", "amount must be non-negative");
            }

            GrossIncome += amount;
            CurrentLevel += amount;
        }

        public void SubtractIncome(int amount)
        {
            if (amount < 0)
            {
                amount *= -1;
            }

            if (amount > CurrentLevel)
            {
                amount = CurrentLevel;
            }

            GrossIncome -= amount;
            CurrentLevel -= amount;
        }

        public void ClearIncome()
        {
            GrossIncome = 0;
        }

        public void AddMaintenance(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException("amount", "amount must be non-negative");
            }

            Maintenance += amount;
        }

        public void Update()
        {
            PreviousChange = CurrentChange;
            PreviousLevel = CurrentLevel;
            GrossIncome = 0;
            Maintenance = 0;
        }

        public void ClearAllValues()
        {
            CurrentLevel = 0;
            PreviousChange = 0;
            PreviousLevel = 0;
            GrossIncome = 0;
            Maintenance = 0;
        }

        public Treasury Clone()
        {
            return new Treasury(this);
        }

        #region ICloneable Members
        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion
    }
}