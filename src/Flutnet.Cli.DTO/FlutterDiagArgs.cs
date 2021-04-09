using System.Collections.Generic;

namespace Flutnet.Cli.DTO
{
    /// <summary>
    /// Class that holds all the input arguments of the CLI 'exec' command
    /// for retrieving a diagnostic report about the current installation of Flutnet.
    /// </summary>
    public class FlutterDiagInArg : InArg
    {
    }

    /// <summary>
    /// Class that holds the result details of the CLI 'exec' command
    /// for retrieving a diagnostic report about the current installation of Flutnet.
    /// </summary>
    public class FlutterDiagOutArg : OutArg
    {
        public FlutterIssues Issues { get; set; }
        public string FlutterSdkLocation { get; set; }
        public string AndroidSdkLocation { get; set; }
        public string JavaSdkLocation { get; set; }

        public string InstalledVersion { get; set; }
        public FlutterCompatibility Compatibility { get; set; }
        public string NextSupportedVersion { get; set; }
        public string LatestSupportedVersion { get; set; }

        public Dictionary<string, string> DoctorErrors { get; set; }
        public Dictionary<string, string> DoctorWarnings { get; set; }

        public string CurrentShell { get; set; }
        public string CurrentShellConfigurationFile { get; set; }
    }
}