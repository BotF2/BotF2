﻿// // FocusHelper.cs
// //
// // Copyright (c) 2011 
// //
// // This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// // For details, see <http://www.opensource.org/licenses/ms-rl.html>.
// //
// // All other rights reserved.

using System;
using System.Windows;
using System.Windows.Input;

using Supremacy.Client.Interop;

namespace Supremacy.Client
{
    public static class FocusHelper
    {
        private static FrameworkElement _pendingFocusElement;

        private static FrameworkElement PendingFocusElement
        {
            get => _pendingFocusElement;
            set
            {
                if (_pendingFocusElement == value)
                {
                    return;
                }

                if (_pendingFocusElement != null)
                {
                    _pendingFocusElement.Loaded -= OnPendingFocusElementLoaded;
                }

                _pendingFocusElement = value;

                if (_pendingFocusElement != null)
                {
                    _pendingFocusElement.Loaded += OnPendingFocusElementLoaded;
                }
            }
        }

        private static void OnPendingFocusElementLoaded(object sender, RoutedEventArgs args)
        {
            _ = Keyboard.Focus(PendingFocusElement);
            PendingFocusElement = null;
        }

        public static void FocusWhenLoaded(this FrameworkElement element)
        {
            if (element.IsLoaded && PresentationSource.FromVisual(element) != null)
            {
                PendingFocusElement = null;
                _ = Keyboard.Focus(element);
            }
            else
            {
                PendingFocusElement = element;
            }
        }

        public static bool IsApplicationActive
        {
            get
            {
                IntPtr foregroundWindow = NativeMethods.GetForegroundWindow();
                if (foregroundWindow == IntPtr.Zero)
                {
                    return false;
                }


                _ = NativeMethods.GetWindowThreadProcessId(foregroundWindow, out uint foregroundProcessId);

                return NativeMethods.GetCurrentProcessId() == foregroundProcessId;
            }
        }
    }
}