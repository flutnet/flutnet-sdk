using System.Reflection;
using Flutnet.Cli.Core.Utilities;

namespace Flutnet.Cli.Core.Dart
{
    internal class DartProperty
    {
        // The typer for this dart property
        public readonly DartType Type;

        // The name for this property in .NET
        public readonly string Name;

        // The name for this property in Dart
        public readonly string DartName;

        public readonly bool IsDeclared;

        public DartProperty(string name, DartType type, PropertyInfo propertyInfo)
        {
            Name = name;
            DartName = DartSupport.GetDartPropertyName(name);
            Type = type;
            IsDeclared = propertyInfo.IsDeclared();
        }

        public DartProperty(string name, DartType type, FieldInfo fieldInfo)
        {
            Name = name;
            DartName = DartSupport.GetDartPropertyName(name);
            Type = type;
            IsDeclared = fieldInfo.IsDeclared();
        }
    }
}