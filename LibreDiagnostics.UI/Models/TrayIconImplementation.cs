/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Platform;
using BlackSharp.Core.Extensions;
using BlackSharp.Core.Logging;
using BlackSharp.MVVM.Dialogs.Enums;
using CommunityToolkit.Mvvm.Input;
using LibreDiagnostics.Language.Resources;
using LibreDiagnostics.Models.Configuration;
using LibreDiagnostics.Models.Enums;
using LibreDiagnostics.Models.Globals;
using LibreDiagnostics.Models.Interfaces;
using LibreDiagnostics.MVVM.Utilities;
using System.Diagnostics;
using System.Reflection;
using OS = BlackSharp.Core.Platform.OperatingSystem;

namespace LibreDiagnostics.UI.Models
{
    internal partial class TrayIconImplementation : ITrayIcon
    {
        #region Constructor

        public TrayIconImplementation(Settings settings)
        {
            var icon = GetTrayIcon();
            if (icon == null)
            {
                return;
            }

            var updateMenu = new NativeMenuItem
            {
                Header = Resources.ButtonUpdate,
                Command = UpdateRequestedCommand,
            };

            //Manually add menu
            icon.Menu = new NativeMenu
            {
                new NativeMenuItem
                {
                    Header = Resources.ButtonSettings,
                    Command = SettingsRequestedCommand,
                },
                new NativeMenuItem
                {
                    Header = Resources.ButtonLHMReport,
                    Command = LHMReportRequestedCommand,
                },
                new NativeMenuItemSeparator(),
                new NativeMenuItem
                {
                    Header = Resources.ButtonDonate,
                    Command = DonateRequestedCommand,
                },
                new NativeMenuItem
                {
                    Header = Resources.ButtonGithub,
                    Command = GithubRequestedCommand,
                },
                updateMenu,
                new NativeMenuItemSeparator(),
                new NativeMenuItem
                {
                    Header = Resources.ButtonRestart,
                    Command = RestartRequestedCommand,
                },
                new NativeMenuItem
                {
                    Header = Resources.ButtonClose,
                    Command = CloseRequestedCommand,
                },
            };

            //Add binding for visibility
            icon.ClearValue(TrayIcon.IsVisibleProperty);

            var binding = new Binding
            {
                Source = settings,
                Path = nameof(Settings.ShowTrayIcon),
                Mode = BindingMode.TwoWay
            };
            icon.Bind(TrayIcon.IsVisibleProperty, binding);

            //Add tooltip
            var version = Assembly.GetEntryAssembly()?.GetName().Version;
            icon.ToolTipText = $"{Resources.AppName} v{version.ToString(3)}";

            //Set icon
            if (!Global.IsUpdateAvailable)
            {
                ChangeTrayIconIcon(TrayIconID.Default);
            }
            else
            {
                ChangeTrayIconIcon(TrayIconID.UpdateAvailable);

                //Update menu to show an update is available
                updateMenu.Header = Resources.ButtonUpdateAvailable;
            }
        }

        #endregion

        #region Fields

        const string DefaultTrayIconIcon         = @"avares://LibreDiagnostics.UI/Assets/Icon.ico";
        const string UpdateAvailableTrayIconIcon = @"avares://LibreDiagnostics.UI/Assets/Icon_Update.ico";

        #endregion

        #region Public

        public void ChangeTrayIconIcon(TrayIconID trayIconID)
        {
            var icon = GetTrayIcon();
            if (icon == null)
            {
                return;
            }

            switch (trayIconID)
            {
                case TrayIconID.Default:
                    icon.Icon = LoadIcon(DefaultTrayIconIcon);
                    break;
                case TrayIconID.UpdateAvailable:
                    icon.Icon = LoadIcon(UpdateAvailableTrayIconIcon);
                    break;
            }
        }

        #endregion

        #region Private

        TrayIcon GetTrayIcon()
        {
            return Application.Current?.GetValue(TrayIcon.IconsProperty)?.FirstOrDefault();
        }

        WindowIcon LoadIcon(string resourceUri)
        {
            var uri = new Uri(resourceUri);
            using var stream = AssetLoader.Open(uri);
            return new WindowIcon(stream);
        }

        void OpenInBrowser(string url)
        {
            if (OS.IsWindows())
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (OS.IsLinux())
            {
                Process.Start("xdg-open", url);
            }
        }

        #endregion

        #region Commands

        [RelayCommand]
        void SettingsRequested()
        {
            MessageBro.DoOpenSettings();
        }

        [RelayCommand]
        async Task LHMReportRequested()
        {
            var report = Global.HardwareManager?.GetReport();

            var filePath = await MessageBro.DoSaveFile();

            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(report))
            {
                return;
            }

            File.WriteAllText(filePath, report);
        }

        [RelayCommand]
        void RestartRequested()
        {
            Process.Start(Environment.ProcessPath);
            MessageBro.DoShutdownApplication();
        }

        [RelayCommand]
        void DonateRequested()
        {
            OpenInBrowser(@"https://github.com/sponsors/Blacktempel");
        }

        [RelayCommand]
        void GithubRequested()
        {
            OpenInBrowser(@"https://github.com/Blacktempel/LibreDiagnostics");
        }

        [RelayCommand]
        async Task UpdateRequested()
        {
            var result = DialogButtonType.Invalid;

            do
            {
                try
                {
                    var updateCheckResult = await Client.CheckUpdateAvailable(false);

                    //Require confirmation
                    await Client.TryUpdate(updateCheckResult, true);
                }
                catch (Exception e)
                {
                    var fullExceptionString = e.FullExceptionString();

                    Logger.Instance.Add(LogLevel.Error, fullExceptionString, DateTime.Now);

                    //Format message to include exception string
                    var message = string.Format(Resources.UpdateFailedMessage.Replace(@"\n", Environment.NewLine), fullExceptionString);

                    result = MessageBro.DoShowMessage(Resources.UpdateFailedTitle, message, DialogButtons.RetryCancel);
                }
            }
            while (result == DialogButtonType.Retry);
        }

        [RelayCommand]
        void CloseRequested()
        {
            MessageBro.DoShutdownApplication();
        }

        #endregion
    }
}
