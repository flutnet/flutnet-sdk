using System;
using System.Collections.Generic;
using System.Text;
using Flutnet.ServiceModel;

namespace Flutnet.Cli.Core.Exceptions
{
    internal class FlutterInheritedDataException : Exception
    {
        public readonly IEnumerable<Type> InvalidTypes;

        public FlutterInheritedDataException(params Type[] invalidType) 
            : base(_getErrorMessages(invalidType))
        {
            InvalidTypes = invalidType;
        }

        static string _getErrorMessages(IEnumerable<Type> types)
        {
            StringBuilder builder = new StringBuilder();

            foreach (Type type in types)
            {
                builder.AppendLine(GetInheritedDataErrorMessage(type));
            }

            return builder.ToString();
        }

        internal static string GetInheritedDataErrorMessage(Type t)
        {
            string typeName = t.FullName;

            return $"The .NET type {typeName} has SubTypes marked with [{nameof(PlatformDataAttribute)}]. Must be decorated with the same attribute.";
        }

        internal static string GetInheritedDataErrorMessageOLD(Type t)
        {
            string typeName = t.FullName;

            return $"The .NET type {typeName} is decorated with [{nameof(PlatformDataAttribute)}] and cannot extends other types. Please remove inheritance from {t.BaseType?.FullName ?? "?"}";
        }
    }
}