/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using LibreHardwareMonitor.Hardware;
using LibreDiagnostics.Models.Enums;

namespace LibreDiagnostics.Models.Hardware.Metrics
{
    public class MetricCPU : MetricBoardItem
    {
        #region Constructor

        public MetricCPU(ISensor sensor, HardwareMetricKey key, DataType type)
            : base(sensor, key, type)
        {
            //Special; add numbering for cores
            switch (key)
            {
                case HardwareMetricKey.CPUClock:
                case HardwareMetricKey.CPUCoreLoad:
                    Label = string.Format(Label, sensor.Index);
                    break;
            }
        }

        #endregion
    }
}
