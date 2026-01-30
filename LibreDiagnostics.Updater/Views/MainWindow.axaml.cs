/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using Avalonia.Controls;
using LibreDiagnostics.Updater.ViewModels;

namespace LibreDiagnostics.Updater.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            if (!Design.IsDesignMode)
            {
                DataContext = new MainWindowViewModel();
            }

            InitializeComponent();
        }
    }
}