using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Supremacy.Client.Behaviors
{
    public partial class MultiSelectBehavior
    {
        internal class SelectionAdorner : Adorner
        {
            private Point? _endPoint;
            private readonly Pen _strokePen;
            private readonly SolidColorBrush _fillBrush;

            public SelectionAdorner(UIElement adornedElement, Point startPoint)
                : base(adornedElement)
            {
                StartPoint = startPoint;
                Color fill = Colors.DodgerBlue;
                fill.A = 31;
                _fillBrush = new SolidColorBrush(fill);
                _fillBrush.Freeze();
                _strokePen = new Pen(Brushes.DodgerBlue, 1.0);
                _strokePen.Freeze();
            }

            public Point StartPoint { get; set; }

            public Point? EndPoint
            {
                get => _endPoint;
                set
                {
                    _endPoint = value;
                    InvalidateVisual();
                }
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                if (!EndPoint.HasValue)
                {
                    return;
                }

                Rect rect = new Rect
                {
                    X = StartPoint.X + Math.Min(0, EndPoint.Value.X - StartPoint.X),
                    Y = StartPoint.Y + Math.Min(0, EndPoint.Value.Y - StartPoint.Y),
                    Width = Math.Abs(EndPoint.Value.X - StartPoint.X),
                    Height = Math.Abs(EndPoint.Value.Y - StartPoint.Y)
                };

                drawingContext.PushGuidelineSet(
                    new GuidelineSet(
                        new[] { Math.Round(rect.X) + 0.5, Math.Round(rect.X + rect.Width) + 0.5 },
                        new[] { Math.Round(rect.Y) + 0.5, Math.Round(rect.Y + rect.Height) + 0.5 }));

                drawingContext.DrawRectangle(
                    _fillBrush,
                    _strokePen,
                    rect);

                drawingContext.Pop();
            }
        }
    }
}