// UnmanagedMethods.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Supremacy.UI
{
    internal static class UnmanagedMethods
    {
        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern IntPtr GetActiveWindow();
        
        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        [DllImport("comctl32.dll", EntryPoint = "TaskDialog", CharSet = CharSet.Auto)]
        internal static extern IntPtr TaskDialog(IntPtr hWnd, IntPtr hInstance, string title, string mainInstruction, string content, IntPtr buttons, int icon, out int buttonsOut);
    }
}
