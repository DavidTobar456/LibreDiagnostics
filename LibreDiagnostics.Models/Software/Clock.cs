/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using BlackSharp.MVVM.ComponentModel;
using LibreDiagnostics.Models.Interfaces;

namespace LibreDiagnostics.Models.Software
{
    /// <summary>
    /// Clock model representing the current date and time.
    /// </summary>
    public class Clock : ViewModelBase, IIcon
    {
        #region Properties

        DateTime? _CurrentDateTime;
        public DateTime? CurrentDateTime
        {
            get { return _CurrentDateTime; }
            set { SetField(ref _CurrentDateTime, value); }
        }

        string _IconData;
        public string IconData
        {
            get { return _IconData; }
            set { SetField(ref _IconData, value); }
        }

        #endregion

        #region Public

        public void Update()
        {
            CurrentDateTime = DateTime.Now;
        }

        #endregion
    }
}
