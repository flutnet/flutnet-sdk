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