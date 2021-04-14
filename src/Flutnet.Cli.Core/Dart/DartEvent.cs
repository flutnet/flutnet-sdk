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
using System.Reflection;

namespace Flutnet.Cli.Core.Dart
{
    internal class DartEvent
    {

        private readonly EventInfo _eventInfo;

        //public readonly List<DartProperty> Params;

        /// List of all imports for the type
        public readonly List<string> Imports;

        public readonly DartType EventArgs;

        // Event name
        public readonly string Name;

        public DartEvent(ICollection<Type> customTypes, EventInfo eventInfo, DartType eventArgs, string package)
        {

            _eventInfo = eventInfo;

            Name = DartSupport.GetDartMethodName(_eventInfo.Name);
            EventArgs = eventArgs;

            //Params = ParamObj.Members.ToList();

            /*
            foreach (ParameterInfo paramInfo in _methodInfo.GetParameters())
            {

                string paramName = paramInfo.Name;
                DartType paramType = new DartType(customTypes, paramInfo.ParameterType, package);

                DartProperty dartProp = new DartProperty(paramName, paramType);

                Params.Add(dartProp);

            }
            */

            Imports = eventArgs.Imports.ToList();

        }
    }
}
