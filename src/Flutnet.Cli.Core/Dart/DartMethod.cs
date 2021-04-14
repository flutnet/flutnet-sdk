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
    internal class DartMethod
    {

        private readonly MethodInfo _methodInfo;

        //public readonly List<DartProperty> Params;

        /// List of all imports for the type
        public readonly List<string> Imports;

        // Return type
        public readonly DartReturnType ReturnType;

        public readonly DartType ReturnObj;

        public readonly DartType ParamObj;

        // Method name
        public readonly string Name;

        // The unique reference for the method id.
        public readonly string MethodId;

        public DartMethod(ICollection<Type> customTypes, MethodInfo methodInfo, DartType @return, DartType @param, string package, Func<MethodInfo,string> getMethodId)
        {
            _methodInfo = methodInfo;

            MethodId = getMethodId(methodInfo);

            Name = DartSupport.GetDartMethodName(_methodInfo.Name);
            ReturnObj = @return;
            ParamObj = @param;

            // Return type for the method
            ReturnType = new DartReturnType(customTypes, _methodInfo.ReturnType, package);

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

            List<string> imports = new List<string>();
            // Return types of the method
            imports.AddRange(ReturnObj.Members.SelectMany(m => m.Type.Imports));
            // Param types of the method
            imports.AddRange(ParamObj.Members.SelectMany(m => m.Type.Imports));
            // Fake classes that wrap the method
            imports.AddRange(@return.Imports.Concat(@param.Imports));

            // All method imports
            Imports = imports.Distinct().ToList();

        }
    }
}
