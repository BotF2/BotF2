// GlyphIconInfo.cs
// 
// Copyright (c) 2011 Mike Strobel
// 
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
// 
// All other rights reserved.

using System;
using System.ComponentModel;

using System.Threading;

using System.Windows.Media;

using Supremacy.Utility;

namespace Supremacy.Client.Controls
{
    public class GlyphIconInfo : INotifyPropertyChanged
    {
        #region NormalBrush Property

        [field: NonSerialized]
        public event EventHandler NormalBrushChanged;

        private Brush _normalBrush;

        public Brush NormalBrush
        {
            get { return _normalBrush; }
            set
            {
                if (Equals(value, _normalBrush))
                    return;

                _normalBrush = value;

                OnNormalBrushChanged();
            }
        }

        protected virtual void OnNormalBrushChanged()
        {
            NormalBrushChanged.Raise(this);
            OnPropertyChanged("NormalBrush");
        }

        #endregion

        #region HoverBrush Property

        [field: NonSerialized]
        public event EventHandler HoverBrushChanged;

        private Brush _hoverBrush;

        public Brush HoverBrush
        {
            get { return _hoverBrush; }
            set
            {
                if (Equals(value, _hoverBrush))
                    return;

                _hoverBrush = value;

                OnHoverBrushChanged();
            }
        }

        protected virtual void OnHoverBrushChanged()
        {
            HoverBrushChanged.Raise(this);
            OnPropertyChanged("HoverBrush");
        }

        #endregion

        #region PressedBrush Property

        [field: NonSerialized]
        public event EventHandler PressedBrushChanged;

        private Brush _pressedBrush;

        public Brush PressedBrush
        {
            get { return _pressedBrush; }
            set
            {
                if (Equals(value, _pressedBrush))
                    return;

                _pressedBrush = value;

                OnPressedBrushChanged();
            }
        }

        protected virtual void OnPressedBrushChanged()
        {
            PressedBrushChanged.Raise(this);
            OnPropertyChanged("PressedBrush");
        }

        #endregion

        #region DisabledBrush Property

        [field: NonSerialized]
        public event EventHandler DisabledBrushChanged;

        private Brush _disabledBrush;

        public Brush DisabledBrush
        {
            get { return _disabledBrush; }
            set
            {
                if (Equals(value, _disabledBrush))
                    return;

                _disabledBrush = value;

                OnDisabledBrushChanged();
            }
        }

        protected virtual void OnDisabledBrushChanged()
        {
            DisabledBrushChanged.Raise(this);
            OnPropertyChanged("DisabledBrush");
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        [NonSerialized] private PropertyChangedEventHandler _propertyChanged;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                while (true)
                {
                    var oldHandler = _propertyChanged;
                    var newHandler = (PropertyChangedEventHandler)Delegate.Combine(oldHandler, value);

                    if (Interlocked.CompareExchange(ref _propertyChanged, newHandler, oldHandler) == oldHandler)
                        return;
                }
            }
            remove
            {
                while (true)
                {
                    var oldHandler = _propertyChanged;
                    var newHandler = (PropertyChangedEventHandler)Delegate.Remove(oldHandler, value);

                    if (Interlocked.CompareExchange(ref _propertyChanged, newHandler, oldHandler) == oldHandler)
                        return;
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            _propertyChanged.Raise(this, propertyName);
        }

        #endregion
    }
}