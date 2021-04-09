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
