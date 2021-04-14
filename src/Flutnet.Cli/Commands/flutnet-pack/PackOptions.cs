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
using System.Reflection;
using System.Text.RegularExpressions;
using CommandLine;
using Flutnet.Cli.Core.Infrastructure;

namespace Flutnet.Cli.Commands
{
    [Verb("pack", HelpText = "Create/update the Flutter package that acts as a proxy between Dart code and native (Xamarin) code.")]
    public class PackOptions : OptionsBase
    {
        [Option('a', "assembly", Required = true, HelpText = "The .NET assembly that defines the contract between Flutter and Xamarin.")]
        public string AssemblyPath { get; set; }
        public Assembly Assembly { get; private set; }

        [Option('n', "name", Required = true, HelpText = "The name of the generated Flutter package.")]
        public string PackageName { get; set; }
        public string Name { get; private set; }

        [Option('o', "output", Required = false, HelpText = "Location to place the generated Flutter package. The default is the current directory.")]
        public string OutputDirectory { get; set; } = Environment.CurrentDirectory;

        [Option("desc", Required = false, HelpText = "An optional description for the generated Flutter package.")]
        public string PackageDescription { get; set; }

        [Option('f', "force", Required = false, HelpText = "Forces content to be generated even if it would change existing files. " +
                                                           "This is required when the output directory already contains a folder with the same name of the chosen package name.")]
        public bool Override { get; set; }

        public override void Validate()
        {
            // 1) Check if .NET assembly exists and can be securely loaded in this context

            if (string.IsNullOrEmpty(AssemblyPath) || !File.Exists(AssemblyPath))
                throw new OptionsValidationException(GetType(), $"Assembly {AssemblyPath} not found.");

            Assembly assembly;
            try
            {
                assembly = Assembly.LoadFrom(AssemblyPath);
            }
            catch (Exception ex)
            {
                throw new OptionsValidationException(GetType(), $"Unable to load assembly {AssemblyPath}. Please make sure it's a .NET Standard class library.'", ex);
            }

            // 2. Check if a proper Dart name has been provided for the package

            if (!NamingConventions.IsValidDartPackageName(PackageName))
                throw new OptionsValidationException(GetType(), $"{PackageName} is not a valid Dart package name. Please use \"lowercase_with_underscores\" for package names.");

            string name = Regex.Replace(PackageName, @"[_]+", "_").Trim('_');

            // 3. Check if we can generate files in the target directory

            if (Directory.Exists(Path.Combine(OutputDirectory, name)) && !Override)
                throw new OptionsValidationException(GetType(), $"{OutputDirectory} already contains a folder \"{name}\". Please run command with --force to override existing files.");

            Assembly = assembly;
            Name = name;
        }
    }
}