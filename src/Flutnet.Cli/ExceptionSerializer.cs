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
