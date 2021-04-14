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
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Flutnet.Cli.Core.Utilities
{
    internal static class SerDes
    {
        public enum XmlEncoding { ASCII, UTF8, Unicode }

        const string cN1 = " xsi:type=\"";
        const string cX1 = "_xsi_type_";

        static readonly Dictionary<string, XmlSerializer> Cache = new Dictionary<string, XmlSerializer>();
        static readonly object SyncRoot = new object();

        /// <summary>
        /// Serializza un oggetto in una stringa XML.
        /// </summary>
        /// <param name="obj"></param>        
        /// <returns></returns>
        public static string ObjectToXmlString<T>(T obj, params Type[] extraTypes)
            where T : class
        {
            string xml = null;

            //XmlSerializer serializer = new XmlSerializer(typeof(T), extraTypes);
            XmlSerializer serializer = GetXmlSerializer(typeof(T), extraTypes);
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.Serialize(ms, obj);
                xml = Encoding.UTF8.GetString(ms.ToArray());
                ms.Close();
            }

            //xml = PostProcess(xml);

            return xml;
        }

        /// <summary>
        /// Deserializza una stringa XML nell'oggetto del tipo richiesto.
        /// </summary>
        /// <param name="xmlString">Stringa ottenuta dalla serializzazione XML dell'oggetto.</param>
        /// <returns></returns>
        public static T XmlStringToObject<T>(string xmlString, out XmlEncoding encoding, params Type[] extraTypes)
            where T : class
        {
            //xmlString = PreProcess(xmlString);

            T obj;

            //XmlSerializer serializer = new XmlSerializer(typeof(T), extraTypes);
            XmlSerializer serializer = GetXmlSerializer(typeof(T), extraTypes);
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] buffer;

                if (xmlString.Contains("encoding=\"UTF-8\""))
                {
                    encoding = XmlEncoding.UTF8;
                    buffer = Encoding.UTF8.GetBytes(xmlString);
                }
                else if (xmlString.Contains("encoding=\"UTF-16\""))
                {
                    encoding = XmlEncoding.Unicode;
                    buffer = Encoding.Unicode.GetBytes(xmlString);
                }
                else
                {
                    encoding = XmlEncoding.ASCII;
                    buffer = Encoding.UTF8.GetBytes(xmlString);
                }

                ms.Write(buffer, 0, buffer.Length);
                ms.Seek(0, SeekOrigin.Begin);
                obj = (T)serializer.Deserialize(ms);
                ms.Close();
            }

            return obj;
        }

        /// <summary>
        /// Deserializza una stringa XML nell'oggetto del tipo richiesto. Un valore restituito indica se la deserializzazione è riuscita o meno.
        /// </summary>
        /// <param name="xmlString">Stringa ottenuta dalla serializzazione XML dell'oggetto.</param>
        /// <returns></returns>
        public static bool TryXmlStringToObject<T>(string xmlString, out T obj, params Type[] extraTypes)
            where T : class
        {
            try
            {
                XmlEncoding encoding = XmlEncoding.UTF8;
                obj = XmlStringToObject<T>(xmlString, out encoding, extraTypes);
                return true;
            }
            catch
            {
                obj = default(T);
                return false;
            }
        }

        /// <summary>
        /// Serializza un oggetto in un file XML.
        /// </summary>
        /// <param name="obj"></param>        
        /// <param name="filename"></param>
        public static void ObjectToXmlFile<T>(T obj, string filename, params Type[] extraTypes)
            where T : class
        {
            //XmlSerializer serializer = new XmlSerializer(typeof(T), extraTypes);
            XmlSerializer serializer = GetXmlSerializer(typeof(T), extraTypes);
            using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                serializer.Serialize(fs, obj);
                fs.Flush();
                fs.Close();
            }
        }

        /// <summary>
        /// Deserializza un file XML nell'oggetto del tipo richiesto.
        /// </summary>
        /// <param name="filename">File XML contenente l'oggetto serializzato.</param>
        /// <returns></returns>
        public static T XmlFileToObject<T>(string filename, params Type[] extraTypes)
            where T : class
        {
            T obj;

            //XmlSerializer serializer = new XmlSerializer(typeof(T), extraTypes);
            XmlSerializer serializer = GetXmlSerializer(typeof(T), extraTypes);
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                obj = (T) serializer.Deserialize(fs);
                fs.Close();
            }

            return obj;
        }

        /// <summary>
        /// Deserializza un file XML nell'oggetto del tipo richiesto. Un valore restituito indica se la deserializzazione è riuscita o meno.
        /// </summary>
        /// <param name="filename">File XML contenente l'oggetto serializzato.</param>
        /// <returns></returns>
        public static bool TryXmlFileToObject<T>(string filename, out T obj, params Type[] extraTypes)
            where T : class
        {
            try
            {
                obj = XmlFileToObject<T>(filename, extraTypes);
                return true;
            }
            catch
            {
                obj = default(T);
                return false;
            }
        }

        private static XmlSerializer GetXmlSerializer(Type type, params Type[] extraTypes)
        {
            if (extraTypes.Length == 0)
                return new XmlSerializer(type);

            //https://msdn.microsoft.com/en-us/library/system.xml.serialization.xmlserializer%28v=vs.110%29.aspx
            //To increase performance, the XML serialization infrastructure dynamically generates assemblies to serialize and deserialize specified types. 
            //The infrastructure finds and reuses those assemblies. This behavior occurs only when using the following constructors: 
            //  XmlSerializer.XmlSerializer(Type)
            //  XmlSerializer.XmlSerializer(Type, String)
            //If you use any of the other constructors, multiple versions of the same assembly are generated and never unloaded,
            //which results in a memory leak and poor performance. The easiest solution is to use one of the previously mentioned two constructors. 
            //Otherwise, you must cache the assemblies.

            StringBuilder keyBuilder = new StringBuilder(type.ToString());
            foreach (Type extraType in extraTypes)
                keyBuilder.AppendFormat(":{0}", extraType);
            string key = keyBuilder.ToString();

            if (!Cache.ContainsKey(key))
            {
                lock (SyncRoot)
                {
                    if (!Cache.ContainsKey(key))
                    {
                        Cache.Add(key, new XmlSerializer(type, extraTypes));
                    }
                }
            }

            return Cache[key];
        }
    }
}
