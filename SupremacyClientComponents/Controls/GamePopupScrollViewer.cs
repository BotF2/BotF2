using System.Windows;
using System.Windows.Controls;

namespace Supremacy.Client.Controls
{
    public class GamePopupScrollViewer : ScrollViewer
    {
        private Size _measureConstraint;

        static GamePopupScrollViewer()
        {
            FocusableProperty.OverrideMetadata(typeof(GamePopupScrollViewer), new FrameworkPropertyMetadata(false));
        }

        internal Size MeasureConstraint
        {
            get { return _measureConstraint; }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _measureConstraint = constraint;
            return base.MeasureOverride(constraint);
        }
    }
}