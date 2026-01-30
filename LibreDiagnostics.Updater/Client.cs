/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using Avalonia;

namespace LibreDiagnostics.Updater
{
    /// <summary>
    /// Provides the entry point and core startup logic for the application.
    /// </summary>
    public static class Client
    {
        #region Properties

        public static List<string> Args { get; private set; }

        #endregion

        #region Public

        /// <summary>
        /// Initializes and starts the application using the specified command-line arguments.
        /// </summary>
        /// <param name="args">A list of command-line arguments to configure the applications startup behavior. Cannot be null.</param>
        public static void Start(IList<string> args)
        {
            //Initialization
            Args = args as List<string> ?? args.ToList();

            //This blocks until app shuts down
            StartApp(args);
        }

        #endregion

        #region Private

        [STAThread]
        static void StartApp(IList<string> args)
        {
            // Initialization code. Don't use any Avalonia, third-party APIs or any
            // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
            // yet and stuff might break.
            App.BuildAvaloniaApp()
               .StartWithClassicDesktopLifetime([.. args]);
        }

        #endregion
    }
}
