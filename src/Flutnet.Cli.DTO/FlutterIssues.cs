using System;

namespace Flutnet.Cli.DTO
{
    [Flags]
    public enum FlutterIssues
    {
        None = 0,
        SdkNotFound = 1,
        CompatibilityIssues = 2,
        ReportingErrors = 4
    }
}
