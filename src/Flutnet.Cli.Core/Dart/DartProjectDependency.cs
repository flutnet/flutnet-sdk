﻿// Copyright (c) 2020-2021 Novagem Solutions S.r.l.
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

namespace Flutnet.Cli.Core.Dart
{
    internal enum DartProjectDependencyType
    {
        Version,
        Path,
        Sdk
    }

    internal class DartProjectDependency
    {
        public DartProjectDependencyType Type { get; }
        public string Name { get; }
        public string Value { get; }

        public DartProjectDependency(string name, DartProjectDependencyType type, string value)
        {
            Type = type;
            Value = value;
            Name = name;
        }
    }
}