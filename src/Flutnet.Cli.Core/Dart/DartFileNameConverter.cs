// Copyright (c) 2020-2021 Novagem Solutions S.r.l.
//
// This file is part of Flutnet.
//
// Flutnet is a free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Flutnet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with Flutnet.  If not, see <http://www.gnu.org/licenses/>.

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