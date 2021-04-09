namespace FlutnetUI.Models
{
    public class AppKeyItem
    {
        public AppKeyItem(string applicationId, string appKey)
        {
            ApplicationId = applicationId;
            AppKey = appKey;
        }

        public string ApplicationId { get; }
        public string AppKey { get; }
    }
}
