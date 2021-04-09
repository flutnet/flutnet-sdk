using System;
using Foundation;

namespace FlutnetApp.PluginInterop
{
    // @interface GeneratedPluginRegistrant : NSObject
    [BaseType(typeof(NSObject))]
    interface GeneratedPluginRegistrant
    {
        // + (void)registerWithRegistry:(NSObject<FlutterPluginRegistry>*)registry;
        [Static]
        [Export("registerWithRegistry:")]
        void Register(NSObject registry);
    }

    [Static]
    interface Constants
    {
        // extern double FlutterPluginRegistrantVersionNumber;
        [Field("FlutterPluginRegistrantVersionNumber", "__Internal")]
        double FlutterPluginRegistrantVersionNumber { get; }

        // extern const unsigned char [] FlutterPluginRegistrantVersionString;
        [Field("FlutterPluginRegistrantVersionString", "__Internal")]
        [Internal]
        //byte[] FlutterPluginRegistrantVersionString { get; }
        IntPtr FlutterPluginRegistrantVersionString { get; }
    }
}