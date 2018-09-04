// UnsafeNativeMethods.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;

using Supremacy.Client.Interop;

namespace Supremacy.Client
{
    internal static class UnsafeNativeMethods
    {
        public delegate bool EnumThreadWindowsCallback(IntPtr hWnd, IntPtr lParam);

        public static HandleRef NullHandleRef;

        static UnsafeNativeMethods()
        {
            NullHandleRef = new HandleRef(null, IntPtr.Zero);
        }

        [SecurityCritical]
        public static bool DeleteObject(IntPtr hObject)
        {
            bool flag = ExternalNativeMethods.IntDeleteObject(hObject);
            Marshal.GetLastWin32Error();
            return flag;
        }

        private static class ExternalNativeMethods
        {
            [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("gdi32.dll", EntryPoint = "DeleteObject", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern bool IntDeleteObject(IntPtr hObject);
        }
    }
}