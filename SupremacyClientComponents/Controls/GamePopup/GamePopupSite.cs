using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Supremacy.Utility;

namespace Supremacy.Client.Controls
{
    public class GamePopupSite : Decorator
    {
        #region Fields

        private readonly GamePopupCollection _popups;
        private readonly GamePopupCollection _openPopupsCore;
        private readonly ReadOnlyGamePopupCollection _openPopups;
        private readonly VisualCollection _openPopupRoots;

        #endregion

        #region Constructors and Finalizers

        public GamePopupSite()
        {
            _popups = new GamePopupCollection();
            _popups.CollectionChanged += OnPopupsCollectionChanged;

            _openPopupsCore = new GamePopupCollection();
            _openPopups = new ReadOnlyGamePopupCollection(_popups);
            _openPopupRoots = new VisualCollection(this);

            AddHandler(LoadedEvent, (RoutedEventHandler)OnLoaded);
        }

        private void OnPopupsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ProcessPopupsCollectionChanged(e);
        }

        private void UpdateRegisteredPopupSite(GamePopup popup, bool register)
        {
            if (register)
            {
                if (popup.RegisteredPopupSite != this)
                {
                    if (popup.RegisteredPopupSite != null)
                        throw new InvalidOperationException("GamePopup is already registered with another GamePopupSite.");
                    popup.RegisteredPopupSite = this;
                }
            }
            else if (popup.RegisteredPopupSite == this)
            {
                popup.RegisteredPopupSite = null;
            }
        }

        private void ProcessPopupsCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                    {
                        var gamePopup = item as GamePopup;
                        if (gamePopup != null)
                            UpdateRegisteredPopupSite(gamePopup, true);
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    if (!DesignerProperties.GetIsInDesignMode(this))
                        throw new NotSupportedException();
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        var gamePopup = item as GamePopup;
                        if (gamePopup == null)
                            continue;
                        if (gamePopup.IsOpen)
                            gamePopup.IsOpen = false;
                        UpdateRegisteredPopupSite(gamePopup, false);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (!DesignerProperties.GetIsInDesignMode(this))
                        throw new NotSupportedException();
                    break;
            }
        }

        #endregion

        #region Properties

        #region HasOpenPopups Property

        public bool HasOpenPopups
        {
            get { return _openPopups.Any(); }
        }

        #endregion

        #region OpenPopups Property

        public ReadOnlyGamePopupCollection OpenPopups
        {
            get { return _openPopups; }
        }

        #endregion

        #region Popups Property

        public GamePopupCollection Popups
        {
            get { return _popups; }
        }

        #endregion

        #endregion

        #region Internal Methods

        internal void AddCanvasChild(UIElement element)
        {
            var popupRoot = element as GamePopupRoot;
            if (popupRoot == null)
                return;

            _openPopupRoots.Add(popupRoot);
            
            InvalidateMeasure();
        }

        internal void RemoveCanvasChild(UIElement element)
        {
            var popupRoot = element as GamePopupRoot;
            if (popupRoot == null)
                return;

            _openPopupRoots.Remove(popupRoot);

            InvalidateMeasure();
        }

        internal void UpdateOpenGamePopups()
        {
            _openPopupsCore.BeginUpdate();
            try
            {
                _openPopupsCore.Clear();
                _openPopupsCore.AddRange(_popups.Where(o => o.IsOpen));
            }
            finally
            {
                _openPopupsCore.EndUpdate();
                InvalidateMeasure();
            }
        }

        #endregion

        #region Private Methods

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateOpenGamePopups();
        }

        #endregion

        #region Visual Child Enumeration

        protected override int VisualChildrenCount
        {
            get { return base.VisualChildrenCount + _openPopupRoots.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            var baseCount = base.VisualChildrenCount;
            if (index < baseCount)
                return base.GetVisualChild(index);
            return _openPopupRoots[index - baseCount];
        }

        #endregion

        #region Measure and Arrange Overrides

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            foreach (GamePopupRoot child in _openPopupRoots)
            {
                var x = 0d;
                var y = 0d;

                var left = Canvas.GetLeft(child);
                if (!DoubleUtil.IsNaN(left))
                {
                    x = left;
                }
                else
                {
                    var right = Canvas.GetRight(child);

                    if (!DoubleUtil.IsNaN(right))
                    {
                        x = arrangeSize.Width - child.DesiredSize.Width - right;
                    }
                }

                var top = Canvas.GetTop(child);
                if (!DoubleUtil.IsNaN(top))
                {
                    y = top;
                }
                else
                {
                    var bottom = Canvas.GetBottom(child);

                    if (!DoubleUtil.IsNaN(bottom))
                        y = arrangeSize.Height - child.DesiredSize.Height - bottom;
                }

                child.Arrange(new Rect(new Point(x, y), child.DesiredSize));
            }

            return base.ArrangeOverride(arrangeSize);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var childConstraint = new Size(double.PositiveInfinity, double.PositiveInfinity);

            foreach (GamePopupRoot popupRoot in _openPopupRoots)
                popupRoot.Measure(childConstraint);

            return base.MeasureOverride(constraint);
        }

        #endregion
    }
}