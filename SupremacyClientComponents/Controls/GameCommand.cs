using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Supremacy.Client.Controls
{
    public class GameCommand : RoutedCommand, IGameCommandUIProvider
    {
        private ImageSource _imageSourceLarge;
        private ImageSource _imageSourceSmall;
        private string _label;

        public event PropertyChangedEventHandler PropertyChanged;

        public GameCommand() { }

        public GameCommand(string name, Type ownerType)
            : this(name, ownerType, null, null, (ImageSource)null, null) { }

        public GameCommand(string name, Type ownerType, string label)
            : this(name, ownerType, label, null, (ImageSource)null, null) { }

        public GameCommand(string name, Type ownerType, string label, string imageSourceLarge, string imageSourceSmall)
            : this(name, ownerType, label, imageSourceLarge, imageSourceSmall, null) { }

        public GameCommand(
            string name,
            Type ownerType,
            string label,
            string imageSourceLarge,
            string imageSourceSmall,
            InputGestureCollection inputGestures)
            : this(name,
                     ownerType,
                     label,
                     !string.IsNullOrEmpty(imageSourceLarge)
                          ? new BitmapImage(new Uri(imageSourceLarge, UriKind.RelativeOrAbsolute))
                          : null,
                     !string.IsNullOrEmpty(imageSourceSmall)
                          ? new BitmapImage(new Uri(imageSourceSmall, UriKind.RelativeOrAbsolute))
                          : null,
                     inputGestures)
        { }

        public GameCommand(
            string name,
            Type ownerType,
            string label,
            ImageSource imageSourceLarge,
            ImageSource imageSourceSmall)
            : this(name, ownerType, label, imageSourceLarge, imageSourceSmall, null) { }

        public GameCommand(
            string name,
            Type ownerType,
            string label,
            ImageSource imageSourceLarge,
            ImageSource imageSourceSmall,
            InputGestureCollection inputGestures)
            : base(name, ownerType, inputGestures)
        {
            Label = label;
            ImageSourceLarge = imageSourceLarge;
            ImageSourceSmall = imageSourceSmall;
        }

        internal static bool CanExecuteCommandSource(ICommandSource commandSource)
        {
            return CanExecuteCommandSource(commandSource, (IInputElement)null);
        }

        internal static bool CanExecuteCommandSource(ICommandSource commandSource, IInputElement alternateTarget)
        {
            ICommand command = commandSource.Command;
            if (command == null)
            {
                return true;
            }

            object parameter = commandSource.CommandParameter;

            if (!(command is RoutedCommand routedCommand))
            {
                return command.CanExecute(parameter);
            }

            return routedCommand.CanExecute(
                parameter,
                commandSource.CommandTarget ?? alternateTarget);
        }

        internal static bool CanExecuteCommandSource(ICommandSource commandSource, ICommand alternateCommand)
        {
            if (alternateCommand == null)
            {
                return true;
            }

            object parameter = commandSource.CommandParameter;

            if (!(alternateCommand is RoutedCommand routedCommand))
            {
                return alternateCommand.CanExecute(parameter);
            }

            return routedCommand.CanExecute(
                parameter,
                commandSource.CommandTarget ?? commandSource as IInputElement);
        }

        internal static void ExecuteCommandSource(ICommandSource commandSource)
        {
            ICommand command = commandSource.Command;
            if (command == null)
            {
                return;
            }

            object parameter = commandSource.CommandParameter;

            if (command is RoutedCommand routedCommand)
            {
                IInputElement commandTarget = commandSource.CommandTarget ?? commandSource as IInputElement;
                if (routedCommand.CanExecute(parameter, commandTarget))
                {
                    routedCommand.Execute(parameter, commandTarget);
                }
            }
            else if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }
        }

        internal static void ExecuteCommandSource(ICommandSource commandSource, ICommand alternateCommand)
        {
            if (alternateCommand == null)
            {
                return;
            }

            object parameter = commandSource.CommandParameter;
            if (alternateCommand is RoutedCommand routedCommand)
            {
                IInputElement commandTarget = commandSource.CommandTarget ?? commandSource as IInputElement;
                if (routedCommand.CanExecute(parameter, commandTarget))
                {
                    routedCommand.Execute(parameter, commandTarget);
                }
            }
            else if (alternateCommand.CanExecute(parameter))
            {
                alternateCommand.Execute(parameter);
            }
        }

        public ImageSource ImageSourceLarge
        {
            get => _imageSourceLarge;
            set
            {
                if (_imageSourceLarge == value)
                {
                    return;
                }

                _imageSourceLarge = value;
                NotifyPropertyChanged("ImageSourceLarge");
            }
        }

        public ImageSource ImageSourceSmall
        {
            get => _imageSourceSmall;
            set
            {
                if (_imageSourceSmall == value)
                {
                    return;
                }

                _imageSourceSmall = value;
                NotifyPropertyChanged("ImageSourceSmall");
            }
        }

        [Localizability(LocalizationCategory.Label)]
        public string Label
        {
            get => _label;
            set
            {
                if (_label == value)
                {
                    return;
                }

                _label = value;
                NotifyPropertyChanged("Label");
            }
        }

        protected void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public object Tag { get; set; }
    }
}