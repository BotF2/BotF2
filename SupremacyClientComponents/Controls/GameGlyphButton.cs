// GameGlyphButton.cs
// 
// Copyright (c) 2011 Mike Strobel
// 
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
// 
// All other rights reserved.

using System.Windows;

using System.Windows.Media;

namespace Supremacy.Client.Controls
{
    public class GameGlyphButton : GameButtonBase
    {
        public static readonly DependencyProperty GlyphProperty =
            DependencyProperty.Register(
                "Glyph",
                typeof(Geometry),
                typeof(GameGlyphButton),
                new PropertyMetadata(default(Geometry)));

        static GameGlyphButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(GameGlyphButton),
                new FrameworkPropertyMetadata(typeof(GameGlyphButton)));
        }

        public Geometry Glyph
        {
            get => (Geometry)GetValue(GlyphProperty);
            set => SetValue(GlyphProperty, value);
        }
    }
}