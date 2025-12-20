/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using BlackSharp.MVVM.Dialogs.Enums;
using BlackSharp.UI.Avalonia.Windows.Dialogs;
using BlackSharp.UI.Avalonia.Windows.Dialogs.Enums;
using LibreDiagnostics.Models.Events;
using LibreDiagnostics.Models.Helper;
using LibreDiagnostics.MVVM.Utilities;
using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.Storage;

namespace LibreDiagnostics.UI.Utilities
{
    internal static class NavigationResolver
    {
        #region Constructor

        static NavigationResolver()
        {
            MessageBro.ShowMessage += ShowMessage;
            MessageBro.ShowMessageTimeout += ShowMessageTimeout;
            MessageBro.OpenSettings += OpenSettings;
            MessageBro.ShutdownApplication += ShutdownApplication;
            MessageBro.GetScreens += GetScreens;

            EventDistributor.ShowDriveDetailsEvent += ShowDriveDetails;
        }

        #endregion

        #region Events

        static DialogButtonType ShowMessage(string title, string message, DialogButtons buttons)
        {
            var msgBox = new MessageBox(DialogType.Information, DialogSize.Medium);
            msgBox.ShowDialog(title, message, buttons);
            return msgBox.Result;
        }

        static DialogButtonType ShowMessageTimeout(string title, string message, DialogButtons buttons, TimeSpan? timeout, out bool timeouted)
        {
            var msgBox = new TimeoutMessageBox(timeout, DialogType.Information, DialogSize.Medium);
            msgBox.ShowDialog(title, message, buttons);

            timeouted = msgBox.Timeouted;
            return msgBox.Result;
        }

        static void OpenSettings()
        {
            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime wnd
             && wnd.MainWindow != null)
            {
                new Windows.SettingsWindow().ShowDialog(wnd.MainWindow);
            }
            else
            {
                new Windows.SettingsWindow().Show();
            }
        }

        static void ShutdownApplication()
        {
            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime wnd
             && wnd.MainWindow != null)
            {
                wnd.Shutdown();
            }
            else
            {
                Console.WriteLine("Warning: doing a hard shutdown of application.");
                Environment.Exit(0);
            }
        }

        static List<TextValuePair<int>> GetScreens()
        {
            var list = new List<TextValuePair<int>>();

            if (Design.IsDesignMode)
            {
                list.Add(new() { Text = "Display 1 (#0)", Value = 0 });
                list.Add(new() { Text = "Display 2 (#1)", Value = 1 });

                return list;
            }

            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime wnd
             && wnd.MainWindow != null)
            {
                var screens = wnd.MainWindow.Screens.All.ToList();

                for (int i = 0; i < screens.Count; ++i)
                {
                    list.Add(new()
                    {
                        Text = $"{screens[i].DisplayName} (#{i})" ?? $"#{i}",
                        Value = i
                    });
                }
            }

            return list;
        }

        static void ShowDriveDetails(IHardware hardware)
        {
            if (hardware is StorageDevice sd)
            {
                if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime wnd
                 && wnd.MainWindow != null)
                {
                    new Windows.StorageWindow(sd).ShowDialog(wnd.MainWindow);
                }
                else
                {
                    new Windows.StorageWindow(sd).Show();
                }
            }
        }

        #endregion
    }
}
