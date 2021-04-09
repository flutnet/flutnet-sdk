using System.Collections.Generic;
using Flutnet.Cli.Core.Utilities;

namespace Flutnet.Cli.Core.Infrastructure
{
    internal class SdkFlutterVersionComparer : Comparer<SdkFlutterVersion>
    {
        public override int Compare(SdkFlutterVersion x, SdkFlutterVersion y)
        {
            if (x == null && y == null)
                return 0;

            if (x == null)
                return -1;

            if (y == null)
                return 1;

            return VersionUtils.Compare(x.Version, y.Version);
        }
    }
}