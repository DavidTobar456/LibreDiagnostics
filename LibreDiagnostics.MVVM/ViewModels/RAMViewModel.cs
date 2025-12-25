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
using LibreHardwareMonitor.Hardware;

namespace LibreDiagnostics.MVVM.ViewModels
{
    public partial class RAMViewModel : ViewModelBase
    {
        #region Constructor

        protected RAMViewModel(object o)
        {
            //Design time
        }

        public RAMViewModel()
        {
            //Run time
        }

        #endregion

        #region Fields

        IHardware _Hardware;

        #endregion

        #region Properties

        string _Name;
        public string Name
        {
            get { return _Name; }
            set { SetField(ref _Name, value); }
        }

        ISensor _Capacity;
        public ISensor Capacity
        {
            get { return _Capacity; }
            set { SetField(ref _Capacity, value); }
        }

        ObservableCollectionEx<ISensor> _Timings;
        public ObservableCollectionEx<ISensor> Timings
        {
            get { return _Timings; }
            set { SetField(ref _Timings, value); }
        }

        #endregion

        #region Public

        public void SetHardware(IHardware hardware)
        {
            _Hardware = hardware;

            Name = _Hardware.Name;

            Capacity = _Hardware.Sensors.Where(s => s.SensorType == SensorType.Data && s.Name.Contains("Capacity", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            Timings = new(_Hardware.Sensors.Where(s => s.SensorType == SensorType.Timing));
        }

        #endregion
    }

    public class MockRAMViewModel : RAMViewModel
    {
        public MockRAMViewModel() : base(null) { }
    }
}
