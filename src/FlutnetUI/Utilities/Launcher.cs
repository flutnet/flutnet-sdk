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