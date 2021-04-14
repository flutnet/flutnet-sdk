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

namespace Flutnet.Utilities
{
    internal static class StringExtensions
    {
        public static string FirstCharUpper(this string value)
        {
            return string.IsNullOrWhiteSpace(value) 
                ? value 
                : value.Length > 1 
                    ? char.ToUpper(value[0]) + value.Substring(1) 
                    : value.ToUpper();
        }

        public static string FirstCharLower(this string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? value
                : value.Length > 1
                    ? char.ToLower(value[0]) + value.Substring(1)
                    : value.ToLower();
        }

        public static string Quoted(this string value)
        {
            return $"\"{value}\"";
        }
    }
}