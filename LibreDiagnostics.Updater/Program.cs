/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using BlackSharp.Core.Extensions;
using BlackSharp.Core.Logging;
using LibreDiagnostics.Tasks.Github;
using System.Diagnostics;
using System.Reflection;
using LDUpdater = LibreDiagnostics.Tasks.Github.Updater;

namespace LibreDiagnostics.Updater
{
    internal class Program
    {
        #region Example

        /*
args = new string[3] { "--calling-app=\"C:/Code/LibreDiagnostics/LibreDiagnostics.Updater/bin/Debug/net8.0/LibreDiagnostics.exe\"", "--start-self-update", "--source-directory=\"C:/Code/LibreDiagnostics/LibreDiagnostics.Updater/bin/Debug/net8.0\"" };

"--calling-app=\"C:/Code/LibreDiagnostics/LibreDiagnostics.Updater/bin/Debug/net8.0/LibreDiagnostics.exe\""
"--start-self-update"
"--source-directory=\"C:/Code/LibreDiagnostics/LibreDiagnostics.Updater/bin/Debug/net8.0\""
         */

        #endregion

        static readonly DateTime FileNameDateTime = DateTime.Now;

        static void LogTrace(string message = "")
        {
            if (Logger.Instance.IsEnabled)
            {
                Logger.Instance.Add(LogLevel.Trace, message, DateTime.Now);
                Logger.Instance.SaveToFile(@$"C:\LDT\UpdaterLog_{FileNameDateTime:yyyyMMdd_HHmmss}.txt", false);
            }
        }

        static async Task Main(string[] args)
        {
            //Enable this for logging output (and verify directory of log file exists)
            //Logger.Instance.LogLevel = LogLevel.Trace;
            //Logger.Instance.IsEnabled = true;

            LogTrace("Arguments:");
            foreach (var arg in args)
            {
                LogTrace($"\"{arg}\"");
            }
            LogTrace();

            var versionOfMyself = Assembly.GetEntryAssembly().GetName().Version;

            //Get own file path
            var currentFilePath = Environment.ProcessPath;
            LogTrace($"{nameof(currentFilePath)} = '{currentFilePath}'");

            //Get current directory
            var currentDirectory = Path.GetDirectoryName(currentFilePath);
            LogTrace($"{nameof(currentDirectory)} = '{currentDirectory}'");

            if (args.Length == 0)
            {
                Console.WriteLine("Please do NOT start the updater manually.");
                Console.WriteLine("This can result in PERMANENT loss of files in the directory where the updater is located.");
                Console.WriteLine($"This would currently be this directory: '{currentDirectory}'.");
                Console.WriteLine("Exiting now.");
            }
            //Check if an update is available, do a self-copy and run
            else if (args.Length == 2
                  && args[0].StartsWith(LDUpdater.CallingApplicationArg)
                  && args[1] == LDUpdater.StartUpdateArg)
            {
                //Get calling application path
                var callingApplication = args[0].Split('=')[1].Trim('"');
                LogTrace($"{nameof(callingApplication)} = '{callingApplication}'");

                var updater = new LDUpdater(Constants.Owner, Constants.Repository);

                var updateCheckResult = await updater.IsUpdateAvailable(versionOfMyself);

                //If no update is available, exit
                if (!updateCheckResult.IsUpdateAvailable)
                {
                    LogTrace($"No update available.");
                    Environment.Exit(666);
                }

                LogTrace($"Update available.");

                //Get required assemblies
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.Location.StartsWith(currentDirectory, StringComparison.OrdinalIgnoreCase))
                    .Select(a => a.Location)
                    .ToList();

                //Get file name without extension
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(currentFilePath);
                LogTrace($"{nameof(fileNameWithoutExtension)} = '{fileNameWithoutExtension}'");

                //Get required files, based on file name
                var files = Directory
                    .EnumerateFiles(currentDirectory)
                    .Where(f => f.StartsWith(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                //Create temp directory
                var tempDir = Directory.CreateTempSubdirectory();
                LogTrace($"{nameof(tempDir)} = '{tempDir}'");

                //Target updater path
                var targetUpdaterPath = Path.Combine(tempDir.FullName, Path.GetFileName(currentFilePath));
                LogTrace($"{nameof(targetUpdaterPath)} = '{targetUpdaterPath}'");

                //Copy self to temp directory
                File.Copy(currentFilePath, targetUpdaterPath);

                //Copy required assemblies
                assemblies.ForEach(assembly =>
                {
                    var destination = Path.Combine(tempDir.FullName, Path.GetFileName(assembly));

                    File.Copy(assembly, destination);
                });

                //Copy required files
                files.ForEach(file =>
                {
                    var destination = Path.Combine(tempDir.FullName, Path.GetFileName(file));
                    File.Copy(file, destination, false); //Do not overwrite, only copy missing files
                });

                LogTrace($"Starting copied version of myself.");

                //Start updater from temp directory
                Process.Start(new ProcessStartInfo(targetUpdaterPath, [$"{LDUpdater.CallingApplicationArg}=\"{callingApplication}\"", LDUpdater.StartSelfUpdateArg, $"{LDUpdater.SourceDirectoryArg}=\"{currentDirectory}\""])
                {
                    CreateNoWindow = true,
                    WorkingDirectory = tempDir.FullName,
                });

                Environment.Exit(0);
            }
            //Start self-update process
            else if (args.Length == 3
                  && args[0].StartsWith(LDUpdater.CallingApplicationArg)
                  && args[1] == LDUpdater.StartSelfUpdateArg
                  && args[2].StartsWith(LDUpdater.SourceDirectoryArg))
            {
                //Get calling application path
                var callingApplication = args[0].Split('=')[1].Trim('"');
                LogTrace($"{nameof(callingApplication)} = '{callingApplication}'");

                //Get source (target for update) directory
                var sourceDirectory = args[2].Split('=')[1].Trim('"');

                var updater = new LDUpdater(Constants.Owner, Constants.Repository);

                try
                {
                    //Download update
                    var downloadedFile = await updater.DownloadUpdate(versionOfMyself);

                    if (string.IsNullOrEmpty(downloadedFile))
                    {
                        //Throw exception, skip update and continue with cleanup
                        throw new Exception("No file to download. Likely already up to date.");
                    }

                    LogTrace($"{nameof(downloadedFile)} = '{downloadedFile}'");

                    //Apply update
                    updater.ApplyUpdate(sourceDirectory, downloadedFile, true);

                    LogTrace($"Applied update.");

                    //Start updated application
                    Process.Start(Path.Combine(sourceDirectory, Path.GetFileName(callingApplication)));
                }
                catch (Exception e)
                {
                    LogTrace($"Update failed: {e.FullExceptionString()}");
                }

                LogTrace($"Update procedure done. Removing myself.");

                //Remove myself
                if (OperatingSystem.IsWindows())
                {
                    //Remove myself with a small delay
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C choice /C Y /N /D Y /T 1 & rmdir /S /Q \"{currentDirectory}\"",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                }
                else if (OperatingSystem.IsLinux())
                {
                    //Remove myself with a small delay
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "/bin/sh",
                        Arguments = $"-c \"sleep 1 && rm -rf '{currentDirectory}'\"",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                }
            }
        }
    }
}
