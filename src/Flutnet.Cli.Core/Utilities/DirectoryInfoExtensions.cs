using System.IO;

namespace Flutnet.Cli.Core.Utilities
{
    internal static class DirectoryInfoExtensions
    {
        /// <summary>
        /// Deletes all files and folders in this directory.
        /// </summary>
        public static void Clear(this DirectoryInfo directory)
        {
            // Delete all files
            foreach (FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }

            // Delete all subdirectories
            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        public static bool IsSymbolic(this DirectoryInfo directory)
        {
            return directory.Attributes.HasFlag(FileAttributes.ReparsePoint);
        }

    }
}