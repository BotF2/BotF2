// ButtonChrome.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

using Supremacy.Types;
using Supremacy.Utility;

namespace Supremacy.UI
{
    /// <summary>Creates the theme-specific look for Microsoft .NET Framework version 3.0 <see cref="T:System.Windows.Controls.Button"></see> elements.</summary>
    public sealed class ButtonChrome : Decorator
    {
        #region Constants
        private const double OverlayOpacityCeiling = 1.0;
        #endregion

        #region Fields
        /// <summary>Identifies the <see cref="P:Supremacy.UI.ButtonChrome.Background"></see> dependency property.</summary>
        /// <returns>The identifier for the <see cref="P:Supremacy.UI.ButtonChrome.Background"></see> dependency property.</returns>
        public static readonly DependencyProperty BackgroundProperty;

        /// <summary>Identifies the <see cref="P:Supremacy.UI.ButtonChrome.BorderBrush"></see> dependency property.</summary>
        /// <returns>The identifier for the <see cref="P:Supremacy.UI.ButtonChrome.BorderBrush"></see> dependency property.</returns>
        public static readonly DependencyProperty BorderBrushProperty;

        public static DependencyProperty RenderBackgroundProperty;
        public static readonly DependencyProperty DisabledBackgroundProperty;
        public static readonly DependencyProperty HoverBackgroundProperty;
        public static readonly DependencyProperty PressedBackgroundProperty;

        private static readonly DependencyPropertyKey RenderBackgroundPropertyKey;

        /// <summary>Identifies the <see cref="P:Supremacy.UI.ButtonChrome.RenderDefaulted"></see> dependency property.</summary>
        /// <returns>The identifier for the <see cref="P:Supremacy.UI.ButtonChrome.RenderDefaulted"></see> dependency property.</returns>
        public static readonly DependencyProperty RenderDefaultedProperty;

        /// <summary>Identifies the <see cref="P:Supremacy.UI.ButtonChrome.RenderMouseOver"></see> dependency property.</summary>
        /// <returns>The identifier for the <see cref="P:Supremacy.UI.ButtonChrome.RenderMouseOver"></see> dependency property.</returns>
        public static readonly DependencyProperty RenderMouseOverProperty;

        /// <summary>Identifies the <see cref="P:Supremacy.UI.ButtonChrome.RenderPressed"></see> dependency property.</summary>
        /// <returns>The identifier for the <see cref="P:Supremacy.UI.ButtonChrome.RenderPressed"></see> dependency property.</returns>
        public static readonly DependencyProperty RenderPressedProperty;

        private readonly StateScope _suppressInvalidateScope;
        private LocalResources _localResources;
        #endregion

        #region Constructors
        static ButtonChrome()
        {
            BackgroundProperty = Control.BackgroundProperty.AddOwner(typeof(ButtonChrome), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
            RenderBackgroundPropertyKey = DependencyProperty.RegisterReadOnly("RenderBackground", typeof(Brush), typeof(ButtonChrome), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
            RenderBackgroundProperty = RenderBackgroundPropertyKey.DependencyProperty;
            HoverBackgroundProperty = DependencyProperty.Register("HoverBackground", typeof(Brush), typeof(ButtonChrome), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
            PressedBackgroundProperty = DependencyProperty.Register("PressedBackground", typeof(Brush), typeof(ButtonChrome), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
            DisabledBackgroundProperty = DependencyProperty.Register("DisabledBackground", typeof(Brush), typeof(ButtonChrome), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
            BorderBrushProperty = Border.BorderBrushProperty.AddOwner(typeof(ButtonChrome), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
            RenderDefaultedProperty = DependencyProperty.Register("RenderDefaulted", typeof(bool), typeof(ButtonChrome), new FrameworkPropertyMetadata(false, OnRenderDefaultedChanged));
            RenderMouseOverProperty = DependencyProperty.Register("RenderMouseOver", typeof(bool), typeof(ButtonChrome), new FrameworkPropertyMetadata(false, OnRenderMouseOverChanged));
            RenderPressedProperty = DependencyProperty.Register("RenderPressed", typeof(bool), typeof(ButtonChrome), new FrameworkPropertyMetadata(false, OnRenderPressedChanged));
            IsEnabledProperty.OverrideMetadata(typeof(ButtonChrome), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));
        }

        public ButtonChrome()
        {
            _suppressInvalidateScope = new StateScope();
            Loaded += OnLoaded;
        }
        #endregion

        #region Properties
        private bool Animates
        {
            get
            {
                if (((SystemParameters.PowerLineStatus == PowerLineStatus.Online) && SystemParameters.ClientAreaAnimation) && (RenderCapability.Tier > 0))
                {
                    return IsEnabled;
                }
                return false;
            }
        }

        /// <summary>Gets or sets the brush used to fill the background of the <see cref="T:System.Windows.Controls.Button"></see>.</summary>
        /// <returns>The brush used to fill the background of the <see cref="T:System.Windows.Controls.Button"></see>.</returns>
        public Brush Background
        {
            get { return GetValue(BackgroundProperty) as Brush ?? Brushes.Transparent; }
            set { SetValue(BackgroundProperty, value); }
        }

        public Brush HoverBackground
        {
            get { return GetValue(HoverBackgroundProperty) as Brush ?? Brushes.Transparent; }
            set { SetValue(HoverBackgroundProperty, value); }
        }

        public Brush PressedBackground
        {
            get { return GetValue(PressedBackgroundProperty) as Brush ?? Brushes.Transparent; }
            set { SetValue(PressedBackgroundProperty, value); }
        }

        public Brush DisabledBackground
        {
            get { return GetValue(DisabledBackgroundProperty) as Brush ?? Brushes.Transparent; }
            set { SetValue(DisabledBackgroundProperty, value); }
        }

        public Brush RenderBackground
        {
            get { return GetValue(RenderBackgroundProperty) as Brush ?? Brushes.Transparent; }
            private set { SetValue(RenderBackgroundPropertyKey, value); }
        }

        private void UpdateRenderBrush()
        {
            VisualBrush visualBrush;
            DrawingVisual drawing = new DrawingVisual();
            Rect bounds = new Rect(0, 0, ActualWidth, ActualHeight);
            using (DrawingContext dc = drawing.RenderOpen())
            {
                if (IsEnabled)
                {
                    Brush brush = Background;
                    if (brush != null)
                    {
                        dc.DrawRectangle(
                            brush,
                            null,
                            bounds);
                        if (IsEnabled)
                        {
                            brush = BackgroundOverlay;
                            if (brush != null)
                            {
                                dc.DrawRectangle(
                                    brush,
                                    null,
                                    bounds);
                            }
                        }
                    }
                }
                else
                {
                    dc.PushOpacity(0.25);
                    dc.DrawRectangle(
                        DisabledBackground,
                        null,
                        new Rect(DesiredSize));
                    dc.Pop();
                }
            }
            visualBrush = new VisualBrush(drawing);
            RenderBackground = visualBrush;
        }

        private Brush BackgroundOverlay
        {
            get
            {
                //if (!base.IsEnabled)
                //{
                //    return DisabledBackground;
                //}
                if (!Animates)
                {
                    if (RenderPressed)
                    {
                        return PressedBackground;
                    }
                    if (RenderMouseOver)
                    {
                        return HoverBackground;
                    }
                    return null;
                }
                if (_localResources == null)
                {
                    return null;
                }
                if (RenderPressed)
                {
                    return PressedBackground;
                }
                if (_localResources.BackgroundOverlay == null)
                {
                    _localResources.BackgroundOverlay = HoverBackground.Clone();
                    _localResources.BackgroundOverlay.Opacity = 0;
                }
                return _localResources.BackgroundOverlay;
            }
        }

        /// <summary>Gets or sets the brush used to draw the outer border of the <see cref="T:System.Windows.Controls.Button"></see>.</summary>
        /// <returns>The brush used to draw the outer border of the <see cref="T:System.Windows.Controls.Button"></see>.</returns>
        public Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        /// <summary>Gets or sets a value indicating whether the <see cref="T:System.Windows.Controls.Button"></see> has the appearance of the default button on the form.</summary>
        /// <returns>true if the <see cref="T:System.Windows.Controls.Button"></see> has the appearance of the default button; otherwise false.</returns>
        public bool RenderDefaulted
        {
            get { return (bool)GetValue(RenderDefaultedProperty); }
            set { SetValue(RenderDefaultedProperty, value); }
        }

        /// <summary>Gets or sets a value indicating whether the <see cref="T:System.Windows.Controls.Button"></see> appears as if the mouse is over it.</summary>
        /// <returns>true if the <see cref="T:System.Windows.Controls.Button"></see> appears as if the mouse is over it; otherwise false.</returns>
        public bool RenderMouseOver
        {
            get { return (bool)GetValue(RenderMouseOverProperty); }
            set { SetValue(RenderMouseOverProperty, value); }
        }

        /// <summary>Gets or sets a value indicating whether the <see cref="T:System.Windows.Controls.Button"></see> appears pressed.</summary>
        /// <returns>true if the <see cref="T:System.Windows.Controls.Button"></see> appears pressed; otherwise false.</returns>
        public bool RenderPressed
        {
            get { return (bool)GetValue(RenderPressedProperty); }
            set { SetValue(RenderPressedProperty, value); }
        }
        #endregion

        #region Methods
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if ((e.Property != RenderBackgroundProperty) && typeof(Brush).IsAssignableFrom(e.Property.PropertyType))
                UpdateRenderBrush();
        }
        private void TryInvalidateVisual()
        {
            if (!_suppressInvalidateScope.IsWithin)
                InvalidateVisual();
        }

        private static void OnRenderDefaultedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ButtonChrome chrome = (ButtonChrome)o;
            if (chrome.Parent == null)
                return;
            try
            {
                if (chrome.Animates)
                {
                    if (!chrome.RenderPressed)
                    {
                        if ((bool)e.NewValue)
                        {
                            if (chrome._localResources == null)
                            {
                                chrome._localResources = new LocalResources();
                                chrome.TryInvalidateVisual();
                            }
                            DoubleAnimationUsingKeyFrames frames1 = new DoubleAnimationUsingKeyFrames();
                            frames1.KeyFrames.Add(new LinearDoubleKeyFrame(1, TimeSpan.FromSeconds(0.5)));
                            frames1.KeyFrames.Add(new DiscreteDoubleKeyFrame(1, TimeSpan.FromSeconds(0.75)));
                            frames1.KeyFrames.Add(new LinearDoubleKeyFrame(0, TimeSpan.FromSeconds(2)));
                            frames1.RepeatBehavior = RepeatBehavior.Forever;
                            //Timeline.SetDesiredFrameRate(frames1, new Nullable<int>(10));
                            chrome.BackgroundOverlay.BeginAnimation(Brush.OpacityProperty, frames1);
                        }
                        else if (chrome._localResources == null)
                        {
                            chrome.TryInvalidateVisual();
                        }
                        else
                        {
                            Duration duration2 = new Duration(TimeSpan.FromSeconds(0.2));
                            DoubleAnimation animation2 = new DoubleAnimation();
                            animation2.Duration = duration2;
                            chrome.BackgroundOverlay.BeginAnimation(Brush.OpacityProperty, animation2);
                            ColorAnimation animation3 = new ColorAnimation();
                            animation3.Duration = duration2;
                        }
                    }
                }
                else
                {
                    chrome._localResources = null;
                    chrome.TryInvalidateVisual();
                }
            }
            catch (Exception ex) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(ex);
            }

            chrome.UpdateRenderBrush();
        }

        private static void OnRenderMouseOverChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ButtonChrome chrome = (ButtonChrome)o;
            if (chrome.Parent == null)
                return;
            try
            {
                if (chrome.Animates)
                {
                    if (!chrome.RenderPressed)
                    {
                        if ((bool)e.NewValue)
                        {
                            if (chrome._localResources == null)
                            {
                                chrome._localResources = new LocalResources();
                                chrome.TryInvalidateVisual();
                            }
                            Duration duration1 = new Duration(TimeSpan.FromSeconds(0.3));
                            DoubleAnimation animation1 = new DoubleAnimation(OverlayOpacityCeiling, duration1);
                            chrome.BackgroundOverlay.BeginAnimation(Brush.OpacityProperty, animation1);
                        }
                        else if (chrome._localResources == null)
                        {
                            chrome.TryInvalidateVisual();
                        }
                        else if (chrome.RenderDefaulted)
                        {
                            double num1 = chrome.BackgroundOverlay.Opacity;
                            double num2 = (OverlayOpacityCeiling - num1) * 0.5;
                            DoubleAnimationUsingKeyFrames frames1 = new DoubleAnimationUsingKeyFrames();
                            frames1.KeyFrames.Add(new LinearDoubleKeyFrame(OverlayOpacityCeiling, TimeSpan.FromSeconds(num2)));
                            frames1.KeyFrames.Add(new DiscreteDoubleKeyFrame(OverlayOpacityCeiling, TimeSpan.FromSeconds(num2 + 0.25)));
                            frames1.KeyFrames.Add(new LinearDoubleKeyFrame(0, TimeSpan.FromSeconds(num2 + 1.5)));
                            frames1.KeyFrames.Add(new LinearDoubleKeyFrame(num1, TimeSpan.FromSeconds(2)));
                            frames1.RepeatBehavior = RepeatBehavior.Forever;
                            //Timeline.SetDesiredFrameRate(frames1, new Nullable<int>(10));
                            chrome.BackgroundOverlay.BeginAnimation(Brush.OpacityProperty, frames1);
                        }
                        else
                        {
                            Duration duration2 = new Duration(TimeSpan.FromSeconds(0.2));
                            DoubleAnimation animation2 = new DoubleAnimation();
                            animation2.Duration = duration2;
                            chrome.BackgroundOverlay.BeginAnimation(Brush.OpacityProperty, animation2);
                        }
                    }
                }
                else
                {
                    chrome._localResources = null;
                    chrome.TryInvalidateVisual();
                }
            }
            catch (Exception ex) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(ex);
            }

            chrome.UpdateRenderBrush();
        }

        private static void OnRenderPressedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            //ButtonChrome chrome = (ButtonChrome)o;
            //if (chrome.Animates)
            //{
            //    if ((bool)e.NewValue)
            //    {
            //        if (chrome._localResources == null)
            //        {
            //            chrome._localResources = new Supremacy.UI.ButtonChrome.LocalResources();
            //            chrome.TryInvalidateVisual();
            //        }
            //        Duration duration1 = new Duration(TimeSpan.FromSeconds(0.1));
            //        DoubleAnimation animation1 = new DoubleAnimation(1, duration1);
            //        chrome.BackgroundOverlay.BeginAnimation(Brush.OpacityProperty, animation1);
            //        ColorAnimation animation2 = new ColorAnimation(Color.FromRgb(0xc2, 0xe4, 0xf6), duration1);
            //        GradientStopCollection collection1 = ((LinearGradientBrush)chrome.BackgroundOverlay).GradientStops;
            //        collection1[0].BeginAnimation(GradientStop.ColorProperty, animation2);
            //        collection1[1].BeginAnimation(GradientStop.ColorProperty, animation2);
            //        animation2 = new ColorAnimation(Color.FromRgb(0xab, 0xda, 0xf3), duration1);
            //        collection1[2].BeginAnimation(GradientStop.ColorProperty, animation2);
            //        animation2 = new ColorAnimation(Color.FromRgb(0x90, 0xcb, 0xeb), duration1);
            //        collection1[3].BeginAnimation(GradientStop.ColorProperty, animation2);
            //        animation2 = new ColorAnimation(Color.FromRgb(0x2c, 0x62, 0x8b), duration1);
            //    }
            //    else if (chrome._localResources == null)
            //    {
            //        chrome.TryInvalidateVisual();
            //    }
            //    else
            //    {
            //        bool flag1 = chrome.RenderMouseOver;
            //        Duration duration2 = new Duration(TimeSpan.FromSeconds(0.1));
            //        DoubleAnimation animation3 = new DoubleAnimation();
            //        animation3.Duration = duration2;
            //        if (!flag1)
            //        {
            //            chrome.BackgroundOverlay.BeginAnimation(Brush.OpacityProperty, animation3);
            //        }
            //        ColorAnimation animation4 = new ColorAnimation();
            //        animation4.Duration = duration2;
            //        GradientStopCollection collection2 = ((LinearGradientBrush)chrome.BackgroundOverlay).GradientStops;
            //        collection2[0].BeginAnimation(GradientStop.ColorProperty, animation4);
            //        collection2[1].BeginAnimation(GradientStop.ColorProperty, animation4);
            //        collection2[2].BeginAnimation(GradientStop.ColorProperty, animation4);
            //        collection2[3].BeginAnimation(GradientStop.ColorProperty, animation4);
            //    }
            //}
            //else
            //{
            //    chrome._localResources = null;
            //    chrome.TryInvalidateVisual();
            //}

            try
            {
                ButtonChrome chrome = (ButtonChrome)o;
                if (chrome.Parent == null)
                    return;
                chrome.UpdateRenderBrush();
            }
            catch (Exception ex) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(ex);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            TryInvalidateVisual();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            //Rect rect1 = new Rect();
            //rect1.Width = Math.Max((double)0, (double)(finalSize.Width - 4));
            //rect1.Height = Math.Max((double)0, (double)(finalSize.Height - 4));
            //rect1.X = (finalSize.Width - rect1.Width) * 0.5;
            //rect1.Y = (finalSize.Height - rect1.Height) * 0.5;
            //UIElement element1 = this.Child;
            //if (element1 != null)
            //{
            //    element1.Arrange(rect1);
            //}
            //return finalSize;
            Size size = base.ArrangeOverride(finalSize);
            UpdateRenderBrush();
            return size;
        }
        #endregion

        #region LocalResources Type
        private class LocalResources
        {
            #region Fields
            public Brush BackgroundOverlay;
            #endregion
        }
        #endregion
    }
}