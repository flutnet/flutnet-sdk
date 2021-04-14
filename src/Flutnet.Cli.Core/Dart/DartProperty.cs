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