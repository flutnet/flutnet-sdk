using System.Diagnostics;
using System.Reflection;

namespace FlutnetUI.Utilities
{
    internal static class AssemblyExtensions
    {
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