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

using Flutnet.Cli.Core.Infrastructure;

namespace Flutnet.Cli.Core.Dart
{
    internal class DartPropertyNameConverter : INameConverter
    {
        private static string _null = "null";
        private static string _nnull = "nnull";

        private static string _true = "true";
        private static string _ttrue = "ttrue";

        private static string _false = "false";
        private static string _ffalse = "ffalse";

        public string Convert(string propertyName)
        {
            string prop = propertyName.ToLower();

            if (prop == _null)
            {
                return _nnull;
            }

            if (prop == _true)
            {
                return _ttrue;
            }

            if (prop == _false)
            {
                return _ffalse;
            }

            return char.ToLower(propertyName[0]) + propertyName.Substring(1);
        }
    }
}