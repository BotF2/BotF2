// ColonyScreenView.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.Windows.Input;

using Microsoft.Practices.Unity;

using Supremacy.Annotations;

namespace Supremacy.Client.Views
{
    public class ColonyScreenView : GameScreenView<ColonyScreenPresentationModel>, IColonyScreenView
    {
        public ColonyScreenView([NotNull] IUnityContainer container) : base(container)
        {
            InputBindings.Add(
                new KeyBinding(
                    ColonyScreenCommands.PreviousColonyCommand,
                    Key.Left,
                    ModifierKeys.None));

            InputBindings.Add(
                new KeyBinding(
                    ColonyScreenCommands.NextColonyCommand,
                    Key.Right,
                    ModifierKeys.None));

            InputBindings.Add(
                new KeyBinding(
                    ColonyScreenCommands.PreviousColonyCommand,
                    Key.BrowserBack,
                    ModifierKeys.None));

            InputBindings.Add(
                new KeyBinding(
                    ColonyScreenCommands.NextColonyCommand,
                    Key.BrowserForward,
                    ModifierKeys.None));

            // ToDo: Numeric 1 to 4  should toggles between Tab 1 to 4  (not done yet)
            InputBindings.Add(
                new KeyBinding(
                    ColonyScreenCommands.NextColonyCommand,
                    Key.D1,
                    ModifierKeys.Control));

            InputBindings.Add(
                new KeyBinding(
                    ColonyScreenCommands.NextColonyCommand,
                    Key.D2,
                    ModifierKeys.Control));

            InputBindings.Add(
                new KeyBinding(
                    ColonyScreenCommands.NextColonyCommand,
                    Key.D3,
                    ModifierKeys.Control));

            InputBindings.Add(
                new KeyBinding(
                    ColonyScreenCommands.NextColonyCommand,
                    Key.D3,
                    ModifierKeys.Control));
        }

        public override void OnDestroyed()
        {
            base.OnDestroyed();
            Style = null;
            Template = null;
        }

/*
        ~ColonyScreenView()
        {
            Console.Beep();
        }
*/
    }
}