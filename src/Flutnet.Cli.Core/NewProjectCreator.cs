using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Flutnet.Cli.Core.Dart;
using Flutnet.Cli.Core.Infrastructure;
using Flutnet.Cli.Core.Utilities;
using Flutnet.Utilities;
using Medallion.Shell;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using OperatingSystem = Flutnet.Cli.Core.Utilities.OperatingSystem;

namespace Flutnet.Cli.Core
{
    internal class NewProjectCreator
    {
        const string FlutterModuleDebugAAR = "flutter_debug-1.0.aar";
        const string FlutterModuleReleaseAAR = "flutter_release-1.0.aar";
        const string FlutterModuleAppFramework = "App.xcframework";
        const string FlutterEngineFramework = "Flutter.xcframework";
        const string FlutterPluginRegistrantFramework = "FlutterPluginRegistrant.xcframework";
        
        const string MSBuildDebugCondition = " '$(Configuration)'=='Debug' ";
        const string MSBuildReleaseCondition = " '$(Configuration)'=='Release' ";

        const string FlutnetTemplateName = "FlutnetApp";
        const string FlutnetTemplateFolderName = "FlutnetSolutionName";

        public static void Create(NewProjectSettings settings, bool verbose = false)
        {
            try
            {
                MSBuildLocator.RegisterDefaults();
            }
            catch (Exception e)
            {
                Log.Ex(e);
                throw new CommandLineException(CommandLineErrorCode.NewProject_MSBuildDetectionFailed, e);
            }

            try
            {
                InstallFlutnetTemplate();

                if (!Directory.Exists(settings.Location))
                    Directory.CreateDirectory(settings.Location);

                Console.WriteLine("Creating Xamarin (.NET) projects...");
                CreateDotNetProjects(settings, verbose);
                ConfigurePostBuildEvent(settings, verbose);
                Console.WriteLine("Done.");

                if (!Directory.Exists(settings.FlutterSubfolderPath))
                    Directory.CreateDirectory(settings.FlutterSubfolderPath);
                
                Console.WriteLine("Creating Flutter module...");
                DartProject project = CreateFlutterModule(settings, verbose);
                Console.WriteLine("Done.");

                Console.WriteLine("Building Flutter module...");
                BuildFlutterModule(settings, verbose);
                Console.WriteLine("Done.");

                Console.WriteLine("Configuring native references in Xamarin projects... ");
                SetNativeReferences(settings, project);
                Console.WriteLine("Done.");

                Console.WriteLine("Configuring package references... ");
                UpdatePackageReferences(settings);
                Console.WriteLine("Done.");
            }
            finally
            {
                UninstallFlutnetTemplate();
            }
        }

        /// <summary>
        /// Runs a .NET Core CLI command to install Flutnet project template on the current machine.
        /// </summary>
        private static void InstallFlutnetTemplate()
        {
            string path = Path.Combine(AppSettings.Default.TemplatesFolder, FlutnetTemplateFolderName);
            Log.Debug("Installing Flutnet project template: {0}", path);
            Command command = Command.Run("dotnet", 
                arguments: new object[] { "new", "-i", path },
                options: options => { options.ThrowOnError(); });
            command.WaitWithFancyError(CommandLineErrorCode.NewProject_InstallFlutnetTemplateFailed);

            // Command to check if Flutnet project template is already installed
            //Command command = Command.Run("dotnet", "new", FlutnetTemplateName, "-l");
            //command.Wait();
        }

        /// <summary>
        /// Runs a .NET Core CLI command to uninstall Flutnet project template from the current machine.
        /// </summary>
        private static void UninstallFlutnetTemplate()
        {
            string path = Path.Combine(AppSettings.Default.TemplatesFolder, FlutnetTemplateFolderName);
            Log.Debug("Uninstalling Flutnet project template: {0}", path);
            try
            {
                Command command = Command.Run("dotnet", "new", "-u", path);
                command.Wait();
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Runs a .NET Core CLI command to create a new project tree from template.
        /// </summary>
        private static void CreateDotNetProjects(NewProjectSettings settings, bool verbose = false)
        {
            List<object> arguments = new List<object> { "new", FlutnetTemplateName };
            arguments.Add("--name");
            arguments.Add(settings.ProjectName);
            arguments.Add("--output");
            arguments.Add(settings.SolutionName);
            arguments.Add("--appName");
            arguments.Add(settings.AppName);
            arguments.Add("--solutionName");
            arguments.Add(settings.SolutionName);
            if (settings.TargetAndroid)
                arguments.Add("--target-android");
            if (settings.TargetIos) 
                arguments.Add("--target-ios");
            arguments.Add("--androidPackage");
            arguments.Add(settings.AndroidAppId);
            arguments.Add("--iosBundle");
            arguments.Add(settings.IosAppId);

            Command command = Command.Run("dotnet", arguments, options => { options.WorkingDirectory(settings.Location).ThrowOnError(); });
            if (verbose)
            {
                command.RedirectTo(Console.Out);
                command.RedirectStandardErrorTo(Console.Error);
            }
            command.WaitWithFancyError(CommandLineErrorCode.NewProject_CreateDotNetProjectsFailed);
        }

        /// <summary>
        /// Creates the Flutter module to be integrated into Xamarin (native) applications.
        /// </summary>
        private static DartProject CreateFlutterModule(NewProjectSettings settings, bool verbose = false)
        {
            try
            {
                DartProject project = FlutterTools.CreateModule(settings.FlutterSubfolderPath, settings.FlutterModuleName,
                    organization: settings.OrganizationId, verbose: verbose);
                
                FlutterTools.CreatePackage(settings.FlutterSubfolderPath, settings.FlutterPackageName, 
                    description: DartBridgeBuilder.DefaultPackageDescription, verbose: verbose);

                project.AddDevDependency(new DartProjectDependency(settings.FlutterPackageName, DartProjectDependencyType.Path, $"../{settings.FlutterPackageName}"));
                project.ApplyChanges();

                // Before building the module on a new machine, run "flutter pub get" first
                // to regenerate the .android/ and .ios/ directories.
                // (see: https://flutter.dev/docs/development/add-to-app/android/project-setup#create-a-flutter-module
                //  and: https://flutter.dev/docs/development/add-to-app/ios/project-setup#create-a-flutter-module)
                project.GetDependencies(verbose);
               
                return project;
            }
            catch (Exception e)
            {
                throw new CommandLineException(CommandLineErrorCode.NewProject_CreateFlutterModuleFailed, e);
            }
        }

        /// <summary>
        /// Build the Flutter module to generate the Android Archive (AAR) and/or the iOS framework
        /// that will be embedded into Xamarin applications.
        /// </summary>
        private static void BuildFlutterModule(NewProjectSettings settings, bool verbose = false)
        {
            try
            {
                if (settings.TargetAndroid)
                    FlutterTools.BuildAndroidArchive(settings.FlutterModulePath, FlutterModuleBuildConfig.Debug | FlutterModuleBuildConfig.Release, verbose);
                if (settings.TargetIos && OperatingSystem.IsMacOS())
                    FlutterTools.BuildIosFramework(settings.FlutterModulePath, FlutterModuleBuildConfig.Debug | FlutterModuleBuildConfig.Release, verbose);
            }
            catch (Exception e)
            {
                throw new CommandLineException(CommandLineErrorCode.NewProject_BuildFlutterModuleFailed, e);
            }
        }

        /// <summary>
        /// Configures the post-build event of the .NET Standard class library.
        /// </summary>
        public static void ConfigurePostBuildEvent(NewProjectSettings settings, bool verbose = false)
        {
            try
            {
                string csprojPath = Path.Combine(settings.SolutionPath, $"{settings.ProjectName}.ServiceLibrary", $"{settings.ProjectName}.ServiceLibrary.csproj");

                string executableWin;
                string executableMac;

                if (OperatingSystem.IsWindows())
                {
                    executableWin = Path.Combine(AppSettings.Default.AppPath, "flutnet.exe");
                    executableMac = $"{AppSettings.DefaultBinPath_macOS}/flutnet";
                }
                else
                {
                    executableWin = $"{AppSettings.DefaultBinPath_Windows}\\flutnet.exe";
                    executableMac = Path.Combine(AppSettings.Default.AppPath, "flutnet");
                }

                ProjectRootElement prjElement = ProjectRootElement.Open(csprojPath);

                // Configures the post-build event(s) for Visual Studio for Mac

                List<string> commandArguments = new List<string>
                {
                    executableMac.Quoted(), "pack",
                    "-a", "${TargetFile}".Quoted(),
                    "-n", settings.FlutterPackageName,
                    "-o", (settings.CreateFlutterSubfolder ? $"${{SolutionDir}}/{settings.FlutterSubfolderName}" : "${SolutionDir}").Quoted(),
                    "--force"
                };

                foreach (ProjectPropertyGroupElement groupElement in prjElement.PropertyGroups)
                {
                    if (groupElement.Condition.Contains("$(Configuration)") && groupElement.Condition.Contains("$(Platform)"))
                    {
                        ProjectPropertyElement propElement = groupElement.Properties.FirstOrDefault(p => p.Name == "CustomCommands");
                        propElement.Value = propElement.Value
                            .Replace("<command></command>", $"<command>{string.Join(' ', commandArguments)}</command>");
                    }
                }

                // Configures the post-build event(s) for Visual Studio

                List<string> taskArguments = new List<string>
                {
                    executableWin.Quoted(), "pack",
                    "-a", "$(TargetPath)".Quoted(),
                    "-n", settings.FlutterPackageName,
                    "-o", (settings.CreateFlutterSubfolder ? $"$(SolutionDir)\\{settings.FlutterSubfolderName}" : "$(SolutionDir)").Quoted(),
                    "--force"
                };

                ProjectTargetElement targetElement = prjElement.Targets.FirstOrDefault(t => t.Name == "PostBuild");
                ProjectTaskElement taskElement = targetElement.AddTask("Exec");
                taskElement.SetParameter("Command", string.Join(' ', taskArguments));

                prjElement.Save();
            }
            catch (Exception e)
            {
                Log.Ex(e);
                throw new CommandLineException(CommandLineErrorCode.NewProject_CreateDotNetProjectsFailed, e);
            }
        }

        private static void SetNativeReferences(NewProjectSettings settings, DartProject project)
        {
            try
            {
                if (settings.TargetAndroid)
                {
                    string csprojPath = Path.Combine(settings.SolutionPath, $"{settings.ProjectName}.ModuleInterop.Android", $"{settings.ProjectName}.ModuleInterop.Android.csproj");
                    SetNativeReference_AndroidBindings(csprojPath, project);
                }
                if (settings.TargetIos && OperatingSystem.IsMacOS())
                {
                    string csprojPath = Path.Combine(settings.SolutionPath, $"{settings.ProjectName}.PluginInterop.iOS", $"{settings.ProjectName}.PluginInterop.iOS.csproj");
                    SetNativeReference_iOSBindings(csprojPath, project);

                    csprojPath = Path.Combine(settings.SolutionPath, $"{settings.ProjectName}.iOS", $"{settings.ProjectName}.iOS.csproj");
                    SetNativeReference_iOSApp(csprojPath, project);
                }
            }
            catch (Exception e)
            {
                Log.Ex(e);
                throw new CommandLineException(CommandLineErrorCode.NewProject_SetNativeReferencesFailed, e);
            }
        }

        /// <summary>
        /// Adjusts the references to the Flutter module AARs that need to be embedded
        /// inside the Xamarin.Android bindings library.
        /// </summary>
        /// <param name="csprojPath">Path of the Xamarin.Android bindings library project file</param>
        /// <param name="project"><see cref="DartProject"/> representing the newly created Flutter module</param>
        private static void SetNativeReference_AndroidBindings(string csprojPath, DartProject project)
        {
            string aarDebugPath = project.GetAndroidArchivePath(FlutterModuleBuildConfig.Debug);
            string aarReleasePath = project.GetAndroidArchivePath(FlutterModuleBuildConfig.Release);

            FileSystemInfo csProjPathDirInfo = new FileInfo(csprojPath);

            string aarDebugPathRelative = csProjPathDirInfo.GetRelativePathTo(new FileInfo(aarDebugPath));
            string aarReleasePathRelative = csProjPathDirInfo.GetRelativePathTo(new FileInfo(aarReleasePath));

            using (ProjectCollection prjColl = new ProjectCollection())
            {
                Project prj = prjColl.LoadProject(csprojPath);

                ICollection<ProjectItem> aarItems = prj.GetItemsIgnoringCondition("LibraryProjectZip");
                prj.RemoveItems(aarItems);

                IList<ProjectItem> debugItems = prj.AddItem("LibraryProjectZip", aarDebugPathRelative);
                // need to specify Condition on parent ItemGroup
                debugItems[0].Xml.Parent.Condition = MSBuildDebugCondition;
                //debugItems[0].Xml.Condition = MSBuildDebugCondition;
                debugItems[0].SetMetadataValue("Link", $"Jars\\{FlutterModuleDebugAAR}");

                IList<ProjectItem> releaseItems = prj.AddItem("LibraryProjectZip", aarReleasePathRelative);
                // need to specify Condition on parent ItemGroup
                releaseItems[0].Xml.Parent.Condition = MSBuildReleaseCondition;
                //releaseItems[0].Xml.Condition = MSBuildReleaseCondition;
                releaseItems[0].SetMetadataValue("Link", $"Jars\\{FlutterModuleReleaseAAR}");

                prj.Save();
            }
        }

        /// <summary>
        /// Adjusts the references to the Flutter module App.xcframework that need to be linked by the Xamarin.iOS app.
        /// </summary>
        /// <param name="csprojPath">Path of the Xamarin.iOS app project file</param>
        /// <param name="project"><see cref="DartProject"/> representing the newly created Flutter module</param>
        /// <param name="addFlutterEngineNativeRef">Indicates whether Flutter engine XCFramework should be linked by the Xamarin.iOS app.</param>
        private static void SetNativeReference_iOSApp(string csprojPath, DartProject project, bool addFlutterEngineNativeRef = false)
        {
            string appFrameworkDebugPath = project.GetIosXCFrameworkPath(FlutterModuleBuildConfig.Debug);
            string appFrameworkReleasePath = project.GetIosXCFrameworkPath(FlutterModuleBuildConfig.Release);

            FileSystemInfo csProjPathDirInfo = new FileInfo(csprojPath);

            string appFrameworkDebugPathRelative = csProjPathDirInfo.GetRelativePathTo(new FileInfo(appFrameworkDebugPath));
            string appFrameworkReleasePathRelative = csProjPathDirInfo.GetRelativePathTo(new FileInfo(appFrameworkReleasePath));

            using (ProjectCollection prjColl = new ProjectCollection())
            {
                Project prj = prjColl.LoadProject(csprojPath);

                ICollection<ProjectItem> nativeRefItems = prj.GetItemsIgnoringCondition("NativeReference");
                prj.RemoveItems(nativeRefItems);

                IList<ProjectItem> debugItems = prj.AddItem("NativeReference", appFrameworkDebugPathRelative);
                // need to specify Condition on parent ItemGroup
                debugItems[0].Xml.Parent.Condition = MSBuildDebugCondition;
                //debugItems[0].Xml.Condition = MSBuildDebugCondition;
                debugItems[0].SetMetadataValue("Kind", "Framework");
                debugItems[0].SetMetadataValue("SmartLink", "False");

                IList<ProjectItem> releaseItems = prj.AddItem("NativeReference", appFrameworkReleasePathRelative);
                // need to specify Condition on parent ItemGroup
                releaseItems[0].Xml.Parent.Condition = MSBuildReleaseCondition;
                //releaseItems[0].Xml.Condition = MSBuildReleaseCondition;
                releaseItems[0].SetMetadataValue("Kind", "Framework");
                releaseItems[0].SetMetadataValue("SmartLink", "False");

                prj.Save();
            }
        }

        private static void UpdatePackageReferences(NewProjectSettings settings)
        {
            try
            {
                string productVersion = Assembly.GetEntryAssembly().GetProductVersion();
                string flutterVersion = settings.FlutterVersion;
                if (string.IsNullOrEmpty(flutterVersion))
                {
                    flutterVersion = FlutterTools.GetVersion().Version;
                }

                SdkVersion sdk = AppSettings.Default.SdkTable.Versions.First(v => v.Version == productVersion);
                SdkFlutterVersion fv = sdk.Compatibility.FirstOrDefault(f => VersionUtils.Compare(f.Version, flutterVersion) == 0);
                if (fv == null)
                {
                    // We're using a version of Flutter that is not officially supported by Flutnet (no binding libraries exist).
                    // Let's try and see if we can find a prior version that's fully compatible.
                    // If we can't find anything, simply return and keep the template configuration as is.
                    sdk.Compatibility.Sort(new SdkFlutterVersionComparer());
                    fv = sdk.Compatibility.LastOrDefault(f => VersionUtils.Compare(f.Version, flutterVersion) < 0);
                    if (fv == null)
                        return;
                }

                if (settings.TargetAndroid)
                {
                    string interopVersion = !string.IsNullOrEmpty(fv.AndroidInteropVersion)
                        ? fv.AndroidInteropVersion
                        : fv.Version;

                    string csprojPath = Path.Combine(settings.SolutionPath, $"{settings.ProjectName}.Android", $"{settings.ProjectName}.Android.csproj");
                    UpdatePackageReferences_AndroidProject(csprojPath, interopVersion);
                    csprojPath = Path.Combine(settings.SolutionPath, $"{settings.ProjectName}.ModuleInterop.Android", $"{settings.ProjectName}.ModuleInterop.Android.csproj");
                    UpdatePackageReferences_AndroidProject(csprojPath, interopVersion);
                }

                if (settings.TargetIos && OperatingSystem.IsMacOS())
                {
                    string interopVersion = !string.IsNullOrEmpty(fv.IosInteropVersion)
                        ? fv.IosInteropVersion
                        : fv.Version;

                    string csprojPath = Path.Combine(settings.SolutionPath, $"{settings.ProjectName}.iOS", $"{settings.ProjectName}.iOS.csproj");
                    UpdatePackageReferences_iOSProject(csprojPath, interopVersion);
                    csprojPath = Path.Combine(settings.SolutionPath, $"{settings.ProjectName}.PluginInterop.iOS", $"{settings.ProjectName}.PluginInterop.iOS.csproj");
                    UpdatePackageReferences_iOSProject(csprojPath, interopVersion);
                }
            }
            catch (Exception e)
            {
                throw new CommandLineException(CommandLineErrorCode.NewProject_SetNativeReferencesFailed, e);
            }
        }

        private static void UpdatePackageReferences_AndroidProject(string csprojPath, string interopVersion)
        {
            using (ProjectCollection prjColl = new ProjectCollection())
            {
                Project prj = prjColl.LoadProject(csprojPath);

                foreach (ProjectItem reference in prj.GetItems("PackageReference"))
                {
                    if (!string.Equals(reference.UnevaluatedInclude, "Flutnet.Interop.Android"))
                        continue;

                    reference.SetMetadataValue("Version", interopVersion);
                }

                prj.Save();
            }
        }

        private static void UpdatePackageReferences_iOSProject(string csprojPath, string interopVersion)
        {
            using (ProjectCollection prjColl = new ProjectCollection())
            {
                Project prj = prjColl.LoadProject(csprojPath);

                foreach (ProjectItem reference in prj.GetItems("PackageReference"))
                {
                    if (!string.Equals(reference.UnevaluatedInclude, "Flutnet.Interop.iOS"))
                        continue;

                    reference.SetMetadataValue("Version", interopVersion);
                }

                prj.Save();
            }
        }

        /// <summary>
        /// Adjusts the references to the FlutterPluginRegistrant.xcframework that need to be embedded
        /// inside the Xamarin.iOS bindings library.
        /// </summary>
        /// <param name="csprojPath">Path of the Xamarin.iOS bindings library project file</param>
        /// <param name="project"><see cref="DartProject"/> representing the newly created Flutter module</param>
        private static void SetNativeReference_iOSBindings(string csprojPath, DartProject project)
        {
            string frameworkDebugPath = project.GetIosXCFrameworkPath(FlutterModuleBuildConfig.Debug);
            frameworkDebugPath = frameworkDebugPath.Replace(FlutterModuleAppFramework, FlutterPluginRegistrantFramework);
            string frameworkReleasePath = project.GetIosXCFrameworkPath(FlutterModuleBuildConfig.Release);
            frameworkReleasePath = frameworkReleasePath.Replace(FlutterModuleAppFramework, FlutterPluginRegistrantFramework);

            FileSystemInfo csProjPathDirInfo = new FileInfo(csprojPath);

            string frameworkDebugPathRelative = csProjPathDirInfo.GetRelativePathTo(new FileInfo(frameworkDebugPath));
            string frameworkReleasePathRelative = csProjPathDirInfo.GetRelativePathTo(new FileInfo(frameworkReleasePath));

            using (ProjectCollection prjColl = new ProjectCollection())
            {
                Project prj = prjColl.LoadProject(csprojPath);

                ICollection<ProjectItem> aarItems = prj.GetItemsIgnoringCondition("NativeReference");
                prj.RemoveItems(aarItems);

                IList<ProjectItem> debugItems = prj.AddItem("NativeReference", frameworkDebugPathRelative);
                // need to specify Condition on parent ItemGroup
                debugItems[0].Xml.Parent.Condition = MSBuildDebugCondition;
                //debugItems[0].Xml.Condition = MSBuildDebugCondition;
                debugItems[0].SetMetadataValue("Kind", "Framework");
                debugItems[0].SetMetadataValue("SmartLink", "False");

                IList<ProjectItem> releaseItems = prj.AddItem("NativeReference", frameworkReleasePathRelative);
                // need to specify Condition on parent ItemGroup
                releaseItems[0].Xml.Parent.Condition = MSBuildReleaseCondition;
                //releaseItems[0].Xml.Condition = MSBuildReleaseCondition;
                releaseItems[0].SetMetadataValue("Kind", "Framework");
                releaseItems[0].SetMetadataValue("SmartLink", "False");

                prj.Save();
            }
        }
    }
}