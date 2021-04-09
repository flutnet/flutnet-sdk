using System;
using System.IO;
using System.Linq;
using Flutnet.Cli.Core;
using Flutnet.Cli.Core.Infrastructure;
using Flutnet.Cli.Core.Utilities;
using Flutnet.Cli.DTO;
using Newtonsoft.Json;

namespace Flutnet.Cli.Commands
{
    internal class ExecCommand : CommandBase
    {
        readonly ExecOptions _options;

        public ExecCommand(ExecOptions options)
        {
            _options = options;
        }

        public static int Run(ExecOptions options)
        {
            options.Validate();
            return new ExecCommand(options).Execute();
        }

        public override int Execute()
        {
            if (_options.Arguments is NewProjectInArg arg1)
            {
                return ExecuteCore(() =>
                {
                    NewProjectCreator.Create(ToNewProjectSettings(arg1), _options.Verbose);
                    return new NewProjectOutArg();
                });
            }

            if (_options.Arguments is UpdateCheckInArg)
            {
                return ExecuteCore(() =>
                {
                    var check = UpdateManager.CheckForUpdates(_options.Verbose);
                    return new UpdateCheckOutArg
                    {
                        UpToDate = check.UpToDate,
                        NewVersion = check.NewVersion,
                        DownloadUrl = check.DownloadUrl
                    };
                });
            }

            if (_options.Arguments is FlutterInfoInArg)
            {
                return ExecuteCore(() =>
                { 
                    var check = FlutterAssistant.CheckCompatibility(_options.Verbose);
                    return new FlutterInfoOutArg
                    {
                        InstalledVersion = check.InstalledVersion.Version,
                        Compatibility = (DTO.FlutterCompatibility) (int) check.Compatibility,
                        NextSupportedVersion = check.NextSupportedVersion,
                        LatestSupportedVersion = check.LatestSupportedVersion
                    };
                });
            }

            if (_options.Arguments is FlutterDiagInArg)
            {
                return ExecuteCore(() =>
                {
                    var diag = FlutterAssistant.RunDiagnostic(_options.Verbose);
                    return new FlutterDiagOutArg
                    {
                        AndroidSdkLocation = diag.AndroidSdkLocation,
                        FlutterSdkLocation = diag.FlutterSdkLocation,
                        JavaSdkLocation = diag.JavaSdkLocation,
                        Issues = (DTO.FlutterIssues) (int) diag.Issues,

                        InstalledVersion = diag.FlutterCompatibility.InstalledVersion.Version,
                        Compatibility = (DTO.FlutterCompatibility) (int) diag.FlutterCompatibility.Compatibility,
                        NextSupportedVersion = diag.FlutterCompatibility.NextSupportedVersion,
                        LatestSupportedVersion = diag.FlutterCompatibility.LatestSupportedVersion,

                        DoctorErrors = diag.FlutterDoctor.Items.Where(i => i.Type == FlutterDoctorReportItemType.Error).ToDictionary(i => i.Description, i => i.Details),
                        DoctorWarnings = diag.FlutterDoctor.Items.Where(i => i.Type == FlutterDoctorReportItemType.Warning).ToDictionary(i => i.Description, i => i.Details),

                        CurrentShell = FlutnetShell.CurrentShell,
                        CurrentShellConfigurationFile = FlutnetShell.CurrentShellConfigurationFile
                    };
                });
            }

            if (_options.Arguments is FlutnetSetupInArg arg6)
            {
                return ExecuteCore(() =>
                {
                    FlutterAssistant.Configure(arg6.FlutterSdkLocation, arg6.AndroidSdkLocation, arg6.JavaSdkLocation, _options.Verbose);
                    return new FlutnetSetupOutArg();
                });
            }

            return 0;
        }

        private int ExecuteCore<TOut>(Func<TOut> action) where TOut : OutArg, new()
        {
            try
            {
                TOut response = action.Invoke();
                return ReturnSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is CommandLineException))
                    Log.Ex(ex);

                return ReturnError<TOut>(ex);
            }
        }

        private int ReturnSuccess<TOut>(TOut successfulResponse) where TOut : OutArg, new()
        {
            WriteResult(successfulResponse);
            return ReturnCodes.Success;
        }

        private int ReturnError<TOut>(Exception ex) where TOut: OutArg, new()
        {
            TOut result = new TOut { Success = false };

            if (ex is CommandLineException cliex)
            {
                result.ErrorCode = (int)cliex.Code;
                result.ErrorMessage = cliex.Message;
                result.SourceError = ExceptionSerializer.Serialize(cliex.InnerException);
            }
            else
            {
                result.ErrorCode = (int) CommandLineErrorCode.Unknown;
                result.ErrorMessage = CommandLineErrorCode.Unknown.GetDefaultMessage();
                result.SourceError = ExceptionSerializer.Serialize(ex);
            }

#if DEBUG
            if (result.SourceError != null)
                Console.Error.WriteLine(result.SourceError);
            Console.WriteLine(result.ErrorMessage);
#endif

            WriteResult(result);
            //return ReturnCodes.CommandExecutionError;
            return ReturnCodes.Success;
        }

        private void WriteResult<TOut>(TOut result)
        {
            string json = JsonConvert.SerializeObject(result);
            File.WriteAllText(_options.OutputFile, json);
        }

        private NewProjectSettings ToNewProjectSettings(NewProjectInArg arg)
        {
            NewProjectSettings settings = new NewProjectSettings
            {
                AppName = arg.AppName,
                OrganizationId = arg.OrganizationId,
                AndroidAppId = arg.AndroidAppId,
                IosAppId = arg.IosAppId,
                TargetAndroid = arg.TargetAndroid,
                TargetIos = arg.TargetIos,
                ProjectName = arg.ProjectName,
                SolutionName = arg.SolutionName,
                Location = arg.Location,
                CreateFlutterSubfolder = arg.CreateFlutterSubfolder,
                FlutterModuleName = arg.FlutterModuleName,
                FlutterPackageName = arg.FlutterPackageName,
                FlutterVersion = arg.FlutterVersion
            };
            return settings;
        }
    }
}