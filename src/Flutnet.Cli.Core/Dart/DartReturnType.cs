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