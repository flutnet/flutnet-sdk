using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FlutnetUI.Utilities
{
    internal static class NamingConventions
    {
        public const string DefaultAppName = "MyApp";
        const string DefaultAppNameForAndroidAppId = "myapp";
        const string DefaultAppNameForIosAppId = DefaultAppName;
        public const string DefaultOrganizationId = "com.companyname";

        public const string DefaultProjectName = "Project";
        public const string DefaultSolutionName = "Solution";
        public const string DefaultFlutterModuleName = "flutter_module";

        const string InvalidCharsPatternWithDiacritic = @"[^\w\-. ]";
        const string InvalidCharsPattern = @"[^a-zA-Z0-9_\-. ]";

        /// <summary>
        /// An expression to check if a string is a valid filename.
        /// The filename must contain only alphanumeric characters (A-Z, a-z, 0-9), hyphen (-), underscore (_), period (.) and space ( ).
        /// </summary>
        static readonly Regex FilenameRegex = new Regex(@"^[\w\-. ]+$");

        /// <summary>
        /// An expression to check if a string is a valid filename.
        /// The filename must contain only alphanumeric characters (A-Z, a-z, 0-9), underscore (_) and period (.).
        /// </summary>
        static readonly Regex FilenameStrictRegex = new Regex(@"^[\w.]+$");

        /// <summary>
        /// An expression to check if a string is a valid Dart package name (lower_case_with_underscores).
        /// </summary>
        static readonly Regex DartPackageNameRegex = new Regex("^[a-z][a-z0-9_]*$");

        #region VS naming

        public static bool IsValidFilename(string input)
        {
            return FilenameStrictRegex.IsMatch(input);
        }

        public static string ToSafeFilename(string input, string defaultValueIfEmpty)
        {
            if (string.IsNullOrEmpty(input))
                return defaultValueIfEmpty;

            if (!IsValidFilename(input))
            {
                input = Regex.Replace(input, @"[^\w\-.]", string.Empty);
                input = Regex.Replace(input, @"[-]", "_");
            }
            input = input.Trim('_', '.');

            if (string.IsNullOrWhiteSpace(input))
                return "x";

            if (Regex.IsMatch(input, "^[0-9]"))
                return "x{segment}";

            return input;
        }

        public static string AppNameToProjectName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            input = Regex.Replace(input, @"[^\w\-.]", string.Empty);
            input = Regex.Replace(input, @"[-]", "_");
            input = input.Trim('_', '.');

            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            if (Regex.IsMatch(input, "^[0-9]"))
                return $"x{input}";

            return input;
        }

        #endregion

        #region Dart / Flutter naming

        public static bool IsValidDartPackageName(string input)
        {
            return DartPackageNameRegex.IsMatch(input);
        }

        public static string ToSafeDartPackageName(string input)
        {
            if (string.IsNullOrEmpty(input))
                return DefaultFlutterModuleName;

            if (!IsValidDartPackageName(input))
            {
                input = input.ToLowerInvariant();
                input = Regex.Replace(input, @"[^a-zA-Z0-9_]", "_");
            }
            input = Regex.Replace(input, @"[_]+", "_");
            input = input.Trim('_');

            if (string.IsNullOrWhiteSpace(input))
                return "x";

            if (Regex.IsMatch(input, "^[0-9]"))
                return "x{segment}";

            return input;
        }

        public static string AppNameToDartPackageName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            if (!IsValidDartPackageName(input))
            {
                input = Regex.Replace(input, @"(\p{L})(\p{Lu})(\p{Ll})", "$1_$2$3");
                input = input.ToLowerInvariant();
                input = Regex.Replace(input, @"[^a-zA-Z0-9_]", "_");
            }
            input = Regex.Replace(input, @"[_]+", "_");
            input = input.Trim('_');

            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            if (Regex.IsMatch(input, "^[0-9]"))
                return $"x{input}";

            return input;
        }

        #endregion

        #region Android naming

        public static string ToSafeAndroidOrganizationId(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return DefaultOrganizationId;

            input = input.ToLowerInvariant();
            //input = Regex.Replace(input, @"[^a-zA-Z0-9_\-. ]", string.Empty);
            //input = Regex.Replace(input, @"[ ]", "_");
            //input = Regex.Replace(input, @"[-]", "_");
            input = Regex.Replace(input, @"[^a-zA-Z0-9_.]", "_");
            input = Regex.Replace(input, @"[.]+", ".");
            input = Regex.Replace(input, @"[_]+", "_");
            input = input.Trim('.', '_');

            if (string.IsNullOrWhiteSpace(input))
                return "x";

            List<string> segments = new List<string>();
            foreach (string segment in input.Split('.', StringSplitOptions.RemoveEmptyEntries))
            {
                if (Regex.IsMatch(segment, "^[0-9]"))
                {
                    segments.Add($"x{segment}");
                    continue;
                }
                segments.Add(segment);
            }
            return string.Join('.', segments);
        }

        public static string AppNameForAndroidAppId(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return DefaultAppNameForAndroidAppId;

            input = input.ToLowerInvariant();
            //input = Regex.Replace(input, @"[^a-zA-Z0-9_\-. ]", string.Empty);
            //input = Regex.Replace(input, @"[ ]+", string.Empty);
            //input = Regex.Replace(input, @"[.]+", string.Empty);
            //input = Regex.Replace(input, @"[-]+", "_");
            input = Regex.Replace(input, @"[^a-zA-Z0-9_]", "_");
            input = Regex.Replace(input, @"[_]+", "_");
            input = input.Trim('_');

            if (string.IsNullOrWhiteSpace(input))
                return "x";

            if (Regex.IsMatch(input, "^[0-9]"))
                return $"x{input}";

            return input;
        }

        public static string BuildAndroidAppId(string appName, string organizationId)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ToSafeAndroidOrganizationId(organizationId));
            sb.Append(".");
            sb.Append(AppNameForAndroidAppId(appName));
            return sb.ToString();
        }

        #endregion

        #region iOS naming

        public static string ToSafeIosOrganizationId(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return DefaultOrganizationId;

            //input = Regex.Replace(input, @"[^a-zA-Z0-9_\-. ]", string.Empty);
            //input = Regex.Replace(input, @"[ ]", "-");
            //input = Regex.Replace(input, @"[_]", "-");
            input = Regex.Replace(input, @"[^a-zA-Z0-9\-.]", "-");
            input = Regex.Replace(input, @"[.]+", ".");
            input = Regex.Replace(input, @"[-]+", "-");
            input = input.Trim('.', '-');

            if (string.IsNullOrWhiteSpace(input))
                return "x";

            List<string> segments = new List<string>();
            foreach (string segment in input.Split('.', StringSplitOptions.RemoveEmptyEntries))
            {
                if (Regex.IsMatch(segment, "^[0-9]"))
                {
                    segments.Add("x" + segment);
                    continue;
                }
                segments.Add(segment);
            }
            return string.Join('.', segments);
        }

        public static string AppNameForIosAppId(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return DefaultAppNameForIosAppId;

            //input = Regex.Replace(input, @"[^a-zA-Z0-9_\-. ]", string.Empty);
            //input = Regex.Replace(input, @"[ ]+", string.Empty);
            //input = Regex.Replace(input, @"[.]+", string.Empty);
            //input = Regex.Replace(input, @"[_]+", "-");
            input = Regex.Replace(input, @"[^a-zA-Z0-9\-]", "-");
            input = Regex.Replace(input, @"[-]+", "-");
            input = input.Trim('-');

            if (string.IsNullOrWhiteSpace(input))
                return "x";

            if (Regex.IsMatch(input, "^[0-9]"))
                return $"x{input}";

            return input;
        }

        public static string BuildIosAppId(string appName, string organizationId)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ToSafeIosOrganizationId(organizationId));
            sb.Append(".");
            sb.Append(AppNameForIosAppId(appName));
            return sb.ToString();
        }

        #endregion
    }
}