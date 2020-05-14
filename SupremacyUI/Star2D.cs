// Star2D.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using ShaderEffectLibrary;

using Supremacy.Client;
using Supremacy.Resources;
using Supremacy.Universe;



namespace Supremacy.UI
{
    public class Star2D : FrameworkElement, IAnimationsHost
    {
        #region Fields
        private static readonly Dictionary<StarType, Tuple<Color, Color>> Colors;
        private static readonly ImageBrush[] Frames;

        private readonly Grid _grid;
        private ClockGroup _clockGroup;
        private ParallelTimeline _parallelTimeline;
        #endregion

        #region Constructors
        static Star2D()
        {
            FocusVisualStyleProperty.OverrideMetadata(
                typeof(Star2D),
                new FrameworkPropertyMetadata(
                    null,
                    (d, baseValue) => null));

            Colors = new Dictionary<StarType, Tuple<Color, Color>>
                     {
                         {
                             StarType.Yellow,
                             new Tuple<Color, Color>(
                                Color.FromArgb(0xFF, 0xEA, 0xEB, 0xBF),
                                Color.FromArgb(0xFF, 0x3D, 0x21, 0x0D))
                         },
                         {
                             StarType.White,
                             new Tuple<Color, Color>(
                                Color.FromArgb(0xFF, 0xE7, 0xE7, 0xE8),
                                Color.FromArgb(0xFF, 0x2C, 0x2E, 0x37))
                         },
                         {
                             StarType.Blue,
                             new Tuple<Color, Color>(
                                Color.FromArgb(0xFF, 0xE2, 0xE8, 0xF4),
                                Color.FromArgb(0xFF, 0x0D, 0x26, 0x37))
                         },
                         {
                             StarType.Red,
                             new Tuple<Color, Color>(
                                Color.FromArgb(0xFF, 0xF4, 0xE3, 0xE2),
                                Color.FromArgb(0xFF, 0x37, 0x0D, 0x0D))
                         },
                         {
                             StarType.Orange,
                             new Tuple<Color, Color>(
                                Color.FromArgb(0xFF, 0xF1, 0xDF, 0xDB),
                                Color.FromArgb(0xFF, 0x39, 0x2C, 0x0A))
                         }
                     };

            Frames = new ImageBrush[5];
            for (int i = 0; i < Frames.Length; i++)
            {
                CachedBitmap frame = new CachedBitmap(
                    new BitmapImage(ResourceManager.GetResourceUri("Resources/Images/Stars/Star" + (i + 1) + ".png")),
                    BitmapCreateOptions.None,
                    BitmapCacheOption.OnLoad);
                frame.Freeze();
                Frames[i] = new ImageBrush(frame);
                Frames[i].Freeze();
            }
        }

        public Star2D(StarType starType)
        {
            _grid = new Grid
                    {
                        Width = 128,
                        Height = 128,
                        Background = Brushes.Transparent,
                        Clip = new EllipseGeometry(
                            new Point(63, 63),
                            56,
                            56)
                    };

            Initialize(starType);

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            ResumeAnimations();
        }
        #endregion

        #region Properties
        protected override int VisualChildrenCount => 1;
        #endregion

        #region IAnimationsHost Members
        public void PauseAnimations()
        {
            if (!_clockGroup.IsPaused && (_clockGroup.Controller != null))
            {
                _clockGroup.Controller.Pause();
            }
        }

        public void ResumeAnimations()
        {
            switch (_clockGroup.CurrentState)
            {
                case ClockState.Stopped:
                    if (_clockGroup.Controller != null)
                    {
                        _clockGroup.Controller.Begin();
                    }

                    break;
                default:
                    if (_clockGroup.IsPaused && (_clockGroup.Controller != null))
                    {
                        _clockGroup.Controller.Resume();
                    }

                    break;
            }
        }

        void IAnimationsHost.StopAnimations()
        {
            StopAnimations();
        }
        #endregion

        #region Methods
        private void Initialize(StarType starType)
        {
            const int secondsPerTransition = 3;

            Rectangle[] rectangles = new Rectangle[Frames.Length];
            DoubleAnimationUsingKeyFrames[] animations = new DoubleAnimationUsingKeyFrames[Frames.Length];

            _parallelTimeline = new ParallelTimeline(
                null,
                new Duration(new TimeSpan(0, 0, secondsPerTransition * Frames.Length)),
                RepeatBehavior.Forever);

            Grid innerGrid = new Grid();

            for (int i = 0; i < Frames.Length; i++)
            {
                rectangles[i] = new Rectangle
                {
                    Fill = Frames[i],
                    Height = 128,
                    Width = 128,
                    Opacity = (i == 0) ? 1.0 : 0.0
                };

                _ = innerGrid.Children.Add(rectangles[i]);

                animations[i] = new DoubleAnimationUsingKeyFrames
                {
                    AutoReverse = true,
                    AccelerationRatio = 0.0,
                    DecelerationRatio = 0.0,
                    BeginTime = new TimeSpan(0),
                    Duration = _parallelTimeline.Duration,
                    FillBehavior = FillBehavior.HoldEnd
                };

                if (i > 0)
                {
                    animations[i].KeyFrames.Add(
                        new DiscreteDoubleKeyFrame(
                            0.0, 
                            KeyTime.FromTimeSpan(new TimeSpan(0, 0, secondsPerTransition * (i - 1)))));
                    animations[i].KeyFrames.Add(
                        new LinearDoubleKeyFrame(
                            1.0,
                            KeyTime.FromTimeSpan(new TimeSpan(0, 0, secondsPerTransition * i))));
                    if (i == (Frames.Length - 1))
                    {
                        animations[i].KeyFrames.Add(
                            new LinearDoubleKeyFrame(
                                0.0,
                                KeyTime.FromTimeSpan(new TimeSpan(0, 0, secondsPerTransition * Frames.Length))));
                    }
                    else
                    {
                        animations[i].KeyFrames.Add(
                            new DiscreteDoubleKeyFrame(
                                0.0,
                                KeyTime.FromTimeSpan(new TimeSpan(0, 0, secondsPerTransition * (i + 1)))));
                    }
                }

                _parallelTimeline.Children.Add(animations[i]);
            }

            _clockGroup = _parallelTimeline.CreateClock();

            for (int i = 0; i < rectangles.Length; i++)
            {
                rectangles[i].ApplyAnimationClock(OpacityProperty, _clockGroup.Children[i] as AnimationClock);
            }

            Effect[] effects = new Effect[]
                          {
                              new BloomEffect
                              {
                                  BloomIntensity = 2.0, //1.09472,
                                  BaseIntensity = 1.5, //0.79472,
                                  BloomSaturation = 0.0, //1.21759,
                                  BaseSaturation = 1.52328,
                              },
                              new BrightExtractEffect
                              {
                                  Threshold = 0.50
                              },
                              new ColorToneEffect
                              {
                                  Desaturation = 0.0,
                                  Toned = 1.91693,
                                  LightColor = Colors[starType].Item1,
                                  DarkColor = Colors[starType].Item2
                              },
                              new LightStreakEffect
                              {
                                  BrightThreshold = 0.15,
                                  Scale = 0.35,
                                  Attenuation = 0.4
                              },
                              new MonochromeEffect
                              {
                                  FilterColor = Color.FromArgb(0xFF, 0xEF, 0xEF, 0xEF)
                              },
                              new ToneMappingEffect
                              {
                                  Exposure = 0.5,
                                  Gamma = 0.854545,
                                  Defog = 0.5,
                                  FogColor = Color.FromArgb(0xFF, 0xEF, 0xEF, 0xEF)
                              },
                              new ZoomBlurEffect
                              {
                                  BlurAmount = 0.1,
                                  Center = new Point(0.5, 0.5)
                              }
                          };

            Border parent = new Border();
            foreach (Effect effect in effects)
            {
                Border child = new Border { Effect = effect };
                parent.Child = child;
                parent = child;

            }
            parent.Child = innerGrid;

            while (parent.Parent != null)
                parent = (Border)parent.Parent;

            _grid.Children.Add(parent);
            AddVisualChild(_grid);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            StopAnimations();
        }

        private void StopAnimations()
        {
            if (_clockGroup.Controller == null)
            {
                return;
            }

            _clockGroup.Controller.Stop();
            _clockGroup.Controller.Remove();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _grid.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return _grid;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            _grid.Measure(availableSize);
            return _grid.DesiredSize;
        }
        #endregion
    }
}