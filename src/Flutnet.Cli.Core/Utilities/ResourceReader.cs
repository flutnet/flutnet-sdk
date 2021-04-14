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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Flutnet.Cli.Core.Utilities
{
    internal static class ResourceReader
    {
        public static IEnumerable<string> ReadStringResourcesEachLine(Type assemblyType, string name)
        {
            Assembly assembly = assemblyType.Assembly;

            string resourceName = assembly.GetManifestResourceNames()
                .SingleOrDefault(str => str.EndsWith(name));

            if (resourceName == null)
                yield return null;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream ?? throw new InvalidOperationException()))
            {
                string line = reader.ReadLine();
                yield return line;
            }
        }

        public static string ReadStringResources(Type assemblyType, string name)
        {
            Assembly assembly = assemblyType.Assembly;

            string resourceName = assembly.GetManifestResourceNames()
                .SingleOrDefault(str => str.EndsWith(name));

            if (resourceName == null)
                return null;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream ?? throw new InvalidOperationException()))
            {
                string res = reader.ReadToEnd();
                return res;
            }
        }

        public static byte[] ReadByteResources(Type assemblyType, string name)
        {
            Assembly assembly = assemblyType.Assembly;

            string resourceName = assembly.GetManifestResourceNames()
                .SingleOrDefault(str => str.EndsWith(name));

            if (resourceName == null)
                return null;

            byte[] data;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                data = new byte[stream.Length];
                stream.Read(data, 0, (int)stream.Length);
            }

            return data;
        }
    }
}