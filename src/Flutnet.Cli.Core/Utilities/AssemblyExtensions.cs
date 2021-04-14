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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Flutnet.Cli.Core.Utilities
{
    internal static class AssemblyExtensions
    {
        public static IEnumerable<string> GetNamespaces(this Assembly assembly)
        {
            return assembly.GetTypes()
                .Where(t => !string.IsNullOrEmpty(t.Namespace))
                .Select(t => t.Namespace)
                .Distinct();
        }

        public static IEnumerable<string> GetTopLevelNamespaces(this Assembly assembly) 
        {
            return assembly.GetNamespaces()
                .Select(n => n.Split('.').First())
                .Distinct();
        }

        public static IEnumerable<Type> GetTypesInNamespace(this Assembly assembly, string @namespace)
        {
            return assembly.GetTypes().Where(t => t.Namespace == @namespace);
        }

        public static string GetProductVersion(this Assembly assembly)
        {
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.ProductVersion;
        }

        public static string GetFileVersion(this Assembly assembly)
        {
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }

        public static string GetVersion(this Assembly assembly)
        {
            AssemblyName name = assembly.GetName();
            return name.Version.ToString();
        }
    }
}