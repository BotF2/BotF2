// Distribution.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Supremacy.Types
{
    [Serializable]
    public sealed class DistributionEventArgs<TKey> : EventArgs
    {
        #region Fields
        private readonly Distribution<TKey> _source;
        #endregion

        #region Constructors
        public DistributionEventArgs(Distribution<TKey> source)
        {
            _source = source;
        }
        #endregion

        #region Properties and Indexers
        public Distribution<TKey> Source
        {
            get { return _source; }
        }
        #endregion
    }

    [Serializable]
    public sealed class Distribution<TKey> : INotifyPropertyChanged, IDeserializationCallback
    {
        #region Constants
        public static readonly Percentage MaxValue = 1.0f;
        public static readonly Percentage MinValue = 0.0f;
        #endregion

        #region Fields
        private readonly DistributionGroup<TKey> _group;
        private bool _isLocked;

        [NonSerialized]
        private StateScope _suppressValueChangedScope;

        private Percentage _value;
        #endregion

        #region Events
        [field: NonSerialized]
        public event EventHandler<DistributionEventArgs<TKey>> ValueChanged;
        #endregion

        #region Constructors
        internal Distribution(DistributionGroup<TKey> parent)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");
            _group = parent;
            _suppressValueChangedScope = new StateScope();
        }
        #endregion

        #region Properties and Indexers
        public bool IsLocked
        {
            get { return _isLocked; }
            set
            {
                if (value == _isLocked)
                {
                    return;
                }
                if (value)
                {
                    Lock();
                }
                else
                {
                    Unlock();
                }
            }
        }

        internal StateScope SuppressValueChangedScope
        {
            get { return _suppressValueChangedScope; }
        }

        public Percentage Value
        {
            get { return _value; }
            set
            {
                if (SetValue(value))
                {
                    OnValueChanged();
                }
            }
        }
        #endregion

        #region Methods
        public void Lock()
        {
            _isLocked = true;
            OnPropertyChanged("IsLocked");
        }

        public void Unlock()
        {
            _isLocked = false;
            OnPropertyChanged("IsLocked");
        }

        internal void SetValueInternal(Percentage initialValue)
        {
            _value = initialValue;
            OnValueChanged();
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnValueChanged()
        {
            if (SuppressValueChangedScope.IsWithin)
                return;
            if (ValueChanged != null)
                ValueChanged(this, new DistributionEventArgs<TKey>(this));
            OnPropertyChanged("Value");
        }

        private bool SetValue(Percentage newValue)
        {
            if (IsLocked)
                throw new InvalidOperationException("Value cannot be set while locked.");
            if (newValue == Value)
                return false;
            if (newValue < MinValue)
                throw new ArgumentOutOfRangeException("newValue", "Value must be >= MinValue.");
            if (newValue > MaxValue)
                throw new ArgumentOutOfRangeException("newValue", "Value must be <= MaxValue.");

            int passesSinceChanged = 0;
            float change = newValue - Value;
            float delta = (change > 0) ? 0.01f : -0.01f;
            Percentage originalValue = Value;

            var changeRounded = (float)Math.Round(change, 2);
            if (FloatUtil.AreClose(change, changeRounded))
                change = changeRounded;

            var otherValues = new List<Distribution<TKey>>(_group.Children);
            var changedChildren = new HashSet<Distribution<TKey>>();

            using (SuppressValueChangedScope.Enter())
            {
                if (otherValues.Count <= 1)
                {
                    if (Value != 1.0f)
                    {
                        SetValueInternal(1.0f);
                        return true;
                    }
                    return false;
                }

                if (_group._lastChangeIndex == otherValues.IndexOf(this))
                {
                    if (this == otherValues[otherValues.Count - 1])
                    {
                        _group._lastChangeIndex = 0;
                    }
                    else
                    {
                        _group._lastChangeIndex = otherValues.IndexOf(this) + 1;
                    }
                }

                while (Math.Abs(change) >= 0.01f)
                {
                    bool noChange = true;
                    for (int i = _group._lastChangeIndex; i < otherValues.Count; i++)
                    {
                        if ((otherValues[i] == this) || otherValues[i].IsLocked)
                        {
                            continue;
                        }
                        if (((delta > 0) && (delta > otherValues[i].Value))
                            || ((delta < 0) && ((MaxValue - delta) < otherValues[i].Value)))
                        {
                            continue;
                        }
                        using (otherValues[i].SuppressValueChangedScope.Enter())
                        {
                            otherValues[i].SetValueInternal(otherValues[i].Value - delta);
                            changedChildren.Add(otherValues[i]);
                        }
                        SetValueInternal(Value + delta);
                        change -= delta;
                        var ceiling = (float)Math.Round(change, 2);
                        if (FloatUtil.AreClose(change, ceiling))
                            change = ceiling;
                        _group._lastChangeIndex = i + 1;
                        noChange = false;
                        passesSinceChanged = 0;
                        if (Math.Abs(change) < 0.01f)
                            break;
                    }
                    if (noChange)
                    {
                        if (++passesSinceChanged > 1)
                            break;
                        _group._lastChangeIndex = 0;
                    }
                }
            }

            if (Value != originalValue)
                OnValueChanged();

            foreach (var changedChild in changedChildren)
            {
                changedChild.OnValueChanged();
            }

            return ((Value == newValue) || (Value != originalValue));
        }
        #endregion

        #region IDeserializationCallback Members
        public void OnDeserialization(object sender)
        {
            if (_suppressValueChangedScope == null)
                _suppressValueChangedScope = new StateScope();
            if (_group != null)
                ValueChanged += _group.DistributionGroup_ValueChanged;
        }
        #endregion

        #region INotifyPropertyChanged Members
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }

    [Serializable]
    public sealed class DistributionGroup<TKey> : INotifyPropertyChanged
    {
        #region Fields
        public readonly DistributionValueCollection Values;
        internal int _lastChangeIndex;
        private readonly Dictionary<TKey, Distribution<TKey>> _children;
        private int _totalValue;
        #endregion

        #region Constructors
        public DistributionGroup()
        {
            _children = new Dictionary<TKey, Distribution<TKey>>();
            Values = new DistributionValueCollection(this);
        }

        public DistributionGroup(params TKey[] initialKeys) : this((IEnumerable<TKey>)initialKeys) { }

        public DistributionGroup(IEnumerable<TKey> initialKeys) : this()
        {
            if (initialKeys == null)
                throw new ArgumentNullException("initialKeys");
            foreach (TKey key in initialKeys)
            {
                _children[key] = new Distribution<TKey>(this);
                _children[key].ValueChanged += DistributionGroup_ValueChanged;
            }
            DistributeEvenly();
        }
        #endregion

        #region Properties and Indexers
        public int TotalValue
        {
            get { return _totalValue; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("value must be non-negative");
                _totalValue = value;
                OnPropertyChanged("TotalValue");
            }
        }

        public ICollection<Distribution<TKey>> Children
        {
            get { return _children.Values; }
        }

        public Distribution<TKey> this[TKey key]
        {
            get
            {
                if (!_children.ContainsKey(key))
                    return null;
                return _children[key];
            }
        }
        #endregion

        #region Methods
        public void DistributeEvenly()
        {
            float totalValue = Distribution<TKey>.MaxValue;
            float unitValue = (float)Math.Round(totalValue / _children.Count, 2);
            if ((unitValue * _children.Count) > totalValue)
                unitValue -= 0.01f;
            Percentage remainder = totalValue - (unitValue * _children.Count);
            foreach (var child in _children.Values)
            {
                child.SetValueInternal(unitValue);
            }
            if (remainder >= 0.01f)
            {
                foreach (var child in _children.Values)
                {
                    child.SetValueInternal(child.Value + 0.01f);
                    remainder -= 0.01f;
                    if (remainder < 0.01f)
                        break;
                }
            }
        }

        internal void DistributionGroup_ValueChanged(object sender, DistributionEventArgs<TKey> e)
        {
            OnPropertyChanged("Values");
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region INotifyPropertyChanged Members
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region DistributionValueCollection Nested Type
        [Serializable]
        public sealed class DistributionValueCollection
        {
            #region Fields
            private readonly DistributionGroup<TKey> _source;
            #endregion

            #region Constructors
            public DistributionValueCollection(DistributionGroup<TKey> source)
            {
                _source = source;
            }
            #endregion

            #region Properties and Indexers
            public int this[TKey key]
            {
                get { return (int)(_source[key].Value * _source.TotalValue); }
            }
            #endregion
        }
        #endregion
    }
}