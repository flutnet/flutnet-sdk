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
