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