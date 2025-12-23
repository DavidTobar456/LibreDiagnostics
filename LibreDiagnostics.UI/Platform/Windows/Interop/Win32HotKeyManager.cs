/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Win32.Input;
using LibreDiagnostics.Models.Configuration;
using LibreDiagnostics.Models.Enums;
using LibreDiagnostics.UI.Platform.Interfaces;
using System.Windows.Input;

namespace LibreDiagnostics.UI.Platform.Windows.Interop
{
    internal class Win32HotKeyManager : IHotKeyManager
    {
        #region Fields

        IntPtr _Hwnd;

        bool _AreHotKeysRegistered = false;

        readonly Dictionary<HotKeyID, Win32HotKey> _HotKeys = new();

        #endregion

        #region Public

        public void EnableHotKeyHandling(TopLevel topLevel)
        {
            if (_AreHotKeysRegistered)
            {
                return;
            }

            Win32Properties.AddWndProcHookCallback(topLevel, WndProc);
            _Hwnd = topLevel.TryGetPlatformHandle().Handle;

            _AreHotKeysRegistered = true;
        }

        public void DisableHotKeyHandling(TopLevel topLevel)
        {
            if (_AreHotKeysRegistered)
            {
                Win32Properties.RemoveWndProcHookCallback(topLevel, WndProc);
                _Hwnd = IntPtr.Zero;

                _AreHotKeysRegistered = false;
            }
        }

        public void RegisterHotKey(HotKey hotKey, ICommand command)
        {
            //Already exists, update Hotkey if necessary
            if (_HotKeys.TryGetValue(hotKey.ID, out var val))
            {
                //Correct ID, check if something has changed
                if (!val.IsHotKeySame(hotKey))
                {
                    //HotKey has changed
                    //We must unregister the old HotKey first, according to MSDN docs
                    User32.UnregisterHotKey(_Hwnd, (int)hotKey.ID);

                    //Create Win32 translation
                    var win32HotKey = new Win32HotKey(hotKey, command);

                    //Register HotKey again, with the changes
                    if (User32.RegisterHotKey(_Hwnd, (int)hotKey.ID, win32HotKey.Modifiers, win32HotKey.Key))
                    {
                        _HotKeys[hotKey.ID] = win32HotKey;
                    }
                }
            }
            else //Register new HotKey
            {
                //Create Win32 translation
                var win32HotKey = new Win32HotKey(hotKey, command);

                //Register HotKey
                if (User32.RegisterHotKey(_Hwnd, (int)hotKey.ID, win32HotKey.Modifiers, win32HotKey.Key))
                {
                    _HotKeys.Add(hotKey.ID, win32HotKey);
                }
            }
        }

        public void UnregisterHotKey(HotKey hotKey)
        {
            //Check if HotKey is registered
            if (_HotKeys.TryGetValue(hotKey.ID, out var val))
            {
                //HotKey exists, remove it
                User32.UnregisterHotKey(_Hwnd, (int)hotKey.ID);

                //Also remove from internal list
                _HotKeys.Remove(hotKey.ID);
            }
        }

        public void ClearHotKeys()
        {
            //Unregister all HotKeys
            foreach (var hotKey in _HotKeys.Values)
            {
                User32.UnregisterHotKey(_Hwnd, (int)hotKey.HotKeyID);
            }
        }

        #endregion

        #region Private

        IntPtr WndProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == Shell32.WM_HOTKEY)
            {
                var hotKeyID = (HotKeyID)wParam.ToInt32();

                if (_HotKeys.TryGetValue(hotKeyID, out var hotKey))
                {
                    if (hotKey.Command.CanExecute(null))
                    {
                        hotKey.Command.Execute(null);
                    }

                    handled = true;
                }
            }

            return IntPtr.Zero;
        }

        #endregion

        #region Private Classes

        class Win32HotKey
        {
            #region Constructor

            public Win32HotKey(HotKey hotKey, ICommand command)
            {
                HotKeyID = hotKey.ID;

                //Modifiers are same in both Avalonia and Win32
                Modifiers = (uint)hotKey.Modifiers;
                Key = (uint)KeyInterop.VirtualKeyFromKey((Key)hotKey.Key);

                Command = command;
            }

            #endregion

            #region Properties

            public HotKeyID HotKeyID { get; }

            public uint Modifiers { get; }

            public uint Key { get; }

            public ICommand Command { get; }

            #endregion

            #region Public

            public bool IsHotKeySame(HotKey hotKey)
            {
                return HotKeyID == hotKey.ID
                    && Modifiers == (uint)hotKey.Modifiers
                    && Key == (uint)KeyInterop.VirtualKeyFromKey((Key)hotKey.Key);
            }

            #endregion
        }

        #endregion
    }
}
