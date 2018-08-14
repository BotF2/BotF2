using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Supremacy.Client;
using Supremacy.Resources;
using Supremacy.Utility;
using System.IO;
using Supremacy.Client.Dialogs;
using log4net;

namespace Supremacy.UI
{

    public sealed class Animation : FrameworkElement, IAnimationsHost
    {
        #region Constants
        private const string ImagePath = "Resources/";
        #endregion

        #region Fields
        private static readonly DependencyProperty CurrentFrameProperty;
        private static readonly CachedBitmap[] Frames;
        private static readonly ILog _log = GameLog.Debug.GameData;

        private readonly Int32Animation _animation;
        private readonly AnimationClock _animationClock;
        private readonly Rectangle _rectangle;
        private readonly ImageBrush _imageBrush;
        #endregion

        #region Constructors

        private static Collections.CollectionBase<string> GetStarNames()
        {
            var result = MessageDialog.Show("header", "Hello", MessageDialogButtons.Ok);
        var file = new FileStream(
             ResourceManager.GetResourcePath("Resources/Images/Animation1.txt"),
             FileMode.Open,
             FileAccess.Read);
        var result3 = MessageDialog.Show("header", "Hello", MessageDialogButtons.Ok);

                    var reader = new StreamReader(file);
            var names = new Collections.CollectionBase<string>();

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var result4 = MessageDialog.Show("header", "Hello", MessageDialogButtons.Ok);

                GameLog.Client.General.Error(string.Format(@"Error loading saved game '{0}'.", file));

                if (line == null)
                    break;

                names.Add(line.Trim());
                var result2 = MessageDialog.Show("header", "Hello", MessageDialogButtons.Ok);

                //Log.DebugFormat(line);
                //empireCivs[index].ShortName,
                //location);
            }

            return names;
        }

        static Animation()
        {
            FocusVisualStyleProperty.OverrideMetadata(
                typeof(Animation),
                new FrameworkPropertyMetadata(
                    null,
                    (d, baseValue) => null));
            WidthProperty.OverrideMetadata(
                typeof(Animation),
                new FrameworkPropertyMetadata(
                    96.0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure));
            HeightProperty.OverrideMetadata(
                typeof(Animation),
                new FrameworkPropertyMetadata(
                    96.0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure));
            CurrentFrameProperty = DependencyProperty.Register(
                "CurrentFrame",
                typeof(int),
                typeof(Animation),
                new FrameworkPropertyMetadata(
                    0,
                    FrameworkPropertyMetadataOptions.None));
            Frames = new CachedBitmap[255];

            //imageUri = ResourceManager.GetResourceUri(@"Resources\Images\Insignias\" + names(i) + ".png");
            for (int i = 0; i < Frames.Length; i++)
            {
                var filename = ImagePath + String.Format("Images/Animation/animation1_{0:000}.png", i);
                //var result4 = MessageDialog.Show("header", filename, MessageDialogButtons.Ok);
                if (File.Exists(filename))
                {
                    Frames[i] = new CachedBitmap(
                        new BitmapImage(
                            ResourceManager.GetResourceUri(ImagePath + String.Format("Images/Animation/animation1_{0:000}.png", i))),
                        BitmapCreateOptions.None,
                        BitmapCacheOption.OnLoad);
                    //var result3 = MessageDialog.Show("header", filename, MessageDialogButtons.Ok);
                    //RenderOptions.SetBitmapScalingMode(Frames[i], BitmapScalingMode.LowQuality);
                    Frames[i].Freeze();
                }
            }
        }

        public Animation()
        {
            /* Start on a random frame to provide some variation */
            CurrentFrame = RandomHelper.Random(Frames.Length);

            _imageBrush = new ImageBrush { ImageSource = Frames[CurrentFrame % 255] };
            _rectangle = new Rectangle
            {
                ToolTip = "some pictures out of the game",
                Fill = _imageBrush
            };

            AddVisualChild(_rectangle);

            _animation = new Int32Animation(
                CurrentFrame,
                CurrentFrame + 255,
                new Duration(new TimeSpan(0, 9, 30)))
                    {
                        RepeatBehavior = RepeatBehavior.Forever
                    };
            //Timeline.SetDesiredFrameRate(_animation, 24);

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

            //BeginAnimation(CurrentFrameProperty, null);
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
            StartAnimations();
        }

        private void StartAnimations()
        {
            if (!HasAnimatedProperties)
                ApplyAnimationClock(CurrentFrameProperty, _animationClock);

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

