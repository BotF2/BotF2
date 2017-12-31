using System.Windows;
using System.Windows.Media;

namespace Supremacy.Client.Controls
{
    public class Chevron : FrameworkElement
    {
        #region Background Property
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            "Background",
            typeof(Brush),
            typeof(Chevron),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }
        #endregion

        #region IsInsetOnLeft Property
        public static readonly DependencyProperty IsInsetOnLeftProperty = DependencyProperty.Register(
            "IsInsetOnLeft",
            typeof(bool),
            typeof(Chevron),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.AffectsRender));

        public bool IsInsetOnLeft
        {
            get { return (bool)GetValue(IsInsetOnLeftProperty); }
            set { SetValue(IsInsetOnLeftProperty, value); }
        }
        #endregion

        #region IsOutsetOnRight Property
        public static readonly DependencyProperty IsOutsetOnRightProperty = DependencyProperty.Register(
            "IsOutsetOnRight",
            typeof(bool),
            typeof(Chevron),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.AffectsRender));

        public bool IsOutsetOnRight
        {
            get { return (bool)GetValue(IsOutsetOnRightProperty); }
            set { SetValue(IsOutsetOnRightProperty, value); }
        }
        #endregion

        protected override void OnRender(DrawingContext drawingContext)
        {
            var halfHeight = ActualHeight / 2;

            var topLeft = new Point(0, 0);
            var middleLeft = new Point(halfHeight, halfHeight);
            var bottomLeft = new Point(0, halfHeight * 2);

            var bottomRight = new Point(IsOutsetOnRight ? ActualWidth - halfHeight : ActualWidth, halfHeight * 2);
            var middleRight = new Point(ActualWidth, halfHeight);
            var topRight = new Point(IsOutsetOnRight ? ActualWidth - halfHeight : ActualWidth, 0);

            var geometry = new StreamGeometry();

            using (var c = geometry.Open())
            {
                c.BeginFigure(topLeft, true, true);

                if (IsInsetOnLeft)
                    c.LineTo(middleLeft, false, true);

                c.LineTo(bottomLeft, false, true);
                c.LineTo(bottomRight, false, true);

                if (IsOutsetOnRight)
                    c.LineTo(middleRight, false, true);

                c.LineTo(topRight, false, true);
                c.LineTo(topLeft, false, true);
            }

            drawingContext.DrawGeometry(
                Background,
                null,
                geometry);
        }
    }
}