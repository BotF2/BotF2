using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Supremacy.Client.Controls
{
    public class GamePopupContentPresenter : ContentPresenter
    {
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static GamePopupContentPresenter()
        {
            Control.IsTabStopProperty.OverrideMetadata(
                typeof(GamePopupContentPresenter),
                new FrameworkPropertyMetadata(false));

            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(
                typeof(GamePopupContentPresenter),
                new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));

            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(
                typeof(GamePopupContentPresenter),
                new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));

            FocusableProperty.OverrideMetadata(
                typeof(GamePopupContentPresenter),
                new FrameworkPropertyMetadata(true));
        }

        protected override Size MeasureOverride(Size constraint)
        {
            GamePopupScrollViewer scrollViewer = this.FindVisualAncestorByType<GamePopupScrollViewer>();
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