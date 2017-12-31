using System.Windows;
using System.Windows.Controls;

using Supremacy.Client.Data;

namespace Supremacy.Client.Controls
{
    public enum RoundMode
    {
        None,
        Floor,
        FloorToEven,
        FloorToOdd,
        Ceiling,
        CeilingToEven,
        CeilingToOdd,
        Round,
        RoundToEven,
        RoundToOdd,
    }

    public class PixelSnapper : Decorator
    {
        #region Dependency Properties
        public static readonly DependencyProperty HorizontalRoundModeProperty =
            DependencyProperty.Register(
                "HorizontalRoundMode",
                typeof(RoundMode),
                typeof(PixelSnapper),
                new FrameworkPropertyMetadata(RoundMode.Ceiling, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty VerticalRoundModeProperty =
            DependencyProperty.Register(
                "VerticalRoundMode",
                typeof(RoundMode),
                typeof(PixelSnapper),
                new FrameworkPropertyMetadata(RoundMode.Ceiling, FrameworkPropertyMetadataOptions.AffectsMeasure));
        #endregion

        public RoundMode HorizontalRoundMode
        {
            get { return (RoundMode)GetValue(HorizontalRoundModeProperty); }
            set { SetValue(HorizontalRoundModeProperty, value); }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var child = Child;
            if (child == null)
                return new Size();

            child.Measure(constraint);

            return new Size(
                MathHelper.Round(HorizontalRoundMode, child.DesiredSize.Width),
                MathHelper.Round(VerticalRoundMode, child.DesiredSize.Height));
        }

        public RoundMode VerticalRoundMode
        {
            get { return (RoundMode)GetValue(VerticalRoundModeProperty); }
            set { SetValue(VerticalRoundModeProperty, value); }
        }
    }
}