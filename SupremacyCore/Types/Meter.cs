// Meter.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.ComponentModel;

using Supremacy.IO.Serialization;

namespace Supremacy.Types
{
    [Serializable]
    public class MeterChangedEventArgs : CancelEventArgs
    {
        #region Fields
        private readonly Meter _meter;
        #endregion

        #region Constructors
        public MeterChangedEventArgs(Meter meter)
        {
            _meter = meter;
        }
        #endregion

        #region Properties
        public Meter Meter
        {
            get { return _meter; }
        }
        #endregion
    }

    public delegate void MeterChangedEvent(object sender, MeterChangedEventArgs e);

    /// <summary>
    /// An adjustable Meter with minimum and maximum bounds and
    /// a base efficiency.  When each turn is processed, the current 
    /// efficiency should be reset to its base efficiency, and then any
    /// necessary adjustments should be made as a result of specials
    /// and other effects.
    /// </summary>
    [Serializable]
    public class Meter : Cloneable, IOwnedDataSerializableAndRecreatable, ICloneable, INotifyPropertyChanged
    {
        #region Constants
        public const int MaxValue = int.MaxValue;
        public const int MinValue = int.MinValue;
        #endregion

        #region Fields
        private bool _autoClamp = true;
        private int _baseValue;
        private int _currentValue;
        private int _lastValue;
        private int _minimum;
        private int _maximum;
        #endregion

        #region Events
        [field: NonSerialized]
        public event MeterChangedEvent CurrentValueChanged;
        #endregion

        #region Constructors
        /// <summary>
        /// Construct a new Meter
        /// </summary>
        /// <param name="baseValue">Base efficiency</param>
        /// <param name="minValue">Minimum boundary</param>
        /// <param name="maxValue">Maximum boundary</param>
        public Meter(int baseValue, int minValue, int maxValue)
        {
            _baseValue = baseValue;
            _currentValue = baseValue;
            _lastValue = baseValue;
            _minimum = minValue;
            _maximum = maxValue;
            Clamp();
            Reset();
        }

        /// <summary>
        /// Construct a new Meter with a base efficiency of zero (0)
        /// </summary>
        /// <param name="minValue">Minimum boundary</param>
        /// <param name="maxValue">Maximum boundary</param>
        public Meter(int minValue, int maxValue)
            : this(0, minValue, maxValue) { }

        public Meter()
            : this(MinValue, MaxValue) { }
        #endregion

        #region Properties
        public bool IsMaximized
        {
            get { return (CurrentValue == Maximum); }
        }

        public bool IsMinimized
        {
            get { return (CurrentValue == Minimum); }
        }

        public bool AutoClamp
        {
            get { return _autoClamp; }
            set
            {
                _autoClamp = value;
                OnPropertyChanged("AutoClamp");
                if (_autoClamp)
                    Clamp();
            }
        }

        /// <summary>
        /// The current (adjusted) efficiency of the Meter
        /// </summary>
        public int CurrentValue
        {
            get { return Math.Max(Minimum, Math.Min(Maximum, _currentValue)); }
            set
            {
                _currentValue = value;
                OnPropertyChanged("CurrentValue");
                OnPropertyChanged("CurrentChange");
                OnPropertyChanged("PercentFilled");
                OnPropertyChanged("PercentFilledLogarithmic");
                OnCurrentValueChanged();
            }
        }

        /// <summary>
        /// The relative percent of a Meter that is filled by
        /// the current value
        /// </summary>
        public Percentage PercentFilled
        {
            // ReSharper disable PossibleLossOfFraction
            get
            {
                if (IsMinimized)
                    return 0f; 
                return 0.01f * (((CurrentValue - Minimum) * 100) / (Maximum - Minimum));
            }
            // ReSharper restore PossibleLossOfFraction
        }

        /// <summary>
        /// The relative percent of a Meter that is filled by
        /// the current value
        /// </summary>
        public Percentage PercentFilledLogarithmic
        {
            // ReSharper disable PossibleLossOfFraction
            get
            {
                if (IsMinimized)
                    return 0f;
                return (Percentage)(1d + Math.Log(PercentFilled, 10d));
            }
            // ReSharper restore PossibleLossOfFraction
        }

        /// <summary>
        /// The difference between the current and base values
        /// </summary>
        public int CurrentChange
        {
            get { return CurrentValue - BaseValue; }
        }

        /// <summary>
        /// The base efficiency of the Meter
        /// </summary>
        public int BaseValue
        {
            get { return Math.Max(Minimum, Math.Min(Maximum, _baseValue)); }
            set { SetBaseValue(value); }
        }

        /// <summary>
        /// The minimum bound of the Meter
        /// </summary>
        public int Minimum
        {
            get { return _minimum; }
            set
            {
                _minimum = Math.Min(MaxValue, Math.Max(MinValue, Math.Min(Maximum, value)));
                OnPropertyChanged("Minimum");
                if (_autoClamp)
                {
                    Clamp();
                    return;
                }
                OnPropertyChanged("BaseValue");
                OnPropertyChanged("CurrentValue");
                OnPropertyChanged("CurrentChange");
                OnPropertyChanged("PercentFilled");
                OnPropertyChanged("PercentFilledLogarithmic");
            }
        }

        /// <summary>
        /// The maximum bound of the Meter
        /// </summary>
        public int Maximum
        {
            get { return _maximum; }
            set
            {
                _maximum = Math.Max(MinValue, Math.Min(MaxValue, Math.Max(Minimum, value)));
                OnPropertyChanged("Maximum");
                if (_autoClamp)
                {
                    Clamp();
                    return;
                }
                OnPropertyChanged("BaseValue");
                OnPropertyChanged("CurrentValue");
                OnPropertyChanged("CurrentChange");
                OnPropertyChanged("PercentFilled");
                OnPropertyChanged("PercentFilledLogarithmic");
            }
        }

        /// <summary>
        /// The change between the current value and last value
        /// </summary>
        public int LastChange
        {
            get { return (CurrentValue - LastValue); }
        }

        /// <summary>
        /// The last (adjusted) efficiency of the Meter
        /// </summary>
        public int LastValue
        {
            get { return _lastValue; }
            protected set
            {
                _lastValue = value;
                OnPropertyChanged("LastValue");
                OnPropertyChanged("LastChange");
            }
        }
        #endregion

        #region ICloneable Members
        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region INotifyPropertyChanged Members
        [field : NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Methods
        /// <summary>
        /// Add some efficiency to the current efficiency of the Meter
        /// </summary>
        /// <param name="delta">Value to add</param>
        /// <returns>The magnitude of the actual adjustment applied</returns>
        public int AdjustCurrent(int delta)
        {
            return Math.Abs(CurrentValue - SetCurrentValue(CurrentValue + delta));
        }

        /// <summary>
        /// Multiply the current efficiency of the Meter by some efficiency
        /// </summary>
        /// <param name="multiplier">Multiplier</param>
        /// <returns>The magnitude of the actual adjustment applied</returns>
        public int AdjustCurrent(float multiplier)
        {
            return Math.Abs(CurrentValue - SetCurrentValue(CurrentValue + (int)(((double)BaseValue * multiplier) - BaseValue)));
        }

        public int ApplyModifier(ValueModifier modifier)
        {
            if (modifier != null)
                CurrentValue = modifier.Apply(BaseValue, CurrentValue);
            return CurrentValue;
        }

        public Meter Clone()
        {
            return Clone(this);
        }

        public void SetValues(Meter meter)
        {
            if (meter == null)
                throw new ArgumentNullException("meter");

            _autoClamp = meter._autoClamp;
            _minimum = meter._minimum;
            _maximum = meter._maximum;
            _baseValue = meter._baseValue;
            _currentValue = meter._currentValue;
            _lastValue = meter._lastValue;

            OnPropertyChanged("AutoClamp");
            OnPropertyChanged("Minimum");
            OnPropertyChanged("Maximum");
            OnPropertyChanged("CurrentValue");
            OnPropertyChanged("BaseValue");
            OnPropertyChanged("LastValue");
            OnPropertyChanged("PercentFilled");
            OnPropertyChanged("PercentFilledLogarithmic");
        }

        /// <summary>
        /// Equivalent to setting BaseValue = Maximum and calling Reset()
        /// </summary>
        public void ReplenishAndReset()
        {
            SetBaseValue(Maximum);
            Reset();
        }

        /// <summary>
        /// Reset the current efficiency to the base efficiency
        /// </summary>
        public void Reset()
        {
            CurrentValue = BaseValue;
        }

        /// <summary>
        /// Set the base efficiency of the meter and reset the
        /// current efficiency to the new base efficiency
        /// </summary>
        /// <param name="baseValue">Base efficiency</param>
        public void Reset(int baseValue)
        {
            BaseValue = baseValue;
            Reset();
        }

        /// <summary>
        /// Set the base efficiency of the Meter; if the new value
        /// is outside the [min, max] bounds, set to the nearest
        /// boundary
        /// </summary>
        /// <param name="value">Base value</param>
        public void SetBaseValue(int value)
        {
            LastValue = BaseValue;
            _baseValue = _autoClamp ? Math.Max(Minimum, Math.Min(Maximum, value)) : value;
            OnPropertyChanged("BaseValue");
            OnPropertyChanged("CurrentChange");
        }

        public override string ToString()
        {
            return CurrentValue.ToString();
        }

        /// <summary>
        /// Equivalent to setting BaseValue = CurrentValue and calling Reset()
        /// </summary>
        public void UpdateAndReset()
        {
            SetBaseValue(CurrentValue);
            Reset();
        }

        public void SaveCurrentAndResetToBase()
        {
            LastValue = CurrentValue;
            CurrentValue = BaseValue;
        }

        private int SetCurrentValue(int value)
        {
            if (_autoClamp)
            {
                if (value > Maximum)
                    CurrentValue = Maximum;
                else if (value < _minimum)
                    CurrentValue = Minimum;
                else
                    CurrentValue = value;
            }
            else
            {
                CurrentValue = value;
            }
            return CurrentValue;
        }

        public void Clamp()
        {
            if (BaseValue < Minimum)
                BaseValue = Minimum;
            else if (BaseValue > Maximum)
                BaseValue = Maximum;
            if (CurrentValue < Minimum)
                CurrentValue = Minimum;
            else if (CurrentValue > Maximum)
                CurrentValue = Maximum;
        }

        protected void OnCurrentValueChanged()
        {
            if (CurrentValueChanged != null)
                CurrentValueChanged(this, new MeterChangedEventArgs(this));
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public virtual void SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.Write(_autoClamp);
            writer.Write(_baseValue);
            writer.Write(_currentValue);
            writer.Write(_lastValue);
            writer.Write(_maximum);
            writer.Write(_minimum);
        }

        public virtual void DeserializeOwnedData(SerializationReader reader, object context)
        {
            _autoClamp = reader.ReadBoolean();
            _baseValue = reader.ReadInt32();
            _currentValue = reader.ReadInt32();
            _lastValue = reader.ReadInt32();
            _maximum = reader.ReadInt32();
            _minimum = reader.ReadInt32();
        }

        #region Implementation of ICloneable<Meter>

        protected override Cloneable CreateInstance(ICloneContext context)
        {
            return new Meter(_minimum, _maximum);
        }

        public override void CloneFrom(Cloneable source, ICloneContext context)
        {
            SetValues((Meter)source);
        }

        #endregion
    }
}