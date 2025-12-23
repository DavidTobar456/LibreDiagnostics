/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using Avalonia.Controls;
using LibreDiagnostics.Models.Configuration;
using System.Windows.Input;

namespace LibreDiagnostics.UI.Platform.Interfaces
{
    public interface IHotKeyManager
    {
        void EnableHotKeyHandling(TopLevel topLevel);
        void DisableHotKeyHandling(TopLevel topLevel);

        void RegisterHotKey(HotKey hotKey, ICommand command);
        void UnregisterHotKey(HotKey hotKey);

        void ClearHotKeys();
    }
}
