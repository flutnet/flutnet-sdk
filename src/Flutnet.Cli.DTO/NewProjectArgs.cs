namespace Flutnet.Cli.DTO
{
    /// <summary>
    /// Class that holds all the input arguments of the CLI 'exec' command
    /// for creating a new Flutnet project.
    /// </summary>
    public class NewProjectInArg : InArg
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
        public string FlutterModuleName { get; set; }
        public string FlutterPackageName { get; set; }
        public string FlutterVersion { get; set; }
    }

    /// <summary>
    /// Class that holds the result details of the CLI 'exec' command
    /// for creating a new Flutnet project.
    /// </summary>
    public class NewProjectOutArg : OutArg
    {
    }
}
