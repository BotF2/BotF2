// MessageDialog.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Input;

namespace Supremacy.Client.Dialogs
{
    [Flags]
    public enum MessageDialogButtons
    {
        Ok = 0x01,
        Cancel = 0x02,
        OkCancel = Ok | Cancel,
        Yes = 0x04,
        No = 0x08,
        YesNo = Yes | No,
        YesNoCancel = YesNo | Cancel,
        Close = 0x10
    }

    public enum MessageDialogResult
    {
        None,
        Ok,
        Cancel,
        Yes,
        No,
        Close
    }

    public sealed class MessageDialog : Dialog
    {
        public static readonly RoutedCommand SetMessageDialogResultCommand = new RoutedCommand(
            "SetMessageDialogResult",
            typeof(MessageDialog));

        #region Buttons (Dependency Property)
        public static readonly DependencyProperty ButtonsProperty =
            DependencyProperty.Register(
                "Buttons",
                typeof(MessageDialogButtons),
                typeof(MessageDialog),
                new FrameworkPropertyMetadata(MessageDialogButtons.Ok));

        public MessageDialogButtons Buttons
        {
            get => (MessageDialogButtons)GetValue(ButtonsProperty);
            set => SetValue(ButtonsProperty, value);
        }
        #endregion

        #region Result (Dependency Property)
        private static readonly DependencyPropertyKey ResultPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "Result",
                typeof(MessageDialogResult),
                typeof(MessageDialog),
                new FrameworkPropertyMetadata(
                    MessageDialogResult.None,
                    OnResultChanged,
                    CoerceResult));

        private static object CoerceResult(DependencyObject d, object value)
        {
            if (!(d is MessageDialog messageDialog))
            {
                return value;
            }

            MessageDialogResult result = (MessageDialogResult)value;
            if ((result == MessageDialogResult.None) &&
                Equals(messageDialog.ReadLocalValue(ResultProperty), MessageDialogResult.None) &&
                ((messageDialog.Buttons & MessageDialogButtons.Cancel) == MessageDialogButtons.Cancel))
            {
                return MessageDialogResult.Cancel;
            }

            return value;
        }

        private static void OnResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is MessageDialog messageDialog))
            {
                return;
            }

            switch ((MessageDialogResult)e.NewValue)
            {
                case MessageDialogResult.None:
                    messageDialog.Close();
                    return;
                case MessageDialogResult.Cancel:
                    messageDialog.DialogResult = false;
                    return;
                default:
                    messageDialog.DialogResult = true;
                    return;
            }
        }

        public static readonly DependencyProperty ResultProperty = ResultPropertyKey.DependencyProperty;

        public MessageDialogResult Result
        {
            get => (MessageDialogResult)GetValue(ResultProperty);
            private set => SetValue(ResultPropertyKey, value);
        }
        #endregion

        static MessageDialog()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(MessageDialog),
                new FrameworkPropertyMetadata(typeof(MessageDialog)));
        }

        private MessageDialog()
        {
            _ = CommandBindings.Add(
                new CommandBinding(
                    SetMessageDialogResultCommand,
                    ExecuteSetMessageDialogResultCommand));
        }

        private void ExecuteSetMessageDialogResultCommand(object sender, ExecutedRoutedEventArgs args)
        {
            object parameter = args.Parameter;
            if (!(parameter is MessageDialogButtons))
            {
                return;
            }

            MessageDialogButtons button = (MessageDialogButtons)parameter;
            switch (button)
            {
                case MessageDialogButtons.Ok:
                    Result = MessageDialogResult.Ok;
                    return;
                case MessageDialogButtons.Cancel:
                    Result = MessageDialogResult.Cancel;
                    return;
                case MessageDialogButtons.Yes:
                    Result = MessageDialogResult.Yes;
                    return;
                case MessageDialogButtons.No:
                    Result = MessageDialogResult.No;
                    return;
                case MessageDialogButtons.Close:
                    Result = MessageDialogResult.Close;
                    return;
                default:
                    Result = MessageDialogResult.None;
                    return;
            }
        }

        public static MessageDialogResult Show(object content, MessageDialogButtons buttons)
        {
            return Show(null, content, buttons);
        }

        public static MessageDialogResult Show(string header, object content, MessageDialogButtons buttons)
        {
            MessageDialog dialog = new MessageDialog { Header = header, Content = content, Buttons = buttons };
            bool? dialogResult = dialog.ShowDialog();
            if (!dialogResult.HasValue)
            {
                if ((buttons & MessageDialogButtons.Cancel) == MessageDialogButtons.Cancel)
                {
                    return MessageDialogResult.Cancel;
                }

                return MessageDialogResult.None;
            }
            return dialog.Result;
        }
    }
}