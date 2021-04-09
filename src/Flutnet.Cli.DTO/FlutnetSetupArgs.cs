namespace Flutnet.Cli.DTO
{
    /// <summary>
    /// Class that holds all the input arguments of the CLI 'exec' command
    /// for properly configuring Flutter and all the required environment variables.
    /// </summary>
    public class FlutnetSetupInArg : InArg
    {
        public string FlutterSdkLocation { get; set; }
        public string AndroidSdkLocation { get; set; }
        public string JavaSdkLocation { get; set; }
    }

    /// <summary>
    /// Class that holds the result details of the CLI 'exec' command
    /// for properly configuring Flutter and all the required environment variables.
    /// </summary>
    public class FlutnetSetupOutArg : OutArg
    {
    }
}