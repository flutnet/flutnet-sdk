// Copyright (c) 2020-2021 Novagem Solutions S.r.l.
//
// This file is part of Flutnet.
//
// Flutnet is a free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Flutnet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with Flutnet.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using Flutnet.Cli.Core.Utilities;
using Medallion.Shell;

namespace Flutnet.Cli.Core.Infrastructure
{
#if DEBUG

    internal static class DevWorkUnit
    {
        public const string DefaultFlutterSdkPath_Windows = @"C:\SDKs\flutter";
        public const string DefaultFlutterSdkPath_macOS = "/Users/main/Development/flutter";

        public static int Run()
        {
            if (Utilities.OperatingSystem.IsMacOS())
            {
                string shell1 = GetCurrentLoginShell();
                string shell2 = GetDefaultLoginShell();
                string rc = GetShellConfigurationFile(shell1);
            }

            string flutterSdkPath = null;
            if (!FlutnetShell.TryLocateFile("flutter", out string path))
            {
                FlutterAssistant.Configure(Utilities.OperatingSystem.IsMacOS()
                    ? DefaultFlutterSdkPath_macOS : DefaultFlutterSdkPath_Windows, string.Empty, string.Empty);

                if (FlutnetShell.TryLocateFile("flutter", out string path2))
                    flutterSdkPath = Path.GetDirectoryName(Path.GetDirectoryName(path2));
            }
            else
            {
                flutterSdkPath = Path.GetDirectoryName(Path.GetDirectoryName(path));
            }

            Console.WriteLine("Flutter bin path: {0}", flutterSdkPath);

            //FlutterTools.GetDoctorReport();

            //string env = GetEnvironmentVariable("ANDROID_SDK_ROOT");

            //FlutterAssistant.DiagnosticResult diag = FlutterAssistant.RunDiagnostic();

            return 0;
        }

        /// <summary>
        /// Returns the current login shell (macOS only). Based on: https://stackoverflow.com/a/41553295
        /// </summary>
        private static string GetCurrentLoginShell()
        {
            Command command = Command.Run("/bin/sh",
                arguments: new object[] { "-c", "echo $SHELL" },
                options: options => { options.ThrowOnError(); });

            try
            {
                CommandResult result = command.Result;
                string path = result.StandardOutput.Split(new[] { Environment.NewLine }, 2, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                return path;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the default login shell (macOS only). Based on: https://stackoverflow.com/a/41553295
        /// </summary>
        private static string GetDefaultLoginShell()
        {
            Command command = Command.Run("/bin/sh",
                arguments: new object[] { "-c", "dscl . -read ~/ UserShell | sed 's/UserShell: //'" },
                options: options => { options.ThrowOnError(); });

            try
            {
                CommandResult result = command.Result;
                string path = result.StandardOutput.Split(new [] { Environment.NewLine }, 2, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                return path;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the RC file for the target shell (macOs only).
        /// </summary>
        /// <returns></returns>
        private static string GetShellConfigurationFile(string shell)
        {
            FileInfo fi = new FileInfo(shell);
            if (string.Equals(fi.Name, "zsh", StringComparison.OrdinalIgnoreCase))
            {
                //string config = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ".zshrc");
                string config = Path.GetFullPath("~/.zshrc");
            }
            else if (string.Equals(fi.Name, "bash", StringComparison.OrdinalIgnoreCase))
            {
                //string config1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ".bashrc");
                string config1 = Path.GetFullPath("~/.bashrc");

                //string config2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ".bash_profile");
                string config2 = Path.GetFullPath("~/.bash_profile");

                return File.Exists(config2) ? config2 : config1;
            }
            return string.Empty;
        }

        private static string GetEnvironmentVariable(string variable)
        {
            string value = null;

            if (Utilities.OperatingSystem.IsWindows())
            {
                value = Environment.GetEnvironmentVariable(variable) ??
                        Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Machine);
            }
            else if (Utilities.OperatingSystem.IsMacOS())
            {
                try
                {
                    CommandResult result = FlutnetShell.RunInMacOsShell($"echo ${variable}", Environment.CurrentDirectory);
                    value = result.StandardOutput.Split(new[] { Environment.NewLine }, 2, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return value;
        }
    }
#endif
}