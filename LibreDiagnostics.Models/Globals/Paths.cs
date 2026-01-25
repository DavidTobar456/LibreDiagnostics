/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using System.Diagnostics;
using System.Reflection;

namespace LibreDiagnostics.Models.Globals
{
    public static class Paths
    {
        #region Fields

        const string _SettingsFilename = "Settings.json";

#if DEBUG
        const string _SettingsFilenameDebug = "Settings.Debug.json";
#endif

        #endregion

        #region Properties

        static string _AppDataLocal;
        public static string AppDataLocal
        {
            get
            {
                if (_AppDataLocal == null)
                {
                    _AppDataLocal = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetEntryAssembly().GetName().Name);
                }

                return _AppDataLocal;
            }
        }

        static string _SettingsFile;
        public static string SettingsFile
        {
            get
            {
                if (_SettingsFile == null)
                {
#if DEBUG
                    if (Debugger.IsAttached)
                    {
                        _SettingsFile = Path.Combine(AppDataLocal, _SettingsFilenameDebug);
                    }
                    else
#endif
                    {
                        _SettingsFile = Path.Combine(AppDataLocal, _SettingsFilename);
                    }
                }

                return _SettingsFile;
            }
        }

        #endregion
    }
}
