﻿using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Medallion.Shell;

namespace FlutnetUI.Utilities
{
    internal class AndroidSdkLocator
    {
        public static bool TryLocate(out string sdkPath)
        {
            string path;

            // Check #1 - ANDROID_SDK_ROOT or ANDROID_HOME environment variables

            path = GetEnvironmentVariable("ANDROID_SDK_ROOT");
            if (!string.IsNullOrEmpty(path))
            {
                sdkPath = path;
                return true;
            }

            path = GetEnvironmentVariable("ANDROID_HOME");
            if (!string.IsNullOrEmpty(path))
            {
                sdkPath = path;
                return true;
            }

            // Check #2 - Default Xamarin Android SDK location

            if (OperatingSystem.IsWindows())
            {
                // https://docs.microsoft.com/en-us/xamarin/android/troubleshooting/questions/android-sdk-location?tabs=windows
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Android", "android-sdk");
            }
            else
            {
                // https://docs.microsoft.com/en-us/xamarin/android/troubleshooting/questions/android-sdk-location?tabs=macos
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Developer", "Xamarin", "android-sdk-macosx");
            }

            if (Directory.Exists(path))
            {
                sdkPath = path;
                return true;
            }

            // Check #3 - Android Studio configuration

            // Try looking for Android SDK location inside Android Studio settings
            // https://developer.android.com/studio/intro/studio-config

            // Android Studio 4.1: The locations of user configuration directories have been changed
            // https://developer.android.com/studio/releases#directory-configuration-changes

            try
            {
                if (OperatingSystem.IsWindows())
                {
                    // Example: C:\Users\YourUserName\AppData\Roaming\Google\AndroidStudio4.1
                    foreach (string dir in Directory.EnumerateDirectories(
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Google"),
                        "AndroidStudio*", SearchOption.TopDirectoryOnly))
                    {
                        string jdkTableFile = Path.Combine(dir, "options", "jdk.table.xml");
                        if (ParseAndValidateJdkTable(jdkTableFile, out path))
                        {
                            sdkPath = path;
                            return true;
                        }
                    }

                    // Example: C:\Users\YourUserName\.AndroidStudio4.0
                    foreach (string dir in Directory.EnumerateDirectories(
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".AndroidStudio*",
                        SearchOption.TopDirectoryOnly))
                    {
                        string jdkTableFile = Path.Combine(dir, "config", "options", "jdk.table.xml");
                        if (ParseAndValidateJdkTable(jdkTableFile, out path))
                        {
                            sdkPath = path;
                            return true;
                        }
                    }
                }
                else if (OperatingSystem.IsMacOS())
                {
                    // Example: ~/Library/Application Support/Google/AndroidStudio4.1
                    foreach (string dir in Directory.EnumerateDirectories(
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library",
                            "Application Support", "Google"), "AndroidStudio*", SearchOption.TopDirectoryOnly))
                    {
                        string jdkTableFile = Path.Combine(dir, "options", "jdk.table.xml");
                        if (ParseAndValidateJdkTable(jdkTableFile, out path))
                        {
                            sdkPath = path;
                            return true;
                        }
                    }

                    // Example: ~/Library/Preferences/AndroidStudio4.0
                    foreach (string dir in Directory.EnumerateDirectories(
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library",
                            "Preferences"), "AndroidStudio*", SearchOption.TopDirectoryOnly))
                    {
                        string jdkTableFile = Path.Combine(dir, "options", "jdk.table.xml");
                        if (ParseAndValidateJdkTable(jdkTableFile, out path))
                        {
                            sdkPath = path;
                            return true;
                        }
                    }
                }

            }
            catch
            {
                // ignored
            }

            // Check #4 - Try run ADB command

            try
            {
                //NOTE: This command can fail also on machines where Android SDK and Xamarin.Android are correctly installed 
                //      (You're not required to add Android SDK platform-tools folder - where adb resides - to PATH environment variable)

                if (Bash.TryLocateFile("adb", out string adbFullPath))
                {
                    sdkPath = Path.GetDirectoryName(Path.GetDirectoryName(adbFullPath));
                    return true;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            sdkPath = default;
            return false;
        }

        private static bool ParseAndValidateJdkTable(string path, out string sdkPath)
        {
            sdkPath = null;

            if (string.IsNullOrEmpty(path))
                return false;

            if (!File.Exists(path))
                return false;

            XElement component = XElement.Load(path).Element("component");
            if (component == null)
                return false;

            foreach (XElement jdk in component.Elements("jdk"))
            {
                XElement type = jdk.Element("type");
                XAttribute typeValue = type?.Attribute("value");
                if (typeValue == null || !string.Equals(typeValue.Value, "Android SDK"))
                    continue;

                XElement homePath = jdk.Element("homePath");
                XAttribute homePathValue = homePath?.Attribute("value");
                if (homePathValue == null || string.IsNullOrEmpty(homePathValue.Value))
                    continue;

                sdkPath = homePathValue.Value;
                return Directory.Exists(sdkPath);
            }

            return false;
        }

        private static string GetFullPathFromEnvironmentVariables(string filename)
        {
            var paths = new[] { Environment.CurrentDirectory }
                .Concat(Environment.GetEnvironmentVariable("PATH").Split(';'));
            var extensions = new[] { string.Empty }
                .Concat(Environment.GetEnvironmentVariable("PATHEXT").Split(';').Where(e => e.StartsWith(".")));
            var combinations = paths.SelectMany(x => extensions, (path, extension) => Path.Combine(path, filename + extension));
            return combinations.FirstOrDefault(File.Exists);
        }

        private static string GetEnvironmentVariable(string variable)
        {
            string value = null;

            if (Utilities.OperatingSystem.IsWindows())
            {
                value = Environment.GetEnvironmentVariable(variable) ??
                        Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Machine);
            }
            else if (Utilities.OperatingSystem.IsMacOS())
            {
                try
                {
                    CommandResult result = Bash.RunInMacOsShell($"echo ${variable}", Environment.CurrentDirectory);
                    value = result.StandardOutput.Split(new[] { Environment.NewLine }, 2, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return value;
        }
    }
}