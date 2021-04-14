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

using System.Diagnostics;

namespace FlutnetUI.Utilities
{
    internal static class Launcher
    {
        /// <summary>
        /// Opens a URL using system's default browser application.
        /// </summary>
        public static void OpenURL(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (OperatingSystem.IsWindows())
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (OperatingSystem.IsLinux())
                {
                    Process.Start("xdg-open", url);
                }
                else if (OperatingSystem.IsMacOS())
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Opens system's default email client with provided destination email.
        /// </summary>
        public static void MailTo(string emailAddress)
        {
            OpenURL($"mailto:{emailAddress}");
        }

        /// <summary>
        /// Shows a directory content inside system's default file explorer.
        /// </summary>
        public static void OpenFolder(string path)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(path)
            {
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }
    }
}