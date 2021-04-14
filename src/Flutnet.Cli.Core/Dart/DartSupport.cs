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
using System.Text;
using Flutnet.Cli.Core.Utilities;
using Flutnet.Data;
using Flutnet.ServiceModel;
using Flutnet.Utilities;

namespace Flutnet.Cli.Core.Dart
{
    internal static class DartSupport
    {
        /// <summary>
        /// All supported primitive types
        /// </summary>
        private static readonly Dictionary<Type, string> _primitiveDartMapping = new Dictionary<Type, string>()
        {
            {typeof(bool), "bool" },
            {typeof(string), "String" },
            {typeof(byte), "int" },
            {typeof(sbyte), "int" },
            {typeof(char), "String" },
            {typeof(short), "int" },
            {typeof(ushort), "int" },
            {typeof(int), "int" },
            {typeof(uint), "int" },
            {typeof(long), "int" },
            {typeof(ulong), "int" },
            {typeof(double), "double" },
            {typeof(float), "double" },
            {typeof(DateTime), DartClasses.DateTimeClassName },
        };

        private static readonly Type[] _supportedPrimitiveTypes = _primitiveDartMapping.Keys.ToArray();

        internal static bool IsDartPrimitive(Type t)
        {
            return _primitiveDartMapping.ContainsKey(t);
        }

        internal static IEnumerable<Type> GetReferencedTypes(Type t)
        {
            if (t.IsValidPlatformOperationException())
            {
                var tmp = t.GetFieldsAndPropertiesTypes(BindingFlags.Instance | BindingFlags.Public, typeof(PlatformOperationException));
                return tmp;
            }

            return t.GetFieldsAndPropertiesTypes(BindingFlags.Instance | BindingFlags.Public);
        }

        internal static IEnumerable<PropertyInfo> GetReferencedProperty(Type t)
        {
            if (t.IsValidPlatformOperationException())
            {
                var tmp = t.GetProperties(BindingFlags.Instance | BindingFlags.Public, typeof(PlatformOperationException));
                return tmp;
            }

            return t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        }

        internal static IEnumerable<FieldInfo> GetReferencedFields(Type t)
        {
            if (t.IsValidPlatformOperationException())
            {
                var tmp = t.GetFields(BindingFlags.Instance | BindingFlags.Public, typeof(PlatformOperationException));
                return tmp;
            }

            return t.GetFields(BindingFlags.Instance | BindingFlags.Public);
        }

        // Get all referenced types
        internal static IEnumerable<Type> GetAllReferencedTypes(Type t, ICollection<Type> customTypes, List<Type> list = null)
        {

            if (list == null)
                list = new List<Type>();

            List<Type> referencedTypes = GetReferencedTypes(t).ToList();

            // Tutti i tipi relativi alle property e campi pubblici
            List<Type> propTypes = referencedTypes
                .Where(type => !list.Contains(type) && customTypes.Contains(type))
                .Distinct().ToList();

            // tutti i tipi innestati generici come liste, dictionary, ecc..
            List<Type> genericsParamTypes = referencedTypes
                .Where(tt => tt.IsGenericType)
                .SelectMany(p => ReflectionHelper.GetAllTypeParams(p))
                .Where(type => !list.Contains(type) && customTypes.Contains(type))
                .Distinct().ToList();

            List<Type> allPropsType = propTypes.Concat(genericsParamTypes).Distinct().ToList();

            
            if(t.IsGenericType || t.IsArray)
            {
                Type[] interfaces = t.GetInterfaces();

                bool isArray = t.IsArray && t.GetElementType() != null;

                if (isArray)
                {
                    IEnumerable<Type> argsTypes = GetAllReferencedTypes(t.GetElementType(), customTypes)
                                                 .Where(type => !list.Contains(type) && customTypes.Contains(type) && !allPropsType.Contains(type)).Distinct();
                    allPropsType.AddRange(argsTypes);
                }
                else
                {
                    bool isDict = interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>) && t.GenericTypeArguments[0] == typeof(string));

                    bool isSet = interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISet<>));

                    bool isList = interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                    if (isDict || isSet || isList)
                    {

                        Type nestedType = isDict ? t.GenericTypeArguments[1] : t.GenericTypeArguments[0];

                        IEnumerable <Type> argsTypes = GetAllReferencedTypes(nestedType, customTypes)
                            .Where(type => !list.Contains(type) && !allPropsType.Contains(type) && customTypes.Contains(type) && !allPropsType.Contains(type)).Distinct();
                        allPropsType.AddRange(argsTypes);
                    }

                }

            }
            

            if (allPropsType.Count > 0)
            {
                list.AddRange(allPropsType);

                // Recursively process all types
                foreach (Type props in allPropsType)
                    GetAllReferencedTypes(props, customTypes, list);
            }
            return list.Distinct();

        }

        // Find all the invalid type 
        internal static IEnumerable<Type> GetUnsupportedDartTypes(ICollection<Type> customTypes)
        {
            // Try to find all unsupported dart types
            List <Type> invalidTypes = new List<Type>();

            foreach (Type ct in customTypes)
            {
                // The main type cannot be generic
                if (ct.IsGenericType)
                {
                    invalidTypes.Add(ct);
                    continue;
                }

                // Obtain all the properties from this type
                List<Type> treeTypes = new List<Type>();

                IEnumerable<Type> referencedTypes = GetReferencedTypes(ct);

                // Find all the nested types in case this type is generic --> List< List<int> > --> [ List, List<int>, int ]
                foreach (Type pt in referencedTypes)
                {
                    treeTypes.Add(pt);
                    treeTypes.AddRange(ReflectionHelper.GetAllTypeParams(pt));

                    if (pt.IsArray && pt.GetElementType() == null || pt.IsArray && pt.IsGenericType)
                    {
                        invalidTypes.Add(ct);
                        invalidTypes.Add(pt);
                    }
                    else if (pt.IsArray)
                    {
                        treeTypes.Add(pt.GetElementType());
                        treeTypes.AddRange(ReflectionHelper.GetAllTypeParams(pt.GetElementType()));
                    }
                }

                foreach (Type nti in treeTypes)
                {

                    if(nti.IsValidPlatformOperationException() || nti == typeof(PlatformOperationException))
                        continue;

                    Type nt = nti.IsArray && nti.GetElementType() != null ? nti.GetElementType() : nti;

                    if (nt.IsGenericType)
                    {
                        Type[] interfaces = nt.IsInterface ? new []{nt} : nt.GetInterfaces();

                        bool isDict = interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));

                        if (isDict && nt.GenericTypeArguments[0] != typeof(string))
                        {
                            invalidTypes.Add(ct);
                            invalidTypes.Add(nt);
                            continue;
                        }

                        bool isSet = interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISet<>));

                        bool isList = interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                        if (!isSet && !isList)
                        {
                            invalidTypes.Add(ct);
                            invalidTypes.Add(nt);
                            continue;
                        }
                    }
                    else
                    {

                        bool isCustom = customTypes.Contains(nt);

                        bool isPrimitive = _supportedPrimitiveTypes.Contains(nt);

                        if (!isCustom && !isPrimitive)
                        {
                            invalidTypes.Add(ct);
                            invalidTypes.Add(nt);
                            continue;
                        }

                        // 20201127: add support to inheritance for PlatformData
                        /*
                        bool haveInheritance = HaveInheritance(customTypes, nt);

                        // All the data protocol types cannot inerith from other objects, only Exception can
                        if (isCustom && 
                            haveInheritance && 
                            nt.IsValidPlatformOperationException() == false)
                        {
                            invalidTypes.Add(ct);
                            invalidTypes.Add(nt);
                            continue;
                        }
                        */
                        // 20201127 add support to inheritance for PlatformData

                    }
                }


            }

            return invalidTypes.Distinct();
        }

        // Check if specific type is supported by dart conversion
        internal static bool IsSupportedByDart(Type t, ICollection<Type> supportedCustomTypes)
        {

            bool isCustom = supportedCustomTypes.Contains(t);

            if (isCustom)
                return true;

            bool isPrimitive = _supportedPrimitiveTypes.Contains(t);

            if (isPrimitive)
                return true;

            bool isArray = t.IsArray && t.GetElementType() != null;

            if (isArray)
            {
                return IsSupportedByDart(t.GetElementType(), supportedCustomTypes);
            }

            if (t.IsGenericType == false)
            {
                return false;
            }

            {

                List<Type> paramsType = ReflectionHelper.GetAllTypeParams(t).ToList();

                foreach (Type pt in paramsType)
                {

                    bool _isCustom = supportedCustomTypes.Contains(pt);

                    if (_isCustom)
                        continue;

                    bool _isPrimitive = _supportedPrimitiveTypes.Contains(pt);

                    if (_isPrimitive)
                        continue;

                    bool _isArray = pt.IsArray && pt.GetElementType() != null;

                    if (_isArray && IsSupportedByDart(pt.GetElementType(), supportedCustomTypes))
                    {
                        continue;
                    }

                    if (pt.IsGenericType)
                    {

                        Type[] interfaces = pt.IsInterface ? new []{pt} : pt.GetInterfaces();

                        bool isDict = interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>) && pt.GenericTypeArguments[0] == typeof(string));

                        if(isDict && IsSupportedByDart(pt.GenericTypeArguments[1], supportedCustomTypes))
                        {
                            continue;
                        }

                        bool isSet = interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISet<>));

                        if (isSet && IsSupportedByDart(pt.GenericTypeArguments[0], supportedCustomTypes))
                        {
                            continue;
                        }

                        bool isList = interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                        if (isList && IsSupportedByDart(pt.GenericTypeArguments[0], supportedCustomTypes))
                        {
                            continue;
                        }
                    }

                    // Nessuna condizione soddisfatta
                    return false;

                }

                // All the generics are suppored
                return true;


            }

        }

        // Find all the dart imports needed for specific dart type
        internal static List<string> GetDartImports(ICollection<Type> customTypes, Type t, string packageName)
        {

            List<string> imports = new List<string>();

            // Custom types cannot be generic!!
            bool isCustom = customTypes.Contains(t);

            if (isCustom)
            {
                string dartNamespace = GetDartNamespace(t);
                string dartFilename = GetDartFilename(t);
                string import = DartPackageImport(packageName,dartNamespace, dartFilename);
                imports.Add(import);
                return imports;
            }

            // UINT 8 LIST non supportato da json_serializable library
            bool isUint8List = t.IsArray && t.GetElementType() != null && t.GetElementType() == typeof(byte);
            if (isUint8List)
            {
                // Import per Uint8List
                imports.Add(DartImports.DartTypedDataImport);
                return imports;
            }

            Type[] allSubtypes = ReflectionHelper.GetAllTypeParams(t);

            foreach (Type st in allSubtypes)
            {
                if (st != t) // Skip same object reference
                {
                    var otherImports = GetDartImports(customTypes, st, packageName);
                    imports.AddRange(otherImports);
                }
            }

            return imports.Distinct().ToList();

        }

        internal static string DartPackageImport(string packageName, string dartNamespace, string dartFilename)
        {
            string importValue = $"package:{packageName}{dartNamespace}{dartFilename}";
            return $"import '{importValue}';" ;
        }

        internal static string GetDartFilePath(string dartLibFolderPath, DartType type)
        {
            // Example /app/protocol/my_item.dart
            string packagePath = type.Namespace + type.FileName;

            string totalPath = $"{dartLibFolderPath}{packagePath.Replace(DartProject.DartSeparator,Path.DirectorySeparatorChar)}";

            return totalPath;
        }

        internal static string GetDartPartialFilePath(string dartLibFolderPath, DartType type)
        {
            // Example /app/protocol/my_item.g.dart
            string packagePath = type.Namespace + type.PartialFileName;

            string totalPath = $"{dartLibFolderPath}{packagePath.Replace(DartProject.DartSeparator, Path.DirectorySeparatorChar)}";

            return totalPath;
        }

        internal static string GetDartDirectoryPath(string dartLibFolderPath, DartType type)
        {
            // Example /app/protocol/my_item.dart
            string packagePath = type.Namespace;//+ type.FileName;

            string totalPath = $"{dartLibFolderPath}{packagePath.Replace(DartProject.DartSeparator, Path.DirectorySeparatorChar)}";

            return totalPath;
        }

        /// <summary>
        /// Obtain a namespace for a specific NET type
        /// </summary>
        /// <param name="t"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        internal static string GetDartNamespace(Type t, DartFileNameConverter converter = null)
        {
            // Convert the namespace to flutter file path in package ==> /models/index.dart
            DartFileNameConverter fileNameConverter = converter ?? new DartFileNameConverter();

            char[] separator = { '.' };
            StringBuilder pathBuilder = new StringBuilder();
            pathBuilder.Clear();

            string ns = t.Namespace;

            pathBuilder.Append(DartProject.DartSeparator); // Add "/"

            if (!string.IsNullOrEmpty(ns))
            {
                string[] nsFolders = ns.Split(separator);

                foreach (string folder in nsFolders)
                {
                    pathBuilder.Append(fileNameConverter.Convert(folder));
                    pathBuilder.Append(DartProject.DartSeparator);
                }
            }

            return pathBuilder.ToString();
        }

        /// <summary>
        /// Obtain a filename for a specific NET type
        /// </summary>
        /// <param name="t"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        internal static string GetDartFilename(Type t, DartFileNameConverter converter = null)
        {
            // Convert the namespace to flutter file path in package ==> /models/index.dart
            DartFileNameConverter fileNameConverter = converter ?? new DartFileNameConverter();

            // Dart filename
            string dartFileName = fileNameConverter.Convert(t.Name) + ".dart";

            return dartFileName;
        }

        /// <summary>
        /// Obtain the partial filename (.g) for a specific NET type
        /// </summary>
        /// <param name="t"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        internal static string GetDartPartialFilename(Type t, DartFileNameConverter converter = null)
        {
            // Convert the namespace to flutter file path in package ==> /models/index.dart
            DartFileNameConverter fileNameConverter = converter ?? new DartFileNameConverter();

            // Dart filename
            string dartFileName = fileNameConverter.Convert(t.Name) + ".g.dart";

            return dartFileName;
        }

        /// <summary>
        /// Obtain a classname for a specific NET type
        /// </summary>
        /// <param name="t"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        internal static string GetDartClassName(Type t, DartClassNameConverter converter = null)
        {
            DartClassNameConverter classNameConverter = converter ?? new DartClassNameConverter();

            // Dart classname
            string dartClassName = classNameConverter.Convert(t.Name);

            return dartClassName;
        }

        internal static string GetDartPropertyName(string propertyName, DartPropertyNameConverter converter = null)
        {
            DartPropertyNameConverter propertyNameConverter = converter ?? new DartPropertyNameConverter();

            // Dart property name
            string propName = propertyNameConverter.Convert(propertyName);

            return propName;
        }

        internal static string GetDartMethodName(string methodName, DartPropertyNameConverter converter = null)
        {
            DartPropertyNameConverter propertyNameConverter = converter ?? new DartPropertyNameConverter();

            // Dart methodName
            string metName = propertyNameConverter.Convert(methodName);

            return metName;
        }

        /// <summary>
        /// Return the name for the type in dart language.
        /// </summary>
        /// <param name="customTypes"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        /// <exception cref="DartSupportException"></exception>
        internal static string GetDartTypeName(ICollection<Type> customTypes, Type t)
        {

            // Incase the type is primitive
            if (_primitiveDartMapping.ContainsKey(t))
            {
                return _primitiveDartMapping[t]; // String, int, long, ....ecc
            }

            bool isCustom = customTypes.Contains(t);

            if (isCustom && t.IsGenericType == false)
            {
                return GetDartClassName(t);
            }

            Type[] interfaces = t.IsInterface ? new []{t} : t.GetInterfaces();

            bool isDict = interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));

            // Support only Dictionary<string,T>
            if (isDict && t.GenericTypeArguments[0] == typeof(string))
            {
                Type valueType = t.GenericTypeArguments[1];
                string valueTypeAsString = GetDartTypeName(customTypes, valueType);
                return $"Map<String,{valueTypeAsString}>";
            }

            bool isSet = interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISet<>));
            if (isSet)
            {
                Type setType = t.GenericTypeArguments[0];
                string setTypeAsString = GetDartTypeName(customTypes, setType);
                return $"Set<{setTypeAsString}>";
            }

            bool isUint8List = t.IsArray && t.GetElementType() != null && t.GetElementType() == typeof(byte);
            if (isUint8List)
            {
                return DartClasses.Uint8ListClassName;
            }

            bool isArray = t.IsArray;
            if (isArray)
            {
                Type arrayType = t.GetElementType();
                string arrayTypeAsString = GetDartTypeName(customTypes, arrayType);
                return $"List<{arrayTypeAsString}>";
            }

            bool isList = interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            if (isList)
            {
                Type listType = t.GenericTypeArguments[0];
                string listTypeAsString = GetDartTypeName(customTypes, listType);
                return $"List<{listTypeAsString}>";
            }

            // Dart support error
            throw new Exception($"{nameof(DartSupport)}.{nameof(GetDartTypeName)} :{GetUnsupportedDartTypeErrorMessage(t)}" );

        }

        /// <summary>
        /// Obtain info about the type in Dart language.
        /// </summary>
        /// <param name="customTypes"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        internal static DartTypeCategory GetDartCategory(ICollection<Type> customTypes, Type t)
        {

            // Incase the type is primitive
            if (_primitiveDartMapping.ContainsKey(t))
            {
                return DartTypeCategory.Primitive; // String, int, long, ....ecc
            }

            bool isCustom = customTypes.Contains(t);

            if (isCustom)
            {
                return DartTypeCategory.Custom;
            }

            bool isArray = t.IsArray;
            if (isArray)
            {
                return DartTypeCategory.List;
            }

            if(t.IsGenericType)
            {
                Type[] interfaces = t.GetInterfaces();

                bool isDict = interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>) && t.GenericTypeArguments[0] == typeof(string));

                if (isDict)
                    return DartTypeCategory.Map;
                
                bool isSet = interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISet<>));
                if (isSet)
                    return DartTypeCategory.Set;
                
                bool isList = interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                if (isList)
                    return DartTypeCategory.List;
            }

            return DartTypeCategory.None;

        }

        internal static string GetTypeName(Type t)
        {
            if (t.IsArray)
            {
                return $"{GetTypeName(t.GetElementType())}[]";
            }

            if (t.IsGenericType)
            {
                List<string> typeNames = new List<string>();
                foreach (Type arg in t.GetGenericArguments())
                {
                    typeNames.Add(GetTypeName(arg));
                }
                return $"{t.Name}<{string.Join(",", typeNames)}>";
            }

            return t.Name;
        }

        internal static bool IsNullable(Type t, bool isProperty = false)
        {

            if (!t.IsValueType) return true; // ref-type
            if (Nullable.GetUnderlyingType(t) != null) return true; // Nullable<T>
            return false; //

            if (t.IsClass || t.IsArray)
            {
                return true;
            }

            return false;

        }

        internal static string GetDartUniqueId(Type t)
        {
            // The FlutnetException class is SHARED Between Flutnet.Android &  & Flutnet.iOS --> Assembly Flutnet
            string assembly = (t == typeof(FlutnetException)) ? "Flutnet" : t.Assembly.GetName().Name;
            return $"{t.FullName}, {assembly}";
        }

        internal static string GetUnsupportedDartTypeErrorMessage(Type t)
        {
            string typeName = DartSupport.GetTypeName(t);

            return $"The type {typeName} has no mapping to dart type. Unsupported!!";
        }

        internal static bool HaveInheritance(ICollection<Type> customTypes, Type t)
        {

            bool isExceptionBase = t == typeof(PlatformOperationException);

            if (isExceptionBase)
                return false;

            bool isCustom = customTypes.Contains(t);

            if (isCustom == false)
                return false;

            bool isExceptionCustom = t.IsValidPlatformOperationException();

            if (isExceptionCustom)
                return true;

            bool isBase = t.BaseType == null ||
                          t.BaseType == typeof(object) ||
                          //20200312: hineritance is added to [PlatformData] --> we have added the events args class
                          //t.BaseType == typeof(EventArgs) ||
                          t.IsPrimitive ||
                          t.IsEnum;

            return !isBase;

        }

        internal static Type GetBaseType(ICollection<Type> customTypes, Type t)
        {

            bool haveInheritance = HaveInheritance(customTypes, t);

            if (haveInheritance)
            {
                return t.BaseType;
            }

            return null;

        }

        internal static ConverterType GetConverterType(ICollection<Type> customTypes, Type t)
        {

            string dartTypeName = GetDartTypeName(customTypes, t);

            // List<Uint8List>
            if (dartTypeName.Contains(DartClasses.Uint8ListClassName))
            {
                return ConverterType.Uint8Converter;
            }

            if (dartTypeName.Contains(DartClasses.DateTimeClassName))
            {
                return ConverterType.DateTimeConverter;
            }

            return ConverterType.None;

        }

        internal static string GetConverterName(ConverterType t)
        {
            switch (t)
            {
                case ConverterType.Uint8Converter:
                    return $"@{DartClasses.Uint8ListConverterClassName}()";
                case ConverterType.DateTimeConverter:
                    return $"@{DartClasses.DateTimeConverterClassName}()";
                default:
                    throw new ArgumentOutOfRangeException(nameof(t), t, $"{nameof(GetConverterName)}: ArgumentOutOfRangeException");
            }
        }


        // Obtian all Dart subtypes
        internal static ICollection<DartType> GetDartSubtypes(DartType type, ICollection<DartType> exportedTypes)
        {
            ICollection<Type> netSubTypes = ReflectionHelper.GetSubTypes(exportedTypes.Select(ct => ct.DotNetType).ToList(), type.DotNetType).ToList();
            ICollection<DartType> dartSubtypes = exportedTypes.Where(ct => netSubTypes.Contains(ct.DotNetType)).OrderBy(et=>et.DotNetType.FullName).ToList();
            return dartSubtypes;
        }

    }

    internal enum ConverterType
    {
        None,
        Uint8Converter,
        DateTimeConverter
    }
}