using System.Windows;

namespace Supremacy.UI
{
    #region DependencyPropertyChangedEventArgs<T>
    public delegate void DependencyPropertyChangedEventHandler<T>(object sender, DependencyPropertyChangedEventArgs<T> e);
    #endregion

    public struct DependencyPropertyChangedEventArgs<T>
    {
        #region Fields
        private DependencyPropertyChangedEventArgs _baseArgs;
        #endregion

        #region Constructors
        public DependencyPropertyChangedEventArgs(DependencyPropertyChangedEventArgs args)
        {
            _baseArgs = args;
        }
        #endregion

        #region Properties
        public T OldValue
        {
            get
            {
                if (_baseArgs.OldValue == null)
                    return default(T);
                return (T)_baseArgs.OldValue;
            }
        }

        public T NewValue
        {
            get
            {
                if (_baseArgs.NewValue == null)
                    return default(T);
                return (T)_baseArgs.NewValue;
            }
        }

        public DependencyProperty Property
        {
            get { return _baseArgs.Property; }
        }
        #endregion
    }
}