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
using System.Text;
using Flutnet.Cli.Core.Dart;
using Flutnet.Utilities;
using Medallion.Shell;

namespace Flutnet.Cli.Core.Infrastructure
{
    internal class FlutterTools
    {
        /// <summary>
        /// Runs the "flutter --version" command to retrieve the current version of Flutter.
        /// </summary>
        public static FlutterVersion GetVersion(bool verbose = false)
        {
            CommandResult result = FlutnetShell.RunCommand("flutter --version --no-version-check", Environment.CurrentDirectory, verbose);

            FlutterVersion version = new FlutterVersion();

            using (StringReader reader = new StringReader(result.StandardOutput))
            {
                string versionLine = reader.ReadLine(); 
                string[] parts = versionLine
                    .Replace("Flutter", string.Empty, StringComparison.InvariantCultureIgnoreCase)
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    version.Version = parts[0].Trim();
                }

                string frameworkRevLine = reader.ReadLine();
                int index = frameworkRevLine.IndexOf("revision ", StringComparison.InvariantCultureIgnoreCase);
                if (index != -1)
                {
                    version.FrameworkRev = frameworkRevLine.Substring(index + 9, 10).Trim();
                }

                string engineRevLine = reader.ReadLine();
                index = engineRevLine.IndexOf("revision ", StringComparison.InvariantCultureIgnoreCase);
                if (index != -1)
                {
                    version.EngineRev = engineRevLine.Substring(index + 9).Trim();
                }
            }

            return version;
        }

        /// <summary>
        /// Runs the "flutter doctor" command to retrieve the status of the current installation of Flutter.
        /// </summary>
        public static FlutterDoctorReport GetDoctorReport(bool verbose = false)
        {
            CommandResult result = FlutnetShell.RunCommand("flutter doctor --no-version-check", Environment.CurrentDirectory, verbose);

            FlutterDoctorReport report = new FlutterDoctorReport();

            using (StringReader reader = new StringReader(result.StandardOutput))
            {
                // Read "Doctor summary" first line
                reader.ReadLine();

                FlutterDoctorReportItemBuilder builder = null;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        if (builder != null)
                        {
                            report.Items.Add(builder.Build());
                            builder = null;
                        }
                    }

                    if (!line.StartsWith("["))
                    {
                        builder?.AddContent(line.TrimStart());
                        continue;
                    }

                    int index = line.IndexOf("]", StringComparison.InvariantCultureIgnoreCase);
                    if (index != -1)
                    {
                        string symbol = line.Substring(1, index - 1);
                        string msg = line.Substring(index + 2);

                        if (builder != null)
                            report.Items.Add(builder.Build());
                        builder = new FlutterDoctorReportItemBuilder(msg);

                        switch (symbol)
                        {
                            case "√":
                            case "✓":
                                builder.SetType(FlutterDoctorReportItemType.Check);
                                break;

                            case "!":
                                builder.SetType(FlutterDoctorReportItemType.Warning);
                                break;
                            
                            default:
                                builder.SetType(FlutterDoctorReportItemType.Error);
                                break;
                        }
                    }
                }
            }

            return report;
        }

        /// <summary>
        /// Runs the "flutter clean" command to remove all the files produced by the previous build(s), such as the build folder.
        /// </summary>
        public static void Clean(string projectFolder, bool verbose = false)
        {
            FlutnetShell.RunCommand("flutter clean", projectFolder, verbose);
        }

        /// <summary>
        /// Runs the "flutter pub upgrade" command.
        /// </summary>
        public static void PubUpgrade(string projectFolder, bool verbose = false)
        {
            FlutnetShell.RunCommand("flutter pub upgrade", projectFolder, verbose);
        }

        /// <summary>
        /// Runs the "flutter pub get" command to get all the dependencies listed in the pubspec.yaml file
        /// in the current working directory, as well as their transitive dependencies.
        /// </summary>
        public static void GetDependencies(string projectFolder, bool verbose = false)
        {
            FlutnetShell.RunCommand("flutter pub get", projectFolder, verbose);
        }

        /// <summary>
        /// Runs the "flutter pub run build_runner build" command in the current working directory.
        /// The directory must contain a pubspec.yaml file.
        /// </summary>
        public static void BuildBuildRunner(string projectFolder, bool deleteConflictingOutputs = false, bool verbose = false)
        {
            FlutnetShell.RunCommand(
                deleteConflictingOutputs
                    ? $"flutter pub run build_runner build --delete-conflicting-outputs"
                    : $"flutter pub run build_runner build", projectFolder, verbose);
        }

        /// <summary>
        /// Runs the "flutter build aar" command to create an Android Archive (AAR)
        /// to be integrated into a native Android application.
        /// </summary>
        public static void BuildAndroidArchive(string projectFolder, FlutterModuleBuildConfig buildConfig, bool verbose = false)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("flutter build aar ");

            if (buildConfig != FlutterModuleBuildConfig.Default)
            {
                if (!buildConfig.HasFlag(FlutterModuleBuildConfig.Debug))
                    sb.Append("--no-debug ");
                if (!buildConfig.HasFlag(FlutterModuleBuildConfig.Profile))
                    sb.Append("--no-profile ");
                if (!buildConfig.HasFlag(FlutterModuleBuildConfig.Release))
                    sb.Append("--no-release ");
            }

            FlutnetShell.RunCommand(sb.ToString(), projectFolder, verbose);
        }

        /// <summary>
        /// Runs the "flutter build ios-framework" command to create an iOS framework
        /// to be integrated into a native iOS application (available only on macOS).
        /// </summary>
        public static void BuildIosFramework(string projectFolder, FlutterModuleBuildConfig buildConfig, bool verbose = false)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("flutter build ios-framework ");

            if (buildConfig != FlutterModuleBuildConfig.Default)
            {
                if (!buildConfig.HasFlag(FlutterModuleBuildConfig.Debug))
                    sb.Append("--no-debug ");
                if (!buildConfig.HasFlag(FlutterModuleBuildConfig.Profile))
                    sb.Append("--no-profile ");
                if (!buildConfig.HasFlag(FlutterModuleBuildConfig.Release))
                    sb.Append("--no-release ");
            }

            FlutnetShell.RunCommand(sb.ToString(), projectFolder, verbose);
        }

        /// <summary>
        /// Runs the "flutter create -t package" command to create a Flutter package.
        /// </summary>
        /// <returns>A <see cref="Flutnet.Cli.Core.Dart.DartProject"/> that represents the newly created Flutter package.</returns>
        public static DartProject CreatePackage(string workingDir, string name, string description = null, bool verbose = false)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("flutter create -t package ");
            if (!string.IsNullOrEmpty(description))
                sb.Append($"--description {description.Quoted()} ");
            sb.Append(name);

            FlutnetShell.RunCommand(sb.ToString(), workingDir, verbose);

            DartProject prj = new DartProject(new DirectoryInfo(Path.Combine(workingDir, name)));
            prj.Load();
            return prj;
        }

        /// <summary>
        /// Runs the "flutter create -t module" command to create a Flutter module.
        /// </summary>
        /// <returns>A <see cref="Flutnet.Cli.Core.Dart.DartProject"/> that represents the newly created Flutter module.</returns>
        public static DartProject CreateModule(string workingDir, string name, string description = null, string organization = null, bool verbose = false)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("flutter create -t module ");
            if (!string.IsNullOrEmpty(description))
                sb.Append($"--description {description.Quoted()} ");
            if (!string.IsNullOrEmpty(organization))
                sb.Append($"--org {organization} ");
            sb.Append(name);

            FlutnetShell.RunCommand(sb.ToString(), workingDir, verbose);

            DartProject prj = new DartProject(new DirectoryInfo(Path.Combine(workingDir, name)));
            prj.Load();
            return prj;
        }

        /// <summary>
        /// Runs the "flutter create -t app" command to create a Flutter app.
        /// </summary>
        /// <returns>A <see cref="Flutnet.Cli.Core.Dart.DartProject"/> that represents the newly created Flutter app.</returns>
        public static DartProject CreateApp(string workingDir, string name, string description = null, string organization = null, bool verbose = false)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("flutter create -t app ");
            if (!string.IsNullOrEmpty(description))
                sb.Append($"--description {description.Quoted()} ");
            if (!string.IsNullOrEmpty(organization))
                sb.Append($"--org {organization} ");
            sb.Append(name);

            FlutnetShell.RunCommand(sb.ToString(), workingDir, verbose);

            DartProject prj = new DartProject(new DirectoryInfo(Path.Combine(workingDir, name)));
            prj.Load();
            return prj;
        }
    }
}