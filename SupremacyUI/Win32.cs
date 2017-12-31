// Win32.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Runtime.InteropServices;

namespace Supremacy.UI
{
	internal class Win32
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public int X;
			public int Y;

			public POINT(int x, int y)
			{
				X = x;
				Y = y;
			}
		}

		public static Int32 GWL_EXSTYLE = -20;
		public static Int32 WS_EX_LAYERED = 0x00080000;
		public static Int32 WS_EX_TRANSPARENT = 0x00000020;

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern bool GetCursorPos(out POINT pt);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern Int32 GetWindowLong(IntPtr hWnd, Int32 nIndex);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern Int32 SetWindowLong(IntPtr hWnd, Int32 nIndex, Int32 newVal);
	}
}