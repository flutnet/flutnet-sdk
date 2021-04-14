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
using System.Reflection;
using Newtonsoft.Json;

namespace Flutnet.Cli
{
    internal static class ExceptionSerializer
    {
        public static string Serialize(Exception exc)
        {
            if (exc == null)
                return null;

            Dictionary<string, object> dict = new Dictionary<string, object>();

            Type type = exc.GetType();
            dict["ClassName"] = type.FullName;
            dict["Message"] = exc.Message;
            dict["Data"] = exc.Data;
            //dict["InnerException"] = Serialize(exc.InnerException);
            dict["HResult"] = exc.HResult;
            dict["Source"] = exc.Source;

            //foreach (PropertyInfo p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            //{
            //    if (!dict.ContainsKey(p.Name))
            //        dict[p.Name] = p.GetValue(exc);
            //}

            return JsonConvert.SerializeObject(dict);
        }
    }
}
