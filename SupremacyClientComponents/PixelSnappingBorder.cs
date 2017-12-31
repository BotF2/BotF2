// PixelSnappingBorder.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;

namespace Supremacy.Client
{
    public class PixelSnappingBorder : Border
    {
        protected override Size MeasureOverride(Size constraint)
        {
            var desiredSize = base.MeasureOverride(constraint);
            return new Size(Math.Ceiling(desiredSize.Width), Math.Ceiling(desiredSize.Height));
        }
    }
}