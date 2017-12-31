using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Supremacy.Client.Controls
{
    public class PopupContentPresenter : ContentPresenter
    {
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static PopupContentPresenter()
        {
            Control.IsTabStopProperty.OverrideMetadata(
                typeof(PopupContentPresenter),
                new FrameworkPropertyMetadata(false));
            
            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(
                typeof(PopupContentPresenter),
                new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
            
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(
                typeof(PopupContentPresenter),
                new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
         
            FocusableProperty.OverrideMetadata(
                typeof(PopupContentPresenter),
                new FrameworkPropertyMetadata(true));
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var scrollViewer = this.FindVisualAncestorByType<PopupScrollViewer>();
            if (scrollViewer != null)
                constraint.Width = scrollViewer.MeasureConstraint.Width;
            return base.MeasureOverride(constraint);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            ScrollViewerItemsPresenter.ProcessKeyDown(this, e);
            base.OnKeyDown(e);
        }
    }
}