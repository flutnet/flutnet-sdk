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

using System.IO;

namespace Flutnet.Cli.Core.Utilities
{
    internal static class StreamExtensions
    {
        public static void WriteTo(this Stream input, string filepath)
        {
            //your fav write method:
            /*
            using (var stream = File.Create(file))
            {
                input.CopyTo(stream);
            }
            */
            //or

            using (var stream = new MemoryStream())
            {
                input.CopyTo(stream);
                File.WriteAllBytes(filepath, stream.ToArray());
            }

            //whatever that fits.
        }
    }
}