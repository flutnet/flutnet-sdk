using System;
using System.Linq;
using System.Text.RegularExpressions;
using Flutnet.Cli.Core.Infrastructure;

namespace Flutnet.Cli.Core.Dart
{
    internal class DartFileNameConverter : INameConverter
    {
        public string Convert(string cSharpName)
        {

            if (string.IsNullOrEmpty(cSharpName) || cSharpName.Contains(" "))
                throw new Exception("Invalid sharp name!");

            // Rimuovo il primo carattere
            string noCamelCase = _SplitCamelCase(cSharpName, "_");

            if (noCamelCase.StartsWith("_"))
            {
                noCamelCase = noCamelCase.Remove(1);
            }

            string lower = noCamelCase.ToLower();

            return lower.Replace(".", "_");

        }

        private static string _patternCamel_1 = @"(^[a-z]+|[A-Z]+(?![a-z])|[A-Z][a-z]+)";
        private static string _patternCamel_2 = @"(^[a-z]+|[A-Z]+(?![a-z])|[A-Z][a-z]+|[0-9\.*]+|[a-z]+)";

        static string _SplitCamelCase(string input, string fitter)
        {
            MatchCollection matchCollection = Regex.Matches(input, _patternCamel_2);
            string[] words = matchCollection
                .Select(m => m.Value)
                .ToArray();
            string result = string.Join(fitter, words);
            return result;
        }

    }
}