/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

namespace LibreDiagnostics.Updater
{
    internal sealed class Program
    {
        #region Example

        /*
args = new string[3] { "--calling-app=\"C:/Code/LibreDiagnostics/LibreDiagnostics.Updater/bin/Debug/net8.0/LibreDiagnostics.exe\"", "--start-self-update", "--source-directory=\"C:/Code/LibreDiagnostics/LibreDiagnostics.Updater/bin/Debug/net8.0\"" };

"--calling-app=\"C:/Code/LibreDiagnostics/LibreDiagnostics.Updater/bin/Debug/net8.0/LibreDiagnostics.exe\""
"--start-self-update"
"--source-directory=\"C:/Code/LibreDiagnostics/LibreDiagnostics.Updater/bin/Debug/net8.0\""
         */

        #endregion

        public static void Main(string[] args)
        {
            Client.Start(args?.ToList() ?? new());
        }

        // For Avalonia Designer; do not remove, even if it shows "unused"
        static object BuildAvaloniaApp() => App.BuildAvaloniaApp();
    }
}
