using System.IO;

namespace Flutnet.Cli.Core
{
    internal class NewProjectSettings
    {
        public string AppName { get; set; }
        public string OrganizationId { get; set; }
        public string AndroidAppId { get; set; }
        public string IosAppId { get; set; }
        public bool TargetAndroid { get; set; }
        public bool TargetIos { get; set; }
        public string ProjectName { get; set; }
        public string SolutionName { get; set; }
        public string Location { get; set; }
        public bool CreateFlutterSubfolder { get; set; }
        public string FlutterSubfolderName { get; set; } = "Flutter";
        public string FlutterModuleName { get; set; }
        public string FlutterPackageName { get; set; }
        public string FlutterVersion { get; set; }

        public string SolutionPath => Path.Combine(Path.GetFullPath(Location), SolutionName);
        public string FlutterSubfolderPath => CreateFlutterSubfolder ? Path.Combine(SolutionPath, FlutterSubfolderName) : SolutionPath;
        public string FlutterModulePath => Path.Combine(FlutterSubfolderPath, FlutterModuleName);
    }
}
