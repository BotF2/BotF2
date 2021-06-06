using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Linq;

namespace Supremacy.Client.Behaviors
{
    public partial class MultiSelectBehavior : Behavior<Selector>
    {
        private bool _isSelectionInProgress;
        private Selector _selector;
        private SelectionAdorner _selectionAdorner;
        private AdornerLayer _adornerLayer;
        private readonly Dictionary<object, UIElement> _newlySelectedItems = new Dictionary<object, UIElement>();

        protected override void OnAttached()
        {
            _selector = AssociatedObject;
            _selector.PreviewMouseDown += OnSelectorPreviewMouseDown;
            _selector.PreviewMouseUp += OnSelectorPreviewMouseUp;
            _selector.PreviewMouseMove += OnSelectorPreviewMouseMove;
            _selector.LostMouseCapture += OnSelectorLostMouseCapture;
            _selector.PreviewKeyDown += OnSelectorPreviewKeyDown;
        }

        private void OnSelectorLostMouseCapture(object sender, MouseEventArgs e)
        {
            if (!_isSelectionInProgress)
                return;
            EndSelection();
            e.Handled = true;
        }

        private void EndSelection()
        {
            _isSelectionInProgress = false;
            RemoveSelectionAdorner();
            _selector.ReleaseMouseCapture();
            _newlySelectedItems.Clear();
        }

        private void OnSelectorPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isSelectionInProgress)
                return;
            EndSelection();
            e.Handled = true;
        }

        private void OnSelectorPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isSelectionInProgress)
                return;

            Point endPoint = e.GetPosition(_selector);

            if (!_selectionAdorner.EndPoint.HasValue)
            {
                Vector dragDistance = endPoint - _selectionAdorner.StartPoint;
                if ((Math.Abs(dragDistance.X) < SystemParameters.MinimumHorizontalDragDistance) &&
                    (Math.Abs(dragDistance.Y) < SystemParameters.MinimumVerticalDragDistance))
                {
                    return;
                }
                _selector.CaptureMouse();
            }

            _selectionAdorner.EndPoint = endPoint;

            List<KeyValuePair<object, UIElement>> itemsInSelectionRectangle = FindItemsInSelectionRectangle().ToList();

            foreach (KeyValuePair<object, UIElement> valuePair in itemsInSelectionRectangle)
            {
                if (Selector.GetIsSelected(valuePair.Value))
                    continue;
                _newlySelectedItems[valuePair.Key] = valuePair.Value;
                AddSelection(valuePair.Key, valuePair.Value);
            }

            foreach (KeyValuePair<object, UIElement> unselectedItem in _newlySelectedItems.Except(itemsInSelectionRectangle).ToList())
            {
                RemoveSelection(unselectedItem.Key, unselectedItem.Value);
                _newlySelectedItems.Remove(unselectedItem.Key);
            }

            _adornerLayer.Update(_selector);

            e.Handled = true;
        }

        private IEnumerable<KeyValuePair<object, UIElement>> FindItemsInSelectionRectangle()
        {
            Rect rect = new Rect
            {
                X = Math.Min(
                               _selectionAdorner.StartPoint.X,
                               _selectionAdorner.EndPoint.HasValue
                                   ? _selectionAdorner.EndPoint.Value.X
                                   : 0),

                Y = Math.Min(
                               _selectionAdorner.StartPoint.Y,
                               _selectionAdorner.EndPoint.HasValue ? _selectionAdorner.EndPoint.Value.Y : 0),

                Width = Math.Abs(
                               _selectionAdorner.EndPoint.HasValue
                                   ? _selectionAdorner.EndPoint.Value.X - _selectionAdorner.StartPoint.X
                                   : 0),

                Height = Math.Abs(
                               _selectionAdorner.EndPoint.HasValue
                                   ? _selectionAdorner.EndPoint.Value.Y - _selectionAdorner.StartPoint.Y
                                   : 0)

            };

            return from object item in _selector.Items
                   let container = item as UIElement ??
                                   _selector.ItemContainerGenerator.ContainerFromItem(item) as UIElement
                   where container != null
                   let descendantBounds = VisualTreeHelper.GetDescendantBounds(container)
                   let transformToSelector = container.TransformToAncestor(_selector)
                   where transformToSelector.TransformBounds(descendantBounds).IntersectsWith(rect)
                   select new KeyValuePair<object, UIElement>(item, container);
        }

        private IEnumerable<UIElement> GetSelectedItemContainers()
        {
            return from object item in _selector.Items
                   select item as UIElement ??
                          _selector.ItemContainerGenerator.ContainerFromItem(item) as UIElement
                   into container
                   where container != null
                   where Selector.GetIsSelected(container)
                   select container;
        }

        private void OnSelectorPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject originalSource = e.OriginalSource as DependencyObject;
            if (originalSource == null)
                return;

            ScrollBar scrollBar = originalSource.FindVisualAncestorByType<ScrollBar>();
            if (scrollBar != null)
                return;

            IEnumerable<HitTestResult> hitTestResults = from object item in _selector.Items
                                                        select item as UIElement ?? _selector.ItemContainerGenerator.ContainerFromItem(item) as UIElement
                                 into container
                                                        where container != null
                                                        select VisualTreeHelper.HitTest(container, e.GetPosition(container));

            if (hitTestResults.Any(hitTestResult => (hitTestResult != null) && (hitTestResult.VisualHit != null)))
                return;

            _isSelectionInProgress = true;

            if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift)
                ClearSelection();

            _selector.Focus();

            CreateSelectionAdorner(e.GetPosition(_selector));

            e.Handled = true;
        }

        private static void AddSelection(object item, DependencyObject container)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            if (container == null)
                throw new ArgumentNullException("container");

            if (!Selector.GetIsSelected(container))
                Selector.SetIsSelected(container, true);
        }

        private void RemoveSelection(object item, DependencyObject container)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            if (container == null)
                throw new ArgumentNullException("container");

            Selector.SetIsSelected(container, false);

            if (_selector.SelectedItem == item)
                _selector.SelectedItem = null;
        }

        private void ClearSelection()
        {
            IEnumerable<DependencyObject> containers = from object item in _selector.Items
                                                       let container = item as UIElement ??
                                                                       _selector.ItemContainerGenerator.ContainerFromItem(item)
                                                       where container != null
                                                       select container;

            foreach (DependencyObject container in containers)
                Selector.SetIsSelected(container, false);
        }

        private void CreateSelectionAdorner(Point startPoint)
        {
            RemoveSelectionAdorner();

            if (_adornerLayer == null)
                _adornerLayer = AdornerLayer.GetAdornerLayer(_selector);

            _selectionAdorner = new SelectionAdorner(_selector, startPoint);
            _adornerLayer.Add(_selectionAdorner);
        }

        private void RemoveSelectionAdorner()
        {
            if (_adornerLayer == null)
                return;
            if (_selectionAdorner == null)
                return;

            _selectionAdorner.EndPoint = null;

            _adornerLayer.Remove(_selectionAdorner);

            _selectionAdorner = null;
            _adornerLayer = null;
        }

        protected override void OnDetaching()
        {
            if (_selector == null)
                return;

            _selector.PreviewMouseDown -= OnSelectorPreviewMouseDown;
            _selector.PreviewMouseUp -= OnSelectorPreviewMouseUp;
            _selector.PreviewMouseMove -= OnSelectorPreviewMouseMove;
            _selector.LostMouseCapture -= OnSelectorLostMouseCapture;
            _selector.PreviewKeyDown -= OnSelectorPreviewKeyDown;

            _selector = null;
        }

        private void OnSelectorPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key != Key.Escape) || !GetSelectedItemContainers().Any())
                return;

            ClearSelection();

            e.Handled = true;
            return;
        }
    }
}