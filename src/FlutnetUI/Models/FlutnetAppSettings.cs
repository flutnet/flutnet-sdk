namespace FlutnetUI.Models
{
    public class FlutnetAppSettings
    {
        public string AppName { get; set; }
        public string OrganizationId { get; set; }
        /// <summary>
        /// A proper Android application ID (https://developer.android.com/studio/build/application-id)
        /// built upon the app name and organization identifier provided by the user.
        /// </summary>
        public string AndroidAppId { get; set; }
        /// <summary>
        /// A proper iOS bundle ID (https://developer.apple.com/documentation/bundleresources/information_property_list/cfbundleidentifier)
        /// built upon the app name and organization identifier provided by the user.
        /// </summary>
        public string IosAppId { get; set; }
        /// <summary>
        /// Indicates whether a Xamarin.Android application should be created.
        /// </summary>
        public bool TargetAndroid { get; set; }
        /// <summary>
        /// Indicates whether a Xamarin.iOS application should be created.
        /// </summary>
        public bool TargetIos { get; set; }
    }
}
