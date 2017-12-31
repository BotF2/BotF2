// Designer.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.ComponentModel;
using System.Windows;

namespace Supremacy.Client
{
    public static class Designer
    {
        private static readonly Lazy<bool> _isInDesignMode;

        public static bool IsInDesignMode
        {
            get { return _isInDesignMode.Value; }
        }

        static Designer()
        {
            _isInDesignMode = new Lazy<bool>(() => DesignerProperties.GetIsInDesignMode(new DependencyObject()));
        }
    }
}
