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