using System.Windows;
using System.Windows.Controls;

namespace Supremacy.Client.Controls
{
    public class PopupScrollViewer : ScrollViewer
    {
        private Size _measureConstraint;

        static PopupScrollViewer()
        {
            FocusableProperty.OverrideMetadata(typeof(PopupScrollViewer), new FrameworkPropertyMetadata(false));
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