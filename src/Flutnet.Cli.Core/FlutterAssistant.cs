using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Flutnet.Cli.Core.Infrastructure;
using Flutnet.Cli.Core.Utilities;

namespace Flutnet.Cli.Core
{
    internal class FlutterAssistant
    {
        public static CompatibilityResult CheckCompatibility(bool verbose = false)
        {
            try
            {
                string productVersion = Assembly.GetEntryAssembly().GetProductVersion();
                FlutterVersion flutterVersion = FlutterTools.GetVersion(verbose);

                SdkVersion sdk = AppSettings.Default.SdkTable.Versions.First(v => v.Version == productVersion);
                sdk.Compatibility.Sort(new SdkFlutterVersionComparer());

                bool supported = false; // Indicates whether this SDK explicitly supports the installed Flutter version
                SdkFlutterVersion prev = null; // The last supported Flutter version that precedes the installed one (can be NULL)
                SdkFlutterVersion next = null; // The first supported Flutter version that follows the installed one (can be NULL)
                SdkFlutterVersion latest = null; // The latest supported Flutter version

                for (int i = 0; i < sdk.Compatibility.Count; i++)
                {
                    SdkFlutterVersion fv = sdk.Compatibility[i];
                    int compare = VersionUtils.Compare(fv.Version, flutterVersion.Version);

                    if (compare == 0)
                    {
                        supported = true;
                        break;
                    }

                    if (compare < 0)
                        prev = fv;
                    else if (next == null)
                        next = fv;

                    if (i == sdk.Compatibility.Count - 1)
                        latest = fv;
                }

                FlutterCompatibility compatibility;
                if (supported)
                    compatibility = FlutterCompatibility.Supported;
                else if (prev != null)
                    compatibility = FlutterCompatibility.SupportNotGuaranteed;
                else
                    compatibility = FlutterCompatibility.NotSupported;

                CompatibilityResult result = new CompatibilityResult
                {
                    Compatibility = compatibility,
                    InstalledVersion = flutterVersion,
                    NextSupportedVersion = next?.Version,
                    LatestSupportedVersion = latest?.Version
                };

                return result;
            }
            catch (Exception ex)
            {
                Log.Ex(ex);
                Console.WriteLine(ex);
                throw;
            }
        }

        public static DiagnosticResult RunDiagnostic(bool verbose = false)
        {
            DiagnosticResult result = new DiagnosticResult();

            // 1. Locate Android SDK and Java SDK

            if (AndroidToolsLocator.TryLocateAndroidSdk(out string androidSdkPath))
                result.AndroidSdkLocation = androidSdkPath;

            if (AndroidToolsLocator.TryLocateJavaSdk(out string javaSdkPath))
                result.JavaSdkLocation = javaSdkPath;

            // 2. Locate Flutter SDK

            if (FlutnetShell.TryLocateFile("flutter", out string path))
            {
                result.FlutterSdkLocation = Path.GetDirectoryName(Path.GetDirectoryName(path));
            }
            else
            {
                result.Issues |= FlutterIssues.SdkNotFound;
                result.FlutterCompatibility = new CompatibilityResult
                {
                    Compatibility = FlutterCompatibility.NotSupported,
                    InstalledVersion = new FlutterVersion(),
                    LatestSupportedVersion = GetLatestSupportedVersion(),
                };
                result.FlutterDoctor = new FlutterDoctorReport();
                return result;
            }

            // 3. Check Flutter version and its compatibility with Flutter
            result.FlutterCompatibility =  CheckCompatibility(verbose);
            if (result.FlutterCompatibility.Compatibility != FlutterCompatibility.Supported)
                result.Issues |= FlutterIssues.CompatibilityIssues;

            // 4. Run 'flutter doctor' to discover any error
            result.FlutterDoctor =  FlutterTools.GetDoctorReport(verbose);
            if (result.FlutterDoctor.Items.Any(i => i.Type == FlutterDoctorReportItemType.Error))
                result.Issues |= FlutterIssues.ReportingErrors;

            return result;
        }

        public static void Configure(string flutterSdkLocation, string androidSdkLocation, string javaSdkLocation, bool verbose = false)
        {
            if (Utilities.OperatingSystem.IsWindows())
            {
                string path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);

                string flutterBinPath = Path.Combine(flutterSdkLocation, "bin");
                if (!path.Contains(flutterBinPath, StringComparison.OrdinalIgnoreCase))
                    path = $"{path};{flutterBinPath}";
            
                Environment.SetEnvironmentVariable("FLUTTER_HOME", flutterSdkLocation, EnvironmentVariableTarget.User);       
               
                if (!string.IsNullOrEmpty(androidSdkLocation))
                {
                    Environment.SetEnvironmentVariable("ANDROID_HOME", androidSdkLocation, EnvironmentVariableTarget.User);

                    string androidToolsPath = Path.Combine(androidSdkLocation, "tools");
                    if (!path.Contains(androidToolsPath, StringComparison.OrdinalIgnoreCase))
                        path = $"{path};{androidToolsPath}";

                    string androidPlatformToolsPath = Path.Combine(androidSdkLocation, "platform-tools");
                    if (!path.Contains(androidPlatformToolsPath, StringComparison.OrdinalIgnoreCase))
                        path = $"{path};{androidPlatformToolsPath}";
                }

                if (!string.IsNullOrEmpty(javaSdkLocation))
                    Environment.SetEnvironmentVariable("JAVA_HOME", javaSdkLocation, EnvironmentVariableTarget.User);

                Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.User);
            }
            else
            {
                string flutterBinPath = "$FLUTTER_HOME/bin";
                string androidToolsPath = "$ANDROID_HOME/tools";
                string androidPlatformToolsPath = "$ANDROID_HOME/platform-tools";

                if (!File.Exists(FlutnetShell.CurrentShellConfigurationFile))
                {
                    // Create the rc file from scratch

                    string path = $"$PATH:{flutterBinPath}";
                    if (!string.IsNullOrEmpty(androidSdkLocation))
                        path = $"{path}:{androidToolsPath}:{androidPlatformToolsPath}";

                    using (StreamWriter writer = File.CreateText(FlutnetShell.CurrentShellConfigurationFile))
                    {
                        writer.WriteLine($"#!{FlutnetShell.CurrentShell}");
                        writer.WriteLine();
                        writer.WriteLine($"export FLUTTER_HOME={flutterSdkLocation}");
                        if (!string.IsNullOrEmpty(androidSdkLocation))
                            writer.WriteLine($"export ANDROID_HOME={androidSdkLocation}");
                        if (!string.IsNullOrEmpty(javaSdkLocation))
                            writer.WriteLine($"export JAVA_HOME={javaSdkLocation}");
                        writer.WriteLine($"export PATH={path}");
                    }
                }
                else
                {
                    // Read and edit the existing rc file
                    string[] lines = File.ReadAllLines(FlutnetShell.CurrentShellConfigurationFile);

                    List<string> newLines = new List<string>();
                    bool pathSet = false;
                    bool flutterSet = false;
                    bool androidSet = false;
                    bool javaSet = false;

                    foreach (string line in lines)
                    {
                        if (line.StartsWith("export PATH", StringComparison.OrdinalIgnoreCase))
                        {
                            string value = line.Split('=')[1].Trim(' ', '"');

                            if (!line.Contains(flutterBinPath, StringComparison.OrdinalIgnoreCase))
                                value = $"{value}:{flutterBinPath}";
                            if (!string.IsNullOrEmpty(androidSdkLocation))
                            {
                                if (!line.Contains(androidToolsPath, StringComparison.OrdinalIgnoreCase))
                                    value = $"{value}:{androidToolsPath}";
                                if (!line.Contains(androidPlatformToolsPath, StringComparison.OrdinalIgnoreCase))
                                    value = $"{value}:{androidPlatformToolsPath}";
                            }
                            newLines.Add($"export PATH={value}");
                            pathSet = true;
                        }
                        else if (line.StartsWith("export FLUTTER_HOME", StringComparison.OrdinalIgnoreCase))
                        {
                            string value = line.Split('=')[1].Trim(' ', '"');
                            newLines.Add($"export FLUTTER_HOME={flutterSdkLocation}");
                            flutterSet = true;
                        }
                        else if (line.StartsWith("export ANDROID_HOME", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!string.IsNullOrEmpty(androidSdkLocation))
                            {
                                string value = line.Split('=')[1].Trim(' ', '"');
                                newLines.Add($"export ANDROID_HOME={androidSdkLocation}");
                            }
                            else
                            {
                                newLines.Add(line);
                            }
                            androidSet = true;
                        }
                        else if (line.StartsWith("export JAVA_HOME", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!string.IsNullOrEmpty(javaSdkLocation))
                            {
                                string value = line.Split('=')[1].Trim(' ', '"');
                                newLines.Add($"export JAVA_HOME={javaSdkLocation}");
                            }
                            else
                            {
                                newLines.Add(line);
                            }
                            javaSet = true;
                        }
                        else
                        {
                            newLines.Add(line);
                        }
                    }

                    if (!flutterSet)
                        newLines.Add($"export FLUTTER_HOME={flutterSdkLocation}");
                    if (!androidSet && !string.IsNullOrEmpty(androidSdkLocation))
                        newLines.Add($"export ANDROID_HOME={androidSdkLocation}");
                    if (!javaSet && !string.IsNullOrEmpty(javaSdkLocation))
                        newLines.Add($"export JAVA_HOME={javaSdkLocation}");
                    if (!pathSet)
                    {
                        string path = $"$PATH:{flutterBinPath}";
                        if (!string.IsNullOrEmpty(androidSdkLocation))
                            path = $"{path}:{androidToolsPath}:{androidPlatformToolsPath}";
                        newLines.Add($"export PATH={path}");
                    }

                    File.WriteAllLines(FlutnetShell.CurrentShellConfigurationFile, newLines);
                }
            }
        }

        private static string GetLatestSupportedVersion()
        {
            try
            {
                string productVersion = Assembly.GetEntryAssembly().GetProductVersion();
                SdkVersion sdk = AppSettings.Default.SdkTable.Versions.First(v => v.Version == productVersion);
                sdk.Compatibility.Sort(new SdkFlutterVersionComparer());
                return sdk.Compatibility.Last().Version;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public class DiagnosticResult
        {
            public FlutterIssues Issues { get; set; }
            public string FlutterSdkLocation { get; set; }
            public CompatibilityResult FlutterCompatibility { get; set; }
            public FlutterDoctorReport FlutterDoctor { get; set; }
            public string AndroidSdkLocation { get; set; }
            public string JavaSdkLocation { get; set; }
        }

        public class CompatibilityResult
        {
            public FlutterCompatibility Compatibility { get; set; }
            public FlutterVersion InstalledVersion { get; set; }
            public string NextSupportedVersion { get; set; }
            public string LatestSupportedVersion { get; set; }
        }

        [Flags]
        public enum FlutterIssues
        {
            None                = 0,
            SdkNotFound         = 1,
            CompatibilityIssues = 2,
            ReportingErrors     = 4
        }

        public enum FlutterCompatibility
        {
            Supported,
            SupportNotGuaranteed,
            NotSupported
        }
    }
}
