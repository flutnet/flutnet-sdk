using System;
using System.Collections;
using System.Reflection;

namespace Flutnet.Cli.Core.Infrastructure
{
    internal enum CommandLineErrorCode
    {
        /// <summary>
        /// An unexpected error occurred.
        /// </summary>
        [CommandLineErrorCode("An unexpected error occurred.")]
        Unknown = 1,

        /// <summary>
        /// Cannot connect to the server. Please check your Internet connection and try again.
        /// </summary>
        [CommandLineErrorCode("Cannot connect to the server. Please check your Internet connection and try again.")]
        WebApiUnreachable = 10,

        /// <summary>
        /// APIs returned a non-successful HTTP status code.
        /// </summary>
        [CommandLineErrorCode("Server returned {0:D} ({0:G})")]
        WebApiCallGenericError = 11,

        /// <summary>
        /// APIs returned an error object containing a friendly message to display to the user.
        /// </summary>
        WebApiCallFriendlyError = 12,

        /// <summary>
        /// Cannot find MSBuild. Is Visual Studio installed on this machine?
        /// </summary>
        [CommandLineErrorCode("Cannot find MSBuild. Are Visual Studio and .NET Core 3.1 SDK installed on this machine?")]
        NewProject_MSBuildDetectionFailed = 100,
        /// <summary>
        /// An error occurred while trying to setup project templates.
        /// </summary>
        [CommandLineErrorCode("An error occurred while trying to setup project templates.")]
        NewProject_InstallFlutnetTemplateFailed = 101,
        /// <summary>
        /// An error occurred while trying to create the Xamarin projects.
        /// </summary>
        [CommandLineErrorCode("An error occurred while trying to create the Xamarin projects.")]
        NewProject_CreateDotNetProjectsFailed = 102,
        /// <summary>
        /// An error occurred while trying to create the Flutter module.
        /// </summary>
        [CommandLineErrorCode("An error occurred while trying to create the Flutter module.")]
        NewProject_CreateFlutterModuleFailed = 103,
        /// <summary>
        /// An error occurred while building Flutter module.
        /// </summary>
        [CommandLineErrorCode("An error occurred while building the Flutter module.")]
        NewProject_BuildFlutterModuleFailed = 104,
        /// <summary>
        /// An error occurred while adjusting Xamarin project references to Flutter module.
        /// </summary>
        [CommandLineErrorCode("An error occurred while adjusting Xamarin project references to Flutter module.")]
        NewProject_SetNativeReferencesFailed = 105,

        /// <summary>
        /// An error occurred while loading license information.
        /// </summary>
        [CommandLineErrorCode("An error occurred while loading license information.")]
        Licensing_LoadLicenseFailed = 200,

        /// <summary>
        /// This operation is not allowed: software is running in Trial Mode.
        /// </summary>
        [CommandLineErrorCode("This operation is not allowed: software is running in Trial Mode.")]
        Licensing_TrialModeOperationNotAllowed = 201
    }

    internal class CommandLineErrorCodeAttribute : Attribute
    {
        public CommandLineErrorCodeAttribute(string message, string localizationKey = null)
        {
            DefaultMessage = message;
            LocalizationKey = localizationKey;
        }

        public string DefaultMessage { get; set; }
        public string LocalizationKey { get; set; }
    }

    internal static class CommandLineErrorCodeExtensions
    {
        static readonly Hashtable Cache = new Hashtable();

        static CommandLineErrorCodeExtensions()
        {
            foreach (CommandLineErrorCode code in Enum.GetValues(typeof(CommandLineErrorCode)))
                Cache.Add(code, GetAttribute(code));
        }

        /// <summary>
        /// Returns all <see cref="CommandLineErrorCode"/> values.
        /// </summary>
        public static ICollection GetValues()
        {
            return Cache.Keys;
        }

        public static string GetDefaultMessage(this CommandLineErrorCode code)
        {
            CommandLineErrorCodeAttribute attribute = (CommandLineErrorCodeAttribute) Cache[code];
            return attribute != null ? attribute.DefaultMessage : string.Empty;
        }

        public static string GetLocalizationKey(this CommandLineErrorCode code)
        {
            CommandLineErrorCodeAttribute attribute = (CommandLineErrorCodeAttribute) Cache[code];
            return attribute?.LocalizationKey;
        }

        private static CommandLineErrorCodeAttribute GetAttribute(CommandLineErrorCode code)
        {
            return (CommandLineErrorCodeAttribute) Attribute.GetCustomAttribute(ForValue(code), typeof(CommandLineErrorCodeAttribute));
        }

        private static MemberInfo ForValue(CommandLineErrorCode code)
        {
            return typeof(CommandLineErrorCode).GetField(Enum.GetName(typeof(CommandLineErrorCode), code));
        }
    }
}