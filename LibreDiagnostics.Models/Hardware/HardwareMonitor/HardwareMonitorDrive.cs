/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

#pragma warning disable CA1416 // Platform compatibility warning

using BlackSharp.Core.Converters;
using BlackSharp.Core.Extensions;
using DiskInfoToolkit;
using LibreDiagnostics.Models.Configuration;
using LibreDiagnostics.Models.Enums;
using LibreDiagnostics.Models.Events;
using LibreDiagnostics.Models.Globals;
using LibreDiagnostics.Models.Hardware.Metrics;
using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.Storage;

namespace LibreDiagnostics.Models.Hardware.HardwareMonitor
{
    public class HardwareMonitorDrive : HardwareMonitorBoardItem
    {
        #region Constructor

        public HardwareMonitorDrive(IHardware hardware, HardwareConfig config)
            : base(hardware, config)
        {
            //Get underlying Storage object via reflection
            var sd = Hardware as StorageDevice;
            _Storage = sd.Storage;

            Initialize();

            Global.SettingsChanged += OnSettingsChanged;

            //Initial call to apply settings
            OnSettingsChanged(this, new(Global.Settings));
        }

        #endregion

        #region Fields

        readonly Storage _Storage;

        #endregion

        #region Properties

        MetricBase _LoadMetric;
        public MetricBase LoadMetric
        {
            get { return _LoadMetric; }
            set { SetField(ref _LoadMetric, value); }
        }

        MetricBase _UsedMetric;
        public MetricBase UsedMetric
        {
            get { return _UsedMetric; }
            set { SetField(ref _UsedMetric, value); }
        }

        MetricBase _FreeMetric;
        public MetricBase FreeMetric
        {
            get { return _FreeMetric; }
            set { SetField(ref _FreeMetric, value); }
        }

        DriveLayout _DriveLayout;
        public DriveLayout DriveLayout
        {
            get { return _DriveLayout; }
            set { SetField(ref _DriveLayout, value); }
        }

        #endregion

        #region Public

        public override void Update()
        {
            if (Hardware is StorageDevice sd)
            {
                var driveLettersForStorageDevice = _Storage.Partitions
                                                        .Where(p => p.DriveLetter is not null)
                                                        .OrderBy(p => p.DriveLetter)
                                                        .Select(p => $"{p.DriveLetter.Value}:")
                                                        .ToList();

                if (driveLettersForStorageDevice.Count > 0)
                {
                    Name = StringExtensions.Join(", ", driveLettersForStorageDevice);
                }
                else
                {
                    Name = Hardware.Name;
                }

                var freeBytes = _Storage.TotalFreeSize.GetValueOrDefault();
                var usedBytes = _Storage.TotalSize - freeBytes;
                float usedPercent = 0f;

                if (_Storage.TotalSize > 0)
                {
                    usedPercent = 100.0f - (100.0f * freeBytes / _Storage.TotalSize);
                }
                else
                {
                    usedPercent = 100.0f;
                }

                if (LoadMetric != null)
                {
                    LoadMetric.Update(usedPercent);
                }

                if (UsedMetric != null)
                {
                    UsedMetric.Update((double)DataStorageSizeConverter.ByteToGigabyte(usedBytes));
                }

                if (FreeMetric != null)
                {
                    FreeMetric.Update((double)DataStorageSizeConverter.ByteToGigabyte(freeBytes));
                }
            }

            base.Update();
        }

        #endregion

        #region Protected

        protected override void OnRequestHardwareDetails()
        {
            EventDistributor.ShowDriveDetails(Hardware as StorageDevice);
        }

        #endregion

        #region Private

        void Initialize()
        {
            var hardwareMetricList = new List<MetricBase>();

            //Drive Load
            {
                LoadMetric = new MetricBase(HardwareMetricKey.DriveLoad, DataType.Percent);
                hardwareMetricList.Add(LoadMetric);
            }

            //Drive Used
            {
                UsedMetric = new MetricBase(HardwareMetricKey.DriveUsed, DataType.Gigabyte);
                hardwareMetricList.Add(UsedMetric);
            }

            //Drive Free
            {
                FreeMetric = new MetricBase(HardwareMetricKey.DriveFree, DataType.Gigabyte);
                hardwareMetricList.Add(FreeMetric);
            }

            //Drive Read
            {
                var readActivity = Hardware.Sensors.Where(s => s.SensorType == SensorType.Load && s.Index == 51).FirstOrDefault();
                if (readActivity != null)
                {
                    hardwareMetricList.Add(new MetricBoardItem(readActivity, HardwareMetricKey.DriveRead, DataType.kBps));
                }
            }

            //Drive Write
            {
                var writeActivity = Hardware.Sensors.Where(s => s.SensorType == SensorType.Load && s.Index == 52).FirstOrDefault();
                if (writeActivity != null)
                {
                    hardwareMetricList.Add(new MetricBoardItem(writeActivity, HardwareMetricKey.DriveWrite, DataType.kBps));
                }
            }

            //Drive Temp
            {
                ISensor tempSensor = Hardware.Sensors.Where(s => s.SensorType == SensorType.Temperature).FirstOrDefault();

                if (tempSensor != null)
                {
                    hardwareMetricList.Add(new MetricBoardItem(tempSensor, HardwareMetricKey.DriveTemp, DataType.Celcius));
                }
            }

            HardwareMetrics.Clear();
            HardwareMetrics.AddRange(hardwareMetricList);
        }

        void OnSettingsChanged(object sender, SettingsChangedEventArgs e)
        {
            if (e.NewSettings == null)
            {
                return;
            }

            ShowName = e.NewSettings.GetHardwareConfigOptionValue<bool>(HardwareMonitorType.Storage, HardwareConfigOption.HardwareNames);

            SharedMethods.SetRoundAll     (this, HardwareMonitorType.Storage, e.NewSettings);
            SharedMethods.SetUseFahrenheit(this, HardwareMonitorType.Storage, e.NewSettings);
            SharedMethods.SetTempAlert    (this, HardwareMonitorType.Storage, e.NewSettings);

            long throttleInterval = e.NewSettings.GetHardwareConfigOptionValue<long>(HardwareMonitorType.Storage, HardwareConfigOption.ThrottleInterval);
            if (throttleInterval > 0)
            {
                //Enable
                StorageDevice.ThrottleInterval = TimeSpan.FromMilliseconds(throttleInterval);
            }
            else
            {
                //Disable
                StorageDevice.ThrottleInterval = TimeSpan.FromMilliseconds(0);
            }

            short usedSpaceAlert = e.NewSettings.GetHardwareConfigOptionValue<short>(HardwareMonitorType.Storage, HardwareConfigOption.UsedSpaceAlert);

            HardwareMetrics.ForEach(hm =>
            {
                if (hm is MetricBase mb
                 && mb.HardwareMetricKey == HardwareMetricKey.DriveLoad)
                {
                    mb.AlertValue = usedSpaceAlert;
                }
            });

            //Set Load Bar layout
            bool loadBarEnabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.Storage, HardwareMetricKey.DriveLoadBar);
            if (loadBarEnabled)
            {
                DriveLayout = DriveLayout.LoadBarStacked;
            }
            else
            {
                DriveLayout = DriveLayout.NoLoadBar;
            }

            //Set DriveLoad
            var load = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.DriveLoad);
            load?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.Storage, HardwareMetricKey.DriveLoad);

            //Set DriveUsed
            var used = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.DriveUsed);
            used?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.Storage, HardwareMetricKey.DriveUsed);

            //Set DriveFree
            var free = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.DriveFree);
            free?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.Storage, HardwareMetricKey.DriveFree);

            //Set DriveRead
            var read = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.DriveRead);
            read?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.Storage, HardwareMetricKey.DriveRead);

            //Set DriveWrite
            var write = HardwareMetrics.Find(hm => hm.HardwareMetricKey == HardwareMetricKey.DriveWrite);
            write?.Enabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.Storage, HardwareMetricKey.DriveWrite);

            //Set DriveTemp
            var temps = HardwareMetrics.Where(hm => hm.HardwareMetricKey == HardwareMetricKey.DriveTemp).ToList();
            bool tempsEnabled = e.NewSettings.IsConfigEnabled(HardwareMonitorType.Storage, HardwareMetricKey.DriveTemp);
            temps.ForEach(t => t.Enabled = tempsEnabled);
        }

        #endregion
    }
}

#pragma warning restore CA1416 // Platform compatibility warning
