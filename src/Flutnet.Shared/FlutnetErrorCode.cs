using System.Reflection;
using Flutnet.ServiceModel;

namespace Flutnet.Data
{
    [Obfuscation(Exclude = true)]
    [PlatformData]
    internal enum FlutnetErrorCode
    {
        OperationNotImplemented,
        OperationArgumentCountMismatch,
        InvalidOperationArguments,
        OperationArgumentParsingError,
        OperationFailed,
        OperationCanceled,
        EnvironmentNotInitialized,
        AppKeyErrorBadFormat,
        AppKeyErrorApplicationIdMismatch,
        AppKeyErrorUnsupportedLibraryVersion,
        TrialCallsExceeded
    }
}