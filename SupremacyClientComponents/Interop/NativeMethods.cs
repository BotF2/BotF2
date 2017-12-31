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
        #region Structures
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

        [StructLayout(LayoutKind.Sequential)]
        public class BITMAP
        {
            public int bmType;
            public int bmWidth;
            public int bmHeight;
            public int bmWidthBytes;
            public short bmPlanes;
            public short bmBitsPixel;
            public int bmBits;
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

        [StructLayout(LayoutKind.Sequential)]
        internal class ICONINFO
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public BitmapHandle hbmMask;
            public BitmapHandle hbmColor;
        }
        #endregion

        #region Kernel

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("kernel32", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern void SetLastError(int dwErrorCode);

        #endregion

        #region User

        #region Enums

        public enum WindowLongValue
        {
            WndProc = -4,
            HInstace = -6,
            HwndParent = -8,
            Style = -16,
            ExtendedStyle = -20,
            UserData = -21,
            ID = -12,
        }

        [Flags]
        public enum WindowStyles
        {
            SysMemu = 0x80000,
            MinimizeBox = 0x20000,
            MaximizeBox = 0x10000,
            ThickFrame = 0x40000,
        }

        [Flags]
        public enum WindowExStyles
        {
            DlgModalFrame = 0x1,
        }

        public enum WindowMessage
        {
            Destroy = 0x2,
            Close = 0x10,
            SetIcon = 0x80,
            MeasureItem = 0x2c,
            MouseMove = 0x200,
            MouseDown = 0x201,
            LButtonUp = 0x0202,
            LButtonDblClk = 0x0203,
            RButtonDown = 0x0204,
            RButtonUp = 0x0205,
            RButtonDblClk = 0x0206,
            MButtonDown = 0x0207,
            MButtonUp = 0x0208,
            MButtonDblClk = 0x0209,
            TrayMouseMessage = 0x800,
        }

        public enum NotifyIconMessage
        {
            BalloonShow = 0x402,
            BalloonHide = 0x403,
            BalloonTimeout = 0x404,
            BalloonUserClick = 0x405,
            PopupOpen = 0x406,
            PopupClose = 0x407,
        }

        public enum SystemMenu
        {
            Size = 0xF000,
            Close = 0xF060,
            Restore = 0xF120,
            Minimize = 0xF020,
            Maximize = 0xF030,
        }

        #endregion

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(HandleRef hWnd, WindowMessage msg, IntPtr wParam, IntPtr lParam);

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("user32", CharSet = CharSet.Auto)]
        public static extern bool PostMessage(HandleRef hwnd, WindowMessage msg, IntPtr wparam, IntPtr lparam);

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetForegroundWindow(HandleRef hWnd);

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("user32", CharSet = CharSet.Auto)]
        public static extern int RegisterWindowMessage(string msg);

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("user32", CharSet = CharSet.Auto)]
        public static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll")]
        public extern static IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public extern static uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentProcessId();

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("user32", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int IntGetWindowLong(HandleRef hWnd, int nIndex);

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("user32", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr IntGetWindowLongPtr(HandleRef hWnd, int nIndex);

        [DllImport("user32", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int IntSetWindowLong(HandleRef hWnd, int nIndex, int dwNewLong);

        [DllImport("user32", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(HandleRef hWnd, int nIndex, IntPtr dwNewLong);

        [SecurityCritical]
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public static int GetWindowLong(HandleRef hWnd, WindowLongValue nIndex)
        {
            int result;
            SetLastError(0);
            if (IntPtr.Size == 4)
            {
                result = IntGetWindowLong(hWnd, (int)nIndex);
                Marshal.GetLastWin32Error();
            }
            else
            {
                IntPtr resultPtr = IntGetWindowLongPtr(hWnd, (int)nIndex);
                Marshal.GetLastWin32Error();
                result = IntPtrToInt32(resultPtr);
            }
            return result;
        }

        [SecurityCritical, SecuritySafeCritical]
        public static IntPtr SetWindowLong(HandleRef hWnd, WindowLongValue nIndex, IntPtr dwNewLong)
        {
            IntPtr result;
            SetLastError(0);
            if (IntPtr.Size == 4)
            {
                int intResult = IntSetWindowLong(hWnd, (int)nIndex, IntPtrToInt32(dwNewLong));
                Marshal.GetLastWin32Error();
                result = new IntPtr(intResult);
            }
            else
            {
                result = IntSetWindowLongPtr(hWnd, (int)nIndex, dwNewLong);
                Marshal.GetLastWin32Error();
            }
            return result;
        }

        [SecurityCritical, DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern bool EnableMenuItem(HandleRef hMenu, SystemMenu UIDEnabledItem, int uEnable);

        [SecurityCritical, DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetSystemMenu(HandleRef hWnd, bool bRevert);

        [SecurityCritical]
        public static void SetSystemMenuItems(HandleRef hwnd, bool isEnabled, params SystemMenu[] menus)
        {
            if ((menus == null) || (menus.Length == 0))
                return;
            var menuHandle = new HandleRef(null, GetSystemMenu(hwnd, false));
            foreach (var menu in menus)
                SetMenuItem(menuHandle, menu, isEnabled);
        }

        [SecurityCritical]
        public static void SetMenuItem(HandleRef hMenu, SystemMenu menu, bool isEnabled)
        {
            EnableMenuItem(hMenu, menu, (isEnabled) ? ~1 : 1);
        }

        #endregion

        #region Shell

        #region Structures and Enums

        [StructLayout(LayoutKind.Sequential)]
        public struct BROWSEINFO
        {
            /// <summary>
            /// Handle to the owner window for the dialog box.
            /// </summary>
            public IntPtr HwndOwner;

            /// <summary>
            /// Pointer to an item identifier list (PIDL) specifying the 
            /// location of the root folder from which to start browsing.
            /// </summary>
            public IntPtr Root;

            /// <summary>
            /// Address of a buffer to receive the display name of the
            /// folder selected by the user.
            /// </summary>
            [MarshalAs(UnmanagedType.LPStr)]
            public string DisplayName;

            /// <summary>
            /// Address of a null-terminated string that is displayed 
            /// above the tree view control in the dialog box.
            /// </summary>
            [MarshalAs(UnmanagedType.LPStr)]
            public string Title;

            /// <summary>
            /// Flags specifying the options for the dialog box.
            /// </summary>
            public uint Flags;

            /// <summary>
            /// Address of an application-defined function that the
            /// dialog box calls when an event occurs.
            /// </summary>
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public WndProc Callback;

            /// <summary>
            /// Application-defined value that the dialog box passes to 
            /// the callback function
            /// </summary>
            public int LParam;

            /// <summary>
            /// Variable to receive the image associated with the selected folder.
            /// </summary>
            public int Image;
        }

        [Flags]
        public enum FolderBrowserOptions
        {
            /// <summary>
            /// None.
            /// </summary>
            None = 0,
            /// <summary>
            /// For finding a folder to start document searching
            /// </summary>
            FolderOnly = 0x0001,
            /// <summary>
            /// For starting the Find Computer
            /// </summary>
            FindComputer = 0x0002,
            /// <summary>
            /// Top of the dialog has 2 lines of text for BROWSEINFO.lpszTitle and 
            /// one line if this flag is set.  Passing the message 
            /// BFFM_SETSTATUSTEXTA to the hwnd can set the rest of the text.  
            /// This is not used with BIF_USENEWUI and BROWSEINFO.lpszTitle gets
            /// all three lines of text.
            /// </summary>
            ShowStatusText = 0x0004,
            ReturnAncestors = 0x0008,
            /// <summary>
            /// Add an editbox to the dialog
            /// </summary>
            ShowEditBox = 0x0010,
            /// <summary>
            /// insist on valid result (or CANCEL)
            /// </summary>
            ValidateResult = 0x0020,
            /// <summary>
            /// Use the new dialog layout with the ability to resize
            /// Caller needs to call OleInitialize() before using this API
            /// </summary>
            UseNewStyle = 0x0040,
            UseNewStyleWithEditBox = (UseNewStyle | ShowEditBox),
            /// <summary>
            /// Allow URLs to be displayed or entered. (Requires BIF_USENEWUI)
            /// </summary>
            AllowUrls = 0x0080,
            /// <summary>
            /// Add a UA hint to the dialog, in place of the edit box. May not be
            /// combined with BIF_EDITBOX.
            /// </summary>
            ShowUsageHint = 0x0100,
            /// <summary>
            /// Do not add the "New Folder" button to the dialog.  Only applicable 
            /// with BIF_NEWDIALOGSTYLE.
            /// </summary>
            HideNewFolderButton = 0x0200,
            /// <summary>
            /// don't traverse target as shortcut
            /// </summary>
            GetShortcuts = 0x0400,
            /// <summary>
            /// Browsing for Computers.
            /// </summary>
            BrowseComputers = 0x1000,
            /// <summary>
            /// Browsing for Printers.
            /// </summary>
            BrowsePrinters = 0x2000,
            /// <summary>
            /// Browsing for Everything
            /// </summary>
            BrowseFiles = 0x4000,
            /// <summary>
            /// sharable resources displayed (remote shares, requires BIF_USENEWUI)
            /// </summary>
            BrowseShares = 0x8000
        }

        #endregion

        #region Notify Icon

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

        [SecurityCritical, DllImport("shell32", CharSet = CharSet.Auto)]
        public static extern int Shell_NotifyIcon(int message, NOTIFYICONDATA pnid);

        #endregion

        #region Malloc

        [ComImport, SuppressUnmanagedCodeSecurity, Guid("00000002-0000-0000-c000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IMalloc
        {
            [PreserveSig]
            IntPtr Alloc(int cb);
            [PreserveSig]
            IntPtr Realloc(IntPtr pv, int cb);
            [PreserveSig]
            void Free(IntPtr pv);
            [PreserveSig]
            int GetSize(IntPtr pv);
            [PreserveSig]
            int DidAlloc(IntPtr pv);
            [PreserveSig]
            void HeapMinimize();
        }

        [SecurityCritical]
        public static IMalloc GetSHMalloc()
        {
            var ppMalloc = new IMalloc[1];
            SHGetMalloc(ppMalloc);
            return ppMalloc[0];
        }

        [SecurityCritical, DllImport("shell32")]
        private static extern int SHGetMalloc([Out, MarshalAs(UnmanagedType.LPArray)] IMalloc[] ppMalloc);

        #endregion

        #region Folders

        [SecurityCritical, DllImport("shell32")]
        public static extern int SHGetFolderLocation(IntPtr hwndOwner, Int32 nFolder, IntPtr hToken, uint dwReserved, out IntPtr ppidl);

        [SecurityCritical, DllImport("shell32")]
        public static extern int SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)]string pszName, IntPtr pbc, out IntPtr ppidl, uint sfgaoIn, out uint psfgaoOut);

        [SecurityCritical, DllImport("shell32")]
        public static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lbpi);

        [SecurityCritical, DllImport("shell32", CharSet = CharSet.Auto)]
        public static extern bool SHGetPathFromIDList(IntPtr pidl, IntPtr pszPath);

        #endregion

        #endregion

        #region Helpers

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

        public static int IntPtrToInt32(IntPtr intPtr)
        {
            return (int)intPtr.ToInt64();
        }

        public delegate IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        #endregion

        [DllImport("kernel32.dll", SetLastError = true,
        ThrowOnUnmappableChar = true, BestFitMapping = false)]
        internal static extern IntPtr LoadLibrary(
             [MarshalAs(UnmanagedType.LPStr)] string lpFileName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int LoadString(IntPtr hInstance,
            int uID,
            StringBuilder buffer,
            int nBufferMax);

        [StructLayout(LayoutKind.Sequential)]
        internal class ICONINFO_IMPL
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask = IntPtr.Zero;
            public IntPtr hbmColor = IntPtr.Zero;
        }

        
 
        

        /// <summary>
        /// Get a string resource given a resource Id
        /// </summary>
        /// <param name="resourceId">The resource Id</param>
        /// <returns>The string resource corresponding to the given resource Id</returns>
        public static string GetStringResource(string resourceId)
        {
            string[] parts;
            string library;
            int index;

            if (String.IsNullOrEmpty(resourceId))
            {
                return String.Empty;
            }
            // Known folder "Recent" has a malformed resource id
            // for its tooltip. This causes the resource id to
            // parse into 3 parts instead of 2 parts if we don't fix.
            resourceId = resourceId.Replace("shell32,dll", "shell32.dll");
            parts = resourceId.Split(new[] { ',' });

            library = parts[0];
            library = library.Replace(@"@", String.Empty);

            parts[1] = parts[1].Replace("-", String.Empty);
            index = Int32.Parse(parts[1]);

            library = Environment.ExpandEnvironmentVariables(library);
            var handle = LoadLibrary(library);
            var stringValue = new StringBuilder(255);
            var retval = LoadString(
                handle, index, stringValue, 255);

            if (retval == 0)
            {
                var error = Marshal.GetLastWin32Error();
                throw new Win32Exception(error);
            }
            return stringValue.ToString();
        }

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