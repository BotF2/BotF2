// AsteroidsView.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Supremacy.Client;
using Supremacy.Resources;
using Supremacy.Utility;

namespace Supremacy.UI
{
    public sealed class AsteroidsView : FrameworkElement, IAnimationsHost
    {
        #region Constants
        private const string ImagePath = "Resources/Images/Planets/Asteroids/";
        #endregion

        #region Fields
        private static readonly DependencyProperty CurrentFrameProperty;
        private static readonly CachedBitmap[] Frames;

        private readonly Int32Animation _animation;
        private readonly AnimationClock _animationClock;
        private readonly Rectangle _rectangle;
        private readonly ImageBrush _imageBrush;
        #endregion

        #region Constructors
        static AsteroidsView()
        {
            FocusVisualStyleProperty.OverrideMetadata(
                typeof(AsteroidsView),
                new FrameworkPropertyMetadata(
                    null,
                    (d, baseValue) => null));
            WidthProperty.OverrideMetadata(
                typeof(AsteroidsView),
                new FrameworkPropertyMetadata(
                    96.0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure));
            HeightProperty.OverrideMetadata(
                typeof(AsteroidsView),
                new FrameworkPropertyMetadata(
                    96.0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure));
            CurrentFrameProperty = DependencyProperty.Register(
                "CurrentFrame",
                typeof(int),
                typeof(AsteroidsView),
                new FrameworkPropertyMetadata(
                    0,
                    FrameworkPropertyMetadataOptions.None));
            Frames = new CachedBitmap[256];
            for (int i = 0; i < Frames.Length; i++)
            {
                Frames[i] = new CachedBitmap(
                    new BitmapImage(
                        ResourceManager.GetResourceUri(ImagePath + string.Format("asteroids1_{0:000}.png", i))),
                    BitmapCreateOptions.None,
                    BitmapCacheOption.OnLoad);
                Frames[i].Freeze();
            }
        }

        public AsteroidsView()
        {
            /* Start on a random frame to provide some variation */
            CurrentFrame = RandomHelper.Random(Frames.Length);

            _imageBrush = new ImageBrush { ImageSource = Frames[CurrentFrame % 255] };
            _rectangle = new Rectangle
            {
                ToolTip = ResourceManager.GetString("PLANET_TYPE_ASTEROIDS"),
                Fill = _imageBrush
            };

            AddVisualChild(_rectangle);

            _animation = new Int32Animation(
                CurrentFrame,
                CurrentFrame + 255,
                new Duration(new TimeSpan(0, 0, 30)))
                    {
                        RepeatBehavior = RepeatBehavior.Forever
                    };

            _animationClock = _animation.CreateClock();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }
        #endregion

        #region Properties
        public int CurrentFrame
        {
            get { return (int)GetValue(CurrentFrameProperty); }
            private set { SetValue(CurrentFrameProperty, value); }
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }
        #endregion

        #region IAnimationsHost Members
        public void PauseAnimations()
        {
            if (!_animationClock.IsPaused && _animationClock.Controller != null)
                _animationClock.Controller.Pause();
        }

        public void ResumeAnimations()
        {
            if (_animationClock.Controller == null)
                return;

            if (_animationClock.CurrentState == ClockState.Stopped)
                _animationClock.Controller.Begin();
            if (_animationClock.IsPaused)
                _animationClock.Controller.Resume();
        }

        public void StopAnimations()
        {
            if (_animationClock.Controller == null)
                return;

            _animationClock.Controller.Pause();
            _animationClock.Controller.Remove();
        }
        #endregion

        #region Methods
        public static void PreloadImages() {}

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            StopAnimations();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ClientSettings.Current.EnableAnimation)  
                StartAnimations();
        }

        private void StartAnimations()
        {
            if (!HasAnimatedProperties)
                ApplyAnimationClock(CurrentFrameProperty, _animationClock);

            if(ClientSettings.Current.EnableAnimation)
                ResumeAnimations();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _rectangle.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return _rectangle;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            _rectangle.Measure(availableSize);
            return _rectangle.DesiredSize;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property != CurrentFrameProperty)
                return;
            if (IsVisible)
                _imageBrush.ImageSource = Frames[(int)e.NewValue % 255];
        }
        #endregion
    }
}