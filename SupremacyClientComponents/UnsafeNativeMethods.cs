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
        #region Delegates
        public delegate bool EnumThreadWindowsCallback(IntPtr hWnd, IntPtr lParam);
        #endregion

        #region Fields
        public static HandleRef NullHandleRef;
        #endregion

        #region Constructors and Finalizers
        static UnsafeNativeMethods()
        {
            NullHandleRef = new HandleRef(null, IntPtr.Zero);
        }
        #endregion

        #region Public and Protected Methods
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool EnumThreadWindows(int dwThreadId, EnumThreadWindowsCallback lpfn, HandleRef lParam);

        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetCapture();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern int GetCurrentThreadId();

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetDesktopWindow();

        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool IsWindow(HandleRef hWnd);

        [SecuritySafeCritical]
        [SecurityCritical]
        public static bool IsWindowEnabled(HandleRef hWnd)
        {
            return ExternalNativeMethods.IsWindowEnabled(hWnd);
        }

        [SecurityCritical]
        [SecuritySafeCritical]
        public static bool IsWindowVisible(HandleRef hWnd)
        {
            return ExternalNativeMethods.IsWindowVisible(hWnd);
        }
        #endregion

        #region Internal Methods
        [SecurityCritical]
        public static void ClientToScreen(HandleRef hWnd, [In, Out] NativeMethods.POINT pt)
        {
            if (ExternalNativeMethods.IntClientToScreen(hWnd, pt) == 0)
                throw new Win32Exception();
        }

        [SecuritySafeCritical, SecurityCritical]
        internal static int GetWindowStyle(HandleRef hWnd, bool exStyle)
        {
            int nIndex = exStyle ? -20 : -16;
            return GetWindowLong(hWnd, nIndex);
        }

        [SecuritySafeCritical, SecurityCritical]
        internal static void GetClientRect(HandleRef hWnd, [In, Out] ref NativeMethods.RECT rect)
        {
            if (!ExternalNativeMethods.IntGetClientRect(hWnd, ref rect))
                throw new Win32Exception();
        }

        [SecurityCritical]
        internal static bool TryGetCursorPos([In, Out] ref NativeMethods.POINT pt)
        {
            var flag = ExternalNativeMethods.IntTryGetCursorPos(ref pt);
            if (!flag)
            {
                pt.X = 0;
                pt.Y = 0;
            }
            return flag;
        }

        [SecurityCritical, SecuritySafeCritical]
        internal static void ScreenToClient(HandleRef hWnd, [In, Out] NativeMethods.POINT pt)
        {
            if (ExternalNativeMethods.IntScreenToClient(hWnd, pt) == 0)
                throw new Win32Exception();
        }

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int GetObject(HandleRef hObject, int nSize, [In, Out] NativeMethods.BITMAP bm);

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern int GetBitmapBits(HandleRef hbmp, int cbBuffer, byte[] lpvBits);

        [SecuritySafeCritical, SecurityCritical]
        internal static IntPtr GetCursor()
        {
            return ExternalNativeMethods.GetCursor();
        }

        [SecurityCritical]
        public static bool DeleteObject(IntPtr hObject)
        {
            bool flag = ExternalNativeMethods.IntDeleteObject(hObject);
            Marshal.GetLastWin32Error();
            return flag;
        }

        [SecurityCritical]
        public static void DeleteObject(HandleRef hObject)
        {
            NativeMethods.HandleCollector.Remove((IntPtr)hObject, NativeMethods.CommonHandles.GDI);
            if (!ExternalNativeMethods.IntDeleteObject(hObject))
            {
                throw new Win32Exception();
            }
        }

        [SecurityCritical]
        internal static void GetIconInfo(HandleRef hIcon, out NativeMethods.ICONINFO piconinfo)
        {
            bool iconInfoImpl;
            piconinfo = new NativeMethods.ICONINFO();
            var iconInfo = new NativeMethods.ICONINFO_IMPL();
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                iconInfoImpl = ExternalNativeMethods.GetIconInfoImpl(hIcon, iconInfo);
                Marshal.GetLastWin32Error();
                if (iconInfoImpl)
                {
                    piconinfo.hbmMask = new NativeMethods.BitmapHandle(iconInfo.hbmMask, true);
                    piconinfo.hbmColor = new NativeMethods.BitmapHandle(iconInfo.hbmColor, true);
                    piconinfo.fIcon = iconInfo.fIcon;
                    piconinfo.xHotspot = iconInfo.xHotspot;
                    piconinfo.yHotspot = iconInfo.yHotspot;
                }
            }
            if (!iconInfoImpl)
            {
                throw new Win32Exception();
            }
        }

        [SecurityCritical]
        internal static IntPtr GetParent(HandleRef hWnd)
        {
            SetLastError(0);
            var ptr = ExternalNativeMethods.IntGetParent(hWnd);
            int error = Marshal.GetLastWin32Error();
            if ((ptr == IntPtr.Zero) && (error != 0))
                throw new Win32Exception(error);
            return ptr;
        }

        [SecurityCritical]
        internal static int GetWindowLong(HandleRef hWnd, int nIndex)
        {
            int windowLong;
            SetLastError(0);
            if (IntPtr.Size == 4)
            {
                windowLong = ExternalNativeMethods.IntGetWindowLong(hWnd, nIndex);
                Marshal.GetLastWin32Error();
            }
            else
            {
                var zero = ExternalNativeMethods.IntGetWindowLongPtr(hWnd, nIndex);
                Marshal.GetLastWin32Error();
                windowLong = ExternalNativeMethods.IntPtrToInt32(zero);
            }
            return windowLong;
        }

        [SecurityCritical]
        [SecuritySafeCritical]
        internal static bool ReleaseCapture()
        {
            if (!ExternalNativeMethods.IntReleaseCapture())
                throw new Win32Exception();
            return true;
        }

        [SecuritySafeCritical]
        [SecurityCritical]
        internal static IntPtr SetActiveWindow(HandleRef hWnd)
        {
            var ptr = ExternalNativeMethods.IntSetActiveWindow(hWnd);
            if (ptr == IntPtr.Zero)
                throw new Win32Exception();
            return ptr;
        }

        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern void SetLastError(int dwErrorCode);

        [SecurityCritical]
        internal static bool TrySetFocus(HandleRef hWnd, ref IntPtr result)
        {
            SetLastError(0);
            result = ExternalNativeMethods.IntSetFocus(hWnd);
            if ((result == IntPtr.Zero) && (Marshal.GetLastWin32Error() != 0))
                return false;
            return true;
        }

        [SecurityCritical]
        public static bool EnableWindowNoThrow(HandleRef hWnd, bool enable)
        {
            return ExternalNativeMethods.IntEnableWindowNoThrow(hWnd, enable);
        }
        #endregion

        #region Nested type: ExternalNativeMethods
        private static class ExternalNativeMethods
        {
            #region Internal Methods
            [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("user32.dll", EntryPoint = "EnableWindow", CharSet = CharSet.Auto)]
            public static extern bool IntEnableWindowNoThrow(HandleRef hWnd, bool enable);

            [SuppressUnmanagedCodeSecurity]
            [SecurityCritical]
            [DllImport("user32.dll", EntryPoint = "GetParent", CharSet = CharSet.Auto, SetLastError = true,
                ExactSpelling = true)]
            internal static extern IntPtr IntGetParent(HandleRef hWnd);

            [SuppressUnmanagedCodeSecurity]
            [SecurityCritical]
            [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern int IntGetWindowLong(HandleRef hWnd, int nIndex);

            [SuppressUnmanagedCodeSecurity]
            [SecurityCritical]
            [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern IntPtr IntGetWindowLongPtr(HandleRef hWnd, int nIndex);

            internal static int IntPtrToInt32(IntPtr intPtr)
            {
                return (int)intPtr.ToInt64();
            }

            [DllImport("user32.dll", EntryPoint = "ReleaseCapture", CharSet = CharSet.Auto, SetLastError = true,
                ExactSpelling = true)]
            internal static extern bool IntReleaseCapture();

            [DllImport("user32.dll", EntryPoint = "SetActiveWindow", CharSet = CharSet.Auto, SetLastError = true,
                ExactSpelling = true)]
            internal static extern IntPtr IntSetActiveWindow(HandleRef hWnd);

            [SecurityCritical]
            [SuppressUnmanagedCodeSecurity]
            [DllImport("user32.dll", EntryPoint = "SetFocus", CharSet = CharSet.Auto, SetLastError = true,
                ExactSpelling = true)]
            internal static extern IntPtr IntSetFocus(HandleRef hWnd);

            [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern bool IsWindowEnabled(HandleRef hWnd);

            [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
            internal static extern bool IsWindowVisible(HandleRef hWnd);

            [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("gdi32.dll", EntryPoint = "DeleteObject", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern bool IntDeleteObject(IntPtr hObject);

            [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("gdi32.dll", EntryPoint = "DeleteObject", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
            internal static extern bool IntDeleteObject(HandleRef hObject);

            [SuppressUnmanagedCodeSecurity, SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("user32.dll", EntryPoint = "GetIconInfo", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern bool GetIconInfoImpl(HandleRef hIcon, [Out] NativeMethods.ICONINFO_IMPL piconinfo);

            [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
            internal static extern IntPtr GetCursor();

            [DllImport("user32.dll", EntryPoint = "ScreenToClient", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
            internal static extern int IntScreenToClient(HandleRef hWnd, [In, Out] NativeMethods.POINT pt);

            [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("user32.dll", EntryPoint = "GetCursorPos", CharSet = CharSet.Auto, ExactSpelling = true)]
            internal static extern bool IntTryGetCursorPos([In, Out] ref NativeMethods.POINT pt);

            [DllImport("user32.dll", EntryPoint = "GetClientRect", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
            internal static extern bool IntGetClientRect(HandleRef hWnd, [In, Out] ref NativeMethods.RECT rect);

            [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("user32.dll", EntryPoint = "ClientToScreen", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
            internal static extern int IntClientToScreen(HandleRef hWnd, [In, Out] NativeMethods.POINT pt);
            #endregion
        }
        #endregion
    }
}