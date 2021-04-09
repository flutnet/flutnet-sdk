using System;
using System.IO;

namespace Flutnet.Cli.Core.Utilities
{
    internal static class FileSystemInfoExtensions
    {
        public static string GetRelativePathFrom(this FileSystemInfo to, FileSystemInfo from)
        {
            return from.GetRelativePathTo(to);
        }

        public static string GetRelativePathTo(this FileSystemInfo from, FileSystemInfo to)
        {
            Func<FileSystemInfo, string> getPath = fsi => fsi is DirectoryInfo 
                ? fsi.FullName.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + "\\" 
                : fsi.FullName;

            string fromPath = getPath(from);
            string toPath = getPath(to);

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }

        public static bool IsSymbolic(string path)
        {
            FileInfo pathInfo = new FileInfo(path);
            return pathInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
        }

        public static bool IsSymbolic(this FileInfo pathInfo)
        {
            return pathInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
        }

    }
}