// GameContextMenu.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Supremacy.Client
{
    /// <summary>
    /// ========================================
    /// WinFX Custom Control
    /// ========================================
    ///
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Supremacy.Client"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Supremacy.Client;assembly=Supremacy.Client"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file. Note that Intellisense in the
    /// XML editor does not currently work on custom controls and its child elements.
    ///
    ///     <MyNamespace:GameContextMenu/>
    ///
    /// </summary>
    public class GameContextMenu : ContextMenu
    {
        public static readonly RoutedCommand MainCommand = new RoutedCommand("Main", typeof(GameContextMenu));
        public static readonly RoutedCommand AffairsCommand = new RoutedCommand("Intel", typeof(GameContextMenu));
        public static readonly RoutedCommand SystemCommand = new RoutedCommand("System", typeof(GameContextMenu));
        public static readonly RoutedCommand ScienceCommand = new RoutedCommand("Science", typeof(GameContextMenu));
        public static readonly RoutedCommand MenuCommand = new RoutedCommand("Menu", typeof(GameContextMenu));
        public static readonly RoutedCommand DiplomacyCommand = new RoutedCommand("Diplomacy", typeof(GameContextMenu));

        static GameContextMenu()
        {
            // PropertyChangedEventHandler fred = PropertyChanged();
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GameContextMenu), new FrameworkPropertyMetadata(typeof(GameContextMenu)));
        }

        public GameContextMenu()
        {
            SetBinding(
                LayoutTransformProperty,
                new Binding
                {
                    Source = Application.Current.MainWindow,
                    Path = new PropertyPath(LayoutTransformProperty)
                });
        }
    }
}
