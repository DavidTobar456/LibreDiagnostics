/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using BlackSharp.MVVM.ComponentModel;
using DiskInfoToolkit;
using LibreHardwareMonitor.Hardware.Storage;

namespace LibreDiagnostics.MVVM.ViewModels
{
    public partial class StorageViewModel : ViewModelBase
    {
        #region Constructor

        protected StorageViewModel(object o)
        {
            //Design time
        }

        public StorageViewModel()
        {
            //Run time
        }

        #endregion

        #region Properties

        StorageDevice _StorageDevice;
        public StorageDevice StorageDevice
        {
            get { return _StorageDevice; }
            set { SetField(ref _StorageDevice, value); Storage = _StorageDevice.Storage; }
        }

        Storage _Storage;
        public Storage Storage
        {
            get { return _Storage; }
            set { SetField(ref _Storage, value); }
        }

        #endregion
    }

    public class MockStorageViewModel : StorageViewModel
    {
        public MockStorageViewModel() : base(null) { }
    }
}
