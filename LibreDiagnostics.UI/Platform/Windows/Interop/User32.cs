/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using System.Runtime.InteropServices;

namespace LibreDiagnostics.UI.Platform.Windows.Interop
{
    internal static class User32
    {
        const string DLL_NAME = "user32.dll";

        public const int GWL_EXSTYLE = -20;

        public const long WS_EX_TRANSPARENT = 0x00000020L;
        public const long WS_EX_TOOLWINDOW  = 0x00000080L;
        public const long WS_EX_LAYERED     = 0x00080000L;

        public const uint SWP_NOSIZE     = 0x0001;
        public const uint SWP_NOMOVE     = 0x0002;
        public const uint SWP_NOACTIVATE = 0x0010;

        public static readonly IntPtr HWND_BOTTOM    = new IntPtr( 1);
        public static readonly IntPtr HWND_TOPMOST   = new IntPtr(-1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        [DllImport(DLL_NAME)]
        public static extern long GetWindowLong(IntPtr hwnd, int index);

        [DllImport(DLL_NAME)]
        public static extern long GetWindowLongPtr(IntPtr hwnd, int index);

        [DllImport(DLL_NAME)]
        public static extern long SetWindowLong(IntPtr hwnd, int index, long newStyle);

        [DllImport(DLL_NAME)]
        public static extern long SetWindowLongPtr(IntPtr hwnd, int index, long newStyle);

        [DllImport(DLL_NAME)]
        public static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwnd_after, int x, int y, int cx, int cy, uint uflags);

        [DllImport(DLL_NAME)]
        public static extern int RegisterWindowMessage(string msg);

        [DllImport(DLL_NAME)]
        public static extern bool RegisterHotKey(IntPtr hwnd, int id, uint fsModifiers, uint vk);

        [DllImport(DLL_NAME)]
        public static extern bool UnregisterHotKey(IntPtr hwnd, int id);
    }
}
