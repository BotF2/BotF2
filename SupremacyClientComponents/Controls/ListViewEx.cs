// File:ListViewEx.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.Windows.Controls;
using System.Windows.Input;

namespace Supremacy.Client.Controls
{
    public class ListViewItemEx : ListViewItem
    {
        #region Public and Protected Methods
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            //base.OnMouseEnter(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!IsSelected
                || Keyboard.IsKeyDown(Key.LeftShift)
                || Keyboard.IsKeyDown(Key.LeftCtrl)
                || Keyboard.IsKeyDown(Key.RightShift)
                || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                base.OnMouseLeftButtonDown(e);
                return;
            }
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (!IsSelected
                || Keyboard.IsKeyDown(Key.LeftShift)
                || Keyboard.IsKeyDown(Key.LeftCtrl)
                || Keyboard.IsKeyDown(Key.RightShift)
                || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                base.OnMouseLeftButtonUp(e);
            }
            else
            {
                base.OnMouseLeftButtonDown(e);
            }
        }
        #endregion
    }

    public class ListViewEx : ListView
    {
        protected override System.Windows.DependencyObject GetContainerForItemOverride()
        {
            return new ListViewItemEx();
        }
    }
}