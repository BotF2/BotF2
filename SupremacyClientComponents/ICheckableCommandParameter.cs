using System;
using System.Windows;

namespace Supremacy.Client
{
    public interface ICheckableCommandParameter
    {
        event EventHandler InnerParameterChanged;

        bool Handled { get; set; }
        bool? IsChecked { get; set; }
        object InnerParameter { get; set; }
    }

    public class CheckableCommandParameter : Freezable, ICheckableCommandParameter
    {
        #region Dependency Properties
        public static readonly DependencyProperty HandledProperty = DependencyProperty.Register(
            "Handled",
            typeof(bool),
            typeof(CheckableCommandParameter),
            new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
            "IsChecked",
            typeof(bool?),
            typeof(CheckableCommandParameter),
            new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty InnerParameterProperty = DependencyProperty.Register(
            "InnerParameter",
            typeof(object),
            typeof(CheckableCommandParameter),
            new FrameworkPropertyMetadata(OnInnerParameterChanged));
        #endregion

        public event EventHandler InnerParameterChanged;

        public bool Handled
        {
            get => (bool)GetValue(HandledProperty);
            set => SetValue(HandledProperty, value);
        }

        public bool? IsChecked
        {
            get => (bool?)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        public object InnerParameter
        {
            get => GetValue(InnerParameterProperty);
            set => SetValue(InnerParameterProperty, value);
        }

        private static void OnInnerParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CheckableCommandParameter)d).InnerParameterChanged?.Invoke(d, EventArgs.Empty);
        }

        #region Overrides of Freezable
        protected override Freezable CreateInstanceCore()
        {
            return new CheckableCommandParameter();
        }
        #endregion
    }
}