using System;
using System.Collections;
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;

using Supremacy.Text;
using Supremacy.Utility;

namespace Supremacy.Client.Views
{
    public class DiplomacyMessageAvailableElement : INotifyPropertyChanged
    {
        #region Description Property

        [field: NonSerialized]
        public event EventHandler DescriptionChanged;

        private string _description;

        public string Description
        {
            get => _description;
            private set
            {
                if (Equals(value, _description))
                {
                    return;
                }

                _description = value;

                OnDescriptionChanged();
            }
        }

        protected virtual void OnDescriptionChanged()
        {
            DescriptionChanged.Raise(this);
            OnPropertyChanged("Description");
        }

        #endregion

        #region ActionActionDescription Property

        [field: NonSerialized]
        public event EventHandler ActionDescriptionChanged;

        private string _actionDescription;

        public string ActionDescription
        {
            get => _actionDescription;
            private set
            {
                if (Equals(value, _actionDescription))
                {
                    return;
                }

                _actionDescription = value;

                OnActionDescriptionChanged();
            }
        }

        protected virtual void OnActionDescriptionChanged()
        {
            ActionDescriptionChanged.Raise(this);
            OnPropertyChanged("ActionDescription");
        }

        #endregion

        #region ActionCategory Property

        [field: NonSerialized]
        public event EventHandler ActionCategoryChanged;

        private DiplomacyMessageElementActionCategory _actionCategory;

        public DiplomacyMessageElementActionCategory ActionCategory
        {
            get => _actionCategory;
            set
            {
                if (value != _actionCategory)
                {
                    _actionCategory = value;
                    OnActionCategoryChanged();
                }

                ActionDescription = LocalizedTextDatabase.Instance.GetString(
                    LocalizedTextGroups.DiplomacyScreen,
                    new ContextualTextEntryKey("AvailableMessageElementList", _actionCategory));
            }
        }

        protected virtual void OnActionCategoryChanged()
        {
            ActionCategoryChanged.Raise(this);
            OnPropertyChanged("ActionCategory");
        }

        #endregion

        #region ElementType Property

        [field: NonSerialized]
        public event EventHandler ElementTypeChanged;

        private DiplomacyMessageElementType _elementType;

        public DiplomacyMessageElementType ElementType
        {
            get => _elementType;
            set
            {
                if (value != _elementType)
                {
                    _elementType = value;
                    OnElementTypeChanged();
                }

                Description = LocalizedTextDatabase.Instance.GetString(
                    LocalizedTextGroups.DiplomacyScreen,
                    new ContextualTextEntryKey("AvailableMessageElementList", _elementType));
            }
        }

        protected virtual void OnElementTypeChanged()
        {
            ElementTypeChanged.Raise(this);
            OnPropertyChanged("ElementType");
        }

        #endregion

        #region AddCommand Property

        [field: NonSerialized]
        public event EventHandler AddCommandChanged;

        private ICommand _addCommand;

        public ICommand AddCommand
        {
            get => _addCommand;
            set
            {
                if (Equals(value, _addCommand))
                {
                    return;
                }

                _addCommand = value;

                OnAddCommandChanged();
            }
        }

        protected virtual void OnAddCommandChanged()
        {
            AddCommandChanged.Raise(this);
            OnPropertyChanged("AddCommand");
        }

        #endregion

        #region AvailableParameters Property

        [field: NonSerialized]
        public event EventHandler AvailableParametersChanged;

        private Func<IEnumerable> _parametersCallback;

        public Func<IEnumerable> ParametersCallback
        {
            get => _parametersCallback;
            set
            {
                if (Equals(value, _parametersCallback))
                {
                    return;
                }

                _parametersCallback = value;

                OnAvailableParametersChanged();
            }
        }

        protected virtual void OnAvailableParametersChanged()
        {
            AvailableParametersChanged.Raise(this);
            OnPropertyChanged("AvailableParameters");
        }

        #endregion

        #region FixedParameter Property

        [field: NonSerialized]
        public event EventHandler FixedParameterChanged;

        private object _fixedParameter;

        public object FixedParameter
        {
            get => _fixedParameter;
            set
            {
                if (Equals(value, _fixedParameter))
                {
                    return;
                }

                _fixedParameter = value;

                OnFixedParameterChanged();
            }
        }

        protected virtual void OnFixedParameterChanged()
        {
            FixedParameterChanged.Raise(this);
            OnPropertyChanged("FixedParameter");
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
                    PropertyChangedEventHandler oldHandler = _propertyChanged;
                    PropertyChangedEventHandler newHandler = (PropertyChangedEventHandler)Delegate.Combine(oldHandler, value);

                    if (Interlocked.CompareExchange(ref _propertyChanged, newHandler, oldHandler) == oldHandler)
                    {
                        return;
                    }
                }
            }
            remove
            {
                while (true)
                {
                    PropertyChangedEventHandler oldHandler = _propertyChanged;
                    PropertyChangedEventHandler newHandler = (PropertyChangedEventHandler)Delegate.Remove(oldHandler, value);

                    if (Interlocked.CompareExchange(ref _propertyChanged, newHandler, oldHandler) == oldHandler)
                    {
                        return;
                    }
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            _propertyChanged.Raise(this, propertyName);
        }

        #endregion
    }
}