using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using Supremacy.Types;

namespace Supremacy.Client.Controls
{
    /// <summary>
    /// Defines a flexible grid area that consists of columns and rows.
    /// Depending on the orientation, either the rows or the columns are auto-generated,
    /// and the children's position is set according to their index.
    /// </summary>
    public class AutoGrid : Grid
    {
        #region Constants
        protected const int Undefined = -1;
        #endregion

        #region Fields
        /// <summary>
        /// A value of <c>true</c> forces children to be re-indexed at the next oportunity.
        /// </summary>
        private bool _shouldReindex = true;
        private int _rowOrColumnCount;

        private readonly Dictionary<UIElement, ChildLayoutInfo> _childData;
        private readonly StateScope _layoutScope;
        #endregion

        #region ChildLayoutInfo Nested Type
        private class ChildLayoutInfo
        {
            public int OriginalRow { get; set; }
            public int OriginalColumn { get; set; }
            public int ActualRow { get; set; }
            public int ActualColumn { get; set; }
        }
        #endregion

        #region ReservedPosition Nested Type
        private struct ReservedPosition : IEquatable<ReservedPosition>
        {
            // ReSharper disable MemberCanBePrivate.Local
            public readonly int Row;
            public readonly int Column;
            // ReSharper restore MemberCanBePrivate.Local

            public ReservedPosition(int row, int column)
            {
                Row = row;
                Column = column;
            }

            public bool Equals(ReservedPosition other)
            {
                return other.Row == Row && other.Column == Column;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;
                if (obj.GetType() != typeof(ReservedPosition))
                    return false;
                return Equals((ReservedPosition)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Row * 397) ^ Column;
                }
            }

            public static bool operator ==(ReservedPosition left, ReservedPosition right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(ReservedPosition left, ReservedPosition right)
            {
                return !left.Equals(right);
            }
        }
        #endregion

        #region Constructors and Finalizers
        static AutoGrid()
        {
            ChildMarginProperty = DependencyProperty.Register(
                "ChildMargin",
                typeof(Thickness?),
                typeof(AutoGrid),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnPropertyChanged));

            IsAutoIndexingProperty = DependencyProperty.Register(
                "IsAutoIndexing",
                typeof(bool),
                typeof(AutoGrid),
                new FrameworkPropertyMetadata(
                    true,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnPropertyChanged));

            OrientationProperty = DependencyProperty.Register(
                "Orientation",
                typeof(Orientation),
                typeof(AutoGrid),
                new FrameworkPropertyMetadata(
                    Orientation.Vertical,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnPropertyChanged));

            ChildHorizontalAlignmentProperty = DependencyProperty.Register(
                "ChildHorizontalAlignment",
                typeof(HorizontalAlignment?),
                typeof(AutoGrid),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnPropertyChanged));

            ChildVerticalAlignmentProperty = DependencyProperty.Register(
                "ChildVerticalAlignment",
                typeof(VerticalAlignment?),
                typeof(AutoGrid),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnPropertyChanged));
        }

        /// <summary>
        /// Creates a new AutoGrid instance.
        /// </summary>
        public AutoGrid()
        {
            _childData = new Dictionary<UIElement, ChildLayoutInfo>();
            _layoutScope = new StateScope();
        }

        #endregion

        #region Dependency Properties
        /// <summary>
        /// Gets or sets a value indicating whether the children are automatically indexed.
        /// <remarks>
        /// The default is <c>true</c>.
        /// Note that if children are already indexed, setting this property to <c>false</c> will not remove their indices.
        /// </remarks>
        /// </summary>
        public bool IsAutoIndexing
        {
            get { return (bool)GetValue(IsAutoIndexingProperty); }
            set { SetValue(IsAutoIndexingProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="IsAutoIndexing"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsAutoIndexingProperty;

        /// <summary>
        /// Gets or sets the orientation.
        /// <remarks>The default is Vertical.</remarks>
        /// </summary>
        /// <value>The orientation.</value>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Orientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty;


        /// <summary>
        /// Gets or sets the child margin.
        /// </summary>
        /// <value>The child margin.</value>
        public Thickness? ChildMargin
        {
            get { return (Thickness?)GetValue(ChildMarginProperty); }
            set { SetValue(ChildMarginProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ChildMargin"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ChildMarginProperty;


        /// <summary>
        /// Gets or sets the child horizontal alignment.
        /// </summary>
        /// <value>The child horizontal alignment.</value>
        public HorizontalAlignment? ChildHorizontalAlignment
        {
            get { return (HorizontalAlignment?)GetValue(ChildHorizontalAlignmentProperty); }
            set { SetValue(ChildHorizontalAlignmentProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ChildHorizontalAlignment"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ChildHorizontalAlignmentProperty;


        /// <summary>
        /// Gets or sets the child vertical alignment.
        /// </summary>
        /// <value>The child vertical alignment.</value>
        public VerticalAlignment? ChildVerticalAlignment
        {
            get { return (VerticalAlignment?)GetValue(ChildVerticalAlignmentProperty); }
            set { SetValue(ChildVerticalAlignmentProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ChildVerticalAlignment"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ChildVerticalAlignmentProperty;

        private static void OnPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((AutoGrid)o)._shouldReindex = true;
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Measures the children of a <see cref="T:System.Windows.Controls.Grid"/> in anticipation of arranging them during the <see cref="FrameworkElement.ArrangeOverride"/> pass.
        /// </summary>
        /// <param name="constraint">Indicates an upper limit size that should not be exceeded.</param>
        /// <returns>
        /// 	<see cref="Size"/> that represents the required size to arrange child content.
        /// </returns>
        protected override Size MeasureOverride(Size constraint)
        {
            using (_layoutScope.Enter())
            {
                var isVertical = Orientation == Orientation.Vertical;

                if (_shouldReindex || (IsAutoIndexing &&
                                       ((isVertical && _rowOrColumnCount != ColumnDefinitions.Count) ||
                                        (!isVertical && _rowOrColumnCount != RowDefinitions.Count))))
                {
                    _shouldReindex = false;

                    if (IsAutoIndexing)
                    {
                        _rowOrColumnCount = (isVertical) ? ColumnDefinitions.Count : RowDefinitions.Count;
                        if (_rowOrColumnCount == 0) _rowOrColumnCount = 1;

                        var cellCount = 0;
                        var currentRow = 0;
                        var currentColumn = 0;
                        var reservedPositions = new HashSet<ReservedPosition>();

                        foreach (UIElement child in Children)
                        {
                            ChildLayoutInfo layoutInfo;

                            if (!_childData.TryGetValue(child, out layoutInfo))
                            {
                                layoutInfo = new ChildLayoutInfo
                                             {
                                                 OriginalColumn = Undefined,
                                                 OriginalRow = Undefined
                                             };

                                if (!child.HasDefaultValue(ColumnProperty))
                                {
                                    layoutInfo.OriginalColumn = GetColumn(child);
                                
                                    if ((Orientation == Orientation.Vertical) &&
                                        (layoutInfo.OriginalColumn >= ColumnDefinitions.Count))
                                    {
                                        layoutInfo.OriginalColumn = ColumnDefinitions.Count - 1;
                                    }
                                }

                                if (!child.HasDefaultValue(RowProperty))
                                {
                                    layoutInfo.OriginalRow = GetRow(child);

                                    if ((Orientation == Orientation.Horizontal) &&
                                        (layoutInfo.OriginalRow >= RowDefinitions.Count))
                                    {
                                        layoutInfo.OriginalRow = RowDefinitions.Count - 1;
                                    }
                                }

                                _childData[child] = layoutInfo;
                            }

                            if (layoutInfo.OriginalColumn != Undefined)
                            {
                                if (layoutInfo.OriginalRow != Undefined)
                                {
                                    layoutInfo.ActualColumn = layoutInfo.OriginalColumn;
                                    layoutInfo.ActualRow = layoutInfo.OriginalRow;
                                    reservedPositions.Add(
                                        new ReservedPosition(
                                            layoutInfo.ActualColumn,
                                            layoutInfo.ActualRow));
                                }
                                else if (isVertical)
                                {
                                    while ((layoutInfo.OriginalColumn != currentColumn) ||
                                           reservedPositions.Contains(new ReservedPosition(currentRow, currentColumn)))
                                    {
                                        ++cellCount;
                                        if (++currentColumn >= _rowOrColumnCount)
                                        {
                                            currentColumn = 0;
                                            ++currentRow;
                                        }
                                    }
                                    layoutInfo.ActualRow = currentRow;
                                    layoutInfo.ActualColumn = currentColumn;
                                }
                            }
                            else if (layoutInfo.OriginalRow != Undefined)
                            {
                                if (!isVertical)
                                {
                                    while ((layoutInfo.OriginalRow != currentRow) ||
                                           reservedPositions.Contains(new ReservedPosition(currentRow, currentColumn)))
                                    {
                                        ++cellCount;
                                        if (++currentRow >= _rowOrColumnCount)
                                        {
                                            currentRow = 0;
                                            ++currentColumn;
                                        }
                                    }
                                    layoutInfo.ActualRow = currentRow;
                                    layoutInfo.ActualColumn = currentColumn;
                                }
                            }
                            else
                            {
                                while (reservedPositions.Contains(new ReservedPosition(currentRow, currentColumn)))
                                {
                                    ++cellCount;
                                    if (isVertical)
                                    {
                                        if (++currentColumn >= _rowOrColumnCount)
                                        {
                                            currentColumn = 0;
                                            ++currentRow;
                                        }
                                    }
                                    else
                                    {
                                        if (++currentRow >= _rowOrColumnCount)
                                        {
                                            currentRow = 0;
                                            ++currentColumn;
                                        }
                                    }
                                }
                                layoutInfo.ActualColumn = currentColumn;
                                layoutInfo.ActualRow = currentRow;
                            }

                            ++cellCount;

                            SetRow(child, layoutInfo.ActualRow);
                            SetColumn(child, layoutInfo.ActualColumn);

                            var childRowSpan = GetRowSpan(child);
                            var childColumnSpan = GetColumnSpan(child);

                            if (isVertical)
                            {
                                if (!child.HasDefaultValue(ColumnSpanProperty))
                                {
                                    if ((currentColumn + childColumnSpan) >= _rowOrColumnCount)
                                        childColumnSpan -= ((currentColumn + childColumnSpan) - _rowOrColumnCount - 1);

                                    cellCount += childRowSpan * childColumnSpan;

                                    for (var i = 0; i < (childColumnSpan - 1); i++)
                                        reservedPositions.Add(new ReservedPosition(currentRow, currentColumn + i));
                                }
                                if (!child.HasDefaultValue(RowSpanProperty))
                                {
                                    cellCount += childRowSpan * childColumnSpan;

                                    for (var i = 0; i < (childRowSpan - 1); i++)
                                        reservedPositions.Add(new ReservedPosition(currentRow + i, currentColumn));

                                }
                                else if (++currentColumn >= _rowOrColumnCount)
                                {
                                    currentColumn = 0;
                                    ++currentRow;
                                }
                            }
                            else
                            {
                                if (!child.HasDefaultValue(RowSpanProperty))
                                {
                                    if ((currentColumn + childRowSpan) >= _rowOrColumnCount)
                                        childRowSpan -= ((currentColumn + childRowSpan) - _rowOrColumnCount - 1);

                                    cellCount += childRowSpan * childColumnSpan;

                                    for (var i = 0; i < (childRowSpan - 1); i++)
                                        reservedPositions.Add(new ReservedPosition(currentRow + i, currentColumn));
                                }
                                if (!child.HasDefaultValue(ColumnSpanProperty))
                                {
                                    cellCount += childRowSpan * childColumnSpan;

                                    for (var i = 0; i < (childColumnSpan - 1); i++)
                                        reservedPositions.Add(new ReservedPosition(currentRow, currentColumn + i));
                                }
                                else if (++currentRow >= _rowOrColumnCount)
                                {
                                    currentRow = 0;
                                    ++currentColumn;
                                }
                            }
                        }

                        //  Update the number of rows/columns
                        if (isVertical)
                        {
                            int newRowCount = ((cellCount - 1) / _rowOrColumnCount + 1);
                            while (RowDefinitions.Count < newRowCount)
                            {
                                RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                            }
                            if (RowDefinitions.Count > newRowCount)
                            {
                                RowDefinitions.RemoveRange(newRowCount, RowDefinitions.Count - newRowCount);
                            }
                        }
                        else // horizontal
                        {
                            int newColumnCount = ((cellCount - 1) / _rowOrColumnCount + 1);
                            while (ColumnDefinitions.Count < newColumnCount)
                            {
                                ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                            }
                            if (ColumnDefinitions.Count > newColumnCount)
                            {
                                ColumnDefinitions.RemoveRange(newColumnCount, ColumnDefinitions.Count - newColumnCount);
                            }
                        }
                    }

                    // Set margin and alignment
                    foreach (UIElement child in Children)
                    {
                        if (ChildMargin != null)
                        {
                            child.SetIfDefault(MarginProperty, ChildMargin.Value);
                        }
                        if (ChildHorizontalAlignment != null)
                        {
                            child.SetIfDefault(HorizontalAlignmentProperty, ChildHorizontalAlignment.Value);
                        }
                        if (ChildVerticalAlignment != null)
                        {
                            child.SetIfDefault(VerticalAlignmentProperty, ChildVerticalAlignment.Value);
                        }
                    }
                }

                return base.MeasureOverride(constraint);
            }
        }

        public void InvalidateIndexes()
        {
            _shouldReindex = true;
            InvalidateMeasure();
        }
        #endregion
    }
}