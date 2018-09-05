// UnitActivationBar.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Supremacy.UI
{
    public class UnitActivationGroup
    {
        private int _poolSize;
        private int _lastFreePoolSize;
        private UnitActivationBar _poolBar;
        private readonly ObservableCollection<UnitActivationBar> _children;

        public event EventHandler PoolSizeChanged;
        public event EventHandler FreePoolSizeChanged;

        public ICollection<UnitActivationBar> Children
        {
            get { return _children; }
        }

        public UnitActivationBar PoolBar
        {
            get { return _poolBar; }
            set
            {
                if (_poolBar != null)
                {
                    _poolBar.Group = null;
                    _poolBar.ActiveUnits = 0;
                    _poolBar.Units = 0;
                }
                _poolBar = value;
                if (_poolBar != null)
                {
                    _poolBar.Group = this;
                    if (_children.Count > 0)
                        _poolBar.UnitCost = _children.Min(o => o.UnitCost);
                    _poolBar.Units = PoolSize / _poolBar.UnitCost;
                    _poolBar.MaxActiveUnits = FreePoolSize / _poolBar.UnitCost;
                }
            }
        }

        public int PoolSize
        {
            get { return _poolSize; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "value",
                        @"Value must be a non-negative integer.");
                }
                _poolSize = value;
                if (_poolBar != null)
                {
                    _poolBar.Units = _poolSize;
                    _poolBar.MaxActiveUnits = _poolSize;
                }
                if (PoolSizeChanged != null)
                {
                    PoolSizeChanged(this, new EventArgs());
                }
                ReconcilePool();
            }
        }

        public int FreePoolSize
        {
            get
            {
                var usedUnits = _children.Sum(child => (child.ActiveUnits * child.UnitCost));
                return Math.Max(_poolSize - usedUnits, 0);
            }
        }

        public UnitActivationGroup()
        {
            _children = new ObservableCollection<UnitActivationBar>();
            _children.CollectionChanged += OnChildrenChanged;
        }

        public void ResetPool(int poolSize)
        {
            if (poolSize < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "poolSize",
                    @"Value must be a non-negative integer");
            }
            foreach (var child in _children)
            {
                child.ActiveUnits = 0;
                child.MaxActiveUnits = 0;
            }
            PoolSize = poolSize;
        }

        private void OnChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (UnitActivationBar oldChild in e.OldItems)
                {
                    oldChild.Group = null;
                    oldChild.ActiveUnitsChanged -= OnChildActiveUnitsChanged;
                    oldChild.UnitCostChanged -= OnChildUnitCostChanged;
                    oldChild.MaxActiveUnits = oldChild.Units;
                }
            }
            if (e.NewItems != null)
            {
                foreach (UnitActivationBar newChild in e.NewItems)
                {
                    newChild.MaxActiveUnits = FreePoolSize;
                    newChild.Group = this;
                    newChild.ActiveUnitsChanged += OnChildActiveUnitsChanged;
                    newChild.UnitCostChanged += OnChildUnitCostChanged;
                }
            }
            ReconcilePool();
        }

        private void OnChildUnitCostChanged(
            object sender,
            DependencyPropertyChangedEventArgs<int> e)
        {
            ReconcilePool();
        }

        private void OnChildActiveUnitsChanged(
            object sender,
            DependencyPropertyChangedEventArgs<int> e)
        {
            var source = sender as UnitActivationBar;
            if (source == _poolBar)
                return;
            var freePoolSize = _lastFreePoolSize;
            ReconcilePool();
            _lastFreePoolSize = FreePoolSize;
            if ((freePoolSize != FreePoolSize) && (FreePoolSizeChanged != null))
                FreePoolSizeChanged(this, new EventArgs());
        }

        private void ReconcilePool()
        {
            var freePoolSize = FreePoolSize;
            var adjustedFreePoolSize = -1;

            while ((freePoolSize < 0) && (adjustedFreePoolSize != freePoolSize))
            {
                adjustedFreePoolSize = freePoolSize;

                for (var i = _children.Count - 1; (freePoolSize < 0) && (i >= 0); i--)
                {
                    if (freePoolSize >= 0)
                        break;

                    if (_children[i].Decrement())
                        continue;

                    while ((freePoolSize < 0) && (_children[i].Decrement()))
                        freePoolSize += _children[i].UnitCost;
                }
            }

            foreach (var child in _children)
            {
                child.MaxActiveUnits = Math.Min(
                    child.ActiveUnits + (freePoolSize / child.UnitCost),
                    child.Units);
            }

            if (_poolBar == null)
                return;

            _poolBar.UnitCost = _children.Min(o => o.UnitCost);
            _poolBar.Units = PoolSize / _poolBar.UnitCost;
            _poolBar.MaxActiveUnits = FreePoolSize / _poolBar.UnitCost;
        }
    }

    [TemplatePart(Name = "PART_IncrementButton", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_DecrementButton", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ContentArea", Type = typeof(UIElement))]
    public class UnitActivationBar : Control
    {
        #region Static Members
        public static readonly RoutedCommand IncrementValueCommand;
        public static readonly RoutedCommand DecrementValueCommand;
        public static readonly RoutedCommand SetActiveUnitsCommand;

        public static readonly DependencyProperty UnitsProperty;
        public static readonly DependencyProperty UnitCostProperty;
        public static readonly DependencyProperty MaxActiveUnitsProperty;
        public static readonly DependencyProperty ActiveUnitsProperty;
        public static readonly DependencyProperty IsReadOnlyProperty;
        public static readonly DependencyProperty UnitBrushProperty;

        private static readonly Brush DefaultUnitBrush;

        static UnitActivationBar()
        {
            FocusVisualStyleProperty.OverrideMetadata(
                typeof(UnitActivationBar),
                new FrameworkPropertyMetadata(
                    null,
                    (d, baseValue) => null));

            DefaultUnitBrush = new LinearGradientBrush(
                new GradientStopCollection(
                    new[]
                    {
                        new GradientStop((Color)ColorConverter.ConvertFromString("#ffd6bd6b"), 0.0),
                        new GradientStop((Color)ColorConverter.ConvertFromString("#ffed9300"), 0.1)
                    }),
                new Point(0.5, 0.0),
                new Point(0.5, 1.0));

            DefaultUnitBrush.Freeze();

            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(UnitActivationBar),
                new FrameworkPropertyMetadata(typeof(UnitActivationBar)));

            SnapsToDevicePixelsProperty.OverrideMetadata(
                typeof(UnitActivationBar),
                new FrameworkPropertyMetadata(true));

            IncrementValueCommand = new RoutedCommand(
                "IncrementValue",
                typeof(UnitActivationBar));

            DecrementValueCommand = new RoutedCommand(
                "DecrementValue",
                typeof(UnitActivationBar));

            SetActiveUnitsCommand = new RoutedCommand(
                "SetActiveUnits",
                typeof(UnitActivationBar));

            UnitsProperty = DependencyProperty.Register(
                "Units",
                typeof(int),
                typeof(UnitActivationBar),
                new FrameworkPropertyMetadata(
                    0,
                    FrameworkPropertyMetadataOptions.AffectsArrange
                        | FrameworkPropertyMetadataOptions.AffectsRender,
                    UnitsChangedCallback),
                ValidateValue);

            UnitCostProperty = DependencyProperty.Register(
                "UnitCost",
                typeof(int),
                typeof(UnitActivationBar),
                new PropertyMetadata(
                    1,
                    UnitCostChangedCallback,
                    CoerceUnitCost),
                ValidateUnitCost);

            ActiveUnitsProperty = DependencyProperty.Register(
                "ActiveUnits",
                typeof(int),
                typeof(UnitActivationBar),
                new FrameworkPropertyMetadata(
                    0,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    ActiveUnitsChangedCallback,
                    CoerceActiveUnits),
                ValidateValue);

            MaxActiveUnitsProperty = DependencyProperty.Register(
                "MaxActiveUnits",
                typeof(int),
                typeof(UnitActivationBar),
                new PropertyMetadata(
                    0,
                    MaxActiveUnitsChangedCallback,
                    CoerceMaxActiveUnits),
                ValidateValue);

            IsReadOnlyProperty = DependencyProperty.Register(
                "IsReadOnly",
                typeof(bool),
                typeof(UnitActivationBar));

            UnitBrushProperty = DependencyProperty.Register(
                "UnitBrush",
                typeof(Brush),
                typeof(UnitActivationBar),
                new PropertyMetadata(
                    DefaultUnitBrush,
                    delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
                    {
                        var source = sender as UnitActivationBar;
                        if (source != null)
                        {
                            source.InvalidateVisual();
                        }
                    }));

            IsHitTestVisibleProperty.OverrideMetadata(
                typeof(UnitActivationBar),
                new UIPropertyMetadata(true));

            MaxHeightProperty.OverrideMetadata(
                typeof(UnitActivationBar),
                new FrameworkPropertyMetadata(28.0));
        }

        private static void UnitsChangedCallback(
            DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            var source = sender as UnitActivationBar;
            if (source == null)
                return;
            if (source.UnitsChanged != null)
                source.UnitsChanged(source, new DependencyPropertyChangedEventArgs<int>(e));
            source.CoerceValue(MaxActiveUnitsProperty);
        }

        private static void UnitCostChangedCallback(
            DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            var source = sender as UnitActivationBar;
            if ((source == null) || (source.UnitCostChanged == null))
                return;
            source.UnitCostChanged(source, new DependencyPropertyChangedEventArgs<int>(e));
        }

        private static void ActiveUnitsChangedCallback(
            DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            var source = sender as UnitActivationBar;
            if ((source == null) || (source.ActiveUnitsChanged == null))
                return;
            source.ActiveUnitsChanged(source, new DependencyPropertyChangedEventArgs<int>(e));
        }

        private static void MaxActiveUnitsChangedCallback(
            DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            var source = sender as UnitActivationBar;
            if (source == null)
                return;
            if (source.MaxActiveUnitsChanged != null)
                source.MaxActiveUnitsChanged(source, new DependencyPropertyChangedEventArgs<int>(e));
            source.CoerceValue(ActiveUnitsProperty);
        }

        private static object CoerceUnitCost(DependencyObject sender, object value)
        {
            if ((int)value < 1)
                value = 1;
            return value;
        }

        private static object CoerceActiveUnits(DependencyObject sender, object value)
        {
            var source = sender as UnitActivationBar;
            if (source == null)
                return value;
            return Math.Max(Math.Min((int)value, source.MaxActiveUnits), 0);
        }

        private static object CoerceMaxActiveUnits(DependencyObject sender, object value)
        {
            var source = sender as UnitActivationBar;
            if (source == null)
                return value;
            return Math.Max(Math.Min((int)value, source.Units), 0);
        }

        private static bool ValidateValue(object value)
        {
            return ((int) value >= 0);
        }

        private static bool ValidateUnitCost(object value)
        {
            return ((int) value > 0);
        }
        #endregion

        #region Fields
        private Rect[] _unitBlockBounds;
        private UnitActivationGroup _group;
        private ButtonBase _decrementButton;
        private ButtonBase _incrementButton;
        private UIElement _contentArea;
        #endregion

        #region Properties
        public int Units
        {
            get { return (int)GetValue(UnitsProperty); }
            set { SetValue(UnitsProperty, value); }
        }

        public int UnitCost
        {
            get { return (int) GetValue(UnitCostProperty); }
            set { SetValue(UnitCostProperty, value); }
        }

        public int ActiveUnits
        {
            get { return (int) GetValue(ActiveUnitsProperty); }
            set { SetValue(ActiveUnitsProperty, value); }
        }

        public int MaxActiveUnits
        {
            get { return (int) GetValue(MaxActiveUnitsProperty); }
            set
            {
                SetValue(MaxActiveUnitsProperty, value);
                if (ActiveUnits > MaxActiveUnits)
                    ActiveUnits = MaxActiveUnits;
            }
        }

        public bool IsReadOnly
        {
            get { return (bool) GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public Brush UnitBrush
        {
            get { return GetValue(UnitBrushProperty) as Brush; }
            set { SetValue(UnitBrushProperty, value); }
        }

        public UnitActivationGroup Group
        {
            get { return _group; }
            internal set { _group = value; }
        }
        #endregion

        #region Events
        public event DependencyPropertyChangedEventHandler<int> UnitsChanged;
        public event DependencyPropertyChangedEventHandler<int> UnitCostChanged;
        public event DependencyPropertyChangedEventHandler<int> ActiveUnitsChanged;
        public event DependencyPropertyChangedEventHandler<int> MaxActiveUnitsChanged;
        #endregion

        #region Constructors
        public UnitActivationBar()
        {
            _incrementButton = null;
            _decrementButton = null;
            _contentArea = null;
            CommandBindings.Add(
                new CommandBinding(IncrementValueCommand,
                                   IncrementValueExecuted,
                                   CanIncrementValue));
            CommandBindings.Add(
                new CommandBinding(DecrementValueCommand,
                                   DecrementValueExecuted,
                                   CanDecrementValue));
            CommandBindings.Add(
                new CommandBinding(SetActiveUnitsCommand,
                                   SetValueExecuted));
        }
        #endregion

        #region Command Handlers
        private void CanIncrementValue(object sender, CanExecuteRoutedEventArgs e)
        {
            if (IsReadOnly)
                e.CanExecute = false;
            else
                e.CanExecute = (ActiveUnits < MaxActiveUnits);
        }

        private void CanDecrementValue(object sender, CanExecuteRoutedEventArgs e)
        {
            if (IsReadOnly)
                e.CanExecute = false;
            else
                e.CanExecute = (ActiveUnits > 0);
        }

        private void IncrementValueExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!IsReadOnly)
                Increment();
        }

        private void DecrementValueExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!IsReadOnly)
                Decrement();
        }

        private void SetValueExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SetActiveUnits((int) e.Parameter);
        }
        #endregion

        #region Methods
        public bool Increment()
        {
            if (ActiveUnits < MaxActiveUnits)
            {
                ActiveUnits++;
                return true;
            }
            return false;
        }

        public bool Decrement()
        {
            if (ActiveUnits > 0)
            {
                ActiveUnits--;
                return true;
            }
            return false;
        }

        public void SetActiveUnits(int value)
        {
            if (value <= 0)
            {
                value = 0;
            }
            if (value > MaxActiveUnits)
            {
                value = MaxActiveUnits;
            }
            ActiveUnits = value;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _incrementButton = GetTemplateChild("PART_IncrementButton") as ButtonBase;
            _decrementButton = GetTemplateChild("PART_DecrementButton") as ButtonBase;
            _contentArea = GetTemplateChild("PART_ContentArea") as UIElement;
            if (_incrementButton != null)
            {
                _incrementButton.Command = IncrementValueCommand;
                _incrementButton.Focusable = false;
            }
            if (_decrementButton != null)
            {
                _decrementButton.Command = DecrementValueCommand;
                _decrementButton.Focusable = false;
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (IsReadOnly)
                return;
            var hitResult = VisualTreeHelper.HitTest(this, e.GetPosition(this));
            if ((hitResult.VisualHit != null))
            {
                if ((_incrementButton != null)
                    && ((hitResult.VisualHit == _incrementButton) || (_incrementButton.IsAncestorOf(hitResult.VisualHit))))
                {
                    base.OnPreviewMouseLeftButtonDown(e);
                    return;
                }
                if ((_decrementButton != null)
                    && ((hitResult.VisualHit == _decrementButton) || (_decrementButton.IsAncestorOf(hitResult.VisualHit))))
                {
                    base.OnPreviewMouseLeftButtonDown(e);
                    return;
                }
            }
            CaptureMouse();
            SetValueFromMousePosition(e.GetPosition(this));
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (IsMouseCaptured)
            {
                SetValueFromMousePosition(e.GetPosition(this));
            }
            else
            {
                OnMouseMove(e);
            }
        }

        protected void SetValueFromMousePosition(Point point)
        {
            if (_unitBlockBounds.Length <= 0)
                return;

            if (point.X < _unitBlockBounds[0].Left)
            {
                SetActiveUnits(0);
                return;
            }

            if (point.X > _unitBlockBounds[_unitBlockBounds.Length - 1].Right)
            {
                SetActiveUnits(Units);
                return;
            }
            
            var closestIndex = -1;
            var minDistance = double.MaxValue;
                    
            for (var i = 0; i < _unitBlockBounds.Length; i++)
            {
                var centerX = _unitBlockBounds[i].Left + (_unitBlockBounds[i].Width / 2);
                var distance = Math.Abs(point.X - centerX);
                if (distance >= minDistance)
                    continue;
                minDistance = distance;
                closestIndex = i;
            }
                    
            if (closestIndex != -1)
                SetActiveUnits(closestIndex + 1);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                SetValueFromMousePosition(e.GetPosition(this));
                ReleaseMouseCapture();
                return;
            }
            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (_unitBlockBounds.Length == 0)
                return;

            var guidelinesX = new double[2 * _unitBlockBounds.Length];
            var guidelinesY = new[] { _unitBlockBounds[0].Top, _unitBlockBounds[0].Height };
            
            for (var i = 0; i < _unitBlockBounds.Length; i++)
            {
                guidelinesX[2 * i] = _unitBlockBounds[i].Left;
                guidelinesX[(2 * i) + 1] = _unitBlockBounds[i].Right;
            }

            drawingContext.PushGuidelineSet(
                new GuidelineSet(
                    guidelinesX,
                    guidelinesY));

            for (var i = 0; i < _unitBlockBounds.Length; i++)
            {
                if (i == ActiveUnits)
                    drawingContext.PushOpacity(0.25);
                
                drawingContext.DrawRectangle(
                    UnitBrush,
                    null,
                    _unitBlockBounds[i]);
            }

            if (ActiveUnits < Units)
                drawingContext.Pop();

            drawingContext.Pop();
        }

        protected Rect GetUnitBounds(int index, double unitBlockWidth)
        {
            var offset = (_contentArea != null)
                             ? _contentArea.TransformToAncestor(this).Transform(new Point(0, 0))
                             : new Point(0, 0);

            return new Rect(
                offset.X + ((unitBlockWidth + (unitBlockWidth / 2.0)) * index) + (unitBlockWidth / 2.0),
                offset.Y,
                unitBlockWidth,
                (_contentArea != null)
                    ? _contentArea.RenderSize.Height
                    : ActualHeight);
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var result = base.ArrangeOverride(arrangeBounds);

            var actualWidth = (_contentArea != null)
                                  ? _contentArea.RenderSize.Width
                                  : ActualWidth;

            if ((Group != null) && (Group.Children.Count > 0))
            {
                var maxSiblingWidth = Group.Children.Max(
                    o => (o._contentArea != null)
                             ? o._contentArea.RenderSize.Width
                             : o.ActualWidth);

                if (!double.IsNaN(maxSiblingWidth) && !double.IsInfinity(maxSiblingWidth) && (maxSiblingWidth < actualWidth))
                    actualWidth = maxSiblingWidth;
            }

            if ((actualWidth != 0)
                && !double.IsNaN(actualWidth)
                && !double.IsInfinity(actualWidth))
            {
                var unitBlockWidth = Math.Round(Math.Max(Math.Min((actualWidth * 0.5) / GetMaxUnitsInGroup(), 24.0), 1.0));

                _unitBlockBounds = new Rect[Units];

                for (var i = 0; i < Units; i++)
                    _unitBlockBounds[i] = GetUnitBounds(i, unitBlockWidth);
            }

            return result;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var availableWidth = double.IsInfinity(constraint.Width) ? 32767 : constraint.Width;
            var availableHeight = double.IsInfinity(constraint.Height) ? 32767 : constraint.Height;

            return new Size(availableWidth, Math.Min(availableHeight, MaxHeight));
        }

        private int GetMaxUnitsInGroup()
        {
            var maxUnits = Units;

            if (_group != null)
            {
                if ((_group.PoolBar != null) && (_group.PoolBar.Units > maxUnits))
                    maxUnits = _group.PoolBar.Units;
                
                foreach (var sibling in _group.Children)
                {
                    if (sibling.Units > maxUnits)
                        maxUnits = sibling.Units;
                }
            }

            return maxUnits;
        }
        #endregion
    }
}