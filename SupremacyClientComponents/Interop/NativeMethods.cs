using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows;

namespace Supremacy.Client.Interop
{
    internal static class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            public int X;
            public int Y;
            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        /// <summary>
        /// Represents a Win32 rectangle structure that is used with GDI.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            internal int Left;
            internal int Top;
            internal int Right;
            internal int Bottom;

            internal RECT(Rect bounds)
            {
                Bottom = (int)Math.Ceiling(bounds.Bottom);
                Left = (int)Math.Ceiling(bounds.Left);
                Right = (int)Math.Ceiling(bounds.Right);
                Top = (int)Math.Ceiling(bounds.Top);
            }

            internal Rect ToRect()
            {
                return new Rect(
                    Left,
                    Top,
                    Right - Left,
                    Bottom - Top);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class MONITORINFO
        {
            internal int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            internal RECT rcMonitor;
            internal RECT rcWork;
            internal int dwFlags;
        }

        internal class BitmapHandle : SafeHandle
        {
            [SecuritySafeCritical, SecurityCritical]
            internal BitmapHandle()
                : base(IntPtr.Zero, true)
            {
            }

            [SecuritySafeCritical, SecurityCritical]
            internal BitmapHandle(IntPtr ptr, bool fOwnsHandle)
                : base(ptr, fOwnsHandle)
            {
                // ReSharper disable DoNotCallOverridableMethodsInConstructor
                HandleCollector.Add(handle, HandleCollectorType);
                // ReSharper restore DoNotCallOverridableMethodsInConstructor
            }

            public HandleRef MakeHandleRef(object wrapper)
            {
                return new HandleRef(wrapper, handle);
            }

            [SecurityCritical]
            public static bool DeleteObject(IntPtr hObject)
            {
                var result = UnsafeNativeMethods.DeleteObject(hObject);
                Marshal.GetLastWin32Error();
                return result;
            }

            [SecurityCritical]
            protected override bool ReleaseHandle()
            {
                HandleCollector.Remove(handle, CommonHandles.GDI);
                return UnsafeNativeMethods.DeleteObject(handle);
            }

            internal virtual int HandleCollectorType
            {
                get
                {
                    return CommonHandles.GDI;
                }
            }

            public override bool IsInvalid
            {
                [SecurityCritical, SecuritySafeCritical]
                get
                {
                    return (handle == IntPtr.Zero);
                }
            }
        }

        [DllImport("user32.dll")]
        public extern static IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public extern static uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentProcessId();

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class NOTIFYICONDATA
        {
            public int cbSize = Marshal.SizeOf(typeof(NOTIFYICONDATA));
            public IntPtr hWnd;
            public int uID;
            public NotifyIconFlags uFlags;
            public int uCallbackMessage;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x80)]
            public string szTip;
            public int dwState;
            public int dwStateMask;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x100)]
            public string szInfo;
            public int uTimeoutOrVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x40)]
            public string szInfoTitle;
            public int dwInfoFlags;
        }

        [Flags]
        public enum NotifyIconFlags
        {
            /// <summary>
            /// The hIcon member is valid.
            /// </summary>
            Icon = 2,
            /// <summary>
            /// The uCallbackMessage member is valid.
            /// </summary>
            Message = 1,
            /// <summary>
            /// The szTip member is valid.
            /// </summary>
            ToolTip = 4,
            /// <summary>
            /// The dwState and dwStateMask members are valid.
            /// </summary>
            State = 8,
            /// <summary>
            /// Use a balloon ToolTip instead of a standard ToolTip. The szInfo, uTimeout, szInfoTitle, and dwInfoFlags members are valid.
            /// </summary>
            Balloon = 0x10,
        }

        /// <summary>
        /// Modifies to the specified bounds to ensure they are on-screen.
        /// </summary>
        /// <param name="bounds">The bounds to examine.</param>
        /// <returns>The modified bounds.</returns>
        internal static Rect EnsureBoundsOnScreen(Rect bounds)
        {
            const int MONITOR_DEFAULTTONEAREST = 0x00000002;

            var monitorInfo = new MONITORINFO();
            var rect = new RECT(bounds);
            var monitor = MonitorFromRect(ref rect, MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero)
            {
                if (GetMonitorInfo(monitor, monitorInfo))
                {
                    var workingArea = monitorInfo.rcWork.ToRect();

                    if (bounds.Right > workingArea.Right)
                        bounds.Offset(workingArea.Right - bounds.Right, 0);
                    if (bounds.Left < workingArea.Left)
                        bounds.Offset(workingArea.Left - bounds.Left, 0);

                    if (bounds.Bottom > workingArea.Bottom)
                        bounds.Offset(0, workingArea.Bottom - bounds.Bottom);
                    if (bounds.Top < workingArea.Top)
                        bounds.Offset(0, workingArea.Top - bounds.Top);
                }
            }

            return bounds;
        }

        public delegate IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
 
        // ReSharper disable InconsistentNaming
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int GetDeviceCaps(HandleRef hDC, int nIndex);

        [SecurityCritical]
        public static IntPtr GetDC(HandleRef hWnd)
        {
            return IntGetDC(hWnd);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("user32.dll", EntryPoint = "GetDC", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr IntGetDC(HandleRef hWnd);

        [SecurityCritical]
        public static int ReleaseDC(HandleRef hWnd, HandleRef hDC)
        {
            return IntReleaseDC(hWnd, hDC);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("user32.dll", EntryPoint = "ReleaseDC", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern int IntReleaseDC(HandleRef hWnd, HandleRef hDC);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr MonitorFromRect(ref RECT rect, int flags);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        public static class CommonHandles
        {
            public static readonly int Accelerator = HandleCollector.RegisterType("Accelerator", 80, 50);
            public static readonly int Cursor = HandleCollector.RegisterType("Cursor", 20, 500);
            public static readonly int EMF = HandleCollector.RegisterType("EnhancedMetaFile", 20, 500);
            public static readonly int Find = HandleCollector.RegisterType("Find", 0, 1000);
            public static readonly int GDI = HandleCollector.RegisterType("GDI", 50, 500);
            public static readonly int HDC = HandleCollector.RegisterType("HDC", 100, 2);
            public static readonly int Icon = HandleCollector.RegisterType("Icon", 20, 500);
            public static readonly int Kernel = HandleCollector.RegisterType("Kernel", 0, 1000);
            public static readonly int Menu = HandleCollector.RegisterType("Menu", 30, 1000);
            public static readonly int Window = HandleCollector.RegisterType("Window", 5, 1000);
        }

        internal static class HandleCollector
        {
            private static object handleMutex = new object();
            private static int handleTypeCount = 0;
            private static HandleType[] handleTypes;

            internal static IntPtr Add(IntPtr handle, int type)
            {
                handleTypes[type - 1].Add();
                return handle;
            }

            internal static SafeHandle Add(SafeHandle handle, int type)
            {
                handleTypes[type - 1].Add();
                return handle;
            }

            internal static int RegisterType(string typeName, int expense, int initialThreshold)
            {
                lock (handleMutex)
                {
                    if ((handleTypeCount == 0) || (handleTypeCount == handleTypes.Length))
                    {
                        HandleType[] destinationArray = new HandleType[handleTypeCount + 10];
                        if (handleTypes != null)
                        {
                            Array.Copy(handleTypes, 0, destinationArray, 0, handleTypeCount);
                        }
                        handleTypes = destinationArray;
                    }
                    handleTypes[handleTypeCount++] = new HandleType(typeName, expense, initialThreshold);
                    return handleTypeCount;
                }
            }

            internal static IntPtr Remove(IntPtr handle, int type)
            {
                handleTypes[type - 1].Remove();
                return handle;
            }

            internal static SafeHandle Remove(SafeHandle handle, int type)
            {
                handleTypes[type - 1].Remove();
                return handle;
            }

            private class HandleType
            {
                private readonly int _initialThreshHold;
                private readonly int _deltaPercent;
                private int _handleCount;
                private readonly string _name;
                private int _threshold;

                internal HandleType(string name, int expense, int initialThreshHold)
                {
                    _name = name;
                    _initialThreshHold = initialThreshHold;
                    _threshold = initialThreshHold;
                    _deltaPercent = 100 - expense;
                }

                internal void Add()
                {
                    lock (this)
                    {
                        _handleCount++;
                        if (!NeedCollection())
                        {
                            return;
                        }
                    }
                    
                    GC.Collect();
                    var millisecondsTimeout = (100 - _deltaPercent) / 4;
                    Thread.Sleep(millisecondsTimeout);
                }

                internal bool NeedCollection()
                {
                    if (_handleCount > _threshold)
                    {
                        _threshold = _handleCount + ((_handleCount * _deltaPercent) / 100);
                        return true;
                    }
                    int num = (100 * _threshold) / (100 + _deltaPercent);
                    if ((num >= _initialThreshHold) && (_handleCount < ((int)(num * 0.9f))))
                    {
                        _threshold = num;
                    }
                    return false;
                }

                internal void Remove()
                {
                    lock (this)
                    {
                        _handleCount--;
                        _handleCount = Math.Max(0, _handleCount);
                    }
                }
            }
        }
    }
}