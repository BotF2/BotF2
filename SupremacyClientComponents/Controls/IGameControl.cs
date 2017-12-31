using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Supremacy.Client.Controls
{
    internal interface IVariantControl
    {
        VariantSize VariantSize { get; set; }
        GameControlContext Context { get; set; }
    }

    internal interface IGameControl : ICommandSource, IVariantControl
    {
        bool CanUpdateCanExecuteWhenHidden { get; }
        object CoerceCommandParameter(DependencyObject obj, object value);
        void CoerceValue(DependencyProperty property);
        EventHandler CommandCanExecuteHandler { get; set; }
        GameControlService.GameControlFlagManager Flags { get; }
        bool HasImage { get; set; }
        bool HasLabel { get; set; }
        ImageSource ImageSourceLarge { get; set; }
        ImageSource ImageSourceSmall { get; set; }
        bool IsVisible { get; }
        string Label { get; set; }
        void OnCanExecuteChanged(object sender, EventArgs e);
        void OnCommandChanged(ICommand oldCommand, ICommand newCommand);
        void OnCommandUIProviderPropertyChanged(object sender, PropertyChangedEventArgs e);
        void UpdateCanExecute();
        void OnCommandParameterChanged(object oldValue, object newValue);
    }
}