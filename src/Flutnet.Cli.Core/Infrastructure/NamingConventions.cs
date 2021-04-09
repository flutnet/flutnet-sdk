using System.Text.RegularExpressions;

namespace Flutnet.Cli.Core.Infrastructure
{
    internal static class NamingConventions
    {
        /// <summary>
        /// An expression to check if a string is a valid Dart package name (lower_case_with_underscores).
        /// </summary>
        static readonly Regex DartPackageNameRegex = new Regex("^[a-z][a-z0-9_]*$");

        public static bool IsValidDartPackageName(string input)
        {
            return DartPackageNameRegex.IsMatch(input);
        }
    }
}