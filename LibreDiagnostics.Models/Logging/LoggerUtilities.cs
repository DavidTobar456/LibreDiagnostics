/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using BlackSharp.Core.Logging;

namespace LibreDiagnostics.Models.Logging
{
    public class LoggerUtilities
    {
        #region Fields

        public const string LogFile = $"AppLog{nameof(LibreDiagnostics)}.txt";

        #endregion

        #region Public

        public static void SaveLogFile()
        {
            Logger.Instance.SaveToFile(LogFile);
        }

        #endregion
    }
}
