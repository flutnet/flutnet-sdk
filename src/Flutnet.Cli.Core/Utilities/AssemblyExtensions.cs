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