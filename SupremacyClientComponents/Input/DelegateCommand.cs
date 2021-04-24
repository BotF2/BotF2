using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

using Microsoft.Practices.Composite;

namespace Supremacy.Client.Input
{
    public class DelegateCommand : ICommand, IActiveAware
    {
        private readonly Action _executeMethod;
        private readonly Func<bool> _canExecuteMethod;
        private bool _isActive;

        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                if (_isActive == value)
                    return;
                _isActive = value;
                OnIsActiveChanged();
            }
        }

        public event EventHandler CanExecuteChanged;
        public event EventHandler IsActiveChanged;

        public DelegateCommand(Action executeMethod)
            : this(executeMethod, null) { }

        public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod)
        {
            if (executeMethod == null && canExecuteMethod == null)
                throw new ArgumentNullException("executeMethod", "Execute and CanExecute callbacks cannot both be null.");

            _executeMethod = executeMethod;
            _canExecuteMethod = canExecuteMethod;
        }

        public bool CanExecute()
        {
            if (_canExecuteMethod == null)
                return true;
            return _canExecuteMethod();
        }

        public void Execute()
        {
            if (_executeMethod == null)
                return;
            _executeMethod();
        }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute();
        }

        void ICommand.Execute(object parameter)
        {
            Execute();
        }

        //[SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }

        protected virtual void OnIsActiveChanged()
        {
            var eventHandler = IsActiveChanged;
            if (eventHandler == null)
                return;
            eventHandler(this, EventArgs.Empty);
        }

        protected virtual void OnCanExecuteChanged()
        {
            var dispatcher = (Dispatcher)null;

            if (Application.Current != null)
                dispatcher = Application.Current.Dispatcher;
           
            var eventHandler = CanExecuteChanged;
            if (eventHandler == null)
                return;

            if (dispatcher != null && !dispatcher.CheckAccess())
                dispatcher.Invoke((Action)OnCanExecuteChanged);
            else
                eventHandler(this, EventArgs.Empty);
        }
    }

    public class DelegateCommand<T> : ICommand, IActiveAware
    {
        private readonly Action<T> _executeMethod;
        private readonly Func<T, bool> _canExecuteMethod;
        private bool _isActive;

        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                if (_isActive == value)
                    return;
                _isActive = value;
                OnIsActiveChanged();
            }
        }

        public event EventHandler CanExecuteChanged;
        public event EventHandler IsActiveChanged;

        public DelegateCommand(Action<T> executeMethod)
            : this(executeMethod, null) { }

        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
        {
            if (executeMethod == null && canExecuteMethod == null)
                throw new ArgumentNullException("executeMethod", "Execute and CanExecute callbacks cannot both be null.");

            _executeMethod = executeMethod;
            _canExecuteMethod = canExecuteMethod;
        }

        public bool CanExecute(T parameter)
        {
            if (_canExecuteMethod == null)
                return true;
            return _canExecuteMethod(parameter);
        }

        public void Execute(T parameter)
        {
            if (_executeMethod == null)
                return;
            _executeMethod(parameter);
        }

        bool ICommand.CanExecute(object parameter)
        {
            return (parameter == null || parameter is T) &&
                   CanExecute((T)parameter);
        }

        void ICommand.Execute(object parameter)
        {
            Execute((T)parameter);
        }

        //[SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }

        protected virtual void OnIsActiveChanged()
        {
            var eventHandler = IsActiveChanged;
            if (eventHandler == null)
                return;
            eventHandler(this, EventArgs.Empty);
        }

        protected virtual void OnCanExecuteChanged()
        {
            var dispatcher = (Dispatcher)null;

            if (Application.Current != null)
                dispatcher = Application.Current.Dispatcher;
           
            var eventHandler = CanExecuteChanged;
            if (eventHandler == null)
                return;

            if (dispatcher != null && !dispatcher.CheckAccess())
                dispatcher.Invoke((Action)OnCanExecuteChanged);
            else
                eventHandler(this, EventArgs.Empty);
        }
    }
}