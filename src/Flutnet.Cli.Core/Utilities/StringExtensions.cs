using System;

namespace Flutnet.Cli.Core.Utilities
{
    public static class StringExtensions
    {
        public static string ToNetStringVersion(this string pubVersion)
        {
            if (string.IsNullOrEmpty(pubVersion))
                return pubVersion;

            // From ^2.2.0+2 --> 2.2.0.2
            return pubVersion.Replace("^", string.Empty).Replace("+", ".");
        }

        public static Version ToNetVersion(this string pubVersion)
        {
            string netVersion = pubVersion.ToNetStringVersion();
            try
            {
                var version = Version.Parse(netVersion);
                return version;
            }
            catch
            {
                return new Version(0, 0);
            }
        }

    }
}
