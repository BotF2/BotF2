// -------------------------------------------------------------------------------
// 
// This file is part of the FluidKit project: http://www.codeplex.com/fluidkit
// 
// Copyright (c) 2008, The FluidKit community 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
// are permitted provided that the following conditions are met:
// 
// * Redistributions of source code must retain the above copyright notice, this 
// list of conditions and the following disclaimer.
// 
// * Redistributions in binary form must reproduce the above copyright notice, this 
// list of conditions and the following disclaimer in the documentation and/or 
// other materials provided with the distribution.
// 
// * Neither the name of FluidKit nor the names of its contributors may be used to 
// endorse or promote products derived from this software without specific prior 
// written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR 
// ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON 
// ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS 
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
// -------------------------------------------------------------------------------
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;

namespace Supremacy.Client.DragDrop
{
    public static class DragDropManager
    {
        private const string DragOffsetFormat = "DnD.DragOffset";

        public static readonly DependencyProperty DragSourceAdvisorProperty =
            DependencyProperty.RegisterAttached(
                "DragSourceAdvisor",
                typeof(IDragSourceAdvisor),
                typeof(DragDropManager),
                new FrameworkPropertyMetadata(
                    new PropertyChangedCallback(OnDragSourceAdvisorChanged)));

        public static readonly DependencyProperty DropTargetAdvisorProperty =
            DependencyProperty.RegisterAttached(
                "DropTargetAdvisor",
                typeof(IDropTargetAdvisor),
                typeof(DragDropManager),
                new FrameworkPropertyMetadata(
                    new PropertyChangedCallback(OnDropTargetAdvisorChanged)));

        private static Point _adornerPosition;

        private static UIElement _draggedElement;
        private static Point _dragStartPoint;
        private static bool _isMouseDown;
        private static Point _offsetPoint;
        private static DropPreviewAdorner _overlayElement;

        private static IDragSourceAdvisor CurrentDragSourceAdvisor { get; set; }
        private static IDropTargetAdvisor CurrentDropTargetAdvisor { get; set; }

        #region Dependency Properties Getter/Setters

        public static void SetDragSourceAdvisor(DependencyObject d, IDragSourceAdvisor advisor)
        {
            d.SetValue(DragSourceAdvisorProperty, advisor);
        }

        public static void SetDropTargetAdvisor(DependencyObject d, IDropTargetAdvisor advisor)
        {
            d.SetValue(DropTargetAdvisorProperty, advisor);
        }

        public static IDragSourceAdvisor GetDragSourceAdvisor(DependencyObject d)
        {
            return d.GetValue(DragSourceAdvisorProperty) as IDragSourceAdvisor;
        }

        public static IDropTargetAdvisor GetDropTargetAdvisor(DependencyObject d)
        {
            return d.GetValue(DropTargetAdvisorProperty) as IDropTargetAdvisor;
        }

        #endregion

        #region Property Change handlers

        private static void OnDragSourceAdvisorChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (!(d is UIElement sourceElement))
            {
                return;
            }

            if (e.NewValue != null && e.OldValue == null)
            {
                sourceElement.PreviewMouseLeftButtonDown += OnDragSourcePreviewMouseLeftButtonDown;
                sourceElement.PreviewMouseMove += OnDragSourcePreviewMouseMove;
                sourceElement.PreviewMouseUp += OnDragSourcePreviewMouseUp;

                // Set the Drag source UI
                if (e.NewValue is IDragSourceAdvisor advisor)
                {
                    advisor.SourceElement = sourceElement;
                }
            }
            else if (e.NewValue == null && e.OldValue != null)
            {
                sourceElement.PreviewMouseLeftButtonDown -= OnDragSourcePreviewMouseLeftButtonDown;
                sourceElement.PreviewMouseMove -= OnDragSourcePreviewMouseMove;
                sourceElement.PreviewMouseUp -= OnDragSourcePreviewMouseUp;
            }
        }

        private static void OnDropTargetAdvisorChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (!(d is UIElement targetElement))
            {
                return;
            }

            if (e.NewValue != null && e.OldValue == null)
            {
                targetElement.PreviewDragEnter += OnDropTargetPreviewDragEnter;
                targetElement.PreviewDragOver += OnDropTargetPreviewDragOver;
                targetElement.PreviewDragLeave += OnDropTargetPreviewDragLeave;
                targetElement.PreviewDrop += OnDropTargetPreviewDrop;
                targetElement.AllowDrop = true;

                // Set the Drag source UI
                if (e.NewValue is IDropTargetAdvisor advisor)
                {
                    advisor.TargetElement = targetElement;
                }
            }
            else if (e.NewValue == null && e.OldValue != null)
            {
                targetElement.PreviewDragEnter -= OnDropTargetPreviewDragEnter;
                targetElement.PreviewDragOver -= OnDropTargetPreviewDragOver;
                targetElement.PreviewDragLeave -= OnDropTargetPreviewDragLeave;
                targetElement.PreviewDrop -= OnDropTargetPreviewDrop;
                targetElement.AllowDrop = false;
            }
        }

        #endregion

        private static void OnDropTargetPreviewDrop(object sender, DragEventArgs e)
        {
            UpdateEffects(e);

            Point dropPoint = e.GetPosition(sender as UIElement);

            // Calculate displacement for (Left, Top)
            Point offset = e.GetPosition(_overlayElement);
            dropPoint.X -= offset.X;
            dropPoint.Y -= offset.Y;

            RemovePreviewAdorner();
            _offsetPoint = new Point(0, 0);

            if (CurrentDropTargetAdvisor.IsValidDataObject(e.Data))
            {
                CurrentDropTargetAdvisor.OnDropCompleted(e.Data, dropPoint);
            }

            e.Handled = true;
        }

        private static void OnDropTargetPreviewDragLeave(object sender, DragEventArgs e)
        {
            UpdateEffects(e);

            RemovePreviewAdorner();
            e.Handled = true;
        }

        private static void OnDropTargetPreviewDragOver(object sender, DragEventArgs e)
        {
            UpdateEffects(e);

            // Update position of the preview Adorner
            _adornerPosition = e.GetPosition(sender as UIElement);
            PositionAdorner();

            e.Handled = true;
        }

        private static void OnDropTargetPreviewDragEnter(object sender, DragEventArgs e)
        {
            // Get the current drop target advisor
            CurrentDropTargetAdvisor = GetDropTargetAdvisor(sender as DependencyObject);

            UpdateEffects(e);

            // Setup the preview Adorner
            _offsetPoint = new Point();
            if (CurrentDropTargetAdvisor.ApplyMouseOffset && e.Data.GetData(DragOffsetFormat) != null)
            {
                _offsetPoint = (Point)e.Data.GetData(DragOffsetFormat);
            }

            CreatePreviewAdorner(sender as UIElement, e.Data);

            e.Handled = true;
        }

        private static void UpdateEffects(DragEventArgs e)
        {
            if (CurrentDropTargetAdvisor.IsValidDataObject(e.Data) == false)
            {
                e.Effects = DragDropEffects.None;
            }

            else if ((e.AllowedEffects & DragDropEffects.Move) == 0 &&
                     (e.AllowedEffects & DragDropEffects.Copy) == 0)
            {
                e.Effects = DragDropEffects.None;
            }

            else if ((e.AllowedEffects & DragDropEffects.Move) != 0 &&
                     (e.AllowedEffects & DragDropEffects.Copy) != 0)
            {
                e.Effects = ((e.KeyStates & DragDropKeyStates.ControlKey) != 0)
                                ? DragDropEffects.Copy
                                : DragDropEffects.Move;
            }
        }

        private static void OnDragSourcePreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }

            ScrollBar scrollBar = ((DependencyObject)e.OriginalSource).FindVisualAncestorByType<ScrollBar>();
            if (scrollBar != null)
            {
                return;
            }

            DependencyObject source = ((DependencyObject)e.OriginalSource).FindVisualAncestor(o => GetDragSourceAdvisor(o) != null);
            if (source == null)
            {
                return;
            }

            // Make this the new drag source
            CurrentDragSourceAdvisor = GetDragSourceAdvisor(source);

            if (CurrentDragSourceAdvisor.IsDraggable(source as UIElement) == false)
            {
                return;
            }

            _draggedElement = source as UIElement;
            _dragStartPoint = e.GetPosition(CurrentDragSourceAdvisor.GetTopContainer());
            _offsetPoint = e.GetPosition(_draggedElement);
            _isMouseDown = true;
        }

        private static void OnDragSourcePreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDown && IsDragGesture(e.GetPosition(CurrentDragSourceAdvisor.GetTopContainer())))
            {
                DragStarted(sender as UIElement);
            }
        }

        private static void OnDragSourcePreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = false;
            _ = Mouse.Capture(null);
        }

        private static void DragStarted(UIElement uiElt)
        {
            _isMouseDown = false;
            _ = Mouse.Capture(uiElt);

            DataObject data = CurrentDragSourceAdvisor.GetDataObject(_draggedElement);

            data.SetData(DragOffsetFormat, _offsetPoint);
            DragDropEffects supportedEffects = CurrentDragSourceAdvisor.SupportedEffects;

            // Perform DragDrop

            DragDropEffects effects = System.Windows.DragDrop.DoDragDrop(_draggedElement, data, supportedEffects);
            CurrentDragSourceAdvisor.FinishDrag(_draggedElement, effects);

            // Clean up
            RemovePreviewAdorner();
            _ = Mouse.Capture(null);
            _draggedElement = null;
        }

        private static bool IsDragGesture(Point point)
        {
            bool hGesture = Math.Abs(point.X - _dragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance;
            bool vGesture = Math.Abs(point.Y - _dragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance;

            return hGesture | vGesture;
        }

        private static void CreatePreviewAdorner(UIElement adornedElt, IDataObject data)
        {
            if (_overlayElement != null)
            {
                return;
            }

            AdornerLayer layer = AdornerLayer.GetAdornerLayer(CurrentDropTargetAdvisor.GetTopContainer());
            UIElement feedbackElement = CurrentDropTargetAdvisor.GetVisualFeedback(data);

            _overlayElement = new DropPreviewAdorner(feedbackElement, adornedElt);

            PositionAdorner();

            layer.Add(_overlayElement);
        }

        private static void PositionAdorner()
        {
            _overlayElement.Left = _adornerPosition.X - _offsetPoint.X;
            _overlayElement.Top = _adornerPosition.Y - _offsetPoint.Y;
        }

        private static void RemovePreviewAdorner()
        {
            if (_overlayElement == null)
            {
                return;
            }

            AdornerLayer.GetAdornerLayer(CurrentDropTargetAdvisor.GetTopContainer()).Remove(_overlayElement);
            _overlayElement = null;
        }
    }
}