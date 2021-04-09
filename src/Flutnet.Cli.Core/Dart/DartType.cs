using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Flutnet.Cli.Core.Dart
{
    internal class DartType
    {
        // The .NET type associated
        public readonly Type DotNetType;

        // The dart package where the type exists
        public readonly string Package;

        // The import to use in dart for this class
        public readonly string Namespace;

        // The name of the type in dart
        public readonly string Name;

        // The file name of the type in dart
        public readonly string FileName;

        // The partial (.g) file name of the type in dart
        public readonly string PartialFileName;

        // The type is nullable
        public readonly bool IsNullable;

        public readonly DartTypeCategory Category;

        public readonly ConverterType ConverterType;

        /// <summary>
        ///  List of all type members
        /// </summary>
        public List<DartProperty> Members = new List<DartProperty>();

        /// List of all imports for the type
        public readonly List<string> Imports;

        readonly ICollection<Type>_customTypes;

        // The unique id used in dart to map the NET type
        public readonly string DartId;

        // The corrisponding base dart type: can be null
        public readonly DartType BaseDartType;

        public DartType(ICollection<Type> customTypes, Type type, string package)
        {
            _customTypes = customTypes;
            DotNetType = type;
            Package = package;

            // Init all the information for the DART type
            Namespace = DartSupport.GetDartNamespace(type);
            Name = DartSupport.GetDartTypeName(customTypes, type);
            FileName = DartSupport.GetDartFilename(type);
            PartialFileName = DartSupport.GetDartPartialFilename(type);
            Imports = DartSupport.GetDartImports(customTypes, type, package);
            IsNullable = DartSupport.IsNullable(type);
            Category = DartSupport.GetDartCategory(customTypes, type);
            DartId = DartSupport.GetDartUniqueId(type);

            // Custom json converter for serializing the type
            ConverterType = DartSupport.GetConverterType(customTypes, type);

            // Base dart type
            Type baseType = DartSupport.GetBaseType(customTypes, type);

            BaseDartType = (baseType == null) ? null : new DartType(customTypes, baseType, package);

            // Extract all the properties for the dart type
            List<PropertyInfo> properties = customTypes.Contains(type) ? DartSupport.GetReferencedProperty(type).ToList() : new List<PropertyInfo>(0);

            foreach (PropertyInfo prop in properties)
            {
                string propName = prop.Name;

                // Check if the object have reference to his same types (like a graph)
                DartType propType = null;
                if (prop.PropertyType != this.DotNetType)
                {
                    propType = new DartType(customTypes, prop.PropertyType, package);
                }
                else
                {
                    propType = this;
                }

                DartProperty dartProp = new DartProperty(propName, propType, prop);

                Members.Add(dartProp);
            }

            // Extract all the fields for the dart type
            List<FieldInfo> fields = customTypes.Contains(type) ? DartSupport.GetReferencedFields(type).ToList() : new List<FieldInfo>(0);

            foreach (FieldInfo field in fields)
            {
                string propName = field.Name;

                // Check if the object have reference to his same types (like a graph)
                DartType propType = null;
                if (field.FieldType != this.DotNetType)
                {
                    propType = new DartType(customTypes, field.FieldType, package);
                }
                else
                {
                    propType = this;
                }

                DartProperty dartProp = new DartProperty(propName, propType, field);

                Members.Add(dartProp);
            }

        }

    }
}
