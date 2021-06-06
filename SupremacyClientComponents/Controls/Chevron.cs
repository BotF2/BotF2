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
            double halfHeight = ActualHeight / 2;

            Point topLeft = new Point(0, 0);
            Point middleLeft = new Point(halfHeight, halfHeight);
            Point bottomLeft = new Point(0, halfHeight * 2);

            Point bottomRight = new Point(IsOutsetOnRight ? ActualWidth - halfHeight : ActualWidth, halfHeight * 2);
            Point middleRight = new Point(ActualWidth, halfHeight);
            Point topRight = new Point(IsOutsetOnRight ? ActualWidth - halfHeight : ActualWidth, 0);

            StreamGeometry geometry = new StreamGeometry();

            using (StreamGeometryContext c = geometry.Open())
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