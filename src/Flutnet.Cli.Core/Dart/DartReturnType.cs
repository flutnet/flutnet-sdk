using System;
using System.Collections.Generic;
using System.Linq;
using Flutnet.Utilities;

namespace Flutnet.Cli.Core.Dart
{
    internal class DartReturnType
    {
        public readonly bool IsVoid;

        public readonly DartType Type;

        public DartReturnType(ICollection<Type> customTypes, Type type, string package)
        {

            Type t = GetNestedType(type);

            IsVoid = (t == typeof(void));
            Type = IsVoid ? null : new DartType(customTypes, t, package);

        }

        public static Type GetNestedType(Type type)
        {
            if (type.IsTaskVoid())
            {
                return typeof(void);
            }

            if (type.IsTaskT())
            {
                Type t = type.GetGenericArguments().First();
                return t;
            }

            return type;
        }
    }
}