namespace Flutnet.Cli.DTO
{
    /// <summary>
    /// Class that holds all the input arguments of the CLI 'exec' command
    /// for retrieving information about the installed version of Flutter
    /// and its compatibility with the current version of Flutnet SDK.
    /// </summary>
    public class FlutterInfoInArg : InArg
    {
    }

    /// <summary>
    /// Class that holds the result details of the CLI 'exec' command
    /// for retrieving information about the installed version of Flutter
    /// and its compatibility with the current version of Flutnet SDK.
    /// </summary>
    public class FlutterInfoOutArg : OutArg
    {
        public string InstalledVersion { get; set; }
        public FlutterCompatibility Compatibility { get; set; }
        public string NextSupportedVersion { get; set; }
        public string LatestSupportedVersion { get; set; }
    }
}