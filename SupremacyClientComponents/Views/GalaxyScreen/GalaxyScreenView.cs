// GalaxyScreenView.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Microsoft.Practices.Unity;
using Supremacy.Annotations;
using Supremacy.Client.Commands;
using System.Windows;
using System.Windows.Input;

namespace Supremacy.Client.Views
{
    public class GalaxyScreenView : GameScreenView<GalaxyScreenPresentationModel>, IGalaxyScreenView
    {
        static GalaxyScreenView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(GalaxyScreenView),
                new FrameworkPropertyMetadata(typeof(GalaxyScreenView)));
        }

        public GalaxyScreenView([NotNull] IUnityContainer container)
            : base(container)
        {

            // ModifierKeys:  None is not available, whyever - HotKeys for GalaxyScreen

            _ = InputBindings.Add(new KeyBinding(ClientCommands.EscapeCommand, Key.Escape, ModifierKeys.Control));
            _ = InputBindings.Add(new KeyBinding(ClientCommands.EndTurn, Key.Enter, ModifierKeys.Control));

            _ = InputBindings.Add(new KeyBinding(DebugCommands.RevealMap, Key.F, ModifierKeys.Control)); // lift Fog of War
            _ = InputBindings.Add(new KeyBinding(DebugCommands.CheatMenu, Key.C, ModifierKeys.Control));
            _ = InputBindings.Add(new KeyBinding(DebugCommands.OutputMap, Key.M, ModifierKeys.Alt)); // Map output

            // _ = InputBindings.Add(new KeyBinding(GalaxyScreenCommands.SetOverviewMode, Key.M, ModifierKeys.Control)); // Military view
            // _ = InputBindings.Add(new KeyBinding(GalaxyScreenCommands.SetOverviewMode, Key.T, ModifierKeys.Control)); // Trade view = Economy


            _ = InputBindings.Add(new KeyBinding(GalaxyScreenCommands.MapZoomIn, Key.Add, ModifierKeys.Control));
            _ = InputBindings.Add(new KeyBinding(GalaxyScreenCommands.MapZoomIn, Key.OemPlus, ModifierKeys.Control));
            _ = InputBindings.Add(new KeyBinding(GalaxyScreenCommands.MapZoomOut, Key.Subtract, ModifierKeys.Control));
            _ = InputBindings.Add(new KeyBinding(GalaxyScreenCommands.MapZoomOut, Key.OemMinus, ModifierKeys.Control));


            _ = CommandBindings.Add(new CommandBinding(ClientCommands.EscapeCommand, ExecuteEscapeCommand, CanExecuteEscapeCommand));
        }

        private void CanExecuteEscapeCommand(object sender, CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = (Model.SelectedTaskForce != null) ||
                               (Model.SelectedTradeRoute != null) ||
                               (Model.InputMode == GalaxyScreenInputMode.RedeployShips);
        }

        private void ExecuteEscapeCommand(object sender, ExecutedRoutedEventArgs args)
        {
            if (Model.SelectedTaskForce != null)
            {
                Model.SelectedTaskForce = null;
                args.Handled = true;
            }
            else if (Model.SelectedTradeRoute != null)
            {
                Model.SelectedTradeRoute = null;
                args.Handled = true;
            }
            else if (Model.InputMode == GalaxyScreenInputMode.RedeployShips)
            {
                Model.InputMode = GalaxyScreenInputMode.Normal;
                args.Handled = true;
            }
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            if (Model.InputMode == GalaxyScreenInputMode.Normal)
            {
                /*
                 * If a task force or trade route is selected, and the user presses the
                 * right mouse button, we only want to cancel the selection--we don't want
                 * to display the pop-up menu in this case.
                 */
                if (Model.SelectedTaskForce != null)
                {
                    Model.SelectedTaskForce = null;
                    e.Handled = true;
                    _ = CaptureMouse();
                    return;
                }

                if (Model.SelectedTradeRoute != null)
                {
                    Model.SelectedTradeRoute = null;
                    e.Handled = true;
                    _ = CaptureMouse();
                    return;
                }
            }
            base.OnMouseRightButtonDown(e);
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
                e.Handled = true;
                return;
            }
            base.OnMouseRightButtonUp(e);
        }
    }
}
