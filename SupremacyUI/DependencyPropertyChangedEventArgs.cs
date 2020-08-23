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
        public T OldValue => _baseArgs.OldValue == null ? default : (T)_baseArgs.OldValue;

        public T NewValue => _baseArgs.NewValue == null ? default : (T)_baseArgs.NewValue;

        public DependencyProperty Property => _baseArgs.Property;
        #endregion
    }
}