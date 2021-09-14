// BlockBar.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Supremacy.UI
{
    public class BlockBar : RangeBase
    {
        public static readonly RoutedCommand DecrementCommand;
        public static readonly RoutedCommand IncrementCommand;

        public static readonly DependencyProperty BlockBrushProperty;
        public static readonly DependencyProperty BlockCountProperty;
        public static readonly DependencyProperty BlockMarginProperty;
        public static readonly DependencyProperty IsReadOnlyProperty;
        public static readonly DependencyProperty PrecisionProperty;

        static BlockBar()
        {
            DecrementCommand = new RoutedCommand("Decrement", typeof(BlockBar));
            IncrementCommand = new RoutedCommand("Increment", typeof(BlockBar));

            BlockCountProperty = DependencyProperty.Register(
                "BlockCount",
                typeof(int),
                typeof(BlockBar),
                new FrameworkPropertyMetadata(
                    5,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    null,
                    CoerceBlockCount));

            BlockMarginProperty = DependencyProperty.Register(
                "BlockMargin",
                typeof(double),
                typeof(BlockBar),
                new FrameworkPropertyMetadata(
                    (double)0,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    null,
                    CoerceBlockMargin));

            BlockBrushProperty = DependencyProperty.Register(
                "BlockBrush",
                typeof(Brush),
                typeof(BlockBar),
                new FrameworkPropertyMetadata(
                    Brushes.Yellow,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    null,
                    CoerceBlockBrush));

            FocusVisualStyleProperty.OverrideMetadata(
                typeof(BlockBar),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.None,
                    null,
                    (d, value) => null));

            ClipToBoundsProperty.OverrideMetadata(
                typeof(BlockBar),
                new FrameworkPropertyMetadata(true));

            SnapsToDevicePixelsProperty.OverrideMetadata(
                typeof(BlockBar),
                new FrameworkPropertyMetadata(true));

            IsReadOnlyProperty = DependencyProperty.Register(
                "IsReadOnly",
                typeof(bool),
                typeof(BlockBar),
                new FrameworkPropertyMetadata(false));

            PrecisionProperty = DependencyProperty.Register(
                "Precision",
                typeof(int),
                typeof(BlockBar),
                new FrameworkPropertyMetadata(
                    2,
                    null,
                    (d, baseValue) => ((int)baseValue < 0) ? 0 : (int)baseValue));

            SmallChangeProperty.OverrideMetadata(
                typeof(BlockBar),
                new FrameworkPropertyMetadata(0.01));

            ValueProperty.OverrideMetadata(
                typeof(BlockBar),
                new FrameworkPropertyMetadata(
                    0.0,
                    FrameworkPropertyMetadataOptions.AffectsRender));
        }

        public BlockBar()
        {
            _ = CommandBindings.Add(new CommandBinding(IncrementCommand, IncrementCommand_Executed));
            _ = CommandBindings.Add(new CommandBinding(DecrementCommand, DecrementCommand_Executed));

            _ = InputBindings.Add(new KeyBinding(DecrementCommand, Key.Left, ModifierKeys.None));
            _ = InputBindings.Add(new KeyBinding(DecrementCommand, Key.Down, ModifierKeys.None));
            _ = InputBindings.Add(new KeyBinding(IncrementCommand, Key.Right, ModifierKeys.None));
            _ = InputBindings.Add(new KeyBinding(IncrementCommand, Key.Up, ModifierKeys.None));
        }

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public int BlockCount
        {
            get => (int)GetValue(BlockCountProperty);
            set => SetValue(BlockCountProperty, value);
        }

        public double BlockMargin
        {
            get => (double)GetValue(BlockMarginProperty);
            set => SetValue(BlockMarginProperty, value);
        }

        public Brush BlockBrush
        {
            get => GetValue(BlockBrushProperty) as Brush;
            set => SetValue(BlockBrushProperty, value);
        }

        public int Precision
        {
            get => (int)GetValue(PrecisionProperty);
            set => SetValue(PrecisionProperty, value);
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (CaptureMouse())
            {
                HandleMouseInput(e.GetPosition(this));
            }
            else
            {
                base.OnPreviewMouseLeftButtonDown(e);
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (IsMouseCaptured)
            {
                HandleMouseInput(e.GetPosition(this));
            }
            else
            {
                OnMouseMove(e);
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                HandleMouseInput(e.GetPosition(this));
                ReleaseMouseCapture();
            }
            else
            {
                base.OnMouseLeftButtonUp(e);
            }
        }

        private void HandleMouseInput(Point position)
        {
            if (IsReadOnly)
            {
                return;
            }

            Value = position.X < 0
                ? Minimum
                : position.X > RenderSize.Width
                    ? Maximum
                    : Math.Round(Minimum + (position.X / RenderSize.Width * (Maximum - Minimum)), Precision);

            if (!BindingOperations.IsDataBound(this, ValueProperty))
            {
                return;
            }

            BindingExpression bindingExpression = BindingOperations.GetBindingExpression(this, ValueProperty);
            if (bindingExpression != null)
            {
                bindingExpression.UpdateTarget();
            }
        }

        private void IncrementCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (IsReadOnly)
            {
                return;
            }

            Value = Math.Max(Minimum, Math.Min(Maximum, Value + SmallChange));
        }

        private void DecrementCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (IsReadOnly)
            {
                return;
            }

            Value = Math.Max(Minimum, Math.Min(Maximum, Value - SmallChange));
        }

        private static object CoerceBlockBrush(DependencyObject element, object value)
        {
            return value ?? Brushes.Transparent;
        }

        protected int GetThreshold(double value, int blockCount)
        {
            if (value < Minimum || value > Maximum)
            {
                throw new ArgumentOutOfRangeException("value");
            }

            if (blockCount < 1)
            {
                throw new ArgumentOutOfRangeException("blockCount");
            }

            int blockNumber = (int)Math.Min((value - Minimum) / (Maximum - Minimum) * blockCount, blockCount);

            return blockNumber;
        }

        private static object CoerceBlockCount(DependencyObject element, object value)
        {
            return CoerceBlockCount((int)value);
        }

        private static int CoerceBlockCount(int input)
        {
            return (input < 1) ? 1 : input;
        }

        private static object CoerceBlockMargin(DependencyObject element, object value)
        {
            return CoerceBlockMargin((double)value);
        }

        private static double CoerceBlockMargin(double input)
        {
            return input < 0 || double.IsNaN(input) || double.IsInfinity(input) ? 0 : input;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            int blockCount = BlockCount;
            Size renderSize = RenderSize;
            double blockMargin = BlockMargin;
            double value = Value;

            int threshold = GetThreshold(value, blockCount);

            // Ensure background is hit-test visible (for tool tips and such).
            drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(renderSize));

            for (int i = 0; i < blockCount; i++)
            {
                Rect rect = GetRect(renderSize, blockCount, blockMargin, i);
                if (rect.IsEmpty)
                {
                    continue;
                }

                if (i >= threshold)
                {
                    drawingContext.PushOpacity(0.25);
                }

                drawingContext.DrawRectangle(BlockBrush, null, rect);
                if (i >= threshold)
                {
                    drawingContext.Pop();
                }
            }
        }

        private static Rect GetRect(Size targetSize, int blockCount, double blockMargin, int blockNumber)
        {
            if (targetSize.IsEmpty)
            {
                throw new ArgumentNullException();
            }

            if (blockCount < 1)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (blockNumber >= blockCount)
            {
                throw new ArgumentOutOfRangeException();
            }

            double width = (targetSize.Width - ((blockCount - 1) * blockMargin)) / blockCount;
            double left = (width + blockMargin) * blockNumber;
            double height = targetSize.Height;

            return width > 0 && height > 0 ? new Rect(left, 0, width, height) : Rect.Empty;
        }
    }
}