namespace Flutnet.Cli.DTO
{
    /// <summary>
    /// Class that holds all the input arguments of the CLI 'exec' command
    /// for checking for software updates.
    /// </summary>
    public class UpdateCheckInArg : InArg
    {
    }

    /// <summary>
    /// Class that holds the result details of the CLI 'exec' command
    /// for checking for software updates.
    /// </summary>
    public class UpdateCheckOutArg : OutArg
    {
        public bool UpToDate { get; set; }
        public string NewVersion { get; set; }
        public string DownloadUrl { get; set; }
    }
}
