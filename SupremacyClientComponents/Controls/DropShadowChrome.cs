using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Supremacy.Client.Controls
{
    [Flags]
    public enum Sides
    {
        None = 0x00,
        Left = 0x01,
        Top = 0x02,
        Right = 0x04,
        Bottom = 0x08,
        All = Left | Top | Right | Bottom
    }

    [Flags]
    public enum Corners
    {
        None = 0x00,
        TopLeft = 0x01,
        TopRight = 0x02,
        BottomRight = 0x04,
        BottomLeft = 0x08,
        Left = TopLeft | BottomLeft,
        Top = TopLeft | TopRight,
        Right = TopRight | BottomRight,
        Bottom = BottomLeft | BottomRight,
        All = TopLeft | TopRight | BottomLeft | BottomRight
    }

    public class DropShadowChrome : Decorator
    {
        #region Dependency Properties
        public static readonly DependencyProperty BorderThicknessProperty =
            DependencyProperty.Register(
                "BorderThickness",
                typeof(Thickness),
                typeof(DropShadowChrome),
                new FrameworkPropertyMetadata(new Thickness(5), 
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            "Color",
            typeof(Color),
            typeof(DropShadowChrome),
            new FrameworkPropertyMetadata(Color.FromArgb(0x71, 0, 0, 0),
                FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius",
            typeof(CornerRadius),
            typeof(DropShadowChrome),
            new FrameworkPropertyMetadata(new CornerRadius(0), 
                FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty XOffsetProperty = DependencyProperty.Register(
            "XOffset",
            typeof(double),
            typeof(DropShadowChrome),
            new FrameworkPropertyMetadata(5.0,
                FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty YOffsetProperty = DependencyProperty.Register(
            "YOffset",
            typeof(double),
            typeof(DropShadowChrome),
            new FrameworkPropertyMetadata(5.0,
                FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ZOffsetProperty = DependencyProperty.Register(
            "ZOffset",
            typeof(double),
            typeof(DropShadowChrome),
            new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        private GradientStopCollection CreateStops(double offsetPercentage)
        {
            var color = Color;
            var alpha = color.A;

            var gradientAmount = 1.0 - offsetPercentage;

            var stops = new GradientStopCollection
                        {
                            new GradientStop(
                                color, 
                                offsetPercentage + 0.0 * gradientAmount),
                            new GradientStop(
                                Color.FromArgb((byte)(0.74336 * alpha), color.R, color.G, color.B),
                                offsetPercentage + 0.2 * gradientAmount),
                            new GradientStop(
                                Color.FromArgb((byte)(0.38053 * alpha), color.R, color.G, color.B),
                                offsetPercentage + 0.2 * gradientAmount),
                            new GradientStop(
                                Color.FromArgb((byte)(0.12389 * alpha), color.R, color.G, color.B),
                                offsetPercentage + 0.2 * gradientAmount),
                            new GradientStop(
                                Color.FromArgb((byte)(0.00000 * alpha), color.R, color.G, color.B),
                                offsetPercentage + 0.2 * gradientAmount)
                        };

            stops.Freeze();

            return stops;
        }

        private void DrawLinearBorder(
            DrawingContext drawingContext,
            double left,
            double top,
            double width,
            double height,
            Sides side)
        {
            if ((width <= 0) || (height <= 0))
                return;

            var bounds = new Rect(left, top, width, height);

            Point startPoint;
            Point endPoint;

            switch (side)
            {
                case Sides.Left:
                    startPoint = new Point(1, 0);
                    endPoint = new Point(0, 0);
                    break;
                case Sides.Top:
                    startPoint = new Point(0, 1);
                    endPoint = new Point(0, 0);
                    break;
                case Sides.Right:
                    startPoint = new Point(0, 0);
                    endPoint = new Point(1, 0);
                    break;
                default:
                    startPoint = new Point(0, 0);
                    endPoint = new Point(0, 1);
                    break;
            }

            var brush = new LinearGradientBrush(CreateStops(0.0), startPoint, endPoint);
            brush.Freeze();

            drawingContext.DrawRectangle(brush, null, bounds);
        }

        private void DrawRadialBorder(
            DrawingContext drawingContext,
            double left,
            double top,
            double width,
            double height,
            Corners corner)
        {
            if ((width <= 0) || (height <= 0))
                return;

            var bounds = new Rect(left, top, width, height);
            var offsetPercentage = 0.0;

            switch (corner)
            {
                case Corners.TopLeft:
                    offsetPercentage = CornerRadius.TopLeft /
                                       (CornerRadius.TopLeft + BorderThickness.Top);
                    break;
                case Corners.TopRight:
                    offsetPercentage = CornerRadius.TopRight /
                                       (CornerRadius.TopRight + BorderThickness.Top);
                    break;
                case Corners.BottomRight:
                    offsetPercentage = CornerRadius.BottomRight /
                                       (CornerRadius.BottomRight + BorderThickness.Bottom);
                    break;
                case Corners.BottomLeft:
                    offsetPercentage = CornerRadius.BottomLeft /
                                       (CornerRadius.BottomLeft + BorderThickness.Bottom);
                    break;
            }

            var brush = new RadialGradientBrush(CreateStops(offsetPercentage));
            
            switch (corner)
            {
                case Corners.TopLeft:
                    brush.Center = new Point(1.0, 1.0);
                    break;
                case Corners.TopRight:
                    brush.Center = new Point(0.0, 1.0);
                    break;
                case Corners.BottomRight:
                    brush.Center = new Point(0.0, 0.0);
                    break;
                case Corners.BottomLeft:
                    brush.Center = new Point(1.0, 0.0);
                    break;
            }

            brush.GradientOrigin = brush.Center;
            brush.RadiusX = 1.0;
            brush.RadiusY = 1.0;
            brush.Freeze();

            drawingContext.DrawRectangle(brush, null, bounds);
        }

        public Thickness BorderThickness
        {
            get { return (Thickness)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (Color.A == 0.0)
                return;

            var bounds = new Rect(new Point(XOffset, YOffset), RenderSize);

            bounds.Inflate(ZOffset, ZOffset);

            if ((bounds.Width <= 0) || (bounds.Height <= 0))
                return;

            var innerBounds = new Rect(
                bounds.Left + BorderThickness.Left,
                bounds.Top + BorderThickness.Top,
                Math.Max(0, bounds.Width - BorderThickness.Left - BorderThickness.Right),
                Math.Max(0, bounds.Height - BorderThickness.Top - BorderThickness.Bottom));

            var guidelinesX = new[] { bounds.Left, innerBounds.Left, innerBounds.Right, bounds.Right };
            var guidelinesY = new[] { bounds.Top, innerBounds.Top, innerBounds.Bottom, bounds.Bottom };

            drawingContext.PushGuidelineSet(new GuidelineSet(guidelinesX, guidelinesY));

            if ((innerBounds.Width > 0) && (innerBounds.Height > 0))
            {
                var brush = new SolidColorBrush(Color);
                brush.Freeze();

                var figure = new PathFigure
                             {
                                 StartPoint = new Point(innerBounds.Left + CornerRadius.TopLeft, innerBounds.Top)
                             };
                figure.Segments.Add(
                    new LineSegment(
                        new Point(
                            innerBounds.Right - CornerRadius.TopRight,
                            innerBounds.Top),
                        true));
                figure.Segments.Add(
                    new LineSegment(
                        new Point(
                            innerBounds.Right - CornerRadius.TopRight,
                            innerBounds.Top + CornerRadius.TopRight),
                        true));
                figure.Segments.Add(
                    new LineSegment(
                        new Point(
                            innerBounds.Right,
                            innerBounds.Top + CornerRadius.TopRight),
                        true));
                figure.Segments.Add(
                    new LineSegment(
                        new Point(
                            innerBounds.Right,
                            innerBounds.Bottom - CornerRadius.BottomRight),
                        true));
                figure.Segments.Add(
                    new LineSegment(
                        new Point(
                            innerBounds.Right - CornerRadius.BottomRight,
                            innerBounds.Bottom - CornerRadius.BottomRight),
                        true));
                figure.Segments.Add(
                    new LineSegment(
                        new Point(
                            innerBounds.Right - CornerRadius.BottomRight,
                            innerBounds.Bottom),
                        true));
                figure.Segments.Add(
                    new LineSegment(
                        new Point(
                            innerBounds.Left + CornerRadius.TopLeft,
                            innerBounds.Bottom),
                        true));
                figure.Segments.Add(
                    new LineSegment(
                        new Point(
                            innerBounds.Left + CornerRadius.TopLeft,
                            innerBounds.Bottom - CornerRadius.BottomLeft),
                        true));
                figure.Segments.Add(
                    new LineSegment(
                        new Point(
                            innerBounds.Left,
                            innerBounds.Bottom - CornerRadius.BottomLeft),
                        true));
                figure.Segments.Add(
                    new LineSegment(
                        new Point(
                            innerBounds.Left,
                            innerBounds.Top + CornerRadius.TopLeft),
                        true));
                figure.Segments.Add(
                    new LineSegment(
                        new Point(
                            innerBounds.Left + CornerRadius.TopLeft,
                            innerBounds.Top + CornerRadius.TopLeft),
                        true));
                figure.IsClosed = true;

                var path = new PathGeometry();
                path.Figures.Add(figure);
                path.Freeze();
                drawingContext.DrawGeometry(brush, null, path);
            }

            DrawLinearBorder(
                drawingContext,
                bounds.Left,
                innerBounds.Top + CornerRadius.TopLeft,
                BorderThickness.Left,
                innerBounds.Height - CornerRadius.TopLeft - CornerRadius.BottomLeft,
                Sides.Left);
            DrawLinearBorder(
                drawingContext,
                innerBounds.Left + CornerRadius.TopLeft,
                bounds.Top,
                innerBounds.Width - CornerRadius.TopLeft - CornerRadius.TopRight,
                BorderThickness.Top,
                Sides.Top);
            DrawLinearBorder(
                drawingContext,
                innerBounds.Right,
                innerBounds.Top + CornerRadius.TopRight,
                BorderThickness.Right,
                innerBounds.Height - CornerRadius.TopRight - CornerRadius.BottomRight,
                Sides.Right);
            DrawLinearBorder(
                drawingContext,
                innerBounds.Left + CornerRadius.BottomLeft,
                innerBounds.Bottom,
                innerBounds.Width - CornerRadius.BottomLeft - CornerRadius.BottomRight,
                BorderThickness.Bottom,
                Sides.Bottom);

            DrawRadialBorder(
                drawingContext,
                bounds.Left,
                bounds.Top,
                BorderThickness.Left + CornerRadius.TopLeft,
                BorderThickness.Top + CornerRadius.TopLeft,
                Corners.TopLeft);
            DrawRadialBorder(
                drawingContext,
                innerBounds.Right - CornerRadius.TopRight,
                bounds.Top,
                BorderThickness.Right + CornerRadius.TopRight,
                BorderThickness.Top + CornerRadius.TopRight,
                Corners.TopRight);
            DrawRadialBorder(
                drawingContext,
                innerBounds.Right - CornerRadius.BottomRight,
                innerBounds.Bottom - CornerRadius.BottomRight,
                BorderThickness.Right + CornerRadius.BottomRight,
                BorderThickness.Bottom + CornerRadius.BottomRight,
                Corners.BottomRight);
            DrawRadialBorder(
                drawingContext,
                bounds.Left,
                innerBounds.Bottom - CornerRadius.BottomLeft,
                BorderThickness.Left + CornerRadius.BottomLeft,
                BorderThickness.Bottom + CornerRadius.BottomLeft,
                Corners.BottomLeft);

            drawingContext.Pop();
        }

        public double XOffset
        {
            get { return (double)GetValue(XOffsetProperty); }
            set { SetValue(XOffsetProperty, value); }
        }

        public double YOffset
        {
            get { return (double)GetValue(YOffsetProperty); }
            set { SetValue(YOffsetProperty, value); }
        }

        public double ZOffset
        {
            get { return (double)GetValue(ZOffsetProperty); }
            set { SetValue(ZOffsetProperty, value); }
        }
    }
}