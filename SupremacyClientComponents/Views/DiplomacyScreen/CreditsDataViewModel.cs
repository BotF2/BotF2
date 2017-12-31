using System;
using System.ComponentModel;
using System.Threading;

using Supremacy.Diplomacy;
using Supremacy.Economy;
using Supremacy.Utility;

namespace Supremacy.Client.Views
{
    public class CreditsDataViewModel : IClauseParameterInfo, INotifyPropertyChanged
    {
        private const int StepCount = 5;

        public CreditsDataViewModel(Treasury treasury)
        {
            var stepSize = treasury.CurrentLevel / StepCount;

            ImmediateStepSize = stepSize;
            RecurringStepSize = stepSize;
            Maximum = treasury.CurrentLevel;
            Minimum = 0;
        }

        public CreditsDataViewModel()
        {
            ImmediateStepSize = 1000;
            RecurringStepSize = 1000;
            Maximum = int.MaxValue;
            Minimum = 0;
        }

        #region Minimum Property

        [field: NonSerialized]
        public event EventHandler MinimumChanged;

        private int _minimum;

        public int Minimum
        {
            get { return _minimum; }
            set
            {
                if (Equals(value, _minimum))
                    return;

                _minimum = value;

                OnMinimumChanged();
            }
        }

        protected virtual void OnMinimumChanged()
        {
            MinimumChanged.Raise(this);
            OnPropertyChanged("Minimum");
        }

        #endregion

        #region Maximum Property

        [field: NonSerialized]
        public event EventHandler MaximumChanged;

        private int _maximum;

        public int Maximum
        {
            get { return _maximum; }
            private set
            {
                if (Equals(value, _maximum))
                    return;

                _maximum = value;

                OnMaximumChanged();
            }
        }

        protected virtual void OnMaximumChanged()
        {
            MaximumChanged.Raise(this);
            OnPropertyChanged("Maximum");
        }

        #endregion

        #region ImmediateAmount Property

        [field: NonSerialized]
        public event EventHandler ImmediateAmountChanged;

        private int _immediateAmount;

        public int ImmediateAmount
        {
            get { return _immediateAmount; }
            set
            {
                if (value < 0)
                    value = 0;

                if (Equals(value, _immediateAmount))
                    return;

                _immediateAmount = (int)Math.Round(value * 1.0d / _immediateStepSize) * _immediateStepSize;

                OnImmediateAmountChanged();
            }
        }

        protected virtual void OnImmediateAmountChanged()
        {
            ImmediateAmountChanged.Raise(this);
            OnPropertyChanged("ImmediateAmount");
            OnPropertyChanged("IsParameterValid");
        }

        #endregion

        #region RecurringAmount Property

        [field: NonSerialized]
        public event EventHandler RecurringAmountChanged;

        private int _recurringAmount;

        public int RecurringAmount
        {
            get { return _recurringAmount; }
            set
            {
                if (value < 0)
                    value = 0;

                if (Equals(value, _recurringAmount))
                    return;

                _recurringAmount = (int)Math.Round(value * 1.0d / _recurringStepSize) * _recurringStepSize;

                OnRecurringAmountChanged();
            }
        }

        protected virtual void OnRecurringAmountChanged()
        {
            RecurringAmountChanged.Raise(this);
            OnPropertyChanged("RecurringAmount");
            OnPropertyChanged("IsParameterValid");
        }

        #endregion

        #region ImmediateStepSize Property

        [field: NonSerialized]
        public event EventHandler ImmediateStepSizeChanged;

        private int _immediateStepSize;

        public int ImmediateStepSize
        {
            get { return _immediateStepSize; }
            private set
            {
                if (Equals(value, _immediateStepSize))
                    return;

                _immediateStepSize = value;

                OnImmediateStepSizeChanged();
            }
        }

        protected virtual void OnImmediateStepSizeChanged()
        {
            ImmediateStepSizeChanged.Raise(this);
            OnPropertyChanged("ImmediateStepSize");
        }

        #endregion

        #region RecurringStepSize Property

        [field: NonSerialized]
        public event EventHandler RecurringStepSizeChanged;

        private int _recurringStepSize;

        public int RecurringStepSize
        {
            get { return _recurringStepSize; }
            private set
            {
                if (Equals(value, _recurringStepSize))
                    return;

                _recurringStepSize = value;

                OnRecurringStepSizeChanged();
            }
        }

        protected virtual void OnRecurringStepSizeChanged()
        {
            RecurringStepSizeChanged.Raise(this);
            OnPropertyChanged("RecurringStepSize");
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        [NonSerialized] private PropertyChangedEventHandler _propertyChanged;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                while (true)
                {
                    var oldHandler = _propertyChanged;
                    var newHandler = (PropertyChangedEventHandler)Delegate.Combine(oldHandler, value);

                    if (Interlocked.CompareExchange(ref _propertyChanged, newHandler, oldHandler) == oldHandler)
                        return;
                }
            }
            remove
            {
                while (true)
                {
                    var oldHandler = _propertyChanged;
                    var newHandler = (PropertyChangedEventHandler)Delegate.Remove(oldHandler, value);

                    if (Interlocked.CompareExchange(ref _propertyChanged, newHandler, oldHandler) == oldHandler)
                        return;
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            _propertyChanged.Raise(this, propertyName);
        }

        #endregion

        #region Implementation of IClauseParameterInfo

        bool IClauseParameterInfo.IsParameterValid
        {
            get { return ImmediateAmount > 0 || RecurringAmount > 0; }
        }

        object IClauseParameterInfo.GetParameterData()
        {
            return new CreditsClauseData(ImmediateAmount, RecurringAmount);
        }

        #endregion
    }
}