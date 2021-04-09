namespace FlutnetSite.WebApi.DTO
{
    /// <summary>
    /// The object that is sent back by the REST API when errors occur
    /// while processing a request.
    /// </summary>
    public class ApiError
    {
        /// <summary>
        /// Any code that identifies the type of error.
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// A more complete detail of the error and the associated code.
        /// </summary>
        public string Message { get; set; }
    }

    public class CheckForUpdatesRequest
    {
        public string CurrentVersion { get; set; }
        public int OS { get; set; }
        public bool Is64BitOS { get; set; }
        public string SdkTableRevision { get; set; }
    }

    public class CheckForUpdatesResponse
    {
        public bool UpToDate { get; set; }
        public string NewVersion { get; set; }
        public string DownloadUrl { get; set; }
        public string NewSdkTable { get; set; }
    }
}
