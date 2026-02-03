/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using BlackSharp.MVVM.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibreDiagnostics.Models.Configuration;
using LibreDiagnostics.Models.Globals;
using LibreDiagnostics.Models.Hardware;
using LibreDiagnostics.Models.Interfaces;
using LibreDiagnostics.Models.Software;
using LibreDiagnostics.MVVM.Utilities;
using LibreDiagnostics.Tasks.Platform.Windows;
using System.Diagnostics;
using OS = BlackSharp.Core.Platform.OperatingSystem;

namespace LibreDiagnostics.MVVM.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase, IHardwareMonitorManager
    {
        #region Constructor

        protected MainWindowViewModel(object o)
        {
            //Design time
        }

        public MainWindowViewModel()
        {
            //Run time

            Clock = new Clock()
            {
                IconData = IconData.Clock
            };

            Global.HardwareMonitorManager = this;

            ApplySettings(this, new(Global.Settings));

            Global.SettingsChanged += ApplySettings;

            //Start monitoring
            StartMonitoring();

            MessageBro.DoCheckForUpdate();
        }

        #endregion

        #region Fields

        Thread HardwareUpdateThread;
        CancellationTokenSource HardwareUpdateThreadCancellation = new();

        #endregion

        #region Properties

        Clock _Clock;
        public Clock Clock
        {
            get { return _Clock; }
            set { SetField(ref _Clock, value); }
        }

        HardwareManager _HardwareManager;
        public HardwareManager HardwareManager
        {
            get { return _HardwareManager; }
            set { SetField(ref _HardwareManager, value); }
        }

        bool _ShowMachineName;
        public bool ShowMachineName
        {
            get { return _ShowMachineName; }
            set { SetField(ref _ShowMachineName, value); }
        }

        string _MachineName;
        public string MachineName
        {
            get { return _MachineName; }
            set { SetField(ref _MachineName, value); }
        }

        bool _ShowClock;
        public bool ShowClock
        {
            get { return _ShowClock; }
            set { SetField(ref _ShowClock, value); }
        }

        string _Time;
        public string Time
        {
            get { return _Time; }
            set { SetField(ref _Time, value); }
        }

        bool _ShowDate;
        public bool ShowDate
        {
            get { return _ShowDate; }
            set { SetField(ref _ShowDate, value); }
        }

        string _Date;
        public string Date
        {
            get { return _Date; }
            set { SetField(ref _Date, value); }
        }

        bool _IsReady;
        public bool IsReady
        {
            get { return _IsReady; }
            set { SetField(ref _IsReady, value); }
        }

        #endregion

        #region Public

        public void Start()
        {
            Stop();

            HardwareUpdateThreadCancellation.TryReset();

            HardwareUpdateThread = new(HardwareUpdateLoop);
            HardwareUpdateThread.Name = $"{nameof(MainWindowViewModel)}.{nameof(HardwareUpdateLoop)}";
            HardwareUpdateThread.IsBackground = true;
            HardwareUpdateThread.Start();
        }

        public void Stop()
        {
            if (HardwareUpdateThread == null)
            {
                return;
            }

            HardwareUpdateThreadCancellation.Cancel();

            HardwareUpdateThread.Join();
            HardwareUpdateThread = null;
        }

        #endregion

        #region Private

        void StartMonitoring()
        {
            IsReady = false;

            Stop();

            if (HardwareManager != null)
            {
                HardwareManager.Dispose();
                Global.HardwareManager = HardwareManager = null;
            }

            Global.HardwareManager = HardwareManager = new HardwareManager();
            HardwareManager.Update();

            Start();

            IsReady = true;
        }

        void HardwareUpdateLoop()
        {
            DateTime lastUpdate;

            while (!HardwareUpdateThreadCancellation.IsCancellationRequested)
            {
                TimeSpan updateInterval = TimeSpan.FromMilliseconds(Global.Settings.UpdateInterval);

                lastUpdate = DateTime.Now;
                Clock.Update();

                HardwareManager.Update();

                var timeItTookForUpdating = DateTime.Now - lastUpdate;

                var nextUpdateTime = updateInterval - timeItTookForUpdating;

                if (nextUpdateTime.Ticks <= 0)
                {
                    continue;
                }

                Thread.Sleep(nextUpdateTime);
            }
        }

        void ApplySettings(object sender, SettingsChangedEventArgs e)
        {
            if (OS.IsWindows())
            {
                //Do not modify startup task while debugging
                if (!Debugger.IsAttached)
                {
                    if (Global.Settings.StartWithSystem && !WindowsStartup.StartupTaskExists())
                    {
                        WindowsStartup.EnableStartupTask();
                    }
                    else if (!Global.Settings.StartWithSystem && WindowsStartup.StartupTaskExists())
                    {
                        WindowsStartup.DisableStartupTask();
                    }
                }
            }

            if (Global.Settings.HardwareMonitorConfigs == null ||
                Global.Settings.HardwareMonitorConfigs.IsEmpty())
            {
                Global.Settings.HardwareMonitorConfigs = HardwareMonitorConfig.Default;
            }
            else
            {
                HardwareMonitorConfig.NormalizeConfig(Global.Settings.HardwareMonitorConfigs);
            }
        }

        #endregion

        #region Commands

        [RelayCommand]
        void ShowSettings()
        {
            MessageBro.DoOpenSettings();
        }

        [RelayCommand]
        void Close()
        {
            MessageBro.DoShutdownApplication();
        }

        #endregion
    }

    public class MockMainWindowViewModel : MainWindowViewModel
    {
        public MockMainWindowViewModel() : base(null) { }
    }
}
