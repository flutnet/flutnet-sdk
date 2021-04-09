using System;
using System.Reflection;
using Flutnet.Cli.Core.Dart;
using Flutnet.Utilities;

namespace Flutnet.Cli.Core.Infrastructure
{
    internal static class DotNetTools
    {
        public const string ReturnPropertyName = "ReturnValue";

        private static string ToFakeNamespace(string @namespace)
        {
            return $"{@namespace}.generated";
        }

        /// <summary>
        /// Partendo dal metodo specificato, genera dinamicamente un tipo classe
        /// contenente una property di nome <see cref="ReturnPropertyName" />
        /// dello stesso tipo del valore di ritorno del metodo.
        /// </summary>
        public static Type WrapReturnIntoFakeClass(Type @class, MethodInfo method)
        {
            var builder = CustomTypeBuilder.Builders.CustomTypeBuilder.New()
                .SetNameSpace(ToFakeNamespace(@class.Namespace))
                .SetName($"Res{@class.Name}{method.Name.FirstCharUpper()}")
                .FinalizeOptionsBuilder();

            Type returnType = DartReturnType.GetNestedType(method.ReturnType);

            if (returnType != typeof(void))
            {
                builder = builder.AddProperty(ReturnPropertyName, returnType);
            }

            Type fakeReturnType = builder.Compile();

            return fakeReturnType;
        }

        /// <summary>
        /// Partendo dal metodo specificato, genera dinamicamente un tipo classe
        /// contenente una property per ciascun parametro del metodo.
        /// </summary>
        public static Type WrapParamsIntoFakeClass(Type @class, MethodInfo method)
        {
            var builder = CustomTypeBuilder.Builders.CustomTypeBuilder.New()
                .SetNameSpace(ToFakeNamespace(@class.Namespace))
                .SetName($"Cmd{@class.Name}{method.Name.FirstCharUpper()}")
                .FinalizeOptionsBuilder();

            foreach (ParameterInfo parameter in method.GetParameters())
            {
                builder = builder.AddProperty(parameter.Name.FirstCharUpper(), parameter.ParameterType);
            }

            Type fakeReturnType = builder.Compile();

            return fakeReturnType;
        }
    }
}