/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using BlackSharp.Core.Collections;
using BlackSharp.MVVM.ComponentModel;
using LibreDiagnostics.Models.Enums;
using LibreDiagnostics.Models.Interfaces;

namespace LibreDiagnostics.Models.Hardware
{
    public class HardwarePanel : ViewModelBase, IIcon
    {
        #region Constructor

        public HardwarePanel(HardwareMonitorType hardwareMonitorType, string iconData, string title, IEnumerable<IHardwareMonitor> hardwareMonitors)
        {
            HardwareMonitorType = hardwareMonitorType;
            IconData = iconData;
            Title = title;
            Hardware = new ObservableCollectionEx<IHardwareMonitor>(hardwareMonitors);
        }

        #endregion

        #region Properties

        HardwareMonitorType _HardwareMonitorType;
        public HardwareMonitorType HardwareMonitorType
        {
            get { return _HardwareMonitorType; }
            set { SetField(ref _HardwareMonitorType, value); }
        }

        string _IconData;
        public string IconData
        {
            get { return _IconData; }
            set { SetField(ref _IconData, value); }
        }

        string _Title;
        public string Title
        {
            get { return _Title; }
            set { SetField(ref _Title, value); }
        }

        ObservableCollectionEx<IHardwareMonitor> _Hardware;
        public ObservableCollectionEx<IHardwareMonitor> Hardware
        {
            get { return _Hardware; }
            set { SetField(ref _Hardware, value); }
        }

        #endregion
    }
}
