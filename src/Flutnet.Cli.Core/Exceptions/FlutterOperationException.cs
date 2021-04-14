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
using System.Reflection;
using System.Text;
using Flutnet.Cli.Core.Dart;
using Flutnet.ServiceModel;

namespace Flutnet.Cli.Core.Exceptions
{
    internal class FlutterOperationException : Exception
    {
        private readonly Type _flutterOperationType;
        private readonly MethodInfo _methodInfo;
        private readonly List<ParameterInfo> _parametersInfo;
        private readonly Type _returnType;

        public FlutterOperationException(Type flutterOperationType, MethodInfo methodInfo, string invalidOperationName) 
            : base(_getErrorMessages2(flutterOperationType, methodInfo, invalidOperationName))
        {
            _flutterOperationType = flutterOperationType;
            _methodInfo = methodInfo;
        }

        public FlutterOperationException(Type flutterOperationType, MethodInfo methodInfo, List<ParameterInfo> parametersInfo, Type returnType) 
            : base(_getErrorMessages(flutterOperationType, methodInfo, parametersInfo, returnType))
        {
            _flutterOperationType = flutterOperationType;
            _methodInfo = methodInfo;
            _parametersInfo = parametersInfo;
            _returnType = returnType;
        }

        static string _getErrorMessages(Type flutterOperationType, MethodInfo methodInfo, List<ParameterInfo> parametersInfo, Type returnType)
        {
            if (parametersInfo == null)
                parametersInfo = new List<ParameterInfo>(0);

            StringBuilder builder = new StringBuilder();

            string operationClassName = DartSupport.GetTypeName(flutterOperationType);

            string methodName = methodInfo.Name;

            builder.AppendLine($"Invalid {nameof(PlatformOperationAttribute)} for class {operationClassName}.");
            builder.AppendLine($"Method name: {methodName}");

            if (returnType != null)
            {
                string returnTypeName = DartSupport.GetTypeName(returnType);
                builder.AppendLine($"- Invalid return type: {returnTypeName}");
            }

            foreach (ParameterInfo info in parametersInfo)
            {
                string paramName = info.Name;
                string paramType = DartSupport.GetTypeName(info.ParameterType);
                builder.AppendLine($"- Invalid \"{paramName}\" type: {paramType}");
            }

            return builder.ToString();
        }

        static string _getErrorMessages2(Type flutterOperationType, MethodInfo methodInfo, string invalidOperationName)
        {
            StringBuilder builder = new StringBuilder();

            string operationClassName = DartSupport.GetTypeName(flutterOperationType);

            string methodName = methodInfo.Name;

            builder.AppendLine($"Invalid {nameof(PlatformOperationAttribute)} for class {operationClassName}, method:{methodName}.");
            //builder.AppendLine($"ID for Method {methodName} => '{invalidOperationName}' is already used by another method!");
            builder.AppendLine($"Cannot exists multiple {nameof(PlatformOperationAttribute)} with the same method name: Function overloading is not supported in Dart at all.");

            return builder.ToString();
        }
    }
}
